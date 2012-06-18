using System;
using System.Collections.Generic;
using System.Drawing;
using Linn.Kinsky;
using Linn.Topology;
using Upnp;
namespace KinskyWeb.Kinsky
{
    public class ViewStub : IView
    {
        public IViewWidgetSelector ViewWidgetSelectorRoom
        {
            get { return new ViewWidgetSelectorStub(); }
        }

        public IViewWidgetButton ViewWidgetButtonStandby
        {
            get { return new ViewWidgetButtonStub(); }
        }

        public IViewWidgetSelector ViewWidgetSelectorSource
        {
            get { return new ViewWidgetSelectorStub(); }
        }

        public IViewWidgetVolumeControl ViewWidgetVolumeControl
        {
            get { return new ViewWidgetVolumeControlStub(); }
        }

        public IViewWidgetMediaTime ViewWidgetMediaTime
        {
            get { return new ViewWidgetMediaTimeStub(); }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlMediaRenderer
        {
            get { return new ViewWidgetTransportControlStub(); }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlDiscPlayer
        {
            get { return new ViewWidgetTransportControlStub(); }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlRadio
        {
            get { return new ViewWidgetTransportControlStub(); }
        }

        public IViewWidgetTrack ViewWidgetTrack
        {
            get { return new ViewWidgetTrackStub(); }
        }

        public IViewWidgetPlayMode ViewWidgetPlayMode
        {
            get { return new ViewWidgetPlayModeStub(); }
        }

        public IViewWidgetPlaylist ViewWidgetPlaylist
        {
            get { return new ViewWidgetPlaylistStub(); }
        }

        public IViewWidgetPlaylistRadio ViewWidgetPlaylistRadio
        {
            get { return new ViewWidgetPlaylistRadioStub(); }
        }

        public IViewWidgetPlaylistAux ViewWidgetPlaylistAux
        {
            get { return new ViewWidgetPlaylistAuxStub(); }
        }

        public IViewWidgetPlaylistDiscPlayer ViewWidgetPlaylistDiscPlayer
        {
            get { return new ViewWidgetPlaylistDiscPlayerStub(); }
        }

        public IViewWidgetButton ViewWidgetButtonSize
        {
            get { return new ViewWidgetButtonStub(); }
        }

        public IViewWidgetButton ViewWidgetButtonView
        {
            get { return new ViewWidgetButtonStub(); }
        }

        public IViewWidgetButton ViewWidgetButtonSave
        {
            get { return new ViewWidgetButtonStub(); }
        }

        public IViewWidgetButton ViewWidgetButtonWasteBin
        {
            get { return new ViewWidgetButtonStub(); }
        }
    }

    public class ViewWidgetMediaTimeStub : IViewWidgetMediaTime
    {
        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void SetTransportState(ETransportState aTransportState)
        {
        }

        public void SetDuration(uint aDuration)
        {
        }

        public void SetSeconds(uint aSeconds)
        {
        }

        public event EventHandler<EventArgsSeekSeconds> EventSeekSeconds;
    }

    public class ViewWidgetVolumeControlStub : IViewWidgetVolumeControl
    {
        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void SetVolume(uint aVolume)
        {
        }

        public void SetMute(bool aMute)
        {
        }

        public void SetVolumeLimit(uint aVolumeLimit)
        {
        }

        public event EventHandler<EventArgs> EventVolumeIncrement;
        public event EventHandler<EventArgs> EventVolumeDecrement;
        public event EventHandler<EventArgsVolume> EventVolumeChanged;
        public event EventHandler<EventArgsMute> EventMuteChanged;
    }

    public class ViewWidgetSelectorStub : IViewWidgetSelector
    {
        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Add(string aName, object aTag)
        {
        }

        public void Remove(object aTag)
        {
        }

        public void SetSelected(object aTag)
        {
        }

        public event EventHandler<EventArgsSelection> EventSelectionChanged;
    }

    public class ViewWidgetButtonStub : IViewWidgetButton
    {
        public void Open()
        {
        }

        public void Close()
        {
        }

        public void SetEnabled(bool aEnabled)
        {
        }

        public event EventHandler<EventArgs> EventClick { add { } remove { } }
    }

    public class ViewWidgetTransportControlStub : IViewWidgetTransportControl
    {

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void SetPlayNowEnabled(bool aEnabled)
        {
        }

        public void SetPlayNextEnabled(bool aEnabled)
        {
        }

        public void SetPlayLaterEnabled(bool aEnabled)
        {
        }

        public void SetDragging(bool aDragging)
        {
        }

        public void SetTransportState(ETransportState aTransportState)
        {
        }
		
		public void SetDuration(uint aDuration)
        {
        }

        public event EventHandler<EventArgs> EventPause;
        public event EventHandler<EventArgs> EventPlay;
        public event EventHandler<EventArgs> EventStop;
        public event EventHandler<EventArgs> EventPrevious;
        public event EventHandler<EventArgs> EventNext;

        public event EventHandler<EventArgsPlay> EventPlayNow;
        public event EventHandler<EventArgsPlay> EventPlayNext;
        public event EventHandler<EventArgsPlay> EventPlayLater;
    }
    public class ViewWidgetTrackStub : IViewWidgetTrack
    {

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }
		
		public void SetItem(upnpObject aObject)
		{
		}

        public void SetNoArtwork() { }
        public void SetArtworkError() { }

        public void SetArtwork(Image aArtwork)
        {
        }

        public void SetTitle(string aTitle)
        {
        }

        public void SetArtist(string aArtist)
        {
        }

        public void SetAlbum(string aAlbum)
        {
        }

        public void SetBitrate(uint aBitrate)
        {
        }

        public void SetSampleRate(float aSampleRate)
        {
        }

        public void SetBitDepth(uint aBitDepth)
        {
        }

        public void SetCodec(string aCodec)
        {
        }

        public void SetLossless(bool aLossless)
        {
        }

        public void Update()
        {
        }

    }
    public class ViewWidgetPlayModeStub : IViewWidgetPlayMode
    {

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void SetShuffle(bool aShuffle)
        {
        }

        public void SetRepeat(bool aRepeat)
        {
        }

        public event EventHandler<EventArgs> EventToggleShuffle;

        public event EventHandler<EventArgs> EventToggleRepeat;

    }
    public class ViewWidgetPlaylistStub : IViewWidgetPlaylist
    {

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void SetPlaylist(IList<Linn.Topology.MrItem> aPlaylist)
        {
        }

        public void SetTrack(Linn.Topology.MrItem aTrack)
        {
        }

        public void Save()
        {
        }

        public void Delete()
        {
        }

        public event EventHandler<EventArgsSeekTrack> EventSeekTrack;

        public event EventHandler<EventArgsPlaylistInsert> EventPlaylistInsert;

        public event EventHandler<EventArgsPlaylistMove> EventPlaylistMove;

        public event EventHandler<EventArgsPlaylistDelete> EventPlaylistDelete;

        public event EventHandler<EventArgs> EventPlaylistDeleteAll;

    }
    public class ViewWidgetPlaylistRadioStub : IViewWidgetPlaylistRadio
    {

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void SetPresets(IList<MrItem> aPresets)
        {
        }

        public void SetChannel(Channel aChannel)
        {
        }

        public void SetPreset(int aPresetIndex)
        {
        }

        public void Save()
        {
        }

        public event EventHandler<EventArgsSetChannel> EventSetChannel;
        public event EventHandler<EventArgsSetPreset> EventSetPreset;
    }

    public class ViewWidgetPlaylistAuxStub : IViewWidgetPlaylistAux
    {

        public void Open(string aType)
        {
        }

        public void Close()
        {
        }

    }

    public class ViewWidgetPlaylistDiscPlayerStub : IViewWidgetPlaylistDiscPlayer
    {

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void Eject()
        {
        }

    }
}