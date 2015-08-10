using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.FileFormats;

namespace VSViewer.Loader
{
    public class SHPLoader
    {
        static public SHP FromStream(EndianBinaryReader reader)
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

            byte[] overlayX = new byte[8];
            byte[] overlayY = new byte[8];
            byte[] width = new byte[8];
            byte[] height = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                overlayX[i] = reader.ReadByte();
                overlayY[i] = reader.ReadByte();
                width[i] = reader.ReadByte();
                height[i] = reader.ReadByte();
            }

            reader.Skip(0x24); // unkown, skip padding assumed

            reader.Skip(0x06); // skip collision size and height (shape is a cylinder)
            reader.Skip(0x02); // skip menu position Y
            reader.Skip(0x0C); // skip Unknown
            reader.Skip(0x02); // skip Shadow radius
            reader.Skip(0x02); // skip Shadow size increase rate
            reader.Skip(0x02); // skip Shadow size decrease rate
            reader.Skip(0x04); // skip Unknown
            reader.Skip(0x02); // skip Menu scale
            reader.Skip(0x02); // skip Unknown
            reader.Skip(0x02); // skip Target sphere position Y
            reader.Skip(0x08); // skip Unknown

            uint[] animLBA = new uint[12];
            for (int i = 0; i < 12; i++)
            {
                animLBA[i] = reader.ReadUInt32();
            }

            ushort[] chainIDs = new ushort[13];
            for (int i = 0; i < 12; i++)
            {
                chainIDs[i] = reader.ReadUInt16();
            }

            uint[] specialLBAs = new uint[4];
            for (int i = 0; i < 4; i++)
            {
                specialLBAs[i] = reader.ReadUInt32();
            }

            reader.Skip(0x20); //unknown (probably more LBA tables, there are also special attack ids stored here.)

            uint ptrMagic = (uint)(reader.ReadUInt32() + 0xF8); // same as ptrTexture... why?

            reader.Skip(0x18 * 2); // unknown (noticeable effects when casting spells)

            uint ptrAkao = (uint)(reader.ReadUInt32() + 0xF8);
            uint ptrGroups = (uint)(reader.ReadUInt32() + 0xF8);
            uint ptrVertices = (uint)(reader.ReadUInt32() + 0xF8);
            uint ptrPolygons = (uint)(reader.ReadUInt32() + 0xF8);

            /*=====================================================================
                LOCALS
            =====================================================================*/
            int numAllPolygons = numTriangles + numQuads + numPolygons;
            int numOfPalettes = 2; // all characters have 2 palettes. I wonder why?

            /*=====================================================================
                STREAM READER
            =====================================================================*/
            SHP tempSHP = new SHP();

            VSTools.GetJoints(reader, tempSHP.joints, numJoints);
            VSTools.GetGroups(reader, tempSHP.groups, numGroups);
            VSTools.GetVertices(reader, tempSHP.vertices, tempSHP.groups);
            VSTools.GetPolygons(reader, tempSHP.polygons, numAllPolygons);

            // skip AKAO
            reader.Skip(ptrMagic - ptrAkao);

            // skip magic section
            reader.Skip(4); // unknown
            reader.Skip(reader.ReadUInt32()); // skip length of the magic section.

            VSTools.GetTextures(reader, tempSHP.textures, numOfPalettes);

            return tempSHP;
        }
    }
}
