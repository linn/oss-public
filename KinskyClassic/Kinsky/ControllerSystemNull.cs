using System;
using Linn.Gui;
using Linn.Gui.Resources;
using System.Threading;

namespace Linn {
namespace Kinsky {

public class ControllerSystemNull : IMessengerObserver, IDisposable
{
    public ControllerSystemNull() {
        iControllerSource = new ControllerNullSource();
        iView = new ViewSystemNull();
        iDisposeMutex = new Mutex(false);
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        Messenger.Instance.EEventAppMessage -= Receive;
        iDisposeMutex.WaitOne();
        iDisposing = true;
        if(iControllerSource != null) {
            iControllerSource.Dispose();
            //iControllerSource = null;
        }
        if(iView != null) {
            iView.Dispose();
            //iView = null;
        }
        iDisposeMutex.ReleaseMutex();
    }
    
    public void Receive(Message aMessage) {
        iDisposeMutex.WaitOne();
        if(!iDisposing) {
            if(aMessage.Fullname == "RoomSelection.Root" || aMessage.Fullname == "SourceSelection.Root") {
                MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
                if(msgActiveChanged != null) {
                    if(msgActiveChanged.NewActive == true) {
                        iView.Enable();
                    }
                }
            }
        }
        iDisposeMutex.ReleaseMutex();
    }
    
    private bool iDisposing = false; 
    private IDisposable iControllerSource = null;
    private ViewSystemNull iView = null;
    private Mutex iDisposeMutex = null;
}

} // Kinsky
} // Linn
