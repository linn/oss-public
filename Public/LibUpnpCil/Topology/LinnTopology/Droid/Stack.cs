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
using Android.Runtime;

namespace LinnTopologyDroid
{
    [Application(Icon="@drawable/icon", Label="Linn Topology")]
    public class Stack : ApplicationDroid, IStack
    {


        public Stack(IntPtr aHandle, JniHandleOwnership aHandleOwnership)
            : base(aHandle, aHandleOwnership)
        {
            iLockObject = new object();
            Android.Util.Log.Info("LOG", "STACK CONSTRUCTOR CALLED");
            iScheduler = new Scheduler("StackScheduler", 1);
            iHouseModel = new HouseModel();
            iLibraryModel = new LibraryModel();
            iScheduler.SchedulerError += SchedulerErrorHandler;
        }

        private void SchedulerErrorHandler(object sender, EventArgsSchedulerError e)
        {
            throw e.Error;
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
            iUserLogListener = new AndroidUserLogListener();
            UserLog.AddListener(iUserLogListener);
            iTraceListener = new AndroidTraceListener();
            Trace.AddListener(iTraceListener);
            iWifiManager = (WifiManager)GetSystemService(Context.WifiService);
            if (iWifiManager != null)
            {
                iWifiLock = iWifiManager.CreateWifiLock("myWifiLock");
                iWifiLock.Acquire();
                iMulticastLock = iWifiManager.CreateMulticastLock("myMcastlock");
                iMulticastLock.Acquire();
            }

            iHelper = new Helper(new string[0] { });
            OptionPageCrashDumper optionCrashDumper = new OptionPageCrashDumper();
            iHelper.AddOptionPage(optionCrashDumper);
            iWifiListener = new WifiListener(iHelper);
            RegisterReceiver(iWifiListener, new IntentFilter(Android.Net.Wifi.WifiManager.NetworkStateChangedAction));
            iWifiListener.Refresh(this.ApplicationContext);

            iHelper.ProcessOptionsFileAndCommandLine();
            iHelper.Stack.SetStack(this);
            iCrashLogDumper = new CrashDumper(this.ApplicationContext, Resource.Drawable.Icon, iHelper, optionCrashDumper);
            iHelper.AddCrashLogDumper(iCrashLogDumper);
            iEventServer = new EventServerUpnp();
            iListenerNotify = new SsdpListenerMulticast();

            iHouse = new House(iListenerNotify, iEventServer, new ModelFactory());
            iHouse.EventRoomAdded += RoomAdded;
            iHouse.EventRoomRemoved += RoomRemoved;

            iLibrary = new Library(iListenerNotify);
            iLibrary.EventMediaServerAdded += LibraryAdded;
            iLibrary.EventMediaServerRemoved += LibraryRemoved;

            iRescanTimer = new System.Timers.Timer(kRescanTimeoutMilliseconds);
            iRescanTimer.Elapsed += (d, e) =>
            {
                if (iHelper.Stack.Status.State == EStackState.eOk)
                {
                    try
                    {
                        if (iHouse != null)
                        {
                            iHouse.Rescan();
                        }

                        if (iLibrary != null)
                        {
                            iLibrary.Rescan();
                        }
                    }
                    catch (Exception ex)
                    {
                        UserLog.WriteLine("Error caught on rescan: " + ex);
                    }
                }
            };

            iHelper.Stack.Start();
        }


        private void StopStack()
        {
            iRescanTimer.Dispose();
            iWifiLock.Release();
            iMulticastLock.Release();
            UnregisterReceiver(iWifiListener);
            iHouse.EventRoomAdded -= RoomAdded;
            iHouse.EventRoomRemoved -= RoomRemoved;
            iLibrary.EventMediaServerAdded -= LibraryAdded;
            iLibrary.EventMediaServerRemoved -= LibraryRemoved;
            iHelper.Stack.Stop();
            iHelper.RemoveCrashLogDumper(iCrashLogDumper);
            iHelper.Dispose();
            UserLog.RemoveListener(iUserLogListener);
            Trace.RemoveListener(iTraceListener);
        }

        private void RoomAdded(object obj, EventArgsRoom e)
        {
            iHouseModel.AddRoom(e.Room);
        }

        private void RoomRemoved(object obj, EventArgsRoom e)
        {
            iHouseModel.RemoveRoom(e.Room);
        }

        private void LibraryAdded(object sender, Library.EventArgsMediaServer e)
        {
            iLibraryModel.AddMediaServer(e.MediaServer);
        }

        private void LibraryRemoved(object sender, Library.EventArgsMediaServer e)
        {
            iLibraryModel.RemoveMediaServer(e.MediaServer);
        }

        public IHouseModel House
        {
            get { return iHouseModel; }
        }
        public ILibraryModel Library
        {
            get { return iLibraryModel; }
        }

        #region IStack Members

