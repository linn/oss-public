using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

using Upnp;
using Linn.Kinsky;

namespace OssKinskyMppSoundcard
{
    interface IView
    {
        Control ViewControl { get; }
        string[] Path { get; set; }
        void Open();
        void Close();
        void OnSizeClick();
        void OnViewClick();
        event EventHandler<EventArgs> EventLocationChanged;
    }

    [System.ComponentModel.DesignerCategory("")]

    class View : Control, IView
    {
        internal View(IMediaProviderSupportV7 aSupport, SoundDriver aDirectSound)
        {
            iSupport = aSupport;
            iSupport.ViewSupport.EventSupportChanged += EventSupportChanged;

            iDirectSound = aDirectSound;

            iMutex = new Mutex();

            iLevel = 0;

            iLocation = new List<string>();

            iViewSize = 1;

            iListView = new ListViewKinsky();
            iListView.Dock = DockStyle.Fill;
            iListView.Visible = true;
            iListView.AllowDrop = true;

            SetViewColours();

            iListView.View = System.Windows.Forms.View.Tile;

            ColumnHeader header;
            header = new ColumnHeader();
            header.Name = "Icon";
            header.Text = "Icon";
            iListView.Columns.Add(header);
            header = new ColumnHeader();
            header.Name = "Title";
            header.Text = "Title";
            header.Width = -2;
            iListView.Columns.Add(header);
            iListView.HeaderStyle = ColumnHeaderStyle.None;

            iListView.ItemActivate += OnActivate;

            iListView.ItemDrag += OnItemDrag;
            iListView.DragOver += OnDragOver;

            Populate();

            Controls.Add(iListView);

            Dock = DockStyle.Fill;
        }

        public event EventHandler<EventArgs> EventLocationChanged;

        public string[] Path
        {
            get
            {
                return (iLocation.ToArray());
            }
            set
            {
            }
        }

        void ResizeControls()
        {
            if (iViewSize == 0)
            {
                iListView.Font = iSupport.ViewSupport.FontSmall;
                iListView.LargeIconSize = new Size(64, 64);
                iListView.SmallIconSize = new Size(20, 20);
                iListView.TileHeight = 70;
            }
            else if (iViewSize == 1)
            {
                iListView.Font = iSupport.ViewSupport.FontMedium;
                
                iListView.LargeIconSize = new Size(128, 128);
                iListView.SmallIconSize = new Size(40, 40);
                iListView.TileHeight = 140;
            }
            else
            {
                iListView.Font = iSupport.ViewSupport.FontLarge;

                iListView.LargeIconSize = new Size(256, 256);
                iListView.SmallIconSize = new Size(80, 80);
                iListView.TileHeight = 281;
            }
        }

        public void OnSizeClick()
        {
            iListView.Visible = false;

            if (++iViewSize > 2)
            {
                iViewSize = 0;
            }

            Populate();

            iListView.Visible = true;
        }

        public void OnViewClick()
        {
            iListView.Visible = false;

            if (iListView.View == System.Windows.Forms.View.LargeIcon)
            {
                iListView.View = System.Windows.Forms.View.Tile;
            }
            else if (iListView.View == System.Windows.Forms.View.Tile)
            {
                iListView.View = System.Windows.Forms.View.LargeIcon;
            }

            Populate();

            iListView.Visible = true;
        }

        void Populate()
        {
            iMutex.WaitOne();

            iListView.Visible = false;

            iListView.BeginUpdate();

            iListView.Items.Clear();

            ResizeControls();

            foreach (ISource s in iDirectSound.SourceList)
            {
                ListViewKinsky.Item item = new ListViewKinsky.Item();
                item.Text = s.Name;
                item.Icon = OssKinskyMppSoundcard.Properties.Resources.Soundcard;
                item.IconSelected = item.Icon;

                ListViewItem.ListViewSubItem subitem;

                subitem = new ListViewItem.ListViewSubItem();
                subitem.Name = "Title";
                subitem.Text = s.Name;
                item.SubItems.Add(subitem);

                iListView.Items.Add(item);
            }

            iListView.Columns[0].Width = iListView.SmallIconSize.Width + 8;
            iListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);

