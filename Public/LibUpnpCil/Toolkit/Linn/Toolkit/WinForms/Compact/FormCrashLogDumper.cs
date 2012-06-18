using System;
using System.Drawing;
using System.Windows.Forms;

namespace Linn.Toolkit.WinForms
{
    public partial class FormCrashLogDumper : Form
    {
        public FormCrashLogDumper(string aTitle, string aCrashReport)
        {
            InitializeComponent();
            
            Text = aTitle;
            iLabelTitle.Text = aTitle + " has encountered a problem and needs to close. We are sorry for the inconvenience.";
            iLabelMessage.Text = "We have created an error report that you can send to help us improve " + aTitle + ".";
            iTextBoxDetails.Text = aCrashReport;
        }

        private void MenuItemSendClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void MenuItemNoSendClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }        
    }
}