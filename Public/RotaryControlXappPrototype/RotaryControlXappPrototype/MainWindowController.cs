using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

using OpenHome.Xapp;

namespace RotaryControlXappPrototype
{
    public partial class MainWindowController : MonoMac.AppKit.NSWindowController
    {
        #region Constructors
        
        // Called when created from unmanaged code
        public MainWindowController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public MainWindowController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        
        // Call to load from the XIB/NIB file
        public MainWindowController () : base ("MainWindow")
        {
            Initialize ();
        }
        
        // Shared initialization code
        void Initialize ()
        {
        }
        
        #endregion
        
        //strongly typed window accessor
        public new MainWindow Window {
            get {
                return (MainWindow)base.Window;
            }
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();

            iFramework = new Framework(OpenHome.Xen.Environment.AppPath + "/presentation");
            iFramework.AddCss("main/index.css");

            iPage = new MainPage("main", "main");
            iFramework.AddPage(iPage);

            iWebServer = new WebServer(iFramework);
            iViewer = new ViewerBrowser(iWebView, iWebServer.ResourceUri + iPage.UriPath);
        }

        private Framework iFramework;
        private WebServer iWebServer;

        private MainPage iPage;

        private ViewerBrowser iViewer;
    }
}

