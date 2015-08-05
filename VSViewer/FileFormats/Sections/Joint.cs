using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.FileFormats
{
    /// <summary>
    /// A joint within a skeletal hierarchy.
    /// </summary>
    class Joint
    {
        // relative distance from parent joint.
        public int boneLength;
        // parent index within joint array
        public SByte parentID;

        // XYZ vectors
        public SByte x;
        public SByte y;
        public SByte z;

        // Mode, used during animation?
        public SByte mode;
        // 0 - 2 normal ?
        // 3 - 6 normal + roll 90 degrees
        // 7 - 255 absolute, different angles
    }
}
