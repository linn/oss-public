using System;

namespace Linn {
namespace Kinsky {
    
public class ControllerNullSource : IDisposable
{
    public ControllerNullSource() {
        iControllerPlaylist = new ControllerPlaylistPhantom();
        iControllerLibrary = new ControllerLibraryPhantom();
    }
    
    public void Dispose() {
        if(iControllerPlaylist != null) {
            iControllerPlaylist.Dispose();
            //iControllerPlaylist = null;
        }
        if(iControllerLibrary != null) {
            iControllerLibrary.Dispose();
            //iControllerLibrary = null;
        }
    }
    
    private ControllerPlaylistPhantom iControllerPlaylist = null;
    private ControllerLibraryPhantom iControllerLibrary = null;
}

} // Kinsky
} // Linn
