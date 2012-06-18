using System;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn.Topology;
using System.Threading;

namespace Linn {
namespace KinskyPda {

public class ViewMediaServer : IListableRenderer, IDisposable
{
    public ViewMediaServer(Node aRoot, ModelMediaServer aModelMediaServer) {
        iModelMediaServer = aModelMediaServer;
        
        VisitorSearch search = new VisitorSearch("Library.Insert");
        iInsert = (NodePolygon)search.Search(aRoot);
        Assert.Check(iInsert != null);
        search = new VisitorSearch("Library.ContainerList");
        iContainerList = (NodeList)search.Search(aRoot);
        iContainerList.ListEntryProvider = iModelMediaServer.ListEntryProvider;
        iContainerList.ListableRenderer = this;
        iContainerList.Refresh();
        iContainerList.Focused = false;
        Assert.Check(iContainerList != null);
        iDisposedMutex = new Mutex(false);
        
        iModelMediaServer.EEventSubscribed += EventSubscribed;
        iModelMediaServer.EEventDirectory += EventDirectory;
        iModelMediaServer.EEventContainerUpdated += EventContainerUpdated;
    }
    
    public void Dispose() {
        iDisposedMutex.WaitOne();
        iModelMediaServer.EEventSubscribed -= EventSubscribed;
        iModelMediaServer.EEventDirectory -= EventDirectory;
        iModelMediaServer.EEventContainerUpdated -= EventContainerUpdated;
        
        iContainerList.ListableRenderer = new ListableRendererNull();
        iContainerList.ListEntryProvider = new ListEntryProviderNull();
        iContainerList.Refresh();
        iDisposed = true;
        iDisposedMutex.ReleaseMutex();
    }
    
    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        int state = aHighlighted ? 1 : 0;
        if(aListable.NodeHit == null || (aListable.State != state)) {
	        NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
	        hit.Width = aWidth;
	        hit.Height = aHeight;
	        
	        ListableUpnpObject obj = aListable as ListableUpnpObject;
	        Assert.Check(obj != null);
	        if(obj.Object as Container != null) {
	            NodePolygon icon = IconDirectory(obj.Object, aHeight, aHighlighted);
	            float icon_width = aHeight + 10;//TextureManager.Instance.TextureByNameOrLoad("Bitmaps/folder.png").Width;
	            icon.LocalTranslation.X = (icon_width - icon.Width) * 0.5f;
	            hit.AddChild(icon);
	            NodeText text = Text(obj.Object, icon_width, aWidth - icon_width, aHeight, aNodeFont, aHighlighted);
	            text.AllowHits = false;
	            hit.AddChild(text);
	        } else {
	            /*NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
	            poly.AllowHits = false;
	            poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));*/
	            float icon_width = aHeight + 10;//poly.Width;
	            NodeText text = Text(obj.Object, icon_width, aWidth - icon_width, aHeight, aNodeFont, aHighlighted);
	            //hit.AddChild(poly);
	            hit.AddChild(text);
	        }
	        
	        NodePolygon line = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
	        line.AllowHits = false;
	        line.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/line.png")));
	        line.LocalTranslation.Y = aHeight - (line.Height * 0.5f);
	        line.LocalTranslation.Z = 1;
	        hit.AddChild(line);
	        aListable.NodeHit = hit;
	        aListable.State = state;
        }
        return aListable.NodeHit;
    }
    
    private NodePolygon IconDirectory(UpnpObject aObject, float aHeight, bool aHighlighted) {
        NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
        poly.AllowHits = false;
        MusicAlbum album = aObject as MusicAlbum;
        if(album != null && album.AlbumArtUri != "") {
            System.Console.WriteLine(album.Title + ": " + album.Artist + ", " + album.AlbumArtUri);
            poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad(album.AlbumArtUri)));
        } else {
            poly.Texture(new ReferenceTexture(/*TextureManager.Instance.TextureByNameOrLoad("Bitmaps/default_cover.jpg")*/));
        }
        poly.ClampToTextureSize = false;
        poly.Width = aHeight;
        poly.Height = aHeight;
        if (poly.CurrRenderState.Texture.Object != null && poly.CurrRenderState.Texture.Object.Width > 0 && poly.CurrRenderState.Texture.Object.Height > 0)
        {
            poly.Height = aHeight * ((float)poly.CurrRenderState.Texture.Object.Height / (float)poly.CurrRenderState.Texture.Object.Width);
            poly.LocalTranslation.Y = (aHeight - poly.Height) * 0.5f;
            if(poly.Height > aHeight) {
                poly.Height = aHeight;
                poly.Width = aHeight * ((float)poly.CurrRenderState.Texture.Object.Width / (float)poly.CurrRenderState.Texture.Object.Height);
                poly.LocalTranslation.Y = 0;
                poly.LocalTranslation.X = (aHeight - poly.Width) * 0.5f;
            }
        }
        return poly;
    }
    
    private NodeText Text(UpnpObject aObject, float aX, float aWidth, float aHeight, NodeFont aNodeFont, bool aHighlighted) {
        NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
        text.AllowHits = false;
        text.LocalTranslation.X = aX;
        text.Width = aWidth - (aHeight + 10);//iInsert.Active && aHighlighted ? iInsert.LocalTranslation.X - aX : aWidth;
        text.Height = aHeight;
        text.Justification = aNodeFont.Justification;
        text.Alignment = aNodeFont.Alignment;
        text.Trimming = aNodeFont.Trimming;
        text.CurrFont = aNodeFont.CurrFont;
        text.Colour = aNodeFont.Colour;
        text.Text = aObject.Title;
        return text;
    }
    
    public void SetInsert(bool aEnable) {
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.Insert", aEnable));
        //iContainerList.Refresh();
    }
    
    private void EventSubscribed(object aSender) {
        Trace.WriteLine(Trace.kMediaServer, ">ViewMediaServer.EventSubscribed");
        EventDirectory();
    }
    
    private void EventDirectory() {
        Trace.WriteLine(Trace.kMediaServer, ">ViewMediaServer.EventDirectory");
        iDisposedMutex.WaitOne();
        if(iDisposed) return;
        DirInfo dirInfo = iModelMediaServer.DirInfo;
        if(dirInfo != null) {
            iContainerList.ListEntryProvider = iModelMediaServer.ListEntryProvider;
            Messenger.Instance.PresentationMessage(new MsgSetHighlight("Library.ContainerList", dirInfo.Index, NodeList.EAlignment.EA_Centre));
        }
        iDisposedMutex.ReleaseMutex();
        Trace.WriteLine(Trace.kMediaServer, "<ViewMediaServer.EventDirectory");
    }
    
    private void EventContainerUpdated() {
    }
    
    private Mutex iDisposedMutex = null;
    private bool iDisposed = false;
    private ModelMediaServer iModelMediaServer = null;
    private NodeList iContainerList = null;
    private NodePolygon iInsert = null;
}

} // KinskyPda
} // Linn
