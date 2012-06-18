using System;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn.Topology;

namespace Linn {
namespace KinskyPda {

public class ViewPlaylistMedia : IDisposable, IListableRenderer
{
    public ViewPlaylistMedia(Node aRoot, ControllerPlaylistMedia aController, ModelSourceMediaRendererLinn aModel) {
        iController = aController;
        iModel = aModel;
        
        VisitorSearch search = new VisitorSearch("Playlist.Playlist");
        iPlaylist = (NodeList)search.Search(aRoot);
        Assert.Check(iPlaylist != null);
        
        search = new VisitorSearch("CurrentTrack.Root");
        iNodeCurrentTrack = (NodeHit)search.Search(aRoot);
        iNodeCurrentTrackParent = iNodeCurrentTrack.Parent;
        Assert.Check(iNodeCurrentTrack != null);
        
        search = new VisitorSearch("CurrentTrack.CoverArt");
        iCoverArt = (NodePolygon)search.Search(aRoot);
        Assert.Check(iCoverArt != null);
        iCoverArtTextureArray = iCoverArt.NextPlugin.NextPlugin as TextureArrayFixed;
        Assert.Check(iCoverArtTextureArray != null);
        
        search = new VisitorSearch("CurrentTrack.CoverArtReflection");
        iCoverArtReflection = (NodePolygon)search.Search(aRoot);
        Assert.Check(iCoverArtReflection != null);
        
        search = new VisitorSearch("CurrentTrack.Play");
        iPlay = (NodePolygon)search.Search(aRoot);
        Assert.Check(iPlay != null);
        
        search = new VisitorSearch("CurrentTrack.TimeSlider");
        iSlider = (NodeSlider)search.Search(aRoot);
        Assert.Check(iSlider != null);
        
        iModel.EEventSubscribed += EventSubscribed;
        iModel.EEventCurrentTrackIndex += EventCurrentTrackIndex;
        iModel.EEventTrackElapsedTime += EventTrackElapsedTime;
        iModel.EEventTransportState += EventTransportState;
    }
    
    public void Dispose() {
        iModel.EEventSubscribed -= EventSubscribed;
        iModel.EEventCurrentTrackIndex -= EventCurrentTrackIndex;
        iModel.EEventTrackElapsedTime -= EventTrackElapsedTime;
        iModel.EEventTransportState -= EventTransportState;
        
        iPlaylist.ListableRenderer = new ListableRendererNull();
        iPlaylist.ListEntryProvider = new ListEntryProviderNull();
        iPlaylist.Refresh();
        
        if(iNodeCurrentTrack.Parent != iNodeCurrentTrackParent) {
            iNodeCurrentTrackParent.AddChild(iNodeCurrentTrack);
        }
    }
    
    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        /*if(aIndex == iModel.ModelPlaylist.CurrentTrackIndex) {
            return ToNodeHitHighlighted(aListable, aWidth, aHeight, aNodeFont, aIndex, aHighlighted);
        } else {*/
        int state = aHighlighted ? 1 : 0;
        if(aListable.NodeHit == null || (aListable.State != state)) {
            aListable.NodeHit = ToNodeHitUnHighlighted(aListable, aWidth, aHeight, aNodeFont, aIndex, aHighlighted);
            aListable.State = state;
        }
        return aListable.NodeHit;
        //}
    }
    
    /*private NodeHit ToNodeHitHighlighted(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        MusicTrack musicTrack = (aListable as TrackUpnp).Track as MusicTrack;

        iTrackText.Text = (aListable as TrackUpnp).Track.Title;
        if(musicTrack != null) {
            if(musicTrack.AlbumArtUri != "") {
                Trace.WriteLine(Trace.kMediaRenderer, "ViewPlaylistMedia.ToNodeHitUnHighlighted: t.AlbumArtUri=" + musicTrack.AlbumArtUri);
                iCoverArtTextureArray[0] = TextureManager.Instance.TextureByNameOrLoad(musicTrack.AlbumArtUri);
                iCoverArtTextureArray[1] = TextureManager.Instance.TextureByNameOrLoad(musicTrack.AlbumArtUri);
                ITexture reflection = TextureManager.Instance.TextureByName(musicTrack.AlbumArtUri + "Reflection");
                if(reflection == null) {
                    reflection = new TextureGdiReflection(musicTrack.AlbumArtUri + "Reflection", TextureManager.Instance.TextureByNameOrLoad(musicTrack.AlbumArtUri) as TextureGdi);
                    reflection.Refresh();
                    TextureManager.Instance.AddTexture(reflection);
                }
                iCoverArtReflection.Texture(new ReferenceTexture(reflection));
            } else {
                iCoverArtTextureArray[0] = null;
                iCoverArtTextureArray[1] = null;
                iCoverArtReflection.Texture(new ReferenceTexture());
            }
            iArtistAlbumText.Text = musicTrack.Artist + " \u2013 " + musicTrack.Album;
        } else {
            iArtistAlbumText.Text = ""; 
            iCoverArtTextureArray[0] = null;
            iCoverArtTextureArray[1] = null;
        }
        
        return ToNodeHitUnHighlighted(aListable, aWidth, aHeight, aNodeFont, aIndex, aHighlighted);//iNodeCurrentTrack;
    }*/
    
