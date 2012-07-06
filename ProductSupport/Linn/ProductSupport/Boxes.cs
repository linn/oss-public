using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Xml;
using System.IO;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

namespace Linn.ProductSupport
{
    public class Boxes
    {
        public Boxes(Helper aHelper, EventServerUpnp aEventServer, ISsdpNotifyProvider aSsdpNotify)
            : this(aHelper, aEventServer, aSsdpNotify, true)
        {
        }

        public Boxes(Helper aHelper, EventServerUpnp aEventServer, ISsdpNotifyProvider aSsdpNotify, bool aDiscoverProxyDevices)
        {
            iLock = new object();
            iHelper = aHelper;
            iEventServer = aEventServer;
            iListenerNotify = aSsdpNotify;
            iTree = new Tree(aHelper);

            //listen to the volkano service
            iDeviceListVolkano = new DeviceListUpnp(ServiceVolkano.ServiceType(), iListenerNotify);
            iDeviceListVolkano.EventDeviceAdded += VolkanoAddedHandler;
            iDeviceListVolkano.EventDeviceRemoved += VolkanoRemovedHandler;

            //listen to the proxy service
            if (aDiscoverProxyDevices)
            {
                iDeviceListProxy = new DeviceListUpnp(ServiceProxy.ServiceType(), iListenerNotify);
                iDeviceListProxy.EventDeviceAdded += ProxyAddedHandler;
                iDeviceListProxy.EventDeviceRemoved += ProxyRemovedHandler;
            }

            // Configure Service Point Manager
            ServicePointManager.DefaultConnectionLimit = 50;
            ServicePointManager.Expect100Continue = false;
#if !PocketPC
            ServicePointManager.UseNagleAlgorithm = false;
#endif
        }

        public void Start(IPAddress aInterface)
        {
            iDeviceListVolkano.Start(aInterface);
            if (iDeviceListProxy != null)
            {
                iDeviceListProxy.Start(aInterface);
            }
        }

        public void Stop()
        {
            iDeviceListVolkano.Stop();
            if (iDeviceListProxy != null)
            {
                iDeviceListProxy.Stop();
            }
            lock (iLock)
            {
                foreach (Item i in iItemList.Values)
                {
                    i.Close();
                }
                iItemList.Clear();
                foreach (Device device in iPendingDeviceListVolkano)
                {
                    device.EventOpened -= HandlerEventOpenedVolkano;
                    device.EventOpenFailed -= HandlerEventOpenFailedVolkano;
                }
                iPendingDeviceListVolkano.Clear();
                foreach (Device device in iPendingDeviceListProxy)
                {
                    device.EventOpened -= HandlerEventOpenedProxy;
                    device.EventOpenFailed -= HandlerEventOpenFailedProxy;
                }
                iPendingDeviceListProxy.Clear();
            }
        }

        public void Rescan()
        {
            iDeviceListVolkano.Rescan();
            if (iDeviceListProxy != null)
            {
                iDeviceListProxy.Rescan();
            }
        }

        public EventHandler<EventArgsRoom> EventRoomRemoved
        {
            get
            {
                return iTree.EventRoomRemoved;
            }
            set
            {
                iTree.EventRoomRemoved = value;
            }
        }

        public EventHandler<EventArgsRoom> EventRoomAdded
        {
            get
            {
                return iTree.EventRoomAdded;
            }
            set
            {
                iTree.EventRoomAdded = value;
            }
        }

        public void CheckForUpdates()
        {
            iTree.CheckForUpdates();
        }

        public bool UpdateCheckInProgress
        {
            get
            {
                return iTree.UpdateCheckInProgress;
            }
        }

        public bool UpdateCheckFailed
        {
            get
            {
                return iTree.UpdateCheckFailed;
            }
        }

        public override string ToString()
        {
            return iTree.ToString();
        }

        private void VolkanoAddedHandler(object obj, DeviceList.EventArgsDevice e)
        {
            lock (iLock)
            {
                iPendingDeviceListVolkano.Add(e.Device);
                e.Device.EventOpened += HandlerEventOpenedVolkano;
                e.Device.EventOpenFailed += HandlerEventOpenFailedVolkano;
                e.Device.Open();
            }
        }

