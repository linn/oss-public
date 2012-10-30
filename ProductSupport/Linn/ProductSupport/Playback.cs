using System;
using System.Xml;
using System.IO;

using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;
using Linn.Kinsky;

namespace Linn.ProductSupport
{
    public class Playback
    {
        public event EventHandler<EventArgsPlaybackInfo> EventPlaybackInfo;
        public event EventHandler<EventArgsPlaybackError> EventPlaybackError;
        public event EventHandler<EventArgsTransportState> EventTransportStateChanged;
        public event EventHandler<EventArgsVolume> EventVolumeChanged;
        public event EventHandler<EventArgsStandby> EventStandbyChanged;
        public event EventHandler<EventArgsTrackInfo> EventTrackInfoChanged;

        public Playback(Helper aHelper, Device aDevice, EventServerUpnp aEventServer) {
            iLock = new object();
            iHelper = aHelper;
            iDevice = aDevice;
            iEventServer = aEventServer;

            if (iDevice.HasService(ServicePlaylist.ServiceType(1)) > 0) {
                iServicePlaylist = new ServicePlaylist(iDevice, iEventServer);
                iServicePlaylist.EventInitial += EventHandlerPlaylistInitial;
                iActionInsert = iServicePlaylist.CreateAsyncActionInsert();
                iActionPlay = iServicePlaylist.CreateAsyncActionPlay();
                iActionSetRepeat = iServicePlaylist.CreateAsyncActionSetRepeat();
                iActionDeleteAll = iServicePlaylist.CreateAsyncActionDeleteAll();
            }

            if (iDevice.HasService(ServiceProduct.ServiceType(1)) > 0) {
                iServiceProduct = new ServiceProduct(iDevice, iEventServer);
                iServiceProduct.EventInitial += EventHandlerProductInitial;
                iActionStandby = iServiceProduct.CreateAsyncActionStandby();
                iActionSetStandby = iServiceProduct.CreateAsyncActionSetStandby();
                iActionSetSourceIndex = iServiceProduct.CreateAsyncActionSetSourceIndex();
                iActionSourceIndex = iServiceProduct.CreateAsyncActionSourceIndex();
                iActionSetSourceIndexByName = iServiceProduct.CreateAsyncActionSetSourceIndexByName();
            }

            if (iDevice.HasService(ServiceVolume.ServiceType(1)) > 0) {
                iServiceVolume = new ServiceVolume(iDevice, iEventServer);
                iServiceVolume.EventInitial += EventHandlerVolumeInitial;
                iActionVolumeInc = iServiceVolume.CreateAsyncActionVolumeInc();
                iActionVolumeDec = iServiceVolume.CreateAsyncActionVolumeDec();
            }
        }
        public void Close()
        {
            lock (iLock)
            {
                if (iServiceVolume != null)
                {
                    iServiceVolume.EventInitial -= EventHandlerVolumeInitial;
                    iServiceVolume.EventStateVolume -= EventHandlerVolumeChange;
                    iServiceVolume.Close();
                    iServiceVolume = null;
                }
                if (iServiceProduct != null)
                {
                    iServiceProduct.EventInitial -= EventHandlerProductInitial;
                    iServiceProduct.EventStateStandby -= EventHandlerStandbyChange;
                    iServiceProduct.EventStateStandby -= EventHandlerStandbyChangeForStart;
                    iServiceProduct.EventStateSourceIndex -= EventHandlerSourceIndexChangeForStart;
                    iServiceProduct.Close();
                    iServiceProduct = null;
                }
                if (iServicePlaylist != null)
                {
                    iServicePlaylist.EventInitial -= EventHandlerPlaylistInitial;
                    iServicePlaylist.EventStateTransportState -= EventHandlerTransportStateChange;
                    iServicePlaylist.EventStateId -= EventHandlerIdChangeForStart;
                    iServicePlaylist.EventStateId -= EventHandlerIdChangeForStop;
                    iServicePlaylist.EventStateTransportState -= EventHandlerTransportStateChangeForStop;
                    iServicePlaylist.Close();
                    iServicePlaylist = null;
                }
            }
        }

