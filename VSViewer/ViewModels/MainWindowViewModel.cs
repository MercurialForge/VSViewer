using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using System.IO;
using VSViewer.FileFormats.Loaders;
using VSViewer.FileFormats;

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

            using ( LoaderWEP Loader = new LoaderWEP(file) )
            {
                WEP wep = Loader.Read();
            }

        }
    }
}
