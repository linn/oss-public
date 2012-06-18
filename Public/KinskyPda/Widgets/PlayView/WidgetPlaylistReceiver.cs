
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

using Linn;
using Linn.Topology;
using Linn.Kinsky;

namespace KinskyPda.Widgets
{
    public class WidgetPlaylistReceiver : BaseListView, IViewWidgetPlaylistReceiver
    {
        public WidgetPlaylistReceiver()
        {
            iImageList = new ImageList();
            iImageList.ImageSize = LayoutManager.Instance.SmallImageSize;
            iImageList.Images.Add(TextureManager.Instance.IconPlaying);

            this.SuspendLayout();
            this.Bounds = LayoutManager.Instance.CentreBounds;
            this.Activation = ItemActivation.TwoClick;
            this.BackColor = ViewSupport.Instance.BackColour;
            this.Font = ViewSupport.Instance.FontMedium;
            this.ForeColor = ViewSupport.Instance.ForeColourBright;
            this.FullRowSelect = true;
            this.HeaderStyle = ColumnHeaderStyle.None;
            this.SmallImageList = iImageList;
            this.View = View.Details;
            this.ItemActivate += ListViewItemActivate;
            this.ResumeLayout();

            int trackColumnWidth = GetTrackNumberColumnWidth();
            int titleColumnWidth = Width - BaseListView.kScrollbarWidth - trackColumnWidth;

            Columns.Clear();
            Columns.Add("", trackColumnWidth, HorizontalAlignment.Left);
            Columns.Add("Title", titleColumnWidth, HorizontalAlignment.Left);

            HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }

        public delegate void DEventHandler();
        public event DEventHandler EventOpen;
        public event DEventHandler EventClose;

        #region IViewWidgetPlaylistReceiver implementation
        private delegate void DOpen();
        public void Open()
        {
            if (InvokeRequired)
            {
                BeginInvoke((DOpen)Open);
            }
            else
            {
                iCurrent = -1;
                iChannel = null;
                Items.Clear();
                if (EventOpen != null)
                    EventOpen();
            }
        }

        private delegate void DClose();
        public void Close()
        {
            if (InvokeRequired)
            {
                BeginInvoke((DClose)Close);
            }
            else
            {
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
                BeginInvoke((DInitialised)Initialised);
            }
            else
            {
                Visible = true;
            }
        }

        private delegate void DSetSenders(IList<ModelSender> aSenders);
        public void SetSenders(IList<ModelSender> aSenders)
        {
            if (InvokeRequired)
            {
                BeginInvoke((DSetSenders)SetSenders, aSenders);
            }
            else
            {
                BeginUpdate();

                Items.Clear();

                for (int i = 0; i < aSenders.Count; i++)
                {
                    ListViewItem item = new ListViewItem((i + 1).ToString());
                    item.SubItems.Add(Upnp.DidlLiteAdapter.Title(aSenders[i].Metadata[0]));
                    item.Tag = aSenders[i];
                    item.ImageIndex = -1;
                    Items.Add(item);
                }

                UpdateCurrent();

                for (int i = 0; i < Items.Count; i++)
                {
                    Items[i].ImageIndex = (i == iCurrent) ? 0 : -1;
                }

                EndUpdate();
            }
        }

        private delegate void DSetChannel(Channel aChannel);
        public void SetChannel(Channel aChannel)
        {
            if (InvokeRequired)
            {
                BeginInvoke((DSetChannel)SetChannel, aChannel);
            }
            else
            {
                iChannel = aChannel;

                if (UpdateCurrent())
                {
                    BeginUpdate();
                    for (int i = 0; i < Items.Count; i++)
                    {
                        Items[i].ImageIndex = (i == iCurrent) ? 0 : -1;
                    }
                    EndUpdate();
                }
            }
        }

        public void Save()
        {
        }

        public event EventHandler<EventArgsSetChannel> EventSetChannel;
        #endregion IViewWidgetPlaylistReceiver implementation

        private void ListViewItemActivate(object sender, EventArgs e)
        {
            ListViewItem selectedItem = null;
            int? selectedIndex = base.GetSelectedItem(out selectedItem);

            if (selectedIndex.HasValue)
            {
                selectedItem.Selected = false;
                selectedItem.Focused = false;

                if (EventSetChannel != null)
                {
                    ModelSender s = selectedItem.Tag as ModelSender;
                    List<Upnp.upnpObject> items = new List<Upnp.upnpObject>();
                    items.Add(s.Metadata[0]);
                    IMediaRetriever ret = new MediaRetrieverNoRetrieve(items);

                    EventSetChannel(this, new EventArgsSetChannel(ret));
                }
            }
        }

        private int GetTrackNumberColumnWidth()
        {
            SizeF size = MeasureString("888");

            int textwidth = (int)Math.Ceiling(size.Width);
            int imageWidth = iImageList.ImageSize.Width;
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

        private bool UpdateCurrent()
        {
            int index = -1;

            if (iChannel != null)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    ModelSender sender = Items[i].Tag as ModelSender;

                    if (sender.Metadata[0].Title == iChannel.DidlLite[0].Title)
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index != iCurrent)
            {
                iCurrent = index;
                return true;
            }

            return false;
        }

        private ImageList iImageList;
        private int iCurrent;
        private Channel iChannel;
    }
}


