using System;
using System.Net;

using Linn;
using Linn.Kinsky;

namespace KinskyDesktopGtk
{
    public class Model : IStack, IAppRestartHandler
    {
        public Model(HelperKinsky aHelper)
        {
            aHelper.SetStackExtender(this);
            aHelper.Stack.EventStatusChanged += StatusChanged;

            iHttpServer = new HttpServer(HttpServer.kPortKinskyDesktop);
            iHttpClient = new HttpClient();

            iLocalPlaylists = new LocalPlaylists(aHelper, true);
            iRemotePlaylists = new RemotePlaylists(aHelper);
            iLibrary = new MediaProviderLibrary(aHelper);
            iSupport = new MediaProviderSupport(iHttpServer);

            PluginManager pluginManager = new PluginManager(aHelper, iHttpClient, iSupport);

            iLocator = new ContentDirectoryLocator(pluginManager, this);

            OptionBool optionLocalPlaylists = iLocator.Add(LocalPlaylists.kRootId, iLocalPlaylists);
            OptionBool optionRemotePlaylists = iLocator.Add(RemotePlaylists.kRootId, iRemotePlaylists);
            iLocator.Add(MediaProviderLibrary.kLibraryId, iLibrary);

            aHelper.AddOptionPage(iLocator.OptionPage);

            iPlaySupport = new PlaySupport();
            iSaveSupport = new SaveSupport(aHelper, iRemotePlaylists, optionRemotePlaylists, iLocalPlaylists, optionLocalPlaylists);
            iViewSaveSupport = new ViewSaveSupport(SavePlaylistHandler, iSaveSupport);
        }

        public void Start(IPAddress aIpAddress)
        {
            Console.WriteLine ("Start");
            iLibrary.Start(aIpAddress);
            iRemotePlaylists.Start(aIpAddress);
            iHttpClient.Start();
            iHttpServer.Start(aIpAddress);
            //iLocator.Start();
        }

        public void Stop()
        {
            Console.WriteLine ("Stop");
            //iLocator.Stop();
            iHttpServer.Stop();
            iHttpClient.Stop();
            iRemotePlaylists.Stop();
            iLibrary.Stop();
        }

        public void Rescan()
        {
            iLocator.Refresh();
        }

        public void Restart()
        {
        }

        private void StatusChanged(object sender, EventArgsStackStatus e)
        {
            Console.WriteLine(e);
        }

        private void SavePlaylistHandler(ISaveSupport aSaveSupport)
        {
        }

        private HttpServer iHttpServer;
        private HttpClient iHttpClient;

        private ContentDirectoryLocator iLocator;
        private MediaProviderSupport iSupport;
        private LocalPlaylists iLocalPlaylists;
        private RemotePlaylists iRemotePlaylists;
        private MediaProviderLibrary iLibrary;

        private PlaySupport iPlaySupport;
        private SaveSupport iSaveSupport;
        private ViewSaveSupport iViewSaveSupport;
    }
}

