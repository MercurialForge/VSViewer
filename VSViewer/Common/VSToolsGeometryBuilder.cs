using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.Rendering;

namespace VSViewer
{
    static partial class VSTools
    {
        static public Geometry CreateGeometry (WEP sourceObject)
        {
            float tw = sourceObject.textures[0].Width;
            float th = sourceObject.textures[0].Height;

	        Geometry geometry = new Geometry();

            for (int i = 0; i < sourceObject.polygons.Count; i++) 
            {
                Polygon p = sourceObject.polygons[i];

		        Vertex v1 = sourceObject.vertices[ p.vertex1 ];
                Vertex v2 = sourceObject.vertices[p.vertex2];
                Vertex v3 = sourceObject.vertices[p.vertex3];

		        int iv = geometry.vertices.Count;

                // vertex positions
		        geometry.vertices.Add(v1.GetVector());
		        geometry.vertices.Add(v2.GetVector());
		        geometry.vertices.Add(v3.GetVector());

                // skip weights

                // bone weight IDs
                geometry.jointID.Add( v1.boneID );
                geometry.jointID.Add( v2.boneID );
                geometry.jointID.Add( v3.boneID );

		        if ( p.polygonType == PolygonType.Quad ) 
                {

                    Vertex v4 = sourceObject.vertices[p.vertex4];

			        geometry.vertices.Add(v4.GetVector());

                    geometry.uv1.Add(new Vector2(p.u1 / tw, p.v1 / th));
                    geometry.uv1.Add(new Vector2(p.u2 / tw, p.v2 / th));
                    geometry.uv1.Add(new Vector2(p.u3 / tw, p.v3 / th));
                    geometry.uv1.Add(new Vector2(p.u4 / tw, p.v4 / th));

                    // skip weights

			        geometry.jointID.Add( v4.boneID );

                    geometry.indices.Add((UInt16)(iv + 1));
                    geometry.indices.Add((UInt16)(iv + 2));
                    geometry.indices.Add((UInt16)(iv + 3));

                    geometry.indices.Add((UInt16)(iv + 2));
                    geometry.indices.Add((UInt16)(iv + 1));
                    geometry.indices.Add((UInt16)(iv + 0));

			        if ( p.BaceFaceMode == FaceMode.Back ) 
                    {
                        geometry.indices.Add((UInt16)(iv + 0));
                        geometry.indices.Add((UInt16)(iv + 1));
                        geometry.indices.Add((UInt16)(iv + 2));

                        geometry.indices.Add((UInt16)(iv + 3));
                        geometry.indices.Add((UInt16)(iv + 2));
                        geometry.indices.Add((UInt16)(iv + 1));
			        }

		        } else {

                    geometry.uv1.Add(new Vector2(p.u2 / tw, p.v2 / th));
                    geometry.uv1.Add(new Vector2(p.u3 / tw, p.v3 / th));
                    geometry.uv1.Add(new Vector2(p.u1 / tw, p.v1 / th));

                    geometry.indices.Add((UInt16)(iv + 2));
                    geometry.indices.Add((UInt16)(iv + 1));
                    geometry.indices.Add((UInt16)(iv + 0));

			        if ( p.BaceFaceMode == FaceMode.Back ) 
                    {
                        geometry.indices.Add((UInt16)(iv + 0));
                        geometry.indices.Add((UInt16)(iv + 1));
                        geometry.indices.Add((UInt16)(iv + 2));
			        }
		        }
	        }

	        //geometry.computeFaceNormals();
	        //geometry.computeVertexNormals();

            for ( var i = 0; i < sourceObject.joints.Count; i++ ) 
            {
                int parent = sourceObject.joints[i].parentID;

                SkeletalJoint bone = new SkeletalJoint();
                bone.name += i;
                bone.parentIndex = (parent < sourceObject.joints.Count) ? parent + sourceObject.joints.Count : -1;
                geometry.skeleton.Add(bone);
	        }

	        // translation bones
            for (var i = sourceObject.joints.Count; i < sourceObject.joints.Count * 2; ++i) 
            {
                SkeletalJoint bone = new SkeletalJoint();
                bone.name += i;
                bone.parentIndex = i - sourceObject.joints.Count;
                geometry.skeleton.Add(bone);
	        }

            for (var i = sourceObject.joints.Count; i < sourceObject.joints.Count * 2; ++i)
            {
                Vector3 tempPos = geometry.skeleton[i].LocalPosition;
                tempPos.X = sourceObject.joints[i - sourceObject.joints.Count].boneLength;
                geometry.skeleton[i].LocalPosition = tempPos;
            }

            geometry.instancedSkeleton = geometry.skeleton.ToArray();
            geometry.instancedVertices = new InputVertex[geometry.vertices.Count];

            geometry.coreObject = sourceObject;
            return geometry;
        }
    }
}
