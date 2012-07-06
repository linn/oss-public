using System;
using System.Diagnostics;
using System.IO;


namespace Linn.Songbox
{
    public class Updater
    {
        public static void RunInstaller()
        {
            string filename = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library"), "Songbox"), "Updates"), "InstallerSongbox.pkg");

            try
            {
                Process.Start(filename);
            }
            catch { }
        }
    }
}

