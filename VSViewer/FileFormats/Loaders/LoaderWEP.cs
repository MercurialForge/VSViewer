using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            for ( int i = 0; i < numJoints; i++)
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
            for (int i = 0; i < numGroups; i++ )
            {
                Group group = new Group();
                group.boneID = reader.ReadInt16();
                group.lastVertex = reader.ReadUInt16();
                wep.groups.Add(group);
            }

            // Vertices

            // Polygons

            // Textures
            

                return null;
        }

    }
}
