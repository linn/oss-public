using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;

using Linn.Topology.Boxes;

namespace LinnSetup
{
    public partial class Config : UserControl
    {
        public Config(Target aTarget) {
            iTarget = aTarget;
            InitializeComponent();
            buttonRefresh.Image = Linn.Toolkit.WinForms.Properties.Resources.Refresh;
            buttonToggle.Image = Linn.Toolkit.WinForms.Properties.Resources.Toggle;

            if (iTarget.Box.State == Box.EState.eOff || iTarget.Box.State == Box.EState.eFallback) {
                Disable();
            }
            else {
                Enable();
            }
        }

        public void Enable() {
            if (!iEnabled) {
                Navigate(new Uri(iTarget.Box.ConfigurationUri));
                iEnabled = true;
            }

            this.BeginInvoke(
             (MethodInvoker)delegate() {
                buttonToggle.Enabled = true;
                buttonRefresh.Enabled = true;
            });
        }

        public void Disable() {
            iEnabled = false;
            if (iTarget.Box.State == Box.EState.eOff) {
                Navigate(kConfigOff);
            }
            else if (iTarget.Box.State == Box.EState.eFallback) {
                Navigate(kConfigFallback);
            }

            this.BeginInvoke(
            (MethodInvoker)delegate() {
                buttonToggle.Enabled = false;
                buttonRefresh.Enabled = false;
            });
        }

        private void Navigate(string aMessage) {
            webBrowser2.DocumentText = "<html><head></head><body>" + aMessage + "</body></html>";
        }

        private void Navigate(Uri uri) {
            uriLabel.Text = uri.ToString();
            try {
                webBrowser2.Navigated += webBrowser2_Navigated;
                webBrowser2.Navigate(uri);
            }
            catch (Exception e) {
                Navigate("Could not load: " + uri + "<br><br>" + e.Message);
            }
        }

        private void webBrowser2_Navigated(object sender, WebBrowserNavigatedEventArgs e) {
            webBrowser2.Navigated -= webBrowser2_Navigated;
            webBrowser2.Refresh(WebBrowserRefreshOption.Completely);
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            webBrowser2.Refresh(WebBrowserRefreshOption.Completely);
        }

        private void buttonToggle_Click(object sender, EventArgs e) {
            string uri = iTarget.Box.ConfigurationUri;
            if (uriLabel.Text == uri) {
                uri = iTarget.Box.ConfigurationAppUri;
            }
            Navigate(new Uri(uri));
        }

        private const int kRequestTimeout = 500;
        private const string kConfigFallback = "Device in Fallback Mode: Configuration not available";
        private const string kConfigOff = "Device is Off: Configuration not available";

        private Target iTarget;
        private bool iEnabled = false;
    }
}
