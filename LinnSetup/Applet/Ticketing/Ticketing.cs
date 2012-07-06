using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Windows.Forms;

using Linn;
using Linn.ProductSupport;
using Linn.ProductSupport.Ticketing;
using Linn.ProductSupport.Diagnostics;

namespace LinnSetup
{
    public partial class Ticketing : UserControl
    {
        public Ticketing(Target aTarget, Helper aHelper, Diagnostics aDiagnostics) {
            iTarget = aTarget;
            iHelper = aHelper;
            iDiagnostics = aDiagnostics;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e) {
            if (iTarget.Box.State == Box.EState.eOn && !iTarget.Box.IsProxy) {
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
                  firstName.Enabled = true;
                  ticketXml.Enabled = true;
                  email.Enabled = true;
                  phone.Enabled = true;
                  lastName.Enabled = true;
                  faultDesc.Enabled = true;
                  contactNotes.Enabled = true;
                  response.Enabled = true;
                  submit.Enabled = true;
                  clear.Enabled = true;
                  runTests.Enabled = true;
                  testInfo.Enabled = true;
            });

            if (!iEnabled) {
                iEnabled = true;
            }
        }

        public void Disable() {
            this.BeginInvoke(
              (MethodInvoker)delegate() {
                    firstName.Text = "";
                    ticketXml.Text = "";
                    email.Text = "";
                    phone.Text = "";
                    lastName.Text = "";
                    faultDesc.Text = "";
                    contactNotes.Text = "";
                    response.Text = "";
                    testInfo.Text = "";

                    firstName.Enabled = false;
                    ticketXml.Enabled = false;
                    email.Enabled = false;
                    phone.Enabled = false;
                    lastName.Enabled = false;
                    faultDesc.Enabled = false;
                    contactNotes.Enabled = false;
                    response.Enabled = false;
                    submit.Enabled = false;
                    clear.Enabled = false;
                    runTests.Enabled = false;
                    testInfo.Enabled = false;
            });

            if (iEnabled) {
                iEnabled = false;
            }
        }

        private void clear_Click(object sender, EventArgs e) {
            firstName.Text = "";
            ticketXml.Text = "";
            email.Text = "";
            phone.Text = "";
            lastName.Text = "";
            faultDesc.Text = "";
            contactNotes.Text = "";
            response.Text = "";
            testInfo.Text = "";
        }

        private void runTests_Click(object sender, EventArgs e) {
            string adapter = iHelper.Interface.Interface.Info.IPAddress.ToString();
            if (iDiagnostics != null && adapter != null) {
                testInfo.Text = "Dhcp Test in progress";
                iDiagnostics.Run(ETest.eDhcp, adapter);
                testInfo.Text = "Internet Test in progress";
                iDiagnostics.Run(ETest.eInternet, adapter);
                testInfo.Text = "Upnp Test in progress";
                iDiagnostics.Run(ETest.eUpnp, adapter);

                string deviceIp = iTarget.Box.IpAddress;
                testInfo.Text = "MulticastFromDs Test in progress";
                iDiagnostics.Run(ETest.eMulticastFromDs, adapter, deviceIp);
                testInfo.Text = "MulticastToDs Test in progress";
                iDiagnostics.Run(ETest.eMulticastToDs, adapter, deviceIp);
                testInfo.Text = "TcpEcho Test in progress";
                iDiagnostics.Run(ETest.eTcpEcho, adapter, deviceIp);
                testInfo.Text = "UdpEcho Test in progress";
                iDiagnostics.Run(ETest.eUdpEcho, adapter, deviceIp);
                testInfo.Text = "Tests Complete";
            }
        }

        private void submit_Click(object sender, EventArgs e) {
            string ticketResponse = "";
            string ticketXmlResponse = "";
            testInfo.Text = "Submit Ticket in progress";
            if (!Ticket.SubmitTicket(iHelper.Version, "Kiko DSM", "Unpack1", firstName.Text, lastName.Text, email.Text, phone.Text, contactNotes.Text, faultDesc.Text, iDiagnostics, iTarget.Box, out ticketResponse, out ticketXmlResponse))
            {
                Console.WriteLine("failed to post!!!");
            }
            testInfo.Text = "Submit Ticket complete";
            response.Text = ticketResponse;
            ticketXml.Text = ticketXmlResponse;
        }

        private Target iTarget;
        private Helper iHelper;
        private bool iEnabled = false;
        private Diagnostics iDiagnostics;
    }
}
