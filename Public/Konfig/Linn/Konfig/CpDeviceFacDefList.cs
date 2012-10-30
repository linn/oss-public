using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using OpenHome.Net.Core;
using OpenHome.Net.ControlPoint;

using OpenHome.Xapp;

using Linn.ProductSupport.Flash;

namespace Linn.Konfig
{
    public class CpDeviceFacDef : ICpDeviceReprogrammable, IDisposable, IComparable<CpDeviceUpdate>
    {
        public class FactoryDefaulterHelper : IConsole
        {
            public FactoryDefaulterHelper(IPAddress aAdapterAddress, string aUglyname, ChangedHandler aHandler)
            {
                iHandler = aHandler;

                iMessage = "Restoring To Factory Defaults...";
                iProgress = 0;
                iRebooting = false;
                iSettingDefaults = false;
                iCount = 0;
                iCountMax = kSettingDefaultsCount + kRebootCount;

                if (iHandler != null)
                {
                    iHandler(iMessage, iProgress);
                }

                FactoryDefaulter defaulter = new FactoryDefaulter(aAdapterAddress, this, aUglyname);
                defaulter.NoExec = false;
                defaulter.Wait = true; // wait to discover device after reprogramming

                string failMessage;
                bool result = defaulter.Execute(out failMessage);

                defaulter.Close();

                iProgress = 100;

                if (!result)
                {
                    iMessage = "Resetting To Factory Defaults Failed";
                }

                if (iHandler != null)
                {
                    iHandler(iMessage, iProgress);
                }
            }

            #region IConsole implementation
            public void Newline()
            {
            }

            public void Title(string aMessage)
            {
                if (aMessage == kStartSettingDefaults)
                {
                    iRebooting = false;
                    iSettingDefaults = true;
                    iCount = 0;    // ensure we are at the correct count
                }
                else if (aMessage == kEndSettingDefaults)
                {
                    iRebooting = true;
                    iCount = iCountMax - kRebootCount;    // ensure we are at the correct count
                }
                else if (aMessage.EndsWith(kSuccess))
                {
                    iRebooting = false;
                    iCount += kRebootCount;
                }

                UpdateProgress();
            }

            public void Write(string aMessage)
            {
            }

            public void ProgressOpen(int aMax)
            {
                iLastValue = 0;
            }

            public void ProgressSetValue(int aValue)
            {
                if (!iRebooting)
                {
                    if (iSettingDefaults)
                    {
                        if (iCount < iCountMax - kRebootCount)
                        {
                            iCount += (uint)(aValue - iLastValue);
                            UpdateProgress();
                        }
                    }
                }
                iLastValue = aValue;
            }

            public void ProgressClose()
            {
            }
            #endregion

            private void UpdateProgress()
            {
                uint oldProgress = iProgress;
                iProgress = (uint)((iCount * 100) / iCountMax);

                if (iProgress != oldProgress)
                {
                    if (iHandler != null)
                    {
                        iHandler(iMessage, iProgress);
                    }
                }
            }

            private const uint kSettingDefaultsCount = 1361494;
            private const uint kRebootCount = 13753;

            private static readonly string kStartSettingDefaults = "Collect main rom directory";
            private static readonly string kEndSettingDefaults = "Reboot to main";
            private static readonly string kSuccess = "set to factory defaults successfully";

            private ChangedHandler iHandler;

            private uint iProgress;
            private string iMessage;

            private bool iRebooting;
            private bool iSettingDefaults;
            private uint iCount;
            private uint iCountMax;
            private int iLastValue;
        }

        public CpDeviceFacDef(CpDeviceReprogrammable aDevice, IPAddress aAdapterAddress, IUpdateListener aUpdateListener)
        {
            iLock = new object();
            iAdapterAddress = aAdapterAddress;

            iDevice = aDevice;
            iDevice.Changed += HandleChanged;

            iUpdateListener = aUpdateListener;

            iUglyname = string.Format("linn-{0}", aDevice.MacAddress.Replace(":", "").Substring(6, 6));
        }

        public void Dispose()
        {
            iDevice.WaitForUpdateToComplete();

            iDevice.Changed -= HandleChanged;
            iDevice = null;
        }

        public void SetAdapterAddress(IPAddress aAdapterAddress)
        {
            lock (iLock)
            {
                iAdapterAddress = aAdapterAddress;
            }
        }

