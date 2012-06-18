using System;
using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Gui.Resources;
using System.ComponentModel;
using System.Collections.Generic;
using Linn.Topology;

namespace Linn {
namespace Kinsky {

public class ControllerSourceMediaRendererUpnpAv : ControllerSourceMediaRenderer
{
    public ControllerSourceMediaRendererUpnpAv(Node aRoot, ModelSourceMediaRendererUpnpAv aModel, House aHouse) : base(aRoot, aModel) {
        iModelPlaylist = aModel.ModelPlaylist;
        iView = new ViewSourceMediaRendererUpnpAv(aRoot, this, aModel);
        iControllerLibrary = new ControllerLibrary(aRoot, aModelLibrary);
        iControllerPlaylist = new ControllerPlaylistUpnpAv(aRoot, aModel.ModelPlaylist);
        Messenger.Instance.PresentationMessage(new MsgSetFocus("CurrentPlaylist.Playlist", true));
    }
    
    public override void Dispose() {
        base.Dispose();
        Trace.WriteLine(Trace.kLinnGuiMediaRenderer, ">ControllerSourceMediaRendererUpnpAv.Dispose");
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
    }
    
    public override void Receive(Message aMessage) {
        base.Receive(aMessage);
        if(aMessage.Fullname == "ControlPanel.Play.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    //iModel.SelectSource();
                    if(iHasFocus || iControllerLibrary.ControllerMediaServer == null) {
                        if(iControllerPlaylist.TrackHighlighted != iModelPlaylist.CurrentTrackIndex) {
                            if(iControllerPlaylist.TrackHighlighted != -1 && iModel.TransportState != "PAUSED_PLAYBACK") {
                                iModel.SeekTracks(iControllerPlaylist.TrackHighlighted, "Absolute");
                            } else {
                                iModel.Play();
                            }
                        } else {
                            iModel.Play();
                        }
                    } else {
                        if(iInsertAtEnd) {
                            Insert(iModelPlaylist.TrackCount, true);
                        } else {
                            Insert((uint)(iModelPlaylist.CurrentTrackIndex + 1), true);
                        }
                    }
                }
            }
        }
        if(aMessage.Fullname == "Library.Insert.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    if(iInsertAtEnd) {
                        Insert(iModelPlaylist.TrackCount, false);
                    } else {
                        Insert((uint)(iControllerPlaylist.CurrentTrackIndex + 1), false);
                    }
                }
            }
        }
    }
    
    protected override void Select(int aIndex, ListableUpnpObject aObject) {
        Assert.Check(iControllerLibrary.ControllerMediaServer != null);
        if(iControllerLibrary.ControllerMediaServer.InsertAllowed()) {
            if(aObject.Object as Item != null) {
                if(iInsertAtEnd) {
                    Insert(iModelPlaylist.TrackCount, true);
                } else {
                    Insert((uint)(iModelPlaylist.CurrentTrackIndex + 1), true);
                }
            }
        }
    }
    
    protected override void DoInsertWork(object sender, DoWorkEventArgs args) {
        iView.InsertStarted();
        iControllerLibrary.ControllerMediaServer.SetInserting(true);
        MessageInsertObject msg = args.Argument as MessageInsertObject;
        Assert.Check(msg != null);
        List<Item> items = iControllerLibrary.ControllerMediaServer.RetrieveItems(1000, iModel.ProtocolInfo);
        if(items.Count > 0) {
            try {
                iModelPlaylist.Insert(msg.StartIndex, items);
                if(msg.PlayAfterInsert) {
                    iModel.SeekTracks((int)msg.StartIndex, "Absolute");
                }
            } catch(ControlRequest.InvokeError e) {
                System.Console.WriteLine("ControllerSourceMediaRendererLinn.DoInsertWork:\n" + e);
            }
        }
    }
    
    protected override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
        if(e.Error != null) {
            System.Console.WriteLine("ControllerSourceMediaRendererLinn.RunWorkerCompleted:\n" + e.Error.ToString());
            throw new System.Exception(e.Error.ToString());
        }
        // NOTE: issue here, if we move to a new media server whilst inserting the insert check will not be correct as its stored
        //          per server at the moment....
        ControllerMediaServer server = iControllerLibrary.ControllerMediaServer;
        if(server != null) {
            server.SetInserting(false);
        }
        iInsertWorker = null;
        iView.InsertFinished();
    }
    
    private ControllerLibrary iControllerLibrary;
    private ControllerPlaylistMediaRenderer iControllerPlaylist;
}
    
} // Kinsky
} // Linn
