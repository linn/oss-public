using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;

using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

namespace Linn.Topology
{
    public class Library
    {
        public class EventArgsMediaServer : EventArgs
        {
            internal EventArgsMediaServer(MediaServer aMediaServer)
            {
                MediaServer = aMediaServer;
            }

            public MediaServer MediaServer;
        }

        public event EventHandler<EventArgsMediaServer> EventMediaServerAdded;
        public event EventHandler<EventArgsMediaServer> EventMediaServerRemoved;

        internal interface IJob
        {
            void Execute(Library aLibrary);
        }

        internal abstract class Job : IJob
        {
            public Job(Device aDevice)
            {
                iDevice = aDevice;
            }

            public abstract void Execute(Library aLibrary);

            protected Device iDevice;
        }

        internal class JobMediaServerAdded : Job
        {
            public JobMediaServerAdded(Device aDevice)
                : base(aDevice)
            {
            }

            public override void Execute(Library aLibrary)
            {
                aLibrary.DoMediaServerAdded(iDevice);
            }
        }

        internal class JobMediaServerRemoved : Job
        {
            public JobMediaServerRemoved(Device aDevice)
                : base(aDevice)
            {
            }

            public override void Execute(Library aLibrary)
            {
                aLibrary.DoMediaServerRemoved(iDevice);
            }
        }

        internal class JobCloudServersChanged : IJob
        {
            public JobCloudServersChanged(IList<string> aAddCloudServers, IList<string> aRemoveCloudServers)
            {
                iAddCloudServers = aAddCloudServers;
                iRemoveCloudServers = aRemoveCloudServers;
            }

            public void Execute(Library aLibrary)
            {
                foreach(string s in iAddCloudServers)
                {
                    aLibrary.DoCloudServerAdded(s);
                }

                foreach(string s in iRemoveCloudServers)
                {
                    aLibrary.DoCloudServerRemoved(s);
                }
            }

            private IList<string> iAddCloudServers;
            private IList<string> iRemoveCloudServers;
        }

        internal class JobAbort : IJob
        {
            public JobAbort()
            {
            }

            public void Execute(Library aLibrary)
            {
                Thread.CurrentThread.Abort();
            }
        }

        private const ThreadPriority kPriority = ThreadPriority.Normal;

        public Library(ISsdpNotifyProvider aListenerNotify)
        {
            // create discovery system

            iDeviceListContentDirectory = new DeviceListUpnp(ServiceContentDirectory.ServiceType(), aListenerNotify);
            iDeviceListContentDirectory.EventDeviceAdded += ContentDirectoryAdded;
            iDeviceListContentDirectory.EventDeviceRemoved += ContentDirectoryRemoved;

            iMutex = new Mutex();
            iJobList = new List<IJob>();
            iJobReady = new ManualResetEvent(false);

            iCloudServers = new Dictionary<string, Device>();
            //iCloudServers.Add("http://89.238.133.245:26125/DeviceDescription.xml", new DeviceUpnp("http://89.238.133.245:26125/DeviceDescription.xml"));
        }

        public void SetCloudServers(IList<Uri> aCloudServers)
        {
            List<string> cloudServers = new List<string>();
            List<string> addCloudServers = new List<string>();
            List<string> removeCloudServers = new List<string>();

            foreach(Uri s in aCloudServers)
            {
                cloudServers.Add(s.OriginalString);

                if(!iCloudServers.ContainsKey(s.OriginalString))
                {
                    addCloudServers.Add(s.OriginalString);
                }
            }

            foreach(KeyValuePair<string, Device> k in iCloudServers)
            {
                if(!cloudServers.Contains(k.Key))
                {
                    removeCloudServers.Add(k.Key);
                }
            }

            ScheduleJob(new JobCloudServersChanged(addCloudServers, removeCloudServers));
        }

        public void Start(IPAddress aInterface)
        {
            Assert.Check(iThread == null);

            // start the discovery system

            iThread = new Thread(new ThreadStart(Run));
            iThread.Priority = kPriority;
            iThread.Name = "Library";

            foreach(Device d in iCloudServers.Values)
            {
                DoMediaServerAdded(d);
            }

            iThread.Start();
            iDeviceListContentDirectory.Start(aInterface);
        }

