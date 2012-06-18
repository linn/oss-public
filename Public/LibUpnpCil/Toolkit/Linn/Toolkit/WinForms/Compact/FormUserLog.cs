using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Linn;

namespace Linn.Toolkit.WinForms
{
    public partial class FormUserLog : Form, IUserLogListener
    {
        public FormUserLog(Icon aIcon)
        {
            InitializeComponent();
            
            //on pocket pc display the ok button rather than the close button
            MinimizeBox = false;

            UserLog.AddListener(this);
        }

        public new void Dispose()
        {
            base.Dispose();
            UserLog.RemoveListener(this);
        }

        public void Write(string aMessage)
        {
        }

        public void WriteLine(string aMessage)
        {  
        }

        private void FormUserLog_Activated(object sender, EventArgs e)
        {
            textBoxLog.Text = "";
            textBoxLog.Text = ClipToMaxLength(UserLog.Text);
            textBoxLog.SelectionStart = textBoxLog.Text.Length;
            textBoxLog.ScrollToCaret();
        }

        public void SetForeColour(Color aForeColour)
        {
            textBoxLog.ForeColor = aForeColour;
        }

        public void SetFont(Font aFont)
        {
            textBoxLog.Font = aFont;
        }

        private string ClipToMaxLength(string aText)
        {
            if (aText.Length >= textBoxLog.MaxLength)
            {
                return aText.Remove(0, aText.Length - textBoxLog.MaxLength);
            }

            return aText;
        }
    }
}
