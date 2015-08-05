using System;
using System.Collections.Generic;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace SharpDX.WPF
{
    public class D3D11 : D3D
    {
        /// <summary>
        /// 
        /// </summary>
        public D3D11()
            : this((Device)null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minLevel"></param>
        public D3D11(Direct3D.FeatureLevel minLevel)
        {
            m_device = DeviceUtil.Create11(DeviceCreationFlags.BgraSupport, minLevel);
            if (m_device == null)
                throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dev"></param>
        public D3D11(Device dev)
        {
            // REMARK: SharpDX.Direct3D.DriverType.Warp works without graphics card!
            if (dev != null)
            {
                //dev.AddReference();
                m_device = dev;
            }
            else
            {
                m_device = DeviceUtil.Create11(DeviceCreationFlags.BgraSupport);
                if (m_device == null)
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public D3D11(Adapter a)
        {
            if (a == null)
            {
                m_device = DeviceUtil.Create11(DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_0);
                if (m_device == null)
                    throw new NotSupportedException();
            }
            m_device = new Device(a);
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
            Set(ref m_depthStencil, null);
            Set(ref m_depthStencilView, null);
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

            var desc = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = w,
                Height = h,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = RenderTargetOptionFlags,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };
            Set(ref m_renderTarget, new Texture2D(this.m_device, desc));
            Set(ref m_renderTargetView, new RenderTargetView(this.m_device, this.m_renderTarget));

            Set(ref m_depthStencil, DXUtils.CreateTexture2D(this.m_device, w, h, BindFlags.DepthStencil, Format.D24_UNorm_S8_UInt));
            Set(ref m_depthStencilView, new DepthStencilView(this.m_device, m_depthStencil));

            m_device.ImmediateContext.Rasterizer.SetViewports(new Viewport(0, 0, w, h, 0.0f, 1.0f));
            m_device.ImmediateContext.OutputMerger.SetTargets(m_depthStencilView, m_renderTargetView);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public override void BeginRender(DrawEventArgs args)
        {
            m_device.GetOrThrow();
            m_device.ImmediateContext.ClearDepthStencilView(this.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public override void EndRender(DrawEventArgs args)
        {
            m_device.ImmediateContext.Flush();
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

        /// <summary>
        /// 
        /// </summary>
        public ResourceOptionFlags RenderTargetOptionFlags
        {
            get { return m_renderTargetOptionFlags; }
            set
            {
                if (value == m_renderTargetOptionFlags) return;
                m_renderTargetOptionFlags = value;
                OnPropertyChanged("RenderTargetOptionFlags");
            }
        }

        #region Fields

        /// <summary>
        /// must be Shared to be displayed in a D3DImage
        /// but, it seams, the Shared flag makes the WARP device to throw the OutOfMem exception...
        /// </summary>
        private ResourceOptionFlags m_renderTargetOptionFlags = ResourceOptionFlags.Shared;
        protected Device m_device;
        protected Texture2D m_renderTarget;
        protected RenderTargetView m_renderTargetView;
        protected Texture2D m_depthStencil;
        protected DepthStencilView m_depthStencilView;

        #endregion
    }
}
