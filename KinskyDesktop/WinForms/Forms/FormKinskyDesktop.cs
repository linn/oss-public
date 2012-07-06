using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Diagnostics;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Topology;
using Linn.Kinsky;
using Linn.Toolkit.WinForms;

namespace KinskyDesktop
{
    partial class FormKinskyDesktop : Form, IStack
    {
        private const string kOnlineManualUrl = "http://oss.linn.co.uk/trac/wiki/KinskyDesktopDavaarManual";
        private const string kPluginsUrl = "http://oss.linn.co.uk/Feeds/Plugins/KinskyPluginsV8.html";

        private HelperKinsky iHelper;

        private Image kHeaderLeft;
        private Image kHeaderRight;
        private Image kHeaderFiller;
        
        private Image kFooterLeft;
        private Image kFooterRight;
        private Image kFooterFiller;

        private Image kRightEdgeFiller;
        private Image kLeftEdgeFiller;

        private Image kMiniLeft;
        private Image kMiniRight;
        private Image kMiniFiller;

        private TextureBrush iBrushHeaderFiller;
        private TextureBrush iBrushLeftFiller;
        private TextureBrush iBrushRightFiller;
        private TextureBrush iBrushFooterFiller;
        private TextureBrush iBrushMiniFiller;

        private bool iDragging;
        private Point iLastMouseLocation;
        private Size iStartSize;

        private FormStatus iViewStatus;
        private uint iCurrentView;

        private HttpClient iHttpClient;
        private HttpServer iHttpServer;
        private ViewSupport iViewSupport;
        private PlaySupport iPlaySupport;
        private SaveSupport iSaveSupport;
        private ArtworkCache iArtworkCache;

        private MediaProviderLibrary iLibrary;
        private LocalPlaylists iLocalPlaylists;
        private SharedPlaylists iSharedPlaylists;

        private FormUserLog iFormUserLog;

        private View iView;
        private Model iModel;
        private Mediator iMediator;
        private ContentDirectoryLocator iLocator;
        private ViewSaveSupport iViewSaveSupport;

        private UiOptions iUiOptions;
        private OptionPageGeneral iOptionPageGeneral;
        private OptionPageColours iOptionPageColours;
        private OptionPageFonts iOptionPageFonts;

        private ReceiverSourceList iSourceList;

        public FormKinskyDesktop(HelperKinsky aHelperKinsky)
        {
            iCurrentView = 0;

            iHelper = aHelperKinsky;

            KinskyDesktop.Properties.Resources.SetBasePath(iHelper.ExePath.FullName);

            InitializeComponent();

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            }

            Icon = Icon.FromHandle(Linn.Kinsky.Properties.Resources.KinskyLogo.GetHicon());

            SplitContainer.Panel1MinSize = 200;
            SplitContainer.Panel2MinSize = 200;
            SplitContainer.SplitterDistance = (int)(Width * 0.5f);

            Text = iHelper.Product;

            notifyIcon.Text = iHelper.Product;
            try
            {
                notifyIcon.Icon = Icon;
            }
            catch (NotImplementedException) { } // thrown on MacOSX

            kHeaderLeft = KinskyDesktop.Properties.Resources.Wing1;
            kHeaderRight = KinskyDesktop.Properties.Resources.Wing2;
            kHeaderFiller = KinskyDesktop.Properties.Resources.FillerTop;
            
            kFooterLeft = KinskyDesktop.Properties.Resources.Wing3;
            kFooterRight = KinskyDesktop.Properties.Resources.Wing4;
            kFooterFiller = KinskyDesktop.Properties.Resources.FillerBottom;

            kRightEdgeFiller = KinskyDesktop.Properties.Resources.RightEdgeFiller;
            kLeftEdgeFiller = KinskyDesktop.Properties.Resources.LeftEdgeFiller;

            kMiniLeft = KinskyDesktop.Properties.Resources.MiniWing1;
            kMiniRight = KinskyDesktop.Properties.Resources.MiniWing2;
            kMiniFiller = KinskyDesktop.Properties.Resources.MiniFiller;

            iBrushHeaderFiller = new TextureBrush(kHeaderFiller);
            iBrushLeftFiller = new TextureBrush(kLeftEdgeFiller);
            iBrushLeftFiller.TranslateTransform(0, kHeaderLeft.Height);
            iBrushRightFiller = new TextureBrush(kRightEdgeFiller);
            iBrushFooterFiller = new TextureBrush(kFooterFiller);
            iBrushMiniFiller = new TextureBrush(kMiniFiller);

            iHttpClient = new HttpClient();
            iHttpServer = new HttpServer(HttpServer.kPortKinskyDesktop);

            iUiOptions = new UiOptions(iHelper);

            iOptionPageGeneral = new OptionPageGeneral();
            iHelper.AddOptionPage(iOptionPageGeneral);

            iOptionPageColours = new OptionPageColours();
            iHelper.AddOptionPage(iOptionPageColours);

            iOptionPageFonts = new OptionPageFonts();
            iHelper.AddOptionPage(iOptionPageFonts);

            iArtworkCache = new ArtworkCache(ArtworkCache.ECacheSize.eMedium);

            iViewSupport = new ViewSupport(iOptionPageColours, iOptionPageFonts);

            iLibrary = new MediaProviderLibrary(iHelper);
            iLocalPlaylists = new LocalPlaylists(iHelper, true);
            iSharedPlaylists = new SharedPlaylists(iHelper);

