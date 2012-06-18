using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System;
using System.Drawing;
using System.Windows.Forms;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ViewSourceDiscPlayer : IDisposable
{
    public ViewSourceDiscPlayer(Node aRoot, ControllerSourceSdp aController, ModelSourceDiscPlayer aModel) {
        iModel = aModel;
        //iController = aController;

        VisitorSearch search = new VisitorSearch("CurrentPlaylist.Playlist");
        iPlaylist = (NodeList)search.Search(aRoot);
        Assert.Check(iPlaylist != null);
        
        iModel.EEventSubscribed += EventSubscribed;
        iModel.EEventUnSubscribed += EventUnSubscribed;
        iModel.EEventPlayState += EventPlayState;
        iModel.EEventRepeatMode += EventRepeatMode;
        iModel.EEventProgramMode += EventProgramMode;
    }
    
    public void Dispose() {
        iModel.EEventSubscribed -= EventSubscribed;
        iModel.EEventUnSubscribed -= EventUnSubscribed;
        iModel.EEventPlayState -= EventPlayState;
        iModel.EEventRepeatMode -= EventRepeatMode;
        iModel.EEventProgramMode -= EventProgramMode;
        DoUnSubscribe();
    }
    
    public void DisableShuffleRepeat() {
        Trace.WriteLine(Trace.kLinnGuiSdp, "ViewSourceSdp.DisableShuffleRepeat: " + iPlaylist.Active);
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.PlaybackControlEx", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.RepeatBlue", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.ShuffleBlue", false));
    }
    
    public void EnableShuffleRepeat() {
        Trace.WriteLine(Trace.kLinnGuiSdp, "ViewSourceSdp.EnableShuffleRepeat: " + iPlaylist.Active);
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.PlaybackControlEx", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.RepeatBlue", iModel.RepeatAll));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.ShuffleBlue", iModel.Shuffle));
    }
    
    private void EventSubscribed(object aSender) {
        iSubscribed = true;
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlaybackControl", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Main.PlaybackControlEx", iPlaylist.Active));
        EventPlayState();
        EventRepeatMode();
        EventProgramMode();
        Renderer.Instance.Render();
    }
    
    private void EventUnSubscribed(object aSender) {
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
    
    private void EventPlayState() {
        if(iSubscribed) {
            if(iModel.PlayState == "Playing") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", false));
            } else if(iModel.PlayState == "Paused" || iModel.PlayState == "Suspended") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", false));
            } else if(iModel.PlayState == "Stopped") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", true));
            } else {
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PlayBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.PauseBlue", false));
                Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.StopBlue", false));
            }
            Renderer.Instance.Render();
        }
    }
    
    private void EventRepeatMode() {
        if(iSubscribed) {
            Trace.WriteLine(Trace.kLinnGuiSdp, "ViewSourceSdp.EventRepeatMode: " + iPlaylist.Active);
            Messenger.Instance.PresentationMessage(new MsgSetActive("Main.RepeatBlue", iModel.RepeatAll && iPlaylist.Active));
            Renderer.Instance.Render();
        }
    }
    
    private void EventProgramMode() {
        if(iSubscribed) {
            Messenger.Instance.PresentationMessage(new MsgSetActive("Main.ShuffleBlue", iModel.Shuffle && iPlaylist.Active));
            Renderer.Instance.Render();
        }
    }

    private ModelSourceDiscPlayer iModel;
    //private ControllerSourceSdp iController = null;
    private NodeList iPlaylist;
    private bool iSubscribed = false;
}
    
} // Kinsky
} // Linn
