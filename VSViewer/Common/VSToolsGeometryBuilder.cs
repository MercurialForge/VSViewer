﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.Common;
using VSViewer.FileFormats;

namespace VSViewer
{
    static partial class VSTools
    {
        static public Geometry CreateGeometry (List<Vertex> vertices, List<Polygon> polygons, TextureMap textureMap)
        {
            var tw = textureMap.width;
	        var th = textureMap.height;

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

                // skip weights

                // bone weight IDs
                geometry.boneID.Add( v1.boneID );
                geometry.boneID.Add( v2.boneID );
                geometry.boneID.Add( v3.boneID );

		        if ( p.polygonType == PolygonType.Quad ) 
                {

			        Vertex v4 = vertices[ p.vertex4 ];

			        geometry.vertices.Add(v4.GetVector());

                    // skip weights

			        geometry.boneID.Add( v4.boneID );

			        geometry.uv1.Add( new Vector2( p.u3 / tw, 1 - p.v3 / th ) );
			        geometry.uv1.Add( new Vector2( p.u2 / tw, 1 - p.v2 / th ) );
			        geometry.uv1.Add( new Vector2( p.u1 / tw, 1 - p.v1 / th ) );

                    geometry.uv1.Add( new Vector2( p.u2 / tw, 1 - p.v2 / th ) );
			        geometry.uv1.Add( new Vector2( p.u3 / tw, 1 - p.v3 / th ) );
			        geometry.uv1.Add( new Vector2( p.u4 / tw, 1 - p.v4 / th ) );

                    geometry.indices.Add((UInt16)(iv + 2));
                    geometry.indices.Add((UInt16)(iv + 1));
                    geometry.indices.Add((UInt16)(iv + 0));

                    geometry.indices.Add((UInt16)(iv + 1));
                    geometry.indices.Add((UInt16)(iv + 2));
                    geometry.indices.Add((UInt16)(iv + 3));

			        if ( p.BaceFaceMode == BackFaceMode.On ) 
                    {
                        geometry.indices.Add((UInt16)(iv + 0));
                        geometry.indices.Add((UInt16)(iv + 1));
                        geometry.indices.Add((UInt16)(iv + 2));

                        geometry.indices.Add((UInt16)(iv + 3));
                        geometry.indices.Add((UInt16)(iv + 2));
                        geometry.indices.Add((UInt16)(iv + 1));


                        geometry.uv1.Add( new Vector2( p.u1 / tw, 1 - p.v1 / th ) );
			            geometry.uv1.Add( new Vector2( p.u2 / tw, 1 - p.v2 / th ) );
			            geometry.uv1.Add( new Vector2( p.u3 / tw, 1 - p.v3 / th ) );

                        geometry.uv1.Add( new Vector2( p.u4 / tw, 1 - p.v4 / th ) );
			            geometry.uv1.Add( new Vector2( p.u3 / tw, 1 - p.v3 / th ) );
			            geometry.uv1.Add( new Vector2( p.u3 / tw, 1 - p.v2 / th ) );
			        }

		        } else {

			        geometry.uv1.Add( new Vector2( p.u1 / tw, 1 - p.v1 / th ) );
			        geometry.uv1.Add( new Vector2( p.u2 / tw, 1 - p.v2 / th ) );
			        geometry.uv1.Add( new Vector2( p.u3 / tw, 1 - p.v3 / th ) );

                    geometry.indices.Add((UInt16)(iv + 2));
                    geometry.indices.Add((UInt16)(iv + 1));
                    geometry.indices.Add((UInt16)(iv + 0));

			        if ( p.BaceFaceMode == BackFaceMode.On ) {

				        geometry.indices.Add((UInt16)(iv + 0));
                        geometry.indices.Add((UInt16)(iv + 1));
                        geometry.indices.Add((UInt16)(iv + 2));

				        geometry.uv1.Add( new Vector2( p.u3 / tw, 1 - p.v3 / th ) );
			            geometry.uv1.Add( new Vector2( p.u2 / tw, 1 - p.v2 / th ) );
			            geometry.uv1.Add( new Vector2( p.u1 / tw, 1 - p.v1 / th ) );

			        }

		        }

	        }

	        //geometry.computeFaceNormals();
	        //geometry.computeVertexNormals();
            return geometry;
        }
    }
}
