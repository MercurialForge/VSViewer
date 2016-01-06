using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.Common;
using VSViewer.FileFormats.Sections;

namespace VSViewer.FileFormats
{
    public class SEQ : AssetBase
    {
        public List<Animation> animations = new List<Animation>();
        public int NumberOfAnimations { get; set; }

        public SEQ(List<Animation> anims)
        {
            animations = anims;
            NumberOfAnimations = animations.Count;
        }
    }
}
