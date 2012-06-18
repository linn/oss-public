using System;
using System.IO;


namespace Linn
{
    /// <summary>
    /// Description of Environment.
    /// </summary>
    internal static class SystemInfo
    {
        internal static string ServicePack
        {
            get
            {
                return Environment.OSVersion.ServicePack;
            }
        }

        internal static string VersionString
        {
            get
            {
                return Environment.OSVersion.VersionString;
            }
        }

        internal static string ComputerName
        {
            get
            {
                return System.Environment.MachineName;
            }
        }
        
        internal static DirectoryInfo DataPathForApp(string aAppTitle)
        {
            return new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
        }

        internal static DirectoryInfo ExePathForApp(string aAppTitle)
        {
            return DataPathForApp(aAppTitle);
        }
    }
}
