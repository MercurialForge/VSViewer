using SharpDX;
using System.Collections.Generic;
using System.Linq;
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

        private bool bInitialized = false;
        private void Initialize ()
        {
            if (!bInitialized)
            {
                bInitialized = true;
                m_previousKeyframe = new Keyframe[jointKeys.Count];
                m_currentKeyframe = new Keyframe[jointKeys.Count];
                m_nextKeyframe = new Keyframe[jointKeys.Count];
            }
        }

        Keyframe[] m_previousKeyframe;
        Keyframe[] m_currentKeyframe;
        Keyframe[] m_nextKeyframe;
        float lastQueryTime;

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

        public Transform QueryAnimationTime(float time, int jointIndex)
        {
            Initialize();
            Transform f = new Transform();

            if (time < lastQueryTime) { UpdateAnimation(); }

            // query frames
            int totalKeysForJoint = jointKeys[jointIndex].Count;

            for (int k = 0; k < totalKeysForJoint; k++)
            {
                Keyframe keyframe = jointKeys[jointIndex][k];
                if (m_currentKeyframe[jointIndex] == null) { m_currentKeyframe[jointIndex] = keyframe; m_nextKeyframe[jointIndex] = keyframe; }

                if (time >= m_nextKeyframe[jointIndex].Time)
                {
                    if (keyframe.Time >= time)
                    {
                        m_previousKeyframe[jointIndex] = m_currentKeyframe[jointIndex];
                        m_currentKeyframe[jointIndex] = m_nextKeyframe[jointIndex];
                        m_nextKeyframe[jointIndex] = keyframe;
                        break;
                    }
                }

            }


            float f1 = m_currentKeyframe[jointIndex].Time;
            float f2 = m_nextKeyframe[jointIndex].Time;
            float query = time;

            float a = query - f1;
            float b = f2 - f1;

            float t = MathUtil.Clamp(a / b, 0, 1);

            if (float.IsNaN(t))
            {
                t = 0;
            }

            f.Position = Vector3.Lerp(m_currentKeyframe[jointIndex].Position, m_nextKeyframe[jointIndex].Position, t);
            f.Rotation = Quaternion.Slerp(m_currentKeyframe[jointIndex].Rotation, m_nextKeyframe[jointIndex].Rotation, t);
            f.LocalScale = Vector3.Lerp(m_currentKeyframe[jointIndex].Scale, m_nextKeyframe[jointIndex].Scale, t);

            lastQueryTime = time;
            return f;
        }

        private void UpdateAnimation()
        {
            Initialize();
            for (int i = 0; i < jointKeys.Count; i++)
            {
                int totalKeysForJoint = jointKeys[i].Count;
                m_previousKeyframe[i] = jointKeys[i][totalKeysForJoint - 1];
                m_currentKeyframe[i] = jointKeys[i][0];
                if (totalKeysForJoint > 1)
                {
                    m_nextKeyframe[i] = jointKeys[i][1];
                }
                else
                {
                    m_nextKeyframe[i] = jointKeys[i][0];
                }
            }
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
