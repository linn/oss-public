using System;
using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Topology;
using System.Threading;
using System.Collections.Generic;

namespace Linn {
namespace KinskyPda {

public class ControllerPreampPhantom : IMessengerObserver, IDisposable
{
    public ControllerPreampPhantom(IModelPreamp aModelPreamp, ModelRoom aModelRoom) {
        iModelPreamp = aModelPreamp;
        iModelRoom = aModelRoom;
        iViewPreamp = new ViewPreampPhantom();
        iDisposeMutex = new Mutex(false);
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        iDisposeMutex.WaitOne();
        Messenger.Instance.EEventAppMessage -= Receive;
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
        /*if(aMessage.Fullname == "RoomSelection.StandByButton.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    Standby = true;
                }
            }
        }*/
    }
    
    public bool Standby {
        set {
            iDisposeMutex.WaitOne();
            if (iDisposed)
            {
                iDisposeMutex.ReleaseMutex();
                return;
            }
            iDisposeMutex.ReleaseMutex();
            Trace.WriteLine(Trace.kPreamp, "ControllerPreampPhantom.Standby.set: " + value);
            iModelPreamp.Standby = value;
            List<IListable> listables = iModelRoom.SourceList.Entries(0, iModelRoom.SourceList.Count);
            foreach(IListable listable in listables) {
                ModelRoomSource source = listable as ModelRoomSource;
                if(source != null) {
                    source.ModelDeviceSource.ModelSource.Standby = value;
                }
            }
        }
    }
    
    private Mutex iDisposeMutex = null;
    private bool iDisposed = false;
    private ModelRoom iModelRoom = null;
    private IModelPreamp iModelPreamp = null;
    private ViewPreampPhantom iViewPreamp = null;
}

} // KinskyPda
} // Linn
