using System;
using System.Windows.Forms;
using Linn.Kinsky;
using Linn;

namespace TestOssKinskyMppLibraryCf
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(string[] aArgs)
        {
            AppNetwork appControl = new AppNetwork(aArgs);
            AppKinskyWinForm app = new AppKinskyWinForm(appControl);

            app.Start();
            Application.Run(new Form1(app));
            app.Stop();
        }
    }
}