using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.Models;

namespace VSViewer.ViewModels
{
    public class TexturesToolViewModel : ViewModelBase
    {
        MainWindowViewModel m_mainWindowModelView;
        public RenderCore Core { get; set; }

        public TexturesToolViewModel(MainWindowViewModel mainWindowModelView, RenderCore renderCore)
        {
            m_mainWindowModelView = mainWindowModelView;
            Core = renderCore;
        }

        public ICommand TextureSelected
        {
            get { return new RelayCommand(x => SendTextureSelected(x)); }
        }

        internal void SendTextureSelected(object sentObject)
        {
            m_mainWindowModelView.RenderCore.TextureRequiresUpdate = true;
            m_mainWindowModelView.RenderCore.TextureIndex = (int)sentObject;
        }
    }
}
