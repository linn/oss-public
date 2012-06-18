using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System;
using System.Collections.Generic;
using System.Threading;
using Linn.Topology;

namespace Linn {
namespace Kinsky {

public class ModelRoomRenderer : IListableRenderer
{
    public ModelRoomRenderer(Node aRoot) {
        VisitorSearch search = new VisitorSearch("RoomSelection.StandByButton");
        iStandby = (NodePolygon)search.Search(aRoot);
        Assert.Check(iStandby != null);
    }
    
    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
        hit.Width = aWidth;
        hit.Height = aHeight;
        
        NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
        poly.AllowHits = false;
        poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));
        
        AdapterRoom.ListableRoom room = aListable as AdapterRoom.ListableRoom;
        Assert.Check(room != null);

        NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
        text.AllowHits = false;
        text.LocalTranslation.X = poly.Width;
        text.Width = aWidth - poly.Width - (iStandby.Active && aHighlighted ? iStandby.Width : 0);
        text.Height = aHeight;
        text.Justification = aNodeFont.Justification;
        text.Alignment = aNodeFont.Alignment;
        text.Trimming = aNodeFont.Trimming;
        text.CurrFont = aNodeFont.CurrFont;
        text.Colour = aNodeFont.Colour;
        text.Text = room.Room.Name;
        hit.AddChild(poly);
        hit.AddChild(text);
        return hit;
    }
    
    private NodePolygon iStandby = null;
}

public class ViewRooms : ViewNodeList, IDisposable
{
    public ViewRooms(Node aRoot, House aHouse)
        : base(aRoot, "RoomSelection.RoomList")
    {
        iHouse = aHouse;
        iMutex = new Mutex();
        
        iAdapter = new AdapterRoom(aHouse);
        iAdapter.EventChanged += EventListChanged;

        iList.AutoSelect = false;
        iList.ListEntryProvider = iAdapter;
        iList.ListableRenderer = new ModelRoomRenderer(aRoot);
        iList.Refresh();

        iScrollBar = iRoot.Search("RoomSelection.RoomScrollbar");
        iStatusBar = iRoot.Search("RoomSelection.Number");

        iSubFocus = true;

        Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
        //Messenger.Instance.PresentationMessage(new MsgSetFocus(iList, true));
        Renderer.Instance.Render(false);

        Messenger.Instance.EEventAppMessage += Receive;
    }

