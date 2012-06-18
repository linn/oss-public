using System;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows;
using System.IO;

using OpenHome.Songcast;

namespace Linn.Songcast
{
    public class ModelController : IDisposable, IConfigurationChangedHandler, INotifyPropertyChanged
    {
        public ModelController(IHelper aHelper, OptionPageUpdates aOptionPageAutoUpdate, AutoUpdate aAutoUpdate, Dispatcher aDispatcher)
        {
            iDispatcher = aDispatcher;

            iOptionEnabled = new OptionBool("songcaster.enabled", "Enabled", "Send audio over network", false);
            iOptionEnabled.EventValueChanged += EnabledChanged;

            iOptionShowInSysTray = new OptionBool("songcaster.showinsystray", "Show in SysTray", "Show in SysTray", true);
            iOptionShowInSysTray.EventValueChanged += ShowInSysTrayChanged;

            iOptionFirstRun = new OptionBool("songcaster.firstrun", "First run", "First time application run", true);
            iOptionShowBalloonTip = new OptionBool("songcaster.showballoontip", "Show balloon tip", "Show balloon tip on first close of preferences dialog", true);

            iOptionPageAdvanced = new OptionPage("Advanced");

            iOptionSubnet = new OptionUint("songcaster.subnet", "Subnet", "Subnet to songcast over", 0);
            iOptionSubnet.EventValueChanged += SubnetChanged;

            iOptionMulticast = new OptionBool("songcast.usemulticast", "Multicast", "Send audio over multicast rather than unicast", false);
            iOptionMulticast.EventValueChanged += MulticastChanged;

            iOptionChannel = new OptionUint("songcaster.channel", "Channel", "Multcast channel to send audio over", CreateRandomChannel());
            iOptionChannel.EventValueChanged += ChannelChanged;

            iOptionTtl = new OptionUint("songcaster.ttl", "TTL", "Time To Live", 4);
            iOptionTtl.EventValueChanged += TtlChanged;

            iOptionPreset = new OptionUint("songcaster.preset", "Preset", "Songcast preset", 0);
            iOptionPreset.EventValueChanged += PresetChanged;

            iOptionLatency = new OptionUint("songcaster.latency", "Latency", "Songcast latency", 100);
            iOptionLatency.EventValueChanged += LatencyChanged;

            iOptionPageAdvanced.Add(iOptionSubnet);
            iOptionPageAdvanced.Add(iOptionMulticast);
            iOptionPageAdvanced.Add(iOptionChannel);
            iOptionPageAdvanced.Add(iOptionTtl);
            iOptionPageAdvanced.Add(iOptionPreset);
            iOptionPageAdvanced.Add(iOptionLatency);

            iAutoUpdate = aAutoUpdate;
            iOptionPageAutoUpdate = aOptionPageAutoUpdate;

            iReceiverList = new ReceiverList(aDispatcher);
            iReceiverList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(EventReceiverListCollectionChanged);

            iMediaPlayerConfiguration = MediaPlayerConfiguration.Load(iOptionEnabled.Native, aHelper);

            iMediaPlayerList = iMediaPlayerConfiguration.MediaPlayerList;

            iSubnetList = new SubnetList(aDispatcher);
            iSubnetList.CountChanged += EventSubnetListCountChanged;

            aHelper.AddOption(iOptionEnabled);
            aHelper.AddOption(iOptionFirstRun);
            aHelper.AddOption(iOptionShowBalloonTip);
            aHelper.AddOptionPage(iOptionPageAdvanced);
        }

        public void Dispose()
        {
            Enabled = false;

            if(iSongcaster != null)
            {
                iSongcaster.Dispose();
            }
        }

        public void Start()
        {
            System.Drawing.Image image = System.Drawing.Image.FromStream(ResourceManager.Icon.StreamSource);

            MemoryStream stream = new MemoryStream();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            byte[] bytes = stream.ToArray();

            iSongcaster = new OpenHome.Songcast.Songcast("linn.co.uk", iOptionSubnet.Native, iOptionChannel.Native, iOptionTtl.Native, iOptionLatency.Native, iOptionMulticast.Native, false, iOptionPreset.Native, iReceiverList, iSubnetList, this, "Linn", "http://www.linn.co.uk", "http://www.linn.co.uk", bytes, "image/png");
        }

