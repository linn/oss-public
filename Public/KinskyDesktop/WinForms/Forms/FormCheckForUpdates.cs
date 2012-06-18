using System.Threading;
using System.Windows.Forms;

using Linn;
using Linn.Kinsky;

namespace KinskyDesktop
{
    public partial class FormCheckForUpdates : FormThemed
    {
        private Thread iThread;
        private AutoUpdate iAutoUpdate;
        private AutoUpdate.AutoUpdateInfo iInfo;

        public FormCheckForUpdates(AutoUpdate aAutoUpdate)
        {
            iAutoUpdate = aAutoUpdate;

            InitializeComponent();

            iThread = new Thread(Run);
            iThread.Name = "CheckForUpdate";
            iThread.IsBackground = true;
        }

        public AutoUpdate.AutoUpdateInfo Info
        {
            get
            {
                return iInfo;
            }
        }

        private void Run()
        {
            iInfo = iAutoUpdate.CheckForUpdate();

            BeginInvoke((MethodInvoker)delegate()
            {
                DialogResult = DialogResult.OK;
                Close();
            });
        }

        private void tableLayoutPanel1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void tableLayoutPanel1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void tableLayoutPanel1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        private void tableLayoutPanel1_MouseLeave(object sender, System.EventArgs e)
        {
            OnMouseLeave(e);
        }

        private void FormCheckForUpdates_Shown(object sender, System.EventArgs e)
        {
            iThread.Start();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            iThread.Abort();
        }
    }
}
