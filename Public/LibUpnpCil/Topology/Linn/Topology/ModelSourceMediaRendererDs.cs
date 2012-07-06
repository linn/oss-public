using System;
using System.Collections.Generic;
using Upnp;
using Linn.ControlPoint.Upnp;
using System.Threading;
using Linn.ControlPoint;
using System.Xml;

namespace Linn.Topology
{
    public class ModelSourceMediaRendererDs : ModelSourceMediaRenderer
    {
        internal class IdArrayMetadataCollector
        {
            public IdArrayMetadataCollector(ServicePlaylist aServicePlaylist)
            {
                iIdList = string.Empty;
                iServicePlaylist = aServicePlaylist;
            }

            public void ClearIds()
            {
                iIdList = string.Empty;
                iCount = 0;
            }

            public void AddId(uint aId)
            {
                iIdList += ((iIdList.Length > 0) ? " " : "") + aId.ToString();
                ++iCount;
            }

            public IList<MrItem> Process()
            {
                return Process(false);
            }

            public IList<MrItem> Process(bool aForce)
            {
                if ((iCount + 1) > kCountPerCall || (aForce && iCount > 0))
                {
                    Trace.WriteLine(Trace.kMediaRenderer, "IdArrayMetadataCollector.Process: iCount=" + iCount + ", aForce=" + aForce);
                    string result = iServicePlaylist.ReadListSync(iIdList);

                    ClearIds();

                    return ParseMetadataXml(result);
                }

                return null;
            }

            private IList<MrItem> ParseMetadataXml(string aXml)
            {
                List<MrItem> list = new List<MrItem>();

                if (aXml != null)
                {
                    try
                    {
                        XmlDocument document = new XmlDocument();
                        document.LoadXml(aXml);

                        foreach (XmlNode n in document.SelectNodes("/TrackList/Entry"))
                        {
                            uint id = uint.Parse(n["Id"].InnerText);
                            string uri = n["Uri"].InnerText;
                            string metadata = n["Metadata"].InnerText;
                            DidlLite didl = null;
                            try
                            {
                                didl = new DidlLite(metadata);
                                if (didl.Count == 0)
                                {
                                    UserLog.WriteLine(string.Format("Empty DidlLite created from metadata '{0}'", metadata));
                                    item item = new item();
                                    item.Title = uri;

                                    didl.Add(item);
                                }
                            }
                            catch (XmlException)
                            {
                                didl = new DidlLite();

                                item item = new item();
                                item.Title = uri;

                                didl.Add(item);
                            }
                            list.Add(new MrItem(id, uri, didl));
                        }
                    }
                    catch (XmlException e)
                    {
                        Trace.WriteLine(Trace.kTopology, "IdArrayMetadataCollector.ParseMetadataXml: " + e.Message);
                    }
                    catch (FormatException) { }
                }

                return list;
            }

            private const uint kCountPerCall = 100;

            private string iIdList;
            private uint iCount;
            private ServicePlaylist iServicePlaylist;
        }

        public ModelSourceMediaRendererDs(Source aSource)
        {
            DidlLite didl = new DidlLite();
            audioItem itemAudio = new audioItem();
            itemAudio.Title = "Unknown";
            didl.Add(itemAudio);
            kUnknownPlaylistItem = new MrItem(0, null, didl);

            iSource = aSource;
            iMutex = new Mutex(false);
            iIds = new List<uint>();
            iCacheMetadata = new Dictionary<uint, MrItem>();
            iCacheUsage = new List<uint>();
            iEventIdArray = new ManualResetEvent(false);
            iInserting = false;

            try
            {
                iServicePlaylist = new ServicePlaylist(iSource.Device, iSource.House.EventServer);
            }
            catch (ServiceException)
            {
                throw new ModelSourceException(301, "Service failure");
            }

            iActionPlay = iServicePlaylist.CreateAsyncActionPlay();
            iActionPause = iServicePlaylist.CreateAsyncActionPause();
            iActionStop = iServicePlaylist.CreateAsyncActionStop();
            iActionNext = iServicePlaylist.CreateAsyncActionNext();
            iActionPrevious = iServicePlaylist.CreateAsyncActionPrevious();
            iActionSeekSecondAbsolute = iServicePlaylist.CreateAsyncActionSeekSecondAbsolute();
            iActionSeekIndex = iServicePlaylist.CreateAsyncActionSeekIndex();
            iActionDelete = iServicePlaylist.CreateAsyncActionDeleteId();
            iActionDeleteAll = iServicePlaylist.CreateAsyncActionDeleteAll();
            iActionSetRepeat = iServicePlaylist.CreateAsyncActionSetRepeat();
            iActionSetShuffle = iServicePlaylist.CreateAsyncActionSetShuffle();

            iMetadataCollector = new IdArrayMetadataCollector(iServicePlaylist);
        }

