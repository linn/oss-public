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
        bool StartAtLogin { get; set; }
        void OpenConfiguration();
        void CheckForUpdates();
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IMediaServerApp
    {
        private FormSysTray iFormSysTray;
        private ConfigurationWindow iWindow;
        private Helper iHelper;
        private HelperAutoUpdate iHelperAutoUpdate;
        private Server iServer;

        public string Name
        {
            get
            {
                return iHelper.Title;
            }
        }

        public void OpenConfiguration()
        {
            iWindow.Show();
            iWindow.Focus();
            iWindow.WindowState = System.Windows.WindowState.Normal;
        }

        public void CheckForUpdates()
        {
            iHelperAutoUpdate.CheckForUpdates();
        }

        public bool StartAtLogin
        {
            get
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
                if (key != null)
                {
                    object o = key.GetValue(iHelper.Title);

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
                            key.SetValue(iHelper.Title, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String);
                        }
                        else
                        {
                            key.DeleteValue(iHelper.Title);
                        }

                        key.Close();
                    }
                }
            }
        }

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            // this prevents the UI framework from handling unhandled exceptions so that they are let throught
            // to be handled by the Linn code
            System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.ThrowException);

            // create the app helper
            iHelper = new Helper(Environment.GetCommandLineArgs());
            iHelper.ProcessOptionsFileAndCommandLine();

            // create crash log dumper
            ICrashLogDumper d = new CrashLogDumperWindow(Songbox.Properties.Resources.Icon, iHelper.Title, iHelper.Product, iHelper.Version);
            iHelper.AddCrashLogDumper(d);

            // create view and helper for auto updates
            IViewAutoUpdate autoUpdateView = new Toolkit.Wpf.ViewAutoUpdateStandard(Songbox.Properties.Resources.Icon, Songbox.Properties.Resources.Image106x106);
            iHelperAutoUpdate = new HelperAutoUpdate(iHelper, autoUpdateView, new Invoker(this.Dispatcher));
            //iHelperAutoUpdate.OptionPageUpdates.BetaVersions = true;
            iHelperAutoUpdate.Start();

            try
            {
                // create the media server
                iServer = new Server("git://github.com/linnoss/MediaApps.git", iHelper.Company, "http://www.linn.co.uk", iHelper.Title, "http://www.linn.co.uk");
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

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            if (iHelperAutoUpdate != null)
            {
                iHelperAutoUpdate.Dispose();
            }

            if (iServer != null)
            {
                iServer.Dispose();
            }

            if (iFormSysTray != null)
            {
                iFormSysTray.Close();
            }

            if (iWindow != null)
            {
                iWindow.Close();
            }
        }

        private void EventWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // prevent the configuration window from being closed - just hide it
            e.Cancel = true;
            iWindow.Hide();
        }
    }
}
