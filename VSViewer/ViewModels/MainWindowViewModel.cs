using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using System.IO;

namespace VSViewer.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            Console.WriteLine("hello from the view model");
        }

        public void Read_Click()
        {
            string file = "";

            // open file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                file = openFileDialog.FileName;
            }

            // load file into memory stream
            FileStream fileStream = File.OpenRead(file);
            MemoryStream memStream = new MemoryStream();
            memStream.SetLength(fileStream.Length);
            fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);

            // create endian reader
            EndianBinaryReader reader = new EndianBinaryReader(memStream, Endian.Big);

            //being reader

            // Magic H01 + 0x00
            reader.SkipInt32();
            Console.WriteLine(" ");
            Console.WriteLine((int)reader.ReadByte());
            Console.WriteLine((int)reader.ReadByte());
            Console.WriteLine(reader.ReadUInt16());

            fileStream.Close();
            memStream.Close();
            reader.Close();

        }
    }
}
