using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public sealed class ViewStatusSourceMediaRendererUpnpAv : ViewStatusSourceMediaRenderer
{
    public ViewStatusSourceMediaRendererUpnpAv(Node aRoot, ControllerSourceMediaRendererUpnpAv aController, ModelSourceMediaRendererUpnpAv aModel)
        : base(aRoot, aController, aModel) {
    }
    
    protected override void EventTransportState() {
        if(iSubscribed) {
            if(iModelSourceMediaRenderer.TransportState == "PLAYING" || iModelSourceMediaRenderer.TransportState == "PAUSED") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TimeSlider", true));
            } else if(iModelSourceMediaRenderer.TransportState == "STOPPED") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TimeSlider", false));
            } else if(iModelSourceMediaRenderer.TransportState == "NO_MEDIA_PRESENT") {
                iDefaultCoverArt.Texture(new ReferenceTexture());
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", false));
            }
            Renderer.Instance.Render();
        }
    }
}

} // Kinsky
} // Linn
