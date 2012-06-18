using System;
using System.Drawing;
using System.Windows.Forms;

namespace Linn.Toolkit.WinForms
{
    public partial class FormCrashLogDumper : Form
    {
        public FormCrashLogDumper(string aTitle, Icon aIcon, string aCrashReport)
        {
            InitializeComponent();
            Text = aTitle;
            Icon = aIcon;

            tableLayoutPanel1.Dock = DockStyle.Fill;
            Height = 218;

            label1.Text = aTitle + " has encountered a problem and needs to close. We are sorry for the inconvenience";
            labelMessage.Text = "We have created an error report that you can send to help us improve " + aTitle;

            textBoxDetails.Text = aCrashReport;
            buttonSend.Select();
        }

        private void PanelIcon_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Linn.Toolkit.WinForms.Properties.Resources.Error, new Rectangle(0, 0, Linn.Toolkit.WinForms.Properties.Resources.Error.Width, Linn.Toolkit.WinForms.Properties.Resources.Error.Height));
        }

        private void buttonDetails_Click(object sender, EventArgs e)
        {
            if (Height == 218)
            {
                tableLayoutPanel1.Dock = DockStyle.Top;
                Height = 514;
            }
            else
            {
                tableLayoutPanel1.Dock = DockStyle.Fill;
                Height = 218;
            }
        }
    }
}
