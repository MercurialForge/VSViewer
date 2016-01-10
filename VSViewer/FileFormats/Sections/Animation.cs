using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using VSViewer.Common;

namespace VSViewer.FileFormats.Sections
{
    public class Animation
    {
        public string name = "Animation";
        public const float fps = 25f;
        public float LengthInSeconds
        {
            get
            {
                if(!bLengthIsSet) { SetLengthInSeconds(); }
                return m_lengthInSeconds;
            }
        }
        public float LengthInMilliseconds
        {
            get { return LengthInSeconds * 1000; }
        }
        public int LengthInFrames
        {
            get { return (int)Math.Floor(LengthInSeconds * fps); }
        }
        // A list for each bone and a list of keys for that bone
        public List<List<Keyframe>> jointKeys = new List<List<Keyframe>>();

        private bool bLengthIsSet;
        public void SetLengthInSeconds()
        {
            bLengthIsSet = true;
            for (int i = 0; i < jointKeys.Count; i++)
            {
                for (int j = 0; j < jointKeys[i].Count; j++)
                {
                    if (jointKeys[i][j].Time > LengthInSeconds)
                    {
                        m_lengthInSeconds = jointKeys[i][j].Time;
                    }
                }
            }
        }

        #region SEQ Source Creation Data
        public Animation baseAnimation; // not sure how to use this.
        public Vector3[] poses;
        public List<NullableVector4>[] keyframes; //TODO: maybe just make it a list of lists? Consistancy?
        #endregion

        private bool bInitialized = false;
        private void Initialize()
        {
            if (!bInitialized)
            {
                bInitialized = true;
                m_currentKeyframe = new Keyframe[jointKeys.Count];
                m_nextKeyframe = new Keyframe[jointKeys.Count];
            }
        }
        // keyframe arrays keep track of respective state for each joint
        private Keyframe[] m_currentKeyframe;
        private Keyframe[] m_nextKeyframe;
        private float lastQueryTime;
        private float m_lengthInSeconds;

        public Animation() { }

        /// <summary>
        /// Creates a non-destrctive copy of the original animation
        /// </summary>
        public Animation(Animation Anim)
        {
            name = Anim.name;

            for (int i = 0; i < Anim.jointKeys.Count; i++)
            {
                List<Keyframe> keys = new List<Keyframe>();
                for (int j = 0; j < Anim.jointKeys[i].Count; j++)
                {
                    keys.Add(Anim.jointKeys[i][j].Copy());
                }
                jointKeys.Add(keys);
            }
        }

        //TODO: query in frames?
        /// <summary>
        /// Query the animation at the queryTime in seconds. Returns a local spaced Transform for jointIndex.
        /// </summary>
        public Transform QueryAnimationTime(float queryTime, int jointIndex)
        {
            Initialize();
            Transform constructedFrame = new Transform();

            if (queryTime < lastQueryTime) { RestartAniamtion(); }

            // using the queryTime, check to see if m_nextKeyframe is no longer the next. If so find the next keyframe and shuffle the order
            for (int k = 0; k < jointKeys[jointIndex].Count; k++)
            {
                Keyframe keyframe = jointKeys[jointIndex][k];
                if (m_currentKeyframe[jointIndex] == null) { m_currentKeyframe[jointIndex] = keyframe; m_nextKeyframe[jointIndex] = keyframe; }

                // if the query time has passed m_nextKeyframe
                if (queryTime >= m_nextKeyframe[jointIndex].Time)
                {
                    if (keyframe.Time >= queryTime) // and the current keyframe is greater than the query time
                    {
                        m_currentKeyframe[jointIndex] = m_nextKeyframe[jointIndex];
                        m_nextKeyframe[jointIndex] = keyframe;
                        break;
                    }
                }

            }


            // calculate noralized time between currentkeyframe and nextkeyframe
            float currentKeyframeTime = m_currentKeyframe[jointIndex].Time;
            float nextKeyframeTime = m_nextKeyframe[jointIndex].Time;
            float query = queryTime;

            float timeSinceCurrentKeyframe = query - currentKeyframeTime;
            float timeUntilNextKeyframe = nextKeyframeTime - currentKeyframeTime;

            float t = MathUtil.Clamp(timeSinceCurrentKeyframe / timeUntilNextKeyframe, 0, 1);

            if (float.IsNaN(t))
            {
                t = 0;
            }

            constructedFrame.Position = Vector3.Lerp(m_currentKeyframe[jointIndex].Position, m_nextKeyframe[jointIndex].Position, t);
            constructedFrame.Rotation = Quaternion.Slerp(m_currentKeyframe[jointIndex].Rotation, m_nextKeyframe[jointIndex].Rotation, t);
            constructedFrame.LocalScale = Vector3.Lerp(m_currentKeyframe[jointIndex].Scale, m_nextKeyframe[jointIndex].Scale, t);

            lastQueryTime = queryTime;
            return constructedFrame;
        }

        /// <summary>
        /// Resets the keyframes to their initial states. Causes a pop in the loop, but allows animations to restart.
        /// </summary>
        private void RestartAniamtion()
        {
            Initialize();
            for (int i = 0; i < jointKeys.Count; i++)
            {
                int totalKeysForJoint = jointKeys[i].Count;
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

        /// <summary>
        /// Returns a single animation comprised of the beginning and end
        /// </summary>
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

                // skip adding more keys if the count is 1. A single key means it's only holding a default root rotation, another will cause unwanted rotations.
                if (beginning.jointKeys[i].Count != 1)
                {
                    float lastTime = keys[keys.Count - 1].Time;

                    for (int e = 0; e < end.jointKeys[i].Count; e++)
                    {
                        keys.Add(end.jointKeys[i][e]);
                        keys.Last().Time += lastTime;
                    }
                }
                newAnim.jointKeys.Add(keys);
            }
            newAnim.SetLengthInSeconds();
            return newAnim;
        }
    }
}
