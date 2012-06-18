using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Linn.Kinsky;
using KinskyWeb.Kinsky;
using Linn;
using Timer = System.Timers.Timer;

namespace KinskyWeb.TestStubs
{
    public class TopologyStub
    {
        public static class House
        {
            static public event EventHandler<EventArgs> RoomsChanged;
            static private Dictionary<string, Room> iRooms;
            static public Room GetRoom(string aName)
            {
                lock (iRooms)
                {
                    if (iRooms.ContainsKey(aName))
                    {
                        return iRooms[aName];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            static public Room[] GetRooms()
            {
                lock (iRooms)
                {
                    return iRooms.Values.ToArray<Room>();
                }
            }
            static Timer poltergeist;

            static House()
            {
                iRooms = new Dictionary<string, Room>();
                AddRoom("Lounge");
                GetRoom("Lounge").AddSource(new Source("Playlist", ESourceType.ePlaylist));
                GetRoom("Lounge").AddSource(new Source("Radio", ESourceType.eRadio));
                GetRoom("Lounge").AddSource(new Source("Aux", ESourceType.eRadio));
                GetRoom("Lounge").AddSource(new Source("CD", ESourceType.eDisc));
                AddRoom("Kitchen");
                GetRoom("Kitchen").AddSource(new Source("Playlist", ESourceType.ePlaylist));
                GetRoom("Kitchen").AddSource(new Source("Radio", ESourceType.eRadio));
                AddRoom("Master Bedroom");
                GetRoom("Master Bedroom").AddSource(new Source("Playlist", ESourceType.ePlaylist));
                GetRoom("Master Bedroom").AddSource(new Source("Radio", ESourceType.eRadio));
                GetRoom("Lounge").AddSource(new Source("CD", ESourceType.eDisc));
                AddRoom("Spare Bedroom");
                GetRoom("Spare Bedroom").AddSource(new Source("Playlist", ESourceType.ePlaylist));
                GetRoom("Spare Bedroom").AddSource(new Source("Radio", ESourceType.eRadio));
                poltergeist = new Timer(1000);
                poltergeist.Elapsed += (sender, e) =>
                {
                    Random rnd = new Random();
                    int rndInt = rnd.Next(10);
                    // 1 in 10 chance of doing something
                    if (rndInt % 10 == 0)
                    {
                        rndInt = rnd.Next(iRooms.Keys.Count);
                        Room haunted = GetRoom(iRooms.Keys.Skip(rndInt).Take(1).Single());
                        rndInt = rnd.Next(2);
                        if (rndInt % 2 == 0)
                        {
                            haunted.Mute = !haunted.Mute;
                            Trace.WriteLine(Trace.kKinskyWeb, String.Format("The ghost hit the mute/unmute button in {0}.", haunted.Name));
                        }
                        else
                        {
                            haunted.Volume = (uint)rnd.Next((int)haunted.VolumeLimit);
                            Trace.WriteLine(Trace.kKinskyWeb, String.Format("The ghost set {0} volume to {1}.", haunted.Name, haunted.Volume));
                        }
                    }
                };
                poltergeist.Start();
            }
            static private void AddRoom(string aName)
            {
                lock (iRooms)
                {
                    iRooms.Add(aName, new Room(aName));
                }
            }
        }

        public class Room
        {

            public event EventHandler<EventArgs> VolumeChanged;
            public event EventHandler<EventArgs> MuteChanged;
            public event EventHandler<EventArgs> SourceChanged;

            private Dictionary<string, Source> iSources;

            private string iCurrentSourceName;
            private Object iLockObject;

            public Room(string aName)
            {
                iLockObject = new Object();
                Random rndControls = new Random();
                int rnd = rndControls.Next(2);
                this.Mute = (rnd % 2 == 0);
                this.VolumeLimit = (uint)rndControls.Next(100);
                this.Volume = (uint)rndControls.Next((int)VolumeLimit);
                this.Name = aName;
                this.iSources = new Dictionary<string, Source>();
            }

            public Source CurrentSource()
            {
                lock (iLockObject)
                {
                    if (iCurrentSourceName != null)
                    {
                        return iSources[iCurrentSourceName];
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public void SetCurrentSource(string aSource)
            {
                lock (iLockObject)
                {
                    iCurrentSourceName = aSource;
                    if (SourceChanged != null)
                    {
                        SourceChanged(this, new EventArgs());
                    }
                }
            }

            public void AddSource(Source aSource)
            {
                lock (iLockObject)
                {
                    if (!iSources.ContainsKey(aSource.Name))
                    {
                        iSources.Add(aSource.Name, aSource);
                        if (iCurrentSourceName == null)
                        {
                            iCurrentSourceName = aSource.Name;
                        }
                        if (SourceChanged != null)
                        {
                            SourceChanged(this, new EventArgs());
                        }
                    }
                }
            }

            public Source GetSource(string aSourceName)
            {
                lock (iLockObject)
                {
                    return iSources[aSourceName];
                }
            }

            public Source[] GetSources()
            {
                lock (iLockObject)
                {
                    return iSources.Values.ToArray();
                }
            }

            private string iName;
            public string Name
            {
                get
                {
                    lock (iLockObject)
                    {
                        return iName;

                    }
                }
                private set
                {
                    lock (iLockObject)
                    {
                        iName = value;
                    }
                }
            }
            private bool iMute;
            public bool Mute
            {
                get
                {
                    lock (iLockObject)
                    {
                        return iMute;
                    }
                }
                set
                {
                    lock (iLockObject)
                    {
                        iMute = value;
                        OnMuteChanged();
                    }
                }
            }

            private uint iVolume;
            public uint Volume
            {
                get
                {
                    lock (iLockObject)
                    {
                        return iVolume;
                    }
                }
                set
                {
                    iVolume = value;
                    OnVolumeChanged();
                }
            }
            public uint VolumeLimit { get; private set; }

            private void OnVolumeChanged()
            {
                if (VolumeChanged != null)
                {
                    VolumeChanged(this, new EventArgs());
                }
            }
            private void OnMuteChanged()
            {
                if (MuteChanged != null)
                {
                    MuteChanged(this, new EventArgs());
                }
            }
        }

        public class Source
        {
            public event EventHandler<EventArgs> TransportStateChanged;
            public event EventHandler<EventArgs> MediaTimeChanged;
            public event EventHandler<EventArgs> PlaylistChanged;
            public event EventHandler<EventArgs> TrackChanged;
            public event EventHandler<EventArgs> PlayModeChanged;
            public string Name { get; private set; }
            public ESourceType Type { get; private set; }
            private List<Track> iPlayList;
            private Dictionary<string, int> iPlayListCache;
            private int iCurrentTrackIndex;
            private Timer iMediaTimer;
            private Object iLockObject;
            private bool iShuffle;
            private bool iRepeat;

            private ETransportState iTransportState;
            public ETransportState TransportState()
            {
                return iTransportState;
            }

            private void OnTransportStateChanged()
            {
                if (TransportStateChanged != null)
                {
                    TransportStateChanged(this, new EventArgs());
                }
            }
            private void OnMediaTimeChanged()
            {
                if (MediaTimeChanged != null)
                {
                    MediaTimeChanged(this, new EventArgs());
                }
            }
            private void OnPlaylistChanged()
            {
                if (PlaylistChanged != null)
                {
                    PlaylistChanged(this, new EventArgs());
                }
            }
            private void OnTrackChanged()
            {
                if (TrackChanged != null)
                {
                    TrackChanged(this, new EventArgs());
                }
            }
            private void OnPlayModeChanged()
            {
                if (PlayModeChanged != null)
                {
                    PlayModeChanged(this, new EventArgs());
                }
            }

            public void Pause()
            {

                lock (iLockObject)
                {
                    iTransportState = ETransportState.ePaused;
                    OnTransportStateChanged();
                }
            }
            public void Play()
            {
                lock (iLockObject)
                {
                    iTransportState = ETransportState.ePlaying;
                    OnTransportStateChanged();
                }
            }

            public void Stop()
            {
                lock (iLockObject)
                {
                    iTransportState = ETransportState.eStopped;
                    OnTransportStateChanged();
                }
            }
            public void Previous()
            {
                lock (iLockObject)
                {
                    if (iPlayList.Count > 0)
                    {
                        Track current = CurrentTrack();
                        if (current != null)
                        {
                            current.Seconds = 0;
                        }
                        if (iShuffle)
                        {
                            Random r = new Random();
                            iCurrentTrackIndex = r.Next(0, iPlayList.Count);
                        }
                        else
                        {
                            if (iCurrentTrackIndex <= 0)
                            {
                                if (iRepeat)
                                {
                                    iCurrentTrackIndex = iPlayList.Count - 1;
                                }
                            }
                            else
                            {
                                iCurrentTrackIndex -= 1;
                            }
                        }
                        current = CurrentTrack();
                        if (current != null)
                        {
                            current.Seconds = 0;
                            OnMediaTimeChanged();
                        }
                    }
                    else
                    {
                        iCurrentTrackIndex = -1;
                    }
                    OnTrackChanged();
                }

            }
            public void Next()
            {
                lock (iLockObject)
                {
                    if (iPlayList.Count > 0)
                    {
                        Track current = CurrentTrack();
                        if (current != null)
                        {
                            current.Seconds = 0;
                        }
                        if (iShuffle)
                        {
                            Random r = new Random();
                            iCurrentTrackIndex = r.Next(0, iPlayList.Count);
                        }
                        else
                        {
                            if (iCurrentTrackIndex < iPlayList.Count - 1)
                            {
                                iCurrentTrackIndex += 1;
                            }
                            else
                            {
                                if (iRepeat)
                                {
                                    iCurrentTrackIndex = 0;
                                }
                                else
                                {
                                    iCurrentTrackIndex = -1;
                                    this.iTransportState = ETransportState.eStopped;
                                    OnTransportStateChanged();
                                }
                            }
                        }
                        current = CurrentTrack();
                        if (current != null)
                        {
                            current.Seconds = 0;
                            OnMediaTimeChanged();
                        }
                    }
                    else
                    {
                        iCurrentTrackIndex = -1;
                    }
                    OnTrackChanged();
                }
            }

            public uint Seconds()
            {
                lock (iLockObject)
                {
                    if (iCurrentTrackIndex != -1)
                    {
                        return iPlayList[iCurrentTrackIndex].Seconds;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public uint Duration()
            {
                lock (iLockObject)
                {
                    if (iCurrentTrackIndex != -1)
                    {
                        return iPlayList[iCurrentTrackIndex].Duration;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public void SetSeconds(uint aSeconds)
            {
                lock (iLockObject)
                {
                    if (iCurrentTrackIndex != -1)
                    {
                        Track current = iPlayList[iCurrentTrackIndex];
                        current.Seconds = (aSeconds > current.Duration) ? current.Duration : aSeconds;
                    }
                }
            }

            public void PlayNow()
            {
            }
            public void PlayNext()
            {
            }
            public void PlayLater()
            {
            }

            public bool ContainsTrack(string aTrackID)
            {
                return iPlayListCache.ContainsKey(aTrackID);
            }

            public void SetCurrentTrack(string aTrackID)
            {
                if (iPlayListCache.ContainsKey(aTrackID))
                {
                    var prevTrackIndex = iCurrentTrackIndex;
                    iCurrentTrackIndex = iPlayListCache[aTrackID];
                    if (prevTrackIndex != iCurrentTrackIndex)
                    {
                        OnTrackChanged();
                    }
                }
            }

            public Track GetTrack(string aTrackID)
            {
                return iPlayList[iPlayListCache[aTrackID]];
            }

            public int TrackIndex(string aTrackID)
            {
                return iPlayListCache[aTrackID];
            }

            public Source(string aName, ESourceType aSourceType)
            {
                iLockObject = new Object();
                Name = aName;
                iPlayList = new List<Track>();
                iPlayListCache = new Dictionary<string, int>();

                Random rnd = new Random();
                int rndInt = rnd.Next(1, 10);
                for (var i = 0; i < rndInt; i++)
                {
                    Track t = new Track();
                    t.Name = "Track " + i;
                    Random rnd2 = new Random();
                    t.Duration = (uint)rnd2.Next(300);
                    t.Seconds = 0;
                    this.iPlayList.Add(t);
                }
                RebuildPlaylistCache();
                iCurrentTrackIndex = rnd.Next(rndInt);
                iTransportState = (ETransportState)rnd.Next(Enum.GetNames(typeof(ETransportState)).Length);
                Type = aSourceType;
                iMediaTimer = new Timer(1000);
                iMediaTimer.Elapsed += new ElapsedEventHandler(iMediaTimer_Elapsed);
                iMediaTimer.Start();
                OnTransportStateChanged();
                iShuffle = false;
                iRepeat = false;
            }

            public int Count()
            {
                lock (iLockObject)
                {
                    return iPlayList.Count();
                }
            }

            public void Move(Track[] aTracks, int aPosition)
            {
                lock (iLockObject)
                {
                    int insertPosition = aPosition;
                    foreach (Track track in aTracks)
                    {
                        int position = iPlayList.IndexOf(track);
                        if (position < insertPosition)
                        {
                            insertPosition--;
                        }
                        iPlayList.Remove(track);
                    }
                    foreach (Track track in aTracks)
                    {
                        iPlayList.Insert(insertPosition++, track);
                    }
                    RebuildPlaylistCache();
                    OnPlaylistChanged();
                }
            }

            private void RebuildPlaylistCache()
            {
                this.iPlayListCache.Clear();
                for (int i = 0; i < iPlayList.Count; i++)
                {
                    this.iPlayListCache[iPlayList[i].ID] = i;
                }
            }

            public void Insert(Upnp.upnpObject[] aTracks, int aPosition)
            {
                lock (iLockObject)
                {
                    int position = aPosition;
                    if (position > iPlayList.Count)
                    {
                        position = iPlayList.Count - 1;
                    }
                    foreach (Upnp.upnpObject item in aTracks)
                    {
                        Track track = new Track();
                        track.Name = item.Title;
                        iPlayList.Insert(position++, track);
                    }

                    RebuildPlaylistCache();
                    OnPlaylistChanged();
                }
            }

            void iMediaTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                lock (iLockObject)
                {
                    if (iTransportState == ETransportState.ePlaying)
                    {
                        if (iCurrentTrackIndex != -1)
                        {
                            Track current = iPlayList[iCurrentTrackIndex];
                            if (current.Seconds + 1 < current.Duration)
                            {
                                current.Seconds++;
                            }
                            else
                            {
                                Next();
                            }
                        }
                        else
                        {
                            Next();
                        }
                    }
                }
            }

            public Upnp.upnpObject[] Playlist()
            {
                return (from p in iPlayList select p.GetUpnpObject()).ToArray();
            }

            public Track CurrentTrack()
            {
                if (iCurrentTrackIndex != -1)
                {
                    return iPlayList[iCurrentTrackIndex];
                }
                else
                {
                    return null;
                }
            }

            public void ToggleShuffle()
            {
                lock (iLockObject)
                {
                    iShuffle = !iShuffle;
                    OnPlayModeChanged();
                }
            }
            public void ToggleRepeat()
            {
                lock (iLockObject)
                {
                    iRepeat = !iRepeat;
                    OnPlayModeChanged();
                }
            }
            public bool Repeat()
            {
                lock (iLockObject)
                {
                    return iRepeat;
                }
            }
            public bool Shuffle()
            {
                lock (iLockObject)
                {
                    return iShuffle;
                }
            }

            public void Delete(Track aTrack)
            {
                lock (iLockObject)
                {
                    if (iPlayListCache.ContainsKey(aTrack.ID))
                    {
                        int index = iPlayListCache[aTrack.ID];
                        iPlayList.RemoveAt(index);
                        if (iCurrentTrackIndex == index)
                        {
                            iCurrentTrackIndex = -1;
                        }
                        iPlayListCache.Remove(aTrack.ID);
                        RebuildPlaylistCache();
                        OnPlaylistChanged();
                    }
                }
            }
            public void DeleteAll()
            {
                lock (iLockObject)
                {
                    iPlayList.Clear();
                    iPlayListCache.Clear();
                    iCurrentTrackIndex = -1;
                    RebuildPlaylistCache();
                    OnPlaylistChanged();
                }
            }
        }

        public class Track
        {
            public Track()
            {
                ID = Guid.NewGuid().ToString();
            }
            public uint Duration { get; set; }
            public uint Seconds { get; set; }
            public string Name { get; set; }
            public string ID { get; set; }
            private Upnp.upnpObject iUpnpObject;
            public Upnp.upnpObject GetUpnpObject()
            {
                Upnp.item itm = new Upnp.item();
                itm.Id = ID;
                itm.Title = Name;
                return itm;
            }
        }

    }
}
