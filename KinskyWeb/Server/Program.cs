using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using KinskyWeb.Comms;
using System.ServiceModel.Activation;
using KinskyWeb.Services;
using KinskyWeb.TestStubs;
using KinskyWeb.Kinsky;
using Linn.Kinsky;
using System.Threading;
using Upnp;
using System.Windows.Forms;
using System.Net;
using Linn;

namespace KinskyWeb
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            if (Environment.OSVersion.Platform.Equals(PlatformID.MacOSX))
            {
                Application.Run(new MainFormMac());
            }
            else
            {
                Application.Run(new MainForm());
            }
        }
    }
}
