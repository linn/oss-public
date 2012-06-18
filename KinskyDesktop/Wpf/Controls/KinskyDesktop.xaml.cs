using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Linn.Kinsky;
using Linn.ControlPoint.Upnp;
using Linn.Control.Ssdp;
using System.Net;
using Linn;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Interop;

namespace KinskyDesktopWpf
{
    /// <summary>
    /// Interaction logic for KinskyDesktop.xaml
    /// </summary>
    public partial class KinskyDesktop : Window, IStack
    {

        private const string kUpdateFeedUrl = "http://oss.linn.co.uk/Feeds/Updates/Application.xml";
        private const string kOnlineManualUrl = "http://oss.linn.co.uk/trac/wiki/KinskyWindowsDavaarManual";
        private HelperKinsky iHelper;
        private Mediator iMediator;

        private ContentDirectoryLocator iLocator;

        private HttpServer iHttpServer;
        private HttpClient iHttpClient;
        private MediaProviderLibrary iLibrary;
        private LocalPlaylists iLocalPlaylists;
        private RemotePlaylists iRemotePlaylists;

        private PlaySupport iPlaySupport;
        private SaveSupport iSaveSupport;
        private ViewSaveSupport iViewSaveSupport;

        private MediaProviderSupport iSupport;
        private ResourceDictionary iThemeOverrides;

        private OptionEnum iFontsOption;
        private OptionBool iHideCursorOption;
        private OptionBool iRotaryControlsOption;
        private OptionBool iShowToolTipsOption;
        private OptionBool iShowExtendedTrackInfoOption;
        private OptionBool iTransparentOption;
        private OptionBool iGroupPlaylistOption;
        private OptionBool iSoftwareRenderingOption;

        private const string kWindowTitleBarBrushKey = "WindowTitleBarBrush";
        private const string kCursorKey = "Cursor";
        private const string kToolTipVisibilityKey = "ToolTipVisibility";

        private const string kLargeFontSizeKey = "LargeFontSize";
        private const string kSemiLargeFontSizeKey = "SemiLargeFontSize";
        private const string kMediumFontSizeKey = "MediumFontSize";
        private const string kSmallFontSizeKey = "SmallFontSize";

        public const string kFontOptionNormal = "Normal";
        public const string kFontOptionLarge = "Large";

        private const string kKinskyDesktopNamedMutex = "KinskyDesktopMutex";
        private static string kUpdateReadySignal = "KinskyDesktopUpdateReadySignal";
        private static string kUpdateStartedSignal = "KinskyDesktopUpdateStartedSignal";
        private bool iUpdateOnExit;

        private double[] iFontSizesSmall = { 10, 14 };
        private double[] iFontSizesMedium = { 12, 16 };
        private double[] iFontSizesSemiLarge = { 13, 17 };
        private double[] iFontSizesLarge = { 18, 22 };

        private UiOptions iUIOptions;
        private bool iMiniModeChanging = false;
        private ViewMaster iViewMaster;

        private SystrayForm iSystrayForm;
        private Mutex iNamedMutex;
        private const double kMinHeight = 400;
        private const double kMiniModeHeight = 120;
        private const int kAnimationTimeMilliseconds = 300;
        private AutoResetEvent iWindowLoaded;
        private AutoUpdate iAutoUpdate;
        private static string kApplicationTarget = "win32";
        private static string kUpdatesAvailableMessage = "Updates are available for Kinsky.  Click here to download.";
        private const uint kUpdateVersion = 1;
        private bool iProcessedOptions = false;
        private AutoUpdate.EUpdateType iAutoUpdateType = AutoUpdate.EUpdateType.Stable;
        private OptionPageUpdates iOptionPageUpdates;

        public KinskyDesktop()
        {
            iWindowLoaded = new AutoResetEvent(false);
            InitializeComponent();
            iUpdateOnExit = false;
            iHelper = new HelperKinsky(Environment.GetCommandLineArgs(), new Invoker(this.Dispatcher));
            ICrashLogDumper d = new CrashLogDumperForm(this,
                                                       iHelper.Title,
                                                       iHelper.Product,
                                                       iHelper.Version);
            iHelper.AddCrashLogDumper(d);
            System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.ThrowException);
            iUIOptions = new UiOptions(iHelper);
            InitialiseStack();
            WindowChrome.SetIsMiniModeActive(mainWindowChrome, iUIOptions.MiniMode);
            SetWindowDimensions();
            iProcessedOptions = true;
            AllowsTransparency = iTransparentOption.Native;
            this.Loaded += new RoutedEventHandler(KinskyDesktop_Loaded);
        }

