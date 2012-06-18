using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace KinskyPda
{
    public class ImageButtonFactory
    {
        public static ImageButton CreateLeft(Bitmap aPushed, Bitmap aUnpushed)
        {
            ImageButton button = new ImageButton(TextureManager.Instance.BlankControl,
                aPushed,
                aUnpushed,
                LayoutManager.Instance.ButtonLeftBackgroundBounds,
                LayoutManager.Instance.ButtonLeftBounds,
                LayoutManager.Instance.ButtonLeftHitBounds);
            return button;
        }

        public static ImageButton CreateCentre(Bitmap aPushed, Bitmap aUnpushed)
        {
            ImageButton button = new ImageButton(TextureManager.Instance.BlankControl,
                aPushed,
                aUnpushed,
                LayoutManager.Instance.ButtonCentreBackgroundBounds,
                LayoutManager.Instance.ButtonCentreBounds,
                LayoutManager.Instance.ButtonCentreHitBounds);
            return button;
        }

        public static ImageButton CreateRight(Bitmap aPushed, Bitmap aUnpushed)
        {
            ImageButton button = new ImageButton(TextureManager.Instance.BlankControl,
                aPushed,
                aUnpushed,
                LayoutManager.Instance.ButtonRightBackgroundBounds,
                LayoutManager.Instance.ButtonRightBounds,
                LayoutManager.Instance.ButtonRightHitBounds);
            return button;
        }
    }


    public class ImageButton : Control
    {
        public ImageButton(Bitmap aBitmapBkgd, Bitmap aBitmapPushed, Bitmap aBitmapUnpushed,
                            Rectangle aBkgdRect, Rectangle aRect, Rectangle aHitRect)
        {
            iBitmapBkgd = aBitmapBkgd;
            iBitmapPushed = aBitmapPushed;
            iBitmapUnpushed = aBitmapUnpushed;
            iBkgdRect = aBkgdRect;
            iRect = aRect;
            iHitRect = aHitRect;

            Rectangle bounds = iHitRect;
            bounds.Offset(iRect.X, iRect.Y);
            Bounds = bounds;
            iPushed = false;

            // need to scale these rects here because autoscaling is used for all controls
            iBkgdRect = Linn.Kinsky.DisplayManager.ScaleRectangle(iBkgdRect);
            iRect = Linn.Kinsky.DisplayManager.ScaleRectangle(iRect);
            iHitRect = Linn.Kinsky.DisplayManager.ScaleRectangle(iHitRect);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // draw the parts of the button that exist outside the control - draw them into its parent
            Graphics g = Parent.CreateGraphics();
            if (Enabled && iPushed)
            {
                g.DrawImage(iBitmapPushed, iRect, new Rectangle(0, 0, iBitmapPushed.Width, iBitmapPushed.Height), GraphicsUnit.Pixel);
            }
            else
            {
                g.DrawImage(iBitmapBkgd, iRect, iBkgdRect, GraphicsUnit.Pixel);
            }
            g.Dispose();

            // draw the control
            if (Enabled)
            {
                if (iPushed)
                {
                    e.Graphics.DrawImage(iBitmapPushed, 0, 0, iHitRect, GraphicsUnit.Pixel);
                }
                else
                {
                    e.Graphics.DrawImage(iBitmapUnpushed, 0, 0, iHitRect, GraphicsUnit.Pixel);
                }
            }
            else
            {
                Rectangle bkgdRect = iHitRect;
                bkgdRect.Offset(iBkgdRect.X, iBkgdRect.Y);
                e.Graphics.DrawImage(iBitmapBkgd, 0, 0, bkgdRect, GraphicsUnit.Pixel);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            iPushed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            iPushed = false;
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        private Bitmap iBitmapBkgd;
        private Bitmap iBitmapPushed;
        private Bitmap iBitmapUnpushed;
        private Rectangle iBkgdRect;
        private Rectangle iRect;
        private Rectangle iHitRect;
        private bool iPushed;
    }
}
