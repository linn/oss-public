using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.IO;

using System.Security.AccessControl;
using Microsoft.Win32;

using Linn.Toolkit;
using Linn.Toolkit.Wpf;

using OpenHome.Xapp;
using OpenHome.MediaServer;

namespace Linn.Songbox
{
    public interface IMediaServerApp
    {
        string Name { get; }
        void OpenConfiguration();
    }

    public class StartAtLoginOption : IStartAtLoginOption
    {
        private string iTitle;
        public StartAtLoginOption(string aTitle)
        {
            iTitle = aTitle;
        }

        public bool StartAtLogin
        {
            get
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
                if (key != null)
                {
                    object o = key.GetValue(iTitle);

                    key.Close();

                    return (o != null);
                }

                return false;
            }
            set
            {
                if (value != StartAtLogin)
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (key != null)
                    {
                        if (value)
                        {
                            key.SetValue(iTitle, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String);
                        }
                        else
                        {
                            key.DeleteValue(iTitle);
                        }

                        key.Close();
                    }
                }
            }
        }

    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IMediaServerApp
    {
        private FormSysTray iFormSysTray;
        private ConfigurationWindow iWindow;
        private Helper iHelper;
        private Server iServer;
        private PageMain iPageMain;
        private HelperAutoUpdate iHelperAutoUpdate;
        private IViewAutoUpdate iViewAutoUpdate;

        public event EventHandler<EventArgs> EventSendUsageDataChanged;

        public string Name
        {
            get
            {
                return iHelper.Title;
            }
        }

        public void OpenConfiguration()
        {
            iPageMain.TrackPageVisibilityChange(true);
            iWindow.Show();
            iWindow.Focus();
            iWindow.WindowState = System.Windows.WindowState.Normal;
        }

        
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            // this prevents the UI framework from handling unhandled exceptions so that they are let throught
            // to be handled by the Linn code
            System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.ThrowException);

            // create the app helper
            iHelper = new Helper(Environment.GetCommandLineArgs());
            OptionPagePrivacy optionPagePrivacy = new OptionPagePrivacy(iHelper);
            iHelper.AddOptionPage(optionPagePrivacy);
            iHelper.ProcessOptionsFileAndCommandLine();

            // create crash log dumper
            ICrashLogDumper d = new CrashLogDumperWindow(Songbox.Properties.Resources.Icon, iHelper.Title, iHelper.Product, iHelper.Version);
            iHelper.AddCrashLogDumper(d);

            // create view and helper for auto updates
            iViewAutoUpdate = new Toolkit.Wpf.ViewAutoUpdateStandard(Songbox.Properties.Resources.Icon, Songbox.Properties.Resources.Image106x106);
            iViewAutoUpdate.EventButtonUpdateClicked += EventButtonUpdateClicked;
            iHelperAutoUpdate = new HelperAutoUpdate(iHelper, iViewAutoUpdate, new Invoker(this.Dispatcher));
            iHelperAutoUpdate.OptionPageUpdates.BetaVersions = iHelper.BuildType == EBuildType.Beta;
            iHelperAutoUpdate.Start();

            iPageMain = new Linn.Songbox.PageMain(iHelper, optionPagePrivacy, iHelperAutoUpdate, new StartAtLoginOption(iHelper.Title));

            IconInfo iconInfo = new IconInfo("logo.png", "image/png", 106, 106, 32);

            try
            {
                // create the media server
                iServer = new Server("git://github.com/linnoss/MediaApps.git", iHelper.Company, "http://www.linn.co.uk", iHelper.Title, "http://www.linn.co.uk", new Presentation(iPageMain), iconInfo);
            }
            catch (ApplicationException)
            {
                Shutdown(-1);
            }

            // create the configuration window
            iWindow = new ConfigurationWindow(Songbox.Properties.Resources.Icon, iHelper.Title, iServer);
            iWindow.Closing += EventWindowClosing;

            // create and show the sys tray icon
            iFormSysTray = new FormSysTray(this);
        }

        private void EventButtonUpdateClicked(object sender, EventArgs e)
        {
            // fix #1073 - perform time consuming shutdown code here to prevent a race with installer
            ShutdownApp();
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            ShutdownApp();
            
            if (iHelperAutoUpdate != null)
            {
                iHelperAutoUpdate.Dispose();
                iViewAutoUpdate.EventButtonUpdateClicked -= EventButtonUpdateClicked;
            }
        }

        private void ShutdownApp()
        {
            if (iServer != null)
            {
                iServer.Dispose();
                iServer = null;
            }

            if (iFormSysTray != null)
            {
                iFormSysTray.Close();
                iFormSysTray = null;
            }

            if (iWindow != null)
            {
                iWindow.Close();
                iWindow = null;
            }
        }

        private void EventWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            iPageMain.TrackPageVisibilityChange(false);
            // prevent the configuration window from being closed - just hide it            
            e.Cancel = true;
            if (iWindow != null)
            {
                iWindow.Hide();
            }
        }
    }
}
