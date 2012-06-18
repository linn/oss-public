using Linn.Gui.Scenegraph;
using System;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ControllerSourceAuxiliary : IDisposable
{
    public ControllerSourceAuxiliary(Node aRoot, ModelSourceAuxiliary aModel) {
        Trace.WriteLine(Trace.kLinnGui, ">ControllerSourceAuxiliary.ControllerProxySource");
        //iModel = aModel;
        iControllerPlaylist = new ControllerPlaylistPhantom();
        iControllerLibrary = new ControllerLibraryPhantom();
    }
    
    public void Dispose() {
        Trace.WriteLine(Trace.kLinnGui, ">ControllerSourceAuxiliary.Dispose");
        if(iControllerPlaylist != null) {
            iControllerPlaylist.Dispose();
            //iControllerPlaylist = null;
        }
        if(iControllerLibrary != null) {
            iControllerLibrary.Dispose();
            //iControllerLibrary = null;
        }
    }
    
    //private ModelAuxiliarySource iModel = null;
    private ControllerPlaylistPhantom iControllerPlaylist = null;
    private ControllerLibraryPhantom iControllerLibrary = null;
}
    
} // Kinsky
} // Linn