        public void Kill() {
            lock (iLock)
            {
                if (iServiceVolume != null)
                {
                    iServiceVolume.EventInitial -= EventHandlerVolumeInitial;
                    iServiceVolume.EventStateVolume -= EventHandlerVolumeChange;
                    iServiceVolume.Kill();
                    iServiceVolume = null;
                }
                if (iServiceProduct != null)
                {
                    iServiceProduct.EventInitial -= EventHandlerProductInitial;
                    iServiceProduct.EventStateStandby -= EventHandlerStandbyChange;
                    iServiceProduct.EventStateStandby -= EventHandlerStandbyChangeForStart;
                    iServiceProduct.EventStateSourceIndex -= EventHandlerSourceIndexChangeForStart;
                    iServiceProduct.Kill();
                    iServiceProduct = null;
                }
                if (iServicePlaylist != null)
                {
                    iServicePlaylist.EventInitial -= EventHandlerPlaylistInitial;
                    iServicePlaylist.EventStateTransportState -= EventHandlerTransportStateChange;
                    iServicePlaylist.EventStateId -= EventHandlerIdChangeForStart;
                    iServicePlaylist.EventStateId -= EventHandlerIdChangeForStop;
                    iServicePlaylist.EventStateTransportState -= EventHandlerTransportStateChangeForStop;
                    iServicePlaylist.Kill();
                    iServicePlaylist = null;
                }
            }
        }

        public void Start(string aTrackFileName) {
            iTrackFilename = aTrackFileName;
            try {
                if (iHelper.Interface.Interface.Info != null) {
                    // check file exists
                    if (File.Exists(aTrackFileName)) {
                        string ext = Path.GetExtension(aTrackFileName).ToLowerInvariant();
                        // file must be of a valid audio type: *.flac, *.mp*, *.wav*, *.aif*, *.m4a, *.wma, *.ogg
                        if (ext == ".flac" || ext.StartsWith(".mp") || ext.StartsWith(".wav") || ext.StartsWith(".aif") || ext == ".m4a" || ext == ".wma" || ext == ".ogg") {
                            // start http server
                            iHttpServer = new HttpServer(HttpServer.kPortInstallWizard);
                            iHttpServer.Start(iHelper.Interface.Interface.Info.IPAddress);

                            SetPlaybackInfo("Preparing device for playback... check device is not in standby");

                            // determine standby state
                            iActionStandby.EventResponse += InvokeActionStandby;
                            iActionStandby.EventError += InvokeActionError;
                            iActionStandby.StandbyBegin();
                        }
                        else {
                            PlaybackError("'" + ext + "' is not recognised as a valid audio file type");
                        }
                    }
                    else {
                        PlaybackError("Could not find the file '" + aTrackFileName + "'");
                    }
                }
                else {
                    PlaybackError("(Http Server Start) Network Interface is invalid");
                }
            }
            catch (Exception e) {
                PlaybackError(e.Message);
            }
        }

        public void Start() {
            try {
                // try the default installer location
                string fullpath = Path.GetFullPath(Path.Combine(iHelper.ExePath.FullName, "AudioTrack.flac"));
                if (File.Exists(fullpath)) {
                    Start(fullpath);
                }
                else {
                    // try the default build location
                    fullpath = Path.GetFullPath(Path.Combine(iHelper.ExePath.FullName, "../../share/Linn/ProductSupport/AudioTrack.flac"));
                    if (File.Exists(fullpath)) {
                        Start(fullpath);
                    }
                    else {
                        PlaybackError("Could not find the file 'AudioTrack.flac'");
                    }
                }
            }
            catch (Exception e) {
                PlaybackError(e.Message);
            }
        }

        public void Stop() {
            try {
                SetPlaybackInfo("Playback test stopped");

                // set repeat off
                iActionSetRepeat.EventResponse += InvokeActionSetRepeat;
                iActionSetRepeat.EventError += InvokeActionError;
                iActionSetRepeat.SetRepeatBegin(false);

                // clear playlist
                iActionDeleteAll.EventResponse += InvokeActionDeleteAllStop;
                iActionDeleteAll.EventError += InvokeActionError;
                iActionDeleteAll.DeleteAllBegin();
            }
            catch (Exception e) {
                PlaybackError(e.Message);
            }
        }

