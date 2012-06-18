using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Net;
using System.Threading;
using System.Windows.Forms;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;
using Linn.Toolkit.WinForms;
using Linn.Topology;

namespace LinnDiagnostics
{
    public partial class Form1 : Form, IStack
    {
        public Form1(Helper aHelper)
        {
            InitializeComponent();

            iDevices = new SortedList<string, Device>();
            iDevicesMutex = new Mutex();

            iServiceDiagnosticsMutex = new Mutex();
            iResultMutex = new Mutex();

            iRefresh = 0;
            iTimerRefresh = new System.Threading.Timer(RefreshElapsed);
            iRefreshMutex = new Mutex();

            comboBoxRefresh.SelectedIndex = 0;

            iActionsStart = true;

            // create discovery system

            iHelper = aHelper;

            iListener = new SsdpListenerMulticast();

            iDeviceList = new DeviceListUpnp(ServiceDiagnostics.ServiceType(), iListener);
            iDeviceList.EventDeviceAdded += DeviceAdded;
            iDeviceList.EventDeviceRemoved += DeviceRemoved;

            iHelper.Stack.SetStack(this);
            iHelper.Stack.Start();
        }

        public void Start(IPAddress aIpAddress)
        {
            iListener.Start(aIpAddress);
            iDeviceList.Start(aIpAddress);
        }

        public void Stop()
        {
            iDeviceList.Stop();
            iListener.Stop();
        }

        public void DeviceAdded(object obj, DeviceList.EventArgsDevice e)
        {
            iDevicesMutex.WaitOne();
            iDevices.Add(e.Device.Udn, e.Device);
            iDevicesMutex.ReleaseMutex();

            ReplaceComboBoxItems();
        }

        public void DeviceRemoved(object obj, DeviceList.EventArgsDevice e)
        {
            iDevicesMutex.WaitOne();
            iDevices.Remove(e.Device.Udn);
            iDevicesMutex.ReleaseMutex();

            ReplaceComboBoxItems();
        }

        private delegate void ReplaceComboBoxItemsDelegate();

        private void ReplaceComboBoxItems()
        {
            if (toolStrip1.InvokeRequired)
            {
                // This is a non UI thread so delegate the task.
                toolStrip1.Invoke(new ReplaceComboBoxItemsDelegate(ReplaceComboBoxItems), null);
            }
            else
            {
                // This is the UI thread so perform the task.

                listBoxDevice.Items.Clear();

                iDevicesMutex.WaitOne();

                foreach (string item in iDevices.Keys)
                {
                    listBoxDevice.Items.Add(item);
                }

                iDevicesMutex.ReleaseMutex();
            }
        }

        private delegate void UpdateTextBoxResultDelegate();

        private void UpdateTextBoxResult()
        {
            if (textBoxResult.InvokeRequired)
            {
                // This is a non UI thread so delegate the task.
                textBoxResult.Invoke(new UpdateTextBoxResultDelegate(UpdateTextBoxResult), null);
            }
            else
            {
                // This is the UI thread so perform the task.
                iResultMutex.WaitOne();
                textBoxResult.Text = iResult;
                iResultMutex.ReleaseMutex();
            }
        }

