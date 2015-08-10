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
        public const float Rot13ToRad = (1f / 4096f) * (float)Math.PI;

        public static Vector3 UnitX = new Vector3(1, 0, 0);
        public static Vector3 UnitY = new Vector3(0, 1, 0);
        public static Vector3 UnitZ = new Vector3(0, 0, 1);

        // convert XYZ rotation in radians to quaternion
        // first apply x, then y, then z rotation
        // THREE.Quaternion.setFromEuler is not equivalent
        public static Quaternion Rot2Quat(float rx, float ry, float rz)
        {
            Quaternion qu = new Quaternion();
            qu = Quaternion.RotationAxis(new Vector3(1, 0, 0), rx);

            Quaternion qv = new Quaternion();
            qv = Quaternion.RotationAxis(new Vector3(0, 1, 0), ry);

            Quaternion qw = new Quaternion();
            qw = Quaternion.RotationAxis(new Vector3(0, 0, 1), rz);

            return qu * qv * qw;
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

            if (color == 0 || a == 1)
            {
                theColor = new byte[] { 0, 0, 0, 0 };
                return theColor;
            }

            // 5bit -> 8bit is factor 2^3 = 8
            theColor = new byte[] { (byte)(r * 8), (byte)(g * 8), (byte)(b * 8), 255 };
            return theColor;
        }

    }
}
