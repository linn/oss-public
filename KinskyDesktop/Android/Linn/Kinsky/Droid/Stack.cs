using Android.App;
using Linn;
using System.Threading;
using Android.Content;
using System;
using Android.Net.Wifi;
using OssToolkitDroid;
using Linn.Kinsky;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace KinskyDroid
{
    [Application(Icon = "@drawable/icon", Label = "Kinsky", Debuggable = true)] //add Debuggable=true parameter for ddms support!
    public class Stack : ApplicationDroid, IStack
    {

        public Stack(IntPtr aHandle, JniHandleOwnership aHandleOwnership)
            : base(aHandle, aHandleOwnership)
        {
            iEventCreated = new AutoResetEvent(false);
        }

        public bool IsTabletView
        {
            get
            {
                lock (this)
                {
                    if (!iIsTabletViewDetected)
                    {
                        try
                        {
                            // Compute screen size 
                            DisplayMetrics dm = this.ApplicationContext.Resources.DisplayMetrics;
                            UserLog.WriteLine("Display Metrics: WidthPixels=" + dm.WidthPixels + ", Xdpi=" + dm.Xdpi + ", HeightPixels=" + dm.HeightPixels + ", Ydpi=" + dm.Ydpi);
                            float screenWidth = dm.WidthPixels / dm.Xdpi;
                            float screenHeight = dm.HeightPixels / dm.Ydpi;
                            double size = Math.Sqrt(Math.Pow(screenWidth, 2) +
                                                    Math.Pow(screenHeight, 2));
                            // Tablet devices should have a screen size greater than 8 inches 
                            iIsTabletView = size >= 8;
                            iIsTabletViewDetected = true;
                        }
                        catch (Exception ex)
                        {
                            UserLog.WriteLine("Exception caught computing display metrics: " + ex);
                            return false;
                        }
                    }
                    return iIsTabletView;
                }
            }
        }

        public override void OnCreate()
        {
            iScheduler = new Scheduler("StackScheduler", 1);
            iScheduler.SchedulerError += SchedulerErrorHandler;
            iStopTimer = new System.Threading.Timer(StopTimeout);
            iStopTimer.Change(Timeout.Infinite, Timeout.Infinite);

            iScheduler.Schedule(() =>
            {
                InitialiseStack();
            });
            base.OnCreate();
            iEventCreated.Set();
        }

        private void InitialiseStack()
        {
            iEventCreated.WaitOne();
            iEventLock = new object();
            iUserLogListener = new AndroidUserLogListener();
            UserLog.AddListener(iUserLogListener);
            iTraceListener = new AndroidTraceListener();
            Trace.AddListener(iTraceListener);
            iInvoker = new Invoker(this.ApplicationContext);
            iResourceManager = new AndroidResourceManager(this.Resources);
            iIconResolver = new IconResolver(iResourceManager);
            iLayoutInflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);

            iHelperKinsky = new HelperKinsky(new string[0] { }, Invoker);
            Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += UnhandledExceptionRaiser;

            // name the crash dumper section general and add other UI options
            OptionPageCrashDumper generalOptions = new OptionPageCrashDumper("General");
            iOptionExtendedTrackInfo = new OptionBool("trackinfo", "Extended track info", "Show extended track information for the current track", true);
            generalOptions.Add(iOptionExtendedTrackInfo);
            iOptionEnableRocker = new OptionBool("rocker", "Button controls", "Enable button controls for controlling volume and seeking", false);
            generalOptions.Add(iOptionEnableRocker);
            iOptionGroupTracks = new OptionBool("groupplaylist", "Group playlist tracks", "Grouping tracks by album within the playlist window", true);
            generalOptions.Add(iOptionGroupTracks);

            iOptionAutoLock = new OptionEnum("autolock", "Prevent auto-lock", "When to prevent auto-lock");
            iOptionAutoLock.AddDefault(kAutoLockNever);
            iOptionAutoLock.Add(kAutoLockCharging);
            iOptionAutoLock.Add(kAutoLockAlways);
            generalOptions.Add(iOptionAutoLock);
            iOptionAutoLock.EventValueChanged += OptionAutoLock_EventValueChangedHandler;
            iHelperKinsky.AddOptionPage(generalOptions);

            iHelperKinsky.SetStackExtender(this);
            iCrashLogDumper = new CrashDumper(this.ApplicationContext, Resource.Drawable.Icon, iHelperKinsky, generalOptions);
            iHelperKinsky.AddCrashLogDumper(iCrashLogDumper);

            iOptionInsertMode = new OptionInsertMode();
            iHelperKinsky.AddOption(iOptionInsertMode);

            iViewMaster = new ViewMaster();
            iHttpServer = new HttpServer(HttpServer.kPortKinskyDroid);
            iHttpClient = new HttpClient();

            iLibrary = new MediaProviderLibrary(iHelperKinsky);
            iSharedPlaylists = new SharedPlaylists(iHelperKinsky);
            iLocalPlaylists = new LocalPlaylists(iHelperKinsky, false);

            PluginManager pluginManager = new PluginManager(iHelperKinsky, iHttpClient, new MediaProviderSupport(iHttpServer));
            iLocator = new ContentDirectoryLocator(pluginManager, new AppRestartHandler());

            OptionBool optionSharedPlaylists = iLocator.Add(SharedPlaylists.kRootId, iSharedPlaylists);
            OptionBool optionLocalPlaylists = iLocator.Add(LocalPlaylists.kRootId, iLocalPlaylists);
            iLocator.Add(MediaProviderLibrary.kLibraryId, iLibrary);
            iHelperKinsky.AddOptionPage(iLocator.OptionPage);

            iSaveSupport = new SaveSupport(iHelperKinsky, iSharedPlaylists, optionSharedPlaylists, iLocalPlaylists, optionLocalPlaylists);
            iPlaySupport = new PlaySupport();
            iHelperKinsky.ProcessOptionsFileAndCommandLine();

            Linn.Kinsky.Model model = new Linn.Kinsky.Model(iViewMaster, iPlaySupport);
            iMediator = new Mediator(iHelperKinsky, model);

            iAndroidViewMaster = new AndroidViewMaster(this,
                                                       iViewMaster,
                                                       iInvoker,
                                                       iResourceManager,
                                                       iSaveSupport,
                                                       iIconResolver,
                                                       iOptionGroupTracks,
                                                       iOptionExtendedTrackInfo,
                                                       IsTabletView ? kMaxImageCacheSizeTablet : kMaxImageCacheSizePhone);

            iStackWatchdog = new System.Threading.Timer(StackWatchdogExpired);
            iPowerListener = new PowerStateListener(this.ApplicationContext);
            iPowerListener.EventPowerStateChanged += EventPowerStateChangedHandler;
            RegisterReceiver(iPowerListener, new IntentFilter(Intent.ActionBatteryChanged));
            iUserPresentListener = new ActionUserPresentListener(this.ApplicationContext);
            RegisterReceiver(iUserPresentListener, new IntentFilter(Intent.ActionUserPresent));
            iScreenStateListener = new ScreenStateListener(this.ApplicationContext);
            RegisterReceiver(iScreenStateListener, new IntentFilter(Intent.ActionScreenOff));

            iIsCharging = PowerStateListener.IsConnected(this.ApplicationContext);
            iRescanTimer = new System.Threading.Timer((e) =>
            {
                Rescan();
            });
            iRescanTimer.Change(Timeout.Infinite, Timeout.Infinite);
            SetAutoLock();
            EventLowMemory += EventLowMemoryHandler;
            iUserPresentListener.EventUserPresent += EventUserPresentHandler;
            iScreenStateListener.EventScreenStateChanged += EventScreenStateChangedHandler;
            iInitialised = true;
            StartStack();
        }

        private void EventUserPresentHandler(object sender, EventArgs e)
        {
            if (IsRunning)
            {
                iStopTimer.Change(Timeout.Infinite, Timeout.Infinite);
                StartStack();
            }
        }

        private void OptionAutoLock_EventValueChangedHandler(object sender, EventArgs e)
        {
            SetAutoLock();
        }

        private void EventPowerStateChangedHandler(object sender, EventArgsPowerState e)
        {
            iIsCharging = e.IsConnected;
            SetAutoLock();
        }

        private void EventScreenStateChangedHandler(object sender, EventArgsScreenState e)
        {
            Console.WriteLine("EventScreenStateChangedHandler: " + e.IsScreenOn);
            if (!e.IsScreenOn)
            {
                iStopTimer.Change(kStopTimeout, Timeout.Infinite);
            }
        }

        private void SetAutoLock()
        {
            bool locked = iOptionAutoLock.Value == kAutoLockAlways || (iOptionAutoLock.Value == kAutoLockCharging && iIsCharging);
            if (locked != AutoLock)
            {
                AutoLock = locked;
                OnEventAutoLockChanged();
            }
        }

        private void StackWatchdogExpired(object aSender)
        {
            UserLog.WriteLine("Stack start/stop failed to complete in a timely manner, terminating current process...");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public OptionBool OptionExtendedTrackInfo
        {
            get
            {
                return iOptionExtendedTrackInfo;
            }
        }

        public OptionBool OptionGroupTracks
        {
            get
            {
                return iOptionGroupTracks;
            }
        }

        public OptionBool OptionEnableRocker
        {
            get
            {
                return iOptionEnableRocker;
            }
        }

        public OptionInsertMode OptionInsertMode
        {
            get
            {
                return iOptionInsertMode;
            }
        }

        public HelperKinsky HelperKinsky
        {
            get
            {
                return iHelperKinsky;
            }
        }

        public AndroidResourceManager ResourceManager
        {
            get
            {
                return iResourceManager;
            }
        }

        public LayoutInflater LayoutInflater
        {
            get
            {
                return iLayoutInflater;
            }
        }

        public void Rescan()
        {
            iScheduler.Schedule(() =>
                  {
                      try
                      {
                          if (iRunningStack && iHelperKinsky.Stack.Status.State == EStackState.eOk)
                          {
                              iLibrary.Refresh();
                              iHelperKinsky.Rescan();
                              iSharedPlaylists.Refresh();
                          }
                      }
                      catch (Exception ex)
                      {
                          UserLog.WriteLine("Error caught on rescan: " + ex);
                      }
                  });
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

        public IContainer RootLocation
        {
            get
            {
                return iLocator.Root;
            }
        }

        public BreadcrumbTrail CurrentLocation
        {
            get
            {
                return iHelperKinsky.LastLocation.BreadcrumbTrail;
            }
            set
            {
                iHelperKinsky.LastLocation.BreadcrumbTrail = value;
            }
        }

        protected override System.Reflection.Assembly GetEntryAssembly()
        {
            return System.Reflection.Assembly.GetExecutingAssembly();
        }

        protected override void OnStart()
        {
            iStopTimer.Change(Timeout.Infinite, Timeout.Infinite);
            StartStack();
        }

        protected override void OnStop()
        {
            iStopTimer.Change(kStopTimeout, Timeout.Infinite);
        }

        private void StopTimeout(object aState)
        {
            StopStack();
        }

        public void StartStack()
        {
            iScheduler.Schedule(() =>
               {
                   if (!iRunningStack && iInitialised)
                   {
                       iStackWatchdog.Change(kStackWatchdogTimeout, Timeout.Infinite);
                       UserLog.WriteLine("START STACK");
                       iWifiManager = (WifiManager)GetSystemService(Context.WifiService);
                       Assert.Check(iWifiManager != null, "iWifiManager != null");
                       iWifiLock = iWifiManager.CreateWifiLock("myWifiLock");
                       iWifiLock.Acquire();
                       iMulticastLock = iWifiManager.CreateMulticastLock("myMcastlock");
                       iMulticastLock.Acquire();

                       iWifiListener = new WifiListener(iHelperKinsky);
                       RegisterReceiver(iWifiListener, new IntentFilter(Android.Net.Wifi.WifiManager.NetworkStateChangedAction));
                       iWifiListener.Refresh(this.ApplicationContext);


                       iHelperKinsky.Stack.Start();
                       iRescanTimer.Change(kRescanWaitIntervalMilliseconds, kRescanRepeatIntervalMilliseconds);
                       UserLog.WriteLine("STACK STARTED");
                       iStackWatchdog.Change(Timeout.Infinite, Timeout.Infinite);
                       iRunningStack = true;
                   }
               });
        }


        public void StopStack()
        {
            iScheduler.Schedule(() =>
               {
                   if (iRunningStack && iInitialised)
                   {
                       iStackWatchdog.Change(kStackWatchdogTimeout, Timeout.Infinite);
                       UserLog.WriteLine("STOP STACK");
                       iHelperKinsky.Stack.Stop();
                       iWifiLock.Release();
                       iWifiLock.Dispose();
                       iWifiLock = null;
                       iMulticastLock.Release();
                       iMulticastLock.Dispose();
                       iMulticastLock = null;
                       UnregisterReceiver(iWifiListener);
                       iWifiListener.Dispose();
                       iWifiListener = null;
                       iWifiManager.Dispose();
                       iWifiManager = null;
                       iRescanTimer.Change(Timeout.Infinite, Timeout.Infinite);
                       UserLog.WriteLine("STACK STOPPED");
                       iStackWatchdog.Change(Timeout.Infinite, Timeout.Infinite);
                       iRunningStack = false;
                   }
               });
        }

        #region IStack Members

        public void Start(System.Net.IPAddress aIpAddress)
        {
            iIpAddress = aIpAddress;

            iMediator.Open();
            iLibrary.Start(aIpAddress);
            iSharedPlaylists.Start(aIpAddress);
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
            iLocator.Stop();
            iHttpServer.Stop();
            iHttpClient.Stop();
            iSharedPlaylists.Stop();
            iLibrary.Stop();
            iMediator.Close();

            iStarted = false;
            EventHandler<EventArgs> eventStopped = StackStopped;
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
                lock (iEventLock)
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
                lock (iEventLock)
                {
                    iEventStackStarted -= value;
                }
            }
        }

        public event EventHandler<EventArgs> StackStopped;

        public SaveSupport SaveSupport
        {
            get
            {
                return iSaveSupport;
            }
        }

        public PlaySupport PlaySupport
        {
            get
            {
                return iPlaySupport;
            }
        }

        public AndroidViewMaster ViewMaster
        {
            get
            {
                return iAndroidViewMaster;
            }
        }

        public IconResolver IconResolver
        {
            get
            {
                return iIconResolver;
            }
        }

        private void UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            iHelperKinsky.UnhandledException(e.Exception);
            // an unknown error has occurred that could be anywhere - no guarantee
            // that a clean exit can be performed - kill it
            try
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch (Exception) { }
        }

        private void EventLowMemoryHandler(object sender, EventArgs e)
        {
            iAndroidViewMaster.ClearCache();
        }

        public event EventHandler<EventArgs> EventAutoLockChanged;
        private void OnEventAutoLockChanged()
        {
            EventHandler<EventArgs> del = EventAutoLockChanged;
            if (del != null)
            {
                del(this, EventArgs.Empty);
            }
        }

        public bool AutoLock { get; set; }

        private System.Net.IPAddress iIpAddress;
        private WifiListener iWifiListener;
        private HelperKinsky iHelperKinsky;
        private const int kRescanRepeatIntervalMilliseconds = 15 * 60 * 60 * 1000;
        private const int kRescanWaitIntervalMilliseconds = 2 * 1000;
        private Android.Net.Wifi.WifiManager.WifiLock iWifiLock;
        private Android.Net.Wifi.WifiManager.MulticastLock iMulticastLock;
        private WifiManager iWifiManager;
        private AndroidUserLogListener iUserLogListener;
        private AndroidTraceListener iTraceListener;
        private CrashDumper iCrashLogDumper;
        private Scheduler iScheduler;
        private IInvoker iInvoker;
        private ContentDirectoryLocator iLocator;
        private HttpClient iHttpClient;
        private HttpServer iHttpServer;
        private LocalPlaylists iLocalPlaylists;
        private SharedPlaylists iSharedPlaylists;
        private ViewMaster iViewMaster;
        private MediaProviderLibrary iLibrary;
        private Mediator iMediator;
        private volatile bool iStarted;
        private bool iRunningStack; 
        private object iEventLock;
        private SaveSupport iSaveSupport;
        private PlaySupport iPlaySupport;
        private AndroidViewMaster iAndroidViewMaster;
        private const int kMaxImageCacheSizePhone = 1 * 1024 * 1024;
        private const int kMaxImageCacheSizeTablet = 5 * 1024 * 1024;
        private IconResolver iIconResolver;

        // options
        private OptionInsertMode iOptionInsertMode;
        private OptionBool iOptionExtendedTrackInfo;
        private OptionBool iOptionEnableRocker;
        private OptionEnum iOptionAutoLock;
        private OptionBool iOptionGroupTracks;

        private System.Threading.Timer iStackWatchdog;
        private const int kStackWatchdogTimeout = 10000;
        private PowerStateListener iPowerListener;
        private ActionUserPresentListener iUserPresentListener;
        private ScreenStateListener iScreenStateListener;

        private bool iIsCharging;
        private const string kAutoLockNever = "Never";
        private const string kAutoLockCharging = "When charging";
        private const string kAutoLockAlways = "Always";
        private System.Threading.Timer iRescanTimer;
        private AndroidResourceManager iResourceManager;
        private LayoutInflater iLayoutInflater;
        private bool iIsTabletViewDetected;
        private bool iIsTabletView;
        private bool iInitialised;

        private const long kStopTimeout = 30000;
        private System.Threading.Timer iStopTimer;
        private AutoResetEvent iEventCreated;
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

    public class OptionInsertMode : OptionEnum
    {
        public const string kPlayNow = "Play Now";
        public const string kPlayNext = "Play Next";
        public const string kPlayLater = "Play Later";

        public OptionInsertMode()
            : base("insertmode", "Insert Mode", "How to insert tracks into the playlist")
        {
            AddDefault(kPlayNow);
            Add(kPlayNext);
            Add(kPlayLater);
        }
    }
}