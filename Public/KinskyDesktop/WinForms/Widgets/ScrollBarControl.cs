using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Threading;

using Linn;

namespace KinskyDesktop.Widgets
{
    public partial class ScrollBarControl : UserControl
    {
        private enum EBlendState
        {
            eIn,
            eOut,
            eUnknown
        }

        private const int kBlendTime = 100; // in ms
        private const int kUpdateRate = 20;

        private float iAlpha;
        private EBlendState iBlendState;

        private Ticker iTicker;
        private System.Threading.Timer iTimer;

        private const int kAutoRepeatTime = 100;
        private System.Threading.Timer iTimerAutoRepeat;

        protected Image iImageChannel;
        protected Image iImageUpArrow;
        protected Image iImageThumbTop;
        protected Image iImageThumbTopSpan;
        protected Image iImageThumb;
        protected Image iImageThumbBottomSpan;
        protected Image iImageThumbBottom;
        protected Image iImageDownArrow;

        protected Image iImageUpArrowMouse;
        protected Image iImageThumbMouse;
        protected Image iImageDownArrowMouse;

        protected Image iImageUpArrowTouch;
        protected Image iImageThumbTouch;
        protected Image iImageDownArrowTouch;

        protected int iLargeChange;
        protected int iSmallChange;

        protected int iMinimum;
        protected int iMaximum;
        protected int iValue;
        
        protected bool iMouseOverShow;

        private const int kMinimumThumbHeight = 56;

        private float iThumbBarHeight;
        private int iThumbTop;
        private bool iMouseDown;
        private bool iMouseDownUpArrow;
        private bool iMouseDownDownArrow;
        private int iMouseDownY;
        private bool iMouseOver;

        public EventHandler<EventArgs> EventScroll;
        public EventHandler<EventArgs> EventValueChanged;

        public ScrollBarControl()
        {
            iAlpha = 0.0f;
            iBlendState = EBlendState.eUnknown;
            iTicker = new Ticker();
            iTimer = new System.Threading.Timer(TimerElapsed, null, Timeout.Infinite, Timeout.Infinite);

            iTimerAutoRepeat = new System.Threading.Timer(TimerAutoRepeatElapsed, null, Timeout.Infinite, Timeout.Infinite);

            InitializeComponent();

            iLargeChange = 10;
            iSmallChange = 1;
            iMaximum = 100;
        }

        public Image ImageChannel
        {
            get
            {
                return iImageChannel;
            }
            set
            {
                iImageChannel = value;
            }
        }

        public Image ImageUpArrow
        {
            get
            {
                return iImageUpArrow;
            }
            set
            {
                iImageUpArrow = value;
            }
        }

        public Image ImageThumbTop
        {
            get
            {
                return iImageThumbTop;
            }
            set
            {
                iImageThumbTop = value;
            }
        }

        public Image ImageThumbTopSpan
        {
            get
            {
                return iImageThumbTopSpan;
            }
            set
            {
                iImageThumbTopSpan = value;
            }
        }

        public Image ImageThumb
        {
            get
            {
                return iImageThumb;
            }
            set
            {
                iImageThumb = value;
            }
        }

        public Image ImageThumbBottomSpan
        {
            get
            {
                return iImageThumbBottomSpan;
            }
            set
            {
                iImageThumbBottomSpan = value;
            }
        }

        public Image ImageThumbBottom
        {
            get
            {
                return iImageThumbBottom;
            }
            set
            {
                iImageThumbBottom = value;
            }
        }

        public Image ImageDownArrow
        {
            get
            {
                return iImageDownArrow;
            }
            set
            {
                iImageDownArrow = value;
            }
        }

        public Image ImageUpArrowMouse
        {
            get
            {
                return iImageUpArrowMouse;
            }
            set
            {
                iImageUpArrowMouse = value;
            }
        }

        public Image ImageThumbMouse
        {
            get
            {
                return iImageThumbMouse;
            }
            set
            {
                iImageThumbMouse = value;
            }
        }

        public Image ImageDownArrowMouse
        {
            get
            {
                return iImageDownArrowMouse;
            }
            set
            {
                iImageDownArrowMouse = value;
            }
        }

        public Image ImageUpArrowTouch
        {
            get
            {
                return iImageUpArrowTouch;
            }
            set
            {
                iImageUpArrowTouch = value;
            }
        }

        public Image ImageThumbTouch
        {
            get
            {
                return iImageThumbTouch;
            }
            set
            {
                iImageThumbTouch = value;
            }
        }

