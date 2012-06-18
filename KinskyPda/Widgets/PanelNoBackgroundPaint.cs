using System.Drawing;

using System.Windows.Forms;

namespace KinskyPda
{
    public class PanelNoBackgroundPaint : Panel
    {     
        protected Image iBackgroundImage;

        public Image BackgroundImage
        {
            get
            {
                return iBackgroundImage;
            }
            set
            {
                iBackgroundImage = value;
            }
        }

        protected virtual void PaintBackground(PaintEventArgs e) 
        {
            if (iBackgroundImage != null)
            {
                if (Size != iBackgroundImage.Size)
                {
                    e.Graphics.Clear(BackColor);
                }
                e.Graphics.DrawImage(iBackgroundImage, e.ClipRectangle.X, e.ClipRectangle.Y);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            PaintBackground(e);
        }
    }
}
