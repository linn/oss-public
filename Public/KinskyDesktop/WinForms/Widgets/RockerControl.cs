using System;
using System.Drawing;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Linn;

namespace KinskyDesktop.Widgets
{
    public partial class RockerControl : Kontrol
    {
        private enum EBlendState
        {
            eIn,
            eOut,
            eUnknown
        }

        private const int kBlendTime = 100; // in ms
        private const int kUpdateRate = 20;

        private float iAlphaRing;
        private EBlendState iBlendStateRing;
        private Ticker iTickerRing;

        private float iAlphaCentre;
        private EBlendState iBlendStateCentre;
        private Ticker iTickerCentre;

        private System.Threading.Timer iTimer;

        private const uint kAutoRepeatDelayPeriod = 250;
        private uint kAutoRepeatPeriod = 150;

        private Mutex iMutex;
        private bool iAutoRepeating;
        private System.Threading.Timer iTimerAutoRepeat;
        private System.Threading.Timer iTimerAutoRepeatDelay;

        private bool iMouseOverRing;
        private bool iMouseOverCentre;

        private bool iMouseDownLeft;
        private bool iMouseDownMiddle;
        private bool iMouseDownRight;

        private bool iHitLeft;
        private bool iHitRight;

        private Image iImageClickLeft;
        private Image iImageClickRight;

        private Image iImageMouseOverRing;
        private Image iImageMouseOverCentre;

        public RockerControl()
        {
            iMutex = new Mutex(false);

            iAlphaRing = 0.0f;
            iBlendStateRing = EBlendState.eUnknown;
            iTickerRing = new Ticker();

            iAlphaCentre = 0.0f;
            iBlendStateCentre = EBlendState.eUnknown;
            iTickerCentre = new Ticker();

            iTimer = new System.Threading.Timer(TimerElapsed);
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);

            iTimerAutoRepeatDelay = new System.Threading.Timer(TimerAutoRepeatDelayElapsed);
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);

