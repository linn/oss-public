using System;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Topology;

namespace Linn {
namespace KinskyPda {

public class ViewPreamp : IDisposable
{
    public ViewPreamp(Node aRoot, IModelPreamp aModelPreamp) {
        iModelPreamp = aModelPreamp;
        iModelPreamp.EEventMute += EventMute;
        Messenger.Instance.PresentationMessage(new MsgSetActive("VolumeControl.Enabled", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("VolumeControl.Disabled", false));
        Renderer.Instance.Render();
    }
    
    public void Dispose() {
        iModelPreamp.EEventMute -= EventMute;
    }
    
    private void EventMute() {
        //Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.MuteBlue", Convert.ToBoolean(iModel.Mute)));
        //Renderer.Instance.Render();
    }
    
    private IModelPreamp iModelPreamp = null;
}

} // KinskyPda
} // Linn
