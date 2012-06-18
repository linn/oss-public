using System;
using System.Threading;
using System.IO;
using System.Security;      // SecurityException
using System.Reflection;    // Assembly loading
using System.Net;           // WebClient
using System.Collections.Generic;
using System.Xml;

namespace Linn
{
    public class AutoUpdate : IDisposable
    {
        [Flags]
        public enum EUpdateType
        {
            Nightly = 0x00,
            Development = 0x01,
            Beta = 0x02,
            Stable = 0x04
        }

        public class EventArgsUpdateFound : EventArgs
        {
            public EventArgsUpdateFound(AutoUpdateInfo aInfo)
            {
                Info = aInfo;
            }

            public AutoUpdateInfo Info;
        }

        public class AutoUpdateInfo
        {
            public AutoUpdateInfo(string aName, string aVersion, Uri aHistory, Uri aUri, EUpdateType aUpdateType, bool aIsCompatibilityFamilyUpgrade)
            {
                Name = aName;
                Version = aVersion;
                History = aHistory;
                Uri = aUri;
                UpdateType = aUpdateType;
                IsCompatibilityFamilyUpgrade = aIsCompatibilityFamilyUpgrade;
            }
            public EUpdateType UpdateType;
            public string Name;
            public string Version;
            public Uri History;
            public Uri Uri;
            public string FileName { get; set; }
            public bool IsCompatibilityFamilyUpgrade { get; set; }
        }

        public static readonly string kDefaultFeedLocation = "http://oss.linn.co.uk/Feeds/Updates/Application.xml";

        public static readonly string kTargetWindows = "win32";
        public static readonly string kTargetMacOsX = "macosx";
        public static readonly string kTargetIos = "ios";
        public static readonly string kTargetWindowsCe = "windowsce";

        public AutoUpdate(IHelper aHelper, string aUpdateFeedLocation, int aUpdateInterval, EUpdateType aUpdateTypes, uint aUpdateVersion)
        {
            AutoUpdate.EUpdateType currentBuildType = AutoUpdate.EUpdateType.Stable;
            if (aHelper.Product.Contains("(NightlyBuild)"))
            {
                currentBuildType = AutoUpdate.EUpdateType.Nightly;
            }
            else if (aHelper.Product.Contains("(Beta)"))
            {
                currentBuildType = AutoUpdate.EUpdateType.Beta;
            }
            else if (aHelper.Product.Contains("(Development)"))
            {
                currentBuildType = AutoUpdate.EUpdateType.Development;
            }

            string applicationTarget = string.Empty;
            switch(SystemInfo.Platform)
            {
                case PlatformId.Win32NT:
                case PlatformId.Win32S:
                case PlatformId.Win32Windows:
                    applicationTarget = kTargetWindows;
                    break;
                case PlatformId.MacOSX:
                    applicationTarget = kTargetMacOsX;
                    break;
                case PlatformId.IOS:
                    applicationTarget = kTargetIos;
                    break;
                case PlatformId.WinCE:
                    applicationTarget = kTargetWindowsCe;
                    break;
                default:
                    Assert.Check(false);
                    break;
            }

            Initialise(aHelper, aUpdateFeedLocation, aUpdateInterval, aUpdateTypes, aHelper.Title, applicationTarget, aUpdateVersion, currentBuildType);
        }

        public AutoUpdate(IHelper aHelper, string aUpdateFeedLocation, int aUpdateInterval, EUpdateType aUpdateTypes, string aApplicationName, string aApplicationTarget, uint aUpdateVersion, EUpdateType aApplicationBuildType)
        {
            Initialise(aHelper, aUpdateFeedLocation, aUpdateInterval, aUpdateTypes, aApplicationName, aApplicationTarget, aUpdateVersion, aApplicationBuildType);
        }

