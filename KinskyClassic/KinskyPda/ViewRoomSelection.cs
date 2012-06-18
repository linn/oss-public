using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Topology;
using System;
using Linn.Gui.Resources;

namespace Linn {
namespace KinskyPda {

public class ViewRoomSelection : IListableRenderer, IDisposable
{
    public ViewRoomSelection(Node aRoot, ModelSystem aModelSystem) {
        VisitorSearch search = new VisitorSearch("RoomSelection.RoomList");
        iRoomList = (NodeList)search.Search(aRoot);
        Assert.Check(iRoomList != null);
        
        iRoomList.ListEntryProvider = aModelSystem.RoomList;
        iRoomList.ListableRenderer = this;
        iRoomList.Refresh();
    }
    
    public void Dispose() {
    }
    
    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        int state = aHighlighted ? 1 : 0;
        if(aListable.NodeHit == null || (aListable.State != state)) {
	        NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
	        hit.Width = aWidth;
	        hit.Height = aHeight;
	        
	        /*NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
	        poly.AllowHits = false;
	        poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));*/
	        
	        ModelRoom room = aListable as ModelRoom;
	        Assert.Check(room != null);
	        NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
	        text.AllowHits = false;
	        //text.LocalTranslation.X = poly.Width;
	        text.Width = aWidth;// - poly.Width - (iStandby.Active && aHighlighted ? iStandby.Width : 0);
	        text.Height = aHeight;
	        text.Justification = aNodeFont.Justification;
	        text.Alignment = aNodeFont.Alignment;
	        text.Trimming = aNodeFont.Trimming;
	        text.CurrFont = aNodeFont.CurrFont;
	        text.Colour = aNodeFont.Colour;
	        text.Text = room.Room;
	        
	        NodePolygon line = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
	        line.AllowHits = false;
	        line.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/line.png")));
	        line.LocalTranslation.Y = aHeight - (line.Height * 0.5f);
	
	        //hit.AddChild(poly);
	        hit.AddChild(text);
	        hit.AddChild(line);
	        aListable.NodeHit = hit;
	        aListable.State = state;
	    }
        //System.Console.WriteLine("ModelRoomRenderer.ToNodeHit: " + text.Text);
        return aListable.NodeHit;
    }
    
    public void EventRoomUpdated(ModelRoom aRoom) {
        iRoomList.Refresh();
    }
    
    public void Highlight(int aIndex, ModelRoom aRoom) {
    }
    
    public void HighlightUpdated(int aIndex, ModelRoom aRoom) {
    }
    
    public void UnHighlight(ModelRoom aRoom) {
    }
    
    public void Subscribed(int aRoomIndex, ModelRoom aModelRoom) {
        if(aRoomIndex == -1) {
            Messenger.Instance.PresentationMessage(new MsgSetHighlight("RoomSelection.RoomList", aRoomIndex, NodeList.EAlignment.EA_None));
        }
    }
    
    private NodeList iRoomList = null;
}

} // KinskyPda
} // Linn
