using System;
using System.Xml;
using System.Globalization;

namespace Linn
{
    public class VersionSupport
    {

        public static int CompareVersions(string aVersion, string aCompareVersion)
        {
            uint family = FamilyNumber(aVersion);
            uint family2 = FamilyNumber(aCompareVersion);
            if (family != family2)
            {
                return family < family2 ? -1 : 1;
            }
            else
            {
                uint major = Release(aVersion);
                uint major2 = Release(aCompareVersion);
                if (major < major2)
                {
                    return -1;
                }
                else if (major > major2)
                {
                    return 1;
                }
            }
            return Build(aVersion).CompareTo(Build(aCompareVersion));
        }

        public static int CompareProxyVersions(string aVersion, string aCompareVersion) {
            string currVersion = aCompareVersion.ToLower(CultureInfo.InvariantCulture);
            if (currVersion.StartsWith("sp")) {
                currVersion = currVersion.Remove(0, currVersion.IndexOf("v") + 1);
            }
            else if (currVersion.StartsWith("s")) {
                currVersion = currVersion.Remove(0, 1);
            }
            string latestVersion = aVersion.ToLower(CultureInfo.InvariantCulture);
            if (latestVersion.StartsWith("sp")) {
                latestVersion = latestVersion.Remove(0, currVersion.IndexOf("v") + 1);
            }
            else if (latestVersion.StartsWith("s")) {
                latestVersion = latestVersion.Remove(0, 1);
            }
            try {
                uint current = uint.Parse(currVersion);
                uint latest = uint.Parse(latestVersion);
                if (current < latest) {
                    return 1;
                }
            }
            catch(FormatException) { }
            return -1;
        }

        public static VersionSupport Instance
        {
            get
            {
                if (iInstance == null)
                {
                    iInstance = new VersionSupport();
                }
                return iInstance;
            }
        }

        public static string Family(string aSoftwareVersion)
        {
            string[] split = aSoftwareVersion.Split('.');
            if (split.Length > 0)
            {
                int index = int.Parse(split[0]) - 1;
                if (index >= 0 && index < VersionSupport.Instance.kFamilyNames.Length)
                {
                    return VersionSupport.Instance.kFamilyNames[index];
                }
            }
            return kFamilyUnknown;
        }

        public static uint FamilyNumber(string aSoftwareVersion)
        {
            string[] split = aSoftwareVersion.Split('.');
            if (split.Length > 0)
            {
                return uint.Parse(split[0]);
            }
            return 0;
        }

        public static uint Release(string aSoftwareVersion)
        {
            string[] split = aSoftwareVersion.Split('.');
            if (split.Length > 1)
            {
                return uint.Parse(split[1]);
            }
            return 0;
        }

        public static uint Build(string aSoftwareVersion)
        {
            string[] split = aSoftwareVersion.Split('.');
            if (split.Length > 2)
            {
                return uint.Parse(split[2]);
            }
            return 0;
        }

        public static string SoftwareVersionPretty(string aSoftwareVersion, bool aIncludeFull)
        {
            if (aSoftwareVersion != null && aSoftwareVersion.Contains(".") && !aSoftwareVersion.Contains("-"))
            {
                string result = Family(aSoftwareVersion) + " " + Release(aSoftwareVersion);
                if (Build(aSoftwareVersion) > 1000)
                {
                    result = Family(aSoftwareVersion) + " Nightly Build (" + aSoftwareVersion + ")";
                }
                else if (aIncludeFull)
                {
                    result += " (" + aSoftwareVersion + ")";
                }
                return result;
            }
            else
            {
                return aSoftwareVersion;
            }
        }

        // new software naming convention: Family.Version.BuildNumber (3.2.37 = Cara 2 build 37)
        public readonly string[] kFamilyNames = new string[] { "Auskerry", "Bute", "Cara", "Davaar", "Eriska" };
        public const string kFamilyUnknown = "Unknown";

        static private VersionSupport iInstance = null;
    }
} // Linn