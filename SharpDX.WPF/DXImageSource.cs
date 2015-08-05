using System;
using System.Windows;
using System.Windows.Interop;
using SharpDX.Direct3D9;

namespace SharpDX.WPF
{
	public class DXImageSource : D3DImage, IDisposable
	{
        /// <summary>
        /// 
        /// </summary>
		public DXImageSource()
		{
			StartD3D9();
		}
		~DXImageSource() { Dispose(false); }

        /// <summary>
        /// 
        /// </summary>
		public void Dispose() { Dispose(true); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
		protected void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			if (disposing)
			{
				SetBackBuffer((Texture)null);
				GC.SuppressFinalize(this);
			}
			EndD3D9();
			m_isDisposed = true;
		}

        /// <summary>
        /// 
        /// </summary>
		public bool IsDisposed { get { return m_isDisposed; } }
	
        /// <summary>
        /// 
        /// </summary>
		public void Invalidate()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(GetType().Name);

			if (m_backBuffer != null)
			{
				Lock();
				AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
				Unlock();
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
		public void SetBackBuffer(SharpDX.Direct3D10.Texture2D texture) { SetBackBuffer(s_d3d9.Device.GetSharedD3D9(texture)); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
		public void SetBackBuffer(SharpDX.Direct3D11.Texture2D texture) { SetBackBuffer(s_d3d9.Device.GetSharedD3D9(texture)); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
		public void SetBackBuffer(Texture texture)
		{
			if (IsDisposed)
				throw new ObjectDisposedException(GetType().Name);

			Texture toDelete = null;
			try
			{
				if (texture != m_backBuffer)
				{
					// if it's from the private (SDX9ImageSource) D3D9 device, dispose of it
					if (m_backBuffer != null && m_backBuffer.Device.NativePointer == s_d3d9.Device.NativePointer)
						toDelete = m_backBuffer;
					m_backBuffer = texture;
				}

				if (texture != null)
				{
					using (Surface surface = texture.GetSurfaceLevel(0))
					{
						Lock();
						SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
						AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
						Unlock();
					}
				}
				else
				{
					Lock();
					SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
					AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
					Unlock();
				}
			}
			finally 
			{
				if (toDelete != null)
				{
					toDelete.Dispose();
				}
			}
		}
    
        /// <summary>
        /// 
        /// </summary>
		private static void StartD3D9()
		{
			if (s_activeClients == 0)
				s_d3d9 = new D3D9();
			s_activeClients++;
		}

        /// <summary>
        /// 
        /// </summary>
		private static void EndD3D9()
		{
			s_activeClients--;
			if (s_activeClients == 0)
				s_d3d9.Dispose();
		}


		private static int s_activeClients;
		private static D3D9 s_d3d9;


        private bool m_isDisposed;
        private Texture m_backBuffer;
		
	}
}
