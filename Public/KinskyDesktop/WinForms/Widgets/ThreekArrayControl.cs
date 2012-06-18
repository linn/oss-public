using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Text;
using System.Windows.Forms;

using Linn;

namespace KinskyDesktop.Widgets
{
    public partial class ThreekArrayControl : UserControl
    {
        public enum EMode
        {
            ePlaylist,
            eControl
        }

        private enum EBlendState
        {
            eIn,
            eOut,
            eUnknown
        }

        private const int kBlendTime = 100; // in ms
        private const int kUpdateRate = 20;

        private float iAlphaMiddle;
        private EBlendState iBlendStateMiddle;
        private Ticker iTickerMiddle;

        private System.Threading.Timer iTimer;

        private bool iMouseDown;
        private bool iMouseDownLeft;
        private bool iMouseDownMiddle;
        private bool iMouseDownRight;

        private bool iMouseOverLeft;
        private bool iMouseOverMiddle;
        private bool iMouseOverRight;

        private bool iControlLeftEnabled;
        private bool iControlMiddleEnabled;
        private bool iControlRightEnabled;

        private bool iPlaylistLeftEnabled;
        private bool iPlaylistMiddleEnabled;
        private bool iPlaylistRightEnabled;

        private EMode iMode;
        private bool iDragging;
        private bool iState;

        private bool iHitLeft;
        private Rectangle iHitRectLeft;
        private bool iHitMiddle;
        private Rectangle iHitRectMiddle;
        private bool iHitRight;
        private Rectangle iHitRectRight;

        private Point iLeftLocation;
        private Image iLeft;
        private Point iLeftTouchLocation;
        private Image iLeftTouch;
        private Image iLeftMouseOver;

        private Point iMiddleLocationState1;
        private Image iMiddleState1;
        private Point iMiddleTouchLocationState1;
        private Image iMiddleTouchState1;
        private Image iMiddleMouseOverState1;

        private Point iMiddleLocationState2;
        private Image iMiddleState2;
        private Point iMiddleTouchLocationState2;
        private Image iMiddleTouchState2;
        private Image iMiddleMouseOverState2;

        private Point iRightLocation;
        private Image iRight;
        private Point iRightTouchLocation;
        private Image iRightTouch;
        private Image iRightMouseOver;

        private Image iPlayNow;
        private Image iPlayNowMouseOver;
        private Image iPlayNext;
        private Image iPlayNextMouseOver;
        private Image iPlayLater;
        private Image iPlayLaterMouseOver;

        public ThreekArrayControl()
        {
            iAlphaMiddle = 0.0f;
            iBlendStateMiddle = EBlendState.eUnknown;
            iTickerMiddle = new Ticker();

            iTimer = new System.Threading.Timer(TimerElapsed);
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);

            iHitRectLeft = new Rectangle(26, 40, 50, 50);
            iHitRectMiddle = new Rectangle(79, 19, 93, 92);
            iHitRectRight = new Rectangle(175, 40, 50, 50);

            iControlLeftEnabled = true;
            iControlMiddleEnabled = true;
            iControlRightEnabled = true;

            iPlaylistLeftEnabled = true;
            iPlaylistMiddleEnabled = true;
            iPlaylistRightEnabled = true;

            iMode = EMode.eControl;

            InitializeComponent();
        }

        public EventHandler<EventArgs> EventClickLeft;
        public EventHandler<EventArgs> EventClickMiddle;
        public EventHandler<EventArgs> EventClickRight;
        public EventHandler<DragEventArgs> EventDragOverLeft;
        public EventHandler<DragEventArgs> EventDragOverMiddle;
        public EventHandler<DragEventArgs> EventDragOverRight;
        public EventHandler<DragEventArgs> EventDragDropLeft;
        public EventHandler<DragEventArgs> EventDragDropMiddle;
        public EventHandler<DragEventArgs> EventDragDropRight;

        public bool State
        {
            get
            {
                return iState;
            }
            set
            {
                iState = value;
                Invalidate();
            }
        }

        public EMode Mode
        {
            get
            {
                return iMode;
            }
            set
            {
                iMode = value;
                Invalidate();
            }
        }

        public bool ControlLeftEnabled
        {
            get
            {
                return iControlLeftEnabled;
            }
            set
            {
                iControlLeftEnabled = value;
                Invalidate();
            }
        }

