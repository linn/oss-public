using System;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;

using Linn;
using Linn.Kinsky;
using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Topology;

using Upnp;

namespace KinskyClassic
{
    class View : Linn.Kinsky.IView, IDisposable
    {
        public View(Canvas aCanvas)
        {
            Node root = aCanvas.CurrLayout.Root;

            iViewWidgetSelectorRoom = new ViewWidgetSelectorRoom(root);
            iViewWidgetSelectorSource = new ViewWidgetSelectorSource(root);
            iViewWidgetButtonStandby = new ViewWidgetButtonStandby(root);
            iViewWidgetVolumeControl = new ViewWidgetVolumeControl(root);
            iViewWidgetMediaTime = new ViewWidgetMediaTime(root);
            iViewWidgetTransportControl = new ViewWidgetTransportControl(root);
            iViewWidgetTrack = new ViewWidgetTrack(root);
            iViewWidgetPlaylist = new ViewWidgetPlaylistMediaRenderer(root);
            iViewWidgetPlaylistRadio = new ViewWidgetPlaylistRadio(root);
            iViewWidgetPlaylistReceiver = new ViewWidgetPlaylistReceiver(root);
            iViewWidgetPlaylistAux = new ViewWidgetPlaylistAux(root);
            iViewWidgetPlaylistDiscPlayer = new ViewWidgetPlaylistDiscPlayer(root);
            iViewWidgetPlayMode = new ViewWidgetPlayMode(root);
            iViewWidgetButtonSave = new ViewWidgetButtonSave(root);
            iViewWidgetButtonWasteBin = new ViewWidgetButtonWasteBin(root);
            iViewWidgetButtonSize = new ViewWidgetButtonSize(root);
            iViewWidgetButtonView = new ViewWidgetButtonView(root);
            iViewWidgetReceivers = new ViewWidgetReceivers(root);
            iViewWidgetButtonReceivers = new ViewWidgetButtonStub();
        }

        public void Dispose()
        {
            iViewWidgetSelectorRoom.Dispose();
            iViewWidgetSelectorSource.Dispose();
            iViewWidgetButtonStandby.Dispose();
            iViewWidgetVolumeControl.Dispose();
            iViewWidgetMediaTime.Dispose();
            iViewWidgetTransportControl.Dispose();
            //iViewWidgetTrack
            iViewWidgetPlaylist.Dispose();
            iViewWidgetPlaylistRadio.Dispose();
            ///iViewWidgetPlaylistMultipus
            iViewWidgetPlaylistAux.Dispose();
            iViewWidgetPlaylistDiscPlayer.Dispose();
            iViewWidgetPlayMode.Dispose();
            //iViewWidgetButtonSave
            //iViewWidgetButtonWasteBin.Dispose();
            //iViewWidgetButtonSize
            //iViewWidgetButtonView
            //iViewWidgetMultipusReceivers
            //iViewWidgetButtonMultipusReceivers
        }

        public IViewWidgetSelector<Linn.Kinsky.Room> ViewWidgetSelectorRoom
        {
            get
            {
                return iViewWidgetSelectorRoom;
            }
        }

        public IViewWidgetButton ViewWidgetButtonStandby
        {
            get
            {
                return iViewWidgetButtonStandby;
            }
        }
        public IViewWidgetSelector<Linn.Kinsky.Source> ViewWidgetSelectorSource
        {
            get
            {
                return iViewWidgetSelectorSource;
            }
        }

        public IViewWidgetVolumeControl ViewWidgetVolumeControl
        {
            get
            {
                return iViewWidgetVolumeControl;
            }
        }

        public IViewWidgetMediaTime ViewWidgetMediaTime
        {
            get
            {
                return iViewWidgetMediaTime;
            }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlMediaRenderer
        {
            get
            {
                return iViewWidgetTransportControl;
            }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlDiscPlayer
        {
            get
            {
                return iViewWidgetTransportControl;
            }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlRadio
        {
            get
            {
                return iViewWidgetTransportControl;
            }
        }

        public IViewWidgetTrack ViewWidgetTrack
        {
            get
            {
                return iViewWidgetTrack;
            }
        }

        public IViewWidgetPlayMode ViewWidgetPlayMode
        {
            get
            {
                return iViewWidgetPlayMode;
            }
        }

        public IViewWidgetPlaylist ViewWidgetPlaylist
        {
            get
            {
                return iViewWidgetPlaylist;
            }
        }

        public IViewWidgetPlaylistRadio ViewWidgetPlaylistRadio
        {
            get
            {
                return iViewWidgetPlaylistRadio;
            }
        }

        public IViewWidgetPlaylistReceiver ViewWidgetPlaylistReceiver
        {
            get
            {
                return iViewWidgetPlaylistReceiver;
            }
        }

        public IViewWidgetPlaylistAux ViewWidgetPlaylistAux
        {
            get
            {
                return iViewWidgetPlaylistAux;
            }
        }

        public IViewWidgetPlaylistDiscPlayer ViewWidgetPlaylistDiscPlayer
        {
            get
            {
                return iViewWidgetPlaylistDiscPlayer;
            }
        }

        public IViewWidgetButton ViewWidgetButtonSize
        {
            get
            {
                return iViewWidgetButtonSize;
            }
        }

        public IViewWidgetButton ViewWidgetButtonView
        {
            get
            {
                return iViewWidgetButtonView;
            }
        }

        public IViewWidgetButton ViewWidgetButtonSave
        {
            get
            {
                return iViewWidgetButtonSave;
            }
        }

        public IViewWidgetButton ViewWidgetButtonWasteBin
        {
            get
            {
                return iViewWidgetButtonWasteBin;
            }
        }

        public IViewWidgetReceivers ViewWidgetReceivers
        {
            get
            {
                return iViewWidgetReceivers;
            }
        }
        
        public IViewWidgetButton ViewWidgetButtonReceivers
        {
            get
            {
                return iViewWidgetButtonReceivers;
            }
        }

        private ViewWidgetSelectorRoom iViewWidgetSelectorRoom;
        private ViewWidgetSelectorSource iViewWidgetSelectorSource;
        private ViewWidgetButtonStandby iViewWidgetButtonStandby;
        private ViewWidgetVolumeControl iViewWidgetVolumeControl;
        private ViewWidgetMediaTime iViewWidgetMediaTime;
        private ViewWidgetTransportControl iViewWidgetTransportControl;
        private ViewWidgetTrack iViewWidgetTrack;
        private ViewWidgetPlaylistMediaRenderer iViewWidgetPlaylist;
        private ViewWidgetPlaylistRadio iViewWidgetPlaylistRadio;
        private ViewWidgetPlaylistReceiver iViewWidgetPlaylistReceiver;
        private ViewWidgetPlaylistAux iViewWidgetPlaylistAux;
        private ViewWidgetPlaylistDiscPlayer iViewWidgetPlaylistDiscPlayer;
        private ViewWidgetPlayMode iViewWidgetPlayMode;
        private ViewWidgetButtonSave iViewWidgetButtonSave;
        private ViewWidgetButtonWasteBin iViewWidgetButtonWasteBin;
        private ViewWidgetButtonSize iViewWidgetButtonSize;
        private ViewWidgetButtonView iViewWidgetButtonView;
        private ViewWidgetReceivers iViewWidgetReceivers;
        private ViewWidgetButtonStub iViewWidgetButtonReceivers;
    }

    class ViewWidgetButtonStub : IViewWidgetButton
    {
        public void Open()
        {
        }

        public void Close()
        {
        }

        public event EventHandler<EventArgs> EventClick;
    }

    abstract class ViewNodeList
    {
        public ViewNodeList(Node aRoot, string aName)
        {
            Assert.Check(aRoot != null);
            Assert.Check(aName != null);

            iRoot = aRoot;

            iList = (NodeList)iRoot.Search(aName);

            Assert.Check(iList != null);

            iKeyBindings = new KeyBindings();
        }

        protected bool ProcessKeyboard(Linn.Gui.Resources.Message aMessage)
        {
            if (iList.Focused && iList.Active && aMessage.Fullname == "")
            {
                MsgKeyDown key = aMessage as MsgKeyDown;

                if (key != null)
                {
                    string action = iKeyBindings.Action(key.Key);

                    if (action == "PageUp")
                    {
                        Messenger.Instance.PresentationMessage(new MsgScroll(iList, (int)-iList.LineCount));
                        return (true);
                    }
                    else if (action == "PageDown")
                    {
                        Messenger.Instance.PresentationMessage(new MsgScroll(iList, (int)iList.LineCount));
                        return (true);
                    }
                    else if (action == "Up")
                    {
                        Messenger.Instance.PresentationMessage(new MsgScroll(iList, -1));
                        return (true);
                    }
                    else if (action == "Down")
                    {
                        Messenger.Instance.PresentationMessage(new MsgScroll(iList, 1));
                        return (true);
                    }
                    else if (action == "Select")
                    {
                        int index = iList.HighlightedIndex;

                        if (index > -1 && index < iList.ListEntryProvider.Count)
                        {
                            IListable listable = iList.ListEntryProvider.Entries((uint)index, 1)[0];

                            listable.Select();

                            Messenger.Instance.ApplicationMessage(new MsgSelect(iList, index, listable));
                        }

                        return (true);
                    }
                }
            }

            return (false);
        }

        protected Node iRoot;
        protected NodeList iList;
        protected KeyBindings iKeyBindings;
    }

    class ViewWidgetSelectorRoom : ViewNodeList, IViewWidgetSelector<Linn.Kinsky.Room>, IDisposable
    {
        public class RendererRoom : IListableRenderer
        {
            public RendererRoom(Node aRoot)
            {
                iNodeStandby = (NodeHit)aRoot.Search("RoomSelection.StandByButton");
            }

            public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted)
            {
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
                text.Width = aWidth - poly.Width - (iNodeStandby.Active && aHighlighted ? iNodeStandby.Width : 0);
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

            private NodeHit iNodeStandby;
        }

        public ViewWidgetSelectorRoom(Node aRoot)
            : base(aRoot, "RoomSelection.RoomList")
        {
            iAdapter = new AdapterRoom();

            iKeyBindings = new KeyBindings();

            iList.AutoSelect = false;
            iList.ListEntryProvider = iAdapter;
            iList.ListableRenderer = new RendererRoom(aRoot);
            iList.Refresh();

            iScrollBar = iRoot.Search("RoomSelection.RoomScrollbar");
            iStatusBar = iRoot.Search("RoomSelection.Number");
            iRoomText = iRoot.Search("StatusBar.RoomText");
            iSourceList = iRoot.Search("SourceSelection.SourceList");

            iSwitchViewButton = (Bistable)iRoot.Search("StatusBar.Room").NextPlugin;

            iSubFocus = true;

            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
            Renderer.Instance.Render(false);

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (!iSubFocus || !ProcessKeyboard(aMessage))
            {
                if (aMessage.Fullname == iList.Fullname)
                {
                    MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                    if (activeMsg != null)
                    {
                        SetScrollbar(iList.TopEntry);
                    }

                    MsgHighlight highlightMsg = aMessage as MsgHighlight;

                    if (highlightMsg != null)
                    {
                        SetStatusText(highlightMsg.Index);

                        AdapterRoom.ListableRoom room = highlightMsg.Listable as AdapterRoom.ListableRoom;

                        Linn.Kinsky.Room r = null;
                        string name = kNoRoomText;
                        if (room != null)
                        {
                            r = room.Room;
                            name = r.Name;
                        }

                        if (EventSelectionChanged != null)
                        {
                            EventSelectionChanged(this, new EventArgsSelection<Linn.Kinsky.Room>(r));
                        }

                        Messenger.Instance.PresentationMessage(new MsgSetText(iRoomText, name));
                    }

                    // handle item selected

                    MsgSelect selectMsg = aMessage as MsgSelect;

                    if (selectMsg != null)
                    {
                        Messenger.Instance.PresentationMessage(new MsgSetFocus(iList, false));
                    }

                    // handle top entry changed

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

                    // handle focus changed

                    MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;

                    if (focusMsg != null)
                    {
                        iSubFocus = focusMsg.NewFocus;
                    }
                }

                if (aMessage.Fullname == "RoomSelection.RoomScrollbar")
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

            if (aMessage.Fullname == string.Empty)
            {
                MsgKeyDown keyDown = aMessage as MsgKeyDown;
                if (keyDown != null)
                {
                    string action = iKeyBindings.Action(keyDown.Key);
                    Trace.WriteLine(Trace.kKinskyClassic, "ViewWidgetSelectorRoom.Receive: keyDown->action=" + action);
                    if (action == "SwitchView")
                    {
                        Messenger.Instance.PresentationMessage(new MsgToggleState(iSwitchViewButton));
                        Renderer.Instance.Render();
                    }

                    /*if (action == "SwapFocus")
                    {
                        bool focus = iSubFocus;
                        Messenger.Instance.PresentationMessage(new MsgSetFocus(iSourceList, focus));
                        Messenger.Instance.PresentationMessage(new MsgSetFocus(iList, !focus));
                    }*/
                }
            }
        }

        public void Open()
        {
        }

        public void Close()
        {
            iAdapter.Clear();

            SetScrollbar(iList.TopEntry);
            SetStatusText(iList.HighlightedIndex);
            iList.Refresh();
        }

        public void InsertItem(int aIndex, Linn.Kinsky.Room aItem)
        {
            iAdapter.Add(aItem);
            ListChanged();
        }

        public void RemoveItem(Linn.Kinsky.Room aItem)
        {
            iAdapter.Remove(aItem);
            ListChanged();
        }

        public void ItemChanged(Linn.Kinsky.Room aItem)
        {
        }

        public void SetSelected(Linn.Kinsky.Room aItem)
        {
            Messenger.Instance.PresentationMessage(new MsgSetHighlight(iList, iAdapter.Index(aItem), NodeList.EAlignment.EA_Centre));

            string name = kNoRoomText;
            if (aItem != null)
            {
                name = aItem.Name;
            }

            Messenger.Instance.PresentationMessage(new MsgSetText(iRoomText, name));

        }

        public event EventHandler<EventArgsSelection<Linn.Kinsky.Room>> EventSelectionChanged;

        private void ListChanged()
        {
            Console.WriteLine("ListChanged:" + iList.TopEntry);
            SetScrollbar(iList.TopEntry);
            SetStatusText(iList.HighlightedIndex);
            iList.Refresh();
        }

        private void SetScrollbar(uint aPosition)
        {
            if (iAdapter.Count <= iList.LineCount)
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
            }
            else
            {
                int rangeMax = (int)iAdapter.Count - (int)iList.LineCount;
                //System.Console.WriteLine("ViewSystem.SetRoomScrollbar: aPosition=" + aPosition + ", iModel.RoomList.Count=" + iModel.RoomList.Count + ", rangeMax=" + rangeMax);
                if (rangeMax > 0)
                {
                    SetScrollbar(aPosition / (float)rangeMax);
                }
                else
                {
                    SetScrollbar(0.0f);
                }
            }
        }

        private void SetScrollbar(float aPosition)
        {
            if (iAdapter.Count <= iList.LineCount)
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
            }
            else
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, iList.Active));
                Messenger.Instance.PresentationMessage(new MsgSetPosition(iScrollBar, aPosition));
            }
        }

