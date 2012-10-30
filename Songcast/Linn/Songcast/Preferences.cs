
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;


namespace Linn.Songcast
{

    public class Preferences
    {
        private const uint kDefaultMusicLatencyMs = 300;
        private const uint kDefaultVideoLatencyMs = 50;
        private const string kTrackerAccountDev = "UA-35644544-1";
        private const string kTrackerAccountBeta = "UA-35652600-1";
        private const string kTrackerAccountRelease = "UA-35647838-1";

        public Preferences(Helper aHelper)
        {
            iOptionPagePrivacy = new OptionPagePrivacy(aHelper);
            aHelper.AddOptionPage(iOptionPagePrivacy);

            iOptionReceiverList = new OptionString("linn.songcaster", "Media player list", "List of media players that have ever been seen", new MediaPlayerConfiguration().Save());
            iOptionSelectedReceiverUdn = new OptionString("songcast.selectedreceiverudn", "Selected Receiver UDN", "The UDN of the currently selected Songcast receiver", string.Empty);
            iOptionSubnetAddress = new OptionUint("songcaster.subnet", "Subnet", "Subnet to songcast over", 0);
            iOptionMulticast = new OptionBool("songcast.usemulticast", "Multicast", "Send audio over multicast rather than unicast", false);
            iOptionChannel = new OptionUint("songcaster.channel", "Channel", "Multcast channel to send audio over", 0);
            iOptionUseMusicLatency = new OptionBool("songcast.usemusiclatency", "Use Music Latency", "Send audio using the music latency rather than video latency", true);
            iOptionMusicLatencyMs = new OptionUint("songcast.musiclatencyms", "Music Latency (ms)", "Songcast music latency", kDefaultMusicLatencyMs);
            iOptionVideoLatencyMs = new OptionUint("songcast.videolatencyms", "Video Latency (ms)", "Songcast video latency", kDefaultVideoLatencyMs);
            iOptionRotaryVolumeControl = new OptionBool("songcast.rotaryvolumecontrol", "Rotary volume control", "Whether to use rotary or rocker volume control", true);

            aHelper.AddOption(iOptionReceiverList);
            aHelper.AddOption(iOptionSelectedReceiverUdn);
            aHelper.AddOption(iOptionSubnetAddress);
            aHelper.AddOption(iOptionMulticast);
            aHelper.AddOption(iOptionChannel);
            aHelper.AddOption(iOptionUseMusicLatency);
            aHelper.AddOption(iOptionMusicLatencyMs);
            aHelper.AddOption(iOptionVideoLatencyMs);
            aHelper.AddOption(iOptionRotaryVolumeControl);

            if (iOptionChannel.Native == 0)
            {
                // channel has not been set - assign a random value
                Random r = new Random();
                int byte1 = r.Next(254) + 1;    // in range [1,254]
                int byte2 = r.Next(254) + 1;    // in range [1,254]
                int channel = byte1 << 8 | byte2;
                iOptionChannel.Native = (uint)channel;
            }
            iTrackerAccount = kTrackerAccountDev;
            if (aHelper.BuildType == EBuildType.Release)
            {
                iTrackerAccount = kTrackerAccountRelease;
            }
            else if (aHelper.BuildType == EBuildType.Beta)
            {
                iTrackerAccount = kTrackerAccountBeta;
            }
        }

        public string TrackerAccount
        {
            get
            {
                return iTrackerAccount;
            }
        }

        public Receiver[] ReceiverList
        {
            get
            {
                List<Receiver> recvList = new List<Receiver>();

                MediaPlayerConfiguration c = MediaPlayerConfiguration.Create(iOptionReceiverList.Native);

                foreach (MediaPlayer p in c.List)
                {
                    recvList.Add(new Receiver(p.Udn, p.Room, p.Group, p.Name));
                }

                return recvList.ToArray();
            }
            set
            {
                MediaPlayerConfiguration c = new MediaPlayerConfiguration(value);
                iOptionReceiverList.Native = c.Save();
            }
        }

        public string SelectedReceiverUdn
        {
            get { return iOptionSelectedReceiverUdn.Native; }
            set { iOptionSelectedReceiverUdn.Native = value; }
        }

        public uint SelectedSubnetAddress
        {
            get { return iOptionSubnetAddress.Native; }
            set { iOptionSubnetAddress.Native = value; }
        }

        public bool MulticastEnabled
        {
            get { return iOptionMulticast.Native; }
            set { iOptionMulticast.Native = value; }
        }

        public uint MulticastChannel
        {
            get { return iOptionChannel.Native; }
            set { iOptionChannel.Native = value; }
        }

        public bool UseMusicLatency
        {
            get { return iOptionUseMusicLatency.Native; }
            set { iOptionUseMusicLatency.Native = value; }
        }

