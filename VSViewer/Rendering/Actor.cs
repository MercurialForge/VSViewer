using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.FileFormats;

namespace VSViewer.Rendering
{
    /// <summary>
    /// An Actor is an object that can be read by the render system.
    /// </summary>
    public class Actor
    {
        public string name { get; set; }
        public Geometry Shape { get; set; }
        public AssetBase Asset { get; set; }
        public int NumberOfAnimations { get; set; }
        public int CurrentAnimationIndex { get; set; }
        public int LoopWithTargetIndex { get; set; }
        public int TotalAnimationsFrames { get; set; }
        public int CurrentAnimationFrame { get; set; }
        //public 
    }
}
