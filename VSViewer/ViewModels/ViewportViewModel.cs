using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using VSViewer.Rendering;
using System.Windows;
using VSViewer.Models;

namespace VSViewer.ViewModels
{
    public class ViewportViewModel : ViewModelBase
    {
        public RenderSystem RenderSystem { get; private set; }

        public ViewportViewModel(RenderCore core)
        {
            RenderSystem = new RenderSystem(core);
            if (!RenderSystem.Initialize())
            {
                Application.Current.Shutdown();
            }
        }
    }
}
