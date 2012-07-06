using System;
using System.Diagnostics;
using System.IO;


namespace Linn.Wizard
{
    public class Updater
    {
        public static void RunInstaller()
        {
            string filename = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library"), "Wizard"), "Updates"), "InstallerWizard.pkg");

            try
            {
                Process.Start(filename);
            }
            catch { }
        }
    }
}

