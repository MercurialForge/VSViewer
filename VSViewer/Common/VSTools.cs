using System;

namespace VSViewer
{
    static partial class VSTools
    {
        // convert 16 bit color values to 32RGBA
	    // first bit == 1 or bits == 0 means fully transparent
	    // then 5 bits for each of B, G, R
	    public static byte[] BitColorConverter ( UInt16 color) 
        {
            byte[] theColor;
            int a = (color & 0x8000) >> 15;
            int b = (color & 0x7C00) >> 10;
            int g = (color & 0x03E0) >> 5;
            int r = (color & 0x001F);

            if ( color == 0 || a == 1 ) 
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
