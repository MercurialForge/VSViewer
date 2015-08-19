using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static public List<Joint> GetJoints(EndianBinaryReader reader, int numJoints)
        {
            List<Joint> joints = new List<Joint>();

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

                joints.Add(joint);
            }
            return joints;
        }

        static public List<Group> GetGroups(EndianBinaryReader reader, int numGroups)
        {
            List<Group> groups = new List<Group>();
            for (int i = 0; i < numGroups; i++)
            {
                Group group = new Group();

                group.boneID = reader.ReadInt16();
                group.lastVertex = reader.ReadUInt16();

                groups.Add(group);
            }
            return groups;
        }

        static public List<Vertex> GetVertices(EndianBinaryReader reader, List<Group> groups)
        {
            List<Vertex> vertices = new List<Vertex>();
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
                vertex.boneID = groups[g].boneID;

                vertices.Add(vertex);
            }
            return vertices;
        }

        static public List<Polygon> GetPolygons(EndianBinaryReader reader, int numAllPolygons)
        {
            List<Polygon> polygons = new List<Polygon>();
            for (int i = 0; i < numAllPolygons; i++)
            {
                Polygon polygon = new Polygon();

                polygon.polygonType = (PolygonType)reader.ReadByte();
                polygon.size = reader.ReadByte();
                polygon.BaceFaceMode = (FaceMode)reader.ReadByte();

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

                polygons.Add(polygon);
            }
            return polygons;
        }

        static public List<TextureMap> GetTextures(EndianBinaryReader reader, int numOfPalettes)
        {
            List<TextureMap> textures = new List<TextureMap>();

            UInt32 size = reader.ReadUInt32();
            //reader.Skip(0x01); // version number, always 1 for WEPs. SHP?
            byte temp = reader.ReadByte();
            Trace.Assert(temp == 1);

            int width = reader.ReadByte() * 2;
            int height = reader.ReadByte() * 2;
            byte colorsPerPalette = reader.ReadByte();

            if (numOfPalettes != 2)
            {
                ConstructWeaponPalettes(reader, textures, numOfPalettes, width, height, colorsPerPalette);
            }
            else
            {
                ConstructCharacterPalettes(reader, textures, numOfPalettes, width, height, colorsPerPalette);
            }

            // A linear representation of the texture map, each byte is a palette index.
            byte[] paletteMap = new byte[width * height];

            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    paletteMap[(y * width) + x] = reader.ReadByte();
                }
            }

            for (int i = 0; i < numOfPalettes; i++)
            {
                textures[i].map = paletteMap;
                textures[i].SaveToDisk(i.ToString());
            }
            return textures;
        }

        private static void ConstructWeaponPalettes(EndianBinaryReader reader, List<TextureMap> outTextures, int numOfPalettes, int width, int height, byte colorsPerPalette)
        {
            // construct the handle palette the first 1/3 of the colors.
            Palette handlePalette = new Palette();
            for (int i = 0; i < colorsPerPalette / 3; i++)
            {
                handlePalette.colors.Add(VSTools.BitColorConverter(reader.ReadUInt16()));
            }

            // construct the next 7 palettes out of the data stream that follows.
            for (int p = 0; p < numOfPalettes; p++)
            {
                TextureMap tex = new TextureMap(width, height);
                Palette palette = new Palette();

                // pack first 1/3 with handle colors.
                for (int h = 0; h < handlePalette.GetColorCount(); h++)
                {
                    palette.colors.Add(handlePalette.colors[h]);
                }

                int count = (int)(colorsPerPalette / 3);
                count += count;

                // read blade from stream
                for (int c = 0; c < count; c++)
                {
                    palette.colors.Add(VSTools.BitColorConverter(reader.ReadUInt16()));
                }
                tex.ColorPalette = palette;

                tex.Index = p;
                outTextures.Add(tex);
            }
        }

        private static void ConstructCharacterPalettes(EndianBinaryReader reader, List<TextureMap> outTextures, int numOfPalettes, int width, int height, byte colorsPerPalette)
        {
            for (int p = 0; p < numOfPalettes; p++)
            {
                TextureMap tex = new TextureMap(width, height);
                Palette palette = new Palette();

                for (int c = 0; c < colorsPerPalette; c++)
                {
                    palette.colors.Add(VSTools.BitColorConverter(reader.ReadUInt16()));
                }
                tex.ColorPalette = palette;

                tex.Index = p;
                outTextures.Add(tex);
            }
        }
    }
}
