using GameFormatReader.Common;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.Loader;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Windows;

namespace VSViewer.Rendering
{
    class RenderSystem : D3D11
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

        private Geometry geometry;
        private InputVertex[] m_instanceVertices;
        private WEP wep;
        int deleteme = 70;
        bool m_isPendingVertexBufferUpdate;

        public void SetCamera(BaseCamera newCamera) { Camera = newCamera; }

        public void Load()
        {
            string file = @"C:\Users\Oliver\Desktop\VSDump\OBJ\" + deleteme.ToString("X2") + ".WEP";
            Console.WriteLine(deleteme.ToString("X2"));
            deleteme++;

            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                // should this return a Geometry, should they all return Geometries? But then where do I keep others like textures etc?
                wep = WEPLoader.FromStream(reader);
                wep.textures[0].Save("Bronze");
                geometry = VSTools.CreateGeometry(wep.vertices, wep.polygons, wep.joints, wep.textures[0]);
                m_instanceVertices = new InputVertex[geometry.vertices.Count];
            }
            m_isPendingVertexBufferUpdate = true;
        }

        // Render system initialize all initialization should be done here.
        public bool Initialize()
        {
            Load();
            ApplySkinning();
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

                // Create constant matrix buffer
                m_matrixBuffer = new ConstantBuffer<MatrixBuffer>(Device);
                Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_matrixBuffer.Buffer);

                // Create texture resource.
                m_textureResourceView = new ShaderResourceView(Device, wep.textures[0].GetTexture2D(Device));

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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing shader. Error is " + ex.Message);
                return false;
            };

            Camera = new FirstPersonCamera();
            Camera.EnableYAxisMovement = false;
            Camera.SetProjParams(65, 1, 0.01f, 2000);
            Camera.SetViewParams(new Vector3(0.0f, 0.0f, -5.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0, 0, -1));
            return true;
        }

        public override void RenderScene(DrawEventArgs args)
        {
            // fill the back buffer with solid black
            Device.ImmediateContext.ClearRenderTargetView(RenderTargetView, new Color4(0, 0, 0, 1));

            ApplySkinning();

            SetShaderParameters(args);

            UpdateVertexAndIndiceBuffers();

            PushShaders();
        }

        private void UpdateVertexAndIndiceBuffers()
        {
            if (!m_isPendingVertexBufferUpdate) { return; }

            Set(ref m_textureResourceView, new ShaderResourceView(Device, wep.textures[0].GetTexture2D(Device)));

            // Setup vertex buffer
            vertexBuffer = DXUtils.CreateBuffer(Device, m_instanceVertices);
            VertexBufferBinding vertexBufferBinding = new VertexBufferBinding(vertexBuffer, InputVertex.SizeInBytes, 0);
            Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

            // Setup index buffer
            indexBuffer = DXUtils.CreateBuffer(Device, geometry.indices.ToArray());
            Device.ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
            m_isPendingVertexBufferUpdate = false;
        }

        private void ApplySkinning()
        {
            // Construct the matrices of each bone from it's many parents
            Matrix[] boneTransforms = new Matrix[geometry.skeleton.Count];
            for (int i = 0; i < geometry.skeleton.Count; i++)
            {
                SkeletalBone bone = geometry.skeleton[i];
                Matrix cumulativeTransform = Matrix.Identity;

                while (bone != null)
                {
                    cumulativeTransform = cumulativeTransform * Matrix.Scaling(bone.scale) * Matrix.RotationQuaternion(bone.quaternion) * Matrix.Translation(bone.position);
                    bone = (bone.parentIndex != -1) ? geometry.skeleton[bone.parentIndex] : null;
                }

                boneTransforms[i] = cumulativeTransform;
            }

            // Each bone is now in it's final position, so we can apply that to the vertices based on their bone weighting.
            // However, in Vagrant Story all vertices have a max influence of 1, so this is very simple.
            Vector3[] temporarySkinnedVertices = new Vector3[geometry.vertices.Count];
            for (int v = 0; v < m_instanceVertices.Length; v++)
            {
                temporarySkinnedVertices[v] = Vector3.TransformCoordinate(geometry.vertices[v], boneTransforms[geometry.boneID[v]]);
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
            var g_World = Matrix.RotationY(t);

            // Update matrices in the constant buffer
            MatrixBuffer projectionModel = new MatrixBuffer
            {
                World = Matrix.Transpose(g_World),
                View = Matrix.Transpose(Camera.View),
                Projection = Matrix.Transpose(Camera.Projection)
            };
            m_matrixBuffer.Value = projectionModel;

            // Set Vertex shader resources
            Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_matrixBuffer.Buffer);

            // Set pixel shader resources
            Device.ImmediateContext.PixelShader.SetShaderResource(0, m_textureResourceView);
        }

        private void PushShaders()
        {
            // Set the shaders to be used this frame
            Device.ImmediateContext.VertexShader.Set(m_vertexShader);
            Device.ImmediateContext.PixelShader.Set(m_pixelShader);

            Device.ImmediateContext.PixelShader.SetSampler(0, m_samplerState);
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
            Camera.SetProjParams(90 * VSTools.Deg2Rad, w / (float)h, 0.01f, 2000);
        }
    }
}
