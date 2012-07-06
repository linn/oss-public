using System;
using System.Xml;
using System.Threading;
using System.Collections.Generic;

using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

namespace Linn.ProductSupport
{
    public class BasicSetup
    {
        public event EventHandler<EventArgsSetupError> EventSetupError;
        public event EventHandler<EventArgsSetupValue> EventSetupValueChanged {
            add {
                iEventSetupValueChanged += value;
                lock (iLock)
                {
                    if (iInitialInfoReady)
                    {
                        value(this, iEventArgsSetupValue);
                    }
                }
            }
            remove {
                iEventSetupValueChanged -= value;
            }
        }
        private EventHandler<EventArgsSetupValue> iEventSetupValueChanged;

        internal BasicSetup(Device aDevice, EventServerUpnp aEventServer) {
            iLock = new object();
            if (aDevice.HasService(ServiceConfiguration.ServiceType(1)) > 0)
            {
                iServiceConfiguration = new ServiceConfiguration(aDevice, aEventServer);
                iServiceConfiguration.EventInitial += EventHandlerConfigurationInitial;
                iActionSetParameter = iServiceConfiguration.CreateAsyncActionSetParameter();
                iActionSetParameter.EventError += SetParameterError;
            }

            if (aDevice.HasService(ServiceVolkano.ServiceType(1)) > 0) {
                iServiceVolkano = new ServiceVolkano(aDevice);
                iActionReboot = iServiceVolkano.CreateAsyncActionReboot();
            }
            iDeviceIpAddress = aDevice.IpAddress;
        }

        public void Close() {
            lock (iLock)
            {
                if (iServiceConfiguration != null)
                {
                    iServiceConfiguration.EventInitial -= EventHandlerConfigurationInitial;
                    iServiceConfiguration.EventStateParameterXml -= EventHandlerParameterXmlChange;
                    iServiceConfiguration.Close();
                    iServiceConfiguration = null;
                }
                if (iServiceVolkano != null)
                {
                    iServiceVolkano.Close();
                    iServiceVolkano = null;
                }
            }
        }

        public void Kill()
        {
            lock (iLock)
            {
                if (iServiceConfiguration != null)
                {
                    iServiceConfiguration.EventInitial -= EventHandlerConfigurationInitial;
                    iServiceConfiguration.EventStateParameterXml -= EventHandlerParameterXmlChange;
                    iServiceConfiguration.Kill();
                    iServiceConfiguration = null;
                }
                if (iServiceVolkano != null)
                {
                    iServiceVolkano.Kill();
                    iServiceVolkano = null;
                }
            }
        }

        public void Reboot() {
            iActionReboot.RebootBegin();
        }

        public void SetRoom(string aRoom) {
            iActionSetParameter.SetParameterBegin(Parameter.kTargetDevice, Parameter.kNameRoom, aRoom);
        }

        public void SetSourceName(string aSourceSystemName, string aName) {
            iActionSetParameter.SetParameterBegin(aSourceSystemName, Parameter.kNameSourceName, aName);
        }

        public void SetSourceIcon(string aSourceSystemName, string aIconName) {
            iActionSetParameter.SetParameterBegin(aSourceSystemName, Parameter.kNameSourceIconName, aIconName);
        }

        public void SetSourceVisible(string aSourceSystemName, bool aVisible) {
            iActionSetParameter.SetParameterBegin(aSourceSystemName, Parameter.kNameSourceVisible, Parameter.String(aVisible));
        }

        public void SetStartupSource(string aSourceSystemName, bool aEnabled) {
            iActionSetParameter.SetParameterBegin(Parameter.kTargetDevice, Parameter.kNameStartupSourceEnabled, Parameter.String(aEnabled));

            SourceInfo source;
            if (iSourceInfoList.TryGetValue(aSourceSystemName, out source)) {
                iActionSetParameter.SetParameterBegin(Parameter.kTargetDevice, Parameter.kNameStartupSourceIndex, source.Index.ToString());
            }
        }

        public SourceInfo SourceInfoAt(string aSourceSystemName) {
            SourceInfo source = null;
            iSourceInfoList.TryGetValue(aSourceSystemName, out source);
            return source;
        }

