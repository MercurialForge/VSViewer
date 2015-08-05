using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VSViewer.FileFormats;

namespace VSViewer
{
    /// <summary>
    /// Parses Joints, Groups, Vertices, Polygons, and Textures.
    /// </summary>
    static partial class VSTools
    {

        static public void GetJoints(EndianBinaryReader reader, List<Joint> outJoints, int numJoints)
        {
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

                outJoints.Add(joint);
            }
        }

        static public void GetGroups(EndianBinaryReader reader, List<Group> outGroups, int numGroups)
        {
            for (int i = 0; i < numGroups; i++)
            {
                Group group = new Group();

                group.boneID = reader.ReadInt16();
                group.lastVertex = reader.ReadUInt16();

                outGroups.Add(group);
            }
        }

        static public void GetVertices(EndianBinaryReader reader, List<Vertex> outVertices, List<Group> groups)
        {
            for (int i = 0, g = 0; i < groups[groups.Count - 1].lastVertex; i++)
            {
                if (i >= groups[g].lastVertex)
                {
                    g++; // next group
                }

                Vertex vertex = new Vertex();

                vertex.x = reader.ReadInt16();
                vertex.y = reader.ReadInt16();
                vertex.z = reader.ReadInt16();

                reader.Skip(0x02); // padding

                vertex.groupID = g;
                vertex.group = groups[g];
                vertex.boneID = groups[g].boneID;

                outVertices.Add(vertex);
            }
        }

        static public void GetPolygons(EndianBinaryReader reader, List<Polygon> outPolygons, int numAllPolygons)
        {
            for (int i = 0; i < numAllPolygons; i++)
            {
                Polygon polygon = new Polygon();

                polygon.polygonType = (PolygonType)reader.ReadByte();
                polygon.size = reader.ReadByte();
                polygon.BaceFaceMode = (BackFaceMode)reader.ReadByte();

                polygon.unknown = reader.ReadByte();

                polygon.vertex1 = reader.ReadUInt16();
                polygon.vertex1 /= 4;
                polygon.vertex2 = reader.ReadUInt16();
                polygon.vertex2 /= 4;
                polygon.vertex3 = reader.ReadUInt16();
                polygon.vertex3 /= 4;

                if (polygon.polygonType == PolygonType.Quad)
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

                if (polygon.polygonType == PolygonType.Quad)
                {
                    polygon.u4 = reader.ReadByte();
                    polygon.v4 = reader.ReadByte();
                }

                outPolygons.Add(polygon);
            }
        }

        static public void GetTextures(EndianBinaryReader reader, List<TextureMap> outTextures, int numOfPalettes)
        {
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

            //TODO: put the following into a texture class so byte or texture can be retrived.

            // List of a list of bytes. Represents x,y texture map palette index.
            byte[] map = new byte[width * height];

            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    map[(y * width) + x] = reader.ReadByte();
                }
            }

            // create texture
            for (int i = 0, l = colorPalettes.Count; i < l; ++i)
            {
                Palette palette = colorPalettes[i];
                byte[] buffer = new byte[width * height * 4];
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        int c = map[(y * width) + x];
                        int index = (((y * width) + x) * 4);

                        // TODO sometimes c >= colorsPerPalette?? set transparent, for now
                        if (c < colorsPerPalette)
                        {
                            // swizzled to BGRA
                            buffer[index + 0] = palette.colors[c][2]; //b
                            buffer[index + 1] = palette.colors[c][1]; //g
                            buffer[index + 2] = palette.colors[c][0]; //r
                            buffer[index + 3] = palette.colors[c][3]; //a
                        }
                        else
                        {
                            Console.WriteLine("Over colors per palette bounds");
                            // defaults to byte[] = {0, 0, 0, 0} transparent
                        }
                    }
                }

                // copy stream to texture
                BitmapSource tempTexture = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgra32, null, buffer.ToArray(), 4 * ((width * 4 + 3) / 4));
                TextureMap tempTMap = new TextureMap();
                tempTMap.width = width;
                tempTMap.height = height;
                tempTMap.texture = tempTexture;
                outTextures.Add(tempTMap);

                // write to disk
                using (FileStream stream = new FileStream("paletteMap" + i + ".png", FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(tempTexture));
                    encoder.Save(stream);
                }
            }
        }
    }
}
