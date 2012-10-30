using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.Core;
using OpenHome.Xapp;

namespace Linn.Konfig
{
    public class CpDeviceUpdate : ICpDeviceReprogrammable, IDisposable, IComparable<CpDeviceUpdate>
    {
        public CpDeviceUpdate(CpDeviceReprogrammable aDevice, Firmware aFirmware, IFirmwareManager aFirmwareManager, IPAddress aAdapterAddress, IUpdateListener aUpdateListener)
        {
            iLock = new object();
            iDisposed = false;
            iAdapterAddress = aAdapterAddress;

            iDevice = aDevice;
            iDevice.Changed += HandleChanged;
            iDevice.ProgressChanged += HandleProgressChanged;
            iDevice.MessageChanged += HandleMessageChanged;

            iUpdateListener = aUpdateListener;

            iUglyname = string.Format("linn-{0}", aDevice.MacAddress.Replace(":", "").Substring(6, 6));

            iFirmware = aFirmware;
            iFirmwareManager = aFirmwareManager;
        }

        public void Dispose()
        {
            iDevice.WaitForUpdateToComplete();

            lock (iLock)
            {
                iDevice.Changed -= HandleChanged;
                iDevice.ProgressChanged -= HandleProgressChanged;
                iDevice.MessageChanged -= HandleMessageChanged;
                iDevice = null;

                iFirmwareManager = null;

                iDisposed = true;
            }
        }

        public void SetAdapterAddress(IPAddress aAdapterAddress)
        {
            lock(iLock)
            {
                iAdapterAddress = aAdapterAddress;
            }
        }

        public void Update()
        {
            lock(iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                if(!iDevice.Updating)
                {
                    iThread = new Thread(ApplyUpdate);
                    iThread.Name = string.Format("{0} Updater", MacAddress);

                    iDevice.Updating = true;
                    iUpdateListener.SetUpdating(true);
                    iFirmwareManager.SetUpdating(true);

                    iThread.Start();
                }
            }
        }

        public uint Progress
        {
            get
            {
                return iDevice.Progress;
            }
        }

        public string Message
        {
            get
            {
                return iDevice.Message;
            }
        }
   
        public string Fullname
        {
            get
            {
                return iDevice.Fullname;
            }
        }

        public string UpdateSoftwareVersion
        {
            get
            {
                return iFirmware.Version;
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
                JsonObject info = iDevice.Json;
                if (iFirmware.ReleaseNotesUri != null)
                {
                    info.Add("ReleaseNotesUri", new JsonValueString(iFirmware.ReleaseNotesUri.OriginalString));
                }
                else
                {
                    info.Add("ReleaseNotesUri", new JsonValueString(string.Empty));
                }
                return info;
            }
        }
        #endregion

