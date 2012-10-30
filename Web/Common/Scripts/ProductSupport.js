if (typeof(Linn) == "undefined"){
    Linn = {};
}

Linn.ProductSupport = 
{
    kModelKlimaxDs:"Klimax DS",
    kModelKlimaxDsm:"Klimax DSM",
    kModelRenewDs:"Renew DS",
    kModelAkurateDs:"Akurate DS",
    kModelAkurateDsm:"Akurate DSM",
    kModelMajikDs:"Majik DS",
    kModelSneakyMusicDs:"Sneaky Music DS",
    kModelSekritDsi:"Sekrit DS-I",
    kModelMajikDsi:"Majik DS-I",
    kModelMajikDsm:"Majik DSM",
    kModelKikoDsm:"Kiko DSM",
	kModelMusikbox:"Musikbox",
	kModelSekritDsm:"Sekrit DSM",
    kModelConceptDs:"Concept DS",
    kModelConceptDsm:"Concept DS-M",
    kModelProxyNone:"None",
    kModelCd12:"CD12",
    kModelAkurateCd:"Akurate CD",
    kModelUnidisk1_1:"Unidisk 1.1",
    kModelUnidisk2_1:"Unidisk 2.1",
    kModelUnidiskSC:"Unidisk SC",
    kModelMajikCd:"Majik CD",
    kModelClassikMovie:"Classik Movie",
    kModelClassikMusic:"Classik Music",
    kModelKlimaxKontrol:"Klimax Kontrol",
    kModelAkurateKontrol:"Akurate Kontrol",
    kModelKisto:"Kisto",
    kModelKinos:"Kinos",
    kModelMajikKontrol:"Majik Kontrol",
    kModelMajikI:"Majik-I",
    kModelRoomAmp2:"Roomamp 2",
    
    kSourceNamePlaylist:"Playlist",
    kSourceNameRadio:"Radio",
    kSourceNameSongcast:"Songcast",
    kSourceNameUpnpAv:"UpnpAv",
    kSourceNameAirplay:"Net Aux"
}

Linn.Parameter = 
{
    // Parameter Values
    kValueTrue:"true",
    kValueFalse:"false",

    // Parameter Collections
    kCollectionSources:"Sources",
    kCollectionDelays:"Delay Presets",

    // Parameter Targets
    kTargetDevice:"Device",
    kTargetDisplay:"Display",
    kTargetTuneIn:"TuneIn Radio",
    kTargetJukebox:"Jukebox",
    kTargetVolume:"Volume",
    kTargetSender:"Songcast Sender",
    kTargetRs232:"RS232 Connections",
    kTargetRem020Handset:"Slimline Handset",
    kTargetHdmi:"HDMI",

    // Parameter Names (target=Device)
    kNameStandbyDisabled:"Sleep Mode",
    kNameAutoPlayEnabled:"Auto Play",
    kNameBootIntoStandby:"Startup Mode",
    kNameEnableInternalPowerAmp:"Internal Power Amplifier",
    kNameAudioOutputMono:"Audio Output Mode",
    kNameRoom:"Room",
    kNameName:"Name",
    kNameStartupSourceIndex:"Startup Source",
    kNameStartupSourceEnabled:"Startup Source Enabled",
    kNameDigitalOutputMode:"Digital Output Mode",
    kNameHandsetCommandsAccepted:"Handset Commands Accepted",
    kNameBasik3CommandsAccepted:"Basik 3 Commands Accepted",
    kNameCurrentDelayPreset:"Current Delay Preset",
    kNameOutputModeRca:"Analog Output Mode",
    kNameHdmi:"HDMI",
    kNameEthernetLeds:"Ethernet LEDs",
	kNameAmplifierMode:"Internal Power Amplifier Mode",
    // Parameter Names (target=<Source System Name>, collection=Sources)
    kNameSourceName:"Name",
    kNameSourceVisible:"Visible",
    kNameSourceIconName:"Icon", 
    kNameSourceDelayMode:"Delay Mode",
    kNameSourceVolumeOffset:"Volume Offset",
    kNameSourceUnityGain:"Unity Gain",
    kNameSourceAdcInputLevel:"Input Level",
    kNameSourceTransformerEnabled:"Transformer",
    kNameNetAuxAutoSwitchEnable:"Auto Select",
    // Parameter Names (target=TuneIn Radio)
    kNameTuneInUsername:"Username",
    kNameTuneInPassword:"Password",
    kNameTuneInTestMode:"Test Mode",
    // Parameter Names (target=<PresetX>, collection=Delay Presets)
    kDelayPresetPrefix:"Preset",
    kNameDelayPresetName:"Name",
    kNameDelayPresetVisible:"Visible",
    kNameDelayPresetDelay:"Delay",
    // Parameter Names (target=Jukebox)
    kNameJukeboxPresetPrefix:"Presets Folder URL",
    kNameJukeboxAutoLoadEnabled:"Auto Load",
    // Parameter Names (target=Volume)
    kNameEnableInternalVolumeControl:"Internal Volume Control",
    kNameVolumeLimit:"Volume Limit",
    kNameStartupVolume:"Startup Volume",
    kNameStartupVolumeEnabled:"Startup Volume Enabled",
    kNameHeadphoneVolumeOffset:"Headphone Volume Offset",
    kNameBalance:"Balance",
    kNameAnalogAttenuation:"Output Attenuation",
    // Parameter Names (target=Songcast Sender)
    kNameSenderMulticast:"Output Mode",
    kNameSenderEnabled:"Enabled",
    kNameSenderPreset:"Preset",
    kNameSenderChannel:"Multicast Channel",
    // Parameter Names (target=RS232 Connections)
    kNameKontrolProductConnected:"Linn Preamp",
    kNameKontrolProductComPort:"Linn Preamp Com Port",
    kNameDiscPlayerConnected:"Linn Disc Player",
    kNameDiscPlayerComPort:"Linn Disc Player Com Port",
    // Parameter Names (target=Display)
    kNameDisplayFlipOrientation:"Orientation",
    kNameDisplayScrollText:"Scroll Text on Track Change",
    kNameDisplayFrontLedOffStandby:"Turn Front Led Off in Sleep",
    kNameDisplayAutoBrightness:"Auto Brightness",
    kNameDisplayBrightness:"Brightness",
    kNameDisplaySleep:"Sleep Mode",
    kNameDisplayOrientationLed:"Linn Logo LED",
    kNameDisplayOrientationLedStandby:"Linn Logo LED (Sleep Mode)",
    kNameUpdateNotifications:"Update Notifications",
    // Parameter Names (target=Slimline Handset)
    kNameDirectSource1:"Input 1 Button",
    kNameDirectSource2:"Input 2 Button",
    kNameDirectSource3:"Input 3 Button",
    // Parameter Names (target=HDMI)
    kNameHdmiOffInSleep:"Turn Off in Sleep",
	kNameHdmiMode:"Audio Mode",
	kNameHdmiMixCentreChannel:"Downmix Centre To Fronts",
	kNameHdmiMixLfeChannel:"Downmix LFE To Fronts",
	kNameHdmiDownstreamVolumeOffset:"AV Volume Offset",
	kNameHdmiAvLatency:"AV Latency",

    // Parameter Units
    kUnitsDb:"dB",
    kUnitsMs:"ms",
    kUnitsVrms:"Vrms",
    
    // Parameter Values
    kValueMulticast:"Multicast",
    kValueSurroundMode:"Surround"
}


        