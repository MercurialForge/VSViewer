using System;
using System.Collections.ObjectModel;
using VSViewer.Models;

namespace VSViewer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // The main viewport
        public ViewportViewModel ViewportViewModel { get; set; }
        public ImporterViewModel ImporterTool { get; set; }
        public TexturesViewModel TextureTool { get; set; }
        public AnimationViewModel AnimationTool { get; set; }
        static public RenderCore RenderCore { get; set; }

        // The tool bar stack
        public ObservableCollection<ViewModelBase> ToolBarViewModels
        {
            get
            {
                return m_toolBarViewModels;
            }
            private set
            {
                m_toolBarViewModels = value;
                OnPropertyChanged("ToolBarViewModels");
            }
        }

        ObservableCollection<ViewModelBase> m_toolBarViewModels = new ObservableCollection<ViewModelBase>();

        public MainWindowViewModel()
        {
            RenderCore = new RenderCore();
            ViewportViewModel = new ViewportViewModel(RenderCore);
            ImporterTool = new ImporterViewModel(this, RenderCore);
            TextureTool = new TexturesViewModel(this, RenderCore);
            AnimationTool = new AnimationViewModel();
        }

        private void AddToolBarTool(ViewModelBase tool)
        {
            ObservableCollection<ViewModelBase> updatedToolBarCollection = new ObservableCollection<ViewModelBase>();
            for (int i = 0; i < ToolBarViewModels.Count; i++)
            {
                updatedToolBarCollection.Add(ToolBarViewModels[i]);
            }
            updatedToolBarCollection.Add(tool);
            ToolBarViewModels = updatedToolBarCollection;
        }
    }
}