        public void SetStartupVolume(uint aVolume) {
            iActionSetParameter.SetParameterBegin(Parameter.kTargetVolume, Parameter.kNameStartupVolume, aVolume.ToString());
        }

        public void SetStartupVolumeEnabled(bool aEnabled) {
            iActionSetParameter.SetParameterBegin(Parameter.kTargetVolume, Parameter.kNameStartupVolumeEnabled, Parameter.String(aEnabled));
        }

        public void SetAutoPlay(bool aEnabled) {
            iActionSetParameter.SetParameterBegin(Parameter.kTargetDevice, Parameter.kNameAutoPlayEnabled, Parameter.String(aEnabled));
        }

        public void SetTuneInUsername(string aUsername) {

            string username = null;
            if (!Parameter.kDictionaryTuneInUsernameAllowedValues.TryGetValue(aUsername, out username)) {
                username = aUsername;
            }

            iActionSetParameter.SetParameterBegin(Parameter.kTargetTuneIn, Parameter.kNameTuneInUsername, username);
        }

        private void SetParameterError(object obj, EventArgsError e) {
            string errorMessage = "Set Parameter Error: " + e.Code + ", " + e.Description;
            UserLog.WriteLine(errorMessage);

            if (EventSetupError != null) {
                EventSetupError(this, new EventArgsSetupError(errorMessage));
            }
        }

        private void EventHandlerConfigurationInitial(object obj, EventArgs e) {
            lock (iLock)
            {
                if (obj == iServiceConfiguration)
                {
                    iServiceConfiguration.EventStateParameterXml += EventHandlerParameterXmlChange;

                    EventHandlerParameterXmlChange(this, EventArgs.Empty);
                }
            }
        }

        private void EventHandlerParameterXmlChange(object obj, EventArgs e) {
            UserLog.WriteLine("Event ParameterXml Changed");
            try {
                lock (iLock)
                {
                    ParseParameterXml(iServiceConfiguration.ParameterXml);
                    if (iEventSetupValueChanged != null)
                    {
                        iEventSetupValueChanged(this, iEventArgsSetupValue);
                    }
                }
            }
            catch (ControlPoint.ServiceException) {
                // device has gone
            }
        }

