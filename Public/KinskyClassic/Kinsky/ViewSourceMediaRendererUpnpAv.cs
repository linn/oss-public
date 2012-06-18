using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ViewSourceMediaRendererUpnpAv : ViewSourceMediaRenderer
{
    public ViewSourceMediaRendererUpnpAv(Node aRoot, ControllerSourceMediaRendererUpnpAv aController, ModelSourceMediaRenderer aModel) : base(aRoot, aModel) {
        //iController = aController;
    }
    
    protected override void EventTransportState() {
        if(iSubscribed) {
            Trace.WriteLine(Trace.kLinnGuiMediaRenderer, ">ViewSourceMediaRendererUpnpAv.EventTransportState: aValue=" + iModel.TransportState);
            if(iModel.TransportState == "PLAYING") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", false));
            } else if(iModel.TransportState == "PAUSED_PLAYBACK") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", false));
            } else if(iModel.TransportState == "STOPPED") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", true));
            } else if(iModel.TransportState == "NO_MEDIA_PRESENT") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", false));
            }
            Renderer.Instance.Render();
        }
    }
    
    //private ControllerMediaRendererUpnpAv iController = null;
}

} // Kinsky
} // Linn
