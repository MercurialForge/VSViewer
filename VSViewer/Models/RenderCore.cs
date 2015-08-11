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

        public List<Actor> Actors
        {
            get { return m_actors; }
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

        #endregion

        #region BackingFields

        List<Actor> m_actors;
        FillMode m_fillMode = FillMode.Solid;
        ShadingMode m_shadingMode = ShadingMode.Textured;
        bool m_showSkeleton = false;
        bool m_useTurntable = false;
        float m_turntableSpeed = 1; 

        #endregion

        public void AddActor(Actor newActor)
        {
            m_actors.Add(newActor);
        }

        public void ClearActors ()
        {
            m_actors.Clear();
            m_actors = null;
        }
    }
}