        private void ParseParameterXml(string aParameterXml) {
            XmlDocument document = new XmlDocument();
            document.LoadXml(aParameterXml);
            bool startupSourceEnabled = false;
            int startupSourceIndex = -1;

            foreach (XmlNode n in document.SelectNodes("/ParameterList/Parameter")) {
                string target = n["Target"].InnerText;
                string name = n["Name"].InnerText;
                string collection = (n["Collection"] != null ? n["Collection"].InnerText : String.Empty);
                XmlNodeList allowedValuesXml = (n["AllowedValueList"] != null ? n["AllowedValueList"].ChildNodes : null);
                List<string> allowedValues = new List<string>();
                if (allowedValuesXml != null) {
                    foreach (XmlNode av in allowedValuesXml) {
                        allowedValues.Add(av.InnerText);
                    }
                }

                bool parseOk = true;

                if (target == Parameter.kTargetDevice) {
                    if (name == Parameter.kNameRoom) {
                        iRoom = n[kXmlValue].InnerText;
                    }
                    else if (name == Parameter.kNameAutoPlayEnabled) {
                        iAutoPlay = (n[kXmlValue].InnerText == Parameter.kValueTrue);
                    }
                    else if (name == Parameter.kNameStartupSourceIndex) {
                        parseOk = int.TryParse(n[kXmlValue].InnerText, out startupSourceIndex);
                    }
                    else if (name == Parameter.kNameStartupSourceEnabled) {
                        startupSourceEnabled = (n[kXmlValue].InnerText == Parameter.kValueTrue);
                    }
                }
                else if (target == Parameter.kTargetVolume) {
                    if (name == Parameter.kNameStartupVolume) {
                        parseOk = uint.TryParse(n[kXmlValue].InnerText, out iStartupVolume);
                    }
                    else if (name == Parameter.kNameStartupVolumeEnabled) {
                        iStartupVolumeEnabled = (n[kXmlValue].InnerText == Parameter.kValueTrue);
                    }
                }
                else if (target == Parameter.kTargetTuneIn) {
                    if (name == Parameter.kNameTuneInUsername) {
                        iTuneInUsername = (n[kXmlValue].InnerText);
                    }
                }
                else if (collection == Parameter.kCollectionSources) {
                    SourceInfo source;
                    if (!iSourceInfoList.TryGetValue(target, out source)) {
                        source = new SourceInfo(target);
                        source.Index = iSourceInfoList.Count;
                        iSourceInfoList[target] = source;
                    }

                    if (name == Parameter.kNameSourceName) {
                        source.Name = (n[kXmlValue].InnerText);
                    }
                    else if (name == Parameter.kNameSourceVisible) {
                        source.Visible = (n[kXmlValue].InnerText == Parameter.kValueTrue);
                    }
                    else if (name == Parameter.kNameSourceIconName) {
                        source.Icon.Set(n[kXmlValue].InnerText, iDeviceIpAddress);
                        source.DefaultIcon = new SourceIcon(source.SystemName, iDeviceIpAddress);

                        if (source.AllowedIcons.Count < allowedValues.Count) {
                            foreach (string iconName in allowedValues) {
                                source.AllowedIcons.Add(new SourceIcon(iconName, iDeviceIpAddress));
                            }
                        }
                    }
                }

                if (!parseOk) {
                    UserLog.WriteLine("The following parameter could not be parsed correctly..." + Environment.NewLine + "Target: " + target + Environment.NewLine + "Name: " + name + Environment.NewLine + "Value: " + n[kXmlValue].InnerText);
                }
            }

            foreach (SourceInfo source in iSourceInfoList.Values) {
                source.IsStartup = (startupSourceIndex >= 0 && startupSourceEnabled && source.Index == startupSourceIndex);
            }

            if (iTuneInUsername.Length > 0 && Parameter.kDictionaryTuneInUsernameAllowedValues.ContainsValue(iTuneInUsername)) {
                foreach (string key in Parameter.kDictionaryTuneInUsernameAllowedValues.Keys) {
                    if (Parameter.kDictionaryTuneInUsernameAllowedValues[key] == iTuneInUsername) {
                        iTuneInUsername = key;
                        break;
                    }
                }
            }

            iEventArgsSetupValue.Room = iRoom;
            iEventArgsSetupValue.SourceInfoList = iSourceInfoList.Values;
            iEventArgsSetupValue.StartupVolume = iStartupVolume;
            iEventArgsSetupValue.StartupVolumeEnabled = iStartupVolumeEnabled;
            iEventArgsSetupValue.AutoPlay = iAutoPlay;
            iEventArgsSetupValue.TuneInUsername = iTuneInUsername;
            iInitialInfoReady = true;
        }

        private string iRoom = "";
        private SortedList<string, SourceInfo> iSourceInfoList = new SortedList<string, SourceInfo>(); // key = Source System Name (string), value = SourceInfo object
        private uint iStartupVolume = 0;
        private bool iStartupVolumeEnabled = false;
        private bool iAutoPlay = false;
        private string iTuneInUsername = "";
        private const string kXmlValue = "Value";
        private string iDeviceIpAddress = null;

        private ServiceConfiguration iServiceConfiguration = null;
        private ServiceConfiguration.AsyncActionSetParameter iActionSetParameter;
        private ServiceVolkano iServiceVolkano = null;
        private ServiceVolkano.AsyncActionReboot iActionReboot;
        private EventArgsSetupValue iEventArgsSetupValue = new EventArgsSetupValue();
        private bool iInitialInfoReady = false;
        private object iLock;
    }

    public class SourceInfo
    {
        public SourceInfo(string aSystemName) {
            SystemName = aSystemName;
            Name = "";
            Visible = false;
            Index = -1;
            IsStartup = false;
            Icon = new SourceIcon();
            DefaultIcon = new SourceIcon();
            AllowedIcons = new List<SourceIcon>();
        }

