using System;
using Linn.Gui.Scenegraph;
using Linn;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ViewStatusSourceSdp : ViewStatus
{
    public ViewStatusSourceSdp(Node aRoot, ModelSourceDiscPlayerSdp aModel) : base(aRoot, aModel) {
        iModel = aModel;
        
        iModel.EEventSubscribed += EventSubscribed;
        iModel.EEventUnSubscribed += EventUnSubscribed;
        iModel.EEventTrackElapsedTime += EventTrackElapsedTime;
        iModel.EEventTrack += EventTrack;
        iModel.EEventPlayState += EventPlayState;
        iModel.EEventDomain += EventDomain;
    }
    
    public override void Dispose() {
        Trace.WriteLine(Trace.kLinnGuiSdp, ">ViewSdpStatus.Dispose");
        base.Dispose();
        iModel.EEventSubscribed -= EventSubscribed;
        iModel.EEventUnSubscribed -= EventUnSubscribed;
        iModel.EEventTrackElapsedTime -= EventTrackElapsedTime;
        iModel.EEventTrack -= EventTrack;
        iModel.EEventPlayState -= EventPlayState;
        iModel.EEventDomain -= EventDomain;
        DoUnSubscribe();
    }
    
    private void EventSubscribed(object aSender) {
        iSubscribed = true;
        EventTrackElapsedTime();
        EventDiscElapsedTime();
        EventTrack();
        //Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", true));
        EventPlayState();
        //Renderer.Instance.Render();
    }
    
    private void EventUnSubscribed(object aSender) {
        DoUnSubscribe();
    }
    
    private void DoUnSubscribe() {
        iSubscribed = false;
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Album", ""));
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Track", ""));
        Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", false));
        //Renderer.Instance.Render();
    }
    
    private void EventTrackElapsedTime() {
        iSlider.Position = iModel.SeekPosition;
        iSlider.Render(false);
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.TimeText", iModel.TrackElapsedTime));
    }
    
    private void EventDiscElapsedTime() {
        iSlider.Position = iModel.SeekPosition;
        iSlider.Render(false);
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.TimeText", iModel.DiscElapsedTime));
    }
    
    private void EventTrack() {
        Trace.WriteLine(Trace.kLinnGuiSdp, "ViewSdpStatus.EventTrack: track=" + iModel.ModelPlaylist.Track);
        if(iModel.Track > -1) {
            string str = "Track ";
            if(iModel.DiscType == "DVD") {
                str = "Chapter ";
            }
            Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Track", str + (iModel.ModelPlaylist.Track + 1).ToString()));
            Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Album", ""));
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Track", ""));
            Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Album", ""));
        }
        iDefaultCoverArt.Texture(new ReferenceTexture());
        iDefaultCoverArt.Render();
    }
    
    private void EventPlayState() {
        if(iSubscribed) {
            if(iModel.PlayState == "Playing" || iModel.PlayState == "Paused") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", true));
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TimeSlider", true));
            } else if(iModel.PlayState == "Stopped") {
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TimeSlider", false));
            } else {
                Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", false));
            }
            Renderer.Instance.Render();
        }
    }
    
    private void EventDomain() {
        string domain = iModel.Domain;
        if(domain == "None") {
            EventTrack();
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Track", domain));
            Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Album", ""));
        }
    }
    
    private bool iSubscribed = false;
    private ModelSourceDiscPlayerSdp iModel;
}

} // Kinsky
} // Linn
