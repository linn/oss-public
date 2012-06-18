using Android.Content;
using Android.Views;
using Android.Graphics;
using Android.Runtime;
using Android.Widget;
using Android.Graphics.Drawables;
using Linn;
using System;

namespace OssToolkitDroid
{

    public class Popup
    {

        public Popup(Context aContext, View aViewRoot, View aAnchor, int aDesiredWidth, Paint aStroke, Paint aFill)
        {
            iContext = aContext;
            iViewRoot = aViewRoot;
            iAnchor = aAnchor;
            iDesiredWidth = aDesiredWidth;
            iStroke = aStroke;
            iFill = aFill;
        }

        public event EventHandler<EventArgs> EventDismissed;

        public void Show()
        {
            IWindowManager windowManager = iContext.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            int screenWidth = windowManager.DefaultDisplay.Width - (kPadding * 2);
            int screenHeight = windowManager.DefaultDisplay.Height - (kPadding * 2);

            int width = iDesiredWidth + (kPadding * 2);
            if (width > screenWidth)
            {
                width = screenWidth;
            }

            int[] location = new int[2];
            iAnchor.GetLocationOnScreen(location);

            PopupFrame frame = new PopupFrame(iContext, kPadding, kAnchorHeight, kAnchorWidth, kCornerRadius, iStroke, iFill);
            frame.AddView(iViewRoot);

            Rect anchorRect = new Rect(location[0], location[1], location[0] + iAnchor.Width, location[1] + iAnchor.Height);


            int xPos = (anchorRect.Left + ((anchorRect.Right - anchorRect.Left) / 2)) - (width / 2);

            int anchorOffset = xPos < kPadding ? kPadding - xPos : xPos + width > screenWidth - kPadding ? screenWidth - (xPos + width) + kPadding : 0;
            xPos += anchorOffset;

            int anchorCenterY = anchorRect.Top + ((anchorRect.Bottom - anchorRect.Top) / 2);

            int height = anchorRect.Top - kPadding;
            int yPos = kPadding;
            bool anchorTop = false;
            if (anchorCenterY <= screenHeight / 2)
            {
                height = screenHeight + kPadding - anchorRect.Bottom;
                yPos = anchorRect.Bottom;
                anchorTop = true;
            }

            frame.AnchorOffset = anchorOffset;
            frame.AnchorOnTop = anchorTop;

            //setAnimationStyle(screenWidth, anchorRect.centerX(), onTop);

            iPopup = new PopupWindow(frame);
            iPopup.SetBackgroundDrawable(new BitmapDrawable());
            iPopup.Width = width;
            iPopup.Height = height;
            iPopup.Touchable = true;
            iPopup.Focusable = true;
            iPopup.OutsideTouchable = true;
            iPopup.DismissEvent += iPopup_DismissEvent;
            iPopup.ShowAtLocation(iAnchor, (int)Android.Views.GravityFlags.NoGravity, xPos, yPos);
        }

        private void iPopup_DismissEvent(object sender, EventArgs e)
        {
            OnEventDismissed(this, EventArgs.Empty);
        }

        private void OnEventDismissed(object sender, EventArgs e)
        {
            EventHandler<EventArgs> dismissed = this.EventDismissed;
            if (dismissed != null)
            {
                dismissed(sender, e);
            }
        }

        public void Dismiss()
        {
            Assert.Check(iPopup != null);
            iPopup.DismissEvent -= iPopup_DismissEvent;
            iPopup.Dismiss();
        }

        private PopupWindow iPopup;
        private Context iContext;
        private View iViewRoot;
        private View iAnchor;
        private const int kAnchorHeight = 20;
        private const int kAnchorWidth = 10;
        private const int kPadding = 10;
        private const int kCornerRadius = 25;
        private Paint iStroke;
        private Paint iFill;
        private int iDesiredWidth;
    }

    public class PopupFrame : LinearLayout
    {
        public PopupFrame(Context aContext, int aPadding, int aAnchorHeight, int aAnchorWidth, int aCornerRadius, Paint aStroke, Paint aFill)
            : base(aContext)
        {
            iPadding = aPadding;
            iAnchorHeight = aAnchorHeight;
            iAnchorWidth = aAnchorWidth;
            iCornerRadius = aCornerRadius;
            iStroke = aStroke;
            iFill = aFill;
            SetWillNotDraw(false);
            this.SetPadding(iPadding, iPadding + iAnchorHeight, iPadding, iPadding);
        }

        public bool AnchorOnTop
        {
            set
            {
                iAnchorOnTop = value;
                if (value)
                {
                    this.SetPadding(iPadding, iPadding + iAnchorHeight, iPadding, iPadding);
                }
                else
                {
                    this.SetPadding(iPadding, iPadding, iPadding, iPadding + iAnchorHeight);
                }
                Invalidate();
            }
        }

        public int AnchorOffset
        {
            set
            {
                iAnchorOffset = value;
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            int left = 0;
            int right = canvas.Width - (int)iStroke.StrokeWidth;
            int top = iAnchorOnTop ? iAnchorHeight : 0;
            int bottom = iAnchorOnTop ? canvas.Height - (int)iStroke.StrokeWidth : canvas.Height - iAnchorHeight - (int)iStroke.StrokeWidth;
            int anchorApex = (int)((right - left) * 0.5) - iAnchorOffset;

            Path path = new Path();
            path.MoveTo(left, bottom - iCornerRadius);
            path.LineTo(left, top + iCornerRadius);
            path.ArcTo(new RectF(new Rect(left, top, left + iCornerRadius, top + iCornerRadius)), 180, 90);

            if (iAnchorOnTop)
            {
                path.LineTo((float)(anchorApex - iAnchorWidth * 0.5), top);
                path.LineTo(anchorApex, top - iAnchorHeight);
                path.LineTo((float)(anchorApex + iAnchorWidth * 0.5), top);
            }

            path.LineTo(right - iCornerRadius, top);
            path.ArcTo(new RectF(new Rect(right - iCornerRadius, top, right, top + iCornerRadius)), 270, 90);
            path.LineTo(right, bottom - iCornerRadius);
            path.ArcTo(new RectF(new Rect(right - iCornerRadius, bottom - iCornerRadius, right, bottom)), 0, 90);
            if (!iAnchorOnTop)
            {
                path.LineTo((float)(anchorApex + iAnchorWidth * 0.5), bottom);
                path.LineTo(anchorApex, bottom + iAnchorHeight);
                path.LineTo((float)(anchorApex - iAnchorWidth * 0.5), bottom);
            }
            path.LineTo(left + iCornerRadius, bottom);
            path.ArcTo(new RectF(new Rect(left, bottom - iCornerRadius, left + iCornerRadius, bottom)), 90, 90);
            path.Close();

            canvas.DrawPath(path, iFill);
            canvas.DrawPath(path, iStroke);
        }

        private int iAnchorOffset;
        private bool iAnchorOnTop;
        private int iPadding;
        private int iAnchorHeight;
        private int iAnchorWidth;
        private int iCornerRadius;
        private Paint iStroke;
        private Paint iFill;
    }
}