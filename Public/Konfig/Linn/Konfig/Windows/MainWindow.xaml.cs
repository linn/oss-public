using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Linn.Konfig
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IUpdateListener
    {
        private object iLock;
        private bool iUpdating;
        private string iTitle;

        public MainWindow(string aProduct, string aTitle)
        {
            iLock = new object();
            iTitle = aTitle;

            InitializeComponent();

            Title = aProduct;
        }

        public void SetUpdating(bool aUpdating)
        {
            lock (iLock)
            {
                iUpdating = aUpdating;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            bool updating = false;
            lock (iLock)
            {
                updating = iUpdating;
            }

            if (updating)
            {
                MessageBox.Show(this, string.Format("{0} cannot be closed in the process of updating product software", iTitle), iTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            e.Cancel = updating;
        }
    }
}
