﻿using SharpDX;
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
        #region Properties

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

        public BitmapSource Bitmap
        {
            get { return GetBitmap(); }
        }

        public Palette ColorPalette { get; set; }

        public byte[] map;

        private int PixelCount { get { return Width * Height * 4; } } 

        #endregion

        #region Private Fields

        SamplerStateDescription m_samplerDesc;
        ushort m_width;
        ushort m_height; 

        #endregion

        public TextureMap(int w, int h)
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

        #region Methods

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

        public void SaveToDisk(string name, string directory = "")
        {
            // write to disk
            using (FileStream stream = new FileStream(directory + name + ".png", FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(GetBitmap()));
                encoder.Save(stream);
            }
        }

        private byte[] GetPixelData()
        {
            byte[] buffer = new byte[m_width * m_height * 4];
            for (int y = 0; y < m_height; ++y)
            {
                for (int x = 0; x < m_width; ++x)
                {
                    int index = (y * m_width) + x;
                    int c = map[index];

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
                        // Make fully green if out of range. // should be black in final build or transparent.
                        buffer[index * 4 + 0] = (byte)0;    //b
                        buffer[index * 4 + 1] = (byte)255;  //g
                        buffer[index * 4 + 2] = (byte)0;    //r
                        buffer[index * 4 + 3] = (byte)255;  //a
                    }

                    // CLUT index of 0 is Fully Transparent Alpha
                    if (map[index] == 0)
                    {
                        buffer[index * 4 + 0] = (byte)0;    //b
                        buffer[index * 4 + 1] = (byte)0;    //g
                        buffer[index * 4 + 2] = (byte)0;    //r
                        buffer[index * 4 + 3] = (byte)0;    //a
                    }
                }
            }
            return buffer;
        }

        private BitmapSource GetBitmap()
        {
            return BitmapSource.Create(m_width, m_height, 96d, 96d, PixelFormats.Bgra32, null, GetPixelData(), 4 * ((m_width * 4 + 3) / 4));
        } 

        #endregion
    }
}
