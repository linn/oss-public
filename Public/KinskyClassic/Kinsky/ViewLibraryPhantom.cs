using System;
using Linn.Gui;
using Linn.Gui.Resources;

namespace Linn {
namespace Kinsky {
    
public sealed class ViewLibraryPhantom : IDisposable
{
    public ViewLibraryPhantom() {
        Disable();
    }
    
    public void Dispose() {
    }
    
    public void Disable() {
        Trace.WriteLine(Trace.kLinnGui, ">ViewPhantomLibrary.Disable");
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.BackButton", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.Location", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.LocationStart", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.InsertAtEnd", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasList", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasScrollbarBkgrd", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasNumEntries", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.DirectoryList", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.DirectoryScrollBar", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.DirectoryNumEntries", false));
        //Renderer.Instance.Render();
    }
}
    
} // Kinsky
} // Linn
