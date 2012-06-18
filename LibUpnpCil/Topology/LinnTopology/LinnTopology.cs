using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Linn;

using System.Threading;

namespace LinnTopology
{	
    static class Program
    {	
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        private static void Main(string[] aArgs)
        {
            Helper helper = new Helper(aArgs);
            helper.ProcessOptionsFileAndCommandLine();

            Form1 form = new Form1();
            App app = new App(helper, form);

            Application.Run(form);
        }
    }
}