        public uint MusicLatencyMs
        {
            get { return iOptionMusicLatencyMs.Native; }
            set { iOptionMusicLatencyMs.Native = value; }
        }

        public uint VideoLatencyMs
        {
            get { return iOptionVideoLatencyMs.Native; }
            set { iOptionVideoLatencyMs.Native = value; }
        }

        public uint DefaultMusicLatencyMs
        {
            get { return kDefaultMusicLatencyMs; }
        }

        public uint DefaultVideoLatencyMs
        {
            get { return kDefaultVideoLatencyMs; }
        }

        public bool RotaryVolumeControl
        {
            get { return iOptionRotaryVolumeControl.Native; }
            set { iOptionRotaryVolumeControl.Native = value; }
        }

        public bool UsageData
        {
            get { return iOptionPagePrivacy.UsageData; }
            set { iOptionPagePrivacy.UsageData = value; }
        }

        public event EventHandler<EventArgs> EventSelectedReceiverChanged
        {
            add { iOptionSelectedReceiverUdn.EventValueChanged += value; }
            remove { iOptionSelectedReceiverUdn.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSelectedSubnetChanged
        {
            add { iOptionSubnetAddress.EventValueChanged += value; }
            remove { iOptionSubnetAddress.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventMulticastEnabledChanged
        {
            add { iOptionMulticast.EventValueChanged += value; }
            remove { iOptionMulticast.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventMulticastChannelChanged
        {
            add { iOptionChannel.EventValueChanged += value; }
            remove { iOptionChannel.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventUseMusicLatencyChanged
        {
            add { iOptionUseMusicLatency.EventValueChanged += value; }
            remove { iOptionUseMusicLatency.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventMusicLatencyMsChanged
        {
            add { iOptionMusicLatencyMs.EventValueChanged += value; }
            remove { iOptionMusicLatencyMs.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventVideoLatencyMsChanged
        {
            add { iOptionVideoLatencyMs.EventValueChanged += value; }
            remove { iOptionVideoLatencyMs.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventRotaryVolumeControlChanged
        {
            add { iOptionRotaryVolumeControl.EventValueChanged += value; }
            remove { iOptionRotaryVolumeControl.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventUsageDataChanged
        {
            add { iOptionPagePrivacy.EventUsageDataChanged += value; }
            remove { iOptionPagePrivacy.EventUsageDataChanged -= value; }
        }

        private OptionString iOptionReceiverList;
        private OptionString iOptionSelectedReceiverUdn;
        private OptionUint iOptionSubnetAddress;
        private OptionBool iOptionMulticast;
        private OptionUint iOptionChannel;
        private OptionBool iOptionUseMusicLatency;
        private OptionUint iOptionMusicLatencyMs;
        private OptionUint iOptionVideoLatencyMs;
        private OptionBool iOptionRotaryVolumeControl;
        private OptionPagePrivacy iOptionPagePrivacy;
        private string iTrackerAccount;
    }


    // Classes to help in the serialisation of the receiver list into XML

    [XmlRoot("MediaPlayerConfiguration")]
    [XmlInclude(typeof(MediaPlayer))]
    public class MediaPlayerConfiguration
    {
        public static MediaPlayerConfiguration Create(string aXml)
        {
            XmlSerializer xml = new XmlSerializer(typeof(MediaPlayerConfiguration));
            TextReader reader = new StringReader(aXml);
            MediaPlayerConfiguration config = (MediaPlayerConfiguration)xml.Deserialize(reader);
            reader.Close();
            return config;
        }

        public MediaPlayerConfiguration()
        {
            List = new List<MediaPlayer>();
        }

        public MediaPlayerConfiguration(Receiver[] aReceiverList)
        {
            List = new List<MediaPlayer>();

            foreach (Receiver recv in aReceiverList)
            {
                List.Add(new MediaPlayer(recv));
            }
        }

        public string Save()
        {
            XmlSerializer xml = new XmlSerializer(typeof(MediaPlayerConfiguration));
            StringWriter writer = new StringWriter();
            xml.Serialize(writer, this);
            writer.Close();
            return writer.ToString();
        }

        [XmlElement("MediaPlayerList")]
        public List<MediaPlayer> List;
    }

    public class MediaPlayer
    {
        public MediaPlayer()
        {
        }

        public MediaPlayer(Receiver aReceiver)
        {
            Udn = aReceiver.Udn;
            Room = aReceiver.Room;
            Group = aReceiver.Group;
            Name = aReceiver.Name;
        }

        [XmlElement("Udn")]
        public string Udn;

        [XmlElement("Room")]
        public string Room;

        [XmlElement("Group")]
        public string Group;

        [XmlElement("Name")]
        public string Name;
    }
}

