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
        public ImporterToolViewModel ImporterTool { get; set; }
        public TexturesToolViewModel TextureTool { get; set; }
        public AnimationToolViewModel AnimationTool { get; set; }
        public ViewportToolViewModel ViewportTool { get; set; }
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
            ImporterTool = new ImporterToolViewModel(this, RenderCore);
            TextureTool = new TexturesToolViewModel(this, RenderCore);
            AnimationTool = new AnimationToolViewModel(this);
            ViewportTool = new ViewportToolViewModel();

            TextureTool.HideTool();
            AnimationTool.HideTool();
            ViewportTool.HideTool();

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
    }
}
