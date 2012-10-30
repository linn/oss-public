
using System;
using System.Collections.Generic;

using OpenHome.Songcast;


namespace Linn.Songcast
{
    public class EventArgsReceiver : EventArgs
    {
        public EventArgsReceiver(string aReceiverUdn)
        {
            ReceiverUdn = aReceiverUdn;
        }

        public readonly string ReceiverUdn;
    }


    // Receiver class that caches the values of all receiver properties and also provides some locking
    // around the equivalent openhome receiver. The problem is that these receivers are primarily
    // accessed from the main thread, while the underlying openhome receiver can disappear in a
    // different thread - this class provides some thread safety
    public class Receiver : IReceiver
    {
        public Receiver(string aUdn, string aRoom, string aGroup, string aName)
        {
            // create a receiver from saved preferences
            iLock = new object();
            iReceiver = null;

            iUdn = aUdn;
            iRoom = aRoom;
            iGroup = aGroup;
            iName = aName;

            // give the rest of these cached values some sensible defaults
            iHasVolumeControl = false;
            iMute = false;
            iStatus = EReceiverStatus.eDisconnected;
            iVolume = 0;
            iVolumeLimit = 0;
            iIpAddress = 0;
        }

        public Receiver(OpenHome.Songcast.IReceiver aReceiver)
        {
            // create a receiver from a receiver found on the network
            iLock = new object();
            iReceiver = aReceiver;
            UpdateCachedValues();
        }

        public void Update(OpenHome.Songcast.IReceiver aReceiver)
        {
            // update a receiver
            lock (iLock)
            {
                if (aReceiver != null)
                {
                    Assert.Check(iUdn == aReceiver.Udn);
                    iReceiver = aReceiver;
                    UpdateCachedValues();
                }
                else
                {
                    iReceiver = null;
                }
            }
        }

        #region Implementation of OpenHome.Songcast.IReceiver

        public void Play()
        {
            lock (iLock)
            {
                if (iReceiver != null)
                    iReceiver.Play();
            }
        }

        public void Stop()
        {
            lock (iLock)
            {
                if (iReceiver != null)
                    iReceiver.Stop();
            }
        }

        public void Standby()
        {
            lock (iLock)
            {
                if (iReceiver != null)
                    iReceiver.Standby();
            }
        }

        public void SetVolume(uint aValue)
        {
            lock (iLock)
            {
                if (iReceiver != null)
                    iReceiver.SetVolume(aValue);
            }
        }

        public void VolumeInc()
        {
            lock (iLock)
            {
                if (iReceiver != null)
                    iReceiver.VolumeInc();
            }
        }

        public void VolumeDec()
        {
            lock (iLock)
            {
                if (iReceiver != null)
                    iReceiver.VolumeDec();
            }
        }

        public void SetMute(bool aValue)
        {
            lock (iLock)
            {
                if (iReceiver != null)
                    iReceiver.SetMute(aValue);
            }
        }

        public string Udn
        {
            get { return iUdn; }
        }

        public string Room
        {
            get { return iRoom; }
        }

        public string Group
        {
            get { return iGroup; }
        }

        public string Name
        {
            get { return iName; }
        }

        public EReceiverStatus Status
        {
            get { return iStatus; }
        }

        public bool HasVolumeControl
        {
            get { return iHasVolumeControl; }
        }

        public uint Volume
        {
            get { return iVolume; }
        }

        public bool Mute
        {
            get { return iMute; }
        }

        public uint VolumeLimit
        {
            get { return iVolumeLimit; }
        }

        public uint IpAddress
        {
            get { return iIpAddress; }
        }

        #endregion

        public bool IsOnline
        {
            get
            {
                lock (iLock)
                {
                    return (iReceiver != null);
                }
            }
        }

        private void UpdateCachedValues()
        {
            Assert.Check(iReceiver != null);

            iUdn = iReceiver.Udn;
            iRoom = iReceiver.Room;
            iGroup = iReceiver.Group;
            iName = iReceiver.Name;

            iHasVolumeControl = iReceiver.HasVolumeControl;
            iMute = iReceiver.Mute;
            iStatus = iReceiver.Status;
            iVolume = iReceiver.Volume;
            iVolumeLimit = iReceiver.VolumeLimit;
            iIpAddress = iReceiver.IpAddress;
        }

        private object iLock;
        private OpenHome.Songcast.IReceiver iReceiver;