        private void SetStatusText(int aIndex)
        {
            uint count = iAdapter.Count;

            string text = string.Empty;

            if (aIndex > -1)
            {
                text = (aIndex + 1).ToString() + " of ";
            }
            
            text += count.ToString() + " " + kRoomStatusText + (count == 1 ? "" : "s");

            Messenger.Instance.PresentationMessage(new MsgSetText(iStatusBar, text));
        }

        private const string kRoomStatusText = "room";
        private const string kNoRoomText = "Room N/A";

        private AdapterRoom iAdapter;

        private Linn.Gui.Resources.Plugin iScrollBar;
        private Linn.Gui.Resources.Plugin iStatusBar;
        private Linn.Gui.Resources.Plugin iRoomText;
        private Bistable iSwitchViewButton;
        private Node iSourceList;

        private bool iSubFocus;
    }

    class ViewWidgetSelectorSource : ViewNodeList, IViewWidgetSelector<Linn.Kinsky.Source>, IDisposable
    {
        public class RendererSource : IListableRenderer
        {
            public RendererSource(Node aRoot)
            {
                //iSelectedIndicator = (NodeHit)aRoot.Search("SourceSelection.SourceSelectedIndicator");
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
                NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
                text.AllowHits = false;
                text.LocalTranslation.X = /*iSelectedIndicator.Width +*/ poly.Width;
                text.Width = aWidth - poly.Width /*- iSelectedIndicator.Width*/;
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

        public ViewWidgetSelectorSource(Node aRoot)
            : base(aRoot, "SourceSelection.SourceList")
        {
            iMutex = new Mutex(false);

            iAdapter = new AdapterSource();

            iList.AutoSelect = false;
            iList.ListableRenderer = new RendererSource(aRoot);
            iList.Refresh();

            iScrollBar = iRoot.Search("SourceSelection.SourceScrollbar");
            iStatusBar = iRoot.Search("SourceSelection.Number");
            iPanelButton = PackageManager.Instance.PluginByName("StatusBar.Room.Bistable");
            iSourceText = iRoot.Search("StatusBar.SourceText");

            iSubFocus = false;

            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
            Renderer.Instance.Render(false);

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (!iSubFocus || !ProcessKeyboard(aMessage))
            {
                if (aMessage.Fullname == iList.Fullname)
                {
                    MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                    if (activeMsg != null)
                    {
                        SetScrollbar(iList.TopEntry);
                    }

                    // handle highlight changed

                    MsgHighlight highlightMsg = aMessage as MsgHighlight;

                    if (highlightMsg != null)
                    {
                        SetStatusText(highlightMsg.Index);

                        AdapterSource.ListableSource source = highlightMsg.Listable as AdapterSource.ListableSource;

                        Linn.Kinsky.Source s = null;
                        string name = kNoSourceText;
                        if (source != null)
                        {
                            s = source.Source;
                            name = string.Format("{0}", s.Name);
                        }                        

                        Messenger.Instance.PresentationMessage(new MsgSetText(iSourceText, name));
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

                    // handle focus changed

                    MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;

                    if (focusMsg != null)
                    {
                        iSubFocus = focusMsg.NewFocus;
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

                            //if (iAdapter != null)
                            //{
                            //    iAdapter.Room.EventCurrentChanged -= EventCurrentChanged;
                            //}

                            //iAdapter.Room.EventCurrentChanged += EventCurrentChanged;

                            iMutex.ReleaseMutex();
                        }
                        else
                        {
                            iMutex.WaitOne();

                            iAdapter.Clear();

                            iMutex.ReleaseMutex();
                        }

                        ListChanged();
                        //CurrentChanged();
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

        public void Open()
        {
            iList.ListEntryProvider = iAdapter;
        }

        public void Close()
        {
            iAdapter.Clear();
            iList.ListEntryProvider = new ListEntryProviderNull();
            Messenger.Instance.PresentationMessage(new MsgSetText(iSourceText, kNoSourceText));
        }

        public void InsertItem(int aIndex, Linn.Kinsky.Source aItem)
        {
            iAdapter.Add(aItem);
            ListChanged();
        }

        public void RemoveItem(Linn.Kinsky.Source aItem)
        {
            iAdapter.Remove(aItem);
            ListChanged();
        }

        public void ItemChanged(Linn.Kinsky.Source aItem)
        {
        }

        public void SetSelected(Linn.Kinsky.Source aItem)
        {
            Messenger.Instance.PresentationMessage(new MsgSetHighlight(iList, iAdapter.Index(aItem), NodeList.EAlignment.EA_Centre));

            /*if (EventSelectionChanged != null)
            {
                EventSelectionChanged(this, new EventArgsSelection(aTag));
            }*/
        }
    
        public event EventHandler<EventArgsSelection<Linn.Kinsky.Source>> EventSelectionChanged;

        private void ListChanged()
        {
            SetScrollbar(iList.TopEntry);
            SetStatusText(iList.HighlightedIndex);
            iList.Refresh();
        }

        private void SetScrollbar(uint aPosition)
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

        private void SetScrollbar(float aPosition)
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
            string text = string.Empty;

            uint count = 0;

            if (iAdapter != null)
            {
                count += iAdapter.Count;
            }

            if (aHighlighted > -1)
            {
                text += (aHighlighted + 1).ToString() + " of ";
            }

            text += count.ToString() + " " + kSourceStatusText + (count == 1 ? "" : "s");

            Messenger.Instance.PresentationMessage(new MsgSetText(iStatusBar, text));
        }

        private const string kSourceStatusText = "source";
        private const string kNoSourceText = "Source N/A";

        private Mutex iMutex;
        private AdapterSource iAdapter;

        private Node iScrollBar;
        private Node iStatusBar;
        private Linn.Gui.Resources.Plugin iPanelButton;
        private Node iSourceText;
        private bool iSubFocus;
    }

    class ViewWidgetButtonStandby: IViewWidgetButton, IDisposable
    {
        public ViewWidgetButtonStandby(Node aRoot)
        {
            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (aMessage.Fullname == "RoomSelection.StandByButton.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState == false)
                    {
                        if (EventClick != null)
                        {
                            EventClick(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        public void Open()
        {
            Assert.Check(!iOpen);

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iOpen = false;
            }
        }

        public void SetEnabled(bool aEnabled)
        {
        }

        public event EventHandler<EventArgs> EventClick;

        private bool iOpen;
    }

    class ViewWidgetVolumeControl : IViewWidgetVolumeControl, IDisposable
    {
        public ViewWidgetVolumeControl(Node aRoot)
        {
            iVolumeUp = (Monostable)aRoot.Search("ControlPanel.VolumeUp").NextPlugin;
            iVolumeDown = (Monostable)aRoot.Search("ControlPanel.VolumeDown").NextPlugin;
            iVolumeMute = (Monostable)aRoot.Search("ControlPanel.Mute").NextPlugin;
            iMuteActive = aRoot.Search("ControlPanel.MuteBlue");
            iVolumeText = aRoot.Search("StatusBar.VolumeText");
            iVolumeControl = aRoot.Search("ControlPanel.VolumeControl");
            iVolumeInfo = aRoot.Search("StatusBar.VolumeInfo");

            iKeyBindings = new KeyBindings();

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (aMessage.Fullname == "ControlPanel.VolumeUp.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState)
                    {
                        if (EventVolumeIncrement != null)
                        {
                            EventVolumeIncrement(this, EventArgs.Empty);
                        }
                    }
                }
            }
            if (aMessage.Fullname == "ControlPanel.VolumeDown.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState)
                    {
                        if (EventVolumeDecrement != null)
                        {
                            EventVolumeDecrement(this, EventArgs.Empty);
                        }
                    }
                }
            }
            if (aMessage.Fullname == "ControlPanel.Mute.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState == true)
                    {
                        if (EventMuteChanged != null)
                        {
                            EventMuteChanged(this, new EventArgsMute(!iMuteActive.Active));
                        }
                    }
                }
            }
            /*if (aMessage.Fullname == "StatusBar.VolumeText")
            {
                if (aMessage as MsgInputRotate != null)
                {
                    try
                    {
                        iModel.Volume = uint.Parse(iVolume.Text);
                    }
                    catch (System.FormatException e)
                    {
                        System.Console.WriteLine("WARNING: ControllerPreamp.Receive:\n" + e);
                    }
                }
            }*/
            if (iOpen)
            {
                if (aMessage.Fullname == string.Empty)
                {
                    MsgKeyDown keyDown = aMessage as MsgKeyDown;
                    if (keyDown != null)
                    {
                        string action = iKeyBindings.Action(keyDown.Key);
                        Trace.WriteLine(Trace.kKinskyClassic, "ViewWidgetVolumeControl.Receive: keyDown->action=" + action);
                        if (action == "VolumeDown")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iVolumeDown, true));
                            Renderer.Instance.Render();
                        }
                        else if (action == "VolumeUp")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iVolumeUp, true));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Mute")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iVolumeMute, true));
                            Renderer.Instance.Render();
                        }
                    }

                    MsgKeyUp keyUp = aMessage as MsgKeyUp;
                    if (keyUp != null)
                    {
                        string action = iKeyBindings.Action(keyUp.Key);
                        Trace.WriteLine(Trace.kKinskyClassic, "ViewWidgetVolumeControl.Receive: keyUp->action=" + action);
                        if (action == "VolumeDown")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iVolumeDown, false));
                            Renderer.Instance.Render();
                        }
                        else if (action == "VolumeUp")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iVolumeUp, false));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Mute")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iVolumeMute, false));
                            Renderer.Instance.Render();
                        }
                    }
                }
            }
        }

        public void Open()
        {
            Assert.Check(!iOpen);

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iVolumeInfo.Active = false;
                iVolumeControl.Active = false;
                Renderer.Instance.Render();

                iOpen = false;
            }
        }

        public void Initialised()
        {
            if (iOpen)
            {
                iVolumeInfo.Active = true;
                iVolumeControl.Active = true;
                Renderer.Instance.Render();
            }
        }

        public void SetVolume(uint aVolume)
        {
            if (iOpen)
            {
                Messenger.Instance.PresentationMessage(new MsgSetText(iVolumeText, aVolume.ToString()));
            }
        }

        public void SetMute(bool aMute)
        {
            if (iOpen)
            {
                iMuteActive.Active = aMute;
                Renderer.Instance.Render();
            }
        }

        public void SetVolumeLimit(uint aVolumeLimit)
        {
        }

        public event EventHandler<EventArgs> EventVolumeIncrement;
        public event EventHandler<EventArgs> EventVolumeDecrement;
        public event EventHandler<EventArgsMute> EventMuteChanged;
        public event EventHandler<EventArgsVolume> EventVolumeChanged;

        private bool iOpen;

        private Node iVolumeControl;
        private Node iVolumeInfo;
        private Monostable iVolumeUp;
        private Monostable iVolumeDown;
        private Monostable iVolumeMute;
        private Node iMuteActive;
        private Node iVolumeText;

        private KeyBindings iKeyBindings;
    }

    class ViewWidgetMediaTime : IViewWidgetMediaTime, IDisposable
    {
        public ViewWidgetMediaTime(Node aRoot)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iNodeTime = aRoot.Search("StatusBar.TimeText");
            iNodeSlider = aRoot.Search("StatusBar.TimeSlider");

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (aMessage.Plugin == iNodeTime)
            {
                MsgHit hitMsg = aMessage as MsgHit;

                if (hitMsg != null)
                {
                    iShowTimeRemaining = !iShowTimeRemaining;
                    UpdateSeconds(iSeconds);
                }
            }

            if (aMessage.Fullname == "StatusBar.TimeSliderInput")
            {
                if (aMessage as MsgHit != null)
                {
                    iMutex.WaitOne();
                    iSeeking = true;        // user has click the seekbar process the next slider position message
                    iMutex.ReleaseMutex();
                }
            }

            if (aMessage.Fullname == "StatusBar.TimeSlider")
            {
                MsgPositionChanged posMsg = aMessage as MsgPositionChanged;

                if (posMsg != null)
                {
                    iMutex.WaitOne();
                    if (iSeeking && iDuration > 0)
                    {
                        uint seconds = (uint)(posMsg.NewPosition * iDuration);
                        iMutex.ReleaseMutex();

                        if (EventSeekSeconds != null)
                        {
                            EventSeekSeconds(this, new EventArgsSeekSeconds(seconds));
                        }
                    }
                    else
                    {
                        iMutex.ReleaseMutex();
                    }
                }
            }
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            Messenger.Instance.PresentationMessage(new MsgSetText(iNodeTime, ""));
            Messenger.Instance.PresentationMessage(new MsgSetPosition(iNodeSlider, 0));

            iStopped = true;
            iBuffering = false;

            iOpen = true;

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iOpen = false;
            }

            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            UpdateSeconds(iSeconds);
        }

        public void SetAllowSeeking(bool aAllowSeeking)
        {
        }

        public void SetTransportState(ETransportState aTransportState)
        {
            SetBuffering(aTransportState == ETransportState.eBuffering);
            SetStopped(aTransportState == ETransportState.eStopped);
        }

        public void SetDuration(uint aDuration)
        {
            iMutex.WaitOne();
            iDuration = aDuration;
            iMutex.ReleaseMutex();
        }

        public void SetSeconds(uint aSeconds)
        {
            iMutex.WaitOne();
            iSeconds = aSeconds;
            iMutex.ReleaseMutex();

            UpdateSeconds(aSeconds);
        }

        public event EventHandler<EventArgsSeekSeconds> EventSeekSeconds;

        private void SetBuffering(bool aBuffering)
        {
            iMutex.WaitOne();
            iBuffering = aBuffering;
            if (!aBuffering)
            {
                iSeeking = false;
            }
            iMutex.ReleaseMutex();

            /*if (aBuffering)
            {
                Messenger.Instance.PresentationMessage(new MsgSetText(iNodeTime, ""));
                Messenger.Instance.PresentationMessage(new MsgSetPosition(iNodeSlider, 0));
                Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeSlider, false));
            }*/
        }

        private void SetStopped(bool aStopped)
        {
            iMutex.WaitOne();
            iStopped = aStopped;
            iMutex.ReleaseMutex();

            if (aStopped)
            {
                Messenger.Instance.PresentationMessage(new MsgSetText(iNodeTime, ""));
                Messenger.Instance.PresentationMessage(new MsgSetPosition(iNodeSlider, 0));
                Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeSlider, false));
            }
        }

        private void UpdateSeconds(uint aSeconds)
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                if (!iBuffering && !iStopped && !iSeeking)
                {
                    Time time = null;
                    if (iShowTimeRemaining)
                    {
                        time = new Time((int)aSeconds - (int)iDuration);
                    }
                    else
                    {
                        time = new Time((int)aSeconds);
                    }
                    Messenger.Instance.PresentationMessage(new MsgSetText(iNodeTime, time.ToPrettyString()));
                    float p = 0.0f;
                    float duration = (float)iDuration;
                    if (duration > 0)
                    {
                        p = aSeconds / duration;
                    }
                    Messenger.Instance.PresentationMessage(new MsgSetPosition(iNodeSlider, p));
                    if (!iNodeSlider.Active)
                    {
                        Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeSlider, true));
                    }
                }
                else if(iStopped)
                {
                    Messenger.Instance.PresentationMessage(new MsgSetText(iNodeTime, ""));
                    Messenger.Instance.PresentationMessage(new MsgSetPosition(iNodeSlider, 0));
                    if (iNodeSlider.Active)
                    {
                        Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeSlider, false));
                    }
                }
            }

            iMutex.ReleaseMutex();
        }

        private Mutex iMutex;
        private bool iOpen;

        private bool iSeeking;

        private bool iBuffering;
        private bool iStopped;

        private bool iShowTimeRemaining;
        private uint iSeconds;
        private uint iDuration;

        private Node iNodeTime;
        private Node iNodeSlider;
    }

    class ViewWidgetTransportControl : IViewWidgetTransportControl, IDisposable
    {
        public ViewWidgetTransportControl(Node aRoot)
        {
            iKeyBindings = new KeyBindings();

            iNodePlaybackControl = aRoot.Search("ControlPanel.PlaybackControl");
            iNodePlaying = aRoot.Search("ControlPanel.PlayBlue");
            iNodePaused = aRoot.Search("ControlPanel.PauseBlue");
            iNodeStopped = aRoot.Search("ControlPanel.StopBlue");

            iPlay = (Monostable)aRoot.Search("ControlPanel.Play").NextPlugin;
            iPause = (Monostable)aRoot.Search("ControlPanel.Pause").NextPlugin;
            iStop = (Monostable)aRoot.Search("ControlPanel.Stop").NextPlugin;
            iPrevious = (Monostable)aRoot.Search("ControlPanel.Skip Backward").NextPlugin;
            iNext = (Monostable)aRoot.Search("ControlPanel.Skip Forward").NextPlugin;

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (aMessage.Fullname == "ControlPanel.Play.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState == true)
                    {
                        if (EventPlay != null)
                        {
                            EventPlay(this, EventArgs.Empty);
                        }
                    }
                }
            }

            if (aMessage.Fullname == "ControlPanel.Pause.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState == true)
                    {
                        if (EventPause != null)
                        {
                            EventPause(this, EventArgs.Empty);
                        }
                    }
                }
            }

            if (aMessage.Fullname == "ControlPanel.Stop.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState == true)
                    {
                        if (EventStop != null)
                        {
                            EventStop(this, EventArgs.Empty);
                        }
                    }
                }
            }

            if (aMessage.Fullname == "ControlPanel.Skip Forward.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState == true)
                    {
                        if (EventNext != null)
                        {
                            EventNext(this, EventArgs.Empty);
                        }
                    }
                }
            }

            if (aMessage.Fullname == "ControlPanel.Skip Backward.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState == true)
                    {
                        if (EventPrevious != null)
                        {
                            EventPrevious(this, EventArgs.Empty);
                        }
                    }
                }
            }

            if (iOpen)
            {
                if (aMessage.Fullname == string.Empty)
                {
                    MsgKeyDown keyDown = aMessage as MsgKeyDown;
                    if (keyDown != null)
                    {
                        string action = iKeyBindings.Action(keyDown.Key);
                        Trace.WriteLine(Trace.kKinskyClassic, "ViewWidgetTransportControl.Receive: keyDown->action=" + action);
                        if (action == "Play")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iPlay, true));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Pause")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iPause, true));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Stop")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iStop, true));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Previous")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iPrevious, true));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Next")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iNext, true));
                            Renderer.Instance.Render();
                        }
                    }

                    MsgKeyUp keyUp = aMessage as MsgKeyUp;
                    if (keyUp != null)
                    {
                        string action = iKeyBindings.Action(keyUp.Key);
                        Trace.WriteLine(Trace.kKinskyClassic, "ViewWidgetTransportControl.Receive: keyUp->action=" + action);
                        if (action == "Play")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iPlay, false));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Pause")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iPause, false));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Stop")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iStop, false));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Previous")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iPrevious, false));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Next")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iNext, false));
                            Renderer.Instance.Render();
                        }
                    }
                }
            }
        }


        public void Open()
        {
            Assert.Check(!iOpen);

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iInitialised = false;
                iNodePlaybackControl.Active = false;
                iNodePlaying.Active = false;
                iNodePaused.Active = false;
                iNodeStopped.Active = false;

                Renderer.Instance.Render();

                iOpen = false;
            }
        }

        public void Initialised()
        {
            iInitialised = true;
            iNodePlaybackControl.Active = true;
            Renderer.Instance.Render();
        }

        public void SetPlayNowEnabled(bool aEnabled) { }
        public void SetPlayNextEnabled(bool aEnabled) { }
        public void SetPlayLaterEnabled(bool aEnabled) { }

        public void SetDragging(bool aDragging) { }

        public void SetTransportState(ETransportState aTransportState)
        {
            if (iOpen)
            {
                iNodePlaying.Active = aTransportState == ETransportState.ePlaying || aTransportState == ETransportState.eBuffering;
                iNodeStopped.Active = aTransportState == ETransportState.eStopped || aTransportState == ETransportState.eUnknown;
                iNodePaused.Active = aTransportState == ETransportState.ePaused;
                
                if (iInitialised)
                {
                    Renderer.Instance.Render(false);
                }
            }
        }
		
		public void SetDuration(uint aDuration)
        {
        }

        public void SetAllowSkipping(bool aAllowSkipping) { }
        public void SetAllowPausing(bool aAllowPausing) { }

        public event EventHandler<EventArgs> EventPause;
        public event EventHandler<EventArgs> EventPlay;
        public event EventHandler<EventArgs> EventStop;
        public event EventHandler<EventArgs> EventPrevious;
        public event EventHandler<EventArgs> EventNext;

        public event EventHandler<EventArgsPlay> EventPlayNow;
        public event EventHandler<EventArgsPlay> EventPlayNext;
        public event EventHandler<EventArgsPlay> EventPlayLater;

        private bool iOpen;

        private KeyBindings iKeyBindings;

        private Node iNodePlaybackControl;
        private Node iNodePlaying;
        private Node iNodePaused;
        private Node iNodeStopped;

        private Monostable iPlay;
        private Monostable iPause;
        private Monostable iStop;
        private Monostable iNext;
        private Monostable iPrevious;

        private bool iInitialised;
    }

    class ViewWidgetTrack : IViewWidgetTrack
    {
        private class TextureGdiImage : TextureGdi
        {
            public TextureGdiImage(Image aImage)
                : base(aImage.Tag as string)
            {
                iImage = aImage;
                iWidth = aImage.Width;
                iHeight = aImage.Height;
            }

            public override void Dispose() { }
            public override void Refresh() { }
        }

        public ViewWidgetTrack(Node aRoot)
        {
            iMutex = new Mutex(false);
            iOpen = false;
            iArtist = string.Empty;
            iAlbum = string.Empty;

            iNodeTrackInfo = aRoot.Search("StatusBar.TrackInfo");
            iNodeTitle = aRoot.Search("StatusBar.Track");
            iNodeAlbum = aRoot.Search("StatusBar.Album");
            iNodeArtwork = aRoot.Search("StatusBar.CoverArt");
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            Messenger.Instance.PresentationMessage(new MsgSetText(iNodeTitle, ""));
            Messenger.Instance.PresentationMessage(new MsgSetText(iNodeAlbum, ""));
            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeTrackInfo, true));
            iNodeArtwork.Texture(new ReferenceTexture());

            iOpen = true;

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeTrackInfo, false));

            iOpen = false;

            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
        }

        public void SetItem(upnpObject aObject)
        {
            iMutex.WaitOne();

            iTitle = DidlLiteAdapter.Title(aObject);
            iArtist = DidlLiteAdapter.Artist(aObject);
            iAlbum = DidlLiteAdapter.Album(aObject);

            iMutex.ReleaseMutex();
        }

        public void SetMetatext(upnpObject aObject)
        {
            iMutex.WaitOne();

            iAlbum = DidlLiteAdapter.Title(aObject);
            iArtist = string.Empty;

            iMutex.ReleaseMutex();
        }

        public void SetArtwork(Image aArtwork)
        {
            /*iMutex.WaitOne();

            TextureGdiImage image = null;
            if (aArtwork != null)
            {
                image = new TextureGdiImage(aArtwork);
            }
            iNodeArtwork.Texture(new ReferenceTexture(image));

            iMutex.ReleaseMutex();*/
        }

        public void SetBitrate(uint aBitrate)
        {
        }

        public void SetSampleRate(float aSampleRate)
        {
        }

        public void SetBitDepth(uint aBitDepth)
        {
        }

        public void SetCodec(string aCodec)
        {
        }

        public void SetLossless(bool aLossless)
        {
        }

        public void Update()
        {
            iMutex.WaitOne();
            iNodeArtwork.Render();
            Messenger.Instance.PresentationMessage(new MsgSetText(iNodeTitle, iTitle));
            string albumAndArtist = iAlbum;
            if (iArtist != "")
            {
                if (albumAndArtist != "")
                {
                    albumAndArtist += " \u2013 ";
                }
                albumAndArtist += iArtist;
            }
            Messenger.Instance.PresentationMessage(new MsgSetText(iNodeAlbum, albumAndArtist));
            iMutex.ReleaseMutex();
        }

        private static readonly Image kNoAlbumArt = Linn.Kinsky.Properties.Resources.NoAlbumArt;

        private Mutex iMutex;
        private bool iOpen;

        private string iTitle;
        private string iArtist;
        private string iAlbum;

        private Node iNodeTrackInfo;
        private Node iNodeTitle;
        private Node iNodeAlbum;
        private Node iNodeArtwork;
    }

    abstract class ViewWidgetPlaylist : ViewNodeList, IDisposable
    {
        protected ViewWidgetPlaylist(Node aRoot)
            : base(aRoot, "CurrentPlaylist.Playlist")
        {
            iOpen = false;

            iKeyBindings = new KeyBindings();

            iList.AutoSelect = true;
            iList.Focused = false;

            iSubFocus = false;

            iScrollBar = iRoot.Search("CurrentPlaylist.Scrollbar");
            iStatusBar = iRoot.Search("CurrentPlaylist.NumEntries");
            iNodePlayingTrack = (NodeHit)aRoot.Search("CurrentPlaylist.PlayingTrack");

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public virtual void Receive(Message aMessage)
        {
            if (iOpen)
            {
                if (!ProcessKeyboard(aMessage))
                {
                    if (aMessage.Plugin == iStatusBar)
                    {
                        MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;

                        if (msgActiveChanged != null)
                        {
                            if (msgActiveChanged.NewActive == true)
                            {
                                if (!iOpen)
                                {
                                    Messenger.Instance.PresentationMessage(new MsgSetActive(iStatusBar, false));
                                }
                            }
                        }
                    }

                    if (aMessage.Plugin == iList)
                    {
                        MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                        if (activeMsg != null)
                        {
                            if (activeMsg.NewActive == true)
                            {
                                if (!iOpen)
                                {
                                    Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
                                    Messenger.Instance.PresentationMessage(new MsgSetActive(iNodePlayingTrack, false));
                                }
                                else
                                {
                                    SetScrollbar(iList.TopEntry);
                                }
                            }
                        }

                        MsgHighlight highlightMsg = aMessage as MsgHighlight;

                        if (highlightMsg != null)
                        {
                            SetStatusText(highlightMsg.Index);
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

                        // handle top entry changed

                        MsgTopEntry topMsg = aMessage as MsgTopEntry;

                        if (topMsg != null)
                        {
                            SetScrollbar(topMsg.Index);
                        }

                        // handle focus changed

                        MsgFocusChanged msgFocus = aMessage as MsgFocusChanged;

                        if (msgFocus != null)
                        {
                            iSubFocus = msgFocus.NewFocus;
                            SetStatusText(iList.HighlightedIndex);
                        }
                    }

                    if (aMessage.Plugin == iScrollBar)
                    {
                        MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;

                        if (msgActiveChanged != null)
                        {
                            if (msgActiveChanged.NewActive == true)
                            {
                                if (!iOpen)
                                {
                                    Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
                                }
                            }
                        }

                        // handle scrollbar position changed

                        MsgPositionChanged positionMsg = aMessage as MsgPositionChanged;

                        if (positionMsg != null)
                        {
                            uint index = 0;

                            if (iList.ListEntryProvider.Count > iList.LineCount)
                            {
                                index = (uint)Math.Round((iList.ListEntryProvider.Count - iList.LineCount) * positionMsg.NewPosition);
                            }

                            Messenger.Instance.PresentationMessage(new MsgSetTopEntry(iList, index));
                        }
                    }

                    OnReceive(aMessage);
                }
            }
        }

        protected abstract void OnReceive(Message aMessage);

        protected void OnOpen()
        {
            Assert.Check(!iOpen);
            iOpen = true;
        }

        protected void OnClose()
        {
            if (iOpen)
            {
                SetScrollbar(iList.TopEntry);
                iList.Refresh();

                SetStatusText(iList.HighlightedIndex);

                iOpen = false;
            }
        }

        protected void OnInitialised()
        {
            if (iOpen)
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iList, true));
                Messenger.Instance.PresentationMessage(new MsgSetActive(iStatusBar, true));
                SetScrollbar(iList.TopEntry);
            }
        }

        private void SetScrollbar(uint aPosition)
        {
            if (iList.ListEntryProvider.Count <= iList.LineCount)
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
            }
            else
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
        }

        protected void SetScrollbar(float aPosition)
        {
            Console.WriteLine(iList.ListEntryProvider.Count + ", " + iList.LineCount);

            if (iList.ListEntryProvider.Count <= iList.LineCount)
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
            }
            else
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, true));
                Messenger.Instance.PresentationMessage(new MsgSetPosition(iScrollBar, aPosition));
            }
        }

        protected void SetStatusText(int aIndex)
        {
            uint count = iList.ListEntryProvider.Count;
            string text = string.Empty;

            if (aIndex > -1 && iSubFocus)
            {
                text = (aIndex + 1).ToString() + " of ";
            }

            text += count.ToString() + " " + BaseStatusText + (count == 1 ? "" : "s");

            Messenger.Instance.PresentationMessage(new MsgSetText(iStatusBar, text));
        }

        protected abstract string BaseStatusText { get; }

        protected bool iOpen;
        protected bool iSubFocus;

        protected NodeHit iNodePlayingTrack;

        private Node iStatusBar;
        private Node iScrollBar;
    }

    class ViewWidgetPlaylistMediaRenderer : ViewWidgetPlaylist, IViewWidgetPlaylist
    {
        public class RendererPlaylist : IListableRenderer
        {
            public RendererPlaylist(Node aRoot, ViewWidgetPlaylistMediaRenderer aView)
            {
                iView = aView;
                iNodeDelete = (NodeHit)aRoot.Search("CurrentPlaylist.RemoveButton");
                iNodePlayingTrack = (NodeHit)aRoot.Search("CurrentPlaylist.PlayingTrack");
            }

            public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted)
            {
                NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
                hit.Width = aWidth;
                hit.Height = aHeight;

                if (!iNodeDelete.Active)
                {
                    iNodeDelete.Active = true;
                }

                //NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
                //poly.AllowHits = false;
                //poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));

                if (aIndex == iView.TrackIndex)
                {
                    hit.AddChild(iNodePlayingTrack);
                    iNodePlayingTrack.LocalTranslation.Y = (aHeight - iNodePlayingTrack.Height) * 0.5f;
                    iNodePlayingTrack.Active = true;
                }

                AdapterPlaylist.ListableItem item = aListable as AdapterPlaylist.ListableItem;
                Assert.Check(item != null);

                NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
                text.AllowHits = false;
                text.LocalTranslation.X = iNodePlayingTrack.Width;
                text.Width = aWidth - iNodePlayingTrack.Width - (iNodeDelete.Active && aHighlighted ? iNodeDelete.Width : 0);
                text.Height = aHeight;
                text.Justification = aNodeFont.Justification;
                text.Alignment = aNodeFont.Alignment;
                text.Trimming = aNodeFont.Trimming;
                text.CurrFont = aNodeFont.CurrFont;
                text.Colour = aNodeFont.Colour;
                text.Text = DidlLiteAdapter.Title(item.PlaylistItem.DidlLite[0]);
                //hit.AddChild(poly);
                hit.AddChild(text);
                return hit;
            }

            private ViewWidgetPlaylistMediaRenderer iView;

            private NodeHit iNodeDelete;
            private NodeHit iNodePlayingTrack;
        }

        public ViewWidgetPlaylistMediaRenderer(Node aRoot)
            : base(aRoot)
        {
            iMutex = new Mutex(false);
            iTrackIndex = -1;

            iAdapter = new AdapterPlaylist();
            iRenderer = new RendererPlaylist(aRoot, this);

            iDeleteTrack = (Monostable)aRoot.Search("CurrentPlaylist.RemoveButton").NextPlugin;
            iDeleteAll = (Monostable)aRoot.Search("CurrentPlaylist.DeleteAll").NextPlugin;
            iNodeDeleteAll = aRoot.Search("CurrentPlaylist.DeleteAll");
            iNodeHeader = aRoot.Search("CurrentPlaylist.CurrentPlaylist_Text");
        }

        public void Open()
        {
            iMutex.WaitOne();
            
            OnOpen();

            Messenger.Instance.PresentationMessage(new MsgSetText(iNodeHeader, kHeader));
            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeDeleteAll, true));

            iList.ListEntryProvider = iAdapter;
            iList.ListableRenderer = iRenderer;
            iList.Refresh();

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();            
            
            OnClose();
            iAdapter.Clear();
            
            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            OnInitialised();
        }

        public void SetPlaylist(IList<MrItem> aPlaylist)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                int count = aPlaylist.Count;

                iAdapter.SetPlaylist(aPlaylist);

                iTrackIndex = iAdapter.Index(iTrack);
                iMutex.ReleaseMutex();

                SetScrollbar(iList.TopEntry);
                SetStatusText(iList.HighlightedIndex);

                iList.Refresh();
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        public void SetTrack(MrItem aTrack)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iTrack = aTrack;
                iTrackIndex = iAdapter.Index(aTrack);

                if (iTrackIndex == -1)
                {
                    iMutex.ReleaseMutex();

                    Messenger.Instance.PresentationMessage(new MsgSetActive(iNodePlayingTrack, false));
                }
                else
                {
                    iMutex.ReleaseMutex();

                    iList.Refresh();
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        public void Save()
        {
        }

        public void Delete()
        {
            if (EventPlaylistDeleteAll != null)
            {
                EventPlaylistDeleteAll(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgsSeekTrack> EventSeekTrack;

        public event EventHandler<EventArgsPlaylistInsert> EventPlaylistInsert;
        public event EventHandler<EventArgsPlaylistMove> EventPlaylistMove;
        public event EventHandler<EventArgsPlaylistDelete> EventPlaylistDelete;

        public event EventHandler<EventArgs> EventPlaylistDeleteAll;

        internal int TrackIndex
        {
            get
            {
                return iTrackIndex;
            }
        }

        protected override void OnReceive(Message aMessage)
        {
            if (aMessage.Plugin == iList)
            {
                MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                if (activeMsg != null)
                {
                    if (activeMsg.NewActive == true)
                    {
                        if (iOpen)
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodePlayingTrack, iTrackIndex > -1 && iAdapter.Count > iTrackIndex));
                        }
                    }
                }

                // handle item selected

                MsgSelect selectMsg = aMessage as MsgSelect;

                if (selectMsg != null)
                {
                    if (EventSeekTrack != null)
                    {
                        EventSeekTrack(this, new EventArgsSeekTrack((uint)iList.HighlightedIndex));
                    }
                }
            }

            if (aMessage.Fullname == "CurrentPlaylist.RemoveButton.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState == false)
                    {
                        List<MrItem> tracks = new List<MrItem>();
                        AdapterPlaylist.ListableItem item = iAdapter.Entry((uint)iList.HighlightedIndex) as AdapterPlaylist.ListableItem;
                        tracks.Add(item.PlaylistItem);

                        if (EventPlaylistDelete != null)
                        {
                            EventPlaylistDelete(this, new EventArgsPlaylistDelete(tracks));
                        }
                    }
                }
            }

            if (iOpen)
            {
                if (aMessage.Fullname == string.Empty)
                {
                    MsgKeyDown keyDown = aMessage as MsgKeyDown;
                    if (keyDown != null)
                    {
                        string action = iKeyBindings.Action(keyDown.Key);
                        Trace.WriteLine(Trace.kKinskyClassic, "ViewWidgetPlaylistMediaRenderer.Receive: keyDown->action=" + action);
                        if (action == "DeleteTrack")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iDeleteTrack, true));
                            Renderer.Instance.Render();
                        }
                        else if (action == "DeleteAllTracks")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iDeleteAll, true));
                            Renderer.Instance.Render();
                        }
                    }

                    MsgKeyUp keyUp = aMessage as MsgKeyUp;
                    if (keyUp != null)
                    {
                        string action = iKeyBindings.Action(keyUp.Key);
                        Trace.WriteLine(Trace.kKinskyClassic, "ViewWidgetPlaylistMediaRenderer.Receive: keyUp->action=" + action);
                        if (action == "DeleteTrack")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iDeleteTrack, false));
                            Renderer.Instance.Render();
                        }
                        else if (action == "DeleteAllTracks")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iDeleteAll, false));
                            Renderer.Instance.Render();
                        }
                    }
                }
            }
        }

        protected override string BaseStatusText
        {
            get
            {
                return kPlaylistStatusText;
            }
        }

        private const string kHeader = "Current Playlist";
        private const string kPlaylistStatusText = "track";

        private Mutex iMutex;
        private AdapterPlaylist iAdapter;
        private RendererPlaylist iRenderer;

        private MrItem iTrack;
        private int iTrackIndex;

        private Monostable iDeleteTrack;
        private Monostable iDeleteAll;
        private Node iNodeHeader;
        private Node iNodeDeleteAll;
    }

    class ViewWidgetPlaylistRadio : ViewWidgetPlaylist, IViewWidgetPlaylistRadio
    {
        public class RendererRadio : IListableRenderer
        {
            public RendererRadio(Node aRoot, ViewWidgetPlaylistRadio aView)
            {
                iView = aView;
                iNodeDelete = (NodeHit)aRoot.Search("CurrentPlaylist.RemoveButton");
                iNodePlayingPreset = (NodeHit)aRoot.Search("CurrentPlaylist.PlayingTrack");
            }

            public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted)
            {
                NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
                hit.Width = aWidth;
                hit.Height = aHeight;

                if (iNodeDelete.Active)
                {
                    iNodeDelete.Active = false;
                }

                //NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
                //poly.AllowHits = false;
                //poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));

                if (aIndex == iView.PresetIndex)
                {
                    hit.AddChild(iNodePlayingPreset);
                    iNodePlayingPreset.LocalTranslation.Y = (aHeight - iNodePlayingPreset.Height) * 0.5f;
                    iNodePlayingPreset.Active = true;
                }

                AdapterRadio.ListableItem item = aListable as AdapterRadio.ListableItem;
                Assert.Check(item != null);

                NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
                text.AllowHits = false;
                text.LocalTranslation.X = iNodePlayingPreset.Width;
                text.Width = aWidth - iNodePlayingPreset.Width ;
                text.Height = aHeight;
                text.Justification = aNodeFont.Justification;
                text.Alignment = aNodeFont.Alignment;
                text.Trimming = aNodeFont.Trimming;
                text.CurrFont = aNodeFont.CurrFont;
                text.Colour = aNodeFont.Colour;
                text.Text = string.Format("{0}. {1}", (aIndex + 1), DidlLiteAdapter.Title(item.Preset.DidlLite[0]));
                //hit.AddChild(poly);
                hit.AddChild(text);
                return hit;
            }

            private ViewWidgetPlaylistRadio iView;

            private NodeHit iNodeDelete;
            private NodeHit iNodePlayingPreset;
        }

        public ViewWidgetPlaylistRadio(Node aRoot)
            : base(aRoot)
        {
            iMutex = new Mutex(false);

            iAdapter = new AdapterRadio();
            iRenderer = new RendererRadio(aRoot, this);

            iNodeDeleteAll = aRoot.Search("CurrentPlaylist.DeleteAll");
            iNodeHeader = aRoot.Search("CurrentPlaylist.CurrentPlaylist_Text");
            iNodeGlow = aRoot.Search("CurrentPlaylist.Glow");
        }

        public void Open()
        {
            iMutex.WaitOne();

            OnOpen();

            Messenger.Instance.PresentationMessage(new MsgSetText(iNodeHeader, kHeader));
            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeDeleteAll, false));

            iList.ListEntryProvider = iAdapter;
            iList.ListableRenderer = iRenderer;
            iList.Refresh();
            
            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            OnClose();
            iAdapter.Clear();
            
            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            OnInitialised();
        }

        public void SetPresets(IList<MrItem> aPresets)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();

                int count = aPresets.Count;

                iAdapter.SetPresets(aPresets);

                SetScrollbar(iList.TopEntry);
                SetStatusText(iList.HighlightedIndex);

                iList.Refresh();
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        public void SetChannel(Channel aChannel)
        {
            iMutex.WaitOne();
            iChannel = aChannel;
            //string uri = DidlLiteAdapter.ArtworkUri(iChannel.DidlLite[0]);
            //string title = DidlLiteAdapter.Title(iChannel.DidlLite[0]);
            iMutex.ReleaseMutex();
        }

        public void SetPreset(int aPresetIndex)
        {
            iMutex.WaitOne();
            iPresetIndex = aPresetIndex;
            iMutex.ReleaseMutex();

            iList.Refresh();
        }

        public void Save()
        {
        }

        public event EventHandler<EventArgsSetChannel> EventSetChannel;
        public event EventHandler<EventArgsSetPreset> EventSetPreset;

        internal int PresetIndex
        {
            get
            {
                return iPresetIndex;
            }
        }

        protected override void OnReceive(Message aMessage)
        {
            if (aMessage.Plugin == iList)
            {
                MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                if (activeMsg != null)
                {
                    if (activeMsg.NewActive == true)
                    {
                        if (iOpen)
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodePlayingTrack, iPresetIndex > -1 && iAdapter.Count > iPresetIndex));
                        }
                    }
                }

                // handle item selected

                MsgSelect selectMsg = aMessage as MsgSelect;

                if (selectMsg != null)
                {
                    if (EventSetPreset != null)
                    {
                        AdapterRadio.ListableItem item = iList.HighlightedItem as AdapterRadio.ListableItem;
                        EventSetPreset(this, new EventArgsSetPreset(item.Preset));
                    }
                }
            }

            if (aMessage.Fullname == "CurrentPlaylist.Root")
            {
                MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                if (activeMsg != null)
                {
                    if (activeMsg.NewActive == true)
                    {
                        if (iOpen)
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeDeleteAll, false));
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeGlow, false));
                        }
                    }
                }
            }
        }

        protected override string BaseStatusText
        {
            get
            {
                return kPresetStatusText;
            }
        }

        private const string kHeader = "Radio Presets";
        private const string kPresetStatusText = "preset";

        private Mutex iMutex;
        private AdapterRadio iAdapter;
        private RendererRadio iRenderer;

        private Channel iChannel;
        private int iPresetIndex;

        private Node iNodeDeleteAll;
        private Node iNodeHeader;
        private Node iNodeGlow;
    }

    class ViewWidgetPlaylistReceiver : IViewWidgetPlaylistReceiver
    {
        public ViewWidgetPlaylistReceiver(Node aRoot)
        {
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void SetSenders(IList<ModelSender> aSenders)
        {
        }

        public void SetChannel(Channel aChannel)
        {
        }

        public void Save()
        {
        }

        public event EventHandler<EventArgsSetChannel> EventSetChannel;
    }

    class ViewWidgetPlaylistAux : IViewWidgetPlaylistAux, IDisposable
    {
        public ViewWidgetPlaylistAux(Node aRoot)
        {
            iOpen = false;

            iNodeDeleteAll = aRoot.Search("CurrentPlaylist.DeleteAll");
            iNodeStatusBar = aRoot.Search("CurrentPlaylist.NumEntries");
            iNodeHeader = aRoot.Search("CurrentPlaylist.CurrentPlaylist_Text");
            iNodePlaylist = aRoot.Search("CurrentPlaylist.Playlist");
            iNodeGlow = aRoot.Search("CurrentPlaylist.Glow");

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (aMessage.Fullname == "CurrentPlaylist.Root")
            {
                MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                if (activeMsg != null)
                {
                    if (activeMsg.NewActive == true)
                    {
                        if (iOpen)
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeDeleteAll, false));
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeStatusBar, false));
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeHeader, false));
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodePlaylist, false));
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeGlow, false));
                        }
                    }
                }
            }
        }

        public void Open(string aType)
        {
            Assert.Check(!iOpen);

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iOpen = false;
            }
        }

        private bool iOpen;

        private Node iNodeDeleteAll;
        private Node iNodeStatusBar;
        private Node iNodeHeader;
        private Node iNodePlaylist;
        private Node iNodeGlow;
    }

    class ViewWidgetPlaylistDiscPlayer : IViewWidgetPlaylistDiscPlayer, IDisposable
    {
        public ViewWidgetPlaylistDiscPlayer(Node aRoot)
        {
            iOpen = false;

            iNodeDeleteAll = aRoot.Search("CurrentPlaylist.DeleteAll");
            iNodeStatusBar = aRoot.Search("CurrentPlaylist.NumEntries");
            iNodeHeader = aRoot.Search("CurrentPlaylist.CurrentPlaylist_Text");
            iNodePlaylist = aRoot.Search("CurrentPlaylist.Playlist");
            iNodeGlow = aRoot.Search("CurrentPlaylist.Glow");

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (aMessage.Fullname == "CurrentPlaylist.Root")
            {
                MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                if (activeMsg != null)
                {
                    if (activeMsg.NewActive == true)
                    {
                        if (iOpen)
                        {
                            //Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeDeleteAll, true));
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeDeleteAll, false));
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeStatusBar, false));
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeHeader, false));
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodePlaylist, false));
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeGlow, false));
                        }
                    }
                }
            }
        }

        public void Open()
        {
            Assert.Check(!iOpen);

            //Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeDeleteAll, true));

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iOpen = false;
            }
        }

        public void Initialised()
        {
        }

        public void Eject()
        {
        }

        private bool iOpen;

        private Node iNodeDeleteAll;
        private Node iNodeStatusBar;
        private Node iNodeHeader;
        private Node iNodePlaylist;
        private Node iNodeGlow;
    }

    class ViewWidgetPlayMode : IViewWidgetPlayMode, IDisposable
    {
        public ViewWidgetPlayMode(Node aRoot)
        {
            iKeyBindings = new KeyBindings();

            iNodePlaybackControlEx = aRoot.Search("Main.PlaybackControlEx");
            iNodeRepeating = aRoot.Search("Main.RepeatBlue");
            iNodeShuffling = aRoot.Search("Main.ShuffleBlue");

            iShuffle = (Monostable)aRoot.Search("Main.Shuffle").NextPlugin;
            iRepeat = (Monostable)aRoot.Search("Main.Repeat").NextPlugin;

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (aMessage.Fullname == "Main.Shuffle.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;
                if (stateMsg != null)
                {
                    if (stateMsg.NewState == true)
                    {
                        if (EventToggleShuffle != null)
                        {
                            EventToggleShuffle(this, EventArgs.Empty);
                        }
                    }
                }
            }

            if (aMessage.Fullname == "Main.Repeat.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;
                if (stateMsg != null)
                {
                    if (stateMsg.NewState == true)
                    {
                        if (EventToggleRepeat != null)
                        {
                            EventToggleRepeat(this, EventArgs.Empty);
                        }
                    }
                }
            }

            if (iOpen)
            {
                if (aMessage.Fullname == string.Empty)
                {
                    MsgKeyDown keyDown = aMessage as MsgKeyDown;
                    if (keyDown != null)
                    {
                        string action = iKeyBindings.Action(keyDown.Key);
                        Trace.WriteLine(Trace.kKinskyClassic, "ViewWidgetPlayMode.Receive: keyDown->action=" + action);
                        if (action == "Shuffle")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iShuffle, true));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Repeat")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iRepeat, true));
                            Renderer.Instance.Render();
                        }
                    }

                    MsgKeyUp keyUp = aMessage as MsgKeyUp;
                    if (keyUp != null)
                    {
                        string action = iKeyBindings.Action(keyUp.Key);
                        Trace.WriteLine(Trace.kKinskyClassic, "ViewWidgetPlayMode.Receive: keyUp->action=" + action);
                        if (action == "Shuffle")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iShuffle, false));
                            Renderer.Instance.Render();
                        }
                        else if (action == "Repeat")
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetState(iRepeat, false));
                            Renderer.Instance.Render();
                        }
                    }
                }
            }
        }

        public void Open()
        {
            Assert.Check(!iOpen);

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iNodePlaybackControlEx.Active = false;
                iNodeRepeating.Active = false;
                iNodeShuffling.Active = false;
                Renderer.Instance.Render();

                iOpen = false;
            }
        }

        public void Initialised()
        {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodePlaybackControlEx, true));
            Renderer.Instance.Render();
        }

        public void SetShuffle(bool aShuffle)
        {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeShuffling, aShuffle));
            Renderer.Instance.Render();
        }

        public void SetRepeat(bool aRepeat)
        {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeRepeating, aRepeat));
            Renderer.Instance.Render();
        }

        public event EventHandler<EventArgs> EventToggleShuffle;
        public event EventHandler<EventArgs> EventToggleRepeat;

        private bool iOpen;

        private KeyBindings iKeyBindings;

        private Node iNodePlaybackControlEx;
        private Node iNodeRepeating;
        private Node iNodeShuffling;

        private Monostable iShuffle;
        private Monostable iRepeat;
    }

    class ViewWidgetButtonSave : IViewWidgetButton
    {
        public ViewWidgetButtonSave(Node aRoot)
        {
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void SetEnabled(bool aEnabled)
        {
        }

        public event EventHandler<EventArgs> EventClick;
    }

    class ViewWidgetButtonWasteBin : IViewWidgetButton, IDisposable
    {
        public ViewWidgetButtonWasteBin(Node aRoot)
        {
            iOpen = false;
            iNodeDeleteAll = aRoot.Search("CurrentPlaylist.DeleteAll");

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            /*if (aMessage.Fullname == "CurrentPlaylist.DeleteAll")
            {
                MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
                
                if (msgActiveChanged != null)
                {
                    if (msgActiveChanged.NewActive == true)
                    {
                        if (!iOpen)
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeDeleteAll, false));
                        }
                    }
                }
            }*/

            if (aMessage.Fullname == "CurrentPlaylist.DeleteAll.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;
                
                if (stateMsg != null)
                {
                    if (stateMsg.NewState == false)
                    {
                        if (EventClick != null)
                        {
                            EventClick(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        public void Open()
        {
            Assert.Check(!iOpen);

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iOpen = false;
            }
        }

        public void SetEnabled(bool aEnabled)
        {
        }

        public event EventHandler<EventArgs> EventClick;

        private bool iOpen;
        private Node iNodeDeleteAll;
    }

    class ViewWidgetBreadcrumb : IViewWidgetLocation
    {
        public ViewWidgetBreadcrumb(Node aRoot, IControllerLocation aControllerLocation, IViewWidgetButton aViewWidgetButton)
        {
            iOpen = false;
            
            iNodeBreadcrumb = (NodeText)aRoot.Search("Library.Location");

            iControllerLocation = aControllerLocation;
            
            iViewWidgetButton = aViewWidgetButton;
            iViewWidgetButton.EventClick += ClickEvent;
        }

        public void Open()
        {
            Assert.Check(!iOpen);
            iOpen = true;
        }

        public void Close()
        {
            iOpen = false;
        }

        public void SetLocation(IList<string> aLocation)
        {
            if (aLocation.Count > 1)
            {
                iViewWidgetButton.Close();
                iViewWidgetButton.Open();
            }
            else
            {
                iViewWidgetButton.Close();
            }

            string location = aLocation[aLocation.Count - 1];
            Messenger.Instance.PresentationMessage(new MsgSetText(iNodeBreadcrumb, location));
        }

        private void ClickEvent(object sender, EventArgs e)
        {
            iControllerLocation.Up(1);
        }

        private bool iOpen;
        private NodeText iNodeBreadcrumb;
        private IControllerLocation iControllerLocation;
        private IViewWidgetButton iViewWidgetButton;
    }

    class ViewWidgetButtonSize : IViewWidgetButton
    {
        public ViewWidgetButtonSize(Node aRoot)
        {
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void SetEnabled(bool aEnabled)
        {
        }

        public event EventHandler<EventArgs> EventClick;
    }

    class ViewWidgetButtonView : IViewWidgetButton
    {
        public ViewWidgetButtonView(Node aRoot)
        {
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void SetEnabled(bool aEnabled)
        {
        }

        public event EventHandler<EventArgs> EventClick;
    }

    class ViewWidgetButtonUp : IViewWidgetButton, IDisposable
    {
        public ViewWidgetButtonUp(Node aRoot)
        {
            iOpen = false;

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (aMessage.Fullname == "Library.BackButton.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                if (stateMsg != null)
                {
                    if (stateMsg.NewState == false)
                    {
                        if (EventClick != null)
                        {
                            EventClick(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        public void Open()
        {
            Assert.Check(!iOpen);

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iOpen = false;
            }
        }

        public void SetEnabled(bool aEnabled)
        {
        }

        public event EventHandler<EventArgs> EventClick;

        private bool iOpen;
    }

    /*class ViewWidgetBrowser : ViewNodeList, IViewWidgetContent, IContentHandler
    {
        public class RendererBrowser : IListableRenderer
        {
            public RendererBrowser(Node aRoot)
            {
            }

            public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted)
            {
                NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
                hit.Width = aWidth;
                hit.Height = aHeight;

                NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
                poly.AllowHits = false;
                poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));

                AdapterBrowser.ListableItem item = aListable as AdapterBrowser.ListableItem;
                Assert.Check(item != null);

                NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
                text.AllowHits = false;
                text.LocalTranslation.X = poly.Width;
                text.Width = aWidth - poly.Width;// -(iStandby.Active && aHighlighted ? iStandby.Width : 0);
                text.Height = aHeight;
                text.Justification = aNodeFont.Justification;
                text.Alignment = aNodeFont.Alignment;
                text.Trimming = aNodeFont.Trimming;
                text.CurrFont = aNodeFont.CurrFont;
                text.Colour = aNodeFont.Colour;
                text.Text = item.Object.Text;
                hit.AddChild(poly);
                hit.AddChild(text);
                return hit;
            }

            //private NodePolygon iStandby = null;
        }

        public ViewWidgetBrowser(System.Windows.Forms.Control aCanvas, Node aRoot, IBrowser aBrowser)
            : base(aRoot, "Library.NasList")
        {
            iOpen = false;
            iCanvas = aCanvas;

            iAdapter = new AdapterBrowser();

            iList.AutoSelect = true;
            iList.ListEntryProvider = iAdapter;
            iList.ListableRenderer = new RendererBrowser(aRoot);
            iList.Refresh();

            iNodeDirectoryList = (NodeHit)aRoot.Search("Library.DirectoryList");
            iNodeLocationStart = aRoot.Search("Library.LocationStart");
            
            iScrollBar = (NodeHit)aRoot.Search("Library.NasScrollbar");
			
			iBrowser = aBrowser;
            iBrowser.EventLocationChanged += LocationChanged;

            //Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Receive(Message aMessage)
        {
            if (aMessage.Plugin == iNodeDirectoryList)
            {
                MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                if (activeMsg != null)
                {
                    if (activeMsg.NewActive == true)
                    {
                        iNodeDirectoryList.Active = false;
                    }
                }
            }
            if (aMessage.Plugin == iList)
            {
                 MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                if (activeMsg != null)
                {
                    if (activeMsg.NewActive == true)
                    {
                        iNodeLocationStart.Active = false;

                        if (!iOpen)
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
                            DoSwapControl(null);
                        }
                        else
                        {
                            DoSwapControl(iControl);
                        }
                    }
                    else
                    {
                        DoSwapControl(null);
                    }

                    SetScrollbar(iList.TopEntry);
                }

                MsgHighlight highlightMsg = aMessage as MsgHighlight;

                if (highlightMsg != null)
                {
                    //SetRoomStatusText(highlightMsg.Index);

                    AdapterBrowser.ListableItem item = highlightMsg.Listable as AdapterBrowser.ListableItem;
                    //iHighlightedIndex = iAdapter.Index(item);
                }

                // handle item selected

                MsgSelect selectMsg = aMessage as MsgSelect;

                if (selectMsg != null)
                {
                    if (iControl is System.Windows.Forms.ListView)
                    {
                        System.Windows.Forms.ListView list = iControl as System.Windows.Forms.ListView;
                        //list.Items[0].S
                    }
                }

                // handle top entry changed

                MsgTopEntry topMsg = aMessage as MsgTopEntry;

                if (topMsg != null)
                {
                    SetScrollbar(topMsg.Index);
                }
            }

            if (aMessage.Plugin == iScrollBar)
            {
                MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;

                if (msgActiveChanged != null)
                {
                    if (msgActiveChanged.NewActive == true)
                    {
                        if (!iOpen)
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
                        }
                    }
                }

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

        public void Open()
        {
            Assert.Check(!iOpen);

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iAdapter.Clear();

                SetScrollbar(iList.TopEntry);
                iList.Refresh();

                iOpen = false;
            }
        }
		
        public void Open(uint aCount)
        {
            iMutex.WaitOne();
            
            UpdateStarted(iContainer, aCount, string.Empty);
            
            if (aCount > 0)
            {
                iContentCollector.Range(0, aCount);
            }
            else
            {
                UpdateFinished(iContainer, string.Empty);
            }
            
            iMutex.ReleaseMutex();
        }
		
        public void Item(uint aIndex, upnpObject aObject)
        {
            iMutex.WaitOne();

            List<upnpObject> list = new List<upnpObject>();
            list.Add(aObject);
            Update(iContainer, list, string.Empty);

            iMutex.ReleaseMutex();
        }

        public void Items(uint aStartIndex, IList<upnpObject> aObjectList)
        {
        }

        public void ContentError(string aMessage)
        {
        }
		
        private void LocationChanged(object sender, EventArgs e)
        {
            OnLocationChanged();
        }

        private void OnLocationChanged()
        {
            iMutex.WaitOne();

            Connecting();
            iContainer = iBrowser.Location.Current;
            if (iContentCollector != null)
            {
                iMutex.ReleaseMutex();

                iContentCollector.Dispose();

                iMutex.WaitOne();
            }
            iContentCollector = ContentCollectorMaster.Create(iContainer, this);

            iMutex.ReleaseMutex();
        }

        private void SetScrollbar(uint aPosition)
        {
            if (iAdapter.Count <= iList.LineCount)
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
            }
            else
            {
                int rangeMax = (int)iAdapter.Count - (int)iList.LineCount;
                //System.Console.WriteLine("ViewSystem.SetRoomScrollbar: aPosition=" + aPosition + ", iModel.RoomList.Count=" + iModel.RoomList.Count + ", rangeMax=" + rangeMax);
                if (rangeMax > 0)
                {
                    SetScrollbar(aPosition / (float)rangeMax);
                }
                else
                {
                    SetScrollbar(0.0f);
                }
            }
        }

        private void SetScrollbar(float aPosition)
        {
            if (iAdapter.Count <= iList.LineCount)
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
            }
            else
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, iList.Active));
                Messenger.Instance.PresentationMessage(new MsgSetPosition(iScrollBar, aPosition));
            }
        }

        private Mutex iMutex;
        private bool iOpen;

        private IBrowser iBrowser;
        private AdapterBrowser iAdapter;

        private NodeHit iNodeDirectoryList;
        private NodeHit iScrollBar;
        private Node iNodeLocationStart;
    }*/

    class ViewWidgetBrowser : ViewNodeList, IViewWidgetContent, IContentHandler, IDisposable
    {
        public class RendererLibrary : IListableRenderer
        {
            public RendererLibrary(Node aRoot)
            {
                iNodeInsert = (NodeHit)aRoot.Search("Library.Insert");
            }

            public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted)
            {
                NodeHit hit = (NodeHit)PluginFactory.Instance.Create("NodeHit");
                hit.Width = aWidth;
                hit.Height = aHeight;

                NodePolygon poly = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
                poly.AllowHits = false;

                AdapterLibrary.ListableItem item = aListable as AdapterLibrary.ListableItem;
                Assert.Check(item != null);

                if (item.Object is container)
                {
                    IconDirectory(poly, item.Object, aHeight, aHighlighted);
                }
                else
                {
                    poly.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/spacer.png")));
                }

                NodeText text = Text(item.Object, poly.Width, aWidth - poly.Width, aHeight, aNodeFont, aHighlighted);
                text.AllowHits = false;

                hit.AddChild(poly);
                hit.AddChild(text);
                
                return hit;
            }

            private void IconDirectory(NodePolygon aNodePolygon, upnpObject aObject, float aHeight, bool aHighlighted)
            {
                //string uri = DidlLiteAdapter.ArtworkUri(aObject);
                //if (uri != string.Empty)
                //{
                //    aNodePolygon.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad(uri)));
                //    aNodePolygon.ClampToTextureSize = false;
                //    aNodePolygon.Width = aHeight;
                //    aNodePolygon.Height = aHeight;
                //}
                //else
                //{
                    if (aHighlighted)
                    {
                        aNodePolygon.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/folder.png")));
                    }
                    else
                    {
                        aNodePolygon.Texture(new ReferenceTexture(TextureManager.Instance.TextureByNameOrLoad("Bitmaps/folder_outline.png")));
                    }
                //}

            }

            private NodeText Text(upnpObject aObject, float aX, float aWidth, float aHeight, NodeFont aNodeFont, bool aHighlighted)
            {
                NodeText text = (NodeText)PluginFactory.Instance.Create("NodeText");
                text.AllowHits = false;
                text.LocalTranslation.X = aX;
                text.Width = aWidth - (iNodeInsert.Active && aHighlighted ? iNodeInsert.Width : 0);
                text.Height = aHeight;
                text.Justification = aNodeFont.Justification;
                text.Alignment = aNodeFont.Alignment;
                text.Trimming = aNodeFont.Trimming;
                text.CurrFont = aNodeFont.CurrFont;
                text.Colour = aNodeFont.Colour;
                text.Text = DidlLiteAdapter.Title(aObject);
                return text;
            }

            private NodeHit iNodeInsert;
        }

        public ViewWidgetBrowser(Node aRoot, IPlaylistSupport aSupport, IBrowser aBrowser)
            : base(aRoot, "Library.NasList")
        {
            iSupport = aSupport;
            iSupport.EventIsInsertingChanged += IsInsertingChanged;

            iOpen = false;
            iShowingServers = false;

            iSubFocus = true;

			iMutex = new Mutex(false);
			
            iKeyBindings = new KeyBindings();

            iAdapter = new AdapterLibrary();

            iSearchClearTimer = new System.Threading.Timer(ClearSearchString, null, Timeout.Infinite, Timeout.Infinite);

            iList.AutoSelect = true;
            iList.ListEntryProvider = iAdapter;
            iList.ListableRenderer = new RendererLibrary(aRoot);
            iList.Refresh();

            iNodeGlow = aRoot.Search("CurrentPlaylist.Glow");

            iPlaylist = aRoot.Search("CurrentPlaylist.Playlist");
            iNodeDirectoryList = (NodeHit)aRoot.Search("Library.DirectoryList");
            iNodeLocationStart = aRoot.Search("Library.LocationStart");

            iStatusBar = iRoot.Search("Library.NasNumEntries");
            iScrollBar = (NodeHit)aRoot.Search("Library.NasScrollbar");
            iNodeBreadcrumb = (NodeText)aRoot.Search("Library.Location");

            Bistable bistable = (Bistable)aRoot.Search("Library.InsertAtEnd").NextPlugin;
            iPlayLater = bistable.State;

            iBackButton = (Monostable)aRoot.Search("Library.BackButton").NextPlugin;
            iInsertAtEnd = (Bistable)aRoot.Search("Library.InsertAtEnd").NextPlugin;
            iInsert = (Monostable)aRoot.Search("Library.Insert").NextPlugin;
			
			iBrowser = aBrowser;
            iBrowser.EventLocationChanged += LocationChanged;

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Dispose()
        {
            Messenger.Instance.EEventAppMessage -= Receive;
        }

        public void Receive(Message aMessage)
        {
            if (!ProcessKeyboard(aMessage))
            {
                if (aMessage.Plugin == iNodeDirectoryList)
                {
                    MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                    if (activeMsg != null)
                    {
                        if (activeMsg.NewActive == true)
                        {
                            iNodeDirectoryList.Active = false;
                        }
                    }
                }
                if (aMessage.Plugin == iList)
                {
                    MsgActiveChanged activeMsg = aMessage as MsgActiveChanged;

                    if (activeMsg != null)
                    {
                        if (activeMsg.NewActive == true)
                        {
                            iNodeLocationStart.Active = false;

                            if (!iOpen)
                            {
                                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
                            }
                            else
                            {
                                SetInsertGlow();
                            }
                        }

                        SetScrollbar(iList.TopEntry);
                    }

                    MsgHighlight highlightMsg = aMessage as MsgHighlight;

                    if (highlightMsg != null)
                    {
                        SetStatusText(highlightMsg.Index);
                    }

                    // handle item selected

                    MsgSelect selectMsg = aMessage as MsgSelect;

                    if (selectMsg != null)
                    {
                        AdapterLibrary.ListableItem item = selectMsg.Listable as AdapterLibrary.ListableItem;
                        if (item.Object is container)
                        {
                            iBrowser.Down(item.Object as container);
                        }
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

                    // handle top entry changed

                    MsgTopEntry topMsg = aMessage as MsgTopEntry;

                    if (topMsg != null)
                    {
                        SetScrollbar(topMsg.Index);
                    }

                    // handle focus changed

                    MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;

                    if (focusMsg != null)
                    {
                        iSubFocus = focusMsg.NewFocus;
                        SetStatusText(iList.HighlightedIndex);
                        if (iSubFocus)
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetFocus(iPlaylist, false));
                        }
                    }
                }

                if (aMessage.Plugin == iPlaylist)
                {
                    // handle focus changed

                    MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;

                    if (focusMsg != null)
                    {
                        if (focusMsg.NewFocus)
                        {
                            Messenger.Instance.PresentationMessage(new MsgSetFocus(iList, false));
                        }
                    }
                }

                if (aMessage.Plugin == iScrollBar)
                {
                    MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;

                    if (msgActiveChanged != null)
                    {
                        if (msgActiveChanged.NewActive == true)
                        {
                            if (!iOpen)
                            {
                                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
                            }
                        }
                    }

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

                /*if (aMessage.Plugin == iBackButton)
                {
                    MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                    if (stateMsg != null)
                    {
                        if (stateMsg.NewState == false)
                        {
                            iBrowser.Up(1);
                        }
                    }
                }*/

                if (aMessage.Fullname == "Library.Insert.Monostable")
                {
                    MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                    if (stateMsg != null)
                    {
                        if (stateMsg.NewState == false)
                        {
                            AdapterLibrary.ListableItem item = iList.HighlightedItem as AdapterLibrary.ListableItem;

                            List<upnpObject> list = new List<upnpObject>();
                            list.Add(item.Object);

                            if (iPlayLater)
                            {
                                iSupport.PlayLater(new MediaRetrieverNoRetrieve(list));
                            }
                            else
                            {
                                iSupport.PlayNext(new MediaRetrieverNoRetrieve(list));
                            }
                        }
                    }
                }

                if (aMessage.Fullname == "Library.InsertAtEnd.Bistable")
                {
                    MsgStateChanged stateMsg = aMessage as MsgStateChanged;

                    if (stateMsg != null)
                    {
                        iPlayLater = stateMsg.NewState;
                    }
                }

                if (iList.Active)
                {
                    if (aMessage.Fullname == string.Empty)
                    {
                        MsgKeyDown keyDown = aMessage as MsgKeyDown;

                        if (keyDown != null)
                        {
                            string action = iKeyBindings.Action(keyDown.Key);
                            if (iSubFocus)
                            {
                                iSearchClearTimer.Change(Timeout.Infinite, Timeout.Infinite);
                                if (action == "Back")
                                {
                                    Messenger.Instance.PresentationMessage(new MsgSetState(iBackButton, true));
                                    Renderer.Instance.Render();
                                }
                                if (action == string.Empty)
                                {
                                    if (keyDown.Key == System.Windows.Forms.Keys.Back)
                                    {
                                        if (iSearchString.Length > 0)
                                        {
                                            iSearchString = iSearchString.Remove(iSearchString.Length - 1, 1);
                                            Trace.WriteLine(Trace.kKinskyClassic, "ViewOssKinskyMppLibrary.Receive: iSearchString=" + iSearchString);
                                            FindEntryByString(iSearchString);
                                        }
                                    }
                                    else if ((keyDown.Key & System.Windows.Forms.Keys.Alt) != System.Windows.Forms.Keys.Alt && (keyDown.Key & System.Windows.Forms.Keys.Control) != System.Windows.Forms.Keys.Control)
                                    {
                                        iSearchString += (char)keyDown.Key;
                                        Trace.WriteLine(Trace.kKinskyClassic, "ViewOssKinskyMppLibrary.Receive: iSearchString=" + iSearchString);
                                        FindEntryByString(iSearchString);
                                    }
                                }
                                iSearchClearTimer.Change(kKeyTimeOut, Timeout.Infinite);
                            }

                            if (action == "SwapFocus")
                            {
                                bool focus = iSubFocus;
                                Messenger.Instance.PresentationMessage(new MsgSetFocus(iPlaylist, focus));
                                Messenger.Instance.PresentationMessage(new MsgSetFocus(iList, !focus));
                            }
                        }

                        MsgKeyUp keyUp = aMessage as MsgKeyUp;

                        if (keyUp != null)
                        {
                            if (iSubFocus)
                            {
                                string action = iKeyBindings.Action(keyUp.Key);
                                if (action == "Back")
                                {
                                    Messenger.Instance.PresentationMessage(new MsgSetState(iBackButton, false));
                                    Renderer.Instance.Render();
                                }
                            }
                        }
                    }
                }

                if (iOpen)
                {
                    if (aMessage.Fullname == string.Empty)
                    {
                        MsgKeyDown keyDown = aMessage as MsgKeyDown;
                        if (keyDown != null)
                        {
                            string action = iKeyBindings.Action(keyDown.Key);
                            Trace.WriteLine(Trace.kKinskyClassic, "ViewOssKinskyMppLibrary.Receive: keyDown->action=" + action);
                            if (action == "InsertMode")
                            {
                                Messenger.Instance.PresentationMessage(new MsgToggleState(iInsertAtEnd));
                                Renderer.Instance.Render();
                            }
                            else if (action == "Insert")
                            {
                                Messenger.Instance.PresentationMessage(new MsgSetState(iInsert, true));
                                Renderer.Instance.Render();
                            }
                        }

                        MsgKeyUp keyUp = aMessage as MsgKeyUp;
                        if (keyUp != null)
                        {
                            string action = iKeyBindings.Action(keyUp.Key);
                            Trace.WriteLine(Trace.kKinskyClassic, "ViewOssKinskyMppLibrary.Receive: keyUp->action=" + action);
                            if (action == "Insert")
                            {
                                Messenger.Instance.PresentationMessage(new MsgSetState(iInsert, false));
                                Renderer.Instance.Render();
                            }
                        }
                    }
                }
            }
        }

        public void Open()
        {
            Assert.Check(!iOpen);

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iAdapter.Clear();

                SetScrollbar(iList.TopEntry);
                iList.Refresh();

                iOpen = false;
            }
        }

        public void Open(IContentCollector aCollector, uint aCount)
        {
            iMutex.WaitOne();
            
            UpdateStarted(iContainer, aCount, string.Empty);
            
            if (aCount > 0)
            {
                iContentCollector.Range(0, aCount);
            }
            else
            {
                UpdateFinished(iContainer, string.Empty);
            }
            
            iMutex.ReleaseMutex();
        }
		
        public void Item(IContentCollector aCollector, uint aIndex, upnpObject aObject)
        {
            iMutex.WaitOne();

            List<upnpObject> list = new List<upnpObject>();
            list.Add(aObject);
            Update(iContainer, list, string.Empty);

            iMutex.ReleaseMutex();
        }

        public void Items(IContentCollector aCollector, uint aStartIndex, IList<upnpObject> aObjectList)
        {
        }

        public void ContentError(IContentCollector aCollector, string aMessage)
        {
        }

        public void Connecting()
        {
        }

        public void UpdateStarted(IContainer aContainer, uint aCount, string iSelectedId)
        {
            iContainer = aContainer;
            iShowingServers = (aContainer.Metadata.Id == ModelLibrary.kLibraryId);

            Messenger.Instance.PresentationMessage(new MsgSetText(iNodeBreadcrumb, aContainer.Metadata.Title));
            iAdapter.Clear();

            SetStatusText(iList.HighlightedIndex);
            
            iList.Refresh();
        }

        public void Update(IContainer aContainer, IList<upnpObject> aList, string iSelectedId)
        {
            iAdapter.AddItems(aList);

            SetStatusText(iList.HighlightedIndex);
            SetScrollbar(iList.TopEntry);
            
            iList.Refresh();
        }

        public void UpdateFinished(IContainer aContainer, string iSelectedId)
        {
            SetStatusText(iList.HighlightedIndex);
        }

        public void OnSizeClick()
        {
            throw new NotSupportedException();
        }

        public void OnViewClick()
        {
            throw new NotSupportedException();
        }

        public void DoDragDrop(MediaProviderDraggable aDraggable)
        {
            throw new NotSupportedException();
        }

        public void Focus() { }
		
		private void LocationChanged(object sender, EventArgs e)
        {
            OnLocationChanged();
        }

        private void OnLocationChanged()
        {
            iMutex.WaitOne();

            Connecting();
            iContainer = iBrowser.Location.Current;
            if (iContentCollector != null)
            {
                iMutex.ReleaseMutex();

                iContentCollector.Dispose();

                iMutex.WaitOne();
            }
            iContentCollector = ContentCollectorMaster.Create(iContainer, this);

            iMutex.ReleaseMutex();
        }

        private void SetScrollbar(uint aPosition)
        {
            if (iAdapter.Count <= iList.LineCount)
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
            }
            else
            {
                int rangeMax = (int)iAdapter.Count - (int)iList.LineCount;
                //System.Console.WriteLine("ViewSystem.SetRoomScrollbar: aPosition=" + aPosition + ", iModel.RoomList.Count=" + iModel.RoomList.Count + ", rangeMax=" + rangeMax);
                if (rangeMax > 0)
                {
                    SetScrollbar(aPosition / (float)rangeMax);
                }
                else
                {
                    SetScrollbar(0.0f);
                }
            }
        }

        private void SetScrollbar(float aPosition)
        {
            if (iAdapter.Count <= iList.LineCount)
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, false));
            }
            else
            {
                Messenger.Instance.PresentationMessage(new MsgSetActive(iScrollBar, iList.Active));
                Messenger.Instance.PresentationMessage(new MsgSetPosition(iScrollBar, aPosition));
            }
        }

        private void SetStatusText(int aIndex)
        {
            uint count = iAdapter.Count;

            string statusText = string.Empty;

            if (aIndex > -1 && iSubFocus)
            {
                statusText = (aIndex + 1).ToString() + " of ";
            }

            statusText += count.ToString();
            
            if (iShowingServers)
            {
                statusText += " " + kLibraryStatusText + (count == 1 ? "" : "s");
            }
            
            Messenger.Instance.PresentationMessage(new MsgSetText(iStatusBar, statusText));
        }

        private void IsInsertingChanged(object sender, EventArgs e)
        {
            SetInsertGlow();
        }

        private void SetInsertGlow()
        {
            Messenger.Instance.PresentationMessage(new MsgSetActive(iNodeGlow, iSupport.IsInserting()));
        }

        private void ClearSearchString(object aSender)
        {
            iSearchString = "";
        }

        private void FindEntryByString(string aSearchString)
        {
            Trace.WriteLine(Trace.kKinskyClassic, "ViewOssKinskyMppLibrary.FindEntryByString");
            if ((iContainer is musicAlbum) || (iContainer is playlistContainer) || (iContainer is storageFolder))
            {
                // if our parent is a music album, a playlist container or a storage folder we dont allow quick key search
                return;
            }
            uint count = iAdapter.Count;
            if (count > 0)
            {
                // get first stab at index for letter
                uint index = (uint)Math.Round((float)count / 2.0f);
                int result = FindEntryByString(aSearchString, index, 0, count);
                Trace.WriteLine(Trace.kKinskyClassic, "ViewOssKinskyMppLibrary.FindEntryByString: result=" + result);
                if (result != -1 && result != iList.HighlightedIndex)
                {
                    Messenger.Instance.PresentationMessage(new MsgSetHighlight(iList, result, NodeList.EAlignment.EA_Centre));
                }
            }
        }

        private int FindEntryByString(string aSearchString, uint aIndex, uint aRangeMin, uint aRangeMax)
        {
            Trace.WriteLine(Trace.kKinskyClassic, "ViewOssKinskyMppLibrary.FindEntryByString: aIndex=" + aIndex + ", aRangeMin=" + aRangeMin + ", aRangeMax=" + aRangeMax);
            IListable listable = iAdapter.Entry(aIndex);
            AdapterLibrary.ListableItem obj = listable as AdapterLibrary.ListableItem;
            string title = DidlLiteAdapter.Title(obj.Object);
            if (string.Compare(title, 0, aSearchString, 0, aSearchString.Length, true) < 0)
            {
                uint index = (uint)Math.Round(((float)(aRangeMax - aIndex - 0.5f) / 2.0f) + aIndex);
                Trace.WriteLine(Trace.kKinskyClassic, "ViewOssKinskyMppLibrary.FindEntryByString(<): index=" + index);
                if (index == aIndex)
                {   // we can't find an item starting with the required letter, return first after desired letter
                    return (int)aIndex;
                }
                return FindEntryByString(aSearchString, index, aIndex + 1, aRangeMax);
            }
            else
            {
                if ((string.Compare(title, 0, aSearchString, 0, aSearchString.Length, true) == 0 && aIndex == aRangeMin) || (aRangeMax - aRangeMin == 0))
                {
                    return (int)aIndex;
                }
                uint index = (uint)Math.Round(((float)(aIndex - aRangeMin - 0.5f) / 2.0f) + aRangeMin);
                Trace.WriteLine(Trace.kKinskyClassic, "ViewOssKinskyMppLibrary.FindEntryByString(=>): index=" + index);
                if (index == aIndex)
                {   // we can't find an item starting with the required letter, return first after desired letter
                    return (int)aIndex;
                }
                return FindEntryByString(aSearchString, index, aRangeMin, (string.Compare(title, 0, aSearchString, 0, aSearchString.Length, true) == 0) ? aIndex + 1 : aIndex);
            }
        }

        private const int kKeyTimeOut = 1000;
        private const string kLibraryStatusText = "server";

        private IPlaylistSupport iSupport;

		private Mutex iMutex;
        private bool iOpen;
        private bool iShowingServers;
        private bool iPlayLater;

        private bool iSubFocus;

        private IContainer iContainer;
        private IBrowser iBrowser;
        private IContentCollector iContentCollector;

        private AdapterLibrary iAdapter;

        private Node iNodeGlow;
        private NodeHit iNodeDirectoryList;
        private NodeHit iScrollBar;
        private NodeText iNodeBreadcrumb;
        private Node iNodeLocationStart;
        private Node iStatusBar;
        private Node iPlaylist;

        private Monostable iBackButton;
        private Bistable iInsertAtEnd;
        private Monostable iInsert;

        private System.Threading.Timer iSearchClearTimer;
	    private string iSearchString = "";
    }

    class ViewWidgetReceivers : IViewWidgetReceivers
    {
        public ViewWidgetReceivers(Node aRoot)
        {
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void SetSender(ModelSender aSender)
        {
        }
    }
} // KinskyClassic
