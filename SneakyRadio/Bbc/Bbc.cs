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
    public class StationBbc
    {
        public StationBbc(string aChannel)
        {
            iChannel = aChannel;
        }

        public List<IShow> Shows()
        {
            if (iShows == null)
            {
                WebRequest request = WebRequest.Create("http://www.bbc.co.uk/radio/podcasts/ip/lists/" + iChannel + ".sssi");
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();
                XmlTextReader reader = new XmlTextReader(response.GetResponseStream());
                XPathDocument document = new XPathDocument(reader);
                XPathNavigator navigator = document.CreateNavigator();

                iShows = new List<IShow>();

                foreach (XPathNavigator n in navigator.Select("//*[@class='li_with_image']"))
                {
                    try
                    {
                        string title = n.SelectSingleNode("*/*/*[@class='podcast_title']").Value;
                        string link = n.SelectSingleNode("a/@href").Value;
                        WebRequest linkrequest = WebRequest.Create("http://www.bbc.co.uk" + link);
                        linkrequest.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                        WebResponse linkresponse = linkrequest.GetResponse();
                        XmlTextReader linkreader = new XmlTextReader(linkresponse.GetResponseStream());
                        XPathDocument linkdocument = new XPathDocument(linkreader);
                        XPathNavigator linknavigator = linkdocument.CreateNavigator();

                        foreach (XPathNavigator l in linknavigator.Select("//*[@class='title']"))
                        {
                            Uri uri = new Uri(l.SelectSingleNode("../embed/@src").Value);

                            string subtitle = l.Value;

                            int colon = subtitle.IndexOf(':');

                            if (colon > 0)
                            {
                                subtitle = subtitle.Substring(colon + 1);
                            }

                            iShows.Add(new ShowBbc(title, subtitle, uri));
                        }
                    }
                    catch (NullReferenceException)
                    {
                    }
                    catch (WebException)
                    {
                    }
                }
            }
            return (iShows);
        }

        private string iChannel;

        private List<IShow> iShows;
    }

    public class ShowBbc : IShow
    {
        public ShowBbc(string aTitle, string aSubTitle, Uri aUri)
        {
            iTitle = aTitle;
            iSubTitle = aSubTitle;
            iUri = aUri;
        }

        public string Name()
        {
            return (iTitle);
        }

        public string Details()
        {
            return (iSubTitle);
        }

        public Uri Uri()
        {
            return (iUri);
        }

        private string iTitle;
        private string iSubTitle;
        private Uri iUri;
    }
}