        private void Initialise(IHelper aHelper, string aUpdateFeedLocation, int aUpdateInterval, EUpdateType aUpdateTypes, string aApplicationName, string aApplicationTarget, uint aUpdateVersion, EUpdateType aApplicationBuildType)
        {
            iHelper = aHelper;
            iApplicationBuildType = aApplicationBuildType;
            iUpdateFolder = Path.Combine(iHelper.DataPath.FullName, "Updates");
            try
            {
                if (!Directory.Exists(iUpdateFolder))
                {
                    Directory.CreateDirectory(iUpdateFolder);
                }
            }
            catch (Exception ex)
            {
                UserLog.WriteLine("Failed to create update folder: " + ex.ToString());
            }
            iApplicationName = aApplicationName;
            iApplicationTarget = aApplicationTarget;
            UpdateTypes = aUpdateTypes;
            iUpdateVersion = aUpdateVersion;

            iUpdateFeedLocation = aUpdateFeedLocation;
            iUpdateInterval = aUpdateInterval;

            CleanupTemporaryFiles();        // do some housekeeping
        }

        public void Dispose()
        {
            Stop();
            CleanupTemporaryFiles();        // do some housekeeping
        }

        public void Start()
        {
            if (iUpdateInterval > 0)
            {
                iUpdateTimer = new Timer(iUpdateInterval);
                iUpdateTimer.Enabled = true;
                iUpdateTimer.AutoReset = true;
                iUpdateTimer.Elapsed += TimerElapsed;
                iUpdateTimer.Start();
            }

            Thread t = new Thread(DoCheckForUpdate);
            t.IsBackground = true;
            t.Name = "Initial Auto Update Check";
            t.Start();
        }

        public void Stop()
        {
            if (iUpdateTimer != null)
            {
                iUpdateTimer.Stop();
                iUpdateTimer.Elapsed -= TimerElapsed;
                iUpdateTimer = null;
            }
        }

        public int UpdateInterval
        {
            get
            {
                return iUpdateInterval;
            }
            set
            {
                Stop();
                iUpdateInterval = value;
                Start();
            }
        }

        public int UpdateProgress
        {
            get
            {
                return iUpdateProgress;
            }
        }

        public AutoUpdateInfo CheckForUpdate()
        {
            WebResponse response = null;
            Stream stream = null;
            try
            {
                WebRequest request = WebRequest.Create(iUpdateFeedLocation);
                request.Credentials = CredentialCache.DefaultCredentials;
                if (request.Proxy != null)
                {
                    request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                }
                response = request.GetResponse();
                stream = response.GetResponseStream();

                XmlDocument document = new XmlDocument();
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.XmlResolver = null;
                settings.ProhibitDtd = true;
                XmlReader xmlReader = XmlTextReader.Create(stream, settings);
                document.Load(xmlReader);

                XmlNamespaceManager xmlNsMan = new XmlNamespaceManager(document.NameTable);
                xmlNsMan.AddNamespace("ns", "urn:linn-co-uk/autoupdate");

                string latestVersion = iHelper.Version;
                string product = iHelper.Product;                

                AutoUpdateInfo result = null;

                KeyValuePair<EUpdateType, string>[] versionTypeNodeNames = new KeyValuePair<EUpdateType, string>[] 
                {
                    new KeyValuePair<EUpdateType, string>(EUpdateType.Stable, "stable"),
                    new KeyValuePair<EUpdateType, string>(EUpdateType.Beta, "beta"),
                    new KeyValuePair<EUpdateType, string>(EUpdateType.Development, "development"),
                    new KeyValuePair<EUpdateType, string>(EUpdateType.Nightly, "nightly")
                };

                foreach (XmlNode applicationNode in document.SelectNodes(string.Format("/ns:autoupdate/ns:updateinfo[@version='{0}']/ns:application[@name='{1}']",iUpdateVersion, iApplicationName), xmlNsMan))
                {
                    Uri historyUri = new Uri(applicationNode.SelectSingleNode("ns:history", xmlNsMan).FirstChild.Value);

                    foreach (KeyValuePair<EUpdateType, string> versionType in versionTypeNodeNames)
                    {
                        if ((UpdateTypes & versionType.Key) == versionType.Key)
                        {
                            foreach (XmlNode versionNode in applicationNode.SelectNodes(string.Format("ns:{0}", versionType.Value), xmlNsMan))
                            {
                                string version = versionNode.Attributes["version"].Value;

                                if (ProductSupport.CompareVersions(version, latestVersion) > 0
                                    || (ProductSupport.CompareVersions(version, latestVersion) == 0 && versionType.Key > iApplicationBuildType))
                                {
                                    XmlNode uriNode = versionNode.SelectSingleNode(String.Format("ns:url[@target='{0}']", iApplicationTarget), xmlNsMan);
                                    if (uriNode != null)
                                    {
                                        UserLog.WriteLine(String.Format("{0} update available: {1}", versionType.Value, version));
                                        Uri uri = new Uri(uriNode.InnerText);
                                        result = new AutoUpdateInfo(iApplicationName, version, historyUri, uri, versionType.Key, ProductSupport.Family(iHelper.Version) != ProductSupport.Family(version));
                                        latestVersion = version;
                                    }
                                }
                            }
                        }
                    }

                }
                return result;

            }
            catch (Exception ex)
            {
                UserLog.WriteLine(String.Format("Error caught checking for updates: {0}", ex.ToString()));
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }

            return null;
        }

