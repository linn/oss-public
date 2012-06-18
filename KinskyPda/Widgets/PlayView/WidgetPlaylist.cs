
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

using KinskyPda;

using Upnp;
using Linn;
using Linn.Kinsky;
using Linn.Topology;

namespace KinskyPda.Widgets
{
    public class WidgetPlaylist : BaseListView, IViewWidgetPlaylist
    {
        private const int kTrackIcon = 0;

        private bool iDisplayExtendedInfo;
        private OptionBool iOptionPlaylistInfo;

        public event EventHandler<EventArgsPlaylistDelete> EventPlaylistDelete;
        public event EventHandler<EventArgs> EventPlaylistDeleteAll;
        public event EventHandler<EventArgsSeekTrack> EventSeekTrack;
        public event EventHandler<EventArgsPlaylistInsert> EventPlaylistInsert { add { } remove { } }
        public event EventHandler<EventArgsPlaylistMove> EventPlaylistMove { add { } remove { } }

        public delegate void DEventHandler();
        public event DEventHandler EventOpen;
        public event DEventHandler EventClose;

        private IList<MrItem> iPlaylist;
        
        //A DS currently has a moderator time of 300ms. The PDA will sometimes take longer than this to insert tracks to the DS
        //as such we end up with the device sending the track listing multiple times. In order to stop multiple calls to 
        //setplaylist a timer is used to implement a moderator on the pda.
        private System.Threading.Timer iTimer;
        private const int kModeratorInterval = 750;

        //max column width is 1.5 screen size
        private const int kExtMaxColumnWidth = 720;
        //initial size .5 screen width (minus .5 scrollbar)
        private const int kExtColumnWidth = 225;

        private Mutex iMutex;

        private int iExtTitleWidth = kExtColumnWidth;
        private int iExtArtistWidth = kExtColumnWidth;
        private int iExtAlbumWidth = kExtColumnWidth;
        private const int kExtTypeWidth = 140;
        private const int kExtDurationWidth = 140;

        private int iCurrentlySelectedIndex = -1;
        //the set track and set track list events can occur out of sequence this var is used to set a
        //track if the event occurs before the playlist is populated
        private MrItem iSelectedTrack = null;

        private System.Windows.Forms.ImageList iSmallImageList;

        public WidgetPlaylist()
        {
            iSmallImageList = new System.Windows.Forms.ImageList();
            iSmallImageList.ImageSize = LayoutManager.Instance.SmallImageSize;
            iSmallImageList.Images.Add(TextureManager.Instance.IconPlaying);

            this.SuspendLayout();
            this.Bounds = LayoutManager.Instance.CentreBounds;
            this.Activation = ItemActivation.TwoClick;
            this.BackColor = ViewSupport.Instance.BackColour;
            this.Font = ViewSupport.Instance.FontMedium;
            this.ForeColor = ViewSupport.Instance.ForeColourBright;
            this.FullRowSelect = true;
            this.HeaderStyle = ColumnHeaderStyle.None;
            this.SmallImageList = iSmallImageList;
            this.View = View.Details;
            this.ItemActivate += WidgetPlaylist_ItemActivate;
            this.SelectedIndexChanged += WidgetPlaylist_SelectedIndexChanged;
            this.ResumeLayout(false);

            iTimer = new System.Threading.Timer(EventTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);

            iMutex = new Mutex();
        }

        public void Load(ContextMenu aContextMenu, OptionBool aOptionPlaylistInfo)
        {
            iOptionPlaylistInfo = aOptionPlaylistInfo;
            iOptionPlaylistInfo.EventValueChanged += ExtendedInfoChanged;

            this.ContextMenu = aContextMenu;

            SubscribeToMenuEvents(aContextMenu);
        }

        private void SubscribeToMenuEvents(ContextMenu aContextMenu)
        {
            foreach (MenuItem item in aContextMenu.MenuItems)
            {
                if (item.Text == "Delete Track")
                {
                    item.Click += new System.EventHandler(this.ContextMenuDelete_Click);
                }
                if (item.Text == "Delete All")
                {
                    item.Click += new System.EventHandler(this.ContextMenuDeleteAll_Click);
                }
            }
        }

