using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.Common;

namespace VSViewer
{
    class Geometry
    {
        public const int skinWeight = 1;

        // the positions of the vertices
        public List<Vector3> vertices = new List<Vector3>();
        // uv1
        public List<Vector2> uv1 = new List<Vector2>();
        // uv2
        public List<Vector2> uv2 = new List<Vector2>();
        // triangle indices 
        public List<UInt16> indices = new List<UInt16>();
        // skinned bone
        public List<int> boneID = new List<int>();
    }
}
