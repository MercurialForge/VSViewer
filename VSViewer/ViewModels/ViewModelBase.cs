using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;

namespace VSViewer.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        protected ViewModelBase()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private Visibility m_toolVisibility = Visibility.Visible;
        public Visibility ToolVisiblity
        {
            get { return m_toolVisibility; }
            set
            {
                m_toolVisibility = value;
                OnPropertyChanged("ToolVisiblity");
            }
        }

        public void ShowTool()
        {
            ToolVisiblity = Visibility.Visible;
        }

        public void HideTool ()
        {
            ToolVisiblity = Visibility.Collapsed;
        }

        public void Dispose()
        {
            this.OnDispose();
        }

        protected virtual void OnDispose()
        {
        }
    }
}
