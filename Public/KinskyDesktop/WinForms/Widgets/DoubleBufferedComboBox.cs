using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System;

using Linn;

namespace KinskyDesktop.Widgets
{
    [System.ComponentModel.DesignerCategory("")]

    public class DoubleBufferedComboBox : ComboBox
    {
        public class ComboBoxItem
        {
            public ComboBoxItem(Image aImage, string aName, object aTag)
            {
                Image = aImage;
                Name = aName;
                Tag = aTag;
            }

            public override string ToString()
            {
                return Name;
            }

            public Image Image;
            public string Name;
            public object Tag;
        }

        public const int kIconWidth = 35;

        private ComboBoxItem iCurrent;

        private Color iHighlightForeColor;
        private Color iForeColorMuted;
        private SolidBrush iBrushBackColor;
        private SolidBrush iBrushHighlightBackColor;

        public DoubleBufferedComboBox()
        {
            DoubleBuffered = true;
            DrawMode = DrawMode.OwnerDrawFixed;

            iBrushBackColor = new SolidBrush(BackColor);
            iBrushHighlightBackColor = new SolidBrush(Color.White);
            iHighlightForeColor = Color.Black;
            iForeColorMuted = Color.Gray;
            
            DrawItem += EventComboBoxNoPaint_DrawItem;
            DropDown += EventComboBoxNoPaint_DropDown;
        }

        public ComboBoxItem Current
        {
            get
            {
                return iCurrent;
            }
            set
            {
                iCurrent = value;
                Invalidate();
            }
        }

        public Color HighlightForeColor
        {
            get
            {
                return iHighlightForeColor;
            }
            set
            {
                iHighlightForeColor = value;
            }
        }

        public Color HighlightBackColor
        {
            get
            {
                return iBrushHighlightBackColor.Color;
            }
            set
            {
                if (iBrushHighlightBackColor != null)
                {
                    iBrushHighlightBackColor.Dispose();
                }
                iBrushHighlightBackColor = new SolidBrush(value);
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

        public override Color BackColor
        {
            set
            {
                base.BackColor = value;
                if (iBrushBackColor != null)
                {
                    iBrushBackColor.Dispose();
                }
                iBrushBackColor = new SolidBrush(value);
            }
        }

        private void EventComboBoxNoPaint_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            //e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;            

            e.Graphics.FillRectangle(iBrushBackColor, e.Bounds);

            if (((e.State & DrawItemState.Selected) == DrawItemState.Selected) && (iCurrent != null))
            {
                if (combo.Name == "RoomSelection")
                {
                    e.Graphics.FillRectangle(iBrushHighlightBackColor, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
                }
                else
                {
                    e.Graphics.FillRectangle(iBrushHighlightBackColor, e.Bounds.Left + kIconWidth, e.Bounds.Top, e.Bounds.Width - kIconWidth, e.Bounds.Height);
                }
            }

            if (e.Index > -1)
            {
                string s = string.Empty;
                Image image = null;
                try
                {
                    if (e.Index < Items.Count)
                    {
                        object obj = Items[e.Index];
                        if (obj is DoubleBufferedComboBox.ComboBoxItem)
                        {
                            DoubleBufferedComboBox.ComboBoxItem item = obj as DoubleBufferedComboBox.ComboBoxItem;
                            s = item.ToString();

                            if (item.Image != null)
                            {
                                image = item.Image;
                                float width = (float)kIconWidth;
                                float height = (width / item.Image.Width) * item.Image.Height;
                                if (height > e.Bounds.Height)
                                {
                                    width = (int)((e.Bounds.Height / (float)height) * width);
                                }
                                e.Graphics.DrawImage(item.Image, e.Bounds.Left + ((kIconWidth - width) * 0.5f), e.Bounds.Top, width, e.Bounds.Height);
                            }
                        }

                        TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                        Rectangle rect = new Rectangle((int)(e.Bounds.Left + kIconWidth + 5), e.Bounds.Top, (int)(e.Bounds.Width - kIconWidth - 5), e.Bounds.Height);
                        if (image == null)
                        {
                            rect.X = rect.X - kIconWidth + e.Bounds.Height;
                            rect.Width += kIconWidth;
                        }
                        //e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.White)), rect);
                        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                        {
                            TextRenderer.DrawText(e.Graphics, s, Font, rect, iHighlightForeColor, flags);
                        }
                        else
                        {
                            TextRenderer.DrawText(e.Graphics, s, Font, rect, ForeColor, flags);
                        }
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("ERROR: e.Index= " + e.Index + ", Items.Count=" + Items.Count);
                }
            }
            else
            {
                if (iCurrent == null)
                {
                    string s = "";
                    if (combo.Name == "RoomSelection")
                    {
                        if (Items.Count == 0)
                        {
                            s = "No rooms found";
                        }
                        else
                        {
                            s = "Select room";
                        }
                    }
                    if (combo.Name == "SourceSelection")
                    {
                        if (Items.Count == 0)
                        {
                            s = "No sources found";
                        }
                        else
                        {
                            s = "Select source";
                        }
                    }

                    using (Font font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold | FontStyle.Italic))
                    {
                        TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;
                        Rectangle rect = e.Bounds;
                        rect.Inflate(new Size(-(e.Bounds.Height + 5), 0));
                        TextRenderer.DrawText(e.Graphics, s, font, rect, iForeColorMuted, flags);
                    }
                }
                else
                {
                    string s = iCurrent.ToString();

                    if (iCurrent.Image != null)
                    {
                        float width = (float)kIconWidth;
                        float height = (width / iCurrent.Image.Width) * iCurrent.Image.Height;
                        if (height > e.Bounds.Height)
                        {
                            width = (int)((e.Bounds.Height / (float)height) * width);
                        }
                        e.Graphics.DrawImage(iCurrent.Image, e.Bounds.Left + ((kIconWidth - width) * 0.5f), e.Bounds.Top, width, e.Bounds.Height);
                    }

                    TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                    Rectangle rect = new Rectangle((int)(e.Bounds.Left + kIconWidth + 5), e.Bounds.Top, (int)(e.Bounds.Width - kIconWidth - 5), e.Bounds.Height);
                    if (iCurrent.Image == null)
                    {
                        rect.X = rect.X - kIconWidth + e.Bounds.Height;
                        rect.Width += kIconWidth;
                    }
                    
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    {
                        TextRenderer.DrawText(e.Graphics, s, Font, rect, iHighlightForeColor, flags);
                    }
                    else
                    {
                        TextRenderer.DrawText(e.Graphics, s, Font, rect, ForeColor, flags);
                    }
                }
            }
        }

        private void EventComboBoxNoPaint_DropDown(object sender, System.EventArgs e)
        {
            int width = Width;
            int vertScrollBarWidth = (Items.Count > MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;

            using (Graphics g = CreateGraphics())
            {
                foreach (object s in Items)
                {
                    int adjWidth = (int)g.MeasureString(s.ToString(), Font).Width + vertScrollBarWidth + kIconWidth;
                    if (width < adjWidth)
                    {
                        width = adjWidth;
                    }
                }
            }

            DropDownWidth = width;
        }
    }
}