        public void VolumeInc() {
            iActionVolumeInc.EventResponse += InvokeActionVolumeInc;
            iActionVolumeInc.EventError += InvokeActionError;
            iActionVolumeInc.VolumeIncBegin();
        }

        public void VolumeDec() {
            iActionVolumeDec.EventResponse += InvokeActionVolumeDec;
            iActionVolumeDec.EventError += InvokeActionError;
            iActionVolumeDec.VolumeDecBegin();
        }

        public void SetSourceIndexByName(string aName)
        {
            iActionSetSourceIndexByName.EventResponse += InvokeActionSetSourceIndexByName;
            iActionSetSourceIndexByName.EventError += InvokeActionError;
            iActionSetSourceIndexByName.SetSourceIndexByNameBegin(aName);
        }

        public void SetStandby(bool aStandby) {
            iActionSetStandby.SetStandbyBegin(aStandby);
        }

        public bool Standby {
            get {
                return iServiceProduct.Standby;
            }
        }

        public uint Volume {
            get {
                return iServiceVolume.Volume;
            }
        }

        public string TransportState {
            get {
                return iServicePlaylist.TransportState;
            }
        }

        private void InvokeActionStandby(object obj, ServiceProduct.AsyncActionStandby.EventArgsResponse e) {
            bool inStandby = e.Value;
            iActionStandby.EventResponse -= InvokeActionStandby;
            iActionStandby.EventError -= InvokeActionError;

            // bring out of standy if device currently in standby
            if (inStandby) {
                SetPlaybackInfo("Preparing device for playback... bring device out of standby");

                iActionSetStandby.EventResponse += InvokeActionSetStandby;
                iActionSetStandby.EventError += InvokeActionError;
                iActionSetStandby.SetStandbyBegin(false);
                // wait for device to come out of standby before determining exisiting source
                iServiceProduct.EventStateStandby += EventHandlerStandbyChangeForStart;
            }
            else {
                DetermineCurrentSource();
            }
        }

        private void InvokeActionSourceIndex(object obj, ServiceProduct.AsyncActionSourceIndex.EventArgsResponse e) {
            uint sourceIndex = e.Value;
            iActionSourceIndex.EventResponse -= InvokeActionSourceIndex;
            iActionSourceIndex.EventError -= InvokeActionError;

            // select playlist source if not already selected
            if (sourceIndex != 0) {
                SetPlaybackInfo("Preparing device for playback... select playlist source");

                iActionSetSourceIndex.EventResponse += InvokeActionSetSourceIndex;
                iActionSetSourceIndex.EventError += InvokeActionError;
                iActionSetSourceIndex.SetSourceIndexBegin(0);
                // wait for playlist source to be selected before playing track
                iServiceProduct.EventStateSourceIndex += EventHandlerSourceIndexChangeForStart;
            }
            else {
                PlayTrack();
            }
        }

        private void InvokeActionSetStandby(object obj, ServiceProduct.AsyncActionSetStandby.EventArgsResponse e) {
            iActionSetStandby.EventResponse -= InvokeActionSetStandby;
            iActionSetStandby.EventError -= InvokeActionError;
        }

        private void InvokeActionSetSourceIndex(object obj, ServiceProduct.AsyncActionSetSourceIndex.EventArgsResponse e) {
            iActionSetSourceIndex.EventResponse -= InvokeActionSetSourceIndex;
            iActionSetSourceIndex.EventError -= InvokeActionError;
        }

        private void InvokeActionSetSourceIndexByName(object obj, ServiceProduct.AsyncActionSetSourceIndexByName.EventArgsResponse e)
        {
            iActionSetSourceIndexByName.EventResponse -= InvokeActionSetSourceIndexByName;
            iActionSetSourceIndexByName.EventError -= InvokeActionError;
        }

