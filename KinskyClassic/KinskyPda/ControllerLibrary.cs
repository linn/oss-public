using System;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using Linn.Topology;
using System.Threading;

namespace Linn {
namespace KinskyPda {

public class ControllerLibrary : IMessengerObserver, IDisposable
{
    public ControllerLibrary(Node aRoot, ModelLibrary aModelLibrary) {
        iRoot = aRoot;
        iModelLibrary = aModelLibrary;
        iDisposeMutex = new Mutex(false);
        iViewLibrary = new ViewLibrary(aRoot, aModelLibrary);
        iModelLibrary.EEventMediaServerDeleted += EventMediaServerDeleted;
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        iDisposeMutex.WaitOne();
        Messenger.Instance.EEventAppMessage -= Receive;
        iModelLibrary.EEventMediaServerDeleted -= EventMediaServerDeleted;
        
        if(iControllerMediaServer != null) {
            iControllerMediaServer.ModelMediaServer.UnSubscribe();
            iControllerMediaServer.ModelMediaServer.EEventDirectory -= EventDirectory;
            iControllerMediaServer.Dispose();
        }
        if(iViewLibrary != null) {
            iViewLibrary.Dispose();
        }
        iDisposed = true;
        iDisposeMutex.ReleaseMutex();
    }
    
    public void Receive(Message aMessage) {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        if(aMessage.Fullname == "Library.Root") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == true) {
                    iViewLibrary.SetEnabled(iControllerMediaServer == null);
                }
            }
        }
        if(aMessage.Fullname == "Library.LibraryList") {
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
                    iViewLibrary.Highlight(highlightMsg.Index, server);
                }
            }
            MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;
            if(highlightUpdatedMsg != null) {
                ModelMediaServer server = highlightUpdatedMsg.Listable as ModelMediaServer;
                Assert.Check(server != null);
                iViewLibrary.HighlightUpdated(highlightUpdatedMsg.Index, server);
            }
            MsgUnHighlight unHighlightMsg = aMessage as MsgUnHighlight;
            if(unHighlightMsg != null) {
                ModelMediaServer server = unHighlightMsg.Listable as ModelMediaServer;
                Assert.Check(server != null);
                iViewLibrary.UnHighlight(server);
            }
        }
    }
    
    public ModelSource ModelSource {
        get {
            return iModelSource;
        }
        set {
            iDisposeMutex.WaitOne();
            if (iDisposed)
            {
                iDisposeMutex.ReleaseMutex();
                return;
            }
            iDisposeMutex.ReleaseMutex();
            iModelSource = value;
            if(iControllerMediaServer != null) {
                iControllerMediaServer.ModelSourceMediaRenderer = value as ModelSourceMediaRenderer;
            }
        }
    }
    
    private void Select(int aIndex, ModelMediaServer aModelMediaServer) {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        Assert.Check(iControllerMediaServer == null);
        iViewLibrary.SetEnabled(false);
        iControllerMediaServer = new ControllerMediaServer(iRoot, aModelMediaServer);
        iControllerMediaServer.ModelSourceMediaRenderer = iModelSource as ModelSourceMediaRenderer;
        aModelMediaServer.EEventDirectory += EventDirectory;
    }
    
    private void EventMediaServerDeleted(ModelMediaServer aModelMediaServer) {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        Assert.Check(iControllerMediaServer != null);
        if(iControllerMediaServer.ModelMediaServer == aModelMediaServer) {
            iControllerMediaServer.ModelMediaServer.UnSubscribe();
            iControllerMediaServer.ModelMediaServer.EEventDirectory -= EventDirectory;
            iControllerMediaServer.Dispose();
            iControllerMediaServer = null;
            iViewLibrary.SetEnabled(true);
        }
    }
    
    private void EventDirectory() {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        Assert.Check(iControllerMediaServer != null);
        if(iControllerMediaServer.ModelMediaServer.DirInfo == null) {
            iControllerMediaServer.ModelMediaServer.UnSubscribe();
            iControllerMediaServer.ModelMediaServer.EEventDirectory -= EventDirectory;
            iControllerMediaServer.Dispose();
            iControllerMediaServer = null;
            iViewLibrary.SetEnabled(true);
        }
    }
    
    
    private Mutex iDisposeMutex = null;
    private bool iDisposed = false;
    private Node iRoot = null;
    private ModelLibrary iModelLibrary = null;
    private ViewLibrary iViewLibrary = null;
    private ControllerMediaServer iControllerMediaServer = null;
    private ModelSource iModelSource = null;
}

} // KinskyPda
} // Linn
