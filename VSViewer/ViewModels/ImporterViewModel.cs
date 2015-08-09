using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VSViewer.Common;

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
        FileInfo m_mainFile;
        FileInfo m_subFile;
        bool m_isActive = false;
        #endregion

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
            Console.WriteLine("Preping loading file.");
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
