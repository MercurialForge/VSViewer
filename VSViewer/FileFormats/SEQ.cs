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
        public int CurrentAnimationIndex
        {
            get { return m_currentAnimationIndex; }
            set
            {
                m_currentAnimationIndex = value;
                UpdateAnimationTo(m_currentAnimationIndex);
            }
        }

        public int LoopWithTargetIndex { get; set; }
        public int TotalAnimationsFrames { get; set; }
        public int CurrentAnimationFrame { get; set; }

        Keyframe[] m_previousKeyframe;
        Keyframe[] m_currentKeyframe;
        Keyframe[] m_nextKeyframe;
        int m_currentAnimationIndex;
        float lastQueryTime;

        public Transform QueryAnimationTime(float time, int jointIndex)
        {
            Animation anim = animations[CurrentAnimationIndex];
            Transform f = new Transform();

            if (time < lastQueryTime) { UpdateAnimationTo(m_currentAnimationIndex); }

            // query frames
            int totalKeysForJoint = anim.jointKeys[jointIndex].Count;

            for (int k = 0; k < totalKeysForJoint; k++)
            {
                Keyframe keyframe = anim.jointKeys[jointIndex][k];
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

        public SEQ(List<Animation> anims)
        {
            animations = anims;
            m_previousKeyframe = new Keyframe[animations[0].jointKeys.Count];
            m_currentKeyframe = new Keyframe[animations[0].jointKeys.Count];
            m_nextKeyframe = new Keyframe[animations[0].jointKeys.Count];

            NumberOfAnimations = animations.Count;
            CurrentAnimationIndex = 0;
            LoopWithTargetIndex = 0;
            TotalAnimationsFrames = (int)(animations[0].length * 25);
            CurrentAnimationFrame = 0;

        }

        private void UpdateAnimationTo(int m_currentAnimationIndex)
        {
            Animation anim = animations[CurrentAnimationIndex];

            for (int i = 0; i < anim.jointKeys.Count; i++)
            {
                int totalKeysForJoint = anim.jointKeys[i].Count;
                m_previousKeyframe[i] = anim.jointKeys[i][totalKeysForJoint - 1];
                m_currentKeyframe[i] = anim.jointKeys[i][0];
                if (totalKeysForJoint > 1)
                {
                    m_nextKeyframe[i] = anim.jointKeys[i][1];
                }
                else
                {
                    m_nextKeyframe[i] = anim.jointKeys[i][0];
                }
            }
        }
    }
}
