using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

using Upnp;

using Linn.Kinsky;
using Linn;

[assembly: ContentDirectoryFactoryType("OssKinskyMppSoundcard.MediaProviderSoundcardFactory")]

namespace OssKinskyMppSoundcard
{
    public class MediaProviderSoundcardFactory : IMediaProviderFactoryV7
    {
        public IMediaProviderV7 Create(IMediaProviderSupportV7 aSupport)
        {
            return (new MediaProviderSoundcard(aSupport));
        }
    }

    public class MediaProviderSoundcard : IMediaProviderV7
    {
        public MediaProviderSoundcard(IMediaProviderSupportV7 aSupport)
        {
            iSupport = aSupport;

            iMutex = new Mutex(false);

            iIcon = OssKinskyMppSoundcard.Properties.Resources.Soundcard;
            iIconSelected = OssKinskyMppSoundcard.Properties.Resources.SoundcardActive;

            iDirectSound = new SoundDriver();

            iView = new View(iSupport, iDirectSound);

            iView.EventLocationChanged += OnLocationChanged;
        }

        public void Start()
        {
            iDirectSound.Start(iSupport.AppSupport.Interface);
        }

        private string SaveFilePath
        {
            get
            {
                return (Path.Combine(iSupport.AppSupport.SavePath, "Soundcard.xml"));
            }
        }

        public void Stop()
        {
            iDirectSound.Stop();
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
                return "Soundcard";
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
        private SoundDriver iDirectSound;
        private IView iView;
    }
} 