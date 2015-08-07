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

namespace VSViewer.Rendering
{

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Projections
    {
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SimpleVertex
    {
        public SimpleVertex(Vector3 p, Vector2 t)
        {
            Pos = p;
            Tex = t;
        }
        public Vector3 Pos;
        public Vector2 Tex;

        public const int SizeInBytes = (3 + 2) * 4;
    }

    class RenderSystem : D3D11
    {
        private VertexShader m_vertexShader;
        private PixelShader m_pixelShader;
        private ConstantBuffer<Projections> m_constantProjectionBuffer;
        private ShaderResourceView m_textureRV;
        private SamplerState m_samplerLinear;

        private Geometry geometry;
        private SimpleVertex[] m_skinnedVertices;
        private WEP wep;

        public void SetCamera(BaseCamera newCamera) { Camera = newCamera; }

        public void Load()
        {
            string file = @"C:\Users\Oliver\Desktop\VSDump\OBJ\41.WEP";
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                // should this return a Geometry, should they all return Geometries? But then where do I keep others like textures etc?
                wep = WEPLoader.FromStream(reader);
                wep.textures[0].Save("Bronze");
                geometry = VSTools.CreateGeometry(wep.vertices, wep.polygons, wep.joints, wep.textures[0]);

            }
        }

        public RenderSystem()
        {
            using (DisposeGroup dg = new DisposeGroup())
            {
                // --- init shaders
                ShaderFlags sFlags = ShaderFlags.EnableStrictness;

                #if DEBUG
                sFlags |= ShaderFlags.Debug;
                #endif

                CompilationResult pVSBlob = dg.Add(ShaderBytecode.CompileFromFile("VagrantStoryMaterial.fx", "VS", "vs_4_0", sFlags, EffectFlags.None));
                ShaderSignature inputSignature = dg.Add(ShaderSignature.GetInputSignature(pVSBlob));
                Set(ref m_vertexShader, new VertexShader(Device, pVSBlob));

                // --- let DX know about the pixels memory layout
                InputLayout layout = dg.Add(new InputLayout(Device, inputSignature, new[]{
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0),
				}));
                Device.ImmediateContext.InputAssembler.InputLayout = (layout);

                CompilationResult pPSBlob = dg.Add(ShaderBytecode.CompileFromFile("VagrantStoryMaterial.fx", "PS", "ps_4_0", sFlags, EffectFlags.None));
                Set(ref m_pixelShader, new PixelShader(Device, pPSBlob));

                Load();
                ApplySkinning();

                // --- init vertices
                var vertexBuffer = dg.Add(DXUtils.CreateBuffer(Device, m_skinnedVertices));
                Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, SimpleVertex.SizeInBytes, 0));
                // --- init indices
                var g_pIndexBuffer = dg.Add(DXUtils.CreateBuffer(Device, geometry.indices.ToArray() ));
                Device.ImmediateContext.InputAssembler.SetIndexBuffer(g_pIndexBuffer, Format.R16_UInt, 0);
                Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                // --- create the constant buffer
                m_constantProjectionBuffer = new ConstantBuffer<Projections>(Device);
                Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_constantProjectionBuffer.Buffer);

                RasterizerStateDescription RSD = new RasterizerStateDescription();
                RSD.CullMode = CullMode.Back;
                RSD.IsFrontCounterClockwise = false;
                RSD.FillMode = FillMode.Solid;

                RasterizerState RS = new RasterizerState(Device, ref RSD);
                Device.ImmediateContext.Rasterizer.State = RS;

                m_textureRV = ShaderResourceView.FromFile(Device, @"D:\Projects\C#\VSViewer\VSViewer\bin\Debug\Bronze.png");

                m_samplerLinear = new SamplerState(Device, new SamplerStateDescription
                {
                    Filter = Filter.MinMagMipPoint,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    ComparisonFunction = Comparison.Never,
                    MinimumLod = 0,
                    MaximumLod = 0,
                });

            }
            Camera = new FirstPersonCamera();
            Camera.EnableYAxisMovement = false;
            Camera.SetProjParams(90, 1, 0.01f, 2000);
            Camera.SetViewParams(new Vector3(0.0f, 0.0f, -5.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0, 0, 1));
        }

        public override void Reset(int w, int h)
        {
            base.Reset(w, h);
            Camera.SetProjParams(90 * Deg2Rad, w / (float)h, 0.01f, 2000);
        }

        public const float Rad2Deg = (float)(180.0 / Math.PI);
        public const float Deg2Rad = (float)(Math.PI / 180.0);

        public override void RenderScene(DrawEventArgs args)
        {
            // fill the back buffer with solid black
            m_device.ImmediateContext.ClearRenderTargetView(RenderTargetView, new Color4(0, 0, 0, 1));

            // rotate the object for debugging
            float t = (float)args.TotalTime.TotalSeconds;
            var g_World = Matrix.RotationY(0);


            // update vertex shader projections MVP
            Projections projectionModel = new Projections();
            projectionModel.World = Matrix.Transpose(g_World);
            projectionModel.View = Matrix.Transpose(Camera.View);
            projectionModel.Projection = Matrix.Transpose(Camera.Projection);
            m_constantProjectionBuffer.Value = projectionModel;

            ApplySkinning();

            m_device.ImmediateContext.VertexShader.Set(m_vertexShader);
            m_device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_constantProjectionBuffer.Buffer);
            m_device.ImmediateContext.PixelShader.Set(m_pixelShader);
            Device.ImmediateContext.PixelShader.SetShaderResource(0, m_textureRV);
            Device.ImmediateContext.PixelShader.SetSampler(0, m_samplerLinear);
            m_device.ImmediateContext.DrawIndexed(geometry.indices.Count, 0 ,0);
        }

        private void ApplySkinning()
        {
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

            // Each boneCopy is now in it's final position, so we can apply that to the vertexes based on their bone weighting.
            // However, vertex positions have already been uploaded once, so we're uh... going to hack it and re-upload them.
            Vector3[] origVerts = geometry.vertices.ToArray();
            Vector3[] skinnedVertices = new Vector3[geometry.vertices.Count];
            Array.Copy(origVerts, skinnedVertices, origVerts.Length);
            int g = 0;

            for (int v = 0; v < skinnedVertices.Length; v++)
            {

                Matrix finalMatrix = Matrix.Zero;
                Matrix boneInfluence = boneTransforms[geometry.boneID[v]];
                finalMatrix = (boneInfluence * 1) + finalMatrix;

                skinnedVertices[v] = Vector3.TransformCoordinate(skinnedVertices[v], finalMatrix);
            }

            // Now re-assign our Vertices to the mesh so they get uploaded to the GPU...
            InterleavedBuffer(skinnedVertices, geometry.uv1);
        }

        private void InterleavedBuffer(Vector3[] vertices, List<Vector2> uvs)
        {
            SimpleVertex[] sva = new SimpleVertex[vertices.Length];
            for (int i = 0; i < vertices.Length; i++ )
            {
                sva[i] = new SimpleVertex(vertices[i], uvs[i]);
            }
            m_skinnedVertices = sva;
        }

        private void DrawMap()
        {

        }

        private void DrawScene()
        {

        }

        public void UnloadAll()
        {

        }

        private void DrawDebugShapes()
        {

        }

        private void DrawMesh()
        {

        }

        public void SetOutputSize()
        {

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // NOTE: SharpDX 1.3 requires explicit Dispose() of everything
            Set(ref m_vertexShader, null);
            Set(ref m_pixelShader, null);
            Set(ref m_constantProjectionBuffer, null);
        }

    }
}