        void KinskyDesktop_Loaded(object sender, RoutedEventArgs e)
        {
            if (iSoftwareRenderingOption.Native)
            {
                HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                HwndTarget hwndTarget = hwndSource.CompositionTarget;
                hwndTarget.RenderMode = RenderMode.SoftwareOnly; 
            }
            iWindowLoaded.Set();
        }

        public void window_Closed(object sender, EventArgs e)
        {
            Exit();
        }

        internal void InitialiseStack()
        {
            OptionPage generalOptionsPage = new OptionPage("General");
            iHideCursorOption = new OptionBool("hidecursor", "Hide Cursor", "Hide Cursor", false);
            generalOptionsPage.Add(iHideCursorOption);
            iRotaryControlsOption = new OptionBool("rotarycontrol", "Use rotary controls", "Use rotary controls", true);
            generalOptionsPage.Add(iRotaryControlsOption);
            iShowToolTipsOption = new OptionBool("tooltips", "Show tooltips", "Show tooltips", true);
            generalOptionsPage.Add(iShowToolTipsOption);
            iShowExtendedTrackInfoOption = new OptionBool("trackinfo", "Extended track info", "Show extended track information", true);
            generalOptionsPage.Add(iShowExtendedTrackInfoOption);
            iGroupPlaylistOption = new OptionBool("groupplaylist", "Group playlist items by album", "Allows grouping of items within the playlist window", false);
            generalOptionsPage.Add(iGroupPlaylistOption);
            iTransparentOption = new OptionBool("customwindow", "Transparent window (requires restart)", "Toggles custom window", true);
            generalOptionsPage.Add(iTransparentOption);
            iSoftwareRenderingOption = new OptionBool("softwarerendering", "Force software rendering (requires restart)", "Disables hardware rendering for troublesome display cards", false);
            generalOptionsPage.Add(iSoftwareRenderingOption);
            iHelper.AddOptionPage(generalOptionsPage);

            iOptionPageUpdates = new OptionPageUpdates(iHelper);
            iHelper.AddOptionPage(iOptionPageUpdates);



            OptionPage fontsOptionPage = new OptionPage("Fonts");
            iFontsOption = new OptionEnum("fontsize", "Font size", "Font size");
            iFontsOption.AddDefault(kFontOptionNormal);
            iFontsOption.Add(kFontOptionLarge);
            fontsOptionPage.Add(iFontsOption);

            iHideCursorOption.EventValueChanged += OnOptionChanged;
            iRotaryControlsOption.EventValueChanged += OnOptionChanged;
            iShowToolTipsOption.EventValueChanged += OnOptionChanged;
            iShowExtendedTrackInfoOption.EventValueChanged += OnOptionChanged;
            iFontsOption.EventValueChanged += OnOptionChanged;
            iTransparentOption.EventValueChanged += OnOptionChanged;
            iOptionPageUpdates.EventBetaVersionsChanged += OnUpdatesChanged;
            iOptionPageUpdates.EventAutoUpdateChanged += OnUpdatesChanged;

            iHelper.AddOptionPage(fontsOptionPage);

            iViewMaster = new ViewMaster();
            iHttpServer = new HttpServer(HttpServer.kPortKinskyDesktop);
            iHttpClient = new HttpClient();

            iLibrary = new MediaProviderLibrary(iHelper);
            iRemotePlaylists = new RemotePlaylists(iHelper);
            iLocalPlaylists = new LocalPlaylists(iHelper, true);

            iSupport = new MediaProviderSupport(iHttpServer);

            PluginManager pluginManager = new PluginManager(iHelper, iHttpClient, iSupport);
            iLocator = new ContentDirectoryLocator(pluginManager, new AppRestartHandler());
            OptionBool optionRemotePlaylists = iLocator.Add(RemotePlaylists.kRootId, iRemotePlaylists);
            OptionBool optionLocalPlaylists = iLocator.Add(LocalPlaylists.kRootId, iLocalPlaylists);
            iLocator.Add(MediaProviderLibrary.kLibraryId, iLibrary);
            iHelper.AddOptionPage(iLocator.OptionPage);

            iSaveSupport = new SaveSupport(iHelper, iRemotePlaylists, optionRemotePlaylists, iLocalPlaylists, optionLocalPlaylists);
            iViewSaveSupport = new ViewSaveSupport(RequestLocalPlaylistFilename, iSaveSupport);
            iPlaySupport = new PlaySupport();

            iHelper.ProcessOptionsFileAndCommandLine();
            SetUpdateTypes();
        }

