using System;
using System.Drawing;
using System.IO;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

using Linn;
using Linn.Toolkit;
using Linn.Toolkit.Mac;

using OpenHome.Xapp;


namespace Linn.Wizard
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate ()
        {
        }

        public override void FinishedLaunching (NSObject notification)
        {
            NSImage img = new NSImage(NSBundle.MainBundle.PathForImageResource("Icon106x106.png"));

            // create the helpers
            iHelper = new Helper(Environment.GetCommandLineArgs());
            iHelper.ProcessOptionsFileAndCommandLine();

            CrashLogDumperWindowController d = new CrashLogDumperWindowController(img, iHelper.Title, iHelper.Product, iHelper.Version);
            d.LoadWindow();
            iHelper.AddCrashLogDumper(d);

            iHelperAutoUpdate = new HelperAutoUpdate(iHelper, new Linn.Toolkit.Mac.ViewAutoUpdateStandard(img), new Linn.Toolkit.Mac.Invoker());
            iHelperAutoUpdate.Start();

            // create the xapp components
            iXapp = new Framework<Session>(Path.Combine(OpenHome.Xen.Environment.AppPath, "PageHtml"));
            iWebServer = new WebServer(iXapp);
            iControl = new PageControl(iHelper, iXapp, Path.Combine(OpenHome.Xen.Environment.AppPath, "PageHtml"), "PageDefinitions.xml");
            iControl.EventCloseApplicationRequested += CloseApplicationRequested;

            // create and show the main window
            iMainWindow = new MainWindowController();
            iMainWindow.Window.Title = iHelper.Product;            
            iViewBrowser = new ViewerBrowser(iMainWindow.WebView, iWebServer.ResourceUri);
            iMainWindow.Window.MakeKeyAndOrderFront(this);
        }
        
        public override void WillTerminate (NSNotification notification)
        {
            if (iHelperAutoUpdate != null)
            {
                iHelperAutoUpdate.Dispose();
            }
            
            iXapp.Dispose();
            iWebServer.Dispose();
            iControl.Close();
        }
        
        public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
        {
            return true;
        }
        
        private void CloseApplicationRequested(object sender, EventArgs e)
        {
            Invoker invoker = new Invoker();
            if (invoker.TryBeginInvoke(new EventHandler<EventArgs>(CloseApplicationRequested), sender, e)) {
                return;
            }

            iMainWindow.Close();
        }

        private Helper iHelper;
        private HelperAutoUpdate iHelperAutoUpdate;
        private Framework iXapp;
        private WebServer iWebServer;
        private PageControl iControl;
        private MainWindowController iMainWindow;
        private ViewerBrowser iViewBrowser;
    }
}