            MediaProviderSupport mediaProviderSupport = new MediaProviderSupport(iHttpServer);

            iOptionPageGeneral.EventTransparencyChanged += TransparencyOrBorderChanged;
            iOptionPageGeneral.EventWindowBorderChanged += TransparencyOrBorderChanged;
            iOptionPageGeneral.EventHideMouseCursorChanged += HideMouseCursorChanged;
            iOptionPageGeneral.EventUseRotaryControlsChanged += UseRotaryControlsChanged;
            iOptionPageGeneral.EventShowToolTipsChanged += ShowToolTipsChanged;
            iViewSupport.EventSupportChanged += EventSupportChanged;

            iFormUserLog = new FormUserLog(Icon);

            iViewStatus = new FormStatus(this, iArtworkCache, iViewSupport);
            iViewStatus.UseTheme = false;
            iViewStatus.Icon = Icon;
            iViewStatus.Visible = false;
            iViewStatus.TopLevel = false;
            iViewStatus.FormBorderStyle = FormBorderStyle.None;
            iViewStatus.MouseMove += FormKinskyDesktop_MouseMove;
            Controls.Add(iViewStatus);

            PluginManager pluginManager = new PluginManager(iHelper, iHttpClient, mediaProviderSupport);
            iLocator = new ContentDirectoryLocator(pluginManager, new AppRestartHandler());
            iLocator.Add(MediaProviderLibrary.kLibraryId, iLibrary);
            OptionBool optionLocalPlaylists = iLocator.Add(LocalPlaylists.kRootId, iLocalPlaylists);
            OptionBool optionSharedPlaylists = iLocator.Add(SharedPlaylists.kRootId, iSharedPlaylists);

            iSaveSupport = new SaveSupport(iHelper, iSharedPlaylists, optionSharedPlaylists, iLocalPlaylists, optionLocalPlaylists);
            iViewSaveSupport = new ViewSaveSupport(RequestLocalPlaylistFilename, iSaveSupport);

            iPlaySupport = new PlaySupport();

            // create the DropConverter for the browse views
            DropConverter browseDropConverter = new DropConverter();
            browseDropConverter.Add(new DropConverterInternal());
            browseDropConverter.Add(new DropConverterUri());
            browseDropConverter.Add(new DropConverterFileDrop(iHttpServer, false));
            browseDropConverter.Add(new DropConverterText());

            MediatorLocation location = new MediatorLocation();

            MediatorTab tab = new MediatorTab(this, location);

            IBrowser b = new Browser(new Location(iLocator));
            tab.Add(new ViewWidgetTabPage(this, 0), new ViewWidgetBrowser(this, b, iArtworkCache, iViewSupport, iPlaySupport, browseDropConverter, iUiOptions), b);
            b = new Browser(new Location(iLocator));
            tab.Add(new ViewWidgetTabPage(this, 1), new ViewWidgetBrowser(this, b, iArtworkCache, iViewSupport, iPlaySupport, browseDropConverter, iUiOptions), b);
            b = new Browser(new Location(iLocator));
            tab.Add(new ViewWidgetTabPage(this, 2), new ViewWidgetBrowser(this, b, iArtworkCache, iViewSupport, iPlaySupport, browseDropConverter, iUiOptions), b);
            b = new Browser(new Location(iLocator));
            tab.Add(new ViewWidgetTabPage(this, 3), new ViewWidgetBrowser(this, b, iArtworkCache, iViewSupport, iPlaySupport, browseDropConverter, iUiOptions), b);
            
            location.Add(tab);

            ViewWidgetBreadcrumb breadcrumb = new ViewWidgetBreadcrumb(this, iViewSupport, location, new ViewWidgetButtonUp(this));
            location.Add(breadcrumb);

            // create the DropConverter for the other views
            DropConverter viewDropConverter = new DropConverter();
            viewDropConverter.Add(new DropConverterInternal());
            viewDropConverter.Add(new DropConverterUri());
            viewDropConverter.Add(new DropConverterFileDrop(iHttpServer, true));
            viewDropConverter.Add(new DropConverterText());

            iSourceList = new ReceiverSourceList(iHelper);

            iView = new View(this, iArtworkCache, iViewSupport, iPlaySupport, iViewSaveSupport, viewDropConverter, iSourceList, iHelper.Senders);
            iModel = new Model(iView, iPlaySupport);

            iMediator = new Mediator(iHelper, iModel);

            iHelper.SetStackExtender(this);
            iHelper.Stack.SetStatusHandler(new StackStatusHandlerWinForms(iHelper.Title, notifyIcon));
        }

        public FormStatus ViewStatus
        {
            get
            {
                return iViewStatus;
            }
        }

