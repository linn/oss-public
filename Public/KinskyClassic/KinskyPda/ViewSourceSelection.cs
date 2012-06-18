using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Topology;
using System;
using System.Threading;
using Linn.Gui.Resources;

namespace Linn {
namespace KinskyPda {

public class ViewSourceSelection : IListableRenderer, IDisposable
{
    public ViewSourceSelection(Node aRoot) {
        VisitorSearch search = new VisitorSearch("SourceSelection.SourceList");
        iSourceList = (NodeList)search.Search(aRoot);
        Assert.Check(iSourceList != null);
        
        iSourceListMutex = new Mutex(false);
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
	        
	        ModelRoomSource source = aListable as ModelRoomSource;
	        Assert.Check(source != null);
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
	        text.Text = source.Name;
	        
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
    
    public void SetSourceList(ModelRoom aModelRoom) {
        iSourceListMutex.WaitOne();
        if(aModelRoom != null) {
            iSourceList.ListEntryProvider = aModelRoom.SourceList;
            iSourceList.ListableRenderer = this;
        } else {
            iSourceList.ListableRenderer = new ListableRendererNull();
            iSourceList.ListEntryProvider = new ListEntryProviderNull();
        }
        iSourceListMutex.ReleaseMutex();
        iSourceList.Refresh();
    }
    
    public void EventSourceUpdated(ModelRoomSource aSource) {
        iSourceList.Refresh();
    }
    
    public void Highlight(int aIndex, ModelSource aSource) {
    }
    
    public void HighlightUpdated(int aIndex, ModelSource aSource) {
    }
    
    public void UnHighlight(ModelSource aSource) {
    }
    
    public void Select(int aIndex, ModelSource aSource) {
    }
    
    public void Subscribed(int aSourceIndex, ModelRoomSource aModelRoomSource) {
        iSourceListMutex.WaitOne();
        ModelRoomSource roomSource = null;
        if(aSourceIndex != -1) {
            roomSource = iSourceList.ListEntryProvider.Entries((uint)aSourceIndex, 1)[0] as ModelRoomSource;
        }
        if(aSourceIndex == -1 || roomSource == aModelRoomSource) {
            Messenger.Instance.PresentationMessage(new MsgSetHighlight("SourceSelection.SourceList", aSourceIndex, NodeList.EAlignment.EA_None));
        }
        iSourceListMutex.ReleaseMutex();
    }
    
    private Mutex iSourceListMutex = null;
    private NodeList iSourceList = null;
}

} // KinskyPda
} // Linn
