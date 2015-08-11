using GameFormatReader.Common;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Input;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.Loader;

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

        public ICommand OnShowSkeleton
        {
            get { return new RelayCommand(x => ShowSkeleton()); }
        }

        public ICommand OnRequestLoad
        {
            get { return new RelayCommand(x => Load(), x => QueryPrepedFiles); }
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
        #endregion

        public ImporterViewModel(ViewportViewModel viewportViewModel, MainWindowViewModel mainWindowViewModel)
        {
            m_viewport = viewportViewModel;
            m_mainWindow = mainWindowViewModel;
        }

        #region Command Methods
        internal void PrepMainFile()
        {
            MainFilePath = OpenFile();
        }

        internal void PrepSubFile()
        {
            SubFilePath = OpenFile();
        }

        internal void ShowSkeleton()
        {
            if (!IsActive) { return; }
            Console.WriteLine("Show Skeleton is not implemented.");
        }

        internal void Load()
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(MainFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                switch (m_mainFile.Extension)
                {
                    case ".WEP":
                        // load type
                        WEP wep = WEPLoader.FromStream(reader);
                        m_loadedContent = wep;
                        Geometry wepGeometry = VSTools.CreateGeometry(wep);
                        // push to viewport
                        m_viewport.PushGeometry(wepGeometry);
                        // push extra tools
                        m_mainWindow.EnableImporter();
                        break;

                    case ".SHP":
                        SHP shp = SHPLoader.FromStream(reader);
                        m_loadedContent = shp;
                        shp.textures[0].Save("Bronze");
                        Geometry shpGeometry = VSTools.CreateGeometry(shp);
                        // push to viewport
                        m_viewport.PushGeometry(shpGeometry);
                        // push extra tools
                        m_mainWindow.EnableImporter();
                        break;
                }
            }
            if (m_subFile != null)
            {
                using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(SubFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
                {
                    switch (m_subFile.Extension)
                    {
                        case ".SEQ":
                            SEQ seq = SEQLoader.FromStream(reader, m_loadedContent);
                            m_viewport.RenderSystem.PushSequence(seq);
                            break;
                    }
                }
            }
        }
        #endregion

        #region Local Methods
        private string OpenFile()
        {
            string temp = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                temp = openFileDialog.FileName;
            }
            return temp;
        }
        #endregion
    }
}