        public bool ControlMiddleEnabled
        {
            get
            {
                return iControlMiddleEnabled;
            }
            set
            {
                iControlMiddleEnabled = value;
                Invalidate();
            }
        }

        public bool ControlRightEnabled
        {
            get
            {
                return iControlRightEnabled;
            }
            set
            {
                iControlRightEnabled = value;
                Invalidate();
            }
        }

        public bool PlaylistLeftEnabled
        {
            get
            {
                return iPlaylistLeftEnabled;
            }
            set
            {
                iPlaylistLeftEnabled = value;
                Invalidate();
            }
        }

        public bool PlaylistMiddleEnabled
        {
            get
            {
                return iPlaylistMiddleEnabled;
            }
            set
            {
                iPlaylistMiddleEnabled = value;
                Invalidate();
            }
        }

        public bool PlaylistRightEnabled
        {
            get
            {
                return iPlaylistRightEnabled;
            }
            set
            {
                iPlaylistRightEnabled = value;
                Invalidate();
            }
        }

        public Rectangle HitRectangleLeft
        {
            get
            {
                return iHitRectLeft;
            }
            set
            {
                iHitRectLeft = value;
                Invalidate();
            }
        }

        public Rectangle HitRectangleMiddle
        {
            get
            {
                return iHitRectMiddle;
            }
            set
            {
                iHitRectMiddle = value;
                Invalidate();
            }
        }

        public Rectangle HitRectangleRight
        {
            get
            {
                return iHitRectRight;
            }
            set
            {
                iHitRectRight = value;
                Invalidate();
            }
        }

        public Point LeftLocation
        {
            get
            {
                return iLeftLocation;
            }
            set
            {
                iLeftLocation = value;
                Invalidate();
            }
        }

        public Image LeftImage
        {
            get
            {
                return iLeft;
            }
            set
            {
                iLeft = value;
                Invalidate();
            }
        }

        public Point LeftTouchLocation
        {
            get
            {
                return iLeftTouchLocation;
            }
            set
            {
                iLeftTouchLocation = value;
                Invalidate();
            }
        }

        public Image LeftTouchImage
        {
            get
            {
                return iLeftTouch;
            }
            set
            {
                iLeftTouch = value;
                Invalidate();
            }
        }

        public Image LeftMouseOverImage
        {
            get
            {
                return iLeftMouseOver;
            }
            set
            {
                iLeftMouseOver = value;
                Invalidate();
            }
        }

        public Point MiddleLocationState1
        {
            get
            {
                return iMiddleLocationState1;
            }
            set
            {
                iMiddleLocationState1 = value;
                Invalidate();
            }
        }

        public Image MiddleImageState1
        {
            get
            {
                return iMiddleState1;
            }
            set
            {
                iMiddleState1 = value;
                Invalidate();
            }
        }

        public Point MiddleTouchLocationState1
        {
            get
            {
                return iMiddleTouchLocationState1;
            }
            set
            {
                iMiddleTouchLocationState1 = value;
                Invalidate();
            }
        }

        public Image MiddleTouchImageState1
        {
            get
            {
                return iMiddleTouchState1;
            }
            set
            {
                iMiddleTouchState1 = value;
                Invalidate();
            }
        }

        public Image MiddleMouseOverImageState1
        {
            get
            {
                return iMiddleMouseOverState1;
            }
            set
            {
                iMiddleMouseOverState1 = value;
                Invalidate();
            }
        }

        public Point MiddleLocationState2
        {
            get
            {
                return iMiddleLocationState2;
            }
            set
            {
                iMiddleLocationState2 = value;
                Invalidate();
            }
        }

        public Image MiddleImageState2
        {
            get
            {
                return iMiddleState2;
            }
            set
            {
                iMiddleState2 = value;
                Invalidate();
            }
        }

        public Point MiddleTouchLocationState2
        {
            get
            {
                return iMiddleTouchLocationState2;
            }
            set
            {
                iMiddleTouchLocationState2 = value;
                Invalidate();
            }
        }

        public Image MiddleTouchImageState2
        {
            get
            {
                return iMiddleTouchState2;
            }
            set
            {
                iMiddleTouchState2 = value;
                Invalidate();
            }
        }

