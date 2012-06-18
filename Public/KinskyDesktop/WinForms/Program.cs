using System;
using System.Threading;
using System.Windows.Forms;

using Linn;
using Linn.Topology;
using Linn.Kinsky;
using Linn.Toolkit.WinForms;

namespace KinskyDesktop
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] aArgs)
        {
            //Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // ensure that all unhandled exceptions in this thread bypass the
            // Application class handlers and let the AppDomain handlers that 
            // have been added in to the helper handle it
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            // create the application helper, adding a form based crash log dumper
            HelperKinsky helper = new HelperKinsky(aArgs, new Invoker());
            ICrashLogDumper d = new CrashLogDumperForm(helper.Title,
                                                       helper.Product,
                                                       helper.Version,
                                                       System.Drawing.Icon.FromHandle(Linn.Kinsky.Properties.Resources.KinskyLogo.GetHicon()));
            helper.AddCrashLogDumper(d);
            helper.ProcessOptionsFileAndCommandLine();


            Application.Run(new FormKinskyDesktop(helper));
            
            helper.Dispose();
        }

    }
}
