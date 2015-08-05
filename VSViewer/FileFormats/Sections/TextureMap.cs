using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VSViewer.FileFormats
{
    class TextureMap
    {
        public int width;
        public int height;
        public Palette colorPalette;
        public byte[] map;

        public TextureMap (int w, int h)
        {
            width = w;
            height = h;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetPaletteMap ()
        { 
            return map;
        }

        /// <summary>
        /// Get the texture as a byte stream
        /// </summary>
        /// <returns>byte[] stream of the texture as BGRA32</returns>
        public byte[] GetColorStream ()
        {
            byte[] buffer = new byte[width * height * 4];
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    int index = (y * width) + x;
                    int c = map[index];

                    // TODO: sometimes c >= colorsPerPalette?? set transparent, for now
                    if (c < colorPalette.GetColorCount())
                    {
                        // swizzled to BGRA
                        buffer[index * 4 + 0] = colorPalette.colors[c][2]; //b
                        buffer[index * 4 + 1] = colorPalette.colors[c][1]; //g
                        buffer[index * 4 + 2] = colorPalette.colors[c][0]; //r
                        buffer[index * 4 + 3] = colorPalette.colors[c][3]; //a
                    }
                    else
                    {
                        Console.WriteLine("Over colors per palette bounds");
                        // defaults to byte[] = {0, 0, 0, 0} transparent
                    }
                }
            }
            return buffer;
        }

        public BitmapSource GetBitmap()
        {
            return BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgra32, null, GetColorStream(), 4 * ((width * 4 + 3) / 4));
        }

        public void Save (string name, string directory = "")
        {
            // write to disk
            using (FileStream stream = new FileStream(name + ".png", FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(GetBitmap()));
                encoder.Save(stream);
            }
        }
    }
}
