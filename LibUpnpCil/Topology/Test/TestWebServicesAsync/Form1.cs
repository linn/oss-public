using System;
using System.Net;
using System.Windows.Forms;
using System.Threading;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;
using Linn.Toolkit.WinForms;

namespace TestWebServicesAsync
{
    public partial class Form1 : Form, IStack
    {
        private Helper iHelper;
        private FormUserLog iFormUserLog;

        private ServiceProduct iService;
        private ServiceProduct.AsyncActionProduct iAction;

        private SsdpListenerMulticast iListenerNotify;
        private DeviceListUpnp iDeviceListProduct;

        private string iFriendlyName;
        private uint iIterations;
        private uint iCount;

        private Thread iThread;

        public Form1(Helper aHelper, string aFriendlyName, uint aIterations)
        {
            InitializeComponent();

            MinimizeBox = true;
            iHelper = aHelper;

            iFriendlyName = aFriendlyName;
            iIterations = aIterations;

            iListenerNotify = new SsdpListenerMulticast();
            iDeviceListProduct = new DeviceListUpnp(ServiceProduct.ServiceType(), iListenerNotify);

            iDeviceListProduct.EventDeviceAdded += EventDeviceAdded;
            iDeviceListProduct.EventDeviceRemoved += EventDeviceRemoved;

            iHelper.Stack.SetStack(this);
        }

        private void EventDeviceAdded(object sender, DeviceList.EventArgsDevice e)
        {
            if (e.Device.Name == iFriendlyName)
            {
                lock (this)
                {
                    iService = new ServiceProduct(e.Device);
                    iAction = iService.CreateAsyncActionProduct();
                    iAction.EventResponse += EventResponse;

                    iThread = new Thread(Run);
                    iThread.Name = "TestRunner";
                    iThread.IsBackground = true;
                    iThread.Start();
                }
            }
        }

        private void EventDeviceRemoved(object sender, DeviceList.EventArgsDevice e)
        {
            if (e.Device.Name == iFriendlyName)
            {
                lock (this)
                {
                    iAction.EventResponse -= EventResponse;
                    iAction = null;
                    iService = null;
                    iThread = null;
                }
            }
        }

        private void Run()
        {
            for (uint i = 0; i < iIterations; ++i)
            {
                Display(iText, (i+1).ToString());

                lock (this)
                {
                    if (iAction != null)
                    {
                        iAction.ProductBegin();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void EventResponse(object sender, ServiceProduct.AsyncActionProduct.EventArgsResponse e)
        {
            iCount++;
            Display(iResult, e.Room + " (" + iCount + ")");
            Thread.Sleep(1500);
        }

        private delegate void DisplayCallback(TextBox aTextBox, string aText);
        private void Display(TextBox aTextBox, string aText)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DisplayCallback(Display), new object[] { aTextBox, aText });
            }
            else
            {
                aTextBox.Text = aText;
            }
        }

        private void ShowOptionsDialog()
        {
            FormUserOptions optionsDialog = new FormUserOptions(iHelper.OptionPages);
            optionsDialog.ShowDialog();
            optionsDialog.Dispose();
        }

        void IStack.Start(IPAddress aIpAddress)
        {
            iListenerNotify.Start(aIpAddress);
            iDeviceListProduct.Start(aIpAddress);
        }

        void IStack.Stop()
        {
            iDeviceListProduct.Stop();
            iListenerNotify.Stop();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            iHelper.Stack.Start();

            StackStatus status = iHelper.Stack.Status;
            if (status.State != EStackState.eOk)
            {
                ShowOptionsDialog();
            }
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            iHelper.Stack.Stop();
        }

        private void MenuItemOptionsClick(object sender, EventArgs e)
        {
            ShowOptionsDialog();
        }

        private void MenuItemDebugClick(object sender, EventArgs e)
        {
            if (iFormUserLog == null || iFormUserLog.IsDisposed)
            {
                iFormUserLog = new FormUserLog(Icon);
            }
            iFormUserLog.Show();
        }

        private void MenuItemAboutClick(object sender, EventArgs e)
        {
            FormAboutBox aboutDialog = new FormAboutBox(iHelper);
            aboutDialog.ShowDialog();
            aboutDialog.Dispose();
        }
    }
}