        public override resource BestSupportedResource(upnpObject aObject)
        {
            return BestSupportedResource(iServicePlaylist.ProtocolInfo, aObject);
        }

        public override void Open()
        {
            Assert.Check(iThreadIdArray == null);

            iTrackIndex = -1;
            iTrackPlaylistItem = null;
            iInserting = false;

            iAbortThread = false;
            iRecollectMetadata = false;
            iThreadIdArray = new Thread(ProcessEventIdArray);
            iThreadIdArray.Name = "ProcessEventIdArray{" + iSource.ToString() + "}";
            iThreadIdArray.IsBackground = true;
            iThreadIdArray.Start();

            iServicePlaylist.EventStateTransportState += EventStateTransportStateResponse;
            iServicePlaylist.EventStateId += EventStateTrackIdResponse;
            iServicePlaylist.EventStateIdArray += EventStateIdArrayResponse;
            iServicePlaylist.EventStateRepeat += EventStateRepeatResponse;
            iServicePlaylist.EventStateShuffle += EventStateShuffleResponse;
            iServicePlaylist.EventSubscriptionError += EventSubscriptionErrorHandler;
            iServicePlaylist.EventInitial += EventInitialResponsePlaylist;
        }

        public override void Close()
        {
            iServicePlaylist.EventStateTransportState -= EventStateTransportStateResponse;
            iServicePlaylist.EventStateId -= EventStateTrackIdResponse;
            iServicePlaylist.EventStateIdArray -= EventStateIdArrayResponse;
            iServicePlaylist.EventStateRepeat -= EventStateRepeatResponse;
            iServicePlaylist.EventStateShuffle -= EventStateShuffleResponse;
            iServicePlaylist.EventSubscriptionError -= EventSubscriptionErrorHandler;
            iServicePlaylist.EventInitial -= EventInitialResponsePlaylist;

            try
            {
                Lock();
                iAbortThread = true;
                iEventIdArray.Set();
            }
            finally
            {
                Unlock();
            }
            iThreadIdArray.Join();
            iThreadIdArray = null;
        }

        public override Source Source
        {
            get
            {
                return iSource;
            }
        }

        public override string Name
        {
            get
            {
                return (iSource.FullName);
            }
        }

        public void EventInitialResponsePlaylist(object sender, EventArgs e)
        {
            if (EventPlaylistInitialised != null)
            {
                EventPlaylistInitialised(this, EventArgs.Empty);
            }

            if (EventControlInitialised != null)
            {
                EventControlInitialised(this, EventArgs.Empty);
            }
        }

        public override event EventHandler<EventArgs> EventControlInitialised;
        public override event EventHandler<EventArgs> EventPlaylistInitialised;

        public override event EventHandler<EventArgs> EventTransportStateChanged;
        public override event EventHandler<EventArgs> EventTrackChanged;
        public override event EventHandler<EventArgs> EventRepeatChanged;
        public override event EventHandler<EventArgs> EventShuffleChanged;

        public override event EventHandler<EventArgs> EventPlaylistChanged;

        public override void Play()
        {
            iActionPlay.PlayBegin();
        }

        public override void Pause()
        {
            iActionPause.PauseBegin();
        }

        public override void Stop()
        {
            iActionStop.StopBegin();
        }

        public override void Previous()
        {
            iActionPrevious.PreviousBegin();
        }

        public override void Next()
        {
            iActionNext.NextBegin();
        }

        public override void SeekSeconds(uint aSeconds)
        {
            iActionSeekSecondAbsolute.SeekSecondAbsoluteBegin(aSeconds);
        }

        public override void SeekTrack(uint aTrack)
        {
            iActionSeekIndex.SeekIndexBegin(aTrack);
        }

        public override void ToggleRepeat()
        {
            iActionSetRepeat.SetRepeatBegin(!Repeat);
        }

        public override void ToggleShuffle()
        {
            iActionSetShuffle.SetShuffleBegin(!Shuffle);
        }

