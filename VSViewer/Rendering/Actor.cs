using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.FileFormats.Sections;

namespace VSViewer.Rendering
{
    /// <summary>
    /// An Actor is an object that can be read by the render system.
    /// </summary>
    public class Actor : Transform
    {

        public string name { get; set; }
        public Geometry Shape { get; set; }
        public SEQ SEQ { get; set; }

        private Animation m_playbackAnimation;
        private float m_playbackSpeed;

        public Animation PlaybackAnimation
        {
            get { return m_playbackAnimation; }
            set
            {
                m_playbackAnimation = value;
                OnPropertyChanged("CurrentAnimation");
            }
        }

        public float PlaybackSpeed
        {
            get { return m_playbackSpeed; }
        }

        public void SetPlaybackSpeed (int speed)
        {
            switch (speed)
            {
                case 0: m_playbackSpeed = 0.125f; break;
                case 1: m_playbackSpeed = 0.166f; break;
                case 2: m_playbackSpeed = 0.25f; break;
                case 3: m_playbackSpeed = 0.333f; break;
                case 4: m_playbackSpeed = 0.5f; break;
                case 5: m_playbackSpeed = 1f; break;
                case 6: m_playbackSpeed = 2f; break;
                case 7: m_playbackSpeed = 3f; break;
                case 8: m_playbackSpeed = 4f; break;
                case 9: m_playbackSpeed = 6f; break;
                case 10: m_playbackSpeed = 8f; break;
                default: m_playbackSpeed = 1f; break;
            }
        }

        public void StopAnimation()
        {
            m_playbackSpeed = 0;
        }

        public Actor()
        {
        }

        public Actor(Geometry theGeometry)
        {
            Shape = theGeometry;
        }

        public void AttachSEQ (SEQ sequence)
        {
            SEQ = sequence;
        }
    }
}