        public string SystemName { get; set; }
        public string Name { get; set; }
        public bool Visible { get; set; }
        public int Index { get; set; }
        public bool IsStartup { get; set; }
        public SourceIcon Icon { get; set; }
        public SourceIcon DefaultIcon { get; set; }
        public List<SourceIcon> AllowedIcons { get; set; }
    }

    public class SourceIcon
    {
        public SourceIcon() {
            iName = null;
            iImageUri = null;
        }

        public SourceIcon(string aName, string aDeviceIpAddress) {
            Set(aName, aDeviceIpAddress);
        }

        public void Set(string aName, string aDeviceIpAddress) {
            iName = aName;

            if (aName == null || aDeviceIpAddress == null) {
                iImageUri = null;
            }
            else {
                string condensedName = aName.Replace(" ", "").Replace("/", "");
                iImageUri = "http://" + aDeviceIpAddress + "/images/SourceIcons/" + condensedName + ".jpg";
            }
        }

        public string Name {
            get {
                return iName;
            }
        }

        public string ImageUri {
            get {
                return iImageUri;
            }
        }

        private string iName;
        private string iImageUri;

        // Source Names
        private const string kSourceNamePlaylist = "Playlist";
        private const string kSourceNameUpnpAv = "UPnP AV";
        private const string kSourceNameRadio = "Radio";
        private const string kSourceNameSongcast = "Songcast";
        private const string kSourceNameNetAux = "Net Aux";
        private const string kSourceNameFrontAux = "Front Aux";
        private const string kSourceNameAnalog = "Analog";
        private const string kSourceNameSpdif = "SPDIF";
        private const string kSourceNameToslink = "TOSLINK";
        private const string kSourceNameHdmi1 = "HDMI1";
        private const string kSourceNameHdmi2 = "HDMI2";
        private const string kSourceNameHdmi3 = "HDMI3";
        // Source Icon names
        private const string kSourceIconNameAnalog = kSourceNameAnalog;
        private const string kSourceIconNameCable = "Cable";
        private const string kSourceIconNameComputer = "Computer";
        private const string kSourceIconNameDisc = "Disc Player";
        private const string kSourceIconNameFrontAux = kSourceNameFrontAux;
        private const string kSourceIconNameController = "Games Console";
        private const string kSourceIconNameHdmi1 = kSourceNameHdmi1;
        private const string kSourceIconNameHdmi2 = kSourceNameHdmi2;
        private const string kSourceIconNameHdmi3 = kSourceNameHdmi3;
        private const string kSourceIconNameIpad = "iPad / Tablet";
        private const string kSourceIconNameIphone = "iPhone / Smartphone";
        private const string kSourceIconNameIpod = "iPod / MP3 Player";
        private const string kSourceIconNameMovies = "Movies";
        private const string kSourceIconNameMusic = "Music";
        private const string kSourceIconNameNetAux = kSourceNameNetAux;
        private const string kSourceIconNamePlaylist = kSourceNamePlaylist;
        private const string kSourceIconNamePlaystation = "Playstation";
        private const string kSourceIconNameRadio = kSourceNameRadio;
        private const string kSourceIconNameSatellite = "Satellite";
        private const string kSourceIconNameSongcast = kSourceNameSongcast;
        private const string kSourceIconNameSpdif = kSourceNameSpdif;
        private const string kSourceIconNameToslink = kSourceNameToslink;
        private const string kSourceIconNameTurntable = "Turntable";
        private const string kSourceIconNameTv = "TV";
        private const string kSourceIconNameUpnpAv = kSourceNameUpnpAv;
        private const string kSourceIconNameWii = "Wii";
        private const string kSourceIconNameXbox = "XBOX";
    }

    public class EventArgsSetupValue : EventArgs
    {
        public EventArgsSetupValue() {
            Room = "";
            SourceInfoList = null;
            StartupVolume = 0;
            StartupVolumeEnabled = false;
            AutoPlay = false;
            TuneInUsername = "";
            TuneInUsernameAllowedValues = new List<string>(Parameter.kDictionaryTuneInUsernameAllowedValues.Keys);
        }

