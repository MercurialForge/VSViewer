using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.Common
{
    public class NullableVector4
    {
        public float? X;
        public float? Y;
        public float? Z;
        public float? W;

        public static NullableVector4 Zero()
        {
            return new NullableVector4(0, 0, 0, 0);
        }

        public NullableVector4 ()
        {
            X = null;
            Y = null;
            Z = null;
            W = null;
        }

        public NullableVector4(float? x, float? y, float? z, float? w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}
