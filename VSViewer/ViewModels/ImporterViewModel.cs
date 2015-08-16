using GameFormatReader.Common;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.Loader;
using VSViewer.Models;
using VSViewer.Rendering;

namespace VSViewer.ViewModels
{
    public class ImporterViewModel : ViewModelBase
    {
        #region Properties
        public string MainFileName
        {
            get
            {
                if (m_mainFile == null) { return "No File Chosen"; }
                return m_mainFile.Name;
            }
        }
        public string MainFilePath
        {
            get { return m_mainFile.ToString(); }
            set
            {
                m_mainFile = new FileInfo(value);
                OnPropertyChanged("MainFileName");
            }
        }

        public string SubFileName
        {
            get
            {
                if (m_subFile == null) { return "No File Chosen"; }
                return m_subFile.Name;
            }
        }
        public string SubFilePath
        {
            get { return m_subFile.ToString(); }
            set
            {
                m_subFile = new FileInfo(value);
                OnPropertyChanged("SubFileName");
            }
        }

        bool QueryMainStatus
        {
            get
            {
                if (m_mainFile != null) { return true; }
                return false;
            }
        }
        #endregion

        #region Commands
        public ICommand OnMainFile
        {
            get { return new RelayCommand(x => PrepMainFile()); }
        }

        public ICommand OnSubFile
        {
            get { return new RelayCommand(x => PrepSubFile(), x => QueryMainStatus); }
        }
        #endregion

        #region Private Fields / Properties
        ViewportViewModel m_viewport;
        MainWindowViewModel m_mainWindow;
        FileInfo m_mainFile;
        FileInfo m_subFile;
        bool m_isActive = false;
        ContentBase m_loadedContent;
        AssetBase m_loadedAsset;
        RenderCore core;
        #endregion

        public ImporterViewModel(MainWindowViewModel mainWindowViewModel, RenderCore theCore)
        {
            core = theCore;
            m_mainWindow = mainWindowViewModel;
        }

        #region Debug Code
        public ICommand FindNext
        {
            get { return new RelayCommand(x => FindNextCommand()); }
        }
        internal void FindNextCommand()
        {
            string path = @"E:\CloudServices\GoogleDrive\VSTools\OBJ\" + t.ToString("X2") + ".WEP";
            Console.WriteLine("Viewing WEP:" + t.ToString("X2"));
                core.Actor.SEQ = null;
                LoadShape(path);
                t++;
        }
        int t = 1;
        private void LoadShape(string path)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                // load type
                Geometry wepGeometry = VSTools.CreateGeometry(WEPLoader.FromStream(reader));
                core.Actor = new Actor(wepGeometry);
                core.TextureRequiresUpdate = true;
            }
        } 
        #endregion

        #region Command Methods
        internal void PrepMainFile()
        {
            string path = "";
            if (OpenMainFile(out path))
            {
                MainFilePath = path;
                core.Actor.SEQ = null;
            }
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(MainFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                LoadActor(reader);
            }
        }

        internal void PrepSubFile()
        {
            string path = "";
            if (OpenSubFile(out path))
            {
                SubFilePath = path;
            }
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(SubFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                if (m_subFile.Extension == ".SEQ")
                {
                    if (LoadAsset(reader)) { return; }
                    else 
                    {
                        SubFilePath = "No File Chosen";
                        MessageBox.Show("A SEQ cannot be applied.", "Warning");
                    }
                }
            }
        }
        #endregion

        #region Local Methods
        private void LoadActor(EndianBinaryReader reader)
        {
            switch (m_mainFile.Extension)
            {
                case ".WEP":
                    Geometry wepGeometry = VSTools.CreateGeometry(WEPLoader.FromStream(reader));
                    core.Actor = new Actor(wepGeometry);
                    core.TextureRequiresUpdate = true;
                    break;

                case ".SHP":
                    Geometry shpGeometry = VSTools.CreateGeometry(SHPLoader.FromStream(reader));
                    if (core.TextureIndex >= 2) { core.TextureIndex = 0; }
                    core.Actor = new Actor(shpGeometry);
                    core.TextureRequiresUpdate = true;
                    break;
            }
        }

        private bool LoadAsset(EndianBinaryReader reader)
        {
            if (core.Actor.Shape != null)
            {
                if (core.Actor.Shape.IsSHP)
                {
                    SEQ seq = SEQLoader.FromStream(reader, core.Actor.Shape.coreObject);
                    core.Actor.AttachSEQ(seq);
                    m_mainWindow.AnimationTool.Reset();
                    return true;
                }
            }
            return false;
        }

        private bool OpenMainFile(out string outPath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = 
                "Actors (*.WEP,*.SHP)|*.WEP;*.SHP|" +
                "SHP (*.SHP)|*SHP|" +
                "WEP (*.WEP)|*.WEP";
            if (openFileDialog.ShowDialog() == true)
            {
                outPath = openFileDialog.FileName;
                return true;
            }
            outPath = "";
            return false;
        }

        private bool OpenSubFile(out string outPath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "SEQ (*.SEQ)|*.SEQ";
            if (openFileDialog.ShowDialog() == true)
            {
                outPath = openFileDialog.FileName;
                return true;
            }
            outPath = "";
            return false;
        }
        #endregion
    }
}
