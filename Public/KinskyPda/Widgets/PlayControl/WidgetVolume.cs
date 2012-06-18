using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KinskyPda;

using Linn;
using Linn.Kinsky;

namespace KinskyPda.Widgets
{
    public class WidgetVolume : KinskyPda.PanelNoBackgroundPaint, IViewWidgetVolumeControl
    {
        public event EventHandler<EventArgs> EventVolumeIncrement;
        public event EventHandler<EventArgs> EventVolumeDecrement;
        public event EventHandler<EventArgsMute> EventMuteChanged;
        public event EventHandler<EventArgsVolume> EventVolumeChanged { add { } remove { } }

        public WidgetMediaTime ViewWidgetMediaTime 
        {
            get
            {
                return iWidgetMediaTime;
            }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlPause
        {
            get
            {
                return iPnlTransportControlPause;
            }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlStop
        {
            get
            {
                return iPnlTransportControlStop;
            }
        }

        public IViewWidgetVolumeControl ViewWidgetVolumeControl
        {
            get
            {
                return this;
            }
        }

        public WidgetVolume()
        {
            this.BackgroundImage = TextureManager.Instance.BlankControl;
            this.Bounds = LayoutManager.Instance.BottomBounds;

            iPnlTransportControlPause = new WidgetTransportPause();
            iPnlTransportControlPause.Bounds = LayoutManager.Instance.PanelToolsBounds;

            iPnlTransportControlStop = new WidgetTransportStop();
            iPnlTransportControlStop.Bounds = LayoutManager.Instance.PanelToolsBounds;

            iButtonVolumeDown = new ImageButton(TextureManager.Instance.BlankControl,
                                                TextureManager.Instance.VolumeDownTouch,
                                                TextureManager.Instance.VolumeDown,
                                                LayoutManager.Instance.ButtonVolumeDownBounds,
                                                LayoutManager.Instance.ButtonVolumeDownBounds,
                                                LayoutManager.Instance.ButtonVolumeDownHitBounds);
            iButtonVolumeDown.Visible = false;
            iButtonVolumeDown.Click += ButtonVolumeDownClick;

            iButtonVolumeUp = new ImageButton(TextureManager.Instance.BlankControl,
                                              TextureManager.Instance.VolumeUpTouch,
                                              TextureManager.Instance.VolumeUp,
                                              LayoutManager.Instance.ButtonVolumeUpBounds,
                                              LayoutManager.Instance.ButtonVolumeUpBounds,
                                              LayoutManager.Instance.ButtonVolumeUpHitBounds);
            iButtonVolumeUp.Visible = false;
            iButtonVolumeUp.Click += ButtonVolumeUpClick;

            iWidgetMediaTime = new WidgetMediaTime();
            iWidgetMediaTime.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Bold);
            iWidgetMediaTime.ForeColor = Color.White;
            iWidgetMediaTime.Visible = false;
            iWidgetMediaTime.Bounds = new Rectangle(LayoutManager.Instance.ButtonVolumeDownBounds.X,
                                                    LayoutManager.Instance.ButtonVolumeDownBounds.Bottom - 5,
                                                    LayoutManager.Instance.ButtonVolumeDownBounds.Width,
                                                    LayoutManager.Instance.BottomBounds.Bottom - LayoutManager.Instance.ButtonVolumeDownBounds.Bottom - 5);

            iLabelVolume = new TransparentLabel();
            iLabelVolume.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Bold);
            iLabelVolume.ForeColor = Color.White;
            iLabelVolume.TextAlign = ContentAlignment.TopCenter;
            iLabelVolume.Visible = false;
            iLabelVolume.Click += LabelVolumeClick;
            iLabelVolume.Bounds = new Rectangle(LayoutManager.Instance.ButtonVolumeUpBounds.X,
                                                LayoutManager.Instance.ButtonVolumeUpBounds.Bottom - 5,
                                                LayoutManager.Instance.ButtonVolumeUpBounds.Width,
                                                LayoutManager.Instance.BottomBounds.Bottom - LayoutManager.Instance.ButtonVolumeUpBounds.Bottom - 5);

            this.SuspendLayout();

            iButtonVolumeDown.Parent = this;
            iButtonVolumeUp.Parent = this;
            iLabelVolume.Parent = this;
            iWidgetMediaTime.Parent = this;
            iPnlTransportControlPause.Parent = this;
            iPnlTransportControlStop.Parent = this;

            this.ResumeLayout(false);

        }

