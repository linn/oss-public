using System;
using System.Net;
using System.Collections.Generic;

using Linn;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;
using Linn.Topology;

using Upnp;

namespace Linn.Kinsky
{
    public class PlaylistManagerNotFoundException : Exception
    {
        public PlaylistManagerNotFoundException()
        {
        }

        public PlaylistManagerNotFoundException(string aMessage)
            : base(aMessage)
        {
        }
    }

    public class RemotePlaylists : IContainer
    {
        public RemotePlaylists(HelperKinsky aHelper)
        {
            iEventProvider = aHelper.EventServer;

            iLock = new object();
            iStarted = false;

            iMetadata = new container();
            iMetadata.Id = kRootId;
            iMetadata.Title = "Remote Playlists";
            iMetadata.WriteStatus = "PROTECTED";
            iMetadata.Restricted = true;
            iMetadata.Searchable = true;

            iPlaylistManagers = new SortedList<PlaylistManager, ModelPlaylistManager>(new PlaylistManagerComparer());
            iManagers = new PlaylistManagers(aHelper.SsdpNotifyProvider);
        }

        public void Start(IPAddress aIpAddress)
        {
            Trace.WriteLine(Trace.kMediaServer, "RemotePlaylists.Start(): Using interface " + aIpAddress);

            Assert.Check(!iStarted);

            iManagers.EventPlaylistManagerAdded += PlaylistManagerAdded;
            iManagers.EventPlaylistManagerRemoved += PlaylistManagerRemoved;
            iManagers.Start(aIpAddress);

            iStarted = true;

            Trace.WriteLine(Trace.kMediaServer, "RemotePlaylists.Start() successful");
        }

        public void Stop()
        {
            if (iStarted)
            {
                iManagers.Stop();
                iManagers.EventPlaylistManagerAdded -= PlaylistManagerAdded;
                iManagers.EventPlaylistManagerRemoved -= PlaylistManagerRemoved;

                lock (iLock)
                {
                    iPlaylistManagers.Clear();
                    iMetadata.ChildCount = 0;
                }

                iStarted = false;

                if (EventContentUpdated != null)
                {
                    EventContentUpdated(this, EventArgs.Empty);
                }

                Trace.WriteLine(Trace.kMediaServer, "RemotePlaylists.Stop() successful");
            }
            else
            {
                Trace.WriteLine(Trace.kMediaServer, "RemotePlaylists.Stop() already stopped - silently do nothing");
            }
        }

        public uint Open()
        {
            lock (iLock)
            {
                return (uint)iPlaylistManagers.Count;
            }
        }

        public void Close()
        {
        }

        public void Refresh()
        {
            iManagers.Rescan();
        }

        public IContainer ChildContainer(container aContainer)
        {
            lock (iLock)
            {
                foreach (ModelPlaylistManager pm in iPlaylistManagers.Values)
                {
                    if (pm.Metadata[0].Id == aContainer.Id)
                    {
                        return new ContainerPlaylistManager(pm, aContainer);
                    }
                }
                return null;
            }
        }

        public container Metadata
        {
            get
            {
                return iMetadata;
            }
        }
        
        public bool HandleMove(DidlLite aDidlLite)
        {
            return false;
        }

        public bool HandleInsert(DidlLite aDidlLite)
        {
            return false;
        }

        public void Insert(string aAfterId, DidlLite aDidlLite)
        {
            throw new NotSupportedException();
        }
        
        public bool HandleDelete(DidlLite aDidlLite)
        {
            return false;
        }

        public void Delete(string aId)
        {
            throw new NotSupportedException();
        }

        public bool HandleRename(upnpObject aObject)
        {
            return false;
        }

        public void Rename(string aId, string aTitle)
        {
            throw new NotSupportedException();
        }

        public DidlLite Items(uint aStartIndex, uint aCount)
        {
            DidlLite didl = new DidlLite();

            lock (iLock)
            {
                for (int i = (int)aStartIndex;
                    i < aStartIndex + aCount && i < iPlaylistManagers.Count;
                    i++)
                {
                    didl.Add(iPlaylistManagers.Values[i].Metadata[0]);
                }
            }

            return didl;
        }

        public DidlLite Search(string aSearchCriterea, uint aStartIndex, uint aCount)
        {
            throw new NotImplementedException();
        }

        public ModelPlaylistManager Find(string aName)
        {
            lock(iLock)
            {
                foreach(ModelPlaylistManager p in iPlaylistManagers.Values)
                {
                    if(p.PlaylistManager.Name == aName)
                    {
                        return p;
                    }
                }

                return null;
            }
        }

        public event EventHandler<EventArgs> EventContentUpdated;
        public event EventHandler<EventArgs> EventContentAdded;
        public event EventHandler<EventArgsContentRemoved> EventContentRemoved;

