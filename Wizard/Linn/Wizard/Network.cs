using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using OpenHome.Xapp;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;

namespace Linn.Wizard
{
    public class Network : IStack
    {
        private bool iStackStarted = false;
        private List<Box> iBoxList;
        private NetworkStack iNetworkStack;
        private List<NetworkEventHandler> iNetworkEventHandlerList;
        private Helper iHelper;
        private NetworkChangeWatcher iNetworkChangeWatcher;

        public Network(Helper aHelper)
        {
            iHelper = aHelper;
            iBoxList = new List<Box>();
            iNetworkEventHandlerList = new List<NetworkEventHandler>();
            iNetworkStack = new NetworkStack();
            iNetworkChangeWatcher = new NetworkChangeWatcher();
            iNetworkChangeWatcher.EventNetworkChanged += NetworkChangedHandler;
            StartStack();
        }

        public NetworkChangeWatcher GetNetworkChangeWatcher()
        {
            return (iNetworkChangeWatcher);
        }

        private void NetworkChangedHandler(object sender, EventArgs e)
        {

        }



        public List<Box> BoxList()
        {
            List<Box> boxListSnapshot;
            lock (this)
            {
                boxListSnapshot = new List<Box>(iBoxList);
            }
            return (boxListSnapshot);
        }


        public Box Box(string aMacAddress)
        {
            lock (this)
            {
                foreach (Box box in iBoxList)
                {
                    if (box.MacAddress == aMacAddress)
                    {
                        return(box);
                    }
                }
            }
            Assert.Check(false);
            return(null);
        }

        public List<NetworkInterface> NetworkInterfaceList()
        {
            return(iNetworkStack.NetworkInterfaceList());
        }

        public void SetNetworkInterface(string aMacAddress)
        {
            foreach (Box b in iBoxList)
            {
                if (b.MacAddress == aMacAddress)
                {
                    SetNetworkInterface(b.NetworkInterfaceIpAddress());
                    return;
                }
            }
            Assert.Check(false);
        }


        public void SetNetworkInterface(System.Net.IPAddress aIpaddress)
        {
            foreach (NetworkInterface n in NetworkInterfaceList())
            {
                if (n.Info.IPAddress == aIpaddress)
                {
                    iHelper.Interface.Set(n.Name);
                    return;
                }
            }
            Assert.Check(false);
        }

        public void Refresh()
        {
            foreach (NetworkEventHandler eventHandler in iNetworkEventHandlerList)
            {
                eventHandler.Boxes().Rescan();
            }
        }

        private void StartStack()
        {
            if (!iStackStarted)
            {
                // start the network stack
                iNetworkStack.SetStack(this);
                iNetworkStack.Start();
                iStackStarted = true;
            }
        }

        public void Close()
        {
            if (iStackStarted)
            {
                iNetworkStack.Stop();
                iStackStarted = false;
            }
        }


        void IStack.Stop()
        {
            // stop all the handlers in our list
            foreach (NetworkEventHandler eventHandler in iNetworkEventHandlerList)
            {
                eventHandler.Stop();
            }
        }


        void IStack.Start(System.Net.IPAddress aIpAddress)
        {
            NetworkEventHandler eventHandler = new NetworkEventHandler(iHelper, aIpAddress);
            iNetworkEventHandlerList.Add(eventHandler);
            Boxes boxes = eventHandler.Boxes();
            boxes.EventRoomAdded += RoomAddedHandler;
            boxes.EventRoomRemoved += RoomRemovedHandler;
            eventHandler.Start();
        }

        public void RoomAddedHandler(object obj, EventArgsRoom e)
        {
            // Console.WriteLine("RoomAddedHandler {0}]", e.RoomArg.Name);
            lock (this)
            {
                e.RoomArg.EventBoxAdded += BoxAddedHandler;
                e.RoomArg.EventBoxRemoved += BoxRemovedHandler;
            }
        }

