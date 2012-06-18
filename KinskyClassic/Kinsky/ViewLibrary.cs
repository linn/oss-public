using System;
using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ModelLibraryRenderer : IListableRenderer
{   
    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
        hit.Width = aWidth;
        hit.Height = aHeight;
        
        NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
        poly.AllowHits = false;
        poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));
        
        Library server = aListable as Library;
        Assert.Check(server != null);
        NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
        text.AllowHits = false;
        text.LocalTranslation.X = poly.Width;
        text.Width = aWidth - poly.Width;
        text.Height = aHeight;
        text.Justification = aNodeFont.Justification;
        text.Alignment = aNodeFont.Alignment;
        text.Trimming = aNodeFont.Trimming;
        text.CurrFont = aNodeFont.CurrFont;
        text.Colour = aNodeFont.Colour;
        text.Text = server.Name;
        
        hit.AddChild(poly);
        hit.AddChild(text);
        //System.Console.WriteLine("ModelSourceRenderer.ToNodeHit: " + text.Text);
        return hit;
    }
}
    
public class ViewLibrary : IDisposable
{
    public ViewLibrary(Node aRoot, ControllerLibrary aController, House aHouse) {
        iRoot = aRoot;
        //iController = aController;
        iHouse = aHouse;
        
        VisitorSearch search = new VisitorSearch("Library.NasList");
        iNasList = (NodeList)search.Search(iRoot);
        iNasList.ListEntryProvider = aModel.ListEntryProvider;
        iNasList.ListableRenderer = new ModelLibraryRenderer();
        iNasList.Refresh();
        Assert.Check(iNasList != null);
        
        iHouse.EventLibraryAdded += EventLibraryAdded;
        iHouse.EventLibraryRemoved += EventLibraryRemoved;
        
        SetStatusText(iSelected, iModel.ListEntryProvider.Count);
        Messenger.Instance.PresentationMessage(new MsgSetPosition("Library.NasScrollbar", 0));
        Enable();
    }
    
    public void Dispose() {
        iModel.EEventMediaServerAdded -= EventMediaServerAdded;
        iModel.EEventMediaServerDeleted -= EventMediaServerDeleted;
        iNasList.ListEntryProvider = new ListEntryProviderNull();
        iNasList.ListableRenderer = new ListableRendererNull();
        iNasList.Refresh();
        Messenger.Instance.PresentationMessage(new MsgSetText("Library.NasNumEntries", ""));
    }
    
    public void Enable() {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, ">ViewLibrary.Enable");
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasNumEntries", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasList", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasScrollbar", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.DirectoryList", false));
        if(iModel.ListEntryProvider.Count <= iNasList.LineCount) {
            Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasScrollbar", false));
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasScrollbar", true));
        }
    }
    
    public void Disable() {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, ">ViewLibrary.Disable");
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.LocationStart", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasList", false));
        //Renderer.Instance.Render();
    }
    
    public void SetFocus(bool aFocus) {
        iHasFocus = aFocus;
        SetStatusText(iSelected, iModel.ListEntryProvider.Count);
    }
    
    /*public void SetFocus(bool aFocus) {
        Messenger.Instance.PresentationMessage(new MsgSetActive("CurrentPlaylist.FocusHit", aFocus));
        Messenger.Instance.PresentationMessage(new MsgSetActive("Library.FocusHit", !aFocus));
        iNasList.HighlightActive = aFocus;
        System.Console.WriteLine("ViewLibrary.SetFocus: focus=" + aFocus);
        iNasList.Render(false);
    }*/
    
    public void SetScrollbar(uint aPosition) {
        int rangeMax = (int)iModel.ListEntryProvider.Count - (int)iNasList.LineCount;
        if(rangeMax > 0) {
            SetScrollbar(aPosition / (float)rangeMax);
        } else {
            SetScrollbar(0.0f);
        }
    }
    
    public void SetScrollbar(float aPosition) {
        if(iModel.ListEntryProvider.Count <= iNasList.LineCount) {
            Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasScrollbar", false));
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetActive("Library.NasScrollbar", iNasList.Active));
            Messenger.Instance.PresentationMessage(new MsgSetPosition("Library.NasScrollbar", aPosition));
        }
    }
    
    public void SetTopEntry(float aPosition) {
        uint index = 0;
        if((int)iModel.ListEntryProvider.Count - (int)iNasList.LineCount >= 0) {
            index = (uint)Math.Round((iModel.ListEntryProvider.Count - iNasList.LineCount) * aPosition);
        }
        Messenger.Instance.PresentationMessage(new MsgSetTopEntry("Library.NasList", index));
    }
    
    private void EventLibraryAdded(object obj, House.EventArgsLibrary e) {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, "ViewLibrary.Added: " + e.Name);
        SetScrollbar(iNasList.TopEntry);
        SetStatusText(iSelected, iModel.ListEntryProvider.Count);
    }
    
    private void EventLibraryRemoved(object obj, House.EventArgsLibrary e) {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, "ViewLibrary.Deleted: " + e.Name);
        SetScrollbar(iNasList.TopEntry);
        SetStatusText(iSelected, iModel.ListEntryProvider.Count);
    }
    
    public void Highlight(int aIndex, Library aLibrary) {
        SetStatusText(aIndex, iModel.ListEntryProvider.Count);
        SetScrollbar(iNasList.TopEntry / (float)(iModel.ListEntryProvider.Count - iNasList.LineCount));
    }
    
    public void HighlightUpdated(int aIndex, Library aLibrary) {
        SetStatusText(aIndex, iModel.ListEntryProvider.Count);
        float bottom = (float)(iModel.ListEntryProvider.Count - iNasList.LineCount);
        if(bottom > 0) {
            SetScrollbar(iNasList.TopEntry / bottom);
        } else {
            SetScrollbar(0.0f);
        }
    }
    
    public void UnHighlight(Library aLibrary) {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, "ViewLibrary.UnSelected: " + aLibrary.Name);
        SetStatusText(-1, iModel.ListEntryProvider.Count);
    }
    
    private void SetStatusText(int aIndex, uint aCount) {
        iSelected = aIndex;
        string text = "";
        if(aIndex > -1 && iHasFocus) {
            text = (aIndex + 1).ToString() + " of " + aCount.ToString() + " " + kStatusText + (aCount == 1 ? "" : "s");
        } else {
            text = aCount.ToString() + " " + kStatusText + (aCount == 1 ? "" : "s");
        }
        Messenger.Instance.PresentationMessage(new MsgSetText("Library.NasNumEntries", text));
    }
    
    private static const string kStatusText = "server";
    private Node iRoot = null;
    //private ControllerLibrary iController = null;
    private House iHouse;
    private NodeList iNasList;
    private bool iHasFocus = false;
    private int iSelected = -1;
}

} // Kinsky
} // Linn