    public void Receive(Message aMessage)
    {
        if (!iSubFocus || !ProcessKeyboard(aMessage))
        {
            if (aMessage.Fullname == iList.Fullname)
            {
                MsgHighlight highlightMsg = aMessage as MsgHighlight;

                if (highlightMsg != null)
                {
                    SetRoomStatusText(highlightMsg.Index);
                }

                // handle item selected

                MsgSelect selectMsg = aMessage as MsgSelect;

                if (selectMsg != null)
                {
                    Messenger.Instance.PresentationMessage(new MsgSetFocus(iList, false));
                }

                // handle top entry chnaged

                MsgTopEntry topMsg = aMessage as MsgTopEntry;

                if (topMsg != null)
                {
                    SetScrollbar(topMsg.Index);
                }

                // handle highlighted item's index changes (due to deletion or insertion, etc)

                MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;

                if (highlightUpdatedMsg != null)
                {
                    SetRoomStatusText(highlightUpdatedMsg.Index);
                }

                // handle item unhighlighted

                MsgUnHighlight unhighlightMsg = aMessage as MsgUnHighlight;

                if (unhighlightMsg != null)
                {
                    SetRoomStatusText(-1);
                }
            }

            if (aMessage.Fullname == "RoomSelection.RoomScrollbar")
            {
                // handle scrollbar position changed

                MsgPositionChanged positionMsg = aMessage as MsgPositionChanged;

                if (positionMsg != null)
                {
                    uint index = 0;

                    if(iAdapter.Count > iList.LineCount)
                    {
                        index = (uint)Math.Round((iAdapter.Count - iList.LineCount) * positionMsg.NewPosition);
                    }

                    Messenger.Instance.PresentationMessage(new MsgSetTopEntry(iList, index));
                }
            }

            if (aMessage.Fullname == "SourceSelection.SourceList")
            {
                MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;

                if (focusMsg != null)
                {
                    if (focusMsg.NewFocus)
                    {
                        Messenger.Instance.PresentationMessage(new MsgSetFocus(iList, false));
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        /*
        iModel.EEventRoomAdded -= EventRoomAdded;
        iModel.EEventRoomDeleted -= EventRoomDeleted;
        iModel.EEventSourceAdded -= EventSourceAdded;
        iModel.EEventSourceDeleted -= EventSourceDeleted;
        iRoomList.ListEntryProvider = new ListEntryProviderNull();
        iRoomList.ListableRenderer = new ListableRendererNull();
        iRoomList.Refresh();
        iSourceList.ListEntryProvider = new ListEntryProviderNull();
        iSourceList.ListableRenderer = new ListableRendererNull();
        iSourceList.Refresh();
        Enable();
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.RoomText", kNoRoomText));
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.SourceText", kNoSourceText));
        SetRoomStatusText(-1, 0);
        SetSourceStatusText(-1, 0);
        */
    }
    
    public void Enable() {
        if(iHouse.RoomList.Count <= iList.LineCount) {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, iList.Active));
        }
    }
    
    public void SetScrollbar(uint aPosition) {
        int rangeMax = (int)iHouse.RoomList.Count - (int)iList.LineCount;
        //System.Console.WriteLine("ViewSystem.SetRoomScrollbar: aPosition=" + aPosition + ", iModel.RoomList.Count=" + iModel.RoomList.Count + ", rangeMax=" + rangeMax);
        if(rangeMax > 0) {
            SetScrollbar(aPosition / (float)rangeMax);
        } else {
            SetScrollbar(0.0f);
        }
    }
    
    public void SetScrollbar(float aPosition) {
        if(iHouse.RoomList.Count <= iList.LineCount) {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, iList.Active));
            Messenger.Instance.PresentationMessage(new MsgSetPosition(iScrollBar, aPosition));
        }
    }
    
    public void EventListChanged(object obj, EventArgs e)
    {
        Trace.WriteLine(Trace.kKinskyTouch, "ViewRooms.Changed");
        SetScrollbar(iList.TopEntry);
        SetRoomStatusText(iList.HighlightedIndex);
        iList.Refresh();
    }
    
