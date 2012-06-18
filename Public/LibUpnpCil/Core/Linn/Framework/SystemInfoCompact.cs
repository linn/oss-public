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
		internal static string ServicePack {
			get {
				return "";
			}
		}

        internal static PlatformId Platform
        {
            get
            {
                return PlatformIdConverters.From(Environment.OSVersion.Platform);
            }
        }
		
		internal static string VersionString {
			get {
                return Environment.OSVersion.Platform.ToString();
			}
		}
		
		internal static string ComputerName {
			get {
				return "";
			}
		}

        internal static DirectoryInfo DataPathForApp(string aAppTitle)
        {
            return new DirectoryInfo(Path.Combine("\\Application Data", aAppTitle));
        }		     
		
		internal static DirectoryInfo ExePathForApp(string aAppTitle)
        {
            return new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName));
        }
	}
}


