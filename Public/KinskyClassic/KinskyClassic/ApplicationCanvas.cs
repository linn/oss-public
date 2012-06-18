using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System;
using System.IO;
using System.Net;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Kinsky;
using Linn.Topology;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using Linn.Toolkit.WinForms;

namespace KinskyClassic
{
    public partial class ApplicationCanvas : Form, IStack
    {
        private const string kCompatabilityFamily = "Cara";

        private HelperKinsky iHelper;
        private OptionPageGeneral iPage;
        private FormUserLog iFormUserLog;

        private HttpServer iHttpServer;
        private HttpClient iHttpClient;
        private MediaProviderLibrary iLibrary;
        private IContentDirectorySupportV2 iMediaProviderSupport;

        private bool iDeveloperMode = false;
        private CanvasGdiPlus iCanvas;

        private ContentDirectoryLocator iLocator;
        private ViewWidgetBrowser iViewWidgetBrowser;
        private Mediator iMediator;
        private View iView;

        private Node iMinimiseButton;
        private Node iExitButton;

        public ApplicationCanvas(HelperKinsky aHelper/*, string aWindowTitle, string aSoftwareVersion, string aPackageCache, string aTextureCache, bool aDeveloperMode, bool aMouse*/)
        {
            iHelper = aHelper;

            iPage = new OptionPageGeneral();
            iPage.EventFullscreenChanged += FullscreenChanged;
            iPage.EventHideMouseCursorChanged += HideMouseCursorChanged;
            iPage.EventSkinChanged += SkinChanged;

            iCanvas = new CanvasGdiPlus();
            new RendererGdiPlus(iCanvas);
            Trace.WriteLine(Trace.kKinskyClassic, "ApplicationCanvas: Exe dir: " + Application.StartupPath);
            TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Default"));
            
            TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/800x480"));
            PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/800x480/Laptop"));

            InitializeComponent();

            Icon = Icon.FromHandle(Linn.Kinsky.Properties.Resources.KinskyLogo.GetHicon());

            iHttpServer = new HttpServer(HttpServer.kPortKinskyClassic);
            iHttpClient = new HttpClient();
            iLibrary = new MediaProviderLibrary(aHelper);

            iMediaProviderSupport = new MediaProviderSupport(iHttpServer);

            SetSkin(iPage.Skin);
            SetFullscreen(iPage.Fullscreen);
            SetMouseCursor(iPage.HideMouseCursor);
            
            Text = iHelper.Product;

            Controls.Add(iCanvas);
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            UpdateStyles();

            PluginManager pluginManager = new PluginManager(iHelper, iHttpClient, iMediaProviderSupport);
            
            iLocator = new ContentDirectoryLocator(pluginManager, new AppRestartHandler());
            iLocator.Add(MediaProviderLibrary.kLibraryId, iLibrary);
            //iLocator.Add(iSaveSupport.ContentDirectoryId, iSaveSupport.ContentDirectoryRoot);

            MediatorLocation location = new MediatorLocation();

            ViewWidgetBreadcrumb breadcrumb = new ViewWidgetBreadcrumb(iCanvas.CurrLayout.Root, location, new ViewWidgetButtonUp(iCanvas.CurrLayout.Root));
            location.Add(breadcrumb);

            PlaySupport support = new PlaySupport();
            IBrowser b = new Browser(new Location(iLocator.Root));
            iViewWidgetBrowser = new ViewWidgetBrowser(iCanvas.CurrLayout.Root, support, b);

            location.SetBrowser(b);

            iMediator = new Mediator(iHelper, new Model(iView, support));

            /*if (aDeveloperMode)
            {
                Node versionText = layout.Root.Search("Main.VersionText");
                Node wibbleSpeed = layout.Root.Search("Main.WibbleSpeed");

                Messenger.Instance.PresentationMessage(new MsgSetText(versionText, aSoftwareVersion));
                Messenger.Instance.PresentationMessage(new MsgSetActive(versionText, true));
                Messenger.Instance.PresentationMessage(new MsgSetActive(wibbleSpeed, true));
            }*/

            iHelper.SetStackExtender(this);
            iHelper.Stack.SetStatusHandler(new StackStatusHandlerWinForms(iHelper.Title));
        }

