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
		public ConfigurationWindowController (Server aServer, Linn.Songbox.PageMain aPageMain) : base ("ConfigurationWindow")
		{
            iServer = aServer;
			iPageMain = aPageMain;
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
			iWindowDelegate = new ConfigurationWindowDelegate();
			iWindowDelegate.EventDidBecomeKey += WindowDidBecomeKey;
			iWindowDelegate.EventShouldClose += WindowShouldClose;
			iWindowDelegate.EventWillClose += WindowWillClose;
			Window.Delegate = iWindowDelegate;
		}

		#endregion
		
		//strongly typed window accessor
		public new ConfigurationWindow Window {
			get {
				return (ConfigurationWindow)base.Window;
			}
		}

		private void WindowShouldClose(object sender, EventArgs e)
		{
			Console.WriteLine("WindowShouldClose");
			iPageMain.TrackPageVisibilityChange(false);
			Window.IsVisible = false;
		}

        private void WindowDidBecomeKey (object sender, EventArgs e)
        {
            if(iViewer == null)
            {
                iViewer = new ViewerBrowser(iWebView, iServer.PresentationUri);
            }
			iPageMain.TrackPageVisibilityChange(true);
        }

        private void WindowWillClose (object sender, EventArgs e)
        {
			Console.WriteLine("WindowWillClose");
			iPageMain.TrackPageVisibilityChange(false);
            if (iViewer != null)
            {
                iViewer.Dispose();
                iViewer = null;
			}
			iWindowDelegate.EventDidBecomeKey -= WindowDidBecomeKey;
			iWindowDelegate.EventShouldClose -= WindowShouldClose;
			iWindowDelegate.EventWillClose -= WindowWillClose;
			iWindowDelegate = null;
        }

        private Server iServer;
        private ViewerBrowser iViewer;
		private PageMain iPageMain;
		private ConfigurationWindowDelegate iWindowDelegate;
	}

	public class ConfigurationWindowDelegate : MonoMac.AppKit.NSWindowDelegate
	{
		public event EventHandler<EventArgs> EventDidBecomeKey;
		public event EventHandler<EventArgs> EventShouldClose;
		public event EventHandler<EventArgs> EventWillClose;

		public ConfigurationWindowDelegate() : base()
		{
		}

		public override void DidBecomeKey (NSNotification notification)
		{
			EventHandler<EventArgs> del = EventDidBecomeKey;
			if (del != null){
				del(this, EventArgs.Empty);
			}
		}

		public override bool WindowShouldClose (NSObject sender)
		{
			EventHandler<EventArgs> del = EventShouldClose;
			if (del != null){
				del(this, EventArgs.Empty);
			}
			return false;
		}

		public override void WillClose (NSNotification notification)
		{
			EventHandler<EventArgs> del = EventWillClose;
			if (del != null){
				del(this, EventArgs.Empty);
			}
		}
	}
}