        private void OnUpdatesChanged(object sender, EventArgs args)
        {
            SetUpdateTypes();
        }

        private void SetUpdateTypes()
        {
            iAutoUpdateType = AutoUpdate.EUpdateType.Stable;
            if (iOptionPageUpdates.BetaVersions)
            {
                iAutoUpdateType |= AutoUpdate.EUpdateType.Beta;
            }
            if (iOptionPageUpdates.DevelopmentVersions)
            {
                iAutoUpdateType |= AutoUpdate.EUpdateType.Development;
            }
            if (iOptionPageUpdates.NightlyBuilds)
            {
                iAutoUpdateType |= AutoUpdate.EUpdateType.Nightly;
            }
            if (iAutoUpdate != null)
            {
                iAutoUpdate.UpdateTypes = iAutoUpdateType;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs args)
        {
            bool createdNew = false;
            iNamedMutex = new Mutex(false, kKinskyDesktopNamedMutex, out createdNew);
            // add the following to prevent multiple kinsky desktops running
            //if (!createdNew)
            //{
            //    iNamedMutex.Close();
            //    iNamedMutex = null;
            //    MessageBox.Show("Another instance of Kinsky is currently running - exiting.");
            //    Environment.Exit(0);
            //}
            ThreadStart ts = new ThreadStart(delegate()
            {
                LoadStack(createdNew);
            });
            Thread t = new Thread(ts);
            t.Start();
        }

        private void LoadStack(bool aCreatedNew)
        {

            iWindowLoaded.WaitOne();
            Linn.Kinsky.Model model = new Linn.Kinsky.Model(iViewMaster, iPlaySupport);
            iMediator = new Mediator(iHelper, model);

            Dispatcher.Invoke((Action)delegate()
            {
                iSystrayForm = new SystrayForm(this);
                iSystrayForm.EventClosed += window_Closed;
                iSystrayForm.Text = this.Title;
            });

            // create the drop converter for the browser views
            DropConverter browseDropConverter = new DropConverter();
            browseDropConverter.Add(new DropConverterInternal());
            browseDropConverter.Add(new DropConverterUri());
            browseDropConverter.Add(new DropConverterFileDrop(iHttpServer, false));
            browseDropConverter.Add(new DropConverterText());

            // create the drop converter for the other views
            DropConverter viewDropConverter = new DropConverter();
            viewDropConverter.Add(new DropConverterInternal());
            viewDropConverter.Add(new DropConverterUri());
            viewDropConverter.Add(new DropConverterFileDrop(iHttpServer, true));
            viewDropConverter.Add(new DropConverterText());

            Dispatcher.Invoke((Action)delegate()
            {
                if (!aCreatedNew)
                {
                    double newX = iUIOptions.WindowLocation.X + 10;
                    double newY = iUIOptions.WindowLocation.Y + 10;
                    Point midpoint = new Point(newX + (Width / 2), newY + (Height / 2));
                    if (PointToScreen(midpoint).X >= SystemParameters.VirtualScreenWidth)
                    {
                        newX = 0;
                    }
                    if (PointToScreen(midpoint).Y >= SystemParameters.VirtualScreenHeight)
                    {
                        newY = 0;
                    }
                    Left = newX;
                    Top = newY;
                    //iUIOptions.WindowLocation = new Point(Left, Top);
                }
                iSystrayForm.Initialise(iViewMaster);
                viewKinsky.Initialise(this, iHelper, iLocator, iViewSaveSupport, iPlaySupport, browseDropConverter, viewDropConverter, iViewMaster, iUIOptions, iGroupPlaylistOption, iHelper.Senders);

                SetThemeOverrides();
                kompactMenuItem.IsChecked = iUIOptions.MiniMode;
                mainWindowChrome.MiniModeActiveChanged += new EventHandler(mainWindowChrome_MiniModeActiveChanged);
            });

            Linn.AutoUpdate.EUpdateType currentBuildType = Linn.AutoUpdate.EUpdateType.Stable;
            if (iHelper.Product.Contains("(NightlyBuild)"))
            {
                currentBuildType = Linn.AutoUpdate.EUpdateType.Nightly;
            }
            else if (iHelper.Product.Contains("(Beta)"))
            {
                currentBuildType = Linn.AutoUpdate.EUpdateType.Beta;
            }
            else if (iHelper.Product.Contains("(Development)"))
            {
                currentBuildType = Linn.AutoUpdate.EUpdateType.Development;
            }

            iAutoUpdate = new AutoUpdate(iHelper,
                                         kUpdateFeedUrl,
                                         1000 * 60 * 60,
                                         iAutoUpdateType,
                                         iHelper.Title,
                                         kApplicationTarget,
                                         kUpdateVersion,
                                         currentBuildType);
            iAutoUpdate.EventUpdateFound += iAutoUpdate_EventUpdateFound;

            iHelper.SetStackExtender(this);
            iHelper.Stack.SetStatusHandler(new StackStatusHandlerWpf(iHelper.Title, iSystrayForm.NotifyIcon));
            Open();
            // show the options dialog if specified by the user
            if (iHelper.Stack.StatusHandler.ShowOptions)
            {
                Dispatcher.BeginInvoke((Action)delegate()
                {
                    ShowOptionsDialog(true);
                });
            }

        }

        void iAutoUpdate_EventUpdateFound(object sender, AutoUpdate.EventArgsUpdateFound e)
        {
            if (!iSystrayForm.IsDisposed && iOptionPageUpdates.AutoUpdate)
            {
                iSystrayForm.Invoke((Action)(() =>
                {
                    iSystrayForm.NotifyIcon.BalloonTipClosed += DownloadUpdatesBalloonTipClosed;
                    iSystrayForm.NotifyIcon.BalloonTipClicked += DownloadUpdatesBalloonTipClick;
                    iSystrayForm.NotifyIcon.ShowBalloonTip(5000, "Updates Available", kUpdatesAvailableMessage, System.Windows.Forms.ToolTipIcon.Info);

                }));
            }
        }

        void DownloadUpdatesBalloonTipClosed(object sender, EventArgs args)
        {
            iSystrayForm.NotifyIcon.BalloonTipClosed -= DownloadUpdatesBalloonTipClosed;
            iSystrayForm.NotifyIcon.BalloonTipClosed -= DownloadUpdatesBalloonTipClick;
        }

        void DownloadUpdatesBalloonTipClick(object sender, EventArgs args)
        {
            CheckForUpdates();
        }

        void mainWindowChrome_MiniModeActiveChanged(object sender, EventArgs e)
        {
            iMiniModeChanging = true;
            iUIOptions.MiniMode = WindowChrome.GetIsMiniModeActive(mainWindowChrome);
            kompactMenuItem.IsChecked = iUIOptions.MiniMode;
            SetWindowDimensions();
            iMiniModeChanging = false;
            if (iUIOptions.MiniMode && viewKinsky.ShowFullScreenArtwork)
            {
                viewKinsky.ShowFullScreenArtwork = false;
            }
        }

        void Window_StateChanged(object sender, EventArgs args)
        {
            if (!WindowChrome.GetIsMiniModeActive(mainWindowChrome))
            {
                iUIOptions.Fullscreen = WindowState == WindowState.Maximized;
            }
        }

        void Window_LocationChanged(object sender, EventArgs args)
        {
            if (!iMiniModeChanging && !(this.WindowState == WindowState.Minimized) && !(this.WindowState == WindowState.Maximized) && Left != -32000 && Top != -32000)
            {
                iUIOptions.WindowLocation = new Point(Left, Top);
            }
        }

        void SetWindowDimensions()
        {
            if (WindowChrome.GetIsMiniModeActive(mainWindowChrome))
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
                AnimateHeight(kMiniModeHeight);
            }
            else
            {
                AnimateHeight(iUIOptions.WindowSize.Height);
                WindowState = iUIOptions.Fullscreen ? WindowState.Maximized : WindowState.Normal;
            }
            Width = iUIOptions.WindowSize.Width;
            Top = iUIOptions.WindowLocation.Y;
            Left = iUIOptions.WindowLocation.X;
        }

