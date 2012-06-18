using System;
using System.Windows.Forms;

using Linn;

namespace TestMediaServerSearch
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(string[] aArgs)
        {
            IHelper helper = new Helper(aArgs);

            helper.ProcessCommandLine();
            Application.Run(new Form1(helper));
            helper.Dispose();
        }
    }
}
