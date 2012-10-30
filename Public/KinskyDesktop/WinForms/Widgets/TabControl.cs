using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KinskyDesktop.Widgets
{
    public partial class TabControl : UserControl
    {
        private List<TabPage> iTabPages;

        private int iSelectedTabPageIndex;
        private TabPage iMouseOverPage;

        private Color iHighlightColour;
        private Color iForeColourBright;
        private Color iForeColourMuted;

        private Pen iPenForeColourMuted;
        private Brush iBrushForeColour;
        private Brush iBrushHighlightColour;
        private Brush iBrushForeColourBright;
        private Brush iBrushForeColourMuted;

        private const int kTopOffset = 4;
        private const int kBottomOffset = 5;

        public TabControl()
        {
            iTabPages = new List<TabPage>();

            iSelectedTabPageIndex = -1;

            InitializeComponent();

            DoubleBuffered = true;

            iHighlightColour = ForeColor;
            iForeColourBright = ForeColor;

            iPenForeColourMuted = new Pen(ForeColor);
            iBrushForeColour = new SolidBrush(ForeColor);
            iBrushHighlightColour = new SolidBrush(iHighlightColour);
            iBrushForeColourBright = new SolidBrush(iForeColourBright);
        }

        public void InsertTabPage(uint aIndex, string aLabel)
        {
            iTabPages.Insert((int)aIndex, new TabPage(aLabel));
        }

        public void RemoveTabPage(uint aIndex)
        {
            iTabPages.RemoveAt((int)aIndex);
            if (aIndex == iSelectedTabPageIndex)
            {
                iSelectedTabPageIndex = -1;

                if (EventSelectedIndexChanged != null)
                {
                    EventSelectedIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        public int SelectedTabPageIndex
        {
            get
            {
                return iSelectedTabPageIndex;
            }
            set
            {
                if (value != iSelectedTabPageIndex)
                {
                    iSelectedTabPageIndex = value;
                    if (EventSelectedIndexChanged != null)
                    {
                        EventSelectedIndexChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public List<TabPage> TabPages
        {
            get
            {
                return iTabPages;
            }
        }

        public override Color ForeColor
        {
            set
            {
                base.ForeColor = value;
                if (iBrushForeColour != null)
                {
                    iBrushForeColour.Dispose();
                }
                iBrushForeColour = new SolidBrush(ForeColor);
            }
        }

        public Color HighlightColour
        {
            get
            {
                return iHighlightColour;
            }
            set
            {
                if (iBrushHighlightColour != null)
                {
                    iBrushHighlightColour.Dispose();
                }
                iBrushHighlightColour = new SolidBrush(value);
                iHighlightColour = value;
            }
        }

        public Color ForeColourMuted
        {
            get
            {
                return iForeColourMuted;
            }
            set
            {
                if (iPenForeColourMuted != null)
                {
                    iPenForeColourMuted.Dispose();
                }
                iPenForeColourMuted = new Pen(value);
                if (iBrushForeColourMuted != null)
                {
                    iBrushForeColourMuted.Dispose();
                }
                iBrushForeColourMuted = new SolidBrush(value);
                iForeColourMuted = value;
            }
        }

        public Color ForeColourBright
        {
            get
            {
                return iForeColourBright;
            }
            set
            {
                if (iBrushForeColourBright != null)
                {
                    iBrushForeColourBright.Dispose();
                }
                iBrushForeColourBright = new SolidBrush(value);
                iForeColourBright = value;
            }
        }

        public void SetTabLabel(int aIndex, string aText)
        {
            iTabPages[aIndex].Label = aText;
            Invalidate();
        }

        public event EventHandler<EventArgs> EventSelectedIndexChanged;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (iTabPages.Count > 0)
            {
                e.Graphics.DrawLine(iPenForeColourMuted, 4, 0, ClientSize.Width, 0);
            }

            float tabHeight = (ClientSize.Height / (float)iTabPages.Count);
            for (int i = 0; i < iTabPages.Count; ++i)
            {
                TabPage page = iTabPages[i];

                Rectangle rect = new Rectangle(0, (int)(i * tabHeight), ClientSize.Width - 1, (int)tabHeight);

                int topOffset = (i == 0) ? 0 : kTopOffset;
                int middleOffset = (i == iTabPages.Count - 1) ? kBottomOffset : 0;
                int bottomOffset = (i == iTabPages.Count - 1) ? 0 : kBottomOffset;

                if (i != iSelectedTabPageIndex + 1 && i != 0)
                {
                    if (i == iSelectedTabPageIndex)
                    {
                        e.Graphics.DrawLine(iPenForeColourMuted, rect.Right, rect.Top - (2 * topOffset), rect.Right - 3, rect.Top - topOffset);
                        e.Graphics.DrawLine(iPenForeColourMuted, rect.Right - 3, rect.Top - topOffset, rect.Left + 3, rect.Top - topOffset);
                    }
                    else
                    {
                        e.Graphics.DrawLine(iPenForeColourMuted, rect.Right, rect.Top - topOffset, rect.Left + 3, rect.Top - topOffset);
                    }
                }
                e.Graphics.DrawLine(iPenForeColourMuted, rect.Left + 3, rect.Top -  topOffset, rect.Left, rect.Top + 3 - topOffset);
                e.Graphics.DrawLine(iPenForeColourMuted, rect.Left, rect.Top + 3 - topOffset, rect.Left, rect.Bottom - 15 - middleOffset);
                e.Graphics.DrawLine(iPenForeColourMuted, rect.Left, rect.Bottom - 15 - middleOffset, rect.Left + 2, rect.Bottom - 8 - middleOffset);
                e.Graphics.DrawLine(iPenForeColourMuted, rect.Left + 2, rect.Bottom - 8 - middleOffset, rect.Left + 4, rect.Bottom - 6 - middleOffset);
                if (i == iSelectedTabPageIndex || i == iTabPages.Count - 1)
                {
                    e.Graphics.DrawLine(iPenForeColourMuted, rect.Left + 4, rect.Bottom - 6 - middleOffset, rect.Right, rect.Bottom + bottomOffset);
                }
                if (i != iSelectedTabPageIndex)
                {
                    if (i == iSelectedTabPageIndex + 1)
                    {
                        e.Graphics.DrawLine(iPenForeColourMuted, rect.Right, rect.Top + kBottomOffset, rect.Right, rect.Bottom);
                    }
                    else
                    {
                        if (i == iTabPages.Count - 1)
                        {
                            e.Graphics.DrawLine(iPenForeColourMuted, rect.Right, rect.Top - 2 * kTopOffset, rect.Right, rect.Bottom);
                        }
                        else
                        {
                            e.Graphics.DrawLine(iPenForeColourMuted, rect.Right, rect.Top - 2 * kTopOffset, rect.Right, rect.Bottom - 2 * kTopOffset);
                        }
                    }
                }

                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.DirectionVertical;

                if (iMouseOverPage == page)
                {
                    e.Graphics.DrawString(page.Label, Font, iBrushHighlightColour, rect, format);
                }
                else
                {
                    if (iSelectedTabPageIndex == i)
                    {
                        e.Graphics.DrawString(page.Label, Font, iBrushForeColourBright, rect, format);
                    }
                    else
                    {
                        e.Graphics.DrawString(page.Label, Font, iBrushForeColour, rect, format);
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int index = PageIndex(e.Y);
            if (index >= 0 && index < iTabPages.Count)
            {
                if (iMouseOverPage != iTabPages[index])
                {
                    iMouseOverPage = iTabPages[index];
                    Invalidate();
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            int index = PageIndex(e.Y);

            if (iSelectedTabPageIndex != index)
            {
                iSelectedTabPageIndex = index;
                if (EventSelectedIndexChanged != null)
                {
                    EventSelectedIndexChanged(this, EventArgs.Empty);
                }

                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            iMouseOverPage = null;
            Invalidate();
        }

        private int PageIndex(int aY)
        {
            int tabHeight = (int)(ClientSize.Height / (float)iTabPages.Count);
            int index = (int)(aY / (float)tabHeight);
            if (index == iTabPages.Count)
            {
                index = iTabPages.Count - 1;
            }
            return index;
        }
    }

    public class TabPage
    {
        public TabPage(string aLabel)
        {
            iLabel = aLabel;
        }

        public string Label
        {
            get
            {
                return iLabel;
            }
            set
            {
                iLabel = value;
            }
        }

        private string iLabel;
    }
}
