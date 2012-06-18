using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System;
using System.Drawing;
using Linn.Gui;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ModelDirectoryRenderer : IListableRenderer
{
    public ModelDirectoryRenderer(Node aRoot) {
        VisitorSearch search = new VisitorSearch("Library.Insert");
        iInsert = (NodePolygon)search.Search(aRoot);
        Assert.Check(iInsert != null);
    }
    
    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
        hit.Width = aWidth;
        hit.Height = aHeight;
        
        ListableUpnpObject obj = aListable as ListableUpnpObject;
        Assert.Check(obj != null);
        if(obj.Object as Container != null) {
            NodePolygon icon = IconDirectory(obj.Object, aHeight, aHighlighted);
            float icon_width = TextureManager.Instance.TextureByNameOrLoad("Bitmaps/folder.png").Width;
            icon.LocalTranslation.X = (icon_width - icon.Width) * 0.5f;
            hit.AddChild(icon);
            NodeText text = Text(obj.Object, icon_width, aWidth - icon_width, aHeight, aNodeFont, aHighlighted);
            text.AllowHits = false;
            hit.AddChild(text);
        } else {
            NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
            poly.AllowHits = false;
            poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));
            NodeText text = Text(obj.Object, poly.Width, aWidth, aHeight, aNodeFont, aHighlighted);
            hit.AddChild(poly);
            hit.AddChild(text);
        }
        return hit;
    }
    
    private NodePolygon IconDirectory(UpnpObject aObject, float aHeight, bool aHighlighted) {
        NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
        poly.AllowHits = false;
        MusicAlbum album = aObject as MusicAlbum;
#if TRACE || DEBUG
        if(album != null && album.AlbumArtUri != "") {
            poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad(album.AlbumArtUri)));
            poly.ClampToTextureSize = false;
            poly.Width = aHeight;
            poly.Height = aHeight;
        } else {
#endif
            if(aHighlighted) {
                poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/folder.png")));
            } else {
                poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/folder_outline.png")));
            }
#if TRACE || DEBUG
        }
#endif
        return poly;
    }
    
    private NodeText Text(UpnpObject aObject, float aX, float aWidth, float aHeight, NodeFont aNodeFont, bool aHighlighted) {
        NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
        text.AllowHits = false;
        text.LocalTranslation.X = aX;
        text.Width = aWidth - (iInsert.Active && aHighlighted ? iInsert.Width : 0);
        text.Height = aHeight;
        text.Justification = aNodeFont.Justification;
        text.Alignment = aNodeFont.Alignment;
        text.Trimming = aNodeFont.Trimming;
        text.CurrFont = aNodeFont.CurrFont;
        text.Colour = aNodeFont.Colour;
        text.Text = aObject.Title;
        return text;
    }
    
    private NodePolygon iInsert = null;
}
    
public class ViewMediaServer : IDisposable
{
    public ViewMediaServer(Node aRoot, ControllerMediaServer aController, Library aLibrary) {
        iRoot = aRoot;
        //iController = aController;
        iModel = aLibrary;
        
        VisitorSearch search = new VisitorSearch("Library.DirectoryList");
        iDirectoryList = (NodeList)search.Search(iRoot);
        iDirectoryList.ListEntryProvider = aLibrary.ListEntryProvider;
        iDirectoryList.ListableRenderer = new ModelDirectoryRenderer(aRoot);
        iDirectoryList.Refresh();
        iDirectoryList.Focused = false;
        Assert.Check(iDirectoryList != null);
        
        search = new VisitorSearch("Library.Location");
        iBreadcrumb = (NodeText)search.Search(iRoot);
        Assert.Check(iBreadcrumb != null);
        
        iFadeTimer = new Timer();
        iFadeTimer.Interval = 10;
        iFadeTimer.Elapsed += FadeText;
        iFadeTimer.AutoReset = false;
        
        Messenger.Instance.PresentationMessage(new MsgSetText("Library.Location", aLibrary.Name));
        Messenger.Instance.PresentationMessage(new MsgSetText("Library.NumEntries", ""));
        
        //iKeyBindings = new KeyBindings();
        iModel.EEventSubscribed += EventSubscribed;
        iModel.EEventDirectory += EventDirectory;
        iModel.EEventContainerUpdateIDs += EventContainerUpdateIDs;
        
        Messenger.Instance.PresentationMessage(new MsgSetPosition("Library.DirectoryScrollbar", 0));
        Enable();
    }
    
    public void Dispose() {
        iModel.EEventSubscribed -= EventSubscribed;
        iModel.EEventDirectory -= EventDirectory;
        iModel.EEventContainerUpdateIDs -= EventContainerUpdateIDs;
        iDirectoryList.ListEntryProvider = new ListEntryProviderNull();
        iDirectoryList.ListableRenderer = new ListableRendererNull();
        iDirectoryList.Refresh();
        Messenger.Instance.PresentationMessage(new MsgSetText("Library.DirectoryNumEntries", ""));
    }
    
