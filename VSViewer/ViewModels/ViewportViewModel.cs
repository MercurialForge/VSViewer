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
using VSViewer.FileFormats;
using VSViewer.Loader;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace VSViewer
{

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Projections
    {
        public Matrix World;

        public Matrix View;

        public Matrix Projection;
    }

    public class ViewportViewModel : D3D11
    {
        
		VertexShader g_pVertexShader;
		PixelShader g_pPixelShader;
        WEP wep;
        Geometry geo;
        private ConstantBuffer<Projections> m_pConstantBuffer;
        private Matrix m_Projection;

        protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			// NOTE: SharpDX 1.3 requires explicit Dispose() of everything
			Set(ref g_pVertexShader, null);
			Set(ref g_pPixelShader, null);
            Set(ref m_pConstantBuffer, null);
		}

        public void Load ()
        {
            string file = "E:/CloudServices/GoogleDrive/VSTools/WEPs/44.WEP";
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                wep = WEPLoader.FromStream(reader);
                wep.textures[0].Save("Bronze");
            }
            geo = VSTools.CreateGeometry(wep.vertices, wep.polygons, wep.textures[0]);
        }

        public ViewportViewModel()
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
				g_pVertexShader = new VertexShader(Device, pVSBlob);

                CompilationResult pPSBlob = dg.Add(ShaderBytecode.CompileFromFile("T2_SimpleTriangle.fx", "PShader", "ps_4_0", sFlags, EffectFlags.None));
				g_pPixelShader = new PixelShader(Device, pPSBlob);

				// --- let DX know about the pixels memory layout
				InputLayout layout = dg.Add(new InputLayout(Device, inputSignature, new[]{
					new InputElement("VERTEX", 0, Format.R32G32B32_Float, 0),
				}));
				Device.ImmediateContext.InputAssembler.InputLayout = (layout);

                Load();
				// --- init vertices
				var vertexBuffer = dg.Add(DXUtils.CreateBuffer(Device, geo.vertices.ToArray()));
                Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Vector3.SizeInBytes, 0));
                // --- init indices
                var g_pIndexBuffer = dg.Add(DXUtils.CreateBuffer(Device, geo.indices.ToArray()));
                Device.ImmediateContext.InputAssembler.SetIndexBuffer(g_pIndexBuffer, Format.R16_UInt, 0);
                Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

                /// --- create the constant buffer
                m_pConstantBuffer = new ConstantBuffer<Projections>(Device);
                Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_pConstantBuffer.Buffer);
			}
            Camera = new ModelViewerCamera();
            Camera.SetProjParams((float)Math.PI / 2, 1, 0.01f, 2000);
            Camera.SetViewParams(new Vector3(0.0f, 0.0f, -5.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0, 0, 1));
		}

        /// <summary>
        /// 
        /// </summary>
        public override void Reset(int w, int h)
        {
            base.Reset(w, h);
            m_Projection = Matrix.PerspectiveFovLH((float)Math.PI / 2, w / (float)h, 0.01f, 2000.0f);
        }

		public override void RenderScene(DrawEventArgs args)
		{

            //
            // Animate the cube
            //
            float t = (float)args.TotalTime.TotalSeconds;
            var g_World = Matrix.RotationY(t);

			Device.ImmediateContext.ClearRenderTargetView(this.RenderTargetView, new Color4(0,0,0,1));

            //
            // Update variables
            //
            m_pConstantBuffer.Value = new Projections
            {
                World = Matrix.Transpose(g_World),
                View = Matrix.Transpose(Camera.View),
                Projection = Matrix.Transpose(m_Projection),
            };

			Device.ImmediateContext.VertexShader.Set(g_pVertexShader);
            Device.ImmediateContext.VertexShader.SetConstantBuffer(0, m_pConstantBuffer.Buffer);
			Device.ImmediateContext.PixelShader.Set(g_pPixelShader);
			Device.ImmediateContext.Draw(geo.indices.Count, 0);
		}
    }
}
