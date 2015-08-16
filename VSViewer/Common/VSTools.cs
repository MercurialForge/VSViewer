using SharpDX;
using System;

namespace VSViewer
{
    static partial class VSTools
    {
        public const float TimeScale = 0.08f;

        public const float Rad2Deg = (float)(180.0 / Math.PI);
        public const float Deg2Rad = (float)(Math.PI / 180.0);

        // convert 13-bit rotation to radians
        public const float Rot13ToRad = (1f / 4096f) * MathUtil.Pi;

        public static Vector3 UnitX = new Vector3(1, 0, 0);
        public static Vector3 UnitY = new Vector3(0, 1, 0);
        public static Vector3 UnitZ = new Vector3(0, 0, 1);

        // convert XYZ rotation in radians to quaternion
        // first apply x, then y, then z rotation
        // but in sharpdx for reason I cannot explain, in version 2.3.2+ the
        // quaternions must be multiplied z * x * y
        public static Quaternion Rot2Quat(float radX, float radY, float radZ)
        {
            Quaternion quatX = new Quaternion();
            quatX = Quaternion.RotationAxis(new Vector3(1, 0, 0), radX);

            Quaternion quatY = new Quaternion();
            quatY = Quaternion.RotationAxis(new Vector3(0, 1, 0), radY);

            Quaternion quatZ = new Quaternion();
            quatZ = Quaternion.RotationAxis(new Vector3(0, 0, 1), radZ);

            // updated to SDX 2.6.3 and this is backwards y x z... WHY?!?
            return quatZ * quatY * quatX;

        }

        // convert 16 bit color values to 32RGBA
        // first bit == 1 or bits == 0 means fully transparent
        // then 5 bits for each of B, G, R
        public static byte[] BitColorConverter(UInt16 color)
        {
            byte[] theColor;
            int a = (color & 0x8000) >> 15;
            int b = (color & 0x7C00) >> 10;
            int g = (color & 0x03E0) >> 5;
            int r = (color & 0x001F);

            if (a == 1)
            {
                theColor = new byte[] { (byte)(255), (byte)(0), (byte)(0), (byte)(255) };
                return theColor;
            } 

            // 5bit -> 8bit is factor 2^3 = 8
            theColor = new byte[] { (byte)(r * 8), (byte)(g * 8), (byte)(b * 8), 255 };
            return theColor;
        }

    }
}
