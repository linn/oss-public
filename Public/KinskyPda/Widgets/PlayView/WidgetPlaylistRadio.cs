
using System;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

using Linn;
using Linn.Topology;
using Linn.Kinsky;
using Upnp;

namespace KinskyPda.Widgets
{
    public class WidgetPlaylistRadio : BaseListView, IViewWidgetPlaylistRadio
    {
        public WidgetPlaylistRadio()
        {
            iSmallImageList = new ImageList();
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
            this.ItemActivate += WidgetPlaylistRadio_ItemActivate;
            this.SelectedIndexChanged += WidgetPlaylistRadio_SelectedIndexChanged;
            this.ResumeLayout(false);

            iOpen = false;

            int trackColumnWidth = GetTrackNumberColumnWidth();
            int titleColumnWidth = Width - BaseListView.kScrollbarWidth - trackColumnWidth;

            Columns.Clear();
            Columns.Add("", trackColumnWidth, HorizontalAlignment.Left);
            Columns.Add("Title", titleColumnWidth, HorizontalAlignment.Left);

            HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
        }

        public delegate void DEventHandler();
        public event DEventHandler EventOpen;
        public event DEventHandler EventClose;

        private delegate void DOpen();
        public void Open()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DOpen(Open));
            }
            else
            {
                Assert.Check(!iOpen);
                SetPreset(-1);
                iOpen = true;
                if (EventOpen != null)
                    EventOpen();
            }
        }

        private delegate void DClose();
        public void Close()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DClose(Close));
            }
            else
            {
                iOpen = false;
                Visible = false;
                if (EventClose != null)
                    EventClose();
            }
        }

        private delegate void DInitialised();
        public void Initialised()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DInitialised(Initialised));
            }
            else
            {
                if (iOpen)
                {
                    Visible = true;
                }
            }
        }

        private delegate void DSetPresets(IList<MrItem> aPresets);
        public void SetPresets(IList<MrItem> aPresets)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DSetPresets(SetPresets), new object[] { aPresets });
            }
            else
            {
                BeginUpdate();

                int i = 0;
                // re-use previously created objects
                for (; i < Items.Count && i < aPresets.Count; ++i)
                {
                    MrItem preset = aPresets[i];
                    string title = DidlLiteAdapter.Title(preset.DidlLite[0]);
                    /*string artist = DidlLiteAdapter.Artist(preset.DidlLite[0]);
                    string album = DidlLiteAdapter.Album(preset.DidlLite[0]);
                    string type = DidlLiteAdapter.MimeType(preset.DidlLite[0]);
                    string duration = DidlLiteAdapter.Duration(preset.DidlLite[0]);*/

                    ListViewItem item = Items[i];
                    item.SubItems[1].Text = title;
                    /*item.SubItems[2].Text = artist;
                    item.SubItems[3].Text = album;
                    item.SubItems[4].Text = type;
                    item.SubItems[5].Text = duration;*/
                    item.Tag = preset;
                }

                // add new items as list has increased in size
                for (; i < aPresets.Count; ++i)
                {
                    MrItem preset = aPresets[i];
                    string title = DidlLiteAdapter.Title(preset.DidlLite[0]);
                    /*string artist = DidlLiteAdapter.Artist(preset.DidlLite[0]);
                    string album = DidlLiteAdapter.Album(preset.DidlLite[0]);
                    string type = DidlLiteAdapter.MimeType(preset.DidlLite[0]);
                    string duration = DidlLiteAdapter.Duration(preset.DidlLite[0]);*/

                    //add track number to start of track
                    ListViewItem item = new ListViewItem((i + 1).ToString());
                    item.SubItems.Add(title);
                    /*item.SubItems.Add(artist);
                    item.SubItems.Add(album);
                    item.SubItems.Add(type);
                    item.SubItems.Add(duration);*/
                    item.Tag = preset;

                    Items.Add(item);
                }

                // remove unused items
                int count = Items.Count;
                for (int j = i; j < count; ++j)
                {
                    Items.RemoveAt(i);
                }

                SetPreset(iPresetIndex);

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
        }

        public void SetChannel(Channel aChannel)
        {
        }

        private delegate void DSetPreset(int aPresetIndex);
        public void SetPreset(int aPresetIndex)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DSetPreset(SetPreset), new object[] { aPresetIndex });
            }
            else
            {
                //remove now playing icon from previously selected if the track was changed from a different device
                if (iPresetIndex > -1 && iPresetIndex != aPresetIndex)
                {
                    if (iPresetIndex < Items.Count)
                    {
                        ListViewItem item = Items[iPresetIndex];
                        item.ImageIndex = -1;
                    }
                }

                if (aPresetIndex > -1 && aPresetIndex < Items.Count)
                {
                    ListViewItem item = Items[aPresetIndex];

                    //set listview item icon
                    item.ImageIndex = kPresetIcon;

                    //remove highlighting from item
                    item.Selected = false;
                    item.Focused = false;
                }

                iPresetIndex = aPresetIndex;
            }
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgsSetPreset> EventSetPreset;
        public event EventHandler<EventArgsSetChannel> EventSetChannel { add { } remove { } }

        private int GetTrackNumberColumnWidth()
        {
            SizeF size = MeasureString("888");

            int textwidth = (int)Math.Ceiling(size.Width);
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

        private void WidgetPlaylistRadio_ItemActivate(object sender, EventArgs e)
        {
            ListViewItem selectedItem = null;
            int? selectedIndex = base.GetSelectedItem(out selectedItem);

            if (selectedIndex.HasValue)
            {
                //make unselected to remove highlight
                selectedItem.Selected = false;
                selectedItem.Focused = false;

                if (EventSetPreset != null)
                {
                    EventSetPreset(this, new EventArgsSetPreset(selectedItem.Tag as MrItem));
                }
            }
        }

        private void WidgetPlaylistRadio_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndices.Count == 0)
            {
                try
                {
                    //make focussed item unfocussed
                    ListViewItem item = FocusedItem;
                    if (item != null)
                    {
                        item.Focused = false;
                    }
                }
                catch (ArgumentOutOfRangeException) { } // can throw if deleting item that is focused between BeginUpdate/EndUpdate
            }
        }

        private const int kPresetIcon = 0;

        private bool iOpen;
        private int iPresetIndex;
        private System.Windows.Forms.ImageList iSmallImageList;
    }
}