        public OptionPageGeneral OptionPageGeneral
        {
            get { return iOptionPageGeneral; }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                iLocator.Refresh();
                iHelper.Rescan();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void TransparencyOrBorderChanged(object sender, EventArgs e)
        {
            FormBorderStyle current = FormBorderStyle;
            if (!iOptionPageGeneral.Transparency)
            {
                FormBorderStyle = iOptionPageGeneral.WindowBorder ? FormBorderStyle.Sizable : FormBorderStyle.None;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
            }

            if (current != FormBorderStyle)
            {
                if (FormBorderStyle == FormBorderStyle.None)
                {
                    MouseMove += FormKinskyDesktop_MouseMove;
                    MouseDown += FormKinskyDesktop_MouseDown;
                    MouseUp += FormKinskyDesktop_MouseUp;
                    Leave += FormKinskyDesktop_Leave;

                    RotaryControlVolume.MouseMove += FormKinskyDesktop_MouseMove;
                    RotaryControlVolume.MouseDown += FormKinskyDesktop_MouseDown;
                    RotaryControlVolume.MouseUp += FormKinskyDesktop_MouseUp;

                    RockerControlVolume.MouseMove += FormKinskyDesktop_MouseMove;
                    RockerControlVolume.MouseDown += FormKinskyDesktop_MouseDown;
                    RockerControlVolume.MouseUp += FormKinskyDesktop_MouseUp;

                    RotaryControlTracker.MouseMove += FormKinskyDesktop_MouseMove;
                    RotaryControlTracker.MouseDown += FormKinskyDesktop_MouseDown;
                    RotaryControlTracker.MouseUp += FormKinskyDesktop_MouseUp;

                    RockerControlTracker.MouseMove += FormKinskyDesktop_MouseMove;
                    RockerControlTracker.MouseDown += FormKinskyDesktop_MouseDown;
                    RockerControlTracker.MouseUp += FormKinskyDesktop_MouseUp;

                    ThreekArrayControl.MouseMove += FormKinskyDesktop_MouseMove;
                    ThreekArrayControl.MouseDown += FormKinskyDesktop_MouseDown;
                    ThreekArrayControl.MouseUp += FormKinskyDesktop_MouseUp;

                    PanelTrackInfo.MouseMove += FormKinskyDesktop_MouseMove;
                    PanelTrackInfo.MouseDown += FormKinskyDesktop_MouseDown;
                    PanelTrackInfo.MouseUp += FormKinskyDesktop_MouseUp;

                    PanelSizer.Enabled = true;
                }
                else
                {
                    MouseMove -= FormKinskyDesktop_MouseMove;
                    MouseDown -= FormKinskyDesktop_MouseDown;
                    MouseUp -= FormKinskyDesktop_MouseUp;
                    Leave -= FormKinskyDesktop_Leave;

                    RotaryControlVolume.MouseMove -= FormKinskyDesktop_MouseMove;
                    RotaryControlVolume.MouseDown -= FormKinskyDesktop_MouseDown;
                    RotaryControlVolume.MouseUp -= FormKinskyDesktop_MouseUp;

                    RockerControlVolume.MouseMove -= FormKinskyDesktop_MouseMove;
                    RockerControlVolume.MouseDown -= FormKinskyDesktop_MouseDown;
                    RockerControlVolume.MouseUp -= FormKinskyDesktop_MouseUp;

                    RotaryControlTracker.MouseMove -= FormKinskyDesktop_MouseMove;
                    RotaryControlTracker.MouseDown -= FormKinskyDesktop_MouseDown;
                    RotaryControlTracker.MouseUp -= FormKinskyDesktop_MouseUp;

                    RockerControlTracker.MouseMove -= FormKinskyDesktop_MouseMove;
                    RockerControlTracker.MouseDown -= FormKinskyDesktop_MouseDown;
                    RockerControlTracker.MouseUp -= FormKinskyDesktop_MouseUp;

                    ThreekArrayControl.MouseMove -= FormKinskyDesktop_MouseMove;
                    ThreekArrayControl.MouseDown -= FormKinskyDesktop_MouseDown;
                    ThreekArrayControl.MouseUp -= FormKinskyDesktop_MouseUp;

                    PanelTrackInfo.MouseMove -= FormKinskyDesktop_MouseMove;
                    PanelTrackInfo.MouseDown -= FormKinskyDesktop_MouseDown;
                    PanelTrackInfo.MouseUp -= FormKinskyDesktop_MouseUp;

                    PanelSizer.Enabled = false;
                }
            }
            LayoutFormKinskyDesktop();
        }

        private void HideMouseCursorChanged(object sender, EventArgs e)
        {
            if (iOptionPageGeneral.HideMouseCursor)
            {
                Cursor.Hide();
            }
            else
            {
                Cursor.Show();
            }
        }

        private void UseRotaryControlsChanged(object sender, EventArgs e)
        {
            RotaryControlVolume.Visible = iOptionPageGeneral.UseRotaryControls;
            RotaryControlTracker.Visible = iOptionPageGeneral.UseRotaryControls;
            RockerControlVolume.Visible = !iOptionPageGeneral.UseRotaryControls;
            RockerControlTracker.Visible = !iOptionPageGeneral.UseRotaryControls;
        }

        private void ShowToolTipsChanged(object sender, EventArgs e)
        {
            ToolTip.Active = iOptionPageGeneral.ShowToolTips;
        }

        private void FormKinskyDesktop_Load(object sender, EventArgs e)
        {
            UseRotaryControlsChanged(this, EventArgs.Empty);
            TransparencyOrBorderChanged(this, EventArgs.Empty);
            ShowToolTipsChanged(this, EventArgs.Empty);

            SetMiniMode(iUiOptions.MiniMode, true);

            Location = iUiOptions.WindowLocation;
        }

        void IStack.Start(IPAddress aIpAddress)
        {
            iMediator.Open();
            iSourceList.Start();
            iLibrary.Start(aIpAddress);
            iSharedPlaylists.Start(aIpAddress);
            iHttpClient.Start();
            iHttpServer.Start(aIpAddress);
            iLocator.Start();
        }

        void IStack.Stop()
        {
            iLocator.Stop();
            iHttpServer.Stop();
            iHttpClient.Stop();
            iSharedPlaylists.Stop();
            iLibrary.Stop();
            iSourceList.Stop();
            iMediator.Close();
        }

        private void FormKinskyDesktop_Shown(object sender, EventArgs e)
        {
            SplitContainer.SplitterDistance = iUiOptions.SplitterLocation;
            SplitContainer.SplitterMoved += splitContainer1_SplitterMoved;

            EventSupportChanged(null, EventArgs.Empty);

            iHelper.Stack.Start();

            // show the options dialog if specified by the user
            if (iHelper.Stack.StatusHandler.ShowOptions)
            {
                ShowOptionsDialog(true);
            }
        }

        private void FormKinskyDesktop_FormClosed(object sender, FormClosedEventArgs e)
        {
            iHelper.Stack.Stop();
        }

        private void FormKinskyDesktop_Resize(object sender, EventArgs e)
        {
            if (iUiOptions.MiniMode)
            {
                iUiOptions.MiniModeWidth = Width;
            }
            else if(WindowState != FormWindowState.Maximized)
            {
                iUiOptions.WindowSize = new Size(Width, Height);
            }

            if (iCurrentView < 3)
            {
                iUiOptions.Fullscreen = (WindowState == FormWindowState.Maximized);
            }

            LayoutFormKinskyDesktop();
            SetMaximiseOrRestore();
            SetControlButtonsVisible(PointToClient(MousePosition));
        }

        private void FormKinskyDesktop_Move(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                if (iUiOptions.MiniMode)
                {
                    iUiOptions.MiniModeLocation = Location;
                }
                else
                {
                    iUiOptions.WindowLocation = Location;
                }
            }
        }