    private NodeHit ToNodeHitUnHighlighted(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        MusicTrack musicTrack = (aListable as TrackUpnp).Track as MusicTrack;
        
        NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
        hit.Width = aWidth;
        hit.Height = aHeight;
        hit.AllowHits = false;
        
        NodePolygon cover = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
        cover.Height = aHeight * 0.7f;
        cover.Width = cover.Height;
        cover.LocalTranslation.X = 30;
        cover.LocalTranslation.Y = (aHeight * 0.5f) - (cover.Height * 0.5f);
        cover.AllowHits = false;
        cover.ClampToTextureSize = false;
        if(musicTrack != null && musicTrack.AlbumArtUri != "") {
            Trace.WriteLine(Trace.kMediaRenderer, "ViewPlaylistMedia.ToNodeHitUnHighlighted: t.AlbumArtUri=" + musicTrack.AlbumArtUri);
            cover.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad(musicTrack.AlbumArtUri)));
        } else {
            cover.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/default_cover.jpg")));
        }
        
        NodeText track = (NodeText)PluginFactory.Instance.Create("NodeText");
        track.LocalTranslation.X = 80;
        track.LocalTranslation.Y = 1;
        track.LocalTranslation.Z = 1;
        track.Width = aWidth - 150;
        track.Height = 40;
        track.AllowHits = false;
        track.Bold = true;
        track.PointSize = 13;
        if(aIndex < iModel.ModelPlaylist.CurrentTrackIndex) {
            track.Colour = new Colour(255, 145, 168, 193);
        } else {
            track.Colour = aNodeFont.Colour;
        }
        track.Alignment = NodeFont.EAlignment.EA_Centre;
        track.Justification = NodeFont.EJustification.EJ_Centre;
        track.Trimming = NodeFont.ETrimming.ET_EllipsisPath;
        track.FaceName = "Arial";
        track.Text = (aListable as TrackUpnp).Track.Title;
        
        /*NodeText trackShadow = (NodeText)PluginFactory.Instance.Create("NodeText");
        trackShadow.LocalTranslation.X = track.LocalTranslation.X - 1;
        trackShadow.LocalTranslation.Y = track.LocalTranslation.Y + 1;
        trackShadow.Width = track.Width;
        trackShadow.Height = track.Height;
        trackShadow.AllowHits = track.AllowHits;
        trackShadow.Bold = track.Bold;
        trackShadow.PointSize = track.PointSize;
        trackShadow.Alignment = track.Alignment;
        trackShadow.Justification = track.Justification;
        trackShadow.Trimming = track.Trimming;
        trackShadow.FaceName = track.FaceName;
        trackShadow.Text = track.Text;*/
        
        NodeText albumArtist = (NodeText)PluginFactory.Instance.Create("NodeText");
        albumArtist.LocalTranslation.X = 80;
        albumArtist.LocalTranslation.Y = 38;
        albumArtist.LocalTranslation.Z = 1;
        albumArtist.Width = aWidth - 150;
        albumArtist.Height = 20;
        albumArtist.AllowHits = false;
        albumArtist.Bold = false;
        albumArtist.PointSize = 12;
        if(aIndex < iModel.ModelPlaylist.CurrentTrackIndex) {
            albumArtist.Colour = new Colour(255, 145, 168, 193);
        } else {
            albumArtist.Colour = aNodeFont.Colour;
        }
        albumArtist.Alignment = NodeFont.EAlignment.EA_Centre;
        albumArtist.Justification = NodeFont.EJustification.EJ_Centre;
        albumArtist.Trimming = NodeFont.ETrimming.ET_EllipsisPath;
        albumArtist.FaceName = "Arial";
        if(musicTrack != null) {
            albumArtist.Text = musicTrack.Artist + " \u2013 " + musicTrack.Album;
        } else {
            albumArtist.Text = ""; 
        }
        
        /*NodeText albumArtistShadow = (NodeText)PluginFactory.Instance.Create("NodeText");
        albumArtistShadow.LocalTranslation.X = albumArtist.LocalTranslation.X - 1;
        albumArtistShadow.LocalTranslation.Y = albumArtist.LocalTranslation.Y + 1;
        albumArtistShadow.Width = albumArtist.Width;
        albumArtistShadow.Height = albumArtist.Height;
        albumArtistShadow.AllowHits = albumArtist.AllowHits;
        albumArtistShadow.Bold = albumArtist.Bold;
        albumArtistShadow.PointSize = albumArtist.PointSize;
        albumArtistShadow.Alignment = albumArtist.Alignment;
        albumArtistShadow.Justification = albumArtist.Justification;
        albumArtistShadow.Trimming = albumArtist.Trimming;
        albumArtistShadow.FaceName = albumArtist.FaceName;
        albumArtistShadow.Text = albumArtist.Text;*/
        
