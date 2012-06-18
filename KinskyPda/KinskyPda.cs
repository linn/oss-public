using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;

using Linn;
using Linn.Kinsky;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Toolkit.WinForms;

using KinskyPda.Widgets;

namespace KinskyPda
{
    public partial class FormKinskyPda : Form, IView, IAppRestartHandler, IStack
    {
        private HelperKinsky iHelper;
        private Mediator iMediator;
        private HttpClient iHttpClient;
        private HttpServer iHttpServer;
        private ContentDirectoryLocator iLocator;
        private MediaProviderLibrary iLibrary;
        private BrowserItems iBrowserItems;

        private ViewSupport iViewSupport;
        private KinskyPda.PlaylistSupport iPlaySupport;

        private OptionBool iOptionPlaylistInfo;
        private OptionEnum iOptionVolumeStep;
        private OptionBool iOptionShowArtwork;
        private FormUserLog iFormUserLog;

        private UserControl iVisibleControl;


        public IViewWidgetSelector<Linn.Kinsky.Room> ViewWidgetSelectorRoom
        {
            get
            {
                return userControlSource1.ViewWidgetSelectorRoom;
            }
        }
        public IViewWidgetButton ViewWidgetButtonStandby
        {
            get
            {
                return userControlSource1.ViewWidgetButtonStandby;
            }
        }
        public IViewWidgetSelector<Linn.Kinsky.Source> ViewWidgetSelectorSource
        {
            get
            {
                return userControlSource1.ViewWidgetSelectorSource;
            }
        }
        public IViewWidgetVolumeControl ViewWidgetVolumeControl
        {
            get
            {
                return userControlPlay.ViewWidgetVolumeControl;
            }
        }
        public IViewWidgetMediaTime ViewWidgetMediaTime
        {
            get
            {
                return userControlPlay.ViewWidgetMediaTime;
            }
        }
        public IViewWidgetTransportControl ViewWidgetTransportControlMediaRenderer
        {
            get
            {
                return userControlPlay.ViewWidgetTransportControlPause;
            }
        }
        public IViewWidgetTransportControl ViewWidgetTransportControlDiscPlayer
        {
            get
            {
                return userControlPlay.ViewWidgetTransportControlStop;
            }
        }
        public IViewWidgetTransportControl ViewWidgetTransportControlRadio
        {
            get
            {
                return userControlPlay.ViewWidgetTransportControlStop;
            }
        }
        public IViewWidgetTrack ViewWidgetTrack
        {
            get
            {
                return userControlPlay.ViewWidgetTrack;
            }
        }
        public IViewWidgetPlayMode ViewWidgetPlayMode
        {
            get
            {
                return userControlPlay.ViewWidgetPlayMode;
            }
        }
        public IViewWidgetPlaylist ViewWidgetPlaylist
        {
            get
            {
                return userControlPlay.ViewWidgetPlaylist;
            }
        }
        public IViewWidgetPlaylistRadio ViewWidgetPlaylistRadio
        {
            get
            {
                return userControlPlay.ViewWidgetPlaylistRadio;
            }
        }
        public IViewWidgetPlaylistReceiver ViewWidgetPlaylistReceiver
        {
            get
            {
                return userControlPlay.ViewWidgetPlaylistReceiver;
            }
        }
        public IViewWidgetPlaylistAux ViewWidgetPlaylistAux
        {
            get
            {
                return userControlPlay.ViewWidgetPlaylistAux;
            }
        }
        public IViewWidgetPlaylistDiscPlayer ViewWidgetPlaylistDiscPlayer
        {
            get
            {
                return new ViewWidgetPlaylistDiscPlayer();
            }
        }

        public IViewWidgetButton ViewWidgetButtonSave
        {
            get
            {
                return new KinskyPda.Widgets.ViewWidgetButton();
            }
        }
        public IViewWidgetButton ViewWidgetButtonWasteBin
        {
            get
            {
                return new KinskyPda.Widgets.ViewWidgetButton();
            }
        }
        public IViewWidgetReceivers ViewWidgetReceivers
        {
            get
            {
                return new KinskyPda.Widgets.ViewWidgetReceivers();
            }
        }
        public IViewWidgetButton ViewWidgetButtonReceivers
        {
            get
            {
                return new KinskyPda.Widgets.ViewWidgetButton();
            }
        }


