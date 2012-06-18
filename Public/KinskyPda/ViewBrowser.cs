
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Linn;
using Linn.Kinsky;
using Upnp;


namespace KinskyPda
{
    public class ViewBrowserContextMenu : ContextMenu
    {
        private MenuItem AddMenuItem(string aText)
        {
            MenuItem item = new MenuItem();
            item.Text = aText;
            this.MenuItems.Add(item);
            return item;
        }

        public ViewBrowserContextMenu()
        {
            iMenuItemOptions = AddMenuItem("Options...");
            iMenuItemAbout = AddMenuItem("About");
            iMenuItemDebug = AddMenuItem("Debug");
            AddMenuItem("-");
            iMenuItemExit = AddMenuItem("Exit");
        }

        private MenuItem iMenuItemOptions;
        private MenuItem iMenuItemAbout;
        private MenuItem iMenuItemDebug;
        private MenuItem iMenuItemExit;
    }


    internal class ViewBrowser : IViewWidgetContent
    {
        public ViewBrowser(BrowserItems aBrowserItems, IPlaylistSupport aPlaylistSupport, ViewSupport aViewSupport, OptionBool aOptionShowArtwork)
        {
            iOpen = false;
            iContextMenu = new ViewBrowserContextMenu();
            iBrowserItems = aBrowserItems;
            iPlaylistSupport = aPlaylistSupport;

            iListView = new ListView();
            iListView.Name = "ViewBrowserListView";
            iListView.BackColor = aViewSupport.BackColour;
            iListView.ForeColor = aViewSupport.ForeColour;
            iListView.Font = aViewSupport.FontMedium;
            iListView.FullRowSelect = true;
            iListView.HeaderStyle = ColumnHeaderStyle.None;
            iListView.Dock = DockStyle.Fill;
            iListView.View = View.Details;
            iListView.ContextMenu = iContextMenu;

            iListView.Columns.Clear();
            iListView.Columns.Add("Title", DisplayManager.ScaleWidth(440), HorizontalAlignment.Left);

            iListView.SmallImageList = new System.Windows.Forms.ImageList();
            iListView.SmallImageList.ImageSize = LayoutManager.Instance.BrowseIconImageSize;

            iArtworkCache = new ArtworkCache(ArtworkCache.ECacheSize.eSmall);
            iImageList = null;
            iPanelBusy = new KinskyPda.Widgets.PanelBusy();

            iOptionShowArtwork = aOptionShowArtwork;
        }

        public void SetParent(Control aParent)
        {
            aParent.Controls.Add(iListView);
            aParent.Controls.Add(iPanelBusy);

            // calculate the bounds of the busy panel relative to the parent
            Rectangle bounds = new Rectangle(0, 0,
                                             aParent.Bounds.Width,
                                             iPanelBusy.MinimumSize.Height);
            bounds.Offset(0, (aParent.Bounds.Height - bounds.Height) / 2);
            bounds.Inflate(-50, 0);
            iPanelBusy.Bounds = bounds;
            iPanelBusy.BringToFront();
        }

        public ContextMenu ContextMenu
        {
            get { return iContextMenu; }
        }

        public void Open()
        {
            if (!iOpen)
            {
                iBrowserItems.EventStateChanged += BrowserItemsStateChanged;
                iBrowserItems.EventProgress += BrowserItemsProgress;

                iPlaylistSupport.EventPlayNowRequest += PlayNowRequest;
                iPlaylistSupport.EventPlayLaterRequest += PlayLaterRequest;

                iListView.SelectedIndexChanged += ListViewSelectedIndexChanged;
                iListView.ItemActivate += ListViewItemActivate;

                iOptionShowArtwork.EventValueChanged += OptionShowArtworkChanged;

                iOpen = true;
                BrowserItemsStateChanged(iBrowserItems);
            }
        }