        void IStack.Start(IPAddress aIpAddress)
        {
            iViewWidgetBrowser.Open();
            iMediator.Open();

            iLibrary.Start(aIpAddress);
            iHttpClient.Start();
            iHttpServer.Start(aIpAddress);
            iLocator.Start();
		}

        void IStack.Stop()
        {
            iLocator.Stop();
            iHttpServer.Stop();
            iHttpClient.Stop();
            iLibrary.Stop();

            iViewWidgetBrowser.Close();
            iMediator.Close();
        }

        private void ApplicationCanvas_Load(object sender, System.EventArgs e)
        {
            Trace.WriteLine(Trace.kKinskyClassic, ">ApplicationCanvas_Load");

            iHelper.Stack.Start();

            // show the options dialog if specified by the user
            if (iHelper.Stack.StatusHandler.ShowOptions)
            {
                ShowOptionsDialog(true);
            }
            
            iFadeInTimer.Start();
        }

        private void ApplicationCanvas_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Opacity > 0)
            {
                iHelper.Stack.Stop();

                iFadeOutTimer.Start();
                e.Cancel = true;
            }
            Trace.WriteLine(Trace.kKinskyClassic, ">ApplicationCanvas_Closing");
        }

        private void ApplicationCanvas_Deactivate(object sender, EventArgs e)
        {
            Trace.WriteLine(Trace.kKinskyClassic, ">ApplicationCanvas_Deactivate");
            iCanvas.Deactivate();
        }

        private void iFadeInTimer_Tick(object sender, EventArgs e)
        {
            if (Opacity + 0.1 >= 1)
            {
                Opacity = 1.0;
                iFadeInTimer.Stop();
            }
            else
            {
                Opacity += 0.2;
            }
        }

        private void iFadeOutTimer_Tick(object sender, EventArgs e)
        {
            Opacity -= 0.2;
            if (Opacity <= 0)
            {
                iFadeOutTimer.Stop();
                Close();
            }
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            iHelper.Rescan();
            iLocator.Refresh();
        }

        private void optionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowOptionsDialog(false);
        }

        private void ShowOptionsDialog(bool aStartOnNetwork)
        {
            // add a new stack status change handler while the options page  is visible
            // leave the default one so the balloon tips still appear
            iHelper.Stack.EventStatusChanged += iHelper.Stack.StatusHandler.StackStatusOptionsChanged;

            // show the dialog
            FormUserOptions form = new FormUserOptions(iHelper.OptionPages);
            form.Icon = System.Drawing.Icon.FromHandle(Linn.Kinsky.Properties.Resources.KinskyLogo.GetHicon());
            if (aStartOnNetwork)
            {
                form.SetPageByName("Network");
            }
            form.ShowDialog(this);

            // cleanup
            form.Dispose();
            iHelper.Stack.EventStatusChanged -= iHelper.Stack.StatusHandler.StackStatusOptionsChanged;
        }

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (iFormUserLog == null || iFormUserLog.IsDisposed)
            {
                iFormUserLog = new FormUserLog(Icon);
            }
            iFormUserLog.Show();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormAboutBox form = new FormAboutBox(iHelper);
            form.ShowDialog(this);
            form.Dispose();
        }

        private void HideMouseCursorChanged(object sender, EventArgs e)
        {
            SetMouseCursor(iPage.HideMouseCursor);
        }

        private void SetMouseCursor(bool aHide)
        {
            if (aHide)
            {
                Cursor.Hide();
            }
            else
            {
                Cursor.Show();
            }
        }

        /*private void AutoDetectChanged(object sender, EventArgs e)
        {
            if (iHelper.AutoDetect)
            {
                iLocator.Stop();
                iMediator.Close();

                PackageManager.Instance.RemovePath(PackageManager.Instance.PathList[0]);
                TextureManager.Instance.RemovePath(TextureManager.Instance.PathList[0]);

                AutoDetect();

                SetSkin(iPage.Skin);
                SetFullscreen(iPage.Fullscreen);
                SetMouseCursor(iPage.HideMouseCursor);

                iMediator = new Mediator(iHelper.StartupRoom, new Model(iView, iMediaProviderSupport), iEventServer, iListenerNotify, iHouse);
                iMediator.Open();

                iLocator = new OssKinskyMppLibrary.MediaProviderLibrary(iMediaProviderSupport, new OssKinskyMppLibrary.UserOptions(iMediaProviderSupport), new ViewOssKinskyMppLibrary(iCanvas.CurrLayout.Root, iMediaProviderSupport));
                iLocator.Start();
            }
        }*/

        private void AutoDetect()
        {
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            if (rect.Width == 800 && rect.Height == 480)
            {
                iPage.Skin = OptionPageGeneral.k800x480Touch;
                iPage.Fullscreen = true;
                iPage.HideMouseCursor = true;
            }
            else if (rect.Width == 1024 && rect.Height == 600)
            {
                iPage.Skin = OptionPageGeneral.k1024x600Touch;
                iPage.Fullscreen = true;
                iPage.HideMouseCursor = true;
            }
            else
            {
                iPage.Skin = OptionPageGeneral.k800x480Laptop;
                iPage.Fullscreen = false;
                iPage.HideMouseCursor = false;
            }
        }

        private void FullscreenChanged(object sender, EventArgs e)
        {
            SetFullscreen(iPage.Fullscreen);
        }

        private void SetFullscreen(bool aFullscreen)
        {
            if (aFullscreen)
            {
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.None;

                Messenger.Instance.PresentationMessage(new MsgSetActive(iMinimiseButton, true));
                Messenger.Instance.PresentationMessage(new MsgSetActive(iExitButton, true));
            }
            else
            {
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.FixedSingle;
                ControlBox = true;
                ClientSize = new System.Drawing.Size(iCanvas.ClientSize.Width, iCanvas.ClientSize.Height);

                Messenger.Instance.PresentationMessage(new MsgSetActive(iMinimiseButton, false));
                Messenger.Instance.PresentationMessage(new MsgSetActive(iExitButton, false));
            }
        }

        private void SkinChanged(object sender, EventArgs e)
        {
            //Hide();

            /*iLocator.Stop();
            iMediator.Close();*/

            PackageManager.Instance.RemovePath(PackageManager.Instance.PathList[0]);
            TextureManager.Instance.RemovePath(TextureManager.Instance.PathList[0]);

            SetSkin(iPage.Skin);
            SetFullscreen(iPage.Fullscreen);
            SetMouseCursor(iPage.HideMouseCursor);

            /*iMediator = new Mediator(iHelper.StartupRoom, new Model(iView, iMediaProviderSupport), iEventServer, iListenerNotify, iHouse);
            iMediator.Open();

            iLocator = new OssKinskyMppLibrary.MediaProviderLibrary(iMediaProviderSupport, new OssKinskyMppLibrary.UserOptions(iMediaProviderSupport), new ViewOssKinskyMppLibrary(iCanvas.CurrLayout.Root, iMediaProviderSupport));
            iLocator.Start();*/

            //Show();
        }

        private void SetSkin(string aSkin)
        {
            switch (aSkin)
            {
            case OptionPageGeneral.kAutoDetect:
                AutoDetect();
                break;

            case OptionPageGeneral.k800x480Touch:
                PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/800x480/Touchscreen"));
                TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/800x480"));

                PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "Skins/Klimax/800x480/Touchscreen"));
                TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "Skins/Klimax/800x480"));
                break;

            case OptionPageGeneral.k800x480Laptop:
                PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/800x480/Laptop"));
                TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/800x480"));

                PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "Skins/Klimax/800x480/Laptop"));
                TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "Skins/Klimax/800x480"));
                break;

            case OptionPageGeneral.k1024x600Touch:
                PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/1024x600/Touchscreen"));
                TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/1024x600"));

                PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "Skins/Klimax/1024x600/Touchscreen"));
                TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "Skins/Klimax/1024x600"));
                break;

            case OptionPageGeneral.k1024x600Laptop:
                PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/1024x600/Laptop"));
                TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/1024x600"));

                PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "Skins/Klimax/1024x600/Laptop"));
                TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "Skins/Klimax/1024x600"));
                break;
            }

            TextureManager.Instance.Refresh();

            iCanvas.Load("Main.xml");
            Package layout = iCanvas.CurrLayout;

            iMinimiseButton = layout.Root.Search("Main.MinimiseButton");
            iExitButton = layout.Root.Search("Main.ExitButton");

            ClientSize = new System.Drawing.Size(iCanvas.ClientSize.Width, iCanvas.ClientSize.Height);

            if (iView != null)
            {
                iView.Dispose();
            }

            iView = new View(iCanvas);
        }
    }
} // KinskyClassic

