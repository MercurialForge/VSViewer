using System;
using System.Collections.ObjectModel;
using System.Threading;
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
        public RenderCore RenderCore { get; set; }

        public bool IsAnimationToolEnabled
        {
            get { return m_isAnimationToolEnabled; }
            set
            {
                m_isAnimationToolEnabled = value;
                OnPropertyChanged("IsAnimationToolEnabled");
            }
        }

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
        private bool m_isAnimationToolEnabled = false;
        private Timer m_tickTimer;
        private DateTime m_previousDeltaQuery;

        public MainWindowViewModel()
        {
            RenderCore = new RenderCore();
            ViewportViewModel = new ViewportViewModel(RenderCore);
            ImporterTool = new ImporterViewModel(this, RenderCore);
            TextureTool = new TexturesViewModel(this, RenderCore);
            AnimationTool = new AnimationViewModel(this);

            TextureTool.HideTool();
            AnimationTool.HideTool();

            // Set default tick timer
            m_tickTimer = new Timer(new TimerCallback(this.Tick), null, 0, 16);
            m_previousDeltaQuery = DateTime.Now;
        }

        public void Tick(object objectState)
        {
            TimeSpan deltaTime = new TimeSpan(0, 0, 0, 0, (DateTime.Now - m_previousDeltaQuery).Milliseconds);
            AnimationTool.Tick(deltaTime);
            m_previousDeltaQuery = DateTime.Now;
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
