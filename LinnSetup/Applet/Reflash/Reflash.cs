using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Linn.ProductSupport;

namespace LinnSetup
{
    public partial class Reflash : UserControl, IUpdateFirmwareHandler
    {
        public Reflash(Target aTarget) {
            iTarget = aTarget;
            InitializeComponent();
            buttonRefresh.Image = Linn.Toolkit.WinForms.Properties.Resources.Refresh;
            buttonBack.Image = Linn.Toolkit.WinForms.Properties.Resources.Back;
            buttonForward.Image = Linn.Toolkit.WinForms.Properties.Resources.Forward;
        }

        protected override void OnLoad(EventArgs e) {
            if (iTarget.Box.State != Box.EState.eOff && !iTarget.Box.IsProxy) {
                Enable();
            }
            else {
                Disable();
            }
            base.OnLoad(e);
        }

        public void Enable() {
            this.BeginInvoke(
                (MethodInvoker)delegate() {
                    progressBar1.Enabled = true;
                    textBox1.Enabled = true;
                    buttonRefresh.Enabled = true;
                    buttonBack.Enabled = true;
                    buttonForward.Enabled = true;
                    webBrowser1.DocumentText = iTarget.Box.ReleaseNotesHtml;
                    if (!iTarget.Box.UpdateFirmwareInProgress) {
                        if (iTarget.Box.SoftwareUpdateAvailable) {
                            button1.Enabled = true;
                            checkBox1.Enabled = true;
                        }
                        else {
                            button1.Enabled = false;
                            checkBox1.Enabled = false;
                        }
                        button2.Enabled = true;
                    }
                });
        }

        public void Disable() {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                button1.Enabled = false;
                checkBox1.Enabled = false;
                button2.Enabled = false;
                if (!iTarget.Box.UpdateFirmwareInProgress) {
                    progressBar1.Value = 0;
                    textBox1.Text = "";
                    webBrowser1.DocumentText = "";
                    buttonRefresh.Enabled = false;
                    buttonBack.Enabled = false;
                    buttonForward.Enabled = false;
                    progressBar1.Enabled = false;
                    textBox1.Enabled = false;
                }
            });
        }

        // IUpdateFirmwareHandler interface
        public void Started() {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                progressBar1.Value = 0;
                textBox1.Text = "";
            });
        }
        public void OverallProgress(int aValue) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                progressBar1.Value = aValue;
            });
        }
        public void Status(string aMessage) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                textBox1.Text = aMessage;
            });
        }
        public void Completed() {
            if (iTarget.Box.State != Box.EState.eOff && !iTarget.Box.IsProxy) {
                Enable();
            }
        }
        public void Error(string aMessage) {
            if (iTarget.Box.State != Box.EState.eOff && !iTarget.Box.IsProxy) {
                Enable();
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            button1.Enabled = false;
            checkBox1.Enabled = false;
            button2.Enabled = false;
            iTarget.Box.UpdateFirmware(this, checkBox1.Checked);
        }

        private void button2_Click(object sender, EventArgs e) {
            button1.Enabled = false;
            checkBox1.Enabled = false;
            button2.Enabled = false;
            iTarget.Box.RestoreFactoryDefaults(this);
        }

        private void buttonBack_Click(object sender, EventArgs e) {
            if (!webBrowser1.GoBack()) {
                webBrowser1.DocumentText = iTarget.Box.ReleaseNotesHtml;
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            webBrowser1.Refresh(WebBrowserRefreshOption.Completely);
        }

        private void buttonForward_Click(object sender, EventArgs e) {
            webBrowser1.GoForward();
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e) {
            if (e.Url.ToString() != "about:blank") {
                toolStripLabel2.Text = e.Url.ToString();
            }
            else {
                toolStripLabel2.Text = "";
            }
        }

        private void webBrowser1_NewWindow(object sender, CancelEventArgs e) {
            e.Cancel = true;
            Navigate(new Uri(webBrowser1.StatusText));
        }

        private void Navigate(Uri uri) {
            try {
                webBrowser1.Navigate(uri, false);
            }
            catch (Exception e) {
                Navigate("Could not load: " + uri + "<br><br>" + e.Message);
            }
        }

        private void Navigate(string aMessage) {
            webBrowser1.DocumentText = "<html><head></head><body>" + aMessage + "</body></html>";
        }

        private Target iTarget;
    }
}
