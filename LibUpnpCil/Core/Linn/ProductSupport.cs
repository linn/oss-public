using System;
using System.Xml;
using System.Globalization;

namespace Linn
{
    public class ProductSupport
    {

        public static int CompareVersions(string aVersion, string aCompareVersion)
        {
            uint family = FamilyNumber(aVersion);
            uint family2 = FamilyNumber(aCompareVersion);
            if (family != family2)
            {
                return family < family2 ? -1 : 1;
            }
            else
            {
                uint major = Release(aVersion);
                uint major2 = Release(aCompareVersion);
                if (major < major2)
                {
                    return -1;
                }
                else if (major > major2)
                {
                    return 1;
                }
            }
            return Build(aVersion).CompareTo(Build(aCompareVersion));
        }

        public static int CompareProxyVersions(string aVersion, string aCompareVersion) {
            string currVersion = aCompareVersion.ToLower(CultureInfo.InvariantCulture);
            if (currVersion.StartsWith("sp")) {
                currVersion = currVersion.Remove(0, currVersion.IndexOf("v") + 1);
            }
            else if (currVersion.StartsWith("s")) {
                currVersion = currVersion.Remove(0, 1);
            }
            string latestVersion = aVersion.ToLower(CultureInfo.InvariantCulture);
            if (latestVersion.StartsWith("sp")) {
                latestVersion = latestVersion.Remove(0, currVersion.IndexOf("v") + 1);
            }
            else if (latestVersion.StartsWith("s")) {
                latestVersion = latestVersion.Remove(0, 1);
            }
            try {
                uint current = uint.Parse(currVersion);
                uint latest = uint.Parse(latestVersion);
                if (current < latest) {
                    return 1;
                }
            }
            catch(FormatException) { }
            return -1;
        }

        public static ProductSupport Instance
        {
            get
            {
                if (iInstance == null)
                {
                    iInstance = new ProductSupport();
                }
                return iInstance;
            }
        }

        public static string Family(string aSoftwareVersion)
        {
            string[] split = aSoftwareVersion.Split('.');
            if (split.Length > 0)
            {
                int index = int.Parse(split[0]) - 1;
                if (index >= 0 && index < ProductSupport.Instance.kFamilyNames.Length)
                {
                    return ProductSupport.Instance.kFamilyNames[index];
                }
            }
            return kFamilyUnknown;
        }

        public static uint FamilyNumber(string aSoftwareVersion)
        {
            string[] split = aSoftwareVersion.Split('.');
            if (split.Length > 0)
            {
                return uint.Parse(split[0]);
            }
            return 0;
        }

        public static uint Release(string aSoftwareVersion)
        {
            string[] split = aSoftwareVersion.Split('.');
            if (split.Length > 1)
            {
                return uint.Parse(split[1]);
            }
            return 0;
        }

        public static uint Build(string aSoftwareVersion)
        {
            string[] split = aSoftwareVersion.Split('.');
            if (split.Length > 2)
            {
                return uint.Parse(split[2]);
            }
            return 0;
        }

        public static string SoftwareVersionPretty(string aSoftwareVersion, bool aIncludeFull)
        {
            if (aSoftwareVersion == kDefaultNightlyVersion) {
                return kFamilyNightly;
            }
            else if (aSoftwareVersion != null && aSoftwareVersion.Contains(".") && !aSoftwareVersion.Contains("-"))
            {
                string result = Family(aSoftwareVersion) + " " + Release(aSoftwareVersion);
                if (aIncludeFull)
                {
                    result += " (" + aSoftwareVersion + ")";
                }
                return result;
            }
            else
            {
                return aSoftwareVersion;
            }
        }

        public static string GetParameterValue(string aParameterXml, string aTarget, string aName)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(aParameterXml);

            foreach (XmlNode n in document.SelectNodes("/ParameterList/Parameter"))
            {
                string target = n["Target"].InnerText;
                string name = n["Name"].InnerText;

                if (target == aTarget && name == aName)
                {
                    return (n["Value"].InnerText);
                }
            }
            throw new Exception("Paramater not found");
        }

        // new software naming convention: Family.Version.BuildNumber (3.2.37 = Cara 2 build 37)
        public readonly string[] kFamilyNames = new string[] { "Auskerry", "Bute", "Cara", "Davaar" };
        public const string kFamilyUnknown = "Unknown";
        public const string kFamilyNightly = "Nightly Build";
        public const string kDefaultNightlyVersion = "9999.9999.9999";

        public const string kModelKlimaxDs = "Klimax DS";
        public const string kModelKlimaxDsm = "Klimax DSM";
        public const string kModelAkurateDs = "Akurate DS";
        public const string kModelAkurateDsm = "Akurate DSM";
        public const string kModelMajikDs = "Majik DS";
        public const string kModelSneakyMusicDs = "Sneaky Music DS";
        public const string kModelSekritDsi = "Sekrit DS-I";
        public const string kModelMajikDsi = "Majik DS-I";
        public const string kModelMajikDsm = "Majik DSM";
        public const string kModelRenewDs = "Renew DS";
        public const string kModelConceptDs = "Concept DS";
        public const string kModelConceptDsm = "Concept DSM";
        public const string kModelProxyNone = "None";
        public const string kModelCd12 = "CD12";
        public const string kModelAkurateCd = "Akurate CD";
        public const string kModelUnidisk1_1 = "Unidisk 1.1";
        public const string kModelUnidisk2_1 = "Unidisk 2.1";
        public const string kModelUnidiskSC = "Unidisk SC";
        public const string kModelMajikCd = "Majik CD";
        public const string kModelClassikMovie = "Classik Movie";
        public const string kModelClassikMusic = "Classik Music";
        public const string kModelKlimaxKontrol = "Klimax Kontrol";
        public const string kModelAkurateKontrol = "Akurate Kontrol";
        public const string kModelKisto = "Kisto";
        public const string kModelKinos = "Kinos";
        public const string kModelMajikKontrol = "Majik Kontrol";
        public const string kModelMajikI = "Majik-I";
        public const string kModelRoomAmp2 = "Roomamp 2";

        static private ProductSupport iInstance = null;
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
        public const string kNameSourceDelayMode = "Delay Mode";
        public const string kNameSourceVolumeOffset = "Volume Offset";
        public const string kNameSourceUnityGain = "Unity Gain";
        public const string kNameSourceAdcInputLevel = "Input Level";
        public const string kNameSourceTransformerEnabled = "Transformer";
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
        // Parameter Names (target=Slimline Handset)
        public const string kNameDirectSource1 = "Input 1 Button";
        public const string kNameDirectSource2 = "Input 2 Button";
        public const string kNameDirectSource3 = "Input 3 Button";
        // Parameter Names (target=HDMI)
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
    }
} // Linn