    public void Enable() {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, ">ViewMediaServer.Enable");
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.DirectoryNumEntries", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.Insert", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.DirectoryList", true));
        if(iModel.ListEntryProvider.Count <= iDirectoryList.LineCount) {
            Messenger.Instance.PresentationMessage(new MsgSetActive("Library.DirectoryScrollbar", false));
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetActive("Library.DirectoryScrollbar", true));
        }
    }
    
    public void SetFocus(bool aFocus) {
        iHasFocus = aFocus;
        SetStatusText(iItemSelected, iModel.ListEntryProvider.Count);
    }
    
    public void Highlight(int aIndex, ListableUpnpObject aObject) {
        SetStatusText(aIndex, iModel.ListEntryProvider.Count);
        SetScrollbar(iDirectoryList.TopEntry / (float)(iModel.ListEntryProvider.Count - iDirectoryList.LineCount));
    }
    
    public void HighlightUpdated(int aIndex, ListableUpnpObject aObject) {
        SetStatusText(aIndex, iModel.ListEntryProvider.Count);
        SetScrollbar(iDirectoryList.TopEntry);
    }
    
    public void UnHighlight(ListableUpnpObject aObject) {
        SetStatusText(-1, iModel.ListEntryProvider.Count);
    }
    
    public void SetInsert(bool aEnable) {
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.Insert", aEnable));
        //Renderer.Instance.Render();
    }
    
    public void SetScrollbar(uint aPosition) {
        int rangeMax = (int)iModel.ListEntryProvider.Count - (int)iDirectoryList.LineCount;
        if(rangeMax > 0) {
            SetScrollbar(aPosition / (float)rangeMax);
        } else {
            SetScrollbar(0.0f);
        }
    }
    
    public void SetScrollbar(float aPosition) {
        if(iModel.ListEntryProvider.Count <= iDirectoryList.LineCount) {
            Messenger.Instance.PresentationMessage(new MsgSetActive("Library.DirectoryScrollbar", false));
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetActive("Library.DirectoryScrollbar", iDirectoryList.Active));
            Messenger.Instance.PresentationMessage(new MsgSetPosition("Library.DirectoryScrollbar", aPosition));
        }
    }
    
    public void SetTopEntry(float aPosition) {
        uint index = 0;
        if((int)iModel.ListEntryProvider.Count - (int)iDirectoryList.LineCount >= 0) {
            index = (uint)Math.Round((iModel.ListEntryProvider.Count - iDirectoryList.LineCount) * aPosition);
        }
        Messenger.Instance.PresentationMessage(new MsgSetTopEntry("Library.DirectoryList", index));
    }
    
    private void EventSubscribed(object aSender) {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, ">ViewMediaServer.EventSubscribed");
        EventDirectory();
    }
    
    private void EventDirectory() {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, ">ViewMediaServer.EventDirectory");
        DirInfo dirInfo = iModel.DirInfo;
        if(dirInfo != null) {
            iDirectoryList.ListEntryProvider = iModel.ListEntryProvider;
            Messenger.Instance.PresentationMessage(new MsgSetHighlight("Library.DirectoryList", dirInfo.Index, NodeList.EAlignment.EA_Centre));
            iDirectoryList.Refresh();
            Messenger.Instance.PresentationMessage(new MsgSetText("Library.Location", dirInfo.Name));
            SetStatusText(dirInfo.Index, iModel.ListEntryProvider.Count);
            if(dirInfo.Index == -1) {
                SetScrollbar(0);
            } else {
                SetScrollbar(iDirectoryList.TopEntry);
            }
            Renderer.Instance.Render();
        }
        Trace.WriteLine(Trace.kLinnGuiMediaServer, "<ViewMediaServer.EventDirectory");
    }
    
    private void EventContainerUpdateIDs() {
        SetStatusText(iItemSelected, iModel.ListEntryProvider.Count);
    }
    
    private void SetStatusText(int aIndex, uint aCount) {
        iItemSelected = aIndex;
        string text = "";
        if(aIndex > -1 && iHasFocus) {
            text = (aIndex + 1).ToString() + " of " + aCount.ToString();
        } else {
            text = aCount.ToString();
        }
        Messenger.Instance.PresentationMessage(new MsgSetText("Library.DirectoryNumEntries", text));
    }
    
    private void FadeText(object aSender, EventArgs aArgs) {
        double t = TimeSpan.FromTicks(DateTime.Now.Ticks - iPrevTime).TotalSeconds;
        double delta = 255.0f * t * kFadeTime;
        //Trace.WriteLine(Trace.kLinnGuiMediaServer, "ViewMediaServer.FadeText: t=" + t + ", delta=" + delta);

        if(!iFadeOut) {
            if(iAlpha + delta > 255) {
                iAlpha = 255;
            } else {
                iAlpha += delta;
                iFadeTimer.Start();
            }
        } else {
            if(iAlpha - delta < 0) {
                iAlpha = 0;

            } else {
                iAlpha -= delta;
                iFadeTimer.Start();
            }
        }
        
        //iProgress.Colour = Color.FromArgb((int)iAlpha, iProgress.Colour.R, iProgress.Colour.G, iProgress.Colour.B);
        
        iPrevTime = DateTime.Now.Ticks;
        Renderer.Instance.Render();
    }
    
    private Node iRoot = null;
    //private ControllerMediaServer iController = null;
    private Library iModel;
    private NodeList iDirectoryList;
    private NodeText iBreadcrumb;
    private bool iHasFocus;
    private int iItemSelected = -1;
    
    private const float kFadeTime = 2.0f;
    private Timer iFadeTimer;
    private double iAlpha = 255;
    private bool iFadeOut = false;
    private long iPrevTime = 0;
}
    
} // Kinsky
} // Linn
