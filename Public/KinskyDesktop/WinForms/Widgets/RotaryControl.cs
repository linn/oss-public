using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;

using Linn;

namespace KinskyDesktop.Widgets
{
    public partial class RotaryControl : Kontrol
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

        protected bool iMouseDown;
        protected bool iMouseOverCentre;
        protected bool iMouseOverRing;

        private uint iStepSize;

        private double iAngle;
        private double iLastAngle;
        
        private double iOldAngle;
        private double iNewAngle;
        private bool iFirstUpdate;

        private bool iClickEventStart;

        private Image iImageKnob;
        private Image iImageMouseOverRing;
        private Image iImageMouseOverCentre;

        // pre-defined resources
        private PointF[] iRingPoints;

        public RotaryControl()
        {
            iAlphaRing = 0.0f;
            iBlendStateRing = EBlendState.eUnknown;
            iTickerRing = new Ticker();

            iAlphaCentre = 0.0f;
            iBlendStateCentre = EBlendState.eUnknown;
            iTickerCentre = new Ticker();
            
            iTimer = new System.Threading.Timer(TimerElapsed);
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);

            Text = String.Empty;

            iPenArc = new Pen(Color.FromArgb(128, Color.Red), 4);
            
            iAngle = 0;
            iStepSize = 30;

            InitialiseRing();

