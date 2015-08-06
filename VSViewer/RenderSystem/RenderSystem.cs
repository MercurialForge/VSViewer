using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VSViewer.FileFormats;
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

    class RenderSystem
    {
        public BaseCamera camera;

        private Device device; 
        private VertexShader vertexShader;
        private PixelShader pixelShader;
        private ConstantBuffer<Projections> m_pConstantBuffer;
        private Matrix m_Projection;

        private Geometry geometry;

        public RenderSystem(Device d)
        {
            device = d;
            camera = new ModelViewerCamera();
        }

        public RenderSystem(Device d, BaseCamera cam)
        {
            device = d;
            camera = cam;
        }

        public void InitializeSystem()
        {
            using (DisposeGroup dg = new DisposeGroup())
            {
                // --- init shaders
                ShaderFlags sFlags = ShaderFlags.EnableStrictness;

                #if DEBUG
                sFlags |= ShaderFlags.Debug;
                #endif

                CompilationResult pVSBlob = dg.Add(ShaderBytecode.CompileFromFile("T2_SimpleTriangle.fx", "VShader", "vs_4_0", sFlags, EffectFlags.None));
                ShaderSignature inputSignature = dg.Add(ShaderSignature.GetInputSignature(pVSBlob));
                vertexShader = new VertexShader(device, pVSBlob);

                CompilationResult pPSBlob = dg.Add(ShaderBytecode.CompileFromFile("T2_SimpleTriangle.fx", "PShader", "ps_4_0", sFlags, EffectFlags.None));
                pixelShader = new PixelShader(device, pPSBlob);

                // --- let DX know about the pixels memory layout
                InputLayout layout = dg.Add(new InputLayout(device, inputSignature, new[]{
					new InputElement("VERTEX", 0, Format.R32G32B32_Float, 0),
				}));
                device.ImmediateContext.InputAssembler.InputLayout = (layout);

                // --- init vertices
                var vertexBuffer = dg.Add(DXUtils.CreateBuffer(device, geometry.vertices.ToArray()));
                device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Vector3.SizeInBytes, 0));
                // --- init indices
                var g_pIndexBuffer = dg.Add(DXUtils.CreateBuffer(device, geometry.indices.ToArray()));
                device.ImmediateContext.InputAssembler.SetIndexBuffer(g_pIndexBuffer, Format.R16_UInt, 0);
                device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
                // --- create the constant buffer
                m_pConstantBuffer = new ConstantBuffer<Projections>(device);
                device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_pConstantBuffer.Buffer);
            }
            camera.SetProjParams((float)Math.PI / 2, 1, 0.01f, 2000);
            camera.SetViewParams(new Vector3(0.0f, 0.0f, -5.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0, 0, 1));
        }

        public void ShutdownSystem()
        {

        }

        internal void RenderFrame()
        {

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

    }
}
