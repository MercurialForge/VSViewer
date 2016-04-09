using SharpDX;
using SharpDX.WPF.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VSViewer.Rendering
{
    class VSViewerCamera : BaseCamera
    {
        private Quaternion m_modelRotQuat;
        private float m_radius;
        private Matrix m_worldMat;

        public Matrix World { get { return m_worldMat; } }

        public override void SetViewParams(Vector3 eye, Vector3 lookAt, Vector3 vUp)
        {
            base.SetViewParams(eye, lookAt, vUp);

            m_modelRotQuat = Quaternion.Identity;
            m_radius = (eye - lookAt).Length();
            UpdateWorld();
        }

        protected override void MouseRotation(Quaternion m_mouseQuat)
        {
            // all this camera shit needs a total rewrite :/

            float x, y, z, w;

            x = m_mouseQuat.X;
            y = m_mouseQuat.Y;
            z = m_mouseQuat.Z;
            w = m_mouseQuat.W;

            float roll = (float)Math.Atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z);
            float pitch = (float)Math.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z);
            float yaw = (float)Math.Asin(2 * x * y + 2 * z * w);

            m_modelRotQuat = m_modelRotQuat * Quaternion.RotationYawPitchRoll(roll, 0, 0);
            //m_modelRotQuat = m_modelRotQuat * m_mouseQuat;
            UpdateWorld();
        }

        private void UpdateWorld()
        {
            m_worldMat = Matrix.RotationQuaternion(m_modelRotQuat);
        }
    }
}
