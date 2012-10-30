using System;
using System.Collections.Generic;
using System.Windows;

using Linn.Toolkit;
using Linn.Toolkit.Wpf;

using OpenHome.Xapp;

namespace Linn.Konfig
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Helper iHelper;
        private HelperAutoUpdate iHelperAutoUpdate;
        private MainWindow iMainWindow;
        private XappController iXappController;
        private ViewerBrowser iViewer;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // create helpers
            iHelper = new Helper(e.Args);
            iHelper.ProcessOptionsFileAndCommandLine();

            ICrashLogDumper d = new CrashLogDumperWindow(ResourceManager.IconLarge, iHelper.Title, iHelper.Product, iHelper.Version);
            iHelper.AddCrashLogDumper(d);

            iHelperAutoUpdate = new HelperAutoUpdate(iHelper, new Linn.Toolkit.Wpf.ViewAutoUpdateStandard(Konfig.ResourceManager.Icon, Konfig.Properties.Resources.IconLarge), new Invoker(this.Dispatcher));
            iHelperAutoUpdate.Start();

            // create the main window
            iMainWindow = new MainWindow(iHelper.Product, iHelper.Title);
            iMainWindow.Show();

            Preferences preferences = new Preferences(iHelper);
            Model.Instance = new Model(preferences);

            // create the xapp controller and view
            Invoker invoker = new Invoker(this.Dispatcher);
            SettingsPageAdvanced settings = new SettingsPageAdvanced(invoker, Model.Instance, preferences, iHelperAutoUpdate, "settings", "settings");
            UpdateListenerRepeater listeners = new UpdateListenerRepeater(new IUpdateListener[] { iMainWindow, settings });
            iXappController = new XappController(invoker, iHelper, Model.Instance, preferences, settings, listeners);
            iViewer = new ViewerBrowser(iMainWindow.WebBrowser, iXappController.MainPageUri);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (iHelperAutoUpdate != null)
            {
                iHelperAutoUpdate.Dispose();
                iHelperAutoUpdate = null;
            }

            if (iXappController != null)
            {
                iXappController.Dispose();
                iXappController = null;
            }

            if (iViewer != null)
            {
                iViewer.Dispose();
                iViewer = null;
            }
        }
    }
}
