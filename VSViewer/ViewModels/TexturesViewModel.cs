using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.ViewModels
{
    class TexturesViewModel : ViewModelBase
    {

        ViewportViewModel m_viewport;

        public TexturesViewModel(ViewportViewModel viewport)
        {
            m_viewport = viewport;
        }
    }
}
