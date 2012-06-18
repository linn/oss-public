using System;
using System.Collections.Generic;
using System.Linq;
using Linn.Kinsky;
using Upnp;
using Linn.Topology;
using KinskyWeb.Services;
using System.Xml.Linq;
using System.Drawing;
using Linn;
using System.Threading;

namespace KinskyWeb.Kinsky
{
    public interface IViewWidgetBrowseableAdaptor
    {
        void Add(IKinskyWidget aItem);
        void Remove(IKinskyWidget aItem);
        void Rescan();
        LocationDTO[] CurrentLocation();
        bool Connected();
        uint ChildCount();
        UPnpObjectDTO[] GetChildren(uint aStartIndex, uint aCount);
        void Activate(UPnpObjectDTO aChild);
        void Up(uint aNumberLevels);
        IContainer CurrentContainer();
        void SetCurrentLocation(string[] aLocation);
        bool ErrorOccured();
    }

    public interface IViewWidgetPlaylistAdaptor
    {
        void Add(IKinskyWidget aItem);
        void Remove(IKinskyWidget aItem);
        void Move(UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem);
        void MoveToEnd(UPnpObjectDTO[] aChildren);
        void Insert(UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem, IContainer aSourceContainer);
        void Delete(UPnpObjectDTO[] aChildren);
        void DeleteAll();
        UPnpObjectDTO CurrentItem();
    }

    #region class ViewWidgetSelectorRoomAdaptor

    public class ViewWidgetSelectorRoomAdaptor : IViewWidgetSelector
    {
        private Dictionary<string, Linn.Topology.Room> iRooms;
        private List<IKinskyWidget> iSubscribers;
        private Object iLockObject = new Object();
        private string iSelectedRoom;

        public ViewWidgetSelectorRoomAdaptor()
        {
            iSubscribers = new List<IKinskyWidget>();
            iRooms = new Dictionary<string, Linn.Topology.Room>();
        }

        public void Add(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Add(aItem);
                aItem.Updated = true;
            }
        }

