using System;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn.Topology;

namespace Linn {
namespace Kinsky {

public class ViewPreampPhantom : IDisposable
{
    public ViewPreampPhantom() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.VolumeInfo", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.VolumeControl", false));
        Renderer.Instance.Render();
    }
    
    public void Dispose() {}
}

} // Kinsky
} // Linn
