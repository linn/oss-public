using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using System;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public abstract class ViewSourceMediaRenderer : IDisposable
{
    public ViewSourceMediaRenderer(Node aRoot, ModelSourceMediaRenderer aModel) {
        iRoot = aRoot;
        iModel = aModel;
        
        VisitorSearch search = new VisitorSearch("CurrentPlaylist.Playlist");
        iPlaylist = (NodeList)search.Search(aRoot);
        Assert.Check(iPlaylist != null);
        
        iModel.EEventSubscribed += EventSubscribed;
        iModel.EEventUnSubscribed += EventUnSubscribed;
        iModel.EEventTransportState += EventTransportState;
    }
    
    public virtual void Dispose() {
        iModel.EEventSubscribed -= EventSubscribed;
        iModel.EEventUnSubscribed -= EventUnSubscribed;
        iModel.EEventTransportState -= EventTransportState;
        DoUnSubscribe();
    }
    
    public void InsertStarted() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.Glow", true));
        Renderer.Instance.Render();
    }
    
    public void InsertFinished() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.Glow", false));
        Renderer.Instance.Render();
    }
    
    protected void EventSubscribed(object aSender) {
        iSubscribed = true;
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlaybackControl", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.PlaybackControlEx", false));
        EventTransportState();
        Renderer.Instance.Render();
    }
    
    protected void EventUnSubscribed(object aSender) {
        DoUnSubscribe();
    }
    
    private void DoUnSubscribe() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlaybackControl", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.PlaybackControlEx", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.RepeatBlue", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.ShuffleBlue", false));
        Renderer.Instance.Render();
        iSubscribed = false;
    }
    
    protected abstract void EventTransportState();
    
    protected Node iRoot = null;
    protected ModelSourceMediaRenderer iModel = null;
    protected NodeList iPlaylist = null;
    protected bool iSubscribed = false;
}

} // Kinsky
} // Linn
