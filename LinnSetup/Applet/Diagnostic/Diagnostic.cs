using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Threading;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

using Linn.ProductSupport;

namespace LinnSetup
{
    public partial class Diagnostic : UserControl
    {
        public Diagnostic(Target aTarget) {
            iTarget = aTarget;
            iMutex = new Mutex();
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
                iServiceDiagnostics = iTarget.Box.ServiceDiagnostics;
                iServiceDiagnostics.EventStateLastTerminalInputName += EventStateLastTerminalInputNameHandler;
                iServiceDiagnostics.EventInitial += EventInitialHandler;
                RunDiagnosticHelp();
                iEnabled = true;
            }

            this.BeginInvoke(
              (MethodInvoker)delegate() {
                textBoxDiagnosticResult.Enabled = true;
                diagnosticComboBox.Enabled = true;
                buttonRefresh.Enabled = true;
                diagnosticInputBox.Enabled = true;
            });
        }

        public void Disable() {
            iEnabled = false;
            try {
                iServiceDiagnostics.EventStateLastTerminalInputName -= EventStateLastTerminalInputNameHandler;
                iServiceDiagnostics.EventInitial -= EventInitialHandler;
            }
            catch { } // should handle this better
            this.BeginInvoke(
              (MethodInvoker)delegate() {
                textBoxDiagnosticResult.Text = "";
                textBoxDiagnosticResult.Enabled = false;
                diagnosticComboBox.Items.Clear();
                diagnosticInputBox.Text = "";
                diagnosticComboBox.Enabled = false;
                buttonRefresh.Enabled = false;
                diagnosticInputBox.Enabled = false;
            });
        }

        private void RunDiagnosticHelp() {
            iActionDiagnostic = iServiceDiagnostics.CreateAsyncActionDiagnostic();
            iActionDiagnostic.EventResponse += DiagnosticHelp;
            iActionDiagnostic.DiagnosticBegin(kHelpCommand);
        }

        private void RunDiagnostic() {
            string[] entry = diagnosticComboBox.Text.Split(' ');
            if (entry != null && entry.Length == 1) {
                iActionDiagnostic = iServiceDiagnostics.CreateAsyncActionDiagnostic();
                iActionDiagnostic.EventResponse += DiagnosticResponse;
                iActionDiagnostic.DiagnosticBegin(entry[0]);
            }
            else if (entry != null && entry.Length > 1) {
                iActionDiagnosticTest = iServiceDiagnostics.CreateAsyncActionDiagnosticTest();
                iActionDiagnosticTest.EventResponse += DiagnosticTestResponse;
                iActionDiagnosticTest.DiagnosticTestBegin(entry[0], diagnosticInputBox.Text);
            }
        }

        private void DiagnosticResponse(object obj, ServiceDiagnostics.AsyncActionDiagnostic.EventArgsResponse e) {
            iActionDiagnostic.EventResponse -= DiagnosticResponse;

            Lock();
            string[] lines = e.aDiagnosticInfo.Split(new char[] { '\n' });
            string iResult = String.Join(Environment.NewLine, lines);
            Unlock();

            this.BeginInvoke(
               (MethodInvoker)delegate() {
                textBoxDiagnosticResult.Text = iResult;
            });
        }

        private void DiagnosticTestResponse(object obj, ServiceDiagnostics.AsyncActionDiagnosticTest.EventArgsResponse e) {
            iActionDiagnosticTest.EventResponse -= DiagnosticTestResponse;

            Lock();
            string text = e.aDiagnosticInfo + Environment.NewLine + "Test Result: " + (e.aDiagnosticResult == true ? "Passed" : "Failed") + Environment.NewLine;
            string[] lines = text.Split(new char[] { '\n' });
            string iResult = String.Join(Environment.NewLine, lines);
            Unlock();

            this.BeginInvoke(
               (MethodInvoker)delegate() {
                textBoxDiagnosticResult.Text = iResult;
            });
        }

        private void DiagnosticHelp(object obj, ServiceDiagnostics.AsyncActionDiagnostic.EventArgsResponse e) {
            iActionDiagnostic.EventResponse -= DiagnosticHelp;

            Lock();
            string[] lines = e.aDiagnosticInfo.Split(new char[] { '\n' });
            Unlock();

            this.BeginInvoke(
                (MethodInvoker)delegate() {
                diagnosticComboBox.Items.Clear();

                foreach (string line in lines) {
                    if (line != "") {
                        diagnosticComboBox.Items.Add(line);
                    }
                }

                iActionDiagnosticTest = iServiceDiagnostics.CreateAsyncActionDiagnosticTest();
                iActionDiagnosticTest.EventResponse += DiagnosticTestHelp;
                iActionDiagnosticTest.DiagnosticTestBegin(kHelpCommand, "");
            }
           );
        }

        private void DiagnosticTestHelp(object obj, ServiceDiagnostics.AsyncActionDiagnosticTest.EventArgsResponse e) {
            iActionDiagnosticTest.EventResponse -= DiagnosticTestHelp;

            Lock();
            string[] lines = e.aDiagnosticInfo.Split(new char[] { '\n' });
            Unlock();

            this.BeginInvoke(
                (MethodInvoker)delegate() {
                foreach (string line in lines) {
                    if (line != "") {
                        diagnosticComboBox.Items.Add(line);
                    }
                }

                //set first item to be selected
                if (diagnosticComboBox.Items.Count != 0) {
                    diagnosticComboBox.SelectedIndex = 0;
                }
            }
           );
        }

        private void EventInitialHandler(object sender, EventArgs e) {
        }

        private void EventStateLastTerminalInputNameHandler(object sender, EventArgs e) {
            string name = iServiceDiagnostics.LastTerminalInputName;
            if (name == null || name == "") {
                return;
            }
            name = name + " (0x" + iServiceDiagnostics.LastTerminalInputCode.ToString("X") + ")";
            string text = Environment.NewLine + "<DIAGNOSTIC EVENT> Last Terminal Input: " + name + Environment.NewLine;

            this.BeginInvoke(
               (MethodInvoker)delegate() {
                   textBoxDiagnosticResult.Text += text;
            });
        }

        private void Lock() {
            iMutex.WaitOne();
        }

        private void Unlock() {
            iMutex.ReleaseMutex();
        }

        private void diagnosticComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            diagnosticInputBox.Text = "";
            RunDiagnostic();
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            RunDiagnostic();
        }

        private void diagnosticInputBox_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                RunDiagnostic();
            }
        }

        private const string kHelpCommand = "help";
        private const string kNone = "none available";

        private ServiceDiagnostics iServiceDiagnostics;
        private ServiceDiagnostics.AsyncActionDiagnostic iActionDiagnostic;
        private ServiceDiagnostics.AsyncActionDiagnosticTest iActionDiagnosticTest;
        private Mutex iMutex;
        private Target iTarget;
        private bool iEnabled = false;
    }
}
