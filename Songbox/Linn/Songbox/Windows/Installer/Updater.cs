using System;
using System.Diagnostics;
using System.IO;


namespace Linn.Songbox
{
    public class Updater
    {
        public static void RunInstaller()
        {
            string filename = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Songbox"), "Updates"), "InstallerSongbox.exe");
            
            try
            {
                Process.Start(filename);
            }
            catch { }
        }
    }
}

