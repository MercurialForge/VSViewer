using System;
using System.Collections.Generic;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D10.Device;

namespace SharpDX.WPF
{
    public class D3D10 : D3D
    {
        /// <summary>
        /// 
        /// </summary>
        public D3D10()
            : this(null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minLevel"></param>
        public D3D10(Direct3D10.FeatureLevel minLevel)
        {
            m_device = DeviceUtil.Create10(DeviceCreationFlags.BgraSupport, minLevel);
            if (m_device == null)
                throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dev"></param>
        public D3D10(Device dev)
        {
            if (dev != null)
            {
                //dev.AddReference();
                this.m_device = dev;
            }
            else
            {
                m_device = DeviceUtil.Create10(DeviceCreationFlags.BgraSupport);
                if (m_device == null)
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // NOTE: SharpDX 1.3 requires explicit Dispose() of everything
            Set(ref m_device, null);
            Set(ref m_renderTarget, null);
            Set(ref m_renderTargetView, null);
        }

        /// <summary>
        /// 
        /// </summary>
        public Device Device { get { return m_device.GetOrThrow(); } }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDisposed { get { return m_device == null; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dximage"></param>
        public override void SetBackBuffer(DXImageSource dximage) { dximage.SetBackBuffer(RenderTarget); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public override void Reset(int w, int h)
        {
            m_device.GetOrThrow();

            if (w < 1)
                throw new ArgumentOutOfRangeException("w");
            if (h < 1)
                throw new ArgumentOutOfRangeException("h");

            var colordesc = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = w,
                Height = h,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.Shared,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            Set(ref m_renderTarget, new Texture2D(this.m_device, colordesc));
            Set(ref m_renderTargetView, new RenderTargetView(this.m_device, this.m_renderTarget));

            Set(ref m_depthStencil, DXUtils.CreateTexture2D(this.m_device, w, h, BindFlags.DepthStencil, Format.D24_UNorm_S8_UInt));
            Set(ref m_depthStencilView, new DepthStencilView(this.m_device, m_depthStencil));

            m_device.Rasterizer.SetViewports(new Viewport(0, 0, w, h, 0.0f, 1.0f));
            m_device.OutputMerger.SetRenderTargets(1, new RenderTargetView[] { m_renderTargetView }, m_depthStencilView);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public override void BeginRender(DrawEventArgs args)
        {
            m_device.GetOrThrow();
            m_device.ClearDepthStencilView(this.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public override void EndRender(DrawEventArgs args)
        {
            Device.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        protected T Prepared<T>(ref T property)
        {
            m_device.GetOrThrow();
            if (property == null)
                Reset(1, 1);
            return property;
        }

        /// <summary>
        /// 
        /// </summary>
        public Texture2D RenderTarget { get { return Prepared(ref m_renderTarget); } }

        /// <summary>
        /// 
        /// </summary>
        public RenderTargetView RenderTargetView { get { return Prepared(ref m_renderTargetView); } }

        /// <summary>
        /// 
        /// </summary>
        public Texture2D DepthStencil { get { return Prepared(ref m_depthStencil); } }

        /// <summary>
        /// 
        /// </summary>
        public DepthStencilView DepthStencilView { get { return Prepared(ref m_depthStencilView); } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override System.Windows.Media.Imaging.WriteableBitmap ToImage() { return RenderTarget.GetBitmap(); }

        #region Fields

        protected Device m_device;
        protected Texture2D m_renderTarget;
        protected RenderTargetView m_renderTargetView;
        protected Texture2D m_depthStencil;
        protected DepthStencilView m_depthStencilView;

        #endregion
    }
}
