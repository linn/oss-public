using System;
using System.Diagnostics;
using System.Collections.Generic;

using OpenHome.Net.Core;
using OpenHome.Xapp;

namespace Linn.Konfig
{
    public class TrackerSession : Session, ITrackerSender
    {
        public TrackerSession()
            : base()
        {
            iTracker = new Tracker(Model.Instance.Preferences.TrackerAccount, this);
            Model.Instance.Preferences.EventUsageDataChanged += Preferences_EventUsageDataChanged;
        }

        void Preferences_EventUsageDataChanged(object sender, EventArgs e)
        {
            iTracker.SetTracking(Model.Instance.Preferences.UsageData);
        }

        public override void Dispose()
        {
            Model.Instance.Preferences.EventUsageDataChanged -= Preferences_EventUsageDataChanged;            
            base.Dispose();
        }

        public Tracker Tracker { get { return iTracker; } }

        void ITrackerSender.Send(string aName, JsonObject aValue)
        {
            Send(aName, aValue);
        }

        private Tracker iTracker;
    }

    public class XappController : IDisposable
    {
        public XappController(IInvoker aInvoker, IHelper aHelper, INetworkManager aNetworkManager, Preferences aPreferences, PageBase aPageSettings, IUpdateListener aUpdateListener)
        {
            iNetworkManager = aNetworkManager;
            iNetworkManager.AdapterListChanged += HandleAdapterListChanged;

            iFramework = new Framework<TrackerSession>(OpenHome.Xen.Environment.AppPath + "/presentation");
            iFramework.AddCss("css/global.css");

            iRepeater = new CpDeviceReprogramListRepeater();

            iCache = new FirmwareCache(aHelper.DataPath.FullName);

            VersionInfoReader.EUpdateType updateTypes = VersionInfoReader.EUpdateType.Stable;
            if (aPreferences.FirmwareBeta)
            {
                updateTypes |= VersionInfoReader.EUpdateType.Beta;
            }
            iVersionReader = new VersionInfoReader(VersionInfoReader.kDefaultUpdatesUrl, 1000 * 60 * 60 * 12, aHelper.DataPath.FullName, updateTypes, iCache);

            XappView mainView = new XappView("main", OpenHome.Xen.Environment.AppPath + "/presentation");
            iMainPage = new MainPage(aInvoker, iNetworkManager.AdapterList.Adapter, aPreferences, aUpdateListener, iRepeater, "main", mainView.Id);
            iFramework.AddPage(iMainPage);
            iFramework.AddView(mainView);

            XappView updateView = new XappView("update", OpenHome.Xen.Environment.AppPath + "/presentation");
            iUpdatePage = new UpdatePage(aInvoker, iNetworkManager.AdapterList.Adapter, aPreferences, aUpdateListener, iRepeater, iCache, iVersionReader, "update", updateView.Id);
            iUpdatePage.NumUpdateableChanged += HandleNumUpdateableChanged;
            iFramework.AddPage(iUpdatePage);
            iFramework.AddView(updateView);

            XappView settingsView = new XappView(aPageSettings.Id, OpenHome.Xen.Environment.AppPath + "/presentation");
            iSettingsPage = aPageSettings;
            iFramework.AddPage(iSettingsPage);
            iFramework.AddView(settingsView);

            XappView diagnosticsView = new XappView("diagnostics", OpenHome.Xen.Environment.AppPath + "/presentation");
            iDiagnosticsPage = new DiagnosticsPage(aInvoker, aHelper, aPreferences, "diagnostics", diagnosticsView.Id);
            iDiagnosticsPage.NumDiagnosticsChanged += HandleNumDiagnosticsChanged;
            iFramework.AddPage(iDiagnosticsPage);
            iFramework.AddView(diagnosticsView);

            XappView advancedView = new XappView("advanced", OpenHome.Xen.Environment.AppPath + "/presentation");
            iAdvancedPage = new AdvancedPage(aInvoker, iNetworkManager.AdapterList.Adapter, aUpdateListener, iRepeater, iVersionReader, "advanced", advancedView.Id);
            iFramework.AddPage(iAdvancedPage);
            iFramework.AddView(advancedView);


            XappView aboutView = new XappView("about", OpenHome.Xen.Environment.AppPath + "/presentation");
            iAboutPage = new AboutPage(aInvoker, aHelper, "about", aboutView.Id);
            iFramework.AddPage(iAboutPage);
            iFramework.AddView(aboutView);

            iWebServer = new WebServer(iFramework);

            iVersionReader.Start();
        }

