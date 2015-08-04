using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.FileFormats.Sections
{
    enum PolygonType : byte
    {
        Triangle = 0x24,
        Quad = 0x2c
    }

    class Polygon
    {
        public PolygonType polyType;
        public byte size;
        public bool isDoubleSided;
        public byte unknown; // unknown possible normal or alpha?

        // vertices with option 4th for quads
        public UInt16 vertex1;
        public UInt16 vertex2;
        public UInt16 vertex3;
        public UInt16 vertex4;

        // uvs with option forth set for quads
        public byte u1;
        public byte v1;
        public byte u2;
        public byte v2;
        public byte u3;
        public byte v3;
        public byte u4;
        public byte v4;

    }
}
