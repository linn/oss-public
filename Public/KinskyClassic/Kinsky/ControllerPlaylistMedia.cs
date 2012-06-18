using System;
using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ControllerPlaylistMediaRenderer : ControllerNodeList
{
    public ControllerPlaylistMediaRenderer(Node aRoot, ModelSourceMediaRenderer aModel) : base(aRoot, "CurrentPlaylist.Playlist") {
        iModel = aModel;
        iView = new ViewPlaylistMedia(aRoot, this, aModel);
        HighlightCurrentTrack();
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public override void Dispose() {
        base.Dispose();
        Messenger.Instance.EEventAppMessage -= Receive;
        if(iView != null) {
            iView.Dispose();
            //iView = null;
        }
    }
    
    public override void Receive(Message aMessage) {
        base.Receive(aMessage);
        if(aMessage.Fullname == "CurrentPlaylist.Root") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == true) {
                    iView.UpdateScrollbar();
                }
            }
        }
        if(aMessage.Fullname == "CurrentPlaylist.DeleteAll.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    iModel.PlaylistDeleteAll();
                }
            }
        }
        if(aMessage.Fullname == "CurrentPlaylist.RemoveButton.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    int highlight = iView.TrackHighlighted;
                    iModel.PlaylistDelete((uint)highlight, 1);
                }
            }
        }
        if(aMessage.Fullname == "CurrentPlaylist.Playlist") {
            MsgHighlight highlightMsg = aMessage as MsgHighlight;
            if(highlightMsg != null && highlightMsg.Listable != null) {
                TrackUpnp track = highlightMsg.Listable as TrackUpnp;
                Assert.Check(track != null);
                iTrackHighlighted = highlightMsg.Index;
                iView.TrackHighlighted = highlightMsg.Index;
            }
            MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;
            if(highlightUpdatedMsg != null) {
                TrackUpnp track = highlightUpdatedMsg.Listable as TrackUpnp;
                Assert.Check(track != null);
                iTrackHighlighted = highlightUpdatedMsg.Index;
                iView.TrackHighlighted = highlightUpdatedMsg.Index;
            }
            MsgUnHighlight unHighlightMsg = aMessage as MsgUnHighlight;
            if(unHighlightMsg != null) {
                iTrackHighlighted = -1;
                iView.TrackHighlighted = -1;
            }
            MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;
            if(focusMsg != null) {
                iView.SetFocus(focusMsg.NewFocus);
            }
            MsgTopEntry topEntry = aMessage as MsgTopEntry;
            if(topEntry != null) {
                iView.SetScrollbar(topEntry.Index);
            }
        }
        if(aMessage.Fullname == "CurrentPlaylist.Scrollbar") {
            MsgPositionChanged positionMsg = aMessage as MsgPositionChanged;
            if(positionMsg != null) {
                iView.SetTopEntry(positionMsg.NewPosition);
            }
        }
        if(aMessage.Fullname == "StatusBar.Track") {
            MsgHit hitMsg = aMessage as MsgHit;
            if(hitMsg != null) {
                HighlightCurrentTrack();
            }
        }
    }
    
    public void HighlightCurrentTrack() {
        if(iView != null) {
            int index = iModel.CurrentTrackIndex;
            iView.HighlightCurrentTrack(index);
            iTrackHighlighted = index;
            iView.TrackHighlighted = index;
        }
    }
    
    public int TrackHighlighted {
        get {
            return iTrackHighlighted;
        }
    }
    
    public int CurrentTrackIndex {
        get {
            return iModel.CurrentTrackIndex;
        }
    }
    
    private ModelSourceMediaRenderer iModel;
    private ViewPlaylistMediaRenderer iView;
    private int iTrackHighlighted = -1;
}

} // Kinsky
} // Linn
