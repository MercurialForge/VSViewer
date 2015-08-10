using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using VSViewer.Rendering;
using System.Windows;

namespace VSViewer.ViewModels
{
    public class ViewportViewModel : ViewModelBase
    {
        public RenderSystem RenderSystem { get; private set; }

        public ViewportViewModel()
        {
            RenderSystem = new RenderSystem();
            if (!RenderSystem.Initialize())
            {
                Application.Current.Shutdown();
            }
        }

        public void PushGeometry (Geometry geometryToRender)
        {
            RenderSystem.PushGeometry(geometryToRender);
        }
    }
}
