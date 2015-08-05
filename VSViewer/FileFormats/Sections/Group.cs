using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.FileFormats
{
    /// <summary>
    /// A group identifies a sequence of vertices that are weighted to a bone.
    /// </summary>
    class Group
    {
        // The bone all vertices in the group or weighted to
        public Int16 boneID;
        // the vertices weighted to the bone. The next vertex (if applicable) is the first vertex for the next group.
        public UInt16 lastVertex;
    }
}
