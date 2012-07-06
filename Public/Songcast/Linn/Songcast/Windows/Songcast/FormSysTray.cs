using System;
using System.Drawing;
using System.Windows.Forms;

namespace Linn.Songcast
{
    public partial class FormSysTray : Form
    {
        private Model iModel;

        public FormSysTray(Model aModel)
        {
            InitializeComponent();
#if DEBUG || TRACE
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
#endif
            iModel = aModel;
            iModel.EventEnabledChanged += ModelEnabledChanged;

            SetEnabled(iModel.Enabled);
        }

        public EventHandler EventIconClick;

        private void ModelEnabledChanged(object sender, EventArgs e)
        {
            SetEnabled(iModel.Enabled);
        }

        private void SetEnabled(bool aEnabled)
        {
            if (aEnabled)
            {
                notifyIcon.Icon = ResourceManager.SysTrayIconOn;
                notifyIcon.Text = App.kName + ": On";
            }
            else
            {
                notifyIcon.Icon = ResourceManager.SysTrayIconOff;
                notifyIcon.Text = App.kName + ": Off";
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (EventIconClick != null)
                {
                    EventIconClick(this, EventArgs.Empty);
                }
            }
        }
    }
}
