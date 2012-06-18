using System.Windows.Forms;
using System.Drawing;
using System;
using System.Runtime.InteropServices;
using System.Threading;

using Upnp;
using Linn;
using Linn.Kinsky;
using Linn.Topology;

namespace KinskyDesktop.Widgets
{
    [System.ComponentModel.DesignerCategory("")]

    public class ListViewLibrary : ListViewKinsky
    {
        public new class Item : ListViewKinsky.Item
        {
            public Item(IArtworkCache aCache)
            {
                iCache = aCache;
                iMutex = new Mutex(false);
            }

            public Item(string aText, IArtworkCache aCache)
                : base(aText)
            {
                iCache = aCache;
                iMutex = new Mutex(false);
            }

            public void SetArtwork(Uri aUri)
            {
                iMutex.WaitOne();
                iUri = aUri;
                iMutex.ReleaseMutex();
            }

            public override Image Icon
            {
                get
                {
                    iMutex.WaitOne();

                    if (iUri != null)
                    {
                        IArtwork artwork = iCache.Artwork(iUri);
                        if (artwork.Error)
                        {
                            ToolTipText = "Error downloading artwork from server";
                            if (iErrorIcon != null)
                            {
                                return iErrorIcon;
                            }
                        }
                        else if(artwork.Image != null)
                        {
                            return artwork.Image;
                        }
                    }

                    iMutex.ReleaseMutex();

                    return base.Icon;
                }
            }

            public override Image IconSelected
            {
                get
                {
                    iMutex.WaitOne();

                    if (iUri != null)
                    {
                        IArtwork artwork = iCache.Artwork(iUri);
                        if (artwork.Error)
                        {
                            if (iErrorIconSelected != null)
                            {
                                return iErrorIconSelected;
                            }
                        }
                        else
                        {
                            if (artwork.Image != null)
                            {
                                return artwork.Image;
                            }
                        }
                    }

                    iMutex.ReleaseMutex();

                    return base.IconSelected;
                }
            }

            public Image ErrorIcon
            {
                get
                {
                    return iErrorIcon;
                }
                set
                {
                    iErrorIcon = value;
                }
            }

            public Image ErrorIconSelected
            {
                get
                {
                    return iErrorIconSelected;
                }
                set
                {
                    iErrorIconSelected = value;
                }
            }

            private Mutex iMutex;

            private Uri iUri;
            private IArtworkCache iCache;

            private Image iErrorIcon;
            private Image iErrorIconSelected;
        }

        public ListViewLibrary()
        {
            iIsAlbum = false;
            iView = base.View;
        }

        public bool IsAlbum
        {
            get
            {
                return iIsAlbum;
            }
            set
            {
                if (value)
                {
                    if (base.View != System.Windows.Forms.View.Details)
                    {
                        iView = base.View;
                        base.View = System.Windows.Forms.View.Details;
                    }
                }
                else
                {
                    base.View = iView;
                }
                iIsAlbum = value;
            }
        }

        public new System.Windows.Forms.View View
        {
            get
            {
                return iView;
            }
            set
            {
                if (iIsAlbum)
                {
                    base.View = System.Windows.Forms.View.Details;
                }
                else
                {
                    base.View = value;
                }
                iView = value;
            }
        }

        protected override void EventListViewLibrary_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;

