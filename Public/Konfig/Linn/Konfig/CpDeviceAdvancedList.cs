using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using OpenHome.Net.Core;
using OpenHome.Net.ControlPoint;

using OpenHome.Xapp;

namespace Linn.Konfig
{
    public class CpDeviceAdvanced : ICpDeviceReprogrammable, IDisposable, IComparable<CpDeviceAdvanced>
    {
        private class UpdateArgs
        {
            public UpdateArgs(string aFilename, bool aRecovery)
            {
                iFilename = aFilename;
                iRecovery = aRecovery;
            }

            public string Filename
            {
                get
                {
                    return iFilename;
                }
            }

            public bool Recovery
            {
                get
                {
                    return iRecovery;
                }
            }

            private string iFilename;
            private bool iRecovery;
        }

        public CpDeviceAdvanced(CpDeviceReprogrammable aDevice, string aVariant, IPAddress aAdapterAddress, IUpdateListener aUpdateListener)
        {
            iLock = new object();
            iDisposed = false;

            iDevice = aDevice;
            iDevice.Changed += HandleChanged;
            iDevice.ProgressChanged += HandleProgressChanged;
            iDevice.MessageChanged += HandleMessageChanged;

            iVariant = aVariant;

            iAdapterAddress = aAdapterAddress;
            iUpdateListener = aUpdateListener;

            iUglyname = string.Format("linn-{0}", aDevice.MacAddress.Replace(":", "").Substring(6, 6));
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

                iDisposed = true;
            }
        }

        public void SetAdapterAddress(IPAddress aAdapterAddress)
        {
            lock (iLock)
            {
                iAdapterAddress = aAdapterAddress;
            }
        }