        public void ConfigurationChanged(IConfiguration aConfiguration)
        {
            iDispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
            {
                iOptionEnabled.Native = iSongcaster.Enabled();
                iOptionSubnet.Native = aConfiguration.Subnet();
                iOptionMulticast.Native = aConfiguration.Multicast();
                iOptionChannel.Native = aConfiguration.Channel();
                iOptionTtl.Native = aConfiguration.Ttl();
                iOptionPreset.Native = aConfiguration.Preset();
                iOptionLatency.Native = aConfiguration.Latency();
            });
        }

        public bool Enabled
        {
            get
            {
                return iOptionEnabled.Native;
            }
            set
            {
                iOptionEnabled.Native = value;
            }
        }

        public bool ShowInSysTray
        {
            get
            {
                return iOptionShowInSysTray.Native;
            }
            set
            {
                iOptionShowInSysTray.Native = value;
            }
        }

        public bool FirstRun
        {
            get
            {
                return iOptionFirstRun.Native;
            }
            set
            {
                iOptionFirstRun.Native = false;
            }
        }

        public bool ShowBalloonTip
        {
            get
            {
                return iOptionShowBalloonTip.Native;
            }
            set
            {
                iOptionShowBalloonTip.Native = false;
            }
        }

        public MediaPlayerList MediaPlayerList
        {
            get
            {
                return iMediaPlayerList;
            }
        }

        public SubnetList SubnetList
        {
            get
            {
                return iSubnetList;
            }
        }