        public FormKinskyPda(HelperKinsky aHelper)
        {
            iHelper = aHelper;

            // create general options page
            iOptionPlaylistInfo = new OptionBool("playlistinfo", "Show extended playlist information", "Show extended playlist information", false);
            iOptionVolumeStep = new OptionEnum("volumestep", "Change volume in steps of", "Volume step size");
            iOptionVolumeStep.Add("1");
            iOptionVolumeStep.Add("2");
            iOptionVolumeStep.AddDefault("3");
            iOptionVolumeStep.Add("4");
            iOptionVolumeStep.Add("5");
            iOptionShowArtwork = new OptionBool("displayartwork", "Display artwork", "Display the album artwork while browsing media", false);
            OptionPage optionPageGeneral = new OptionPage("General");
            optionPageGeneral.Add(iOptionVolumeStep);
            optionPageGeneral.Add(iOptionPlaylistInfo);
            optionPageGeneral.Add(iOptionShowArtwork);
            iHelper.AddOptionPage(optionPageGeneral);

            // initialise form after options are created
            InitializeComponent();

            iHelper.EventErrorOccurred += CleanupOnError;

            iFormUserLog = new FormUserLog(null);


            // create stack objects
            iHttpClient = new HttpClient();
            iHttpServer = new HttpServer(HttpServer.kPortKinskyPda);

            iLibrary = new MediaProviderLibrary(iHelper);

            iViewSupport = new ViewSupport();
            iPlaySupport = new KinskyPda.PlaylistSupport(userControlBrowser1.ButtonPlayNow, userControlBrowser1.ButtonPlayLater);
            MediaProviderSupport mediaProviderSupport = new MediaProviderSupport(iHttpServer);

            Linn.Kinsky.Model model = new Linn.Kinsky.Model(this, iPlaySupport);
            iMediator = new Mediator(iHelper, model);

            PluginManager pluginManager = new PluginManager(iHelper, iHttpClient, mediaProviderSupport);
            iLocator = new ContentDirectoryLocator(pluginManager, this);
            iLocator.Add(MediaProviderLibrary.kLibraryId, iLibrary);


            // create the browser and its controllers 
            Browser browser = new Browser(new Location(iLocator.Root));
            iBrowserItems = new BrowserItems(browser);

            // create the location mediator
            MediatorLocation location = new MediatorLocation();
            // the location mediator events changes in the browser location
            // to any interested views
            location.Add(userControlBrowser1.ViewWidgetBreadcrumb);
            // the location mediator also acts as a controller to the other
            // views that can changed the browser location
            userControlBrowser1.ViewWidgetBreadcrumb.Controller = location;
            userControlBrowser1.ControllerLocation = location;

            // set the browser for the location mediator
            location.SetBrowser(browser);

            // create browser view
            ViewBrowser viewBrowser = new ViewBrowser(iBrowserItems, iPlaySupport, iViewSupport, iOptionShowArtwork);
            userControlBrowser1.SetViewBrowser(viewBrowser);

            iHelper.SetStackExtender(this);
            iHelper.Stack.SetStatusHandler(new StackStatusHandlerCompact(iHelper.Title));
        }

        private void FormLoad(object sender, EventArgs e)
        {
            userControlBrowser1.Load(iPlaySupport);
            userControlSource1.Load(iHelper);
            userControlPlay.Load(iOptionPlaylistInfo, iOptionVolumeStep);

            //initial view to display playing
            userControlPlay.SetPlayState("playing");

            iMenuItemLeft.Text = "Playlist";
            iMenuItemRight.Text = "Browser";

            iVisibleControl = userControlPlay;
            iVisibleControl.Visible = true;
            iVisibleControl.Focus();

            foreach (MenuItem item in userControlBrowser1.PContextMenu.MenuItems)
            {
                item.Click += new System.EventHandler(this.ContextMenu_Click);
            }

            foreach (MenuItem item in userControlSource1.PContextMenu.MenuItems)
            {
                item.Click += new System.EventHandler(this.ContextMenu_Click);
            }

            foreach (MenuItem item in userControlPlay.PContextMenu.MenuItems)
            {
                item.Click += new System.EventHandler(this.ContextMenu_Click);
            }

            iHelper.Stack.Start();
			
			// show the options dialog if specified by the user
            if (iHelper.Stack.StatusHandler.ShowOptions)
            {
            	iHelper.Stack.EventStatusChanged += iHelper.Stack.StatusHandler.StackStatusOptionsChanged;
                FormUserOptions form = new FormUserOptions(iHelper.OptionPages);
                form.SetTheme(new Theme());
                form.SetPageByName("Network");
                form.ShowDialog();
                form.Dispose();
                iHelper.Stack.EventStatusChanged -= iHelper.Stack.StatusHandler.StackStatusOptionsChanged;
            }
        }

        private void FormClosed(object sender, EventArgs e)
        {
            iHelper.Stack.Stop();
        }

        public void Start(IPAddress aIpAddress)
        {
            iMediator.Open();
            iLibrary.Start(aIpAddress);
            iHttpClient.Start();
            iHttpServer.Start(aIpAddress);
            iLocator.Start();
            iBrowserItems.Start();
        }