        public void FactoryDefaults()
        {
            lock (iLock)
            {
                if (!iDevice.Updating)
                {
                    iThread = new Thread(ApplyFactoryDefaults);
                    iThread.Name = string.Format("{0} Factory Defaulter", MacAddress);

                    iThread.Start();

                    iDevice.Updating = true;
                    iUpdateListener.SetUpdating(true);
                }
            }
        }

        public uint Progress
        {
            get
            {
                return iProgress;
            }
        }

        public string Message
        {
            get
            {
                return iMessage;
            }
        }

        public string Fullname
        {
            get
            {
                return iDevice.Fullname;
            }
        }

        public CpDeviceReprogrammable.EStatus Status
        {
            get
            {
                return iDevice.Status;
            }
        }

        public bool Updating
        {
            get
            {
                return iDevice.Updating;
            }
        }

        public int CompareTo(CpDeviceUpdate aDevice)
        {
            if (aDevice.Udn == Udn)
            {
                return 0;
            }

            int cmp = string.Compare(Fullname, aDevice.Fullname);
            if (cmp == 0)
            {
                return Udn.CompareTo(aDevice.Udn);
            }

            return cmp;
        }

        public EventHandler<EventArgs> Changed;
        public EventHandler<EventArgs> ProgressChanged;
        public EventHandler<EventArgs> MessageChanged;

        public delegate void ChangedHandler(string aMessage, uint aProgress);

        #region ICpDeviceReprogrammable implementation
        public CpDevice Device
        {
            get
            {
                return iDevice.Device;
            }
        }

        public string Udn
        {
            get
            {
                return iDevice.Udn;
            }
        }

        public string MacAddress
        {
            get
            {
                return iDevice.MacAddress;
            }
        }

        public string Room
        {
            get
            {
                return iDevice.Room;
            }
        }

        public string Name
        {
            get
            {
                return iDevice.Name;
            }
        }

        public string Model
        {
            get
            {
                return iDevice.Model;
            }
        }

        public ReadOnlyCollection<string> PcbNumberList
        {
            get
            {
                return iDevice.PcbNumberList;
            }
        }

        public string SoftwareVersion
        {
            get
            {
                return iDevice.SoftwareVersion;
            }
        }

        public string ProductId
        {
            get
            {
                return iDevice.ProductId;
            }
        }

        public OpenHome.Xapp.JsonObject Json
        {
            get
            {
                return iDevice.Json;
            }
        }
        #endregion

        private void ApplyFactoryDefaults()
        {
            HandleChanged(string.Empty, 0);

            IPAddress adapter = null;

            lock (iLock)
            {
                adapter = iAdapterAddress;
            }

            if (adapter != null)
            {
                new FactoryDefaulterHelper(adapter, iUglyname, HandleChanged);
            }
            else
            {
                HandleChanged("Restore To Factory Defaults Failed", 100);
            }

            iDevice.Updating = false;
            iUpdateListener.SetUpdating(false);
        }

        private void HandleChanged(string aMessage, uint aProgress)
        {
            if (aMessage != iMessage)
            {
                iMessage = aMessage;

                if (MessageChanged != null)
                {
                    MessageChanged(this, EventArgs.Empty);
                }
            }

            if (aProgress != iProgress)
            {
                iProgress = aProgress;

                if (ProgressChanged != null)
                {
                    ProgressChanged(this, EventArgs.Empty);
                }
            }
        }

        private void HandleChanged(object sender, EventArgs e)
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        private object iLock;

        private CpDeviceReprogrammable iDevice;

        private string iMessage;
        private uint iProgress;

        private string iUglyname;
        private Thread iThread;
        private IPAddress iAdapterAddress;
        private IUpdateListener iUpdateListener;
    }

    public class CpDeviceFacDefList : IDisposable
    {
        public CpDeviceFacDefList(NetworkAdapter aAdapter, IUpdateListener aUpdateListener, CpDeviceReprogramListRepeater aRepeater, ChangedHandler aAdded, ChangedHandler aRemoved)
        {
            iLock = new object();
            iDisposed = false;

            iAdded = aAdded;
            iRemoved = aRemoved;

            iUpdateListener = aUpdateListener;

            //iDeviceList = new CpDeviceReprogramList(Added, Removed);
            iRepeater = aRepeater;
            iDeviceFacDefList = new List<CpDeviceFacDef>();

            SetAdapter(aAdapter);
        }

        public void Dispose()
        {
            //iDeviceList.Dispose();
            //iDeviceList = null;

            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceFacDefList");
                }

