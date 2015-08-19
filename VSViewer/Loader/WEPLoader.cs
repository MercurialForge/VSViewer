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
            int numOfPalettes = 7; // palettes of 2/3 color count.

            /*=====================================================================
                STREAM READER
            =====================================================================*/
            WEP wep = new WEP();

            wep.joints = VSTools.GetJoints(reader, numJoints);
            wep.groups = VSTools.GetGroups(reader, numGroups);
            wep.vertices = VSTools.GetVertices(reader, wep.groups);
            wep.polygons = VSTools.GetPolygons(reader, numAllPolygons);
            wep.textures = VSTools.GetTextures(reader, numOfPalettes);

            return wep;
        }
    }
}