        public void Dispose()
        {
            iCache.Dispose();
            iCache = null;

            iVersionReader.Dispose();
            iVersionReader = null;

            iRepeater.Dispose();
            iRepeater = null;

            if(iUpdatePage != null)
            {
                iUpdatePage.NumUpdateableChanged -= HandleNumUpdateableChanged;
            }

            if(iDiagnosticsPage != null)
            {
                iDiagnosticsPage.NumDiagnosticsChanged -= HandleNumDiagnosticsChanged;
            }

            if(iFramework != null)
            {
                iFramework.Dispose();
                iFramework = null;
            }

            if(iWebServer != null)
            {
                iWebServer.Dispose();
                iWebServer = null;
            }

            if(iNetworkManager != null)
            {
                iNetworkManager.AdapterListChanged -= HandleAdapterListChanged;
                iNetworkManager = null;
            }
        }

        public string MainPageUri
        {
            get { return iWebServer.ResourceUri + iMainPage.UriPath; }
        }
        
        public MainPage MainPage
        {
            get { return iMainPage; }
        }

        private void HandleAdapterListChanged (object sender, EventArgs e)
        {
            NetworkAdapter adapter = iNetworkManager.AdapterList.Adapter;
            iUpdatePage.SetAdapter(adapter);
            iDiagnosticsPage.SetAdapter(adapter);
            iAdvancedPage.SetAdapter(adapter);
        }

        private void HandleNumDiagnosticsChanged (object sender, EventArgs e)
        {
            SetBadges();
        }

        private void HandleNumUpdateableChanged (object sender, EventArgs e)
        {
            SetBadges();
        }

        private void SetBadges()
        {
            uint numUpdateable = iUpdatePage.NumUpdateable;
            uint numDiagnostics = iDiagnosticsPage.NumDiagnostics;

            iMainPage.SetBadges(numUpdateable, numDiagnostics);
            iUpdatePage.SetBadges(numUpdateable, numDiagnostics);
            iDiagnosticsPage.SetBadges(numUpdateable, numDiagnostics);
            iAdvancedPage.SetBadges(numUpdateable, numDiagnostics);
            iSettingsPage.SetBadges(numUpdateable, numDiagnostics);
            iAboutPage.SetBadges(numUpdateable, numDiagnostics);
        }

        private INetworkManager iNetworkManager;
        private Framework iFramework;
        private WebServer iWebServer;

        private MainPage iMainPage;
        private UpdatePage iUpdatePage;
        private DiagnosticsPage iDiagnosticsPage;
        private AdvancedPage iAdvancedPage;
        private PageBase iSettingsPage;
        private AboutPage iAboutPage;

        private CpDeviceReprogramListRepeater iRepeater;
        private FirmwareCache iCache;
        private VersionInfoReader iVersionReader;
    }


    public class LongPollException : Exception
    {
    }

    public class PageBase : Page<TrackerSession>
    {
        public PageBase(string aId, string aViewId)
            : base(aId, aViewId)
        {
            iLock = new object();

            iNumSoftwareUpdates = 0;
            iNumDiagnostics = 0;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnActivated(TrackerSession aSession)
        {
            lock(iLock)
            {
                SetBadges(iNumSoftwareUpdates, iNumDiagnostics);
            }
            aSession.Tracker.SetTracking(Model.Instance.Preferences.UsageData);
            aSession.Tracker.TrackPageView(this.ViewId);
        }

        protected override void OnReceive(TrackerSession aSession, string aName, string aValue)
        {
            if (aName == "LongPollError")
            {
                UserLog.WriteLine(DateTime.Now + " LongPollError: " + aValue);
                throw new LongPollException();
            }

            if (aName == "Open")
            {
                aSession.Navigate(aValue);
            }

            if (aName == "OpenBrowser")
            {
                Process.Start(aValue);
            }
        }

        public void SetBadges(uint aNumSoftwareUpdates, uint aNumDiagnostics)
        {
            lock(iLock)
            {
                iNumSoftwareUpdates = aNumSoftwareUpdates;
                iNumDiagnostics = aNumDiagnostics;
            }

            JsonObject info = new JsonObject();
            info.Add("NumSoftwareUpdates", new JsonValueUint(aNumSoftwareUpdates));
            info.Add("NumDiagnostics", new JsonValueUint(aNumDiagnostics));
            Send("UpdateBadges", info);
        }

        private object iLock;

        private uint iNumSoftwareUpdates;
        private uint iNumDiagnostics;
    }