        private void InvokeActionInsert(object obj, ServicePlaylist.AsyncActionInsert.EventArgsResponse e)
        {
            iActionInsert.EventResponse -= InvokeActionInsert;
            iActionInsert.EventError -= InvokeActionError;
        }

        private void InvokeActionPlay(object obj, ServicePlaylist.AsyncActionPlay.EventArgsResponse e) {
            iActionPlay.EventResponse -= InvokeActionPlay;
            iActionPlay.EventError -= InvokeActionError;
        }

        private void InvokeActionSetRepeat(object obj, ServicePlaylist.AsyncActionSetRepeat.EventArgsResponse e) {
            iActionSetRepeat.EventResponse -= InvokeActionSetRepeat;
            iActionSetRepeat.EventError -= InvokeActionError;
        }

        private void InvokeActionDeleteAllStart(object obj, ServicePlaylist.AsyncActionDeleteAll.EventArgsResponse e) {
            iActionDeleteAll.EventResponse -= InvokeActionDeleteAllStart;
            iActionDeleteAll.EventError -= InvokeActionError;

            SetPlaybackInfo("Preparing device for playback... add track to playlist");

            // insert track into playlist
            iActionInsert.EventResponse += InvokeActionInsert;
            iActionInsert.EventError += InvokeActionError;
            iActionInsert.InsertBegin(0, iHttpServer.Uri(iTrackFilename), GetDidlLiteMetadata(iTrackFilename));

            // wait for track to be loaded into the playlist before playing
            iServicePlaylist.EventStateId += EventHandlerIdChangeForStart;
        }

        private void InvokeActionDeleteAllStop(object obj, ServicePlaylist.AsyncActionDeleteAll.EventArgsResponse e) {
            iActionDeleteAll.EventResponse -= InvokeActionDeleteAllStop;
            iActionDeleteAll.EventError -= InvokeActionError;

            if (iServicePlaylist.Id == 0 && iServicePlaylist.TransportState == "Stopped") {
                StopHttpServer();
            }
            else {
                // wait for playlist to be cleared and stopped before stopping http server (otherwise device will crash)
                iServicePlaylist.EventStateId += EventHandlerIdChangeForStop;
                iServicePlaylist.EventStateTransportState += EventHandlerTransportStateChangeForStop;
            }
        }

        private void InvokeActionVolumeInc(object obj, ServiceVolume.AsyncActionVolumeInc.EventArgsResponse e) {
            iActionVolumeInc.EventResponse -= InvokeActionVolumeInc;
            iActionVolumeInc.EventError -= InvokeActionError;
        }

        private void InvokeActionVolumeDec(object obj, ServiceVolume.AsyncActionVolumeDec.EventArgsResponse e) {
            iActionVolumeDec.EventResponse -= InvokeActionVolumeDec;
            iActionVolumeDec.EventError -= InvokeActionError;
        }

        private void InvokeActionError(object obj, EventArgsError e) {
            iActionInsert.EventResponse -= InvokeActionInsert;
            iActionInsert.EventError -= InvokeActionError;
            iActionPlay.EventResponse -= InvokeActionPlay;
            iActionPlay.EventError -= InvokeActionError;
            iActionSetRepeat.EventResponse -= InvokeActionSetRepeat;
            iActionSetRepeat.EventError -= InvokeActionError;
            iActionDeleteAll.EventResponse -= InvokeActionDeleteAllStart;
            iActionDeleteAll.EventResponse -= InvokeActionDeleteAllStop;
            iActionDeleteAll.EventError -= InvokeActionError;
            iActionStandby.EventResponse -= InvokeActionStandby;
            iActionStandby.EventError -= InvokeActionError;
            iActionSetStandby.EventResponse -= InvokeActionSetStandby;
            iActionSetStandby.EventError -= InvokeActionError;
            iActionSourceIndex.EventResponse -= InvokeActionSourceIndex;
            iActionSourceIndex.EventError -= InvokeActionError;
            iActionSetSourceIndex.EventResponse -= InvokeActionSetSourceIndex;
            iActionSetSourceIndex.EventError -= InvokeActionError;
            iActionSetSourceIndexByName.EventResponse -= InvokeActionSetSourceIndexByName;
            iActionSetSourceIndexByName.EventError -= InvokeActionError;
            iActionVolumeInc.EventResponse -= InvokeActionVolumeInc;
            iActionVolumeInc.EventError -= InvokeActionError;
            iActionVolumeDec.EventResponse -= InvokeActionVolumeDec;
            iActionVolumeDec.EventError -= InvokeActionError;

            PlaybackError("(Event Action Error) " + e.Code + ", " + e.Description);
        }

