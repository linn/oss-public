using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Linn;
using Linn.Control;

namespace UpnpSpy
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

            Application.Run(new Form1(helper));
            helper.Dispose();
        }
    }


}