    public class MainPage : PageBase
    {
        public MainPage(IInvoker aInvoker, NetworkAdapter aAdapter, Preferences aPreferences, IUpdateListener aUpdateListener, CpDeviceReprogramListRepeater aRepeater, string aId, string aViewId)
            : base(aId, aViewId)
        {
            iInvoker = aInvoker;

            iPreferences = aPreferences;

            iLock = new object();
            iDeviceListRecognised = new List<CpDeviceRecognised>();

            List<IRecogniser> recognisers = new List<IRecogniser>();
            recognisers.Add(new RecogniserLinn());
            recognisers.Add(new RecogniserSonos());
            recognisers.Add (new RecogniserMediaServer());
            iRecogniserList = new CpDeviceRecogniserList(recognisers, Added, Removed);
        }

        public override void Dispose()
        {
            base.Dispose();

            if(iRecogniserList != null)
            {
                iRecogniserList.Dispose();
                iRecogniserList = null;
            }

            lock(iLock)
            {
                foreach(CpDeviceRecognised d in iDeviceListRecognised)
                {
                    d.Changed -= Changed;
                }

                iDeviceListRecognised.Clear();
            }
        }

        private delegate void DOnActivated(Session aSession);
        protected override void OnActivated(Session aSession)
        {
            if (iInvoker.TryBeginInvoke(new DOnActivated(OnActivated), aSession))
                return;

            lock (iLock)
            {
                aSession.Send("SelectProductUdn", iPreferences.SelectedProductUdn);
            }

            base.OnActivated(aSession);

            iRecogniserList.Refresh();

            lock(iLock)
            {
                int i = 0;
                foreach (CpDeviceRecognised d in iDeviceListRecognised)
                {
                    AddDevice(d, i);
                    ++i;
                }
            }
        }
        
        private delegate void DOnReceive(Session aSession, string aName, string aValue);
        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            if (iInvoker.TryBeginInvoke(new DOnReceive(OnReceive), aSession, aName, aValue))
                return;

            base.OnReceive(aSession, aName, aValue);

            if (aName == "SetSelectedProductUdn")
            {
                iPreferences.SelectedProductUdn = aValue;
            }
        }

        private void Added(CpDeviceRecogniserList aList, CpDeviceRecognised aDevice)
        {
            lock(iLock)
            {
                iDeviceListRecognised.Add(aDevice);
                iDeviceListRecognised.Sort();
                int index = iDeviceListRecognised.IndexOf(aDevice);

                AddDevice(aDevice, index);
                aDevice.Changed += Changed;

                string udn = iPreferences.SelectedProductUdn;
                if ((aDevice.Udn == udn) || (string.IsNullOrEmpty(udn) && aDevice is CpDeviceRecognisedLinn))
                {
                    Send("SelectProductUdn", udn);
                }
            }
        }

        private void AddDevice(CpDeviceRecognised aDevice, int aIndex)
        {
            JsonObject info = new JsonObject();
            info.Add("Index", new JsonValueInt(aIndex));
            info.Add("Type", new JsonValueString(aDevice.Type.ToString()));
            info.Add("Udn", new JsonValueString(aDevice.Udn));
            info.Add("PresentationUri", new JsonValueString(aDevice.PresentationUri));
            info.Add("Description", aDevice.Json);
            Send("RecognisedDeviceAdded", info);
        }

        private void Removed(CpDeviceRecogniserList aList, CpDeviceRecognised aDevice)
        {
            lock(iLock)
            {
                RemoveDevice(aDevice);

                aDevice.Changed -= Changed;
                iDeviceListRecognised.Remove(aDevice);
            }
        }

        private void RemoveDevice(CpDeviceRecognised aDevice)
        {
            Send("RecognisedDeviceRemoved", aDevice.Udn);
        }

        private void Changed(object sender, EventArgs e)
        {
            lock(iLock)
            {
                CpDeviceRecognised d = sender as CpDeviceRecognised;

                if(d != null)
                {
                    Removed(null, d);
                    Added (null, d);
                }
            }
        }

        private object iLock;
        private IInvoker iInvoker;

        private Preferences iPreferences;

        private CpDeviceRecogniserList iRecogniserList;
        private List<CpDeviceRecognised> iDeviceListRecognised;
    }

