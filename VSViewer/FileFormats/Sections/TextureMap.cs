using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VSViewer.Common;
using VSViewer.ViewModels;

namespace VSViewer.FileFormats
{
    public class TextureMap
    {
        public int Index { get; set; }

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
            set { ColorPalette = value; }
            get { return ColorPalette; }
        }

        public int PixelCount
        {
            get { return m_width * m_height * 4; }
        }

        public BitmapSource Bitmap
        {
            get { return GetBitmap(); }
        }

        public Palette ColorPalette { get; set; }

        public bool IsPaletteOffset { get; set; }
        public bool IsPaletteLast { get; set; }

        public byte[] map;

        public ICommand TextureSelected
        {
            get { return new RelayCommand(x => SendTextureSelected()); }
        }

        internal void SendTextureSelected()
        {
            MainWindowViewModel.RenderCore.TextureRequiresUpdate = true;
            MainWindowViewModel.RenderCore.TextureIndex = Index;
        }

        SamplerStateDescription m_samplerDesc;
        ushort m_width;
        ushort m_height;

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

                    if (ColorPalette.colors[c][0] != 255)
                    {
                        // offset palette by -[palette color count / 3] to make up for strange wrapping
                        if (IsPaletteOffset)
                        {
                            if (!IsPaletteLast)
                            {
                                c -= ColorPalette.GetColorCount() / 3;
                            }
                            else { c -= (int)(ColorPalette.GetColorCount() / 1.5f); } // if it's the last palette divide by 1.5f
                            if (c < 0)
                            {
                                c += ColorPalette.GetColorCount();
                            }
                        }
                    }

                    if (c < ColorPalette.GetColorCount())
                    {
                        // swizzled to BGRA
                        buffer[index * 4 + 0] = ColorPalette.colors[c][2]; //b
                        buffer[index * 4 + 1] = ColorPalette.colors[c][1]; //g
                        buffer[index * 4 + 2] = ColorPalette.colors[c][0]; //r
                        buffer[index * 4 + 3] = ColorPalette.colors[c][3]; //a
                    }
                    else
                    {
                        // A fall back for items 0x70 - 0x7F which do not support the final implemented palette format
                        // I have reason to believe these items were created first since they no longer follow the
                        // conventional format of the rest.
                        buffer[index * 4 + 0] = (byte)0;
                        buffer[index * 4 + 1] = (byte)255;
                        buffer[index * 4 + 2] = (byte)0;
                        buffer[index * 4 + 3] = (byte)255;
                    }
                    if(map[index] == 0)
                    {
                        buffer[index * 4 + 0] = (byte)0;
                        buffer[index * 4 + 1] = (byte)0;
                        buffer[index * 4 + 2] = (byte)0;
                        buffer[index * 4 + 3] = (byte)0;
                    }
                }
            }
            return buffer;
        }

        public byte[] GetPalettePixels()
        {
            byte[] buffer = new byte[ColorPalette.colors.Count * 4];
            for (int y = 0; y < ColorPalette.colors.Count; ++y)
            {
                        // swizzled to BGRA
                        buffer[y * 4 + 0] = ColorPalette.colors[y][2]; //b
                        buffer[y * 4 + 1] = ColorPalette.colors[y][1]; //g
                        buffer[y * 4 + 2] = ColorPalette.colors[y][0]; //r
                        buffer[y * 4 + 3] = ColorPalette.colors[y][3]; //a
            }
            return buffer;
        }

        private BitmapSource GetBitmap()
        {
            return BitmapSource.Create(m_width, m_height, 96d, 96d, PixelFormats.Bgra32, null, GetPixelData(), 4 * ((m_width * 4 + 3) / 4));
        }

        private BitmapSource GetBitmapPalette()
        {
            return BitmapSource.Create(ColorPalette.colors.Count, 1, 96d, 96d, PixelFormats.Bgra32, null, GetPalettePixels(), 4 * ((ColorPalette.colors.Count * 4 + 3) / 4));
        }

        public void Save (string name, string directory = "")
        {
            // write to disk
            using (FileStream stream = new FileStream(directory + name + ".png", FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(GetBitmap()));
                encoder.Save(stream);
            }

            // write to disk
            using (FileStream stream = new FileStream(directory + name + "Palette.png", FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(GetBitmapPalette()));
                encoder.Save(stream);
            }
        }
    }
}