        public void Close()
        {
            if (iOpen)
            {
                iBrowserItems.EventStateChanged -= BrowserItemsStateChanged;
                iBrowserItems.EventProgress -= BrowserItemsProgress;

                iPlaylistSupport.EventPlayNowRequest -= PlayNowRequest;
                iPlaylistSupport.EventPlayLaterRequest -= PlayLaterRequest;

                iListView.SelectedIndexChanged -= ListViewSelectedIndexChanged;
                iListView.ItemActivate -= ListViewItemActivate;

                iOptionShowArtwork.EventValueChanged -= OptionShowArtworkChanged;

                iOpen = false;
                ClearItems();
            }
        }

        public void OnSizeClick()
        {
        }

        public void OnViewClick()
        {
        }

        private delegate void DFocus();
        public void Focus()
        {
            if (iListView.InvokeRequired)
            {
                iListView.BeginInvoke(new DFocus(Focus));
            }
            else
            {
                iListView.Focus();
            }
        }

        private delegate void DClearItems();
        private void ClearItems()
        {
            if (iListView.InvokeRequired)
            {
                iListView.BeginInvoke(new DClearItems(ClearItems));
            }
            else
            {
                // clear the item list and all image list data
                if (iImageList != null)
                {
                    iImageList.Dispose();
                    iImageList = null;
                }
                iListView.BeginUpdate();
                iListView.Items.Clear();
                iListView.SmallImageList.Images.Clear();
                iListView.EndUpdate();
            }
        }

        private void ListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            bool insertAllowed = false;
            foreach (int i in iListView.SelectedIndices)
            {
                if (iListView.Items[i].Tag != null)
                    insertAllowed = true;
            }
            iPlaylistSupport.SetInsertAllowed(insertAllowed);
        }

        private void ListViewItemActivate(object sender, EventArgs e)
        {
            if (iListView.SelectedIndices.Count == 1)
            {
                // browse into the selected container
                ListViewItem item = iListView.Items[iListView.SelectedIndices[0]];
                container c = item.Tag as container;
                if (c != null)
                {
                    iBrowserItems.Down(c);
                }
            }
        }

        private void OptionShowArtworkChanged(object sender, EventArgs e)
        {
            BrowserItemsStateChanged(iBrowserItems);
        }

        private void BrowserItemsProgress(BrowserItems aBrowserItems, uint aCount, uint aTotal)
        {
            string msg = ((aCount * 100) / aTotal).ToString() + "%";
            iPanelBusy.ForceSetMessage(msg);
        }

        private void BrowserItemsStateChanged(BrowserItems aBrowserItems)
        {
            if (iListView.InvokeRequired)
            {
                iListView.BeginInvoke(new BrowserItems.DEventHandler(BrowserItemsStateChanged), new object[] { aBrowserItems });
                return;
            }

            // get the new browser state
            iBrowserItemsState = aBrowserItems.CurrentState;

            // populate the list view
            if (iBrowserItemsState.Type == BrowserItems.State.EType.eScanning ||
                iBrowserItemsState.Type == BrowserItems.State.EType.eUninitialised)
            {
                ClearItems();
            }
            else
            {
                // create a new image list
                if (iImageList != null)
                {
                    iImageList.Dispose();
                }
                if (iOptionShowArtwork.Native)
                {
                    iImageList = new ImageListArtwork(iArtworkCache, ArtworkUpdated);
                }
                else
                {
                    iImageList = new ImageListDefault();
                }

                // build the list of items
                List<ListViewItem> items;
                int selectedIndex = -1;
                if (iBrowserItemsState.Type == BrowserItems.State.EType.eContentError)
                {
                    items = new List<ListViewItem>();
                    ListViewItem item = new ListViewItem();
                    item.Tag = null;
                    item.Text = "Error occurred";
                    item.ImageIndex = iImageList.IconErrorIndex;
                    items.Add(item);
                }
                else
                {
                    items = new List<ListViewItem>(iBrowserItemsState.Items.Count);
                    for (int i = 0; i < iBrowserItemsState.Items.Count; i++)
                    {
                        upnpObject obj = iBrowserItemsState.Items[i];
                        ListViewItem item = new ListViewItem();
                        item.Tag = obj;
                        item.Text = obj.Title;
                        item.ImageIndex = iImageList.Add(obj);
                        items.Add(item);

                        if (obj.Id == iBrowserItemsState.SelectedId)
                        {
                            selectedIndex = i;
                        }
                    }
                }

                // update the list view
                iListView.BeginUpdate();

                iListView.Items.Clear();
                foreach (ListViewItem item in items)
                {
                    iListView.Items.Add(item);
                }

                iListView.SmallImageList.Images.Clear();
                iImageList.BuildImageList(iListView.SmallImageList);

                if (selectedIndex != -1)
                {
                    iListView.Items[selectedIndex].Selected = true;
                    iListView.Items[selectedIndex].Focused = true;
                    iListView.EnsureVisible(selectedIndex);
                }
                else if (iListView.Items.Count > 0)
                {
                    iListView.Items[0].Focused = true;
                }

                iListView.EndUpdate();
                iListView.Focus();
            }

            // show or hide the hourglass
            if (iBrowserItemsState.Type == BrowserItems.State.EType.eScanning)
            {
                iPanelBusy.StartBusy("Connecting...");
            }
            else
            {
                iPanelBusy.StopBusy();
            }
        }