        public int SelectedSubnetIndex
        {
            get
            {
                return iSelectedSubnetIndex;
            }
            set
            {
                if (value >= 0)
                {
                    uint address = iSubnetList.SubnetAt(value).Address;

                    if (iOptionSubnet.Native != address)
                    {
                        iOptionSubnet.Native = address;
                    }
                }

                if (iSelectedSubnetIndex != value)
                {
                    iSelectedSubnetIndex = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedSubnetIndex"));
                    }
                }
            }
        }

        public bool Unicast
        {
            get
            {
                return !Multicast;
            }
            set
            {
                Multicast = !value;
            }
        }

        public bool Multicast
        {
            get
            {
                return iOptionMulticast.Native;
            }
            set
            {
                iOptionMulticast.Native = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Multicast"));
                }
            }
        }

        public uint Channel
        {
            get
            {
                return iOptionChannel.Native;
            }
            set
            {
                iOptionChannel.Native = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Channel"));
                }
            }
        }

        public uint Ttl
        {
            get
            {
                return iOptionTtl.Native;
            }
            set
            {
                iOptionTtl.Native = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Ttl"));
                }
            }
        }

        public uint Preset
        {
            get
            {
                return iOptionPreset.Native;
            }
            set
            {
                iOptionPreset.Native = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Preset"));
                }
            }
        }

        public uint Latency
        {
            get
            {
                return iOptionLatency.Native;
            }
            set
            {
                iOptionLatency.Native = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Latency"));
                }
            }
        }

        public bool AutomaticUpdateChecks
        {
            get
            {
                return iOptionPageAutoUpdate.AutoUpdate;
            }
            set
            {
                iOptionPageAutoUpdate.AutoUpdate = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AutomaticUpdateChecks"));
                }
            }
        }

        public bool ParticipateInBeta
        {
            get
            {
                return iOptionPageAutoUpdate.BetaVersions;
            }
            set
            {
                iOptionPageAutoUpdate.BetaVersions = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ParticipateInBeta"));
                }
            }
        }

        public void NewChannel()
        {
            Channel = CreateRandomChannel();
        }

        public void RefreshReceiverList()
        {
            iMediaPlayerList.Purge();
            iSongcaster.RefreshReceivers();
        }

        public void ReconnectSelectedReceivers()
        {
            iMediaPlayerList.ReconnectAttachedReceivers();
        }

        public void CheckForUpdates()
        {
            UpdateWindow window = new UpdateWindow(iAutoUpdate);
            window.Closed += UpdateWindowClosed;
            window.Show();
        }

        public void CheckForUpdates(Window aParent)
        {
            UpdateWindow window = new UpdateWindow(aParent, iAutoUpdate);
            window.Closed += UpdateWindowClosed;
            window.Show();
        }

        public event EventHandler<EventArgs> EventEnabledChanged;
        public event EventHandler<EventArgs> EventShowInSysTrayChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private uint CreateRandomChannel()
        {
            return (uint)(new Random().Next(65535) + 1);
        }

        private void UpdateWindowClosed(object sender, EventArgs e)
        {
            UpdateWindow window = sender as UpdateWindow;
            window.Closed -= UpdateWindowClosed;

            if (window.UpdateStarted)
            {
                Application.Current.Shutdown(2);
            }
        }

        private void EnabledChanged(object sender, EventArgs e)
        {
            bool enabled = iOptionEnabled.Native;
            if (iSongcaster != null)
            {
                iSongcaster.SetEnabled(enabled);
            }
            iMediaPlayerConfiguration.SetEnabled(enabled);

            if (EventEnabledChanged != null)
            {
                EventEnabledChanged(this, EventArgs.Empty);
            }
        }

        private void ShowInSysTrayChanged(object sender, EventArgs e)
        {
            if (EventShowInSysTrayChanged != null)
            {
                EventShowInSysTrayChanged(this, EventArgs.Empty);
            }
        }

        private void SubnetChanged(object sender, EventArgs e)
        {
            if (iSongcaster != null)
            {
                iSongcaster.SetSubnet(iOptionSubnet.Native);
            }
        }

        private void MulticastChanged(object sender, EventArgs e)
        {
            if (iSongcaster != null)
            {
                iSongcaster.SetMulticast(iOptionMulticast.Native);
            }
        }

        private void ChannelChanged(object sender, EventArgs e)
        {
            if (iSongcaster != null)
            {
                iSongcaster.SetChannel(iOptionChannel.Native);
            }
        }

        private void TtlChanged(object sender, EventArgs e)
        {
            iSongcaster.SetTtl(iOptionTtl.Native);
        }

        private void PresetChanged(object sender, EventArgs e)
        {
            if (iSongcaster != null)
            {
                iSongcaster.SetPreset(iOptionPreset.Native);
            }
        }

        private void LatencyChanged(object sender, EventArgs e)
        {
            iSongcaster.SetLatency(iOptionLatency.Native);
        }

        private void EventReceiverListCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (object o in e.NewItems)
                    {
                        Receiver receiver = o as Receiver;
                        iMediaPlayerConfiguration.ReceiverAdded(receiver);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (object o in e.OldItems)
                    {
                        Receiver receiver = o as Receiver;
                        iMediaPlayerConfiguration.ReceiverRemoved(receiver);
                    }
                    break;
                default:
                    break;
            }
        }

        private void EventSubnetListCountChanged(object sender, EventArgs e)
        {
            if (iOptionSubnet.Native == 0)
            {
                if (iSubnetList.Count > 0)
                {
                    SelectedSubnetIndex = 0;
                    return;
                }
            }

            int index = 0;

            foreach (Subnet subnet in iSubnetList)
            {
                if (subnet.Address == iOptionSubnet.Native)
                {
                    SelectedSubnetIndex = index;
                    return;
                }

                index++;
            }

            SelectedSubnetIndex = -1;
        }

        private Dispatcher iDispatcher;

        private OpenHome.Songcast.Songcast iSongcaster;

        private AutoUpdate iAutoUpdate;
        private OptionPageUpdates iOptionPageAutoUpdate;

        private OptionBool iOptionEnabled;
        private OptionBool iOptionShowInSysTray;
        private OptionBool iOptionFirstRun;
        private OptionBool iOptionShowBalloonTip;

        private OptionPage iOptionPageAdvanced;
        private OptionUint iOptionSubnet;
        private OptionUint iOptionChannel;
        private OptionUint iOptionTtl;
        private OptionBool iOptionMulticast;
        private OptionUint iOptionPreset;
        private OptionUint iOptionLatency;

        private MediaPlayerList iMediaPlayerList;
        private MediaPlayerConfiguration iMediaPlayerConfiguration;
        private ReceiverList iReceiverList;
        private SubnetList iSubnetList;
        private int iSelectedSubnetIndex;
    }
}
