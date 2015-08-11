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
            EnableImporter();
        }

        public bool EnableImporter()
        {
            foreach (ViewModelBase vmb in ToolBarViewModels)
            {
                if (vmb is ImporterViewModel) { return false; }
            }
            AddToolBarTool(new ImporterViewModel(ViewportView, this));
            return true;
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