    /*
    public void Highlight(int aIndex, Room aRoom) {        
        SetScrollbar(iList.TopEntry);
        SetRoomStatusText(aIndex, iModel.RoomList.Count);
        
        Trace.WriteLine(Trace.kLinnGui, "ViewSystem.Highlight: room has " + aRoom.SourceCount + " sources");
    }
    */
    /*
    public void Subscribed(int aRoomIndex, ModelRoom aRoom, int aSourceIndex, ModelRoomSource aRoomSource) {
        if(aRoom != null) {
            Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.RoomText", aRoom.Room));
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.RoomText", kNoRoomText));
        }
        
        if(aRoomSource != null) {
            Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.SourceText", aRoomSource.Name));
        } else {
            Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.SourceText", kNoSourceText));
        }
        
        iMutex.WaitOne();
        if(aRoomIndex == -1) {
            Messenger.Instance.PresentationMessage(new MsgSetHighlight("RoomSelection.RoomList", aRoomIndex, NodeList.EAlignment.EA_None));
        }
        ModelRoomSource roomSource = iSourceList.ListEntryProvider.Entry((uint)aSourceIndex) as ModelRoomSource;
        if(aSourceIndex == -1 || roomSource == aRoomSource) {
            Messenger.Instance.PresentationMessage(new MsgSetHighlight("SourceSelection.SourceList", aSourceIndex, NodeList.EAlignment.EA_None));
        }
        if(iSourceList.ListEntryProvider != null && aRoom != null) {
            if(iSourceList.ListEntryProvider == aRoom.SourceList) {
                SetSourceStatusText(aSourceIndex, iSourceList.ListEntryProvider.Count);
            }
        }
        iMutex.ReleaseMutex();
        if(aRoomIndex == -1) {
            Messenger.Instance.PresentationMessage(new MsgSetState("StatusBar.Room.Bistable", true));
        }
        //Renderer.Instance.Render();
    }
    */
    /*
    public void UnHighlight(Room aRoom) {
        Trace.WriteLine(Trace.kLinnGui, ">ViewSystem.UnHighlight:Room  aRoom.Room=" + aRoom.Name);
        iLastRoomUnHighlighted = aRoom;
        //Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.RoomText", kNoRoomText));
        
        SetScrollbar(iList.TopEntry);
        SetRoomStatusText(-1, iModel.RoomList.Count);
        //SetSourceStatusText(-1, iSourceList.ListEntryProvider.Count);
        //Renderer.Instance.Render();
        Trace.WriteLine(Trace.kLinnGui, "<ViewSystem.UnHighlight:Room");
    }
    */
    /*
    public void HighlightUpdated(int aIndex, Room aRoom) {
        SetRoomStatusText(aIndex, iModel.RoomList.Count);
        SetScrollbar(iList.TopEntry);
    }
    */
    /*
    private void EventSourceAdded(ModelRoomSource aSource) {
        Trace.WriteLine(Trace.kLinnGui, "ViewSystem.Added:Source: aSource.Name=" + aSource.Name);
        SetSourceScrollbar(iSourceList.TopEntry);
        SetSourceStatusText(iSourceSelected, iSourceList.ListEntryProvider.Count);
    }
    
    public void EventSourceUpdated(ModelRoomSource aSource) {
        iSourceList.Refresh();
    }
     * 
    private void EventSourceDeleted(ModelRoomSource aSource) {
        Trace.WriteLine(Trace.kLinnGui, "ViewSystem.Deleted:Source: aSource.Name=" + aSource.Name);
        SetSourceScrollbar(iSourceList.TopEntry);
        SetSourceStatusText(iSourceSelected, iSourceList.ListEntryProvider.Count);
    }
    */

    private void SetRoomStatusText(int aIndex)
    {
        uint count = iAdapter.Count;

        string text;

        if(aIndex > -1) {
            text = (aIndex + 1).ToString() + " of " + count.ToString() + " " + kRoomStatusText + (count == 1 ? "" : "s");
        } else {
            text = count + " " + kRoomStatusText + (count == 1 ? "" : "s");
        }
        Messenger.Instance.PresentationMessage(new MsgSetText(iStatusBar, text));
    }

    private const string kRoomStatusText = "room";
    private const string kNoRoomText = "Room N/A";
    private House iHouse;
    private AdapterRoom iAdapter;
    //private Room iLastRoomUnHighlighted;
    private Mutex iMutex;
    private Plugin iScrollBar;
    private Plugin iStatusBar;
    private bool iSubFocus;
}


public class ModelSourceRenderer : IListableRenderer
{
    public ModelSourceRenderer(Node aRoot)
    {
        iSelectedIndicator = (NodeHit)aRoot.Search("SourceSelection.SourceSelectedIndicator");
    }

    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted)
    {
        NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
        hit.Width = aWidth;
        hit.Height = aHeight;

        NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
        poly.AllowHits = false;
        poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));

        AdapterSource.ListableSource source = aListable as AdapterSource.ListableSource;
        Assert.Check(source != null);
        if (source.Source == source.Source.Room.Current)
        {
            iSelectedIndicator.Active = true;
            hit.AddChild(iSelectedIndicator);
        }
        NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
        text.AllowHits = false;
        text.LocalTranslation.X = iSelectedIndicator.Width + poly.Width;
        text.Width = aWidth - poly.Width - iSelectedIndicator.Width;
        text.Height = aHeight;
        text.Justification = aNodeFont.Justification;
        text.Alignment = aNodeFont.Alignment;
        text.Trimming = aNodeFont.Trimming;
        text.CurrFont = aNodeFont.CurrFont;
        text.Colour = aNodeFont.Colour;
        text.Text = source.Text;