            iTimerAutoRepeat = new System.Threading.Timer(TimerAutoRepeatElapsed);
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);

            Text = String.Empty;

            InitializeComponent();
        }

        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        public Image ImageClickLeft
        {
            get
            {
                return iImageClickLeft;
            }
            set
            {
                iImageClickLeft = value;
            }
        }

        public Image ImageClickRight
        {
            get
            {
                return iImageClickRight;
            }
            set
            {
                iImageClickRight = value;
            }
        }

        public Image ImageMouseOverRing
        {
            get
            {
                return iImageMouseOverRing;
            }
            set
            {
                iImageMouseOverRing = value;
            }
        }

        public Image ImageMouseOverCentre
        {
            get
            {
                return iImageMouseOverCentre;
            }
            set
            {
                iImageMouseOverCentre = value;
            }
        }

        public uint AutoRepeatPeriod
        {
            get
            {
                return kAutoRepeatPeriod;
            }
            set
            {
                kAutoRepeatPeriod = value;
            }
        }

        public EventHandler<EventArgs> EventClickLeftStarted;
        public EventHandler<EventArgs> EventClickRightFinished;
        public EventHandler<EventArgs> EventClickLeft;

        public EventHandler<EventArgs> EventClickRightStarted;
        public EventHandler<EventArgs> EventClickLeftFinished;
        public EventHandler<EventArgs> EventClickRight;

        public EventHandler<EventArgs> EventClickMiddle;

        public EventHandler<EventArgs> EventClickCancelled;

        public EventHandler<EventArgs> EventMouseDownLeft;
        public EventHandler<EventArgs> EventMouseDownRight;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            bool processed = false;
            if (e.Button == MouseButtons.Left)
            {
                iAutoRepeating = false;

                float x = e.X - (Width * 0.5f);
                float y = e.Y - (Height * 0.5f);
                float distSquared = (x * x) + (y * y);
                if (distSquared < iOuterCircleRadius * iOuterCircleRadius) // inside outer circle
                {
                    if (distSquared < iInnerCircleRadius * iInnerCircleRadius) // inside inner circle
                    {
                        iMouseDownMiddle = iInnerRingEnabled;
                        processed = true;
                    }
                    else
                    {
                        if (e.X < Width * 0.5f)
                        {
                            iMouseDownLeft = iOuterRingEnabled;

                            iMutex.WaitOne();

                            iHitLeft = iOuterRingEnabled;
                            iTimerAutoRepeatDelay.Change(kAutoRepeatDelayPeriod, Timeout.Infinite);

                            iMutex.ReleaseMutex();

                            Refresh();

                            if (iOuterRingEnabled && EventClickLeftStarted != null)
                            {
                                EventClickLeftStarted(this, EventArgs.Empty);
                            }

                            processed = true;
                        }
                        else
                        {
                            iMouseDownRight = iOuterRingEnabled;
                            
                            iMutex.WaitOne();

                            iHitRight = iOuterRingEnabled;
                            iTimerAutoRepeatDelay.Change(kAutoRepeatDelayPeriod, Timeout.Infinite);
                            
                            iMutex.ReleaseMutex();

                            Refresh();

                            if (iOuterRingEnabled && EventClickRightStarted != null)
                            {
                                EventClickRightStarted(this, EventArgs.Empty);
                            }

                            processed = true;
                        }
                    }
                }
            }

            if (!processed)
            {
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            float x = e.X - (Width * 0.5f);
            float y = e.Y - (Height * 0.5f);
            float distSquared = (x * x) + (y * y);
            if (distSquared < iOuterCircleRadius * iOuterCircleRadius) // inside outer circle
            {
                if (distSquared < iInnerCircleRadius * iInnerCircleRadius) // inside inner circle
                {
                    iMutex.WaitOne();

                    iHitLeft = false;
                    iHitRight = false;

                    iTimerAutoRepeatDelay.Change(Timeout.Infinite, Timeout.Infinite);
                    iTimerAutoRepeat.Change(Timeout.Infinite, Timeout.Infinite);

                    iMutex.ReleaseMutex();

                    if (iMouseOverRing)
                    {
                        SetMouseOverRingState(false);
                    }
                    if (!iMouseOverCentre)
                    {
                        SetMouseOverCentreState(true);
                    }
                }
                else
                {
                    if (!iMouseOverRing)
                    {
                        SetMouseOverRingState(true);
                    }
                    if (iMouseOverCentre)
                    {
                        SetMouseOverCentreState(false);
                    }

                    if (e.X < Width * 0.5f)
                    {
                        iMutex.WaitOne();

                        if (iHitRight)
                        {
                            iTimerAutoRepeatDelay.Change(Timeout.Infinite, Timeout.Infinite);
                            iTimerAutoRepeat.Change(Timeout.Infinite, Timeout.Infinite);
                        }

                        iHitLeft = iMouseDownLeft;
                        iHitRight = false;

                        iMutex.ReleaseMutex();
                    }
                    else
                    {
                        iMutex.WaitOne();

                        if (iHitLeft)
                        {
                            iTimerAutoRepeatDelay.Change(Timeout.Infinite, Timeout.Infinite);
                            iTimerAutoRepeat.Change(Timeout.Infinite, Timeout.Infinite);
                        }

                        iHitLeft = false;
                        iHitRight = iMouseDownRight;

                        iMutex.ReleaseMutex();
                    }
                }
            }
            else
            {
                iMutex.WaitOne();

                iHitLeft = false;
                iHitRight = false;

                iTimerAutoRepeatDelay.Change(Timeout.Infinite, Timeout.Infinite);
                iTimerAutoRepeat.Change(Timeout.Infinite, Timeout.Infinite);

                iMutex.ReleaseMutex();

                if (iMouseOverRing)
                {
                    SetMouseOverRingState(false);
                }
                if (iMouseOverCentre)
                {
                    SetMouseOverCentreState(false);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            bool evented = false;
            if (e.Button == MouseButtons.Left)
            {
                float x = e.X - (Width * 0.5f);
                float y = e.Y - (Height * 0.5f);
                float distSquared = (x * x) + (y * y);
                if (distSquared < iOuterCircleRadius * iOuterCircleRadius) // inside outer circle
                {
                    if (distSquared < iInnerCircleRadius * iInnerCircleRadius) // inside inner circle
                    {
                        if (iMouseDownMiddle)
                        {
                            evented = true;

                            if (EventClickMiddle != null)
                            {
                                EventClickMiddle(this, EventArgs.Empty);
                            }
                        }
                    }
                    else
                    {
                        if (e.X < Width * 0.5f)
                        {
                            if (iHitLeft)
                            {
                                evented = true;

                                iMutex.WaitOne();
                                if (!iAutoRepeating)
                                {
                                    iMutex.ReleaseMutex();

                                    if (iOuterRingEnabled && EventClickLeft != null)
                                    {
                                        EventClickLeft(this, EventArgs.Empty);
                                    }
                                }
                                else
                                {
                                    iMutex.ReleaseMutex();
                                }

                                if (iOuterRingEnabled && EventClickLeftFinished != null)
                                {
                                    EventClickLeftFinished(this, EventArgs.Empty);
                                }
                            }
                        }
                        else
                        {
                            if (iHitRight)
                            {
                                evented = true;

                                iMutex.WaitOne();
                                if (!iAutoRepeating)
                                {
                                    iMutex.ReleaseMutex();

                                    if (iOuterRingEnabled && EventClickRight != null)
                                    {
                                        EventClickRight(this, EventArgs.Empty);
                                    }
                                }
                                else
                                {
                                    iMutex.ReleaseMutex();
                                }

                                if (iOuterRingEnabled && EventClickRightFinished != null)
                                {
                                    EventClickRightFinished(this, EventArgs.Empty);
                                }
                            }
                        }
                    }
                }
            }

            if (!evented)
            {
                if (iMouseDownMiddle || ((iMouseDownLeft || iMouseDownRight) && iOuterRingEnabled))
                {
                    if (EventClickCancelled != null)
                    {
                        EventClickCancelled(this, EventArgs.Empty);
                    }
                }
            }

            iMouseDownLeft = false;
            iMouseDownMiddle = false;
            iMouseDownRight = false;

            iMutex.WaitOne();

            iHitLeft = false;
            iHitRight = false;
            iAutoRepeating = false;

            iTimerAutoRepeatDelay.Change(Timeout.Infinite, Timeout.Infinite);
            iTimerAutoRepeat.Change(Timeout.Infinite, Timeout.Infinite);

            iMutex.ReleaseMutex();

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            SetMouseOverRingState(false);
            SetMouseOverCentreState(false);

            iMutex.WaitOne();

            iHitLeft = false;
            iHitRight = false;

            iTimerAutoRepeatDelay.Change(Timeout.Infinite, Timeout.Infinite);
            iTimerAutoRepeat.Change(Timeout.Infinite, Timeout.Infinite);

            iMutex.ReleaseMutex();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (iMouseOverRing && iOuterRingEnabled)
            {
                if (iImageMouseOverRing != null)
                {
                    ImageAttributes ia = new ImageAttributes();
                    ColorMatrix mx = new ColorMatrix();
                    mx.Matrix11 = mx.Matrix22 = mx.Matrix44 = 1.0f;
                    mx.Matrix33 = iAlphaRing;
                    ia.SetColorMatrix(mx);

                    e.Graphics.DrawImage(iImageMouseOverRing, new Rectangle(0, 0, iImageMouseOverRing.Width, iImageMouseOverRing.Height),
                        0, 0, iImageMouseOverRing.Width, iImageMouseOverRing.Height, GraphicsUnit.Pixel, ia);
                }
            }

            if (iMouseOverCentre && iInnerRingEnabled)
            {
                if (iImageMouseOverCentre != null)
                {
                    ImageAttributes ia = new ImageAttributes();
                    ColorMatrix mx = new ColorMatrix();
                    mx.Matrix11 = mx.Matrix22 = mx.Matrix44 = 1.0f;
                    mx.Matrix33 = iAlphaCentre;
                    ia.SetColorMatrix(mx);

                    e.Graphics.DrawImage(iImageMouseOverCentre, new Rectangle(0, 0, iImageMouseOverCentre.Width, iImageMouseOverCentre.Height),
                        0, 0, iImageMouseOverCentre.Width, iImageMouseOverCentre.Height, GraphicsUnit.Pixel, ia);
                }
            }

            if (iHitLeft)
            {
                if (iImageClickLeft != null)
                {
                    e.Graphics.DrawImage(iImageClickLeft, 0, 0);
                }
            }

            if (iHitRight)
            {
                if (iImageClickRight != null)
                {
                    e.Graphics.DrawImage(iImageClickRight, 0, 0);
                }
            }

            base.OnPaint(e);
        }

        private void TimerElapsed(object sender)
        {
            bool update = false;

            switch (iBlendStateRing)
            {
                case EBlendState.eIn:
                    iAlphaRing += iTickerRing.MilliSeconds / kBlendTime;

                    if (iAlphaRing < 1.0f)
                    {
                        iTickerRing.Reset();
                        update = true;
                    }
                    else
                    {
                        iAlphaRing = 1.0f;
                        iBlendStateRing = EBlendState.eUnknown;
                    }
                    break;
                case EBlendState.eOut:
                    iAlphaRing -= iTickerRing.MilliSeconds / kBlendTime;

                    if (iAlphaRing > 0.0f)
                    {
                        iTickerRing.Reset();
                        update = true;
                    }
                    else
                    {
                        iMouseOverRing = false;
                        iAlphaRing = 0.0f;
                        iBlendStateRing = EBlendState.eUnknown;
                    }
                    break;
                default:
                    break;
            }

            switch (iBlendStateCentre)
            {
                case EBlendState.eIn:
                    iAlphaCentre += iTickerCentre.MilliSeconds / kBlendTime;

                    if (iAlphaCentre < 1.0f)
                    {
                        iTickerCentre.Reset();
                        update = true;
                    }
                    else
                    {
                        iAlphaCentre = 1.0f;
                        iBlendStateCentre = EBlendState.eUnknown;
                    }
                    break;
                case EBlendState.eOut:
                    iAlphaCentre -= iTickerCentre.MilliSeconds / kBlendTime;

                    if (iAlphaCentre > 0.0f)
                    {
                        iTickerCentre.Reset();
                        update = true;
                    }
                    else
                    {
                        iMouseOverCentre = false;
                        iAlphaCentre = 0.0f;
                        iBlendStateCentre = EBlendState.eUnknown;
                    }
                    break;
                default:
                    break;
            }

            if (update)
            {
                iTimer.Change(kUpdateRate, Timeout.Infinite);
            }

            Invalidate();
        }

        private void TimerAutoRepeatDelayElapsed(object sender)
        {
            iTimerAutoRepeat.Change(0, Timeout.Infinite);
        }

        private void TimerAutoRepeatElapsed(object sender)
        {
            iMutex.WaitOne();

            iAutoRepeating = true;

            if (iHitLeft || iHitRight)
            {
                if (iHitLeft)
                {
                    iMutex.ReleaseMutex();

                    if (EventClickLeft != null)
                    {
                        EventClickLeft(this, EventArgs.Empty);
                    }
                }
                else if (iHitRight)
                {
                    iMutex.ReleaseMutex();

                    if (EventClickRight != null)
                    {
                        EventClickRight(this, EventArgs.Empty);
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }

                iTimerAutoRepeat.Change(kAutoRepeatPeriod, Timeout.Infinite);
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void SetMouseOverRingState(bool aState)
        {
            iTickerRing.Reset();
            if (aState)
            {
                iMouseOverRing = true;
                iBlendStateRing = EBlendState.eIn;
                iTimer.Change(kUpdateRate, Timeout.Infinite);
            }
            else
            {
                iBlendStateRing = EBlendState.eOut;
                iTimer.Change(kUpdateRate, Timeout.Infinite);
            }
        }

        private void SetMouseOverCentreState(bool aState)
        {
            iTickerCentre.Reset();
            if (aState)
            {
                iMouseOverCentre = true;
                iBlendStateCentre = EBlendState.eIn;
                iTimer.Change(kUpdateRate, Timeout.Infinite);
            }
            else
            {
                iBlendStateCentre = EBlendState.eOut;
                iTimer.Change(kUpdateRate, Timeout.Infinite);
            }
        }
    }
}
