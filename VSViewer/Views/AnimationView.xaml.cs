﻿using System.Windows.Controls;
using System.Windows.Input;

namespace VSViewer.Views
{
    /// <summary>
    /// Interaction logic for AnimationView.xaml
    /// </summary>
    public partial class AnimationView : UserControl
    {
        public AnimationView()
        {
            InitializeComponent();
        }

        private void ValidateNumber(object sender, TextCompositionEventArgs e)
        {
            CheckIsNumeric(e);
        }

        private void CheckIsNumeric(TextCompositionEventArgs e)
        {
            int result;

            if (!(int.TryParse(e.Text, out result) || e.Text == "."))
            {
                e.Handled = true;
            }
        }
    }
}
