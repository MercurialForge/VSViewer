using GameFormatReader.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.FileFormats.Sections;

namespace VSViewer.Loader
{
    public class SEQLoader
    {
        private struct AnimationHeader
        {
            public ushort length;
            public sbyte idOtherAnimation; // some animations use a different animation as base
            public byte mode;
            public ushort ptrLooping; // seems to point to a data block that controls looping
            public ushort ptrTranslation; // points to a translation vector for the animated mesh (root motion?)
            public ushort ptrMove; // points to a data block that controls movement (root motion?)
            public ushort[] ptrBones;
        }

        static public SEQ FromStream(EndianBinaryReader reader, ContentBase activeSHP)
        {
            SHP targetSHP = (SHP)activeSHP;
            /*=====================================================================
                TODO: add lenght
            =====================================================================*/
            // base ptr neede because the SEQ could be embedded.
            uint ptrBase = (uint)reader.BaseStream.Position;

            ushort numFrames = reader.ReadByte(); // total number of frames?
            var unknownPaddingValue1 = reader.ReadByte(); // padding
            Trace.Assert(unknownPaddingValue1 == 0);
            byte numBones = reader.ReadByte();
            var unknownPaddingValue2 = reader.ReadByte(); // padding
            Trace.Assert(unknownPaddingValue2 == 0);
            uint size = reader.ReadUInt32();

            reader.Skip(4); // unknown
            uint ptrFrames = (uint)reader.ReadUInt32() + 8; // pointer to the frames data section
            uint ptrSequence = ptrFrames + (uint)numFrames; // pointer to the sequence section

            // number of animations
            //                  length of all the headers   /   length of one animation header
            int numAnimations = (((int)ptrSequence - numFrames) - 16) / (numBones * 4 + 10);

            List<AnimationHeader> headers = new List<AnimationHeader>();

            for (int i = 0; i < numAnimations; i++)
            {
                AnimationHeader animHeader = new AnimationHeader
                {
                    length = reader.ReadUInt16(),
                    idOtherAnimation = reader.ReadSByte(), // some animations use a different animation as base
                    mode = reader.ReadByte(),
                    ptrLooping = reader.ReadUInt16(), // seems to point to a data block that controls looping
                    ptrTranslation = reader.ReadUInt16(), // points to a translation vector for the animated mesh (root motion?)
                    ptrMove = reader.ReadUInt16(), // points to a data block that controls movement (root motion?)

                    ptrBones = new ushort[numBones],
                };

                // assign bone ptrs
                for (int p = 0; p < numBones; p++)
                {
                    animHeader.ptrBones[p] = reader.ReadUInt16();
                }

                for (int j = 0; j < numBones; j++)
                {
                    var unknownBoneValues = reader.ReadUInt16(); //TODO: this is 0 for all SEQs?
                    //Trace.Assert(unknownBoneValues == 0); // will show if value is ever NOT zero
                }

                headers.Add(animHeader);
            }

            // TODO: Never used!?
            sbyte[] frames = new sbyte[numFrames];
            for (int i = 0; i < numFrames; i++)
            {
                frames[i] = reader.ReadSByte();
            }

            // get source animation data from .SEQ
            List<Animation> animations = new List<Animation>();
            for (int i = 0; i < numAnimations; i++)
            {
                Animation animation = new Animation();
                // seek translation data
                long seekTranslationPtr = (long)(headers[i].ptrTranslation + ptrSequence + ptrBase);
                reader.BaseStream.Seek(seekTranslationPtr, SeekOrigin.Begin);

                reader.CurrentEndian = Endian.Big; // this is code used in the opcode portion (machine code uses Big)
                Int16 x = reader.ReadInt16();
                Int16 y = reader.ReadInt16();
                Int16 z = reader.ReadInt16();
                reader.CurrentEndian = Endian.Little;

                // TODO: implement move

                // set base animation

                if (headers[i].idOtherAnimation != -1)
                {
                    // TODO: FIX THIS
                    // should store other animation inside as base
                    // I assume it only references animations that
                    // have been constructed first otherwise it will hold a dead copy.
                    // animation.baseAnimation = animations[i];
                }

                // read base pose and keyframes
                animation.poses = new Vector3[numBones];
                animation.keyframes = new List<NullableVector4>[numBones];
                for (int k = 0; k < numBones; k++)
                {
                    animation.keyframes[k] = new List<NullableVector4>();
                    animation.keyframes[k].Add(NullableVector4.Zero());

                    // read pose
                    long seekBonePtr = (long)(headers[i].ptrBones[k] + ptrSequence + ptrBase);
                    reader.BaseStream.Seek(seekBonePtr, SeekOrigin.Begin);

                    reader.CurrentEndian = Endian.Big; // machine code uses Big
                    Int16 rx = reader.ReadInt16();
                    Int16 ry = reader.ReadInt16();
                    Int16 rz = reader.ReadInt16();
                    reader.CurrentEndian = Endian.Little;

                    animation.poses[k] = new Vector3(rx, ry, rz);

                    // read keyframe
                    float? f = 0;

                    while (true)
                    {
                        //Vector4 outData;
                        //if(ReadOPCode(reader, out outData))
                        //{

                        //}

                        NullableVector4 op = ReadOPCode(reader);
                        if (op == null) break;

                        f += op.W;

                        animation.keyframes[k].Add(op);

                        if (f >= headers[i].length - 1) break;
                    }
                }
                animations.Add(animation);
            }

            // build useable animation data
            for (int a = 0; a < numAnimations; a++)
            {
                // rotation bones
                for (int i = 0; i < numBones; i++)
                {
                    List<NullableVector4> keyframes = animations[a].keyframes[i];
                    Vector3 pose = animations[a].poses[i];

                    // multiplication by two at 0xad25c, 0xad274, 0xad28c
                    // value * (180f / uint16.max);
                    float rx = pose.X * 2;
                    float ry = pose.Y * 2;
                    float rz = pose.Z * 2;

                    List<Keyframe> keys = new List<Keyframe>();
                    float t = 0;

                    for (var j = 0; j < keyframes.Count; j++)
                    {
                        NullableVector4 keyframe = keyframes[j];

                        float f = (float)keyframe.W;

                        t += f;

                        if (keyframe.X == null) keyframe.X = keyframes[j - 1].X;
                        if (keyframe.Y == null) keyframe.Y = keyframes[j - 1].Y;
                        if (keyframe.Z == null) keyframe.Z = keyframes[j - 1].Z;

                        // if always positive can use - value as key changer?
                        rx += (float)keyframe.X * f;
                        ry += (float)keyframe.Y * f;
                        rz += (float)keyframe.Z * f;

                        Quaternion q = VSTools.Rot2Quat(rx * VSTools.Rot13ToRad, ry * VSTools.Rot13ToRad, rz * VSTools.Rot13ToRad);

                        Keyframe key = new Keyframe();
                        key.Time = t * VSTools.TimeScale;
                        key.Rotation = q;
                        keys.Add(key);
                    }
                    animations[a].jointKeys.Add(keys);
                    animations[a].SetLength();
                }

                // root's translation bone
                List<Keyframe> rootKey = new List<Keyframe>();
                rootKey.Add(new Keyframe());
                animations[a].jointKeys.Add(rootKey);

                // translation bones
                for (int t = 1; t < numBones; t++)
                {
                    List<Keyframe> transBone = new List<Keyframe>();
                    Keyframe key = new Keyframe();
                    key.Position = new Vector3(targetSHP.joints[t].boneLength, 0, 0);
                    transBone.Add(key);
                    animations[a].jointKeys.Add(transBone);
                }

            }
            return new SEQ(animations);
        }