        public void RoomRemovedHandler(object obj, EventArgsRoom e)
        {
            lock (this)
            {
                e.RoomArg.EventBoxAdded -= BoxAddedHandler;
                e.RoomArg.EventBoxRemoved -= BoxRemovedHandler;
            }
        }


        public void BoxAddedHandler(object obj, EventArgsBox e)
        {
            lock (this)
            {
                e.BoxArg.EventBoxChanged += BoxChangedHandler;

                Box box = e.BoxArg;

                foreach (Box existingBox in iBoxList)
                {

                    if (existingBox.MacAddress == box.MacAddress)
                    {
                        // don't add this box if it's already in the list (discovered on a different net adaptor)
                        return;
                    }
                }
                iBoxList.Add(box);
            }
        }

        // when room name changes topology removes the box and adds the new box. 
        // this handler removes the event handler for the box in the old room. 
        // the new box box changed handler will be added in AddBox.
        public void BoxRemovedHandler(object obj, EventArgsBox e)
        {
            lock (this)
            {
                e.BoxArg.EventBoxChanged -= BoxChangedHandler;
                Box box = e.BoxArg;
                iBoxList.Remove(box);
            }
        }

        public void BoxChangedHandler(object obj, EventArgsBox e)
        {
            lock (this)
            {

                Console.WriteLine("BoxChangedHandler {0} ip [{1}] state [{2}] iBoxList count {3}", e.BoxArg.Name, e.BoxArg.IpAddress, e.BoxArg.State, iBoxList.Count);

                Box box = e.BoxArg;

                bool found = false;
                foreach (Box b in iBoxList)
                {
                    if (b.MacAddress == box.MacAddress)
                    {
                        iBoxList[iBoxList.IndexOf(b)] = box;    // modify existing entry
                        found = true;
                        break;
                    }
                }
                if (found == false)
                {
                    iBoxList.Add(box);                          // add new entry
                }
            }
        }
    
        /***************************************************************************/

        private class NetworkEventHandler
        {
            // Provides an event handler for a single network adaptor(interface)

            public NetworkEventHandler(Helper iHelper, System.Net.IPAddress aNetworkInterfaceIpAddress)
            {
                iNetworkInterfaceIpAddress = aNetworkInterfaceIpAddress;
                iEventServerUpnp = new EventServerUpnp();
                iSsdpListenerMulticast = new SsdpListenerMulticast();
                iBoxes = new Boxes(iHelper, iEventServerUpnp, iSsdpListenerMulticast);

            }

            public void Stop()
            {
                Assert.Check(iStarted==true);
                iEventServerUpnp.Stop();
                iSsdpListenerMulticast.Stop();
                iBoxes.Stop();
                iStarted = false;
            }

            public void Start()
            {
                Assert.Check(iStarted==false);
                iEventServerUpnp.Start(iNetworkInterfaceIpAddress);
                iSsdpListenerMulticast.Start(iNetworkInterfaceIpAddress);
                iBoxes.Start(iNetworkInterfaceIpAddress);
                iStarted = true;
            }

            public Boxes Boxes()
            {
                return(iBoxes);
            }

            System.Net.IPAddress iNetworkInterfaceIpAddress;
            Boxes iBoxes;
            EventServerUpnp iEventServerUpnp;
            SsdpListenerMulticast iSsdpListenerMulticast;
            bool iStarted = false;
        }
    
    }

    /***************************************************************************/

    public class NetworkStack
    {
        List<NetworkInterface> iNetworkInterfaceList;
        private IStack iStack;
        private StackStatus iStatus;
        private bool iStackStarted;
        private StackStatusHandler iStackStatusHandler;

        public NetworkStack()
        {
            iNetworkInterfaceList = GetNetworkInterfaces();
            iStatus = new StackStatus(EStackState.eStopped, null);
            iStackStarted = false;
        }

        private List<NetworkInterface> GetNetworkInterfaces()
        {
            List<NetworkInterface> interfaces = new List<NetworkInterface>();

            foreach (NetworkInfoModel i in NetworkInfo.GetAllNetworkInterfaces())
            {
                if (i.OperationalStatus == EOperationalStatus.eUp ||
                    i.OperationalStatus == EOperationalStatus.eUnknown)
                {
                    // don't add 3G network card on iPads/iPhones
                    if (i.Name != "pdp_ip0")
                    {
                        interfaces.Add(new NetworkInterface(i));
                    }
                }
            }

            return (interfaces);
        }

