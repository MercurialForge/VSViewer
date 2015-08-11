using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.Models;

namespace VSViewer.ViewModels
{
    class TexturesViewModel : ViewModelBase
    {

        MainWindowViewModel m_mainWindowModelView;

        public TexturesViewModel(MainWindowViewModel mainWindowModelView, RenderCore renderCore)
        {
            m_mainWindowModelView = mainWindowModelView;
        }
    }
}
