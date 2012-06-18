using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn;
using Linn.Gui;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ViewSourceMediaRendererLinn : ViewSourceMediaRenderer
{
    public ViewSourceMediaRendererLinn(Node aRoot, ControllerSourceMediaRendererLinn aController, ModelSourceMediaRenderer aModel) : base(aRoot, aModel) {
        //iController = aController;
    }
    
    protected override void EventTransportState() {
        if(iSubscribed) {
            Trace.WriteLine(Trace.kLinnGuiMediaRenderer, ">ViewSourceMediaRendererLinn.EventTransportState: aValue=" + iModel.TransportState);
            if(iModel.TransportState == "Playing") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", false));
            } else if(iModel.TransportState == "Paused") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", false));
            } else if(iModel.TransportState == "Stopped") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", true));
            } else if(iModel.TransportState == "Eop") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", false));
            }
            Renderer.Instance.Render();
        }
    }
    
    //private ControllerSourceMediaRendererLinn iController = null;
}

} // Kinsky
} // Linn
