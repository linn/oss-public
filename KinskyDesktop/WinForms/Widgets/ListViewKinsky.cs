using System.Windows.Forms;
using System.Drawing;
using System;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

using Linn;

namespace KinskyDesktop.Widgets
{
    [System.ComponentModel.DesignerCategory("")]

    public class ListViewKinsky : ListView
    {
        public class Item : ListViewItem
        {
            public Item()
            {
            }

            public Item(string aText)
                : base(aText)
            {
            }

            public virtual Image Icon
            {
                get
                {
                    return iImageIcon;
                }
                set
                {
                    iImageIcon = value;
                }
            }

            public virtual Image IconSelected
            {
                get
                {
                    return iImageIconSelected;
                }
                set
                {
                    iImageIconSelected = value;
                }
            }

            public void Draw(Graphics aGraphics, bool aTransparent)
            {
                ListViewKinsky listView = ListView as ListViewKinsky;
                Assert.Check(listView != null);

                aGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                aGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis;

                StringFormat format = new StringFormat();
                format.Trimming = StringTrimming.EllipsisCharacter;

                float alphaFactor = 0.7f;
                int alpha = 255;
                if (aTransparent)
                {
                    alpha = (int)(255 * alphaFactor);
                }

                if (listView.View == System.Windows.Forms.View.LargeIcon || listView.View == System.Windows.Forms.View.SmallIcon)
                {
                    int iconHeight = listView.LargeIconSize.Height;
                    int iconWidth = listView.LargeIconSize.Width;
                    int iconOffset = 3;

                    int y = Bounds.Y + iconOffset + iconHeight;

                    int height = Font.Height * 2;

                    int remainingHeight = Bounds.Bottom - y;
                    height = remainingHeight < height ? Font.Height : height;

                    Image albumArt = Icon;
                    if (albumArt != null)
                    {
                        if (Selected && IconSelected != null)
                        {
                            albumArt = IconSelected;
                        }
                        try
                        {
                            SizeF imageSize = ImageSize(albumArt, iconWidth, iconHeight);
                            if (aTransparent)
                            {
                                ImageAttributes ia = new ImageAttributes();
                                ColorMatrix cm = new ColorMatrix();
                                cm.Matrix33 = alphaFactor;
                                ia.SetColorMatrix(cm);

                                aGraphics.DrawImage(albumArt, new Rectangle((int)(((Bounds.Width - iconWidth) * 0.5f) + ((iconWidth - imageSize.Width) * 0.5f)),
                                    (int)(iconOffset + ((iconHeight - imageSize.Height) * 0.5f)), (int)imageSize.Width, (int)imageSize.Height),
                                    0, 0, albumArt.Width, albumArt.Height, GraphicsUnit.Pixel, ia);
                            }
                            else
                            {
                                aGraphics.DrawImage(albumArt, new RectangleF(Bounds.Left + ((Bounds.Width - iconWidth) * 0.5f) + ((iconWidth - imageSize.Width) * 0.5f),
                                    Bounds.Top + iconOffset + ((iconHeight - imageSize.Height) * 0.5f), imageSize.Width, imageSize.Height));
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            Trace.WriteLine(Trace.kKinsky, "ListViewKinsky.Item.Draw: " + ex.Message);
                        }
                    }

                    using (Brush b = new SolidBrush(Color.FromArgb(alpha, listView.ForeColor)))
                    {
                        format.Alignment = StringAlignment.Center;

                        foreach (ListViewItem.ListViewSubItem i in SubItems)
                        {
                            if (i.Name == "Title")
                            {
                                if (aTransparent)
                                {
                                    aGraphics.DrawString(i.Text, listView.Font, b, new Rectangle(0, y - Bounds.Y, Bounds.Width, height), format);
                                }
                                else
                                {
                                    TextRenderer.DrawText(aGraphics, i.Text, listView.Font, new Rectangle(Bounds.X, y, Bounds.Width, height), listView.ForeColor, flags | TextFormatFlags.HorizontalCenter);
                                }
                                break;
                            }
                        }
                    }
                }
                else if (listView.View == System.Windows.Forms.View.Tile)
                {
                    float scale = 1.0f;
                    int iconHeight = listView.LargeIconSize.Height;
                    if (iconHeight > Bounds.Height - 6)
                    {
                        scale = (listView.LargeIconSize.Height - 6) / (float)iconHeight;
                        iconHeight = (int)(iconHeight * scale);
                    }
                    int iconWidth = (int)(listView.LargeIconSize.Width * scale);
                    int iconOffsetX = 3;
                    int iconOffsetY = (int)(Bounds.Top + ((Bounds.Height - iconHeight) * 0.5f));

                    Image albumArt = Icon;
                    if (albumArt != null)
                    {
                        if (Selected && IconSelected != null)
                        {
                            albumArt = IconSelected;
                        }
                        try
                        {
                            SizeF imageSize = ImageSize(albumArt, iconWidth, iconHeight);
                            if (aTransparent)
                            {
                                ImageAttributes ia = new ImageAttributes();
                                ColorMatrix cm = new ColorMatrix();
                                cm.Matrix33 = alphaFactor;
                                ia.SetColorMatrix(cm);

                                aGraphics.DrawImage(albumArt, new Rectangle(iconOffsetX, iconOffsetY - Bounds.Top, (int)imageSize.Width, (int)imageSize.Height),
                                    0, 0, albumArt.Width, albumArt.Height, GraphicsUnit.Pixel, ia);
                            }
                            else
                            {
                                aGraphics.DrawImage(albumArt, new RectangleF(Bounds.Left + iconOffsetX, iconOffsetY, imageSize.Width, imageSize.Height));
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            Trace.WriteLine(Trace.kKinsky, "ListViewKinsky.EventListViewLibrary_DrawItem: " + ex.Message);
                        }
                    }

                    int x = Bounds.Left + iconOffsetX * 2 + iconWidth;
                    int y = iconOffsetY;

                    for (int i = 0; i < SubItems.Count; ++i)
                    {
                        if (SubItems[i].Name != "")
                        {
                            if (y + listView.Font.Height < iconOffsetY + iconHeight)
                            {
                                using (Brush b = new SolidBrush(Color.FromArgb(alpha, listView.ForeColor)))
                                {
                                    if (aTransparent)
                                    {
                                        aGraphics.DrawString(SubItems[i].Text, listView.Font, b, new Rectangle(x - Bounds.Left, y - Bounds.Top, Bounds.Width - Bounds.Height, listView.Font.Height), format);
                                    }
                                    else
                                    {
                                        TextRenderer.DrawText(aGraphics, SubItems[i].Text, listView.Font, new Rectangle(x, y, Bounds.Width - Bounds.Height, listView.Font.Height), listView.ForeColor, flags);
                                    }
                                }
                                y += listView.Font.Height;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                else if (listView.View == System.Windows.Forms.View.Details)
                {
                    Rectangle rectangleBounds = new Rectangle();
                    rectangleBounds = Bounds;

                    rectangleBounds.Y = 0;
                    rectangleBounds.X += listView.SmallIconSize.Width + 4;
                    rectangleBounds.Width -= listView.SmallIconSize.Width;

                    if (Icon != null)
                    {
                        Image image = Icon;
                        if (Selected && IconSelected != null)
                        {
                            image = IconSelected;
                        }

                        ImageAttributes ia = new ImageAttributes();
                        ColorMatrix cm = new ColorMatrix();
                        cm.Matrix33 = alphaFactor;
                        ia.SetColorMatrix(cm);

                        SizeF imageSize = ImageSize(image, listView.SmallIconSize.Width, listView.SmallIconSize.Height);
                        aGraphics.DrawImage(image, new Rectangle((int)(Bounds.X + 4 + ((listView.SmallIconSize.Width - imageSize.Width) * 0.5f)),
                            (int)(((Bounds.Height - listView.SmallIconSize.Height) * 0.5f) + ((listView.SmallIconSize.Height - imageSize.Height) * 0.5f)),
                            (int)imageSize.Width, (int)imageSize.Height), 0, 0, Icon.Size.Width, Icon.Size.Height, GraphicsUnit.Pixel, ia);
                    }

                    ListViewItem.ListViewSubItem subItem = null;
                    for (int i = 0; i < SubItems.Count; ++i)
                    {
                        if (SubItems[i].Name == "Title")
                        {
                            subItem = SubItems[i];
                            break;
                        }
                    }

                    if (subItem != null)
                    {
                        rectangleBounds.X = subItem.Bounds.X;
                        format.LineAlignment = StringAlignment.Center;

                        using (Brush b = new SolidBrush(listView.ForeColor))
                        {
                            if (aTransparent)
                            {
                                aGraphics.DrawString(subItem.Text, listView.Font, b, rectangleBounds, format);
                            }
                            else
                            {
                                TextRenderer.DrawText(aGraphics, subItem.Text, listView.Font, rectangleBounds, listView.ForeColor, flags);
                            }
                        }
                    }
                }
            }

            private SizeF ImageSize(Image aImage, int aWidth, int aHeight)
            {
                SizeF size = new SizeF(aWidth, aHeight);
                if (aImage.Width > aImage.Height)
                {
                    size.Height = aWidth * (aImage.Height / (float)aImage.Width);
                }
                else if (aImage.Height > aImage.Width)
                {
                    size.Width = aHeight * (aImage.Width / (float)aImage.Height);
                }
                return size;
            }

            private Image iImageIcon;
            private Image iImageIconSelected;
        }

        internal class CursorInfo
        {
            public Item Item;
            public Point HotSpot;
            public Cursor Cursor;
            public DragDropEffects Effect;
        }

        public ListViewKinsky()
        {
            DoubleBuffered = true;
            OwnerDraw = true;

            base.LargeImageList = new ImageList();
            base.SmallImageList = new ImageList();
            base.BorderStyle = BorderStyle.None;
            base.Margin = new Padding(2);
            base.FullRowSelect = true;

            iTimerRedraw = new System.Threading.Timer(TimerRedrawElapsed, null, Timeout.Infinite, Timeout.Infinite);

            iCursorInfo = new CursorInfo();

            iForeColorBright = ForeColor;
            iForeColorMuted = ForeColor;

            iFont = new Font(Font.FontFamily, Font.Size, Font.Style);
            iBackBrush = new SolidBrush(BackColor);
            iForeBrush = new SolidBrush(ForeColor);
            iHighlightBackBrush = new SolidBrush(BackColor);
            iBrightForeBrush = new SolidBrush(ForeColor);

            DrawItem += EventListViewLibrary_DrawItem;
            DrawSubItem += EventListViewLibrary_DrawSubItem;
            DrawColumnHeader += EventListViewLibrary_DrawColumnHeader;
            ItemDrag += EventListViewLibrary_ItemDrag;
            GiveFeedback += EventListViewLibrary_GiveFeedback;
        }

        public Size LargeIconSize
        {
            get
            {
                return iLargeIconSize;
            }
            set
            {
                iLargeIconSize = value;
                int height = value.Height + iFont.Height;
                if(height > 256)
                {
                    height = 256;
                }
                LargeImageList.ImageSize = new Size(value.Width, height);
                //TileSize = new Size(value.Height * 4, (int)(value.Height * 1.1f));
            }
        }

        public Size SmallIconSize
        {
            get
            {
                return iSmallIconSize;
            }
            set
            {
                iSmallIconSize = value;
                SmallImageList.ImageSize = value;
                //TileSize = new Size(value.Height * 4, (int)(value.Height * 1.1f));
            }
        }

        public int TileHeight
        {
            get
            {
                return iTileHeight;
            }
            set
            {
                iTileHeight = value;

                if (ClientSize.Width > 0)
                {
                    base.TileSize = new Size(ClientSize.Width, value);
                }
            }
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);

            if (ClientSize.Width > 0 && iTileHeight > 0)
            {
                base.TileSize = new Size(ClientSize.Width, iTileHeight);
            }
        }

        public new Size TileSize
        {
            get
            {
                return base.TileSize;
            }
            set { }
        }

        public new ImageList LargeImageList
        {
            get
            {
                return base.LargeImageList;
            }
        }

        public new ImageList SmallImageList
        {
            get
            {
                return base.SmallImageList;
            }
        }

        public override Color BackColor
        {
            set
            {
                base.BackColor = value;
                if (iBackBrush != null)
                {
                    iBackBrush.Dispose();
                }
                iBackBrush = new SolidBrush(BackColor);
            }
        }

        public Color ForeColorMuted
        {
            get
            {
                return iForeColorMuted;
            }
            set
            {
                iForeColorMuted = value;
            }
        }

        public override Color ForeColor
        {
            set
            {
                base.ForeColor = value;
                if (iForeBrush != null)
                {
                    iForeBrush.Dispose();
                }
                iForeBrush = new SolidBrush(ForeColor);
                ListViewApi.SetGroupHeaderColor(Handle, ForeColor);
            }
        }

        public Color ForeColorBright
        {
            get
            {
                return iForeColorBright;
            }
            set
            {
                iForeColorBright = value;
                if (iBrightForeBrush != null)
                {
                    iBrightForeBrush.Dispose();
                }
                iBrightForeBrush = new SolidBrush(ForeColorBright);
            }
        }

        public override Font Font
        {
            set
            {
                base.Font = value;
                iFont = value;
                if (iLargeIconSize.Width > 0 && iLargeIconSize.Height > 0)
                {
                    int height = iLargeIconSize.Height + iFont.Height;
                    if (height > 256)
                    {
                        height = 256;
                    }
                    LargeImageList.ImageSize = new Size(iLargeIconSize.Width, height);
                }
            }
        }

        public Color HighlightForeColour
        {
            get
            {
                return iHighlightForeColour;
            }
            set
            {
                iHighlightForeColour = value;
            }
        }

        public Color HighlightBackColour
        {
            get
            {
                return iHighlightBackColour;
            }
            set
            {
                iHighlightBackColour = value;
                if (iHighlightBackBrush != null)
                {
                    iHighlightBackBrush.Dispose();
                }
                iHighlightBackBrush = new SolidBrush(HighlightBackColour);
            }
        }

        public Brush BrushBackColor
        {
            get
            {
                return iBackBrush;
            }
        }

        public Brush BrushForeColor
        {
            get
            {
                return iForeBrush;
            }
        }

        public Brush BrushHighlightBackColor
        {
            get
            {
                return iHighlightBackBrush;
            }
        }

        public Brush BrushBrightForeColor
        {
            get
            {
                return iBrightForeBrush;
            }
        }

        public void UpdateGroupHeaderColour()
        {
            ListViewApi.SetGroupHeaderColor(Handle, ForeColor);
        }

        public void Redraw()
        {
            iTimerRedraw.Change(kRedrawModeration, Timeout.Infinite);
        }

        public void CreateDragCursor(DragDropEffects aEffect)
        {
            if (iCursorInfo.Effect != aEffect)
            {
                if (iCursorInfo.Cursor != null)
                {
                    iCursorInfo.Cursor.Dispose();
                    iCursorInfo.Cursor = null;
                    iCursorInfo.Effect = DragDropEffects.None;
                }
            }

            if (SelectedIndices.Count > 0 && iCursorInfo.Cursor == null)
            {
                ListViewKinsky.Item item = iCursorInfo.Item;

                Bitmap image = new Bitmap(item.ListView.Width + 32, item.Bounds.Size.Height + 32, PixelFormat.Format32bppArgb);

                Graphics g = Graphics.FromImage(image);
                item.Draw(g, true);
                switch (aEffect)
                {
                    case DragDropEffects.Copy:
                        kCursorCopy.Draw(g, new Rectangle(iCursorInfo.HotSpot.X, iCursorInfo.HotSpot.Y, kCursorCopy.Size.Width, kCursorCopy.Size.Height));
                        break;

                    case DragDropEffects.Move:
                        kCursorMove.Draw(g, new Rectangle(iCursorInfo.HotSpot.X, iCursorInfo.HotSpot.Y, kCursorMove.Size.Width, kCursorMove.Size.Height));
                        break;

                    default:
                        kCursorNone.Draw(g, new Rectangle(iCursorInfo.HotSpot.X, iCursorInfo.HotSpot.Y, kCursorNone.Size.Width, kCursorNone.Size.Height));
                        break;
                }
                g.Dispose();

                CursorApi.IconInfo iconInfo = new CursorApi.IconInfo();
                iconInfo.fIcon = false;
                iconInfo.xHotspot = iCursorInfo.HotSpot.X;
                iconInfo.yHotspot = iCursorInfo.HotSpot.Y;
                iconInfo.hbmColor = image.GetHbitmap();
                iconInfo.hbmMask = image.GetHbitmap();

                IntPtr pIconInfo = Marshal.AllocHGlobal(Marshal.SizeOf(iconInfo));
                Marshal.StructureToPtr(iconInfo, pIconInfo, true);
                IntPtr pIcon = CursorApi.CreateIconIndirect(pIconInfo);

                CursorApi.DestroyIcon(pIconInfo);
                CursorApi.DeleteObject(iconInfo.hbmMask);
                CursorApi.DeleteObject(iconInfo.hbmColor);

                iCursorInfo.Cursor = new Cursor(pIcon);

                iCursorInfo.Effect = aEffect;
            }
        }

        protected override void Dispose(bool disposing)
        {
            DrawItem -= EventListViewLibrary_DrawItem;
            DrawSubItem -= EventListViewLibrary_DrawSubItem;
            DrawColumnHeader -= EventListViewLibrary_DrawColumnHeader;

            base.Dispose(disposing);
        }

        protected virtual void EventListViewLibrary_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (e.Bounds.Width > 0 && e.Bounds.Height > 0)
            {
            }
            try
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis;

                if (base.View == System.Windows.Forms.View.LargeIcon || base.View == System.Windows.Forms.View.SmallIcon)
                {
                    int iconHeight = iLargeIconSize.Height;
                    int iconWidth = iLargeIconSize.Width;
                    int iconOffset = 3;
                    Item item = e.Item as Item;

                    int y = e.Bounds.Y + iconOffset + iconHeight;

                    int height = iFont.Height * 2;

                    int remainingHeight = e.Bounds.Bottom - y;
                    height = remainingHeight < height ? iFont.Height : height;

                    e.Graphics.FillRectangle(iBackBrush, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, height + iconOffset + iconHeight));

                    if (item != null)
                    {
                        Image albumArt = item.Icon;
                        if (albumArt != null)
                        {
                            if (e.Item.Selected)
                            {
                                albumArt = item.IconSelected;
                            }
                            try
                            {
                                SizeF imageSize = ImageSize(albumArt, iconWidth, iconHeight);

                                ImageAttributes ia = new ImageAttributes();
                                ColorMatrix cm = new ColorMatrix();
                                cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = 0.75f;
                                cm.Matrix33 = cm.Matrix44 = 1.0f;
                                ia.SetColorMatrix(cm);

                                if (e.Item.Selected)
                                {
                                    e.Graphics.DrawImage(albumArt, new RectangleF((int)(e.Bounds.Left + ((e.Bounds.Width - iconWidth) * 0.5f) + ((iconWidth - imageSize.Width) * 0.5f)),
                                        (int)(e.Bounds.Top + iconOffset + ((iconHeight - imageSize.Height) * 0.5f)), imageSize.Width, imageSize.Height));
                                }
                                else
                                {
                                    e.Graphics.DrawImage(albumArt, new Rectangle((int)(e.Bounds.Left + ((e.Bounds.Width - iconWidth) * 0.5f) + ((iconWidth - imageSize.Width) * 0.5f)),
                                        (int)(e.Bounds.Top + iconOffset + ((iconHeight - imageSize.Height) * 0.5f)), (int)imageSize.Width, (int)imageSize.Height),
                                        0, 0, albumArt.Width, albumArt.Height, GraphicsUnit.Pixel, ia);
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                Trace.WriteLine(Trace.kKinsky, "ListViewKinsky.EventListViewLibrary_DrawItem: " + ex.Message);
                            }
                        }
                    }

                    Color c = e.Item.Selected ? iHighlightForeColour : ForeColor;
                    c = e.Item.Checked ? Color.Green : c;
                    /*using (SolidBrush backBrush = new SolidBrush(ForeColor))
                    {
                        e.Graphics.FillRectangle(backBrush, new Rectangle(e.Bounds.X, y, e.Bounds.Width, height));
                    }*/

                    foreach (ListViewItem.ListViewSubItem i in e.Item.SubItems)
                    {
                        if (i.Name == "Title")
                        {
                            TextRenderer.DrawText(e.Graphics, i.Text, iFont, new Rectangle(e.Bounds.X, y, e.Bounds.Width, height), c, flags | TextFormatFlags.HorizontalCenter);
                            break;
                        }
                    }
                }
                else if (base.View == System.Windows.Forms.View.Tile)
                {
                    float scale = 1.0f;
                    int iconHeight = iLargeIconSize.Height;
                    if (iconHeight > e.Bounds.Height - 6)
                    {
                        scale = (iLargeIconSize.Height - 6) / (float)iconHeight;
                        iconHeight = (int)(iconHeight * scale);
                    }
                    int iconWidth = (int)(iLargeIconSize.Width * scale);
                    int iconOffsetX = 3;
                    int iconOffsetY = (int)(e.Bounds.Top + ((e.Bounds.Height - iconHeight) * 0.5f));
                    Item item = e.Item as Item;

                    e.Graphics.FillRectangle(iBackBrush, new Rectangle(e.Bounds.Left, iconOffsetY, e.Bounds.Width, iconHeight));

                    if (item != null)
                    {
                        Image albumArt = item.Icon;
                        if (e.Item.Selected)
                        {
                            albumArt = item.IconSelected;
                        }
                        if (albumArt != null)
                        {
                            try
                            {
                                SizeF imageSize = ImageSize(albumArt, iconWidth, iconHeight);

                                ImageAttributes ia = new ImageAttributes();
                                ColorMatrix cm = new ColorMatrix();
                                cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = 0.75f;
                                cm.Matrix33 = cm.Matrix44 = 1.0f;
                                ia.SetColorMatrix(cm);

                                if (e.Item.Selected)
                                {
                                    e.Graphics.DrawImage(albumArt, new Rectangle(e.Bounds.Left + iconOffsetX + (int)((iconWidth - imageSize.Width) * 0.5f), iconOffsetY, (int)imageSize.Width, (int)imageSize.Height));
                                }
                                else
                                {
                                    e.Graphics.DrawImage(albumArt, new Rectangle(e.Bounds.Left + iconOffsetX + (int)((iconWidth - imageSize.Width) * 0.5f), iconOffsetY, (int)imageSize.Width, (int)imageSize.Height),
                                        0, 0, albumArt.Width, albumArt.Height, GraphicsUnit.Pixel, ia);
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                Trace.WriteLine(Trace.kKinsky, "ListViewKinsky.EventListViewLibrary_DrawItem: " + ex.Message);
                            }
                        }
                    }

                    int x = e.Bounds.Left + iconOffsetX * 2 + iconWidth;
                    int y = iconOffsetY;
                    bool first = true;
                    foreach (ListViewItem.ListViewSubItem subItem in e.Item.SubItems)
                    {
                        if (!string.IsNullOrEmpty(subItem.Name))
                        {
                            if (y + iFont.Height < iconOffsetY + iconHeight)
                            {
                                if (!string.IsNullOrEmpty(subItem.Text) && subItem.Name != "Uri" && subItem.Name != "Type")
                                {
                                    Color c = !first ? iForeColorMuted : (e.Item.Selected ? iHighlightForeColour : ForeColor);
                                    c = e.Item.Checked ? Color.Green : c;
                                    TextRenderer.DrawText(e.Graphics, subItem.Text, iFont, new Rectangle(x, y, e.Bounds.Width - e.Bounds.Height, iFont.Height), c, flags);
                                    y += iFont.Height;
                                    first = false;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (ArgumentException) { }
        }

        protected virtual void EventListViewLibrary_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Rectangle rectangleBounds = new Rectangle();
            rectangleBounds = e.Bounds;

            if (e.ColumnIndex == 0)
            {
                rectangleBounds.X += iSmallIconSize.Width + 4;
                rectangleBounds.Width -= iSmallIconSize.Width;
            }

            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(iHighlightBackBrush, rectangleBounds);
            }
            else
            {
                e.Graphics.FillRectangle(iBackBrush, e.Bounds);
            }

            if (e.ColumnIndex == 0)
            {
                Item item = e.Item as Item;
                if (item != null && item.Icon != null)
                {
                    Image image = item.Icon;
                    e.Graphics.FillRectangle(iBackBrush, new Rectangle(new Point(e.Bounds.X + 4, (int)(e.Bounds.Top + ((e.Bounds.Height - iSmallIconSize.Height) * 0.5f))), iSmallIconSize));
                    
                    SizeF imageSize = ImageSize(image, iSmallIconSize.Width, iSmallIconSize.Height);

                    ImageAttributes ia = new ImageAttributes();
                    ColorMatrix cm = new ColorMatrix();
                    cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = 0.75f;
                    cm.Matrix33 = cm.Matrix44 = 1.0f;
                    ia.SetColorMatrix(cm);

                    if (e.Item.Selected)
                    {
                        e.Graphics.DrawImage(image,
                            new Rectangle(
                                (int)(e.Bounds.X + 4 + ((iSmallIconSize.Width - imageSize.Width) * 0.5f)),
                                (int)(e.Bounds.Top + ((e.Bounds.Height - iSmallIconSize.Height) * 0.5f) + ((iSmallIconSize.Height - imageSize.Height) * 0.5f)),
                                (int)imageSize.Width, (int)imageSize.Height));
                    }
                    else
                    {
                        e.Graphics.DrawImage(image,
                            new Rectangle(
                                (int)(e.Bounds.X + 4 + ((iSmallIconSize.Width - imageSize.Width) * 0.5f)),
                                (int)(e.Bounds.Top + ((e.Bounds.Height - iSmallIconSize.Height) * 0.5f) + ((iSmallIconSize.Height - imageSize.Height) * 0.5f)),
                                (int)imageSize.Width, (int)imageSize.Height),
                            0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
                    }
                }
            }

            TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

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
                rectangleBounds.Height = iFont.Height;
                rectangleBounds.Y = e.Bounds.Top + (int)((e.Bounds.Height - rectangleBounds.Height) * 0.5f);
                if (e.Item.Selected)
                {
                    TextRenderer.DrawText(e.Graphics, subItem.Text, iFont, rectangleBounds, iHighlightForeColour, flags);
                }
                else
                {
                    if (e.Header.Name != "Title")
                    {
                        TextRenderer.DrawText(e.Graphics, subItem.Text, iFont, rectangleBounds, ForeColorMuted, flags);
                    }
                    else
                    {
                        TextRenderer.DrawText(e.Graphics, subItem.Text, iFont, rectangleBounds, ForeColor, flags);
                    }
                }
            }
        }

        protected virtual void EventListViewLibrary_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            //e.DrawDefault = true;

            e.Graphics.FillRectangle(iBackBrush, e.Bounds);
            e.Graphics.FillRectangle(iForeBrush, new Rectangle(e.Bounds.Right - 1, e.Bounds.Top + 3, 1, e.Bounds.Height - 6));
            //e.Graphics.FillRectangle(iForeBrush, new Rectangle(e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Width, 1));

            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;
            switch (e.Header.TextAlign)
            {
                case HorizontalAlignment.Left:
                    flags |= TextFormatFlags.Left;
                    break;
                case HorizontalAlignment.Center:
                    flags |= TextFormatFlags.HorizontalCenter;
                    break;
                case HorizontalAlignment.Right:
                    flags |= TextFormatFlags.Right;
                    break;
            }
           
            Rectangle rect = e.Bounds;
            rect.Inflate(new Size(-2, 0));
            rect.Offset(2, 0);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, iFont, rect, ForeColorBright, flags);
        }

        private SizeF ImageSize(Image aImage, int aWidth, int aHeight)
        {
            SizeF size = new SizeF(aWidth, aHeight);
            if (aImage.Width > aImage.Height)
            {
                size.Height = aWidth * (aImage.Height / (float)aImage.Width);
            }
            else if (aImage.Height > aImage.Width)
            {
                size.Width = aHeight * (aImage.Width / (float)aImage.Height);
            }
            return size;
        }

        private void EventListViewLibrary_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedIndices.Count > 0)
            {
                ListViewKinsky.Item item = Items[SelectedIndices[0]] as Item;
                Point mousePosition = PointToClient(System.Windows.Forms.Control.MousePosition);

                if (iCursorInfo.Cursor != null)
                {
                    iCursorInfo.Cursor.Dispose();
                }
                iCursorInfo.Cursor = null;
                iCursorInfo.Effect = DragDropEffects.None;
                iCursorInfo.Item = item;
                iCursorInfo.HotSpot = new Point(mousePosition.X - item.Bounds.X, mousePosition.Y - item.Bounds.Y);
            }
        }

        private void EventListViewLibrary_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                CreateDragCursor(e.Effect);
                if (iCursorInfo.Cursor != null)
                {
                    e.UseDefaultCursors = false;
                    Cursor.Current = iCursorInfo.Cursor;
                }
            }
        }

        private void TimerRedrawElapsed(object sender)
        {
            Invalidate();
        }

        private readonly Cursor kCursorCopy = new Cursor(new MemoryStream(KinskyDesktop.Properties.Resources.Copy));
        private readonly Cursor kCursorMove = new Cursor(new MemoryStream(KinskyDesktop.Properties.Resources.Move));
        private readonly Cursor kCursorNone = new Cursor(new MemoryStream(KinskyDesktop.Properties.Resources.None));

        private const int kRedrawModeration = 10;
        private System.Threading.Timer iTimerRedraw;

        private CursorInfo iCursorInfo;

        protected Font iFont;
        protected Size iLargeIconSize;
        protected Size iSmallIconSize;
        protected int iTileHeight;
        protected Color iHighlightForeColour;
        protected Color iHighlightBackColour;
        protected Color iForeColorMuted;
        protected Color iForeColorBright;

        protected Brush iBackBrush;
        protected Brush iForeBrush;
        protected Brush iHighlightBackBrush;
        protected Brush iBrightForeBrush;
    }

