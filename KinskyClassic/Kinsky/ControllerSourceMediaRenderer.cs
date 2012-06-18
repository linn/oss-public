using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Gui.Resources;
using System;
using System.ComponentModel;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public abstract class ControllerSourceMediaRenderer : IMessengerObserver, IDisposable
{
    public ControllerSourceMediaRenderer(Node aRoot, ModelSourceMediaRenderer aModel) {
        iKeyBindings = new KeyBindings();

        iModel = aModel;
        
        VisitorSearch search = new VisitorSearch("CurrentPlaylist.Playlist");
        iPlaylist = (NodeList)search.Search(aRoot);
        Assert.Check(iPlaylist != null);
        
        search = new VisitorSearch("Library.InsertAtEnd");
        NodePolygon insertAtEnd = (NodePolygon)search.Search(aRoot);
        Assert.Check(insertAtEnd != null);
        Bistable bistable = insertAtEnd.NextPlugin as Bistable;
        Assert.Check(bistable != null);
        iInsertAtEnd = bistable.State;
        
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public virtual void Dispose() {
        Trace.WriteLine(Trace.kLinnGuiMediaRenderer, ">ControllerSourceMediaRenderer.Dispose");
        Messenger.Instance.EEventAppMessage -= Receive;
        if(iKeyBindings != null) {
            iKeyBindings.Dispose();
            //iKeyBindings = null;
        }
    }
    
    public virtual void Receive(Message aMessage) {
        if(aMessage.Fullname == "CurrentPlaylist.Glow") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == false) {
                    if(iInsertWorker != null) {
                        iView.InsertStarted();
                    }
                }
            }
        }
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
                    Messenger.Instance.PresentationMessage(new MsgSetFocus("CurrentPlaylist.Playlist", true));
                    iModel.Next();
                }
            }
        }
        if(aMessage.Fullname == "ControlPanel.Pause.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    Messenger.Instance.PresentationMessage(new MsgSetFocus("CurrentPlaylist.Playlist", true));
                    iModel.Pause();
                }
            }
        }
        if(aMessage.Fullname == "ControlPanel.Stop.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    Messenger.Instance.PresentationMessage(new MsgSetFocus("CurrentPlaylist.Playlist", true));
                    iModel.Stop();
                }
            }
        }
        if(aMessage.Fullname == "StatusBar.TimeSliderInput") {
            if(aMessage as MsgHit != null) {
                iSeeking = true;        // user has click the seekbar process the next slider position message
            }
        }
        if(aMessage.Fullname == "StatusBar.TimeSlider") {
            if(aMessage as MsgPositionChanged != null) {
                MsgPositionChanged msgPosition = aMessage as MsgPositionChanged;
                if(iSeeking) {
                    Trace.WriteLine(Trace.kLinnGuiMediaRenderer, "ControllerSourceMediaRenderer.Receive: SEEKING -> msgPosition.NewPosition=" + msgPosition.NewPosition);
                    //iModel.Seek(msgPosition.NewPosition);
                    iSeeking = false;
                }
            }
        }
        /*
        if(aMessage.Fullname == "CurrentPlaylist.Playlist") {
            MsgSelect selectMsg = aMessage as MsgSelect;
            if(selectMsg != null) {
                TrackUpnp track = selectMsg.Listable as TrackUpnp;
                Assert.Check(track != null);
                //iModel.SelectSource();
                iModel.SeekTracks(selectMsg.Index, "Absolute");
            }
            MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;
            if(focusMsg != null) {
                iHasFocus = focusMsg.NewFocus;
            }
        }
        */
        if(aMessage.Fullname == "Library.DirectoryList") {
            MsgSelect selectMsg = aMessage as MsgSelect;
            if(selectMsg != null) {
                ListableUpnpObject obj = selectMsg.Listable as ListableUpnpObject;
                Assert.Check(obj != null);
                Select(selectMsg.Index, obj);
            }
        }
        if(aMessage.Fullname == "Library.DirectoryList") {
            MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;
            if(focusMsg != null) {
                if(focusMsg.NewFocus) {
                    Messenger.Instance.PresentationMessage(new MsgSetFocus("CurrentPlaylist.Playlist", false));
                }
            }
        }
        if(aMessage.Fullname == "Library.NasList") {
            MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;
            if(focusMsg != null) {
                if(focusMsg.NewFocus) {
                    Messenger.Instance.PresentationMessage(new MsgSetFocus("CurrentPlaylist.Playlist", false));
                }
            }
        }
        if(aMessage.Fullname == "StatusBar.Track") {
            MsgHit hitMsg = aMessage as MsgHit;
            if(hitMsg != null) {
                Messenger.Instance.PresentationMessage(new MsgSetFocus("CurrentPlaylist.Playlist", true));
            }
        }
        /*if(aMessage.Fullname == "CurrentPlaylist.Root") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == true) {
                    if(iInsertWorker != null) {
                        iView.InsertStarted();
                    }
                }
            }
        }*/
        if(aMessage.Fullname == "Library.InsertAtEnd.Bistable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                iInsertAtEnd = stateMsg.NewState;
            }
        }
        if(aMessage.Fullname == "") {
            MsgKeyDown key = aMessage as MsgKeyDown;
            if(key != null) {
                string action = iKeyBindings.Action(key.Key);
                if(action == "SwapFocus" && iPlaylist.Active) {
                    bool focus = iHasFocus;
                    Messenger.Instance.PresentationMessage(new MsgSetFocus("CurrentPlaylist.Playlist", !focus));
                    Messenger.Instance.PresentationMessage(new MsgSetFocus("Library.NasList", focus));
                    Messenger.Instance.PresentationMessage(new MsgSetFocus("Library.DirectoryList", focus));
                }
            }
        }
    }
    
    public bool Seeking {
        get {
            return iSeeking;
        }
    }
    
    protected void Insert(uint aStartIndex, bool aPlayAfterInsert) {
        if(iInsertWorker == null) {
            iInsertWorker = new BackgroundWorker();
            iInsertWorker.DoWork += DoInsertWork;
            iInsertWorker.RunWorkerCompleted += RunWorkerCompleted;
            iInsertWorker.RunWorkerAsync(new MessageInsertObject(aStartIndex, aPlayAfterInsert));
        } else {
            System.Console.WriteLine("ControllerSourceMediaRendererLinn.Insert: Command ignored - already inserting!");
        }
    }
    
    protected abstract void Select(int aIndex, ListableUpnpObject aObject);
    protected abstract void DoInsertWork(object sender, DoWorkEventArgs args);
    protected abstract void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e);
    
    protected ModelSourceMediaRenderer iModel = null;
    protected ViewSourceMediaRenderer iView = null;
    protected bool iInsertAtEnd = true;
    protected BackgroundWorker iInsertWorker = null;
    protected bool iHasFocus = true;
    private KeyBindings iKeyBindings = null;
    private NodeList iPlaylist = null;
    private bool iSeeking = false;
}
    
} // Kinsky
} // Linn
