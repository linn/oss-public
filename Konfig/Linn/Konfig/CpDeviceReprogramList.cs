using System;
using System.Xml;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Xapp;

namespace Linn.Konfig
{
    public interface ICpDeviceReprogrammable
    {
        CpDevice Device { get; }
        string Udn { get; }
        string MacAddress { get; }
        string Room { get; }
        string Name { get; }
        string Model { get; }
        ReadOnlyCollection<string> PcbNumberList { get; }
        string SoftwareVersion { get; }
        string ProductId { get; }
        JsonObject Json { get; }
    }

    public delegate void DeviceVolkanoHandler(CpDeviceVolkano aDevice);

    public abstract class CpDeviceVolkano : ICpDeviceReprogrammable, IDisposable
    {
        public CpDeviceVolkano(CpDevice aDevice, DeviceVolkanoHandler aHandler)
        {
            try
            {
                iHandler = aHandler;
                iDevice = aDevice;
                iDevice.AddRef();

                iLock = new object();

                iVolkanoService = new CpProxyLinnCoUkVolkano1(iDevice);

                iModel = "Unknown";

                string xml;
                if(iDevice.GetAttribute("Upnp.DeviceXml", out xml))
                {
                    XmlNameTable table = new NameTable();
                    XmlNamespaceManager manager = new XmlNamespaceManager(table);
                    manager.AddNamespace("ns", "urn:schemas-upnp-org:device-1-0");

                    XmlDocument doc = new XmlDocument(manager.NameTable);
                    doc.LoadXml(xml);

                    XmlNode node = doc.SelectSingleNode("/ns:root/ns:device/ns:modelName", manager);
                    if(node != null && node.FirstChild != null)
                    {
                        iModel = node.FirstChild.InnerText;
                        if(iModel.StartsWith("Reprogram-"))
                        {
                            string[] split = iModel.Split(new char[] { '-' }, 2);
                            if(split.Length == 2)
                            {
                                iModel = split[1];
                            }
                        }
                    }
                }

                iVolkanoService.SyncMacAddress(out iMacAddress);
                iVolkanoService.SyncSoftwareVersion(out iSoftwareVersion);
                iVolkanoService.SyncProductId(out iProductNumber);

                iPcbNumberList = new List<string>();

                uint count;
                iVolkanoService.SyncMaxBoards(out count);

                for(uint i = 0; i < count; ++i)
                {
                    string number;
                    iVolkanoService.SyncBoardType(i, out number);
                    iPcbNumberList.Add(Convert.ToUInt32(number.Substring(0, 8), 16).ToString());
                }
            }
            catch
            {
                aDevice.RemoveRef();
                throw;
            }
        }

        public virtual void Dispose()
        {
            iDevice.RemoveRef();
            iDevice = null;

            iVolkanoService.Dispose();
            iVolkanoService = null;
        }

        #region ICpDeviceReprogammable implementation
        public CpDevice Device
        {
            get
            {
                return iDevice;
            }
        }

        public string Udn
        {
            get
            {
                return iDevice.Udn();
            }
        }

        public string MacAddress
        {
            get
            {
                return iMacAddress;
            }
        }

        public abstract string Room { get; }
        public abstract string Name { get; }

        public string Model
        { 
            get
            {
                return iModel;
            }
        }

        public ReadOnlyCollection<string> PcbNumberList
        {
            get
            {
                return iPcbNumberList.AsReadOnly();
            }
        }

        public string SoftwareVersion
        {
            get
            {
                return iSoftwareVersion;
            }
        }

        public string ProductId
        {
            get
            {
                return iProductNumber;
            }
        }

        public virtual JsonObject Json
        { 
            get
            {
                JsonObject info = new JsonObject();
                info.Add("Model", new JsonValueString(iModel));
                info.Add("SoftwareVersion", new JsonValueString(iSoftwareVersion));
                info.Add("ProductId", new JsonValueString(iProductNumber));
                return info;
            }
        }
        #endregion

