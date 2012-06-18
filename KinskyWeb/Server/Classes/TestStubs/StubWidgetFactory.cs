using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KinskyWeb.Kinsky;
using Linn.Kinsky;
using Upnp;
using KinskyWeb.Services;
using System.Xml.Linq;

namespace KinskyWeb.TestStubs
{
    public class StubWidgetFactory<T> : IWidgetFactory<T> where T : class, IKinskyWidget
    {
        public T Create()
        {
            T stub = CreateStub();
            return stub;
        }

        private T CreateStub()
        {
            T stub = null;

            if (typeof(T) == typeof(IKinskyContainer))
            {
                stub = new MockContainer() as T;
            }
            if (typeof(T) == typeof(IViewKinskyVolumeControl))
            {
                stub = new MockViewVolumeControl() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyVolumeControl))
            {
                stub = new MockControllerVolumeControl() as T;
            }
            if (typeof(T) == typeof(IKinskyBrowser))
            {
                stub = new MockBrowser() as T;
            }
            if (typeof(T) == typeof(IViewKinskyTransportControl))
            {
                stub = new MockViewTransportControl() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyTransportControl))
            {
                stub = new MockControllerTransportControl() as T;
            }
            if (typeof(T) == typeof(IViewKinskyRoom))
            {
                stub = new MockViewKinskyRoom() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyRoom))
            {
                stub = new MockControllerKinskyRoom() as T;
            }
            if (typeof(T) == typeof(IViewKinskyRoomList))
            {
                stub = new MockViewKinskyRoomList() as T;
            }
            if (typeof(T) == typeof(IViewKinskySourceList))
            {
                stub = new MockViewKinskySourceList() as T;
            }
            if (typeof(T) == typeof(IViewKinskyMediaTime))
            {
                stub = new MockViewKinskyMediaTime() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyMediaTime))
            {
                stub = new MockControllerKinskyMediaTime() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyDisc))
            {
                stub = new MockControllerDisc() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyPlaylist))
            {
                stub = new MockControllerPlaylist() as T;
            }
            if (typeof(T) == typeof(IViewKinskyPlaylist))
            {
                stub = new MockViewPlaylist() as T;
            }
            if (typeof(T) == typeof(IViewKinskyTrack))
            {
                stub = new MockViewTrack() as T;
            }
            if (typeof(T) == typeof(IViewKinskyPlayMode))
            {
                stub = new MockViewPlayMode() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyPlayMode))
            {
                stub = new MockControllerPlayMode() as T;
            }



            return stub;
        }



        class MockContainer : KinskyContainerBase
        {
            public override void OnContainerTerminated()
            { }
        }

        class MockViewVolumeControl : KinskyWidgetBase, IViewKinskyVolumeControl
        {

            private bool iOpen = false;
            private string iRoom = string.Empty;
            private Object iLockObject = new Object();

            public virtual uint Volume()
            {
                return TopologyStub.House.GetRoom(Room()).Volume;
            }

            public virtual bool Mute()
            {
                return TopologyStub.House.GetRoom(Room()).Mute;
            }

            public virtual bool Connected()
            {
                return true;
            }
            public virtual uint VolumeLimit()
            {
                return TopologyStub.House.GetRoom(Room()).VolumeLimit;
            }

            public virtual string Room()
            {
                return iRoom;
            }

            public virtual void Subscribe(string aRoom)
            {
                lock (iLockObject)
                {
                    if (!iOpen)
                    {
                        iOpen = true;
                        iRoom = aRoom;
                        TopologyStub.House.GetRoom(Room()).VolumeChanged += OnRoomChanged;
                        TopologyStub.House.GetRoom(Room()).MuteChanged += OnRoomChanged;
                        Updated = true;
                    }
                }
            }

            private void OnRoomChanged(Object sender, EventArgs args)
            {
                Updated = true;
            }

            public virtual void Unsubscribe()
            {
                lock (iLockObject)
                {
                    if (iOpen)
                    {
                        iOpen = false;
                        TopologyStub.House.GetRoom(Room()).VolumeChanged -= OnRoomChanged;
                        TopologyStub.House.GetRoom(Room()).MuteChanged -= OnRoomChanged;
                    }
                }
            }

            public override void OnContainerTerminated()
            {
            }

        }

        public class MockControllerVolumeControl : KinskyWidgetBase, IControllerKinskyVolumeControl
        {
            private bool iOpen = false;
            private string iRoom = string.Empty;
            private Object iLockObject = new Object();

            public virtual void SetVolume(uint aVolume)
            {
                TopologyStub.House.GetRoom(Room()).Volume = aVolume;
            }
            public virtual void SetMute(bool aMute)
            {
                TopologyStub.House.GetRoom(Room()).Mute = aMute;
            }

            public virtual string Room()
            {
                return iRoom;
            }

            public virtual void Subscribe(string aRoom)
            {
                lock (iLockObject)
                {
                    if (!iOpen)
                    {
                        iOpen = true;
                        iRoom = aRoom;
                    }
                }
            }

            public virtual void Unsubscribe()
            {
                lock (iLockObject)
                {
                    if (iOpen)
                    {
                        iOpen = false;
                    }
                }
            }
            public override void OnContainerTerminated()
            {
            }
        }

        class MockBrowser : KinskyWidgetBase, IKinskyBrowser
        {
            private LocationDTO[] iLocation;
            private bool iConnected;
            private Thread iScanThread;
            private List<upnpObject> iChildren;
            private Dictionary<string, int> iChildIndexCache;
            private uint iTotalChildCount;
            private bool iCancelScan;
            private Object iLockObject;
            private uint iChunkSize;

            public MockBrowser()
            {
                iChildren = new List<upnpObject>();
                iChildIndexCache = new Dictionary<string, int>();
                iConnected = false;
                iTotalChildCount = 0;
                iCancelScan = false;
                iChunkSize = 5;
                iLockObject = new Object();
            }

            #region IKinskyContentBrowser Members

            public void Subscribe(string[] aLocation)
            {
                lock (iLockObject)
                {
                    if (aLocation.Length > 1 && aLocation[1].Equals("Rooms"))
                    {
                        iLocation = new LocationDTO[aLocation.Length];
                        for (int i = 0; i < aLocation.Length; i++)
                        {
                            LocationDTO dto = new LocationDTO();
                            dto.ID = aLocation[i];
                            dto.BreadcrumbText = aLocation[i];
                            iLocation[i] = dto;
                        }
                        TopologyStub.Room room = TopologyStub.House.GetRoom(aLocation[2]);
                        if (room != null)
                        {
                            TopologyStub.Source source = room.GetSource(aLocation[3]);
                            if (source != null)
                            {
                                source.PlaylistChanged += PlaylistChanged;
                            }
                        }
                    }
                    {
                        iLocation = new LocationDTO[(aLocation.Length == 0) ? 1 : aLocation.Length];
                        if (aLocation.Length == 0)
                        {
                            LocationDTO home = new LocationDTO();
                            home.ID = "0";
                            home.BreadcrumbText = "Home";
                            iLocation[0] = home;
                        }
                        else
                        {
                            for (int i = 0; i < aLocation.Length; i++)
                            {
                                LocationDTO dto = new LocationDTO();
                                dto.ID = aLocation[i];
                                dto.BreadcrumbText = new MediaProviderStub().GetTitle(aLocation[i]);
                                iLocation[i] = dto;
                            }
                        }
                    }
                    StartScan();
                }
            }

            public void Unsubscribe()
            {
                lock (iLockObject)
                {
                    if (iLocation.Length > 1 && iLocation[1].ID.Equals("Rooms"))
                    {
                        TopologyStub.Room room = TopologyStub.House.GetRoom(iLocation[2].ID);
                        if (room != null)
                        {
                            TopologyStub.Source source = room.GetSource(iLocation[3].ID);
                            if (source != null)
                            {
                                source.PlaylistChanged -= PlaylistChanged;
                            }
                        }
                    }
                    StopScan();
                }
            }

            public void PlaylistChanged(object sender, EventArgs e)
            {
                Rescan();
            }

            public void Rescan()
            {
                lock (iLockObject)
                {
                    StartScan();
                }
            }

            public LocationDTO[] CurrentLocation()
            {
                lock (iLockObject)
                {
                    return (from l in iLocation select new LocationDTO() { ID = l.ID, BreadcrumbText = l.BreadcrumbText }).ToArray();
                }
            }

            public bool Connected()
            {
                lock (iLockObject)
                {
                    return iConnected;
                }
            }

            public uint ChildCount()
            {
                lock (iLockObject)
                {
                    return iTotalChildCount;
                }
            }

            public UPnpObjectDTO[] GetChildren(uint aStartIndex, uint aCount)
            {
                lock (iLockObject)
                {
                    List<UPnpObjectDTO> result = new List<UPnpObjectDTO>();
                    if ((aStartIndex + aCount) <= iChildren.Count && (aStartIndex + aCount) <= iTotalChildCount)
                    {
                        foreach (upnpObject child in iChildren)
                        {
                            UPnpObjectDTO item = new UPnpObjectDTO();
                            DidlLite d = new DidlLite();
                            d.Add(child);
                            item.DidlLite = d.Xml;
                            item.ID = child.Id;
                            result.Add(item);
                        }
                    }
                    return result.ToArray();
                }
            }

            #endregion


            private void StartScan()
            {
                Updated = true;
                iConnected = false;
                iChildren.Clear();
                iChildIndexCache.Clear();
                iScanThread = new Thread(ScanThread);
                iScanThread.Start();
            }
            private void StopScan()
            {
                iConnected = false;
                iChildren.Clear();
                iCancelScan = true;
            }

            private void ScanThread()
            {
                int counter = 0;
                if (iLocation.Length > 1 && iLocation[1].ID.Equals("Rooms"))
                {
                    // browsing a source playlist
                    lock (iLockObject)
                    {
                        upnpObject[] playlist = TopologyStub.House.GetRoom(iLocation[2].ID).GetSource(iLocation[3].ID).Playlist();
                        iConnected = true;
                        iTotalChildCount = (uint)playlist.Length;
                        foreach (upnpObject child in playlist)
                        {
                            iChildren.Add(child);
                            iChildIndexCache[child.Id] = counter++;
                        }
                    }

                }
                else
                {
                    // browsing a media server
                    string containerID = "0";
                    if (iLocation.Length > 0)
                    {
                        containerID = iLocation[iLocation.Length - 1].ID;
                    }

                    // simulate an opening delay
                    //Thread.Sleep(5000);

                    DidlLite didl = new MediaProviderStub().GetChildren(containerID);
                    lock (iLockObject)
                    {
                        if (!iCancelScan)
                        {
                            iConnected = true;
                            iTotalChildCount = (uint)didl.Count;
                        }
                        else
                        {
                            return;
                        }
                    }

                    int current = 0;

                    while (current < iTotalChildCount && !iCancelScan)
                    {
                        // simulate a fetching delay
                        //Thread.Sleep(100);

                        // check we're not fetching more than we have
                        int numberToFetch = (int)iChunkSize;
                        if (numberToFetch + current > iTotalChildCount)
                        {
                            numberToFetch = (int)iTotalChildCount - current;
                        }

                        lock (iLockObject)
                        {
                            if (!iCancelScan)
                            {
                                // add the next chunk of objects
                                upnpObject[] newChildren = didl.Skip(current).Take(numberToFetch).ToArray();
                                foreach (upnpObject child in newChildren)
                                {
                                    iChildren.Add(child);
                                    iChildIndexCache[child.Id] = counter++;
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                        current += numberToFetch;
                    }
                }
            }

            public void Activate(UPnpObjectDTO aChild)
            {
                lock (iLockObject)
                {
                    if (iLocation.Length > 1 && iLocation[1].ID.Equals("Rooms"))
                    {
                        TopologyStub.Source source = TopologyStub.House.GetRoom(iLocation[2].ID).GetSource(iLocation[3].ID);
                        if (source != null)
                        {
                            source.SetCurrentTrack(aChild.ID);
                            source.Play();
                        }
                    }
                    else
                    {
                        upnpObject child = iChildren[iChildIndexCache[aChild.ID]];
                        if (child is container)
                        {
                            LocationDTO[] newLocation = new LocationDTO[iLocation.Length + 1];
                            iLocation.CopyTo(newLocation, 0);
                            LocationDTO newDTO = new LocationDTO();
                            newDTO.ID = child.Id;
                            newDTO.BreadcrumbText = child.Title;
                            newLocation[newLocation.Length - 1] = newDTO;
                            iLocation = newLocation;
                            StartScan();
                        }
                    }
                }
            }

            public void Up(uint aNumberLevels)
            {
                int numberOfLevelsUp = iLocation.Length - (int)aNumberLevels;
                if (numberOfLevelsUp < 1)
                {
                    numberOfLevelsUp = 1;
                }
                LocationDTO[] newLocation = new LocationDTO[numberOfLevelsUp];
                for (int i = 0; i < newLocation.Length; i++)
                {
                    newLocation[i] = new LocationDTO() { ID = iLocation[i].ID, BreadcrumbText = iLocation[i].BreadcrumbText };
                }
                iLocation = newLocation;
                StartScan();
            }
            public override void OnContainerTerminated()
            {
            }

            public bool ErrorOccured()
            {
                return false;
            }

            #region IKinskyBrowser Members


            public IContainer CurrentContainer()
            {
                return null;
            }

            #endregion
        }

        class MockControllerPlaylist : KinskyWidgetBase, IControllerKinskyPlaylist
        {
            private Object iLockObject;
            private TopologyStub.Room iRoom;
            private TopologyStub.Source iSource;

            public MockControllerPlaylist()
            {
                iLockObject = new Object();
            }

            public void Subscribe(string aRoom, string aSource)
            {
                lock (iLockObject)
                {
                    iRoom = TopologyStub.House.GetRoom(aRoom);
                    if (iRoom != null)
                    {
                        iSource = iRoom.GetSource(aSource);
                    }
                }
            }

            public void Unsubscribe()
            {
            }

            public void Move(UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem)
            {
                lock (iLockObject)
                {

                    List<TopologyStub.Track> tracks = new List<TopologyStub.Track>();
                    foreach (UPnpObjectDTO child in aChildren)
                    {
                        DidlLite didl = new DidlLite(child.DidlLite.ToString());
                        foreach (upnpObject item in didl)
                        {
                            if (iSource.ContainsTrack(item.Id))
                            {
                                tracks.Add(iSource.GetTrack(item.Id));
                            }
                        }
                    }
                    int position = iSource.TrackIndex(aInsertAfterItem.ID);
                    iSource.Move(tracks.ToArray(), position);
                }
            }
            public void MoveToEnd(UPnpObjectDTO[] aChildren)
            {
                lock (iLockObject)
                {

                    List<TopologyStub.Track> tracks = new List<TopologyStub.Track>();
                    foreach (UPnpObjectDTO child in aChildren)
                    {
                        DidlLite didl = new DidlLite(child.DidlLite.ToString());
                        foreach (upnpObject item in didl)
                        {
                            if (iSource.ContainsTrack(item.Id))
                            {
                                tracks.Add(iSource.GetTrack(item.Id));
                            }
                        }
                    }
                    int position = iSource.Count() - 1;
                    if (position >= 0)
                    {
                        iSource.Move(tracks.ToArray(), position);
                    }
                }
            }
            public void Insert(UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem, IContainer aContainerSource)
            {
                lock (iLockObject)
                {
                    List<upnpObject> tracks = new List<upnpObject>();
                    foreach (UPnpObjectDTO child in aChildren)
                    {
                        DidlLite didl = new DidlLite(child.DidlLite.ToString());
                        foreach (upnpObject item in didl)
                        {
                            if (item is container)
                            {
                                tracks.AddRange(TraverseContainer(item));
                            }
                            else
                            {
                                tracks.Add(item);
                            }
                        }
                    }
                    int position = iSource.TrackIndex(aInsertAfterItem.ID);
                    iSource.Insert(tracks.ToArray(), position);
                }
            }

            upnpObject[] TraverseContainer(upnpObject aContainer)
            {
                //todo: recurse through and pick up child items
                return new upnpObject[] { aContainer };
            }

            public void Delete(UPnpObjectDTO[] aChildren)
            {
                lock (iLockObject)
                {
                    foreach (UPnpObjectDTO child in aChildren)
                    {
                        TopologyStub.Track track = iSource.GetTrack(child.ID);
                        iSource.Delete(track);
                    }
                }
            }
            public void DeleteAll()
            {
                lock (iLockObject)
                {
                    iSource.DeleteAll();
                }
            }
            public override void OnContainerTerminated()
            {
            }
        }

        class MockViewPlaylist : KinskyWidgetBase, IViewKinskyPlaylist
        {
            private Object iLockObject;
            private TopologyStub.Room iRoom;
            private TopologyStub.Source iSource;

            public MockViewPlaylist()
            {
                iLockObject = new Object();
            }

            public void Subscribe(string aRoom, string aSource)
            {
                lock (iLockObject)
                {
                    iRoom = TopologyStub.House.GetRoom(aRoom);
                    if (iRoom != null)
                    {
                        iSource = iRoom.GetSource(aSource);
                        if (iSource != null)
                        {
                            iSource.TrackChanged += TrackChanged;
                        }
                    }
                }
            }

            public void Unsubscribe()
            {
                if (iSource != null)
                {
                    iSource.TrackChanged -= TrackChanged;
                }
            }

            public void TrackChanged(object sender, EventArgs e)
            {
                Updated = true;
            }

            #region IViewKinskyPlaylist Members


            public UPnpObjectDTO CurrentItem()
            {
                UPnpObjectDTO result = new UPnpObjectDTO();
                if (iSource != null)
                {
                    upnpObject current = iSource.CurrentTrack().GetUpnpObject();
                    if (current != null)
                    {
                        DidlLite d = new DidlLite();
                        d.Add(current);
                        result.DidlLite = d.Xml;
                        result.ID = current.Id;
                    }
                }
                return result;
            }
            public override void OnContainerTerminated()
            {
            }

            #endregion
        }

        class MockViewTransportControl : KinskyWidgetBase, IViewKinskyTransportControl
        {

            #region IViewKinskyTransportControl Members

            private KinskyWeb.TestStubs.TopologyStub.Source iSource;

            public ETransportState TransportState()
            {
                return iSource.TransportState();
            }

            public virtual bool Connected()
            {
                return true;
            }
            public void Subscribe(string aRoom, string aSource)
            {
                iSource = TopologyStub.House.GetRoom(aRoom).GetSource(aSource);
                iSource.TransportStateChanged += SourceTransportStateChanged;
                Updated = true;
            }

            public void Unsubscribe()
            {
                iSource.TransportStateChanged -= SourceTransportStateChanged;
            }

            public void SourceTransportStateChanged(object sender, EventArgs e)
            {
                Updated = true;
            }
            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }


            public bool PlayNowEnabled()
            {
                /* TODO: stubbed */
                return false;
            }

            public bool PlayNextEnabled()
            {
                /* TODO: stubbed */
                return false;
            }

            public bool PlayLaterEnabled()
            {
                /* TODO: stubbed */
                return false;
            }

            #endregion
        }
        class MockControllerTransportControl : KinskyWidgetBase, IControllerKinskyTransportControl
        {

            #region IControllerKinskyTransportControl Members

            private KinskyWeb.TestStubs.TopologyStub.Source iSource;

            public void Pause()
            {
                iSource.Pause();
            }

            public void Play()
            {
                iSource.Play();
            }

            public void Stop()
            {
                iSource.Stop();
            }

            public void Previous()
            {
                iSource.Previous();
            }

            public void Next()
            {
                iSource.Next();
            }

            public void PlayNow(IMediaRetriever aMediaRetriever)
            {
                iSource.PlayNow();
            }

            public void PlayNext(IMediaRetriever aMediaRetriever)
            {
                iSource.PlayNext();
            }

            public void PlayLater(IMediaRetriever aMediaRetriever)
            {
                iSource.PlayLater();
            }

            public void Subscribe(string aRoom, string aSource)
            {
                iSource = TopologyStub.House.GetRoom(aRoom).GetSource(aSource);
            }

            public void Unsubscribe()
            {
            }
            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }

            #endregion
        }
        class MockViewKinskyRoom : KinskyWidgetBase, IViewKinskyRoom
        {

            #region IViewKinskyRoom Members
            private string iRoom;
            public void Subscribe(string aRoom)
            {
                iRoom = aRoom;
                Updated = true;
                TopologyStub.House.GetRoom(iRoom).SourceChanged += SourceChanged;
            }

            public void Unsubscribe()
            {
                TopologyStub.House.GetRoom(iRoom).SourceChanged -= SourceChanged;
            }

            public string CurrentSource()
            {
                TopologyStub.Source current = TopologyStub.House.GetRoom(iRoom).CurrentSource();
                if (current != null)
                {
                    return current.Name;
                }
                else
                {
                    return null;
                }
            }

            private void SourceChanged(object sender, EventArgs e)
            {
                Updated = true;
            }
            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }

            public ESourceType CurrentSourceType()
            {
                TopologyStub.Source current = TopologyStub.House.GetRoom(iRoom).CurrentSource();
                if (current != null)
                {
                    return current.Type;
                }
                else
                {
                    return ESourceType.eUnknown;
                }
            }

            #endregion
        }
        class MockControllerKinskyRoom : KinskyWidgetBase, IControllerKinskyRoom
        {

            #region IControllerKinskyRoom Members

            private string iRoom;
            public void Subscribe(string aRoom)
            {
                iRoom = aRoom;
            }

            public void Unsubscribe()
            {
            }

            public void SelectSource(string aSource)
            {
                TopologyStub.House.GetRoom(iRoom).SetCurrentSource(aSource);
            }
            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }
            public void Standby()
            {
                iRoom = null;
            }

            #endregion
        }
        class MockViewKinskyRoomList : KinskyWidgetBase, IViewKinskyRoomList
        {

            #region IViewKinskyRoomList Members

            public void Subscribe()
            {
                TopologyStub.House.RoomsChanged += RoomsChanged;
                Updated = true;
            }

            public void Unsubscribe()
            {
                TopologyStub.House.RoomsChanged -= RoomsChanged;
            }

            private void RoomsChanged(object sender, EventArgs args)
            {
                Updated = true;
            }

            public string[] Rooms()
            {
                return (from r in TopologyStub.House.GetRooms() select r.Name).ToArray();
            }
            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }

            #endregion
        }
        class MockViewKinskySourceList : KinskyWidgetBase, IViewKinskySourceList
        {

            #region IViewKinskySourceList Members
            private TopologyStub.Room iRoom;
            public void Subscribe(string aRoom)
            {
                this.iRoom = TopologyStub.House.GetRoom(aRoom);
                iRoom.SourceChanged += SourcesChanged;
                Updated = true;
            }

            public void Unsubscribe()
            {
                iRoom.SourceChanged -= SourcesChanged;
            }

            public SourceDTO[] Sources()
            {
                return (from s in iRoom.GetSources() select new SourceDTO() { Name = s.Name, Type = s.Type }).ToArray();
            }

            private void SourcesChanged(object sender, EventArgs args)
            {
                Updated = true;
            }
            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }
            public virtual bool Connected()
            {
                return true;
            }

            #endregion
        }

        class MockViewKinskyMediaTime : KinskyWidgetBase, IViewKinskyMediaTime
        {

            #region IViewKinskyMediaTime Members

            private KinskyWeb.TestStubs.TopologyStub.Source iSource;
            public void Subscribe(string aRoom, string aSource)
            {
                iSource = TopologyStub.House.GetRoom(aRoom).GetSource(aSource);
                iSource.TransportStateChanged += SourceTransportStateChanged;
                iSource.MediaTimeChanged += SourceMediaTimeChanged;
                Updated = true;
            }

            public void Unsubscribe()
            {
                iSource.TransportStateChanged -= SourceTransportStateChanged;
                iSource.MediaTimeChanged -= SourceMediaTimeChanged;
            }

            public ETransportState TransportState()
            {
                return iSource.TransportState();
            }

            public uint Duration()
            {
                return iSource.Duration();
            }

            public uint Seconds()
            {
                return iSource.Seconds();
            }

            public virtual bool Connected()
            {
                return true;
            }
            private void SourceTransportStateChanged(object sender, EventArgs args)
            {
                Updated = true;
            }
            private void SourceMediaTimeChanged(object sender, EventArgs args)
            {
                Updated = true;
            }
            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }

            #endregion
        }
        class MockControllerKinskyMediaTime : KinskyWidgetBase, IControllerKinskyMediaTime
        {

            #region IControllerKinskyMediaTime Members

            private KinskyWeb.TestStubs.TopologyStub.Source iSource;
            public void Subscribe(string aRoom, string aSource)
            {
                iSource = TopologyStub.House.GetRoom(aRoom).GetSource(aSource);
            }

            public void Unsubscribe()
            {
            }

            public void SetSeconds(uint aSeconds)
            {
                iSource.SetSeconds(aSeconds);
            }
            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }

            #endregion
        }
        class MockControllerDisc : KinskyWidgetBase, IControllerKinskyDisc
        {

            #region IControllerKinskyDisc Members

            public void Subscribe(string aRoom, string aSource)
            {

            }

            public void Unsubscribe()
            {

            }

            public void Eject()
            {

            }

            #endregion

            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }
        }

        class MockViewTrack : KinskyWidgetBase, IViewKinskyTrack
        {

            #region IControllerKinskyDisc Members

            private TopologyStub.Room iRoom;
            private TopologyStub.Source iSource;

            public void Subscribe(string aRoom, string aSource)
            {
                iRoom = TopologyStub.House.GetRoom(aRoom);
                if (iRoom != null)
                {
                    iSource = iRoom.GetSource(aSource);
                    if (iSource != null)
                    {
                        iSource.TrackChanged += TrackChanged;
                    }
                }
                Updated = true;
            }

            public void Unsubscribe()
            {
                if (iSource != null)
                {
                    iSource.TrackChanged -= TrackChanged;
                }
            }

            public void TrackChanged(object sender, EventArgs e)
            {
                Updated = true;
            }

            #endregion

            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }

            #region IViewKinskyTrack Members


            public System.Drawing.Image Artwork()
            {
                return Linn.Kinsky.Properties.Resources.NoAlbumArt;
            }

            public string Title()
            {
                if (iSource != null && iSource.CurrentTrack() != null)
                {
                    return iSource.CurrentTrack().GetUpnpObject().Title;
                }
                else
                {
                    return String.Empty;
                }
            }

            public string Artist()
            {
                return "Artist";
            }

            public string Album()
            {
                return "Album";
            }

            public uint Bitrate()
            {
                return 1411;
            }

            public float SampleRate()
            {
                return 44.1F;
            }

            public uint BitDepth()
            {
                return 24;
            }

            public string Codec()
            {
                return "FLAC";
            }

            public bool Lossless()
            {
                return true;
            }
            public bool Connected()
            {
                return true;
            }
            #endregion
        }



        class MockViewPlayMode : KinskyWidgetBase, IViewKinskyPlayMode
        {
            private TopologyStub.Room iRoom;
            private TopologyStub.Source iSource;
            public void Subscribe(string aRoom, string aSource)
            {
                iRoom = TopologyStub.House.GetRoom(aRoom);
                if (iRoom != null)
                {
                    iSource = iRoom.GetSource(aSource);
                    iSource.PlayModeChanged += PlayModeChanged;
                }
                Updated = true;
            }

            public void Unsubscribe()
            {
                if (iSource != null)
                {
                    iSource.PlayModeChanged -= PlayModeChanged;
                }
            }

            private void PlayModeChanged(object sender, EventArgs args)
            {
                Updated = true;
            }

            public bool Shuffle()
            {
                return iSource.Shuffle();
            }

            public bool Repeat()
            {
                return iSource.Repeat();
            }

            public bool Connected()
            {
                return true;
            }

            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }
        }

        class MockControllerPlayMode : KinskyWidgetBase, IControllerKinskyPlayMode
        {
            private TopologyStub.Room iRoom;
            private TopologyStub.Source iSource;
            public void Subscribe(string aRoom, string aSource)
            {
                iRoom = TopologyStub.House.GetRoom(aRoom);
                if (iRoom != null)
                {
                    iSource = iRoom.GetSource(aSource);
                }
                Updated = true;
            }

            public void Unsubscribe()
            {
            }

            public void ToggleShuffle()
            {
                iSource.ToggleShuffle();
            }

            public void ToggleRepeat()
            {
                iSource.ToggleRepeat();
            }

            public override void OnContainerTerminated()
            {
                Unsubscribe();
            }
        }
    }
}
