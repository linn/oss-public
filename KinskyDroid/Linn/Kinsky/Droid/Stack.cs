using Android.App;
using Linn;
using System.Threading;
using Android.Content;
using System;
using Android.Net.Wifi;
using Linn.ControlPoint.Upnp;
using Linn.Topology;
using Linn.Control.Ssdp;
using Android.OS;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using OssToolkitDroid;
using Linn.Kinsky;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Runtime;

namespace KinskyDroid
{
    [Application(Icon = "@drawable/icon", Label = "Linn Kinsky", Debuggable = true)] //add Debuggable=true parameter for ddms support!
    public class Stack : ApplicationDroid, IStack
    {

        public Stack(IntPtr aHandle, JniHandleOwnership aHandleOwnership)
            : base(aHandle, aHandleOwnership)
        {
            iLockObject = new object();
            iUserLogListener = new AndroidUserLogListener();
            UserLog.AddListener(iUserLogListener);
            iTraceListener = new AndroidTraceListener();
            Trace.AddListener(iTraceListener);

            iScheduler = new Scheduler("StackScheduler", 1);
            iScheduler.SchedulerError += SchedulerErrorHandler;
            iInvoker = new Invoker(this.ApplicationContext);
            iResourceManager = new AndroidResourceManager(this.Resources);

            iScheduler.Schedule(() =>
            {
                InitialiseStack();
            });
        }


        private void InitialiseStack()
        {            
            iHelperKinsky = new HelperKinsky(new string[0] { }, Invoker);
            OptionPageCrashDumper crashDumperOptions = new OptionPageCrashDumper();
            iHelperKinsky.AddOptionPage(crashDumperOptions);
            iHelperKinsky.SetStackExtender(this);
            iCrashLogDumper = new CrashDumper(this.ApplicationContext, Resource.Drawable.Icon, iHelperKinsky, crashDumperOptions);
            iHelperKinsky.AddCrashLogDumper(iCrashLogDumper);

            iEventServer = new EventServerUpnp();
            iListenerNotify = new SsdpListenerMulticast();


            IImage<Bitmap> errorImage = new AndroidImageWrapper(ResourceManager.GetBitmap(Resource.Drawable.Loading));
            AndroidImageLoader loader = new AndroidImageLoader(new ArtworkDownscalingUriConverter(kDownscaleImageSize));
            iImageCache = new AndroidImageCache(kMaxImageCacheSize, kDownscaleImageSize, 2, errorImage, loader, iInvoker);
            iIconResolver = new IconResolver(iResourceManager);
                        
            iViewMaster = new ViewMaster();
            iHttpServer = new HttpServer(HttpServer.kPortKinskyDroid);
            iHttpClient = new HttpClient();

            iLibrary = new MediaProviderLibrary(iHelperKinsky);
            iRemotePlaylists = new RemotePlaylists(iHelperKinsky);
            iLocalPlaylists = new LocalPlaylists(iHelperKinsky, true);

            PluginManager pluginManager = new PluginManager(iHelperKinsky, iHttpClient, new MediaProviderSupport(iHttpServer));
            iLocator = new ContentDirectoryLocator(pluginManager, new AppRestartHandler());

            OptionBool optionRemotePlaylists = iLocator.Add(RemotePlaylists.kRootId, iRemotePlaylists);
            OptionBool optionLocalPlaylists = iLocator.Add(LocalPlaylists.kRootId, iLocalPlaylists);
            iLocator.Add(MediaProviderLibrary.kLibraryId, iLibrary);
            iHelperKinsky.AddOptionPage(iLocator.OptionPage);

            iSaveSupport = new SaveSupport(iHelperKinsky, iRemotePlaylists, optionRemotePlaylists, iLocalPlaylists, optionLocalPlaylists);
            iViewSaveSupport = new ViewSaveSupport(RequestLocalPlaylistFilename, iSaveSupport);
            iPlaySupport = new PlaySupport();
            iHelperKinsky.ProcessOptionsFileAndCommandLine();

            Linn.Kinsky.Model model = new Linn.Kinsky.Model(iViewMaster, new PlaySupport());
            iMediator = new Mediator(iHelperKinsky, model);
            iViewWidgetSelectorRoom = new ViewWidgetSelector<Linn.Kinsky.Room>();
            iViewMaster.ViewWidgetSelectorRoom.Add(iViewWidgetSelectorRoom);
            iViewWidgetSelectorSource = new ViewWidgetSelector<Linn.Kinsky.Source>();
            iViewMaster.ViewWidgetSelectorSource.Add(iViewWidgetSelectorSource);
        }

