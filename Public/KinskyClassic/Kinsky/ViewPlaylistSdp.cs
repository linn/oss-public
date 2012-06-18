using System;
using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Gui.Resources;
using System.Collections.Generic;
using System.Drawing;
using Linn.Topology;

namespace Linn {
namespace Kinsky {

public class ViewPlaylistDiscPlayer : IListableRenderer, IDisposable
{
    public ViewPlaylistDiscPlayer(Node aRoot, ControllerPlaylistDiscPlayer aController, ModelSourceDiscPlayer aModel) {
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
        iPlaylist = (NodeList)search.Search(aRoot);
        Assert.Check(iPlaylist != null);
        
        iModel.EEventSubscribed += EventSubscribed;
        iModel.EEventTrack += EventTrack;
        iModel.EEventTotalTracks += EventTotalTracks;
    }
    
    public void Dispose() {
        iModel.EEventSubscribed -= EventSubscribed;
        iModel.EEventTrack -= EventTrack;
        iModel.EEventTotalTracks -= EventTotalTracks;
        
        iPlaylist.ListableRenderer = new ListableRendererNull();
        iPlaylist.ListEntryProvider = new ListEntryProviderNull();
        Messenger.Instance.PresentationMessage(new MsgSetText("CurrentPlaylist.NumEntries", ""));
    }
    
    public void Enable() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.DeleteAll", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.NumEntries", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.CurrentPlaylist_Text", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.Playlist", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.Glow", false));
        UpdateScrollbar();
        //Renderer.Instance.Render();
    }
    
    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex,  bool aHighlighted) {
        NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
        hit.Width = aWidth;
        hit.Height = aHeight;
        
        NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
        poly.AllowHits = false;
        poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));
        
        TrackBasic track = aListable as TrackBasic;
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
        if(aIndex < iModel.Track) {
            text.Colour = new Colour(255, 145, 168, 193);
        } else {
            text.Colour = aNodeFont.Colour;
        }
        text.Text = track.Track;
        
        hit.AddChild(poly);
        hit.AddChild(text);
        //System.Console.WriteLine("TrackBasicRenderer.ToNodeHit: " + text.Text);
        return hit;
    }
    
    public void DisableDelete() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.RemoveButton", false));
    }
    
    public void UpdateScrollbar() {
        if(iModel.ListEntryProvider.Count <= iPlaylist.LineCount) {
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
        EventTotalTracks();
        EventTrack();
        if(iPlaylist.Focused) {
            iController.HighlightCurrentTrack();
        }
    }
    
    private void EventTrack() {
        Trace.WriteLine(Trace.kLinnGuiSdp, "ViewSdpPlaylist.EventTrack: Track=" + iModel.Track);
        SetStatusText();
        iPlaylist.Refresh();
    }
    
    private void EventTotalTracks() {
        Trace.WriteLine(Trace.kLinnGuiSdp, "ViewSdpPlaylist.EventTotalTracks: TotalTracks=" + iModel.TotalTracks);
        SetScrollbar(iPlaylist.TopEntry);
        SetStatusText();
        Renderer.Instance.Render();
    }
    
    private void SetStatusText() {
        string text = "";
        uint count = iModel.ListEntryProvider.Count;
        if(iTrackHighlighted != -1 /*&& iPlaylist.HighlightActive*/) {
            text = (iTrackHighlighted + 1).ToString() + " of ";
        }
        text += count.ToString() + " " + (iModel.DiscType == "DVD" ? kStatusDvdText : kStatusCdText) + (count == 1 ? "" : "s");
        int index = iModel.Track;
        if(index > -1) {
            text += " (current " + (iModel.DiscType == "DVD" ? kStatusDvdText : kStatusCdText) + " " + (index + 1) + ")";
        }
        Messenger.Instance.PresentationMessage(new MsgSetText("CurrentPlaylist.NumEntries", text));
    }
    
    private const string kStatusCdText = "track";
    private const string kStatusDvdText = "chapter";
    private ControllerPlaylistDiscPlayer iController;
    private ModelSourceDiscPlayer iModel;
    private NodeList iPlaylist;
    private NodePolygon iRemoveButton;
    private Bistable iRoomState;
    private int iTrackHighlighted = -1;
}
    
} // Kinsky
} // Linn
