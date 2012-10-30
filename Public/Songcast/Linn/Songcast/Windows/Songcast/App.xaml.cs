using System;
using System.Windows;
using System.ComponentModel;
using System.Threading;

using Linn;
using Linn.Toolkit;
using Linn.Toolkit.Wpf;

using OpenHome.Xapp;
using OpenHome.Songcast;

namespace Linn.Songcast
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string kName = "Linn Songcast";

        private Helper iHelper;
        private HelperAutoUpdate iHelperAutoUpdate;
        private Model iModel;
        private XappController iXappController;
        private ViewerBrowser iViewer;

        private FormSysTray iFormSysTray;
        private PreferencesWindow iPreferencesWindow;
        private MainWindow iMainWindow;
        private Ticker iTicker;

        private EventWaitHandle iWaitHandleMainWindow;
        private AutoResetEvent iWaitMainWindowClosed;
        private Thread iThreadMainWindow;

        private EventWaitHandle iWaitHandleExit;
        private Thread iThreadExit;

        private void ProcessOpenMainWindow()
        {
            try
            {
                while (true)
                {
                    iWaitHandleMainWindow.WaitOne();

                    Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
                    {
                        iMainWindow.Show();
                        iMainWindow.Activate();
                    });

                    iWaitMainWindowClosed.WaitOne();
                    iWaitHandleMainWindow.Reset();
                }
            }
            catch (ThreadAbortException) { }
        }

        private void ProcessExit()
        {
            try
            {
                while (true)
                {
                    iWaitHandleExit.WaitOne();

                    Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
                    {
                        App.Current.Shutdown(0);
                    });
                }
            }
            catch (ThreadAbortException) { }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // create helpers
                iHelper = new Helper(e.Args);
                ICrashLogDumper d = new CrashLogDumperWindow(ResourceManager.Icon, iHelper.Title, iHelper.Product, iHelper.Version);
                iHelper.AddCrashLogDumper(d);

                iHelperAutoUpdate = new HelperAutoUpdate(iHelper, new Linn.Toolkit.Wpf.ViewAutoUpdateStandard(ResourceManager.IconSongcaster, Linn.Songcast.Properties.Resources.IconLarge), new Invoker(this.Dispatcher));
                iHelperAutoUpdate.Start();

                // create the model
                iModel = new Model(new Invoker(this.Dispatcher), iHelper);

                iHelper.ProcessOptionsFileAndCommandLine();

                // this should be created before PreferencesWindowClosing event can be called or PreferencesWindowClosing will raise a null reference exception
                iWaitMainWindowClosed = new AutoResetEvent(false);

                // create the preferences window and controller
                iPreferencesWindow = new PreferencesWindow(iHelper, new PreferenceBindings(iModel, iHelperAutoUpdate.OptionPageUpdates), iModel, iHelperAutoUpdate);
                iPreferencesWindow.Closing += PreferencesWindowClosing;
                iPreferencesWindow.EventButtonHelpClicked += OpenHelp;

                // create the main window
                iMainWindow = new MainWindow();
                iMainWindow.Deactivated += MainWindowDeactivated;

                // create the xapp controller and view
                iXappController = new XappController(iModel, new Invoker(this.Dispatcher));
                iXappController.MainPage.EventShowConfig += OpenPreferences;
                iXappController.MainPage.EventShowHelp += OpenHelp;
                iViewer = new ViewerBrowser(iMainWindow.WebBrowser, iXappController.MainPageUri);

                // create the sys tray icon
                iFormSysTray = new FormSysTray(iModel);
                iFormSysTray.EventIconClick += SysTrayIconClick;

                // start the model
                System.Drawing.Image image = System.Drawing.Image.FromStream(ResourceManager.Icon.StreamSource);
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                iModel.Start("linn.co.uk", "Linn", "http://www.linn.co.uk", "http://www.linn.co.uk", stream.ToArray(), "image/png");

                bool createdNew;
                iWaitHandleMainWindow = new EventWaitHandle(false, EventResetMode.ManualReset, "LinnSongcastOpenPreferences", out createdNew);
                if (!createdNew)
                {
                    iWaitHandleMainWindow.Set();
                    //MessageBox.Show("Linn Songcast is already running", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    App.Current.Shutdown(0);
                    return;
                }

                iWaitHandleExit = new EventWaitHandle(false, EventResetMode.ManualReset, "LinnSongcastExit", out createdNew);
                Assert.Check(createdNew);

                iThreadMainWindow = new Thread(ProcessOpenMainWindow);
                iThreadMainWindow.IsBackground = true;
                iThreadMainWindow.Start();

                iThreadExit = new Thread(ProcessExit);
                iThreadExit.IsBackground = true;
                iThreadExit.Start();

                Microsoft.Win32.SystemEvents.PowerModeChanged += PowerModeChanged;
            }
            catch (SongcastError)
            {
                MessageBox.Show("Failed to initialise Linn Songcast Driver", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown(1);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Microsoft.Win32.SystemEvents.PowerModeChanged -= PowerModeChanged;

            if (iModel != null)
            {
                iModel.Stop();
            }

            if (iHelperAutoUpdate != null)
            {
                iHelperAutoUpdate.Dispose();
            }

            if (iFormSysTray != null)
            {
                iFormSysTray.Close();
            }

            if (iThreadMainWindow != null)
            {
                iThreadMainWindow.Abort();
            }

            if (iPreferencesWindow != null)
            {
                iPreferencesWindow.Close();
            }
        }

        private void OpenHelp(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Model.kOnlineManualUri);
            }
            catch (Exception)
            {
                MessageBox.Show("Warning", "Failed to contact " + Model.kOnlineManualUri + "\n\nFailed to open online manual", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenPreferences(object sender, EventArgs e)
        {
            iPreferencesWindow.Activate();
            iPreferencesWindow.Topmost = true;
            iPreferencesWindow.Topmost = false;
            iPreferencesWindow.Focus();

            iPreferencesWindow.Show();
        }

        private void PreferencesWindowClosing(object sender, CancelEventArgs e)
        {
            // stop the preferences window from actually closing and just hide it
            e.Cancel = true;
            iPreferencesWindow.Hide();
            iPreferencesWindow.tabControl1.SelectedIndex = 0;
        }

        private void SysTrayIconClick(object sender, EventArgs e)
        {
            if (iTicker != null && iTicker.MilliSeconds < 1000)
            {
                // the main window has been deactivated in the last second - discard this event
                iTicker = null;
                return;
            }

            // calculate which corner of the screen the sys tray icon is in - this assumes that
            // the systray always appears on the primary monitor
            double primaryScreenMidPtX = SystemParameters.PrimaryScreenWidth * 0.5;
            double primaryScreenMidPtY = SystemParameters.PrimaryScreenHeight * 0.5;
            System.Drawing.Point mousePt = System.Windows.Forms.Control.MousePosition;
            bool isLeft = (primaryScreenMidPtX > mousePt.X);
            bool isTop = (primaryScreenMidPtY > mousePt.Y);

            // calculate the position of the window so that it fits into the relevant corner of the WorkArea
            Rect screen = SystemParameters.WorkArea;
            Point pt = new Point();
            pt.X = isLeft ? screen.Left : screen.Right - iMainWindow.Width;
            pt.Y = isTop ? screen.Top : screen.Bottom - iMainWindow.Height;

            // correct the window position
            double windowMidX = pt.X + iMainWindow.Width * 0.5;
            double windowMidY = pt.Y + iMainWindow.Height * 0.5;

            // if the window is:
            //   - on the left and the mouse X position is greater than the window mid X position OR
            //   - on the right and the mouse X position is less than the window mid X position
            if ((isLeft && mousePt.X > windowMidX) || (!isLeft && mousePt.X < windowMidX))
            {
                // align the window mid X position with the mouse X position
                pt.X = mousePt.X - iMainWindow.Width * 0.5;
            }

            // if the window is:
            //   - at the top and the mouse Y position is greater than the window mid Y position OR
            //   - at the bottom and the mouse Y position is less than the window mid Y position
            if ((isTop && mousePt.Y > windowMidY) || (!isTop && mousePt.Y < windowMidY))
            {
                // align the window mid Y position with the mouse Y position
                pt.Y = mousePt.Y - iMainWindow.Height * 0.5;
            }

            iMainWindow.Left = pt.X;
            iMainWindow.Top = pt.Y;
            iMainWindow.Show();
            iMainWindow.Activate();
            iXappController.MainPage.TrackPageVisibilityChange(true);
        }

        private void MainWindowDeactivated(object sender, EventArgs e)
        {
            // this event is received when the mouse is clicked away from the main window - it is also received
            // when the sys tray icon is clicked and the window is visible. In this latter case, we want to
            // discard the forthcoming SysTrayIconClick event so that the main window does not get immediately
            // re-shown i.e. the sys tray icon should behave like a toggle button to show and hide the main
            // window. This is implemented by setting a ticker and the discarding of the event is done in the
            // SysTrayIconClick handler above
            iTicker = new Ticker();
            iMainWindow.Hide();
            iXappController.MainPage.TrackPageVisibilityChange(false);

            iWaitMainWindowClosed.Set();
        }

        private void PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            if (e.Mode == Microsoft.Win32.PowerModes.Suspend)
            {
                iModel.Enabled = false;
            }
        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            iModel.Enabled = false;
        }
    }
}
