using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Linn.Kinsky;
using Linn.ControlPoint.Upnp;
using Linn.Control.Ssdp;
using System.Net;
using Linn;
using System.Collections.ObjectModel;
using System.IO;

namespace KinskyDesktopWpf
{

    public partial class CrashLogDialog : Window
    {

        public CrashLogDialog(string aTitle, string aReportText)
        {
            InitializeComponent();
            this.Title = aTitle;
            txtMessage.Text = aTitle + " has encountered a problem and needs to close. We are sorry for the inconvenience";
            txtMessage2.Text = "We have created an error report that you can send to help us improve " + aTitle;
            txtErrorDetails.Text = aReportText;
            btnSendReport.Focus();
        }


        #region Command Bindings

        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
        #endregion

        private void btnSendReport_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void btnToggleDetails_Click(object sender, RoutedEventArgs e)
        {
            if (txtErrorDetails.Visibility == Visibility.Collapsed)
            {
                txtErrorDetails.Visibility = Visibility.Visible;
                Height = 514;
            }
            else
            {
                txtErrorDetails.Visibility = Visibility.Collapsed;
                Height = 238;
            }
        }
    }
}
