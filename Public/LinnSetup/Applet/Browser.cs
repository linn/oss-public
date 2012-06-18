using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace LinnSetup
{
    public partial class Browser : UserControl
    {
        private const int kRequestTimeout = 500;
        private const string kHomeUrl = @"http://www.linn.co.uk/";

        public WebBrowser MyBrowser 
        {
            get
            {
                return webBrowser1;
            }
        }

        public Browser()
        {
            InitializeComponent();
        }

        public void Navigate(string pageText) 
        {
            webBrowser1.DocumentText = pageText;
        }

        public void Navigate(Uri uri)
        {
            uri = Check404(uri);
            webBrowser1.Navigate(uri);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            webBrowser1.GoBack();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            webBrowser1.GoForward();
        }

        private Uri Check404(Uri url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

                // Use a HTTP 1.0 client because of bug 80017 in the mono bug database
                // Use a default WebProxy to avoid proxy authentication errors
                request.Proxy = new WebProxy();
                request.ProtocolVersion = HttpVersion.Version10;
                request.KeepAlive = false;
                request.Timeout = kRequestTimeout;
                request.ReadWriteTimeout = kRequestTimeout;

                request.GetResponse();

                return url;
            }
            catch (WebException)
            {
                return new Uri(kHomeUrl);
            }
        }

    }
}
