using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Linn;

namespace LinnDiagnostics
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] aArgs)
        {
            Helper helper = new Helper(aArgs);

            helper.ProcessOptionsFileAndCommandLine();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new Form1(helper));
            }
            catch (Exception e)
            {
                Trace.WriteLine(Trace.kTopology, "Exception!       " + e);
            }

            Trace.WriteLine(Trace.kTopology, "Completed");
        }
    }
}
