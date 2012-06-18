using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Linn;

namespace OssKinskyMppLastFm
{
    public partial class OptionsPageLastFm : UserControl, IViewUserOptionsPage, IOptionsPageLastFm
    {
        public OptionsPageLastFm()
        {
            InitializeComponent();

            Name = "Last.Fm";
        }

        public Control Control
        {
            get
            {
                return this;
            }
        }

        public event EventHandler<EventArgs> EventUsernameChanged;
        public event EventHandler<EventArgs> EventPasswordChanged;

        public void SetUsername(string aUsername)
        {
            TextBoxUsername.Text = aUsername;
        }

        public string Username
        {
            get
            {
                return TextBoxUsername.Text;
            }
        }

        public void SetPassword(string aPassword)
        {
            TextBoxPassword.Text = aPassword;
        }

        public string Password
        {
            get
            {
                return TextBoxPassword.Text;
            }
        }

        private void TextBoxUsername_TextChanged(object sender, EventArgs e)
        {
            if (EventUsernameChanged != null)
            {
                EventUsernameChanged(this, EventArgs.Empty);
            }
        }

        private void TextBoxPassword_TextChanged(object sender, EventArgs e)
        {
            if (EventPasswordChanged != null)
            {
                EventPasswordChanged(this, EventArgs.Empty);
            }
        }
    }
}
