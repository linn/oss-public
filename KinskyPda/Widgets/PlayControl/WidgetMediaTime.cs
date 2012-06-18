using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Upnp;
using Linn.Kinsky;

namespace KinskyPda.Widgets
{
    public class WidgetMediaTime : TransparentLabel, IViewWidgetMediaTime
    {
        public WidgetMediaTime()
        {
            this.SuspendLayout();
            this.Font = new Font("Tahoma", 7, FontStyle.Bold);
            this.ForeColor = Color.FromArgb(0, 66, 88);
            this.ResumeLayout(false);

            iShowElapsedTime = true;
            TextAlign = ContentAlignment.TopCenter;
        }

        public void Open()
        {
        }

        private delegate void CloseDelegate();
        public void Close()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new CloseDelegate(Close), new object[] { });
            }
            else
            {
                Visible = false;
            }
        }

        private delegate void InitialisedDelegate();
        public void Initialised()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new InitialisedDelegate(Initialised), new object[] { });
            }
            else
            {
                Visible = true;
            }
        }

        private delegate void SetAllowSeekingDelegate(bool aAllowSeeking);
        public void SetAllowSeeking(bool aAllowSeeking)
        {
        }

        private delegate void SetTransportStateDelegate(ETransportState aTransportState);
        public void SetTransportState(ETransportState aTransportState)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetTransportStateDelegate(SetTransportState), new object[] { aTransportState });
            }
            else
            {
                iTransportState = aTransportState;

                if (aTransportState == ETransportState.eBuffering)
                {
                    Text = "...";
                }
                else if (aTransportState == ETransportState.eStopped)
                {
                    Text = string.Empty;
                }
            }
        }

        private delegate void SetDurationDelegate(uint aDuration);
        public void SetDuration(uint aDuration)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetDurationDelegate(SetDuration), new object[] { aDuration });
            }
            else
            {
                iDuration = aDuration;
            }
        }

        private delegate void SetSecondsDelegate(uint aSeconds);
        public void SetSeconds(uint aSeconds)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetSecondsDelegate(SetSeconds), new object[] { aSeconds });
            }
            else
            {
                if (iTransportState == ETransportState.ePlaying || iTransportState == ETransportState.ePaused)
                {
                    Time time = new Time((int)aSeconds);

                    if (iShowElapsedTime && iDuration > 0)
                    {
                        Text = new Time((int)aSeconds).ToPrettyString();
                    }
                    else
                    {
                        Text = new Time((int)aSeconds - (int)iDuration).ToPrettyString();
                    }
                }
            }
        }

        public event EventHandler<EventArgsSeekSeconds> EventSeekSeconds { add { } remove { } }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (iDuration > 0)
            {
                iShowElapsedTime = !iShowElapsedTime;
                Invalidate();
            }
        }

        private ETransportState iTransportState;
        private bool iShowElapsedTime;
        private uint iDuration;
    }
}
