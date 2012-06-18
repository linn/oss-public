using System;
using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Kinsky;
using KinskyWeb.Comms;
using System.ServiceModel;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Net;

namespace KinskyWeb.Kinsky
{
    public class KinskyStack : StackStatusHandler, IStack
    {

        private HelperKinsky iHelper;
        private ContentDirectoryLocator iLocator;
        private Linn.Kinsky.HttpServer iHttpServer;
        private HttpClient iHttpClient;
        private MediaProviderLibrary iLibrary;

        private const string kKinskyWebNamedMutex = "KinskyWebMutex";
        private Mutex iNamedMutex;
        private const int kPort = 8000;

        public class ApplicationAlreadyRunningException : Exception { }

        //public AppKinskyWebWinForm AppKinskyWeb { get; private set; }
        //public EventServerUpnp EventServer { get; private set; }
        //public SsdpListenerMulticast ListenerNotify { get; private set; }
        //public Linn.Kinsky.HttpServer WebServer { get; private set; }
        //public Linn.Topology.House House { get; private set; }
        //public IContentDirectory ContentDirectory { get; private set; }
        //public PluginManager PluginManager { get; private set; }
        private SelfHostingServiceHost[] iServices;
        private ResourceStreamServer iResourceStreamServer;
        //private object iLockObject = new object();
        //private Mutex iMutex;

        //public event EventHandler<EventArgs> ConnectionStateChanged;

        //public bool Connected { get; private set; }

        private static KinskyStack iDefault;
        public static KinskyStack GetDefault()
        {
            if (iDefault == null)
            {
                iDefault = new KinskyStack();
            }
            return iDefault;
        }

        private KinskyStack()
        {
            iHelper = new HelperKinsky(Environment.GetCommandLineArgs());
            iHelper.SetStackExtender(this);
            iHelper.Stack.SetStatusHandler(this);
            iLibrary = new MediaProviderLibrary(iHelper);
            iHttpServer = new Linn.Kinsky.HttpServer(Linn.Kinsky.HttpServer.kPortKinskyWeb);
            iHttpClient = new HttpClient();
            PluginManager pluginManager = new PluginManager(iHelper, iHttpClient, new MediaProviderSupport(iHttpServer));
            iLocator = new ContentDirectoryLocator(pluginManager, new AppRestartHandler());
            LocalPlaylistSupport saveSupport = new LocalPlaylistSupport(iHelper, (d) => { });
            iLocator.Add(saveSupport.ContentDirectoryId, saveSupport.ContentDirectoryRoot);
            iLocator.Add(MediaProviderLibrary.kLibraryId, iLibrary);
        }

        public HelperKinsky Helper
        {
            get
            {
                return iHelper;
            }
        }

        public ContentDirectoryLocator Locator
        {
            get
            {
                return iLocator;
            }
        }

        public void Open()
        {
            bool createdNew = false;
            iNamedMutex = new Mutex(false, kKinskyWebNamedMutex, out createdNew); if (!createdNew)
            {
                iNamedMutex.Close();
                iNamedMutex = null;
                throw new ApplicationAlreadyRunningException();
            }
            iHelper.Stack.Start();
        }

        public void Close()
        {
            if (iNamedMutex != null)
            {
                iNamedMutex.Close();
                iNamedMutex = null;
            }
            iHelper.Stack.Stop();
        }


