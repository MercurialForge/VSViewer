using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.Rendering;

namespace VSViewer.Models
{
    public enum ShadingMode
    {
        Textured,
        Untextured
    }

    public class RenderCore : ObservableBase
    {
        #region Properties

        public Actor Actor
        {
            get { return m_actor; }
            set
            {
                m_actor = value;
                m_renderRequiresUpdate = true;
                OnPropertyChanged("Actor");
            }
        }

        public bool RenderRequiresUpdate
        {
            get { return m_renderRequiresUpdate; }
            set { m_renderRequiresUpdate = value; }
        }

        public FillMode FillMode
        {
            get { return m_fillMode; }
            set
            {
                m_fillMode = value;
                OnPropertyChanged("FillMode");
            }
        }

        public ShadingMode ShadingMode
        {
            get { return m_shadingMode; }
            set
            {
                m_shadingMode = value;
                OnPropertyChanged("ShadingMode");
            }
        }

        public bool ShowSkeleton
        {
            get { return m_showSkeleton; }
            set
            {
                m_showSkeleton = value;
                OnPropertyChanged("ShowSkeleton");
            }
        }

        public bool UseTurntable
        {
            get { return m_useTurntable; }
            set
            {
                m_useTurntable = value;
                OnPropertyChanged("UseTurntable");
            }
        }

        public float TurntableSpeed
        {
            get { return m_turntableSpeed; }
            set
            {
                m_turntableSpeed = value;
                OnPropertyChanged("TurntableSpeed");
            }
        }

        public string TEST { get { return "TEST"; } set { m_TEST = value; } }

        #endregion

        #region BackingFields

        Actor m_actor = new Actor();
        bool m_renderRequiresUpdate = false;
        FillMode m_fillMode = FillMode.Solid;
        ShadingMode m_shadingMode = ShadingMode.Textured;
        bool m_showSkeleton = false;
        bool m_useTurntable = true;
        float m_turntableSpeed = 1;
        private string m_TEST; 

        #endregion
    }
}
