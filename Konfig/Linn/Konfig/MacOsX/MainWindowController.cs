
using System;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.WebKit;

namespace Linn.Konfig
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
        public MainWindowController (string aProduct) : base ("MainWindow")
        {
            Initialize ();
            Window.Title = aProduct;
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

        public WebView WebView
        {
            get
            {
                return iWebView;
            }
        }
    }
}