        public void Stop()
        {
            // stop the discovery system

            ScheduleJob(new JobAbort());
            iDeviceListContentDirectory.Stop();
            // iThread can be null if Stop() is called after a stack start previously failed
            if (iThread != null)
            {
                iThread.Join();
                iThread = null;
            }

            iJobList.Clear();
        }

        public void Rescan()
        {
            // rescan

            iDeviceListContentDirectory.Rescan();
        }

        internal void ScheduleJob(IJob aJob)
        {
            iMutex.WaitOne();
            iJobList.Add(aJob);
            iJobReady.Set();
            iMutex.ReleaseMutex();
        }

        private void Run()
        {
            try
            {
                while (true)
                {
                    IJob job = null;
                    try
                    {
                        iMutex.WaitOne();
    
                        if (iJobList.Count == 0)
                        {
                            iJobReady.Reset();

                            try
                            {
                                iMutex.ReleaseMutex();
    
                                iJobReady.WaitOne();
                            }
                            finally
                            {
                                iMutex.WaitOne();
                            }
                        }
    
                        job = iJobList[0];
    
                        iJobList.RemoveAt(0);
                    }
                    finally
                    {
                        iMutex.ReleaseMutex();
                    }
    
                    job.Execute(this);

                }
            }
            catch (ThreadAbortException)
            {
            }
        }

        private void ContentDirectoryAdded(object obj, DeviceList.EventArgsDevice e)
        {
            Trace.WriteLine(Trace.kTopology, "Library+                " + e.Device);

            e.Device.EventOpened += Opened;

            e.Device.Open();
        }

        private void Opened(object obj, EventArgs e)
        {
            ScheduleJob(new JobMediaServerAdded(obj as Device));
        }

        internal void DoMediaServerAdded(Device aDevice)
        {
            if (EventMediaServerAdded != null)
            {
                EventMediaServerAdded(this, new EventArgsMediaServer(new MediaServer(this, aDevice)));
            }
        }

        internal void DoCloudServerAdded(string aLocation)
        {
            try
            {
                if(!iCloudServers.ContainsKey(aLocation))
                {
                    Device device = new DeviceUpnp(aLocation);
                    iCloudServers.Add(aLocation, device);
    
                    Trace.WriteLine(Trace.kTopology, "Cloud+                " + device);
    
                    if (EventMediaServerAdded != null)
                    {
                        EventMediaServerAdded(this, new EventArgsMediaServer(new MediaServer(this, device)));
                    }
                }
            }
            catch(Exception) { }
        }

        private void ContentDirectoryRemoved(object obj, DeviceList.EventArgsDevice e)
        {
            Trace.WriteLine(Trace.kTopology, "Library-                " + e.Device);

            e.Device.EventOpened -= Opened;

            ScheduleJob(new JobMediaServerRemoved(e.Device));
        }

        internal void DoMediaServerRemoved(Device aDevice)
        {
            if (EventMediaServerRemoved != null)
            {
                EventMediaServerRemoved(this, new EventArgsMediaServer(new MediaServer(this, aDevice)));
            }
        }

        internal void DoCloudServerRemoved(string aLocation)
        {
            Device device;
            if(iCloudServers.TryGetValue(aLocation, out device))
            {
                Trace.WriteLine(Trace.kTopology, "Cloud-                " + device);

                iCloudServers.Remove(aLocation);
    
                if (EventMediaServerRemoved != null)
                {
                    EventMediaServerRemoved(this, new EventArgsMediaServer(new MediaServer(this, device)));
                }
            }
        }

        private Mutex iMutex;
        private List<IJob> iJobList;
        private ManualResetEvent iJobReady;
        private Thread iThread;

        private DeviceListUpnp iDeviceListContentDirectory;

        private Dictionary<string, Device> iCloudServers;
    }

    public class MediaServer
    {
        public MediaServer(Library aLibrary, Device aDevice)
        {
            iLibrary = aLibrary;
            iDevice = aDevice;
        }

