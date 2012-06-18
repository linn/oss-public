using System;
using System.Windows.Forms;
using Linn;

namespace LinnSysTray
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
            Application.Run(new LinnSysTrayForm(helper));
            helper.Dispose();
        }
    }
}
