using System;
using System.Collections.Generic;
using Linn.Kinsky;
using Upnp;
using KinskyWeb.Services;
namespace KinskyWeb.Kinsky
{
    public class ViewKinskyRoomList : KinskyWidgetBase, IViewKinskyRoomList
    {
        public ViewKinskyRoomList()
            : base()
        {
        }

        public void Subscribe()
        {
            ViewWidgetSelectorRoomAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorRoom as ViewWidgetSelectorRoomAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            KinskyContainer c = Container as KinskyContainer;
            if (c != null)
            {
                ViewWidgetSelectorRoomAdaptor master = c.ViewWidgetSelectorRoom as ViewWidgetSelectorRoomAdaptor;
                if (master != null)
                {
                    master.Remove(this);
                }
            }
        }

        public string[] Rooms()
        {
            ViewWidgetSelectorRoomAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorRoom as ViewWidgetSelectorRoomAdaptor;
            return master.Rooms();
        }

        public override void OnContainerTerminated()
        {
            Unsubscribe();
        }

    }

    public class ViewKinskySourceList : KinskyWidgetBase, IViewKinskySourceList
    {
        public ViewKinskySourceList()
            : base()
        {
        }

        public void Subscribe(string aRoom)
        {
            ViewWidgetSelectorSourceAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorSource as ViewWidgetSelectorSourceAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            ViewWidgetSelectorSourceAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorSource as ViewWidgetSelectorSourceAdaptor;
            master.Remove(this);
        }

        public SourceDTO[] Sources()
        {
            ViewWidgetSelectorSourceAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorSource as ViewWidgetSelectorSourceAdaptor;
            return master.Sources();
        }

        public override void OnContainerTerminated()
        {
            Unsubscribe();
        }
    }

    public class ViewKinskyRoom : KinskyWidgetBase, IViewKinskyRoom
    {
        public ViewKinskyRoom()
            : base()
        {
        }

        public void Subscribe(string aRoom)
        {
            ViewWidgetSelectorRoomAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorRoom as ViewWidgetSelectorRoomAdaptor;
            master.Add(this);
            master.OnEventSelectionChanged(aRoom);
        }

        public void Unsubscribe()
        {
            ViewWidgetSelectorRoomAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorRoom as ViewWidgetSelectorRoomAdaptor;
            master.Remove(this);
        }

        public string CurrentSource()
        {
            ViewWidgetSelectorSourceAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorSource as ViewWidgetSelectorSourceAdaptor;
            return master.CurrentSource();
        }

        public ESourceType CurrentSourceType()
        {
            ViewWidgetSelectorSourceAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorSource as ViewWidgetSelectorSourceAdaptor;
            return master.CurrentSourceType();
        }

        public override void OnContainerTerminated()
        {
            Unsubscribe();
        }
    }
    public class ControllerKinskyRoom : KinskyWidgetBase, IControllerKinskyRoom
    {
        public ControllerKinskyRoom()
            : base()
        {
        }

        public void Subscribe(string aRoom)
        {
            ViewWidgetSelectorRoomAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorRoom as ViewWidgetSelectorRoomAdaptor;
            master.Add(this);
            master.OnEventSelectionChanged(aRoom);
        }

        public void Unsubscribe()
        {
            ViewWidgetSelectorRoomAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorRoom as ViewWidgetSelectorRoomAdaptor;
            master.Remove(this);
            master.OnEventSelectionChanged(null);
        }

        public void SelectSource(string aSource)
        {
            ViewWidgetSelectorSourceAdaptor master = (Container as KinskyContainer).ViewWidgetSelectorSource as ViewWidgetSelectorSourceAdaptor;
            master.OnEventSelectionChanged(aSource);
        }

        public void Standby()
        {
            ViewWidgetStandbyAdaptor master = (Container as KinskyContainer).ViewWidgetButtonStandby as ViewWidgetStandbyAdaptor;
            master.OnEventClick();
        }

        public override void OnContainerTerminated()
        {
            Unsubscribe();
        }
    }

    public class ViewVolumeControl : KinskyWidgetBase, IViewKinskyVolumeControl
    {

        #region IViewKinskyVolumeControl Members

        public uint Volume()
        {
            ViewWidgetVolumeControlAdaptor master = (Container as KinskyContainer).ViewWidgetVolumeControl as ViewWidgetVolumeControlAdaptor;
            return master.Volume();
        }