        public ViewWidgetSelector<Linn.Kinsky.Room> RoomSelector
        {
            get
            {
                return iViewWidgetSelectorRoom;
            }
        }

        public ViewWidgetSelector<Linn.Kinsky.Source> SourceSelector
        {
            get
            {
                return iViewWidgetSelectorSource;
            }
        }

        private void SchedulerErrorHandler(object sender, EventArgsSchedulerError e)
        {
            throw e.Error;
        }

        public IInvoker Invoker
        {
            get
            {
                return iInvoker;
            }
        }

        public IconResolver IconResolver
        {
            get
            {
                return iIconResolver;
            }
        }

        public AndroidResourceManager ResourceManager
        {
            get
            {
                return iResourceManager;
            }
        }

        public AndroidImageCache ImageCache
        {
            get
            {
                return iImageCache;
            }
        }

        public Location Location
        {
            get
            {
                return new Location(iLocator.Root);
            }
        }

        protected override void OnStart()
        {
            iScheduler.Schedule(() =>
            {
                StartStack();
            });
        }

        protected override void OnStop()
        {
            iScheduler.Schedule(() =>
            {
                StopStack();
            });
        }

        protected override System.Reflection.Assembly GetEntryAssembly()
        {
            return System.Reflection.Assembly.GetExecutingAssembly();
        }

        private void StartStack()
        {
            UserLog.WriteLine("START STACK");
            iWifiManager = (WifiManager)GetSystemService(Context.WifiService);
            if (iWifiManager != null)
            {
                iWifiLock = iWifiManager.CreateWifiLock("myWifiLock");
                iWifiLock.Acquire();
                iMulticastLock = iWifiManager.CreateMulticastLock("myMcastlock");
                iMulticastLock.Acquire();
            }

            iWifiListener = new WifiListener(iHelperKinsky);
            RegisterReceiver(iWifiListener, new IntentFilter(Android.Net.Wifi.WifiManager.NetworkStateChangedAction));
            iWifiListener.Refresh(this.ApplicationContext);

            iRescanTimer = new System.Timers.Timer(kRescanTimeoutMilliseconds);
            iRescanTimer.Elapsed += (d, e) =>
            {
                if (iHelperKinsky.Stack.Status.State == EStackState.eOk)
                {
                    try
                    {
                        //todo: re-enable this
                        //iHelperKinsky.Rescan();
                    }
                    catch (Exception ex)
                    {
                        UserLog.WriteLine("Error caught on rescan: " + ex);
                    }
                }
            };

            iHelperKinsky.Stack.Start();
            UserLog.WriteLine("STACK STARTED");
        }


        private void StopStack()
        {
            UserLog.WriteLine("STOP STACK");
            iRescanTimer.Dispose();
            iHelperKinsky.Stack.Stop();
            iWifiLock.Release();
            iMulticastLock.Release();
            UnregisterReceiver(iWifiListener);
            UserLog.WriteLine("STACK STOPPED");
        }

        #region IStack Members

