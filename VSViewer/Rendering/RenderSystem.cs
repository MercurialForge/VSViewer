using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WPF;
using SharpDX.WPF.Cameras;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.Models;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace VSViewer.Rendering
{
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

    public class RenderSystem : D3D11
    {
        #region Structures

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
        RenderCore core;

        private float m_animFrameTimer;

        public RenderSystem(RenderCore theCore)
        {
            core = theCore;
        }

        // Render system initialize all initialization should be done here.
        public bool Initialize()
        {
            return InitializeShader("VagrantStoryMaterial.fx");
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
                    // updated to SDX 2.6.3 and this is clockwise now.... why?
                    IsFrontCounterClockwise = false,
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
            Camera.SetProjParams(65 * VSTools.Deg2Rad, 1.5f, 25.0f, 10000f);
            // vagrant story is -y up. Probably an artifact of the hardware restrictions.
            Camera.SetViewParams(new Vector3(0.0f, 0.0f, -500.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0, -1, 0));
            return true;
        }

        public override void RenderScene(DrawEventArgs args)
        {
            // fill the back buffer with solid black
            Device.ImmediateContext.ClearRenderTargetView(RenderTargetView, new Color4(0, 0, 0, 1));
            // clear depth buffer
            Device.ImmediateContext.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1f, 0);

            if (core.Actor.Shape == null) { return; }

            UpdateAnimation(args.DeltaTime);

            ApplySkinning();

            UpdateTexture();

            UpdateVertexAndIndiceBuffers();

            SetShaderParameters(args);

            PushShaders();
        }

        private void UpdateAnimation(TimeSpan timeSpan)
        {
            SEQ seq = core.Actor.SEQ;
            Geometry shape = core.Actor.Shape;

            if (seq == null) { return; }
            m_animFrameTimer += (float)timeSpan.Milliseconds;

            if (seq.animations[seq.CurrentAnimationIndex].length <= m_animFrameTimer / 1000)
            {
                m_animFrameTimer = m_animFrameTimer - seq.animations[seq.CurrentAnimationIndex].length * 1000;
            }

            float frameQueryTime = MathUtil.Clamp(m_animFrameTimer / 1000, 0, seq.animations[seq.CurrentAnimationIndex].length);

            for (int i = 0; i < shape.skeleton.Count; i++)
            {
                Frame f = seq.QueryAnimationTime(frameQueryTime, i);
                shape.instancedSkeleton[i].Position = f.Position;
                shape.instancedSkeleton[i].Rotation = f.Rotation;
                shape.instancedSkeleton[i].LocalScale = f.Scale;
            }

        }

        private void ApplySkinning()
        {
            Geometry shape = core.Actor.Shape;

            // Construct the matrices of each bone from it's many parents
            Matrix[] boneTransforms = new Matrix[shape.skeleton.Count];
            for (int i = 0; i < shape.skeleton.Count; i++)
            {
                SkeletalJoint bone = shape.skeleton[i];
                Matrix cumulativeTransform = Matrix.Identity;

                while (bone != null)
                {
                    cumulativeTransform = cumulativeTransform * Matrix.Scaling(bone.LocalScale) * Matrix.RotationQuaternion(bone.Rotation) * Matrix.Translation(bone.Position);
                    bone = (bone.parentIndex != -1) ? shape.skeleton[bone.parentIndex] : null;
                }

                boneTransforms[i] = cumulativeTransform;
            }

            // Each bone is now in it's final position, so we can apply that to the vertices based on their bone weighting.
            // However, in Vagrant Story all vertices have a max influence of 1, so this is very simple.
            Vector3[] temporarySkinnedVertices = new Vector3[shape.vertices.Count];
            for (int v = 0; v < shape.vertices.Count; v++)
            {
                temporarySkinnedVertices[v] = Vector3.TransformCoordinate(shape.vertices[v], boneTransforms[shape.jointID[v]]);
            }

            InterleaveVerticesWithUVs(temporarySkinnedVertices);
        }

        private void UpdateTexture()
        {
            //if (!core.RenderRequiresUpdate) { return; }
        }

        private void UpdateVertexAndIndiceBuffers()
        {
            if (!core.RenderRequiresUpdate) { return; }
            core.RenderRequiresUpdate = false;

            Set(ref m_textureResourceView, new ShaderResourceView(Device, core.Actor.Shape.Textures[0].GetTexture2D(Device)));
            Device.ImmediateContext.PixelShader.SetSampler(0, m_samplerState);

            // Setup vertex buffer
            Set(ref vertexBuffer, DXUtils.CreateBuffer(Device, core.Actor.Shape.instancedVertices));
            VertexBufferBinding vertexBufferBinding = new VertexBufferBinding(vertexBuffer, InputVertex.SizeInBytes, 0);
            Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

            // Setup index buffer
            Set(ref indexBuffer, DXUtils.CreateBuffer(Device, core.Actor.Shape.indices.ToArray()));
            Device.ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
        }

        private void InterleaveVerticesWithUVs(Vector3[] vertices)
        {
            Geometry shape = core.Actor.Shape;

            for (int i = 0; i < shape.instancedVertices.Length; i++)
            {
                shape.instancedVertices[i] = new InputVertex(vertices[i], shape.uv1[i]);
            }
        }

        private void SetShaderParameters(DrawEventArgs args)
        {
            Geometry shape = core.Actor.Shape;

            // Rotate the object for debugging
            float t = (float)args.TotalTime.TotalSeconds;
            var g_World = Matrix.RotationY(0);
            //core.Actor.Rotation = Quaternion.RotationAxis(VSTools.UnitY, t);

            // Update matrices in the constant buffer
            Matrix modelMatrix = Matrix.Scaling(core.Actor.LocalScale) * Matrix.RotationQuaternion(core.Actor.Rotation) * Matrix.Translation(core.Actor.Position);
            MatrixBuffer projectionModel = new MatrixBuffer
            {
                World = Matrix.Transpose(modelMatrix),
                View = Matrix.Transpose((Camera.View)),
                Projection = Matrix.Transpose(Camera.Projection)
            };
            m_matrixBuffer.Value = projectionModel;

            // Set Vertex shader resources
            Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_matrixBuffer.Buffer);
            Device.ImmediateContext.UpdateSubresource(shape.instancedVertices, vertexBuffer);

            // Set pixel shader resources
            Device.ImmediateContext.PixelShader.SetShaderResource(0, m_textureResourceView);
        }

        private void PushShaders()
        {
            // Set the shaders to be used this frame
            Device.ImmediateContext.VertexShader.Set(m_vertexShader);
            Device.ImmediateContext.PixelShader.Set(m_pixelShader);

            Device.ImmediateContext.DrawIndexed(core.Actor.Shape.indices.Count, 0, 0);
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
            Camera.SetProjParams(65 * VSTools.Deg2Rad, w / (float)h, 25f, 10000f);
        }
    }
}
