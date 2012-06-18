using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Linn;

namespace Linn.Toolkit.WinForms
{
    public interface IViewUserLog
    {
        void SetBackColour(Color aBackColour);
        void SetForeColour(Color aForeColour);
        void SetFont(Font aFont);
    }

    public partial class FormUserLog : Form, IViewUserLog, IUserLogListener
    {
        public FormUserLog(Icon aIcon)
        {
            InitializeComponent();
            
            Icon = aIcon;
            
            IntPtr handle = textBoxLog.Handle; // forces handle to be created - gets around mono visibility issue
            handle.ToInt32();
            
            UserLog.AddListener(this);
        }

        public new void Dispose()
        {
            base.Dispose();
            UserLog.RemoveListener(this);
        }

        public void SetBackColour(Color aBackColour)
        {
            textBoxLog.BackColor = aBackColour;
        }

        public void SetForeColour(Color aForeColour)
        {
            textBoxLog.ForeColor = aForeColour;
        }

        public void SetFont(Font aFont)
        {
            textBoxLog.Font = aFont;
        }

        public void Write(string aMessage)
        {
            //UserLog.Text += aMessage;
        }

        public void WriteLine(string aMessage)
        {
            //UserLog.Text += (aMessage + Environment.NewLine);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            Hide();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                if (textBoxLog.InvokeRequired)
                {
                    textBoxLog.BeginInvoke((MethodInvoker)delegate()
                    {
                        textBoxLog.Text = "";
                        textBoxLog.AppendText(ClipToMaxLength(UserLog.Text));
                        textBoxLog.SelectionStart = textBoxLog.Text.Length;
                        textBoxLog.ScrollToCaret();
                    });
                }
                else
                {
                    textBoxLog.Text = "";
                    textBoxLog.AppendText(ClipToMaxLength(UserLog.Text));
                    textBoxLog.SelectionStart = textBoxLog.Text.Length;
                    textBoxLog.ScrollToCaret();
                }
            }
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
