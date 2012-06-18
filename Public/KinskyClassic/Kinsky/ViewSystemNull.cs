using System;
using Linn.Gui;
using Linn.Gui.Resources;

namespace Linn {
namespace Kinsky {

public class ViewSystemNull : IDisposable
{
    public ViewSystemNull() {
        //Messenger.Instance.PresentationMessage(new MsgSetText("RoomSelection.Number", "0 Rooms"));
        //Messenger.Instance.PresentationMessage(new MsgSetText("SourceSelection.Number", "0 Sources"));
    }
    
    public void Dispose() {
    }
    
    public void Enable() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("RoomSelection.RoomScrollbar", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("SourceSelection.SourceScrollbar", false));
    }
}

} // Kinsky
} // Linn
