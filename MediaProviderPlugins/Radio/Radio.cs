using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Threading;

using Upnp;

using Linn.Kinsky;
using Linn;

[assembly: ContentDirectoryFactoryType("OssKinskyMppRadio.MediaProviderRadioFactory")]

namespace OssKinskyMppRadio
{
    public class MediaProviderRadioFactory : IMediaProviderFactoryV7
    {
        public IMediaProviderV7 Create(IMediaProviderSupportV7 aSupport)
        {
            return (new MediaProviderRadio(aSupport));
        }
    }

    public class MediaProviderRadio : IMediaProviderV7
    {
        public MediaProviderRadio(IMediaProviderSupportV7 aSupport)
        {
            iSupport = aSupport;

            iMutex = new Mutex(false);

            iIcon = OssKinskyMppRadio.Properties.Resources.Radio;
            iIconSelected = OssKinskyMppRadio.Properties.Resources.RadioActive;

            iLrf = new Lrf(iSupport.AppSupport.HttpClient);

            iLrf.EventChanged += OnFeedsChanged;

            iView = new View(iSupport, iLrf);

            iView.EventLocationChanged += OnLocationChanged;
        }

        public void Start()
        {
            LoadFeeds();
            iLrf.Start();
        }

        private void LoadDefaultFeeds()
        {
            iLrf.Add(new Uri("http://oss.linn.co.uk/Feeds/Radio/Npr.xml"), "NPR");
            iLrf.Add(new Uri("http://oss.linn.co.uk/Feeds/Radio/SomaFM.xml"), "SomaFM");
        }

        private string SaveFilePath
        {
            get
            {
                return (Path.Combine(iSupport.AppSupport.SavePath, "Radio.xml"));
            }
        }

        private void LoadFeeds()
        {
            try
            {
                FileStream file = new FileStream(SaveFilePath, FileMode.Open);
                XmlTextReader reader = new XmlTextReader(file);
                XPathDocument document = new XPathDocument(reader);
                XPathNavigator navigator = document.CreateNavigator();

                foreach (XPathNavigator f in navigator.Select("/radio/feed"))
                {
                    string title = f.SelectSingleNode("title").Value;

                    Uri uri;

                    try
                    {
                        uri = new Uri(f.SelectSingleNode("uri").Value);
                    }
                    catch (UriFormatException)
                    {
                        continue;
                    }

                    iLrf.Add(uri, title);
                }

                file.Close();
            }
            catch (Exception)
            {
                LoadDefaultFeeds();
            }
        }

        private void OnFeedsChanged(object obj, EventArgs e)
        {
            IList<IFeed> list = iLrf.FeedList;

            try
            {
                FileStream file = new FileStream(SaveFilePath, FileMode.Create);
                XmlTextWriter writer = new XmlTextWriter(file, Encoding.UTF8);

                writer.WriteStartElement("radio");

                foreach (IFeed f in list)
                {
                    writer.WriteStartElement("feed");
                    writer.WriteElementString("title", f.Title);
                    writer.WriteElementString("uri", f.Uri.AbsoluteUri);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.Flush();
                file.Close();
            }
            catch (Exception)
            {
            }
        }

        public void Stop()
        {
            iLrf.Stop();
        }

        public void Open()
        {
            iView.Open();

            if (EventSizeEnabled != null)
            {
                EventSizeEnabled(this, EventArgs.Empty);
            }

            if (EventViewEnabled != null)
            {
                EventViewEnabled(this, EventArgs.Empty);
            }
        }

        public void Close()
        {
            iView.Close();
        }

        public void Rescan() { }

        public string Name
        {
            get
            {
                return "Radio";
            }
        }

        public string Provider
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

                if (attributes.Length != 0)
                {
                    return ((AssemblyCompanyAttribute)attributes[0]).Company;
                }
                else
                {
                    return "";
                }
            }
        }

        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public Image Icon
        {
            get
            {
                return iIcon;
            }
        }

        public Image IconSelected
        {
            get
            {
                return iIconSelected;
            }
        }

        public Control Control
        {
            get
            {
                return (iView.ViewControl);
            }
        }

        public IViewUserOptionsPage OptionsPage
        {
            get
            {
                return null;
            }
        }

        public string[] Location
        {
            get
            {
                return iView.Path;
            }
            set
            {
                iView.Path = value;
            }
        }

        public void Up(uint aLevels)
        {
            iView.Up(aLevels);
        }

        public void OnSizeClick()
        {
            iView.OnSizeClick();
        }

        public void OnViewClick()
        {
            iView.OnViewClick();
        }

        private void OnLocationChanged(object obj, EventArgs e)
        {
            if (EventLocationChanged != null)
            {
                EventLocationChanged(this, e);
            }
        }

        IMediaProviderSupportV7 iSupport;

        public event EventHandler<EventArgs> EventSizeEnabled;
        public event EventHandler<EventArgs> EventSizeDisabled;
        public event EventHandler<EventArgs> EventViewEnabled;
        public event EventHandler<EventArgs> EventViewDisabled;
        public event EventHandler<EventArgs> EventLocationChanged;

        private Mutex iMutex;
        private Image iIcon;
        private Image iIconSelected;
        private Lrf iLrf;
        private IView iView;
    }
} 