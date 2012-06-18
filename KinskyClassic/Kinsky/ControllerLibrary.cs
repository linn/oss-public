using Linn.Gui.Scenegraph;
using System;
using Linn.Gui;
using Linn.Gui.Resources;
using System.Threading;
using Linn.Topology;

namespace Linn {
namespace Kinsky {

public class ControllerLibrary : ControllerNodeList
{
    public ControllerLibrary(Node aRoot, House aHouse) : base(aRoot, "Library.NasList") {
        iRoot = aRoot;
        iHouse = aHouse;
        iView = new ViewLibrary(aRoot, this, aHouse);
        iMutex = new Mutex(false);
        Messenger.Instance.EEventAppMessage += Receive;
        Messenger.Instance.PresentationMessage(new MsgSetFocus("Library.NasList", false));
    }
    
    public override void Dispose() {
        Messenger.Instance.EEventAppMessage -= Receive;
        iMutex.WaitOne();
        base.Dispose();
        iDisposing = true;
        if(iView != null) {
            iView.Dispose();
            //iView = null;
        }
        if(iControllerMediaServer != null) {
            UnsubscribeFromServer();
        }
        iMutex.ReleaseMutex();
    }
    
    public ControllerMediaServer ControllerMediaServer {
        get {
            return iControllerMediaServer;
        }
    }
    
    public override void Receive(Message aMessage) {
        iMutex.WaitOne();
        if(!iDisposing) {
            base.Receive(aMessage);
            if(aMessage.Fullname == "Library.Root") {
                MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
                if(msgActiveChanged != null) {
                    if(msgActiveChanged.NewActive == true) {
                        if(iControllerMediaServer == null) {
                            iView.Enable();
                        } else {
                            iView.Disable();
                        }
                    }
                }
            }
            if(aMessage.Fullname == "Library.NasList") {
                MsgSelect selectMsg = aMessage as MsgSelect;
                if(selectMsg != null) {
                    ModelMediaServer server = selectMsg.Listable as ModelMediaServer;
                    Assert.Check(server != null);
                    Select(selectMsg.Index, server);
                }
                MsgHighlight highlightMsg = aMessage as MsgHighlight;
                if(highlightMsg != null) {
                    if(highlightMsg.Listable != null) {
                        ModelMediaServer server = highlightMsg.Listable as ModelMediaServer;
                        Assert.Check(server != null);
                        iView.Highlight(highlightMsg.Index, server);
                    }
                }
                MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;
                if(highlightUpdatedMsg != null) {
                    ModelMediaServer server = highlightUpdatedMsg.Listable as ModelMediaServer;
                    Assert.Check(server != null);
                    iView.HighlightUpdated(highlightUpdatedMsg.Index, server);
                }
                MsgUnHighlight unHighlightMsg = aMessage as MsgUnHighlight;
                if(unHighlightMsg != null) {
                    ModelMediaServer server = unHighlightMsg.Listable as ModelMediaServer;
                    Assert.Check(server != null);
                    iView.UnHighlight(server);
                }
                MsgTopEntry topEntry = aMessage as MsgTopEntry;
                if(topEntry != null) {
                    iView.SetScrollbar(topEntry.Index);
                }
            }
            if(aMessage.Fullname == "Library.NasList") {
                MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;
                if(focusMsg != null) {
                    SetFocus(focusMsg.NewFocus);
                }
            }
            if(aMessage.Fullname == "CurrentPlaylist.Playlist") {
                MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;
                if(focusMsg != null) {
                    if(focusMsg.NewFocus) {
                        Messenger.Instance.PresentationMessage(new MsgSetFocus("Library.NasList", false));
                    }
                }
            }
            if(aMessage.Fullname == "Library.NasScrollbar") {
                MsgPositionChanged positionMsg = aMessage as MsgPositionChanged;
                if(positionMsg != null) {
                    iView.SetTopEntry(positionMsg.NewPosition);
                }
            }
        }
        iMutex.ReleaseMutex();
    }
    
    public void SetFocus(bool aFocus) {
        //iHasFocus = aFocus;
        iView.SetFocus(aFocus);
    }
    
    private void Select(int aIndex, Library aLibrary) {
        Assert.Check(iControllerMediaServer == null);
        iView.Disable();
        Renderer.Instance.Render();
        iControllerMediaServer = new ControllerMediaServer(iRoot, aLibrary);
        aLibrary.EEventDirectory += EventDirectory;
        iHouse.EEventMediaServerDeleted += EventMediaServerDeleted;
        Renderer.Instance.Render();
    }
    
    private void EventDirectory() {
        iMutex.WaitOne();
        if(!iDisposing) {
            Assert.Check(iControllerMediaServer != null);
            if(iControllerMediaServer.ModelMediaServer.DirInfo == null) {
                iView.Enable();
                Renderer.Instance.Render();
                UnsubscribeFromServer();
            }
        }
        iMutex.ReleaseMutex();
    }
    
    private void EventMediaServerDeleted(Library aLibrary) {
        iMutex.WaitOne();
        if(!iDisposing) {
            Assert.Check(iControllerMediaServer != null);
            if(iControllerMediaServer.ModelMediaServer == aLibrary) {
                iView.Enable();
                Renderer.Instance.Render();
                UnsubscribeFromServer();
            }
        }
        iMutex.ReleaseMutex();
    }
    
    private void UnsubscribeFromServer() {
        iHouse.EEventMediaServerDeleted -= EventMediaServerDeleted;
        iControllerMediaServer.ModelMediaServer.EEventDirectory -= EventDirectory;
        iControllerMediaServer.ModelMediaServer.UnSelect();
        iControllerMediaServer.Dispose();
        iControllerMediaServer = null;
    }
    
    private Node iRoot;
    private ViewLibrary iView;
    private House iHouse;
    private ControllerMediaServer iControllerMediaServer;
    private Mutex iMutex;
    private bool iDisposing = false;
    //private bool iHasFocus = false;
}
    
} // Kinsky
} // Linn
