using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;

using Linn;
using Linn.Kinsky;

namespace KinskyPda.Widgets
{
    public class WidgetPlaylistAux : PanelNoBackgroundPaint, IViewWidgetPlaylistAux
    {
        public WidgetPlaylistAux()
        {
            iBackgroundImage = TextureManager.Instance.Background;
            iAuxSourceImage = TextureManager.Instance.AuxSource;
        }

        public delegate void DEventHandler();
        public event DEventHandler EventOpen;
        public event DEventHandler EventClose;

        private delegate void DOpen(string aType);
        public void Open(string aType)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DOpen(Open), new object[] { aType });
            }
            else
            {
                if (EventOpen != null)
                    EventOpen();
            }
        }

        private delegate void DClose();
        public void Close()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DClose(Close));
            }
            else
            {
                if (EventClose != null)
                    EventClose();
            }
        }

        protected override void PaintBackground(PaintEventArgs e)
        {
            if (iBackgroundImage != null)
            {
                SizeF scale = DisplayManager.ScaleFactorOriented;
                float ratioW = scale.Width / scale.Height;

                float ratioH = 1.0f;
                if (ratioW > 1.0f)
                {
                    ratioH = 1.0f / ratioW;
                    ratioW = 1.0f;
                }
                e.Graphics.DrawImage(iBackgroundImage,
                    new Rectangle(0, 0, ClientSize.Width, ClientSize.Height),
                    new Rectangle(0, 0, (int)(iBackgroundImage.Width * ratioW), (int)(iBackgroundImage.Height * ratioH)),
                    GraphicsUnit.Pixel);

                Rectangle rect;
                Screen screen = Screen.PrimaryScreen;
                if (screen.Bounds.Width > screen.Bounds.Height)
                {
                    rect = DisplayManager.ScaleRectangle(30, 30, 228, 228);
                }
                else
                {
                    rect = DisplayManager.ScaleRectangle(90, 30, 300, 300);
                }
                e.Graphics.DrawImage(iAuxSourceImage, rect, new Rectangle(0, 0, iAuxSourceImage.Width, iAuxSourceImage.Height), GraphicsUnit.Pixel);
            }
        }

        private Image iAuxSourceImage;
    }
}