        private void EventHandlerPlaylistInitial(object obj, EventArgs e) {
            lock (iLock)
            {
                if (iServicePlaylist == obj)
                {
                    iServicePlaylist.EventStateTransportState += EventHandlerTransportStateChange;

                    if (EventTransportStateChanged != null)
                    {
                        EventTransportStateChanged(this, new EventArgsTransportState(iServicePlaylist.TransportState));
                    }
                }
            }
        }

        private void EventHandlerProductInitial(object obj, EventArgs e) {
            lock (iLock)
            {
                if (iServiceProduct == obj)
                {
                    iServiceProduct.EventStateStandby += EventHandlerStandbyChange;

                    if (EventStandbyChanged != null)
                    {
                        EventStandbyChanged(this, new EventArgsStandby(iServiceProduct.Standby));
                    }
                }
            }
        }

        private void EventHandlerVolumeInitial(object obj, EventArgs e) {
            lock (iLock)
            {
                if (iServiceVolume == obj)
                {
                    iServiceVolume.EventStateVolume += EventHandlerVolumeChange;

                    if (EventVolumeChanged != null)
                    {
                        EventVolumeChanged(this, new EventArgsVolume(iServiceVolume.Volume.ToString()));
                    }
                }
            }
        }

        private void EventHandlerIdChangeForStop(object obj, EventArgs e) {
            lock (iLock)
            {
                if (obj == iServicePlaylist)
                {
                    if (iServicePlaylist.Id == 0)
                    {
                        iServicePlaylist.EventStateId -= EventHandlerIdChangeForStop;
                        StopHttpServer();
                    }
                }
            }
        }

        private void EventHandlerIdChangeForStart(object obj, EventArgs e) {
            lock (iLock)
            {
                if (obj == iServicePlaylist)
                {
                    if (iServicePlaylist.Id > 0)
                    {
                        iServicePlaylist.EventStateId -= EventHandlerIdChangeForStart;

                        SetPlaybackInfo("Preparing device for playback... play track");

                        // play track
                        iActionPlay.EventResponse += InvokeActionPlay;
                        iActionPlay.EventError += InvokeActionError;
                        iActionPlay.PlayBegin();
                    }
                }
            }
        }

        private void EventHandlerTransportStateChange(object obj, EventArgs e) {
            lock (iLock)
            {
                if (obj == iServicePlaylist)
                {
                    if (EventTransportStateChanged != null)
                    {
                        EventTransportStateChanged(this, new EventArgsTransportState(iServicePlaylist.TransportState));
                    }

                    if (iServicePlaylist.TransportState == "Playing")
                    {
                        SetPlaybackInfo("Playback test succeeded... device is currently playing");
                    }
                }
            }
        }

        private void EventHandlerTransportStateChangeForStop(object obj, EventArgs e) {
            lock (iLock)
            {
                if (obj == iServicePlaylist)
                {
                    if (iServicePlaylist.TransportState == "Stopped")
                    {
                        iServicePlaylist.EventStateTransportState -= EventHandlerTransportStateChangeForStop;
                        StopHttpServer();
                    }
                }
            }
        }

        private void EventHandlerSourceIndexChangeForStart(object obj, EventArgs e)
        {
            lock (iLock)
            {
                if (obj == iServiceProduct)
                {
                    if (iServiceProduct.SourceIndex == 0)
                    {
                        iServiceProduct.EventStateSourceIndex -= EventHandlerSourceIndexChangeForStart;
                        PlayTrack();
                    }
                }
            }
        }

