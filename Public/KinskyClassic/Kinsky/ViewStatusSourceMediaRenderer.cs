using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public abstract class ViewStatusSourceMediaRenderer : ViewStatus
{
    public ViewStatusSourceMediaRenderer(Node aRoot, ControllerSourceMediaRenderer iController, ModelSourceMediaRenderer aModel) : base(aRoot, aModel) {
        iControllerSourceMediaRenderer = iController;
        iModelSourceMediaRenderer = aModel;
        
        iModelSourceMediaRenderer.EEventSubscribed += EventSubscribed;
        iModelSourceMediaRenderer.EEventUnSubscribed += EventUnSubscribed;
        iModelSourceMediaRenderer.EEventTrackElapsedTime += EventTrackElapsedTime;
        iModelSourceMediaRenderer.EEventCurrentTrackIndex += EventCurrentTrackIndex;
        iModelSourceMediaRenderer.EEventTransportState += EventTransportState;
    }
    
    public override void Dispose() {
        Trace.WriteLine(Trace.kLinnGuiMediaRenderer, ">ViewMediaRendererStatus.Dispose");
        base.Dispose();
        iModelSourceMediaRenderer.EEventSubscribed -= EventSubscribed;
        iModelSourceMediaRenderer.EEventUnSubscribed -= EventUnSubscribed;
        iModelSourceMediaRenderer.EEventTrackElapsedTime -= EventTrackElapsedTime;
        iModelSourceMediaRenderer.EEventCurrentTrackIndex -= EventCurrentTrackIndex;
        iModelSourceMediaRenderer.EEventTransportState -= EventTransportState;
        DoUnSubscribe();
    }
    
    private void EventSubscribed(object aSender) {
        iSubscribed = true;
        EventTrackElapsedTime();
        EventCurrentTrackIndex();
        //Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", true));
        EventTransportState();
        Renderer.Instance.Render();
    }
    
    private void EventUnSubscribed(object aSender) {
        DoUnSubscribe();
    }
    
    private void DoUnSubscribe() {
        iSubscribed = false;
        iDefaultCoverArt.Texture(new ReferenceTexture());
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Album", ""));
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Track", ""));
        Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.TrackInfo", false));
        Renderer.Instance.Render();
    }
    
    private void EventCurrentTrackIndex() {
        string albumAndArtist = "";
        string track = "";
        UpnpObject o = iModelSourceMediaRenderer.Metadata;
        if(o as MusicTrack != null) {
            MusicTrack t = o as MusicTrack;

#if TRACE || DEBUG
            if(t.AlbumArtUri != "") {
                Trace.WriteLine(Trace.kLinnGuiMediaRenderer, "ViewMediaRendererStatus.EventCurrentTrackIndex: t.AlbumArtUri=" + t.AlbumArtUri);
                iDefaultCoverArt.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad(t.AlbumArtUri)));
            } else {
#endif
                iDefaultCoverArt.Texture(new ReferenceTexture());
#if TRACE || DEBUG
            }
#endif
            
            albumAndArtist = t.Album;
            if(t.Artist != "") {
                if(albumAndArtist != "") {
                    albumAndArtist += " \u2013 ";
                }
                albumAndArtist += t.Artist;
            }
            track = t.Title;
        } else if(o as AudioItem != null) {
            AudioItem a = o as AudioItem;
            track = a.Title;
            
            iDefaultCoverArt.Texture(new ReferenceTexture());
        }
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Album", albumAndArtist));
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.Track", track));
        iDefaultCoverArt.Render();
    }
    
    private void EventTrackElapsedTime() {
        if(!iControllerSourceMediaRenderer.Seeking) {
            Messenger.Instance.PresentationMessage(new MsgSetPosition("StatusBar.TimeSlider", iModelSourceMediaRenderer.TrackPosition));
        }
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.TimeText", iModelSourceMediaRenderer.TrackElapsedTime));
    }
    
    protected abstract void EventTransportState();
    
    protected bool iSubscribed = false;
    protected ControllerSourceMediaRenderer iControllerSourceMediaRenderer = null;
    protected ModelSourceMediaRenderer iModelSourceMediaRenderer = null;
}

} // Kinsky
} // Linn