        private void ArtworkUpdated(ImageList aImageList)
        {
            if (iListView.InvokeRequired)
            {
                iListView.BeginInvoke(new ImageListArtwork.DUpdated(ArtworkUpdated), new object[] { aImageList });
                return;
            }

            if (aImageList != iImageList)
                return;

            iListView.BeginUpdate();
            iListView.SmallImageList.Images.Clear();
            iImageList.BuildImageList(iListView.SmallImageList);
            iListView.EndUpdate();
        }

        private void PlayNowRequest(object sender, EventArgs e)
        {
            lock (this)
            {
                iPlaylistSupport.PlayNow(new MediaRetriever(iBrowserItemsState.Container, SelectedUpnpObjects()));
            }
        }

        private void PlayLaterRequest(object sender, EventArgs e)
        {
            lock (this)
            {
                iPlaylistSupport.PlayLater(new MediaRetriever(iBrowserItemsState.Container, SelectedUpnpObjects()));
            }
        }

        private IList<upnpObject> SelectedUpnpObjects()
        {
            List<upnpObject> items = new List<upnpObject>();
            for (int i = 0; i < iListView.SelectedIndices.Count; ++i)
            {
                ListViewItem item = iListView.Items[iListView.SelectedIndices[i]];
                upnpObject upnpObject = item.Tag as upnpObject;
                if (upnpObject != null)
                    items.Add(upnpObject);
            }
            return items;
        }


        private abstract class ImageList : IDisposable
        {
            public ImageList()
            {
                iImages = new List<Image>();
                iImages.Add(TextureManager.Instance.IconNoAlbumArt);
                iImages.Add(TextureManager.Instance.IconAlbum);
                iImages.Add(TextureManager.Instance.IconArtist);
                iImages.Add(TextureManager.Instance.IconDirectory);
                iImages.Add(TextureManager.Instance.IconError);
                iImages.Add(TextureManager.Instance.IconPlaylist);
                iImages.Add(TextureManager.Instance.IconRadio);
                iImages.Add(TextureManager.Instance.IconServer);
                iImages.Add(TextureManager.Instance.IconTrack);
            }

            public abstract void Dispose();
            public abstract int Add(upnpObject aObject);

            public void BuildImageList(System.Windows.Forms.ImageList aImageList)
            {
                Assert.Check(aImageList.Images.Count == 0);
                lock (this)
                {
                    foreach (Image image in iImages)
                    {
                        aImageList.Images.Add(image);
                    }
                }
            }

            public int IconErrorIndex
            {
                get { return kIndexIconError; }
            }

