using Linn.Gui.Resources;
using Linn.Gui;

namespace Linn {
namespace KinskyPda {

public class ViewPreampPhantom
{
    public ViewPreampPhantom() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("VolumeControl.Enabled", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("VolumeControl.Disabled", true));
        Renderer.Instance.Render();
    }
}

} // KinskyPda
} // Linn