        private string iUdn;
        private string iRoom;
        private string iGroup;
        private string iName;

        private bool iHasVolumeControl;
        private bool iMute;
        private OpenHome.Songcast.EReceiverStatus iStatus;
        private uint iVolume;
        private uint iVolumeLimit;
        private uint iIpAddress;
    }


    // Main model class - all functions in this class should be run in the main thread
    public class Model
    {
        public static string kOnlineManualUri = "http://oss.linn.co.uk/trac/wiki/Songcast_4_2_DavaarManual";

        public Model(IInvoker aInvoker, Helper aHelper)
        {
            iPreferences = new Preferences(aHelper);

            iSongcastMonitor = new SongcastMonitor(aInvoker, iPreferences.ReceiverList);
            iSongcast = null;
            iSelectedReceiverUdn = iPreferences.SelectedReceiverUdn;
            
            iSongcastMonitor.EventReceiverAdded += ReceiverAdded;
            iSongcastMonitor.EventReceiverListChanged += ReceiverListChanged;
            iSongcastMonitor.EventReceiverVolumeControlChanged += ReceiverVolumeControlChanged;
            iSongcastMonitor.EventReceiverVolumeChanged += ReceiverVolumeChanged;
            iSongcastMonitor.EventSubnetListChanged += SubnetListChanged;
            iSongcastMonitor.EventConfigurationChanged += ConfigurationChanged;
            
            iPreferences.EventSelectedReceiverChanged += PreferencesSelectedReceiverChanged;
            iPreferences.EventSelectedSubnetChanged += PreferencesSelectedSubnetChanged;
            iPreferences.EventMulticastEnabledChanged += PreferencesMulticastEnabledChanged;
            iPreferences.EventMulticastChannelChanged += PreferencesMulticastChannelChanged;
            iPreferences.EventUseMusicLatencyChanged += PreferencesUseMusicLatencyChanged;
            iPreferences.EventMusicLatencyMsChanged += PreferencesMusicLatencyMsChanged;
            iPreferences.EventVideoLatencyMsChanged += PreferencesVideoLatencyMsChanged;
        }

        public void Start(string aDomain, string aManufacturer, string aManufacturerUrl, string aModelUrl, byte[] aImage, string aMimeType)
        {
            if (iSongcast != null) {
                return;
            }
            
            // songcast always starts disabled
            if (EventEnabledChanged != null) {
                EventEnabledChanged(this, EventArgs.Empty);
            }
            
            // create the songcast object
            uint ttl = 4;
            bool enabled = false;
            uint preset = 0;
            iSongcast = new OpenHome.Songcast.Songcast(aDomain, iPreferences.SelectedSubnetAddress, iPreferences.MulticastChannel,
                                                       ttl, LatencyMs, iPreferences.MulticastEnabled,
                                                       enabled, preset, iSongcastMonitor, iSongcastMonitor, iSongcastMonitor, iSongcastMonitor,
                                                       aManufacturer, aManufacturerUrl, aModelUrl, aImage, aMimeType);
        }
        
        public void Stop()
        {
            if (iSongcast == null) {
                return;
            }
            
            // disable songcast first - this will put the selected receiver into standby
            this.Enabled = false;
            
            // dispose of the songcast lower layers
            iSongcast.Dispose();
            iSongcast = null;
        }

