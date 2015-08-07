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
    static partial class VSTools
    {
        static public Geometry CreateGeometry (List<Vertex> vertices, List<Polygon> polygons, List<Joint> bones, TextureMap textureMap)
        {
            float tw = textureMap.width;
	        float th = textureMap.height;

	        Geometry geometry = new Geometry();

	        for ( int i = 0; i < polygons.Count; i++ ) {

		        Polygon p = polygons[ i ];

		        Vertex v1 = vertices[ p.vertex1 ];
		        Vertex v2 = vertices[ p.vertex2 ];
		        Vertex v3 = vertices[ p.vertex3 ];

		        int iv = geometry.vertices.Count;

                // vertex positions
		        geometry.vertices.Add(v1.GetVector());
		        geometry.vertices.Add(v2.GetVector());
		        geometry.vertices.Add(v3.GetVector());

                //uvs
                


                // skip weights

                // bone weight IDs
                geometry.boneID.Add( v1.boneID );
                geometry.boneID.Add( v2.boneID );
                geometry.boneID.Add( v3.boneID );

		        if ( p.polygonType == PolygonType.Quad ) 
                {

			        Vertex v4 = vertices[ p.vertex4 ];

			        geometry.vertices.Add(v4.GetVector());

                    geometry.uv1.Add(new Vector2(p.u1 / tw, p.v1 / th));
                    geometry.uv1.Add(new Vector2(p.u2 / tw, p.v2 / th));
                    geometry.uv1.Add(new Vector2(p.u3 / tw, p.v3 / th));
                    geometry.uv1.Add(new Vector2(p.u4 / tw, p.v4 / th));

                    // skip weights

			        geometry.boneID.Add( v4.boneID );

                    geometry.indices.Add((UInt16)(iv + 2));
                    geometry.indices.Add((UInt16)(iv + 1));
                    geometry.indices.Add((UInt16)(iv + 0));

                    geometry.indices.Add((UInt16)(iv + 1));
                    geometry.indices.Add((UInt16)(iv + 2));
                    geometry.indices.Add((UInt16)(iv + 3));

			        if ( p.BaceFaceMode == FaceMode.Back ) 
                    {
                        //geometry.indices.Add((UInt16)(iv + 2));
                        //geometry.indices.Add((UInt16)(iv + 0));
                        //geometry.indices.Add((UInt16)(iv + 1));

                        //geometry.indices.Add((UInt16)(iv + 1));
                        //geometry.indices.Add((UInt16)(iv + 2));
                        //geometry.indices.Add((UInt16)(iv + 3));

                        //geometry.uv1.Add( new Vector2( p.u1 / tw, 1 - p.v1 / th ) );
                        //geometry.uv1.Add( new Vector2( p.u2 / tw, 1 - p.v2 / th ) );
                        //geometry.uv1.Add( new Vector2( p.u3 / tw, 1 - p.v3 / th ) );
                        //geometry.uv1.Add( new Vector2( p.u4 / tw, 1 - p.v4 / th ) );
			        }

		        } else {

                    geometry.uv1.Add(new Vector2(p.u2 / tw, p.v2 / th));
                    geometry.uv1.Add(new Vector2(p.u3 / tw, p.v3 / th));
                    geometry.uv1.Add(new Vector2(p.u1 / tw, p.v1 / th));

                    geometry.indices.Add((UInt16)(iv + 2));
                    geometry.indices.Add((UInt16)(iv + 1));
                    geometry.indices.Add((UInt16)(iv + 0));

			        if ( p.BaceFaceMode == FaceMode.Back ) {

                        //geometry.indices.Add((UInt16)(iv + 2));
                        //geometry.indices.Add((UInt16)(iv + 1));
                        //geometry.indices.Add((UInt16)(iv + 0));

                        //geometry.uv1.Add( new Vector2( p.u2 / tw, 1 - p.v2 / th ) );
                        //geometry.uv1.Add( new Vector2( p.u3 / tw, 1 - p.v3 / th ) );
                        //geometry.uv1.Add( new Vector2( p.u1 / tw, 1 - p.v1 / th ) );

			        }

		        }

	        }

	        //geometry.computeFaceNormals();
	        //geometry.computeVertexNormals();

            
            for ( var i = 0; i < bones.Count; i++ ) 
            {
		        int parent = bones[ i ].parentID;

                SkeletalBone bone = new SkeletalBone();
                bone.name += i;
                bone.parentIndex = (parent < bones.Count) ? parent + bones.Count : -1;
                geometry.skeleton.Add(bone);
	        }

	        // translation bones
	        for ( var i = bones.Count; i < bones.Count * 2; ++i ) 
            {
                SkeletalBone bone = new SkeletalBone();
                bone.name += i;
                bone.parentIndex = i - bones.Count;
                geometry.skeleton.Add(bone);
	        }

            for (var i = bones.Count; i < bones.Count * 2; ++i)
            {

                geometry.skeleton[i].position.X = bones[i - bones.Count].boneLength;
            }

            return geometry;
        }
    }
}
