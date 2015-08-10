using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VSViewer.FileFormats;

namespace VSViewer.Loader
{
    static class WEPLoader
    {
        static public WEP FromStream(EndianBinaryReader reader)
        {
            /*=====================================================================
                WEP HEADER (0x50) 80 bytes long
            =====================================================================*/
            reader.Skip(0x04); // TODO: skip magic 0x04 (4 dec) "H01" check for file type?

            byte numJoints = reader.ReadByte();
            byte numGroups = reader.ReadByte();
            ushort numTriangles = reader.ReadUInt16();
            ushort numQuads = reader.ReadUInt16();
            ushort numPolygons = reader.ReadUInt16();
            uint ptrTexture1 = (uint)(reader.ReadUInt32() + 0x10); // same as ptrTexture... why?

            reader.Skip(0x30); // header padding?

            uint ptrTexture = (uint)(reader.ReadUInt32() + 0x10);
            uint ptrGroups = (uint)(reader.ReadUInt32() + 0x10);
            uint ptrVertices = (uint)(reader.ReadUInt32() + 0x10);
            uint ptrPolygons = (uint)(reader.ReadUInt32() + 0x10);

            /*=====================================================================
                LOCALS
            =====================================================================*/
            int numAllPolygons = numTriangles + numQuads + numPolygons;
            int numOfPalettes = 5; // all items have 5 palettes for each material type.

            /*=====================================================================
                STREAM READER
            =====================================================================*/
            WEP tempWEP = new WEP();

            VSTools.GetJoints(reader, tempWEP.joints, numJoints);
            VSTools.GetGroups(reader, tempWEP.groups, numGroups);
            VSTools.GetVertices(reader, tempWEP.vertices, tempWEP.groups);
            VSTools.GetPolygons(reader, tempWEP.polygons, numAllPolygons);
            VSTools.GetTextures(reader, tempWEP.textures, numOfPalettes);

            return tempWEP;
        }
    }
}
