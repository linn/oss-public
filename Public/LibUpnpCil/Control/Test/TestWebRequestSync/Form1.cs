using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TestWebRequestSync
{
    public partial class Form1 : Form
    {
        private Thread iThread;
        private int iSuccess = 0;
        private int iError = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            iThread = new Thread(Run);
            iThread.Name = "TestRunner";
            iThread.IsBackground = true;
            iThread.Start();
        }

        private void Run()
        {
            for (int i = 0; i < 100; ++i)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://172.20.15.103:9000/DeviceDescription.xml");

                    // Use a HTTP 1.0 client because of bug 80017 in the mono bug database
                    // Use a default WebProxy to avoid proxy authentication errors
                    request.Proxy = new WebProxy();
                    request.ProtocolVersion = HttpVersion.Version10;
                    request.KeepAlive = false;
                    request.Timeout = 500;
                    request.ReadWriteTimeout = 500;

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    
                    string xml = reader.ReadToEnd();

                    reader.Close();
                    response.Close();

                    Success();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    Error(e.Message);
                }

                //Thread.Sleep(1000);
            }
        }

        private delegate void SuccessCallback();
        private void Success()
        {
            if (textBoxSuccess.InvokeRequired)
            {
                textBoxSuccess.BeginInvoke(new SuccessCallback(Success));
            }
            else
            {
                iSuccess++;
                textBoxSuccess.Text = iSuccess.ToString();
            }
        }

        private delegate void ErrorCallback(string aMessage);
        private void Error(string aMessage)
        {
            if (textBoxError.InvokeRequired)
            {
                textBoxError.BeginInvoke(new ErrorCallback(Error), new object[] { aMessage });
            }
            else
            {
                iError++;
                textBoxError.Text = aMessage + "(" + iError.ToString() + ")";
            }
        }
    }
}