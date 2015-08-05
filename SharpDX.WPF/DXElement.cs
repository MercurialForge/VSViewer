using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SharpDX.WPF
{
    /// <summary>
    /// A <see cref="UIElement"/> displaying DirectX scene. 
    /// Takes care of resizing and refreshing a <see cref="DXImageSource"/>.
    /// It does no Direct3D work, which is delegated to
    /// the <see cref="IDirect3D"/> <see cref="Renderer"/> object.
    /// </summary>
    public class DXElement : FrameworkElement, INotifyPropertyChanged
    {
        #region Init

        /// <summary>
        /// 
        /// </summary>
        public DXElement()
        {
            base.SnapsToDevicePixels = true;

            m_renderTimer = new Stopwatch();
            m_surface = new DXImageSource();
            m_surface.IsFrontBufferAvailableChanged += delegate
            {
                UpdateReallyLoopRendering();
                if (!m_isReallyLoopRendering && m_surface.IsFrontBufferAvailable)
                    Render();
            };
            IsVisibleChanged += delegate { UpdateReallyLoopRendering(); };
        }

        #endregion

        #region Dependency Property

        /// <summary>
        /// The D3D device that will handle the drawing
        /// </summary>
        public IDirect3D Renderer
        {
            get { return (IDirect3D)GetValue(RendererProperty); }
            set { SetValue(RendererProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty RendererProperty =
            DependencyProperty.Register(
                "Renderer",
                typeof(IDirect3D),
                typeof(DXElement),
                new PropertyMetadata((d, e) => ((DXElement)d).OnRendererChanged((IDirect3D)e.OldValue, (IDirect3D)e.NewValue)));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private void OnRendererChanged(IDirect3D oldValue, IDirect3D newValue)
        {
            UpdateSize();
            UpdateReallyLoopRendering();
            Focusable = newValue is IInteractiveDirect3D;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The image source where the DirectX scene (from the <see cref="Renderer"/>) will be rendered.
        /// </summary>
        public DXImageSource Surface { get { return m_surface; } } 

        /// <summary>
        /// Wether or not the DirectX scene will be redrawn continuously
        /// </summary>
        public bool IsLoopRendering
        {
            get { return m_isLoopRendering; }
            set
            {
                if (value == m_isLoopRendering)
                    return;
                m_isLoopRendering = value;
                UpdateReallyLoopRendering();
                OnPropertyChanged("IsLoopRendering");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control is in design mode
        /// (running in Blend or Visual Studio).
        /// </summary>
        public bool IsInDesignMode
        {
            get { return DesignerProperties.GetIsInDesignMode(this); }
        }

        #endregion

        #region Protected Size Overrides

        /// <summary>
        /// 
        /// </summary>        
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (IsInDesignMode)
                return;
            UpdateReallyLoopRendering();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            base.ArrangeOverride(finalSize);
            UpdateSize();
            return finalSize;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            int w = (int)Math.Ceiling(availableSize.Width);
            int h = (int)Math.Ceiling(availableSize.Height);
            return new System.Windows.Size(w, h);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// 
        /// </summary>        
        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawImage(Surface, new Rect(RenderSize));
        }

        /// <summary>
        /// 
        /// </summary>
        protected override int VisualChildrenCount { get { return 0; } }
        
        #endregion

        #region Private: ..LoopRendering.., UpdateSize

        /// <summary>
        /// 
        /// </summary>
        private void UpdateReallyLoopRendering()
        {
            var newValue =
                !IsInDesignMode
                && IsLoopRendering
                && Renderer != null
                && Surface.IsFrontBufferAvailable
                && VisualParent != null
                && IsVisible
                ;

            if (newValue != m_isReallyLoopRendering)
            {
                m_isReallyLoopRendering = newValue;
                if (m_isReallyLoopRendering)
                {
                    m_renderTimer.Start();
                    CompositionTarget.Rendering += OnLoopRendering;
                }
                else
                {
                    CompositionTarget.Rendering -= OnLoopRendering;
                    m_renderTimer.Stop();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnLoopRendering(object sender, EventArgs e) 
        {
            if (!m_isReallyLoopRendering)
                return;
            Render(); 
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateSize()
        {
            if (Renderer == null)
                return;
            Renderer.Reset(GetDrawEventArgs());
        }

        #endregion
        
        #region Render

        /// <summary>
        /// Will redraw the underlying surface once.
        /// </summary>
        public void Render()
        {
            if (Renderer == null || IsInDesignMode)
                return;

            Renderer.Render(GetDrawEventArgs());
            Surface.Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DrawEventArgs GetDrawEventArgs()
        {
            var eargs = new DrawEventArgs
            {
                TotalTime = m_renderTimer.Elapsed,
                DeltaTime = m_lastDrawEventArgs != null ? m_renderTimer.Elapsed - m_lastDrawEventArgs.TotalTime : TimeSpan.Zero,
                RenderSize = DesiredSize,
                Target = Surface,
            };
            m_lastDrawEventArgs = eargs;
            return eargs;
        }

        #endregion

        #region Override Mouse and Key Events

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (Renderer is IInteractiveDirect3D)
                ((IInteractiveDirect3D)Renderer).OnMouseDown(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (Renderer is IInteractiveDirect3D)
                ((IInteractiveDirect3D)Renderer).OnMouseMove(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (Renderer is IInteractiveDirect3D)
                ((IInteractiveDirect3D)Renderer).OnMouseUp(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (Renderer is IInteractiveDirect3D)
                ((IInteractiveDirect3D)Renderer).OnMouseWheel(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Renderer is IInteractiveDirect3D)
                ((IInteractiveDirect3D)Renderer).OnKeyDown(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (Renderer is IInteractiveDirect3D)
                ((IInteractiveDirect3D)Renderer).OnKeyUp(this, e);
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged(string name)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Fields
        private bool m_isLoopRendering = true;
        private bool m_isReallyLoopRendering;
        private DrawEventArgs m_lastDrawEventArgs;
        private DXImageSource m_surface;
        private Stopwatch m_renderTimer;
        #endregion
    }
}