        public string Room { get; set; }
        public IList<SourceInfo> SourceInfoList { get; set; }
        public uint StartupVolume { get; set; }
        public bool StartupVolumeEnabled { get; set; }
        public bool AutoPlay { get; set; }
        public string TuneInUsername { get; set; }
        public List<string> TuneInUsernameAllowedValues { get; set; }
    }

    public class EventArgsSetupError : EventArgs
    {
        public EventArgsSetupError(string aErrorMessage) {
            iErrorMessage = aErrorMessage;
        }

        public string ErrorMessage {
            get { return iErrorMessage; }
        }

        private string iErrorMessage;
    }

    public class Parameter
    {
        // Parameter Values
        public const string kValueTrue = "true";
        public const string kValueFalse = "false";

        // Parameter Collections
        public const string kCollectionSources = "Sources";
        public const string kCollectionDelays = "Delay Presets";

        // Parameter Targets
        public const string kTargetDevice = "Device";
        public const string kTargetDisplay = "Display";
        public const string kTargetTuneIn = "TuneIn Radio";
        public const string kTargetJukebox = "Jukebox";
        public const string kTargetVolume = "Volume";
        public const string kTargetSender = "Songcast Sender";
        public const string kTargetRs232 = "RS232 Connections";
        public const string kTargetRem020Handset = "Slimline Handset";
        public const string kTargetHdmi = "HDMI";

        // Parameter Names (target=Device)
        public const string kNameStandbyDisabled = "Sleep Mode";
        public const string kNameAutoPlayEnabled = "Auto Play";
        public const string kNameBootIntoStandby = "Startup Mode";
        public const string kNameEnableInternalPowerAmp = "Internal Power Amplifier";
        public const string kNameAudioOutputMono = "Audio Output Mode";
        public const string kNameRoom = "Room";
        public const string kNameName = "Name";
        public const string kNameStartupSourceIndex = "Startup Source";
        public const string kNameStartupSourceEnabled = "Startup Source Enabled";
        public const string kNameDigitalOutputMode = "Digital Output Mode";
        public const string kNameHandsetCommandsAccepted = "Handset Commands Accepted";
        public const string kNameBasik3CommandsAccepted = "Basik 3 Commands Accepted";
        public const string kNameCurrentDelayPreset = "Current Delay Preset";
        public const string kNameOutputModeRca = "Analog Output Mode";
        public const string kNameHdmi = "HDMI";
        // Parameter Names (target=<Source System Name>, collection=Sources)
        public const string kNameSourceName = "Name";
        public const string kNameSourceVisible = "Visible";
        public const string kNameSourceIconName = "Icon";
        public const string kNameSourceDelayMode = "Delay Mode";
        public const string kNameSourceVolumeOffset = "Volume Offset";
        public const string kNameSourceUnityGain = "Unity Gain";
        public const string kNameSourceAdcInputLevel = "Input Level";
        public const string kNameSourceTransformerEnabled = "Transformer";
        public const string kNameNetAuxAutoSwitchEnable = "Auto Select";
        // Parameter Names (target=TuneIn Radio)
        public const string kNameTuneInUsername = "Username";
        public const string kNameTuneInPassword = "Password";
        public const string kNameTuneInTestMode = "Test Mode";
        // Parameter Names (target=<PresetX>, collection=Delay Presets)
        public const string kDelayPresetPrefix = "Preset";
        public const string kNameDelayPresetName = "Name";
        public const string kNameDelayPresetVisible = "Visible";
        public const string kNameDelayPresetDelay = "Delay";
        // Parameter Names (target=Jukebox)
        public const string kNameJukeboxPresetPrefix = "Presets Folder URL";
        public const string kNameJukeboxAutoLoadEnabled = "Auto Load";
        // Parameter Names (target=Volume)
        public const string kNameEnableInternalVolumeControl = "Internal Volume Control";
        public const string kNameVolumeLimit = "Volume Limit";
        public const string kNameStartupVolume = "Startup Volume";
        public const string kNameStartupVolumeEnabled = "Startup Volume Enabled";
        public const string kNameHeadphoneVolumeOffset = "Headphone Volume Offset";
        public const string kNameBalance = "Balance";
        public const string kNameAnalogAttenuation = "Output Attenuation";
        // Parameter Names (target=Songcast Sender)
        public const string kNameSenderMulticast = "Output Mode";
        public const string kNameSenderEnabled = "Enabled";
        public const string kNameSenderPreset = "Preset";
        public const string kNameSenderChannel = "Multicast Channel";
        // Parameter Names (target=RS232 Connections)
        public const string kNameKontrolProductConnected = "Linn Preamp";
        public const string kNameKontrolProductComPort = "Linn Preamp Com Port";
        public const string kNameDiscPlayerConnected = "Linn Disc Player";
        public const string kNameDiscPlayerComPort = "Linn Disc Player Com Port";
        // Parameter Names (target=Display)
        public const string kNameDisplayFlipOrientation = "Orientation";
        public const string kNameDisplayScrollText = "Scroll Text on Track Change";
        public const string kNameDisplayFrontLedOffStandby = "Turn Front Led Off in Sleep";
        public const string kNameDisplayAutoBrightness = "Auto Brightness";
        public const string kNameDisplayBrightness = "Brightness";
        public const string kNameDisplaySleep = "Sleep Mode";
        public const string kNameUpdateNotifications = "Update Notifications";
        public const string kNameDisplayOrientationLed = "Linn Logo LED";
        // Parameter Names (target=Slimline Handset)
        public const string kNameDirectSource1 = "Input 1 Button";
        public const string kNameDirectSource2 = "Input 2 Button";
        public const string kNameDirectSource3 = "Input 3 Button";
        // Parameter Names (target=HDMI)
        public const string kNameHdmiOffInSleep = "Turn Off in Sleep";
        public const string kNameHdmiMode = "Audio Mode";
        public const string kNameHdmiMixCentreChannel = "Downmix Centre To Fronts";
        public const string kNameHdmiMixLfeChannel = "Downmix LFE To Fronts";
        public const string kNameHdmiDownstreamVolumeOffset = "AV Volume Offset";
        public const string kNameHdmiAvLatency = "AV Latency";

