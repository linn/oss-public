using System;
using System.Drawing;
using System.Windows.Forms;

namespace KinskyDesktop
{
    class DoubleBufferedButton : UserControl
    {
        public DoubleBufferedButton()
        {
            DoubleBuffered = true;

            iPenForeColorMuted = new Pen(ForeColor);
            iBrushForeColor = new SolidBrush(ForeColor);
            SetStringFormat(TextAlign);
        }

        public Color ForeColorMuted
        {
            get
            {
                return iPenForeColorMuted.Color;
            }
            set
            {
                if (iPenForeColorMuted != null)
                {
                    iPenForeColorMuted.Dispose();
                }
                iPenForeColorMuted = new Pen(value);
            }
        }

        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
                if (iBrushForeColor != null)
                {
                    iBrushForeColor.Dispose();
                }
                iBrushForeColor = new SolidBrush(value);
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        public ContentAlignment TextAlign
        {
            get
            {
                return iTextAlign;
            }
            set
            {
                iTextAlign = value;
                SetStringFormat(value);
            }
        }

        public TextImageRelation TextImageRelation
        {
            get
            {
                return iTextImageRelation;
            }
            set
            {
                iTextImageRelation = value;
            }
        }

        public Image Image
        {
            get
            {
                return iImage;
            }
            set
            {
                iImage = value;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            int left = 0;
            if (iStringFormat.Alignment == StringAlignment.Far)
            {
                left = ClientRectangle.Right;
            }
            if (Image != null)
            {
                float height = Height - 2;
                float width = (height / Image.Height) * Image.Width;
                if (TextImageRelation == TextImageRelation.TextBeforeImage)
                {
                    left = (int)(ClientRectangle.Right - width);
                }

                if (iStringFormat.Alignment == StringAlignment.Near)
                {
                    left = left + kSpacer;
                }
                else
                {
                    left = left - kSpacer;
                }

                e.Graphics.DrawImage(Image, left, 1, (int)width, (int)height);

                if (TextImageRelation == TextImageRelation.TextBeforeImage)
                {
                    left -= kSpacer;
                }
                else
                {
                    left += (int)width;
                }
            }

            if (iStringFormat.Alignment == StringAlignment.Near)
            {
                e.Graphics.DrawString(Text, Font, iBrushForeColor, new Rectangle(left + kSpacer, 0, ClientRectangle.Width - left - kSpacer, ClientRectangle.Height), iStringFormat);
                e.Graphics.DrawLine(iPenForeColorMuted, kSpacer, ClientRectangle.Height - 1, ClientRectangle.Width, ClientRectangle.Height - 1);
            }
            else
            {
                e.Graphics.DrawString(Text, Font, iBrushForeColor, new Rectangle(0, 0, left, ClientRectangle.Height), iStringFormat);
                e.Graphics.DrawLine(iPenForeColorMuted, 0, ClientRectangle.Height - 1, ClientRectangle.Width - kSpacer, ClientRectangle.Height - 1);
            }
        }

        private void SetStringFormat(ContentAlignment aAlignment)
        {
            if (iStringFormat != null)
            {
                iStringFormat.Dispose();
            }
            iStringFormat = new StringFormat();
            iStringFormat.Trimming = StringTrimming.EllipsisCharacter;
            switch (aAlignment)
            {
                case ContentAlignment.BottomCenter:
                    iStringFormat.LineAlignment = StringAlignment.Far;
                    iStringFormat.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomLeft:
                    iStringFormat.LineAlignment = StringAlignment.Far;
                    iStringFormat.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.BottomRight:
                    iStringFormat.LineAlignment = StringAlignment.Far;
                    iStringFormat.Alignment = StringAlignment.Far;
                    break;
                case ContentAlignment.MiddleCenter:
                    iStringFormat.LineAlignment = StringAlignment.Center;
                    iStringFormat.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleLeft:
                    iStringFormat.LineAlignment = StringAlignment.Center;
                    iStringFormat.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.MiddleRight:
                    iStringFormat.LineAlignment = StringAlignment.Center;
                    iStringFormat.Alignment = StringAlignment.Far;
                    break;
                case ContentAlignment.TopCenter:
                    iStringFormat.LineAlignment = StringAlignment.Near;
                    iStringFormat.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.TopLeft:
                    iStringFormat.LineAlignment = StringAlignment.Near;
                    iStringFormat.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopRight:
                    iStringFormat.LineAlignment = StringAlignment.Near;
                    iStringFormat.Alignment = StringAlignment.Far;
                    break;
            }
        }

        private const int kSpacer = 5;

        private Image iImage;
        private ContentAlignment iTextAlign;
        private TextImageRelation iTextImageRelation;
        private StringFormat iStringFormat;
        private Brush iBrushForeColor;
        private Pen iPenForeColorMuted;
    }
}