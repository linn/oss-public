using System;
using System.Collections.Generic;

using OpenHome.Xapp;

namespace Linn.Konfig
{
    public abstract class CpDeviceDiagnosticsItem : IDisposable, IComparable<CpDeviceDiagnosticsItem>
    {
        public enum EDiagnosticsType
        {
            eCrash,
            eProxyPreamp,
            eProxyCdPlayer
        }

        public CpDeviceDiagnosticsItem(CpDeviceDiagnostics aDevice)
        {
            iDevice = aDevice;
            iDevice.Changed += HandleChanged;
        }

        public void Dispose()
        {
            iDevice.Changed -= HandleChanged;
            iDevice = null;
        }

        public CpDeviceDiagnostics Device
        {
            get
            {
                return iDevice;
            }
        }

        public abstract string Id { get; }
        public abstract EDiagnosticsType Type { get; }

        public string Udn
        {
            get
            {
                return iDevice.Udn;
            }
        }

        public string Fullname
        {
            get
            {
                return iDevice.Fullname;
            }
        }

        public virtual JsonObject Json
        {
            get
            {
                return iDevice.Json;
            }
        }

        public abstract void Send(DebugReport aReport);
        public abstract void Ignore();

        public int CompareTo(CpDeviceDiagnosticsItem aDevice)
        {
            if (Udn == aDevice.Udn)
            {
                if (Type != aDevice.Type)
                {
                    return (Type < aDevice.Type) ? -1 : 1;
                }
            }

            int cmp = string.Compare(Fullname, aDevice.Fullname);
            if (cmp == 0)
            {
                if (Type != aDevice.Type)
                {
                    return (Type < aDevice.Type) ? -1 : 1;
                }
            }

            return cmp;
        }

        public event EventHandler<EventArgs> Changed;