    internal class ListViewApi
    {
        public const int LVN_FIRST = -100;
        public const int LVN_ITEMCHANGING = LVN_FIRST - 0;

        public const int WM_PAINT = 0xF;
        public const int WM_USER = 0x0400;
        public const int WM_NOTIFY = 0x004E;
        public const int WM_CONTEXTMENU = 0x007B;
        public const int OCM_BASE = WM_USER + 0x1c00;
        public const int OCM_NOTIFY = OCM_BASE + WM_NOTIFY;

        [StructLayout(LayoutKind.Sequential)]
        public struct NMHDR
        {
            public IntPtr hwndFrom;
            public int idFrom;
            public int code;
        }

        public const int LVM_FIRST = 4096;
        public const int LVM_SETGROUPMETRICS = (LVM_FIRST + 155);

        public const int LVGMF_NONE = 0;
        public const int LVGMF_BORDERSIZE = 1;
        public const int LVGMF_BORDERCOLOR = 2;
        public const int LVGMF_TEXTCOLOR = 4;

        [StructLayout(LayoutKind.Sequential)]
        public struct XYCOORDINATE
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LVGROUPMETRICS
        {
            public int cbSize;
            public int mask;
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int crLeft;
            public int crTop;
            public int crRight;
            public int crBottom;
            public int crHeader;
            public int crFooter;
        }

        [DllImport("User32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVGROUPMETRICS lParam);

        public static void SetGroupHeaderColor(IntPtr aHandle, Color aColor)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                LVGROUPMETRICS groupMetrics = new LVGROUPMETRICS();
                groupMetrics.cbSize = Marshal.SizeOf(typeof(LVGROUPMETRICS));
                // set the mask to LVGMF_TEXTCOLOR to enable the new header color to take effect
                groupMetrics.mask = LVGMF_TEXTCOLOR;
                groupMetrics.crHeader = Color.FromArgb(0, aColor.R, aColor.G, aColor.B).ToArgb();
                SendMessage(aHandle, LVM_SETGROUPMETRICS, 0, ref groupMetrics);
            }
        }
    }

    internal class CursorApi
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("User32.dll")]
        public static extern IntPtr CreateIconIndirect(IntPtr pIconInfo);

        [DllImport("User32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("GDI32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
} // Linn.Kinsky
