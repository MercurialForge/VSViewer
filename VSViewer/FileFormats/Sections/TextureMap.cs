using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VSViewer.FileFormats
{
    public class TextureMap
    {
        public ushort Width
        {
            private set { m_width = value; }
            get { return m_width; }
        }

        public ushort Height
        {
            private set { m_height = value; }
            get { return m_height; }
        }

        public Palette ColorPallette
        {
            set { m_colorPalette = value; }
            get { return m_colorPalette; }
        }

        public int PixelCount
        {
            get { return m_width * m_height * 4; }
        }

        public BitmapSource Bitmap
        {
            get { return m_bitmap; }
            set
            {
                m_bitmap = value;
            }
        }

        public Palette m_colorPalette;
        public byte[] map;

        SamplerStateDescription m_samplerDesc;
        ushort m_width;
        ushort m_height;
        private BitmapSource m_bitmap;

        public TextureMap (int w, int h)
        {
            m_width = (ushort)w;
            m_height = (ushort)h;

            // Create a texture sampler default state description.
            m_samplerDesc = new SamplerStateDescription
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                ComparisonFunction = Comparison.Never,
                MinimumLod = 0,
                MaximumLod = 0,
            };
        }

        public Texture2D GetTexture2D(Device device)
        {
            IntPtr unmanagedPtr = Marshal.AllocHGlobal(PixelCount);
            Marshal.Copy(GetPixelData(), 0, unmanagedPtr, PixelCount);
            DataRectangle data = new DataRectangle();
            data.DataPointer = unmanagedPtr;
            data.Pitch = Width * 4;

            Texture2DDescription textureDesc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Dynamic,
                CpuAccessFlags = CpuAccessFlags.Write,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Height = Height,
                Width = Width,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
            };

            Texture2D texture = new Texture2D(device, textureDesc, data);
            Marshal.FreeHGlobal(unmanagedPtr);
            return texture;
        }

        public byte[] GetPaletteMap ()
        { 
            return map;
        }

        /// <summary>
        /// Get the texture as a byte stream
        /// </summary>
        /// <returns>byte[] stream of the texture as BGRA32</returns>
        public byte[] GetPixelData ()
        {
            byte[] buffer = new byte[m_width * m_height * 4];
            for (int y = 0; y < m_height; ++y)
            {
                for (int x = 0; x < m_width; ++x)
                {
                    int index = (y * m_width) + x;
                    int c = map[index];

                    if (c < m_colorPalette.GetColorCount())
                    {
                        // swizzled to BGRA
                        buffer[index * 4 + 0] = m_colorPalette.colors[c][2]; //b
                        buffer[index * 4 + 1] = m_colorPalette.colors[c][1]; //g
                        buffer[index * 4 + 2] = m_colorPalette.colors[c][0]; //r
                        buffer[index * 4 + 3] = m_colorPalette.colors[c][3]; //a
                    }
                    else
                    {
                        // A fall back for items 0x70 - 0x7F which do not support the final implemented palette format
                        // I have reason to believe these items were created first since they no longer follow the
                        // conventional format of the rest.
                        buffer[index * 4 + 0] = (byte)204;
                        buffer[index * 4 + 1] = (byte)102;
                        buffer[index * 4 + 2] = (byte)255;
                        buffer[index * 4 + 3] = (byte)255;
                    }
                }
            }
            return buffer;
        }

        public BitmapSource GetBitmap()
        {
            return BitmapSource.Create(m_width, m_height, 96d, 96d, PixelFormats.Bgra32, null, GetPixelData(), 4 * ((m_width * 4 + 3) / 4));
        }

        public void Save (string name, string directory = "")
        {
            // write to disk
            using (FileStream stream = new FileStream(directory + name + ".png", FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                Bitmap = GetBitmap();
                encoder.Frames.Add(BitmapFrame.Create(Bitmap));
                encoder.Save(stream);
            }
        }
    }
}