        private void AnimateHeight(double aNewHeight)
        {
            mainWindowChrome.IsAnimating = true;
            MinHeight = 0;
            ClearValue(Control.MaxHeightProperty);
            Storyboard storyBoard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation(Height, aNewHeight, new Duration(TimeSpan.FromMilliseconds(kAnimationTimeMilliseconds)), FillBehavior.Stop);

            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Height"));
            storyBoard.Children.Add(animation);

            EventHandler handler = null;
            handler = (d, e) =>
            {
                Height = aNewHeight;
                var t = d as DispatcherTimer;
                MinHeight = WindowChrome.GetIsMiniModeActive(mainWindowChrome) ? kMiniModeHeight : kMinHeight;
                if (WindowChrome.GetIsMiniModeActive(mainWindowChrome))
                {
                    MaxHeight = kMiniModeHeight;
                }
                else
                {
                    ClearValue(Control.MaxHeightProperty);
                }
                storyBoard.Completed -= handler;
                storyBoard.Remove();
                mainWindowChrome.IsAnimating = false;
            };
            storyBoard.Completed += handler;

            storyBoard.Begin();
        }

        private void SetThemeOverrides()
        {
            if (iThemeOverrides == null)
            {
                iThemeOverrides = new ResourceDictionary();
            }

            UseRotaryControls = bool.Parse(iRotaryControlsOption.Value);

            iThemeOverrides[kCursorKey] = bool.Parse(iHideCursorOption.Value) ? Cursors.None : Cursors.Arrow;

            iThemeOverrides[kToolTipVisibilityKey] = bool.Parse(iShowToolTipsOption.Value) ? Visibility.Visible : Visibility.Collapsed;
            viewKinsky.SetShowExtendedTrackInfo(bool.Parse(iShowExtendedTrackInfoOption.Value));

            int fontSizeIndex = iFontsOption.Value == kFontOptionNormal ? 0 : 1;

            iThemeOverrides[kSmallFontSizeKey] = iFontSizesSmall[fontSizeIndex];
            iThemeOverrides[kMediumFontSizeKey] = iFontSizesMedium[fontSizeIndex];
            iThemeOverrides[kLargeFontSizeKey] = iFontSizesLarge[fontSizeIndex];
            iThemeOverrides[kSemiLargeFontSizeKey] = iFontSizesSemiLarge[fontSizeIndex];

            if (Application.Current.Resources.MergedDictionaries.Contains(iThemeOverrides))
            {
                Application.Current.Resources.MergedDictionaries.Remove(iThemeOverrides);
            }
            Application.Current.Resources.MergedDictionaries.Add(iThemeOverrides);
        }

