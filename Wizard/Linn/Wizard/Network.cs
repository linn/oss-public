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
    public class Network
    {
        private Helper iHelper;
        private List<Box> iBoxList;
        private NetworkChangeWatcher iNetworkChangeWatcher;
        private List<NetworkInterface> iInterfaces;
        private List<NetworkStack> iNetworkStacks;

        public Network(Helper aHelper)
        {
            iHelper = aHelper;
            iBoxList = new List<Box>();
            iNetworkChangeWatcher = new NetworkChangeWatcher();

            // create the list of network interfaces
            iInterfaces = new List<NetworkInterface>();

            foreach (NetworkInfoModel i in NetworkInfo.GetAllNetworkInterfaces())
            {
                if (i.OperationalStatus == EOperationalStatus.eUp ||
                    i.OperationalStatus == EOperationalStatus.eUnknown)
                {
                    // don't add 3G network card on iPads/iPhones
                    if (i.Name != "pdp_ip0")
                    {
                        iInterfaces.Add(new NetworkInterface(i));
                    }
                }
            }

            // create separate network stacks for each available interface
            iNetworkStacks = new List<NetworkStack>();

            foreach (NetworkInterface iface in iInterfaces)
            {
                if (iface.Status == NetworkInterface.EStatus.eAvailable)
                {
                    NetworkStack stack = new NetworkStack(aHelper, iface.Info.IPAddress);
                    stack.Boxes.EventRoomAdded += RoomAddedHandler;
                    stack.Boxes.EventRoomRemoved += RoomRemovedHandler;

                    try
                    {
                        // start the stack
                        UserLog.WriteLine(String.Format("{0}: Linn.Wizard.Network starting stack({1})", DateTime.Now, iface.Info.IPAddress.ToString()));
                        stack.Start();
                        UserLog.WriteLine(String.Format("{0}: Linn.Wizard.Network starting stack({1}) ok", DateTime.Now, iface.Info.IPAddress.ToString()));

                        // success - add it to the list
                        iNetworkStacks.Add(stack);
                    }
                    catch (Exception e)
                    {
                        // failed to start - unhook handlers and ignore
                        UserLog.WriteLine(String.Format("{0}: Linn.Wizard.Network starting stack({1}) failed", DateTime.Now, iface.Info.IPAddress.ToString()));
                        UserLog.WriteLine("Error Message: " + e.Message);
                        UserLog.WriteLine("Error Message: " + e.ToString());

                        stack.Boxes.EventRoomAdded -= RoomAddedHandler;
                        stack.Boxes.EventRoomRemoved -= RoomRemovedHandler;
                    }
                }
            }
        }

        public void Close()
        {
            // stop all created stacks
            foreach (NetworkStack stack in iNetworkStacks)
            {
                stack.Stop();
                stack.Boxes.EventRoomAdded -= RoomAddedHandler;
                stack.Boxes.EventRoomRemoved -= RoomRemovedHandler;
            }
        }

        public NetworkChangeWatcher GetNetworkChangeWatcher()
        {
            return (iNetworkChangeWatcher);
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

        public NetworkInterface[] NetworkInterfaceList()
        {
            return iInterfaces.ToArray();
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
            foreach (NetworkStack stack in iNetworkStacks)
            {
                stack.Boxes.Rescan();
            }
        }

        private void RoomAddedHandler(object obj, EventArgsRoom e)
        {
            lock (this)
            {
                e.RoomArg.EventBoxAdded += BoxAddedHandler;
                e.RoomArg.EventBoxRemoved += BoxRemovedHandler;
            }
        }

        private void RoomRemovedHandler(object obj, EventArgsRoom e)
        {
            lock (this)
            {
                e.RoomArg.EventBoxAdded -= BoxAddedHandler;
                e.RoomArg.EventBoxRemoved -= BoxRemovedHandler;
            }
        }

        private void BoxAddedHandler(object obj, EventArgsBox e)
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
        private void BoxRemovedHandler(object obj, EventArgsBox e)
        {
            lock (this)
            {
                e.BoxArg.EventBoxChanged -= BoxChangedHandler;
                Box box = e.BoxArg;
                iBoxList.Remove(box);
            }
        }

        private void BoxChangedHandler(object obj, EventArgsBox e)
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
    
        // private class for a stack on a particular IP address
        private class NetworkStack
        {
            public NetworkStack(Helper iHelper, System.Net.IPAddress aIpAddress)
            {
                iIpAddress = aIpAddress;
                iEventServerUpnp = new EventServerUpnp();
                iSsdpListenerMulticast = new SsdpListenerMulticast();
                iBoxes = new Boxes(iHelper, iEventServerUpnp, iSsdpListenerMulticast);
            }

            public void Start()
            {
                Assert.Check(!iStarted);

                bool eventStarted = false;
                bool ssdpStarted = false;
                try
                {
                    iEventServerUpnp.Start(iIpAddress);
                    eventStarted = true;

                    iSsdpListenerMulticast.Start(iIpAddress);
                    ssdpStarted = true;

                    iBoxes.Start(iIpAddress);
                }
                catch (Exception)
                {
                    if (eventStarted)
                    {
                        iEventServerUpnp.Stop();
                    }
                    if (ssdpStarted)
                    {
                        iSsdpListenerMulticast.Stop();
                    }
                    throw;
                }

                iStarted = true;
            }

            public void Stop()
            {
                Assert.Check(iStarted);

                iEventServerUpnp.Stop();
                iSsdpListenerMulticast.Stop();
                iBoxes.Stop();

                iStarted = false;
            }

            public Boxes Boxes
            {
                get { return iBoxes; }
            }

            System.Net.IPAddress iIpAddress;
            Boxes iBoxes;
            EventServerUpnp iEventServerUpnp;
            SsdpListenerMulticast iSsdpListenerMulticast;
            bool iStarted = false;
        }
    }
}
