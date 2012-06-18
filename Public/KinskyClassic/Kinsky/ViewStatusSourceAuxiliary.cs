using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Topology;

namespace Linn {
namespace Kinsky {

public class ViewStatusSourceAuxiliary : ViewStatus
{
    public ViewStatusSourceAuxiliary(Node aRoot, ModelSourceAuxiliary aModel) : base(aRoot, aModel) {
        Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", false));
        Renderer.Instance.Render();
    }
}

} // Kinsky
} // Linn
