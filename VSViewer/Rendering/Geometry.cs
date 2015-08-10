using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer;
using VSViewer.Common;
using VSViewer.FileFormats;

namespace VSViewer
{
    public class Geometry
    {
        // all skin weights in Vagrant Story are 1
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
        // skeleton
        public List<SkeletalBone> skeleton = new List<SkeletalBone>();

        public List<TextureMap> textures = new List<TextureMap>();

        // TODO: generate normals for faces and verts
        // generate normals
        // a-b
        // a-c
        // cross for vector

    }
}