        public Library Library
        {
            get
            {
                return (iLibrary);
            }
        }

        public string Name
        {
            get
            {
                return (iDevice.Name);
            }
        }

        public Device Device
        {
            get
            {
                return (iDevice);
            }
        }

        public override string ToString()
        {
            return (String.Format("MediaServer({0})", iDevice));
        }

        private Library iLibrary;
        private Device iDevice;
    }

/*
    public class Library
    {
        private DeviceListUpnp iDeviceListMediaServer;

        private bool iStopping;

        public Library(ISsdpNotifyProvider aListenerNotify, IEventUpnpProvider aEventServer)
        {
            // create house mutex

            iMutex = new Mutex();

            EventServer = aEventServer;

            // create public device lists

            iMediaServerList = new SortedList<string, MediaServer>();

            // create discovery system

            iDeviceListMediaServer = new DeviceListUpnp(ServiceContentDirectory.ServiceType(), aListenerNotify);
            iDeviceListMediaServer.EventDeviceAdded += MediaServerAdded;
            iDeviceListMediaServer.EventDeviceRemoved += MediaServerRemoved;
        }

        public void Start(IPAddress aInterface)
        {
            // start the discovery system
            iStopping = false;
            iDeviceListMediaServer.Start(aInterface);
        }

        public void Stop()
        {
            Lock();

            // stop the discovery system
            iStopping = true;
            iDeviceListMediaServer.Stop();

            iMediaServerList.Clear();

            Unlock();
        }

        public void Rescan()
        {
            // rescan

            iDeviceListMediaServer.Rescan();
        }

        // Event system for media server list changes

        public class EventArgsMediaServer : EventArgs
        {
            internal EventArgsMediaServer(MediaServer aMediaServer)
            {
                MediaServer = aMediaServer;
            }

            public MediaServer MediaServer;
        }

        public EventHandler<EventArgsMediaServer> EventMediaServerAdded;
        public EventHandler<EventArgsMediaServer> EventMediaServerRemoved;

        private void MediaServerAdded(object obj, DeviceList.EventArgsDevice e)
        {
            Trace.WriteLine(Trace.kTopology, "MediaServer+         " + e.Device);

            try
            {
                Device device = e.Device;
                string name = device.Name;

                if (name != null)
                {
                    UserLog.WriteLine(DateTime.Now + ": MediaServer+    " + e.Device.Name);

                    MediaServer mediaServer = new MediaServer(this, name, device);

                    Lock();

                    if (!iStopping)
                    {
                        iMediaServerList.Add(e.Device.Udn, mediaServer);

                        Unlock();

                        if (EventMediaServerAdded != null)
                        {
                            EventMediaServerAdded(this, new EventArgsMediaServer(mediaServer));
                        }
                    }
                    else
                    {
                        Unlock();
                    }
                }
            }
            catch (DeviceException)
            {
            }
        }

        private void MediaServerRemoved(object obj, DeviceList.EventArgsDevice e)
        {
            UserLog.WriteLine(DateTime.Now + ": MediaServer-    " + e.Device.Name);
            Trace.WriteLine(Trace.kTopology, "MediaServer-            " + e.Device);

            Lock();

            if (!iStopping)
            {
                MediaServer mediaServer;

                iMediaServerList.TryGetValue(e.Device.Udn, out mediaServer);

                if (mediaServer == null)
                {
                    Unlock();

                    return;
                }

                iMediaServerList.Remove(e.Device.Udn);

                Unlock();

                if (EventMediaServerRemoved != null)
                {
                    EventMediaServerRemoved(this, new EventArgsMediaServer(mediaServer));
                }
            }
            else
            {
                Unlock();
            }
        }

        // Lock functions

        public void Lock()
        {
            iMutex.WaitOne();
        }

        public void Unlock()
        {
            iMutex.ReleaseMutex();
        }

        // Accessor functions

        public SortedList<string, MediaServer> MediaServerList
        {
            get
            {
                return (iMediaServerList);
            }
        }

        private Mutex iMutex;

        public IEventUpnpProvider EventServer;

        private SortedList<string, MediaServer> iMediaServerList;
    }
*/
}