        public void Start(System.Net.IPAddress aIpAddress)
        {
            iIpAddress = aIpAddress;
            iEventServer.Start(aIpAddress);
            iListenerNotify.Start(aIpAddress);
            iHouse.Start(aIpAddress);
            iLibrary.Start(aIpAddress);
            iRescanTimer.Start();
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
            iLibrary.Stop();
            iHouse.Stop();
            iListenerNotify.Stop();
            iEventServer.Stop();
            iHouseModel.Clear();
            iLibraryModel.Clear();
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
        private System.Net.IPAddress iIpAddress;
        private WifiListener iWifiListener;
        private Helper iHelper;
        private EventServerUpnp iEventServer;
        private SsdpListenerMulticast iListenerNotify;
        private House iHouse;
        private HouseModel iHouseModel;
        private Library iLibrary;
        private LibraryModel iLibraryModel;
        private System.Timers.Timer iRescanTimer;
        private const double kRescanTimeoutMilliseconds = 5000;
        private Android.Net.Wifi.WifiManager.WifiLock iWifiLock;
        private Android.Net.Wifi.WifiManager.MulticastLock iMulticastLock;
        private WifiManager iWifiManager;
        private AndroidUserLogListener iUserLogListener;
        private AndroidTraceListener iTraceListener;
        private CrashDumper iCrashLogDumper;
        private Scheduler iScheduler;
        private object iLockObject;
        private bool iStarted;
    }

    public interface IHouseModel
    {
        event EventHandler<EventArgsRoom> RoomAdded;
        event EventHandler<EventArgsRoom> RoomRemoved;
        ReadOnlyCollection<IRoom> Rooms { get; }
    }

    public class HouseModel : IHouseModel
    {
        public HouseModel()
        {
            iRooms = new List<IRoom>();
        }
        internal void AddRoom(IRoom aRoom)
        {
            lock (iRooms)
            {
                iRooms.Add(aRoom);
            }
            OnRoomAdded(aRoom);
        }
        internal void RemoveRoom(IRoom aRoom)
        {
            lock (iRooms)
            {
                iRooms.Remove(aRoom);
            }
            OnRoomRemoved(aRoom);
        }
        internal void Clear()
        {
            ReadOnlyCollection<IRoom> roomList = null;
            lock (iRooms)
            {
                roomList = Rooms;
                iRooms.Clear();
            }
            foreach (IRoom room in roomList)
            {
                OnRoomRemoved(room);
            }
        }
        private void OnRoomAdded(IRoom aRoom)
        {
            if (RoomAdded != null)
            {
                RoomAdded(this, new EventArgsRoom(aRoom));
            }
        }
        private void OnRoomRemoved(IRoom aRoom)
        {
            if (RoomRemoved != null)
            {
                RoomRemoved(this, new EventArgsRoom(aRoom));
            }
        }
        #region IHouseModel Members

        public event EventHandler<EventArgsRoom> RoomAdded;

        public event EventHandler<EventArgsRoom> RoomRemoved;

        public ReadOnlyCollection<IRoom> Rooms
        {
            get
            {
                lock (iRooms)
                {
                    return new List<IRoom>(iRooms).AsReadOnly();
                }
            }
        }

        #endregion
        private List<IRoom> iRooms;
    }


    public interface ILibraryModel
    {
        event EventHandler<EventArgsMediaServer> MediaServerAdded;
        event EventHandler<EventArgsMediaServer> MediaServerRemoved;
        ReadOnlyCollection<MediaServer> MediaServers { get; }
    }

    public class LibraryModel : ILibraryModel
    {
        public LibraryModel()
        {
            iMediaServers = new List<MediaServer>();
        }
        internal void AddMediaServer(MediaServer aMediaServer)
        {
            lock (iMediaServers)
            {
                iMediaServers.Add(aMediaServer);
            }
            OnMediaServerAdded(aMediaServer);
        }
        internal void RemoveMediaServer(MediaServer aMediaServer)
        {
            lock (iMediaServers)
            {
                iMediaServers.Remove(aMediaServer);
            }
            OnMediaServerRemoved(aMediaServer);
        }
        internal void Clear()
        {
            ReadOnlyCollection<MediaServer> mediaServerList = null;
            lock (iMediaServers)
            {
                mediaServerList = MediaServers;
                iMediaServers.Clear();
            }
            foreach (MediaServer server in mediaServerList)
            {
                OnMediaServerRemoved(server);
            }
        }
        private void OnMediaServerAdded(MediaServer aMediaServer)
        {
            if (MediaServerAdded != null)
            {
                MediaServerAdded(this, new EventArgsMediaServer(aMediaServer));
            }
        }
        private void OnMediaServerRemoved(MediaServer aMediaServer)
        {
            if (MediaServerRemoved != null)
            {
                MediaServerRemoved(this, new EventArgsMediaServer(aMediaServer));
            }
        }
        #region IHouseModel Members

        public event EventHandler<EventArgsMediaServer> MediaServerAdded;

        public event EventHandler<EventArgsMediaServer> MediaServerRemoved;

        public ReadOnlyCollection<MediaServer> MediaServers
        {
            get
            {
                lock (iMediaServers)
                {
                    return new List<MediaServer>(iMediaServers).AsReadOnly();
                }
            }
        }

        #endregion
        private List<MediaServer> iMediaServers;
    }
    public class EventArgsMediaServer : EventArgs
    {
        internal EventArgsMediaServer(MediaServer aMediaServer)
        {
            MediaServer = aMediaServer;
        }

        public MediaServer MediaServer;
    }

}