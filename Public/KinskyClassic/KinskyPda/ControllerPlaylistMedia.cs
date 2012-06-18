using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn.Topology;
using System;

namespace Linn {
namespace KinskyPda {

public class ControllerPlaylistMedia : IDisposable, IMessengerObserver
{
    public ControllerPlaylistMedia(Node aRoot, ModelSourceMediaRendererLinn aModel) {
        iModel = aModel;
        iView = new ViewPlaylistMedia(aRoot, this, aModel);

        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        Messenger.Instance.EEventAppMessage -= Receive;
        
        if(iView != null) {
            iView.Dispose();
        }
    }
    
    public void Receive(Message aMessage) {
        if(aMessage.Fullname == "Playlist.Playlist") {
            MsgSelect selectMsg = aMessage as MsgSelect;
            if(selectMsg != null) {
                TrackUpnp track = selectMsg.Listable as TrackUpnp;
                Assert.Check(track != null);
                iModel.SeekTracks(selectMsg.Index, "Absolute");
            }
        }
        if(aMessage.Fullname == "CurrentTrack.CoverArt.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    if(iModel.TransportState == "Playing") {
                        iModel.Pause();
                    } else {
                        iModel.Play();
                    }
                }
            }
        }
        if(aMessage.Fullname == "CurrentTrack.TimeSlider") {
            if(aMessage as MsgPositionChanged != null) {
                MsgPositionChanged msgPosition = aMessage as MsgPositionChanged;
                if(msgPosition.NewPosition != iModel.TrackPosition) { // we only want to ignore response messages from events
                    System.Console.WriteLine("ControllerPlaylistMedia.Receive: msgPosition.NewPosition=" + msgPosition.NewPosition);
                    iModel.Seek(msgPosition.NewPosition);
                }
            }
        }
        if(aMessage.Fullname == "StatusBar.DeletePlaylist.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    iModel.ModelPlaylist.PlaylistDeleteAll();
                }
            }
        }
        if(aMessage.Fullname == "Playlist.DeleteTrack.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    iModel.ModelPlaylist.PlaylistDelete((uint)iHighlightIndex, 1);
                }
            }
        }
        if(aMessage.Fullname == "Playlist.Playlist") {
            MsgHighlight highlightMsg = aMessage as MsgHighlight;
            if(highlightMsg != null && highlightMsg.Listable != null) {
                TrackUpnp track = highlightMsg.Listable as TrackUpnp;
                Assert.Check(track != null);
                iHighlightIndex = highlightMsg.Index;
            }
            MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;
            if(highlightUpdatedMsg != null) {
                TrackUpnp track = highlightUpdatedMsg.Listable as TrackUpnp;
                Assert.Check(track != null);
                iHighlightIndex = highlightUpdatedMsg.Index;
            }
            MsgUnHighlight unHighlightMsg = aMessage as MsgUnHighlight;
            if(unHighlightMsg != null) {
                iHighlightIndex = -1;
            }
        }
    }
    
    public int CurrentTrackIndex {
        get {
            return iModel.ModelPlaylist.CurrentTrackIndex;
        }
    }
    
    private ModelSourceMediaRendererLinn iModel = null;
    private ViewPlaylistMedia iView = null;
    private int iHighlightIndex = -1;
}

} // KinskyPda
} // Linn
