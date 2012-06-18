using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

using Linn;
using Linn.Kinsky;
using Linn.Topology;
using Upnp;

namespace KinskyDesktop
{
    internal class ViewContextMenu
    {
        public ViewContextMenu()
        {
            Initialise();
        }

        public ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return iContextMenu;
            }
        }

        public void SetOpenEnabled(bool aValue)
        {
            if (iContextMenu.InvokeRequired)
            {
                iContextMenu.BeginInvoke((MethodInvoker)delegate()
                {
                    iContextMenuItemOpen.Enabled = aValue;
                });
            }
            else
            {
                iContextMenuItemOpen.Enabled = aValue;
            }
        }

        public void SetPlayEnabled(bool aValue)
        {
            if (iContextMenu.InvokeRequired)
            {
                iContextMenu.BeginInvoke((MethodInvoker)delegate()
                {
                    iContextMenuItemPlayNow.Enabled = aValue;
                    iContextMenuItemPlayNext.Enabled = aValue;
                    iContextMenuItemPlayLater.Enabled = aValue;
                });
            }
            else
            {
                iContextMenuItemPlayNow.Enabled = aValue;
                iContextMenuItemPlayNext.Enabled = aValue;
                iContextMenuItemPlayLater.Enabled = aValue;
            }
        }

        public void SetDeleteEnabled(bool aValue)
        {
            if (iContextMenu.InvokeRequired)
            {
                iContextMenu.BeginInvoke((MethodInvoker)delegate()
                {
                    iContextMenuItemDelete.Enabled = aValue;
                });
            }
            else
            {
                iContextMenuItemDelete.Enabled = aValue;
            }
        }

        public void SetRenameEnabled(bool aValue)
        {
            if (iContextMenu.InvokeRequired)
            {
                iContextMenu.BeginInvoke((MethodInvoker)delegate()
                {
                    iContextMenuItemRename.Enabled = aValue;
                });
            }
            else
            {
                iContextMenuItemRename.Enabled = aValue;
            }
        }

        public EventHandler<EventArgs> EventOpenClicked;

        public EventHandler<EventArgs> EventPlayNowClicked;
        public EventHandler<EventArgs> EventPlayNextClicked;
        public EventHandler<EventArgs> EventPlayLaterClicked;

        public EventHandler<EventArgs> EventDeleteClicked;
        public EventHandler<EventArgs> EventRenameClicked;

        private void Initialise()
        {
            iContextMenu = new ContextMenuStrip();
            iContextMenu.Name = "ContextMenuStripLibrary";
            iContextMenu.Size = new System.Drawing.Size(166, 176);

            ToolStripMenuItem toolStripMenuItem = null;
            ToolStripSeparator toolStripMenuSeperator = null;

            // Open
            toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Enabled = false;
            toolStripMenuItem.Name = "ToolStripMenuItemOpen";
            toolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            toolStripMenuItem.Text = "Open";
            iContextMenuItemOpen = toolStripMenuItem;
            iContextMenu.Items.Add(toolStripMenuItem);

            iContextMenuItemOpen.Click += EventOpen_Click;

            // Seperator
            toolStripMenuSeperator = new ToolStripSeparator();
            toolStripMenuSeperator.Name = "toolStripMenuItemSeperator1";
            toolStripMenuSeperator.Size = new System.Drawing.Size(162, 6);
            iContextMenu.Items.Add(toolStripMenuSeperator);

            // PlayNow
            toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Enabled = false;
            toolStripMenuItem.Name = "ToolStripMenuItemPlayNow";
            toolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            toolStripMenuItem.Text = "Play Now";
            iContextMenuItemPlayNow = toolStripMenuItem;
            iContextMenu.Items.Add(toolStripMenuItem);

            iContextMenuItemPlayNow.Click += EventPlayNow_Click;
            
            // PlayNext
            toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Enabled = false;
            toolStripMenuItem.Name = "ToolStripMenuItemPlayNext";
            toolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            toolStripMenuItem.Text = "Play Next";
            iContextMenuItemPlayNext = toolStripMenuItem;
            iContextMenu.Items.Add(toolStripMenuItem);

            iContextMenuItemPlayNext.Click += EventPlayNext_Click;

            // PlayLater
            toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Enabled = false;
            toolStripMenuItem.Name = "ToolStripMenuItemPlayLater";
            toolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            toolStripMenuItem.Text = "Play Later";
            iContextMenuItemPlayLater = toolStripMenuItem;
            iContextMenu.Items.Add(toolStripMenuItem);

            iContextMenuItemPlayLater.Click += EventPlayLater_Click;

            // Seperator
            toolStripMenuSeperator = new ToolStripSeparator();
            toolStripMenuSeperator.Name = "toolStripMenuItemSeperator2";
            toolStripMenuSeperator.Size = new System.Drawing.Size(162, 6);
            iContextMenu.Items.Add(toolStripMenuSeperator);

            // Delete
            toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Enabled = false;
            toolStripMenuItem.Name = "ToolStripMenuItemDelete";
            toolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            toolStripMenuItem.Text = "Delete";
            iContextMenuItemDelete = toolStripMenuItem;
            iContextMenu.Items.Add(toolStripMenuItem);

            iContextMenuItemDelete.Click += EventDelete_Click;

            // Rename
            toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Enabled = false;
            toolStripMenuItem.Name = "ToolStripMenuItemRename";
            toolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            toolStripMenuItem.Text = "Rename";
            iContextMenuItemRename = toolStripMenuItem;
            iContextMenu.Items.Add(toolStripMenuItem);

            iContextMenuItemRename.Click += EventRename_Click;

            // Seperator
            //toolStripMenuItem = new ToolStripSeparator();
            //toolStripMenuItem.Name = "toolStripMenuItemSeperator3";
            //toolStripMenuItem.Size = new System.Drawing.Size(162, 6);
            //iContextMenu.Items.Add(toolStripMenuItem);
        }

        private void EventOpen_Click(object sender, EventArgs e)
        {
            if (EventOpenClicked != null)
            {
                EventOpenClicked(this, EventArgs.Empty);
            }
        }

        private void EventPlayNow_Click(object sender, EventArgs e)
        {
            if (EventPlayNowClicked != null)
            {
                EventPlayNowClicked(this, EventArgs.Empty);
            }
        }

        private void EventPlayNext_Click(object sender, EventArgs e)
        {
            if (EventPlayNextClicked != null)
            {
                EventPlayNextClicked(this, EventArgs.Empty);
            }
        }

        private void EventPlayLater_Click(object sender, EventArgs e)
        {
            if (EventPlayLaterClicked != null)
            {
                EventPlayLaterClicked(this, EventArgs.Empty);
            }
        }

        private void EventDelete_Click(object sender, EventArgs e)
        {
            if (EventDeleteClicked != null)
            {
                EventDeleteClicked(this, EventArgs.Empty);
            }
        }

        private void EventRename_Click(object sender, EventArgs e)
        {
            if (EventRenameClicked != null)
            {
                EventRenameClicked(this, EventArgs.Empty);
            }
        }

        private ContextMenuStrip iContextMenu;
        private ToolStripMenuItem iContextMenuItemOpen;
        private ToolStripMenuItem iContextMenuItemPlayNow;
        private ToolStripMenuItem iContextMenuItemPlayNext;
        private ToolStripMenuItem iContextMenuItemPlayLater;
        private ToolStripMenuItem iContextMenuItemDelete;
        private ToolStripMenuItem iContextMenuItemRename;
    }

    internal class ViewWidgetBrowser : IViewWidgetContent, IContentHandler
    {
        public ViewWidgetBrowser(FormKinskyDesktop aForm, IBrowser aBrowser, IArtworkCache aArtworkCache, IViewSupport aViewSupport, IPlaylistSupport aPlaySupport, DropConverter aDropConverter, UiOptions aUiOptions)
        {
            iMutex = new Mutex(false);
            iListViewItems = new List<ListViewItem>();
            iListViewGroups = new SortedList<string, ListViewGroup>();
            iListViewDummyItems = new List<ListViewItem>();
            iArtworkUris = new List<string>();

            iViewSupport = aViewSupport;
            iPlaySupport = aPlaySupport;
            iViewSupport.EventSupportChanged += EventSupportChanged;

            iUiOptions = aUiOptions;

            iDropConverter = aDropConverter;

            iTextBoxSearch = new TextBox();
            iTextBoxSearch.Visible = false;
            iTextBoxSearch.BorderStyle = BorderStyle.None;
            iTextBoxSearch.Dock = DockStyle.Top;
            iTextBoxSearch.Name = "TextBoxSearch";
            iPanel = new Widgets.PanelBusy();
            iPanel.Visible = false;
            iPanel.Dock = DockStyle.Fill;
            iPanel.Font = iViewSupport.FontMedium;
            iPanel.ForeColor = iViewSupport.ForeColour;

            iListView = new Widgets.ListViewLibrary();
            iListView.BorderStyle = BorderStyle.None;
            iListView.Dock = DockStyle.Fill;
            iListView.FullRowSelect = true;
            iListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            iListView.HideSelection = false;
            iListView.Name = "ViewBrowser";
            iListView.ShowItemToolTips = true;
            iListView.AllowDrop = true;

            // the visibility of the list view needs to be tracked independently to the
            // iListView.Visible property. This is because, even if the iListView.Visible
            // is set to true, if the parent control is not visible, iListView.Visible
            // can return false
            iListViewVisible = true;

            SetViewColours();

            aForm.PanelBrowser.Controls.Add(iPanel);
            iPanel.Controls.Add(iListView);
            iPanel.Controls.Add(iTextBoxSearch);
            
            iContextMenu = new ViewContextMenu();
            iListView.ContextMenuStrip = iContextMenu.ContextMenuStrip;

            iArtworkCache = aArtworkCache;
            iArtworkCache.EventUpdated += EventUpdated;

            iBrowser = aBrowser;
            iBrowser.EventLocationChanged += LocationChanged;
            LocationChanged(iBrowser, EventArgs.Empty);

            iViewWidgetButtonSize = new ViewWidgetButtonSize(aForm);
            iViewWidgetButtonSize.EventClick += EventSizeChanged;

            iViewWidgetButtonView = new ViewWidgetButtonView(aForm);
            iViewWidgetButtonView.EventClick += EventViewChanged;

            iOpen = false;
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                iMutex.WaitOne();

                Console.WriteLine(iTextBoxSearch.Text);
                if (iContainer != null)
                {
                    if (iTextBoxSearch.Text.Length > 0)
                    {
                        try
                        {
                            DidlLite didl = iContainer.Search(string.Format("dc:title contains \"{0}\" or upnp:artist contains \"{0}\" or upnp:album contains \"{0}\"", iTextBoxSearch.Text), 0, 0);
                            //Console.WriteLine(didl.Xml);
                            UpdateStarted(null, (uint)didl.Count, string.Empty);
                            Update(null, didl, string.Empty);
                        }
                        catch (Exception)
                        { }
                    }
                }

                iMutex.ReleaseMutex();
            }
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iListView.ItemActivate += ItemActivate;
            iListView.SelectedIndexChanged += SelectedIndexChanged;

            iListView.ItemDrag += ItemDrag;
            iListView.DragEnter += DragEnter;
            //iListView.DragOver += DragOver;
            iListView.DragDrop += DragDrop;

            iListView.AfterLabelEdit += EventAfterLabelEdit;

            iListView.MouseUp += EventMouseUp;

            iContextMenu.EventOpenClicked += ItemActivate;
            iContextMenu.EventPlayNowClicked += EventContextMenu_PlayNow;
            iContextMenu.EventPlayNextClicked += EventContextMenu_PlayNext;
            iContextMenu.EventPlayLaterClicked += EventContextMenu_PlayLater;
            iContextMenu.EventDeleteClicked += EventContextMenu_Delete;
            iContextMenu.EventRenameClicked += EventContextMenu_Rename;

            iPlaySupport.EventIsOpenChanged += EventPlaylistSupportIsOpenChanged;
            iPlaySupport.EventIsInsertingChanged += EventPlaylistSupportIsInsertingChanged;

            iViewWidgetButtonSize.Open();
            iViewIndex = (int)(iUiOptions.ContainerView % kAllowedViews.Length);
            SetView();

            iViewWidgetButtonView.Open();
            iSizeIndex = (int)(iUiOptions.ContainerViewSize % kAllowedSizes.Length);
            SetSize();

            iTextBoxSearch.KeyDown += KeyDown;

            //OnLocationChanged();

            OnOpen();

            iOpen = true;

            iMutex.ReleaseMutex();
        }

        private delegate void OnOpenCallback();
        private void OnOpen()
        {
            if (iPanel.InvokeRequired)
            {
                iPanel.BeginInvoke(new OnOpenCallback(OnOpen));
            }
            else
            {
                iPanel.Visible = true;
            }
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iTextBoxSearch.KeyDown -= KeyDown;

                iListView.ItemActivate -= ItemActivate;
                iListView.SelectedIndexChanged -= SelectedIndexChanged;
                
                iListView.ItemDrag -= ItemDrag;
                iListView.DragEnter -= DragEnter;
                //iListView.DragOver -= DragOver;
                iListView.DragDrop -= DragDrop;

                iListView.AfterLabelEdit -= EventAfterLabelEdit;

                iListView.MouseUp -= EventMouseUp;

                iContextMenu.EventOpenClicked -= ItemActivate;
                iContextMenu.EventPlayNowClicked -= EventContextMenu_PlayNow;
                iContextMenu.EventPlayNextClicked -= EventContextMenu_PlayNext;
                iContextMenu.EventPlayLaterClicked -= EventContextMenu_PlayLater;
                iContextMenu.EventDeleteClicked -= EventContextMenu_Delete;
                iContextMenu.EventRenameClicked -= EventContextMenu_Rename;

                iPlaySupport.EventIsOpenChanged -= EventPlaylistSupportIsOpenChanged;
                iPlaySupport.EventIsInsertingChanged -= EventPlaylistSupportIsInsertingChanged;

                iViewWidgetButtonSize.Close();
                iViewWidgetButtonView.Close();

                iListView.BeginInvoke((MethodInvoker)delegate()
                {
                    //iPanel.StopBusy();
                    //iListView.Items.Clear();
                    iPanel.Visible = false;
                });
            }

            iOpen = false;

            iMutex.ReleaseMutex();
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
            Update(iContainer, list, iSelectedId);

            iMutex.ReleaseMutex();
        }

        public void Items(IContentCollector aCollector, uint aStartIndex, IList<upnpObject> aObjectList)
        {
        }

        public void ContentError(IContentCollector aCollector, string aMessage)
        {
            
            iMutex.WaitOne();
            
            UpdateStarted(iContainer, 1, string.Empty);

            item upnpItem = new item();
            upnpItem.WriteStatus = "PROTECTED";
            upnpItem.Restricted = true;
            upnpItem.Title = "Error connecting...";

            Widgets.ListViewLibrary.Item item = new Widgets.ListViewLibrary.Item(iArtworkCache);
            item.Name = "Error";
            item.Tag = upnpItem;
            item.ImageIndex = 0;
            item.Icon = kImageIconError;
            item.IconSelected = kImageIconError;
            item.SubItems[0].Name = "Title";
            item.SubItems[0].Text = "Error connecting...";                           // title
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            iListViewItems.Add(item);

            UpdateFinished(iContainer, string.Empty);

            iMutex.ReleaseMutex();
            
        }

        private void Connecting()
        {
            iListView.BeginInvoke((MethodInvoker)delegate()
            {
                Trace.WriteLine(Trace.kKinsky, "ViewWidgetLibrary.Connecting");

                iListView.Visible = false;
                iListViewVisible = false;
                iPanel.StartBusy();
                iPanel.SetMessage("Connecting...");
            });
        }

        private void UpdateStarted(IContainer aContainer, uint aCount, string aSelectedId)
        {
            iContextMenu.SetOpenEnabled(false);
            iContextMenu.SetPlayEnabled(false);
            iContextMenu.SetRenameEnabled(false);
            iContextMenu.SetDeleteEnabled(false);

            iMutex.WaitOne();

            iCount = (int)aCount;
            iContainer = aContainer;
            iPercentProgress = 0;

            iListViewItems.Clear();
            Console.WriteLine("cleared groups " + ((aContainer != null) ? aContainer.Metadata.Title : string.Empty));
            iListViewGroups.Clear();
            iListViewDummyItems.Clear();
            iArtworkUris.Clear();

            iMutex.ReleaseMutex();

            iListView.BeginInvoke((MethodInvoker)delegate()
            {
                Trace.WriteLine(Trace.kKinsky, "ViewWidgetLibrary.UpdateStarted " + ((aContainer != null) ? aContainer.Metadata.Title : string.Empty));

                iListView.BeginUpdate();

                //iListView.EnsureVisible(0);
                iListView.Columns.Clear();
                iListView.Items.Clear();
                Console.WriteLine("cleared listview groups " + ((aContainer != null) ? aContainer.Metadata.Title : string.Empty));
                iListView.Groups.Clear();

                // we need to set grouping on to allow showing/not showing later on
                //iListView.ShowGroups = true;                

                iListView.IsAlbum = (aContainer != null && aContainer.Metadata is musicAlbum);

                iListView.EndUpdate();
            });
        }

        private void Update(IContainer aContainer, IList<upnpObject> aList, string aSelectedId)
        {
            List<Widgets.ListViewLibrary.Item> list = new List<Widgets.ListViewLibrary.Item>();

            iMutex.WaitOne();

            for (int i = 0; i < aList.Count; ++i)
            {
                upnpObject upnpObject = aList[i];
                Widgets.ListViewLibrary.Item item = new Widgets.ListViewLibrary.Item(iArtworkCache);
                item.Name = upnpObject.Id;
                item.Tag = upnpObject;
                item.ImageIndex = 0;

                Uri uri = DidlLiteAdapter.ArtworkUri(upnpObject);

                if (uri != null)
                {
                    item.SetArtwork(uri);

                    if (!iArtworkUris.Contains(uri.AbsoluteUri))
                    {
                        iArtworkUris.Add(uri.AbsoluteUri);
                    }
                }

                if (upnpObject is item)
                {
                    if (upnpObject is audioItem)
                    {
                        if (aContainer != null && aContainer.Metadata is musicAlbum)
                        {
                            item.SubItems[0].Name = "AlbumArt";
                            item.SubItems[0].Text = "";
                            
                            ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "TrackNumber";
                            subItem.Text = DidlLiteAdapter.OriginalTrackNumber(upnpObject);
                            item.SubItems.Add(subItem);                                         // track number

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Title";
                            subItem.Text = DidlLiteAdapter.Title(upnpObject);
                            item.SubItems.Add(subItem);                                         // title

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Duration";
                            subItem.Text = DidlLiteAdapter.Duration(upnpObject);
                            item.SubItems.Add(subItem);                                         // length

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Artist";
                            subItem.Text = DidlLiteAdapter.Artist(upnpObject);
                            item.SubItems.Add(subItem);                                         // artist

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Album";
                            subItem.Text = DidlLiteAdapter.Album(upnpObject);
                            item.SubItems.Add(subItem);                                         // album

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Contributor";
                            subItem.Text = DidlLiteAdapter.Contributor(upnpObject);
                            item.SubItems.Add(subItem);                                         // contributing artist

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Genre";
                            subItem.Text = DidlLiteAdapter.Genre(upnpObject);
                            item.SubItems.Add(subItem);                                         // genre

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "ReleaseYear";
                            subItem.Text = DidlLiteAdapter.ReleaseYear(upnpObject);
                            item.SubItems.Add(subItem);                                         // release year

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Conductor";
                            subItem.Text = DidlLiteAdapter.Conductor(upnpObject);
                            item.SubItems.Add(subItem);                                         // conductor

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Composer";
                            subItem.Text = DidlLiteAdapter.Composer(upnpObject);
                            item.SubItems.Add(subItem);                                         // composer

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Size";
                            subItem.Text = DidlLiteAdapter.Size(upnpObject);
                            item.SubItems.Add(subItem);                                         // size

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Type";
                            subItem.Text = DidlLiteAdapter.ProtocolInfo(upnpObject);
                            item.SubItems.Add(subItem);                                         // type

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Bitrate";
                            subItem.Text = DidlLiteAdapter.Bitrate(upnpObject);
                            item.SubItems.Add(subItem);                                         // bitrate

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Uri";
                            subItem.Text = DidlLiteAdapter.Uri(upnpObject);
                            item.SubItems.Add(subItem);                                         // uri

                            if (uri != null)
                            {
                                item.Icon = kImageIconAlbum;
                                item.IconSelected = kImageIconAlbum;
                                item.ErrorIcon = kImageIconAlbumError;
                                item.ErrorIconSelected = kImageIconAlbumError;
                            }
                            else if (i == 0)
                            {
                                if (uri != null)
                                {
                                    item.Icon = kImageIconAlbum;
                                    item.IconSelected = kImageIconAlbum;
                                    item.ErrorIcon = kImageIconAlbumError;
                                    item.ErrorIconSelected = kImageIconAlbumError;
                                }
                                else
                                {
                                    item.Icon = kImageNoAlbumArt;
                                    item.IconSelected = kImageNoAlbumArt;
                                }
                            }
                        }
                        else
                        {
                            item.SubItems[0].Name = "Title";
                            item.SubItems[0].Text = DidlLiteAdapter.Title(upnpObject);          // title

                            ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Artist";
                            subItem.Text = DidlLiteAdapter.Artist(upnpObject);
                            item.SubItems.Add(subItem);                                         // artist

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Album";
                            subItem.Text = DidlLiteAdapter.Album(upnpObject);
                            item.SubItems.Add(subItem);                                         // album

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Genre";
                            subItem.Text = DidlLiteAdapter.Genre(upnpObject);
                            item.SubItems.Add(subItem);                                         // genre

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Description";
                            subItem.Text = DidlLiteAdapter.Description(upnpObject);
                            item.SubItems.Add(subItem);                                         // description

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Info";
                            subItem.Text = DidlLiteAdapter.Info(upnpObject);
                            item.SubItems.Add(subItem);                                         // info

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "ReleaseYear";
                            subItem.Text = DidlLiteAdapter.ReleaseYear(upnpObject);
                            item.SubItems.Add(subItem);                                         // release year

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Duration";
                            subItem.Text = DidlLiteAdapter.Duration(upnpObject);
                            item.SubItems.Add(subItem);                                         // length

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Contributor";
                            subItem.Text = DidlLiteAdapter.Contributor(upnpObject);
                            item.SubItems.Add(subItem);                                         // contributing artist

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Conductor";
                            subItem.Text = DidlLiteAdapter.Conductor(upnpObject);
                            item.SubItems.Add(subItem);                                         // conductor

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Composer";
                            subItem.Text = DidlLiteAdapter.Composer(upnpObject);
                            item.SubItems.Add(subItem);                                         // composer

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Size";
                            subItem.Text = DidlLiteAdapter.Size(upnpObject);
                            item.SubItems.Add(subItem);                                         // size

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Type";
                            subItem.Text = DidlLiteAdapter.ProtocolInfo(upnpObject);
                            item.SubItems.Add(subItem);                                         // type

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Bitrate";
                            subItem.Text = DidlLiteAdapter.Bitrate(upnpObject);
                            item.SubItems.Add(subItem);                                         // bitrate

                            subItem = new ListViewItem.ListViewSubItem();
                            subItem.Name = "Uri";
                            subItem.Text = DidlLiteAdapter.Uri(upnpObject);
                            item.SubItems.Add(subItem);                                         // uri

                            if (upnpObject is audioBroadcast)
                            {
                                item.Icon = kImageIconRadio;
                                item.IconSelected = kImageIconRadio;
                            }
                            else
                            {
                                item.Icon = kImageIconTrack;
                                item.IconSelected = kImageIconTrack;
                            }
                        }
                    }
                    else if (upnpObject is videoItem)
                    {
                        item.SubItems[0].Name = "Title";
                        item.SubItems[0].Text = upnpObject.Title;                           // title

                        ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Genre";
                        subItem.Text = DidlLiteAdapter.Genre(upnpObject);
                        item.SubItems.Add(subItem);                                         // genre

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Director";
                        subItem.Text = DidlLiteAdapter.Director(upnpObject);
                        item.SubItems.Add(subItem);                                         // director

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Cast";
                        subItem.Text = DidlLiteAdapter.Actor(upnpObject);
                        item.SubItems.Add(subItem);                                         // actor

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Publisher";
                        subItem.Text = DidlLiteAdapter.Publisher(upnpObject);
                        item.SubItems.Add(subItem);                                         // actor

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Description";
                        subItem.Text = DidlLiteAdapter.Description(upnpObject);
                        item.SubItems.Add(subItem);                                         // description

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Info";
                        subItem.Text = DidlLiteAdapter.Info(upnpObject);
                        item.SubItems.Add(subItem);                                         // info

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Duration";
                        subItem.Text = DidlLiteAdapter.Duration(upnpObject);
                        item.SubItems.Add(subItem);                                         // length

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Size";
                        subItem.Text = DidlLiteAdapter.Size(upnpObject);
                        item.SubItems.Add(subItem);                                         // size

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Type";
                        subItem.Text = DidlLiteAdapter.ProtocolInfo(upnpObject);
                        item.SubItems.Add(subItem);                                         // type

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Bitrate";
                        subItem.Text = DidlLiteAdapter.Bitrate(upnpObject);
                        item.SubItems.Add(subItem);                                         // bitrate

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Uri";
                        subItem.Text = DidlLiteAdapter.Uri(upnpObject);
                        item.SubItems.Add(subItem);                                         // uri

                        item.Icon = kImageIconVideo;
                        item.IconSelected = kImageIconVideo;
                    }
                    else
                    {
                        item.SubItems[0].Name = "Title";
                        item.SubItems[0].Text = upnpObject.Title;                           // title

                        ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Size";
                        subItem.Text = DidlLiteAdapter.Size(upnpObject);
                        item.SubItems.Add(subItem);                                         // size

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Type";
                        subItem.Text = DidlLiteAdapter.ProtocolInfo(upnpObject);
                        item.SubItems.Add(subItem);                                         // type

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Uri";
                        subItem.Text = DidlLiteAdapter.Uri(upnpObject);
                        item.SubItems.Add(subItem);                                         // uri

                        if (upnpObject.Title == "Access denied")
                        {
                            item.Icon = kImageIconError;
                            item.IconSelected = kImageIconError;
                        }
                    }
                }
                else if (upnpObject is musicAlbum)
                {
                    item.SubItems[0].Name = "Title";
                    item.SubItems[0].Text = upnpObject.Title;                           // title

                    ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                    subItem = new ListViewItem.ListViewSubItem();
                    subItem.Name = "Artist";
                    subItem.Text = DidlLiteAdapter.Artist(upnpObject);
                    item.SubItems.Add(subItem);                                         // artist

                    subItem = new ListViewItem.ListViewSubItem();
                    subItem.Name = "Genre";
                    subItem.Text = DidlLiteAdapter.Genre(upnpObject);
                    item.SubItems.Add(subItem);                                         // genre

                    subItem = new ListViewItem.ListViewSubItem();
                    subItem.Name = "ReleaseYear";
                    subItem.Text = DidlLiteAdapter.ReleaseYear(upnpObject);
                    item.SubItems.Add(subItem);                                         // release year

                    subItem = new ListViewItem.ListViewSubItem();
                    subItem.Name = "Count";
                    subItem.Text = DidlLiteAdapter.Count(upnpObject);
                    item.SubItems.Add(subItem);                                         // child count

                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");

                    item.Icon = kImageIconAlbum;
                    item.IconSelected = kImageIconAlbum;

                    if (uri != null)
                    {
                        item.ErrorIcon = kImageIconAlbumError;
                        item.ErrorIconSelected = kImageIconAlbumError;
                    }
                    else
                    {
                        item.Icon = kImageNoAlbumArt;
                        item.IconSelected = kImageNoAlbumArt;
                    }
                }
                else
                {
                    item.SubItems[0].Name = "Title";
                    item.SubItems[0].Text = upnpObject.Title;                           // title

                    ListViewItem.ListViewSubItem subItem;

                    subItem = new ListViewItem.ListViewSubItem();
                    subItem.Name = "Count";
                    subItem.Text = DidlLiteAdapter.Count(upnpObject);
                    item.SubItems.Add(subItem);                                         // child count

                    subItem = new ListViewItem.ListViewSubItem();
                    subItem.Name = "Description";
                    subItem.Text = DidlLiteAdapter.Description(upnpObject);
                    item.SubItems.Add(subItem);                                         // description

                    subItem = new ListViewItem.ListViewSubItem();
                    subItem.Name = "Info";
                    subItem.Text = DidlLiteAdapter.Info(upnpObject);
                    item.SubItems.Add(subItem);                                         // info

                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");

                    if (upnpObject is person)
                    {
                        item.Icon = kImageIconArtist;
                        item.IconSelected = kImageIconArtist;
                    }
                    else if (upnpObject is playlistContainer)
                    {
                        item.Icon = kImageIconPlaylist;
                        item.IconSelected = kImageIconPlaylist;
                    }
                    else if (upnpObject is container)
                    {
                        if (upnpObject.ParentId == MediaProviderLibrary.kLibraryId || upnpObject.Id == MediaProviderLibrary.kLibraryId)
                        {
                            item.Icon = kImageIconServer;
                            item.IconSelected = kImageIconServer;
                        }
                        else
                        {
                            item.Icon = kImageIconDirectory;
                            item.IconSelected = kImageIconDirectory;
                        }
                    }
                }

                iListViewItems.Add(item);
                list.Add(item);
            }

            uint percentProgress = (uint)Math.Ceiling((iListViewItems.Count / (float)iCount) * 100);

            Trace.WriteLine(Trace.kKinsky, "ViewWidgetLibrary.Update " + iListViewItems.Count + " - " + ((aContainer != null) ? aContainer.Metadata.Title : String.Empty));
            if (percentProgress != iPercentProgress)
            {
                iPanel.SetMessage(percentProgress.ToString() + "%");
                iPercentProgress = percentProgress;
            }

            if (iListViewItems.Count == iCount)
            {
                UpdateFinished(aContainer, aSelectedId);
            }

            iMutex.ReleaseMutex();
        }

        private void UpdateFinished(IContainer aContainer, string aSelectedId)
        {
            iListView.BeginInvoke((MethodInvoker)delegate()
            {
                SetColumns();

                iMutex.WaitOne();

                ListViewItem selectedItem = null;
                foreach (ListViewItem i in iListViewItems)
                {
                    upnpObject upnpObject = i.Tag as upnpObject;
                    if (upnpObject is audioItem)
                    {
                        ListViewItem.ListViewSubItem subItemAlbum = new ListViewItem.ListViewSubItem();
                        subItemAlbum.Name = "Album";
                        subItemAlbum.Text = DidlLiteAdapter.Album(upnpObject);
                        /*if (subItemAlbum.Text != string.Empty)
                        {
                            i.Group = GroupByAlbum(subItemAlbum.Text);
                        }
                        else
                        {
                            i.Group = GroupByAlbum("Unknown");
                        }*/
                    }
                    else
                    {
                        //i.Group = GroupByFirstLetter(upnpObject.Title);
                    }
                    if (upnpObject.Id == aSelectedId)
                    {
                        i.Selected = true;
                        selectedItem = i;
                    }
                }

                iListView.BeginUpdate();

                iListView.Items.AddRange(iListViewItems.ToArray());
                iListView.Items.AddRange(iListViewDummyItems.ToArray());

                iMutex.ReleaseMutex();

                /*if (iListView.View != System.Windows.Forms.View.Tile)
                {
                    iListView.ShowGroups = false;
                }*/

                iListView.UpdateGroupHeaderColour();

                iListView.EndUpdate();

                iPanel.StopBusy();
                iListView.Visible = true;
                iListViewVisible = true;

                if (selectedItem != null)
                {
                    iListView.EnsureVisible(selectedItem.Index);
                }

                Trace.WriteLine(Trace.kKinsky, "ViewWidgetLibrary.UpdateFinished " + ((aContainer != null) ? aContainer.Metadata.Title : string.Empty));

                Focus();
            });
        }

        public void OnSizeClick()
        {
            iSizeIndex = (iSizeIndex + 1) % kAllowedSizes.Length;
            iUiOptions.ContainerViewSize = (uint)iSizeIndex;
            SetSize();
        }

        public void OnViewClick()
        {
            iViewIndex = (iViewIndex + 1) % kAllowedViews.Length;
            if (iListView.IsAlbum)
            {
                iUiOptions.AlbumView = (uint)iViewIndex;
            }
            else
            {
                iUiOptions.ContainerView = (uint)iViewIndex;
            }
            SetView();
        }

        public void Focus()
        {
            iListView.BeginInvoke((MethodInvoker)delegate()
            {
                iListView.Focus();
            });
        }

        private void LocationChanged(object sender, EventArgs e)
        {
            OnLocationChanged();
        }

        private void OnLocationChanged()
        {
            iMutex.WaitOne();

            Connecting();

            iSelectedId = iBrowser.SelectedId;
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

        private void SetSize()
        {
            iListView.BeginInvoke((MethodInvoker)delegate()
            {
                // temporarily hide the list view - note that the visibility of the list
                // view is tracked in the separate member iListViewVisible - this is because
                // even if iListView.Visible has been set to true, accessing it can still
                // return false is is parent is not visible
                iListView.Visible = false;
                switch (iSizeIndex)
                {
                    case 0:
                        iListView.Font = iViewSupport.FontSmall;
                        break;
                    case 1:
                        iListView.Font = iViewSupport.FontMedium;
                        break;
                    case 2:
                        iListView.Font = iViewSupport.FontLarge;
                        break;
                    default:
                        Assert.Check(false);
                        break;
                }
                iListView.LargeIconSize = new Size(kAllowedSizes[iSizeIndex], kAllowedSizes[iSizeIndex]);
                iListView.TileHeight = kAllowedSizes[iSizeIndex];
                if (iListView.View != kAllowedViews[0])
                {
                    iListView.View = kAllowedViews[0];
                    iListView.View = kAllowedViews[iViewIndex];
                }
                iListView.Visible = iListViewVisible;
            });
        }

        private void SetView()
        {
            iListView.BeginInvoke((MethodInvoker)delegate()
            {
                // temporarily hide the list view - note that the visibility of the list
                // view is tracked in the separate member iListViewVisible - this is because
                // even if iListView.Visible has been set to true, accessing it can still
                // return false is is parent is not visible
                iListView.Visible = false;
                //iListView.ShowGroups = kAllowedViews[iViewIndex] == System.Windows.Forms.View.Tile;
                iListView.View = kAllowedViews[iViewIndex];
                iListView.Visible = iListViewVisible;
            });
        }

        private void SetColumns()
        {
            ColumnHeader column = null;
            if (iListView.IsAlbum)
            {
                column = new ColumnHeader();
                column.Name = "AlbumArt";
                column.Text = "";
                column.Width = 320;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "TrackNumber";
                column.Text = "";
                column.Width = 50;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Title";
                column.Text = "Title";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Duration";
                column.Text = "Duration";
                column.Width = 100;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Artist";
                column.Text = "Artist";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Album";
                column.Text = "Album";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Contributor";
                column.Text = "Contributing Artist";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Genre";
                column.Text = "Genre";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "ReleaseYear";
                column.Text = "Release Year";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Conductor";
                column.Text = "Conductor";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Composer";
                column.Text = "Composer";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Size";
                column.Text = "Size";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Type";
                column.Text = "Type";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Bitrate";
                column.Text = "Bitrate";
                column.Width = 250;
                iListView.Columns.Add(column);

                column = new ColumnHeader();
                column.Name = "Uri";
                column.Text = "URI";
                column.Width = 250;
                iListView.Columns.Add(column);

                //iViewIndex = iUserOptions.AlbumView;
            }
            else
            {
                //iViewIndex = iUserOptions.ContainerView;
            }

            SetView();
        }

        private void EventUpdated(object sender, EventArgsArtwork e)
        {
            IArtwork artwork = e.Artwork;
            if (iArtworkUris.Contains(artwork.Uri.OriginalString))
            {
                iListView.BeginInvoke((MethodInvoker)delegate()
                {
                    iListView.Redraw();
                });
            }
        }

        private void ItemActivate(object sender, EventArgs e)
        {
            if (iListView.SelectedIndices.Count > 0)
            {
                ListViewItem listItem = iListView.Items[iListView.SelectedIndices[0]];

                container container = listItem.Tag as container;

                if (container != null)
                {
                    iBrowser.Down(container);
                }

                item item = listItem.Tag as item;
                if (item != null)
                {
                    List<upnpObject> list = new List<upnpObject>();
                    list.Add(item);

                    iPlaySupport.PlayNow(new MediaRetrieverNoRetrieve(list));
                }
            }
        }

        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            if (iListView.SelectedIndices.Count > 0)
            {
                if (iListView.SelectedIndices.Count == 1)
                {
                    upnpObject o = iListView.Items[iListView.SelectedIndices[0]].Tag as upnpObject;
                    bool allowEdit = ((iContainer != null) ? iContainer.HandleRename(o) : false);
                    iListView.LabelEdit = allowEdit;

                    iContextMenu.SetOpenEnabled(iListView.Items[iListView.SelectedIndices[0]].Tag is container);
                    iContextMenu.SetRenameEnabled(allowEdit);
                }
                else
                {
                    iContextMenu.SetOpenEnabled(false);
                    iContextMenu.SetRenameEnabled(false);
                }

                DidlLite didl = new DidlLite();
                didl.AddRange(SelectedUpnpObjects());
                iContextMenu.SetDeleteEnabled(((iContainer != null) ? iContainer.HandleDelete(didl) : false));
                iContextMenu.SetPlayEnabled(iPlaySupport.IsOpen() && !iPlaySupport.IsInserting());
            }
            else
            {
                iContextMenu.SetOpenEnabled(false);
                iContextMenu.SetPlayEnabled(false);
                iContextMenu.SetRenameEnabled(false);
                iContextMenu.SetDeleteEnabled(false);
            }

            iMutex.ReleaseMutex();
        }

        private void ItemDrag(object sender, ItemDragEventArgs e)
        {
            iMutex.WaitOne();

            DidlLite didl = new DidlLite();
            didl.AddRange(SelectedUpnpObjects());
            IContainer container = iContainer;

            iMutex.ReleaseMutex();

            if (didl.Count > 0)
            {
                DragDropEffects dragDropEffects = DragDropEffects.Copy;
                if (container != null && container.HandleDelete(didl))
                {
                    dragDropEffects |= DragDropEffects.Move;
                }

                iPlaySupport.SetDragging(true);
                DragDropEffects result = iListView.DoDragDrop(new MediaProviderDraggable(new MediaRetriever(container, didl), this), dragDropEffects);
                iPlaySupport.SetDragging(false);

                if (result == DragDropEffects.Move)
                {
                    foreach (upnpObject o in didl)
                    {
                        iContainer.Delete(o.Id);
                    }
                }
            }
        }

        private void DragEnter(object sender, DragEventArgs e)
        {
            iMutex.WaitOne();

            e.Effect = DragDropEffects.None;

            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy ||
                (e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link ||
                (e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                MediaProviderDraggable r = iDropConverter.Convert(e.Data);
                if (r != null)
                {
                    if (iContainer.HandleInsert(r.DragMedia))
                    {
                        if (((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) && (r.DragSource != this))
                        {
                            e.Effect = DragDropEffects.Copy;
                        }
                        else if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                        else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                        {
                            e.Effect = DragDropEffects.Move;
                        }
                    }
                }
            }

            iMutex.ReleaseMutex();
        }

        private void DragDrop(object sender, DragEventArgs e)
        {
            iMutex.WaitOne();

            int index = iListView.InsertionMark.Index + (iListView.InsertionMark.AppearsAfterItem ? 1 : 0);
            MediaProviderDraggable r = iDropConverter.Convert(e.Data);
            IContainer container = iContainer;

            string id = string.Empty;
            if (iListView.Items.Count > 0)
            {
                upnpObject o = iListView.Items[index].Tag as upnpObject;
                id = o.Id;
            }
                
            iMutex.ReleaseMutex();

            if (r != null)
            {
                if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy ||
                    (e.Effect & DragDropEffects.Link) == DragDropEffects.Link ||
                    (e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    if (container != null)
                    {
                        container.Insert(id, r.DragMedia);
                    }
                }
            }
        }

        private void EventAfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            // A note on this line:
            // Setting the e.CancelEdit flag to true tells the ListView to discard the
            // change to the renamed item. We are always setting this, which may seem odd.
            // The reason for this was to avoid an exception being thrown from within the
            // winforms assembly. A sequence of events is as follows:
            // 1. The user clicks on, say, item 2 in the list view
            // 2. The list view shows a separate EditCtrl box, positioned over the label
            //    for item 2, and the user can change the item name
            // 3. The user hits return to finish editing - ***this event*** gets fired
            // 4. Event handler code runs - in our situation, the IContainer.Rename
            //    function gets called which, in some cases, can cause the IContainer
            //    to emit one of its events (e.g. EventContentChanged) - that event
            //    can then cause the browser to refresh - which essentially clears the
            //    items in the list view and restarts a content collector thread to 
            //    refresh the list of items
            // 5. After the event handler has finished execution, the list view 
            //    sets the name of item 2 to the new name, ***if e.CancelEdit == false***.
            //    Now, if the IContainer fired one of its events which caused a refresh,
            //    the list view item list has possibly been cleared, which means that the
            //    setting the name of item 2 to the new value will cause a
            //    System.ArgumentOutOfRangeException to be thrown - which is the observed
            //    behaviour. Setting e.CancelEdit = true will mean that the list view just
            //    ignores any changes and leaved the list view as it was
            //
            // Now, this may seem a little bit of a hack, but it isn't. We are using this
            // mechanism to all the user to rename an item and we are letting the view be
            // updated by the events coming out of the model. This is a Model-View-Presenter
            // paradigm rather than a Model-View-Controller. In MVC, the
            // controller is responsible for changing the data in the Model **and** changing
            // the View to reflect the change. In MVP, the Presenter just changes the Model
            // appropriately and lets the Model eventing take care of updating the Views. This
            // is what we are doing here
            e.CancelEdit = true;

            if (e.Label == null || e.Label == "\n")
                return;

            iMutex.WaitOne();
            upnpObject o = iListView.Items[e.Item].Tag as upnpObject;
            IContainer container = iContainer;
            iMutex.ReleaseMutex();
            container.Rename(o.Id, e.Label.Trim('\n'));
        }

        private void EventMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton2)
            {
                ItemActivate(this, EventArgs.Empty);
            }
            else if (e.Button == MouseButtons.XButton1)
            {
                iBrowser.Up(1);
            }
        }

        private void EventContextMenu_PlayNow(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            iPlaySupport.PlayNow(new MediaRetriever(iContainer, SelectedUpnpObjects()));
            iMutex.ReleaseMutex();
        }

        private void EventContextMenu_PlayNext(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            iPlaySupport.PlayNext(new MediaRetriever(iContainer, SelectedUpnpObjects()));
            iMutex.ReleaseMutex();
        }

        private void EventContextMenu_PlayLater(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            iPlaySupport.PlayLater(new MediaRetriever(iContainer, SelectedUpnpObjects()));
            iMutex.ReleaseMutex();
        }

        private void EventContextMenu_Delete(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            IList<upnpObject> list = SelectedUpnpObjects();
            IContainer container = iContainer;

            iMutex.ReleaseMutex();

            // delete the items outside of the lock since the IContainer.Delete will cause
            // EventContentRemoved events to be emitted which will cause deadlock
            foreach (upnpObject o in list)
            {
                container.Delete(o.Id);
            }
        }

        private void EventContextMenu_Rename(object sender, EventArgs e)
        {
            iListView.Items[iListView.SelectedIndices[0]].BeginEdit();
        }

        private IList<upnpObject> SelectedUpnpObjects()
        {
            List<upnpObject> items = new List<upnpObject>();
            for (int i = 0; i < iListView.SelectedIndices.Count; ++i)
            {
                ListViewItem item = iListView.Items[iListView.SelectedIndices[i]];
                upnpObject upnpObject = item.Tag as upnpObject;
                items.Add(upnpObject);
            }
            return items;
        }

        private void EventPlaylistSupportIsOpenChanged(object sender, EventArgs e)
        {
            if (iPlaySupport.IsOpen())
            {
                iListView.BeginInvoke((MethodInvoker)delegate()
                {
                    if (iListView.SelectedIndices.Count > 0)
                    {
                        iContextMenu.SetPlayEnabled(!iPlaySupport.IsInserting());
                    }
                });
            }
            else
            {
                iContextMenu.SetPlayEnabled(false);
            }
        }

        private void EventPlaylistSupportIsInsertingChanged(object sender, EventArgs e)
        {
            iListView.BeginInvoke((MethodInvoker)delegate()
            {
                if (iListView.SelectedIndices.Count > 0)
                {
                    iContextMenu.SetPlayEnabled(iPlaySupport.IsOpen() && !iPlaySupport.IsInserting());
                }
            });
        }

        private ListViewGroup GroupByFirstLetter(string aTitle)
        {
            Assert.CheckDebug(aTitle != null);

            ListViewGroup value;
            string group = "Other";
            if (aTitle != null && aTitle.Length > 0)
            {
                group = aTitle.Substring(0, 1).ToUpper();
                if (group[0] < 'A' || group[0] > 'Z')
                {
                    group = kGroupOtherTitle;
                }
            }
            if (iListViewGroups.TryGetValue(group, out value))
            {
                return value;
            }

            value = new ListViewGroup(group);
            Console.WriteLine("added group " + group);
            iListViewGroups[group] = value;
            int index = iListViewGroups.IndexOfKey(group);
            if (index <= iListView.Groups.Count)
            {
                iListView.Groups.Insert(index, value);
            }

            return value;
        }

        private ListViewGroup GroupByAlbum(string aTitle)
        {
            string title = aTitle;
            if (title == null)
            {
                title = kGroupOtherTitle;
            }
            ListViewGroup value;
            if (iListViewGroups.TryGetValue(title, out value))
            {
                return value;
            }

            value = new ListViewGroup(title);
            iListViewGroups[title] = value;
            int index = iListViewGroups.IndexOfKey(title);
            iListView.Groups.Insert(index, value);

            return value;
        }

        private void EventSizeChanged(object sender, EventArgs e)
        {
            OnSizeClick();
        }

        private void EventViewChanged(object sender, EventArgs e)
        {
            OnViewClick();
        }

        private void EventSupportChanged(object sender, EventArgs e)
        {
            SetViewColours();
            SetSize();

            iPanel.Font = iViewSupport.FontMedium;
            iPanel.ForeColor = iViewSupport.ForeColour;
        }

        private void SetViewColours()
        {
            iListView.BackColor = iViewSupport.BackColour;
            iListView.ForeColor = iViewSupport.ForeColour;
            iListView.ForeColorMuted = iViewSupport.ForeColourMuted;
            iListView.ForeColorBright = iViewSupport.ForeColourBright;
            iListView.HighlightBackColour = iViewSupport.HighlightBackColour;
            iListView.HighlightForeColour = iViewSupport.HighlightForeColour;
        }

        private readonly Image kImageNoAlbumArt = KinskyDesktop.Properties.Resources.NoAlbumArt;
        private readonly Image kImageIconAlbum = KinskyDesktop.Properties.Resources.IconAlbum;
        private readonly Image kImageIconAlbumError = KinskyDesktop.Properties.Resources.IconAlbumError;
        private readonly Image kImageIconArtist = KinskyDesktop.Properties.Resources.IconArtist;
        private readonly Image kImageIconDirectory = KinskyDesktop.Properties.Resources.IconDirectory;
        private readonly Image kImageIconError = KinskyDesktop.Properties.Resources.IconError;
        private readonly Image kImageIconPlaylist = KinskyDesktop.Properties.Resources.IconPlaylist;
        private readonly Image kImageIconRadio = KinskyDesktop.Properties.Resources.IconRadio;
        private readonly Image kImageIconServer = KinskyDesktop.Properties.Resources.IconLibrary;
        private readonly Image kImageIconTrack = KinskyDesktop.Properties.Resources.IconTrack;
        private readonly Image kImageIconVideo = KinskyDesktop.Properties.Resources.IconVideo;

        private const string kGroupOtherTitle = "(Other)";
        private const int kMinItemsPerGroup = 12;
        private const uint kCountPerCall = 100;
        
        private int[] kAllowedSizes = { 64, 128, 256 };
        private System.Windows.Forms.View[] kAllowedViews = { System.Windows.Forms.View.LargeIcon, System.Windows.Forms.View.Tile };

        private Mutex iMutex;
        private bool iOpen;
        private IArtworkCache iArtworkCache;
        private IViewSupport iViewSupport;
        private IPlaylistSupport iPlaySupport;
        private DropConverter iDropConverter;
        private UiOptions iUiOptions;

        private IContentCollector iContentCollector;
        private IBrowser iBrowser;

        private IViewWidgetButton iViewWidgetButtonSize;
        private IViewWidgetButton iViewWidgetButtonView;

        private int iSizeIndex;
        private int iViewIndex;

        private ViewContextMenu iContextMenu;
        private Widgets.ListViewLibrary iListView;
        private Widgets.PanelBusy iPanel;
        private bool iListViewVisible;

        private TextBox iTextBoxSearch;

        private int iCount;
        private string iSelectedId;
        private IContainer iContainer;
        private List<ListViewItem> iListViewItems;
        private SortedList<string, ListViewGroup> iListViewGroups;
        private List<ListViewItem> iListViewDummyItems;
        private List<string> iArtworkUris;
        private uint iPercentProgress;

        
    }
} // OssKinskyMppLibrary