        //public void Start()
        //{
        //    lock (iLockObject)
        //    {
        //        if (!Connected)
        //        {
        //            bool createdNew = false;
        //            iMutex = new Mutex(false, AppKinskyWeb.Product, out createdNew);
        //            if (!createdNew)
        //            {
        //                iMutex.Close();
        //                iMutex = null;
        //                throw new ApplicationAlreadyRunningException();
        //            }
        //            bool started = false;
        //            try
        //            {
        //                AppKinskyWeb.Start();
        //                EventServer.Start(AppKinskyWeb.Interface);
        //                ListenerNotify.Start(AppKinskyWeb.Interface);
        //                House.Start(AppKinskyWeb.Interface);
        //                if (PluginManager == null)
        //                {
        //                    MediaProviderSupport support = new MediaProviderSupport(
        //                    AppKinskyWeb,
        //                    EventServer,
        //                    ListenerNotify,
        //                    WebServer);
        //                    PluginManager = new PluginManager(AppKinskyWeb, support);
        //                }
        //                if (ContentDirectory == null)
        //                {
        //                    ContentDirectory = new ContentDirectoryLocator(PluginManager);
        //                }
        //                ContentDirectory.Start();

        //                WebServer.Start(AppKinskyWeb.Interface, Linn.Kinsky.HttpServer.kPortKinskyWeb);
        //                string baseAddress = String.Format("http://{0}:{1}/Services/",
        //                                                   System.Net.Dns.GetHostName(),
        //                                                  AppKinskyWeb.Port);
        //                iServices = SelfHostingServiceHost.GetSelfHostingServices(baseAddress);

        //                if (AppKinskyWeb.ResourceLocation != string.Empty)
        //                {
        //                    ResourceStreamService.ResourceLocation = AppKinskyWeb.ResourceLocation;
        //                }

        //                // TODO: remove this once mono wcf stack is stable
        //                iResourceStreamServer = new ResourceStreamServer(System.Net.IPAddress.Any, AppKinskyWeb.Port, AppKinskyWeb.EnableCaching, iServices);
        //                /*
        //                foreach (SelfHostingServiceHost host in iServices)
        //                {
        //                    //todo: wcf BasicHttpBinding needs admin privileges to run or configured with "netsh http add urlacl"
        //                    //Need to write an http handler based on plain tcp sockets or on top of HttpServer.HttpListener
        //                    host.Open();
        //                }
        //                */
        //                // TODO: remove this once mono wcf stack is stable
        //                iResourceStreamServer.Start();
        //                Trace.WriteLine(Trace.kKinskyWeb, "Started kinsky stack...");
        //                Connected = true;
        //                OnConnectionStateChanged();
        //                started = true;
        //            }
        //            catch(Exception ex)
        //            {
        //                UserLog.WriteLine("Exception caught starting services.");
        //                UserLog.WriteLine(ex.Message + "\n" + ex.StackTrace);
        //                throw ex;
        //            }
        //            finally
        //            {
        //                if (!started)
        //                {
        //                    try{
        //                    CloseServices();
        //                    }catch{}
        //                }
        //            }
        //        }
        //    }
        //}

        //private void CloseServices()
        //{
        //    if (iMutex != null)
        //    {
        //        iMutex.Close();
        //        iMutex = null;
        //    }
        //    if (iResourceStreamServer != null)
        //    {
        //        try
        //        {
        //            iResourceStreamServer.Stop();
        //        }
        //        catch (Exception ex)
        //        {
        //            UserLog.WriteLine("Error caught stopping ResourceStreamServer.");
        //            UserLog.WriteLine(ex.Message + "\n" + ex.StackTrace);
        //        }
        //    }
        //    try
        //    {
        //        WidgetRegistry.GetDefault().Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        UserLog.WriteLine("Error caught stopping WidgetRegistry.");
        //        UserLog.WriteLine(ex.Message + "\n" + ex.StackTrace);
        //    }
        //    if (ContentDirectory != null)
        //    {
        //        try
        //        {
        //            ContentDirectory.Stop();
        //        }
        //        catch (Exception ex)
        //        {
        //            UserLog.WriteLine("Error caught stopping ContentDirectory.");
        //            UserLog.WriteLine(ex.Message + "\n" + ex.StackTrace);
        //        }
        //    }
        //    try
        //    {
        //        House.Stop();
        //    }
        //    catch (Exception ex)
        //    {
        //        UserLog.WriteLine("Error caught stopping House.");
        //        UserLog.WriteLine(ex.Message + "\n" + ex.StackTrace);
        //    }
        //    try
        //    {
        //        AppKinskyWeb.Stop();
        //    }
        //    catch (Exception ex)
        //    {
        //        UserLog.WriteLine("Error caught stopping AppKinskyWeb.");
        //        UserLog.WriteLine(ex.Message + "\n" + ex.StackTrace);
        //    }
        //    try
        //    {
        //        EventServer.Stop();
        //    }
        //    catch (Exception ex)
        //    {
        //        UserLog.WriteLine("Error caught stopping EventServer.");
        //        UserLog.WriteLine(ex.Message + "\n" + ex.StackTrace);
        //    }
        //    try
        //    {
        //        ListenerNotify.Stop();
        //    }
        //    catch (Exception ex)
        //    {
        //        UserLog.WriteLine("Error caught stopping ListenerNotify.");
        //        UserLog.WriteLine(ex.Message + "\n" + ex.StackTrace);
        //    }
        //    try
        //    {
        //        if (WebServer.Started)
        //        {
        //            WebServer.Stop();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        UserLog.WriteLine("Error caught stopping WebServer.");
        //        UserLog.WriteLine(ex.Message + "\n" + ex.StackTrace);
        //    }
        //    Connected = false;
        //    OnConnectionStateChanged();
        //}


