using System;
using System.IO;
using System.Reflection;


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

        internal static PlatformId Platform
        {
            get
            {
                return PlatformId.MacOSX;
            }
        }

        internal static string VersionString
        {
            get
            {
                return String.Format("{0} ({1})", "MacOsX", Environment.OSVersion.VersionString);
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
            return new DirectoryInfo(Path.Combine(
					Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.Personal),
					"Library"),
                aAppTitle));
        }
						        
		internal static DirectoryInfo ExePathForApp(string aAppTitle)
        {
            return new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName));
        }
    }
}