        private void PlaylistManagerAdded(object sender, PlaylistManagers.EventArgsPlaylistManager e)
        {
            ModelPlaylistManager playlistManager = null;
            try
            {
                playlistManager = new ModelPlaylistManager(e.PlaylistManager, iEventProvider);
                playlistManager.EventPlaylistManagerInitialised += Initialised;
                playlistManager.Open();
            }
            catch (Exception)
            {
                // failed to create the playlist manager - a dodgy playlist manger shouldn't crash the application
                UserLog.WriteLine(string.Format("{0}: Failed to create playlist manager ({1}, {2})", DateTime.Now, e.PlaylistManager.Name, e.PlaylistManager.Device.Model));
            }
        }

        private void Initialised(object sender, EventArgs e)
        {
            ModelPlaylistManager pm = sender as ModelPlaylistManager;
            if(pm != null)
            {
                lock (iLock)
                {
                    iPlaylistManagers.Remove(pm.PlaylistManager);
                    iPlaylistManagers.Add(pm.PlaylistManager, pm);
                    iMetadata.ChildCount = iPlaylistManagers.Count;
                }

                if (EventContentAdded != null)
                {
                    EventContentAdded(this, EventArgs.Empty);
                }
            }
        }

        private void PlaylistManagerRemoved(object sender, PlaylistManagers.EventArgsPlaylistManager e)
        {
            bool removed = false;
            ModelPlaylistManager pm;

            lock (iLock)
            {
                if (iPlaylistManagers.TryGetValue(e.PlaylistManager, out pm))
                {
                    pm.EventPlaylistManagerInitialised -= Initialised;
                    pm.Close();

                    iPlaylistManagers.Remove(e.PlaylistManager);
                    iMetadata.ChildCount = iPlaylistManagers.Count;
                
                    removed = true;
                }
            }

            if (removed && EventContentRemoved != null)
            {
                EventContentRemoved(this, new EventArgsContentRemoved(pm.Metadata[0].Id));
            }
        }

        private class PlaylistManagerComparer : IComparer<PlaylistManager>
        {
            public int Compare(PlaylistManager aPlaylistManager1, PlaylistManager aPlaylistManager2)
            {
                // make sure managers with the same UDN are taken as identical
                if (aPlaylistManager1.Device.Udn == aPlaylistManager2.Device.Udn)
                {
                    return 0;
                }

                // use the name as primary comparer for sorting - fallback to UDN
                // if 2 separate servers have the same name
                int nameCmp = string.Compare(aPlaylistManager1.Name, aPlaylistManager2.Name);
                if (nameCmp != 0)
                {
                    return nameCmp;
                }
                else
                {
                    return aPlaylistManager1.Device.Udn.CompareTo(aPlaylistManager2.Device.Udn);
                }
            }
        }

        public const string kRootId = "Remote Playlists";

        private const uint kCountPerCall = 100;

        private object iLock;
        private bool iStarted;
        private IEventUpnpProvider iEventProvider;

        private container iMetadata;

        private PlaylistManagers iManagers;
        private SortedList<PlaylistManager, ModelPlaylistManager> iPlaylistManagers;
    }

    internal class ContainerPlaylistManager : IContainer
    {
        public ContainerPlaylistManager(ModelPlaylistManager aPlaylistManager, container aContainer)
        {
            iLock = new object();

            iMetadata = aContainer;
            iPlaylistManager = aPlaylistManager;
        }

        #region IContainer implementation
        public uint Open()
        {
            iPlaylistManager.EventPlaylistsChanged += PlaylistsChanged;

            return iPlaylistManager.Count;
        }

        public void Close()
        {
            iPlaylistManager.EventPlaylistsChanged -= PlaylistsChanged;
        }

        public void Refresh()
        {
        }

        public IContainer ChildContainer(container aContainer)
        {
            return new ContainerRemotePlaylist(iPlaylistManager, aContainer);
        }

        public bool HandleMove(DidlLite aDidlLite)
        {
            return false;
        }

        public bool HandleInsert(DidlLite aDidlLite)
        {
            return false;
        }

        public void Insert(string aAfterId, DidlLite aDidlLite)
        {
            throw new NotSupportedException();
            /*foreach(playlistContainer c in aDidlLite)
            {
                iPlaylistManager.Insert(aAfterId, c.Title, c.Description, 0);
            }*/
        }

        public bool HandleDelete(DidlLite aDidlLite)
        {
            return true;
        }

        public void Delete(string aId)
        {
            lock(iLock)
            {
                try
                {
                    iPlaylistManager.Delete(uint.Parse(aId));
                }
                catch(FormatException) { }
            }
        }

        public bool HandleRename(upnpObject aUpnpObject)
        {
            return true;
        }