        public void Remove(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Remove(aItem);
            }
        }

        public void Open()
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorRoomAdaptor:Open");
            lock (iLockObject)
            {
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void Close()
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorRoomAdaptor:Close");
            lock (iLockObject)
            {
                //iRooms.Clear();
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }


        public string[] Rooms()
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorRoomAdaptor:Rooms");
            lock (iLockObject)
            {
                return iRooms.Keys.ToArray();
            }
        }

        #region IViewWidgetSelector Members


        public void Add(string aName, object aTag)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorRoomAdaptor:Add Room " + aName);
            lock (iLockObject)
            {
                if (!iRooms.ContainsKey(aName))
                {
                    iRooms.Add(aName, (Linn.Topology.Room)aTag);
                    foreach (IKinskyWidget i in iSubscribers)
                    {
                        i.Updated = true;
                    }
                }
            }
        }

        public void Remove(object aTag)
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorRoomAdaptor:Remove Room " + aTag);
                foreach (string k in iRooms.Keys.ToArray())
                {
                    if (iRooms[k].Equals(aTag))
                    {
                        iRooms.Remove(k);
                        break;
                    }
                }
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetSelected(object aTag)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorRoomAdaptor:SetSelected " + aTag);
            lock (iLockObject)
            {
                bool updated = false;
                foreach (string k in iRooms.Keys.ToArray())
                {
                    if (iRooms[k].Equals(aTag) && !k.Equals(iSelectedRoom))
                    {
                        iSelectedRoom = k;
                        updated = true;
                        break;
                    }
                }
                if (!updated)
                {
                    Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorRoomAdaptor:SetSelected, room not found");
                    iSelectedRoom = null;
                }
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void OnEventSelectionChanged(string aRoom)
        {
            lock (iLockObject)
            {
                Linn.Topology.Room room = null;
                if (aRoom != null && iRooms.ContainsKey(aRoom))
                {
                    room = iRooms[aRoom];
                }
                Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorRoomAdaptor:OnEventSelectionChanged, room " + room);
                if (EventSelectionChanged != null)
                {
                    EventSelectionChanged(this, new EventArgsSelection(room));
                }
            }
        }

        public string CurrentRoom()
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorRoomAdaptor:CurrentRoom, room " + iSelectedRoom);
            lock (iLockObject)
            {
                return iSelectedRoom;
            }

        }

        public event EventHandler<EventArgsSelection> EventSelectionChanged;

        #endregion
    }

    #endregion
    #region class ViewWidgetSelectorSourceAdaptor

    public class ViewWidgetSelectorSourceAdaptor : IViewWidgetSelector
    {
        private Dictionary<string, Linn.Topology.Source> iSources;
        private List<IKinskyWidget> iSubscribers;
        private Object iLockObject = new Object();
        private string iSelectedSource;

        public ViewWidgetSelectorSourceAdaptor()
        {
            iSubscribers = new List<IKinskyWidget>();
            iSources = new Dictionary<string, Linn.Topology.Source>();
        }

        public void Add(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Add(aItem);
                aItem.Updated = true;
            }
        }

        public void Remove(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Remove(aItem);
            }
        }

        public void Open()
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorSourceAdaptor:Open");
            lock (iLockObject)
            {
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void Close()
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorSourceAdaptor:Close");
            lock (iLockObject)
            {
                iSources.Clear();
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }


        public SourceDTO[] Sources()
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorSourceAdaptor:Sources");
            lock (iLockObject)
            {
                return (from s in iSources.Values.OfType<Linn.Topology.Source>()
                        select new SourceDTO()
                        {
                            Name = s.FullName,
                            Type = (ESourceType)Enum.Parse(typeof(ESourceType), String.Format("e{0}", s.Type), true)
                        }).ToArray();
            }
        }

        #region IViewWidgetSelector Members


        public void Add(string aName, object aTag)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorSourceAdaptor:Add");
            lock (iLockObject)
            {
                if (!iSources.ContainsKey(aName))
                {
                    iSources.Add(aName, aTag as Linn.Topology.Source);
                    foreach (IKinskyWidget i in iSubscribers)
                    {
                        i.Updated = true;
                    }
                }
            }
        }

        public void Remove(object aTag)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorSourceAdaptor:Remove");
            lock (iLockObject)
            {
                foreach (string k in iSources.Keys.ToArray())
                {
                    if (iSources[k].Equals(aTag as Linn.Topology.Source))
                    {
                        iSources.Remove(k);
                        break;
                    }
                }
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetSelected(object aTag)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorSourceAdaptor:SetSelected");
            lock (iLockObject)
            {
                bool updated = false;
                foreach (string k in iSources.Keys.ToArray())
                {
                    if (iSources[k].Equals(aTag) && !k.Equals(iSelectedSource))
                    {
                        iSelectedSource = k;
                        updated = true;
                        break;
                    }
                }
                if (updated)
                {
                    foreach (IKinskyWidget i in iSubscribers)
                    {
                        i.Updated = true;
                    }
                }
            }
        }

        public string CurrentSource()
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorSourceAdaptor:CurrentSource");
            lock (iLockObject)
            {
                return iSelectedSource;
            }
        }

        public ESourceType CurrentSourceType()
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorSourceAdaptor:CurrentSourceType");
            lock (iLockObject)
            {
                ESourceType currentType = ESourceType.eUnknown;
                if (iSelectedSource != null)
                {
                    Linn.Topology.Source source = null;
                    if (iSources.ContainsKey(iSelectedSource))
                    {
                        source = iSources[iSelectedSource];
                    }
                    if (source != null)
                    {
                        try
                        {
                            currentType = (ESourceType)Enum.Parse(typeof(ESourceType), String.Format("e{0}", source.Type), true);
                        }
                        catch { }
                    }
                }
                return currentType;
            }
        }

        public void OnEventSelectionChanged(string aSource)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "ViewWidgetSelectorSourceAdaptor:OnEventSelectionChanged");
            Linn.Topology.Source source = null;
            if (aSource != null && iSources.ContainsKey(aSource))
            {
                source = iSources[aSource];
            }
            if (EventSelectionChanged != null)
            {
                EventSelectionChanged(this, new EventArgsSelection(source));
            }
        }

        public event EventHandler<EventArgsSelection> EventSelectionChanged;

        #endregion
    }

    #endregion
    #region class ViewWidgetVolumeControlAdaptor
    public class ViewWidgetVolumeControlAdaptor : IViewWidgetVolumeControl
    {
        private List<IKinskyWidget> iSubscribers;
        private Object iLockObject = new Object();
        private uint iVolume;
        private uint iVolumeLimit;
        private bool iMute;
        private bool iConnected;

        public ViewWidgetVolumeControlAdaptor()
        {
            iSubscribers = new List<IKinskyWidget>();
            iConnected = false;
        }

        public void Add(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Add(aItem);
                aItem.Updated = true;
            }
        }

        public void Remove(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Remove(aItem);
            }
        }

        public void Open()
        {
            lock (iLockObject)
            {
                iConnected = true;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void Close()
        {
            lock (iLockObject)
            {
                iConnected = false;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public bool Connected()
        {
            lock (iLockObject)
            {
                return iConnected;
            }
        }

        #region IViewWidgetVolumeControl Members


        public void Initialised()
        {
            lock (iLockObject)
            {
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetVolume(uint aVolume)
        {
            lock (iLockObject)
            {
                this.iVolume = aVolume;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetMute(bool aMute)
        {
            lock (iLockObject)
            {
                this.iMute = aMute;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetVolumeLimit(uint aVolumeLimit)
        {
            lock (iLockObject)
            {
                this.iVolumeLimit = aVolumeLimit;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        /* not used by web interfaces */
        public event EventHandler<EventArgs> EventVolumeIncrement;

        /* not used by web interfaces */
        public event EventHandler<EventArgs> EventVolumeDecrement;

        public void OnEventVolumeChanged(uint aVolume)
        {
            lock (iLockObject)
            {
                this.iVolume = aVolume;
                if (EventVolumeChanged != null)
                {
                    EventVolumeChanged(this, new EventArgsVolume(aVolume));
                }
            }
        }

        public event EventHandler<EventArgsVolume> EventVolumeChanged;

        public void OnEventMuteChanged(bool aMute)
        {
            lock (iLockObject)
            {
                this.iMute = aMute;
                if (EventMuteChanged != null)
                {
                    EventMuteChanged(this, new EventArgsMute(aMute));
                }
            }
        }
        public event EventHandler<EventArgsMute> EventMuteChanged;

        public bool Mute()
        {
            lock (iLockObject)
            {
                return iMute;
            }
        }
        public uint Volume()
        {
            lock (iLockObject)
            {
                return iVolume;
            }
        }
        public uint VolumeLimit()
        {
            lock (iLockObject)
            {
                return iVolumeLimit;
            }
        }

        #endregion
    }

    #endregion
    #region class ViewWidgetMediaTimeAdaptor
    public class ViewWidgetMediaTimeAdaptor : IViewWidgetMediaTime
    {
        private List<IKinskyWidget> iSubscribers;
        private Object iLockObject = new Object();
        private ETransportState iTransportState;
        private uint iDuration;
        private uint iSeconds;
        private bool iConnected;

        public ViewWidgetMediaTimeAdaptor()
        {
            iSubscribers = new List<IKinskyWidget>();
            iConnected = false;
        }

        public void Add(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Add(aItem);
                aItem.Updated = true;
            }
        }

        public void Remove(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Remove(aItem);
            }
        }

        public void Open()
        {
            lock (iLockObject)
            {
                iConnected = true;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void Close()
        {
            lock (iLockObject)
            {
                iConnected = false;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }


        public bool Connected()
        {
            lock (iLockObject)
            {
                return iConnected;
            }
        }

        #region IViewWidgetMediaTime Members


        public void Initialised()
        {
            lock (iLockObject)
            {
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetTransportState(ETransportState aTransportState)
        {
            lock (iLockObject)
            {
                iTransportState = aTransportState;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetDuration(uint aDuration)
        {
            iDuration = aDuration;
            foreach (IKinskyWidget i in iSubscribers)
            {
                i.Updated = true;
            }
        }

        public void SetSeconds(uint aSeconds)
        {
            iSeconds = aSeconds;
            /*foreach (IKinskyWidget i in iSubscribers)
            {
                i.Updated = true;
            }*/
        }

        public void OnEventSeekSeconds(uint aSeconds)
        {
            lock (iLockObject)
            {
                this.iSeconds = aSeconds;
                if (EventSeekSeconds != null)
                {
                    EventSeekSeconds(this, new EventArgsSeekSeconds(aSeconds));
                }
            }
        }
        public event EventHandler<EventArgsSeekSeconds> EventSeekSeconds;

        #endregion

        public ETransportState TransportState()
        {
            lock (iLockObject)
            {
                return this.iTransportState;
            }
        }
        public uint Duration()
        {
            lock (iLockObject)
            {
                return this.iDuration;
            }
        }
        public uint Seconds()
        {
            lock (iLockObject)
            {
                return this.iSeconds;
            }
        }
    }
    #endregion

    #region class ViewWidgetTransportControlAdaptor
    public class ViewWidgetTransportControlAdaptor : IViewWidgetTransportControl
    {
        private List<IKinskyWidget> iSubscribers;
        private Object iLockObject = new Object();
        private ETransportState iTransportState;
        private bool iPlayNextEnabled;
        private bool iPlayNowEnabled;
        private bool iPlayLaterEnabled;
        private bool iConnected;

        public ViewWidgetTransportControlAdaptor()
        {
            iSubscribers = new List<IKinskyWidget>();
            iTransportState = ETransportState.eUnknown;
            iPlayNextEnabled = false;
            iPlayNowEnabled = false;
            iPlayLaterEnabled = false;
            iConnected = false;
        }

        public void Add(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Add(aItem);
                aItem.Updated = true;
            }
        }

        public void Remove(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Remove(aItem);
            }
        }

        public void Open()
        {
            lock (iLockObject)
            {
                iConnected = true;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void Close()
        {
            lock (iLockObject)
            {
                iConnected = false;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public bool Connected()
        {
            lock (iLockObject)
            {
                return iConnected;
            }
        }

        #region IViewTransportControl Members


        public void Initialised()
        {
            lock (iLockObject)
            {
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetPlayNowEnabled(bool aEnabled)
        {
            lock (iLockObject)
            {
                if (iPlayNowEnabled != aEnabled)
                {
                    iPlayNowEnabled = aEnabled;
                    foreach (IKinskyWidget i in iSubscribers)
                    {
                        i.Updated = true;
                    }
                }
            }
        }


        public void SetPlayNextEnabled(bool aEnabled)
        {
            lock (iLockObject)
            {
                if (iPlayNextEnabled != aEnabled)
                {
                    iPlayNextEnabled = aEnabled;
                    foreach (IKinskyWidget i in iSubscribers)
                    {
                        i.Updated = true;
                    }
                }
            }

        }

        public void SetPlayLaterEnabled(bool aEnabled)
        {
            lock (iLockObject)
            {
                if (iPlayLaterEnabled != aEnabled)
                {
                    iPlayLaterEnabled = aEnabled;
                    foreach (IKinskyWidget i in iSubscribers)
                    {
                        i.Updated = true;
                    }
                }
            }
        }

        /* not implemented at present */
        public void SetDragging(bool aDragging)
        {

        }

        public void SetTransportState(ETransportState aTransportState)
        {
            lock (iLockObject)
            {
                if (iTransportState != aTransportState)
                {
                    iTransportState = aTransportState;
                    foreach (IKinskyWidget i in iSubscribers)
                    {
                        i.Updated = true;
                    }
                }
            }
        }

        public void SetDuration(uint aDuration)
        {
        }

        public void OnPause()
        {
            lock (iLockObject)
            {
                if (EventPause != null)
                {
                    EventPause(this, new EventArgs());
                }
            }
        }
        public event EventHandler<EventArgs> EventPause;

        public void OnPlay()
        {
            lock (iLockObject)
            {
                if (EventPlay != null)
                {
                    EventPlay(this, new EventArgs());
                }
            }
        }
        public event EventHandler<EventArgs> EventPlay;

        public void OnStop()
        {
            lock (iLockObject)
            {
                if (EventStop != null)
                {
                    EventStop(this, new EventArgs());
                }
            }
        }
        public event EventHandler<EventArgs> EventStop;

        public void OnPrevious()
        {
            lock (iLockObject)
            {
                if (EventPrevious != null)
                {
                    EventPrevious(this, new EventArgs());
                }
            }
        }
        public event EventHandler<EventArgs> EventPrevious;

        public void OnNext()
        {
            lock (iLockObject)
            {
                if (EventNext != null)
                {
                    EventNext(this, new EventArgs());
                }
            }
        }
        public event EventHandler<EventArgs> EventNext;

        public void OnPlayNow(IMediaRetriever aMediaRetriever)
        {
            lock (iLockObject)
            {
                if (EventPlayNow != null)
                {
                    EventPlayNow(this, new EventArgsPlay(aMediaRetriever));
                }
            }
        }
        public event EventHandler<EventArgsPlay> EventPlayNow;

        public void OnPlayNext(IMediaRetriever aMediaRetriever)
        {
            lock (iLockObject)
            {
                if (EventPlayNext != null)
                {
                    EventPlayNext(this, new EventArgsPlay(aMediaRetriever));
                }
            }
        }
        public event EventHandler<EventArgsPlay> EventPlayNext;


        public void OnPlayLater(IMediaRetriever aMediaRetriever)
        {
            lock (iLockObject)
            {
                if (EventPlayLater != null)
                {
                    EventPlayLater(this, new EventArgsPlay(aMediaRetriever));
                }
            }
        }
        public event EventHandler<EventArgsPlay> EventPlayLater;

        #endregion

        public ETransportState TransportState()
        {
            lock (iLockObject)
            {
                return iTransportState;
            }
        }
        public bool PlayNowEnabled()
        {
            lock (iLockObject)
            {
                return iPlayNowEnabled;
            }
        }
        public bool PlayNextEnabled()
        {
            lock (iLockObject)
            {
                return iPlayNextEnabled;
            }
        }
        public bool PlayLaterEnabled()
        {
            lock (iLockObject)
            {
                return iPlayLaterEnabled;
            }
        }
    }
    #endregion
    #region class ViewWidgetMediaServerBrowserAdaptor

    public class ViewWidgetBrowserAdaptor : IViewWidgetContent, IViewWidgetBrowseableAdaptor, IContentHandler
    {

        private List<IKinskyWidget> iSubscribers;
        private Object iLockObject = new Object();
        private upnpObject[] iChildren;
        private Dictionary<string, int> iChildrenDict;
        private bool iConnected;
        private IBrowser iBrowser;
        private IContentCollector iContentCollector;
        private AutoResetEvent iResetEvent;
        private bool iErrorOccured;
        //private static Dictionary<string, 

        public ViewWidgetBrowserAdaptor(IBrowser aBrowser)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "New browser.");
            iSubscribers = new List<IKinskyWidget>();
            iChildren = new upnpObject[0];
            iChildrenDict = new Dictionary<string, int>();
            iConnected = false;
            iBrowser = aBrowser;
            iBrowser.EventLocationChanged += LocationChanged;
            iResetEvent = new AutoResetEvent(true);
            iErrorOccured = false;
        }

        private void LocationChanged(object sender, EventArgs e)
        {
            OnLocationChanged();
        }

        private void OnLocationChanged()
        {
            Monitor.Enter(iLockObject);
            if (iContentCollector != null)
            {
                Monitor.Exit(iLockObject);
                Trace.WriteLine(Trace.kKinskyWeb, "Content collector disposed in OnLocationChanged.");
                iContentCollector.Dispose();
                Monitor.Enter(iLockObject);
            }
            Trace.WriteLine(Trace.kKinskyWeb, "Content collector created in OnLocationChanged.");
            // reset error flag
            iErrorOccured = false;
            iConnected = false;
            iContentCollector = ContentCollectorMaster.Create(iBrowser.Location.Current, this);

            foreach (IKinskyWidget i in iSubscribers)
            {
                i.Updated = true;
            }
            Monitor.Exit(iLockObject);
        }


        #region IViewWidgetBrowser Members

        public void Open()
        {
            // not used by kinksy
            throw new NotImplementedException();
        }

        public void Close()
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, "Browser closed.");
                iChildren = null;
                iChildrenDict.Clear();
                iConnected = false;
                if (iContentCollector != null)
                {
                    Trace.WriteLine(Trace.kKinskyWeb, "Content collector disposed in close.");
                    iContentCollector.Dispose();
                }
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void Open(uint aCount)
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, String.Format("Browser opened: {0}.", aCount));
                iChildren = new upnpObject[aCount];
                iChildrenDict.Clear();
                iConnected = true;
                if (iContentCollector != null)
                {
                    Trace.WriteLine(Trace.kKinskyWeb, "Content collector range called.");
                    iContentCollector.Range(0, aCount);
                }
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
            iResetEvent.Set();
        }

        public void Item(uint aIndex, upnpObject aObject)
        {
            lock (iLockObject)
            {
                iChildren[aIndex] = aObject;
                iChildrenDict[aObject.Id] = (int)aIndex;
            }
        }

        public void ContentError(string aMessage)
        {
            bool opened = false;
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, String.Format("Browser ContentError."));
                opened = iConnected;
                iConnected = true;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
                iErrorOccured = true;
            }
            if (!opened)
            {
                // only set event if open hasn't been called
                iResetEvent.Set();
            }
        }

        public void SetArtwork(object aItem, IArtwork aArtwork) { }
        public void OnSizeClick() { }
        public void OnViewClick() { }
        public void Focus() { }


        #endregion

        #region IViewWidgetBrowseableAdaptor Members



        public void Add(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Add(aItem);
                aItem.Updated = true;
            }
        }

        public void Remove(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Remove(aItem);
            }
        }

        /*
        public void SetLocation(string[] aLocation)
        {
            lock (iLockObject)
            {
                //todo: check locations are not equal
                iLocation = aLocation;
                iConnected = false;
                container container;
                if (aLocation.Length > 0)
                {
                    container = new container(aLocation[aLocation.Length - 1]);
                }
                else
                {
                    container = iHomeContainer;
                }
                if (EventContainerSelected != null)
                {
                    EventContainerSelected(this, new EventArgsContainerSelected(container));
                }
            }
        }*/


        public void Rescan()
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, "Rescan.");
                iBrowser.Refresh();
            }
        }

        public LocationDTO[] CurrentLocation()
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, "CurrentLocation.");
                Location loc = iBrowser.Location;
                List<LocationDTO> locations = new List<LocationDTO>();
                foreach (IContainer c in loc.Containers)
                {
                    LocationDTO dto = new LocationDTO();
                    dto.ID = c.Metadata.Id;
                    dto.BreadcrumbText = c.Metadata.Title;
                    locations.Add(dto);
                }
                return locations.ToArray();
            }
        }

        public IContainer CurrentContainer()
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, "CurrentContainer.");
                return iBrowser.Location.Current;
            }
        }

        public bool Connected()
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, "Connected.");
                return iConnected;
            }
        }

        public bool ErrorOccured()
        {
            lock (iLockObject)
            {
                return iErrorOccured;
            }
        }

        public uint ChildCount()
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, "ChildCount.");
                return (uint)iChildren.Length;
            }
        }

        public UPnpObjectDTO[] GetChildren(uint aStartIndex, uint aCount)
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, "GetChildren : " + aStartIndex + ", " + aCount);
                List<UPnpObjectDTO> result = new List<UPnpObjectDTO>();
                bool found = true;

                for (int i = (int)aStartIndex;
                    i < (int)aStartIndex + (int)aCount &&
                    i < iChildren.Length &&
                    found; i++)
                {
                    if (iChildren[i] == null)
                    {
                        found = false;
                    }
                    else
                    {
                        UPnpObjectDTO item = new UPnpObjectDTO();
                        item.ID = iChildren[i].Id;
                        DidlLite d = new DidlLite();
                        d.Add(iChildren[i]);
                        item.DidlLite = d.Xml;
                        result.Add(item);
                    }
                }

                return result.ToArray();
            }
        }

        public void Activate(UPnpObjectDTO aChild)
        {

            Trace.WriteLine(Trace.kKinskyWeb, "Activate.");
            if (iChildrenDict.ContainsKey(aChild.ID))
            {
                upnpObject child = iChildren[iChildrenDict[aChild.ID]];
                if (child is container)
                {
                    iResetEvent.WaitOne();
                    iBrowser.Down((container)child);
                }
                else
                {
                    lock (iLockObject)
                    {
                        foreach (IKinskyWidget i in iSubscribers)
                        {
                            i.Updated = true;
                        }
                    }
                }
            }
            else
            {
                lock (iLockObject)
                {
                    foreach (IKinskyWidget i in iSubscribers)
                    {
                        i.Updated = true;
                    }
                }
            }

        }

        public void SetCurrentLocation(string[] location)
        {

        }

        public void Up(uint aNumberLevels)
        {
            //lock (iLockObject)
            //{
            Trace.WriteLine(Trace.kKinskyWeb, "Up.");
            if (iBrowser.Location.Containers.Count <= aNumberLevels)
            {
                aNumberLevels = (uint)iBrowser.Location.Containers.Count - 1;
            }
            if (aNumberLevels > 0)
            {
                iResetEvent.WaitOne();
                iBrowser.Up(aNumberLevels);
            }
            //}
        }

        #endregion

        #region IViewWidgetContent Members


        public void SetArtwork(object aItem, Uri aUri)
        {

        }

        #endregion
    }
    #endregion


    public class ViewWidgetStandbyAdaptor : IViewWidgetButton
    {
        private List<IKinskyWidget> iSubscribers;
        private Object iLockObject = new Object();
        private bool iConnected;

        public ViewWidgetStandbyAdaptor()
        {
            iSubscribers = new List<IKinskyWidget>();
            iConnected = false;
        }

        public void Add(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Add(aItem);
                aItem.Updated = true;
            }
        }

        public void Remove(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Remove(aItem);
            }
        }

        public void Open()
        {
            lock (iLockObject)
            {
                iConnected = true;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void Close()
        {
            lock (iLockObject)
            {
                iConnected = false;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void OnEventClick()
        {
            if (EventClick != null)
            {
                EventClick(this, new EventArgs());
            }
        }

        #region IViewWidgetButton Members


        public event EventHandler<EventArgs> EventClick;

        #endregion
    }

    public class ViewWidgetPlaylistAdaptor
        : IViewWidgetPlaylist,
        IViewWidgetPlaylistAux,
        IViewWidgetPlaylistDiscPlayer,
        IViewWidgetPlaylistRadio,
        IViewWidgetBrowseableAdaptor,
        IViewWidgetPlaylistAdaptor
    {

        private List<IKinskyWidget> iSubscribers;
        private Object iLockObject = new Object();
        private List<MrItem> iChildren;
        private Dictionary<string, int> iChildIndexCache;
        private bool iConnected;
        private int iCurrentIndex;
        private string iCurrentID;
        private string iType;
        private ESourceType iSourceType;

        public ViewWidgetPlaylistAdaptor(ESourceType aSourceType)
        {
            iSubscribers = new List<IKinskyWidget>();
            iChildren = new List<MrItem>();
            iCurrentIndex = -1;
            iConnected = false;
            iType = string.Empty;
            iSourceType = aSourceType;
            iChildIndexCache = new Dictionary<string, int>();
        }

        public void Add(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Add(aItem);
                aItem.Updated = true;
            }
        }

        public void Remove(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Remove(aItem);
            }
        }


        public void Open(string aType)
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, String.Format("Playlist opened."));
                iType = aType;
                iChildren.Clear();
                iChildIndexCache.Clear();
                iCurrentIndex = -1;
                iCurrentID = null;
                iConnected = false;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void Open()
        {
            Open(null);
        }

        public void Close()
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, String.Format("Playlist closed."));
                iChildren.Clear();
                iChildIndexCache.Clear();
                iCurrentIndex = -1;
                iCurrentID = null;
                iConnected = false;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetPlaylist(IList<MrItem> aPlaylist)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "Set playlist: " + aPlaylist.Count());
            lock (iLockObject)
            {
                iChildren.Clear();
                iChildIndexCache.Clear();
                iCurrentIndex = -1;
                iConnected = true;
                for (int i = 0; i < aPlaylist.Count; i++)
                {
                    if (aPlaylist[i] != null)
                    {
                        string id = aPlaylist[i].Id.ToString();
                        iChildren.Add(aPlaylist[i]);
                        iChildIndexCache.Add(id, iChildren.Count() - 1);
                    }
                }

                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetTrack(MrItem aTrack)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "Set track: " + ((aTrack != null) ? aTrack.Id.ToString() : "null"));
            lock (iLockObject)
            {
                iCurrentIndex = -1;
                if (aTrack != null)
                {
                    iCurrentID = aTrack.Id.ToString();
                }
                else
                {
                    iCurrentID = null;
                }
                foreach (IKinskyWidget i in iSubscribers)
                {
                    if (i is IViewKinskyPlaylist)
                    {
                        i.Updated = true;
                    }
                }
            }
        }

        public void SetPresets(IList<MrItem> aPresets)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "Set presets: " + aPresets.Count());
            lock (iLockObject)
            {
                iChildren.Clear();
                iCurrentID = null;
                for (int i = 0; i < aPresets.Count; i++)
                {
                    if (aPresets[i] != null)
                    {
                        string id = aPresets[i].Id.ToString();
                        iChildren.Add(aPresets[i]);
                        if (!iChildIndexCache.ContainsKey(id))
                        {
                            iChildIndexCache.Add(id, iChildren.Count() - 1);
                        }
                    }
                }
                iConnected = true;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetChannel(Channel aChannel)
        {
            // not used at present
        }

        public void SetPreset(int aPresetIndex)
        {
            Trace.WriteLine(Trace.kKinskyWeb, "Set preset: " + aPresetIndex);
            lock (iLockObject)
            {
                iCurrentID = null;
                iCurrentIndex = aPresetIndex;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    if (i is IViewKinskyPlaylist)
                    {
                        i.Updated = true;
                    }
                }
            }
        }

        public void Save()
        {
            // not used at present
        }
        public void Delete()
        {
            // not used at present
        }

        public void Initialised()
        {
            lock (iLockObject)
            {
                /*iConnected = true;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }*/
            }
        }
        public void Eject()
        {

        }

        public event EventHandler<EventArgsSeekTrack> EventSeekTrack;
        public event EventHandler<EventArgsPlaylistInsert> EventPlaylistInsert;
        public event EventHandler<EventArgsPlaylistMove> EventPlaylistMove;
        public event EventHandler<EventArgsPlaylistDelete> EventPlaylistDelete;
        public event EventHandler<EventArgs> EventPlaylistDeleteAll;
        public event EventHandler<EventArgsSetPreset> EventSetPreset;
        public event EventHandler<EventArgsSetChannel> EventSetChannel;

        #region IViewWidgetBrowserAdaptor Members


        public void Rescan()
        {
        }

        public LocationDTO[] CurrentLocation()
        {
            throw new NotImplementedException();
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
                return (uint)iChildren.Count;
            }
        }

        public UPnpObjectDTO[] GetChildren(uint aStartIndex, uint aCount)
        {
            lock (iLockObject)
            {
                Trace.WriteLine(Trace.kKinskyWeb, String.Format("Playlist.GetChildren: aStartIndex={0}, aCount={1}, iChildren.Count={2}", aStartIndex, aCount, iChildren.Count));
                List<UPnpObjectDTO> result = new List<UPnpObjectDTO>();
                if ((aStartIndex + aCount) <= iChildren.Count)
                {
                    foreach (MrItem child in iChildren.Skip((int)aStartIndex).Take((int)aCount))
                    {
                        UPnpObjectDTO item = new UPnpObjectDTO();
                        item.DidlLite = child.DidlLite.Xml;
                        item.ID = child.Id.ToString();
                        result.Add(item);
                    }
                }
                return result.ToArray();
            }
        }

        public void Activate(UPnpObjectDTO aChild)
        {
            lock (iLockObject)
            {
                if (iChildIndexCache.ContainsKey(aChild.ID))
                {
                    int index = iChildIndexCache[aChild.ID];
                    switch (iSourceType)
                    {
                        case ESourceType.eRadio:
                            if (EventSetPreset != null)
                            {
                                EventSetPreset(this, new EventArgsSetPreset(iChildren[index]));
                            }
                            break;
                        default:
                            if (EventSeekTrack != null)
                            {
                                EventSeekTrack(this, new EventArgsSeekTrack((uint)index));
                            }
                            break;
                    }
                }
            }

        }

        public void Up(uint aNumberLevels)
        {
            // not implemented in playlists
        }

        public IContainer CurrentContainer()
        {
            // not implemented in playlists
            return null;
        }

        public void SetCurrentLocation(string[] aLocation)
        {
            // not used in playlists
        }
        public bool ErrorOccured()
        {
            return false;
        }

        #endregion

        #region IViewWidgetPlaylistAdaptor Members

        public void Move(UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem)
        {
            lock (iLockObject)
            {
                if (EventPlaylistMove != null)
                {
                    List<MrItem> objects = new List<MrItem>();
                    foreach (UPnpObjectDTO aChild in aChildren)
                    {
                        if (iChildIndexCache.ContainsKey(aChild.ID))
                        {
                            objects.Add(iChildren[iChildIndexCache[aChild.ID]]);
                        }
                    }

                    uint insertID = 0;
                    if (aInsertAfterItem != null)
                    {
                        insertID = UInt32.Parse(aInsertAfterItem.ID);
                    }

                    EventPlaylistMove(this, new EventArgsPlaylistMove(insertID, objects));
                }
            }
        }

        public void MoveToEnd(UPnpObjectDTO[] aChildren)
        {
            lock (iLockObject)
            {
                if (EventPlaylistMove != null && iChildren.Count > 0)
                {
                    List<MrItem> objects = new List<MrItem>();
                    foreach (UPnpObjectDTO aChild in aChildren)
                    {
                        if (iChildIndexCache.ContainsKey(aChild.ID))
                        {
                            objects.Add(iChildren[iChildIndexCache[aChild.ID]]);
                        }
                    }

                    uint insertID = iChildren[iChildren.Count - 1].Id;

                    EventPlaylistMove(this, new EventArgsPlaylistMove(insertID, objects));
                }
            }
        }

        public void Insert(UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem, IContainer aSourceContainer)
        {
            lock (iLockObject)
            {
                if (EventPlaylistInsert != null)
                {

                    List<upnpObject> objects = new List<upnpObject>();
                    foreach (UPnpObjectDTO aChild in aChildren)
                    {
                        DidlLite d = new DidlLite(aChild.DidlLite.ToString());
                        objects.Add(d[0]);
                    }
                    IMediaRetriever retriever = null;
                    if (aSourceContainer != null)
                    {
                        retriever = new MediaRetriever(aSourceContainer, objects);
                    }
                    else
                    {
                        retriever = new MediaRetrieverNoRetrieve(objects);
                    }
                    uint insertID = 0;
                    if (aInsertAfterItem != null && aInsertAfterItem.ID != null)
                    {
                        insertID = UInt32.Parse(aInsertAfterItem.ID);
                    }
                    EventPlaylistInsert(this, new EventArgsPlaylistInsert(insertID, retriever));
                }
            }
        }

        public void Delete(UPnpObjectDTO[] aChildren)
        {
            lock (iLockObject)
            {
                if (EventPlaylistDelete != null)
                {
                    List<MrItem> objects = new List<MrItem>();
                    foreach (UPnpObjectDTO aChild in aChildren)
                    {
                        if (iChildIndexCache.ContainsKey(aChild.ID))
                        {
                            objects.Add(iChildren[iChildIndexCache[aChild.ID]]);
                        }
                    }

                    EventPlaylistDelete(this, new EventArgsPlaylistDelete(objects));
                }
            }
        }

        public void DeleteAll()
        {
            lock (iLockObject)
            {
                if (EventPlaylistDeleteAll != null)
                {
                    EventPlaylistDeleteAll(this, new EventArgs());
                }
            }
        }

        public UPnpObjectDTO CurrentItem()
        {
            lock (iLockObject)
            {
                UPnpObjectDTO dto = new UPnpObjectDTO();
                if (iCurrentIndex != -1)
                {
                    MrItem current = iChildren[iCurrentIndex];
                    dto.DidlLite = current.DidlLite.Xml;
                    dto.ID = current.Id.ToString();
                }
                else if (iCurrentID != null && iChildIndexCache.ContainsKey(iCurrentID))
                {

                    MrItem current = iChildren[iChildIndexCache[iCurrentID]];
                    dto.DidlLite = current.DidlLite.Xml;
                    dto.ID = current.Id.ToString();
                }
                return dto;
            }
        }

        #endregion
    }

    #region class ViewWidgetTrackAdaptor
    public class ViewWidgetTrackAdaptor : IViewWidgetTrack
    {
        private List<IKinskyWidget> iSubscribers;
        private Object iLockObject = new Object();
        private Image iArtwork;
        private string iTitle;
        private string iArtist;
        private string iAlbum;
        private uint iBitrate;
        private float iSampleRate;
        private uint iBitDepth;
        private string iCodec;
        private bool iLossless;
        private bool iConnected;

        private static readonly Image kNoAlbumArt = Linn.Kinsky.Properties.Resources.NoAlbumArt;

        public ViewWidgetTrackAdaptor()
        {
            iSubscribers = new List<IKinskyWidget>();
            iConnected = false;
        }

        public void Add(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Add(aItem);
                aItem.Updated = true;
            }
        }

        public void Remove(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Remove(aItem);
            }
        }

        public void Open()
        {
            lock (iLockObject)
            {
                iConnected = true;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void Close()
        {
            lock (iLockObject)
            {
                iConnected = false;
                iArtwork = kNoAlbumArt;
                iTitle = "";
                iArtist = "";
                iAlbum = "";
                iBitrate = 0;
                iSampleRate = 0f;
                iBitDepth = 0;
                iCodec = "";
                iLossless = false;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }


        #region IViewWidgetTrack Members


        public void Initialised()
        {
            lock (iLockObject)
            {
                iConnected = true;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetItem(upnpObject aObject)
        {
        }

        public void SetNoArtwork()
        {
            lock (iLockObject)
            {
                this.iArtwork = kNoAlbumArt;
            }
        }

        public void SetArtworkError()
        {
            lock (iLockObject)
            {
                this.iArtwork = kNoAlbumArt;
            }
        }

        public void SetArtwork(Image aArtwork)
        {
            lock (iLockObject)
            {
                this.iArtwork = aArtwork;
            }
            //Update();
        }

        public void SetTitle(string aTitle)
        {
            lock (iLockObject)
            {
                this.iTitle = aTitle;
            }
            //Update();
        }

        public void SetArtist(string aArtist)
        {
            lock (iLockObject)
            {
                this.iArtist = aArtist;
            }
            //Update();
        }

        public void SetAlbum(string aAlbum)
        {
            lock (iLockObject)
            {
                this.iAlbum = aAlbum;
            }
            //Update();
        }

        public void SetBitrate(uint aBitrate)
        {
            lock (iLockObject)
            {
                this.iBitrate = aBitrate;
            }
            //Update();
        }

        public void SetSampleRate(float aSampleRate)
        {
            lock (iLockObject)
            {
                this.iSampleRate = aSampleRate;
            }
            //Update();
        }

        public void SetBitDepth(uint aBitDepth)
        {
            lock (iLockObject)
            {
                this.iBitDepth = aBitDepth;
            }
            //Update();
        }

        public void SetCodec(string aCodec)
        {
            lock (iLockObject)
            {
                this.iCodec = aCodec;
            }
            //Update();
        }

        public void SetLossless(bool aLossless)
        {
            lock (iLockObject)
            {
                this.iLossless = aLossless;
            }
            //Update();
        }

        public void Update()
        {
            lock (iLockObject)
            {
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public System.Drawing.Image Artwork()
        {
            return iArtwork;
        }

        public string Title()
        {
            return iTitle;
        }

        public string Artist()
        {
            return iArtist;
        }

        public string Album()
        {
            return iAlbum;
        }

        public uint Bitrate()
        {
            return iBitrate;
        }

        public float SampleRate()
        {
            return iSampleRate;
        }

        public uint BitDepth()
        {
            return iBitDepth;
        }

        public string Codec()
        {
            return iCodec;
        }

        public bool Lossless()
        {
            return iLossless;
        }

        public bool Connected()
        {
            return iConnected;
        }

        #endregion
    }
    #endregion
    public class ViewWidgetPlayModeAdaptor : IViewWidgetPlayMode
    {
        private List<IKinskyWidget> iSubscribers;
        private Object iLockObject = new Object();
        private bool iShuffle;
        private bool iRepeat;
        private bool iConnected;


        public ViewWidgetPlayModeAdaptor()
        {
            iSubscribers = new List<IKinskyWidget>();
            iShuffle = false;
            iRepeat = false;
        }

        public void Add(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Add(aItem);
                aItem.Updated = true;
            }
        }

        public void Remove(IKinskyWidget aItem)
        {
            lock (iLockObject)
            {
                iSubscribers.Remove(aItem);
            }
        }

        public void Open()
        {
            lock (iLockObject)
            {
                iConnected = true;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void Close()
        {
            lock (iLockObject)
            {
                iShuffle = false;
                iRepeat = false;
                iConnected = false;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public bool Connected()
        {
            lock (iLockObject)
            {
                return iConnected;
            }
        }

        #region IViewWidgetPlayMode Members


        public void Initialised()
        {
            lock (iLockObject)
            {
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetShuffle(bool aShuffle)
        {
            lock (iLockObject)
            {
                this.iShuffle = aShuffle;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public void SetRepeat(bool aRepeat)
        {
            lock (iLockObject)
            {
                this.iRepeat = aRepeat;
                foreach (IKinskyWidget i in iSubscribers)
                {
                    i.Updated = true;
                }
            }
        }

        public bool Shuffle()
        {
            lock (iLockObject)
            {
                return iShuffle;
            }
        }
        public bool Repeat()
        {
            lock (iLockObject)
            {
                return iRepeat;
            }
        }

        public void OnEventToggleShuffle()
        {
            lock (iLockObject)
            {
                if (EventToggleShuffle != null)
                {
                    EventToggleShuffle(this, new EventArgs());
                }
            }
        }
        public event EventHandler<EventArgs> EventToggleShuffle;

        public void OnEventToggleRepeat()
        {
            lock (iLockObject)
            {
                if (EventToggleRepeat != null)
                {
                    EventToggleRepeat(this, new EventArgs());
                }
            }
        }
        public event EventHandler<EventArgs> EventToggleRepeat;

        #endregion
    }
}