        // Parameter Units
        public const string kUnitsDb = "dB";
        public const string kUnitsMs = "ms";
        public const string kUnitsVrms = "Vrms";

        // Konfig only
        public const string kSelectSender = "Songcast Sender to Listen To";

        // TuneIn dropdown options
        public static Dictionary<string, string> kDictionaryTuneInUsernameAllowedValues = new Dictionary<string, string>() { // key = Country, value = corresponding username
            {"User-defined...", ""},
            {"Worldwide (Default)", "linnproducts"},
            {"Australia", "linnproducts-australia"},
            {"Belgium (Flemish)", "linnproducts-belgium-vlaanderen"},
            {"Belgium (French)", "linnproducts-belgique"},
            {"France", "linnproducts-france"},
            {"Germany", "linnproducts-germany"},
            {"Germany (South)", "linnproducts-germany-south"},
            {"Gibraltar", "linnproducts-gibraltar"},
            {"Japan", "linnproducts-japan"},
            {"Netherlands", "linnproducts-netherlands"},
            {"New Zealand", "linnproducts-newzealand"},
            {"Poland", "linnproducts-poland"},
            {"Portugal", "linnproducts-portugal"},
            {"Russia", "linnproducts-russia"},
            {"Spain", "linnproducts-spain"},
            {"Switzerland", "linnproducts-switzerland"},
            {"UK", "linnproducts-uk"},
            {"USA", "linnproducts-usa"}
        };

        // Retrieve known parameter value (must know parameter target and name) from parameter xml
        public static string GetParameterValue(string aParameterXml, string aTarget, string aName) {
            XmlDocument document = new XmlDocument();
            document.LoadXml(aParameterXml);

            foreach (XmlNode n in document.SelectNodes("/ParameterList/Parameter")) {
                string target = n["Target"].InnerText;
                string name = n["Name"].InnerText;

                if (target == aTarget && name == aName) {
                    return (n["Value"].InnerText);
                }
            }
            throw new Exception("Paramater not found");
        }

        public static string String(bool aValue) {
            return (aValue ? Parameter.kValueTrue : Parameter.kValueFalse);
        }
    }
}