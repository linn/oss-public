using System;
using System.Windows.Forms;

namespace Linn.Toolkit.WinForms
{
    public class StackStatusHandlerCompact : StackStatusHandler
    {
        public StackStatusHandlerCompact(string aTitle)
        {
            iTitle = aTitle;
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
                                          MessageBoxIcon.Exclamation,
                                          MessageBoxDefaultButton.Button1);
                    ShowOptions = (res == DialogResult.Yes);
                    break;

                case EStackState.eBadInterface:
                case EStackState.eNonexistentInterface:
                    msg = "A problem occurred with the configured network adapter (" + e.Status.Interface.Name + ")";
                    msg += "\n\nWould you like to select a different network adapter now?";
                    res = MessageBox.Show(msg, "Network configuration error",
                                          MessageBoxButtons.YesNo,
                                          MessageBoxIcon.Hand,
                                          MessageBoxDefaultButton.Button1);
                    ShowOptions = (res == DialogResult.Yes);
                    break;
            }
        }

        public override void StackStatusChanged(object sender, EventArgsStackStatus e)
        {
        }

        public override void StackStatusOptionsChanged(object sender, EventArgsStackStatus e)
        {
        }

        private string iTitle;
    }
}



