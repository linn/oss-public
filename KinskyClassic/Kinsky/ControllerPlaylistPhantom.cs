using Linn.Gui;
using System;
using Linn.Gui.Resources;

namespace Linn {
namespace Kinsky {
    
public class ControllerPlaylistPhantom : IMessengerObserver, IDisposable
{
    public ControllerPlaylistPhantom() {
        Trace.WriteLine(Trace.kLinnGui, ">ControllerPlaylistPhantom.ControllerPhantomPlaylist");
        iView = new ViewPlaylistPhantom();
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        Trace.WriteLine(Trace.kLinnGui, ">ControllerPlaylistPhantom.Dispose");
        Messenger.Instance.EEventAppMessage -= Receive;
        if(iView != null) {
            iView.Dispose();
            //iView = null;
        }
    }
    
    public void Receive(Message aMessage) {
        if(aMessage.Fullname == "CurrentPlaylist.Root") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == true) {
                    iView.Disable();
                }
            }
        }
    }
    
    private ViewPlaylistPhantom iView = null;
}
    
} // Kinsky
} // Linn
