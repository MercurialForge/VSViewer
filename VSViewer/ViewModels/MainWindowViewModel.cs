using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using System.IO;
using VSViewer.FileFormats;
using VSViewer.Loader;
using SharpDX.WPF;
using SharpDX.Direct3D11;
using VSViewer.Rendering;
using System.Windows;
using System.Collections.ObjectModel;
using VSViewer.Common;
using System.Windows.Input;
using System.Windows.Media;

namespace VSViewer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // The main viewport
        public ViewportViewModel ViewportView { get; private set; }

        // The tool bar stack
        public ObservableCollection<ViewModelBase> ToolBarViewModels 
        {
            get
            {
                if(m_toolBarViewModels == null)
                {
                    m_toolBarViewModels = new ObservableCollection<ViewModelBase>();
                }
                return m_toolBarViewModels;
            }
        }

        ObservableCollection<ViewModelBase> m_toolBarViewModels;

        public MainWindowViewModel()
        {
            ViewportView = new ViewportViewModel();
            AddToolBarTool(new ImporterViewModel());
        }

        public void AddToolBarTool (ViewModelBase tool)
        {
            if(m_toolBarViewModels == null)
            {
                m_toolBarViewModels = new ObservableCollection<ViewModelBase>();
            }

            m_toolBarViewModels.Add(tool);
        }

    }
}