        NodePolygon line = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
        line.AllowHits = false;
        line.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/line.png")));
        line.LocalTranslation.Y = aHeight - (line.Height * 0.5f);
        
        hit.AddChild(cover);
        hit.AddChild(track);
        //hit.AddChild(trackShadow);
        hit.AddChild(albumArtist);
        //hit.AddChild(albumArtistShadow);
        hit.AddChild(line);
        return hit;
    }
    
    private void EventSubscribed(object aSender) {
        if(iPlaylist.ListEntryProvider != iModel.ModelPlaylist.ListEntryProvider) {
	        iPlaylist.ListEntryProvider = iModel.ModelPlaylist.ListEntryProvider;
	        iPlaylist.ListableRenderer = this;
	        EventTrackElapsedTime();       // before track index to ensure elapsed/total time are empty if no track selected
	        EventCurrentTrackIndex();
	        EventTransportState();
	    }
    }
    
    private void EventCurrentTrackIndex() {
        iPlaylist.Refresh();
        RefreshCurrentTrack();
    }
    
    private void EventTrackElapsedTime() {
        iSlider.Position = iModel.TrackPosition;
        iSlider.Render(true);
        Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.ElapsedTime", iModel.TrackElapsedTime));
        Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.ElapsedTimeDropShadow", iModel.TrackElapsedTime));
        Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.RemainingTime", iModel.TrackTime));
        Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.RemainingTimeDropShadow", iModel.TrackTime));
        //iNodeCurrentTrack.Render();
    }
    
    private void EventTransportState() {
        if(iModel.TransportState == "Playing") {
            iPlay.Texture(new ReferenceTexture());
        } else {
            iPlay.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/play.png")));
        }
        if(iModel.TransportState == "Eop") {
            iSlider.Active = false;
        } else {
            iSlider.Active = true;
        }
    }
    
    private void RefreshCurrentTrack() {
        if(iModel.Metadata != null) {
            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.Track", iModel.Metadata.Title));
            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.TrackDropShadow", iModel.Metadata.Title));
            MusicTrack musicTrack = iModel.Metadata as MusicTrack;
            if(musicTrack != null) {
                Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.ArtistAlbum", musicTrack.Artist + " \u2013 " + musicTrack.Album));
                Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.ArtistAlbumDropShadow", musicTrack.Artist + " \u2013 " + musicTrack.Album));
	            if(musicTrack.AlbumArtUri != "") {
	                Trace.WriteLine(Trace.kMediaRenderer, "ViewPlaylistMedia.ToNodeHitUnHighlighted: t.AlbumArtUri=" + musicTrack.AlbumArtUri);
	                iCoverArtTextureArray[0] = TextureManager.Instance.TextureByNameOrLoad(musicTrack.AlbumArtUri);
	                iCoverArtTextureArray[1] = TextureManager.Instance.TextureByNameOrLoad(musicTrack.AlbumArtUri);
	                ITexture reflection = TextureManager.Instance.TextureByName(musicTrack.AlbumArtUri + "Reflection");
	                if(reflection == null) {
	                    ITexture cover = TextureManager.Instance.TextureByNameOrLoad(musicTrack.AlbumArtUri);
	                    System.Console.WriteLine(cover.Loaded());
	                    if(cover.Loaded()) {
	                       reflection = new TextureGdiReflection(musicTrack.AlbumArtUri + "Reflection", cover as TextureGdi);
	                       reflection.Refresh();
	                       TextureManager.Instance.AddTexture(reflection);
	                    }
	                }
	                iCoverArtReflection.Texture(new ReferenceTexture(reflection));
	                return;
	            }
	        } else {
	            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.ArtistAlbum", ""));
	            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.ArtistAlbumDropShadow", ""));
	        }
	    } else {
            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.Track", ""));
            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.TrackDropShadow", ""));
            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.ArtistAlbum", ""));
            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.ArtistAlbumDropShadow", ""));
            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.ElapsedTime", ""));
            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.ElapsedTimeDropShadow", ""));
            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.RemainingTime", ""));
            Messenger.Instance.PresentationMessage(new MsgSetText("CurrentTrack.RemainingTimeDropShadow", ""));
        }
        iCoverArtTextureArray[0] = null;
        iCoverArtTextureArray[1] = null;
        iCoverArtReflection.Texture(new ReferenceTexture());
    }
    
    private ControllerPlaylistMedia iController = null;
    private ModelSourceMediaRendererLinn iModel = null;
    private NodeList iPlaylist = null;
    private Node iNodeCurrentTrackParent = null;
    private NodeHit iNodeCurrentTrack = null;
    private NodePolygon iCoverArt = null;
    private NodePolygon iCoverArtReflection = null;
    private TextureArrayFixed iCoverArtTextureArray = null;
    private NodePolygon iPlay = null;
    private NodeSlider iSlider = null;
}

} // KinskyPda
} // Linn
