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
    public class StationResonanceFM : IStation
    {
        public StationResonanceFM()
        {
            iLogo = new BitmapImage(new Uri("pack://application:,,,/ResonanceFM/ResonanceFM.png"));
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
                WebRequest request = WebRequest.Create("http://podcasts.resonancefm.com/feed");
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();
                XmlTextReader reader = new XmlTextReader(response.GetResponseStream());
                XPathDocument document = new XPathDocument(reader);
                XPathNavigator navigator = document.CreateNavigator();
                XmlNamespaceManager nsmanager = new XmlNamespaceManager(navigator.NameTable);

                nsmanager.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");

                iShows = new List<IShow>();

                iShows.Add(new ShowResonanceFM("Live", DateTime.Now.ToLongDateString(), new Uri("http://icecast.commedia.org.uk:8000/resonance_hi.mp3")));

                foreach (XPathNavigator n in navigator.Select("/rss/channel/item"))
                {
                    string title = n.SelectSingleNode("title").Value;
                    string[] parts = title.Split(new char[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        string encoded = n.SelectSingleNode("content:encoded", nsmanager).Value;
                        string[] split = encoded.Split(new char[] { '"' });
                        foreach (string s in split)
                        {
                            if (s.EndsWith(".mp3"))
                            {
                                iShows.Add(new ShowResonanceFM(parts[0], parts[1], new Uri(s)));
                                break;
                            }
                        }
                    }
                }
            }
            return (iShows);
        }

        private static readonly Color kFill = (Color)ColorConverter.ConvertFromString("#505050");
        private static readonly Color kBack = Colors.White;
        private static readonly Color kStroke = (Color)ColorConverter.ConvertFromString("#2d6980");

        private ImageSource iLogo;

        private List<IShow> iShows;
    }

    public class ShowResonanceFM : IShow
    {
        public ShowResonanceFM(string aName, string aDetails, Uri aUri)
        {
            iName = aName;
            iDetails = aDetails;
            iUri = aUri;
        }

        public string Name()
        {
            return (iName);
        }

        public string Details()
        {
            return (iDetails);
        }

        public Uri Uri()
        {
            return (iUri);
        }

        private string iName;
        private string iDetails;
        private Uri iUri;
    }
}