                foreach (CpDeviceFacDef d in iDeviceFacDefList)
                {
                    d.Dispose();
                }
                iDeviceFacDefList.Clear();

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            //iDeviceList.Refresh();
            iRepeater.Refresh();
        }

        public void SetAdapter(NetworkAdapter aAdapter)
        {
            lock (iLock)
            {
                if (aAdapter != null)
                {
                    iAdapterAddress = new IPAddress(aAdapter.Address());
                }
                else
                {
                    aAdapter = null;
                }
                foreach (CpDeviceFacDef d in iDeviceFacDefList)
                {
                    d.SetAdapterAddress(iAdapterAddress);
                }
            }
        }

        public void FactoryDefaults(string aMacAddress)
        {
            lock (iLock)
            {
                foreach (CpDeviceFacDef d in iDeviceFacDefList)
                {
                    if (d.MacAddress == aMacAddress)
                    {
                        d.FactoryDefaults();
                        return;
                    }
                }
            }
        }

        public delegate void ChangedHandler(CpDeviceFacDefList aList, CpDeviceFacDef aDevice);

        private void Added(CpDeviceReprogramList aList, CpDeviceReprogrammable aDevice)
        {
            CpDeviceFacDef device = null;

            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                UserLog.WriteLine(DateTime.Now + ": CpDeviceUpdateList: DeviceUpdateable+             MacAddress{" + aDevice.MacAddress + "}");

                device = new CpDeviceFacDef(aDevice, iAdapterAddress, iUpdateListener);
                iDeviceFacDefList.Add(device);

                aDevice.Changed += HandleChanged;
            }

            if (device != null)
            {
                if (iAdded != null)
                {
                    iAdded(this, device);
                }
            }
        }

        private void Removed(CpDeviceReprogramList aList, CpDeviceReprogrammable aDevice)
        {
            CpDeviceFacDef device = null;

            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                foreach (CpDeviceFacDef d in iDeviceFacDefList)
                {
                    if (d.MacAddress == aDevice.MacAddress)
                    {
                        UserLog.WriteLine(DateTime.Now + ": CpDeviceUpdateList: DeviceUpdateable-             MacAddress{" + aDevice.MacAddress + "}");

                        device = d;
                        iDeviceFacDefList.Remove(d);
                        break;
                    }
                }
            }

            if (device != null)
            {
                if (iRemoved != null)
                {
                    iRemoved(this, device);
                }

                device.Dispose();
            }
        }

        private void HandleChanged(object sender, EventArgs e)
        {
            CpDeviceReprogrammable device = sender as CpDeviceReprogrammable;
            CpDeviceFacDef removedDevice = null;
            CpDeviceFacDef addedDevice = null;

            lock (iLock)
            {
                foreach (CpDeviceFacDef d in iDeviceFacDefList)
                {
                    if (d.MacAddress == device.MacAddress)
                    {
                        if (d.Status == CpDeviceReprogrammable.EStatus.eOff && !d.Updating)
                        {
                            UserLog.WriteLine(DateTime.Now + ": CpDeviceUpdateList: DeviceUpdateable-             MacAddress{" + device.MacAddress + "}");

                            removedDevice = d;
                            iDeviceFacDefList.Remove(removedDevice);
                            break;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                if (removedDevice == null && device.Status != CpDeviceReprogrammable.EStatus.eOff)
                {
                    UserLog.WriteLine(DateTime.Now + ": CpDeviceUpdateList: DeviceUpdateable+             MacAddress{" + device.MacAddress + "}");

                    addedDevice = new CpDeviceFacDef(device, iAdapterAddress, iUpdateListener);
                    iDeviceFacDefList.Add(addedDevice);
                }
            }

            if (removedDevice != null)
            {
                if (iRemoved != null)
                {
                    iRemoved(this, removedDevice);
                }

                removedDevice.Dispose();
            }

            if (addedDevice != null)
            {
                if (iAdded != null)
                {
                    iAdded(this, addedDevice);
                }
            }
        }

        private object iLock;
        private bool iDisposed;
        private IPAddress iAdapterAddress;

        private ChangedHandler iAdded;
        private ChangedHandler iRemoved;

        private IUpdateListener iUpdateListener;

        //private CpDeviceReprogramList iDeviceList;
        private CpDeviceReprogramListRepeater iRepeater;
        private List<CpDeviceFacDef> iDeviceFacDefList;
    }
}
