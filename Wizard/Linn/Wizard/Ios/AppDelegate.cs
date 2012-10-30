using System;
using System.IO;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Linn;
using Linn.Toolkit;
using Linn.Toolkit.Ios;

using OpenHome.Xapp;

namespace Linn.Wizard
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register ("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;
        WizardViewController viewController;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            //NSImage img = new NSImage(NSBundle.MainBundle.PathForImageResource("Icon106x106.png"));
  
            // create the helpers
            iHelper = new Helper(Environment.GetCommandLineArgs());
            iHelper.ProcessOptionsFileAndCommandLine();

            CrashLogDumper d = new CrashLogDumper(iHelper.Title, iHelper.Product, iHelper.Version);
            iHelper.AddCrashLogDumper(d);

            // create the xapp components
            iXapp = new Framework<Session>(Path.Combine(OpenHome.Xen.Environment.AppPath, "PageHtml"));
            iWebServer = new WebServer(iXapp);

            // create and show the main window
            window = new UIWindow (UIScreen.MainScreen.Bounds);
            
            viewController = new WizardViewController ();
            window.RootViewController = viewController;
            window.MakeKeyAndVisible ();
            
            return true;
        }

        public override void OnActivated (UIApplication application)
        {
            Assert.Check(iControl == null);
            iControl = new PageControl(iHelper, iXapp, Path.Combine(OpenHome.Xen.Environment.AppPath, "PageHtml"), "PageDefinitions.xml");
            //iControl.EventCloseApplicationRequested += ;

            Assert.Check(iViewBrowser == null);
            iViewBrowser = new ViewerBrowser(viewController.WebView, iWebServer.ResourceUri);
        }

        public override void OnResignActivation (UIApplication application)
        {
            Cleanup();
        }

        public override void DidEnterBackground (UIApplication application)
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if(iControl != null)
            {
                iControl.Close();
                iControl = null;
            }

            if(iViewBrowser != null)
            {
                iViewBrowser.Dispose();
                iViewBrowser = null;
            }
        }

        private Helper iHelper;
        private Framework iXapp;
        private WebServer iWebServer;
        private PageControl iControl;
        private ViewerBrowser iViewBrowser;
    }
}

