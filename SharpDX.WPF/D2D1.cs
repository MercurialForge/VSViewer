using System.Collections.Generic;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using FactoryDW = SharpDX.DirectWrite.Factory;
using Factory = SharpDX.Direct2D1.Factory;
namespace SharpDX.WPF
{
	/// <summary>
	/// This supports both D3D10 and D2D1 rendering
	/// </summary>
	public class D2D1 : D3D10
	{
        /// <summary>
        /// 
        /// </summary>
		public D2D1()
			: this(null)
		{
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
		public D2D1(SharpDX.Direct3D10.Device1 device)
			: base(device)
		{
			m_factory2D = new SharpDX.Direct2D1.Factory();
			m_factoryDW = new FactoryDW();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			// NOTE: SharpDX 1.3 requires explicit Dispose() of everything
			Set(ref m_renderTarget2D, null);
			Set(ref m_factory2D, null);
			Set(ref m_factoryDW, null);
		}

        /// <summary>
        /// 
        /// </summary>
		public FactoryDW FactoryDW { get { return m_factoryDW; } }
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
		public override void Reset(int w, int h)
		{
			base.Reset(w, h);

			using(Surface surface = RenderTarget.QueryInterface<Surface>())
				Set(ref m_renderTarget2D, new RenderTarget(
					m_factory2D, 
					surface,
					new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied))
				));
			m_renderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;
			Device.OutputMerger.SetTargets(RenderTargetView);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
		public override void BeginRender(DrawEventArgs args)
		{
			base.BeginRender(args);
			m_renderTarget2D.BeginDraw();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
		public override void EndRender(DrawEventArgs args)
		{
			m_renderTarget2D.EndDraw();
			base.EndRender(args);
		}

        /// <summary>
        /// 
        /// </summary>
		public RenderTarget RenderTarget2D { get { return Prepared(ref m_renderTarget2D); } }


        private RenderTarget m_renderTarget2D;
        private Factory m_factory2D;
        private FactoryDW m_factoryDW;

	}
}
