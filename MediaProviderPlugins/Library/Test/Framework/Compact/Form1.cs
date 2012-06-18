using System;
using System.Windows.Forms;
using OssKinskyMppLibrary;
using Linn.Kinsky;
using Linn.ControlPoint.Upnp;
using Linn.Control.Ssdp;
using System.Net;
using System.Drawing;

namespace TestOssKinskyMppLibraryCf
{
    public partial class Form1 : Form
    {
        private AppKinskyWinForm iApp;
        private EventServerUpnp iEventServer;
        private SsdpListenerMulticast iSsdpListener;
        private IMediaProviderV6 iLibrary;

        public Form1(AppKinskyWinForm aApp)
        {
            InitializeComponent();

            iApp = aApp;

            iEventServer = new EventServerUpnp();
            iSsdpListener = new SsdpListenerMulticast();
            IMediaProviderSupportV6 support = new MediaProviderSupport(new AppSupport(aApp, iEventServer, iSsdpListener), new ViewSupport(), new PlaylistSupport(), new ArtworkCache(ArtworkCache.ECacheSize.eSmall));

            MediaProviderLibraryFactory factory = new MediaProviderLibraryFactory();
            iLibrary = factory.Create(support);

            SuspendLayout();

            iLibrary.Control.Dock = DockStyle.Fill;
            //iLibrary.Control.Height = Height - button1.Height - button2.Height;

            Controls.Clear();

            Controls.Add(iLibrary.Control);
            Controls.Add(this.button2);
            Controls.Add(this.button1);

            ResumeLayout(false);

            iEventServer.Start(aApp.Interface);
            iSsdpListener.Start(aApp.Interface);
            iLibrary.Start();

            iLibrary.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            iLibrary.Up(1);
        }

        private void Form1_Closed(object sender, EventArgs e)
        {
            iLibrary.Close();

            iLibrary.Stop();
            iSsdpListener.Stop();
            iEventServer.Stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            iLibrary.Close();

            iLibrary.Stop();
            iSsdpListener.Stop();
            iEventServer.Stop();

            iEventServer.Start(iApp.Interface);
            iSsdpListener.Start(iApp.Interface);
            iLibrary.Start();

            iLibrary.Open();
        }
    }

    class MediaProviderSupport : IMediaProviderSupportV6
    {
        public MediaProviderSupport(IAppSupport aAppSupport, IViewSupport aViewSupport, IPlaylistSupport aPlaylistSupport, IArtworkCache aArtworkCache)
        {
            iAppSupport = aAppSupport;
            iViewSupport = aViewSupport;
            iPlaylistSupport = aPlaylistSupport;
            iArtworkCache = aArtworkCache;
        }

        public IAppSupport AppSupport
        {
            get
            {
                return iAppSupport;
            }
        }

        public IViewSupport ViewSupport
        {
            get
            {
                return iViewSupport;
            }
        }

        public ILocalPlaylistSupport SaveSupport
        {
            get { throw new NotImplementedException(); }
        }

        public IPlaylistSupport PlaylistSupport
        {
            get
            {
                return iPlaylistSupport;
            }
        }

        public IPluginSupport PluginSupport
        {
            get { throw new NotImplementedException(); }
        }

        public IBrowserSupport BrowserSupport
        {
            get { throw new NotImplementedException(); }
        }

        public IArtworkCache ArtworkCache
        {
            get
            {
                return iArtworkCache;
            }
        }

        private IAppSupport iAppSupport;
        private IViewSupport iViewSupport;
        private IPlaylistSupport iPlaylistSupport;
        private IArtworkCache iArtworkCache;
    }

    class AppSupport : IAppSupport
    {
        public AppSupport(AppKinskyWinForm aAppKinsky, IEventUpnpProvider aEventUpnpProvider, ISsdpNotifyProvider aSsdpNotifyProvider)
        {
            iAppKinsky = aAppKinsky;
            iEventUpnpProvider = aEventUpnpProvider;
            iSsdpNotifyProvider = aSsdpNotifyProvider;
        }

        public string Version
        {
            get
            {
                return iAppKinsky.Version;
            }
        }

        public string SavePath
        {
            get
            {
                return iAppKinsky.BasePath;
            }
        }

        public IEventUpnpProvider EventUpnpProvider
        {
            get
            {
                return iEventUpnpProvider;
            }
        }

        public ISsdpNotifyProvider SsdpNotifyProvider
        {
            get
            {
                return iSsdpNotifyProvider;
            }
        }

        public IPAddress Interface
        {
            get
            {
                return iAppKinsky.Interface;
            }
        }

        public IDropConverter DropConverter
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHttpClient HttpClient
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IVirtualFileSystem VirtualFileSystem
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private AppKinskyWinForm iAppKinsky;
        private IEventUpnpProvider iEventUpnpProvider;
        private ISsdpNotifyProvider iSsdpNotifyProvider;
    }

    class ViewSupport : IViewSupport
    {
        public ViewSupport()
        {
            iFontMedium = new Font("Tahoma", 10, FontStyle.Bold);
        }

        public Color BackColour
        {
            get
            {
                return Color.Black;
            }
        }

        public Color ForeColourMuted
        {
            get { throw new NotImplementedException(); }
        }

        public Color ForeColour
        {
            get
            {
                return Color.White;
            }
        }

        public Color ForeColourBright
        {
            get { throw new NotImplementedException(); }
        }

        public Color HighlightBackColour
        {
            get { throw new NotImplementedException(); }
        }

        public Color HighlightForeColour
        {
            get { throw new NotImplementedException(); }
        }

        public Font FontLarge
        {
            get { throw new NotImplementedException(); }
        }

        public Font FontMedium
        {
            get
            {
                return iFontMedium;
            }
        }

        public Font FontSmall
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<EventArgs> EventSupportChanged;

        private Font iFontMedium;
    }

    class PlaylistSupport : IPlaylistSupport
    {
        public void Open()
        {
            iOpen = true;
        }

        public void Close()
        {
            iOpen = false;
        }

        public bool IsOpen()
        {
            return iOpen;
        }

        public bool IsInserting()
        {
            return false;
        }

        public bool IsDragging()
        {
            return false;
        }

        public bool IsInsertAllowed()
        {
            return true;
        }

        public void SetDragging(bool aDragging)
        {
            throw new NotImplementedException();
        }

        public void SetInsertAllowed(bool aInsertAllowed)
        {
        }

        public void PlayNow(IMediaRetriever aMediaRetriever)
        {
        }

        public void PlayNext(IMediaRetriever aMediaRetriever)
        {
        }

        public void PlayLater(IMediaRetriever aMediaRetriever)
        {
        }

        public void PlayInsert(int aIndex, IMediaRetriever aMediaRetriever)
        {
        }

        public event EventHandler<EventArgs> EventIsOpenChanged;
        public event EventHandler<EventArgs> EventIsInsertingChanged;
        public event EventHandler<EventArgs> EventIsDraggingChanged;
        public event EventHandler<EventArgs> EventIsInsertAllowedChanged;

        public event EventHandler<EventArgsPlay> EventPlayNow;
        public event EventHandler<EventArgsPlay> EventPlayNext;
        public event EventHandler<EventArgsPlay> EventPlayLater;
        public event EventHandler<EventArgsInsert> EventPlayInsert;

        public event EventHandler<EventArgs> EventPlayNowRequest;
        public event EventHandler<EventArgs> EventPlayNextRequest;
        public event EventHandler<EventArgs> EventPlayLaterRequest;

        private bool iOpen;
    }
}