        public override ETransportState TransportState
        {
            get
            {
                return iTransportState;
            }
        }

        public override int TrackIndex
        {
            get
            {
                return iTrackIndex;
            }
        }

        public override MrItem TrackPlaylistItem
        {
            get
            {
                return iTrackPlaylistItem;
            }
        }

        public override bool Repeat
        {
            get
            {
                return iServicePlaylist.Repeat;
            }
        }

        public override bool Shuffle
        {
            get
            {
                return iServicePlaylist.Shuffle;
            }
        }

        public override string ProtocolInfo
        {
            get
            {
                return iServicePlaylist.ProtocolInfo;
            }
        }

        public override void Lock()
        {
            iMutex.WaitOne();
        }

        public override void Unlock()
        {
            iMutex.ReleaseMutex();
        }

        public override uint PlayNow(DidlLite aDidlLite)
        {
            uint id = 0;
            try
            {
                Lock();

                if (iIds.Count > 0)
                {
                    id = iIds[iIds.Count - 1];
                }
            }
            finally
            {
                Unlock();
            }

            return PlaylistInsert(id, aDidlLite, true);
        }

        public override uint PlayNext(DidlLite aDidlLite)
        {
            uint id = 0;
            try
            {
                Lock();
                if (iTrackIndex != -1)
                {
                    id = iIds[iTrackIndex];
                }
            }
            finally
            {
                Unlock();
            }

            return PlaylistInsert(id, aDidlLite);
        }

        public override uint PlayLater(DidlLite aDidlLite)
        {
            uint id = 0;
            try
            {
                Lock();
                if (iIds.Count > 0)
                {
                    id = iIds[iIds.Count - 1];
                }
            }
            finally
            {
                Unlock();
            }

            return PlaylistInsert(id, aDidlLite);
        }

        public override MrItem PlaylistItem(uint aIndex)
        {
            MrItem item;
            try
            {
                Lock();

                uint id = iIds[(int)aIndex];

                if (!iCacheMetadata.TryGetValue(id, out item))
                {
                    item = kUnknownPlaylistItem;
                }
            }
            finally
            {
                Unlock();
            }

            return item;
        }

