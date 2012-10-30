using System;
using System.Collections.Generic;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;

using OpenHome.Xapp;

namespace Linn.Konfig
{
    public delegate void DiagnosticsHandler(CpDeviceDiagnostics aDevice);

    public class CpDeviceDiagnostics : IDisposable
    {
        public enum ECrashDataStatus
        {
            eEmpty,
            eAvailable
        }

        public CpDeviceDiagnostics(CpDevice aDevice, DiagnosticsHandler aHandler)
        {
            try
            {
                iDevice = aDevice;
                aDevice.AddRef();

                iHandler = aHandler;

                iLock = new object();
                iResponses = 0;

                iDiagnosticsService = new CpProxyLinnCoUkDiagnostics1(aDevice);
                iDiagnosticsService.BeginCrashDataStatus(HandleCrashDataStatus);
                iDiagnosticsService.BeginElfFingerprint(HandleElfFingerprint);
                // remove checking of RS232 connections
                //iDiagnosticsService.BeginDiagnosticTest("help", string.Empty, HandleHelp);

                iProductService = new CpProxyAvOpenhomeOrgProduct1(aDevice);
                iProductService.SetPropertyInitialEvent(InitialEvent);
                iProductService.SetPropertyProductRoomChanged(ProductRoomChanged);
                iProductService.SetPropertyProductNameChanged(ProductNameChanged);
                iProductService.SetPropertyProductImageUriChanged(ProductImageUriChanged);
                iProductService.SetPropertyModelNameChanged(ModelNameChanged);
                iProductService.Subscribe();
            }
            catch
            {
                aDevice.RemoveRef();
                throw;
            }
        }

        public void Dispose()
        {
            iDiagnosticsService.Dispose();
            iDiagnosticsService = null;

            iProductService.Dispose();
            iProductService = null;

            iDevice.RemoveRef();
            iDevice = null;
        }