        public void Stop()
        {
            iBrowserItems.Stop();
            iLocator.Stop();
            iHttpServer.Stop();
            iHttpClient.Stop();
            iLibrary.Stop();
            iMediator.Close();
        }

        private void CleanupOnError(object sender, EventArgs e)
        {
            FormClosed(sender, e);
        }

        public void Restart()
        {
            MessageBox.Show("The application must be manually restarted to complete the plugin installation.", "Install warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
        }

        private void ContextMenu_Click(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;

            switch (item.Text)
            {
                case "Exit":
                    this.Close();
                    break;
                case "Room/Source":
                    SwitchToSource();
                    break;
                case "Debug":
                    {
                        if (iFormUserLog == null || iFormUserLog.IsDisposed)
                        {
                            iFormUserLog = new FormUserLog(null);
                        }
                        iFormUserLog.ShowDialog();
                    }
                    break;
                case "Options...":
                    {
                        FormUserOptions form = new FormUserOptions(iHelper.OptionPages);
                        form.SetTheme(new Theme());
                        form.ShowDialog();
                        form.Dispose();
                    }
                    break;
                case "About":
                    {
                        FormAboutBox form = new FormAboutBox(iHelper);
                        form.ShowDialog();
                        form.Dispose();
                    }
                    break;
            }
        }

        private void SwitchToSource()
        {
            if (iMenuItemLeft.Text == "Playlist" && iMenuItemRight.Text == "Browser")
            {
                iMenuItemLeft.Text = "Now Playing";
            }
            else if (iMenuItemLeft.Text == "Now Playing" && iMenuItemRight.Text == "Playlist")
            {
                iMenuItemLeft.Text = "Playlist";
                iMenuItemRight.Text = "Browser";
            }
            else if (iMenuItemLeft.Text == "Playlist" && iMenuItemRight.Text == "Now Playing")
            {
                iMenuItemLeft.Text = "Now Playing";
                iMenuItemRight.Text = "Browser";
            }
            else if (iMenuItemLeft.Text == "Now Playing" && iMenuItemRight.Text == "Browser")
            {
                iMenuItemLeft.Text = "Playlist";
                iMenuItemRight.Text = "Browser";
            }

            SwitchView(userControlSource1);
        }

        private void SwitchView(UserControl aControl)
        {
            iVisibleControl.Visible = false;
            iVisibleControl = aControl;
            iVisibleControl.Visible = true;

            iVisibleControl.Focus();
        }

        private void MenuItemRightClick(object sender, EventArgs e)
        {
            SuspendLayout();

            if (iMenuItemLeft.Text == "Playlist" && iMenuItemRight.Text == "Browser")
            {
                iMenuItemRight.Text = "Now Playing";
                SwitchView(userControlBrowser1);
            }
            else if (iMenuItemLeft.Text == "Playlist" && iMenuItemRight.Text == "Now Playing")
            {
                iMenuItemRight.Text = "Browser";
                userControlPlay.SetPlayState("playing");
                SwitchView(userControlPlay);
            }
            else if (iMenuItemLeft.Text == "Now Playing" && iMenuItemRight.Text == "Playlist")
            {
                iMenuItemRight.Text = "Browser";
                userControlPlay.SetPlayState("playlist");
                SwitchView(userControlPlay);
            }
            else if (iMenuItemLeft.Text == "Now Playing" && iMenuItemRight.Text == "Browser")
            {
                iMenuItemRight.Text = "Playlist";
                SwitchView(userControlBrowser1);
            }

            ResumeLayout(false);
        }

        private void MenuItemLeftClick(object sender, EventArgs e)
        {
            SuspendLayout();

            if (iMenuItemLeft.Text == "Playlist" && iMenuItemRight.Text == "Browser")
            {
                iMenuItemLeft.Text = "Now Playing";
                userControlPlay.SetPlayState("playlist");
                SwitchView(userControlPlay);
            }
            else if (iMenuItemLeft.Text == "Now Playing" && iMenuItemRight.Text == "Browser")
            {
                iMenuItemLeft.Text = "Playlist";
                userControlPlay.SetPlayState("playing");
                SwitchView(userControlPlay);
            }
            else if (iMenuItemLeft.Text == "Playlist" && iMenuItemRight.Text == "Now Playing")
            {
                iMenuItemLeft.Text = "Now Playing";
                iMenuItemRight.Text = "Browser";
                userControlPlay.SetPlayState("playlist");
                SwitchView(userControlPlay);
            }
            else if (iMenuItemLeft.Text == "Now Playing" && iMenuItemRight.Text == "Playlist")
            {
                iMenuItemLeft.Text = "Playlist";
                iMenuItemRight.Text = "Browser";
                userControlPlay.SetPlayState("playing");
                SwitchView(userControlPlay);
            }

            ResumeLayout(false);
        }
    }
}