using GameFormatReader.Common;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Input;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.Loader;
using VSViewer.Models;
using VSViewer.Rendering;

namespace VSViewer.ViewModels
{
    class ImporterViewModel : ViewModelBase
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

        public bool IsActive
        {
            get { return m_isActive; }
            set
            {
                m_isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        bool QueryPrepedFiles
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
            get { return new RelayCommand(x => PrepSubFile()); }
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

        #region Command Methods
        internal void PrepMainFile()
        {
            string path = "";
            if (OpenFile(out path))
            {
                MainFilePath = path;
                core.Actor.SEQ = null;
                LoadShape();
            }
        }

        internal void PrepSubFile()
        {
            string path = "";
            if (OpenFile(out path))
            {
                SubFilePath = path;
                LoadAsset();
            }
        }

        private void LoadShape()
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(MainFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                switch (m_mainFile.Extension)
                {
                    case ".WEP":
                        // load type
                        Geometry wepGeometry = VSTools.CreateGeometry(WEPLoader.FromStream(reader));
                        core.Actor = new Actor(wepGeometry);
                        // push extra tools
                        m_mainWindow.EnableImportTool();
                        break;

                    case ".SHP":
                        Geometry shpGeometry = VSTools.CreateGeometry(SHPLoader.FromStream(reader));
                        core.Actor = new Actor(shpGeometry);
                        // push extra tools
                        m_mainWindow.EnableImportTool();
                        break;
                }
            }
        }

        private void LoadAsset()
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(SubFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                switch (m_subFile.Extension)
                {
                    case ".SEQ":
                        if (core.Actor.Shape.IsSHP)
                        {
                            SEQ seq = SEQLoader.FromStream(reader, core.Actor.Shape.coreObject);
                            core.Actor.AttachSEQ(seq);
                        }
                        break;
                }
            }
        }
        #endregion

        #region Local Methods
        private bool OpenFile(out string outPath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
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