            iListView.EndUpdate();

            iMutex.ReleaseMutex();

            iListView.Visible = true;

            if (iListView.SelectedIndices.Count > 0)
            {
                iListView.EnsureVisible(iListView.SelectedIndices[0]);
            }
            else if (iListView.Items.Count > 0)
            {
                iListView.EnsureVisible(0);
            }
        }

        private void OnActivate(object obj, EventArgs e)
        {
        }

        public void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            int selected = iListView.SelectedIndices[0];

            iSource = iDirectSound.SourceList[selected];

            List<upnpObject> list = new List<upnpObject>();

            list.Add(CreateUpnpObject(iSource));

            iSupport.PlaylistSupport.SetDragging(true);

            DragDropEffects effect = iListView.DoDragDrop(new MediaProviderDraggable(new MediaRetrieverNoRetrieve(list)), DragDropEffects.All);

            iSupport.PlaylistSupport.SetDragging(false);
        }

        private Uri GetUri(MemoryStream aMemoryStream)
        {
            byte[] buffer = new byte[aMemoryStream.Length];

            aMemoryStream.Read(buffer, 0, buffer.Length);

            try
            {
                Uri uri = new Uri(ASCIIEncoding.UTF8.GetString(buffer).TrimEnd('\0'));
                return uri;
            }
            catch (UriFormatException)
            {
                return (null);
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy || (e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
            {
                if (e.Data.GetDataPresent("UniformResourceLocator"))
                {
                    MemoryStream stream = e.Data.GetData("UniformResourceLocator") as MemoryStream;

                    Uri uri = GetUri(stream);

                    if (uri != null)
                    {
                        if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                        {
                            e.Effect = DragDropEffects.Copy;
                        }
                        else
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                    }
                }
                else if (e.Data.GetDataPresent(DataFormats.Text) && e.Effect == DragDropEffects.None)
                {
                    string text = e.Data.GetData(DataFormats.Text) as string;

                    try
                    {
                        Uri uri = new Uri(text);

                        if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                        {
                            e.Effect = DragDropEffects.Copy;
                        }
                        else
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                    }
                    catch (UriFormatException)
                    {
                    }
                }
            }
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public Control ViewControl
        {
            get
            {
                return (iListView);
            }
        }

        private upnpObject CreateUpnpObject(ISource aSource)
        {
            musicTrack item = new musicTrack();
            resource resource = new resource();
            resource.Uri = iDirectSound.SourceUri(aSource);
            resource.ProtocolInfo = "http-get:*:audio/x-wav:*";
            resource.Bitrate = 44100;
            item.Res.Add(resource);
            item.Id = aSource.Id;
            item.Title = aSource.Name;
            item.Album.Add("SoundCard");
            artist artist = new artist();
            artist.Artist = "Various";
            item.Artist.Add(artist);
            //item.AlbumArtUri = aSource.LogoUri;
            return (item);
        }

        private void EventSupportChanged(object sender, EventArgs e)
        {
            SetViewColours();
            Populate();
        }

        private void SetViewColours()
        {
            iListView.BackColor = iSupport.ViewSupport.BackColour;
            iListView.ForeColor = iSupport.ViewSupport.ForeColour;
            iListView.ForeColorMuted = iSupport.ViewSupport.ForeColourMuted;
            iListView.ForeColorBright = iSupport.ViewSupport.ForeColourBright;
            iListView.HighlightBackColour = iSupport.ViewSupport.HighlightBackColour;
            iListView.HighlightForeColour = iSupport.ViewSupport.HighlightForeColour;
        }

        IMediaProviderSupportV7 iSupport;
        SoundDriver iDirectSound;
        Mutex iMutex;
        ListViewKinsky iListView;
        ISource iSource;

        uint iViewSize;
        uint iLevel;
        List<string> iLocation;
    }
}