        public void Rename(string aId, string aTitle)
        {
            try
            {
                iPlaylistManager.SetName(uint.Parse(aId), aTitle);
            }
            catch(FormatException) { }
        }

        public DidlLite Items(uint aStartIndex, uint aCount)
        {
            DidlLite didl = new DidlLite();

            lock(iLock)
            {
                iPlaylistManager.Lock();

                for (uint i = aStartIndex;
                    i < aStartIndex + aCount && i < iPlaylistManager.Count;
                    i++)
                {
                    didl.Add(iPlaylistManager.Playlist(i).DidlLite[0]);
                }

                iPlaylistManager.Unlock();
            }

            return didl;
        }

        public DidlLite Search(string aSearchCriterea, uint aStartIndex, uint aCount)
        {
            throw new NotImplementedException();
        }

        public container Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public event EventHandler<EventArgs> EventContentUpdated;
        public event EventHandler<EventArgs> EventContentAdded;
        public event EventHandler<EventArgsContentRemoved> EventContentRemoved;

        #endregion

        private void PlaylistsChanged(object sender, EventArgs e)
        {
            if(EventContentUpdated != null)
            {
                EventContentUpdated(this, EventArgs.Empty);
            }
        }

        private object iLock;

        private container iMetadata;
        private ModelPlaylistManager iPlaylistManager;
    }

    public class ContainerRemotePlaylist : IContainer
    {
        public ContainerRemotePlaylist(ModelPlaylistManager aPlaylistManager, container aContainer)
        {
            iLock = new object();

            iMetadata = aContainer;
            iPlaylistManager = aPlaylistManager;
        }

        #region IContainer implementation
        public uint Open()
        {
            lock(iLock)
            {
                ++iRefCount;
                if(iRefCount == 1)
                {
                    iPlaylist = iPlaylistManager.CreatePlaylist(uint.Parse(iMetadata.Id));
                    iPlaylist.EventPlaylistChanged += PlaylistChanged;
                }

                return iPlaylist.Count;
            }
        }

        public void Close()
        {
            lock(iLock)
            {
                --iRefCount;
                if(iRefCount == 0)
                {
                    iPlaylist.EventPlaylistChanged -= PlaylistChanged;
                    iPlaylistManager.DestroyPlaylist(iPlaylist);
                    iPlaylist = null;
                }
            }
        }

        public void Refresh()
        {
        }

        public IContainer ChildContainer(container aContainer)
        {
            throw new NotSupportedException();
        }

        public bool HandleMove(DidlLite aDidlLite)
        {
            return true;
        }

        public bool HandleInsert(DidlLite aDidlLite)
        {
            //return true;
            return false;
        }

        public void Insert(string aAfterId, DidlLite aDidlLite)
        {
            //iPlaylist.Insert(uint.Parse(aAfterId), aDidlLite);
            throw new NotSupportedException();
        }

        public bool HandleDelete(DidlLite aDidlLite)
        {
            foreach(upnpObject o in aDidlLite)
            {
                if(o.Id == iMetadata.Id)
                {
                    return false;
                }
            }
            return true;
        }

        public void Delete(string aId)
        {
            lock(iLock)
            {
                try
                {
                    if(aId == iMetadata.Id)
                    {
                        iPlaylistManager.Delete(uint.Parse(aId));
                    }
                    else
                    {
                        iPlaylist.Delete(uint.Parse(aId));
                    }
                }
                catch(FormatException)
                {
                }
            }
        }

        public bool HandleRename(upnpObject aUpnpObject)
        {
            return false;
        }

        public void Rename(string aId, string aTitle)
        {
            throw new NotSupportedException();
        }

        public DidlLite Items(uint aStartIndex, uint aCount)
        {
            DidlLite didl = new DidlLite();

            lock(iLock)
            {
                iPlaylist.Lock();

                for (uint i = aStartIndex;
                    i < aStartIndex + aCount && i < iPlaylist.Count;
                    i++)
                {
                    didl.Add(iPlaylist.Track(i).DidlLite[0]);
                }

                iPlaylist.Unlock();
            }

            return didl;
        }

        public DidlLite Search(string aSearchCriterea, uint aStartIndex, uint aCount)
        {
            throw new NotImplementedException();
        }

        public container Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public event EventHandler<EventArgs> EventContentUpdated;
        public event EventHandler<EventArgs> EventContentAdded;
        public event EventHandler<EventArgsContentRemoved> EventContentRemoved;

        #endregion

        private void PlaylistChanged(object sender, EventArgs e)
        {
            if(EventContentUpdated != null)
            {
                EventContentUpdated(this, EventArgs.Empty);
            }
        }

        private object iLock;
        private uint iRefCount;

        private container iMetadata;
        private ModelPlaylistManager iPlaylistManager;
        private ModelPlaylist iPlaylist;
    }
}

