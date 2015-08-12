namespace SharpDX.WPF.Cameras
{
    /// <summary>
    /// Simple model viewing camera class that rotates around the object.
    /// </summary>
    public class ModelViewerCamera : BaseCamera
    {
        private Quaternion m_modelRotQuat;
        private float m_radius;
        private Matrix m_worldMat;

        /// <summary>
        ///
        /// </summary>
        public Matrix World { get { return m_worldMat; } }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eye"></param>
        /// <param name="lookAt"></param>
        /// <param name="vUp"></param>
        public override void SetViewParams(Vector3 eye, Vector3 lookAt, Vector3 vUp)
        {
            base.SetViewParams(eye, lookAt, vUp);

            m_modelRotQuat = Quaternion.Identity;
            m_radius = (eye - lookAt).Length();
            UpdateWorld();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="m_mouseQuat"></param>
        protected override void MouseRotation(Quaternion m_mouseQuat)
        {
            m_modelRotQuat = m_modelRotQuat * m_mouseQuat;
            UpdateWorld();
        }

        /// <summary>
        ///
        /// </summary>
        private void UpdateWorld()
        {
            m_worldMat = Matrix.RotationQuaternion(m_modelRotQuat);
        }
    }
}