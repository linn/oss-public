using System;
using System.Drawing;
using System.Windows.Forms;

namespace Linn.Songcast
{
    public partial class FormSysTray : Form
    {
        private ModelController iModelController;

        public FormSysTray(ModelController aModelController)
        {
            iModelController = aModelController;

            aModelController.EventShowInSysTrayChanged += ModelController_ShowInSysTrayChanged;
            aModelController.EventEnabledChanged += ModelController_EnabledChanged;

            InitializeComponent();

#if DEBUG || TRACE
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.toolStripMenuItem2, this.exitToolStripMenuItem });
#endif

            openLinnSongcasterPreferencesToolStripMenuItem.Text = "Open " + App.kName  + " Preferences...";

            SetEnabled(aModelController.Enabled);
            notifyIcon.Visible = aModelController.ShowInSysTray;
        }

        public void ShowBalloonTip()
        {
            notifyIcon.ShowBalloonTip(5000, "Information", "You can turn " + App.kName + " on and off or change preferences by clicking on this icon", System.Windows.Forms.ToolTipIcon.Info);
        }

        public EventHandler<EventArgs> EventOpenPreferences;

        private void ModelController_ShowInSysTrayChanged(object sender, EventArgs e)
        {
            notifyIcon.Visible = iModelController.ShowInSysTray;
        }

        private void ModelController_EnabledChanged(object sender, EventArgs e)
        {
            SetEnabled(iModelController.Enabled);
        }

        private void SetEnabled(bool aEnabled)
        {
            if (aEnabled)
            {
                toggleLinnSongcasterToolStripMenuItem.Text = "Turn " + App.kName + " Off";
                linnSongcasterToolStripMenuItem.Text = App.kName + ": On";
                notifyIcon.Icon = ResourceManager.SysTrayIconOn;
                notifyIcon.Text = App.kName + ": On";
            }
            else
            {
                toggleLinnSongcasterToolStripMenuItem.Text = "Turn " + App.kName + " On";
                linnSongcasterToolStripMenuItem.Text = App.kName + ": Off";
                notifyIcon.Icon = ResourceManager.SysTrayIconOff;
                notifyIcon.Text = App.kName + ": Off";
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void toggleLinnSongcasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            iModelController.Enabled = !iModelController.Enabled;
        }

        private void openLinnSongcasterPreferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EventOpenPreferences != null)
            {
                EventOpenPreferences(this, EventArgs.Empty);
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            /*if (EventOpenPreferences != null)
            {
                EventOpenPreferences(this, EventArgs.Empty);
            }*/
        }

        private void reconnectSelectedReceiversToolStripMenuItem_Click(object sender, EventArgs e)
        {
            iModelController.ReconnectSelectedReceivers();
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                System.Reflection.MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                mi.Invoke(notifyIcon, null);
            }
        }
    }
}
