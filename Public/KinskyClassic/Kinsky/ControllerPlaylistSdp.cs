using System;
using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Topology;

namespace Linn {
namespace Kinsky {

public class ControllerPlaylistDiscPlayer : ControllerNodeList
{
    public ControllerPlaylistDiscPlayer(Node aRoot, ModelSourceDiscPlayer aModel) : base(aRoot, "CurrentPlaylist.Playlist") {
        iModel = aModel;
        iView = new ViewPlaylistSdp(aRoot, this, aModel);
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
                    iView.DisableDelete();
                }
            }
        }
        if(aMessage.Fullname == "CurrentPlaylist.Highlight") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == true) {
                    iView.DisableDelete();
                }
            }
        }
        if(aMessage.Fullname == "CurrentPlaylist.Playlist") {
            MsgHighlight highlightMsg = aMessage as MsgHighlight;
            if(highlightMsg != null && highlightMsg.Listable != null) {
                iView.DisableDelete();
                TrackBasic track = highlightMsg.Listable as TrackBasic;
                Assert.Check(track != null);
                iTrackHighlighted = highlightMsg.Index;
                iView.TrackHighlighted = highlightMsg.Index;
            }
            MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;
            if(highlightUpdatedMsg != null) {
                TrackBasic track = highlightUpdatedMsg.Listable as TrackBasic;
                Assert.Check(track != null);
                iTrackHighlighted = highlightUpdatedMsg.Index;
                iView.TrackHighlighted = highlightUpdatedMsg.Index;
            }
            MsgUnHighlight unHighlightMsg = aMessage as MsgUnHighlight;
            if(unHighlightMsg != null) {
                iTrackHighlighted = -1;
                iView.TrackHighlighted = -1;
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
            int index = iModel.Track;
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
    
    private ModelSourceDiscPlayer iModel;
    private ViewPlaylistDiscPlayer iView;
    private int iTrackHighlighted;
}
    
} // Kinsky
} // Linn
