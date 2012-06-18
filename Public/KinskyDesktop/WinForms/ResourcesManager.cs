namespace KinskyDesktop.Properties {
    using System;

    internal class Resources {

        private static System.Resources.ResourceManager resourceMan;
        private static System.Globalization.CultureInfo resourceCulture;

        internal Resources() {
        }

        public static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("KinskyDesktop.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        public static System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        private static string iBasePath;
        internal static void SetBasePath(string aBasePath) {
            iBasePath = aBasePath;
        }

        internal static object GetObject(string aName) {
            string filename = aName + ".png";
            string fullpath = System.IO.Path.Combine(System.IO.Path.Combine(iBasePath, "../../share/Linn/Resources/Kinsky"), filename);
            if (System.IO.File.Exists(fullpath))
            {
                return System.Drawing.Image.FromFile(fullpath);
            }
            else
            {
                fullpath = System.IO.Path.Combine(System.IO.Path.Combine(iBasePath, "../share/Linn/Resources/Kinsky"), filename);
                if(System.IO.File.Exists(fullpath))
                {
                    return System.Drawing.Image.FromFile(fullpath);
                }
                else
                {
                    fullpath = System.IO.Path.Combine(System.IO.Path.Combine(iBasePath, "Resources/Kinsky"), filename);
                    return System.Drawing.Image.FromFile(fullpath);
                }
            }
        }
        
        internal static System.Drawing.Bitmap AboutBox {
            get {
                object obj = GetObject("AboutBox");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap AdornmentAmber
        {
            get
            {
                object obj = GetObject("AdornmentAmber");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap AdornmentGreen
        {
            get
            {
                object obj = GetObject("AdornmentGreen");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap AdornmentRed
        {
            get
            {
                object obj = GetObject("AdornmentRed");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Bin {
            get {
                object obj = GetObject("Bin");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap BinMouse {
            get {
                object obj = GetObject("BinMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap BinTouch {
            get {
                object obj = GetObject("BinTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap BusyIcon
        {
            get
            {
                object obj = GetObject("HourGlass");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap BusyIconElement
        {
            get
            {
                object obj = GetObject("HourGlass2");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap FillerBottom {
            get {
                object obj = GetObject("FillerBottom");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap FillerTop {
            get {
                object obj = GetObject("FillerTop");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap IconAuxSource {
            get {
                object obj = GetObject("AuxSource");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
		
		internal static System.Drawing.Bitmap IconDiscSource {
            get {
                object obj = GetObject("CD");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
		
		internal static System.Drawing.Bitmap IconPlaylistSource {
            get {
                object obj = GetObject("DS");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconRadioSource {
            get {
                object obj = GetObject("Radio");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconSpdifSource {
            get {
                object obj = GetObject("Spdif");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconTosLinkSource {
            get {
                object obj = GetObject("TosLink");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
		
		internal static System.Drawing.Bitmap IconUpnpAvSource {
            get {
                object obj = GetObject("UPNP");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconAlbum {
            get {
                object obj = GetObject("Album");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconAlbumError {
            get {
                object obj = GetObject("AlbumError");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconArtist {
            get {
                object obj = GetObject("Artist");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconDirectory {
            get {
                object obj = GetObject("Directory");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconError {
            get {
                object obj = GetObject("Error");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconPlaylist {
            get {
                object obj = GetObject("Playlist");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconRadio {
            get {
                object obj = GetObject("Radio");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
		
		internal static System.Drawing.Bitmap IconRoom {
            get {
                object obj = GetObject("Room2");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconLibrary {
            get {
                object obj = GetObject("Library");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
 
        internal static System.Drawing.Bitmap IconStar {
            get {
                object obj = GetObject("Star");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap IconTrack {
            get {
                object obj = GetObject("Track");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
 
        internal static System.Drawing.Bitmap IconVideo {
            get {
                object obj = GetObject("Video");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap NoAlbumArt {
            get {
                object obj = GetObject("NoAlbumArt");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap IconPlaying {
            get {
                object obj = GetObject("Playing");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap LeftEdgeFiller {
            get {
                object obj = GetObject("LeftEdgeFiller");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap LinnLogo {
            get {
                object obj = GetObject("LinnLogo");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap LinnLogoActive
        {
            get
            {
                object obj = GetObject("LinnLogoActive");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap LinnLogoMouseOver {
            get {
                object obj = GetObject("LinnLogoMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap MiniFiller {
            get {
                object obj = GetObject("MiniFiller");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Minimize {
            get {
                object obj = GetObject("Minimize");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap MinimizeMouse {
            get {
                object obj = GetObject("MinimizeMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap MinimizeTouch {
            get {
                object obj = GetObject("MinimizeTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap MiniModeClose {
            get {
                object obj = GetObject("MiniModeClose");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap MiniModeCloseMouse {
            get {
                object obj = GetObject("MiniModeCloseMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap MiniModeCloseTouch {
            get {
                object obj = GetObject("MiniModeCloseTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap MiniWing1 {
            get {
                object obj = GetObject("MiniWing1");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap MiniWing2 {
            get {
                object obj = GetObject("MiniWing2");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Pause {
            get {
                object obj = GetObject("Pause");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap PauseMouse {
            get {
                object obj = GetObject("PauseMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap PauseTouch {
            get {
                object obj = GetObject("PauseTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Play {
            get {
                object obj = GetObject("Play");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap PlayLater {
            get {
                object obj = GetObject("PlayLater");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap PlayLaterMouse {
            get {
                object obj = GetObject("PlayLaterMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap PlayMouse {
            get {
                object obj = GetObject("PlayMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap PlayNext {
            get {
                object obj = GetObject("PlayNext");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap PlayNextMouse {
            get {
                object obj = GetObject("PlayNextMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap PlayNow {
            get {
                object obj = GetObject("PlayNow");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap PlayNowMouse {
            get {
                object obj = GetObject("PlayNowMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap PlayTouch {
            get {
                object obj = GetObject("PlayTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Repeat {
            get {
                object obj = GetObject("Repeat");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap RepeatActive {
            get {
                object obj = GetObject("RepeatActive");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap RepeatActiveMouse {
            get {
                object obj = GetObject("RepeatActiveMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap RepeatActiveTouch {
            get {
                object obj = GetObject("RepeatActiveTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap RepeatMouse {
            get {
                object obj = GetObject("RepeatMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap RepeatTouch {
            get {
                object obj = GetObject("RepeatTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap RightEdgeFiller {
            get {
                object obj = GetObject("RightEdgeFiller");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Save {
            get {
                object obj = GetObject("Save");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap SaveMouse {
            get {
                object obj = GetObject("SaveMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap SaveTouch {
            get {
                object obj = GetObject("SaveTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Screw {
            get {
                object obj = GetObject("Screw");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Shuffle {
            get {
                object obj = GetObject("Shuffle");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ShuffleActive {
            get {
                object obj = GetObject("ShuffleActive");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ShuffleActiveMouse {
            get {
                object obj = GetObject("ShuffleActiveMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ShuffleActiveTouch {
            get {
                object obj = GetObject("ShuffleActiveTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ShuffleMouse {
            get {
                object obj = GetObject("ShuffleMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ShuffleTouch {
            get {
                object obj = GetObject("ShuffleTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Size {
            get {
                object obj = GetObject("Size");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap SizeRollover {
            get {
                object obj = GetObject("SizeRollover");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap SizeTouch {
            get {
                object obj = GetObject("SizeTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap SkipBack {
            get {
                object obj = GetObject("SkipBack");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap SkipBackMouse {
            get {
                object obj = GetObject("SkipBackMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap SkipBackTouch {
            get {
                object obj = GetObject("SkipBackTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap SkipForward {
            get {
                object obj = GetObject("SkipForward");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap SkipForwardMouse {
            get {
                object obj = GetObject("SkipForwardMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap SkipForwardTouch {
            get {
                object obj = GetObject("SkipForwardTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Standby {
            get {
                object obj = GetObject("Standby");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap StandbyActive {
            get {
                object obj = GetObject("StandbyActive");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap StandbyActiveMouse {
            get {
                object obj = GetObject("StandbyActiveMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap StandbyActiveTouch {
            get {
                object obj = GetObject("StandbyActiveTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap StandbyMouse {
            get {
                object obj = GetObject("StandbyMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap StandbyTouch {
            get {
                object obj = GetObject("StandbyTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap Stop {
            get {
                object obj = GetObject("Stop");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap StopMouse {
            get {
                object obj = GetObject("StopMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap StopTouch {
            get {
                object obj = GetObject("StopTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap SysTrayPrevious
        {
            get
            {
                object obj = GetObject("SysTrayPrevious");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap SysTrayPlay
        {
            get
            {
                object obj = GetObject("SysTrayPlay");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap SysTrayPause
        {
            get
            {
                object obj = GetObject("SysTrayPause");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap SysTrayStop
        {
            get
            {
                object obj = GetObject("SysTrayStop");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap SysTrayNext
        {
            get
            {
                object obj = GetObject("SysTrayNext");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ThreekArray {
            get {
                object obj = GetObject("3kArray");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap UpDirectory {
            get {
                object obj = GetObject("UpDirectory");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap UpDirectoryMouse {
            get {
                object obj = GetObject("UpDirectoryMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap UpDirectoryTouch {
            get {
                object obj = GetObject("UpDirectoryTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap View {
            get {
                object obj = GetObject("View");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ViewMouse {
            get {
                object obj = GetObject("ViewMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap ViewTouch {
            get {
                object obj = GetObject("ViewTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Wheel {
            get {
                object obj = GetObject("Wheel");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WheelMute {
            get {
                object obj = GetObject("WheelMute");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap WheelInnerGlow
        {
            get
            {
                object obj = GetObject("WheelInnerGlow");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WheelOuterGlow {
            get {
                object obj = GetObject("WheelOuterGlow");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap WheelSkip
        {
            get
            {
                object obj = GetObject("WheelSkip");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap WheelSkipBackMouse
        {
            get
            {
                object obj = GetObject("WheelSkipBackMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap WheelSkipBackTouch
        {
            get
            {
                object obj = GetObject("WheelSkipBackTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap WheelSkipForwardTouch
        {
            get
            {
                object obj = GetObject("WheelSkipForwardTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap WheelVolume
        {
            get
            {
                object obj = GetObject("WheelVolume");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap WheelVolumeDownTouch
        {
            get
            {
                object obj = GetObject("WheelVolumeDownTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap WheelVolumeUpTouch
        {
            get
            {
                object obj = GetObject("WheelVolumeUpTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap WheelVolumeSkipInnerGlow
        {
            get
            {
                object obj = GetObject("WheelVolumeSkipInnerGlow");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static System.Drawing.Bitmap WheelVolumeSkipGlow
        {
            get
            {
                object obj = GetObject("WheelVolumeSkipGlow");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsClose {
            get {
                object obj = GetObject("WindowsClose");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsCloseMouse {
            get {
                object obj = GetObject("WindowsCloseMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsCloseTouch {
            get {
                object obj = GetObject("WindowsCloseTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsMaximize {
            get {
                object obj = GetObject("WindowsMaximize");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsMaximizeMouse {
            get {
                object obj = GetObject("WindowsMaximizeMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsMaximizeTouch {
            get {
                object obj = GetObject("WindowsMaximizeTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsMini {
            get {
                object obj = GetObject("WindowsMini");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsMinimize {
            get {
                object obj = GetObject("WindowsMinimize");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsMinimizeMouse {
            get {
                object obj = GetObject("WindowsMinimizeMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsMinimizeTouch {
            get {
                object obj = GetObject("WindowsMinimizeTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsMiniMouse {
            get {
                object obj = GetObject("WindowsMiniMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsMiniTouch {
            get {
                object obj = GetObject("WindowsMiniTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsRestore {
            get {
                object obj = GetObject("WindowsRestore");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsRestoreMouse {
            get {
                object obj = GetObject("WindowsRestoreMouse");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap WindowsRestoreTouch {
            get {
                object obj = GetObject("WindowsRestoreTouch");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Wing1 {
            get {
                object obj = GetObject("Wing1");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Wing2 {
            get {
                object obj = GetObject("Wing2");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Wing3 {
            get {
                object obj = GetObject("Wing3");
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Wing4 {
            get {
                object obj = GetObject("Wing4");
                return ((System.Drawing.Bitmap)(obj));
            }
        }

        internal static byte[] Copy
        {
            get
            {
                object obj = ResourceManager.GetObject("Copy");
                return ((byte[])(obj));
            }
        }

        internal static byte[] Move
        {
            get
            {
                object obj = ResourceManager.GetObject("Move");
                return ((byte[])(obj));
            }
        }

        internal static byte[] None
        {
            get
            {
                object obj = ResourceManager.GetObject("None");
                return ((byte[])(obj));
            }
        }
    }
}