        private void EventHandlerStandbyChange(object obj, EventArgs e) {
            lock (iLock)
            {
                if (obj == iServiceProduct)
                {
                    if (EventStandbyChanged != null)
                    {
                        EventStandbyChanged(this, new EventArgsStandby(iServiceProduct.Standby));
                    }
                }
            }
        }

        private void EventHandlerStandbyChangeForStart(object obj, EventArgs e) {
            lock (iLock)
            {
                if (obj == iServiceProduct)
                {
                    if (!iServiceProduct.Standby)
                    {
                        iServiceProduct.EventStateStandby -= EventHandlerStandbyChangeForStart;

                        DetermineCurrentSource();
                    }
                }
            }
        }

        private void EventHandlerVolumeChange(object obj, EventArgs e) {
            lock (iLock)
            {
                if (obj == iServiceVolume)
                {
                    if (EventVolumeChanged != null)
                    {
                        EventVolumeChanged(this, new EventArgsVolume(iServiceVolume.Volume.ToString()));
                    }
                }
            }
        }

        private void PlayTrack() {
            SetPlaybackInfo("Preparing device for playback... set repeat on");

            // set repeat on
            iActionSetRepeat.EventResponse += InvokeActionSetRepeat;
            iActionSetRepeat.EventError += InvokeActionError;
            iActionSetRepeat.SetRepeatBegin(true);

            SetPlaybackInfo("Preparing device for playback... clear playlist");

            // clear playlist
            iActionDeleteAll.EventResponse += InvokeActionDeleteAllStart;
            iActionDeleteAll.EventError += InvokeActionError;
            iActionDeleteAll.DeleteAllBegin();
        }

        private void DetermineCurrentSource() {
            SetPlaybackInfo("Preparing device for playback... check current source is playlist");

            // determine existing source
            iActionSourceIndex.EventResponse += InvokeActionSourceIndex;
            iActionSourceIndex.EventError += InvokeActionError;
            iActionSourceIndex.SourceIndexBegin();
        }

        private void StopHttpServer() {
            try {
                if (iServicePlaylist.Id == 0 && iServicePlaylist.TransportState == "Stopped") {
                    ExecuteStop();
                }
            }
            catch (Exception e) {
                PlaybackError(e.Message);
            }
        }

        private void PlaybackError(string aMessage) {
            string message = "Playback Error: " + aMessage;

            try {
                ExecuteStop();
            }
            catch {
            }

            SetPlaybackInfo(message);

            if (EventPlaybackError != null) {
                EventPlaybackError(this, new EventArgsPlaybackError(message));
            }
        }

        private void ExecuteStop() {
            // stop http server
            if (iHttpServer != null) {
                iHttpServer.Stop();
            }
        }

        private string GetDidlLiteMetadata(string aTrackFilename) {
            // populate metadata from tags if it is there
            string metadata = "";
            try {
                TagLib.File tagFile = TagLib.File.Create(aTrackFilename);
                string title = tagFile.Tag.Title;
                string album = tagFile.Tag.Album;
                string artist = tagFile.Tag.FirstPerformer;
                
                Upnp.musicTrack musicTrack = new Upnp.musicTrack();
                musicTrack.Title = (title == null ? "Title" : title);
                musicTrack.Album.Add((album == null ? "Album" : album));
                Upnp.artist performer = new Upnp.artist();
                performer.Artist = (artist == null ? "Artist" : artist);
                performer.Role = "Performer";
                musicTrack.Artist.Add(performer);

                string artPath = null;
                try {
                    artPath = FindArtwork(iTrackFilename);
                    if (artPath != null) {
                        string uri = iHttpServer.Uri(artPath);
                        musicTrack.AlbumArtUri.Add(uri);
                        musicTrack.ArtworkUri.Add(uri);
                    }
                }
                catch (Exception) { // if AlbumArtPath is invalid, just don't add artwork
                }
                Upnp.DidlLite didl = new Upnp.DidlLite();
                didl.Add(musicTrack);
                metadata = didl.Xml;

                if (EventTrackInfoChanged != null) {
                    EventTrackInfoChanged(this, new EventArgsTrackInfo(title, artist, album, artPath));
                }
            }
            catch (Exception) { // if Metadata is invalid, just don't add metadata
                metadata = "";
            }
            return metadata;
        }