#if TRACE || DEBUG
        /*
        if (source.ModelDeviceSource as ModelDeviceSourceAuxiliary != null)
        {
            text.Text += " [Auxiliary]";
        }
        else if (source.ModelDeviceSource as ModelDeviceSourceUpnpAv != null)
        {
            text.Text += " [UpnpAv]";
        }
        */
#endif

        hit.AddChild(poly);
        hit.AddChild(text);
        //System.Console.WriteLine("ModelSourceRenderer.ToNodeHit: " + text.Text);
        return hit;
    }

    private NodeHit iSelectedIndicator;
}

public class ViewSources : ViewNodeList, IDisposable
{
    public ViewSources(Node aRoot, House aHouse)
        : base(aRoot, "SourceSelection.SourceList")
    {
        iHouse = aHouse;
        iMutex = new Mutex(false);

        iList.AutoSelect = false;
        iList.ListableRenderer = new ModelSourceRenderer(aRoot);
        iList.ListEntryProvider = new ListEntryProviderNull();
        iList.Refresh();

        iScrollBar = iRoot.Search("SourceSelection.SourceScrollbar");
        iStatusBar = iRoot.Search("SourceSelection.Number");
        iPanelButton = PackageManager.Instance.PluginByName("StatusBar.Room.Bistable");

        iSubFocus = false;

        Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
        //Messenger.Instance.PresentationMessage(new MsgSetFocus(iList, false));
        Renderer.Instance.Render(false);

        Messenger.Instance.EEventAppMessage += Receive;
    }

    private void EventListChanged(object obj, EventArgs e)
    {
        Trace.WriteLine(Trace.kKinskyTouch, "ViewSource.SourceListChanged");
        SetScrollbar(iList.TopEntry);
        SetStatusText(iList.HighlightedIndex);
        iList.Refresh();
    }

    private void EventCurrentChanged(object obj, EventArgs e)
    {
        Trace.WriteLine(Trace.kKinskyTouch, "ViewSource.CurrentChanged");
        iList.Refresh();
    }

    public void Receive(Message aMessage)
    {
        if (!iSubFocus || !ProcessKeyboard(aMessage))
        {
            if (aMessage.Fullname == iList.Fullname)
            {
                // handle highlight changed

                MsgHighlight highlightMsg = aMessage as MsgHighlight;

                if (highlightMsg != null)
                {
                    SetStatusText(highlightMsg.Index);
                }

                // handle top entry chnaged

                MsgTopEntry topMsg = aMessage as MsgTopEntry;

                if (topMsg != null)
                {
                    SetScrollbar(topMsg.Index);
                }

                // handle highlighted item's index changes (due to deletion or insertion, etc)

                MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;

                if (highlightUpdatedMsg != null)
                {
                    SetStatusText(highlightUpdatedMsg.Index);
                }

                // handle item unhighlighted

                MsgUnHighlight unhighlightMsg = aMessage as MsgUnHighlight;

                if (unhighlightMsg != null)
                {
                    SetStatusText(-1);
                }
            }

            // listen to the room list as well

            if (aMessage.Fullname == "RoomSelection.RoomList")
            {
                // handle item selected

                MsgSelect selectMsg = aMessage as MsgSelect;

                if (selectMsg != null)
                {
                    Messenger.Instance.PresentationMessage(new MsgSetFocus(iList, true));
                }

                // handle highlight changed position

                MsgHighlight highlightMsg = aMessage as MsgHighlight;

                if (highlightMsg != null)
                {
                    if (highlightMsg.Listable != null)
                    {
                        AdapterRoom.ListableRoom room = highlightMsg.Listable as AdapterRoom.ListableRoom;

                        Assert.Check(room != null);

                        iMutex.WaitOne();

                        if (iAdapter != null)
                        {
                            iAdapter.EventChanged -= EventListChanged;
                            iAdapter.Room.EventCurrentChanged -= EventCurrentChanged;
                        }

                        iAdapter = room.AdapterSource;

                        iAdapter.EventChanged += EventListChanged;
                        iAdapter.Room.EventCurrentChanged += EventCurrentChanged;

                        iList.ListEntryProvider = iAdapter;

                        iMutex.ReleaseMutex();
                    }
                    else
                    {
                        iMutex.WaitOne();

                        if (iAdapter != null)
                        {
                            iAdapter.EventChanged -= EventListChanged;
                            iAdapter.Room.EventCurrentChanged -= EventCurrentChanged;
                            iAdapter = null;
                        }

                        iList.ListEntryProvider = new ListEntryProviderNull();

                        iMutex.ReleaseMutex();
                    }

                    EventListChanged(null, EventArgs.Empty);
                    EventCurrentChanged(null, EventArgs.Empty);
                }

                MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;

                if (focusMsg != null)
                {
                    if (focusMsg.NewFocus)
                    {
                        Messenger.Instance.PresentationMessage(new MsgSetFocus(iList, false));
                    }
                }
            }

            if (aMessage.Fullname == "SourceSelection.SourceScrollbar")
            {
                // handle scrollbar position changed

                MsgPositionChanged positionMsg = aMessage as MsgPositionChanged;

                if (positionMsg != null)
                {
                    uint index = 0;

                    if (iAdapter.Count > iList.LineCount)
                    {
                        index = (uint)Math.Round((iAdapter.Count - iList.LineCount) * positionMsg.NewPosition);
                    }

                    Messenger.Instance.PresentationMessage(new MsgSetTopEntry(iList, index));
                }
            }
        }
    }

