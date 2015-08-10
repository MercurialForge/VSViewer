using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.FileFormats
{
    /// <summary>
    /// Used to easily cast byte to determined polygon type
    /// </summary>
    public enum PolygonType : byte
    {
        Triangle = 0x24,
        Quad = 0x2c
    }

    /// <summary>
    /// Used to easily cast byte to determined bace face mode
    /// </summary>
    public enum FaceMode : byte
    {
        Front = 0x04,
        Back = 0x05
    }

    /// <summary>
    /// A polygon contains the information needed to construct it
    /// </summary>
    public class Polygon
    {
        // triangle or quad, determines of forth vertex and uv set is expected
        public PolygonType polygonType;
        // size in bytes of the polygon data, could be used to determine if forth vertex and uv sets are expected
        public byte size;
        // determine if backface should be generated (0x40 => one-sided, 0x50 => double-sided)
        public FaceMode BaceFaceMode;

        //TODO: unknown possible normal or alpha? Could be skipped in reader.
        public byte unknown; 

        // vertex indices with option 4th for quads
        public UInt16 vertex1;
        public UInt16 vertex2;
        public UInt16 vertex3;
        public UInt16 vertex4;

        // uvs co-ordinates with option forth set for quads, each set corresponds to it's above vertex. 
        // the provided co-ordiantes are relative to the u (width) and v (height) of the texture, so...
        // the calculation (u1 / texture.Width) returns the normalized position in UV space for vertext1, u1.
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
