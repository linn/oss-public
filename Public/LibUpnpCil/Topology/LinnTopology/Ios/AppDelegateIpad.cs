
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Topology;

namespace LinnTopology
{

    // The name AppDelegateIPad is referenced in the MainWindowIPad.xib file.
    public partial class AppDelegateIpad : UIApplicationDelegate, IStack
    {
        private Helper iHelper;
        private EventServerUpnp iEventServer;
        private SsdpListenerMulticast iListenerNotify;
        private House iHouse;

        // This method is invoked when the application has loaded its UI and its ready to run
        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            iHelper = new Helper(new string[] {});
            iHelper.ProcessOptionsFileAndCommandLine();
            iHelper.Stack.SetStack(this);
            iHelper.Stack.EventStatusChanged += StackStatusChanged;

            //Trace.Level = Trace.kUpnp | Trace.kTopology;

            iEventServer = new EventServerUpnp();
            iListenerNotify = new SsdpListenerMulticast();
            iHouse = new House(iListenerNotify, iEventServer, new ModelFactory());

            splitViewController.Delegate = new SplitViewControllerDelegate();

            UINavigationController navigation = splitViewController.ViewControllers[0] as UINavigationController;
            if(navigation != null)
            {
                RoomTableViewController room = navigation.TopViewController as RoomTableViewController;
                if(room != null)
                {
                    room.SetHouse(iHouse);

                    navigation = splitViewController.ViewControllers[1] as UINavigationController;
                    if(navigation != null)
                    {
                        SourceTableViewController source = navigation.TopViewController as SourceTableViewController;
                        if(source != null)
                        {
                            room.SetSourceTableViewController(source);
                        }
                    }
                }
            }

            window.AddSubview(splitViewController.View);

            window.MakeKeyAndVisible();

            iHelper.Stack.Start();
            
            return true;
        }

        public override void WillTerminate(UIApplication application)
        {
            iHelper.Stack.Stop();
            iHelper.Dispose();
        }


        public void Start(IPAddress aIpAddress)
        {
            iEventServer.Start(aIpAddress);
            iListenerNotify.Start(aIpAddress);
            iHouse.Start(aIpAddress);

            iHouse.Rescan();
        }

        public void Stop()
        {
            iHouse.Stop();
            iListenerNotify.Stop();
            iEventServer.Stop();
        }

        private void StackStatusChanged(object sender, EventArgsStackStatus e)
        {
        }
    }
}
