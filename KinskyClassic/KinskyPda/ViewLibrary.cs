using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using System;
using Linn.Topology;

namespace Linn {
namespace KinskyPda {

public class ViewLibrary : IListableRenderer, IDisposable 
{
    public ViewLibrary(Node aRoot, ModelLibrary aModel) {
        iRoot = aRoot;
        iModel = aModel;
        
        VisitorSearch search = new VisitorSearch("Library.LibraryList");
        iLibraryList = (NodeList)search.Search(iRoot);
        Assert.Check(iLibraryList != null);
        
        iLibraryList.ListEntryProvider = aModel.ServerList;
        iLibraryList.ListableRenderer = this;
        iLibraryList.Refresh();
    }
    
    public void Dispose() {
    }
    
    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        if(aListable.NodeHit == null) {
	        NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
	        hit.Width = aWidth;
	        hit.Height = aHeight;
	        
	        /*NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
	        poly.AllowHits = false;
	        poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));*/
	        
	        ModelMediaServer server = aListable as ModelMediaServer;
	        Assert.Check(server != null);
	        NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
	        text.AllowHits = false;
	        //text.LocalTranslation.X = poly.Width;
	        text.Width = aWidth; //- poly.Width;
	        text.Height = aHeight;
	        text.Justification = aNodeFont.Justification;
	        text.Alignment = aNodeFont.Alignment;
	        text.Trimming = aNodeFont.Trimming;
	        text.CurrFont = aNodeFont.CurrFont;
	        text.Colour = aNodeFont.Colour;
	        text.Text = server.Name;
	        
	        NodePolygon line = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
	        line.AllowHits = false;
	        line.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/line.png")));
	        line.LocalTranslation.Y = aHeight - (line.Height * 0.5f);
	        
	        //hit.AddChild(poly);
	        hit.AddChild(text);
	        hit.AddChild(line);
	        aListable.NodeHit = hit;
	    }
        //System.Console.WriteLine("ModelSourceRenderer.ToNodeHit: " + text.Text);
        return aListable.NodeHit;
    }
    
    public void SetEnabled(bool aEnabled) {
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.LibraryList", aEnabled));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.ContainerList", !aEnabled));
    }
    
    public void Highlight(int aIndex, ModelMediaServer aServer) {
    }
    
    public void HighlightUpdated(int aIndex, ModelMediaServer aServer) {
    }
    
    public void UnHighlight(ModelMediaServer aServer) {
    }
    
    private Node iRoot = null;
    private ModelLibrary iModel = null;
    private NodeList iLibraryList = null;
}

} // KinskyPda
} // Linn
