using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

using Linn.Kinsky;

namespace KinskyDesktop
{
    public partial class FormSavePlaylist : FormThemed
    {
        public FormSavePlaylist(ISaveSupport aSaveSupport)
        {
            InitializeComponent();
			
			iSaveSupport = aSaveSupport;

            Size = new System.Drawing.Size(470, 123);

            TextBoxName.Text = aSaveSupport.DefaultName;
            TextBoxName.SelectAll();
            TextBoxName.Focus();
        }
		
		public string Filename
		{
			get
			{
				return TextBoxName.Text;
			}
		}

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (iSaveSupport.Exists(TextBoxName.Text))
            {
                // file exists - prompt for overwrite
                DialogResult result = MessageBox.Show(this,
                    TextBoxName.Text + " already exists.\nDo you want to replace it?",
                    "Confirm Save As",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    DialogResult = DialogResult.OK;
                }
            }
            else
            {
                // file does not exist - exit
                DialogResult = DialogResult.OK;
            }
        }

        private void TextBoxName_TextChanged(object sender, EventArgs e)
        {
            if (TextBoxName.Text.Length > 0)
            {
                buttonSave.Enabled = true;
            }
            else
            {
                buttonSave.Enabled = false;
            }
        }
		
		private ISaveSupport iSaveSupport;
    }
}