            private const int kIndexIconNoAlbumArt = 0;
            private const int kIndexIconAlbum = 1;
            private const int kIndexIconArtist = 2;
            private const int kIndexIconDirectory = 3;
            private const int kIndexIconError = 4;
            private const int kIndexIconPlaylist = 5;
            private const int kIndexIconRadio = 6;
            private const int kIndexIconServer = 7;
            private const int kIndexIconTrack = 8;

            protected int DefaultImageIndex(upnpObject aObject)
            {
                if (aObject is audioBroadcast)
                    return kIndexIconRadio;
                else if (aObject is item)
                    return kIndexIconTrack;
                else if (aObject is musicAlbum)
                    return kIndexIconAlbum;
                else if (aObject is person)
                    return kIndexIconArtist;
                else if (aObject is playlistContainer)
                    return kIndexIconPlaylist;
                else if (aObject is container)
                {
                    if (aObject.ParentId == "Library")
                        return kIndexIconServer;
                    else
                        return kIndexIconDirectory;
                }
                else
                    return kIndexIconNoAlbumArt;
            }

            protected List<Image> iImages;
        }


        private class ImageListDefault : ImageList
        {
            public ImageListDefault()
                : base()
            {
            }

            public override void Dispose()
            {
            }

            public override int Add(upnpObject aObject)
            {
                return DefaultImageIndex(aObject);
            }
        }


        private class ImageListArtwork : ImageList
        {
            public delegate void DUpdated(ImageList aImageList);

            public ImageListArtwork(IArtworkCache aArtworkCache, DUpdated aUpdated)
                : base()
            {
                iUriToIndex = new Dictionary<Uri, int>();

                iArtworkCache = aArtworkCache;
                iArtworkCache.EventUpdated += ArtworkUpdated;
                iUpdated = aUpdated;
            }

            public override void Dispose()
            {
                iUpdated = null;
                iArtworkCache.EventUpdated -= ArtworkUpdated;
            }

            public override int Add(upnpObject aObject)
            {
                lock (this)
                {
                    Uri uri = DidlLiteAdapter.ArtworkUri(aObject);
                    if (uri != null)
                    {

                        // get the image index for this uri
                        int index;
                        if (iUriToIndex.TryGetValue(uri, out index))
                        {
                            return index;
                        }
                        else
                        {
                            // this uri does not have an image in the image list
                            // - get it from the cache
                            Image image = iArtworkCache.Artwork(uri).Image;
                            if (image == null)
                            {
                                // this uri has no image in the image cache just
                                // yet, so add a placeholder
                                image = iImages[DefaultImageIndex(aObject)];
                            }

                            index = iImages.Count;
                            iUriToIndex.Add(uri, index);
                            iImages.Add(image);
                            return index;
                        }
                    }
                    else
                    {
                        return DefaultImageIndex(aObject);
                    }
                }
            }

            private void ArtworkUpdated(object sender, EventArgsArtwork e)
            {
                lock (this)
                {
                    int index;
                    if (e.Artwork.Image == null ||
                        !iUriToIndex.TryGetValue(e.Artwork.Uri, out index))
                    {
                        return;
                    }

                    Assert.Check(index < iImages.Count);
                    iImages[index] = e.Artwork.Image;
                }

                DUpdated updated = iUpdated;
                if (updated != null)
                    updated(this);
            }

            private IArtworkCache iArtworkCache;
            private DUpdated iUpdated;
            private Dictionary<Uri, int> iUriToIndex;
        }

        private BrowserItems iBrowserItems;
        private BrowserItems.State iBrowserItemsState;
        private IPlaylistSupport iPlaylistSupport;
        private bool iOpen;
        private ViewBrowserContextMenu iContextMenu;
        private ListView iListView;
        private IArtworkCache iArtworkCache;
        private ImageList iImageList;
        private KinskyPda.Widgets.PanelBusy iPanelBusy;
        private OptionBool iOptionShowArtwork;
    }
}