        public void Update(string aFilename, bool aRecovery)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                if (!iDevice.Updating)
                {
                    iThread = new Thread(ApplyUpdate);
                    iThread.Name = string.Format("{0} Updater", MacAddress);

                    iDevice.Updating = true;
                    iUpdateListener.SetUpdating(true);

                    iThread.Start(new UpdateArgs(aFilename, aRecovery));
                }
            }
        }

        public void FactoryDefaults()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                if (!iDevice.Updating)
                {
                    iThread = new Thread(ApplyFactoryDefaults);
                    iThread.Name = string.Format("{0} Factory Defaulter", MacAddress);

                    iDevice.Updating = true;
                    iUpdateListener.SetUpdating(true);

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

        public int CompareTo(CpDeviceAdvanced aDevice)
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

        private void ApplyUpdate(object aUpdateArgs)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }
            }

            HandleChanged(string.Empty, 0);

            UpdateArgs args = aUpdateArgs as UpdateArgs;
            try
            {
                string filename = FlashSupport.GetRomFilename(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), args.Filename as string, iVariant);
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
                        new FlashSupport.Reprogrammer(adapter, iUglyname, filename, args.Recovery, HandleChanged);
                    }
                    else
                    {
                        HandleChanged("Software Update Failed", 100);
                    }
                }
            }
            catch (FlashSupport.GetRomFilenameFailed e)
            {
                HandleChanged("Software Update Failed (" + e.Message + ")", 100);
            }

            iDevice.Updating = false;
            iUpdateListener.SetUpdating(false);
        }

        private void ApplyFactoryDefaults()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }
            }

            HandleChanged(string.Empty, 0);

            UserLog.WriteLine("Restoring default settings on " + MacAddress );

            IPAddress adapter = null;

            lock (iLock)
            {
                adapter = iAdapterAddress;
            }

            if (adapter != null)
            {
                new FlashSupport.FactoryDefaulter(adapter, iUglyname, HandleChanged);
            }
            else
            {
                HandleChanged("Restore To Factory Defaults Failed", 100);
            }

            iUpdateListener.SetUpdating(false);
            iDevice.Updating = false;
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

        private object iLock;
        private bool iDisposed;

        private string iUglyname;
        private IUpdateListener iUpdateListener;
        private IPAddress iAdapterAddress;

        private Thread iThread;

        private string iVariant;
        private CpDeviceReprogrammable iDevice;
    }

    public class CpDeviceAdvancedList : IDisposable
    {
        public CpDeviceAdvancedList(NetworkAdapter aAdapter, IUpdateListener aUpdateListener, CpDeviceReprogramListRepeater aRepeater, VersionInfoReader aVersionReader, ChangedHandler aAdded, ChangedHandler aRemoved)
        {
            iLock = new object();
            iDisposed = false;

            iUpdateListener = aUpdateListener;
            iVersionReader = aVersionReader;

            iAdded = aAdded;
            iRemoved = aRemoved;

            iDeviceListAdvanced = new List<CpDeviceAdvanced>();

            iRepeater = aRepeater;
            iRepeater.Added += Added;
            iRepeater.Removed += Removed;

            SetAdapter(aAdapter);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceAdvancedList");
                }

                iRepeater.Added -= Added;
                iRepeater.Removed -= Removed;
                iRepeater = null;

                foreach(CpDeviceAdvanced d in iDeviceListAdvanced)
                {
                    d.Dispose();
                }
                iDeviceListAdvanced.Clear();

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceAdvancedList");
                }

                iRepeater.Refresh();
            }
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
                foreach (CpDeviceAdvanced d in iDeviceListAdvanced)
                {
                    d.SetAdapterAddress(iAdapterAddress);
                }
            }
        }

        public void Update(string aMacAddress, string aFilename, bool aRecovery)
        {
            lock (iLock)
            {
                foreach (CpDeviceAdvanced d in iDeviceListAdvanced)
                {
                    if (d.MacAddress == aMacAddress)
                    {
                        d.Update(aFilename, aRecovery);
                        break;
                    }
                }
            }
        }

        public void FactoryDefaults(string aMacAddress)
        {
            lock (iLock)
            {
                foreach (CpDeviceAdvanced d in iDeviceListAdvanced)
                {
                    if (d.MacAddress == aMacAddress)
                    {
                        d.FactoryDefaults();
                        break;
                    }
                }
            }
        }

        public delegate void ChangedHandler(CpDeviceAdvancedList aList, CpDeviceAdvanced aDevice);

        private void Added(object sender, CpDeviceReprogramListRepeater.CpDeviceReprogrammableEventArgs e)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceAdvancedList: Device+             Udn{" + e.Device.Udn + "}");

            CpDeviceAdvanced device = new CpDeviceAdvanced(e.Device, iVersionReader.GetDeviceVariant(e.Device.PcbNumberList), iAdapterAddress, iUpdateListener);

            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iDeviceListAdvanced.Add(device);

                e.Device.Changed += HandleChanged;
            }

            if (iAdded != null)
            {
                iAdded(this, device);
            }
        }

        private void Removed(object sender, CpDeviceReprogramListRepeater.CpDeviceReprogrammableEventArgs e)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceAdvancedList: Device-             Udn{" + e.Device.Udn + "}");

            CpDeviceAdvanced device = null;

            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                foreach (CpDeviceAdvanced d in iDeviceListAdvanced)
                {
                    if (d.MacAddress == e.Device.MacAddress)
                    {
                        device = d;

                        iDeviceListAdvanced.Remove(d);

                        e.Device.Changed -= HandleChanged;

                        break;
                    }
                }
            }

            if (iRemoved != null)
            {
                iRemoved(this, device);

                device.Dispose();
            }
        }

        private void HandleChanged(object sender, EventArgs e)
        {
            CpDeviceReprogrammable device = sender as CpDeviceReprogrammable;
            CpDeviceAdvanced removedDevice = null;
            CpDeviceAdvanced addedDevice = null;

            lock (iLock)
            {
                foreach (CpDeviceAdvanced d in iDeviceListAdvanced)
                {
                    if (d.MacAddress == device.MacAddress)
                    {
                        if (d.Status == CpDeviceReprogrammable.EStatus.eOff && !d.Updating)
                        {
                            UserLog.WriteLine(DateTime.Now + ": CpDeviceAdvancedList: DeviceAdvanced-             MacAddress{" + device.MacAddress + "}");

                            removedDevice = d;
                            iDeviceListAdvanced.Remove(removedDevice);
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
                    UserLog.WriteLine(DateTime.Now + ": CpDeviceAdvancedList: DeviceAdvanced+             MacAddress{" + device.MacAddress + "}");

                    addedDevice = new CpDeviceAdvanced(device, iVersionReader.GetDeviceVariant(device.PcbNumberList), iAdapterAddress, iUpdateListener);
                    iDeviceListAdvanced.Add(addedDevice);
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
        private IUpdateListener iUpdateListener;
        private VersionInfoReader iVersionReader;

        private ChangedHandler iAdded;
        private ChangedHandler iRemoved;

        private List<CpDeviceAdvanced> iDeviceListAdvanced;
        private CpDeviceReprogramListRepeater iRepeater;
    }
}
