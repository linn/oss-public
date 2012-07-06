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

using OpenHome.Xapp;

namespace RotaryControlXappPrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Framework iFramework;
        private WebServer iWebServer;

        private MainPage iPage;

        private ViewerBrowser iViewer;

        public MainWindow()
        {
            InitializeComponent();

            window.Left = System.Windows.SystemParameters.PrimaryScreenWidth - window.Width;
            window.Top = System.Windows.SystemParameters.PrimaryScreenHeight - window.Height - 30;

            iFramework = new Framework(OpenHome.Xen.Environment.AppPath + "/presentation");
            iFramework.AddCss("main/index.css");

            iPage = new MainPage("main", "main");
            iFramework.AddPage(iPage);

            iWebServer = new WebServer(iFramework);

            iViewer = new ViewerBrowser(webBrowser1, iWebServer.ResourceUri + iPage.UriPath);
        }
    }
}
