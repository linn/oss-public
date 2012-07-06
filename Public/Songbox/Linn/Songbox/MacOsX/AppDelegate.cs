using System;
using System.Runtime.InteropServices;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using OpenHome.Xapp;
using OpenHome.MediaServer;

using Linn.Toolkit;
using Linn.Toolkit.Mac;


namespace Linn.Songbox
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate ()
        {
        }

        public override void FinishedLaunching (NSObject notification)
        {
            // load images for the status item and windows
            NSImage sysTrayImage = new NSImage(NSBundle.MainBundle.PathForImageResource("SysTrayIcon.png"));
            NSImage largeImage = new NSImage(NSBundle.MainBundle.PathForImageResource("Icon106x106.png"));
			
            // creating the status item with a length of -2 is equivalent to the call
            // [[NSStatusBar systemStatusBar] statusItemWithLength:NSSquareStatusItemLength]
            iStatusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(-2);
            iStatusItem.HighlightMode = true;
            iStatusItem.Menu = StatusMenu;
            iStatusItem.Image = sysTrayImage;

            // create the app helper
            iHelper = new Helper(Environment.GetCommandLineArgs());            
            iHelper.ProcessOptionsFileAndCommandLine();

            // create window for crash logging
            CrashLogDumperWindowController d = new CrashLogDumperWindowController(largeImage, iHelper.Title, iHelper.Product, iHelper.Version);
            d.LoadWindow();
            iHelper.AddCrashLogDumper(d);

            // create view and helper for the auto updates - hardcode check for beta versions for now
            IViewAutoUpdate autoUpdateView = new Linn.Toolkit.Mac.ViewAutoUpdateStandard(largeImage);
            iHelperAutoUpdate = new HelperAutoUpdate(iHelper, autoUpdateView, new Invoker());
            //iHelperAutoUpdate.OptionPageUpdates.BetaVersions = true;
            iHelperAutoUpdate.Start();

            // create the media server
            iServer = new Server("git://github.com/linnoss/MediaApps.git", iHelper.Company, "http://www.linn.co.uk", iHelper.Title, "http://www.linn.co.uk");

            // create the main configuration window
            iWindow = new ConfigurationWindowController(iServer);
            iWindow.LoadWindow();

            // initialise menu items
            UpdateMenuItem();
	    }
        
        public override void WillTerminate (NSNotification notification)
        {
            if (iHelperAutoUpdate != null)
            {
                iHelperAutoUpdate.Dispose();
            }
            if (iServer != null)
            {
                iServer.Dispose();
            }
        }

        partial void OpenConfiguration (NSObject aSender)
        {
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            iWindow.Window.MakeKeyAndOrderFront(this);
        }

        partial void CheckForUpdates (NSObject sender)
        {
            iHelperAutoUpdate.CheckForUpdates();
        }

        [DllImport("/System/Library/Frameworks/ServiceManagement.framework/ServiceManagement")]
        static extern bool SMLoginItemSetEnabled(IntPtr aId, bool aEnabled);

        partial void StartAtLogin (NSObject sender)
        {
            MonoMac.CoreFoundation.CFString id = new MonoMac.CoreFoundation.CFString("uk.co.linn.SongboxHelper");

            SMLoginItemSetEnabled(id.Handle, (iMenuItemStartAtLogin.State == NSCellStateValue.Off));

            UpdateMenuItem();
        }

        [DllImport("/System/Library/Frameworks/ServiceManagement.framework/ServiceManagement")]
        static extern IntPtr SMJobCopyDictionary(IntPtr aDomain, IntPtr aId);

        private void UpdateMenuItem()
        {
            MonoMac.CoreFoundation.CFString dom = new MonoMac.CoreFoundation.CFString("kSMDomainUserLaunchd");
            MonoMac.CoreFoundation.CFString id = new MonoMac.CoreFoundation.CFString("uk.co.linn.SongboxHelper");

            IntPtr dict = SMJobCopyDictionary(dom.Handle, id.Handle);

            if (dict != IntPtr.Zero) {
                iMenuItemStartAtLogin.State = NSCellStateValue.On;
            }
            else {
                iMenuItemStartAtLogin.State = NSCellStateValue.Off;
            }
        }

        private NSStatusItem iStatusItem;
        private ConfigurationWindowController iWindow;
        private Helper iHelper;
        private HelperAutoUpdate iHelperAutoUpdate;
        private Server iServer;
    }
}