    public void Dispose()
    {
        /*
        iModel.EEventRoomAdded -= EventRoomAdded;
        iModel.EEventRoomDeleted -= EventRoomDeleted;
        iModel.EEventSourceAdded -= EventSourceAdded;
        iModel.EEventSourceDeleted -= EventSourceDeleted;
        iRoomList.ListEntryProvider = new ListEntryProviderNull();
        iRoomList.ListableRenderer = new ListableRendererNull();
        iRoomList.Refresh();
        iSourceList.ListEntryProvider = new ListEntryProviderNull();
        iSourceList.ListableRenderer = new ListableRendererNull();
        iSourceList.Refresh();
        Enable();
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.RoomText", kNoRoomText));
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.SourceText", kNoSourceText));
        SetRoomStatusText(-1, 0);
        SetSourceStatusText(-1, 0);
        */
    }

    public void Enable()
    {
        if (iList.ListEntryProvider.Count <= iList.LineCount)
        {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
        }
        else
        {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, iList.Active));
        }
    }

    public void SetScrollbar(uint aPosition)
    {
        int rangeMax = (int)iList.ListEntryProvider.Count - (int)iList.LineCount;
        if (rangeMax > 0)
        {
            SetScrollbar(aPosition / (float)rangeMax);
        }
        else
        {
            SetScrollbar(0.0f);
        }
    }

    public void SetScrollbar(float aPosition)
    {
        if (iList.ListEntryProvider.Count <= iList.LineCount)
        {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
        }
        else
        {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, iList.Active));
            Messenger.Instance.PresentationMessage(new MsgSetPosition(iScrollBar, aPosition));
        }
    }

    private void SetStatusText(int aHighlighted)
    {
        string text;

        uint count = 0;

        if (iAdapter != null)
        {
            count = iAdapter.Count;
        }

        if (aHighlighted > -1)
        {
            text = (aHighlighted + 1).ToString() + " of " + count.ToString() + " source" + (count == 1 ? "" : "s");
        }
        else
        {
            text = count.ToString() + " source" + (count == 1 ? "" : "s");
        }

        Messenger.Instance.PresentationMessage(new MsgSetText(iStatusBar, text));
    }

    private AdapterSource iAdapter;

    private House iHouse;
    private Mutex iMutex;
    private Node iScrollBar;
    private Node iStatusBar;
    private Plugin iPanelButton;
    private bool iSubFocus;
}


} // Kinsky
} // Linn
