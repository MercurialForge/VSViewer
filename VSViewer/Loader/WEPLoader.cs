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
            UInt16 numTriangles = reader.ReadUInt16();
            UInt16 numQuads = reader.ReadUInt16();
            UInt16 numPolygons = reader.ReadUInt16();
            int ptrTexture1 = (int)(reader.ReadUInt32() + 0x10); // same as ptrTexture... why?

            reader.Skip(0x30); // header padding?

            int ptrTexture = (int)(reader.ReadUInt32() + 0x10);
            int ptrGroups = (int)(reader.ReadUInt32() + 0x10);
            int ptrVertices = (int)(reader.ReadUInt32() + 0x10);
            int ptrPolygons = (int)(reader.ReadUInt32() + 0x10);

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
