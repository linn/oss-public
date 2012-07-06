
using System;
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

using OpenHome.Xapp;

using Linn;
using Linn.Toolkit;
using Linn.Toolkit.Mac;


namespace Linn.Songcast
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }

        public override void AwakeFromNib()
        {
            // initialise resigned state
            iSessionResigned = false;

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
            iHelperAutoUpdate = new HelperAutoUpdate(iHelper, new Linn.Toolkit.Mac.ViewAutoUpdateStandard(largeIcon), new Invoker(), (s, e) => {});
            iHelperAutoUpdate.Start();
            
            // create the main songcast model
            iModel = new Model(new Invoker(), iHelper);
            iModel.EventEnabledChanged += ModelEnabledChanged;

            // create the preferences 'view' for communicating with the system preferences app
            iViewPreferences = new ViewPreferences(new Invoker(), iModel, iHelperAutoUpdate);

            // creating the status item with a length of -2 is equivalent to the call
            // [[NSStatusBar systemStatusBar] statusItemWithLength:NSSquareStatusItemLength]
            iStatusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(-2);
            iStatusItem.HighlightMode = false;
            iStatusItem.Target = this;
            iStatusItem.Action = new Selector("statusItemClicked:");
            
            // setup system event notifications
            NSNotificationCenter center = NSWorkspace.SharedWorkspace.NotificationCenter;
            center.AddObserver(this, new Selector("willSleep:"), new NSString("NSWorkspaceWillSleepNotification"), null);
            center.AddObserver(this, new Selector("didWake:"), new NSString("NSWorkspaceDidWakeNotification"), null);
            center.AddObserver(this, new Selector("sessionDidResignActive:"), new NSString("NSWorkspaceSessionDidResignActiveNotification"), null);
            center.AddObserver(this, new Selector("sessionDidBecomeActive:"), new NSString("NSWorkspaceSessionDidBecomeActiveNotification"), null);            

            // create the main window
            iMainWindow = new MainWindowController();
            iMainWindow.LoadWindow();
            iMainWindow.Window.DidResignKey += MainWindowDidResignKey;
            iMainWindow.Window.CollectionBehavior = NSWindowCollectionBehavior.CanJoinAllSpaces;
            
            // create the xapp controller and view
            iXappController = new XappController(iModel, new Invoker());
            iXappController.MainPage.EventShowConfig += ShowConfig;
            iXappController.MainPage.EventShowHelp += ShowHelp;
            iViewer = new ViewerBrowser(iMainWindow.WebView, iXappController.MainPageUri);
        }

        public override void FinishedLaunching(NSObject notification)
        {
            // start songcast if the user session is active
            if (!iSessionResigned) {
                StartSongcast();
            }
        }
        
        public override void WillTerminate (NSNotification notification)
        {
            iHelperAutoUpdate.Dispose();
            
            iModel.Stop();
            iModel = null;
            iViewPreferences = null;
            
            NSStatusBar.SystemStatusBar.RemoveStatusItem(iStatusItem);
            iStatusItem.Dispose();
        }
        
        private void StartSongcast()
        {
            string domain = NSBundle.MainBundle.InfoDictionary.ObjectForKey(new NSString("SongcastDomain")).ToString();
            string manufac = NSBundle.MainBundle.InfoDictionary.ObjectForKey(new NSString("SongcastManufacturerName")).ToString();
            string manufacUrl = NSBundle.MainBundle.InfoDictionary.ObjectForKey(new NSString("SongcastManufacturerUrl")).ToString();
            string modelUrl = NSBundle.MainBundle.InfoDictionary.ObjectForKey(new NSString("SongcastModelUrl")).ToString();
            string senderIconFile = NSBundle.MainBundle.PathForResource("IconLarge", "png");
            byte[] senderIcon = System.IO.File.ReadAllBytes(senderIconFile);

            iModel.Start(domain, manufac, manufacUrl, modelUrl, senderIcon, "image/png");
        }

        [Export("statusItemClicked:")]
        private void StatusItemClicked(NSObject aSender)
        {
            if (iMainWindow.Window.IsVisible) {
                // the window is already visible - another click hides it
                iMainWindow.Window.OrderOut(this);
                return;
            }

            // must set the application to be activated so it receives messages
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

            // position the window
            RectangleF statusItemFrame = NSApplication.SharedApplication.CurrentEvent.Window.Frame;
            RectangleF mainWindowFrame = iMainWindow.Window.Frame;
            
            // set the vertical position of the window - simple as menu bar is always at the top
            mainWindowFrame.Y = statusItemFrame.Y - mainWindowFrame.Height;

            // determine whether the window should be left or right justified wrt the status item
            NSScreen screen = NSApplication.SharedApplication.CurrentEvent.Window.Screen;
            if (screen != null)
            {
                if (statusItemFrame.Left + mainWindowFrame.Width < screen.Frame.Right) {
                    // aligning the left edge of the main window with the left edge of the status item fits on screen
                    mainWindowFrame.X = statusItemFrame.X;
                }
                else {
                    // aligning the left edge of the main window with the left edge of the status item does not fit on screen
                    // so it their right edges need to be aligned
                    mainWindowFrame.X = statusItemFrame.Right - mainWindowFrame.Width;
                }
            }
            else
            {
                // don't know screen width - just align left edges
                mainWindowFrame.X = statusItemFrame.X;
            }
            
            iMainWindow.Window.SetFrame(mainWindowFrame, true);
                        
            // show the window
            iMainWindow.ShowWindow(this);
            iMainWindow.Window.MakeKeyAndOrderFront(this);
        }
        
        private void MainWindowDidResignKey(object sender, EventArgs e)
        {
            iMainWindow.Window.OrderOut(this);
        }
        
        private void ShowConfig(object sender, EventArgs e)
        {
            string prefPane = NSBundle.MainBundle.InfoDictionary.ObjectForKey(new NSString("SongcastPreferencePane")).ToString();
            NSWorkspace.SharedWorkspace.OpenFile(prefPane);
        }
        
        private void ShowHelp(object sender, EventArgs e)
        {
            NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(Model.kOnlineManualUri));
        }

        [Export("willSleep:")]
        private void WillSleep(NSNotification aNotification)
        {
            // going into hibernation - switch off songcast
            iModel.Enabled = false;
        }

        [Export("didWake:")]
        private void DidWake(NSNotification aNotification)
        {
        }

        [Export("sessionDidResignActive:")]
        private void SessionDidResignActive(NSNotification aNotification)
        {
            // set the resigned state
            iSessionResigned = true;

            // user session has been resigned - stop songcast - this will do nothing
            // if the model has not been started i.e. this notification has occurred on
            // startup of the application
            iModel.Stop();
        }

        [Export("sessionDidBecomeActive:")]
        private void SessionDidBecomeActive(NSNotification aNotification)
        {
            // set the resigned state
            iSessionResigned = false;

            // user session has just become active again - restart songcast
            StartSongcast();
        }

        private void ModelEnabledChanged(object sender, EventArgs e)
        {
            if (iModel.Enabled)
            {
                iStatusItem.Image = new NSImage(NSBundle.MainBundle.PathForImageResource("SysTrayIconOn.png"));
            }
            else
            {
                iStatusItem.Image = new NSImage(NSBundle.MainBundle.PathForImageResource("SysTrayIconOff.png"));
            }
        }
        
        private NSStatusItem iStatusItem;
        private Model iModel;
        private Helper iHelper;
        private HelperAutoUpdate iHelperAutoUpdate;
        private bool iSessionResigned;
        private MainWindowController iMainWindow;
        private XappController iXappController;
        private ViewerBrowser iViewer;
        private ViewPreferences iViewPreferences;
    }
}

