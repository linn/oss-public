using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

using Linn.Kinsky;

namespace KinskyDesktop
{
    public partial class FormArtwork : FormThemed
    {
        private Image iArtwork;

        public FormArtwork()
        {
            InitializeComponent();
        }

        private delegate void DSetArtwork(string aTitle, Image aArtwork);
        public void SetArtwork(string aTitle, Image aArtwork)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new DSetArtwork(SetArtwork), aTitle, aArtwork);
                return;
            }

            Text = aTitle;
            iArtwork = aArtwork;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (iArtwork != null)
            {
                if (iArtwork.Width > iArtwork.Height)
                {
                    Size = new Size(iArtwork.Width + (Width - ClientRectangle.Width), iArtwork.Width + (Height - ClientRectangle.Height));
                }
                else
                {
                    Size = new Size(iArtwork.Height + (Width - ClientRectangle.Width), iArtwork.Height + (Height - ClientRectangle.Height));
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            Hide();
        }

        /*protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (Width > Height)
            {
                Height = Width;
            }
            else
            {
                Width = Height;
            }

            Invalidate();
        }*/

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (iArtwork != null)
            {
                Size size = ImageSize(iArtwork, ClientRectangle.Height);
                e.Graphics.DrawImage(iArtwork,
                    new Rectangle((int)(ClientRectangle.X + ((ClientRectangle.Width - size.Width) * 0.5f)), (int)(ClientRectangle.Y + ((ClientRectangle.Height - size.Height) * 0.5f)), size.Width, size.Height),
                    new Rectangle(0, 0, iArtwork.Width, iArtwork.Height), GraphicsUnit.Pixel);
            }
        }

        private Size ImageSize(Image aImage, int aSize)
        {
            Size size = new Size(aSize, aSize);
            if (aImage.Width > aImage.Height)
            {
                size.Height = (int)(aSize * (aImage.Height / (float)aImage.Width));
            }
            else if (aImage.Height > aImage.Width)
            {
                size.Width = (int)(aSize * (aImage.Width / (float)aImage.Height));
            }
            return size;
        }
    }
}
