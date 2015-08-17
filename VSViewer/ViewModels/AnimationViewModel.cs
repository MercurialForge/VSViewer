using GameFormatReader.Common;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.FileFormats.Sections;
using VSViewer.Loader;

namespace VSViewer.ViewModels
{
    public class AnimationViewModel : ViewModelBase
    {
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
                if (MainWindowViewModel.RenderCore.Actor.Shape != null) 
                {
                    if (MainWindowViewModel.RenderCore.Actor.Shape.IsSHP)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int AnimationIndex
        {
            get { return m_currentIndex; }
            set
            {
                m_currentIndex = value;
                OnPropertyChanged("AnimationIndex");
            }
        }

        public int MaxAnimationCount
        {
            get { return m_maxAnimationCount; }
            set
            {
                m_maxAnimationCount = value;
                OnPropertyChanged("MaxAnimationCount");
            }
        }

        public int PlaybackSpeed
        {
            get { return m_playbackSpeed; }
            set
            {
                m_playbackSpeed = value;
                OnPropertyChanged("PlaybackSpeed");
            }
        }

        public ICommand PreviousAnim
        {
            get { return new RelayCommand(x => StepAnim_Prev()); }

        }

        public ICommand NextAnim
        {
            get { return new RelayCommand(x => StepAnim_Next()); }

        }

        public ICommand OnSubFile
        {
            get { return new RelayCommand(x => PrepSubFile(), x => QueryMainStatus); }
        }

        public ICommand Merge
        {
            get { return new RelayCommand(x => MergeAndView()); }
        }

        public int Anim1 { get; set; }
        public int Anim2 { get; set; }

        private int m_currentIndex;
        private int m_maxAnimationCount;
        private int m_playbackSpeed;
        private FileInfo m_subFile;
        private MainWindowViewModel m_mainWindow;

        public AnimationViewModel(MainWindowViewModel mainWindowViewModel)
        {
            m_mainWindow = mainWindowViewModel;
        }

        public void Reset ()
        {
            AnimationIndex = 0;
            MaxAnimationCount = MainWindowViewModel.RenderCore.Actor.SEQ.NumberOfAnimations - 1;
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

        private bool LoadAsset(EndianBinaryReader reader)
        {
            if (MainWindowViewModel.RenderCore.Actor.Shape != null)
            {
                if (MainWindowViewModel.RenderCore.Actor.Shape.IsSHP)
                {
                    SEQ seq = SEQLoader.FromStream(reader, MainWindowViewModel.RenderCore.Actor.Shape.coreObject);
                    MainWindowViewModel.RenderCore.Actor.AttachSEQ(seq);
                    m_mainWindow.AnimationTool.Reset();
                    return true;
                }
            }
            return false;
        }

        internal void StepAnim_Prev()
        {
            if (AnimationIndex >= 0)
            {
                MainWindowViewModel.RenderCore.Actor.SEQ.CurrentAnimationIndex--;
                AnimationIndex = MainWindowViewModel.RenderCore.Actor.SEQ.CurrentAnimationIndex;
            }
        }

        internal void StepAnim_Next()
        {
            if (AnimationIndex < MainWindowViewModel.RenderCore.Actor.SEQ.NumberOfAnimations - 1)
            {
                MainWindowViewModel.RenderCore.Actor.SEQ.CurrentAnimationIndex++;
                AnimationIndex = MainWindowViewModel.RenderCore.Actor.SEQ.CurrentAnimationIndex;
            }
        }

        internal void MergeAndView()
        {
            Animation anim1 = MainWindowViewModel.RenderCore.Actor.SEQ.animations[Anim1].Copy();
            Animation anim2 = MainWindowViewModel.RenderCore.Actor.SEQ.animations[Anim2].Copy();
            Animation newAnim = Animation.MergeAnimations(anim1, anim2);
            MainWindowViewModel.RenderCore.Actor.SEQ.animations.Add(newAnim);
            MainWindowViewModel.RenderCore.Actor.SEQ.CurrentAnimationIndex = MainWindowViewModel.RenderCore.Actor.SEQ.NumberOfAnimations;
        }

    }

}