        public Image ImageDownArrowTouch
        {
            get
            {
                return iImageDownArrowTouch;
            }
            set
            {
                iImageDownArrowTouch = value;
            }
        }

        //[EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Whether to hide everything apart from the thumb bar when mouse is not over scroll bar")]
        public bool MouseOverShow
        {
            get
            {
                return iMouseOverShow;
            }
            set
            {
                iMouseOverShow = value;
                Invalidate();
            }
        }

        //[EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(10), Category("Behavior"), Description("The amount by which the scroll box changes when the user clicks in the scroll bar or presses the PAGE UP or PAGE DOWN keys")]
        public int LargeChange
        {
            get
            {
                return iLargeChange;
            }
            set
            {
                iLargeChange = value;
            }
        }

        //[EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(1), Category("Behavior"), Description("The amount by which the scroll box changes when the user clicks a scroll arrow or presses an arrow key")]
        public int SmallChange
        {
            get
            {
                return iSmallChange;
            }
            set
            {
                iSmallChange = value;
            }
        }

        //[EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(0), Category("Behavior"), Description("The lower limit of the scrollable range")]
        public int Minimum
        {
            get
            {
                return iMinimum;
            }
            set
            {
                if (value != iMinimum)
                {
                    iMinimum = value;
                    if (iMinimum > iMaximum)
                    {
                        iMaximum = iMinimum;
                    }
                    if (iValue < iMinimum)
                    {
                        Value = value;
                    }
                    else
                    {
                        SetThumbPosition();
                    }
                }
            }
        }

        //[EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(100), Category("Behavior"), Description("The upper limit of the scrollable range")]
        public int Maximum
        {
            get
            {
                return iMaximum;
            }
            set
            {
                if (value != iMaximum)
                {
                    iMaximum = value;
                    if (iMaximum < iMinimum)
                    {
                        iMinimum = iMaximum;
                    }
                    if (iValue > iMaximum)
                    {
                        Value = value;
                    }
                    else
                    {
                        SetThumbPosition();
                    }
                }
            }
        }

