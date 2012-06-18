using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Input;
using System.Drawing;
using System.Net;

namespace KinskyDesktopWpf
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        [STAThread()]
        static void Main()
        {
            HttpWebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
            HttpWebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            App app = new App();
            app.Run();
        }

        public App()
        {
            InitializeComponent();
            iImageCache = new WpfImageCache(kCacheSize, kDownscaleImageSize, kThreadCount);
            iIconResolver = new IconResolver();
        }

        public WpfImageCache ImageCache
        {
            get
            {
                return iImageCache;
            }
        }

        public IconResolver IconResolver
        {
            get
            {
                return iIconResolver;
            }
        }

        private const int kCacheSize = 100 * 1024 * 1024;
        private const int kThreadCount = 4;
        private const int kDownscaleImageSize = 128;
        private WpfImageCache iImageCache;
        private IconResolver iIconResolver;
    }
}
