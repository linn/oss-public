using System;
using System.Diagnostics;
using System.IO;


namespace Linn.Songcast
{
    public class Updater
    {
        public static void RunInstaller()
        {
            string updatesFolder = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library"), "Songcast"), "Updates");
            string mountPoint = Path.Combine(updatesFolder, "MountPoint");
            string dmgFile = Path.Combine(updatesFolder, "InstallerSongcast.dmg");
            string pkgSrc = Path.Combine(mountPoint, "Installer.pkg");
            string pkgDst = Path.Combine(updatesFolder, "Installer.pkg");

            // mount the dmg files
            Process p = Process.Start("hdiutil", string.Format("attach {0} -mountpoint {1}", dmgFile, mountPoint));
            p.WaitForExit();

            // delete any existing pkg install packages
            p = Process.Start("rm", string.Format("-rf {0}", pkgDst));
            p.WaitForExit();

            // copy the installer out
            p = Process.Start("cp", string.Format("-Rf {0} {1}", pkgSrc, pkgDst));
            p.WaitForExit();

            // unmount the dmg
            p = Process.Start("hdiutil", string.Format("detach {0}", mountPoint));
            p.WaitForExit();

            // run the installer
            Process.Start(pkgDst);
        }
    }
}

