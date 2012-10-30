using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Linn.Songbox
{
    public partial class FormSysTray : Form
    {
        private IMediaServerApp iMediaServerApp;

        public FormSysTray(IMediaServerApp aMediaServerApp)
        {
            InitializeComponent();

            iNotifyIcon.Text = aMediaServerApp.Name;
            openMediaServerConfigurationToolStripMenuItem.Text = "Open " + aMediaServerApp.Name + " Configuration...";
            quitMediaServerToolStripMenuItem.Text = "Quit " + aMediaServerApp.Name;

            iMediaServerApp = aMediaServerApp;
            iNotifyIcon.Icon = Linn.Songbox.Properties.Resources.SysTrayIcon;
        }


        private void NotifyIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                System.Reflection.MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                mi.Invoke(iNotifyIcon, null);
            }
        }

        private void MenuItemQuitMediaServerClick(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void MenuItemOpenMediaServerConfigurationClick(object sender, EventArgs e)
        {
            iMediaServerApp.OpenConfiguration();
        }
    }
}
