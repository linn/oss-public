using System;
using System.Diagnostics;
using System.IO;


namespace Linn.MediaServer
{
    public class Updater
    {
        public static void RunInstaller()
        {
            string filename = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wizard"), "Updates"), "InstallerWizard.exe");
            
            try
            {
                Process.Start(filename);
            }
            catch { }
        }
    }
}