        public bool Mute()
        {
            ViewWidgetVolumeControlAdaptor master = (Container as KinskyContainer).ViewWidgetVolumeControl as ViewWidgetVolumeControlAdaptor;
            return master.Mute();
        }

        public uint VolumeLimit()
        {
            ViewWidgetVolumeControlAdaptor master = (Container as KinskyContainer).ViewWidgetVolumeControl as ViewWidgetVolumeControlAdaptor;
            return master.VolumeLimit();
        }

        public bool Connected()
        {
            ViewWidgetVolumeControlAdaptor master = (Container as KinskyContainer).ViewWidgetVolumeControl as ViewWidgetVolumeControlAdaptor;
            return master.Connected();
        }

        public void Subscribe(string aRoom)
        {
            ViewWidgetVolumeControlAdaptor master = (Container as KinskyContainer).ViewWidgetVolumeControl as ViewWidgetVolumeControlAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            ViewWidgetVolumeControlAdaptor master = (Container as KinskyContainer).ViewWidgetVolumeControl as ViewWidgetVolumeControlAdaptor;
            master.Remove(this);
        }

        #endregion
        public override void OnContainerTerminated()
        {
            this.Unsubscribe();
        }
    }
    public class ControllerVolumeControl : KinskyWidgetBase, IControllerKinskyVolumeControl
    {


        public void Subscribe(string aRoom)
        {
            ViewWidgetVolumeControlAdaptor master = (Container as KinskyContainer).ViewWidgetVolumeControl as ViewWidgetVolumeControlAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            ViewWidgetVolumeControlAdaptor master = (Container as KinskyContainer).ViewWidgetVolumeControl as ViewWidgetVolumeControlAdaptor;
            master.Remove(this);
        }

        public override void OnContainerTerminated()
        {
            this.Unsubscribe();
        }

        #region IControllerKinskyVolumeControl Members

        public void SetVolume(uint aVolume)
        {
            ViewWidgetVolumeControlAdaptor master = (Container as KinskyContainer).ViewWidgetVolumeControl as ViewWidgetVolumeControlAdaptor;
            master.OnEventVolumeChanged(aVolume);
        }

        public void SetMute(bool aMute)
        {
            ViewWidgetVolumeControlAdaptor master = (Container as KinskyContainer).ViewWidgetVolumeControl as ViewWidgetVolumeControlAdaptor;
            master.OnEventMuteChanged(aMute);
        }

        #endregion
    }

    public class ViewMediaTime : KinskyWidgetBase, IViewKinskyMediaTime
    {

        public void Unsubscribe()
        {
            ViewWidgetMediaTimeAdaptor master = (Container as KinskyContainer).ViewWidgetMediaTime as ViewWidgetMediaTimeAdaptor;
            master.Remove(this);
        }

        public override void OnContainerTerminated()
        {
            this.Unsubscribe();
        }

        #region IViewKinskyMediaTime Members

        public void Subscribe(string aRoom, string aSource)
        {
            ViewWidgetMediaTimeAdaptor master = (Container as KinskyContainer).ViewWidgetMediaTime as ViewWidgetMediaTimeAdaptor;
            master.Add(this);
        }

        public ETransportState TransportState()
        {
            ViewWidgetMediaTimeAdaptor master = (Container as KinskyContainer).ViewWidgetMediaTime as ViewWidgetMediaTimeAdaptor;
            return master.TransportState();
        }

        public uint Duration()
        {
            ViewWidgetMediaTimeAdaptor master = (Container as KinskyContainer).ViewWidgetMediaTime as ViewWidgetMediaTimeAdaptor;
            return master.Duration();
        }

        public uint Seconds()
        {
            ViewWidgetMediaTimeAdaptor master = (Container as KinskyContainer).ViewWidgetMediaTime as ViewWidgetMediaTimeAdaptor;
            return master.Seconds();
        }
        public bool Connected()
        {
            ViewWidgetMediaTimeAdaptor master = (Container as KinskyContainer).ViewWidgetMediaTime as ViewWidgetMediaTimeAdaptor;
            return master.Connected();
        }

        #endregion
    }

