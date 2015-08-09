using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.Common
{
    class SkeletalBone
    {
        public string name = "bone";
        public Vector3 position;
        public Quaternion quaternion;
        public Vector3 scale;
        public int parentIndex;

        public SkeletalBone()
        {
            position = Vector3.Zero;
            quaternion = Quaternion.Identity;
            scale = Vector3.One;
        }
    }
}