        public void Load(ContextMenu aContextMenu, OptionEnum aOptionVolumeStep)
        {
            iOptionVolumeStep = aOptionVolumeStep;

            foreach (MenuItem item in aContextMenu.MenuItems)
            {
                if (item.Text == "Mute")
                {
                    iMenuItemMute = item;
                    iMenuItemMute.Click += ContextMenuMuteClick;
                    iMenuItemMute.Enabled = false;
                }
            }
        }

        private void ContextMenuMuteClick(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;

            item.Checked = !item.Checked;

            if (EventMuteChanged != null)
            {
                EventArgsMute args = new EventArgsMute(item.Checked);
                EventMuteChanged(this, args);
            }
        }

        private delegate void SetMuteDelegate(bool aMute);
        public void SetMute(bool aMute)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new SetMuteDelegate(SetMute), new object[] { aMute });
            }
            else
            {
                iMute = aMute;

                //MenuItemMute will be null if we are currently viewing an Aux source
                if (iMenuItemMute != null)
                {
                    iMenuItemMute.Checked = aMute;

                    iLabelVolume.ForeColor = aMute ? Color.FromArgb(110, 110, 110) : Color.White;
                    iLabelVolume.Invalidate();
                }
            }
        }

        private delegate void SetVolumeDelegate(uint aVolume);
        public void SetVolume(uint aVolume)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new SetVolumeDelegate(SetVolume), new object[] { aVolume });
            }
            else
            {
                DisplayVolume(true);
                iLabelVolume.Text = aVolume.ToString();
            }
        }

        public void SetVolumeLimit(uint aVolumeLimit)
        {
        }

        public void Open()
        {
        }

        private void DisplayVolume(bool aDisplay)
        {
            SuspendLayout();

            iMenuItemMute.Enabled = aDisplay;
            iButtonVolumeDown.Visible = aDisplay;
            iButtonVolumeUp.Visible = aDisplay;
            iLabelVolume.Visible = aDisplay;

            ResumeLayout(false);
        }

        private delegate void CloseDelegate();
        public void Close()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CloseDelegate(Close), new object[] { });
            }
            else
            {
                DisplayVolume(false);
            }
        }

        private delegate void InitialisedDelegate();
        public void Initialised()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new InitialisedDelegate(Initialised), new object[] { });
            }
            else
            {
                DisplayVolume(true);
            }
        }

        private void ButtonVolumeDownClick(object sender, EventArgs e)
        {
            int volStep = int.Parse(iOptionVolumeStep.Value);
            for (int i = 0; i < volStep; i++)
            {
                if (EventVolumeDecrement != null)
                {
                    EventVolumeDecrement(this, EventArgs.Empty);
                }
            }
        }

        private void ButtonVolumeUpClick(object sender, EventArgs e)
        {
            int volStep = int.Parse(iOptionVolumeStep.Value);
            for (int i = 0; i < volStep; i++)
            {
                if (EventVolumeIncrement != null)
                {
                    EventVolumeIncrement(this, EventArgs.Empty);
                }
            }
        }

        private void LabelVolumeClick(object sender, EventArgs e)
        {
            if (EventMuteChanged != null)
            {
                EventMuteChanged(this, new EventArgsMute(!iMute));
            }
        }

        private MenuItem iMenuItemMute;
        private OptionEnum iOptionVolumeStep;
        private bool iMute;

        private WidgetMediaTime iWidgetMediaTime;
        private WidgetTransport iPnlTransportControlPause;
        private WidgetTransport iPnlTransportControlStop;
        private ImageButton iButtonVolumeDown;
        private ImageButton iButtonVolumeUp;
        private TransparentLabel iLabelVolume;
    }
}