        public List<NetworkInterface> NetworkInterfaceList()
        {
            return (iNetworkInterfaceList);
        }

        public void SetStack(IStack aStack)
        {
            Assert.Check(iStack == null);
            Assert.Check(iStatus.State == EStackState.eStopped);
            iStack = aStack;
        }

        public void SetStatusHandler(StackStatusHandler aHandler)
        {
            Assert.Check(iStackStatusHandler == null);
            iStackStatusHandler = aHandler;
        }

        public void Start()
        {
            lock (this)
            {
                StartStack();

            }
        }

        public void Stop()
        {
            lock (this)
            {
                StopStack();
            }
        }

        private void StartStack()
        {
            iStackStarted = false;

            foreach (NetworkInterface netInterface in iNetworkInterfaceList)
            {
                switch (netInterface.Status)
                {
                    case NetworkInterface.EStatus.eUnconfigured:
                        iStatus = new StackStatus(EStackState.eNoInterface, netInterface);

                        Trace.WriteLine(Trace.kCore, "Stack.StartStack() no configured interface");
                        UserLog.WriteLine(DateTime.Now + ": Linn.Stack start failed - no interface is configured");
                        break;

                    case NetworkInterface.EStatus.eUnavailable:
                        iStatus = new StackStatus(EStackState.eNonexistentInterface, netInterface);

                        Trace.WriteLine(Trace.kCore, "Stack.StartStack() configured interface error");
                        UserLog.WriteLine(DateTime.Now + ": Linn.Stack start failed - configured interface is unavailable " + netInterface.ToString());
                        break;

                    case NetworkInterface.EStatus.eAvailable:
                        try
                        {
                            Trace.WriteLine(Trace.kCore, "Stack.StartStack() starting...");
                            UserLog.WriteLine(DateTime.Now + ": Linn.Stack starting... " + netInterface.ToString());

                            if (iStack != null)
                            {
                                iStack.Start(netInterface.Info.IPAddress);
                            }
                            iStatus = new StackStatus(EStackState.eOk, netInterface);
                            iStackStarted = true;

                            Trace.WriteLine(Trace.kCore, "Stack.StartStack() OK");
                            UserLog.WriteLine(DateTime.Now + ": Linn.Stack start ok " + netInterface.ToString());
                        }
                        catch (Exception e)
                        {
                            iStatus = new StackStatus(EStackState.eBadInterface, netInterface);
                            iStackStarted = false;

                            Trace.WriteLine(Trace.kCore, "Stack.StartStack() failure: " + e.ToString());
                            Console.Write("Stack.StartStack() failure: " + e.ToString());
                            UserLog.WriteLine(DateTime.Now + ": Linn.Stack start failed " + netInterface.ToString());
                            UserLog.WriteLine("Error Message: " + e.Message);
                            UserLog.WriteLine("Error Message: " + e.ToString());

                            // stop the stack to cleanup any stack components that were started
                            if (iStack != null)
                            {
                                iStack.Stop();
                            }
                        }
                        break;

                    default:
                        Assert.Check(false);
                        break;
                }
            }
        }

        private void StopStack()
        {
            if (iStatus.State != EStackState.eStopped)
            {
                Trace.WriteLine(Trace.kCore, "Stack.StopStack() stopping stack...");
                UserLog.WriteLine(DateTime.Now + ": Linn.Stack stopping...");

                if (iStackStarted && iStack != null)
                {
                    iStack.Stop();
                }

                iStatus = new StackStatus(EStackState.eStopped, null);
                iStackStarted = false;

                Trace.WriteLine(Trace.kCore, "Stack.StopStack() stack stopped");
                UserLog.WriteLine(DateTime.Now + ": Linn.Stack stop ok");
            }
        }



    }


}
