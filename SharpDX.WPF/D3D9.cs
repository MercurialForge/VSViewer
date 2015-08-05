using System;
using System.Runtime.InteropServices;
using SharpDX.Direct3D9;

namespace SharpDX.WPF
{
    public class D3D9 : D3D
    {
        /// <summary>
        /// 
        /// </summary>
        public D3D9()
            : this(null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public D3D9(DeviceEx device) 
        {
            if (device != null)
            {
                //context = ???
                throw new NotSupportedException("dunno how to get the context");

                //this.device = device;
                //device.AddReference();
            }
            else
            {
                m_context = new Direct3DEx();

                PresentParameters presentparams = new PresentParameters();
                presentparams.Windowed = true;
                presentparams.SwapEffect = SwapEffect.Discard;
                presentparams.DeviceWindowHandle = GetDesktopWindow();
                presentparams.PresentationInterval = PresentInterval.Default;
                this.m_device = new DeviceEx(m_context, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve, presentparams);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Set(ref m_device, null);
                Set(ref m_context, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDisposed { get { return m_device == null; } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        /// <summary>
        /// 
        /// </summary>
        public DeviceEx Device { get { return m_device.GetOrThrow(); } }

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

            Set(ref m_renderTarget, new Texture(this.m_device, w, h, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default));

            // TODO test that...
            using (var surface = m_renderTarget.GetSurfaceLevel(0))
                m_device.SetRenderTarget(0, surface);
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
        public Texture RenderTarget { get { return Prepared(ref m_renderTarget); } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dximage"></param>
        public override void SetBackBuffer(DXImageSource dximage) { dximage.SetBackBuffer(RenderTarget); }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override System.Windows.Media.Imaging.WriteableBitmap ToImage() { throw new NotImplementedException(); }


        protected Direct3DEx m_context;
        protected DeviceEx m_device;
        private Texture m_renderTarget;
    }
}