        private void ApplyUpdate()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }
            }

            HandleChanged("Downloading firmware...", 0);

            try
            {
                string filename = iFirmwareManager.XmlFilename(iFirmware, HandleDownloadProgress);
                UserLog.WriteLine("Updating software on " + MacAddress + " with " + filename);

                if (filename != null)
                {
                    IPAddress adapter = null;

                    lock (iLock)
                    {
                        adapter = iAdapterAddress;
                    }

                    if (adapter != null)
                    {
                        new FlashSupport.Reprogrammer(adapter, iUglyname, filename, false, HandleChanged);
                    }
                    else
                    {
                        HandleChanged("Software Update Failed", 100);
                    }
                }
            }
            catch (FlashSupport.GetRomFilenameFailed e)
            {
                HandleChanged("Software Download Failed (" + e.Message + ")", 100);
            }

            iDevice.Updating = false;
            iUpdateListener.SetUpdating(false);

            iFirmwareManager.SetUpdating(false);
        }

        private void HandleChanged(string aMessage, uint aProgress)
        {
            iDevice.Message = aMessage;
            iDevice.Progress = aProgress;
        }
        
        private void HandleChanged(object sender, EventArgs e)
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }
        
        private void HandleProgressChanged(object sender, EventArgs e)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, EventArgs.Empty);
            }
        }
        
        private void HandleMessageChanged(object sender, EventArgs e)
        {
            if (MessageChanged != null)
            {
                MessageChanged(this, EventArgs.Empty);
            }
        }

        private void HandleDownloadProgress(uint aProgress)
        {
            iDevice.Progress = aProgress;
        }
       
        private object iLock;
        private bool iDisposed;

        private CpDeviceReprogrammable iDevice;
        private Firmware iFirmware;
        private IFirmwareManager iFirmwareManager;

        private IPAddress iAdapterAddress;
        private string iUglyname;

        private IUpdateListener iUpdateListener;

        private Thread iThread;
    }

    public class CpDeviceUpdateList : IDisposable
    {
        public CpDeviceUpdateList(NetworkAdapter aAdapter, Preferences aPreferences, IUpdateListener aUpdateListener, CpDeviceReprogramListRepeater aRepeater, FirmwareCache aCache, VersionInfoReader aVersionInfoReader, ChangedHandler aAdded, ChangedHandler aRemoved)
        {
            iLock = new object();
            iDisposed = false;
            iVersionInfoAvailable = false;

            iAdded = aAdded;
            iRemoved = aRemoved;

            iPreferences = aPreferences;
            iPreferences.EventFirmwareBetaChanged += HandleFirmwareBetaChanged;

            iUpdateListener = aUpdateListener;
            iCache = aCache;
            iVersionReader = aVersionInfoReader;
            iVersionReader.EventVersionInfoAvailable += VersionInfoAvailable;

            iPendingList = new List<CpDeviceReprogrammable>();
            //iDeviceList = new CpDeviceReprogramList(Added, Removed);
            iRepeater = aRepeater;
            iRepeater.Added += Added;
            iRepeater.Removed += Removed;
            iDeviceUpdateList = new List<CpDeviceUpdate>();

            SetAdapter(aAdapter);
        }

        public void Dispose()
        {
            //iDeviceList.Dispose();
            //iDeviceList = null;
            iRepeater.Added -= Added;
            iRepeater.Removed -= Removed;

            lock(iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceUpdateList");
                }

                foreach(CpDeviceReprogrammable d in iPendingList)
                {
                    d.Changed -= HandleChanged;
                }
                iPendingList.Clear();

                iCache = null;

                foreach(CpDeviceUpdate d in iDeviceUpdateList)
                {
                    d.Dispose();
                }
                iDeviceUpdateList.Clear();

                iVersionReader.EventVersionInfoAvailable -= VersionInfoAvailable;
                iVersionReader = null;

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
            lock(iLock)
            {
                if(aAdapter != null)
                {
                    iAdapterAddress = new IPAddress(aAdapter.Address());
                }
                else
                {
                    aAdapter = null;
                }
                foreach(CpDeviceUpdate d in iDeviceUpdateList)
                {
                    d.SetAdapterAddress(iAdapterAddress);
                }
            }
        }

        public void Update(string aMacAddress)
        {
            lock(iLock)
            {
                foreach(CpDeviceUpdate d in iDeviceUpdateList)
                {
                    if(d.MacAddress == aMacAddress)
                    {
                        d.Update();
                        return;
                    }
                }
            }
        }

        public void UpdateAll()
        {
            lock(iLock)
            {
                foreach(CpDeviceUpdate d in iDeviceUpdateList)
                {
                    d.Update();
                }
            }
        }

        public delegate void ChangedHandler(CpDeviceUpdateList aList, CpDeviceUpdate aDevice);

        private void VersionInfoAvailable(object sender, EventArgs e)
        {
            List<CpDeviceUpdate> removeDeviceList;

            lock(iLock)
            {
                if(iDisposed)
                {
                    return;
                }

                iVersionInfoAvailable = false;

                removeDeviceList = new List<CpDeviceUpdate>(iDeviceUpdateList);
                iDeviceUpdateList.Clear();
            }

            foreach(CpDeviceUpdate d in removeDeviceList)
            {
                if(iRemoved != null)
                {
                    iRemoved(this, d);
                }

                d.Dispose();
            }


            List<CpDeviceUpdate> addDeviceList = new List<CpDeviceUpdate>();

            lock(iLock)
            {
                iVersionInfoAvailable = true;

                foreach(CpDeviceReprogrammable d in iPendingList)
                {
                    Firmware firmware = CheckForSoftwareUpdate(d);
                    if(firmware != null)
                    {
                        CpDeviceUpdate device = new CpDeviceUpdate(d, firmware, iCache, iAdapterAddress, iUpdateListener);
                        iDeviceUpdateList.Add(device);
                        addDeviceList.Add(device);
                    }
                }
            }

            foreach(CpDeviceUpdate d in addDeviceList)
            {
                if(iAdded != null)
                {
                    iAdded(this, d);
                }
            }
        }

        //private void Added(CpDeviceReprogramList aList, CpDeviceReprogrammable aDevice)
        private void Added(object sender, CpDeviceReprogramListRepeater.CpDeviceReprogrammableEventArgs e)
        {
            CpDeviceReprogrammable aDevice = e.Device;
            CpDeviceUpdate device = null;

            lock(iLock)
            {
                if(iDisposed)
                {
                    return;
                }

                if(iVersionInfoAvailable)
                {
                    Firmware firmware = CheckForSoftwareUpdate(aDevice);
                    if(firmware != null)
                    {
                        UserLog.WriteLine(DateTime.Now + ": CpDeviceUpdateList: DeviceUpdateable+             MacAddress{" + aDevice.MacAddress + "}");

                        device = new CpDeviceUpdate(aDevice, firmware, iCache, iAdapterAddress, iUpdateListener);
                        iDeviceUpdateList.Add(device);
                    }
                }

                iPendingList.Add(aDevice);

                aDevice.Changed += HandleChanged;
            }

            if(device != null)
            {
                if(iAdded != null)
                {
                    iAdded(this, device);
                }
            }
        }

        //private void Removed(CpDeviceReprogramList aList, CpDeviceReprogrammable aDevice)
        private void Removed(object sender, CpDeviceReprogramListRepeater.CpDeviceReprogrammableEventArgs e)
        {
            CpDeviceReprogrammable aDevice = e.Device;
            CpDeviceUpdate device = null;

            lock(iLock)
            {
                if(iDisposed)
                {
                    return;
                }

                foreach (CpDeviceReprogrammable d in iPendingList)
                {
                    if (d.Udn == aDevice.Udn)
                    {
                        iPendingList.Remove(d);

                        aDevice.Changed -= HandleChanged;

                        break;
                    }
                }

                foreach(CpDeviceUpdate d in iDeviceUpdateList)
                {
                    if(d.MacAddress == aDevice.MacAddress)
                    {
                        UserLog.WriteLine(DateTime.Now + ": CpDeviceUpdateList: DeviceUpdateable-             MacAddress{" + aDevice.MacAddress + "}");

                        device = d;

                        iDeviceUpdateList.Remove(d);
                        
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

        private Firmware CheckForSoftwareUpdate(CpDeviceReprogrammable aDevice)
        {
            Firmware firmware = iVersionReader.GetFirmware(aDevice.Model, aDevice.PcbNumberList);
            if(firmware != null)
            {
                if(aDevice.IsFallback || firmware.CompareTo(aDevice.SoftwareVersion) > 0 || aDevice.SoftwareVersion.CompareTo(firmware.Version) > 0)
                {
                    return firmware;
                }
            }

            return null;
        }

        private void HandleChanged(object sender, EventArgs e)
        {
            CpDeviceReprogrammable device = sender as CpDeviceReprogrammable;
            CpDeviceUpdate removedDevice = null;
            CpDeviceUpdate addedDevice = null;

            Firmware firmware = CheckForSoftwareUpdate(device);

            lock(iLock)
            {
                foreach(CpDeviceUpdate d in iDeviceUpdateList)
                {
                    if(d.MacAddress == device.MacAddress)
                    {
                        if((firmware == null || d.Status == CpDeviceReprogrammable.EStatus.eOff) && !d.Updating)
                        {
                            UserLog.WriteLine(DateTime.Now + ": CpDeviceUpdateList: DeviceUpdateable-             MacAddress{" + device.MacAddress + "}");

                            removedDevice = d;
                            iDeviceUpdateList.Remove(removedDevice);
                            break;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                if(removedDevice == null && firmware != null && device.Status != CpDeviceReprogrammable.EStatus.eOff)
                {
                    UserLog.WriteLine(DateTime.Now + ": CpDeviceUpdateList: DeviceUpdateable+             MacAddress{" + device.MacAddress + "}");

                    addedDevice = new CpDeviceUpdate(device, firmware, iCache, iAdapterAddress, iUpdateListener);
                    iDeviceUpdateList.Add(addedDevice);
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

            if(addedDevice != null)
            {
                if(iAdded != null)
                {
                    iAdded(this, addedDevice);
                }
            }
        }

        private void HandleFirmwareBetaChanged(object sender, EventArgs e)
        {
            lock(iLock)
            {
                if(iDisposed)
                {
                    return;
                }

                VersionInfoReader.EUpdateType updateTypes = VersionInfoReader.EUpdateType.Stable;
                if (iPreferences.FirmwareBeta)
                {
                    updateTypes |= VersionInfoReader.EUpdateType.Beta;
                }
                iVersionReader.SetUpdateTypes(updateTypes);
            }
        }

        private object iLock;
        private bool iDisposed;
        private bool iVersionInfoAvailable;
        private IPAddress iAdapterAddress;

        private ChangedHandler iAdded;
        private ChangedHandler iRemoved;

        private Preferences iPreferences;
        private IUpdateListener iUpdateListener;

        private FirmwareCache iCache;
        private VersionInfoReader iVersionReader;
        //private CpDeviceReprogramList iDeviceList;
        private CpDeviceReprogramListRepeater iRepeater;
        private List<CpDeviceReprogrammable> iPendingList;
        private List<CpDeviceUpdate> iDeviceUpdateList;
    }
}

