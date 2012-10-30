using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Linn.Toolkit.Ios;

using OpenHome.Xapp;

namespace Linn.Konfig
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register ("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate, IUpdateListener
    {
        // class-level declarations
        private UIWindow iWindow;
        private KonfigViewController iViewController;
        private Helper iHelper;
        private XappController iXappController;
        private ViewerBrowser iViewer;

        #region IUpdateListener implementation
        public void SetUpdating(bool aUpdating)
        {
        }
        #endregion

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            // create the app helper
            iHelper = new Helper(Environment.GetCommandLineArgs());
            iHelper.ProcessOptionsFileAndCommandLine();

            // add a crash log dumper
            CrashLogDumper d = new CrashLogDumper(iHelper.Title, iHelper.Product, iHelper.Version);
            //d.SetAutoSend(false);
            iHelper.AddCrashLogDumper(d);

            iWindow = new UIWindow (UIScreen.MainScreen.Bounds);
            
            iViewController = new KonfigViewController();
            iWindow.RootViewController = iViewController;
            
            Preferences preferences = new Preferences(iHelper);
            Model.Instance = new Model(preferences);

            // create the xapp controller and view
            Invoker invoker = new Invoker();
            PageBase page = new SettingsPageBasic(invoker, preferences, "settings", "settings");
            iXappController = new XappController(invoker, iHelper, Model.Instance, preferences, page, this);

            iWindow.MakeKeyAndVisible();

            iViewer = new ViewerBrowser(iViewController.WebView, iXappController.MainPageUri);

            return true;
        }

        public override void WillTerminate (UIApplication application)
        {
            iXappController.Dispose();
            iXappController = null;

            iViewer.Dispose();
            iViewer = null;
        }
    }
}

