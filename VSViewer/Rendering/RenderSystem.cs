using GameFormatReader.Common;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WPF;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.Loader;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace VSViewer.Rendering
{
    public class RenderSystem : D3D11
    {
        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct InputVertex
        {
            public Vector3 Position;
            public Vector2 Texture;

            public InputVertex(Vector3 p, Vector2 t)
            {
                Position = p;
                Texture = t;
            }

            public void SetToZero()
            {
                Position = Vector3.Zero;
                Texture = Vector2.Zero;
            }

            public const int SizeInBytes = (3 + 2) * 4;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MatrixBuffer
        {
            public Matrix World;
            public Matrix View;
            public Matrix Projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TransparentBuffer
        {
            public float BlendAmount;
            private Vector3 Padding;
        }
        #endregion

        VertexShader m_vertexShader;
        PixelShader m_pixelShader;
        ConstantBuffer<MatrixBuffer> m_matrixBuffer;
        InputLayout m_inputElements;
        ShaderResourceView m_textureResourceView;
        SamplerState m_samplerState;
        Buffer vertexBuffer;
        Buffer indexBuffer;
        DepthStencilState depthStencilState;
        BlendState blendState;
        RasterizerState rasterizerState;

        private Geometry geometry;
        private SEQ seq;
        private int m_currentAnimation;
        private float m_animFrameTimer;

        private Keyframe[] m_previousKeyframe;
        private Keyframe[] m_currentKeyframe;
        private Keyframe[] m_nextKeyframe;

        private InputVertex[] m_instanceVertices;
        private SkeletalJoint[] m_instanceJoints;

        bool m_isPendingVertexBufferUpdate;

        // Render system initialize all initialization should be done here.
        public bool Initialize()
        {
            return InitializeShader("VagrantStoryMaterial.fx");
        }

        public void PushGeometry(Geometry newGeometry)
        {
            geometry = newGeometry;
            m_instanceVertices = new InputVertex[geometry.vertices.Count];
            m_instanceJoints = geometry.skeleton.ToArray();
            m_isPendingVertexBufferUpdate = true;
        }

        public void PushSequence(SEQ newSEQ)
        {
            seq = newSEQ;
            m_previousKeyframe = new Keyframe[geometry.skeleton.Count];
            m_currentKeyframe = new Keyframe[geometry.skeleton.Count];
            m_nextKeyframe = new Keyframe[geometry.skeleton.Count];
        }

        public bool InitializeShader(string shaderName)
        {
            try
            {
                // Set shader flags
                ShaderFlags sFlags = ShaderFlags.EnableStrictness;

#if DEBUG
                sFlags |= ShaderFlags.Debug;
#endif

                // Compile shader code
                CompilationResult vertexShaderByteCode = ShaderBytecode.CompileFromFile(shaderName, "VS", "vs_4_0", sFlags, EffectFlags.None);
                CompilationResult pixelShaderByteCode = ShaderBytecode.CompileFromFile(shaderName, "PS", "ps_4_0", sFlags, EffectFlags.None);

                // Create signature
                ShaderSignature inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                // Set shaders
                Set(ref m_vertexShader, new VertexShader(Device, vertexShaderByteCode));
                Set(ref m_pixelShader, new PixelShader(Device, pixelShaderByteCode));

                // Define inputs
                m_inputElements = new InputLayout(Device, inputSignature, new[]{
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0),
				});
                Device.ImmediateContext.InputAssembler.InputLayout = (m_inputElements);

                // Release the shader buffers.
                vertexShaderByteCode.Dispose();
                pixelShaderByteCode.Dispose();

                // Vagrant Story meshes are written as TriangleLists
                Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                RasterizerStateDescription rasterDesc = new RasterizerStateDescription()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                    DepthBias = 0,
                    DepthBiasClamp = 0,
                    SlopeScaledDepthBias = 0,
                    IsDepthClipEnabled = true,
                    IsMultisampleEnabled = true,
                    IsScissorEnabled = false,
                    IsAntialiasedLineEnabled = true,
                };

                rasterizerState = new RasterizerState(Device, rasterDesc);
                Device.ImmediateContext.Rasterizer.State = rasterizerState;

                RenderTargetBlendDescription RTBDesc = new RenderTargetBlendDescription(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha,
                    BlendOperation.Add, BlendOption.One, BlendOption.Zero, BlendOperation.Add, ColorWriteMaskFlags.All);
                BlendStateDescription bsDesc = new BlendStateDescription();
                bsDesc.RenderTarget[0] = RTBDesc;

                blendState = new BlendState(Device, bsDesc);
                Device.ImmediateContext.OutputMerger.SetBlendState(blendState, null, -1);

                // Create constant matrix buffer
                m_matrixBuffer = new ConstantBuffer<MatrixBuffer>(Device);
                Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_matrixBuffer.Buffer);

                // Create a texture sampler state description.
                SamplerStateDescription samplerDesc = new SamplerStateDescription
                {
                    Filter = Filter.MinMagMipPoint,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    ComparisonFunction = Comparison.Never,
                    MinimumLod = 0,
                    MaximumLod = 0,
                };
                m_samplerState = new SamplerState(Device, samplerDesc);

                DepthStencilStateDescription depthStencilStateDesc = new DepthStencilStateDescription
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.Less,

                    IsStencilEnabled = true,

                    FrontFace = new DepthStencilOperationDescription
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },

                    BackFace = new DepthStencilOperationDescription
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },
                };
                depthStencilState = new DepthStencilState(Device, depthStencilStateDesc);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing shader. Error is " + ex.Message);
                return false;
            };

            Camera = new FirstPersonCamera();
            Camera.EnableYAxisMovement = false;
            Camera.SetProjParams(65 * VSTools.Deg2Rad, 1, 5.0f, 500f);
            Camera.SetViewParams(new Vector3(0.0f, 0.0f, -500.0f), new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0, 0, -1));
            return true;
        }

        public override void RenderScene(DrawEventArgs args)
        {
            // fill the back buffer with solid black
            Device.ImmediateContext.ClearRenderTargetView(RenderTargetView, new Color4(0, 0, 0, 1));
            // clear depth buffer
            Device.ImmediateContext.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1f, 0);
            // something
            Device.ImmediateContext.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);
            // blend state application
            Device.ImmediateContext.OutputMerger.SetBlendState(blendState, null, -1);
            // rasteriser application
            Device.ImmediateContext.Rasterizer.State = rasterizerState;

            // TODO: A fix for the resize bug.
            //m_device.ImmediateContext.Rasterizer.SetViewports(new Viewport(0, 0, w, h, 0.0f, 1.0f));

            // stall renderer if there is Geometry.
            if (geometry == null) { return; }

            UpdateAnimation(args.DeltaTime);

            ApplySkinning();

            UpdateVertexAndIndiceBuffers();

            SetShaderParameters(args);

            PushShaders();
        }

        private void UpdateAnimation(TimeSpan timeSpan)
        {
            if (seq == null) { return; }

            m_animFrameTimer += (float)timeSpan.Milliseconds;
            bool loopFrame = false;

            if (seq.animations[m_currentAnimation].length <= m_animFrameTimer / 1000)
            {
                loopFrame = true;
                // wrap timer
                m_animFrameTimer = m_animFrameTimer - seq.animations[m_currentAnimation].length * 1000;
                m_animFrameTimer += 0.01f;
            }

            float frameQueryTime = MathUtil.Clamp(m_animFrameTimer / 1000, 0, seq.animations[m_currentAnimation].length);

            // query frames
            for (int i = 0; i < geometry.skeleton.Count; i++)
            {
                int totalKeysForJoint = seq.animations[m_currentAnimation].keys[i].Count;
                if (loopFrame)
                {
                    m_previousKeyframe[i] = seq.animations[m_currentAnimation].keys[i][totalKeysForJoint - 1];
                    m_currentKeyframe[i] = seq.animations[m_currentAnimation].keys[i][0];
                    if (totalKeysForJoint > 1)
                    {
                        m_nextKeyframe[i] = seq.animations[m_currentAnimation].keys[i][1];
                    }
                    else
                    {
                        m_nextKeyframe[i] = seq.animations[m_currentAnimation].keys[i][0];
                    }
                }
                else
                {
                    for (int k = 0; k < totalKeysForJoint; k++)
                    {
                        Keyframe keyframe = seq.animations[m_currentAnimation].keys[i][k];
                        if (m_currentKeyframe[i] == null) { m_currentKeyframe[i] = keyframe; m_nextKeyframe[i] = keyframe; }

                        if (frameQueryTime >= m_nextKeyframe[i].Time)
                        {
                            if (keyframe.Time >= frameQueryTime)
                            {
                                m_previousKeyframe[i] = m_currentKeyframe[i];
                                m_currentKeyframe[i] = m_nextKeyframe[i];
                                m_nextKeyframe[i] = keyframe;
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < geometry.skeleton.Count; i++)
            {

                float f1 = m_currentKeyframe[i].Time;
                float f2 = m_nextKeyframe[i].Time;
                float query = frameQueryTime;

                float a = query - f1;
                float b = f2 - f1;

                float t = MathUtil.Clamp(a / b, 0, 1);

                if (float.IsNaN(t))
                {
                    t = 0;
                }

                m_instanceJoints[i].position = Vector3.Lerp(m_currentKeyframe[i].Position, m_nextKeyframe[i].Position, t);
                m_instanceJoints[i].quaternion = Quaternion.Slerp(m_currentKeyframe[i].Rotation, m_nextKeyframe[i].Rotation, t);
                m_instanceJoints[i].scale = Vector3.Lerp(m_currentKeyframe[i].Scale, m_nextKeyframe[i].Scale, t);
            }

        }

        private void UpdateVertexAndIndiceBuffers()
        {
            if (!m_isPendingVertexBufferUpdate) { return; }

            Set(ref m_textureResourceView, new ShaderResourceView(Device, geometry.Textures[0].GetTexture2D(Device)));
            Device.ImmediateContext.PixelShader.SetSampler(0, m_samplerState);

            // Setup vertex buffer
            Set(ref vertexBuffer, DXUtils.CreateBuffer(Device, m_instanceVertices));
            VertexBufferBinding vertexBufferBinding = new VertexBufferBinding(vertexBuffer, InputVertex.SizeInBytes, 0);
            Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

            // Setup index buffer
            Set(ref indexBuffer, DXUtils.CreateBuffer(Device, geometry.indices.ToArray()));
            Device.ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
            m_isPendingVertexBufferUpdate = false;
        }

        private void ApplySkinning()
        {
            // Construct the matrices of each bone from it's many parents
            Matrix[] boneTransforms = new Matrix[geometry.skeleton.Count];
            for (int i = 0; i < m_instanceJoints.Length; i++)
            {
                SkeletalJoint bone = m_instanceJoints[i];
                Matrix cumulativeTransform = Matrix.Identity;

                while (bone != null)
                {
                    cumulativeTransform = cumulativeTransform * Matrix.Scaling(bone.scale) * Matrix.RotationQuaternion(bone.quaternion) * Matrix.Translation(bone.position);
                    bone = (bone.parentIndex != -1) ? m_instanceJoints[bone.parentIndex] : null;
                }

                boneTransforms[i] = cumulativeTransform;
            }

            // Each bone is now in it's final position, so we can apply that to the vertices based on their bone weighting.
            // However, in Vagrant Story all vertices have a max influence of 1, so this is very simple.
            Vector3[] temporarySkinnedVertices = new Vector3[geometry.vertices.Count];
            for (int v = 0; v < m_instanceVertices.Length; v++)
            {
                temporarySkinnedVertices[v] = Vector3.TransformCoordinate(geometry.vertices[v], boneTransforms[geometry.jointID[v]]);
            }

            InterleaveVerticesWithUVs(temporarySkinnedVertices);
        }

        private void InterleaveVerticesWithUVs(Vector3[] vertices)
        {
            for (int i = 0; i < m_instanceVertices.Length; i++)
            {
                m_instanceVertices[i] = new InputVertex(vertices[i], geometry.uv1[i]);
            }
        }

        private void SetShaderParameters(DrawEventArgs args)
        {
            // Rotate the object for debugging
            float t = (float)args.TotalTime.TotalSeconds;
            var g_World = Matrix.RotationY(0);

            // Update matrices in the constant buffer
            MatrixBuffer projectionModel = new MatrixBuffer
            {
                World = Matrix.Transpose(g_World),
                View = Matrix.Transpose((Camera.View)),
                Projection = Matrix.Transpose(Camera.Projection)
            };
            m_matrixBuffer.Value = projectionModel;

            // Set Vertex shader resources
            Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_matrixBuffer.Buffer);
            Device.ImmediateContext.UpdateSubresource(m_instanceVertices, vertexBuffer);

            // Set pixel shader resources
            Device.ImmediateContext.PixelShader.SetShaderResource(0, m_textureResourceView);
        }

        private void PushShaders()
        {
            // Set the shaders to be used this frame
            Device.ImmediateContext.VertexShader.Set(m_vertexShader);
            Device.ImmediateContext.PixelShader.Set(m_pixelShader);

            Device.ImmediateContext.DrawIndexed(geometry.indices.Count, 0, 0);
        }

        private void ShutdownShader()
        {
            Set(ref m_vertexShader, null);
            Set(ref m_pixelShader, null);
            Set(ref m_matrixBuffer, null);
            Set(ref m_inputElements, null);
            Set(ref m_textureResourceView, null);
            Set(ref m_samplerState, null);
        }

        public override void Reset(int w, int h)
        {
            base.Reset(w, h);
            Camera.SetProjParams(65 * VSTools.Deg2Rad, w / (float)h, 25f, 2000f);
        }
    }
}