        public void ClearCrashData()
        {
            iCrashDataStatus = ECrashDataStatus.eEmpty;

            try
            {
                iDiagnosticsService.SyncCrashDataClear();
            }
            catch (ProxyError e)
            {
                UserLog.WriteLine("ClearCrashData: " + iDevice.Udn() + " (" + e.Message + ")");
            }

            if(Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        public string Udn
        {
            get
            {
                return iDevice.Udn();
            }
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
                return iModel;
            }
        }

        public ECrashDataStatus CrashDataStatus
        {
            get
            {
                return iCrashDataStatus;
            }
        }

        public byte[] CrashData
        {
            get
            {
                try
                {
                    byte[] crashData;
                    iDiagnosticsService.SyncCrashDataFetch(out crashData);
                    return crashData;
                }
                catch (ProxyError e)
                {
                    UserLog.WriteLine("CrashData: " + iDevice.Udn() + " (" + e.Message + ")");
                    return null;
                }
            }
        }

        public string ElfFingerprint
        {
            get
            {
                return iElfFingerprint;
            }
        }

        public bool CdPlayerProblem
        {
            get
            {
                return iCdPlayerProblem;
            }
        }

        public bool PreampProblem
        {
            get
            {
                return iPreampProblem;
            }
        }

        public JsonObject Json
        {
            get
            {
                JsonObject info = new JsonObject();
                info.Add("Room", new JsonValueString(iRoom));
                if(iModel != iName)
                {
                    info.Add("Name", new JsonValueString(string.Format(" ({0})", iName)));
                }
                else
                {
                    info.Add("Name", new JsonValueString(string.Empty));
                }
                info.Add("Model", new JsonValueString(iModel));
                info.Add("ImageUri", new JsonValueString(iProductImageUri));
                return info;
            }
        }

        public EventHandler<EventArgs> Changed;

        private void HandleCrashDataStatus(IntPtr aAsyncHandle)
        {
            try
            {
                string status;
                iDiagnosticsService.EndCrashDataStatus(aAsyncHandle, out status);

                if (status == "available")
                {
                    iCrashDataStatus = ECrashDataStatus.eAvailable;
                }
                else
                {
                    iCrashDataStatus = ECrashDataStatus.eEmpty;
                }

                ++iResponses;

                if (iResponses == kNumAsyncActions)
                {
                    iHandler(this);
                }
            }
            catch (ProxyError) { }
        }

        private void HandleElfFingerprint(IntPtr aAsyncHandle)
        {
            try
            {
                iDiagnosticsService.EndElfFingerprint(aAsyncHandle, out iElfFingerprint);

                ++iResponses;

                if(iResponses == kNumAsyncActions)
                {
                    iHandler(this);
                }
            }
            catch(ProxyError) { }
        }

        private void HandleHelp(IntPtr aAsyncHandle)
        {
            try
            {
                string info;
                bool result;
                iDiagnosticsService.EndDiagnosticTest(aAsyncHandle, out info, out result);

                if(info.Contains("discplayerconnection"))
                {
                    iDiagnosticsService.BeginDiagnosticTest("discplayerconnection", string.Empty, HandleDiscPlayerConnection);
                }

                if(info.Contains("kontrolproductconnection"))
                {
                    iDiagnosticsService.BeginDiagnosticTest("kontrolproductconnection", string.Empty, HandlePreampConnection);
                }
            }
            catch(ProxyError) { }
        }

        private void HandleDiscPlayerConnection(IntPtr aAsyncHandle)
        {
            try
            {
                string info;
                bool result;
                iDiagnosticsService.EndDiagnosticTest(aAsyncHandle, out info, out result);
                if(result || info == kCdPlayerConnectionNotConfigured)
                {
                    iCdPlayerProblem = false;
                }
                else
                {
                    iCdPlayerProblem = true;
                }

                ++iResponses;

                if(iResponses == kNumAsyncActions)
                {
                    iHandler(this);
                }
            }
            catch(ProxyError) { }
        }

        private void HandlePreampConnection(IntPtr aAsyncHandle)
        {
            try
            {
                string info;
                bool result;
                iDiagnosticsService.EndDiagnosticTest(aAsyncHandle, out info, out result);
                if(result || info == kPreampConnectionNotSupported || info == kPreampConnectionNotConfigured)
                {
                    iPreampProblem = false;
                }
                else
                {
                    iPreampProblem = true;
                }

                ++iResponses;

                if(iResponses == kNumAsyncActions)
                {
                    iHandler(this);
                }
            }
            catch(ProxyError) { }
        }

        private void InitialEvent()
        {
            uint responses = 0;
            lock(iLock)
            {
                iRoom = iProductService.PropertyProductRoom();
                iName = iProductService.PropertyProductName();
                iModel = iProductService.PropertyModelName();
                iProductImageUri = iProductService.PropertyProductImageUri();

                ++iResponses;

                responses = iResponses;
            }

            if(responses == kNumAsyncActions)
            {
                iHandler(this);
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

        private void ModelNameChanged()
        {
            lock(iLock)
            {
                iModel = iProductService.PropertyModelName();
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

        private static readonly uint kNumAsyncActions = 3; // 5; when tests for premap and cd are included
        private static readonly string  kPreampConnectionNotSupported = "Invalid setup detected\r\n";
        private static readonly string  kPreampConnectionNotConfigured = "No kontrol product connection has been configured\r\n";
        private static readonly string  kCdPlayerConnectionNotConfigured = "No disc player connection has been configured\r\n";

        private object iLock;

        private CpDevice iDevice;
        private DiagnosticsHandler iHandler;

        private CpProxyLinnCoUkDiagnostics1 iDiagnosticsService;
        private ECrashDataStatus iCrashDataStatus;
        private string iElfFingerprint;
        private bool iCdPlayerProblem;
        private bool iPreampProblem;
        private uint iResponses;

        private CpProxyAvOpenhomeOrgProduct1 iProductService;
        private string iRoom;
        private string iName;
        private string iModel;
        private string iProductImageUri;
    }

    public class CpDeviceDiagnosticsList : IDisposable
    {
        public CpDeviceDiagnosticsList(ChangeHandler aAdded, ChangeHandler aRemoved)
        {
            iLock = new object();
            iDisposed = false;

            iAdded = aAdded;
            iRemoved = aRemoved;

            iDeviceListDiagnostics = new List<CpDeviceDiagnostics>();
            iDeviceListPending = new List<CpDeviceDiagnostics>();
            iDeviceList = new CpDeviceListUpnpServiceType("linn.co.uk", "Diagnostics", 1, Added, Removed);
        }

        public void Dispose()
        {
            iDeviceList.Dispose();
            iDeviceList = null;

            lock(iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceDiagnosticsList");
                }

                foreach (CpDeviceDiagnostics d in iDeviceListPending)
                {
                    d.Dispose();
                }

                foreach(CpDeviceDiagnostics d in iDeviceListDiagnostics)
                {
                    d.Dispose();
                }

                iDeviceListPending.Clear();
                iDeviceListDiagnostics.Clear();

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            lock(iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceDiagnosticsList");
                }

                iDeviceList.Refresh();
            }
        }

        public delegate void ChangeHandler(CpDeviceDiagnosticsList aList, CpDeviceDiagnostics aDevice);

        private void Added(CpDeviceList aList, CpDevice aDevice)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceDiagnosticsList: Device+             Udn{" + aDevice.Udn() + "}");

            string version;
            if (!aDevice.GetAttribute("Upnp.Service.av-openhome-org.Product", out version))
            {
                return;
            }

            CpDeviceDiagnostics device = new CpDeviceDiagnostics(aDevice, DiagnosticsAdded);

            lock(iLock)
            {
                if(iDisposed)
                {
                    return;
                }

                // check to see if device has been added immediately
                if (!iDeviceListDiagnostics.Contains(device))
                {
                    iDeviceListPending.Add(device);
                }
            }
        }

        private void DiagnosticsAdded(CpDeviceDiagnostics aDevice)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceDiagnosticsList: DeviceDiagnostics+             Udn{" + aDevice.Udn + "}");

            lock(iLock)
            {
                if (iDisposed)
                {
                    aDevice.Dispose();

                    return;
                }

                iDeviceListPending.Remove(aDevice);
                iDeviceListDiagnostics.Add(aDevice);
            }

            if(iAdded != null)
            {
                iAdded(this, aDevice);
            }
        }

        private void Removed(CpDeviceList aList, CpDevice aDevice)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceDiagnosticsList: Device-             Udn{" + aDevice.Udn() + "}");

            CpDeviceDiagnostics device = null;

            lock(iLock)
            {
                if(iDisposed)
                {
                    return;
                }

                foreach (CpDeviceDiagnostics d in iDeviceListPending)
                {
                    if (d.Udn == aDevice.Udn())
                    {
                        iDeviceListPending.Remove(d);

                        d.Dispose();

                        return;
                    }
                }

                foreach(CpDeviceDiagnostics d in iDeviceListDiagnostics)
                {
                    if(d.Udn == aDevice.Udn())
                    {
                        device = d;

                        iDeviceListDiagnostics.Remove(device);

                        break;
                    }
                }
            }

            if(device != null)
            {
                if(iRemoved != null)
                {
                    iRemoved(this, device);
                }

                device.Dispose();
            }
        }

        private object iLock;
        private bool iDisposed;

        private ChangeHandler iAdded;
        private ChangeHandler iRemoved;

        private CpDeviceListUpnpServiceType iDeviceList;

        private List<CpDeviceDiagnostics> iDeviceListPending;
        private List<CpDeviceDiagnostics> iDeviceListDiagnostics;
    }
}

