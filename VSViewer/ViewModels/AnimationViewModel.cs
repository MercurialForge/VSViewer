using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VSViewer.Common;
using VSViewer.Rendering;

namespace VSViewer.ViewModels
{
    public class AnimationViewModel : ViewModelBase
    {
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

        private int m_currentIndex;
        private int m_maxAnimationCount;
        private int m_playbackSpeed;

        public void Reset ()
        {
            AnimationIndex = 0;
            MaxAnimationCount = MainWindowViewModel.RenderCore.Actor.SEQ.NumberOfAnimations - 1;
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

    }

}
