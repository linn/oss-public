using Linn.Gui;
using Linn.Gui.Resources;
using System;

namespace Linn {
namespace Kinsky {
    
public class ViewPlaylistPhantom : IDisposable
{
    public ViewPlaylistPhantom() {
        Disable();
    }
    
    public void Dispose() {
    }
    
    public void Disable() {
        Trace.WriteLine(Trace.kLinnGui, ">ViewPhantomPlaylist.Disable");
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.DeleteAll", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.NumEntries", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.CurrentPlaylist_Text", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.Playlist", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.Glow", false));
        //Renderer.Instance.Render();
    }
}
    
} // Kinsky
} // Linn
