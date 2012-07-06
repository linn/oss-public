using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.WebKit;

using OpenHome.Xapp;
using OpenHome.MediaServer;

namespace Linn.Songbox
{
	public partial class ConfigurationWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public ConfigurationWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public ConfigurationWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public ConfigurationWindowController (Server aServer) : base ("ConfigurationWindow")
		{
            iServer = aServer;
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
            Window.DidBecomeKey += WindowDidBecomeKey;
            Window.WillClose += WindowWillClose;
		}
		
		#endregion
		
		//strongly typed window accessor
		public new ConfigurationWindow Window {
			get {
				return (ConfigurationWindow)base.Window;
			}
		}

        private void WindowDidBecomeKey (object sender, EventArgs e)
        {
            if(iViewer == null)
            {
                iViewer = new ViewerBrowser(iWebView, iServer.PresentationUri);
            }
        }

        private void WindowWillClose (object sender, EventArgs e)
        {
            if (iViewer != null)
            {
                iViewer.Dispose();
                iViewer = null;
            }
        }

        private Server iServer;
        private ViewerBrowser iViewer;
	}
}

