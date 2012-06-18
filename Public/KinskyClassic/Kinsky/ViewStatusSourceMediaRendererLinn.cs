using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public sealed class ViewStatusSourceMediaRendererLinn : ViewStatusSourceMediaRenderer
{
    public ViewStatusSourceMediaRendererLinn(Node aRoot, ControllerSourceMediaRendererLinn aController, ModelSourceMediaRendererDs aModel)
        : base(aRoot, aController, aModel) {
    }
    
    protected override void EventTransportState() {
        if(iSubscribed) {
            if(iModelSourceMediaRenderer.TransportState == "Playing" || iModelSourceMediaRenderer.TransportState == "Paused") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TimeSlider", true));
            } else if(iModelSourceMediaRenderer.TransportState == "Stopped") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TimeSlider", false));
            } else if(iModelSourceMediaRenderer.TransportState == "Eop") {
                iDefaultCoverArt.Texture(new ReferenceTexture());
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", false));
            }
            Renderer.Instance.Render();
        }
    }
}

} // Kinsky
} // Linn
