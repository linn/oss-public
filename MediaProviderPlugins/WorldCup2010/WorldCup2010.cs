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

[assembly: ContentDirectoryFactoryType("OssKinskyMppWorldCup2010.MediaProviderWorldCup2010Factory")]

namespace OssKinskyMppWorldCup2010
{
    public class MediaProviderWorldCup2010Factory : IMediaProviderFactoryV7
    {
        public IMediaProviderV7 Create(IMediaProviderSupportV7 aSupport)
        {
            return (new MediaProviderWorldCup2010(aSupport));
        }
    }

    public class MediaProviderWorldCup2010 : IMediaProviderV7
    {
        public MediaProviderWorldCup2010(IMediaProviderSupportV7 aSupport)
        {
            iSupport = aSupport;

            iMutex = new Mutex(false);

            iIcon = OssKinskyMppWorldCup2010.Properties.Resources.WorldCup2010;
            iIconSelected = OssKinskyMppWorldCup2010.Properties.Resources.WorldCup2010Active;

            iView = new View(iSupport);

            iView.EventLocationChanged += OnLocationChanged;
        }

        public void Start()
        {
        }

        private string SaveFilePath
        {
            get
            {
                return (Path.Combine(iSupport.AppSupport.SavePath, "WorldCup2010.xml"));
            }
        }

        public void Stop()
        {
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

        public void Rescan()
        {
        }

        public string Name
        {
            get
            {
                return "World Cup 2010";
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
        private IView iView;
    }
} 