        private void HandleChanged(object sender, EventArgs e)
        {
            if(Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        protected CpDeviceDiagnostics iDevice;
    }

    public class CpDeviceDiagnosticsProxyPreamp : CpDeviceDiagnosticsItem
    {
        public CpDeviceDiagnosticsProxyPreamp(CpDeviceDiagnostics aDevice)
            : base(aDevice)
        {
        }

        public bool PreampProblem
        {
            get
            {
                return iDevice.PreampProblem;
            }
        }

        #region implemented abstract members of Linn.Konfig.CpDeviceDiagnosticsItem
        public override string Id
        {
            get
            {
                return string.Format("ProxyPreamp-{0}", iDevice.Udn);
            }
        }

        public override EDiagnosticsType Type
        {
            get
            {
                return EDiagnosticsType.eProxyPreamp;
            }
        }

        public override JsonObject Json
        {
            get
            {
                JsonObject info = base.Json;
                info.Add("PreampProblem", new JsonValueBool(iDevice.PreampProblem));
                return info;
            }
        }
        #endregion

        public override void Send(DebugReport aReport)
        {
        }

        public override void Ignore()
        {
        }
    }

    public class CpDeviceDiagnosticsProxyCdPlayer : CpDeviceDiagnosticsItem
    {
        public CpDeviceDiagnosticsProxyCdPlayer(CpDeviceDiagnostics aDevice)
            : base(aDevice)
        {
        }

        public bool CdPlayerProblem
        {
            get
            {
                return iDevice.CdPlayerProblem;
            }
        }

        #region implemented abstract members of Linn.Konfig.CpDeviceDiagnosticsItem
        public override string Id
        {
            get
            {
                return string.Format("ProxyCdPlayer-{0}", iDevice.Udn);
            }
        }

        public override EDiagnosticsType Type
        {
            get
            {
                return EDiagnosticsType.eProxyCdPlayer;
            }
        }

        public override JsonObject Json
        {
            get
            {
                JsonObject info = base.Json;
                info.Add("CdPlayerProblem", new JsonValueBool(iDevice.CdPlayerProblem));
                return info;
            }
        }
        #endregion

        public override void Send(DebugReport aReport)
        {
        }

        public override void Ignore()
        {
        }
    }

    public class CpDeviceDiagnosticsReport : CpDeviceDiagnosticsItem
    {
        public CpDeviceDiagnosticsReport(CpDeviceDiagnostics aDevice)
            : base(aDevice)
        {
        }

        public CpDeviceDiagnostics.ECrashDataStatus CrashDataStatus
        {
            get
            {
                return iDevice.CrashDataStatus;
            }
        }

        public byte[] CrashData
        {
            get
            {
                return iDevice.CrashData;
            }
        }

        #region implemented abstract members of Linn.Konfig.CpDeviceDiagnosticsItem
        public override string Id
        {
            get
            {
                return string.Format("Crash-{0}", iDevice.Udn);
            }
        }

        public override EDiagnosticsType Type
        {
            get
            {
                return EDiagnosticsType.eCrash;
            }
        }

        public override JsonObject Json
        {
            get
            {
                JsonObject info = base.Json;
                info.Add("CrashData", new JsonValueBool(iDevice.CrashDataStatus == CpDeviceDiagnostics.ECrashDataStatus.eAvailable));
                return info;
            }
        }
        #endregion

        public override void Send(DebugReport aReport)
        {
            byte[] crashData = iDevice.CrashData;
            if (crashData != null)
            {
                string response;
                bool failed = aReport.Post(string.Format("{0}_{1}", iDevice.Model.Replace(" ", ""), iDevice.Udn), string.Empty, crashData, iDevice.ElfFingerprint, out response);

                UserLog.WriteLine(response);

                if (!failed)
                {
                    iDevice.ClearCrashData();
                }
            }
        }

        public override void Ignore()
        {
            iDevice.ClearCrashData();
        }
    }

    public class CpDeviceDiagnosticsReportList : IDisposable
    {
        public CpDeviceDiagnosticsReportList(DebugReport aReport, ChangeHandler aAdded, ChangeHandler aRemoved)
        {
            iLock = new object();
            iDisposed = false;

            iReport = aReport;

            iAdded = aAdded;
            iRemoved = aRemoved;

            iDeviceListDiagnosticsItem = new List<CpDeviceDiagnosticsItem>();

            iDeviceList = new CpDeviceDiagnosticsList(Added, Removed);
        }

        public void Dispose()
        {
            iDeviceList.Dispose();
            iDeviceList = null;

            lock(iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceDiagnosticsReportList");
                }

                foreach(CpDeviceDiagnosticsItem d in iDeviceListDiagnosticsItem)
                {
                    d.Changed -= HandleChanged;
                }

                iDeviceListDiagnosticsItem.Clear();

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            iDeviceList.Refresh();
        }

        public void Send(string aId)
        {
            lock(iLock)
            {
                foreach(CpDeviceDiagnosticsItem d in iDeviceListDiagnosticsItem)
                {
                    if(d.Id == aId)
                    {
                        d.Send(iReport);
                        break;
                    }
                }
            }
        }

        public void Ignore(string aId)
        {
            lock(iLock)
            {
                foreach(CpDeviceDiagnosticsItem d in iDeviceListDiagnosticsItem)
                {
                    if(d.Id == aId)
                    {
                        d.Ignore();
                        break;
                    }
                }
            }
        }

        public void SendAll()
        {
            List<CpDeviceDiagnosticsItem> list;
            lock(iLock)
            {
                list = new List<CpDeviceDiagnosticsItem>(iDeviceListDiagnosticsItem);
            }

            foreach(CpDeviceDiagnosticsItem d in list)
            {
                d.Send(iReport);
            }
        }

        public void IgnoreAll()
        {
            List<CpDeviceDiagnosticsItem> list;
            lock(iLock)
            {
                list = new List<CpDeviceDiagnosticsItem>(iDeviceListDiagnosticsItem);
            }

            foreach(CpDeviceDiagnosticsItem d in list)
            {
                d.Ignore();
            }
        }

        public delegate void ChangeHandler(CpDeviceDiagnosticsReportList aList, CpDeviceDiagnosticsItem aDevice);

        private void Added(CpDeviceDiagnosticsList aList, CpDeviceDiagnostics aDevice)
        {
            List<CpDeviceDiagnosticsItem> devices = new List<CpDeviceDiagnosticsItem>();

            lock(iLock)
            {
                if(iDisposed)
                {
                    return;
                }
                
                CpDeviceDiagnosticsItem device;
                if(aDevice.CrashDataStatus == CpDeviceDiagnostics.ECrashDataStatus.eAvailable)
                {
                    UserLog.WriteLine(DateTime.Now + ": CpDeviceDiagnosticsReportList: DeviceDiagnosticsReport+             Udn{" + aDevice.Udn + "}");

                    device = new CpDeviceDiagnosticsReport(aDevice);
                    device.Changed += HandleChanged;

                    devices.Add(device);
                    iDeviceListDiagnosticsItem.Add(device);
                }

                if(aDevice.CdPlayerProblem)
                {
                    UserLog.WriteLine(DateTime.Now + ": CpDeviceDiagnosticsReportList: DeviceDiagnosticsProxyCdPlayer+             Udn{" + aDevice.Udn + "}");

                    device = new CpDeviceDiagnosticsProxyCdPlayer(aDevice);
                    device.Changed += HandleChanged;

                    devices.Add(device);
                    iDeviceListDiagnosticsItem.Add(device);
                }

                if(aDevice.PreampProblem)
                {
                    UserLog.WriteLine(DateTime.Now + ": CpDeviceDiagnosticsReportList: DeviceDiagnosticsProxyPreamp+             Udn{" + aDevice.Udn + "}");

                    device = new CpDeviceDiagnosticsProxyPreamp(aDevice);
                    device.Changed += HandleChanged;

                    devices.Add(device);
                    iDeviceListDiagnosticsItem.Add(device);
                }
            }

            foreach(CpDeviceDiagnosticsItem d in devices)
            {
                if(iAdded != null)
                {
                    iAdded(this, d);
                }
            }
        }

        private void Removed(CpDeviceDiagnosticsList aList, CpDeviceDiagnostics aDevice)
        {
            List<CpDeviceDiagnosticsItem> devices = new List<CpDeviceDiagnosticsItem>();

            lock(iLock)
            {
                if(iDisposed)
                {
                    return;
                }

                foreach(CpDeviceDiagnosticsItem d in iDeviceListDiagnosticsItem)
                {
                    if(d.Udn == aDevice.Udn)
                    {
                        UserLog.WriteLine(DateTime.Now + ": CpDeviceDiagnosticsReportList: DeviceDiagnosticsItem-             Udn{" + aDevice.Udn + "}");

                        devices.Add(d);
                        d.Changed -= HandleChanged;
                    }
                }

                foreach (CpDeviceDiagnosticsItem d in devices)
                {
                    iDeviceListDiagnosticsItem.Remove(d);
                }
            }

            foreach(CpDeviceDiagnosticsItem d in devices)
            {
                if(iRemoved != null)
                {
                    iRemoved(this, d);
                }

                d.Dispose();
            }
        }

        private void HandleChanged(object sender, EventArgs e)
        {
            CpDeviceDiagnosticsItem device = sender as CpDeviceDiagnosticsItem;
            CpDeviceDiagnosticsItem removedDevice = null;

            lock(iLock)
            {
                foreach(CpDeviceDiagnosticsItem d in iDeviceListDiagnosticsItem)
                {
                    if(d.Udn == device.Udn)
                    {
                        if(device is CpDeviceDiagnosticsReport && device.Device.CrashDataStatus == CpDeviceDiagnostics.ECrashDataStatus.eAvailable ||
                           device is CpDeviceDiagnosticsProxyCdPlayer && device.Device.CdPlayerProblem ||
                           device is CpDeviceDiagnosticsProxyPreamp && device.Device.PreampProblem)
                        {
                            return;
                        }
                        else
                        {
                            removedDevice = d;
                            d.Changed -= HandleChanged;

                            iDeviceListDiagnosticsItem.Remove(removedDevice);

                            break;
                        }
                    }
                }
            }

            if(removedDevice != null)
            {
                if(iRemoved != null)
                {
                    iRemoved(this, removedDevice);
                }

                removedDevice.Dispose();
            }
        }

        private object iLock;
        private bool iDisposed;

        private DebugReport iReport;

        private ChangeHandler iAdded;
        private ChangeHandler iRemoved;

        private CpDeviceDiagnosticsList iDeviceList;
        private List<CpDeviceDiagnosticsItem> iDeviceListDiagnosticsItem;
    }
}