        public void Start(System.Net.IPAddress aIpAddress)
        {
            iIpAddress = aIpAddress;
            iEventServer.Start(aIpAddress);
            iListenerNotify.Start(aIpAddress);
            iRescanTimer.Start();

            iMediator.Open();
            iLibrary.Start(aIpAddress);
            iHttpClient.Start();
            iHttpServer.Start(aIpAddress);
            iLocator.Start();

            iStarted = true;
            EventHandler<EventArgs> eventStarted = iEventStackStarted;
            if (eventStarted != null)
            {
                eventStarted(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            iRescanTimer.Stop();
            iListenerNotify.Stop();
            iEventServer.Stop();

            iLocator.Stop();
            iHttpServer.Stop();
            iHttpClient.Stop();
            iLibrary.Stop();
            iMediator.Close();

            iStarted = false;
            EventHandler<EventArgs> eventStopped = iEventStackStopped;
            if (eventStopped != null)
            {
                eventStopped(this, EventArgs.Empty);
            }
        }

        #endregion

        private EventHandler<EventArgs> iEventStackStarted;
        public event EventHandler<EventArgs> StackStarted
        {
            add
            {
                lock (iLockObject)
                {
                    if (iStarted)
                    {
                        value(this, EventArgs.Empty);
                    }
                    iEventStackStarted += value;
                }
            }
            remove
            {
                lock (iLockObject)
                {
                    iEventStackStarted -= value;
                }
            }
        }

        private EventHandler<EventArgs> iEventStackStopped;
        public event EventHandler<EventArgs> StackStopped
        {
            add
            {
                lock (iLockObject)
                {
                    if (!iStarted)
                    {
                        value(this, EventArgs.Empty);
                    }
                    iEventStackStopped += value;
                }
            }
            remove
            {
                lock (iLockObject)
                {
                    iEventStackStopped -= value;
                }
            }
        }

        private void RequestLocalPlaylistFilename(ISaveSupport aSaveSupport)
        {
            UserLog.WriteLine("RequestLocalPlaylistFilename: not implemented.");
            throw new NotImplementedException("RequestLocalPlaylistFilename: not implemented.");
        }

        private System.Net.IPAddress iIpAddress;
        private WifiListener iWifiListener;
        private HelperKinsky iHelperKinsky;
        private EventServerUpnp iEventServer;
        private SsdpListenerMulticast iListenerNotify;
        private System.Timers.Timer iRescanTimer;
        private const double kRescanTimeoutMilliseconds = 5000;
        private Android.Net.Wifi.WifiManager.WifiLock iWifiLock;
        private Android.Net.Wifi.WifiManager.MulticastLock iMulticastLock;
        private WifiManager iWifiManager;
        private AndroidUserLogListener iUserLogListener;
        private AndroidTraceListener iTraceListener;
        private CrashDumper iCrashLogDumper;
        private Scheduler iScheduler;
        private const int kDownscaleImageSize = 128;
        private const int kMaxImageCacheSize = 5 * 1024 * 1024;
        private const int kImageCacheThreadCount = 2;
        private AndroidImageCache iImageCache;
        private AndroidResourceManager iResourceManager;
        private IInvoker iInvoker;
        private ContentDirectoryLocator iLocator;
        private HttpClient iHttpClient;
        private HttpServer iHttpServer;
        private LocalPlaylists iLocalPlaylists;
        private RemotePlaylists iRemotePlaylists;
        private ViewMaster iViewMaster;
        private MediaProviderLibrary iLibrary;
        private Mediator iMediator;
        private bool iStarted;
        private object iLockObject;
        private IconResolver iIconResolver;
        private SaveSupport iSaveSupport;
        private ViewSaveSupport iViewSaveSupport;
        private PlaySupport iPlaySupport;

        private ViewWidgetSelector<Linn.Kinsky.Room> iViewWidgetSelectorRoom;
        private ViewWidgetSelector<Linn.Kinsky.Source> iViewWidgetSelectorSource;
    }

    internal class AppRestartHandler : IAppRestartHandler
    {
        public void Restart()
        {
            UserLog.WriteLine("Restart required to complete installation of plugin.");
        }
    }

    internal class MediaProviderSupport : IContentDirectorySupportV2
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

    public class IconResolver : AbstractIconResolver<Bitmap>
    {
        public IconResolver(AndroidResourceManager aResourceManager)
        {
            iResourceManager = aResourceManager;
        }
        private AndroidResourceManager iResourceManager;

        public override Icon<Bitmap> IconSource
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Source)); }
        }

        public override Icon<Bitmap> IconDiscSource
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.CD)); }
        }

        public override Icon<Bitmap> IconPlaylistSource
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.PlaylistSource)); }
        }

        public override Icon<Bitmap> IconRadioSource
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Radio)); }
        }

        public override Icon<Bitmap> IconUpnpAvSource
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.UPNP)); }
        }

        public override Icon<Bitmap> IconSenderSource
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Sender)); }
        }

        public override Icon<Bitmap> IconSenderSourceNoSend
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.SenderNoSend)); }
        }

        public override Icon<Bitmap> IconAlbum
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Album)); }
        }

        public override Icon<Bitmap> IconArtist
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Artist)); }
        }

        public override Icon<Bitmap> IconPlaylistContainer
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Playlist)); }
        }

        public override Icon<Bitmap> IconLibrary
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Library)); }
        }

        public override Icon<Bitmap> IconDirectory
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Directory)); }
        }

        public override Icon<Bitmap> IconRadio
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Radio)); }
        }

        public override Icon<Bitmap> IconVideo
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Video)); }
        }

        public override Icon<Bitmap> IconPlaylistItem
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.PlaylistItem)); }
        }

        public override Icon<Bitmap> IconError
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Error)); }
        }

        public override Icon<Bitmap> IconTrack
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Track)); }
        }

        public override Icon<Bitmap> IconRoom
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Room)); }
        }

        public override Icon<Bitmap> IconNoArtwork
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Loading)); }
        }

        public override Icon<Bitmap> IconBookmark
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Bookmark)); }
        }

        public override Icon<Bitmap> IconLoading
        {
            get { return new Icon<Bitmap>(iResourceManager.GetBitmap(Resource.Drawable.Loading)); }
        }

    }
}