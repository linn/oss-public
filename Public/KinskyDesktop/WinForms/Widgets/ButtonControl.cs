using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;

using Linn;

namespace KinskyDesktop.Widgets
{
    public partial class ButtonControl : UserControl
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


        protected bool iMouseDown;
        protected bool iMouseOver;

        protected Image iImageDisabled;
        protected Image iImageTouched;
        protected Image iImageMouseOver;
        protected Image iImageStateInitial;

        protected Image iImageCurrentTouched;
        protected Image iImageCurrentMouseOver;
        protected Image iImageCurrentState;

        public event EventHandler<EventArgs> EventClick;

        public ButtonControl()
        {
            iAlpha = 0.0f;
            iBlendState = EBlendState.eUnknown;
            iTicker = new Ticker();
            iTimer = new System.Threading.Timer(TimerElapsed);
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);
         
            InitializeComponent();
        }

        public Image ImageDisabled
        {
            get
            {
                return iImageDisabled;
            }
            set
            {
                iImageDisabled = value;
            }
        }

        public Image ImageTouched
        {
            get
            {
                return iImageTouched;
            }
            set
            {
                iImageTouched = value;
                iImageCurrentTouched = value;
            }
        }

        public Image ImageMouseOver
        {
            get
            {
                return iImageMouseOver;
            }
            set
            {
                iImageMouseOver = value;
                iImageCurrentMouseOver = value;
            }
        }

        public Image ImageStateInitial
        {
            get
            {
                return iImageStateInitial;
            }
            set
            {
                iImageStateInitial = value;
                iImageCurrentState = value;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                iMouseDown = true;
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        
            if (iMouseDown)
            {
                if (!ClientRectangle.Contains(new Point(e.X, e.Y)))
                {
                    if (iMouseOver)
                    {
                        SetMouseOverState(false);
                        //Invalidate();
                    }
                }
                else
                {
                    if (!iMouseOver)
                    {
                        SetMouseOverState(true);
                        //Invalidate();
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (iMouseDown && iMouseOver)
            {
                if (EventClick != null)
                {
                    EventClick(this, EventArgs.Empty);
                }
            }
            iMouseDown = false;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            SetMouseOverState(true);
            //Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            SetMouseOverState(false);
            //Invalidate();
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            SetMouseOverState(true);
            //Invalidate();
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);

            SetMouseOverState(false);
            //Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Enabled)
            {
                if (iMouseOver)
                {
                    if (iMouseDown)
                    {
                        if (iImageCurrentTouched != null)
                        {
                            e.Graphics.DrawImage(iImageCurrentTouched, (int)((Width - iImageCurrentTouched.Width) * 0.5f), (int)((Height - iImageCurrentTouched.Height) * 0.5f));
                        }
                        else if(iImageCurrentState != null)
                        {
                            e.Graphics.DrawImage(iImageCurrentState, (int)(Width - iImageCurrentState.Width) * 0.5f, (int)(Height - iImageCurrentState.Height) * 0.5f);
                        }
                    }
                    else
                    {
                        if (iImageCurrentState != null)
                        {
                            ImageAttributes ia = new ImageAttributes();
                            ColorMatrix mx = new ColorMatrix();
                            mx.Matrix11 = mx.Matrix22 = mx.Matrix44 = 1.0f;
                            mx.Matrix33 = 1.0f - iAlpha;
                            ia.SetColorMatrix(mx);

                            e.Graphics.DrawImage(iImageCurrentState, new Rectangle((int)((Width - iImageCurrentState.Width) * 0.5f), (int)((Height - iImageCurrentState.Height) * 0.5f), iImageCurrentState.Width, iImageCurrentState.Height),
                                0, 0, iImageCurrentState.Width, iImageCurrentState.Height, GraphicsUnit.Pixel, ia);
                            //e.Graphics.DrawImage(iImageCurrentState, (int)((Width - iImageCurrentState.Width) * 0.5f), (int)((Height - iImageCurrentState.Height) * 0.5f));
                        }

                        if (iImageCurrentMouseOver != null)
                        {
                            ImageAttributes ia = new ImageAttributes();
                            ColorMatrix mx = new ColorMatrix();
                            mx.Matrix11 = mx.Matrix22 = mx.Matrix44 = 1.0f;
                            mx.Matrix33 = iAlpha;
                            ia.SetColorMatrix(mx);

                            e.Graphics.DrawImage(iImageCurrentMouseOver, new Rectangle((int)((Width - iImageCurrentMouseOver.Width) * 0.5f), (int)((Height - iImageCurrentMouseOver.Height) * 0.5f), iImageCurrentMouseOver.Width, iImageCurrentMouseOver.Height),
                                0, 0, iImageCurrentMouseOver.Width, iImageCurrentMouseOver.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                }
                else
                {
                    if (iImageCurrentState != null)
                    {
                        e.Graphics.DrawImage(iImageCurrentState, (int)((Width - iImageCurrentState.Width) * 0.5f), (int)((Height - iImageCurrentState.Height) * 0.5f));
                    }
                }
            }
            else
            {
                if (iImageDisabled != null)
                {
                    e.Graphics.DrawImage(iImageDisabled, (int)((Width - iImageDisabled.Width) * 0.5f), (int)((Height - iImageDisabled.Height) * 0.5f));
                }
                else
                {
                    if (iImageStateInitial != null)
                    {
                        e.Graphics.DrawImage(iImageStateInitial, (int)((Width - iImageStateInitial.Width) * 0.5f), (int)((Height - iImageStateInitial.Height) * 0.5f));
                    }
                }
            }
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
    }
}
