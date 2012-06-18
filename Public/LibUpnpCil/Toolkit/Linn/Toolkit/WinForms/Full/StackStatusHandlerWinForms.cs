using System;
using System.Windows.Forms;

namespace Linn.Toolkit.WinForms
{
    public class StackStatusHandlerWinForms : StackStatusHandler
    {
        public StackStatusHandlerWinForms(string aTitle) {
            iTitle = aTitle;
            iNotifyIcon = null;
        }

        public StackStatusHandlerWinForms(string aTitle, NotifyIcon aNotifyIcon)
        {
            iTitle = aTitle;
            iNotifyIcon = aNotifyIcon;
        }

        public override void StackStatusStartupChanged(object sender, EventArgsStackStatus e)
        {
            string msg;
            DialogResult res;

            switch (e.Status.State)
            {
                case EStackState.eStopped:
                case EStackState.eOk:
                    break;

                case EStackState.eNoInterface:
                    msg = iTitle + " requires a network adapter to be configured.";
                    msg += "\n\nWould you like to select a network adapter now?";
                    res = MessageBox.Show(msg, "No network configuration",
                                          MessageBoxButtons.YesNo,
                                          MessageBoxIcon.Warning);
                    ShowOptions = (res == DialogResult.Yes);
                    break;

                case EStackState.eBadInterface:
                case EStackState.eNonexistentInterface:
                    msg = "A problem occurred with the configured network adapter (" + e.Status.Interface.Name + ")";
                    msg += "\n\nWould you like to select a different network adapter now?";
                    res = MessageBox.Show(msg, "Network configuration error",
                                          MessageBoxButtons.YesNo,
                                          MessageBoxIcon.Error);
                    ShowOptions = (res == DialogResult.Yes);
                    break;
            }
        }

        public override void StackStatusChanged(object sender, EventArgsStackStatus e) {
            switch (e.Status.State)
            {
                case EStackState.eStopped:
                    break;
                case EStackState.eOk:
                    if (iNotifyIcon != null) {
                        try {
                            iNotifyIcon.ShowBalloonTip(5000, iTitle, iTitle + " is now connected using \"" + e.Status.Interface.Name + "\"", ToolTipIcon.Info);
                        }
                        catch {
                            // not supported for all platforms
                        }
                    }
                    break;
                case EStackState.eBadInterface:
                case EStackState.eNoInterface:
                case EStackState.eNonexistentInterface:
                    if (iNotifyIcon != null) {
                        try {
                            iNotifyIcon.ShowBalloonTip(10000, iTitle, iTitle + " has lost connectivity.", ToolTipIcon.Warning);
                        }
                        catch {
                            // not supported for all platforms
                        }
                    }
                    break;
            }
        }

        public override void StackStatusOptionsChanged(object sender, EventArgsStackStatus e) {
            string msg;

            switch (e.Status.State)
            {
                case EStackState.eStopped:
                case EStackState.eOk:
                case EStackState.eNoInterface:
                case EStackState.eNonexistentInterface:
                    break;

                case EStackState.eBadInterface:
                    msg = "A problem occurred with the selected network adapter (" + e.Status.Interface.Name + ")";
                    MessageBox.Show(msg, "Network configuration error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                    break;
            }
        }

        private string iTitle;
        private NotifyIcon iNotifyIcon;
    }

}



