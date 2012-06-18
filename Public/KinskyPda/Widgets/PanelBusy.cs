
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

using Linn;


namespace KinskyPda.Widgets
{
    internal class PanelBusy : Panel
    {
        public PanelBusy()
        {
            iFont = ViewSupport.Instance.FontMedium;
            iBrush = new SolidBrush(ViewSupport.Instance.ForeColourBright);

            iTimer = new System.Windows.Forms.Timer();
            iTimer.Interval = iTickInvervalMs;
            iTimer.Tick += TimerElapsed;

            iTicker = new Ticker();
            iMessage = "";
            iImages = new List<Image>();
            iImages.Add(TextureManager.Instance.Waiting1);
            iImages.Add(TextureManager.Instance.Waiting2);
            iImages.Add(TextureManager.Instance.Waiting3);
            iImages.Add(TextureManager.Instance.Waiting4);
            iImages.Add(TextureManager.Instance.Waiting5);
            iImages.Add(TextureManager.Instance.Waiting6);
            iImages.Add(TextureManager.Instance.Waiting7);
            iImages.Add(TextureManager.Instance.Waiting8);

            this.BackColor = ViewSupport.Instance.BackColour;
            Visible = false;
        }

        public Size MinimumSize
        {
            get
            {
                Graphics g = this.CreateGraphics();
                SizeF sz = g.MeasureString("temp", iFont);
                g.Dispose();
                return new Size(iImages[0].Width, iImages[0].Height + 2*sz.ToSize().Height);
            }
        }

        public void StartBusy(string aMessage)
        {
            iMessage = aMessage;
            iTicker.Reset();
            Visible = true;
            iTimer.Enabled = true;
            Invalidate();
        }

        public void SetMessage(string aMessage)
        {
            iMessage = aMessage;
        }

        private delegate void DForceSetMessage(string aMessage);
        public void ForceSetMessage(string aMessage)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DForceSetMessage(ForceSetMessage), new object[] { aMessage });
            }
            else
            {
                iMessage = aMessage;
                TimerElapsed(this, EventArgs.Empty);
            }
        }

        public void StopBusy()
        {
            iTimer.Enabled = false;
            Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // get the milliseconds elapsed
            int msecs = (int)iTicker.MilliSeconds;
            int tickCount = msecs / iTickInvervalMs;
            int imageIndex = tickCount % iImages.Count;

            // draw the image
            Image image = iImages[imageIndex];
            int imageX = (Width - image.Width)/2;
            int imageY = 0;
            e.Graphics.DrawImage(image, imageX, imageY);

            // draw the message
            string msg = iMessage;
            SizeF textSize = e.Graphics.MeasureString(msg, iFont);
            Rectangle textRect = new Rectangle();
            textRect.X = 0;
            textRect.Y = imageY + image.Height;
            textRect.Width = Width;
            textRect.Height = (int)(textSize.Height + 1);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(msg, iFont, iBrush, textRect, format);
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            Invalidate();
        }

        private const int iTickInvervalMs = 250;
        private Font iFont;
        private Brush iBrush;
        private System.Windows.Forms.Timer iTimer;
        private Ticker iTicker;
        private string iMessage;
        private List<Image> iImages;
    }
}


