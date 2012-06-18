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
    public partial class Presentation : UserControl
    {
        public Presentation(Target aTarget) {
            iTarget = aTarget;
            InitializeComponent();
            buttonRefresh.Image = Linn.Toolkit.WinForms.Properties.Resources.Refresh;

            if (iTarget.Box.State == Box.EState.eOff || iTarget.Box.State == Box.EState.eFallback) {
                Disable();
            }
            else {
                Enable();
            }
        }

        public void Enable() {
            if (!iEnabled && iTarget.Box.PresentationUri != null) {
                Navigate(new Uri(iTarget.Box.PresentationUri));
                iEnabled = true;
            }

            if (iEnabled) {
                this.BeginInvoke(
                 (MethodInvoker)delegate() {
                    buttonRefresh.Enabled = true;
                });
            }
        }

        public void Disable() {
            iEnabled = false;
            if (iTarget.Box.State == Box.EState.eOff) {
                Navigate(kPresentatioOff);
            }
            else if (iTarget.Box.State == Box.EState.eFallback) {
                Navigate(kPresentationFallback);
            }

            this.BeginInvoke(
             (MethodInvoker)delegate() {
                 buttonRefresh.Enabled = false;
             });
        }

        private void Navigate(string aMessage) {
            webBrowser2.DocumentText = "<html><head></head><body>" + aMessage + "</body></html>";
        }

        private void Navigate(Uri uri) {
            toolStripLabel1.Text = uri.ToString();
            try {
                webBrowser2.Navigate(uri);
                webBrowser2.Refresh(WebBrowserRefreshOption.Completely);
            }
            catch (Exception e) {
                Navigate("Could not load: " + uri + "<br><br>" + e.Message);
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            webBrowser2.Refresh(WebBrowserRefreshOption.Completely);
        }

        private const int kRequestTimeout = 500;
        private const string kPresentationFallback = "Device in Fallback Mode: Portal not available";
        private const string kPresentatioOff = "Device is Off: Portal not available";

        private Target iTarget;
        private bool iEnabled = false;
    }
}
