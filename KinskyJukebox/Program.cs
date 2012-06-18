using System;
using System.IO;
using System.Net;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

using Linn;
using Linn.Kinsky;
using Linn.Toolkit.WinForms;

namespace KinskyJukebox
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] aArgs)
        {
            // create the application helper, adding a form based crash log dumper
            iHelper = new HelperKinskyJukebox(aArgs);
            iHelper.AddCrashLogDumper(new CrashLogDumperForm(iHelper.Title, iHelper.Product, iHelper.Version, iHelper.Icon));
            iHelper.ProcessOptionsFileAndCommandLine();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // ensure that all unhandled exceptions in this thread bypass the
            // Application class handlers and let the AppDomain handlers that 
            // have been added in to the helper handle it
            if (iHelper.IsWindows) {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            }
            else {
                Application.ThreadException += UnhandledExceptionNonWindows;
            }

            Application.Run(new FormKinskyJukebox(iHelper));
            iHelper.Dispose();
        }
        
        static private void UnhandledExceptionNonWindows(object sender, ThreadExceptionEventArgs e) {
            iHelper.UnhandledException(e.Exception);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        static private HelperKinskyJukebox iHelper;
    }

    public class HelperKinskyJukebox : Helper, IStack
    {
        public HelperKinskyJukebox(string[] aArgs) : base(aArgs) {
            iHttpServer = new HttpServer(HttpServer.kPortKinskyJukebox);

            // add application specific user options
            iOptionPageSetup = new OptionPageSetup(this);
            iOptionPageOrganisation = new OptionPageOrganisation();
            iOptionPageWizard = new OptionPageWizard();
            iOptionPageUpdates = new OptionPageUpdates(this);
            iApplicationOptions = new ApplicationOptions(this);

            AddOptionPage(iOptionPageSetup);
            AddOptionPage(iOptionPageOrganisation);
            AddOptionPage(iOptionPageWizard);
            AddOptionPage(iOptionPageUpdates);

            Stack.SetStack(this);
        }

        void IStack.Start(IPAddress aIpAddress) {
            if (Interface.Interface != null) {
                iHttpServer.Start(Interface.Interface.Info.IPAddress);
            }
        }

        void IStack.Stop() {
            iHttpServer.Stop();
        }

        public Icon Icon {
            get { return kIcon; }
        }

        public HttpServer HttpServer {
            get { return iHttpServer; }
        }

        public OptionPageSetup OptionPageSetup {
            get { return iOptionPageSetup; }
        }

        public OptionPageOrganisation OptionPageOrganisation {
            get { return iOptionPageOrganisation; }
        }

        public OptionPageWizard OptionPageWizard {
            get { return iOptionPageWizard; }
        }

        public OptionPageUpdates OptionPageUpdates {
            get { return iOptionPageUpdates; }
        }

        public ApplicationOptions ApplicationOptions {
            get { return iApplicationOptions; }
        }

        public string TempDirectoryPath {
            get { return kTempDirectoryPath; }
        }

        public bool IsWindows {
            get {
                return ((Environment.OSVersion.Platform == PlatformID.Win32NT) || (Environment.OSVersion.Platform == PlatformID.Win32S) || (Environment.OSVersion.Platform == PlatformID.Win32Windows));
            }
        }

        private static string kTempDirectoryPath = Path.Combine(Path.GetTempPath(), "KinskyJukebox");
        private readonly Icon kIcon = Icon.FromHandle(Linn.Kinsky.Properties.Resources.KinskyLogo.GetHicon());
        private HttpServer iHttpServer = null;
        private OptionPageSetup iOptionPageSetup;
        private OptionPageOrganisation iOptionPageOrganisation;
        private OptionPageWizard iOptionPageWizard;
        private OptionPageUpdates iOptionPageUpdates;
        private ApplicationOptions iApplicationOptions;
    }

}
