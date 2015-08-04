using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.FileFormats.Sections
{
    class Joint
    {
        public int boneLength;
        public SByte parentID;

        public SByte x;
        public SByte y;
        public SByte z;

        public SByte mode;
    }
}
