using System;
using System.Reflection;
using System.Windows;

namespace VSViewer.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        public string BuildInformation
        {
            get
            {
                Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;

                return string.Format("[Version: {0}]", appVersion);
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