        #region IStack Members

        public void Start(IPAddress aIpAddress)
        {



            string baseAddress = String.Format("http://{0}:{1}/Services/",
                                               System.Net.Dns.GetHostName(),
                                               kPort);
            iServices = SelfHostingServiceHost.GetSelfHostingServices(baseAddress);

            //if (AppKinskyWeb.ResourceLocation != string.Empty)
            //{
            //    ResourceStreamService.ResourceLocation = AppKinskyWeb.ResourceLocation;
            //}

            // TODO: remove this once mono wcf stack is stable
            iResourceStreamServer = new ResourceStreamServer(System.Net.IPAddress.Any, kPort, true, iServices);
            /*
            foreach (SelfHostingServiceHost host in iServices)
            {
                //todo: wcf BasicHttpBinding needs admin privileges to run or configured with "netsh http add urlacl"
                //Need to write an http handler based on plain tcp sockets or on top of HttpServer.HttpListener
                host.Open();
            }
            */
            // TODO: remove this once mono wcf stack is stable
            iResourceStreamServer.Start();
            iLibrary.Start(aIpAddress);
            iHttpClient.Start();
            iHttpServer.Start(aIpAddress);
            iLocator.Start();
        }

        public void Stop()
        {
            iResourceStreamServer.Stop();
            WidgetRegistry.GetDefault().Close();
            iLocator.Stop();
            iHttpServer.Stop();
            iHttpClient.Stop();
            iLibrary.Stop();
        }

        #endregion

        public override void StackStatusChanged(object sender, EventArgsStackStatus e)
        {
            if (e.Status.State == EStackState.eOk)
            {

            }
            else
            {

            }
            //throw new NotImplementedException();
        }

        public override void StackStatusStartupChanged(object sender, EventArgsStackStatus e)
        {
            //throw new NotImplementedException();
        }

        public override void StackStatusOptionsChanged(object sender, EventArgsStackStatus e)
        {
            //throw new NotImplementedException();
        }

        internal class AppRestartHandler : IAppRestartHandler
        {
            public void Restart()
            {
                Console.WriteLine("Restart required to complete installation of plugin.");
            }
        }

        public class MediaProviderSupport : IContentDirectorySupportV2
        {
            public MediaProviderSupport(IVirtualFileSystem aVirtualFileSystem)
            {
                iVirtualFileSystem = aVirtualFileSystem;
            }

            public IVirtualFileSystem VirtualFileSystem
            {
                get
                {
                    return iVirtualFileSystem;
                }
            }

            private IVirtualFileSystem iVirtualFileSystem;
        }
    }


}