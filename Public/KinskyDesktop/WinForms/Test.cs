using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace KinskyDesktopUpdate
{
    public class Updater
    {
        public static void PostInstall()
        {
            Thread t = new Thread(delegate()
            {
                Process.Start("InstallerKinskyDesktop.exe");
            });
            t.Start();
        }
    }
}