    public class ControllerMediaTime : KinskyWidgetBase, IControllerKinskyMediaTime
    {
        public void Subscribe(string aRoom, string aSource)
        {
            ViewWidgetMediaTimeAdaptor master = (Container as KinskyContainer).ViewWidgetMediaTime as ViewWidgetMediaTimeAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            ViewWidgetMediaTimeAdaptor master = (Container as KinskyContainer).ViewWidgetMediaTime as ViewWidgetMediaTimeAdaptor;
            master.Remove(this);
        }

        public override void OnContainerTerminated()
        {
            this.Unsubscribe();
        }

        #region IControllerKinskyMediaTime Members


        public void SetSeconds(uint aSeconds)
        {
            ViewWidgetMediaTimeAdaptor master = (Container as KinskyContainer).ViewWidgetMediaTime as ViewWidgetMediaTimeAdaptor;
            master.OnEventSeekSeconds(aSeconds);
        }

        #endregion
    }

    public class ViewTransportControl : KinskyWidgetBase, IViewKinskyTransportControl
    {
        public void Subscribe(string aRoom, string aSource)
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.Remove(this);
        }

        public override void OnContainerTerminated()
        {
            this.Unsubscribe();
        }

        #region IViewKinskyTransportControl Members

        public ETransportState TransportState()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            return master.TransportState();
        }
        public bool Connected()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            return master.Connected();
        }

        #endregion

        #region IViewKinskyTransportControl Members


        public bool PlayNowEnabled()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            return master.PlayNowEnabled();
        }

        public bool PlayNextEnabled()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            return master.PlayNextEnabled();
        }

        public bool PlayLaterEnabled()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            return master.PlayLaterEnabled();
        }

        #endregion
    }

    public class ControllerTransportControl : KinskyWidgetBase, IControllerKinskyTransportControl
    {
        public void Subscribe(string aRoom, string aSource)
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.Remove(this);
        }

        public override void OnContainerTerminated()
        {
            this.Unsubscribe();
        }

        #region IControllerKinskyTransportControl Members

        public void Pause()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.OnPause();
        }

        public void Play()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.OnPlay();
        }

        public void Stop()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.OnStop();
        }

        public void Previous()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.OnPrevious();
        }

        public void Next()
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.OnNext();
        }

        public void PlayNow(IMediaRetriever aMediaRetriever)
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.OnPlayNow(aMediaRetriever);
        }

        public void PlayNext(IMediaRetriever aMediaRetriever)
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.OnPlayNext(aMediaRetriever);
        }

        public void PlayLater(IMediaRetriever aMediaRetriever)
        {
            ViewWidgetTransportControlAdaptor master = (Container as KinskyContainer).ViewWidgetTransportControlMediaRenderer as ViewWidgetTransportControlAdaptor;
            master.OnPlayLater(aMediaRetriever);
        }

        #endregion
    }


    public class KinskyBrowser : KinskyWidgetBase, IKinskyBrowser
    {

        #region IKinskyBrowser Members

        private EContentBrowserType iBrowserType;
        private ESourceType iSourceType;

        public void Subscribe(string[] aLocation)
        {
            /*
             hack: for now, we differentiate between browser sources at the backend by the location object passed in              
             */
            this.iBrowserType = aLocation.Length > 1 && aLocation[1].Equals("Rooms") ? EContentBrowserType.eMediaRenderer : EContentBrowserType.eMediaServer;

            ViewWidgetSelectorSourceAdaptor sourceSelector = (Container as KinskyContainer).ViewWidgetSelectorSource as ViewWidgetSelectorSourceAdaptor;
            this.iSourceType = sourceSelector.CurrentSourceType();
            IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
            master.SetCurrentLocation(aLocation);
            master.Add(this);
        }

        public void Unsubscribe()
        {
            IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
            master.Remove(this);
        }

        public LocationDTO[] CurrentLocation()
        {
            /*
             hack: for now, we hard code result for a media renderer browser
             */
            if (this.iBrowserType == EContentBrowserType.eMediaRenderer)
            {
                ViewWidgetSelectorRoomAdaptor roomSelector = (Container as KinskyContainer).ViewWidgetSelectorRoom as ViewWidgetSelectorRoomAdaptor;
                ViewWidgetSelectorSourceAdaptor sourceSelector = (Container as KinskyContainer).ViewWidgetSelectorSource as ViewWidgetSelectorSourceAdaptor;
                LocationDTO[] result = new LocationDTO[4];
                LocationDTO home = new LocationDTO();
                home.ID = "Home";
                home.BreadcrumbText = home.ID;
                result[0] = home;
                LocationDTO rooms = new LocationDTO();
                rooms.ID = "Rooms";
                rooms.BreadcrumbText = rooms.ID;
                result[1] = rooms;
                LocationDTO room = new LocationDTO();
                room.ID = roomSelector.CurrentRoom();
                room.BreadcrumbText = room.ID;
                result[2] = room;
                LocationDTO source = new LocationDTO();
                source.ID = sourceSelector.CurrentSource();
                source.BreadcrumbText = source.ID;
                result[3] = source;
                return result;
            }
            else
            {
                IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
                return master.CurrentLocation();
            }
        }

        public bool Connected()
        {
            IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
            return master.Connected();
        }

        public uint ChildCount()
        {
            IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
            return master.ChildCount();
        }

        public UPnpObjectDTO[] GetChildren(uint aStartIndex, uint aCount)
        {
            IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
            return master.GetChildren(aStartIndex, aCount);
        }


        public void Rescan()
        {
            IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
            master.Rescan();
        }

        public void Activate(UPnpObjectDTO aChild)
        {
            IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
            master.Activate(aChild);
        }

        public void Up(uint aNumberLevels)
        {
            IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
            master.Up(aNumberLevels);
        }

        #endregion

        public override void OnContainerTerminated()
        {
            Unsubscribe();
        }

        #region IKinskyBrowser Members


        public IContainer CurrentContainer()
        {
            IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
            if (master != null)
            {
                return master.CurrentContainer();
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region IKinskyBrowser Members


        public bool ErrorOccured()
        {
            IViewWidgetBrowseableAdaptor master = (Container as KinskyContainer).GetContentBrowser(iBrowserType, iSourceType);
            return master.ErrorOccured();
        }

        #endregion
    }

    public class ControllerDisc : KinskyWidgetBase, IControllerKinskyDisc
    {
        public void Subscribe(string aRoom, string aSource)
        {
            ViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).ViewWidgetPlaylistDiscPlayer as ViewWidgetPlaylistAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            ViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).ViewWidgetPlaylistDiscPlayer as ViewWidgetPlaylistAdaptor;
            master.Remove(this);
        }

        public void Eject()
        {
            ViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).ViewWidgetPlaylistDiscPlayer as ViewWidgetPlaylistAdaptor;
            master.Eject();
        }

        public override void OnContainerTerminated()
        {
            Unsubscribe();
        }
    }


    public class ViewTrack : KinskyWidgetBase, IViewKinskyTrack
    {
        public void Subscribe(string aRoom, string aSource)
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            master.Remove(this);
        }

        public override void OnContainerTerminated()
        {
            Unsubscribe();
        }

        #region IViewKinskyTrack Members


        public System.Drawing.Image Artwork()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            return master.Artwork();
        }

        public string Title()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            return master.Title();
        }

        public string Artist()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            return master.Artist();
        }

        public string Album()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            return master.Album();
        }

        public uint Bitrate()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            return master.Bitrate();
        }

        public float SampleRate()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            return master.SampleRate();
        }

        public uint BitDepth()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            return master.BitDepth();
        }

        public string Codec()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            return master.Codec();
        }

        public bool Lossless()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            return master.Lossless();
        }
        public bool Connected()
        {
            ViewWidgetTrackAdaptor master = (Container as KinskyContainer).ViewWidgetTrack as ViewWidgetTrackAdaptor;
            return master.Connected();
        }

        #endregion
    }

    public class ViewPlaylist : KinskyWidgetBase, IViewKinskyPlaylist
    {
        #region IViewKinskyPlaylist Members

        private ESourceType iSourceType;

        public void Subscribe(string aRoom, string aSource)
        {
            ViewWidgetSelectorSourceAdaptor sourceSelector = (Container as KinskyContainer).ViewWidgetSelectorSource as ViewWidgetSelectorSourceAdaptor;
            this.iSourceType = sourceSelector.CurrentSourceType();
            IViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).GetPlaylist(iSourceType);
            master.Add(this);
        }

        public void Unsubscribe()
        {
            IViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).GetPlaylist(iSourceType);
            master.Remove(this);
        }

        public UPnpObjectDTO CurrentItem()
        {
            IViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).GetPlaylist(iSourceType);
            return master.CurrentItem();
        }

        public override void OnContainerTerminated()
        {
            Unsubscribe();
        }
        #endregion
    }

    public class ControllerPlaylist : KinskyWidgetBase, IControllerKinskyPlaylist
    {

        private ESourceType iSourceType;

        public void Subscribe(string aRoom, string aSource)
        {
            ViewWidgetSelectorSourceAdaptor sourceSelector = (Container as KinskyContainer).ViewWidgetSelectorSource as ViewWidgetSelectorSourceAdaptor;
            this.iSourceType = sourceSelector.CurrentSourceType();
            IViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).GetPlaylist(iSourceType);
            master.Add(this);
        }

        public void Unsubscribe()
        {
            IViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).GetPlaylist(iSourceType);
            master.Remove(this);
        }

        public void Move(UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem)
        {
            IViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).GetPlaylist(iSourceType);
            if (master != null)
            {
                master.Move(aChildren, aInsertAfterItem);
            }
        }
        public void MoveToEnd(UPnpObjectDTO[] aChildren)
        {
            IViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).GetPlaylist(iSourceType);
            if (master != null)
            {
                master.MoveToEnd(aChildren);
            }
        }
        public void Insert(UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem, IContainer aSourceContainer)
        {
            IViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).GetPlaylist(iSourceType);
            if (master != null)
            {
                master.Insert(aChildren, aInsertAfterItem, aSourceContainer);
            }
        }

        public void Delete(UPnpObjectDTO[] aChildren)
        {
            IViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).GetPlaylist(iSourceType);
            if (master != null)
            {
                master.Delete(aChildren);
            }
        }

        public void DeleteAll()
        {
            IViewWidgetPlaylistAdaptor master = (Container as KinskyContainer).GetPlaylist(iSourceType);
            if (master != null)
            {
                master.DeleteAll();
            }
        }

        public override void OnContainerTerminated()
        {
            Unsubscribe();
        }
    }

    public class ViewPlayMode : KinskyWidgetBase, IViewKinskyPlayMode
    {
        public void Subscribe(string aRoom, string aSource)
        {
            ViewWidgetPlayModeAdaptor master = (Container as KinskyContainer).ViewWidgetPlayMode as ViewWidgetPlayModeAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            ViewWidgetPlayModeAdaptor master = (Container as KinskyContainer).ViewWidgetPlayMode as ViewWidgetPlayModeAdaptor;
            master.Remove(this);
        }

        public override void OnContainerTerminated()
        {
            this.Unsubscribe();
        }

        #region IViewKinskyPlayMode Members


        public bool Shuffle()
        {
            ViewWidgetPlayModeAdaptor master = (Container as KinskyContainer).ViewWidgetPlayMode as ViewWidgetPlayModeAdaptor;
            return master.Shuffle();
        }

        public bool Repeat()
        {
            ViewWidgetPlayModeAdaptor master = (Container as KinskyContainer).ViewWidgetPlayMode as ViewWidgetPlayModeAdaptor;
            return master.Repeat();
        }

        public bool Connected()
        {
            ViewWidgetPlayModeAdaptor master = (Container as KinskyContainer).ViewWidgetPlayMode as ViewWidgetPlayModeAdaptor;
            return master.Connected();
        }

        #endregion
    }

    public class ControllerPlayMode : KinskyWidgetBase, IControllerKinskyPlayMode
    {
        public void Subscribe(string aRoom, string aSource)
        {
            ViewWidgetPlayModeAdaptor master = (Container as KinskyContainer).ViewWidgetPlayMode as ViewWidgetPlayModeAdaptor;
            master.Add(this);
        }

        public void Unsubscribe()
        {
            ViewWidgetPlayModeAdaptor master = (Container as KinskyContainer).ViewWidgetPlayMode as ViewWidgetPlayModeAdaptor;
            master.Remove(this);
        }

        public override void OnContainerTerminated()
        {
            this.Unsubscribe();
        }

        #region IViewKinskyPlayMode Members


        public void ToggleShuffle()
        {
            ViewWidgetPlayModeAdaptor master = (Container as KinskyContainer).ViewWidgetPlayMode as ViewWidgetPlayModeAdaptor;
            master.OnEventToggleShuffle();
        }

        public void ToggleRepeat()
        {
            ViewWidgetPlayModeAdaptor master = (Container as KinskyContainer).ViewWidgetPlayMode as ViewWidgetPlayModeAdaptor;
            master.OnEventToggleRepeat();
        }

        #endregion
    }
}
