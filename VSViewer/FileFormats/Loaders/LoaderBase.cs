using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.FileFormats.Loaders
{
    abstract class LoaderBase<T> : IDisposable
    {
        public string file = "";
        public EndianBinaryReader reader;

        public LoaderBase(string path)
        {
            file = path;
            LoadReader();
        }

        protected void LoadReader()
        {
            // load file into memory stream
            using ( FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite) )
            {
                MemoryStream memStream = new MemoryStream((int)fileStream.Length);
                fileStream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                // create endian reader
                reader = new EndianBinaryReader(memStream, Endian.Little);
            }
        }

        public void Dispose()
        {
            reader.Dispose();
            GC.SuppressFinalize(this);
        }

        public abstract T Read();

    }
}
