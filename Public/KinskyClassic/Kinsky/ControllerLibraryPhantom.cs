using Linn.Gui;
using System;
using Linn.Gui.Resources;

namespace Linn {
namespace Kinsky {
    
public sealed class ControllerLibraryPhantom : IMessengerObserver, IDisposable
{
    public ControllerLibraryPhantom() {
        Trace.WriteLine(Trace.kLinnGui, ">ControllerPhantomLibrary.ControllerPhantomLibrary");
        iView = new ViewLibraryPhantom();
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        Trace.WriteLine(Trace.kLinnGui, ">ControllerPhantomLibrary.Dispose");
        Messenger.Instance.EEventAppMessage -= Receive;
        if(iView != null) {
            iView.Dispose();
            //iView = null;
        }
    }
    
    public void Receive(Message aMessage) {
        if(aMessage.Fullname == "Library.Root") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == true) {
                    iView.Disable();
                }
            }
        }
    }
    
    private ViewLibraryPhantom iView = null;
}

} // Kinsky
} // Linn