        private void HandlerEventOpenFailedVolkano(object sender, EventArgs e)
        {
            Device device = sender as Device;
            Assert.Check(device != null);
            UserLog.WriteLine("Failed to open device: " + device);
            lock (iLock)
            {
                device.EventOpened -= HandlerEventOpenedVolkano;
                device.EventOpenFailed -= HandlerEventOpenFailedVolkano;
                iPendingDeviceListVolkano.Remove(device);
            }
        }

        private void HandlerEventOpenedVolkano(object sender, EventArgs e)
        {
            Device device = sender as Device;
            Assert.Check(device != null);

            lock (iLock)
            {
                device.EventOpened -= HandlerEventOpenedVolkano;
                device.EventOpenFailed -= HandlerEventOpenFailedVolkano;
                iPendingDeviceListVolkano.Remove(device);
                try
                {
                    string macAddress = Box.VolkanoMacAddress(device.Udn);

                    Item item;
                    if (iItemList.TryGetValue(macAddress, out item))
                    {
                        //if item (box) exists in topology turn the item on
                        item.Open(device);
                    }
                    else
                    {
                        //else create a new item for the box
                        iItemList[macAddress] = new Item(iHelper, iTree, device, iEventServer);
                    }
                }
                catch (Exception ex)
                {
                    UserLog.WriteLine(ex.ToString());
                }
            }
        }

        private void VolkanoRemovedHandler(object obj, DeviceList.EventArgsDevice e)
        {
            lock (iLock)
            {
                if (iPendingDeviceListVolkano.Contains(e.Device))
                {
                    e.Device.EventOpenFailed -= HandlerEventOpenFailedVolkano;
                    e.Device.EventOpened -= HandlerEventOpenedVolkano;
                    iPendingDeviceListVolkano.Remove(e.Device);
                }
                else
                {
                    try
                    {

                        string macAddress = Box.VolkanoMacAddress(e.Device.Udn);

                        //a device can send a removed event without it previously being added to topology
                        Item item;
                        if (iItemList.TryGetValue(macAddress, out item))
                        {
                            item.Kill();
                        }
                    }
                    catch (Exception ex)
                    {
                        UserLog.WriteLine(ex.ToString());
                    }
                }
            }
        }

        private void ProxyAddedHandler(object obj, DeviceList.EventArgsDevice e)
        {
            lock (iLock)
            {
                iPendingDeviceListProxy.Add(e.Device);
                e.Device.EventOpened += HandlerEventOpenedProxy;
                e.Device.EventOpenFailed += HandlerEventOpenFailedProxy;
                e.Device.Open();
            }
        }

        private void HandlerEventOpenFailedProxy(object sender, EventArgs e)
        {
            Device device = sender as Device;
            Assert.Check(device != null);
            UserLog.WriteLine("Failed to open device: " + device);
            lock (iLock)
            {
                device.EventOpened -= HandlerEventOpenedProxy;
                device.EventOpenFailed -= HandlerEventOpenFailedProxy;
                iPendingDeviceListProxy.Remove(device);
            }
        }

        private void HandlerEventOpenedProxy(object sender, EventArgs e)
        {
            Device device = sender as Device;
            Assert.Check(device != null);

            lock (iLock)
            {
                device.EventOpened -= HandlerEventOpenedProxy;
                device.EventOpenFailed -= HandlerEventOpenFailedProxy;
                iPendingDeviceListProxy.Remove(device);
                try
                {
                    string macAddress = Box.ProxyMacAddress(device.Udn);

                    Item item;
                    if (iItemList.TryGetValue(macAddress, out item))
                    {
                        //if item (box) exists in topology turn the item on
                        item.Open(device);
                    }
                    else
                    {
                        //else create a new item for the box
                        iItemList[macAddress] = new Item(iHelper, iTree, device, iEventServer, true);
                    }

                }
                catch (Exception ex)
                {
                    UserLog.WriteLine(ex.ToString());
                }
            }
        }

        private void ProxyRemovedHandler(object obj, DeviceList.EventArgsDevice e)
        {
            lock (iLock)
            {
                if (iPendingDeviceListProxy.Contains(e.Device))
                {
                    e.Device.EventOpenFailed -= HandlerEventOpenFailedProxy;
                    e.Device.EventOpened -= HandlerEventOpenedProxy;
                    iPendingDeviceListProxy.Remove(e.Device);
                }
                else
                {
                    try
                    {
                        string macAddress = Box.ProxyMacAddress(e.Device.Udn);

                        //a device can send a removed event without it previously being added to topology
                        Item item;
                        if (iItemList.TryGetValue(macAddress, out item))
                        {
                            item.Kill();
                        }
                    }
                    catch (Exception ex)
                    {
                        UserLog.WriteLine(ex.ToString());
                    }
                }

            }
        }

