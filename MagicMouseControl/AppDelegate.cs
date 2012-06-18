using System;

using MonoMac;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace MagicMouseControl
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        public override void DidFinishLaunching(NSNotification aNotification)
        {
            iMainWindow = new MainWindow();
            NSBundle.LoadNib("MainWindow", iMainWindow);
        }

        public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication aSender)
        {
            //return base.ApplicationShouldTerminate (aSender);
            iMainWindow.Close();
            return NSApplicationTerminateReply.Now;
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication aSender)
        {
            return true;
        }

        private MainWindow iMainWindow;
    }
}