        private void comboBoxRefreshSelectedIndexChanged(object sender, EventArgs e)
        {
            uint refresh;

            switch (comboBoxRefresh.SelectedIndex)
            {
                case 0:
                    refresh = 0;
                    break;
                case 1:
                    refresh = 1;
                    break;
                case 2:
                    refresh = 3;
                    break;
                case 3:
                    refresh = 5;
                    break;
                case 4:
                    refresh = 10;
                    break;
                case 5:
                    refresh = 30;
                    break;
                case 6:
                    refresh = 60;
                    break;
                case 7:
                    refresh = 600;
                    break;
                default:
                    refresh = 0;
                    break;

            }

            iRefreshMutex.WaitOne();

            iRefresh = refresh;

            iRefreshMutex.ReleaseMutex();

            if (refresh != 0)
            {
                iTimerRefresh.Change(refresh * 1000, Timeout.Infinite);
            }
            else
            {
                iTimerRefresh.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void EventFormClosed(object sender, FormClosedEventArgs e)
        {
            iHelper.Stack.Stop();
        }

        private void listBoxDeviceSelectedIndexChanged(object sender, EventArgs e)
        {
            Device device;

            iDevicesMutex.WaitOne();
            iDevices.TryGetValue((string)listBoxDevice.SelectedItem, out device);
            iDevicesMutex.ReleaseMutex();

            iServiceDiagnosticsMutex.WaitOne();

            if (device == null)
            {
                iServiceDiagnosticsMutex = null;
                iActionDiagnostic = null;
            }
            else
            {
                iServiceDiagnostics = new ServiceDiagnostics(device);
                iActionDiagnostic = iServiceDiagnostics.CreateAsyncActionDiagnostic();
                iActionDiagnostic.EventResponse += DiagnosticResponse;
                iActionDiagnostic.DiagnosticBegin(textBoxDiagnostic.Text);
            }

            iServiceDiagnosticsMutex.ReleaseMutex();
        }

        private void textBoxDiagnosticTextChanged(object sender, EventArgs e)
        {
            iServiceDiagnosticsMutex.WaitOne();
            ServiceDiagnostics.AsyncActionDiagnostic action = iActionDiagnostic;
            iServiceDiagnosticsMutex.ReleaseMutex();

            if (action != null) {
                action.DiagnosticBegin(textBoxDiagnostic.Text);
            }
        }

        private void DiagnosticResponse(object obj, ServiceDiagnostics.AsyncActionDiagnostic.EventArgsResponse e)
        {
            string[] lines = e.aDiagnosticInfo.Split(new char[] {'\n'});
            iResultMutex.WaitOne();
            iResult = String.Join(Environment.NewLine, lines);
            iResultMutex.ReleaseMutex();

            if (iResult.Length != 0)
            {
                iRefreshMutex.WaitOne();

                uint refresh = iRefresh;

                iRefreshMutex.ReleaseMutex();

                if (refresh != 0)
                {
                    iTimerRefresh.Change(refresh * 1000, Timeout.Infinite);
                }
            }

            UpdateTextBoxResult();
        }

        private void RefreshElapsed(Object state)
        {
            iServiceDiagnosticsMutex.WaitOne();
            ServiceDiagnostics.AsyncActionDiagnostic action = iActionDiagnostic;
            iServiceDiagnosticsMutex.ReleaseMutex();

            if (action != null)
            {
                action.DiagnosticBegin(textBoxDiagnostic.Text);
            }
        }

        private void buttonActionsStartClick(object sender, EventArgs e)
        {
            if (iActionsStart)
            {
                buttonActionsStart.Text = "Stop";
                iActionsStart = false;
                iActionsThread = new Thread(ActionsThread);
            }
            else
            {
                buttonActionsStart.Text = "Start";
                iActionsStart = true;
            }
        }

        private void ActionsThread()
        {
            iActionsCount = 0;
            //iServiceDiagnostics.
        }

        private Helper iHelper;
        private SsdpListenerMulticast iListener;
        private SortedList<string, Device> iDevices;
        private Mutex iDevicesMutex;
        private DeviceList iDeviceList;
        private ServiceDiagnostics iServiceDiagnostics;
        private Mutex iServiceDiagnosticsMutex;
        private ServiceDiagnostics.AsyncActionDiagnostic iActionDiagnostic;
        private string iResult;
        private Mutex iResultMutex;
        private uint iRefresh;
        private System.Threading.Timer iTimerRefresh;
        private Mutex iRefreshMutex;
        private bool iActionsStart;
        private uint iActionsCount;
        private uint iActionsStarted;
        private Thread iActionsThread;

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new FormUserOptions(iHelper.OptionPages);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new FormAboutBox(iHelper);
            f.ShowDialog(this);
            f.Dispose();
        }
    }
}
