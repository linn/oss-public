using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Net;
using System.IO;
using System.Globalization;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SneakyRadio
{
    public class StationWfmu : IStation
    {
        public StationWfmu()
        {
            iLogo = new BitmapImage(new Uri("pack://application:,,,/Wfmu/Wfmu.png"));
        }

        public ImageSource Logo()
        {
            return (iLogo);
        }

        public Color Fill()
        {
            return (kFill);
        }

        public Color Back()
        {
            return (kBack);
        }

        public Color Stroke()
        {
            return (kStroke);
        }

        public List<IShow> Shows()
        {
            if (iShows == null)
            {
                WebRequest request = WebRequest.Create("http://www.wfmu.org/archivefeed/mp3.xml");
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();
                XmlTextReader reader = new XmlTextReader(response.GetResponseStream());
                XPathDocument document = new XPathDocument(reader);
                XPathNavigator navigator = document.CreateNavigator();

                iShows = new List<IShow>();

                iShows.Add(new ShowWfmu("Live", DateTime.Now, null));

                foreach (XPathNavigator n in navigator.Select("/rss/channel/item"))
                {
                    string title = n.SelectSingleNode("title").Value.Substring(18);
                    int index = title.LastIndexOf("from");
                    DateTimeFormatInfo format = new DateTimeFormatInfo();
                    format.ShortDatePattern = "MMM d, yyyy";
                    DateTime datetime = DateTime.Parse(title.Substring(index + 5), format);
                    title = title.Substring(0, index);
                    Uri link = new Uri(n.SelectSingleNode("link").Value);
                    iShows.Add(new ShowWfmu(title, datetime, link));
                }
            }
            return (iShows);
        }

        private static readonly Color kFill = (Color)ColorConverter.ConvertFromString("#6395ac");
        private static readonly Color kBack = Colors.White;
        private static readonly Color kStroke = Colors.Black;

        private ImageSource iLogo;

        private List<IShow> iShows;
    }

    public class ShowWfmu : IShow
    {
        public ShowWfmu(string aTitle, DateTime aDateTime, Uri aLink)
        {
            iTitle = aTitle;
            iDateTime = aDateTime;
            iLink = aLink;
        }

        public string Name()
        {
            return (iTitle);
        }

        public string Details()
        {
            return (iDateTime.DayOfWeek + " " + iDateTime.ToLongDateString());
        }

        public Uri Uri()
        {
            if (iLink == null)
            {
                return (new Uri("http://mp3stream.wfmu.org"));
            }

            try
            {
                WebRequest request = WebRequest.Create(iLink);
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                return (new Uri(reader.ReadToEnd()));
            }
            catch (WebException)
            {
                return (null);
            }
        }

        private string iTitle;
        private DateTime iDateTime;
        private Uri iLink;
    }
}
