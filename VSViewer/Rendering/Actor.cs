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

        private Animation m_currentAnimation;
        public Animation CurrentAnimation
        {
            get { return m_currentAnimation; }
            set
            {
                m_currentAnimation = value;
                OnPropertyChanged("CurrentAnimation");
            }
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
        //public 
    }
}
