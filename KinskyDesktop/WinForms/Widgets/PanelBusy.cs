using System.Windows.Forms;
using System.Drawing;
using System.Threading;

using Linn;
using Linn.Kinsky;

namespace KinskyDesktop.Widgets
{
    public class PanelBusy : Panel
    {
        private const uint kUpdateRate = 20;
        private const float kRevolutionsPerSecond = 1.5f;

        private TextureBrush iTextureBrush;
        private Brush iBrushForeColour;

        private Image kImageWaiting = Linn.Kinsky.Properties.Resources.BusyIcon;
        private Image kImageWaitingElement = Linn.Kinsky.Properties.Resources.BusyIconElement;

        private System.Threading.Timer iTimer;
        private Ticker iTicker;
        private bool iWaiting;

        private float iAngle;
        private float iAngleRemainder;
        private string iMessage;

        public PanelBusy()
        {
            iTicker = new Ticker();
            iWaiting = false;

            iTimer = new System.Threading.Timer(TimerElapsed);
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);

            iTextureBrush = new TextureBrush(kImageWaitingElement);
            iBrushForeColour = new SolidBrush(ForeColor);

            DoubleBuffered = true;
        }

        public void StartBusy()
        {
            iAngle = 0.0f;
            iMessage = string.Empty;
            iWaiting = true;
            iTimer.Change(kUpdateRate, kUpdateRate);
            Invalidate();
        }

        public void SetMessage(string aMessage)
        {
            iMessage = aMessage;
            Invalidate();
        }

        public void StopBusy()
        {
            iWaiting = false;
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Invalidate();
        }

        public override Color ForeColor
        {
            set
            {
                if (iBrushForeColour != null)
                {
                    iBrushForeColour.Dispose();
                }
                iBrushForeColour = new SolidBrush(value);
                base.ForeColor = value;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (iWaiting)
            {
                int x = (int)((ClientRectangle.Width - kImageWaiting.Width) * 0.5f);
                int y = (int)((ClientRectangle.Height - kImageWaiting.Height) * 0.5f);
                e.Graphics.DrawImage(kImageWaiting, x, y);

                iTextureBrush.ResetTransform();
                iTextureBrush.TranslateTransform(x, y);
                iTextureBrush.TranslateTransform(kImageWaiting.Width * 0.5f, kImageWaiting.Height * 0.5f);
                iTextureBrush.RotateTransform(iAngle);
                iTextureBrush.TranslateTransform(-kImageWaiting.Width * 0.5f, -kImageWaiting.Height * 0.5f);
                e.Graphics.FillRectangle(iTextureBrush, x, y, kImageWaiting.Width, kImageWaiting.Height);

                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;

                e.Graphics.DrawString(iMessage, Font, iBrushForeColour, new Rectangle(0, y + 75, ClientSize.Width, Font.Height), format);
            }
        }

        private void TimerElapsed(object sender)
        {
            iAngleRemainder += ((360 * kRevolutionsPerSecond) / 1000.0f) * iTicker.MilliSeconds;
            float angle = ((int)(iAngleRemainder / 45.0f)) * 45.0f;
            iAngleRemainder -= angle;
            iAngle += angle;
            if (iAngle > 360.0f)
            {
                iAngle -= 360.0f;
            }
            iTicker.Reset();

            Invalidate();
        }
    }
}
