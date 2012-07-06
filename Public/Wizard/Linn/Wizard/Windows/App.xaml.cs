using System;
using System.Windows;
using System.ComponentModel;
using System.Threading;

using Linn;
using Linn.Toolkit;
using Linn.Toolkit.Wpf;

using OpenHome.Xapp;

namespace Linn.Wizard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string kName = "Linn Wizard";

        private Helper iHelper;
        private HelperAutoUpdate iHelperAutoUpdate;
        private MainWindow iMainWindow;

        private void Application_Startup(object sender, StartupEventArgs e) {
            // this prevents the UI framework from handling unhandled exceptions so that they are let throught
            // to be handled by the Linn code
            System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.ThrowException);

            // create helpers
            iHelper = new Helper(e.Args);
            ICrashLogDumper d = new CrashLogDumperWindow(Wizard.Properties.Resources.Icon, iHelper.Title, iHelper.Product, iHelper.Version);
            iHelper.AddCrashLogDumper(d);

            iHelperAutoUpdate = new HelperAutoUpdate(iHelper, new Linn.Toolkit.Wpf.ViewAutoUpdateStandard(Wizard.Properties.Resources.Icon, Wizard.Properties.Resources.IconImage), new Invoker(this.Dispatcher));
            iHelperAutoUpdate.Start();

            iHelper.ProcessOptionsFileAndCommandLine();

            // create components
            iXapp = new Framework("PageHtml");
            iWebServer = new WebServer(iXapp);
            iControl = new PageControl(iHelper, iXapp, "PageHtml/Resources", "PageDefinitions.xml");

            // create the main window
            iMainWindow = new MainWindow(iHelper, iWebServer.ResourceUri, iControl);
            iMainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            iXapp.Dispose();
            iWebServer.Dispose();
            iControl.Close();

            if (iHelperAutoUpdate != null) {
                iHelperAutoUpdate.Dispose();
            }
        }

        private Framework iXapp;
        private WebServer iWebServer;
        private PageControl iControl;
    }
}
