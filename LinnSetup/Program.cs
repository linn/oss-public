using System;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

using Linn;
using Linn.Toolkit.WinForms;
using Linn.ProductSupport.Diagnostics;
using LinnSetup.Properties;


namespace LinnSetup
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] aArgs) {
            // create the application helper, adding a form based crash log dumper
            iHelper = new HelperLinnSetup(aArgs);
            iHelper.AddCrashLogDumper(new CrashLogDumperForm(iHelper.Title, iHelper.Product, iHelper.Version, iHelper.Icon));
            iHelper.ProcessOptionsFileAndCommandLine();

            iDiagnoctics = new Diagnostics();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // ensure that all unhandled exceptions in this thread bypass the
            // Application class handlers and let the AppDomain handlers that 
            // have been added in to the helper handle it
            if (HelperLinnSetup.IsWindows) {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            }
            else {
                Application.ThreadException += UnhandledExceptionNonWindows;
            }

            Application.Run(new FormLinnSetup(iHelper, iDiagnoctics, new AppletManager(iHelper, iDiagnoctics)));
            iHelper.Dispose();
        }

        static private void UnhandledExceptionNonWindows(object sender, ThreadExceptionEventArgs e) {
            iHelper.UnhandledException(e.Exception);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        static private HelperLinnSetup iHelper;
        static private Diagnostics iDiagnoctics = null;
    }



    public class HelperLinnSetup : Helper
    {
        public HelperLinnSetup(string[] aArgs)
            : base(aArgs) {
            // create icons
            if (!IsWindows) {
                // mono can't currently handle layered icons, so don't load true icon, just use logo (image file)
                iIcon = Icon.FromHandle(Linn.Toolkit.WinForms.Properties.Resources.LinnLogo.GetHicon());
            }
            else {
                try {
                    // attempt to load icon from resource file
                    iIcon = Linn.Toolkit.WinForms.Properties.Resources.Icon;
                }
                catch {
                    // load icon file from disc instead - try the default installer location
                    string fullpath = Path.GetFullPath(Path.Combine(Application.StartupPath, "linn.ico"));
                    if (File.Exists(fullpath)) {
                        iIcon = new Icon(fullpath);
                    }
                    else {
                        // load icon file from disc instead - try the default build location
                        fullpath = Path.GetFullPath(Path.Combine(Application.StartupPath, "../share/Linn/Core/linn.ico"));
                        if (File.Exists(fullpath)) {
                            iIcon = new Icon(fullpath);
                        }
                        else {
                            // default to logo (image file) if no icon file can be found
                            iIcon = Icon.FromHandle(Linn.Toolkit.WinForms.Properties.Resources.LinnLogo.GetHicon());
                        }
                    }
                }
            }
            Size smallIconSize = new Size(16, 16);
            try {
                smallIconSize = SystemInformation.SmallIconSize;
            }
            catch {
                // not supported for all platforms
            }
            iSmallIcon = new Icon(iIcon, smallIconSize);
            iWarningIcon = Icon.FromHandle(Linn.Toolkit.WinForms.Properties.Resources.Warning.GetHicon());

            // add application specific user options
            iOptionPageUpdates = new OptionPageUpdates(this);
            iApplicationOptions = new ApplicationOptions(this);

            AddOptionPage(iOptionPageUpdates);
        }

        public Icon Icon {
            get { return iIcon; }
        }

        public Icon SmallIcon {
            get { return iSmallIcon; }
        }

        public Icon WarningIcon {
            get { return iWarningIcon; }
        }

        public string TempDirectoryPath {
            get { return kTempDirectoryPath; }
        }

        public OptionPageUpdates OptionPageUpdates {
            get { return iOptionPageUpdates; }
        }

        public ApplicationOptions ApplicationOptions {
            get { return iApplicationOptions; }
        }

        static public bool IsWindows {
            get {
                return ((Environment.OSVersion.Platform == PlatformID.Win32NT) || (Environment.OSVersion.Platform == PlatformID.Win32S) || (Environment.OSVersion.Platform == PlatformID.Win32Windows));
            }
        }

        static public NumberFormatInfo Nfi {
            get {
                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
                return nfi;
            }
        }

        public void SetDoubleBuffered(System.Windows.Forms.Control aControl) {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(System.Windows.Forms.Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, aControl, new object[] { true });
        }

        public string GetTempFilePathExe(string aName) {
            string path = Path.Combine(GetTempDirectory(), aName + ".exe");
            return path;
        }

        public string GetTempFilePathZip(string aName) {
            string path = Path.Combine(GetTempDirectory(), aName + ".zip");
            return path;
        }

        public string GetTempDirectory() {
            string path = Path.Combine(kTempDirectoryPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);
            return path;
        }

        private readonly string kTempDirectoryPath = Path.Combine(Path.GetTempPath(), "LinnSetup");
        private Icon iIcon;
        private Icon iSmallIcon;
        private Icon iWarningIcon;
        private OptionPageUpdates iOptionPageUpdates;
        private ApplicationOptions iApplicationOptions;
    }

    public class ComparerListView : System.Collections.IComparer
    {
        public ComparerListView(int aColumn) {
            iColumn = aColumn;
        }
        public int Compare(object x, object y) {
            ListViewItem itemX = (ListViewItem)x;
            ListViewItem itemY = (ListViewItem)y;
            return String.Compare(itemX.SubItems[iColumn].Text, itemY.SubItems[iColumn].Text);
        }
        private int iColumn;
    }
}
