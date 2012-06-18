using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

using Linn;
using Linn.Kinsky;

namespace OssKinskyMppLastFm
{
    public class MediaProviderLastFmFactory : IMediaProviderFactoryV7
    {
        public IMediaProviderV7 Create(IMediaProviderSupportV7 aSupport)
        {
            return new MediaProviderLastFm(aSupport);
        }
    }

    public class MediaProviderLastFm : IMediaProviderV7
    {
        public MediaProviderLastFm(IMediaProviderSupportV7 aSupport)
        {
            iSupport = aSupport;

            iService = new LastFmService();

            iView = new ViewWidgetMediaProviderLastFm(aSupport, iService);

            iOptionsLastFm = new OptionsLastFm(aSupport.AppSupport.SavePath);
            iOptionsLastFm.EventUsernamePasswordChanged += EventUsernamePasswordChanged;

            EventUsernamePasswordChanged(this, EventArgs.Empty);
        }

        public void Start() { }

        public void Stop() { }

        public void Open()
        {
            iView.EventPlayNow += EventPlayNow;
            iView.EventPlayNext += EventPlayNext;
            iView.EventPlayLater += EventPlayLater;

            iView.Open();
        }

        public void Close()
        {
            iView.EventPlayNow -= EventPlayNow;
            iView.EventPlayNext -= EventPlayNext;
            iView.EventPlayLater -= EventPlayLater;

            iView.Close();
        }

        public void Rescan() { }

        public string Name
        {
            get
            {
                return "Last.Fm";
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
                return kLastFmLogo;
            }
        }

        public Image IconSelected
        {
            get
            {
                return kLastFmLogo;
            }
        }

        public Control Control
        {
            get
            {
                return iView.Control;
            }
        }

        public IViewUserOptionsPage OptionsPage
        {
            get
            {
                return iOptionsLastFm.OptionsPageLastFm;
            }
        }

        public string[] Location
        {
            get
            {
                return kLocation;
            }
            set
            {
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

        public event EventHandler<EventArgs> EventSizeEnabled;
        public event EventHandler<EventArgs> EventSizeDisabled;
        public event EventHandler<EventArgs> EventViewEnabled;
        public event EventHandler<EventArgs> EventViewDisabled;
        public event EventHandler<EventArgs> EventLocationChanged;

        private void EventUsernamePasswordChanged(object sender, EventArgs e)
        {
            iView.SetUsernameAndPassword(iOptionsLastFm.Username, iOptionsLastFm.Password);
        }

        private void EventPlayNow(object sender, EventArgsPlay e)
        {
            iSupport.PlaylistSupport.PlayNow(e.Retriever);
        }

        private void EventPlayNext(object sender, EventArgsPlay e)
        {
            iSupport.PlaylistSupport.PlayNext(e.Retriever);
        }

        private void EventPlayLater(object sender, EventArgsPlay e)
        {
            iSupport.PlaylistSupport.PlayLater(e.Retriever);
        }

        private readonly Image kLastFmLogo = OssKinskyMppLastFm.Properties.Resources.LastFmLogo;
        private readonly string[] kLocation = new string[] { };

        private IMediaProviderSupportV7 iSupport;

        private IView iView;
        private ILastFmService iService;
        private OptionsLastFm iOptionsLastFm;
    }
} // OssKinskyMppLastFm