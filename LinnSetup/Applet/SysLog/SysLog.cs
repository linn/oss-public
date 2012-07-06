using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Threading;
using System.IO;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

using Linn.ProductSupport;

namespace LinnSetup
{
    public partial class SysLog : UserControl
    {
        public SysLog(Target aTarget) {
            iTarget = aTarget;
            InitializeComponent();
            buttonRefresh.Image = Linn.Toolkit.WinForms.Properties.Resources.Refresh;
        }

        protected override void OnLoad(EventArgs e) {
            if (iTarget.Box.State == Box.EState.eOn) {
                Enable();
            }
            else {
                Disable();
            }
            base.OnLoad(e);
        }

        public void Enable() {
            if (!iEnabled) {
                iEnabled = true;
            }

            this.BeginInvoke(
              (MethodInvoker)delegate() {
                textBoxSysLogResult.Enabled = true;
                buttonRefresh.Enabled = true;
                iTarget.Box.GetSysLog(SysLogCallback);
            });
        }

        public void Disable() {
            iEnabled = false;
            this.BeginInvoke(
              (MethodInvoker)delegate() {
                textBoxSysLogResult.Text = "";
                textBoxSysLogResult.Enabled = false;
                buttonRefresh.Enabled = false;
            });
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            iTarget.Box.GetSysLog(SysLogCallback);
        }

        private void SysLogCallback(string aSysLogPretty) {
            this.BeginInvoke(
                (MethodInvoker)delegate() {
                    textBoxSysLogResult.Text = aSysLogPretty;
            });
        } 

        private Target iTarget;
        private bool iEnabled = false;
    }
}
