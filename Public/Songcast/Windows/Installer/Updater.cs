using System;
using System.Diagnostics;
using System.IO;


namespace Linn.Songcaster
{
    public class Updater
    {
        public static void RunInstaller()
        {
            string oldFilename = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Songcaster"), "Updates"), "InstallerSongcast.exe");
            string newFilename = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Songcast"), "Updates"), "InstallerSongcast.exe");
            
            try
            {
                if(File.Exists(newFilename))
                {
                    Process.Start(newFilename);
                }
                else
                {
                    Process.Start(oldFilename);
                }
            }
            catch { }
        }
    }
}

