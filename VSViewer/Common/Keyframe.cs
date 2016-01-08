using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.Common
{
    public class Keyframe
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public float Time { get; set; }

        public Keyframe ()
        {
            Scale = Vector3.One;
        }

        public Keyframe Copy ()
        {
            Keyframe key = new Keyframe();
            key.Position = Position;
            key.Rotation = Rotation;
            key.Scale = Scale;
            key.Time = Time;
            return key;
        }
    }
}
