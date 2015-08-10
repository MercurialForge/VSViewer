using SharpDX;
using SharpDX.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.Common
{
    public class EditorCamera : BaseCamera
    {
        public override void SetViewParams(Vector3 eye, Vector3 lookAt, Vector3 vUp)
        {
            base.SetViewParams(eye, lookAt, vUp);
            m_viewRotQuat = Quaternion.Identity;
        }

        #region Interaction
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dp"></param>
        protected override void KeyMove(Vector3 dp)
        {
            if (!EnableYAxisMovement)
                dp.Y = 0;
            dp *= MoveScaler;
            dp = Matrix.RotationQuaternion(m_viewRotQuat).TransformNormal(dp);
            Position += dp;
            LookAt += dp;
            UpdateView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        protected override void KeyRoll(float angle)
        {
            angle *= RotationScaler;
            var m = Matrix.RotationZ(angle);
            Up = m.TransformNormal(Up);
            UpdateView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dMouse"></param>
        protected override void MouseRotation(Quaternion dMouse)
        {
            var mRot = Matrix.RotationQuaternion(dMouse);

            LookAt = Position + mRot.TransformNormal(LookAt - Position);
            Up = mRot.TransformNormal(Up);

            m_viewRotQuat *= dMouse;
            m_viewRotQuat.Normalize();
        }

        #endregion


    }
}
