using System;
using Linn;

namespace KinskyJukebox
{
    public class OptionPageSetup : OptionPage
    {       
        public OptionPageSetup(HelperKinskyJukebox aHelper)
            : base("Setup") {

            iHelper = aHelper;

            iCollectionLocation = new OptionFolderPath("scandirectory", "Scan Directory", "Select the directory to scan for music (root directory of your music collection)", "");
            Add(iCollectionLocation);

            iCollectionHttpLocation = new OptionUri("scanurl", "URL of Scan Directory", "Web accessible address of your scan directory." + Environment.NewLine + "If you are using a NAS based webserver, please enter a valid http location (i.e. http://<ip address of nas>/<music share>)." + Environment.NewLine + "Click the 'Test' button to insure this location opens in a web browser and points to your Scan Directory (root directory of your music collection)." + Environment.NewLine + "Otherwise, you can allow Kinsky Jukebox to run as an Http server by ticking this option box (valid network interface must be selected)." + Environment.NewLine + "Kinsky Jukebox would need to remain running during music playback as it would be serving the music to your DS device.", "http://");
            iCollectionHttpLocation.IncludeTestButton();
            Add(iCollectionHttpLocation);

            iUseHttpServer = new OptionBool("usehttpserver", "Use KinskyJukebox as Http Server", "Use Kinsky Jukebox in place of a dedicated http server for your music (valid network interface must be selected, Kinsky Jukebox would need to remain running during music playback as it would be serving the music to your DS device). URL of scan directory is ignored if this setting is enabled.", false);
            Add(iUseHttpServer);

            iCompilationsFolder = new OptionFolderName("compliationsfoldername", "Compilations Folder Name", "Select the folder where your compilation albums are stored. By default any folder name containing 'compilation' or 'various' will be classed as a compilation folder." + Environment.NewLine + "Should reside within your Scan Directory (root directory of your music collection).", "");
            Add(iCompilationsFolder);

            iRandomize = new OptionBool("randomiselargeplaylists", "Randomize Large Playlists", "Randomize playlists larger than " + MediaCollection.kPlaylistMaxTracks + " tracks. Otherwise the first " + MediaCollection.kPlaylistMaxTracks + " tracks only will be used.", true);
            Add(iRandomize);

            // linked options
            iCollectionHttpLocation.Enabled = !iUseHttpServer.Native;
            EventUseHttpServerChanged += UseHttpServerChanged;
            iUseHttpServer.Enabled = (iHelper.Stack.Status.State == EStackState.eOk);
            iHelper.Stack.EventStatusChanged += EventStackStatusChanged; // linked to iUseHttpServer
        }

        public string CollectionLocation {
            get { return iCollectionLocation.Native; }
            set { iCollectionLocation.Native = value; }
        }

        public string CollectionHttpLocation {
            get { return iCollectionHttpLocation.Native; }
        }

        public bool UseHttpServer {
            get { return iUseHttpServer.Native; }
        }

        public bool Randomize {
            get { return iRandomize.Native; }
        }

        public string CompilationsFolder {
            get { return iCompilationsFolder.Native; }
        }

        public event EventHandler<EventArgs> EventCollectionLocationChanged {
            add { iCollectionLocation.EventValueChanged += value; }
            remove { iCollectionLocation.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventCollectionHttpLocationChanged {
            add { iCollectionHttpLocation.EventValueChanged += value; }
            remove { iCollectionHttpLocation.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventUseHttpServerChanged {
            add { iUseHttpServer.EventValueChanged += value; }
            remove { iUseHttpServer.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventRandomizeChanged {
            add { iRandomize.EventValueChanged += value; }
            remove { iRandomize.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventCompilationsFolderChanged {
            add { iCompilationsFolder.EventValueChanged += value; }
            remove { iCompilationsFolder.EventValueChanged -= value; }
        }

        private void UseHttpServerChanged(object sender, EventArgs e) {
            iCollectionHttpLocation.Enabled = !iUseHttpServer.Native;
        }

        private void EventStackStatusChanged(object sender, EventArgsStackStatus e) {
            iUseHttpServer.Enabled = (e.Status.State == EStackState.eOk);
        }

        private OptionFolderPath iCollectionLocation;
        private OptionUri iCollectionHttpLocation;
        private OptionBool iUseHttpServer;
        private OptionBool iRandomize;
        private OptionFolderName iCompilationsFolder;
        private HelperKinskyJukebox iHelper;
    }
}