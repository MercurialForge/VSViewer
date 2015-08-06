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
using VSViewer.RenderSystem;

namespace VSViewer
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IDirect3D D3D11Renderer { get; private set; }

        private RenderCore m_renderCore;
        private System.Windows.Forms.Timer m_intervalTimer;

        public MainWindowViewModel()
        {
            // D3D11Renderer = new ViewportViewModel();
            // D3D11Renderer = new D3D11();
            //((D3D11)D3D11Renderer).Device
            EnableRenderCore();
            // .net timer 
        }

        public void EnableRenderCore ()
        {
            m_renderCore = new RenderCore();
            m_intervalTimer = new System.Windows.Forms.Timer();
            m_intervalTimer.Interval = 16; // 60 FPS roughly
            m_intervalTimer.Enabled = true;
            m_intervalTimer.Tick += (args, o) =>
            {
                //m_renderCore.Tick();
                Console.WriteLine(System.Environment.TickCount);
            };
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

            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                WEP wep = WEPLoader.FromStream(reader);
                wep.textures[0].Save("Bronze");
            }
        }
    }
}
