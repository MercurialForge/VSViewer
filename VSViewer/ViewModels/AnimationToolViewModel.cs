using GameFormatReader.Common;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.FileFormats.Sections;
using VSViewer.Loader;
using System;
using VSViewer.Rendering;

namespace VSViewer.ViewModels
{
    public class AnimationToolViewModel : ViewModelBase
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
                if (m_mainWindow.RenderCore.Actor.Shape != null)
                {
                    if (m_mainWindow.RenderCore.Actor.Shape.IsSHP)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        bool QueryAnimationStatus
        {
            get { return (m_mainWindow.RenderCore.Actor.PlaybackAnimation != null); }
        }

        bool QuerySEQStatus
        {
            get { return (m_mainWindow.RenderCore.Actor.SEQ != null); }
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

        public string PauseToggleButtonText
        {
            get { return m_pauseToggleButtonText; }
            set
            {
                m_pauseToggleButtonText = value;
                OnPropertyChanged("PauseToggleButtonText");
            }
        }

        public bool PauseToggleButtonCheckStatus { get; set; }

        public int MaxAnimationCount
        {
            get { return m_maxAnimationCount; }
            set
            {
                m_maxAnimationCount = value;
                OnPropertyChanged("MaxAnimationCount");
            }
        }

        public string FrameWindowDisplayText
        {
            get { return m_frameWindowDisplayText; }
            set
            {
                m_frameWindowDisplayText = value;
                OnPropertyChanged("FrameWindowDisplayText");
            }
        }

        public int PlaybackSpeed
        {
            get { return m_playbackSpeed; }
            set
            {
                m_playbackSpeed = value;
                PlaybackSpeedReadout = GetPlaybackSpeedText(m_playbackSpeed);
                OnPropertyChanged("PlaybackSpeed");
                if (!PauseToggleButtonCheckStatus)
                {
                    m_mainWindow.RenderCore.Actor.SetPlaybackSpeed(m_playbackSpeed);
                }
            }
        }

        public string PlaybackSpeedReadout
        {
            get { return m_playbackSpeedReadout; }
            set
            {
                m_playbackSpeedReadout = value;
                OnPropertyChanged("PlaybackSpeedReadout");
            }
        }

        private string GetPlaybackSpeedText(int speed)
        {
            string humanReadable = "";
            switch (speed)
            {
                case 0: humanReadable = "1/8"; break;
                case 1: humanReadable = "1/6"; break;
                case 2: humanReadable = "1/4"; break;
                case 3: humanReadable = "1/3"; break;
                case 4: humanReadable = "1/2"; break;
                case 5: humanReadable = "Normal"; break;
                case 6: humanReadable = "x2"; break;
                case 7: humanReadable = "x3"; break;
                case 8: humanReadable = "x4"; break;
                case 9: humanReadable = "x6"; break;
                case 10: humanReadable = "x8"; break;
                default: humanReadable = "??"; break;
            }
            return humanReadable;
        }

        public ICommand PreviousAnim
        {
            get { return new RelayCommand(x => StepAnim_Prev(), x => QuerySEQStatus); }

        }

        public ICommand NextAnim
        {
            get { return new RelayCommand(x => StepAnim_Next(), x => QuerySEQStatus); }

        }

        public ICommand PrevFrame
        {
            get { return new RelayCommand(x => DoPrevFrame(), x => QueryAnimationStatus); }
        }

        public ICommand NextFrame
        {
            get { return new RelayCommand(x => DoNextFrame(), x => QueryAnimationStatus); }
        }

        public ICommand TogglePause
        {
            get { return new RelayCommand(x => DoTogglePause()); }
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
        private int m_playbackSpeed = 5;
        private FileInfo m_subFile;
        private MainWindowViewModel m_mainWindow;
        private string m_playbackSpeedReadout = "Normal";
        private string m_pauseToggleButtonText = "X";
        private string m_frameWindowDisplayText;
        private Actor m_monitoredActor;

        public AnimationToolViewModel(MainWindowViewModel mainWindowViewModel)
        {
            m_mainWindow = mainWindowViewModel;
        }

        public override void Tick(TimeSpan deltaTime)
        {
            if (m_mainWindow.RenderCore.Actor != null)
            {
                if (m_monitoredActor != m_mainWindow.RenderCore.Actor)
                {
                    m_monitoredActor = m_mainWindow.RenderCore.Actor;
                    m_subFile = null;
                    OnPropertyChanged("SubFileName");
                    Flush();
                }
            }
            // is SK
            if (m_mainWindow.RenderCore.Actor.PlaybackAnimation != null)
            {
                int frame = (int)Math.Floor(m_mainWindow.RenderCore.Actor.PlaybackAnimationTime / 40);
                int totalFrames = m_mainWindow.RenderCore.Actor.PlaybackAnimation.LengthInFrames;
                FrameWindowDisplayText = string.Format("{0}:{1}", frame, totalFrames);
            }
            else
            {
                FrameWindowDisplayText = "0:0";
            }
        }

        public void ForceUpdate()
        {
            m_mainWindow.RenderCore.Actor.PlaybackAnimation = m_mainWindow.RenderCore.Actor.SEQ.animations[0];
            m_mainWindow.AnimationTool.Reset();
        }

        private void Reset()
        {
            Flush();
            PlaybackSpeed = 5;
            MaxAnimationCount = m_mainWindow.RenderCore.Actor.SEQ.NumberOfAnimations - 1;
        }

        public void Flush()
        {
            m_mainWindow.RenderCore.Actor.PlaybackAnimationTime = 0;
            AnimationIndex = 0;
            MaxAnimationCount = 0;
        }

        internal void PrepSubFile()
        {
            string path = "";
            if (OpenSubFile(out path))
            {
                SubFilePath = path;
                using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(SubFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
                {
                    if (m_subFile.Extension == ".SEQ")
                    {
                        LoadAsset(reader);
                    }
                }
            }
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

        private void LoadAsset(EndianBinaryReader reader)
        {
            SEQ seq = SEQLoader.FromStream(reader, m_mainWindow.RenderCore.Actor.Shape.coreObject);
            m_mainWindow.RenderCore.Actor.SEQ = seq;
            ForceUpdate();
        }

        internal void StepAnim_Prev()
        {
            AnimationIndex = WrapAnimationIndex(--AnimationIndex);
            m_mainWindow.RenderCore.Actor.PlaybackAnimation = m_mainWindow.RenderCore.Actor.SEQ.animations[AnimationIndex];
            m_mainWindow.RenderCore.Actor.PlaybackAnimationTime = 0;
        }

        internal void StepAnim_Next()
        {
            AnimationIndex = WrapAnimationIndex(++AnimationIndex);
            m_mainWindow.RenderCore.Actor.PlaybackAnimation = m_mainWindow.RenderCore.Actor.SEQ.animations[AnimationIndex];
            m_mainWindow.RenderCore.Actor.PlaybackAnimationTime = 0;
        }

        private void DoPrevFrame()
        {
            m_mainWindow.RenderCore.Actor.DecrecrementFrame();
        }

        private void DoNextFrame()
        {
            m_mainWindow.RenderCore.Actor.IncrementFrame();
        }

        private void DoTogglePause()
        {
            if (PauseToggleButtonCheckStatus)
            {
                m_mainWindow.RenderCore.Actor.StopAnimation();
                PauseToggleButtonText = "O";
            }
            else
            {
                PlaybackSpeed = PlaybackSpeed;
                PauseToggleButtonText = "X";
            }
        }

        internal int WrapAnimationIndex(int newIndex)
        {
            if (newIndex < 0)
            {
                return m_mainWindow.RenderCore.Actor.SEQ.NumberOfAnimations - 1;
            }
            else if (newIndex > m_mainWindow.RenderCore.Actor.SEQ.NumberOfAnimations - 1)
            {
                return 0;
            }
            else
            {
                return newIndex;
            }
        }

        internal void MergeAndView()
        {
            // copy anim 1
            // copy anim 2
            // create new sequenced anim object with 1 & 2
            // set current animation to new sequened anim.

            Animation anim1 = new Animation(m_mainWindow.RenderCore.Actor.SEQ.animations[Anim1]);
            Animation anim2 = new Animation(m_mainWindow.RenderCore.Actor.SEQ.animations[Anim2]);
            Animation newAnim = Animation.MergeAnimations(anim1, anim2);
            m_mainWindow.RenderCore.Actor.PlaybackAnimation = newAnim;
        }

    }

}