        //[EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(0), Category("Behavior"), Description("The value that the scroll box represents")]
        public int Value
        {
            get
            {
                return iValue;
            }
            set
            {
                if (value != iValue)
                {
                    iValue = value;
                    if (iValue < iMinimum)
                    {
                        iValue = iMinimum;
                    }
                    else if (iValue > iMaximum)
                    {
                        iValue = iMaximum;
                    }

                    SetThumbPosition();

                    Invalidate();

                    if (EventValueChanged != null)
                    {
                        EventValueChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

#if !PocketPC
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
                if (base.AutoSize)
                {
                    if (iImageUpArrow != null)
                    {
                        Width = iImageUpArrow.Width;
                    }
                    else if (iImageDownArrow != null)
                    {
                        Width = iImageDownArrow.Width;
                    }
                    else if (iImageThumb != null)
                    {
                        Width = iImageThumb.Width;
                    }
                }
            }
        }
#endif

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            if (iImageUpArrow != null)
            {
                if ((iMouseOver && iMouseOverShow) || !iMouseOverShow)
                {
                    if (iMouseDownUpArrow)
                    {
                        e.Graphics.DrawImage(iImageUpArrowTouch, new Rectangle(0, 0, Width, iImageUpArrowTouch.Height),
                            new Rectangle(0, 0, iImageDownArrowTouch.Width, iImageDownArrowTouch.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        e.Graphics.DrawImage(iImageUpArrow, new Rectangle(0, 0, Width, iImageUpArrow.Height),
                            new Rectangle(0, 0, iImageUpArrow.Width, iImageUpArrow.Height), GraphicsUnit.Pixel);
                    }
                }
            }

            if (iImageChannel != null)
            {
                if ((iMouseOver && iMouseOverShow) || !iMouseOverShow)
                {
                    e.Graphics.DrawImage(iImageChannel, new Rectangle(0, iImageUpArrow.Height, Width, (Height - iImageDownArrow.Height)),
                        new Rectangle(0, 0, iImageChannel.Width, iImageChannel.Height), GraphicsUnit.Pixel);
                }
            }

            if (iImageDownArrow != null)
            {
                if ((iMouseOver && iMouseOverShow) || !iMouseOverShow)
                {
                    if (iMouseDownDownArrow)
                    {
                        e.Graphics.DrawImage(iImageDownArrowTouch, new Rectangle(0, (Height - iImageDownArrowTouch.Height), Width, iImageDownArrowTouch.Height),
                            new Rectangle(0, 0, iImageDownArrowTouch.Width, iImageDownArrowTouch.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        e.Graphics.DrawImage(iImageDownArrow, new Rectangle(0, (Height - iImageDownArrow.Height), Width, iImageDownArrow.Height),
                            new Rectangle(0, 0, iImageDownArrow.Width, iImageDownArrow.Height), GraphicsUnit.Pixel);
                    }
                }
            }

            int channelHeight = ChannelHeight();
            float thumbHeight = ThumbHeight(channelHeight);

            if (iImageThumb != null)
            {
                ImageAttributes ia1 = new ImageAttributes();
                ImageAttributes ia2 = new ImageAttributes();

#if !PocketPC
                ColorMatrix mx = new ColorMatrix();
                mx.Matrix11 = mx.Matrix22 = mx.Matrix44 = 1.0f;
                mx.Matrix33 = 1.0f - iAlpha;

                ia1.SetColorMatrix(mx);

                mx.Matrix33 = iAlpha;

                ia2.SetColorMatrix(mx);
#endif

                int offset = iImageThumb.Height;
                if (iImageThumbTop != null)
                {
                    offset += iImageThumbTop.Height;
                }
                if (iImageThumbBottom != null)
                {
                    offset += iImageThumbBottom.Height;
                }
                float spanHeight = (thumbHeight - offset) * 0.5f;

                float y = iThumbTop;
                if (iImageUpArrow != null)
                {
                    y += iImageUpArrow.Height;
                }

                if (iImageThumbTop != null)
                {
                    e.Graphics.DrawImage(iImageThumbTop, new Rectangle(0, (int)y, Width, iImageThumbTop.Height),
                        new Rectangle(0, 0, iImageThumbTop.Width, iImageThumbTop.Height), GraphicsUnit.Pixel);
                    y += iImageThumbTop.Height;
                }

                if (iImageThumbTopSpan != null)
                {
                    e.Graphics.DrawImage(iImageThumbTopSpan, new Rectangle(0, (int)y, Width, (int)(spanHeight * 2)),
                        new Rectangle(0, 0, iImageThumbTopSpan.Width, iImageThumbTopSpan.Height), GraphicsUnit.Pixel);
                    y += (int)spanHeight;
                }

                if (iImageThumb != null)
                {
                    if (iMouseOver)
                    {
                        if (iMouseDown)
                        {
                            e.Graphics.DrawImage(iImageThumbTouch, new Rectangle(0, (int)y, Width, iImageThumb.Height),
                                new Rectangle(0, 0, iImageThumbTouch.Width, iImageThumbTouch.Height), GraphicsUnit.Pixel);
                        }
                        else
                        {
                            e.Graphics.DrawImage(iImageThumb, new Rectangle(0, (int)y, Width, iImageThumb.Height),
                                0, 0, iImageThumb.Width, iImageThumb.Height, GraphicsUnit.Pixel, ia1);

                            if (iImageThumbMouse != null && iImageThumb != null)
                            {
                                e.Graphics.DrawImage(iImageThumbMouse, new Rectangle(0, (int)y, Width, iImageThumb.Height),
                                    0, 0, iImageThumb.Width, iImageThumb.Height, GraphicsUnit.Pixel, ia2);
                            }
                        }
                    }
                    else
                    {
                        e.Graphics.DrawImage(iImageThumb, new Rectangle(0, (int)y, Width, iImageThumb.Height),
                            new Rectangle(0, 0, iImageThumb.Width, iImageThumb.Height), GraphicsUnit.Pixel);
                    }

                    y += iImageThumb.Height;
                }

                if (iImageThumbBottomSpan != null)
                {
                    e.Graphics.DrawImage(iImageThumbBottomSpan, new Rectangle(0, (int)y, Width, (int)(spanHeight * 2)),
                        new Rectangle(0, 0, iImageThumbBottomSpan.Width, iImageThumbBottomSpan.Height), GraphicsUnit.Pixel);
                    y += (int)spanHeight;
                }

                iThumbBarHeight = y - iThumbTop;
                if (iImageUpArrow != null)
                {
                    iThumbBarHeight -= iImageUpArrow.Height;
                }

                if (iImageThumbBottom != null)
                {
                    e.Graphics.DrawImage(iImageThumbBottom, new Rectangle(0, (int)y, Width, iImageThumbBottom.Height),
                        new Rectangle(0, 0, iImageThumbBottom.Width, iImageThumbBottom.Height), GraphicsUnit.Pixel);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Point p = PointToClient(MousePosition);
            int channelHeight = ChannelHeight();
            float thumbHeight = ThumbHeight(channelHeight);

            int upArrowHeight = (iImageUpArrow != null) ? iImageUpArrow.Height : 0;
            int downArrowHeight = (iImageDownArrow != null) ? iImageDownArrow.Height : 0;

            Rectangle thumbRect = new Rectangle(0, iThumbTop + upArrowHeight, Width, (int)thumbHeight);
            if (e.Button == MouseButtons.Left && thumbRect.Contains(p))
            {
                iMouseDownY = p.Y - iThumbTop;
                iMouseDown = true;
                Invalidate(thumbRect);
                return;
            }

            if (iImageUpArrow != null)
            {
                Rectangle upRect = new Rectangle(0, 0, Width, upArrowHeight);
                if (upRect.Contains(p))
                {
                    iMouseDownUpArrow = true;
                    iTimerAutoRepeat.Change(0, kAutoRepeatTime);

                    return;
                }
            }

            if (iImageDownArrow != null)
            {
                Rectangle downRect = new Rectangle(0, upArrowHeight + channelHeight, Width, downArrowHeight);
                if (downRect.Contains(p))
                {
                    iMouseDownDownArrow = true;
                    iTimerAutoRepeat.Change(0, kAutoRepeatTime);

                    return;
                }
            }

            Rectangle channelTopRect = new Rectangle(0, upArrowHeight, Width, iThumbTop);
            if (channelTopRect.Contains(p))
            {
                if (UpdateThumbPositionRelative(-iLargeChange))
                {
                    if (EventScroll != null)
                    {
                        EventScroll(this, EventArgs.Empty);
                    }
                }

                return;
            }

            Rectangle channelBottomRect = new Rectangle(0, (int)(iThumbTop + upArrowHeight + iThumbBarHeight), Width, (int)((Height - downArrowHeight) - (iThumbTop + upArrowHeight + iThumbBarHeight)));
            if (channelBottomRect.Contains(p))
            {
                if (UpdateThumbPositionRelative(iLargeChange))
                {
                    if (EventScroll != null)
                    {
                        EventScroll(this, EventArgs.Empty);
                    }
                }

                return;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (iMouseDown || iMouseDownUpArrow || iMouseDownDownArrow)
            {
                iMouseDown = false;
                iMouseDownUpArrow = false;
                iMouseDownDownArrow = false;
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (iMouseDown && e.Button == MouseButtons.Left)
            {
                if (UpdateThumbPositionAbsolute(e.Y))
                {
                    if (EventScroll != null)
                    {
                        EventScroll(this, EventArgs.Empty);
                    }
                }
            }
        }

#if !PocketPC
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            //if (iMouseOverShow)
            //{
            SetMouseOverState(true);
                //iMouseOver = true;
                //Invalidate();
            //}

            if (iMouseDownUpArrow || iMouseDownDownArrow)
            {
                iTimerAutoRepeat.Change(kAutoRepeatTime, kAutoRepeatTime);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            //if (iMouseOverShow)
            //{
            SetMouseOverState(false);
                //iMouseOver = false;
                //Invalidate();
            //}

            iTimerAutoRepeat.Change(Timeout.Infinite, Timeout.Infinite);
        }
#endif

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            Invalidate();
        }

        private int ChannelHeight()
        {
            int channelHeight = Height;
            if (iImageUpArrow != null)
            {
                channelHeight -= iImageUpArrow.Height;
            }
            if (iImageDownArrow != null)
            {
                channelHeight -= iImageDownArrow.Height;
            }
            return channelHeight;
        }

        private float ThumbHeight(int aChannelHeight)
        {
            if (iImageThumbTop == null && iImageThumbTopSpan == null && iImageThumbBottom == null && iImageThumbBottomSpan == null)
            {
                return iImageThumb.Height;
            }
            else
            {
                float thumbHeight = ((float)iLargeChange / (float)iMaximum) * aChannelHeight;
                if (thumbHeight > aChannelHeight)
                {
                    thumbHeight = aChannelHeight;
                }
                if (thumbHeight < kMinimumThumbHeight)
                {
                    thumbHeight = kMinimumThumbHeight;
                }
                return thumbHeight;
            }
        }

        private void SetThumbPosition()
        {
            int channelHeight = ChannelHeight();
            float thumbHeight = ThumbHeight(channelHeight);

            int pixelRange = (int)(channelHeight - thumbHeight);
            int actualRange = (iMaximum - iMinimum);
            float percentage = 0.0f;
            if (actualRange != 0)
            {
                percentage = (float)iValue / (float)actualRange;
            }

            iThumbTop = (int)(percentage * pixelRange);
        }

        private bool UpdateThumbPositionRelative(int aValue)
        {
            int channelHeight = ChannelHeight();
            float thumbHeight = ThumbHeight(channelHeight);

            int actualRange = iMaximum - iMinimum;
            int pixelRange = (int)(channelHeight - thumbHeight);
            if (actualRange > 0 && pixelRange > 0)
            {
                iThumbTop += aValue;
                if (iThumbTop < 0)
                {
                    iThumbTop = 0;
                }
                if (iThumbTop > pixelRange)
                {
                    iThumbTop = pixelRange;
                }

                float percentage = (float)iThumbTop / (float)pixelRange;
                int value = (int)(percentage * iMaximum);
                if (value != iValue)
                {
                    iValue = value;

                    //Invalidate(false);
                    //Update();
                    Refresh();

                    return true;
                }
            }

            return false;
        }

        private bool UpdateThumbPositionAbsolute(int aY)
        {
            int channelHeight = ChannelHeight();
            float thumbHeight = ThumbHeight(channelHeight);

            int actualRange = iMaximum - iMinimum;
            int pixelRange = (int)(channelHeight - thumbHeight);
            if (actualRange > 0 && pixelRange > 0)
            {
                int newThumbTop = aY - iMouseDownY;

                if (newThumbTop < 0)
                {
                    iThumbTop = 0;
                }
                else if (newThumbTop > pixelRange)
                {
                    iThumbTop = pixelRange;
                }
                else
                {
                    iThumbTop = newThumbTop;
                }

                float percentage = (float)iThumbTop / (float)pixelRange;
                int value = (int)(percentage * iMaximum);
                if (value != iValue)
                {
                    iValue = value;

                    //Invalidate(false);
                    //Update();
                    Refresh();

                    return true;
                }
            }

            return false;
        }

        private void TimerElapsed(object sender)
        {
            switch (iBlendState)
            {
                case EBlendState.eIn:
                    iAlpha += iTicker.MilliSeconds / kBlendTime;

                    if (iAlpha < 1.0f)
                    {
                        iTicker.Reset();
                        iTimer.Change(kUpdateRate, Timeout.Infinite);
                    }
                    else
                    {
                        iAlpha = 1.0f;
                        iBlendState = EBlendState.eUnknown;
                    }
                    break;
                case EBlendState.eOut:
                    iAlpha -= iTicker.MilliSeconds / kBlendTime;

                    if (iAlpha > 0.0f)
                    {
                        iTicker.Reset();
                        iTimer.Change(kUpdateRate, Timeout.Infinite);
                    }
                    else
                    {
                        iMouseOver = false;
                        iAlpha = 0.0f;
                        iBlendState = EBlendState.eUnknown;
                    }
                    break;
                default:
                    break;
            }
            Invalidate();
        }

        private void SetMouseOverState(bool aState)
        {
            iTicker.Reset();
            if (aState)
            {
                iMouseOver = true;
                iBlendState = EBlendState.eIn;
                iTimer.Change(kUpdateRate, Timeout.Infinite);
            }
            else
            {
                iBlendState = EBlendState.eOut;
                iTimer.Change(kUpdateRate, Timeout.Infinite);
            }
        }

        private void TimerAutoRepeatElapsed(object sender)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                if (iMouseDownUpArrow)
                {
                    if (UpdateThumbPositionRelative(-iSmallChange))
                    {
                        Rectangle upRect = new Rectangle(0, 0, Width, iImageUpArrow.Height);
                        Invalidate(upRect);

                        if (EventScroll != null)
                        {
                            EventScroll(this, EventArgs.Empty);
                        }
                    }

                    return;
                }

                if (iMouseDownDownArrow)
                {
                    if (UpdateThumbPositionRelative(iSmallChange))
                    {
                        int channelHeight = ChannelHeight();
                        Rectangle downRect = new Rectangle(0, iImageUpArrow.Height + channelHeight, Width, iImageDownArrow.Height);
                        Invalidate(downRect);

                        if (EventScroll != null)
                        {
                            EventScroll(this, EventArgs.Empty);
                        }
                    }

                    return;
                }
            });
        }
    }
}