        public override void PlaylistMove(uint aInsertAfterId, IList<MrItem> aPlaylistItems)
        {
            Assert.Check(aPlaylistItems.Count > 0);

            try
            {
                uint afterId = aInsertAfterId;
                if (afterId == aPlaylistItems[0].Id)
                {
                    Lock();

                    int index = iIds.IndexOf(afterId);

                    Unlock();

                    if (index > 0)
                    {
                        afterId = iIds[index - 1];
                    }
                    else
                    {
                        afterId = 0;
                    }
                }
                foreach (MrItem i in aPlaylistItems)
                {
                    try
                    {
                        afterId = iServicePlaylist.InsertSync(afterId, i.Uri, i.DidlLite.Xml);
                        iServicePlaylist.DeleteIdSync(i.Id);
                    }
                    catch (ServiceException e)
                    {
                        if (e.Code == 801)   // playlist full
                        {
                            iServicePlaylist.DeleteIdSync(i.Id);
                            afterId = iServicePlaylist.InsertSync(afterId, i.Uri, i.DidlLite.Xml);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }
            }
            catch (ServiceException)
            {
                // Handle service exceptions here - these are silently ignored. Basically,
                // if one of these is fired, the likely cause is that the playlist on the DS
                // is out of sync with that displayed in the UI. This can be because another
                // control point is manipulating the list and also because there is some problem
                // with the control point keeping in sync (not receiving events from the DS, for example).
                // There is no real best course of action here - eventually, the eventing should make
                // sure everything is back in sync but, in short of making all playlist manipulations
                // atomic, there is nothing that can be done and, given that these should be exceptional
                // circumstances, doing nothing is ok
            }
        }

        public override uint PlaylistInsert(uint aInsertAfterId, DidlLite aDidlLite)
        {
            return PlaylistInsert(aInsertAfterId, aDidlLite, false);
        }

        private uint PlaylistInsert(uint aInsertAfterId, DidlLite aDidlLite, bool aStartPlaying)
        {
            uint count = 0;
            bool locked = false;
            try
            {
                Lock();
                locked = true;
                if (!iInserting)
                {
                    iInserting = true;

                    Trace.WriteLine(Trace.kMediaRenderer, "ModelSourceMediaRendererDs.PlaylistInsert: aInsertAfterId=" + aInsertAfterId + ", iPlaylistIds.Count=" + iIds.Count);

                    uint id = aInsertAfterId;
                    uint startIndex = 0;
                    uint index = 0;
                    if (aInsertAfterId > 0)
                    {
                        index = (uint)(iIds.IndexOf(aInsertAfterId) + 1);
                    }

                    Unlock();
                    locked = false;

                    bool error = false;
                    for (int i = 0; i < aDidlLite.Count; ++i)
                    {
                        try
                        {
                            upnpObject item = aDidlLite[i];
                            resource resource = BestSupportedResource(item);
                            if (resource != null)
                            {
                                string uri = resource.Uri;
                                DidlLite didl = new DidlLite();
                                didl.Add(item);

                                uint newId = iServicePlaylist.InsertSync(id, uri, didl.Xml);
                                ++count;

                                if (i == 0)
                                {
                                    if (aStartPlaying)
                                    {
                                        Trace.WriteLine(Trace.kMediaRenderer, "ModelSourceMediaRendererDs.PlaylistInsert: SeekTrack " + index);
                                        SeekTrack(index);
                                    }
                                }

                                Lock();
                                locked = true;

                                if (startIndex + i <= iIds.Count)
                                {
                                    iIds.Insert((int)(startIndex + index), newId);
                                }

                                if (!iCacheMetadata.ContainsKey(newId))
                                {
                                    iCacheMetadata.Add(newId, new MrItem(newId, uri, didl));

                                    RemoveStaleCacheItems();

                                    iCacheUsage.Remove(newId);
                                    iCacheUsage.Add(newId);
                                }

                                Unlock();
                                locked = false;

                                id = newId;
                                ++index;
                                error = false;
                            }
                        }
                        catch (System.IO.IOException e)
                        {
                            if (error)
                            {
                                UserLog.WriteLine("Insert failed (" + e.Message + ")");
                                break;
                            }
                            error = true;
                        }
                        catch (ServiceException e)
                        {
                            UserLog.WriteLine("Insert failed (" + e.Message + ")");
                            break;
                        }
                    }

                    Lock();
                    locked = true;
                    iInserting = false;
                    Unlock();
                    locked = false;

                    Trace.WriteLine(Trace.kMediaRenderer, "ModelSourceMediaRendererDs.PlaylistInsert: insert finished");
                }
                else
                {
                    Unlock();
                    locked = false;
                }
            }
            finally
            {
                if (locked)
                {
                    Unlock();
                }
                iInserting = false;
            }

            return count;
        }

        public override void PlaylistDelete(IList<MrItem> aPlaylistItems)
        {
            foreach (MrItem i in aPlaylistItems)
            {
                if (i != kUnknownPlaylistItem)
                {
                    iActionDelete.DeleteIdBegin(i.Id);
                }
            }
        }

        public override void PlaylistDeleteAll()
        {
            iActionDeleteAll.DeleteAllBegin();
        }

        public override bool IsInserting()
        {
            bool result;
            try
            {
                Lock();
                result = iInserting;
            }
            finally
            {
                Unlock();
            }
            return result;
        }

        // You should use Lock() before calling this method
        public override uint PlaylistTrackCount
        {
            get
            {
                return (uint)iIds.Count;
            }
        }

        public override uint PlaylistTracksMax
        {
            get
            {
                return iServicePlaylist.TracksMax;
            }
        }

        private void EventStateTransportStateResponse(object sender, EventArgs e)
        {
            string transportState = iServicePlaylist.TransportState;
            if (transportState == "Playing")
            {
                iTransportState = ETransportState.ePlaying;
            }
            else if (transportState == "Paused")
            {
                iTransportState = ETransportState.ePaused;
            }
            else if (transportState == "Stopped")
            {
                iTransportState = ETransportState.eStopped;
            }
            else if (transportState == "Buffering")
            {
                iTransportState = ETransportState.eBuffering;
            }
            else
            {
                Assert.CheckDebug(false);
                iTransportState = ETransportState.eUnknown;
            }

            if (EventTransportStateChanged != null)
            {
                EventTransportStateChanged(this, EventArgs.Empty);
            }
        }

        private class FindTrackIndexPredicate
        {
            public FindTrackIndexPredicate(uint aTrackId)
            {
                iTrackId = aTrackId;
            }

            public bool FindTrackIndex(uint aId)
            {
                return aId == iTrackId;
            }

            private uint iTrackId;
        }

        private void EventStateTrackIdResponse(object sender, EventArgs e)
        {
            UpdateTrack();
        }

        private void UpdateTrack()
        {
            bool locked = false;
            try
            {
                Lock();
                locked = true;

                uint trackId = iServicePlaylist.Id;
                iTrackIndex = iIds.FindIndex(new FindTrackIndexPredicate(trackId).FindTrackIndex);
                MrItem item;
                if (!iCacheMetadata.TryGetValue(trackId, out item))
                {
                    item = null;
                }

                if (iTrackPlaylistItem == null || item != iTrackPlaylistItem)
                {
                    Trace.WriteLine(Trace.kMediaRenderer, "ModelSourceMediaRendererDs.UpdateTrack: iTrackIndex=" + iTrackIndex + ", iPlaylistIds.Count=" + iIds.Count + ", trackId=" + trackId);
                    iTrackPlaylistItem = item;

                    Unlock();
                    locked = false;

                    if (EventTrackChanged != null)
                    {
                        EventTrackChanged(this, EventArgs.Empty);
                    }
                }
                else
                {
                    Unlock();
                    locked = false;
                }
            }
            finally
            {
                if (locked)
                {
                    Unlock();
                }
            }
        }

        private void EventStateIdArrayResponse(object sender, EventArgs e)
        {
            try
            {
                Lock();

                iRecollectMetadata = true;
                iEventIdArray.Set();
            }
            finally
            {
                Unlock();
            }
        }

        private void ProcessEventIdArray()
        {
            while (!iAbortThread)
            {
                iEventIdArray.WaitOne();
                bool locked = false;
                try
                {
                    Lock();
                    locked = true;

                    iRecollectMetadata = false;

                    Unlock();
                    locked = false;

                    if (!iAbortThread)
                    {
                        UpdateMetadata();
                    }

                    Lock();
                    locked = true;

                    if (!iRecollectMetadata)
                    {
                        iEventIdArray.Reset();
                    }

                    Unlock();
                    locked = false;

                    // signal track index update
                    //EventStateTrackIdResponse(this, EventArgs.Empty);
                }
                catch (ServiceException e) // do nothing with exception, topology will clean this source up
                {
                    Trace.WriteLine(Trace.kMediaRenderer, "ModelSourceMediaRendererDs.ProcessEventIdArray: " + e.Message);
                }
                finally
                {
                    if (locked)
                    {
                        Unlock();
                    }
                }
            }

            Trace.WriteLine(Trace.kMediaRenderer, "ModelSourceMediaRendererDs.ProcessEventIdArray: Aborted");
        }

        private void UpdateMetadata()
        {
            byte[] idArray = iServicePlaylist.IdArray;

            iMetadataCollector.ClearIds();

            IList<MrItem> list;
            List<uint> playlistIds = new List<uint>();
            int j = 0;
            // unpack id array into list of ids
            for (int i = 0; i < idArray.Length; i += 4, ++j)
            {
                if (iRecollectMetadata || iAbortThread)
                {
                    return;
                }

                uint value = Linn.BigEndianConverter.BigEndianToUint32(idArray, i);

                playlistIds.Add(value);

                ItemById(value);

                try
                {
                    list = iMetadataCollector.Process();
                    if (list != null)
                    {
                        UpdateCache(list);
                    }
                }
                catch (System.Net.WebException) { }

                Trace.WriteLine(Trace.kMediaRenderer, "ModelSourceMediaRendererDs.UpdateMetadata: idArray[" + j + "]=" + value);
            }

            try
            {
                list = iMetadataCollector.Process(true);
                if (list != null)
                {
                    UpdateCache(list);
                }
            }
            catch (System.Net.WebException) { }

            try
            {
                Lock();

                iIds = playlistIds;
                UpdateTrack();
            }
            finally
            {
                Unlock();
            }

            if (iRecollectMetadata || iAbortThread)
            {
                return;
            }

            if (EventPlaylistChanged != null)
            {
                EventPlaylistChanged(this, EventArgs.Empty);
            }

            if (EventTrackChanged != null)
            {
                EventTrackChanged(this, EventArgs.Empty);
            }
        }

        private MrItem ItemById(uint aId)
        {
            try
            {
                Lock();

                MrItem value;
                if (iCacheMetadata.TryGetValue(aId, out value))
                {
                    Trace.WriteLine(Trace.kMediaRenderer, "ModelSourceMediaRendererDs.ItemById: Reading " + aId + " from cache");
                    return value;
                }
                else
                {
                    iMetadataCollector.AddId(aId);
                    return null;
                }
            }
            finally
            {
                Unlock();
            }

            /*Trace.WriteLine(Trace.kMediaRenderer, "ModelSourceMediaRendererDs.ItemById: Reading " + aId + " from DS");

            string uri;
            string metadata;
            iServicePlaylist.ReadSync(aId, out uri, out metadata);

            try
            {
                value = new Pair<string, DidlLite>(uri, new DidlLite(metadata));
            }
            catch (XmlException e)
            {
                Console.WriteLine(e.Message);
                DidlLite didl = new DidlLite();
                
                item item = new item();
                item.Title = uri;
                
                didl.Add(item);

                value = new Pair<string, DidlLite>(uri, didl);
            }

            Lock();

            try
            {
                iCacheMetadata.Add(aId, value);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message + ", aId=" + aId);
            }

            Unlock();

            return value;*/
        }

        private void UpdateCache(IList<MrItem> aPlaylistItems)
        {
            foreach (MrItem item in aPlaylistItems)
            {
                try
                {
                    Lock();

                    try
                    {
                        iCacheMetadata.Add(item.Id, item);
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine(e.Message + ", entry.Id=" + item.Id);
                    }

                    RemoveStaleCacheItems();

                    // refresh metadata usage
                    iCacheUsage.Remove(item.Id);
                    iCacheUsage.Add(item.Id);
                }
                finally
                {
                    Unlock();
                }
            }
        }
        private void RemoveStaleCacheItems()
        {
            // remove stale items from cache
            if (iCacheMetadata.Count > kMaxCacheSize)
            {
                uint idToRemove = iCacheUsage[0];
                iCacheMetadata.Remove(idToRemove);
                iCacheUsage.RemoveAt(0);

                Trace.WriteLine(Trace.kMediaRenderer, "ModelSourceMediaRendererDs.RemoveStaleCacheItems: Removed id " + idToRemove);
            }

            Assert.Check(iCacheUsage.Count <= kMaxCacheSize);
            Assert.Check(iCacheMetadata.Count <= kMaxCacheSize);
        }

        private void EventStateRepeatResponse(object sender, EventArgs e)
        {
            if (EventRepeatChanged != null)
            {
                EventRepeatChanged(this, EventArgs.Empty);
            }
        }

        private void EventStateShuffleResponse(object sender, EventArgs e)
        {
            if (EventShuffleChanged != null)
            {
                EventShuffleChanged(this, EventArgs.Empty);
            }
        }

        private void EventSubscriptionErrorHandler(object sender, EventArgs e)
        {
            OnEventSubscriptionError();
        }

        private readonly MrItem kUnknownPlaylistItem;
        private const uint kMaxCacheSize = 1000;

        private Source iSource;

        private ETransportState iTransportState;
        private MrItem iTrackPlaylistItem;
        private int iTrackIndex;

        private ServicePlaylist iServicePlaylist;
        private ServicePlaylist.AsyncActionDeleteId iActionDelete;
        private ServicePlaylist.AsyncActionDeleteAll iActionDeleteAll;
        private ServicePlaylist.AsyncActionSetRepeat iActionSetRepeat;
        private ServicePlaylist.AsyncActionSetShuffle iActionSetShuffle;
        private ServicePlaylist.AsyncActionPlay iActionPlay;
        private ServicePlaylist.AsyncActionPause iActionPause;
        private ServicePlaylist.AsyncActionStop iActionStop;
        private ServicePlaylist.AsyncActionNext iActionNext;
        private ServicePlaylist.AsyncActionPrevious iActionPrevious;
        private ServicePlaylist.AsyncActionSeekSecondAbsolute iActionSeekSecondAbsolute;
        private ServicePlaylist.AsyncActionSeekIndex iActionSeekIndex;

        private bool iInserting;

        private Mutex iMutex;
        private bool iAbortThread;
        private bool iRecollectMetadata;
        private Thread iThreadIdArray;
        private ManualResetEvent iEventIdArray;

        private IdArrayMetadataCollector iMetadataCollector;
        private List<uint> iIds;
        private Dictionary<uint, MrItem> iCacheMetadata;
        private List<uint> iCacheUsage;
    }

} // Linn.Topology
