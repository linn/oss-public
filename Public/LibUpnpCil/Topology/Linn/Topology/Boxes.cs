using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
#if !PocketPC
using System.Net.Cache;
#endif
using System.Xml;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

namespace Linn.Topology.Boxes
{
    public class Boxes
    {
        public Boxes(EventServerUpnp aEventServer, ISsdpNotifyProvider aSsdpNotify) : this(aEventServer, aSsdpNotify, true) {
        }

        public Boxes(EventServerUpnp aEventServer, ISsdpNotifyProvider aSsdpNotify, bool aDiscoverProxyDevices) {
            iEventServer = aEventServer;
            iListenerNotify = aSsdpNotify;

            //listen to the volkano service
            iDeviceListVolkano = new DeviceListUpnp(ServiceVolkano.ServiceType(), iListenerNotify);
            iDeviceListVolkano.EventDeviceAdded += VolkanoAddedHandler;
            iDeviceListVolkano.EventDeviceRemoved += VolkanoRemovedHandler;

            //listen to the proxy service
            if (aDiscoverProxyDevices) {
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

        public void Start(IPAddress aInterface) {
            iDeviceListVolkano.Start(aInterface);
            if (iDeviceListProxy != null) {
                iDeviceListProxy.Start(aInterface);
            }
        }

        public void Stop() {
            iDeviceListVolkano.Stop();
            if (iDeviceListProxy != null) {
                iDeviceListProxy.Stop();
            }
        }

        public void Rescan() {
            iDeviceListVolkano.Rescan();
            if (iDeviceListProxy != null) {
                iDeviceListProxy.Rescan();
            }
        }

        public EventHandler<EventArgsRoom> EventRoomRemoved {
            get {
                return iTree.EventRoomRemoved;
            }
            set {
                iTree.EventRoomRemoved = value;
            }
        }

        public EventHandler<EventArgsRoom> EventRoomAdded {
            get {
                return iTree.EventRoomAdded;
            }
            set {
                iTree.EventRoomAdded = value;
            }
        }

        public void CheckForUpdates() {
            iTree.CheckForUpdates();
        }

        public bool UpdateCheckInProgress {
            get {
                return iTree.UpdateCheckInProgress;
            }
        }

        public override string ToString() {
            return iTree.ToString();
        }

        private void VolkanoAddedHandler(object obj, DeviceList.EventArgsDevice e) {
            Lock();

            string macAddress = Box.VolkanoMacAddress(e.Device.Udn);

            Item item;
            if (iItemList.TryGetValue(macAddress, out item)) {
                //if item (box) exists in topology turn the item on
                item.SetOn(e.Device);
            }
            else {
                //else create a new item for the box
                iItemList[macAddress] = new Item(iTree, e.Device, iEventServer);
            }

            Unlock();
        }

        private void VolkanoRemovedHandler(object obj, DeviceList.EventArgsDevice e) {
            Lock();

            string macAddress = Box.VolkanoMacAddress(e.Device.Udn);

            //a device can send a removed event without it previously being added to topology
            Item item;
            if (iItemList.TryGetValue(macAddress, out item)) {
                item.SetOff();
            }

            Unlock();
        }

        private void ProxyAddedHandler(object obj, DeviceList.EventArgsDevice e) {
            Lock();

            string macAddress = Box.ProxyMacAddress(e.Device.Udn);

            Item item;
            if (iItemList.TryGetValue(macAddress, out item)) {
                //if item (box) exists in topology turn the item on
                item.SetOn(e.Device);
            }
            else {
                //else create a new item for the box
                iItemList[macAddress] = new Item(iTree, e.Device, iEventServer, true);
            }

            Unlock();
        }

        private void ProxyRemovedHandler(object obj, DeviceList.EventArgsDevice e) {
            Lock();

            string macAddress = Box.ProxyMacAddress(e.Device.Udn);

            //a device can send a removed event without it previously being added to topology
            Item item;
            if (iItemList.TryGetValue(macAddress, out item)) {
                item.SetOff();
            }

            Unlock();
        }

        private void Lock() {
            iMutex.WaitOne();
        }

        private void Unlock() {
            iMutex.ReleaseMutex();
        }

        private Mutex iMutex = new Mutex();
        private SortedList<string, Item> iItemList = new SortedList<string, Item>(); //key = macAddress
        private ISsdpNotifyProvider iListenerNotify;
        private DeviceListUpnp iDeviceListVolkano;
        private DeviceListUpnp iDeviceListProxy = null;
        private Tree iTree = new Tree();
        private EventServerUpnp iEventServer;
    }

    internal class Item
    {
        internal Item(Tree aTree, Device aDevice, EventServerUpnp aEventServer) : this(aTree, aDevice, aEventServer, false) {
        }

        internal Item(Tree aTree, Device aDevice, EventServerUpnp aEventServer, bool aIsProxy) {
            iTree = aTree;
            iEventServer = aEventServer;
            iBasicSetup = new BasicSetup(aDevice, aEventServer);
            iIsProxy = aIsProxy;
            if (aIsProxy) {
                iServiceProxy = new ServiceProxy(aDevice);
            }
            else {
                iServiceVolkano = new ServiceVolkano(aDevice);
            }

            if (Box.IsFallBack(aDevice.Udn) && !aIsProxy) {
                iBox = iTree.AddFallbackBox(aDevice, iProductId, iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                GetVersionInfo();
            }
            else {
                //wait for service product to collect box data before turning on Box
                iServiceProduct = new ServiceProduct(aDevice, iEventServer);
                iServiceProduct.EventInitial += EventHandlerProductInitial;
            }
        }

        internal void SetOn(Device aDevice) {
            if (iIsProxy) {
                iServiceProxy = new ServiceProxy(aDevice);
            }
            else {
                iServiceVolkano = new ServiceVolkano(aDevice);
            }

            if (Box.IsFallBack(aDevice.Udn) && !iIsProxy) {
                //if device in fallback state create a box not containing the service product
                iBox.SetFallback(aDevice);
                GetVersionInfo();
            }
            else {
                //wait for service product to collect box data before turning on Box
                iServiceProduct = new ServiceProduct(aDevice, iEventServer);
                iServiceProduct.EventInitial += EventHandlerProductInitialOn;
            }
        }

        internal void SetOff() {
            if (iServiceProduct != null) {
                iServiceProduct.Kill();
                iServiceProduct = null;
            }

            if (iServiceVolkano != null) {
                iServiceVolkano.Kill();
                iServiceVolkano = null;
            }

            if (iServiceProxy != null) {
                iServiceProxy.Kill();
                iServiceProxy = null;
            }

            if (iBox != null) {
                iBox.SetOff();
            }
        }

        //fires after a box not in the tree has been created
        private void EventHandlerProductInitial(object obj, EventArgs e) {
            if (iIsProxy) {
                //create proxy box
                iBox = iTree.AddProxyBox(iServiceProduct, iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                ProductSubscribe();
                GetProxyVersionInfo();
            }
            else {
                //create box not in fallback mode
                iBox = iTree.AddMainBox(iBasicSetup, iServiceProduct, iProductId, iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
                ProductSubscribe();
                GetVersionInfo();
            }
        }

        //fires after a box has been added to topology that did exist in our tree
        private void EventHandlerProductInitialOn(object obj, EventArgs e) {
            if (iBox.Room != iServiceProduct.ProductRoom) {
                //room name changed so add and remove the box thus updating our room box tree structure
                bool discoveredInFallback = (iBox.Name == "Reprogram");
                AddRemoveBoxFromTree();
                if (discoveredInFallback) {
                    iBox.SetOn(iServiceProduct.Device, iServiceProduct.ModelName, iServiceProduct.ProductName);
                }
            }
            else {
                //update box information. box variables may have been changed outwith this current running topology
                iBox.SetOn(iServiceProduct.Device, iServiceProduct.ModelName, iServiceProduct.ProductName);
            }
            ProductSubscribe();
            if (iIsProxy) {
                GetProxyVersionInfo();
            }
            else {
                GetVersionInfo();
            }
        }

        //this event will not fire for boxes that do not have the volkano service
        private void EventHandlerProductRoom(object obj, EventArgs e) {
            AddRemoveBoxFromTree();
        }

        private void EventHandlerProductChange(object obj, EventArgs e) {
            iBox.SetDetails(iServiceProduct.ModelName, iServiceProduct.ProductName);
        }

        private void SoftwareVersionDetails(object obj, ServiceVolkano.AsyncActionSoftwareVersion.EventArgsResponse e) {
            iActionSoftwareVersion.EventResponse -= SoftwareVersionDetails;
            if (iSoftwareVersion != e.aSoftwareVersion) {
                iSoftwareVersion = e.aSoftwareVersion;
                iBox.SetVersionDetails(iSoftwareVersion, iProductId);
            }
        }

        private void ProductIdDetails(object obj, ServiceVolkano.AsyncActionProductId.EventArgsResponse e) {
            iActionProductId.EventResponse -= ProductIdDetails;
            if (iProductId != e.aProductNumber) {
                iProductId = e.aProductNumber;
                iBox.SetVersionDetails(iSoftwareVersion, iProductId);
            }
        }

        private void DeviceInfoDetails(object obj, ServiceVolkano.AsyncActionDeviceInfo.EventArgsResponse e) {
            iActionDeviceInfo.EventResponse -= DeviceInfoDetails;

            iBoardType.Clear();
            iBoardId.Clear();
            iBoardDescription.Clear();
            iBoardNumber.Clear();

            XmlDocument document = new XmlDocument();
            document.LoadXml(e.aDeviceInfoXml);

            iProductId = document.SelectSingleNode("/DeviceInfo/ProductId").InnerText;
            iSoftwareVersion = document.SelectSingleNode("/DeviceInfo/SoftwareVersion").InnerText;

            iBox.SetVersionDetails(iSoftwareVersion, iProductId);

            foreach (XmlNode n in document.SelectNodes("/DeviceInfo/BoardList/Board")) {
                iBoardType.Add(n["TypePretty"].InnerText);
                iBoardId.Add(n["Id"].InnerText);
                iBoardDescription.Add(n["Description"].InnerText);
                iBoardNumber.Add(n["PcbNumber"].InnerText);
            }

            iBox.SetBoardDetails(iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray());
        }

        private void DeviceInfoError(object obj, EventArgsError e) {
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

        private void MaxBoardDetails(object obj, ServiceVolkano.AsyncActionMaxBoards.EventArgsResponse e) {
            iActionMaxBoards.EventResponse -= MaxBoardDetails;

            iMaxBoards = e.aMaxBoards;
            iBoardType.Clear();
            iBoardId.Clear();
            iBoardDescription.Clear();
            iBoardNumber.Clear();

            iActionBoardType = iServiceVolkano.CreateAsyncActionBoardType();
            iActionBoardType.EventResponse += BoardTypeDetails;

            if (e.aMaxBoards > 0) {
                iActionBoardType.BoardTypeBegin(0);
            }
        }

        private void BoardTypeDetails(object obj, ServiceVolkano.AsyncActionBoardType.EventArgsResponse e) {
            uint number = Convert.ToUInt32(e.aBoardNumber.Substring(0, 8), 16);
            iBoardNumber.Add(number.ToString());

            if (iBoardNumber.Count >= iMaxBoards) {
                iActionBoardType.EventResponse -= BoardTypeDetails;
            }
            else {
                iActionBoardType.BoardTypeBegin((uint)iBoardNumber.Count);
            }

            iBox.SetBoardDetails(iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray());
        }

        private void ProxySoftwareVersionDetails(object obj, ServiceProxy.AsyncActionSoftwareVersion.EventArgsResponse e) {
            iActionProxySoftwareVersion.EventResponse -= ProxySoftwareVersionDetails;

            if (iSoftwareVersion != e.aSoftwareVersion) {
                iSoftwareVersion = e.aSoftwareVersion;

                iBoardDescription.Clear();

                if (e.aSoftwareVersion.Length > 0) {
                    iBoardDescription.Add(Box.kRs232Connected);
                }
                else {
                    iBoardDescription.Add(Box.kRs232Disconnected);
                }

                iBox.SetProxyVersionDetails(iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
            }
        }

        private void ProxyHardwareVersionDetails(object obj, ServiceProxy.AsyncActionHardwareVersion.EventArgsResponse e) {
            iActionProxyHardwareVersion.EventResponse -= ProxyHardwareVersionDetails;
            if (iBoardType.Count <= 0 || iBoardType[0] != e.aHardwareVersion) {
                iBoardType.Clear();
                iBoardNumber.Clear();
                iBoardType.Add(e.aHardwareVersion);
                if (e.aHardwareVersion.Contains("PCAS") && e.aHardwareVersion.Length >= 7) {
                    iBoardNumber.Add(e.aHardwareVersion.Substring(4, 3));
                }
                iBox.SetProxyVersionDetails(iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
            }
        }

        private void AddRemoveBoxFromTree() {
            iTree.RemoveBox(iBox);
            if (iIsProxy) {
                iBox = iTree.AddProxyBox(iServiceProduct, iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
            }
            else {
                iBox = iTree.AddMainBox(iBasicSetup, iServiceProduct, iProductId, iBoardId.ToArray(), iBoardType.ToArray(), iBoardDescription.ToArray(), iBoardNumber.ToArray(), iSoftwareVersion);
            }
        }

        private void ProductSubscribe() {
            iServiceProduct.EventStateProductName += EventHandlerProductChange;
            iServiceProduct.EventStateModelName += EventHandlerProductChange;
            iServiceProduct.EventStateProductRoom += EventHandlerProductRoom;
        }

        private void GetVersionInfo() {
            iActionDeviceInfo = iServiceVolkano.CreateAsyncActionDeviceInfo();
            iActionDeviceInfo.EventResponse += DeviceInfoDetails;
            iActionDeviceInfo.EventError += DeviceInfoError;
            iActionDeviceInfo.DeviceInfoBegin();
        }

        private void GetProxyVersionInfo() {
            iActionProxySoftwareVersion = iServiceProxy.CreateAsyncActionSoftwareVersion();
            iActionProxySoftwareVersion.EventResponse += ProxySoftwareVersionDetails;
            iActionProxySoftwareVersion.SoftwareVersionBegin();

            iActionProxyHardwareVersion = iServiceProxy.CreateAsyncActionHardwareVersion();
            iActionProxyHardwareVersion.EventResponse += ProxyHardwareVersionDetails;
            iActionProxyHardwareVersion.HardwareVersionBegin();
        }

        private Box iBox;
        private Tree iTree;
        private EventServerUpnp iEventServer;
        private BasicSetup iBasicSetup;
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

        private List<string> iBoardId = new List<string>();
        private List<string> iBoardType = new List<string>();
        private List<string> iBoardDescription = new List<string>();
        private List<string> iBoardNumber = new List<string>();
    }

    public class BasicSetup
    {
        internal BasicSetup(Device aDevice, EventServerUpnp aEventServer) {
            iServiceConfiguration = new ServiceConfiguration(aDevice, aEventServer);
            iServiceConfiguration.EventInitial += EventHandlerConfigurationInitial;
            iActionSetParameter = iServiceConfiguration.CreateAsyncActionSetParameter();
        }

        public void SetRoom(string aRoom) {
            iActionSetParameter.EventResponse += SetParameterDetails;
            iActionSetParameter.EventError += SetParameterError;
            iActionSetParameter.SetParameterBegin(Parameter.kTargetDevice, Parameter.kNameRoom, aRoom);
        }

        public void SetName(string aName) {
            iActionSetParameter.EventResponse += SetParameterDetails;
            iActionSetParameter.EventError += SetParameterError;
            iActionSetParameter.SetParameterBegin(Parameter.kTargetDevice, Parameter.kNameName, aName);
        }

        private void SetParameterDetails(object obj, ServiceConfiguration.AsyncActionSetParameter.EventArgsResponse e) {
            iActionSetParameter.EventResponse -= SetParameterDetails;
            UserLog.WriteLine("Event SetParameter Success");
        }

        private void SetParameterError(object obj, EventArgsError e) {
            iActionSetParameter.EventError -= SetParameterError;
            UserLog.WriteLine("Event SetParameter Error: " + e.Code + ", " + e.Description);
        }

        private void EventHandlerConfigurationInitial(object obj, EventArgs e) {
            iServiceConfiguration.EventStateParameterXml += EventHandlerParameterXmlChange;
        }

        private void EventHandlerParameterXmlChange(object obj, EventArgs e) {
            UserLog.WriteLine("Event ParameterXml Changed");
        }

        private ServiceConfiguration iServiceConfiguration = null;
        private ServiceConfiguration.AsyncActionSetParameter iActionSetParameter;
    }

    public class UpdateCheck
    {
        public EventHandler<EventArgs> EventUpdateCheckComplete;
        public EventHandler<EventArgsUpdateCheckError> EventUpdateCheckError;

        public UpdateCheck() {
        }

        public void Start() {
            if (!InProgress) {
                UserLog.WriteLine("Update Check Started");
                Lock();
                iUpdateCheckComplete = false;
                iUpdateThread = new Thread(UpdateStart);
                Unlock();
                iUpdateThread.IsBackground = true;
                iUpdateThread.Priority = ThreadPriority.BelowNormal;
                iUpdateThread.Start();
            }
        }

        public bool InProgress {
            get {
                Lock();
                bool inProgress = (iUpdateThread != null);
                Unlock();
                return inProgress;
            }
        }

        public class EventArgsUpdateCheckError : EventArgs
        {
            public EventArgsUpdateCheckError(string aErrorMessage) {
                iErrorMessage = aErrorMessage;
            }

            public string ErrorMessage {
                get { return iErrorMessage; }
            }

            private string iErrorMessage;
        }

        public class UpdateInfo
        {
            public UpdateInfo(string aModel, string aVersion, string aUrl) {
                iModel = aModel;
                iVersion = aVersion;
                iUrl = aUrl;
                UserLog.WriteLine(aModel + ": " + aVersion + " (" + Family + ") - " + aUrl);
            }

            public UpdateInfo(string aModel, string aVersion) {
                iModel = aModel;
                iVersion = aVersion;
                iUrl = null;
                UserLog.WriteLine(aModel + " (Proxy): " + aVersion);
            }

            public string Model {
                get { return iModel; }
            }

            public string Version {
                get { return iVersion; }
            }

            public string Family {
                get { return ProductSupport.Family(iVersion); }
            }

            public string Url {
                get { return iUrl; }
            }

            private string iModel;
            private string iVersion;
            private string iUrl;
        }

        public class VariantInfo
        {
            public VariantInfo(string aPcas, string aName) : this(aPcas, null, aName) {
            }

            public VariantInfo(string aPcas, string aBasePcas, string aName) {
                UserLog.WriteLine("Variant: " + aPcas + (aBasePcas == null ? "" : " (Base " + aBasePcas + ")") + "-" + aName);
                iPcas = aPcas;
                iBasePcas = aBasePcas;
                iName = aName;
            }

            public string Pcas {
                get { return iPcas; }
            }

            public string BasePcas {
                get { return iBasePcas; }
            }

            public string Name {
                get { return iName; }
            }

            private string iPcas;
            private string iBasePcas;
            private string iName;
        }

        public void GetInfo(string aModel, bool aIsProxy, string aSoftwareVersion, string[] aBoardNumber, out bool aAvailable, out string aVersion, out string aUrl, out string aVariant) {
            aAvailable = false;
            aVersion = null;
            aUrl = null;
            aVariant = null;

            Lock();
            if (!iUpdateCheckComplete) {
                Unlock();
                return;
            }
            Unlock();

            UpdateCheck.UpdateInfo updateInfo = null;
            if (aIsProxy) {
                iUpdateInfoProxy.TryGetValue(aModel, out updateInfo);
            }
            else {
                iUpdateInfo.TryGetValue(aModel, out updateInfo);
            }

            if (updateInfo != null && aSoftwareVersion != null) {
                if (aIsProxy) {
                    aAvailable = (ProductSupport.CompareProxyVersions(updateInfo.Version, aSoftwareVersion) > 0);
                }
                else {
                    aAvailable = (ProductSupport.CompareVersions(updateInfo.Version, aSoftwareVersion) > 0);
                }
                aVersion = updateInfo.Version;
                aUrl = updateInfo.Url;
            }

            UpdateCheck.VariantInfo variantInfo = null;
            UpdateCheck.VariantInfo variantInfoTemp = null;
            foreach (string board in aBoardNumber) {
                if (iVariantInfo.TryGetValue(board, out variantInfoTemp)) { 
                    // can't just break when we find the first match as this wouldn't work for Renew DS (it would match as a Klimax DS first)
                    variantInfo = iVariantInfo[board];
                }
            }

            if (variantInfo != null) {
                if (variantInfo.BasePcas != null) {
                    foreach (string board in aBoardNumber) {
                        if (board == variantInfo.BasePcas) {
                            aVariant = variantInfo.Name;
                        }
                    }
                }
                else {
                    aVariant = variantInfo.Name;
                }
            }
        }

        private void UpdateStart() {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(kUpdatesAvailableUrl);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;
#if !PocketPC
                try {
                    request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                }
                catch {
                    // not supported for all platforms
                }
#endif
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                XmlDocument document = new XmlDocument();
                document.Load(response.GetResponseStream());

                XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
                nsManager.AddNamespace("linn", kLinnNamespace);

                foreach (XmlNode n in document.SelectNodes("/rss/channel/" + kReleasedDeviceTag, nsManager)) {
                    string model = n["linn:model"].InnerText;
                    string type = n["linn:type"].InnerText;
                    string version = n["linn:version"].InnerText;
                    string family = n["linn:family"].InnerText;
                    string url = n["linn:url"].InnerText;

                    UpdateInfo updateInfo = null;
                    if (type == "Main") {
                        updateInfo = new UpdateInfo(model, version, url);
                        if (iUpdateInfo.ContainsKey(model)) {
                            iUpdateInfo[model] = updateInfo;
                        }
                        else {
                            iUpdateInfo.Add(model, updateInfo);
                        }
                    }
                    else if (type == "Proxy") {
                        updateInfo = new UpdateInfo(model, version);
                        if (iUpdateInfoProxy.ContainsKey(model)) {
                            iUpdateInfoProxy[model] = updateInfo;
                        }
                        else {
                            iUpdateInfoProxy.Add(model, updateInfo);
                        }
                    }
                }

                foreach (XmlNode n in document.SelectNodes("/rss/channel/linn:variantList/linn:variant", nsManager)) {
                    string pcas = n["linn:pcas"].InnerText;
                    string basepcas = null;
                    if (n["linn:basepcas"] != null) {
                        basepcas = n["linn:basepcas"].InnerText;
                    }
                    string name = n["linn:name"].InnerText;
                    VariantInfo variantInfo = new VariantInfo(pcas, basepcas, name);
                    if (iVariantInfo.ContainsKey(pcas)) {
                        iVariantInfo[pcas] = variantInfo;
                    }
                    else {
                        iVariantInfo.Add(pcas, variantInfo);
                    }
                }

                iUpdateCheckComplete = true;
                if (EventUpdateCheckComplete != null) {
                    EventUpdateCheckComplete(this, EventArgs.Empty);
                }
            }
            catch (Exception e) {
                if (EventUpdateCheckError != null) {
                    EventUpdateCheckError(this, new EventArgsUpdateCheckError(e.Message));
                }
            }
            finally {
                Lock();
                iUpdateThread = null;
                Unlock();
            }
        }

        private void Lock() {
            iMutex.WaitOne();
        }

        private void Unlock() {
            iMutex.ReleaseMutex();
        }

        private const string kUpdatesAvailableUrl = "http://products.linn.co.uk/VersionInfo/LatestVersionInfo.xml";
        private const string kLinnNamespace = "http://products.linn.co.uk/VersionInfo/namespace";
        private const string kReleasedDeviceTag = "linn:release";

        private Thread iUpdateThread = null;
        private bool iUpdateCheckComplete = false;
        private SortedList<string, UpdateInfo> iUpdateInfo = new SortedList<string, UpdateInfo>(); // key = model
        private SortedList<string, UpdateInfo> iUpdateInfoProxy = new SortedList<string, UpdateInfo>(); // key = model
        private SortedList<string, VariantInfo> iVariantInfo = new SortedList<string, VariantInfo>(); // key = pcas number
        private Mutex iMutex = new Mutex();
    }

}



