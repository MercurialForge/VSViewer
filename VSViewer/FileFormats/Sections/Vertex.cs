using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.FileFormats
{
    public class Vertex
    {
        public Int16 x;
        public Int16 y;
        public Int16 z;
        public int groupID;
        public Int16 boneID;

        public Vector3 GetVector ()
        {
            return new Vector3(x, y, z);
        }
    }
}