        public Image MiddleMouseOverImageState2
        {
            get
            {
                return iMiddleMouseOverState2;
            }
            set
            {
                iMiddleMouseOverState2 = value;
                Invalidate();
            }
        }

        public Point RightLocation
        {
            get
            {
                return iRightLocation;
            }
            set
            {
                iRightLocation = value;
                Invalidate();
            }
        }

        public Image RightImage
        {
            get
            {
                return iRight;
            }
            set
            {
                iRight = value;
                Invalidate();
            }
        }

        public Point RightTouchLocation
        {
            get
            {
                return iRightTouchLocation;
            }
            set
            {
                iRightTouchLocation = value;
                Invalidate();
            }
        }

        public Image RightTouchImage
        {
            get
            {
                return iRightTouch;
            }
            set
            {
                iRightTouch = value;
                Invalidate();
            }
        }

        public Image RightMouseOverImage
        {
            get
            {
                return iRightMouseOver;
            }
            set
            {
                iRightMouseOver = value;
                Invalidate();
            }
        }

        public Image PlayNow
        {
            get
            {
                return iPlayNow;
            }
            set
            {
                iPlayNow = value;
                Invalidate();
            }
        }

        public Image PlayNowMouseOver
        {
            get
            {
                return iPlayNowMouseOver;
            }
            set
            {
                iPlayNowMouseOver = value;
                Invalidate();
            }
        }

        public Image PlayNext
        {
            get
            {
                return iPlayNext;
            }
            set
            {
                iPlayNext = value;
                Invalidate();
            }
        }

        public Image PlayNextMouseOver
        {
            get
            {
                return iPlayNextMouseOver;
            }
            set
            {
                iPlayNextMouseOver = value;
                Invalidate();
            }
        }

        public Image PlayLater
        {
            get
            {
                return iPlayLater;
            }
            set
            {
                iPlayLater = value;
                Invalidate();
            }
        }