        public static CpDeviceVolkano Create(CpDevice aDevice, DeviceVolkanoHandler aHandler)
        {
            string value;
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Product", out value))
            {
                return new CpDeviceMain(aDevice, aHandler);
            }

            return new CpDeviceFallback(aDevice, aHandler);
        }

        protected void OnSubscribed()
        {
            if(iHandler != null)
            {
                iHandler(this);
            }
        }

        private object iLock;
        private CpDevice iDevice;
        private DeviceVolkanoHandler iHandler;

        private CpProxyLinnCoUkVolkano1 iVolkanoService;
        private string iModel;
        private string iMacAddress;
        private List<string> iPcbNumberList;
        private string iSoftwareVersion;
        private string iProductNumber;
    }

    public class CpDeviceMain : CpDeviceVolkano
    {
        public CpDeviceMain(CpDevice aDevice, DeviceVolkanoHandler aHandler)
            : base(aDevice, aHandler)
        {
            try
            {
                iLock = new object();

                iProductService = new CpProxyAvOpenhomeOrgProduct1(aDevice);
                iProductService.SetPropertyInitialEvent(InitialEvent);
                iProductService.SetPropertyProductRoomChanged(ProductRoomChanged);
                iProductService.SetPropertyProductNameChanged(ProductNameChanged);
                iProductService.SetPropertyProductImageUriChanged(ProductImageUriChanged);
                iProductService.Subscribe();
            }
            catch
            {
                aDevice.RemoveRef();
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            iProductService.Dispose();
            iProductService = null;
        }

        public string ImageUri
        {
            get
            {
                return iProductImageUri;
            }
        }

        #region implemented abstract members of Linn.Konfig.CpDeviceVolkano
        public override string Room
        {
            get
            {
                lock(iLock)
                {
                    return iRoom;
                }
            }
        }

        public override string Name
        {
            get
            {
                lock(iLock)
                {
                    return iName;
                }
            }
        }

        public override JsonObject Json
        {
            get
            {
                lock(iLock)
                {
                    JsonObject info = base.Json;
                    info.Add("Room", new JsonValueString(iRoom));
                    info.Add("Name", new JsonValueString(iName));
                    info.Add("ImageUri", new JsonValueString(iProductImageUri));
                    return info;
                }
            }
        }
        #endregion

        public EventHandler<EventArgs> Changed;

        private void InitialEvent()
        {
            lock(iLock)
            {
                iRoom = iProductService.PropertyProductRoom();
                iName = iProductService.PropertyProductName();
                iProductImageUri = iProductService.PropertyProductImageUri();

                OnSubscribed();
            }
        }

        private void ProductRoomChanged()
        {
            lock(iLock)
            {
                iRoom = iProductService.PropertyProductRoom();
            }

            if(Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        private void ProductNameChanged()
        {
            lock(iLock)
            {
                iName = iProductService.PropertyProductName();
            }

            if(Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        private void ProductImageUriChanged()
        {
            lock(iLock)
            {
                iProductImageUri = iProductService.PropertyProductImageUri();
            }

            if(Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        private object iLock;
        private CpProxyAvOpenhomeOrgProduct1 iProductService;

        private string iRoom;
        private string iName;
        private string iProductImageUri;
    }

    public class CpDeviceFallback : CpDeviceVolkano
    {
        public CpDeviceFallback(CpDevice aDevice, DeviceVolkanoHandler aHandler)
            : base(aDevice, aHandler)
        {
            try
            {
                OnSubscribed();
            }
            catch
            {
                aDevice.RemoveRef();
            }
        }

        public string ImageUri
        {
            get
            {
                return kImageUri;
            }
        }

        #region implemented abstract members of Linn.Konfig.CpDeviceVolkano
        public override string Room
        {
            get
            {
                return MacAddress;
            }
        }

        public override string Name
        {
            get
            {
                return kName;
            }
        }

        public override JsonObject Json
        {
            get
            {
                JsonObject info = base.Json;
                info.Add("Room", new JsonValueString(MacAddress));
                info.Add("Name", new JsonValueString(kName));
                info.Add("ImageUri", new JsonValueString(kImageUri));
                return info;
            }
        }
        #endregion

        private static readonly string kName = "Reprogram";
        private static readonly string kImageUri = "images/FallbackIcon.png";
    }

    public interface ICpDeviceUpdateable
    {
        bool Updating { get; set; }
        void WaitForUpdateToComplete();
    }

    public class CpDeviceReprogrammable : ICpDeviceReprogrammable, ICpDeviceUpdateable, IDisposable
    {
        public enum EStatus
        {
            eMain,
            eFallback,
            eOff
        }

        public CpDeviceReprogrammable(CpDeviceVolkano aDevice)
            : this()
        {
            SetDevice(aDevice);
        }

        protected CpDeviceReprogrammable()
        {
            iLock = new object();

            iUpdating = false;
            iMessage = string.Empty;
            iProgress = 0;
            iEventUpdating = new ManualResetEvent(true);
        }

        public void Dispose()
        {
            lock(iLock)
            {
                if(iDeviceMain != null)
                {
                    iDeviceMain.Changed -= HandleChanged;
                    iDeviceMain.Dispose();
                    iDeviceMain = null;
                }

                if(iDeviceFallback != null)
                {
                    iDeviceFallback.Dispose();
                    iDeviceFallback = null;
                }
            }
        }

        public bool Updating
        {
            get
            {
                lock (iLock)
                {
                    return iUpdating;
                }
            }
            set
            {
                lock (iLock)
                {
                    iUpdating = value;
                    if (iUpdating)
                    {
                        iEventUpdating.Reset();
                    }
                    else
                    {
                        iEventUpdating.Set();
                    }
                }
            }
        }

        public uint Progress
        {
            get
            {
                lock (iLock)
                {
                    return iProgress;
                }
            }
            set
            {
                lock(iLock)
                {
                    if(iProgress != value)
                    {
                        iProgress = value;

                        if(ProgressChanged != null)
                        {
                            ProgressChanged(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }
        
        public string Message
        {
            get
            {
                lock (iLock)
                {
                    return iMessage;
                }
            }
            set
            {
                lock(iLock)
                {
                    if(iMessage != value)
                    {
                        iMessage = value;

                        if(MessageChanged != null)
                        {
                            MessageChanged(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        public void WaitForUpdateToComplete()
        {
            iEventUpdating.WaitOne();
        }

        public void SetDevice(CpDeviceVolkano aDevice)
        {
            lock(iLock)
            {
                if(iDeviceMain != null)
                {
                    iDeviceMain.Changed -= HandleChanged;
                    iDeviceMain.Dispose();
                    iDeviceMain = null;
                }

                if(iDeviceFallback != null)
                {
                    iDeviceFallback.Dispose();
                    iDeviceFallback = null;
                }

                if(aDevice is CpDeviceMain)
                {
                    SetMain(aDevice as CpDeviceMain);
                }

                if(aDevice is CpDeviceFallback)
                {
                    SetFallback(aDevice as CpDeviceFallback);
                }

                if(aDevice == null)
                {
                    iImageUri = kImageUri;
                }
            }

            if(Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        public EventHandler<EventArgs> Changed;
        public EventHandler<EventArgs> ProgressChanged;
        public EventHandler<EventArgs> MessageChanged;

        private void SetMain(CpDeviceMain aDevice)
        {
            iDeviceMain = aDevice;
            iDeviceMain.Changed += HandleChanged;

            iRoom = iDeviceMain.Room;
            iName = iDeviceMain.Name;
            iModel = iDeviceMain.Model;
            iPcbNumberList = iDeviceMain.PcbNumberList;
            iSoftwareVersion = iDeviceMain.SoftwareVersion;
            iProductId = iDeviceMain.ProductId;
            iMacAddress = iDeviceMain.MacAddress;
            iImageUri = iDeviceMain.ImageUri;
            iIsFallback = false;
        }

        private void HandleChanged(object sender, EventArgs e)
        {
            iRoom = iDeviceMain.Room;
            iName = iDeviceMain.Name;

            if(Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        private void SetFallback(CpDeviceFallback aDevice)
        {
            iDeviceFallback = aDevice;

            if(string.IsNullOrEmpty(iRoom))
            {
                iRoom = iDeviceFallback.Room;
            }

            if(string.IsNullOrEmpty(iName))
            {
                iName = iDeviceFallback.Name;
            }

            if(string.IsNullOrEmpty(iModel))
            {
                iModel = iDeviceFallback.Model;
            }

            if(iPcbNumberList == null)
            {
                iPcbNumberList = iDeviceFallback.PcbNumberList;
            }

            //if(string.IsNullOrEmpty(iSoftwareVersion))
            {
                iSoftwareVersion = iDeviceFallback.SoftwareVersion;
            }

            if (string.IsNullOrEmpty(iProductId))
            {
                iProductId = iDeviceFallback.ProductId;
            }

            if(string.IsNullOrEmpty(iMacAddress))
            {
                iMacAddress = iDeviceFallback.MacAddress;
            }

            iImageUri = iDeviceFallback.ImageUri;

            iIsFallback = true;
        }

        public string Fullname 
        {
            get
            {
                lock(iLock)
                {
                    return string.Format("{0} ({1})", iRoom, iName);
                }
            }
        }

        public bool IsFallback
        {
            get
            {
                return iIsFallback;
            }
        }

        public EStatus Status
        {
            get
            {
                lock(iLock)
                {
                    if(iDeviceMain != null)
                    {
                        return EStatus.eMain;
                    }
                    if(iDeviceFallback != null)
                    {
                        return EStatus.eFallback;
                    }

                    return EStatus.eOff;
                }
            }
        }

        #region ICpDeviceReprogammable implementation
        public CpDevice Device
        {
            get
            {
                lock(iLock)
                {
                    if(iDeviceMain != null)
                    {
                        return iDeviceMain.Device;
                    }

                    if(iDeviceFallback != null)
                    {
                        return iDeviceFallback.Device;
                    }

                    return null;
                }
            }
        }

        public string Udn
        {
            get
            {
                lock(iLock)
                {
                    if(iDeviceMain != null)
                    {
                        return iDeviceMain.Udn;
                    }

                    if(iDeviceFallback != null)
                    {
                        return iDeviceFallback.Udn;
                    }

                    return string.Empty;
                }
            }
        }

        public string MacAddress
        {
            get
            {
                lock(iLock)
                {
                    return iMacAddress;
                }
            }
        }

        public string Room
        {
            get
            {
                lock(iLock)
                {
                    return iRoom;
                }
            }
        }

        public string Name 
        {
            get
            {
                lock(iLock)
                {
                    return iName;
                }
            }
        }

        public string Model 
        {
            get
            {
                lock(iLock)
                {
                    return iModel;
                }
            }
        }

        public ReadOnlyCollection<string> PcbNumberList
        {
            get
            {
                lock(iLock)
                {
                    return iPcbNumberList;
                }
            }
        }

        public string SoftwareVersion
        {
            get
            {
                lock(iLock)
                {
                    if(iDeviceMain != null)
                    {
                        return iDeviceMain.SoftwareVersion;
                    }

                    if(iDeviceFallback != null)
                    {
                        return iDeviceFallback.SoftwareVersion;
                    }

                    return iSoftwareVersion;
                }
            }
        }

        public string ProductId
        {
            get
            {
                lock (iLock)
                {
                    if (iDeviceMain != null)
                    {
                        return iDeviceMain.ProductId;
                    }

                    if (iDeviceFallback != null)
                    {
                        return iDeviceFallback.ProductId;
                    }

                    return iProductId;
                }
            }
        }

        public JsonObject Json
        {
            get
            {
                lock(iLock)
                {
                    JsonObject info = new JsonObject();
                    info.Add("SoftwareVersion", new JsonValueString(iSoftwareVersion));
                    info.Add("Model", new JsonValueString(iModel));
                    info.Add("Room", new JsonValueString(iRoom));
                    if(iModel != iName)
                    {
                        info.Add("Name", new JsonValueString(string.Format(" ({0})", iName)));
                    }
                    else
                    {
                        info.Add("Name", new JsonValueString(string.Empty));
                    }
                    info.Add("ImageUri", new JsonValueString(iImageUri));

                    EStatus result = Status;
                    string status = string.Empty;
                    if(result == EStatus.eOff)
                    {
                        status = "Off";
                    }
                    else if(result == EStatus.eMain)
                    {
                        status = "Main";
                    }
                    else if(result == EStatus.eFallback)
                    {
                        status = "Fallback";
                    }

                    info.Add("Status", new JsonValueString(status));
                    info.Add("Updating", new JsonValueBool(iUpdating));
                    return info;
                }
            }
        }
        #endregion

        private static readonly string kImageUri = "images/FallbackIcon.png";

        private object iLock;
        private string iRoom;
        private string iName;
        private string iModel;
        private ReadOnlyCollection<string> iPcbNumberList;
        private string iSoftwareVersion;
        private string iProductId;
        private string iMacAddress;
        private string iImageUri;
        private bool iIsFallback;

        private bool iUpdating;
        private string iMessage;
        private uint iProgress;
        private ManualResetEvent iEventUpdating;

        private CpDeviceMain iDeviceMain;
        private CpDeviceFallback iDeviceFallback;
    }

    public class CpDeviceReprogramListRepeater : IDisposable
    {
        public class CpDeviceReprogrammableEventArgs : EventArgs
        {
            public CpDeviceReprogrammableEventArgs(CpDeviceReprogrammable aDevice)
            {
                iDevice = aDevice;
            }

            public CpDeviceReprogrammable Device
            {
                get
                {
                    return iDevice;
                }
            }

            private CpDeviceReprogrammable iDevice;
        }

        public CpDeviceReprogramListRepeater()
        {
            iLock = new object();
            iDisposed = false;

            iDeviceListReprogrammable = new List<CpDeviceReprogrammable>();
            iDeviceListReprogram = new CpDeviceReprogramList(AddedHandler, RemovedHandler);
        }

        public void Dispose()
        {
            iDeviceListReprogram.Dispose();
            iDeviceListReprogram = null;

            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceReprogramListRepeater");
                }

                iDeviceListReprogrammable.Clear();

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceReprogramListRepeater");
                }

                iDeviceListReprogram.Refresh();
            }
        }

        private EventHandler<CpDeviceReprogrammableEventArgs> iAdded;
        public event EventHandler<CpDeviceReprogrammableEventArgs> Added
        {
            add
            {
                lock (iLock)
                {
                    foreach (CpDeviceReprogrammable d in iDeviceListReprogrammable)
                    {
                        value(this, new CpDeviceReprogrammableEventArgs(d));
                    }
                    iAdded += value;
                }
            }
            remove
            {
                lock (iLock)
                {
                    iAdded -= value;
                }
            }
        }

        public event EventHandler<CpDeviceReprogrammableEventArgs> Removed;

        private void AddedHandler(CpDeviceReprogramList aList, CpDeviceReprogrammable aDevice)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }
            }

            if (iAdded != null)
            {
                iAdded(this, new CpDeviceReprogrammableEventArgs(aDevice));
            }
        }

        private void RemovedHandler(CpDeviceReprogramList aList, CpDeviceReprogrammable aDevice)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }   
            }

            if (Removed != null)
            {
                Removed(this, new CpDeviceReprogrammableEventArgs(aDevice));
            }
        }

        private object iLock;
        private bool iDisposed;

        private List<CpDeviceReprogrammable> iDeviceListReprogrammable;
        private CpDeviceReprogramList iDeviceListReprogram;
    }

    public class CpDeviceReprogramList : IDisposable
    {
        public CpDeviceReprogramList(ChangedHandler aAdded, ChangedHandler aRemoved)
        {
            iAdded = aAdded;
            iRemoved = aRemoved;

            iDisposed = false;
            iLock = new object();
            iDeviceListPending = new List<CpDeviceVolkano>();
            iDeviceListReprogrammable = new SortedList<string, CpDeviceReprogrammable>();

            iDeviceListVolkano = new CpDeviceListUpnpServiceType("linn.co.uk", "Volkano", 1, Added, Removed);
        }

        public void Dispose()
        {
            iDeviceListVolkano.Dispose();
            iDeviceListVolkano = null;

            lock(iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceReprogramList");
                }

                foreach (CpDeviceVolkano d in iDeviceListPending)
                {
                    d.Dispose();
                }

                foreach(CpDeviceReprogrammable d in iDeviceListReprogrammable.Values)
                {
                    d.Dispose();
                }

                iDeviceListPending.Clear();
                iDeviceListReprogrammable.Clear();

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            lock(iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceReprogramList");
                }

                iDeviceListVolkano.Refresh();
            }
        }

        public delegate void ChangedHandler(CpDeviceReprogramList aList, CpDeviceReprogrammable aDevice);

        private void Added(CpDeviceList aList, CpDevice aDevice)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceReprogramList: Device+             Udn{" + aDevice.Udn() + "}");

            CpDeviceVolkano device = CpDeviceVolkano.Create(aDevice, ReprogrammableAdded);

            lock(iLock)
            {
                if(iDisposed)
                {
                    return;
                }

                // check to see if device has been added immediately
                if (!iDeviceListReprogrammable.ContainsKey(device.MacAddress))
                {
                    iDeviceListPending.Add(device);
                }
            }
        }

        private void ReprogrammableAdded(CpDeviceVolkano aDevice)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceReprogramList: DeviceVolkano+             Udn{" + aDevice.Udn + "}, MacAddress{" + aDevice.MacAddress + "}");
            CpDeviceReprogrammable device = null;

            lock(iLock)
            {
                if (iDisposed)
                {
                    aDevice.Dispose();

                    return;
                }

                iDeviceListPending.Remove(aDevice);
                if(iDeviceListReprogrammable.ContainsKey(aDevice.MacAddress))
                {
                    iDeviceListReprogrammable[aDevice.MacAddress].SetDevice(aDevice);
                }
                else
                {
                    device = new CpDeviceReprogrammable(aDevice);
                    iDeviceListReprogrammable.Add(aDevice.MacAddress, device);
                }
            }

            if(device != null)
            {
                if(iAdded != null)
                {
                    iAdded(this, device);
                }
            }
        }

        private void Removed(CpDeviceList aList, CpDevice aDevice)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceReprogramList: Device-             Udn{" + aDevice.Udn() + "}");

            //CpDeviceReprogrammable removedDevice = null;
            lock(iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                foreach (CpDeviceVolkano d in iDeviceListPending)
                {
                    if (d.Udn == aDevice.Udn())
                    {
                        iDeviceListPending.Remove(d);

                        d.Dispose();

                        return;
                    }
                }

                foreach(KeyValuePair<string, CpDeviceReprogrammable> d in iDeviceListReprogrammable)
                {
                    if(d.Value.Udn == aDevice.Udn())
                    {
                        //removedDevice = d.Value;

                        d.Value.SetDevice(null);

                        break;
                    }
                }
            }

            /*if (removedDevice != null)
            {
                if (iRemoved != null)
                {
                    iRemoved(this, removedDevice);
                }

                removedDevice.Dispose();
            }*/
        }

        private bool iDisposed;
        private object iLock;

        private ChangedHandler iAdded;
        private ChangedHandler iRemoved;
        private CpDeviceListUpnpServiceType iDeviceListVolkano;

        private List<CpDeviceVolkano> iDeviceListPending;
        private SortedList<string, CpDeviceReprogrammable> iDeviceListReprogrammable;
    }
}

