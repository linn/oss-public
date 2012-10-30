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
using System.Windows.Shapes;
using System.Drawing;
using System.IO;

using OpenHome.Xapp;
using OpenHome.MediaServer;

namespace Linn.Songbox
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private Server iServer;
        private ViewerBrowser iViewer;

        public ConfigurationWindow(Icon aIcon, string aTitle, Server aServer)
        {
            InitializeComponent();

            WebBrowser.Focus();

            iServer = aServer;

            MemoryStream iconStream = new MemoryStream();
            aIcon.Save(iconStream);
            iconStream.Seek(0, SeekOrigin.Begin);
            Icon = BitmapFrame.Create(iconStream);

            Title = aTitle;
            //Background = new SolidColorBrush(System.Windows.Media.Colors.Black);
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (iViewer != null)
            {
                iViewer.Dispose();
                iViewer = null;
            }

            if (IsVisible)
            {
                iViewer = new ViewerBrowser(WebBrowser, iServer.PresentationUri);
                WebBrowser.Focus();
            }
        }
    }
}
