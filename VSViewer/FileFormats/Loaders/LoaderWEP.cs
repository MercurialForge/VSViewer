using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VSViewer.FileFormats.Sections;

namespace VSViewer.FileFormats.Loaders
{
    class LoaderWEP : LoaderBase<WEP>
    {
        // nums
        byte numJoints;
        byte numGroups;
        UInt16 numTriangles;
        UInt16 numQuads;
        UInt16 numPolygons;

        int numAllPolygons;
        int numOfPalettes = 5; // Hard coded (5 palettes for every WEP: Bronze, Iron, Silver, Hagane, and Damascus)

        // ptrs
        int ptrTexture1;
        int ptrTexture;
        int ptrGroups;
        int ptrVertices;
        int ptrPolygons;

        public LoaderWEP(string path) : base(path) { }

        public override WEP Read()
        {
            WEP wep = new WEP();

            reader.Skip(0x04); // TODO: skip magic 0x04 (4 dec) "H01" check for file type?

            numJoints = reader.ReadByte();
            numGroups = reader.ReadByte();
            numTriangles = reader.ReadUInt16();
            numQuads = reader.ReadUInt16();
            numPolygons = reader.ReadUInt16();
            numAllPolygons = this.numTriangles + this.numQuads + this.numPolygons;

            ptrTexture1 = (int)(reader.ReadUInt32() + 0x10); // same as ptrTexture... why?

            reader.Skip(0x30); // header padding?

            ptrTexture = (int)(reader.ReadUInt32() + 0x10);
            ptrGroups = (int)(reader.ReadUInt32() + 0x10);
            ptrVertices = (int)(reader.ReadUInt32() + 0x10);
            ptrPolygons = (int)(reader.ReadUInt32() + 0x10);

            // Joints
            for (int i = 0; i < numJoints; i++)
            {
                Joint joint = new Joint();

                joint.boneLength = -reader.ReadInt16();

                reader.Skip(2); // no effect on length, just padding

                joint.parentID = reader.ReadSByte();
                joint.x = reader.ReadSByte();
                joint.y = reader.ReadSByte();
                joint.z = reader.ReadSByte();
                joint.mode = reader.ReadSByte(); // does this matter?

                reader.Skip(0x01); // unknown
                reader.Skip(0x06); // always 0? padding?

                wep.joints.Add(joint);
            }

            // Groups
            for (int i = 0; i < numGroups; i++)
            {
                Group group = new Group();

                group.boneID = reader.ReadInt16();
                group.lastVertex = reader.ReadUInt16();

                wep.groups.Add(group);
            }

            // Vertices
            int g = 0;
            for (int i = 0; i < wep.groups[this.numGroups - 1].lastVertex; i++)
            {
                if (i >= wep.groups[g].lastVertex) { g++; }

                Vertex vertex = new Vertex();

                vertex.x = reader.ReadInt16();
                vertex.y = reader.ReadInt16();
                vertex.z = reader.ReadInt16();

                reader.Skip(0x02); // padding

                vertex.groupID = g;
                vertex.group = wep.groups[g];
                vertex.boneID = wep.groups[g].boneID;

                wep.vertices.Add(vertex);
            }

            // Polygons
            for (int i = 0; i < numAllPolygons; i++)
            {
                Polygon polygon = new Polygon();

                polygon.polyType = (PolygonType)reader.ReadByte();
                polygon.size = reader.ReadByte();
                polygon.isDoubleSided = (reader.ReadByte() == 0x5) ? true : false;

                polygon.unknown = reader.ReadByte();

                polygon.vertex1 = reader.ReadUInt16();
                polygon.vertex1 /= 4;
                polygon.vertex2 = reader.ReadUInt16();
                polygon.vertex2 /= 4;
                polygon.vertex3 = reader.ReadUInt16();
                polygon.vertex3 /= 4;

                if (polygon.polyType == PolygonType.Quad)
                {
                    polygon.vertex4 = reader.ReadUInt16();
                    polygon.vertex4 /= 4;
                }

                polygon.u1 = reader.ReadByte();
                polygon.v1 = reader.ReadByte();
                polygon.u2 = reader.ReadByte();
                polygon.v2 = reader.ReadByte();
                polygon.u3 = reader.ReadByte();
                polygon.v3 = reader.ReadByte();

                if (polygon.polyType == PolygonType.Quad)
                {
                    polygon.u4 = reader.ReadByte();
                    polygon.v4 = reader.ReadByte();
                }

                wep.polygons.Add(polygon);
            }


            // Textures
            UInt32 size = reader.ReadUInt32();

            reader.Skip(0x01); // unknown, always 1?

            int width = reader.ReadByte() * 2;
            int height = reader.ReadByte() * 2;
            byte colorsPerPalette = reader.ReadByte();

            List<Palette> colorPalettes = new List<Palette>();

            for (int p = 0; p < numOfPalettes; p++)
            {
                Palette palette = new Palette();
                for (int c = 0; c < colorsPerPalette; c++)
                {
                    palette.colors.Add(VSTools.BitColorConverter(reader.ReadUInt16()));
                }
                colorPalettes.Add(palette);
            }

            // List of a list of bytes. Represents x,y texture map palette index.
            List<List<byte>> map = new List<List<byte>>();

            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    if (map.Count != x + 1)
                    {
                        map.Add(new List<byte>());
                    }
                    map[x].Add(reader.ReadByte());
                }
            }

            // create texture
            for (int i = 0, l = colorPalettes.Count; i < l; ++i)
            {

                Palette palette = colorPalettes[i];
                List<byte> buffer = new List<byte>();

                for (int y = 0; y < height; ++y)
                {

                    for (int x = 0; x < width; ++x)
                    {

                        int c = map[x][y];

                        // TODO sometimes c >= colorsPerPalette?? set transparent, for now
                        if (c < colorsPerPalette)
                        {
                            // swizzled to BGRA
                            buffer.Add(palette.colors[c][2]);
                            buffer.Add(palette.colors[c][1]);
                            buffer.Add(palette.colors[c][0]);
                            buffer.Add(palette.colors[c][3]);
                        }
                        else
                        {
                            Console.WriteLine("Over colors per palette bounds");
                            buffer.Add(0);
                            buffer.Add(0);
                            buffer.Add(0);
                            buffer.Add(0);
                        }

                    }

                }

                // copy stream to texture
                BitmapSource tempTexture = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgra32, null, buffer.ToArray(), 4 * ((width * 4 + 3) / 4));

                // write to disk
                using (FileStream stream = new FileStream("paletteMap" + i + ".png", FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(tempTexture));
                    encoder.Save(stream);
                }
            }

            return wep;
        }
    }
}
