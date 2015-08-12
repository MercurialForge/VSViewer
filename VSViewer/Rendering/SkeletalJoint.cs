using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.Common
{
    // this could almost just be a transform class.
    public class SkeletalJoint : Transform
    {
        public string name = "bone";
        public int parentIndex;
    }
}
