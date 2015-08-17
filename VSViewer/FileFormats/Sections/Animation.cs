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
        public float Length { get; set; }
        // A list for each bone and a list of keys
        public List<List<Keyframe>> jointKeys = new List<List<Keyframe>>();

        public void SetLength()
        {
            for(int i = 0; i < jointKeys.Count; i++)
            {
                for(int j = 0; j < jointKeys[i].Count; j++)
                {
                    if(jointKeys[i][j].Time > Length)
                    {
                        Length = jointKeys[i][j].Time;
                    }
                }
            }
        }

        // SEQ source data
        public Animation baseAnimation;
        public Vector3[] poses;
        public List<NullableVector4>[] keyframes;

        public Animation Copy ()
        {
            Animation copy = new Animation();

            copy.name = name;
            copy.Length = Length;
            for(int i = 0; i < jointKeys.Count; i++)
            {
                Keyframe[] newKeys = new Keyframe[jointKeys[i].Count];
                jointKeys[i].CopyTo(newKeys);
                List<Keyframe> dup = new List<Keyframe>(newKeys);
                copy.jointKeys.Add(dup);
            }

            return copy;
        }

        // TODO: this edits the original animation and ruins it.... Needs to copy the animations somehow.
        static public Animation MergeAnimations(Animation beginning, Animation end)
        {
            Animation newAnim = new Animation();
            newAnim.jointKeys = new List<List<Keyframe>>();

            for (int i = 0; i < beginning.jointKeys.Count; i++)
            {
                List<Keyframe> keys = new List<Keyframe>();
                for (int b = 0; b < beginning.jointKeys[i].Count; b++)
                {
                    keys.Add(beginning.jointKeys[i][b]);
                }

                float lastTime = keys[keys.Count - 1].Time;

                for (int e = 0; e < end.jointKeys[i].Count; e++)
                {
                    keys.Add(end.jointKeys[i][e]);
                    keys.Last().Time += lastTime;
                }
                newAnim.jointKeys.Add(keys);
            }
            newAnim.SetLength();
            return newAnim;
        }
    }
}
