using System;
using System.Windows;
using System.ComponentModel;
using System.Threading;

using Linn;

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

        private AutoUpdate iAutoUpdate;
        private OptionPageUpdates iOptionPageAutoUpdate;

        private ModelController iModelController;

        private FormSysTray iFormSysTray;
        private PreferencesWindow iPreferencesWindow;

        private EventWaitHandle iWaitHandlePreferences;
        private AutoResetEvent iWaitPreferencesClosed;
        private Thread iThreadPreferences;

        private EventWaitHandle iWaitHandleExit;
        private Thread iThreadExit;

        private void ProcessOpenPreferences()
        {
            try
            {
                while (true)
                {
                    iWaitHandlePreferences.WaitOne();

                    ShowPreferences();

                    iWaitPreferencesClosed.WaitOne();
                    iWaitHandlePreferences.Reset();
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

        private void ShowPreferences()
        {
            Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
            {
                iPreferencesWindow = new PreferencesWindow(iHelper, iModelController, iFormSysTray);
                iPreferencesWindow.Closed += PreferencesClosed;
                iPreferencesWindow.Show();
                iPreferencesWindow.Activate();
            });
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                iHelper = new Helper(e.Args);
                ICrashLogDumper d = new CrashLogDumperWindow(iHelper.Title, iHelper.Product, iHelper.Version);
                iHelper.AddCrashLogDumper(d);

                bool createdNew;
                iWaitHandlePreferences = new EventWaitHandle(false, EventResetMode.ManualReset, "LinnSongcastOpenPreferences", out createdNew);
                if (!createdNew)
                {
                    MessageBox.Show("Linn Songcast is already running", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    App.Current.Shutdown(1);
                    return;
                }

                iWaitPreferencesClosed = new AutoResetEvent(false);

                iWaitHandleExit = new EventWaitHandle(false, EventResetMode.ManualReset, "LinnSongcastExit", out createdNew);
                Assert.Check(createdNew);

                iOptionPageAutoUpdate = new OptionPageUpdates(iHelper);
                iHelper.AddOptionPage(iOptionPageAutoUpdate);

                iAutoUpdate = new AutoUpdate(iHelper, AutoUpdate.kDefaultFeedLocation, 0, UpdateTypes(), 1);
                iAutoUpdate.EventUpdateFound += UpdateFound;

                iModelController = new ModelController(iHelper, iOptionPageAutoUpdate, iAutoUpdate, Dispatcher);
                iModelController.PropertyChanged += PropertyChanged;

                iFormSysTray = new FormSysTray(iModelController);
                iFormSysTray.EventOpenPreferences += OpenPreferences;

                iHelper.ProcessOptionsFileAndCommandLine();

                iModelController.Start();

                iThreadPreferences = new Thread(ProcessOpenPreferences);
                iThreadPreferences.IsBackground = true;
                iThreadPreferences.Start();

                iThreadExit = new Thread(ProcessExit);
                iThreadExit.IsBackground = true;
                iThreadExit.Start();

                Microsoft.Win32.SystemEvents.PowerModeChanged += PowerModeChanged;

                if (iModelController.FirstRun)
                {
                    iWaitHandlePreferences.Set();
                }

                iAutoUpdate.Start();
            }
            catch (SongcastError)
            {
                MessageBox.Show("Failed to initialise Linn Songcast Driver", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown(1);
            }
        }

        private AutoUpdate.EUpdateType UpdateTypes()
        {
            AutoUpdate.EUpdateType updateTypes = AutoUpdate.EUpdateType.Stable;
            if (iOptionPageAutoUpdate.NightlyBuilds)
            {
                updateTypes |= AutoUpdate.EUpdateType.Nightly;
            }
            if (iOptionPageAutoUpdate.DevelopmentVersions)
            {
                updateTypes |= AutoUpdate.EUpdateType.Development;
            }
            if (iOptionPageAutoUpdate.BetaVersions)
            {
                updateTypes |= AutoUpdate.EUpdateType.Beta;
            }

            return updateTypes;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Microsoft.Win32.SystemEvents.PowerModeChanged -= PowerModeChanged;

            if (iModelController != null)
            {
                iModelController.Dispose();
            }

            if (iFormSysTray != null)
            {
                iFormSysTray.Close();
            }

            if (iAutoUpdate != null)
            {
                iAutoUpdate.Stop();
                iAutoUpdate.EventUpdateFound -= UpdateFound;
            }

            if (iThreadPreferences != null)
            {
                iThreadPreferences.Abort();
            }
        }

        private void OpenPreferences(object sender, EventArgs e)
        {
            if (iPreferencesWindow != null)
            {
                iPreferencesWindow.Activate();
            }

            iWaitHandlePreferences.Set();
        }

        private void PreferencesClosed(object sender, EventArgs e)
        {
            iPreferencesWindow.Closed -= PreferencesClosed;
            iPreferencesWindow = null;
            iWaitPreferencesClosed.Set();
        }

        private void UpdateFound(object sender, EventArgs e)
        {
            if (iOptionPageAutoUpdate.AutoUpdate && iPreferencesWindow == null)
            {
                Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
                {
                    UpdateMessageBoxWindow window = new UpdateMessageBoxWindow();
                    window.EventPreferences += Preferences;
                    window.EventUpdate += Update;
                    window.Closed += UpdateClosed;
                    window.Show();
                });
            }
        }

        private void UpdateClosed(object sender, EventArgs e)
        {
            UpdateMessageBoxWindow window = sender as UpdateMessageBoxWindow;
            window.EventPreferences -= OpenPreferences;
            window.EventUpdate -= Update;
            window.Closed -= UpdateClosed;
        }

        private void Preferences(object sender, EventArgs e)
        {
            UpdateMessageBoxWindow window = sender as UpdateMessageBoxWindow;
            window.Close();

            OpenPreferences(sender, e);
        }

        private void Update(object sender, EventArgs e)
        {
            UpdateMessageBoxWindow window = sender as UpdateMessageBoxWindow;
            window.Close();

            iModelController.CheckForUpdates();
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AutomaticUpdateChecks" || e.PropertyName == "ParticipateInBeta")
            {
                iAutoUpdate.Stop();
                iAutoUpdate.UpdateTypes = UpdateTypes();
                iAutoUpdate.Start();
            }
        }

        private void PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            if (e.Mode == Microsoft.Win32.PowerModes.Suspend)
            {
                iModelController.Enabled = false;
            }
        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            iModelController.Enabled = false;
        }
    }
}