        private delegate void ExtendedInfoChangedDelegate(object sender, EventArgs e);
        private void ExtendedInfoChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ExtendedInfoChangedDelegate(ExtendedInfoChanged), new object[] { sender, e });
            }
            else
            {
                iDisplayExtendedInfo = !iDisplayExtendedInfo;

                if (iDisplayExtendedInfo)
                {
                    EnableExtInfo();
                }
                else
                {
                    DisableExtInfo();
                }
            }
        }

        private void ContextMenuDeleteAll_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void ContextMenuDelete_Click(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            ListViewItem selectedItem = null;
            int? selectedIndex = base.GetSelectedItem(out selectedItem);

            if (selectedIndex != null)
            {
                List<MrItem> tracks = new List<MrItem>();
                tracks.Add(selectedItem.Tag as MrItem);

                EventArgsPlaylistDelete args = new EventArgsPlaylistDelete(tracks);

                iMutex.ReleaseMutex();

                if (EventPlaylistDelete != null)
                {
                    EventPlaylistDelete(this, args);
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void StartModerator()
        {
            iTimer.Change(kModeratorInterval, Timeout.Infinite);
        }

        private delegate void SetPlaylistDelegate(IList<MrItem> aPlaylist);
        public void SetPlaylist(IList<MrItem> aPlaylist)
        {
            iMutex.WaitOne();
            iPlaylist = aPlaylist;
            iMutex.ReleaseMutex();
            StartModerator();
        }

        private delegate void EventTimerElapsedDelegate(object sender);
        private void EventTimerElapsed(object sender)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventTimerElapsedDelegate(EventTimerElapsed), new object[] { sender });
            }
            else
            {
                UpdatePlaylist();
            }
        }

        private void UpdatePlaylist()
        {
            iMutex.WaitOne();
            IList<MrItem> playlist = iPlaylist;
            iMutex.ReleaseMutex();

            BeginUpdate();

            int i = 0;
            // re-use previously created objects
            for (; i < Items.Count && i < playlist.Count; ++i)
            {
                MrItem listItem = playlist[i];
                string title = DidlLiteAdapter.Title(listItem.DidlLite[0]);
                string artist = DidlLiteAdapter.Artist(listItem.DidlLite[0]);
                string album = DidlLiteAdapter.Album(listItem.DidlLite[0]);
                string type = DidlLiteAdapter.MimeType(listItem.DidlLite[0]);
                string duration = DidlLiteAdapter.Duration(listItem.DidlLite[0]);

                ListViewItem item = Items[i];
                item.SubItems[1].Text = title;
                item.SubItems[2].Text = artist;
                item.SubItems[3].Text = album;
                item.SubItems[4].Text = type;
                item.SubItems[5].Text = duration;
                item.Tag = listItem;
            }

            // add new items as list has increased in size
            for (; i < playlist.Count; ++i)
            {
                MrItem listItem = playlist[i];
                string title = DidlLiteAdapter.Title(listItem.DidlLite[0]);
                string artist = DidlLiteAdapter.Artist(listItem.DidlLite[0]);
                string album = DidlLiteAdapter.Album(listItem.DidlLite[0]);
                string type = DidlLiteAdapter.MimeType(listItem.DidlLite[0]);
                string duration = DidlLiteAdapter.Duration(listItem.DidlLite[0]);

                //add track number to start of track
                ListViewItem item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(title);
                item.SubItems.Add(artist);
                item.SubItems.Add(album);
                item.SubItems.Add(type);
                item.SubItems.Add(duration);
                item.Tag = listItem;

                Items.Add(item);
            }

            // remove unused items
            int count = Items.Count;
            for (int j = i; j < count; ++j)
            {
                Items.RemoveAt(i);
            }

            //if set track occurred before populate playlist
            if (iSelectedTrack != null)
            {
                ListViewItem foundItem = null;
                int? foundIndex = base.FindByTag(iSelectedTrack, out foundItem);

                if (foundIndex.HasValue)
                {
                    SetSelectedTrack(foundItem, foundIndex.Value);
                }
            }

            EndUpdate();

            if (SelectedIndices.Count == 0)
            {
                //make focussed item unfocussed
                ListViewItem item = FocusedItem;
                if (item != null)
                {
                    item.Focused = false;
                }
            }
        }

        //the width of the colums in the extended play view is default 10 characters expanded to fit column text
        //to a max of max column width
        private int GetExtColumnWidth(int aCurrentWidth, string aText)
        {
            int textWidth = Ceiling(MeasureString(aText).Width);

            if (textWidth >= kExtMaxColumnWidth) 
            {
                return kExtMaxColumnWidth;
            }

            if (textWidth > aCurrentWidth) 
            {
                return textWidth;
            }

            return aCurrentWidth;
        }

        private delegate void DOpen();
        public void Open()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DOpen(Open));
            }
            else
            {
                if (EventOpen != null)
                    EventOpen();
            }
        }

        private delegate void DClose();
        public void Close()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new DClose(Close));
            }
            else
            {
                Visible = false;

                this.HeaderStyle = ColumnHeaderStyle.None;
                this.Columns.Clear();
                this.Items.Clear();

                if (EventClose != null)
                    EventClose();
            }
        }

        private delegate void InitialisedDelegate();
        public void Initialised()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new InitialisedDelegate(Initialised), new object[] { });
            }
            else
            {
                Visible = true;

                HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
                iDisplayExtendedInfo = iOptionPlaylistInfo.Native;

                if (iDisplayExtendedInfo)
                {
                    EnableExtInfo();
                }
                else
                {
                    DisableExtInfo();
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (iDisplayExtendedInfo)
            {
                EnableExtInfo();
            }
            else
            {
                DisableExtInfo();
            }
        }

        private void EnableExtInfo()
        {
            int trackColumnWidth = GetTrackNumberColumnWidth();
            
            this.Columns.Clear();
            this.Columns.Add("", trackColumnWidth, HorizontalAlignment.Left);
            this.Columns.Add("Title", iExtTitleWidth, HorizontalAlignment.Left);
            this.Columns.Add("Artist", iExtArtistWidth, HorizontalAlignment.Left);
            this.Columns.Add("Album", iExtAlbumWidth, HorizontalAlignment.Left);
            this.Columns.Add("Type", kExtTypeWidth, HorizontalAlignment.Left);
            this.Columns.Add("Duration", kExtDurationWidth, HorizontalAlignment.Left);
        }

        private void DisableExtInfo()
        {
            int trackColumnWidth = GetTrackNumberColumnWidth();
            int titleColumnWidth = this.Width - BaseListView.kScrollbarWidth - trackColumnWidth;

            this.Columns.Clear();
            this.Columns.Add("", trackColumnWidth, HorizontalAlignment.Left);
            this.Columns.Add("Title", titleColumnWidth, HorizontalAlignment.Left);
        }

        private int Ceiling(float aNum)
        {
            return (int)(aNum + 1);
        }

        private int GetTrackNumberColumnWidth()
        {
            SizeF size = MeasureString("111"); 

            int textwidth = Ceiling(size.Width);
            int imageWidth = iSmallImageList.ImageSize.Width;
            return textwidth + imageWidth;
        }

        private SizeF MeasureString(string aString)
        {
            Bitmap bitmap = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bitmap);
            SizeF size = g.MeasureString(aString, this.Font);
            g.Dispose();

            return size;
        }

        private delegate void SetTrackDelegate(MrItem aTrack);
        public void SetTrack(MrItem aTrack)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new SetTrackDelegate(SetTrack), new object[] { aTrack });
            }
            else
            {
                iSelectedTrack = aTrack;

                //get selected track
                ListViewItem foundItem = null;
                int? foundIndex = base.FindByTag(aTrack, out foundItem);
                if (foundIndex.HasValue)
                {
                    SetSelectedTrack(foundItem, foundIndex.Value);
                }
            }
        }

        private void SetSelectedTrack(ListViewItem aFoundItem, int aFoundIndex)
        {
            //remove now playing icon from previously selected if the track was changed from a different device
            if (iCurrentlySelectedIndex > -1 && iCurrentlySelectedIndex != aFoundIndex)
            {
                if (iCurrentlySelectedIndex < Items.Count)
                {
                    ListViewItem currentlySelected = Items[iCurrentlySelectedIndex];
                    currentlySelected.ImageIndex = -1;
                }
            }

            //set listview item icon
            aFoundItem.ImageIndex = kTrackIcon;

            //remove highlighting from item
            aFoundItem.Selected = false;
            aFoundItem.Focused = false;

            iCurrentlySelectedIndex = aFoundIndex;
        }

        public void Save()
        {
        }

        public void Delete()
        {
            if (EventPlaylistDeleteAll != null)
            {
                EventPlaylistDeleteAll(this, new EventArgs());
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // Calling the base class OnPaint
            base.OnPaint(pe);
        }

        private void WidgetPlaylist_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.SelectedIndices.Count == 0)
            {
                try
                {
                    //make focussed item unfocussed
                    ListViewItem item = this.FocusedItem;
                    if (item != null)
                    {
                        item.Focused = false;
                    }
                }
                catch (ArgumentOutOfRangeException) { } // can throw if deleting item that is focused between BeginUpdate/EndUpdate
            }
        }

        private void WidgetPlaylist_ItemActivate(object sender, EventArgs e)
        {
            ListViewItem selectedItem = null;
            int? selectedIndex = base.GetSelectedItem(out selectedItem);

            if (selectedIndex.HasValue)
            {
                //make unselected to remove highlight
                selectedItem.Selected = false;
                selectedItem.Focused = false;

                if (EventSeekTrack != null)
                {
                    EventSeekTrack(this, new EventArgsSeekTrack((uint)selectedIndex.Value));
                }
            }

        }


    }
}