        void OnOptionChanged(object sender, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke((Action)delegate()
                {
                    SetThemeOverrides();
                });
            }
            else
            {
                SetThemeOverrides();
            }
        }

        void IStack.Start(IPAddress aIpAddress)
        {
            iMediator.Open();
            iLibrary.Start(aIpAddress);
            iRemotePlaylists.Start(aIpAddress);
            iHttpClient.Start();
            iHttpServer.Start(aIpAddress);
            iLocator.Start();
            iAutoUpdate.Start();
        }

        void IStack.Stop()
        {
            if (iLocator != null)
            {
                iLocator.Stop();
            }
            if (iHttpServer != null)
            {
                iHttpServer.Stop();
            }
            if (iHttpClient != null)
            {
                iHttpClient.Stop();
            }
            if (iRemotePlaylists != null)
            {
                iRemotePlaylists.Stop();
            }
            if (iLibrary != null)
            {
                iLibrary.Stop();
            }
            if (iMediator != null)
            {
                iMediator.Close();
            }
            if (iAutoUpdate != null)
            {
                iAutoUpdate.Stop();
            }
        }

        internal void Open()
        {
            iSystrayForm.Start();
            viewKinsky.Start();
            iHelper.Stack.Start();
        }

        internal new void Close()
        {
            if (iHelper != null && iHelper.Stack != null)
            {
                iHelper.Stack.Stop();
            }
            Trace.WriteLine(Trace.kKinskyDesktop, "Stack stopped");
            viewKinsky.Stop();
            if (iSystrayForm != null)
            {
                iSystrayForm.Stop();
            }
        }

        private void RequestLocalPlaylistFilename(ISaveSupport aSaveSupport)
        {
            try
            {
                SavePlaylistDialog dialog = new SavePlaylistDialog(aSaveSupport);
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                //if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
                //{
                //    aSaveSupport.Save(dialog.Filename(), string.Empty, 0);
                //}
            }
            catch (Exception ex)
            {
                UserLog.WriteLine(DateTime.Now + ": Could not save playlist file: " + ex);

                MessageBox.Show("Could not create the playlist file.", "Error saving playlist", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void ShowOptionsDialog(bool aStartOnNetwork)
        {
            // add a new stack status change handler while the options page  is visible
            // leave the default one so the balloon tips still appear
            iHelper.Stack.EventStatusChanged += iHelper.Stack.StatusHandler.StackStatusOptionsChanged;
            OptionsDialog dialog = new OptionsDialog(iHelper);
            if (aStartOnNetwork)
            {
                dialog.SetPageByName("Network");
            }
            dialog.Owner = Window.GetWindow(this);
            dialog.AllowsTransparency = dialog.Owner.AllowsTransparency;
            dialog.ShowDialog();
            iHelper.Stack.EventStatusChanged -= iHelper.Stack.StatusHandler.StackStatusOptionsChanged;
        }

        private void Exit()
        {
            if (iProcessedOptions)
            {
                iHideCursorOption.EventValueChanged -= OnOptionChanged;
                iRotaryControlsOption.EventValueChanged -= OnOptionChanged;
                iShowToolTipsOption.EventValueChanged -= OnOptionChanged;
                iShowExtendedTrackInfoOption.EventValueChanged -= OnOptionChanged;
                iFontsOption.EventValueChanged -= OnOptionChanged;
            }
            if (iSystrayForm != null && !iSystrayForm.IsDisposed)
            {
                iSystrayForm.Invoke((Action)delegate()
                {
                    iSystrayForm.Close();
                });
            }
            Close();
            if (iNamedMutex != null)
            {
                iNamedMutex.Close();
                iNamedMutex = null;
            }
            if (iUpdateOnExit)
            {
                // allow updater to spawn its process before shutting down
                using (EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, kUpdateReadySignal))
                {
                    using (EventWaitHandle waitHandle2 = new EventWaitHandle(false, EventResetMode.AutoReset, kUpdateStartedSignal))
                    {
                        waitHandle.Set();
                        waitHandle2.WaitOne();
                    }
                }
            }
            Environment.Exit(0);
        }

        #region Command Bindings

        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Exit();
        }

        private void KompactCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void KompactExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!mainWindowChrome.IsAnimating)
            {
                WindowChrome.SetIsMiniModeActive(mainWindowChrome, !WindowChrome.GetIsMiniModeActive(mainWindowChrome));
            }
        }

        private void RescanCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void RescanExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            iLocator.Refresh();
            iHelper.Rescan();
        }

        private void OptionsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OptionsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ShowOptionsDialog(false);
        }

        private void DebugConsoleCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void DebugConsoleExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            UserLogDialog dialog = new UserLogDialog();
            dialog.Owner = Window.GetWindow(this);
            dialog.Show();
        }


        private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(kOnlineManualUrl);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to contact " + kOnlineManualUrl, "Online Help Failed", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void HelpAboutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void HelpAboutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            AboutDialog dialog = new AboutDialog(iHelper, iFontsOption.Value);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private void UpdateCheckCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void UpdateCheckExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CheckForUpdates();
        }


        #endregion



        private void CheckForUpdates()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateDialog updateDialog = new UpdateDialog(iAutoUpdate);
                updateDialog.Owner = Window.GetWindow(this);
                bool? result = updateDialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    iUpdateOnExit = true;
                    Exit();
                }
            }));
        }

        public bool UseRotaryControls
        {
            get { return (bool)GetValue(UseRotaryControlsProperty); }
            set { SetValue(UseRotaryControlsProperty, value); }
        }

        public static readonly DependencyProperty UseRotaryControlsProperty =
            DependencyProperty.Register("UseRotaryControls", typeof(bool), typeof(KinskyDesktop), new UIPropertyMetadata(true));


        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine("Window::DragEnter");
            if (iPlaySupport != null)
            {
                iPlaySupport.SetDragging(true);
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            try{
            if (iPlaySupport != null)
            {
                iPlaySupport.SetDragging(false);
            }
            e.Handled = true;
            }
            catch (Exception ex)
            {
                UserLog.WriteLine("Exception caught in Window.DragLeave: " + ex);
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            try{
            if (iPlaySupport != null)
            {
                iPlaySupport.SetDragging(true);
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            }
            catch (Exception ex)
            {
                UserLog.WriteLine("Exception caught in Window.DragOver: " + ex);
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            try{
            if (iPlaySupport != null)
            {
                iPlaySupport.SetDragging(false);
            }
            e.Handled = true;
            }
            catch (Exception ex)
            {
                UserLog.WriteLine("Exception caught in Window.DragDrop: " + ex);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowChrome.GetIsMiniModeActive(mainWindowChrome))
            {
                iUIOptions.WindowSize = new Size(Width, iUIOptions.WindowSize.Height);
            }
            else
            {
                iUIOptions.WindowSize = new Size(Width, Height);
            }
            iUIOptions.WindowLocation = new Point(Left, Top);
        }


    }
}