        public bool Enabled
        {
            get
            {
                if (iSongcast != null) {
                    return iSongcast.Enabled();
                }
                else {
                    return false;
                }
            }
            set
            {
                if (iSongcast != null)
                {
                    // stop the selected receiver before disabling songcast
                    if (!value) {
                        Receiver recv = iSongcastMonitor.Receiver(iSelectedReceiverUdn);
                        if (recv != null) {
                            StopReceiver(recv);
                        }
                    }
                    
                    iSongcast.SetEnabled(value);
                    
                    // start the selected receiver after enabling songcast
                    if (value) {
                        Receiver recv = iSongcastMonitor.Receiver(iSelectedReceiverUdn);
                        if (recv != null && recv.IsOnline) {
                            recv.Play();
                        }
                    }

                    // send notification
                    if (EventEnabledChanged != null) {
                        EventEnabledChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public Receiver[] ReceiverList
        {
            get { return iSongcastMonitor.ReceiverList; }
        }

        public ISubnet[] SubnetList
        {
            get { return iSongcastMonitor.SubnetList; }
        }

        public Receiver Receiver(string aUdn)
        {
            return iSongcastMonitor.Receiver(aUdn);
        }

        public Preferences Preferences
        {
            get { return iPreferences; }
        }

        public void RefreshReceiverList()
        {
            if (iSongcast != null)
            {
                // remove all offline receivers apart from the selected - this will send out an event that is handled
                // in the ReceiverListChanged handler below
                iSongcastMonitor.RemoveOfflineReceivers(iSelectedReceiverUdn);
                
                // refresh
                iSongcast.RefreshReceivers();
            }
        }
        
        // Events to provide notification of changes to the model
        public event EventHandler EventEnabledChanged;
        public event EventHandler EventReceiverListChanged;
        public event EventHandler<EventArgsReceiver> EventReceiverVolumeControlChanged;
        public event EventHandler<EventArgsReceiver> EventReceiverVolumeChanged;
        public event EventHandler EventSubnetListChanged;


        #region Event handlers from the SongcastMonitor
        
        private void ReceiverAdded(object sender, EventArgsReceiver e)
        {
            UserLog.WriteLine(DateTime.Now + " : Linn.Songcast.Model.ReceiverAdded " + e.ReceiverUdn);

            if (iSongcast == null) {
                return;
            }

            // if the added receiver is the selected receiver and if songcast is enabled, play it
            if (this.Enabled && e.ReceiverUdn == iSelectedReceiverUdn)
            {
                Receiver recv = iSongcastMonitor.Receiver(iSelectedReceiverUdn);
                if (recv != null && recv.IsOnline) {
                    recv.Play();
                }
            }

            // update the preferences before notification
            iPreferences.ReceiverList = iSongcastMonitor.ReceiverList;
            
            if (EventReceiverListChanged != null) {
                EventReceiverListChanged(this, EventArgs.Empty);
            }

            // now check for auto selecting first receiver
            if (iSelectedReceiverUdn == string.Empty && iSongcastMonitor.ReceiverList.Length != 0)
            {
                // just set the value in the preferences - the preference eventing will cause the
                // handler to start playing this if necessary
                iPreferences.SelectedReceiverUdn = iSongcastMonitor.ReceiverList[0].Udn;
            }
        }
        
        private void ReceiverListChanged(object sender, EventArgs e)
        {
            UserLog.WriteLine(DateTime.Now + " : Linn.Songcast.Model.ReceiverListChanged");

            if (iSongcast == null) {
                return;
            }
            
            // update the preferences before notification
            iPreferences.ReceiverList = iSongcastMonitor.ReceiverList;
            
            if (EventReceiverListChanged != null) {
                EventReceiverListChanged(this, EventArgs.Empty);
            }
        }

        private void ReceiverVolumeControlChanged(object sender, EventArgsReceiver e)
        {
            if (iSongcast != null && EventReceiverVolumeControlChanged != null) {
                EventReceiverVolumeControlChanged(this, e);
            }
        }

        private void ReceiverVolumeChanged(object sender, EventArgsReceiver e)
        {
            if (iSongcast != null && EventReceiverVolumeChanged != null) {
                EventReceiverVolumeChanged(this, e);
            }
        }

        private void SubnetListChanged(object sender, EventArgs e)
        {
            if (iSongcast == null) {
                return;
            }

            // send notification
            if (EventSubnetListChanged != null) {
                EventSubnetListChanged(this, EventArgs.Empty);
            }
            
            // now set the subnet used by the lower layers of songcast if needed
            if (iPreferences.SelectedSubnetAddress != 0)
            {
                // the current subnet is set - if it is not set in the songcast lower layers, set it now if it is available
                bool setSubnet = false;
                foreach (ISubnet subnet in iSongcastMonitor.SubnetList)
                {
                    if (subnet.Address == iPreferences.SelectedSubnetAddress && subnet.AdapterName != "Network adapter not present")
                    {
                        if (iPreferences.SelectedSubnetAddress != iSongcast.Subnet())
                        {
                            iSongcast.SetSubnet(iPreferences.SelectedSubnetAddress);
                        }
                        setSubnet = true;
                        break;
                    }
                }

                // if we haven't found our last subnet but there is only one available, select it
                if (!setSubnet)
                {
                    foreach(ISubnet subnet in iSongcastMonitor.SubnetList)
                    {
                        if (subnet.AdapterName != "Network adapter not present")
                        {
                            iSongcast.SetSubnet(subnet.Address);
                            iPreferences.SelectedSubnetAddress = subnet.Address;
                        }
                    }
                }
            }
            else
            {
                // the current subnet has not been set - automatically select it to be the first available
                if (iSongcast.Subnet() == 0 && iSongcastMonitor.SubnetList.Length != 0)
                {
                    // set songcast to the first subnet
                    iSongcast.SetSubnet(iSongcastMonitor.SubnetList[0].Address);

                    // store the new selected subnet in preferences and send notification
                    iPreferences.SelectedSubnetAddress = iSongcastMonitor.SubnetList[0].Address;
                }
            }
        }
        
        private void ConfigurationChanged(object sender, EventArgs e)
        {
            // the lower level songcast has become enabled or disabled - reset it so that
            // the stuff required for this layer gets set
            Enabled = Enabled;
        }
        
        #endregion

        #region Event handlers from the Preferences object
        
        private void PreferencesSelectedReceiverChanged(object sender, EventArgs e)
        {
            // if songcast is enabled, need to stop the previous selected receiver and start the new one
            if (this.Enabled)
            {
                Receiver recv = iSongcastMonitor.Receiver(iSelectedReceiverUdn);
                if (recv != null)
                {
                    StopReceiver(recv);
                }

                recv = iSongcastMonitor.Receiver(iPreferences.SelectedReceiverUdn);
                if (recv != null && recv.IsOnline)
                {
                    recv.Play();
                }
            }

            iSelectedReceiverUdn = iPreferences.SelectedReceiverUdn;
        }
        
        private void PreferencesSelectedSubnetChanged(object sender, EventArgs e)
        {
            if (iSongcast != null) {
                iSongcast.SetSubnet(iPreferences.SelectedSubnetAddress);
            }
        }
        
        private void PreferencesMulticastEnabledChanged(object sender, EventArgs e)
        {
            if (iSongcast != null) {
                iSongcast.SetMulticast(iPreferences.MulticastEnabled);
            }
        }
        
        private void PreferencesMulticastChannelChanged(object sender, EventArgs e)
        {
            if (iSongcast != null) {
                iSongcast.SetChannel(iPreferences.MulticastChannel);
            }
        }
        
        private void PreferencesUseMusicLatencyChanged(object sender, EventArgs e)
        {
            if (iSongcast != null) {
                iSongcast.SetLatency(LatencyMs);
            }
        }

        private void PreferencesMusicLatencyMsChanged(object sender, EventArgs e)
        {
            if (iSongcast != null) {
                iSongcast.SetLatency(LatencyMs);
            }
        }
        
        private void PreferencesVideoLatencyMsChanged(object sender, EventArgs e)
        {
            if (iSongcast != null) {
                iSongcast.SetLatency(LatencyMs);
            }
        }
        
        #endregion
        

        private void StopReceiver(Receiver aReceiver)
        {
            if (!aReceiver.IsOnline) {
                return;
            }
            
            switch (aReceiver.Status)
            {
            case EReceiverStatus.eDisconnected:
                // receiver is disconnected - this is likely to be because the receiver DS has changed
                // sources, for example, so do not interfere with it
                break;
                
            case EReceiverStatus.eConnected:
            case EReceiverStatus.eConnecting:
                // receiver is still connected to this songcast, so stop and put into standby
                aReceiver.Stop();
                aReceiver.Standby();
                break;
            }
        }
        
        private uint LatencyMs
        {
            get { return (iPreferences.UseMusicLatency ? iPreferences.MusicLatencyMs : iPreferences.VideoLatencyMs); }
        }

        private Preferences iPreferences;
        private SongcastMonitor iSongcastMonitor;
        private OpenHome.Songcast.Songcast iSongcast;
        private string iSelectedReceiverUdn;
    }


    // Class to monitor the asynchronous changes in the songcast lower layers - this ensures that all change events occur in the main thread
    public class SongcastMonitor : IReceiverHandler, ISubnetHandler, IConfigurationChangedHandler, IMessageHandler
    {
        public SongcastMonitor(IInvoker aInvoker, Receiver[] aReceiverList)
        {
            iLock = new object();
            iInvoker = aInvoker;
            iReceiverList = new List<Receiver>(aReceiverList);
            iSubnetList = new List<ISubnet>();
        }

        public Receiver[] ReceiverList
        {
            get
            {
                lock (iLock)
                {
                    return iReceiverList.ToArray();
                }
            }
        }
        
        public ISubnet[] SubnetList
        {
            get
            {
                lock (iLock)
                {
                    return iSubnetList.ToArray();
                }
            }
        }

        public Receiver Receiver(string aUdn)
        {
            lock (iLock)
            {
                foreach (Receiver recv in iReceiverList)
                {
                    if (recv.Udn == aUdn) {
                        return recv;
                    }
                }
                return null;
            }
        }
        
        public void RemoveOfflineReceivers(string aExemptReceiverUdn)
        {
            // remove any receivers from the list that are unavailable apart from exemptions
            lock (iLock)
            {
                List<Receiver> newList = new List<Receiver>();
                
                foreach (Receiver recv in iReceiverList)
                {
                    if (recv.Udn == aExemptReceiverUdn || recv.IsOnline) {
                        newList.Add(recv);
                    }
                }
                
                iReceiverList = newList;
            }
            
            // notify that the list has changed
            NotifyReceiverListChanged();
        }

        public event EventHandler<EventArgsReceiver> EventReceiverAdded;
        public event EventHandler EventReceiverListChanged;
        public event EventHandler EventSubnetListChanged;
        public event EventHandler EventConfigurationChanged;
        public event EventHandler<EventArgsReceiver> EventReceiverVolumeControlChanged;
        public event EventHandler<EventArgsReceiver> EventReceiverVolumeChanged;


        #region Implementation of IReceiverHandler

        void IReceiverHandler.ReceiverAdded(OpenHome.Songcast.IReceiver aReceiver)
        {
            UserLog.WriteLine(DateTime.Now + " : Linn.Songcast.SongcastMonitor.ReceiverAdded " + aReceiver.Room + ":" + aReceiver.Group);

            // this function runs in an ohSongcast thread
            lock (iLock)
            {
                Receiver recv = Receiver(aReceiver.Udn);

                // update existing receivers with the new openhome receiver
                if (recv != null) {
                    recv.Update(aReceiver);
                }
                else {
                    iReceiverList.Add(new Receiver(aReceiver));
                }

                // notify in the main thread
                iInvoker.BeginInvoke(new Action<EventArgsReceiver>(NotifyReceiverAdded), new EventArgsReceiver(aReceiver.Udn));
            }
        }

        void IReceiverHandler.ReceiverChanged(OpenHome.Songcast.IReceiver aReceiver)
        {
            UserLog.WriteLine(DateTime.Now + " : Linn.Songcast.SongcastMonitor.ReceiverChanged " + aReceiver.Room + ":" + aReceiver.Group);

            // this function runs in an ohSongcast thread
            lock (iLock)
            {
                Receiver recv = Receiver(aReceiver.Udn);
                
                if (recv != null)
                {
                    // receiver exists in list - update it with the new openhome receiver
                    recv.Update(aReceiver);
                    
                    // notify in the main thread
                    iInvoker.BeginInvoke(new Action(NotifyReceiverListChanged));
                }
            }
        }

        void IReceiverHandler.ReceiverRemoved(OpenHome.Songcast.IReceiver aReceiver)
        {
            UserLog.WriteLine(DateTime.Now + " : Linn.Songcast.SongcastMonitor.ReceiverRemoved " + aReceiver.Room + ":" + aReceiver.Group);

            // this function runs in an ohSongcast thread
            lock (iLock)
            {
                Receiver recv = Receiver(aReceiver.Udn);
                
                if (recv != null)
                {
                    // receiver exists in list - update it with a null openhome receiver
                    recv.Update(null);
                    
                    // notify in the main thread
                    iInvoker.BeginInvoke(new Action(NotifyReceiverListChanged));
                }
            }
        }

        public void ReceiverVolumeControlChanged(OpenHome.Songcast.IReceiver aReceiver)
        {
            // this function runs in an ohSongcast thread
            lock (iLock)
            {
                Receiver recv = Receiver(aReceiver.Udn);

                if (recv != null)
                {
                    // update the receiver that has changed
                    recv.Update(aReceiver);

                    // notify in the main thread
                    iInvoker.BeginInvoke(new Action<EventArgsReceiver>(NotifyReceiverVolumeControlChanged), new EventArgsReceiver(aReceiver.Udn));
                }
            }
        }

        public void ReceiverVolumeChanged(OpenHome.Songcast.IReceiver aReceiver)
        {
            // this function runs in an ohSongcast thread
            lock (iLock)
            {
                Receiver recv = Receiver(aReceiver.Udn);

                if (recv != null)
                {
                    // update the receiver that has changed
                    recv.Update(aReceiver);

                    // notify in the main thread
                    iInvoker.BeginInvoke(new Action<EventArgsReceiver>(NotifyReceiverVolumeChanged), new EventArgsReceiver(aReceiver.Udn));
                }
            }
        }

        public void ReceiverMuteChanged(OpenHome.Songcast.IReceiver aReceiver)
        {
            ReceiverVolumeChanged(aReceiver);
        }

        public void ReceiverVolumeLimitChanged(OpenHome.Songcast.IReceiver aReceiver)
        {
            ReceiverVolumeChanged(aReceiver);
        }

        #endregion


        #region Implementation of ISubnetHandler

        void ISubnetHandler.SubnetAdded(ISubnet aSubnet)
        {
            // this function runs in an ohSongcast thread
            lock (iLock)
            {
                // add to the list
                if (!iSubnetList.Contains(aSubnet)) {
                    iSubnetList.Add(aSubnet);
                }

                // notify in the main thread
                iInvoker.BeginInvoke(new Action(NotifySubnetListChanged));
            }
        }
        
        void ISubnetHandler.SubnetChanged(ISubnet aSubnet)
        {
            // this function runs in an ohSongcast thread
            lock (iLock)
            {
                if (iSubnetList.Contains(aSubnet)) {
                    // notify in the main thread
                    iInvoker.BeginInvoke(new Action(NotifySubnetListChanged));
                }
            }
        }
        
        void ISubnetHandler.SubnetRemoved(ISubnet aSubnet)
        {
            // this function runs in an ohSongcast thread
            lock (iLock)
            {
                if (iSubnetList.Contains(aSubnet)) {
                    iSubnetList.Remove(aSubnet);
                    // notify in the main thread
                    iInvoker.BeginInvoke(new Action(NotifySubnetListChanged));
                }
            }
        }
        
        #endregion
        

        #region Implementation of IConfigurationChangedHandler

        void IConfigurationChangedHandler.ConfigurationChanged(IConfiguration aConfiguration)
        {
            iInvoker.BeginInvoke(new Action(NotifyConfigurationChanged));
        }
        
        #endregion

        #region Implementation of IMessageHandler

        public void Message(string aMessage)
        {
            UserLog.Write(aMessage);
        }

        #endregion

        private void NotifyReceiverAdded(EventArgsReceiver aArgs)
        {
            // in main thread
            Assert.Check(!iInvoker.InvokeRequired);

            if (EventReceiverAdded != null) {
                EventReceiverAdded(this, aArgs);
            }
        }
        
        private void NotifyReceiverListChanged()
        {
            // in main thread
            Assert.Check(!iInvoker.InvokeRequired);
            
            if (EventReceiverListChanged != null) {
                EventReceiverListChanged(this, EventArgs.Empty);
            }
        }

        private void NotifyReceiverVolumeControlChanged(EventArgsReceiver aArgs)
        {
            // in main thread
            Assert.Check(!iInvoker.InvokeRequired);

            if (EventReceiverVolumeControlChanged != null) {
                EventReceiverVolumeControlChanged(this, aArgs);
            }
        }

        private void NotifyReceiverVolumeChanged(EventArgsReceiver aArgs)
        {
            // in main thread
            Assert.Check(!iInvoker.InvokeRequired);

            if (EventReceiverVolumeChanged != null) {
                EventReceiverVolumeChanged(this, aArgs);
            }
        }

        private void NotifySubnetListChanged()
        {
            // in main thread
            Assert.Check(!iInvoker.InvokeRequired);

            if (EventSubnetListChanged != null) {
                EventSubnetListChanged(this, EventArgs.Empty);
            }
        }
        
        private void NotifyConfigurationChanged()
        {
            // in main thread
            Assert.Check(!iInvoker.InvokeRequired);

            if (EventConfigurationChanged != null) {
                EventConfigurationChanged(this, EventArgs.Empty);
            }
        }
        
        private object iLock;
        private IInvoker iInvoker;
        private List<Receiver> iReceiverList;
        private List<ISubnet> iSubnetList;
    }
}


