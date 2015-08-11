using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.Common;
using VSViewer.FileFormats;

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
        public int NumberOfAnimations { get; set; }
        public int CurrentAnimationIndex { get; set; }
        public int LoopWithTargetIndex { get; set; }
        public int TotalAnimationsFrames { get; set; }
        public int CurrentAnimationFrame { get; set; }

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
            NumberOfAnimations = sequence.animations.Count;
            CurrentAnimationIndex = 0;
            LoopWithTargetIndex = 0;
            TotalAnimationsFrames = (int)(sequence.animations[0].length * 25);
            CurrentAnimationFrame = 0;
        }
        //public 
    }
}
