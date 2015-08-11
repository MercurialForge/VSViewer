using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.Common;

namespace VSViewer.FileFormats.Sections
{
    public class Animation
    {
        // the name of the animation
        public string name = "Animation";
        // const fps for all animations
        public const float fps = 25;
        // length of the animation
        public float length;
        // A list for each bone and a list of keys
        public List<List<Keyframe>> jointKeys = new List<List<Keyframe>>();

        public void SetLength()
        {
            for(int i = 0; i < jointKeys.Count; i++)
            {
                for(int j = 0; j < jointKeys[i].Count; j++)
                {
                    if(jointKeys[i][j].Time > length)
                    {
                        length = jointKeys[i][j].Time;
                    }
                }
            }
        }

        // SEQ source data
        public Animation baseAnimation;
        public Vector3[] poses;
        public List<NullableVector4>[] keyframes;
    }
}
