using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Linn.Kinsky;

namespace KinskyPda.Widgets
{
    public abstract class WidgetTransport : TransparentControlBase, IViewWidgetTransportControl
    {
        public abstract event EventHandler<EventArgs> EventStop;
        public abstract event EventHandler<EventArgs> EventPause;

        public event EventHandler<EventArgs> EventPlay;
        public event EventHandler<EventArgs> EventPrevious;
        public event EventHandler<EventArgs> EventNext;

        public event EventHandler<EventArgsPlay> EventPlayNow { add { } remove { } }
        public event EventHandler<EventArgsPlay> EventPlayNext { add { } remove { } }
        public event EventHandler<EventArgsPlay> EventPlayLater { add { } remove { } }

        public WidgetTransport(Bitmap aNonPlayPushed, Bitmap aNonPlayUnpushed)
        {
            iButtonPrev = ImageButtonFactory.CreateLeft(TextureManager.Instance.SkipBackTouch,
                                                        TextureManager.Instance.SkipBack);
            iButtonPrev.Click += ButtonPrevClick;

            iButtonPlay = ImageButtonFactory.CreateCentre(TextureManager.Instance.PlayTouch,
                                                          TextureManager.Instance.Play);
            iButtonPlay.Click += ButtonPlayClick;

            iButtonNext = ImageButtonFactory.CreateRight(TextureManager.Instance.SkipForwardTouch,
                                                         TextureManager.Instance.SkipForward);
            iButtonNext.Click += ButtonNextClick;

            iButtonNonPlay = ImageButtonFactory.CreateCentre(aNonPlayPushed, aNonPlayUnpushed);
            iButtonNonPlay.Click += ButtonNonPlayClick;

            this.SuspendLayout();

            iButtonNonPlay.Parent = this;
            iButtonPrev.Parent = this;
            iButtonPlay.Parent = this;
            iButtonNext.Parent = this;

            this.Visible = false;
            this.ResumeLayout(false);
        }

        private delegate void OpenDelegate();
        public void Open()
        {
            if (InvokeRequired)
            {
                BeginInvoke((OpenDelegate)Open);
            }
            else
            {
                iButtonNext.Visible = true;
                iButtonPrev.Visible = true;
            }
        }

        private delegate void CloseDelegate();
        public void Close()
        {
            if (InvokeRequired)
            {
                BeginInvoke((CloseDelegate)Close);
            }
            else
            {
                Visible = false;
            }
        }

        private delegate void InitialisedDelegate();
        public void Initialised()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((InitialisedDelegate)Initialised);
            }
            else
            {
                Visible = true;
            }
        }

        public void SetPlayNowEnabled(bool aEnabled) { }
        public void SetPlayNextEnabled(bool aEnabled) { }
        public void SetPlayLaterEnabled(bool aEnabled) { }

        public void SetDragging(bool aDragging) { }

        private delegate void SetTransportStateDelegate(ETransportState aTransportState);
        public void SetTransportState(ETransportState aTransportState)
        {
            if (InvokeRequired)
            {
                BeginInvoke((SetTransportStateDelegate)SetTransportState, aTransportState);
            }
            else
            {
                switch (aTransportState)
                {
                    case ETransportState.eBuffering:
                    case ETransportState.ePlaying:
                        iButtonNonPlay.Visible = true;
                        iButtonPlay.Visible = false;
                        break;

                    default:
                        iButtonNonPlay.Visible = false;
                        iButtonPlay.Visible = true;
                        break;
                }
            }
        }

        public void SetDuration(uint aDuration)
        {
        }

        private delegate void DSetAllowSkipping(bool aAllowSkipping);
        public void SetAllowSkipping(bool aAllowSkipping)
        {
            if (InvokeRequired)
            {
                BeginInvoke((DSetAllowSkipping)SetAllowSkipping, aAllowSkipping);
            }
            else
            {
                iButtonNext.Enabled = aAllowSkipping;
                iButtonPrev.Enabled = aAllowSkipping;
            }
        }

        public void SetAllowPausing(bool aAllowPausing) { }
        private void ButtonPrevClick(object sender, EventArgs e)
        {
            if (EventPrevious != null)
            {
                EventPrevious(this, EventArgs.Empty);
            }
        }

        private void ButtonNextClick(object sender, EventArgs e)
        {
            if (EventNext != null)
            {
                EventNext(this, EventArgs.Empty);
            }
        }

        private void ButtonPlayClick(object sender, EventArgs e)
        {
            if (EventPlay != null)
            {
                EventPlay(this, EventArgs.Empty);
            }
        }

        protected abstract void ButtonNonPlayClick(object sender, EventArgs e);

        private ImageButton iButtonPrev;
        private ImageButton iButtonNext;
        private ImageButton iButtonPlay;
        protected ImageButton iButtonNonPlay;
    }


    public class WidgetTransportPause : WidgetTransport
    {
        public override event EventHandler<EventArgs> EventPause;
        public override event EventHandler<EventArgs> EventStop { add { } remove { } }

        public WidgetTransportPause()
            : base(TextureManager.Instance.PauseTouch, TextureManager.Instance.Pause)
        {
        }

        protected override void ButtonNonPlayClick(object sender, EventArgs e)
        {
            if (EventPause != null)
            {
                EventPause(this, EventArgs.Empty);
            }
        }
    }


    public class WidgetTransportStop : WidgetTransport
    {
        public override event EventHandler<EventArgs> EventPause { add { } remove { } }
        public override event EventHandler<EventArgs> EventStop;

        public WidgetTransportStop()
            : base(TextureManager.Instance.StopTouch, TextureManager.Instance.Stop)
        {
        }

        protected override void ButtonNonPlayClick(object sender, EventArgs e)
        {
            if (EventStop != null)
            {
                EventStop(this, EventArgs.Empty);
            }
        }
    }
}