    public class UpdatePage : PageBase
    {
        public UpdatePage(IInvoker aInvoker, NetworkAdapter aAdapter, Preferences aPreferences, IUpdateListener aUpdateListener, CpDeviceReprogramListRepeater aRepeater, FirmwareCache aCache, VersionInfoReader aVersionReader, string aId, string aViewId)
            : base(aId, aViewId)
        {
            iInvoker = aInvoker;

            iLock = new object();
            iDeviceListUpdateable = new List<CpDeviceUpdate>();

            iReprogramList = new CpDeviceUpdateList(aAdapter, aPreferences, aUpdateListener, aRepeater, aCache, aVersionReader, Added, Removed);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (iReprogramList != null)
            {
                iReprogramList.Dispose();
                iReprogramList = null;
            }

            lock (iLock)
            {
                foreach(CpDeviceUpdate d in iDeviceListUpdateable)
                {
                    d.Changed -= Changed;
                    d.ProgressChanged -= Progress;
                    d.MessageChanged -= Message;
                }

                iDeviceListUpdateable.Clear();
            }
        }

        public void SetAdapter(NetworkAdapter aAdapter)
        {
            lock(iLock)
            {
                iReprogramList.SetAdapter(aAdapter);
            }
        }

        public uint NumUpdateable
        {
            get
            {
                lock(iLock)
                {
                    return (uint)iDeviceListUpdateable.Count;
                }
            }
        }

        public event EventHandler<EventArgs> NumUpdateableChanged;

        private delegate void DOnActivated(Session aSession);
        protected override void OnActivated(Session aSession)
        {
            if (iInvoker.TryBeginInvoke(new DOnActivated(OnActivated), aSession))
                return;

            base.OnActivated(aSession);

            iReprogramList.Refresh();

            lock (iLock)
            {
                int i = 0;
                foreach(CpDeviceUpdate d in iDeviceListUpdateable)
                {
                    AddDevice(d, i);
                    ++i;
                }

                if(iDeviceListUpdateable.Count == 0)
                {
                    aSession.Send("AllUptodate", string.Empty);
                }
            }
        }

        private delegate void DOnReceive(Session aSession, string aName, string aValue);
        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            if (iInvoker.TryBeginInvoke(new DOnReceive(OnReceive), aSession, aName, aValue))
                return;

            base.OnReceive(aSession, aName, aValue);

            if (aName == "Update")
            {
                iReprogramList.Update(aValue);
            }

            if (aName == "UpdateAll")
            {
                iReprogramList.UpdateAll();
            }
        }

        private void Added(CpDeviceUpdateList aList, CpDeviceUpdate aDevice)
        {
            lock (iLock)
            {
                iDeviceListUpdateable.Add(aDevice);
                iDeviceListUpdateable.Sort();
                int index = iDeviceListUpdateable.IndexOf(aDevice);

                AddDevice(aDevice, index);
                aDevice.Changed += Changed;
                aDevice.ProgressChanged += Progress;
                aDevice.MessageChanged += Message;
            }

            if(NumUpdateableChanged != null)
            {
                NumUpdateableChanged(this, EventArgs.Empty);
            }
        }

        private void AddDevice(CpDeviceUpdate aDevice, int aIndex)
        {
            JsonObject info = new JsonObject();
            info.Add("Index", new JsonValueInt(aIndex));
            info.Add("MacAddress", new JsonValueString(aDevice.MacAddress));
            info.Add("Description", aDevice.Json);
            info.Add("Progress", new JsonValueUint(aDevice.Progress));
            info.Add("Message", new JsonValueString(aDevice.Message));
            info.Add("NewVersion", new JsonValueString(VersionSupport.SoftwareVersionPretty(aDevice.UpdateSoftwareVersion, true)));
            Send("UpdateableDeviceAdded", info);
        }

        private void Removed(CpDeviceUpdateList aList, CpDeviceUpdate aDevice)
        {
            lock (iLock)
            {
                Send("UpdateableDeviceRemoved", aDevice.MacAddress);

                aDevice.Changed -= Changed;
                aDevice.ProgressChanged -= Progress;
                aDevice.MessageChanged -= Message;
                iDeviceListUpdateable.Remove(aDevice);
            }

            if(NumUpdateableChanged != null)
            {
                NumUpdateableChanged(this, EventArgs.Empty);
            }
        }

        private void Changed(object sender, EventArgs e)
        {
            lock (iLock)
            {
                CpDeviceUpdate d = sender as CpDeviceUpdate;

                if (d != null)
                {
                    JsonObject info = new JsonObject();
                    info.Add("MacAddress", new JsonValueString(d.MacAddress));
                    info.Add("Description", d.Json);
                    info.Add("Progress", new JsonValueUint(d.Progress));
                    info.Add("Message", new JsonValueString(d.Message));
                    info.Add("NewVersion", new JsonValueString(d.UpdateSoftwareVersion));
                    Send("UpdateableDeviceChanged", info);
                }
            }
        }

