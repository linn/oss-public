using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using Linn.Kinsky;

namespace KinskyDesktop
{
    class FormPluginBrowser : FormThemed
    {
        internal class MyWebBrowser : WebBrowser
        {
            public MyWebBrowser(string aHtml)
            {
                AllowNavigation = false;
                AllowWebBrowserDrop = false;
                IsWebBrowserContextMenuEnabled = false;
                DoubleBuffered = true;

                Visible = false;

                DocumentText = aHtml;
            }
        }

        public FormPluginBrowser(ViewSupport aSupport, HttpClient aHttpClient, string aUrl)
        {
            iSupport = aSupport;

            AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSize = true;
            ControlBox = false;
            Font = new Font(iSupport.FontSmall, FontStyle.Bold);
            Text = "Kinsky Plugins";
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = true;
            ShowInTaskbar = false;
            //StartPosition = FormStartPosition.CenterParent;
            ForeColor = iSupport.ForeColourBright;

            string html = "<html><head></head><body bgcolor=\"black\"><p><font face=\"arial\" color=\"white\"><b>Not Available</b></font></p></body>";

            Stream stream = aHttpClient.RequestHigh(new Uri(aUrl));
            if (stream != null)
            {
                int count = 0;
                byte[] tempBuffer = new byte[1024];
                MemoryStream buffer = new MemoryStream();
                do
                {
                    count = stream.Read(tempBuffer, 0, tempBuffer.Length);
                    buffer.Write(tempBuffer, 0, count);
                } while (count > 0);

                byte[] body = buffer.GetBuffer();
                html = ASCIIEncoding.UTF8.GetString(body, 0, body.Length);

                stream.Close();
                buffer.Close();
            }

            iWebBrowser = new MyWebBrowser(html);
            iWebBrowser.Size = ClientSize;
            iWebBrowser.Location = ClientRectangle.Location;

            Controls.Add(iWebBrowser);

            ClientSize = new System.Drawing.Size(600, 400);
            iWebBrowser.Size = ClientSize;

            iSupport.EventSupportChanged += SupportChanged;
        }

        protected override void OnShown(EventArgs e)
        {
            iWebBrowser.Visible = false;
            base.OnShown(e);
            iWebBrowser.Visible = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            Hide();
        }

        private void SupportChanged(object sender, EventArgs e)
        {
            Font = new Font(iSupport.FontSmall, FontStyle.Bold);
            ForeColor = iSupport.ForeColourBright;
        }

        private ViewSupport iSupport;
        private WebBrowser iWebBrowser;
    }
}