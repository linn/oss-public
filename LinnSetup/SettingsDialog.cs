using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LinnSetup
{
    public partial class SettingsDialog : Form
    {
        private bool iIsExpertMode;
        public bool IsExpertMode
        {
            get
            {
                return iIsExpertMode;
            }
        }
        
        public SettingsDialog(bool aIsExpertMode)
        {
            InitializeComponent();
            InitializeDialog(aIsExpertMode);
        }

        private void InitializeDialog(bool aIsExpertMode) 
        {
            iIsExpertMode = aIsExpertMode;

            comboBoxExpert.SelectedItem = (iIsExpertMode)
                ? "True"
                : "False";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //set expert mode to current
            comboBoxExpert.SelectedItem = (iIsExpertMode)
                ? "True"
                : "False";

            this.DialogResult = DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            iIsExpertMode = (comboBoxExpert.SelectedItem.ToString() == "True");

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