        public void DownloadUpdate(AutoUpdateInfo aInfo)
        {
            iUpdateProgress = 0;
            if (EventUpdateProgress != null)
            {
                EventUpdateProgress(this, EventArgs.Empty);
            }

            int startingPoint = 0;
            string tempFile = string.Format("{0}{2}{1}", aInfo.Name, aInfo.Version, ".part");
            tempFile = Path.Combine(iUpdateFolder, tempFile);
            if (File.Exists(tempFile))
            {
                startingPoint = (int)(new FileInfo(tempFile).Length);
            }

            try
            {
                HttpWebRequest headRequest = (HttpWebRequest)WebRequest.Create(aInfo.Uri);
                headRequest.Method = "HEAD";
                HttpWebResponse headResponse = (HttpWebResponse)headRequest.GetResponse();
                int contentLength = Int32.Parse(headResponse.Headers[HttpResponseHeader.ContentLength]);

                if (contentLength > startingPoint)
                {

                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(aInfo.Uri);
                    request.Credentials = CredentialCache.DefaultCredentials;
                    if (request.Proxy != null)
                    {
                        request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    }
                    request.AddRange(startingPoint);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream responseSteam = response.GetResponseStream();
                    FileStream fileStream = null;
                    if (startingPoint == 0 || response.StatusCode != HttpStatusCode.PartialContent)
                    {
                        fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    }
                    else
                    {
                        fileStream = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    }

                    int bytesSize;
                    long fileSize = response.ContentLength;
                    Trace.WriteLine(Trace.kCore, "fileSize=" + fileSize);
                    byte[] downloadBuffer = new byte[kPacketLength];
                    int total = startingPoint;

                    iUpdateProgress = (int)((total / (float)fileSize) * 100.0f);
                    if (EventUpdateProgress != null)
                    {
                        EventUpdateProgress(this, EventArgs.Empty);
                    }

                    while ((bytesSize = responseSteam.Read(downloadBuffer, 0, downloadBuffer.Length)) > 0)
                    {
                        fileStream.Write(downloadBuffer, 0, bytesSize);

                        total += bytesSize;
                        iUpdateProgress = (int)((total / (float)fileSize) * 100.0f);

                        if (EventUpdateProgress != null)
                        {
                            EventUpdateProgress(this, EventArgs.Empty);
                        }
                    }

                    if (fileStream != null)
                    {
                        fileStream.Close();
                        fileStream.Dispose();
                    }
                    if (response != null)
                    {
                        response.Close();
                    }
                }
                string fileName = Path.Combine(iUpdateFolder, Guid.NewGuid().ToString() + ".dll");
                File.Move(tempFile, fileName);
                aInfo.FileName = fileName;
            }
            catch (Exception e)
            {
                UserLog.WriteLine(String.Format("Error downloading update: {0}", e));
                InvalidUpdateFile(aInfo);
            }
        }

        public bool ApplyUpdate(AutoUpdateInfo aInfo)
        {
            bool result = true;
            try
            {
                Assembly updateAssembly = Assembly.LoadFile(new FileInfo(aInfo.FileName).FullName);
                ExtractFiles(aInfo, updateAssembly);
            }
            catch (Exception e)
            {
                InvalidUpdateFile(aInfo);
                UserLog.WriteLine(String.Format("Error applying update: {0}", e));
                result = false;
            }

            CleanupTemporaryFiles();        // do some housekeeping

            return result;
        }