        private void FormKinskyDesktop_Leave(object sender, EventArgs e)
        {
            iDragging = false;
        }

        private void RequestLocalPlaylistFilename(ISaveSupport aSaveSupport)
        {
            try
            {
                FormSavePlaylist dlg = new FormSavePlaylist(aSaveSupport);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    aSaveSupport.Save(dlg.Filename, string.Empty, 0);
                }
            }
            catch (Playlist.SaveException exc)
            {
                UserLog.WriteLine(DateTime.Now + ": Could not save playlist file: " + exc);

                MessageBox.Show("Could not create the playlist file: " + exc.Filename, "Error saving playlist", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
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
            if (aStartOnNetwork)
            {
                form.SetPageByName("Network");
            }
            form.ShowDialog(this);

            // cleanup
            form.Dispose();
            PanelBrowser.Invalidate();
            iHelper.Stack.EventStatusChanged -= iHelper.Stack.StatusHandler.StackStatusOptionsChanged;
        }

        private void onlineHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(kOnlineManualUrl);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to contact " + kOnlineManualUrl, "Online Help Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KinskyDesktop.FormAboutBox about = new KinskyDesktop.FormAboutBox(iHelper);
            about.ShowDialog();
            about.Dispose();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            iHelper.Rescan();
            iLocator.Refresh();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (iCurrentView < 3 || iUiOptions.MiniMode)
            {
                base.OnPaintBackground(e);

                if (!iOptionPageGeneral.Transparency)
                {
                    e.Graphics.Clear(iViewSupport.BackColour);
                }
                else if (WindowState == FormWindowState.Maximized)
                {
                    e.Graphics.Clear(iViewSupport.BackColour);
                }

                if (iUiOptions.MiniMode)
                {
                    e.Graphics.DrawImage(kMiniLeft, 0, 0);
                    e.Graphics.DrawImage(kMiniRight, ClientRectangle.Width - kMiniRight.Width, 0);

                    int width = ClientRectangle.Width - kMiniLeft.Width - kMiniRight.Width;
                    if (width > 0)
                    {
                        e.Graphics.FillRectangle(iBrushMiniFiller, kMiniLeft.Width, 0, width, kMiniFiller.Height);
                    }
                }
                else
                {
                    // draw the header
                    e.Graphics.DrawImage(kHeaderLeft, 0, 0);
                    e.Graphics.DrawImage(kHeaderRight, ClientRectangle.Width - kHeaderRight.Width, 0);

                    // draw the footer
                    e.Graphics.DrawImage(kFooterLeft, 0, ClientRectangle.Height - kFooterLeft.Height);
                    e.Graphics.DrawImage(kFooterRight, ClientRectangle.Width - kFooterRight.Width, ClientRectangle.Height - kFooterRight.Height);

                    int width = ClientRectangle.Width - kHeaderLeft.Width - kHeaderRight.Width;
                    if (width > 0)
                    {
                        e.Graphics.FillRectangle(iBrushHeaderFiller, kHeaderLeft.Width, 0, width, kHeaderFiller.Height);
                    }

                    width = ClientRectangle.Width - kFooterLeft.Width - kFooterRight.Width;
                    if (width > 0)
                    {
                        iBrushFooterFiller.ResetTransform();
                        iBrushFooterFiller.TranslateTransform(kFooterLeft.Width, ClientRectangle.Height - kFooterFiller.Height);
                        e.Graphics.FillRectangle(iBrushFooterFiller, kFooterLeft.Width, ClientRectangle.Height - kFooterFiller.Height, width, kFooterFiller.Height);
                    }

                    // draw the side bars
                    int height = ClientRectangle.Height - kHeaderLeft.Height - kFooterFiller.Height;
                    if (height > 0)
                    {
                        e.Graphics.FillRectangle(iBrushLeftFiller, 0, kHeaderLeft.Height, kLeftEdgeFiller.Width, height);
                        iBrushRightFiller.ResetTransform();
                        iBrushRightFiller.TranslateTransform(ClientRectangle.Width - kRightEdgeFiller.Width, kHeaderRight.Height);
                        e.Graphics.FillRectangle(iBrushRightFiller, ClientRectangle.Width - kRightEdgeFiller.Width, kHeaderRight.Height, kRightEdgeFiller.Width, height);
                    }
                }
            }
            else
            {
                e.Graphics.Clear(Color.Black);
            }
        }

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (iFormUserLog.IsDisposed)
            {
                iFormUserLog = new FormUserLog(Icon);
            }
            iFormUserLog.SetBackColour(iViewSupport.BackColour);
            iFormUserLog.SetForeColour(iViewSupport.ForeColour);
            iFormUserLog.SetFont(iViewSupport.FontSmall);
            iFormUserLog.Show();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (iCurrentView < 3)
            {
                iUiOptions.SplitterLocation = SplitContainer.SplitterDistance;
            }
        }

        private void FormKinskyDesktop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                iDragging = true;
                iLastMouseLocation = PointToScreen(new Point(e.X, e.Y));
            }
        }

        private void FormKinskyDesktop_MouseMove(object sender, MouseEventArgs e)
        {
            if (iDragging)
            {
                Point current = PointToScreen(new Point(e.X, e.Y));
                MoveForm(current);
            }

            if (sender == this || sender == iViewStatus)
            {
                SetControlButtonsVisible(e.Location);
            }
        }

        private void MoveForm(Point aLocation)
        {
            Point delta = new Point(aLocation.X - iLastMouseLocation.X, aLocation.Y - iLastMouseLocation.Y);
            iLastMouseLocation = aLocation;
            Point location = new Point(Location.X + delta.X, Location.Y + delta.Y);

            if (iUiOptions.MiniMode)
            {
                if (location.X < 0)
                {
                    location.X = 0;
                }
                else if (location.X + Width > Screen.GetWorkingArea(this).Width && Screen.AllScreens.Length < 2)
                {
                    location.X = Screen.GetWorkingArea(this).Width - Width;
                }

                if (location.Y < 0)
                {
                    location.Y = 0;
                }
                else if (location.Y + Height > Screen.GetWorkingArea(this).Height)
                {
                    location.Y = Screen.GetWorkingArea(this).Height - Height;
                }
            }

            Location = location;
        }

        private void FormKinskyDesktop_MouseUp(object sender, MouseEventArgs e)
        {
            iDragging = false;
        }

        private void PanelSizer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                iDragging = true;
                iLastMouseLocation = PanelSizer.PointToScreen(new Point(e.X, e.Y));
                iStartSize = Size;
            }
        }

        private void PanelSizer_MouseMove(object sender, MouseEventArgs e)
        {
            if (iDragging)
            {
                Point current = PanelSizer.PointToScreen(new Point(e.X, e.Y));
                Point delta = new Point(current.X - iLastMouseLocation.X, current.Y - iLastMouseLocation.Y);

                Size = new Size(iStartSize.Width + delta.X, iStartSize.Height + delta.Y);
            }
        }

        private void PanelSizer_MouseUp(object sender, MouseEventArgs e)
        {
            iDragging = false;
        }

        private void miniModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMiniMode(miniModeToolStripMenuItem.Checked);
        }

        private void ButtonMini_Click(object sender, EventArgs e)
        {
            SetMiniMode(!iUiOptions.MiniMode);
        }

        private void SetMiniMode(bool aMiniMode)
        {
            SetMiniMode(aMiniMode, false);
        }

        private void SetMiniMode(bool aMiniMode, bool aStartUp)
        {
            ButtonLogo.Visible = !aMiniMode;
            ButtonExitKompact.Visible = aMiniMode;
            ButtonMinimise.Visible = aMiniMode;

            PanelTrackInfo.Visible = aMiniMode;
            SplitContainer.Visible = !aMiniMode;
            ButtonSourceSelector.Visible = !aMiniMode;
            ButtonRoomSelector.Visible = !aMiniMode;
            PanelBreadcrumb.Visible = !aMiniMode;
            ButtonRepeat.Visible = !aMiniMode;
            ButtonShuffle.Visible = !aMiniMode;
            ButtonSave.Visible = !aMiniMode;
            ButtonWasteBin.Visible = !aMiniMode;
            ButtonStandby.Visible = !aMiniMode;
            ButtonUp.Visible = !aMiniMode;
            ButtonView.Visible = !aMiniMode;
            ButtonSize.Visible = !aMiniMode;
            iViewStatus.Visible = false;

            miniModeToolStripMenuItem.Checked = aMiniMode;
            TopMost = aMiniMode;

            if (aMiniMode)
            {
                if (WindowState == FormWindowState.Maximized)
                {
                    ButtonMaximise_Click(this, EventArgs.Empty);
                }

                Resize -= FormKinskyDesktop_Resize;
                Move -= FormKinskyDesktop_Move;

                SuspendLayout();

                int titleBorderHeight = Height - ClientRectangle.Height;
                MinimumSize = new Size(520, 100 + titleBorderHeight);
                MaximumSize = new Size(Int32.MaxValue, 100 + titleBorderHeight);
                Size  = new Size(iUiOptions.MiniModeWidth, MinimumSize.Height);
                Location = iUiOptions.MiniModeLocation;

                ResumeLayout();

                Resize += FormKinskyDesktop_Resize;
                Move += FormKinskyDesktop_Move;
            }
            else
            {
                if (iUiOptions.Fullscreen)
                {
                    WindowState = FormWindowState.Maximized;
                }
                LayoutFormKinskyDesktop();
                SetMaximiseOrRestore();

                Resize -= FormKinskyDesktop_Resize;
                Move -= FormKinskyDesktop_Move;

                SuspendLayout();

                MinimumSize = new Size(800, 480);
                MaximumSize = new Size(0, 0);
                ClientSize = iUiOptions.WindowSize;
                if (!aStartUp)
                {
                    Location = iUiOptions.WindowLocation;
                }

                ResumeLayout();

                Resize += FormKinskyDesktop_Resize;
                Move += FormKinskyDesktop_Move;
            }

            SetControlButtonsVisible(PointToClient(MousePosition));

            iUiOptions.MiniMode = aMiniMode;

            if (!aMiniMode)
            {
                SetCurrentView(iCurrentView);
            }

            LayoutFormKinskyDesktop();
        }

        private void ButtonMinimise_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void ButtonMaximise_Click(object sender, EventArgs e)
        {
            WindowState = (WindowState == FormWindowState.Maximized) ? FormWindowState.Normal : FormWindowState.Maximized;
            //Size = iUserOptions.WindowSize;
        }

        private void SetMaximiseOrRestore()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                ButtonMaximiseWin.ImageStateInitial = KinskyDesktop.Properties.Resources.WindowsRestore;
                ButtonMaximiseWin.ImageMouseOver = KinskyDesktop.Properties.Resources.WindowsRestoreMouse;
                ButtonMaximiseWin.ImageTouched = KinskyDesktop.Properties.Resources.WindowsRestoreTouch;
            }
            else
            {
                ButtonMaximiseWin.ImageStateInitial = KinskyDesktop.Properties.Resources.WindowsMaximize;
                ButtonMaximiseWin.ImageMouseOver = KinskyDesktop.Properties.Resources.WindowsMaximizeMouse;
                ButtonMaximiseWin.ImageTouched = KinskyDesktop.Properties.Resources.WindowsMaximizeTouch;
            }
        }

        private void SetControlButtonsVisible(Point aLocation)
        {
            if ((!iOptionPageGeneral.WindowBorder || iOptionPageGeneral.Transparency) && !iUiOptions.MiniMode)
            {
                Rectangle rect = new Rectangle(Width - 98, 8, 90, 55);
                if (rect.Contains(aLocation))
                {
                    ButtonCloseWin.Visible = true;
                    if (iCurrentView < 3)
                    {
                        ButtonMinimiseWin.Visible = true;
                        ButtonMaximiseWin.Visible = true;
                        ButtonMiniWin.Visible = true;
                    }
                }
                else
                {
                    ButtonCloseWin.Visible = false;
                    ButtonMinimiseWin.Visible = false;
                    ButtonMaximiseWin.Visible = false;
                    ButtonMiniWin.Visible = false;
                }
            }
        }

        private void EventExitRequest(object sender, EventArgs e)
        {
            Close();
        }

        private void SplitContainer_Paint(object sender, PaintEventArgs e)
        {
            using (Brush b = new SolidBrush(iViewSupport.ForeColourMuted))
            {
                e.Graphics.FillRectangle(b, new Rectangle(SplitContainer.SplitterDistance - 1, 0, 3, SplitContainer.Height));
            }
        }

        private void ButtonLogo_EventClick(object sender, EventArgs e)
        {
            iCurrentView = (iCurrentView + 1) % 4;
            SplitContainer.SplitterMoved -= splitContainer1_SplitterMoved;
            SetCurrentView(iCurrentView);
            SplitContainer.SplitterMoved += splitContainer1_SplitterMoved;
            Invalidate();
        }

        private void SetCurrentView(uint aView)
        {
            iViewStatus.Visible = false;
            SplitContainer.Visible = false;
            SplitContainer.SuspendLayout();
            switch (aView)
            {
                case 0:
                    SplitContainer.Panel1Collapsed = false;
                    SplitContainer.Panel2Collapsed = false;
                    SplitContainer.Panel1.Focus();
                    break;
                case 1:
                    SplitContainer.Panel1Collapsed = false;
                    SplitContainer.Panel2Collapsed = true;
                    SplitContainer.Panel1.Focus();
                    break;
                case 2:
                    SplitContainer.Panel1Collapsed = true;
                    SplitContainer.Panel2Collapsed = false;
                    SplitContainer.Panel2.Focus();
                    break;
                case 3:
                    iViewStatus.Focus();
                    break;
            }
            SplitContainer.ResumeLayout(false);
            if (iCurrentView < 3)
            {
                WindowState = (iUiOptions.Fullscreen) ? FormWindowState.Maximized : FormWindowState.Normal;
                SplitContainer.Visible = true;
                SplitContainer.SplitterDistance = iUiOptions.SplitterLocation;
                PanelTrackInfo.Visible = true;
                ButtonSourceSelector.Visible = true;
                ButtonRoomSelector.Visible = true;
                PanelBreadcrumb.Visible = true;
                ButtonRepeat.Visible = true;
                ButtonShuffle.Visible = true;
                ButtonSave.Visible = true;
                ButtonWasteBin.Visible = true;
                ButtonStandby.Visible = true;
                ButtonUp.Visible = true;
                ButtonView.Visible = true;
                ButtonSize.Visible = true;
                ButtonReceivers.Visible = false;
                iViewStatus.Visible = false;
            }
            else
            {
                SplitContainer.Visible = false;
                PanelTrackInfo.Visible = false;
                ButtonSourceSelector.Visible = false;
                ButtonRoomSelector.Visible = false;
                PanelBreadcrumb.Visible = false;
                ButtonRepeat.Visible = false;
                ButtonShuffle.Visible = false;
                ButtonSave.Visible = false;
                ButtonWasteBin.Visible = false;
                ButtonStandby.Visible = false;
                ButtonUp.Visible = false;
                ButtonView.Visible = false;
                ButtonSize.Visible = false;
                ButtonReceivers.Visible = false;
                iViewStatus.Visible = true;
                WindowState = FormWindowState.Maximized;
            }
        }

        private void LayoutFormKinskyDesktop()
        {
            SuspendLayout();
            SplitContainer.SuspendLayout();
            SplitContainer.Panel1.SuspendLayout();
            SplitContainer.Panel2.SuspendLayout();

            float halfWidth = ClientRectangle.Width * 0.5f;

            if (!iUiOptions.MiniMode)
            {
                // header layout
                ButtonCloseWin.Location = new Point(ClientRectangle.Width - kRightEdgeFiller.Width - 5 - ButtonCloseWin.Width, ButtonCloseWin.Location.Y);
                ButtonMaximiseWin.Location = new Point(ButtonCloseWin.Location.X - ButtonMaximiseWin.Width, ButtonMaximiseWin.Location.Y);
                ButtonMinimiseWin.Location = new Point(ButtonMaximiseWin.Location.X - ButtonMinimiseWin.Width, ButtonMinimiseWin.Location.Y);
                ButtonMiniWin.Location = new Point(ButtonMinimiseWin.Location.X - ButtonMiniWin.Width, ButtonMiniWin.Location.Y);

                ButtonStandby.Location = new Point(ClientRectangle.Width - kRightEdgeFiller.Width - ButtonStandby.Width, ButtonStandby.Location.Y);
                ButtonWasteBin.Location = new Point(ButtonStandby.Location.X - ButtonWasteBin.Width, ButtonWasteBin.Location.Y);
                ButtonSave.Location = new Point(ButtonWasteBin.Location.X - ButtonSave.Width, ButtonSave.Location.Y);
                ButtonShuffle.Location = new Point(ButtonSave.Location.X - ButtonShuffle.Width, ButtonShuffle.Location.Y);
                ButtonRepeat.Location = new Point(ButtonShuffle.Location.X - ButtonRepeat.Width, ButtonRepeat.Location.Y);
                ButtonReceivers.Location = new Point(ButtonRepeat.Location.X - ButtonReceivers.Width, ButtonReceivers.Location.Y);
                PanelBreadcrumb.Width = ButtonReceivers.Location.X - (ButtonUp.Location.X + ButtonUp.Width);

                PanelTrackInfo.Location = new Point(PanelBreadcrumb.Location.X, 9);
                PanelTrackInfo.Size = new Size(PanelBreadcrumb.Width, 60);

                ButtonRoomSelector.Width = (int)(halfWidth - kLeftEdgeFiller.Width);
                ButtonSourceSelector.Location = new Point(ButtonRoomSelector.Location.X + ButtonRoomSelector.Width, ButtonSourceSelector.Location.Y);
                ButtonSourceSelector.Width = ClientRectangle.Width - kLeftEdgeFiller.Width - ButtonRoomSelector.Width - kRightEdgeFiller.Width;

                // middle layout
                int height = ClientRectangle.Height - kHeaderLeft.Height - kFooterLeft.Height;// -ButtonRoomSelector.Height;
                SplitContainer.Location = new Point(SplitContainer.Location.X, ButtonRoomSelector.Bottom);
                SplitContainer.Size = new Size(ClientRectangle.Width - kLeftEdgeFiller.Width - kRightEdgeFiller.Width, height);
                iViewStatus.Location = new Point(0, 0);
                iViewStatus.Size = new Size(ClientRectangle.Width, ClientRectangle.Height - kFooterLeft.Height);

                // footer layout
                PanelSizer.Location = new Point(ClientRectangle.Width - PanelSizer.Width, ClientRectangle.Height - PanelSizer.Height);

                ThreekArrayControl.Size = new System.Drawing.Size(170, 100);
                ThreekArrayControl.Location = new Point((int)(halfWidth - (ThreekArrayControl.Width * 0.5f)), ClientRectangle.Height - ThreekArrayControl.Height);
                RotaryControlTracker.Location = new Point(ThreekArrayControl.Location.X - RotaryControlTracker.Width, ClientRectangle.Height - RotaryControlTracker.Height);
                RotaryControlVolume.Location = new Point(ThreekArrayControl.Location.X + ThreekArrayControl.Width, ClientRectangle.Height - RotaryControlVolume.Height);

                RockerControlTracker.Location = new Point(RotaryControlTracker.Location.X, RotaryControlTracker.Location.Y);
                RockerControlVolume.Location = new Point(RotaryControlVolume.Location.X, RotaryControlVolume.Location.Y);

                ButtonLogo.Location = new Point(ButtonStandby.Location.X - ButtonLogo.Width, ClientRectangle.Height - ButtonLogo.Height - 20);
            }
            else
            {
                ButtonExitKompact.Location = new Point(ClientRectangle.Width - kRightEdgeFiller.Width - ButtonExitKompact.Width, 5);
                ButtonMinimise.Location = new Point(ButtonExitKompact.Location.X - ButtonMinimise.Width, 5);
                PanelSizer.Location = new Point(ClientRectangle.Width - PanelSizer.Width, ClientRectangle.Height - PanelSizer.Height);

                RotaryControlVolume.Location = new Point(PanelSizer.Location.X - RotaryControlVolume.Width, ClientRectangle.Height - RotaryControlVolume.Height);
                ThreekArrayControl.Size = new System.Drawing.Size(170, 100);
                ThreekArrayControl.Location = new Point(RotaryControlVolume.Location.X - ThreekArrayControl.Width, ClientRectangle.Height - ThreekArrayControl.Height);
                RotaryControlTracker.Location = new Point(ThreekArrayControl.Location.X - RotaryControlTracker.Width, ClientRectangle.Height - RotaryControlTracker.Height);

                RockerControlTracker.Location = new Point(RotaryControlTracker.Location.X, RotaryControlTracker.Location.Y);
                RockerControlVolume.Location = new Point(RotaryControlVolume.Location.X, RotaryControlVolume.Location.Y);

                PanelTrackInfo.Location = new Point(PanelSizer.Width, 10);
                PanelTrackInfo.Size = new Size(RotaryControlTracker.Location.X - PanelTrackInfo.Location.X, 80);
            }

            SplitContainer.Panel1.ResumeLayout();
            SplitContainer.Panel2.ResumeLayout();
            SplitContainer.ResumeLayout();
            ResumeLayout();

            Invalidate();
        }

        private void ButtonExitKompact_EventClick(object sender, EventArgs e)
        {
            SetMiniMode(false);
        }

        private void EventSupportChanged(object sender, EventArgs e)
        {
            bool layout = false;

            SplitContainer.BackColor = iViewSupport.BackColour;
            SplitContainer.Invalidate();

            PanelBrowser.BackColor = iViewSupport.BackColour;
            PanelBrowser.Invalidate();

            PanelPlaylist.BackColor = iViewSupport.BackColour;
            PanelPlaylist.ForeColor = iViewSupport.ForeColour;
            PanelPlaylist.Font = iViewSupport.FontMedium;
            PanelPlaylist.Invalidate();

            TabControl.BackColor = iViewSupport.BackColour;
            TabControl.ForeColor = iViewSupport.ForeColour;
            TabControl.ForeColourMuted = iViewSupport.ForeColourMuted;
            TabControl.ForeColourBright = iViewSupport.ForeColourBright;
            TabControl.HighlightColour = iViewSupport.HighlightForeColour;
            if (TabControl.Font.Size != iViewSupport.FontSmall.Size)
            {
                doubleBufferedTableLayoutPanel1.SuspendLayout();
                doubleBufferedTableLayoutPanel1.ColumnStyles[0].Width = iViewSupport.FontSmall.Height + 2;
                doubleBufferedTableLayoutPanel1.ResumeLayout(false);

                TabControl.Font = iViewSupport.FontSmall;
            }
            TabControl.Invalidate();

            //ButtonRoomSelector.BackColor = iViewSupport.BackColour;
            ButtonRoomSelector.ForeColor = iViewSupport.ForeColourMuted;
            ButtonRoomSelector.ForeColorMuted = iViewSupport.ForeColourMuted;
            if (ButtonRoomSelector.Font.Size != iViewSupport.FontSmall.Size)
            {
                ButtonRoomSelector.Font = iViewSupport.FontSmall;
                //ButtonRoomSelector.Height = iViewSupport.FontSmall.Height + 4;
                layout = true;
            }
            ButtonRoomSelector.Invalidate();

            //ButtonSourceSelector.BackColor = iViewSupport.BackColour;
            ButtonSourceSelector.ForeColor = iViewSupport.ForeColourMuted;
            ButtonSourceSelector.ForeColorMuted = iViewSupport.ForeColourMuted;
            if (ButtonSourceSelector.Font.Size != iViewSupport.FontSmall.Size)
            {
                ButtonSourceSelector.Font = iViewSupport.FontSmall;
                //ButtonSourceSelector.Height = iViewSupport.FontSmall.Height + 4;
                layout = true;
            }
            ButtonSourceSelector.Invalidate();

            if (layout)
            {
                LayoutFormKinskyDesktop();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
            TopMost = true;
            TopMost = false;
        }

        private void FormKinskyDesktop_Deactivate(object sender, EventArgs e)
        {
            iDragging = false;
        }

        //private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    iAutoUpdate.Stop();

        //    FormCheckForUpdates dialogCheckForUpdates = new FormCheckForUpdates(iAutoUpdate);
        //    DialogResult result = dialogCheckForUpdates.ShowDialog(this);
        //    dialogCheckForUpdates.Dispose();
        //    if (result == DialogResult.OK)
        //    {
        //        if (dialogCheckForUpdates.Info != null)
        //        {
        //            Form dialog = new FormUpdate(iAutoUpdate, dialogCheckForUpdates.Info);
        //            result = dialog.ShowDialog(this);
        //            dialog.Dispose();
        //            if (result == DialogResult.OK)
        //            {
        //                Close();
        //                return;
        //            }
        //        }
        //    }

        //    iAutoUpdate.Start();
        //}

        private void downloadPluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormPluginBrowser browser = new FormPluginBrowser(iViewSupport, iHttpClient, kPluginsUrl);
            browser.Show(this);
            browser.Location = new Point(Location.X + iUiOptions.SplitterLocation, (int)(Location.Y + (Height - browser.Height) * 0.5f));
        }
    }
}
