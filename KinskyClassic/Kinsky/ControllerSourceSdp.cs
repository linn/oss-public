using System;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ControllerSourceSdp : IDisposable
{
    public ControllerSourceSdp(Node aRoot, ModelSourceDiscPlayerSdp aModel) {
        Trace.WriteLine(Trace.kLinnGuiSdp, ">ControllerSourceSdp.ControllerSourceSdp");
        iModel = aModel;
        iView = new ViewSourceSdp(aRoot, this, aModel);
        iControllerLibrary = new ControllerLibraryPhantom();
        iControllerPlaylist = new ControllerPlaylistSdp(aRoot, aModel.ModelPlaylist);
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        Trace.WriteLine(Trace.kLinnGuiSdp, ">ControllerSourceSdp.Dispose");
        Messenger.Instance.EEventAppMessage -= Receive;
        if(iView != null) {
            iView.Dispose();
            //iView = null;
        }
        if(iControllerLibrary != null) {
            iControllerLibrary.Dispose();
            //iControllerLibrary = null;
        }
        if(iControllerPlaylist != null) {
            iControllerPlaylist.Dispose();
            //iControllerPlaylist = null;
        }
        Trace.WriteLine(Trace.kLinnGuiSdp, "<ControllerSourceSdp.Dispose");
    }
    
    public void Receive(Message aMessage) {
        if(aMessage.Fullname == "ControlPanel.Skip Backward.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    iModel.Previous();
                }
            }
        }
        if(aMessage.Fullname == "ControlPanel.Skip Forward.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    iModel.Next();
                }
            }
        }
        if(aMessage.Fullname == "ControlPanel.Play.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    //iModel.SelectSource();
                    Trace.WriteLine(Trace.kLinnGuiSdp, "ControllerSourceSdp.Receive: iControllerPlaylist.TrackHighlighted=" + iControllerPlaylist.TrackHighlighted + ", iModel.Track=" + iModel.Track);
                    if(iControllerPlaylist.TrackHighlighted != iModel.Track) {
                        if(iControllerPlaylist.TrackHighlighted != -1 && iModel.PlayState != "Paused") {
                            iModel.Track = iControllerPlaylist.TrackHighlighted;
                        }
                        iModel.Play();
                    } else {
                        iModel.Play();
                    }
                }
            }
        }
        if(aMessage.Fullname == "ControlPanel.Pause.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    iModel.Pause();
                }
            }
        }
        if(aMessage.Fullname == "ControlPanel.Stop.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    iModel.Stop();
                }
            }
        }
        if(aMessage.Fullname == "Main.Shuffle.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    iModel.Shuffle = !iModel.Shuffle;
                }
            }
        }
        if(aMessage.Fullname == "Main.Repeat.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    iModel.RepeatAll = !iModel.RepeatAll;
                }
            }
        }
        if(aMessage.Fullname == "StatusBar.TimeSlider") {
            MsgPositionChanged positionMsg = aMessage as MsgPositionChanged;
            if(positionMsg != null) {
                if(positionMsg.NewPosition != iModel.SeekPosition) { // we only want to ignore response messages from events
                    iModel.Seek(positionMsg.NewPosition);
                }
            }
        }
        if(aMessage.Fullname == "CurrentPlaylist.Playlist") {
            MsgSelect selectMsg = aMessage as MsgSelect;
            if(selectMsg != null) {
                TrackBasic track = selectMsg.Listable as TrackBasic;
                Assert.Check(track != null);
                //iModel.SelectSource();
                iModel.Track = selectMsg.Index;
                iModel.Play();
            }
        }
        if(aMessage.Fullname == "CurrentPlaylist.DeleteAll.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    iModel.Eject();
                }
            }
        }
        if(aMessage.Fullname == "CurrentPlaylist.Playlist") {
            MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;
            if(activeMsg != null) {
                if(activeMsg.NewActive) {
                    iView.EnableShuffleRepeat();
                } else {
                    iView.DisableShuffleRepeat();
                }
            }
        }
    }
    
    private ModelSourceDiscPlayer iModel;
    private ViewSourceDiscPlayer iView;
    private ControllerLibraryPhantom iControllerLibrary;
    private ControllerPlaylistDiscPlayer iControllerPlaylist;
}

} // Kinsky
} // Linn
