using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing;
using System;

namespace KinskyDesktop.Widgets
{
    public abstract class Kontrol : UserControl
    {
        protected Kontrol()
        {
            iOuterRingEnabled = true;
            iInnerRingEnabled = true;

            iOuterCircleRadius = 47.0f;
            iInnerCircleRadius = 34.0f;

            iHitOuterCircleRadius = 47.0f;
            iHitInnerCircleRadius = 34.0f;

            iPenArc = new Pen(Color.FromArgb(128, Color.Red), 4);
        }

        public virtual float OuterCircleRadius
        {
            get
            {
                return iOuterCircleRadius;
            }
            set
            {
                iOuterCircleRadius = value;
            }
        }

        public virtual float InnerCircleRadius
        {
            get
            {
                return iInnerCircleRadius;
            }
            set
            {
                iInnerCircleRadius = value;
            }
        }

        public float HitOuterCircleRadius
        {
            get
            {
                return iHitOuterCircleRadius;
            }
            set
            {
                iHitOuterCircleRadius = value;
            }
        }

        public float HitInnerCircleRadius
        {
            get
            {
                return iHitInnerCircleRadius;
            }
            set
            {
                iHitInnerCircleRadius = value;
            }
        }

        public float MaxValue
        {
            get
            {
                return iMaxValue;
            }
            set
            {
                iMaxValue = value;
                Invalidate();
            }
        }

        public float Value
        {
            get
            {
                return iValue;
            }
            set
            {
                iValue = value;
                Invalidate();
            }
        }

        public Color ArcColour
        {
            get
            {
                return iPenArc.Color;
            }
            set
            {
                float w = iPenArc.Width;
                iPenArc.Dispose();
                iPenArc = new Pen(value, w);
                Invalidate();
            }
        }

        public float ArcPenWidth
        {
            get
            {
                return iPenArc.Width;
            }
            set
            {
                Color c = iPenArc.Color;
                iPenArc.Dispose();
                iPenArc = new Pen(c, value);
                Invalidate();
            }
        }

        public bool OuterRingEnabled
        {
            get
            {
                return iOuterRingEnabled;
            }
            set
            {
                iOuterRingEnabled = value;
            }
        }

        public bool InnerRingEnabled
        {
            get
            {
                return iInnerRingEnabled;
            }
            set
            {
                iInnerRingEnabled = value;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect;
            if (iMaxValue > 0)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                rect = new Rectangle((int)((Width * 0.5f) - iInnerCircleRadius), (int)((Height * 0.5f) - iInnerCircleRadius), (int)(iInnerCircleRadius * 2) + 1, (int)(iInnerCircleRadius * 2) + 1);
                float d = 360 * (iValue / iMaxValue);

                if (Math.Abs(d) < 1.0f)
                {
                    d = 0;
                }

                e.Graphics.DrawArc(iPenArc, rect, 90, d);
            }

            int offset = 3;
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            rect = new Rectangle((int)((Width * 0.5f) - iInnerCircleRadius) + offset, (int)((Height * 0.5f) - iInnerCircleRadius), (int)(iInnerCircleRadius * 2) - offset, (int)(iInnerCircleRadius * 2));
            //e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.White)), rect);
            TextRenderer.DrawText(e.Graphics, Text, Font, rect, ForeColor, flags);

            base.OnPaint(e);
        }

        protected bool iInnerRingEnabled;
        protected bool iOuterRingEnabled;

        protected float iOuterCircleRadius;
        protected float iInnerCircleRadius;

        protected float iHitOuterCircleRadius;
        protected float iHitInnerCircleRadius;

        protected float iMaxValue;
        protected float iValue;

        protected Pen iPenArc;
    }
}
