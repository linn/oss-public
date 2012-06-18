using System;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn.Kinsky;
using Linn.Topology;
using System.Threading;
using Linn.Control.Upnp.ControlPoint;

namespace Linn {
namespace KinskyPda {

public class ControllerRoomSource : IMessengerObserver, IDisposable
{
    public ControllerRoomSource(Node aRoot, ModelSystem aModelSystem) {
        iRoot = aRoot;
        iDisposeMutex = new Mutex(false);
        iMutexSelectionChanged = new Mutex(false);
        iSelection = new RoomSourceSelection();
        iSelection.EEventSelectionChanged += EventSelectionChanged;
        
        iModelSystem = aModelSystem;
        iViewRoom = new ViewRoomSelection(aRoot, iModelSystem);
        iViewSource = new ViewSourceSelection(aRoot);
        iControllerLibrary = new ControllerLibrary(aRoot, iModelSystem.ModelLibrary);
        
        iModelSystem.EEventRoomAdded += EventRoomAdded;
        iModelSystem.EEventRoomUpdated += EventRoomUpdated;
        iModelSystem.EEventRoomDeleted += EventRoomDeleted;
        iModelSystem.EEventSourceAdded += EventSourceAdded;
        iModelSystem.EEventSourceUpdated += EventSourceUpdated;
        iModelSystem.EEventSourceDeleted += EventSourceDeleted;
        
        iModelSystem.EEventSourceAdded += iSelection.EventSourceAdded;
        iModelSystem.EEventSourceUpdated += iSelection.EventSourceUpdated;
        iModelSystem.EEventSourceDeleted += iSelection.EventSourceDeleted;
        iModelSystem.EEventRoomUpdated += iSelection.EventRoomUpdated;
        iModelSystem.EEventRoomDeleted += iSelection.EventRoomDeleted;
        
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        iDisposeMutex.WaitOne();
        Messenger.Instance.EEventAppMessage -= Receive;
        
        iModelSystem.EEventRoomAdded -= EventRoomAdded;
        iModelSystem.EEventRoomUpdated -= EventRoomUpdated;
        iModelSystem.EEventSourceAdded -= EventSourceAdded;
        iModelSystem.EEventSourceUpdated -= EventSourceUpdated;
        
        iModelSystem.EEventSourceAdded -= iSelection.EventSourceAdded;
        iModelSystem.EEventSourceUpdated -= iSelection.EventSourceUpdated;
        iModelSystem.EEventSourceDeleted -= iSelection.EventSourceDeleted;
        iModelSystem.EEventRoomUpdated -= iSelection.EventRoomUpdated;
        iModelSystem.EEventRoomDeleted -= iSelection.EventRoomDeleted;

        if(iSelection != null) {
            iSelection.EEventSelectionChanged -= EventSelectionChanged;
            iSelection.Dispose();
        }
        if(iControllerLibrary != null) {
            iControllerLibrary.Dispose();
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
        if(aMessage.Fullname == "RoomSelection.RoomList") {
            MsgHighlight highlightMsg = aMessage as MsgHighlight;
            if(highlightMsg != null) {
                ModelRoom room = highlightMsg.Listable as ModelRoom;
                if(highlightMsg.Listable != null) {
                    Assert.Check(room != null);
                    iViewRoom.Highlight(highlightMsg.Index, room);
                    iViewSource.SetSourceList(room);
                    iSelection.SetPendingRoom(highlightMsg.Index, room);
                } else {
                    iSelection.SetPendingRoom(highlightMsg.Index, room);
                }
            }
            MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;
            if(highlightUpdatedMsg != null) {
                ModelRoom room = highlightUpdatedMsg.Listable as ModelRoom;
                Assert.Check(room != null);
                iViewRoom.HighlightUpdated(highlightUpdatedMsg.Index, room);
                iSelection.UpdateRoomIndex(highlightUpdatedMsg.Index);
            }
            MsgUnHighlight unHighlightMsg = aMessage as MsgUnHighlight;
            if(unHighlightMsg != null) {
                ModelRoom room = unHighlightMsg.Listable as ModelRoom;
                Assert.Check(room != null);
                iViewRoom.UnHighlight(room);
                iViewSource.SetSourceList(null);
            }
        }
        if(aMessage.Fullname == "SourceSelection.SourceList") {
            MsgSelect selectMsg = aMessage as MsgSelect;
            if(selectMsg != null) {
                ModelRoomSource source = selectMsg.Listable as ModelRoomSource;
                Assert.Check(source != null);
                iViewSource.Select(selectMsg.Index, source.ModelDeviceSource.ModelSource);
            }
            MsgHighlight highlightMsg = aMessage as MsgHighlight;
            if(highlightMsg != null) {
                if(highlightMsg.Listable != null) {
                    ModelRoomSource source = highlightMsg.Listable as ModelRoomSource;
                    Assert.Check(source != null);
                    iViewSource.Highlight(highlightMsg.Index, source.ModelDeviceSource.ModelSource);
                    iSelection.SetPendingSource(highlightMsg.Index, source, true);
                } else {
                    iSelection.AutoSelectSource();
                }
            }
            MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;
            if(highlightUpdatedMsg != null) {
                ModelRoomSource source = highlightUpdatedMsg.Listable as ModelRoomSource;
                Assert.Check(source != null);
                iViewSource.HighlightUpdated(highlightUpdatedMsg.Index, source.ModelDeviceSource.ModelSource);
                iSelection.UpdateSourceIndex(highlightUpdatedMsg.Index);
            }
            MsgUnHighlight unHighlightMsg = aMessage as MsgUnHighlight;
            if(unHighlightMsg != null) {
                ModelRoomSource source = unHighlightMsg.Listable as ModelRoomSource;
                Assert.Check(source != null);
                iViewSource.UnHighlight(source.ModelDeviceSource.ModelSource);
            }
        }
    }
    
    private void EventSelectionChanged() {
        Trace.WriteLine(Trace.kLinnGui, ">ControllerSystem.EventSelectionChanged");
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        try { 
            iMutexSelectionChanged.WaitOne();
            ModelRoom room = null;
            ModelRoomSource roomSource = null;
            iSelection.GetRoomAndSource(ref iRoomIndex, ref room, ref iSourceIndex, ref roomSource);
            Trace.WriteLine(Trace.kLinnGui, "ControllerSystem.EventSelectionChanged: " + iRoomIndex + " " + ((room == null) ? "" : room.Room) + " " + iSourceIndex + " " + ((roomSource == null) ? "" : roomSource.Name));
            iViewRoom.Subscribed(iRoomIndex, room);
            iViewSource.Subscribed(iSourceIndex, roomSource);
            IModelPreamp preamp = (room == null) ? null : room.ModelDevicePreamp.ModelPreamp;
            if(room != iModelRoom || preamp != iModelPreamp) {
                if(iControllerRoom != null) {
                    Trace.WriteLine(Trace.kLinnGui, "ControllerSystem.EventSelectionChanged: disposing of " + iControllerRoom);
                    iControllerRoom.Dispose();
                    iControllerRoom = null;
                }
                if(preamp != null) {
                    if(preamp as ModelPreampLinn != null || preamp as ModelPreampUpnpAv != null) {
                        ControllerPreamp controller = new ControllerPreamp(iRoot, preamp, room);
                        controller.Standby = false;
                        iControllerRoom = controller;
                    }
                    if(preamp as ModelPreampPhantom != null) {
                        ControllerPreampPhantom controller = new ControllerPreampPhantom(preamp, room);
                        controller.Standby = false;
                        iControllerRoom = controller;
                    }
                } else {
                    iControllerRoom = new ControllerPreampNull();
                }
                Trace.WriteLine(Trace.kLinnGui, "ControllerSystem.EventSelectionChanged: created " + iControllerRoom);
                iModelRoom = room;
                iModelPreamp = preamp;
            }
            ModelSource source = (roomSource == null) ? null : roomSource.ModelDeviceSource.ModelSource;
            if(iModelRoomSource != roomSource || source != iModelSource) {
                if(iControllerSource != null) {
                    Trace.WriteLine(Trace.kLinnGui, "ControllerSystem.EventSelectionChanged: disposing of " + iControllerSource);
                    iControllerSource.Dispose();
                    iControllerSource = null;
                }
                if(source != null) {
                    /*if(source as ModelSourceAuxiliary != null) {
                        ModelSourceAuxiliary auxSource = source as ModelSourceAuxiliary;
                        iControllerSource = new ControllerSourceAuxiliary(iRoot, auxSource);
                        iViewSourceStatus = new ViewStatusSourceAuxiliary(iRoot, auxSource);
                    }
                    if(source as ModelSourceSdp != null) {
                        ModelSourceSdp sdp = source as ModelSourceSdp;
                        iControllerSource = new ControllerSourceSdp(iRoot, sdp);
                        iViewSourceStatus = new ViewStatusSourceSdp(iRoot, sdp);
                    }*/
                    if(source as ModelSourceMediaRendererLinn != null) {
                        ModelSourceMediaRendererLinn mediaRendererLinn = source as ModelSourceMediaRendererLinn;
                        iControllerSource = new ControllerPlaylistMedia(iRoot, mediaRendererLinn);
                    }
                    /*if(source as ModelSourceMediaRendererUpnpAv != null) {
                        ModelSourceMediaRendererUpnpAv mediaRendererUpnpAv = source as ModelSourceMediaRendererUpnpAv;
                        iControllerSource = new ControllerSourceMediaRendererUpnpAv(iRoot, mediaRendererUpnpAv, iModel.ModelLibrary);
                        iViewSourceStatus = new ViewStatusSourceMediaRendererUpnpAv(iRoot, mediaRendererUpnpAv);
                    }*/
                }/* else {
                    iControllerSource = new ControllerNullSource();
                }*/
                Trace.WriteLine(Trace.kLinnGui, "ControllerSystem.EventSelectionChanged: created " + iControllerSource);
                iModelRoomSource = roomSource;
                iModelSource = source;
                iControllerLibrary.ModelSource = source;
            }
        } finally {
            iMutexSelectionChanged.ReleaseMutex();
        }
        Trace.WriteLine(Trace.kLinnGui, "<ControllerSystem.EventSelectionChanged");
    }
    
    private void EventRoomAdded(ModelRoom aRoom) {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        try {
            aRoom.CreateModel();
        } catch(ControlRequest.InvokeFailed e) {
            System.Console.WriteLine("ControllerRoomSourceSelection.EventRoomAdded:\n" + e);
        }
    }
    
    private void EventRoomUpdated(ModelRoom aRoom) {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        try {
            aRoom.CreateModel();
        } catch(ControlRequest.InvokeFailed e) {
            System.Console.WriteLine("ControllerRoomSourceSelection.EventRoomUpdated:\n" + e);
        }
        iViewRoom.EventRoomUpdated(aRoom);
    }
    
    private void EventRoomDeleted(ModelRoom aRoom) {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        iMutexSelectionChanged.WaitOne();
        iMutexSelectionChanged.ReleaseMutex();
    }
    
    private void EventSourceAdded(ModelRoomSource aSource) {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        try {
            aSource.CreateModel();
        } catch(ControlRequest.InvokeFailed e) {
            System.Console.WriteLine("ControllerRoomSourceSelection.EventSourceAdded:\n" + e);
        }
    }
    
    private void EventSourceUpdated(ModelRoomSource aSource) {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        try {
            aSource.CreateModel();
        } catch(ControlRequest.InvokeFailed e) {
            System.Console.WriteLine("ControllerRoomSourceSelection.EventSourceUpdated:\n" + e);
        }
    }
    
    private void EventSourceDeleted(ModelRoomSource aSource) {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        iMutexSelectionChanged.WaitOne();
        iMutexSelectionChanged.ReleaseMutex();
        
    }
    
    private Mutex iDisposeMutex = null;
    private Mutex iMutexSelectionChanged = null;
    private bool iDisposed = false;
    private Node iRoot = null;
    
    private ModelSystem iModelSystem = null;
    private RoomSourceSelection iSelection = null;
    private ViewRoomSelection iViewRoom = null;
    private ViewSourceSelection iViewSource = null;
    private ControllerLibrary iControllerLibrary = null;
    
    private ModelRoom iModelRoom = null;
    private IModelPreamp iModelPreamp = null;
    private int iRoomIndex = -1;
    private IDisposable iControllerRoom = null;
    
    private ModelRoomSource iModelRoomSource = null;
    private ModelSource iModelSource = null;
    private int iSourceIndex = -1;
    private IDisposable iControllerSource = null;
    //private ViewStatus iViewSourceStatus = null;
}

} // KinskyPda
} // Linn