        public event EventHandler<EventArgsUpdateFound> EventUpdateFound;
        public event EventHandler<EventArgs> EventUpdateProgress;
        public event EventHandler<EventArgs> EventUpdateFailed;

        private void TimerElapsed(object aSender, EventArgs args)
        {
            DoCheckForUpdate();
        }

        private void DoCheckForUpdate()
        {
            Trace.WriteLine(Trace.kCore, ">AutoUpdate.OnCheckForUpdate");

            AutoUpdateInfo info = CheckForUpdate();
            if (info != null)
            {
                if (EventUpdateFound != null)
                {
                    EventUpdateFound(this, new EventArgsUpdateFound(info));
                }
            }
        }

        private void ExtractFiles(AutoUpdateInfo aInfo, Assembly aUpdateAssembly)
        {
            // Get resource names from update assembly
            string[] resources = aUpdateAssembly.GetManifestResourceNames();
            Dictionary<string, string> renameLog = new Dictionary<string, string>();
            try
            {
                foreach (string resource in resources)
                {
                    string path = Path.Combine(iUpdateFolder, resource);
                    // If a current file exists with the same name, rename it
                    if (File.Exists(path))
                    {
                        string tempName = CreateTemporaryFilename();
                        File.Move(path, tempName);
                        renameLog[tempName] = path;
                    }
                    // Copy the resource out into the new file
                    // this does not take into consideration file dates and other similar
                    // attributes (but probobly should).
                    FileInfo info = new FileInfo(path);
                    using (Stream res = aUpdateAssembly.GetManifestResourceStream(resource), file = new FileStream(path, FileMode.CreateNew))
                    {
                        Int32 pseudoByte;
                        while ((pseudoByte = res.ReadByte()) != -1)
                        {
                            file.WriteByte((Byte)pseudoByte);
                        }
                    }
                }
                // If we made it this far, it is safe to rename the update assembly
                MoveUpdateToTemporaryFile(aInfo);

                Type[] types = aUpdateAssembly.GetTypes();
                foreach (Type t in types)
                {
                    MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static);
                    foreach (MethodInfo m in methods)
                    {
                        if (m.GetParameters().Length == 0 && m.ReturnType == typeof(void))
                        {
                            // Found one, lets call it
                            m.Invoke(null, null);
                        }
                    }
                }
            }
            catch
            {
                // Unwind failed operation
                foreach (KeyValuePair<string, string> rename in renameLog)
                {
                    string filename = rename.Value;
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                    File.Move(rename.Key, filename);
                }
                throw; // rethrow whatever went wrong
            }
        }

        private void InvalidUpdateFile(AutoUpdateInfo aInfo)
        {
            MoveUpdateToTemporaryFile(aInfo);

            if (EventUpdateFailed != null)
            {
                EventUpdateFailed(this, EventArgs.Empty);
            }
        }

        private void MoveUpdateToTemporaryFile(AutoUpdateInfo aInfo)
        {
            string tempFile = CreateTemporaryFilename();
            try
            {
                if (aInfo.FileName != null)
                {
                    File.Move(aInfo.FileName, tempFile);
                }
            }
            catch (IOException e)
            {
                UserLog.WriteLine(String.Format("Cannot move {0} to {1}", Path.GetFileName(aInfo.Uri.LocalPath), tempFile));
                UserLog.WriteLine(e.ToString());
            }
        }

        private string CreateTemporaryFilename()
        {
            return Path.Combine(iUpdateFolder, Guid.NewGuid().ToString() + ".utmp");
        }

        private void CleanupTemporaryFiles()
        {
            if (Directory.Exists(iUpdateFolder))
            {
                string[] files = Directory.GetFiles(iUpdateFolder, "*.utmp");
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                    catch (SecurityException) { }
                }
            }
        }

        public EUpdateType UpdateTypes { get; set; }

        private const int kPacketLength = 2048;

        private IHelper iHelper;
        private string iApplicationName;
        private string iApplicationTarget;

        private string iUpdateFeedLocation;

        private int iUpdateInterval;
        private int iUpdateProgress;
        private Timer iUpdateTimer;
        private uint iUpdateVersion;
        private string iUpdateFolder;
        private EUpdateType iApplicationBuildType;
    }

} // Linn