        private void Progress(object sender, EventArgs e)
        {
            CpDeviceUpdate device = sender as CpDeviceUpdate;

            JsonObject info = new JsonObject();
            info.Add("MacAddress", new JsonValueString(device.MacAddress));
            info.Add("Progress", new JsonValueUint(device.Progress));
            Send("UpdateableDeviceProgress", info);
        }

        private void Message(object sender, EventArgs e)
        {
            CpDeviceUpdate device = sender as CpDeviceUpdate;

            JsonObject info = new JsonObject();
            info.Add("MacAddress", new JsonValueString(device.MacAddress));
            info.Add("Message", new JsonValueString(device.Message));
            Send("UpdateableDeviceMessage", info);
        }

        private object iLock;

        private IInvoker iInvoker;

        private CpDeviceUpdateList iReprogramList;
        private List<CpDeviceUpdate> iDeviceListUpdateable;
    }

    public class DiagnosticsPage : PageBase
    {
        public DiagnosticsPage(IInvoker aInvoker, IHelper aHelper, Preferences aPreferences, string aId, string aViewId)
            : base(aId, aViewId)
        {
            iLock = new object();

            iInvoker = aInvoker;
            iPreferences = aPreferences;
            iPreferences.EventSendDsCrashDataChanged += HandleEventSendDsCrashDataChanged;

            DebugReport report = new DebugReport(string.Format("Crash log generated by {0} ver {1}", aHelper.Product, aHelper.Version));
            iDiagnosticsList = new CpDeviceDiagnosticsReportList(report, Added, Removed);
            iDeviceListDiagnostics = new List<CpDeviceDiagnosticsItem>();

            CheckAllowSendAll();
        }

        public override void Dispose()
        {
            base.Dispose();

            if(iDiagnosticsList != null)
            {
                iDiagnosticsList.Dispose();
                iDiagnosticsList = null;
            }

            lock(iLock)
            {
                foreach(CpDeviceDiagnosticsItem d in iDeviceListDiagnostics)
                {
                    d.Changed -= Changed;
                }

                iDeviceListDiagnostics.Clear();
            }

            iPreferences.EventSendDsCrashDataChanged -= HandleEventSendDsCrashDataChanged;
        }

        public void SetAdapter(NetworkAdapter Adapter)
        {
            CheckAllowSendAll();
        }

        public uint NumDiagnostics
        {
            get
            {
                lock(iLock)
                {
                    return (uint)iDeviceListDiagnostics.Count;
                }
            }
        }

        public event EventHandler<EventArgs> NumDiagnosticsChanged;

        private delegate void DOnActivated(Session aSession);
        protected override void OnActivated(Session aSession)
        {
            if (iInvoker.TryBeginInvoke(new DOnActivated(OnActivated), aSession))
                return;

            base.OnActivated(aSession);

            iDiagnosticsList.Refresh();

            lock (iLock)
            {
                int i = 0;
                foreach(CpDeviceDiagnosticsItem d in iDeviceListDiagnostics)
                {
                    AddDevice(d, i);
                    ++i;
                }

                if(iDeviceListDiagnostics.Count == 0)
                {
                    aSession.Send("AllWorking", string.Empty);
                }
            }
        }

        private delegate void DOnReceive(Session aSession, string aName, string aValue);
        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            if (iInvoker.TryBeginInvoke(new DOnReceive(OnReceive), aSession, aName, aValue))
                return;

            base.OnReceive(aSession, aName, aValue);

            if (aName == "Send")
            {
                iDiagnosticsList.Send(aValue);
            }

            if (aName == "Ignore")
            {
                iDiagnosticsList.Ignore(aValue);
            }

            if (aName == "SendAll")
            {
                if (iAllowSendAll)
                {
                    iDiagnosticsList.SendAll();
                }
            }

            if (aName == "IgnoreAll")
            {
                iDiagnosticsList.IgnoreAll();
            }

            if (aName == "SendDsCrashData")
            {
                iPreferences.SendDsCrashData = bool.Parse(aValue);
            }
        }

        private void Added(CpDeviceDiagnosticsReportList aList, CpDeviceDiagnosticsItem aDevice)
        {
            // if we are automatically sending crash data and device is reporting crash data, send the data and don't bother to add it to the list
            if(iPreferences.SendDsCrashData && aDevice.Type == CpDeviceDiagnosticsItem.EDiagnosticsType.eCrash)
            {
                iDiagnosticsList.Send(aDevice.Id);
            }
            else
            {
                lock(iLock)
                {
                    iDeviceListDiagnostics.Add(aDevice);
                    iDeviceListDiagnostics.Sort();
                    int index = iDeviceListDiagnostics.IndexOf(aDevice);

                    AddDevice(aDevice, index);
                    aDevice.Changed += Changed;
                }

                if(NumDiagnosticsChanged != null)
                {
                    NumDiagnosticsChanged(this, EventArgs.Empty);
                }
            }
        }

        private void AddDevice(CpDeviceDiagnosticsItem aDevice, int aIndex)
        {
            JsonObject info = new JsonObject();
            info.Add("Index", new JsonValueInt(aIndex));
            info.Add("Id", new JsonValueString(aDevice.Id));
            info.Add("Type", new JsonValueString(aDevice.Type.ToString()));
            info.Add("Description", aDevice.Json);
            Send("DeviceAdded", info);
        }

        private void Removed(CpDeviceDiagnosticsReportList aList, CpDeviceDiagnosticsItem aDevice)
        {
            lock(iLock)
            {
                Send("DeviceRemoved", aDevice.Id);

                aDevice.Changed -= Changed;
                iDeviceListDiagnostics.Remove(aDevice);
            }

            if(NumDiagnosticsChanged != null)
            {
                NumDiagnosticsChanged(this, EventArgs.Empty);
            }
        }

        private void Changed(object sender, EventArgs e)
        {
            lock (iLock)
            {
                CpDeviceDiagnosticsItem d = sender as CpDeviceDiagnosticsItem;

                if (d != null && iDeviceListDiagnostics.Contains(d))
                {
                    Removed(null, d);
                    Added(null, d);
                }
            }
        }

        private void HandleEventSendDsCrashDataChanged (object sender, EventArgs e)
        {
            if(iPreferences.SendDsCrashData)
            {
                iDiagnosticsList.SendAll();
            }
        }

        private void CheckAllowSendAll()
        {
            iAllowSendAll = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName != kLinnDomainName;
        }

        private const string kLinnDomainName = "linn.co.uk";

        private object iLock;

        private bool iAllowSendAll;

        private IInvoker iInvoker;
        private Preferences iPreferences;

        private CpDeviceDiagnosticsReportList iDiagnosticsList;
        private List<CpDeviceDiagnosticsItem> iDeviceListDiagnostics;
    }

    public class AdvancedPage : PageBase
    {
        public AdvancedPage(IInvoker aInvoker, NetworkAdapter aAdapter, IUpdateListener aUpdateListener, CpDeviceReprogramListRepeater aRepeater, VersionInfoReader aVersionReader, string aId, string aViewId)
            : base(aId, aViewId)
        {
            iLock = new object();
            iInvoker = aInvoker;

            iDeviceListAdvanced = new List<CpDeviceAdvanced>();
            iAdvancedList = new CpDeviceAdvancedList(aAdapter, aUpdateListener, aRepeater, aVersionReader, Added, Removed);
        }

        public override void Dispose()
        {
            base.Dispose();

            iAdvancedList.Dispose();
            iAdvancedList = null;

            lock (iLock)
            {
                foreach (CpDeviceAdvanced d in iDeviceListAdvanced)
                {
                    d.Changed -= Changed;
                    d.ProgressChanged -= Progress;
                    d.MessageChanged -= Message;
                }

                iDeviceListAdvanced.Clear();
            }
        }

        public void SetAdapter(NetworkAdapter aAdapter)
        {
            lock (iLock)
            {
                iAdvancedList.SetAdapter(aAdapter);
            }
        }

        private delegate void DOnActivated(Session aSession);
        protected override void OnActivated(Session aSession)
        {
            if (iInvoker.TryBeginInvoke(new DOnActivated(OnActivated), aSession))
                return;

            base.OnActivated(aSession);

            iAdvancedList.Refresh();

            lock (iLock)
            {
                int i = 0;
                foreach (CpDeviceAdvanced d in iDeviceListAdvanced)
                {
                    AddDevice(d, i);
                    ++i;
                }

                if (iDeviceListAdvanced.Count == 0)
                {
                    aSession.Send("NoAdvancedDevicesFound", string.Empty);
                }
            }
        }

        private delegate void DOnReceive(Session aSession, string aName, string aValue);
        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            if (iInvoker.TryBeginInvoke(new DOnReceive(OnReceive), aSession, aName, aValue))
                return;

            base.OnReceive(aSession, aName, aValue);

            if (aName == "Update")
            {
                string macAddress = string.Empty;
                int macAddressIndex = aValue.IndexOf("macAddress(");
                if (macAddressIndex > -1)
                {
                    int end = aValue.Substring(macAddressIndex).IndexOf(')');
                    macAddress = aValue.Substring(macAddressIndex + 11, end - 11);
                }

                string filename = string.Empty;
                int filenameIndex = aValue.IndexOf("filename(");
                if (filenameIndex > -1)
                {
                    int end = aValue.Substring(filenameIndex).IndexOf(')');
                    filename = aValue.Substring(filenameIndex + 9, end - 9);
                }

                bool recovery = false;
                int recoveryIndex = aValue.IndexOf("recovery(");
                if (recoveryIndex > -1)
                {
                    int end = aValue.Substring(recoveryIndex).IndexOf(')');
                    recovery = bool.Parse(aValue.Substring(recoveryIndex + 9, end - 9));
                }

                iAdvancedList.Update(macAddress, filename, recovery);
            }

            if (aName == "Restore")
            {
                iAdvancedList.FactoryDefaults(aValue);
            }
        }

        private void Added(CpDeviceAdvancedList aList, CpDeviceAdvanced aDevice)
        {
            lock (iLock)
            {
                iDeviceListAdvanced.Add(aDevice);
                iDeviceListAdvanced.Sort();
                int index = iDeviceListAdvanced.IndexOf(aDevice);

                AddDevice(aDevice, index);
                aDevice.Changed += Changed;
                aDevice.ProgressChanged += Progress;
                aDevice.MessageChanged += Message;
            }
        }

        private void AddDevice(CpDeviceAdvanced aDevice, int aIndex)
        {
            Console.WriteLine(aDevice.MacAddress);
            JsonObject info = new JsonObject();
            info.Add("Index", new JsonValueInt(aIndex));
            info.Add("MacAddress", new JsonValueString(aDevice.MacAddress));
            info.Add("ProductId", new JsonValueString(aDevice.ProductId));
            info.Add("Description", aDevice.Json);
            info.Add("Progress", new JsonValueUint(aDevice.Progress));
            info.Add("Message", new JsonValueString(aDevice.Message));
            Send("AdvancedDeviceAdded", info);
        }

        private void Removed(CpDeviceAdvancedList aList, CpDeviceAdvanced aDevice)
        {
            lock (iLock)
            {
                RemoveDevice(aDevice);

                aDevice.Changed -= Changed;
                aDevice.ProgressChanged -= Progress;
                aDevice.MessageChanged -= Message;
                iDeviceListAdvanced.Remove(aDevice);
            }
        }

        private void RemoveDevice(CpDeviceAdvanced aDevice)
        {
            Send("AdvancedDeviceRemoved", aDevice.MacAddress);
        }

        private void Changed(object sender, EventArgs e)
        {
            lock (iLock)
            {
                CpDeviceAdvanced d = sender as CpDeviceAdvanced;

                if (d != null)
                {
                    int oldIndex = iDeviceListAdvanced.IndexOf(d);
                    iDeviceListAdvanced.Sort();
                    int newIndex = iDeviceListAdvanced.IndexOf(d);

                    if (oldIndex == newIndex)
                    {
                        JsonObject info = new JsonObject();
                        info.Add("MacAddress", new JsonValueString(d.MacAddress));
                        info.Add("Description", d.Json);
                        info.Add("Progress", new JsonValueUint(d.Progress));
                        info.Add("Message", new JsonValueString(d.Message));
                        Send("AdvancedDeviceChanged", info);
                    }
                    else
                    {
                        RemoveDevice(d);
                        AddDevice(d, newIndex);
                    }
                }
            }
        }

        private void Progress(object sender, EventArgs e)
        {
            CpDeviceAdvanced device = sender as CpDeviceAdvanced;

            JsonObject info = new JsonObject();
            info.Add("MacAddress", new JsonValueString(device.MacAddress));
            info.Add("Progress", new JsonValueUint(device.Progress));
            Send("AdvancedDeviceProgress", info);
        }

        private void Message(object sender, EventArgs e)
        {
            CpDeviceAdvanced device = sender as CpDeviceAdvanced;

            JsonObject info = new JsonObject();
            info.Add("MacAddress", new JsonValueString(device.MacAddress));
            info.Add("Message", new JsonValueString(device.Message));
            Send("AdvancedDeviceMessage", info);
        }

        private object iLock;
        private IInvoker iInvoker;

        private CpDeviceAdvancedList iAdvancedList;
        private List<CpDeviceAdvanced> iDeviceListAdvanced;
    }

    public class SettingsPageBasic : PageBase
    {
        public SettingsPageBasic(IInvoker aInvoker, Preferences aPreferences, string aId, string aViewId)
            : base(aId, aViewId)
        {
            iLock = new object();

            iInvoker = aInvoker;

            iPreferences = aPreferences;
            iPreferences.EventFirmwareBetaChanged += HandleFirmwareBetaChanged;
            iPreferences.EventSendDsCrashDataChanged += HandleEventSendDsCrashDataChanged;
            iPreferences.EventUsageDataChanged += HandleEventUsageDataChanged;
        }

        public override void Dispose()
        {
            base.Dispose();

            iPreferences.EventFirmwareBetaChanged -= HandleFirmwareBetaChanged;
            iPreferences.EventSendDsCrashDataChanged -= HandleEventSendDsCrashDataChanged;
            iPreferences.EventUsageDataChanged -= HandleEventUsageDataChanged;
        }

        public virtual void SetUpdating(bool aUpdating)
        {
            lock (iLock)
            {
                iUpdating = aUpdating;
                Send("EnableFirmwareBeta", !iUpdating);
            }
        }

        private delegate void DOnActivated(Session aSession);
        protected override void OnActivated(Session aSession)
        {
            if (iInvoker.TryBeginInvoke(new DOnActivated(OnActivated), aSession))
                return;

            base.OnActivated(aSession);

            lock(iLock)
            {
                aSession.Send("EnableFirmwareBeta", !iUpdating);
                aSession.Send("SetFirmwareBeta", iPreferences.FirmwareBeta);
                aSession.Send("SetSendDsCrashData", iPreferences.SendDsCrashData);
                aSession.Send("SetUsageDataLogging", iPreferences.UsageData);
            }
        }

        private delegate void DOnReceive(Session aSession, string aName, string aValue);
        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            if (iInvoker.TryBeginInvoke(new DOnReceive(OnReceive), aSession, aName, aValue))
                return;

            base.OnReceive(aSession, aName, aValue);

            if(aName == "FirmwareBeta")
            {
                iPreferences.FirmwareBeta = bool.Parse(aValue);
            }

            if (aName == "SendDsCrashData")
            {
                iPreferences.SendDsCrashData = bool.Parse(aValue);
            }

            if (aName == "UsageDataLogging")
            {
                iPreferences.UsageData = bool.Parse(aValue);
            }
        }

        private void HandleFirmwareBetaChanged(object sender, EventArgs e)
        {
            Send("SetFirmwareBeta", iPreferences.FirmwareBeta);
        }

        void HandleEventSendDsCrashDataChanged(object sender, EventArgs e)
        {
            Send("SetSendDsCrashData", iPreferences.SendDsCrashData);
        }

        void HandleEventUsageDataChanged(object sender, EventArgs e)
        {
            Send("SetUsageDataLogging", iPreferences.UsageData);
        }
        
        protected object iLock;

        protected bool iUpdating;

        private Preferences iPreferences;

        private IInvoker iInvoker;
    }

    public class AboutPage : PageBase
    {
        public AboutPage(IInvoker aInvoker, IHelper aHelper, string aId, string aViewId)
            : base(aId, aViewId)
        {
            iInvoker = aInvoker;
            iHelper = aHelper;
        }

        private delegate void DOnActivated(Session aSession);
        protected override void OnActivated(Session aSession)
        {
            if (iInvoker.TryBeginInvoke(new DOnActivated(OnActivated), aSession))
                return;

            base.OnActivated(aSession);

            JsonObject info = new JsonObject();
            info.Add("Title", new JsonValueString(iHelper.Product));
            info.Add("Version", new JsonValueString(string.Format("Version {0} {1}", iHelper.Version, iHelper.Family)));
            info.Add("Copyright", new JsonValueString(string.Format("{0} {1}", iHelper.Copyright, iHelper.Company)));

            aSession.Send("Activated", info);
        }

        private delegate void DOnReceive(Session aSession, string aName, string aValue);
        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            if (iInvoker.TryBeginInvoke(new DOnReceive(OnReceive), aSession, aName, aValue))
                return;

            base.OnReceive(aSession, aName, aValue);
        }

        private IInvoker iInvoker;
        private IHelper iHelper;
    }
}