        private static NullableVector4 ReadOPCode(EndianBinaryReader reader)
        {
            byte op = reader.ReadByte();
            byte op0 = op;

            if (op == 0) return null; // return abort vector

            // results
            float? x = null, y = null, z = null, f = null;

            if ((op & 0xe0) > 0)
            {
                // number of frames, byte case
                f = op & 0x1f;

                if (f == 0x1f)
                {
                    f = 0x20 + reader.ReadByte();
                }
                else
                {
                    f = 1 + f;
                }
            }
            else
            {
                // number of frames, half word case
                f = op & 0x3;

                if (f == 0x3)
                {
                    f = 4 + reader.ReadByte();
                }
                else
                {
                    f = 1 + f;
                }

                // half word values
                reader.CurrentEndian = Endian.Big;

                op = (byte)(op << 3);

                var h = reader.ReadInt16();

                if ((h & 0x4) > 0)
                {
                    x = h >> 3;
                    op = (byte)(op & 0x60);

                    if ((h & 0x2) > 0)
                    {
                        y = reader.ReadInt16();
                        op = (byte)(op & 0xa0);
                    }

                    if ((h & 0x1) > 0)
                    {
                        z = reader.ReadInt16();
                        op = (byte)(op & 0xc0);
                    }

                }
                else if ((h & 0x2) > 0)
                {
                    y = h >> 3;
                    op = (byte)(op & 0xa0);

                    if ((h & 0x1) > 0)
                    {
                        z = reader.ReadInt16();
                        op = (byte)(op & 0xc0);
                    }
                }
                else if ((h & 0x1) > 0)
                {
                    z = h >> 3;
                    op = (byte)(op & 0xc0);
                }
            }

            // byte values (fallthrough)
            reader.CurrentEndian = Endian.Little;

            if ((op & 0x80) > 0)
            {
                x = reader.ReadSByte();
            }

            if ((op & 0x40) > 0)
            {
                y = reader.ReadSByte();
            }

            if ((op & 0x20) > 0)
            {
                z = reader.ReadSByte();
            }

            return new NullableVector4(x, y, z, f);
        }
    }
}
