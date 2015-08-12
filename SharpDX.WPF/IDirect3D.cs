using System;
using System.Windows;
using System.Windows.Input;

namespace SharpDX.WPF
{
    /// <summary>
    /// The DirectX renderer displayed by the DXElement
    /// </summary>
    public interface IDirect3D
    {
        void Render(DrawEventArgs args);

        void Reset(DrawEventArgs args);
    }

    /// <summary>
    /// A IDirect3D context which handles input event as well
    /// </summary>
    public interface IInteractiveDirect3D : IDirect3D
    {
        void OnKeyDown(UIElement ui, KeyEventArgs e);

        void OnKeyUp(UIElement ui, KeyEventArgs e);

        void OnMouseDown(UIElement ui, MouseButtonEventArgs e);

        void OnMouseMove(UIElement ui, MouseEventArgs e);

        void OnMouseUp(UIElement ui, MouseButtonEventArgs e);

        void OnMouseWheel(UIElement ui, MouseWheelEventArgs e);
    }

    public class DrawEventArgs : EventArgs
    {
        public DrawEventArgs()
        {
            TotalTime = TimeSpan.Zero;
        }

        public TimeSpan DeltaTime { get; set; }

        public System.Windows.Size RenderSize { get; set; }

        public DXImageSource Target { get; set; }

        public TimeSpan TotalTime { get; set; }
    }
}