        private string FindArtwork(string aTrackFilename) {
            try {
                foreach (string ext in kImageFileExtensions) {
                    string[] images = Directory.GetFiles(Path.GetDirectoryName(aTrackFilename), ext, SearchOption.TopDirectoryOnly); // non-recursive
                    if (images.Length > 0) {
                        return Path.GetFullPath(images[0]);
                    }
                }
            }
            catch (Exception) {
            }
            return null;
        }

        private void SetPlaybackInfo(string aInfo) {
            UserLog.WriteLine(aInfo);
            if (EventPlaybackInfo != null) {
                EventPlaybackInfo(this, new EventArgsPlaybackInfo(aInfo));
            }
        }

        private readonly string[] kImageFileExtensions = { "*older.jpg", "*.png", "*.jpg" };

        private ServicePlaylist iServicePlaylist = null;
        private ServicePlaylist.AsyncActionInsert iActionInsert;
        private ServicePlaylist.AsyncActionPlay iActionPlay;
        private ServicePlaylist.AsyncActionSetRepeat iActionSetRepeat;
        private ServicePlaylist.AsyncActionDeleteAll iActionDeleteAll;

        private ServiceProduct iServiceProduct = null;
        private ServiceProduct.AsyncActionStandby iActionStandby;
        private ServiceProduct.AsyncActionSetStandby iActionSetStandby;
        private ServiceProduct.AsyncActionSetSourceIndex iActionSetSourceIndex;
        private ServiceProduct.AsyncActionSourceIndex iActionSourceIndex;
        private ServiceProduct.AsyncActionSetSourceIndexByName iActionSetSourceIndexByName;

        private ServiceVolume iServiceVolume = null;
        private ServiceVolume.AsyncActionVolumeInc iActionVolumeInc;
        private ServiceVolume.AsyncActionVolumeDec iActionVolumeDec;

        private Helper iHelper = null;
        private Device iDevice = null;
        private EventServerUpnp iEventServer = null;
        private HttpServer iHttpServer = null;
        private string iTrackFilename = null;
        private object iLock;
    }

    public class EventArgsPlaybackInfo : EventArgs
    {
        public EventArgsPlaybackInfo(string aInfo) {
            iInfo = aInfo;
        }

        public string PlaybackInfo {
            get { return iInfo; }
        }

        private string iInfo;
    }

    public class EventArgsPlaybackError : EventArgs
    {
        public EventArgsPlaybackError(string aErrorMessage) {
            iErrorMessage = aErrorMessage;
        }

        public string ErrorMessage {
            get { return iErrorMessage; }
        }

        private string iErrorMessage;
    }

    public class EventArgsVolume : EventArgs
    {
        public EventArgsVolume(string aVolume) {
            iVolume = aVolume;
        }

        public string Volume {
            get { return iVolume; }
        }

        private string iVolume;
    }

    public class EventArgsStandby : EventArgs
    {
        public EventArgsStandby(bool aStandby) {
            iStandby = aStandby;
        }

        public bool Standby {
            get { return iStandby; }
        }

        private bool iStandby;
    }

    public class EventArgsTransportState : EventArgs
    {
        public EventArgsTransportState(string aTransportState) {
            iTransportState = aTransportState;
        }

        public string TransportState {
            get { return iTransportState; }
        }

        private string iTransportState;
    }

    public class EventArgsTrackInfo : EventArgs
    {
        public EventArgsTrackInfo(string aTitle, string aArtist, string aAlbum, string aAlbumArtUri) {
            iTitle = aTitle;
            iArtist = aArtist;
            iAlbum = aAlbum;
            iAlbumArtUri = aAlbumArtUri;
        }

        public string Title {
            get { return iTitle; }
        }

        public string Artist {
            get { return iArtist; }
        }

        public string Album {
            get { return iAlbum; }
        }

        public string AlbumArtUri {
            get { return iAlbumArtUri; }
        }

        private string iTitle;
        private string iArtist;
        private string iAlbum;
        private string iAlbumArtUri;
    }
}