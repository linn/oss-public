using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Linn.Core;
using Linn;

namespace SneakyLastFm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] aArgs)
        {
            AppNetwork appControl = new AppNetwork(aArgs);
            AppNetworkWinForm app = new AppNetworkWinForm(appControl);
            
            app.Start();
            app.Run(new FormSneakyLastFm(appControl.Interface));
            app.Stop();
        }
    }
}