            // Because of a bug in the underlying Win32 control, the DrawItem event occurs without accompanying DrawSubItem events once per row in the
            // details view when the mouse pointer moves over the row, causing anything painted in a DrawSubItem event handler to be painted over by a
            // custom background drawn in a DrawItem event handler. See the example in the OwnerDraw reference topic for a workaround that invalidates
            // each row when the extra event occurs. An alternative workaround is to put all your custom drawing code in a DrawSubItem event handler
            // and paint the background for the entire item (including subitems) only when the DrawListViewSubItemEventArgs.ColumnIndex value is 0. 
            if (e.ColumnIndex == 0)
            {
                e.Graphics.FillRectangle(iBackBrush, e.Bounds);

                if (iIsAlbum && Items.Count > 0)
                {
                    ListViewKinsky.Item item = Items[0] as ListViewKinsky.Item;
                    musicTrack track = item.Tag as musicTrack;

                    int iconHeight = (int)(Columns[0].Width * 0.5f);
                    int iconOffset = 5;// (int)((Columns[0].Width - iconHeight) * 0.5f);

                    Image albumArt = item.Icon;
                    if (albumArt != null)
                    {
                        e.Graphics.FillRectangle(iBackBrush, item.Bounds.Left + iconOffset, item.Bounds.Top + iconOffset, iconHeight, iconHeight);
                        try
                        {
                            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            e.Graphics.DrawImage(albumArt, item.Bounds.Left + iconOffset, item.Bounds.Top + iconOffset, iconHeight, iconHeight);
                        }
                        catch (ArgumentException ex)
                        {
                            Trace.WriteLine(Trace.kKinsky, "ListViewLibrary.EventListViewLibrary_DrawSubItem: " + ex.Message);
                        }
                    }

                    if (track != null)
                    {                        
                        int x = item.Bounds.Left + iconOffset + iconHeight + iconOffset;
                        int y = item.Bounds.Top + iconOffset;
                        int width = Columns[0].Width + item.Bounds.Left;
                        int height = iFont.Height;

                        e.Graphics.FillRectangle(iBackBrush, new Rectangle(x, y, width - x, height * 4));

                        string album = DidlLiteAdapter.Album(track);
                        if (!string.IsNullOrEmpty(album))
                        {
                            TextRenderer.DrawText(e.Graphics, album, iFont, new Rectangle(x, y, width - x, height), ForeColor, flags);
                            y += height;
                        }

                        string artist = DidlLiteAdapter.AlbumArtist(track);
                        if (!string.IsNullOrEmpty(artist))
                        {
                            TextRenderer.DrawText(e.Graphics, artist, iFont, new Rectangle(x, y, width - x, height), Color.DarkGray, flags);
                            y += height;
                        }

                        string genre = DidlLiteAdapter.Genre(track);
                        if (!string.IsNullOrEmpty(genre))
                        {
                            TextRenderer.DrawText(e.Graphics, genre, iFont, new Rectangle(x, y, width - x, height), Color.DarkGray, flags);
                            y += height;
                        }

                        TextRenderer.DrawText(e.Graphics, DidlLiteAdapter.ReleaseYear(track), iFont, new Rectangle(x, y, width - x, height), Color.DarkGray, flags);
                    }
                }
            }
            //Console.WriteLine("columIndex=" + e.ColumnIndex + ", header=" + e.Header + ", item=" + e.Item + ", itemIndex=" + e.ItemIndex + ", subItem=" + e.SubItem);
            //e.DrawDefault = true;
            //return;

            if (e.ColumnIndex > 0)
            {
                if (e.Item.Selected)
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    e.Graphics.FillRectangle(iHighlightBackBrush, e.Bounds);
                }
                else
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    e.Graphics.FillRectangle(iBackBrush, e.Bounds);
                }
            }

            ListViewItem.ListViewSubItem subItem = null;
            for (int i = 0; i < e.Item.SubItems.Count; ++i)
            {
                if (e.Item.SubItems[i].Name == e.Header.Name)
                {
                    subItem = e.Item.SubItems[i];
                    break;
                }
            }

            if (subItem != null)
            {
                if (e.Item.Selected)
                {
                    TextRenderer.DrawText(e.Graphics, subItem.Text, iFont, e.Bounds, HighlightForeColour, flags);
                }
                else
                {
                    if (e.Header.Name != "Title")
                    {
                        TextRenderer.DrawText(e.Graphics, subItem.Text, iFont, e.Bounds, ForeColorMuted, flags);
                    }
                    else
                    {
                        TextRenderer.DrawText(e.Graphics, subItem.Text, iFont, e.Bounds, ForeColor, flags);
                    }
                }
            }
        }

        private bool iIsAlbum;
        private System.Windows.Forms.View iView;
    }
} // OssKinskyMppLibrary
