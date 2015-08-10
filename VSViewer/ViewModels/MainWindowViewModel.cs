using System.Collections.ObjectModel;

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
            ViewportView = new ViewportViewModel();
            AddToolBarTool(new ImporterViewModel(ViewportView, this));
        }

        public void AddToolBarTool (ViewModelBase tool)
        {
            ObservableCollection<ViewModelBase> updatedToolBarCollection = new ObservableCollection<ViewModelBase>();
            for (int i = 0; i < ToolBarViewModels.Count; i++ )
            {
                updatedToolBarCollection.Add(ToolBarViewModels[i]);
            }
            updatedToolBarCollection.Add(tool);
            ToolBarViewModels = updatedToolBarCollection;
        }
    }
}