        private Mutex iMutex = new Mutex();
        private SortedList<string, Item> iItemList = new SortedList<string, Item>(); //key = macAddress
        private ISsdpNotifyProvider iListenerNotify;
        private DeviceListUpnp iDeviceListVolkano;
        private DeviceListUpnp iDeviceListProxy = null;
        private Tree iTree = null;
        private EventServerUpnp iEventServer;
        private Helper iHelper;
        private object iLock;
        private List<Device> iPendingDeviceListVolkano = new List<Device>();
        private List<Device> iPendingDeviceListProxy = new List<Device>();
    }

    internal class Item
    {
        internal Item(Helper aHelper, Tree aTree, Device aDevice, EventServerUpnp aEventServer)
            : this(aHelper, aTree, aDevice, aEventServer, false)
        {
        }

        internal Item(Helper aHelper, Tree aTree, Device aDevice, EventServerUpnp aEventServer, bool aIsProxy)
        {
            iLock = new object();
            iHelper = aHelper;
            iTree = aTree;
            iEventServer = aEventServer;
            iIsProxy = aIsProxy;
            iBasicSetup = new BasicSetup(aDevice, aEventServer);
            iPlayback = new Playback(aHelper, aDevice, aEventServer);

            if (aIsProxy)
            {
                iServiceProxy = new ServiceProxy(aDevice);
            }
            else
            {
                iServiceVolkano = new ServiceVolkano(aDevice);
            }

            if (Box.IsFallBack(aDevice.Udn) && !aIsProxy)
            {
                iBox = iTree.AddFallbackBox(iBasicSetup, aDevice, aEventServer, iProductId, iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                GetVersionInfo();
            }
            else
            {
                //wait for service product to collect box data before turning on Box
                iServiceProduct = new ServiceProduct(aDevice, iEventServer);
                iServiceProduct.EventInitial += EventHandlerProductInitial;
            }
        }

        internal void Open(Device aDevice)
        {
            lock (iLock)
            {
                // close previous services cleanly
                if (iIsProxy)
                {
                    iServiceProxy = new ServiceProxy(aDevice);
                }
                else
                {
                    iServiceVolkano = new ServiceVolkano(aDevice);
                }

                iBasicSetup = new BasicSetup(aDevice, iEventServer);
                iPlayback = new Playback(iHelper, aDevice, iEventServer);

                if (Box.IsFallBack(aDevice.Udn) && !iIsProxy)
                {
                    //if device in fallback state create a box not containing the service product
                    if (iBox != null)
                    {
                        iBox.SetFallback(aDevice);
                    }
                    else
                    {
                        iBox = iTree.AddFallbackBox(iBasicSetup, aDevice, iEventServer, iProductId, iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                    }
                    GetVersionInfo();
                }
                else
                {
                    if (iServiceProduct != null)
                    {
                        if (!iEventInitialOn)
                        {
                            iServiceProduct.EventInitial -= EventHandlerProductInitial;
                        }
                        else
                        {
                            iServiceProduct.EventInitial -= EventHandlerProductInitialOn;
                        }
                        iServiceProduct.Kill();
                        iServiceProduct = null;
                    }
                    //wait for service product to collect box data before turning on Box
                    iServiceProduct = new ServiceProduct(aDevice, iEventServer);
                    iServiceProduct.EventInitial += EventHandlerProductInitialOn;
                    iEventInitialOn = true;
                }
            }
        }

        internal void Close()
        {
            lock (iLock)
            {
                if (iServiceProduct != null)
                {
                    if (!iEventInitialOn)
                    {
                        iServiceProduct.EventInitial -= EventHandlerProductInitial;
                    }
                    else
                    {
                        iServiceProduct.EventInitial -= EventHandlerProductInitialOn;
                    }
                    iServiceProduct.Close();
                    iServiceProduct = null;
                }

                if (iServiceVolkano != null)
                {
                    iServiceVolkano.Close();
                    iServiceVolkano = null;
                }

                if (iServiceProxy != null)
                {
                    iServiceProxy.Close();
                    iServiceProxy = null;
                }

                if (iBox != null)
                {
                    iBox.SetOff();
                }

                if (iBasicSetup != null)
                {
                    iBasicSetup.Close();
                    iBasicSetup = null;
                }

                if (iPlayback != null)
                {
                    iPlayback.Close();
                    iPlayback = null;
                }
            }
        }
        
        internal void Kill()
        {
            lock (iLock)
            {
                if (iServiceProduct != null)
                {
                    if (!iEventInitialOn)
                    {
                        iServiceProduct.EventInitial -= EventHandlerProductInitial;
                    }
                    else
                    {
                        iServiceProduct.EventInitial -= EventHandlerProductInitialOn;
                    }
                    iServiceProduct.Kill();
                    iServiceProduct = null;
                }

                if (iServiceVolkano != null)
                {
                    iServiceVolkano.Kill();
                    iServiceVolkano = null;
                }

                if (iServiceProxy != null)
                {
                    iServiceProxy.Kill();
                    iServiceProxy = null;
                }

                if (iBox != null)
                {
                    iBox.SetOff();
                }

                if (iBasicSetup != null)
                {
                    iBasicSetup.Kill();
                    iBasicSetup = null;
                }

                if (iPlayback != null)
                {
                    iPlayback.Kill();
                    iPlayback = null;
                }
            }
        }

        //fires after a box not in the tree has been created
        private void EventHandlerProductInitial(object obj, EventArgs e)
        {
            lock (iLock)
            {
                if (obj == iServiceProduct)
                {
                    if (iIsProxy)
                    {
                        //create proxy box
                        iBox = iTree.AddProxyBox(iServiceProduct, iEventServer, iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                        ProductSubscribe();
                        GetProxyVersionInfo();
                    }
                    else
                    {
                        //create box not in fallback mode
                        iBox = iTree.AddMainBox(iBasicSetup, iPlayback, iServiceProduct, iEventServer, iProductId, iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                        ProductSubscribe();
                        GetVersionInfo();
                    }
                }
            }
        }

        //fires after a box has been added to topology that did exist in our tree
        private void EventHandlerProductInitialOn(object obj, EventArgs e)
        {
            lock (iLock)
            {
                if (obj == iServiceProduct)
                {
                    if (iBox == null)
                    {
                        if (iIsProxy)
                        {
                            //create proxy box
                            iBox = iTree.AddProxyBox(iServiceProduct, iEventServer, iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                        }
                        else
                        {
                            iBox = iTree.AddMainBox(iBasicSetup, iPlayback, iServiceProduct, iEventServer, iProductId, iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                        }
                    }
                    if (iBox.Room != iServiceProduct.ProductRoom)
                    {
                        //room name changed so add and remove the box thus updating our room box tree structure
                        bool discoveredInFallback = (iBox.Name == "Reprogram");
                        AddRemoveBoxFromTree();
                        if (discoveredInFallback)
                        {
                            iBox.SetOn(iServiceProduct.Device, iServiceProduct.ModelName, iServiceProduct.ProductName, iBasicSetup, iPlayback);
                        }
                    }
                    else
                    {
                        //update box information. box variables may have been changed outwith this current running topology
                        iBox.SetOn(iServiceProduct.Device, iServiceProduct.ModelName, iServiceProduct.ProductName, iBasicSetup, iPlayback);
                    }
                    ProductSubscribe();
                    if (iIsProxy)
                    {
                        GetProxyVersionInfo();
                    }
                    else
                    {
                        GetVersionInfo();
                    }
                }
            }
        }

        //this event will not fire for boxes that do not have the volkano service
        private void EventHandlerProductRoom(object obj, EventArgs e)
        {
            lock (iLock)
            {
                AddRemoveBoxFromTree();
            }
        }

        private void EventHandlerProductChange(object obj, EventArgs e)
        {
            lock (iLock)
            {
                iBox.SetDetails(iServiceProduct.ModelName, iServiceProduct.ProductName);
            }
        }

        private void SoftwareVersionDetails(object obj, ServiceVolkano.AsyncActionSoftwareVersion.EventArgsResponse e)
        {
            lock (iLock)
            {
                iActionSoftwareVersion.EventResponse -= SoftwareVersionDetails;
                if (iSoftwareVersion != e.aSoftwareVersion)
                {
                    iSoftwareVersion = e.aSoftwareVersion;
                    iBox.SetVersionDetails(iSoftwareVersion, iProductId);
                }
            }
        }

        private void ProductIdDetails(object obj, ServiceVolkano.AsyncActionProductId.EventArgsResponse e)
        {
            lock (iLock)
            {
                iActionProductId.EventResponse -= ProductIdDetails;
                if (iProductId != e.aProductNumber)
                {
                    iProductId = e.aProductNumber;
                    iBox.SetVersionDetails(iSoftwareVersion, iProductId);
                }
            }
        }

        private void DeviceInfoDetails(object obj, ServiceVolkano.AsyncActionDeviceInfo.EventArgsResponse e)
        {
            lock (iLock)
            {
                iActionDeviceInfo.EventResponse -= DeviceInfoDetails;
                iActionDeviceInfo.EventError -= DeviceInfoError;

                iBoardType.Clear();
                iBoardId.Clear();
                iBoardDescription.Clear();
                iBoardNumber.Clear();

                XmlDocument document = new XmlDocument();
                document.LoadXml(e.aDeviceInfoXml);

                iProductId = document.SelectSingleNode("/DeviceInfo/ProductId").InnerText;
                iSoftwareVersion = document.SelectSingleNode("/DeviceInfo/SoftwareVersion").InnerText;

                iBox.SetVersionDetails(iSoftwareVersion, iProductId);

                foreach (XmlNode n in document.SelectNodes("/DeviceInfo/BoardList/Board"))
                {
                    iBoardType.Add(n["TypePretty"].InnerText);
                    iBoardId.Add(n["Id"].InnerText);
                    iBoardDescription.Add(n["Description"].InnerText);
                    iBoardNumber.Add(n["PcbNumber"].InnerText);
                }

                iBox.SetBoardDetails(iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray());
            }
        }

        private void DeviceInfoError(object obj, EventArgsError e)
        {
            lock (iLock)
            {
                iActionDeviceInfo.EventResponse -= DeviceInfoDetails;
                iActionDeviceInfo.EventError -= DeviceInfoError;

                // old device won't have DeviceInfo action, so get product id, sw version, board info, using separate actions instead

                iActionSoftwareVersion = iServiceVolkano.CreateAsyncActionSoftwareVersion();
                iActionSoftwareVersion.EventResponse += SoftwareVersionDetails;
                iActionSoftwareVersion.SoftwareVersionBegin();

                iActionProductId = iServiceVolkano.CreateAsyncActionProductId();
                iActionProductId.EventResponse += ProductIdDetails;
                iActionProductId.ProductIdBegin();

                iActionMaxBoards = iServiceVolkano.CreateAsyncActionMaxBoards();
                iActionMaxBoards.EventResponse += MaxBoardDetails;
                iActionMaxBoards.MaxBoardsBegin();
            }
        }

        private void MaxBoardDetails(object obj, ServiceVolkano.AsyncActionMaxBoards.EventArgsResponse e)
        {
            lock (iLock)
            {
                iActionMaxBoards.EventResponse -= MaxBoardDetails;

                iMaxBoards = e.aMaxBoards;
                iBoardType.Clear();
                iBoardId.Clear();
                iBoardDescription.Clear();
                iBoardNumber.Clear();

                iActionBoardType = iServiceVolkano.CreateAsyncActionBoardType();
                iActionBoardType.EventResponse += BoardTypeDetails;

                if (e.aMaxBoards > 0)
                {
                    iActionBoardType.BoardTypeBegin(0);
                }
            }
        }

        private void BoardTypeDetails(object obj, ServiceVolkano.AsyncActionBoardType.EventArgsResponse e)
        {
            lock (iLock)
            {
                uint number = Convert.ToUInt32(e.aBoardNumber.Substring(0, 8), 16);
                iBoardNumber.Add(number.ToString());

                if (iBoardNumber.Count >= iMaxBoards)
                {
                    iActionBoardType.EventResponse -= BoardTypeDetails;
                }
                else
                {
                    iActionBoardType.BoardTypeBegin((uint)iBoardNumber.Count);
                }

                iBox.SetBoardDetails(iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray());
            }
        }

        private void ProxySoftwareVersionDetails(object obj, ServiceProxy.AsyncActionSoftwareVersion.EventArgsResponse e)
        {
            lock (iLock)
            {
                iActionProxySoftwareVersion.EventResponse -= ProxySoftwareVersionDetails;

                if (iSoftwareVersion != e.aSoftwareVersion)
                {
                    iSoftwareVersion = e.aSoftwareVersion;

                    iBoardDescription.Clear();

                    if (e.aSoftwareVersion.Length > 0)
                    {
                        iBoardDescription.Add(Box.kRs232Connected);
                    }
                    else
                    {
                        iBoardDescription.Add(Box.kRs232Disconnected);
                    }

                    iBox.SetProxyVersionDetails(iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                }
            }
        }

        private void ProxyHardwareVersionDetails(object obj, ServiceProxy.AsyncActionHardwareVersion.EventArgsResponse e)
        {
            lock (iLock)
            {
                iActionProxyHardwareVersion.EventResponse -= ProxyHardwareVersionDetails;
                if (iBoardType.Count <= 0 || iBoardType[0] != e.aHardwareVersion)
                {
                    iBoardType.Clear();
                    iBoardNumber.Clear();
                    iBoardType.Add(e.aHardwareVersion);
                    if (e.aHardwareVersion.Contains("PCAS") && e.aHardwareVersion.Length >= 7)
                    {
                        iBoardNumber.Add(e.aHardwareVersion.Substring(4, 3));
                    }
                    iBox.SetProxyVersionDetails(iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                }
            }
        }

        private void AddRemoveBoxFromTree()
        {
            iTree.RemoveBox(iBox);
            if (iIsProxy)
            {
                iBox = iTree.AddProxyBox(iServiceProduct, iEventServer, iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
            }
            else
            {
                iBox = iTree.AddMainBox(iBasicSetup, iPlayback, iServiceProduct, iEventServer, iProductId, iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
            }
        }

        private void ProductSubscribe()
        {
            iServiceProduct.EventStateProductName += EventHandlerProductChange;
            iServiceProduct.EventStateModelName += EventHandlerProductChange;
            iServiceProduct.EventStateProductRoom += EventHandlerProductRoom;
        }

        private void GetVersionInfo()
        {
            iActionDeviceInfo = iServiceVolkano.CreateAsyncActionDeviceInfo();
            iActionDeviceInfo.EventResponse += DeviceInfoDetails;
            iActionDeviceInfo.EventError += DeviceInfoError;
            iActionDeviceInfo.DeviceInfoBegin();
        }

        private void GetProxyVersionInfo()
        {
            iActionProxySoftwareVersion = iServiceProxy.CreateAsyncActionSoftwareVersion();
            iActionProxySoftwareVersion.EventResponse += ProxySoftwareVersionDetails;
            iActionProxySoftwareVersion.SoftwareVersionBegin();

            iActionProxyHardwareVersion = iServiceProxy.CreateAsyncActionHardwareVersion();
            iActionProxyHardwareVersion.EventResponse += ProxyHardwareVersionDetails;
            iActionProxyHardwareVersion.HardwareVersionBegin();
        }

        private Box iBox;
        private Tree iTree;
        private Helper iHelper;
        private EventServerUpnp iEventServer;
        private ServiceProduct iServiceProduct = null;
        private ServiceVolkano iServiceVolkano = null;
        private ServiceProxy iServiceProxy = null;
        private ServiceVolkano.AsyncActionDeviceInfo iActionDeviceInfo;
        private ServiceProxy.AsyncActionSoftwareVersion iActionProxySoftwareVersion;
        private ServiceProxy.AsyncActionHardwareVersion iActionProxyHardwareVersion;
        private ServiceVolkano.AsyncActionSoftwareVersion iActionSoftwareVersion;
        private ServiceVolkano.AsyncActionProductId iActionProductId;
        private ServiceVolkano.AsyncActionMaxBoards iActionMaxBoards;
        private ServiceVolkano.AsyncActionBoardType iActionBoardType;
        private string iSoftwareVersion = null;
        private string iProductId = null;
        private bool iIsProxy = false;
        private uint iMaxBoards = 0;

        private BasicSetup iBasicSetup;
        private Playback iPlayback;

        private List<string> iBoardId = new List<string>();
        private List<string> iBoardType = new List<string>();
        private List<string> iBoardDescription = new List<string>();
        private List<string> iBoardNumber = new List<string>();

        private object iLock;
        private bool iEventInitialOn;
    }
}
