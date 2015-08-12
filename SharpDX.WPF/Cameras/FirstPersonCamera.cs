namespace SharpDX.WPF.Cameras
{
    /// <summary>
    /// Simple first person camera class that moves and rotates.
    /// It allows yaw and pitch but not roll.  It uses WM_KEYDOWN and
    /// GetCursorPos() to respond to keyboard and mouse input and updates the
    /// view matrix based on input.
    /// </summary>
    public class FirstPersonCamera : BaseCamera
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="eye"></param>
        /// <param name="lookAt"></param>
        /// <param name="vUp"></param>
        public override void SetViewParams(Vector3 eye, Vector3 lookAt, Vector3 vUp)
        {
            base.SetViewParams(eye, lookAt, vUp);
            m_viewRotQuat = Quaternion.Identity;
        }
    }
}