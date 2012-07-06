using System;
using System.IO;
using System.Reflection;


namespace Linn
{
	/// <summary>
	/// Description of Environment.
	/// </summary>
	public static class SystemInfo
	{		
		public static string ServicePack {
			get {
				return "";
			}
		}

        public static PlatformId Platform
        {
            get
            {
                return PlatformIdConverters.From(Environment.OSVersion.Platform);
            }
        }
		
		public static string VersionString {
			get {
                return Environment.OSVersion.Platform.ToString();
			}
		}
		
		public static string ComputerName {
			get {
				return "";
			}
		}

        public static DirectoryInfo DataPathForApp(string aAppTitle)
        {
            return new DirectoryInfo(Path.Combine("\\Application Data", aAppTitle));
        }		     
		
		public static DirectoryInfo ExePathForApp(string aAppTitle)
        {
            return new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName));
        }
	}
}


