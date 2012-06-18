using System;
using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Gui.Resources;
using System.Threading;
using System.Drawing;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ViewPlaylistMediaRenderer : IListableRenderer, IDisposable
{
    public ViewPlaylistMediaRenderer(Node aRoot, ControllerPlaylistMediaRenderer aController, ModelSourceMediaRenderer aModel) {
        iRoot = aRoot;
        iController = aController;
        iModel = aModel;
        
        VisitorSearch search = new VisitorSearch("CurrentPlaylist.RemoveButton");
        iRemoveButton = (NodePolygon)search.Search(aRoot);
        Assert.Check(iRemoveButton != null);
        
        search = new VisitorSearch("StatusBar.Room");
        NodePolygon roomButton = (NodePolygon)search.Search(aRoot);
        Assert.Check(roomButton != null);
        iRoomState = roomButton.NextPlugin as Bistable;
        Assert.Check(iRoomState != null);
        
        search = new VisitorSearch("CurrentPlaylist.Playlist");
        iPlaylist = (NodeList)search.Search(iRoot);
        Assert.Check(iPlaylist != null);
        
        iModel.EEventSubscribed += EventSubscribed;
        iModel.EEventCurrentTrackIndex += EventCurrentTrackIndex;
        iModel.EEventTrackCount += EventTrackCount;
    }
    
    public void Dispose() {
        iModel.EEventSubscribed -= EventSubscribed;
        iModel.EEventCurrentTrackIndex -= EventCurrentTrackIndex;
        iModel.EEventTrackCount -= EventTrackCount;
        
        iPlaylist.ListableRenderer = new ListableRendererNull();
        iPlaylist.ListEntryProvider = new ListEntryProviderNull();
        iPlaylist.Refresh();
        Messenger.Instance.PresentationMessage(new MsgSetText("CurrentPlaylist.NumEntries", ""));
    }
    
    public void Enable() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.DeleteAll", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.NumEntries", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.CurrentPlaylist_Text", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.Playlist", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.Glow", false));
        UpdateScrollbar();
        Renderer.Instance.Render();
    }
    
    public void SetFocus(bool aFocus) {
        iHasFocus = aFocus;
        SetStatusText();
    }
        
    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
        hit.Width = aWidth;
        hit.Height = aHeight;
        
        NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
        poly.AllowHits = false;
        poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));
        
        TrackUpnp track = aListable as TrackUpnp;
        Assert.Check(track != null);
        NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
        text.AllowHits = false;
        text.LocalTranslation.X = poly.Width;
        text.Width = aWidth - poly.Width - (iRemoveButton.Active && aHighlighted ? iRemoveButton.Width : 0);
        text.Height = aHeight;
        text.Justification = aNodeFont.Justification;
        text.Alignment = aNodeFont.Alignment;
        text.Trimming = aNodeFont.Trimming;
        text.CurrFont = aNodeFont.CurrFont;
        if(aIndex < iModel.CurrentTrackIndex) {
            text.Colour = new Colour(255, 145, 168, 193);
        } else {
            text.Colour = aNodeFont.Colour;
        }
        text.Text = track.Track.Title;
        
        hit.AddChild(poly);
        hit.AddChild(text);
        //System.Console.WriteLine("TrackUpnpRenderer.ToNodeHit: " + text.Text);
        return hit;
    }
    
    public void UpdateScrollbar() {
        if(iModel.TrackCount <= iPlaylist.LineCount) {
            Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.Scrollbar", false));
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.Scrollbar", true));
        }
    }
    
    public void SetScrollbar(uint aPosition) {
        int rangeMax = (int)iModel.ListEntryProvider.Count - (int)iPlaylist.LineCount;
        if(rangeMax > 0) {
            SetScrollbar(aPosition / (float)rangeMax);
        } else {
            SetScrollbar(0.0f);
        }
    }
    
    public void SetScrollbar(float aPosition) {
        UpdateScrollbar();
        Messenger.Instance.PresentationMessage(new MsgSetPosition("CurrentPlaylist.Scrollbar", aPosition));
    }
    
    public void SetTopEntry(float aPosition) {
        uint index = 0;
        if((int)iModel.ListEntryProvider.Count - (int)iPlaylist.LineCount >= 0) {
            index = (uint)Math.Round((iModel.ListEntryProvider.Count - iPlaylist.LineCount) * aPosition);
        }
        Messenger.Instance.PresentationMessage(new MsgSetTopEntry("CurrentPlaylist.Playlist", index));
    }
    
    public void HighlightCurrentTrack(int aTrackIndex) {
        Trace.WriteLine(Trace.kLinnGuiMediaRenderer, "ViewPlaylistMedia.HighlightCurrentTrack: aTrackIndex=" + aTrackIndex);
        Messenger.Instance.PresentationMessage(new MsgSetHighlight("CurrentPlaylist.Playlist", aTrackIndex, NodeList.EAlignment.EA_Centre));
    }
    
    public int TrackHighlighted {
        get{
            return iTrackHighlighted;
        }
        set {
            iTrackHighlighted = value;
            SetScrollbar(iPlaylist.TopEntry);
            SetStatusText();
            //Renderer.Instance.Render();
        }
    }
    
    private void EventSubscribed(object aSender) {
        iPlaylist.ListEntryProvider = iModel.ListEntryProvider;
        iPlaylist.ListableRenderer = this;
        iPlaylist.Refresh();
        if(!iRoomState.State) {
            Enable();
        }
        if(iPlaylist.Focused) {
            iController.HighlightCurrentTrack();
        }
        EventTrackCount();
        EventCurrentTrackIndex();
    }
    
    private void EventCurrentTrackIndex() {
        SetStatusText();
        iPlaylist.Refresh();
    }
    
    private void EventTrackCount() {
        SetScrollbar(iPlaylist.TopEntry);
        SetStatusText();
        //Renderer.Instance.Render();
    }
    
    private void SetStatusText() {
        string text = "";
        uint count = iModel.TrackCount;
        if(iTrackHighlighted != -1 && iHasFocus) {
            text = (iTrackHighlighted + 1).ToString() + " of ";
        }
        text += count.ToString() + " " + kStatusText + (count == 1 ? "" : "s");
        int index = iModel.CurrentTrackIndex;
        if(index > -1) {
            text += " (current track " + (index + 1) + ")";
        }
        Messenger.Instance.PresentationMessage(new MsgSetText("CurrentPlaylist.NumEntries", text));
    }
    
    private static const string kStatusText = "track";
    private Node iRoot;
    private ControllerPlaylistMediaRenderer iController;
    private ModelSourceMediaRenderer iModel;
    private NodeList iPlaylist;
    private NodePolygon iRemoveButton;
    private Bistable iRoomState;
    private int iTrackHighlighted = -1;
    private bool iHasFocus = true;
}

} // Kinsky
} // Linn
