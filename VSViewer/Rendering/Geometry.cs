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
        // the source of the geometry
        public WEP coreObject;
        // query the Geometry's original import type
        public bool IsSHP { get { return coreObject is SHP; } }

        // the imported vertice positions
        public List<Vector3> vertices = new List<Vector3>();
        // triangle indices, always a multiple of 3
        public List<UInt16> indices = new List<UInt16>();
        // the default UV set
        public List<Vector2> uv1 = new List<Vector2>();
        // the sub UV set
        public List<Vector2> uv2 = new List<Vector2>();
        // the bind pose skeleton -- set only to joint lengths
        public List<SkeletalJoint> skeleton = new List<SkeletalJoint>();
        // each vertcies' parent jointID
        public List<int> jointID = new List<int>();

        // all skin weights in Vagrant Story are 1
        public const int skinWeight = 1;
        // the render systems instanced vertices for skinning
        public List<Vector3> instancedVertices = new List<Vector3>();
        // the render system's instance joints for skinning
        public List<SkeletalJoint> instancedSkeleton = new List<SkeletalJoint>(); 

        public List<TextureMap> Textures
        {
            get { return coreObject.textures; }
            set { coreObject.textures = value; }
        }

        // TODO: generate normals for faces and verts
        // generate normals
        // a-b
        // a-c
        // cross for vector

    }
}
