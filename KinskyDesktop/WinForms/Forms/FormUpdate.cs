using System;
using System.Threading;
using System.Windows.Forms;

using Linn;
using Linn.Kinsky;

namespace KinskyDesktop
{
    public partial class FormUpdate : FormThemed
    {
        private AutoUpdate iAutoUpdate;
        private AutoUpdate.AutoUpdateInfo iInfo;
        private Thread iThread;

        public FormUpdate(AutoUpdate aAutoUpdate, AutoUpdate.AutoUpdateInfo aInfo)
        {
            iAutoUpdate = aAutoUpdate;
            iInfo = aInfo;

            InitializeComponent();

            iThread = new Thread(Run);
            iThread.Name = "Update";
            iThread.IsBackground = true;

            iAutoUpdate.EventUpdateProgress += UpdateProgress;
            iAutoUpdate.EventUpdateFailed += UpdateFailed;

            label1.Text = string.Format("There is a new version of {0} ({1}) available.", aInfo.Name, aInfo.Version);
            webBrowser1.Navigate(aInfo.History);
            webBrowser1.Visible = false;
            Height = 22 + 45 + 30 + 8 + 28 + 15;
        }

        private void Run()
        {
            iAutoUpdate.DownloadUpdate(iInfo);

            Invoke((MethodInvoker)delegate()
            {
                buttonCancel.Enabled = false;
            });

            if (iAutoUpdate.ApplyUpdate(iInfo))
            {
                Invoke((MethodInvoker)delegate()
                {
                    DialogResult = DialogResult.OK;
                    Close();
                });
            }
        }

        private void UpdateProgress(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate()
            {
                progressBar1.Value = iAutoUpdate.UpdateProgress;
            });
        }

        private void UpdateFailed(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate()
            {
                label1.Text = "Update failed.";
            });
        }

        private void tableLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void tableLayoutPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void tableLayoutPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        private void tableLayoutPanel1_MouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        private void buttonDetails_Click(object sender, EventArgs e)
        {
            if (Height == 580)
            {
                webBrowser1.Visible = false;
                Height = 22 + 45 + 30 + 8 + 28 + 15;
            }
            else
            {
                webBrowser1.Visible = true;
                Height = 580;
            }
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            buttonInstall.Enabled = false;
            iThread.Start();
        }

        private void FormUpdate_FormClosed(object sender, FormClosedEventArgs e)
        {
            iAutoUpdate.EventUpdateProgress -= UpdateProgress;
            iAutoUpdate.EventUpdateFailed -= UpdateFailed;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            iThread.Abort();
        }
    }
}