            InitializeComponent();
        }

        public EventHandler<EventArgs> EventClockwiseStep;
        public EventHandler<EventArgs> EventAntiClockwiseStep;
        public EventHandler<EventArgs> EventClick;
        public EventHandler<EventArgs> EventStartRotation;
        public EventHandler<EventArgs> EventEndRotation;
        public EventHandler<EventArgs> EventCancelRotation;

        public Image ImageKnob
        {
            get
            {
                return iImageKnob;
            }
            set
            {
                iImageKnob = value;
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

        public override float OuterCircleRadius
        {
            set
            {
                base.OuterCircleRadius = value;
                InitialiseRing();
            }
        }

        public override float InnerCircleRadius
        {
            set
            {
                base.InnerCircleRadius = value;
                InitialiseRing();
            }
        }

        public uint StepSize
        {
            get
            {
                return iStepSize;
            }
            set
            {
                iStepSize = value;
            }
        }

        public double Angle
        {
            get
            {
                return iAngle;
            }
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

        public string TextValue
        {
            get
            {
                return Text;
            }
            set
            {
                Text = value;
            }
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

                    e.Graphics.DrawImage(iImageMouseOverRing, new Rectangle((int)((Width - iImageMouseOverRing.Width) * 0.5f), (int)((Height - iImageMouseOverRing.Height) * 0.5f), iImageMouseOverRing.Width, iImageMouseOverRing.Height),
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

                    e.Graphics.DrawImage(iImageMouseOverCentre, new Rectangle((int)((Width - iImageMouseOverCentre.Width) * 0.5f), (int)((Height - iImageMouseOverCentre.Height) * 0.5f), iImageMouseOverCentre.Width, iImageMouseOverCentre.Height),
                        0, 0, iImageMouseOverCentre.Width, iImageMouseOverCentre.Height, GraphicsUnit.Pixel, ia);
                }
            }

            foreach (PointF p in iRingPoints)
            {
                if (iImageKnob != null)
                {
                    e.Graphics.DrawImage(iImageKnob, p.X + (Width * 0.5f) - (iImageKnob.Width * 0.5f) + 1, p.Y + (Height * 0.5f) - (iImageKnob.Height * 0.5f) + 1);
                }
            }

            base.OnPaint(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            bool processed = false;
            if (e.Button == MouseButtons.Left)
            {
                float x = e.X - (Width * 0.5f);
                float y = e.Y - (Height * 0.5f);
                float distSquared = (x * x) + (y * y);
                if (distSquared < iHitOuterCircleRadius * iHitOuterCircleRadius) // inside outer circle
                {
                    if (distSquared < iHitInnerCircleRadius * iHitInnerCircleRadius) // inside inner circle
                    {
                        iClickEventStart = iInnerRingEnabled;
                        processed = true;
                    }
                    else
                    {
                        iMouseDown = iOuterRingEnabled;
                        iFirstUpdate = true;
                        processed = true;

                        if (iOuterRingEnabled && EventStartRotation != null)
                        {
                            EventStartRotation(this, EventArgs.Empty);
                        }
                    }
                }
            }

            if (!processed)
            {
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                iMouseDown = false;

                float x = e.X - (Width * 0.5f);
                float y = e.Y - (Height * 0.5f);
                float distSquared = (x * x) + (y * y);
                if (distSquared < iHitOuterCircleRadius * iHitOuterCircleRadius) // inside outer circle
                {
                    if (distSquared < iHitInnerCircleRadius * iHitInnerCircleRadius) // inside inner circle
                    {
                        if (iClickEventStart)   // mouse down event was inside inner circle
                        {
                            if (EventClick != null)
                            {
                                EventClick(this, EventArgs.Empty);
                            }
                        }
                    }
                    if (iOuterRingEnabled && EventEndRotation != null)
                    {
                        EventEndRotation(this, EventArgs.Empty);
                    }
                }
                else
                {
                    if (iOuterRingEnabled && EventCancelRotation != null)
                    {
                        EventCancelRotation(this, EventArgs.Empty);
                    }
                }

                iClickEventStart = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            float x = e.X - (Width * 0.5f);
            float y = e.Y - (Height * 0.5f);
            float distSquared = (x * x) + (y * y);
            if (distSquared < iHitOuterCircleRadius * iHitOuterCircleRadius) // inside outer circle
            {
                if (distSquared < iHitInnerCircleRadius * iHitInnerCircleRadius) // inside inner circle
                {
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
                }
            }
            else
            {
                if (iMouseOverRing)
                {
                    SetMouseOverRingState(false);
                }
                if (iMouseOverCentre)
                {
                    SetMouseOverCentreState(false);
                }
            }

            if (iMouseDown)
            {
                UpdateAngle();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            SetMouseOverRingState(false);
            SetMouseOverCentreState(false);
        }

        private void InitialiseRing()
        {
            float scale = 0.4f;
            iRingPoints = new PointF[] { new Point(0, (int)(iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale))),
                          new PointF(0, -(int)(iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale))),
                          new PointF((int)(iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale)), 0),
                          new PointF(-(int)(iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale)), 0),
                          new PointF((int)((iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale)) * 0.707106781187), (int)((iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale)) * 0.707106781187)),
                          new PointF(-(int)((iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale)) * 0.707106781187), -(int)((iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale)) * 0.707106781187)),
                          new PointF(-(int)((iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale)) * 0.707106781187), (int)((iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale)) * 0.707106781187)),
                          new PointF((int)((iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale)) * 0.707106781187), -(int)((iOuterCircleRadius - ((iOuterCircleRadius - iInnerCircleRadius) * scale)) * 0.707106781187)) };
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

        private void UpdateAngle()
        {
            Point mouse = PointToClient(MousePosition);

            double angle = ((Math.Atan2(mouse.Y - (Height * 0.5f), mouse.X - (Width * 0.5f)) * 180) / Math.PI) + 270;

            if (angle > 360)
            {
                angle -= 360;
            }

            iNewAngle = angle;
            if (iFirstUpdate)
            {
                iLastAngle = 0;
                iOldAngle = iNewAngle;
                iFirstUpdate = false;
            }

            double delta = iNewAngle - iOldAngle;

            if (delta > 180)
            {
                delta -= 360;
            }

            if (delta < -180)
            {
                delta += 360;
            }

            iLastAngle += delta;

            //Console.WriteLine("RotaryControl.UpdateAngle: delta=" + delta + ", iNewAngle=" + iNewAngle + ", iOldAngle=" + iOldAngle + ", iLastAngle=" + iLastAngle);

            // event according to angle delta

            if (iLastAngle > iStepSize)
            {
                int steps = (int)(iLastAngle / iStepSize);

                Trace.WriteLine(Trace.kKinsky, "RotaryControl.UpdateAngle: delta=" + delta + ", steps=" + steps);
                
                for (int i = 0; i < steps; ++i)
                {
                    if (EventClockwiseStep != null)
                    {
                        EventClockwiseStep(this, EventArgs.Empty);
                    }
                }

                iLastAngle = iLastAngle - (steps * iStepSize);
            }

            if (iLastAngle < -iStepSize)
            {
                int steps = (int)(-iLastAngle / iStepSize);

                Trace.WriteLine(Trace.kKinsky, "RotaryControl.UpdateAngle: delta=" + delta + ", steps=" + steps);

                for (int i = 0; i < steps; ++i)
                {
                    if (EventAntiClockwiseStep != null)
                    {
                        EventAntiClockwiseStep(this, EventArgs.Empty);
                    }
                }

                iLastAngle = (-iLastAngle) - (steps * iStepSize);
            }

            //Console.WriteLine("iAngle=" + iAngle + ", delta=" + delta);

            PositionRingPoints(delta);

            iOldAngle = iNewAngle;

            Invalidate();
        }

        private void PositionRingPoints(double aDelta)
        {
            Matrix transform = new Matrix();
            transform.Rotate((float)aDelta);
            transform.TransformPoints(iRingPoints);
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
