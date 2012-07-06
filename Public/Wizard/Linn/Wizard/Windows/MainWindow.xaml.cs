using System;
using System.Collections.Generic;
/*
using System.Linq;
using System.Text;
 */
using System.Windows;
/*
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
*/
using OpenHome.Xapp;
using Linn;

namespace Linn.Wizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(Helper aHelper, string aWebServerUri, PageControl aControl)
        {
            InitializeComponent();
            this.Title = aHelper.Product;

            Viewer viewer = new ViewerBrowser(webBrowser1, aWebServerUri);
            aControl.EventCloseApplicationRequested += CloseApplicationRequested;
        }

        private void CloseApplicationRequested(object sender, EventArgs e) {
            Dispatcher.BeginInvoke((Action)(() => {
                this.Close();
            }));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
