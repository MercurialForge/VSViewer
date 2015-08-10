﻿using SharpDX;
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
        public List<List<Keyframe>> keys = new List<List<Keyframe>>();

        // SEQ source data
        public Animation baseAnimation;
        public Vector3[] poses;
        public List<NullableVector4>[] keyframes;
    }
}
