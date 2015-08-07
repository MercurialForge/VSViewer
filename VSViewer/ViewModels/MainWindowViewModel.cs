using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using System.IO;
using VSViewer.FileFormats;
using VSViewer.Loader;
using SharpDX.WPF;
using SharpDX.Direct3D11;
using VSViewer.Rendering;
using System.Windows;

namespace VSViewer
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public RenderSystem Viewport { get; private set; }

        public MainWindowViewModel()
        {
            Viewport = new RenderSystem();
            if(!Viewport.Initialize())
            {
                Application.Current.Shutdown();
            }
        }

        public void Read_Click()
        {
            Viewport.Load();
            //string file = "";

            //// open file
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //if (openFileDialog.ShowDialog() == true)
            //{
            //    file = openFileDialog.FileName;
            //}

            //using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            //{
            //    WEP wep = WEPLoader.FromStream(reader);
            //    wep.textures[0].Save("Bronze");
            //}
        }
    }
}
