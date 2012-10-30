using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

using Linn;
using Linn.Toolkit;
using Linn.Toolkit.Mac;

using OpenHome.Xapp;

namespace Linn.Konfig
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        public class MainWindowDelegate : NSWindowDelegate, IUpdateListener
        {
            public MainWindowDelegate(NSWindow aWindow, string aProduct, NSImage aIcon)
            {
                iWindow = aWindow;
                iProduct = aProduct;
                iIcon = aIcon;
            }

            public override bool WindowShouldClose(NSObject sender)
            {
                if(iUpdating)
                {
                    NSAlert alert = new NSAlert();
                    alert.Icon = iIcon;
                    alert.MessageText = string.Format("{0} cannot be closed", iProduct);
                    alert.InformativeText = string.Format("{0} is in the process of changing product software", iProduct);
                    alert.AddButton("OK");
                    alert.AlertStyle = NSAlertStyle.Warning;
                    alert.BeginSheet(iWindow);
                }

                return !iUpdating;
            }

            public void SetUpdating(bool aUpdating)
            {
                iUpdating = aUpdating;
            }

            private NSWindow iWindow;
            private string iProduct;
            private NSImage iIcon;

            private bool iUpdating;
        }

        public AppDelegate ()
        {
        }

        public override void FinishedLaunching (NSObject notification)
        {
            // load some images from the bundle
            NSImage largeIcon = new NSImage(NSBundle.MainBundle.PathForImageResource("IconLarge.png"));

            // create the app helper
            iHelper = new Helper(Environment.GetCommandLineArgs());
            iHelper.ProcessOptionsFileAndCommandLine();

            // add a crash log dumper
            CrashLogDumperWindowController d = new CrashLogDumperWindowController(largeIcon, iHelper.Title, iHelper.Product, iHelper.Version);
            d.LoadWindow();
            iHelper.AddCrashLogDumper(d);

            // create auto update view and helper
            iHelperAutoUpdate = new HelperAutoUpdate(iHelper, new Linn.Toolkit.Mac.ViewAutoUpdateStandard(largeIcon), new Invoker());
            iHelperAutoUpdate.Start();

            iMainWindowController = new MainWindowController(iHelper.Product);
            MainWindowDelegate windowDelegate = new MainWindowDelegate(iMainWindowController.Window, iHelper.Title, largeIcon);
            iMainWindowController.Window.Delegate = windowDelegate;
            iMainWindowController.Window.MakeKeyAndOrderFront(this);
            
            Preferences preferences = new Preferences(iHelper);
            Model.Instance = new Model(preferences);

            // create the xapp controller and view
            Invoker invoker = new Invoker();
            SettingsPageAdvanced settings = new SettingsPageAdvanced(invoker, Model.Instance, preferences, iHelperAutoUpdate, "settings", "settings");
            UpdateListenerRepeater listeners = new UpdateListenerRepeater(new IUpdateListener[] { windowDelegate, settings });
            iXappController = new XappController(invoker, iHelper, Model.Instance, preferences, settings, listeners);
            iViewer = new ViewerBrowser(iMainWindowController.WebView, iXappController.MainPageUri);
        }

        public override void WillTerminate (NSNotification notification)
        {
            if(iHelperAutoUpdate != null)
            {
                iHelperAutoUpdate.Dispose();
                iHelperAutoUpdate = null;
            }

            if(iXappController != null)
            {
                iXappController.Dispose();
                iXappController = null;
            }

            if(iViewer != null)
            {
                iViewer.Dispose();
                iViewer = null;
            }
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
        {
            return true;
        }

        private Helper iHelper;
        private HelperAutoUpdate iHelperAutoUpdate;
        private MainWindowController iMainWindowController;
        private XappController iXappController;
        private ViewerBrowser iViewer;
    }
}