        public Image PlayLaterMouseOver
        {
            get
            {
                return iPlayLaterMouseOver;
            }
            set
            {
                iPlayLaterMouseOver = value;
                Invalidate();
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            
            iMouseDown = false;
            iMouseDownLeft = false;
            iMouseDownMiddle = false;
            iMouseDownRight = false;
            
            iHitLeft = false;
            iHitMiddle = false;
            iHitRight = false;

            iMouseOverLeft = false;
            SetMouseOverMiddleState(false);
            iMouseOverRight = false;
        }

        private void TimerElapsed(object sender)
        {
            bool update = false;
            switch (iBlendStateMiddle)
            {
                case EBlendState.eIn:
                    iAlphaMiddle += iTickerMiddle.MilliSeconds / kBlendTime;

                    if (iAlphaMiddle < 1.0f)
                    {
                        iTickerMiddle.Reset();
                        update = true;
                    }
                    else
                    {
                        iAlphaMiddle = 1.0f;
                        iBlendStateMiddle = EBlendState.eUnknown;
                    }
                    break;
                case EBlendState.eOut:
                    iAlphaMiddle -= iTickerMiddle.MilliSeconds / kBlendTime;

                    if (iAlphaMiddle > 0.0f)
                    {
                        iTickerMiddle.Reset();
                        update = true;
                    }
                    else
                    {
                        iMouseOverMiddle = false;
                        iAlphaMiddle = 0.0f;
                        iBlendStateMiddle = EBlendState.eUnknown;
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

        private void SetMouseOverMiddleState(bool aState)
        {
            iTickerMiddle.Reset();
            if (aState)
            {
                iMouseOverMiddle = true;
                iBlendStateMiddle = EBlendState.eIn;
                iTimer.Change(kUpdateRate, Timeout.Infinite);
            }
            else
            {
                iBlendStateMiddle = EBlendState.eOut;
                iTimer.Change(kUpdateRate, Timeout.Infinite);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            bool processed = false;
            if (e.Button == MouseButtons.Left)
            {
                Point pos = new Point(e.X, e.Y);
                if (iControlLeftEnabled && iHitRectLeft.Contains(pos))
                {
                    iMouseDown = true;
                    iMouseDownLeft = true;
                    iHitLeft = true;
                    processed = true;

                    Refresh();
                }
                else if (iControlMiddleEnabled && iHitRectMiddle.Contains(pos))
                {
                    iMouseDown = true;
                    iMouseDownMiddle = true;
                    iHitMiddle = true;
                    processed = true;

                    Refresh();
                }
                else if (iControlRightEnabled && iHitRectRight.Contains(pos))
                {
                    iMouseDown = true;
                    iMouseDownRight = true;
                    iHitRight = true;
                    processed = true;

                    Refresh();
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

            Point pos = new Point(e.X, e.Y);

            if (iControlLeftEnabled)
            {
                if (iHitRectLeft.Contains(pos))
                {
                    if (iMouseDownLeft && !iHitLeft)
                    {
                        iHitLeft = true;
                        Invalidate();
                    }

                    if (!iMouseOverLeft)
                    {
                        iMouseOverLeft = true;
                        Invalidate();
                    }
                }
                else
                {
                    if (iMouseDownLeft && iHitLeft)
                    {
                        iHitLeft = false;
                        Invalidate();
                    }

                    if (iMouseOverLeft)
                    {
                        iMouseOverLeft = false;
                        Invalidate();
                    }
                }
            }

            if (iControlMiddleEnabled)
            {
                if (iHitRectMiddle.Contains(pos))
                {
                    if (iMouseDownMiddle && !iHitMiddle)
                    {
                        iHitMiddle = true;
                        Invalidate();
                    }

                    if (!iMouseOverMiddle)
                    {
                        SetMouseOverMiddleState(true);
                        Invalidate();
                    }
                }
                else
                {
                    if (iMouseDownMiddle && iHitMiddle)
                    {
                        iHitMiddle = false;
                        Invalidate();
                    }

                    if (iMouseOverMiddle)
                    {
                        SetMouseOverMiddleState(false);
                        Invalidate();
                    }
                }
            }

            if (iControlRightEnabled)
            {
                if (iHitRectRight.Contains(pos))
                {
                    if (iMouseDownRight && !iHitRight)
                    {
                        iHitRight = true;
                        Invalidate();
                    }

                    if (!iMouseOverRight)
                    {
                        iMouseOverRight = true;
                        Invalidate();
                    }
                }
                else
                {
                    if (iMouseDownRight && iHitRight)
                    {
                        iHitRight = false;
                        Invalidate();
                    }

                    if (iMouseOverRight)
                    {
                        iMouseOverRight = false;
                        Invalidate();
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (iMouseDown)
            {
                Point pos = new Point(e.X, e.Y);

                if (iHitRectLeft.Contains(pos) && iMouseDownLeft)
                {
                    iHitLeft = false;

                    if (EventClickLeft != null)
                    {
                        EventClickLeft(this, EventArgs.Empty);
                    }
                }
                else if (iHitRectMiddle.Contains(pos) && iMouseDownMiddle)
                {
                    iHitMiddle = false;

                    if (EventClickMiddle != null)
                    {
                        EventClickMiddle(this, EventArgs.Empty);
                    }
                }
                else if (iHitRectRight.Contains(pos) && iMouseDownRight)
                {
                    iHitRight = false;

                    if (EventClickRight != null)
                    {
                        EventClickRight(this, EventArgs.Empty);
                    }
                }

                iMouseDown = false;
                iMouseDownLeft = false;
                iMouseDownMiddle = false;
                iMouseDownRight = false;
            }

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            iMouseOverLeft = false;
            SetMouseOverMiddleState(false);
            iMouseOverRight = false;

            Invalidate();
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);

            e.Effect = DragDropEffects.None;
            iDragging = true;

            Point pos = PointToClient(new Point(MousePosition.X, MousePosition.Y));
            if ((iPlaylistLeftEnabled /*&& iMode == EMode.ePlaylist*/) && iHitRectLeft.Contains(pos))
            {
                if (!iMouseOverLeft)
                {
                    iMouseOverLeft = true;
                    Invalidate();
                }

                if (EventDragOverLeft != null)
                {
                    EventDragOverLeft(this, e);
                }
            }
            else
            {
                if (iMouseOverLeft)
                {
                    iMouseOverLeft = false;
                    Invalidate();
                }
            }

            if ((iPlaylistMiddleEnabled /*&& iMode == EMode.ePlaylist*/) && iHitRectMiddle.Contains(pos))
            {
                if (!iMouseOverMiddle)
                {
                    SetMouseOverMiddleState(true);
                    Invalidate();
                }

                if (EventDragOverMiddle != null)
                {
                    EventDragOverMiddle(this, e);
                }
            }
            else
            {
                if (iMouseOverMiddle)
                {
                    SetMouseOverMiddleState(false);
                    Invalidate();
                }
            }

            if ((iPlaylistRightEnabled /*&& iMode == EMode.ePlaylist*/) && iHitRectRight.Contains(pos))
            {
                if (!iMouseOverRight)
                {
                    iMouseOverRight = true;
                    Invalidate();
                }

                if (EventDragOverRight != null)
                {
                    EventDragOverRight(this, e);
                }
            }
            else
            {
                if (iMouseOverRight)
                {
                    iMouseOverRight = false;
                    Invalidate();
                }
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            Point pos = PointToClient(new Point(MousePosition.X, MousePosition.Y));
            if (iControlLeftEnabled && iHitRectLeft.Contains(pos))
            {
                if (EventDragDropLeft != null)
                {
                    EventDragDropLeft(this, e);
                }
            }
            if (iControlMiddleEnabled && iHitRectMiddle.Contains(pos))
            {
                if (EventDragDropMiddle != null)
                {
                    EventDragDropMiddle(this, e);
                }
            }
            if (iControlRightEnabled && iHitRectRight.Contains(pos))
            {
                if (EventDragDropRight != null)
                {
                    EventDragDropRight(this, e);
                }
            }

            iDragging = false;
            Invalidate();
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);

            iDragging = false;
            iMouseOverLeft = false;
            SetMouseOverMiddleState(false);
            iMouseOverRight = false;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Enabled)
            {
                switch (iMode)
                {
                    case EMode.eControl:
                        if (iDragging)
                        {
                            PaintModePlaylist(e);
                        }
                        else
                        {
                            PaintModeControl(e);
                        }
                        break;
                    case EMode.ePlaylist:
                        PaintModePlaylist(e);
                        break;
                }
            }

            /*using (Pen pen = new Pen(new SolidBrush(Color.White)))
            {
                e.Graphics.DrawRectangle(pen, iHitRectLeft);
                e.Graphics.DrawRectangle(pen, iHitRectMiddle);
                e.Graphics.DrawRectangle(pen, iHitRectRight);
            }*/
        }

        private void PaintModeControl(PaintEventArgs e)
        {
            if (iControlLeftEnabled)
            {
                if (iHitLeft)
                {
                    if (iLeftTouch != null)
                    {
                        e.Graphics.DrawImage(iLeftTouch, iLeftTouchLocation);
                    }
                }
                else
                {
                    if (iMouseOverLeft)
                    {
                        if (iLeftMouseOver != null)
                        {
                            e.Graphics.DrawImage(iLeftMouseOver, iLeftLocation);
                        }
                        else if (iLeft != null)
                        {
                            e.Graphics.DrawImage(iLeft, iLeftLocation);
                        }
                    }
                    else
                    {
                        if (iLeft != null)
                        {
                            e.Graphics.DrawImage(iLeft, iLeftLocation);
                        }
                    }
                }
            }

            if (iControlMiddleEnabled)
            {
                if (iState)
                {
                    if (iHitMiddle)
                    {
                        if (iMiddleTouchState2 != null)
                        {
                            e.Graphics.DrawImage(iMiddleTouchState2, iMiddleTouchLocationState2);
                        }
                    }
                    else
                    {
                        if (iMouseOverMiddle)
                        {
                            if (iMiddleState2 != null)
                            {
                                e.Graphics.DrawImage(iMiddleState2, iMiddleLocationState2);
                            }
                            if (iMiddleMouseOverState2 != null)
                            {
                                ImageAttributes ia = new ImageAttributes();
                                ColorMatrix mx = new ColorMatrix();
                                mx.Matrix11 = mx.Matrix22 = mx.Matrix44 = 1.0f;
                                mx.Matrix33 = iAlphaMiddle;
                                ia.SetColorMatrix(mx);

                                e.Graphics.DrawImage(iMiddleMouseOverState2, new Rectangle(iMiddleLocationState2.X, iMiddleLocationState2.Y, iMiddleMouseOverState2.Width, iMiddleMouseOverState2.Height),
                                    0, 0, iMiddleMouseOverState2.Width, iMiddleMouseOverState2.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            if (iMiddleState2 != null)
                            {
                                e.Graphics.DrawImage(iMiddleState2, iMiddleLocationState2);
                            }
                        }
                    }
                }
                else
                {
                    if (iHitMiddle)
                    {
                        if (iMiddleTouchState1 != null)
                        {
                            e.Graphics.DrawImage(iMiddleTouchState1, iMiddleTouchLocationState1);
                        }
                    }
                    else
                    {
                        if (iMouseOverMiddle)
                        {
                            if (iMiddleState1 != null)
                            {
                                e.Graphics.DrawImage(iMiddleState1, iMiddleLocationState1);
                            }
                            if (iMiddleMouseOverState1 != null)
                            {
                                ImageAttributes ia = new ImageAttributes();
                                ColorMatrix mx = new ColorMatrix();
                                mx.Matrix11 = mx.Matrix22 = mx.Matrix44 = 1.0f;
                                mx.Matrix33 = iAlphaMiddle;
                                ia.SetColorMatrix(mx);

                                e.Graphics.DrawImage(iMiddleMouseOverState1, new Rectangle(iMiddleLocationState1.X, iMiddleLocationState1.Y, iMiddleMouseOverState1.Width, iMiddleMouseOverState1.Height),
                                    0, 0, iMiddleMouseOverState1.Width, iMiddleMouseOverState1.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            if (iMiddleState1 != null)
                            {
                                e.Graphics.DrawImage(iMiddleState1, iMiddleLocationState1);
                            }
                        }
                    }
                }
            }

            if (iControlRightEnabled)
            {
                if (iHitRight)
                {
                    if (iRightTouch != null)
                    {
                        e.Graphics.DrawImage(iRightTouch, iRightTouchLocation);
                    }
                }
                else
                {
                    if (iMouseOverRight)
                    {
                        if (iRightMouseOver != null)
                        {
                            e.Graphics.DrawImage(iRightMouseOver, iRightLocation);
                        }
                        else if (iRight != null)
                        {
                            e.Graphics.DrawImage(iRight, iRightLocation);
                        }
                    }
                    else
                    {
                        if (iRight != null)
                        {
                            e.Graphics.DrawImage(iRight, iRightLocation);
                        }
                    }
                }
            }
        }

        private void PaintModePlaylist(PaintEventArgs e)
        {
            if (iPlaylistLeftEnabled)
            {
                if (iMouseOverLeft)
                {
                    if (iPlayLaterMouseOver != null)
                    {
                        e.Graphics.DrawImage(iPlayLaterMouseOver, iLeftLocation);
                    }
                }
                else
                {
                    if (iPlayLater != null)
                    {
                        e.Graphics.DrawImage(iPlayLater, iLeftLocation);
                    }
                }
            }

            if (iPlaylistMiddleEnabled)
            {
                if (iMouseOverMiddle)
                {
                    if (iPlayNow != null)
                    {
                        e.Graphics.DrawImage(iPlayNow, iMiddleLocationState1);
                    }
                    if (iPlayNowMouseOver != null)
                    {
                        ImageAttributes ia = new ImageAttributes();
                        ColorMatrix mx = new ColorMatrix();
                        mx.Matrix11 = mx.Matrix22 = mx.Matrix44 = 1.0f;
                        mx.Matrix33 = iAlphaMiddle;
                        ia.SetColorMatrix(mx);

                        e.Graphics.DrawImage(iPlayNowMouseOver, new Rectangle(iMiddleLocationState1.X, iMiddleLocationState1.Y, iPlayNowMouseOver.Width, iPlayNowMouseOver.Height),
                            0, 0, iPlayNowMouseOver.Width, iPlayNowMouseOver.Height, GraphicsUnit.Pixel, ia);
                    }
                }
                else
                {
                    if (iPlayNow != null)
                    {
                        e.Graphics.DrawImage(iPlayNow, iMiddleLocationState1);
                    }
                }
            }

            if (iPlaylistRightEnabled)
            {
                if (iMouseOverRight)
                {
                    if (iPlayNextMouseOver != null)
                    {
                        e.Graphics.DrawImage(iPlayNextMouseOver, iRightLocation);
                    }
                }
                else
                {
                    if (iPlayNext != null)
                    {
                        e.Graphics.DrawImage(iPlayNext, iRightLocation);
                    }
                }
            }
        }
    }
}
