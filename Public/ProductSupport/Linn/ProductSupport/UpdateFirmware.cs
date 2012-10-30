using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Xml;
using System.IO;


namespace Linn.ProductSupport
{
    public interface IUpdateFirmwareHandler
    {
        void Started();
        void OverallProgress(int aValue);
        void Status(string aMessage);
        void Completed();
        void Error(string aMessage);
    }

    public class UpdateFirmware : IUpdateFirmwareHandler, Flash.IConsole
    {
        public UpdateFirmware(Helper aHelper) {
            iHelper = aHelper;
        }

        public void Start(string aUrl, string aVariant, string aUglyName, string aRoom, string aName, string aSoftwareUpdateVersion, bool aNoTrust, IUpdateFirmwareHandler aUserHandler) {
            if (!InProgress) {
                Lock();
                iUrl = aUrl;
                iVariant = aVariant;
                iUglyName = aUglyName;
                iRoom = aRoom;
                iName = aName;
                iSoftwareUpdateVersion = aSoftwareUpdateVersion;
                iNoTrust = aNoTrust;
                iUserConsole = aUserHandler;
                iIsFaceDef = false;
                iUpdateThread = new Thread(Reflash);
                Unlock();
                iUpdateThread.IsBackground = false;
                iUpdateThread.Start();
            }
        }

        public void RestoreFactoryDefaults(string aUglyName, string aRoom, string aName, IUpdateFirmwareHandler aUserHandler) {
            if (!InProgress) {
                Lock();
                iUrl = null;
                iVariant = null;
                iUglyName = aUglyName;
                iRoom = aRoom;
                iName = aName;
                iSoftwareUpdateVersion = "Restore Factory Defaults";
                iNoTrust = false;
                iUserConsole = aUserHandler;
                iIsFaceDef = true;
                iUpdateThread = new Thread(ResetToFacDef);
                Unlock();
                iUpdateThread.IsBackground = false;
                iUpdateThread.Start();
            }
        }

        public bool InProgress {
            get {
                Lock();
                bool inProgress = (iUpdateThread != null);
                Unlock();
                return inProgress;
            }
        }

        // IConsole interface
        public void Title(string aMessage) {
            UserLog.Write(aMessage + Environment.NewLine);
            UpdateProgress(aMessage);
            iLastTitle = aMessage + Environment.NewLine;
        }
        public void Write(string aMessage) {
            UserLog.Write(aMessage);
            UpdateProgress(aMessage);
            iLastMessage += aMessage;
            this.Status(iLastTitle + iLastMessage);
        }
        public void Newline() {
            UserLog.WriteLine("");
            iLastMessage = "";
        }
        public void ProgressOpen(int aMax){
            iProgressMax = aMax;
            iLastPercent = 0;
            this.Status(iLastTitle + iLastMessage + "0%");
        }
        public void ProgressSetValue(int aValue) {
            int percent = 0;
            if (iProgressMax > 0 && aValue <= iProgressMax) {
                percent = ((aValue * 100) / iProgressMax);
            }
            if ((percent != iLastPercent) && (percent % 5 == 0)) {
                // cut down to 5% steps
                UpdateProgress(iLastMessage);
                this.Status(iLastTitle + iLastMessage + percent + "%");
                iLastPercent = percent;
            }
        }
        public void ProgressClose() {
            this.Status(iLastTitle + iLastMessage + "100%");
        }
        // IUpdateFirmwareHandler interface
        public void Started() {
            UserLog.WriteLine("Update Firmware Started" + (iIsFaceDef ? " (Restore Factory Defaults)" : ""));
            iUpdateProgress = 0;
            this.OverallProgress(0);
            this.Status("Update Firmware Started");
            this.Status("Update Firmware Started" + (iIsFaceDef ? " (Restore Factory Defaults)" : ""));
            if (iUserConsole != null) {
                iUserConsole.Started();
            }
        }
        public void OverallProgress(int aValue) {
            iOverallProgress = aValue;
            if (iUserConsole != null) {
                iUserConsole.OverallProgress(aValue);
            }
        }
        public void Status(string aStatus) {
            string updateMessage = "Update " + iRoom + " (" + iName + ") --> " + iSoftwareUpdateVersion + " .............. " + iOverallProgress + "%" + Environment.NewLine + Environment.NewLine;
            if (iUserConsole != null) {
                iUserConsole.Status(updateMessage + aStatus + Environment.NewLine);
            }
        }
        public void Completed() {
            UserLog.WriteLine("Update Firmware Completed Successfully" + (iIsFaceDef ? " (Restore Factory Defaults)" : ""));
            //iUpdateProgress = 0;
            this.OverallProgress(100);
            this.Status("Update Firmware Completed Successfully" + (iIsFaceDef ? " (Restore Factory Defaults)" : ""));
            iUpdateProgress = 0;
            if (iUserConsole != null) {
                iUserConsole.Completed();
            }
        }
        public void Error(string aMessage) {
            UserLog.WriteLine("Update Firmware Error" + (iIsFaceDef ? " (Restore Factory Defaults)" : "") + ": " + aMessage);
            iUpdateProgress = 0;
            this.OverallProgress(100);
            this.Status("Update Firmware Error" + (iIsFaceDef ? " (Restore Factory Defaults)" : "") + ": " + aMessage);
            if (iUserConsole != null) {
                iUserConsole.Error(aMessage);
            }
        }

        private void Reflash() {
            const string kFilePrefix = "Reprogram_";
            bool result = false;
            string failMessage = null;
            Flash.Reprogrammer reprog = null;
            string zipFileName = null;
            string unzippedDirectoryName = null;
            try {
                this.Started();

                // delete any existing files used for downloading and reflashing
                this.Write("Cleaning up existing files .............. ");
                CleanupTempFiles(iHelper.DataPath.FullName, kFilePrefix, this);
                this.Newline();

                // download files
                this.Write("Downloading required files .............. ");
                DateTime now = DateTime.Now;
                zipFileName = Path.Combine(iHelper.DataPath.FullName, kFilePrefix + now.ToString("u").Replace(":", "-") + now.Millisecond + ".zip");
                DownloadFile(iUrl, zipFileName, this);
                this.Newline();

                // unpack files
                this.Write("Unpacking required files .............. ");
                string romFileName = GetRomFromZip(zipFileName, iVariant, this, out unzippedDirectoryName);
                this.Newline();

                // create the reprogrammer and setup as required
                reprog = new Flash.Reprogrammer(iHelper.Interface.Interface.Info.IPAddress, this, iUglyName, romFileName);
                reprog.Fallback = false;
                reprog.NoExec = false;
                reprog.Wait = true; // wait to discover device after reprogramming
                reprog.NoTrust = iNoTrust;

                // reprogram
                result = reprog.Execute(out failMessage);
            }
            catch (Exception e) {
                failMessage = e.Message;
                result = false;
            }
            finally {
                if (reprog != null) {
                    try {
                        reprog.Close();
                    }
                    catch (Exception e) {
                        failMessage = e.Message;
                        result = false;
                    }
                }

                try {
                    // cleanup thread
                    Lock();
                    iUpdateThread = null;
                    Unlock();

                    this.Newline();

                    // delete files used for reflashing
                    this.Write("Cleaning up temp files .............. ");
                    CleanupTempFiles(iHelper.DataPath.FullName, kFilePrefix, this);
                    this.Newline();

                    // notify update is complete
                    if (result) {
                        this.Completed();
                    }
                    else {
                        this.Error(failMessage);
                    }
                }
                catch (Exception e) {
                    UserLog.WriteLine("Failure on Reflash Completion: " + e.Message);
                }
            }     
        }

        private void ResetToFacDef() {
            bool result = false;
            string failMessage = null;
            Flash.FactoryDefaulter facdef = null;
            try {
                this.Started();

                // create the facdefer
                facdef = new Flash.FactoryDefaulter(iHelper.Interface.Interface.Info.IPAddress, this, iUglyName);

                // set the options
                facdef.NoExec = false;
                facdef.Wait = true; // wait to discover device after reprogramming

                // Reset to facdef
                result = facdef.Execute(out failMessage);
            }
            catch (Exception e) {
                failMessage = e.Message;
                result = false;
            }
            finally {
                if (facdef != null) {
                    try {
                        facdef.Close();
                    }
                    catch (Exception e) {
                        failMessage = e.Message;
                        result = false;
                    }
                }

                try {
                    // cleanup thread
                    Lock();
                    iUpdateThread = null;
                    Unlock();

                    this.Newline();

                    // notify update is complete
                    if (result) {
                        this.Completed();
                    }
                    else {
                        this.Error(failMessage);
                    }
                }
                catch (Exception e) {
                    UserLog.WriteLine("Failure on Restore Factory Defaults Completion: " + e.Message);
                }
                iIsFaceDef = false;
            }    
        }

        private void DownloadFile(string aUrl, string aFilename, Flash.IConsole aConsole) {
            int bytesdone = 0;
            Stream rStream = null;
            Stream lStream = null;
            HttpWebResponse response = null;
            int fileSize = 0;
            try {
                aConsole.ProgressOpen(fileSize);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(aUrl);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;

                if (request != null) {
                    response = (HttpWebResponse)request.GetResponse();
                }
                if (response != null) {
                    fileSize = (int)response.ContentLength;
                    rStream = response.GetResponseStream();
                    lStream = File.Create(aFilename);
                    byte[] buffer = new byte[2048]; // read in 2K chunks
                    int bytesRead;
                    int currPercent = 0;
                    int lastPercent = 0;
                    aConsole.ProgressOpen(fileSize); // open progress console with max value
                    do {
                        bytesRead = rStream.Read(buffer, 0, buffer.Length);
                        lStream.Write(buffer, 0, bytesRead);
                        bytesdone += bytesRead;
                        currPercent = (bytesdone * 100) / fileSize;
                        if ((currPercent != lastPercent) && (currPercent % 5 == 0)) {
                            // mono/win forms can't handle lots of events - cut down to 5% steps
                            aConsole.ProgressSetValue(bytesdone);
                            lastPercent = currPercent;
                        }
                    } while (bytesRead > 0);
                }
            }
            finally {
                if (response != null) {
                    response.Close();
                }
                if (rStream != null) {
                    rStream.Close();
                }
                if (lStream != null) {
                    lStream.Close();
                }
            }
            aConsole.Write("Complete");
            aConsole.ProgressClose();
            UserLog.Write(" [Downloaded File: " + aUrl + " --> " + aFilename + " (" + bytesdone + " bytes)]");
        }

        private class GetRomFromZipFailed : System.Exception
        {
            public GetRomFromZipFailed(string aMessage) : base("Get Rom From Zip Failed: " + aMessage) { }
        }

        private string GetRomFromZip(string aZipFile, string aVariant, Flash.IConsole aConsole, out string aUnzippedDirectory) {
            // unzip downloaded file to temp directory
            aConsole.ProgressOpen(100);
            aUnzippedDirectory = Path.Combine(iHelper.DataPath.FullName, Path.GetFileNameWithoutExtension(aZipFile));
            aConsole.ProgressSetValue(10);
            ICSharpCode.SharpZipLib.Zip.FastZip fz = new ICSharpCode.SharpZipLib.Zip.FastZip();
            aConsole.ProgressSetValue(20);
            fz.ExtractZip(aZipFile, aUnzippedDirectory, ""); // "" is for file filter
            aConsole.ProgressSetValue(60);

            // Only one main folder per zip file (i.e. Reprog_Date/SXXXXXXXX)
            string[] mainDirs = Directory.GetDirectories(aUnzippedDirectory, "*", SearchOption.TopDirectoryOnly);
            if (mainDirs.Length != 1) {
                throw new GetRomFromZipFailed("File structure is invalid in " + aUnzippedDirectory);
            }
            aConsole.ProgressSetValue(70);
            // Directory below main directory must be the aVariant directory (i.e. Reprog_Date/SXXXXXXXX/AkuarateDsMk1)
            string[] dirs = Directory.GetDirectories(mainDirs[0], aVariant, SearchOption.TopDirectoryOnly);
            aConsole.ProgressSetValue(80);
            if (dirs.Length <= 0) {
                throw new GetRomFromZipFailed("Could not find " + aVariant + " directory in " + mainDirs[0]);
            }
            else if (dirs.Length > 1) {
                throw new GetRomFromZipFailed("Found multiple " + aVariant + " directories in " + mainDirs[0]);
            }
            // Only one collection file in the aVariant directory (i.e. Reprog_Date/SXXXXXXXX/AkuarateDsMk1/bin/CollectionAkurateDs.xml)
            string[] files = Directory.GetFiles(dirs[0], "Collection*.xml", SearchOption.AllDirectories);
            aConsole.ProgressSetValue(90);
            if (files.Length <= 0) {
                throw new GetRomFromZipFailed("Could not find required Collection file for " + aVariant + " in " + dirs[0]);
            }
            else if (files.Length > 1) {
                throw new GetRomFromZipFailed("Found multiple Collection files for " + aVariant + " in " + dirs[0]);
            }

            aConsole.Write("Complete");
            aConsole.ProgressClose();
            UserLog.Write(" [ROM file selected: " + files[0] + "]");
            return files[0];
        }

        private void CleanupTempFiles(string aPath, string aPrefix, Flash.IConsole aConsole) {
            try {
                aConsole.ProgressOpen(100);
                string[] fileList = Directory.GetFiles(aPath, (aPrefix + "*"));
                aConsole.ProgressSetValue(25);
                string[] dirList = Directory.GetDirectories(aPath, (aPrefix + "*"));
                aConsole.ProgressSetValue(50);
                foreach (string file in fileList) {
                    File.Delete(file);
                }
                aConsole.ProgressSetValue(75);
                foreach (string dir in dirList) {
                    Directory.Delete(dir, true); // true is for delete recursively
                }
                aConsole.ProgressSetValue(90);
            }
            catch (Exception e) {
                UserLog.WriteLine("Error: " + e.Message);
            }
            aConsole.Write("Complete");
            aConsole.ProgressClose();
        }

        private void Lock() {
            iMutex.WaitOne();
        }

        private void Unlock() {
            iMutex.ReleaseMutex();
        }

        private void UpdateProgress(string aMessage) {
            if (aMessage.Contains("F->F")) {
                iFallbackChecked = true;
            }
            else if (aMessage.Contains("F->M")) {
                iFallbackChecked = true;
            }
            else if (aMessage.Contains("Reboot to main")) {
                SetUpdateProgress(true);
            }
            else if (aMessage.Contains("M->M")) {
                if (!iFallbackChecked) {
                    iFallbackIncluded = false;
                }
            }
            else {
                SetUpdateProgress(false);
            }
        }

        private void SetUpdateProgress(bool aIsFinalStage) {
            int maxSteps = iIsFaceDef ? 300 : (iFallbackIncluded ? 1100 : 600);
            int finalStagePercent = iIsFaceDef ? 90 : (iFallbackIncluded ? 97 : 95);

            if (aIsFinalStage) {
                int finalStageSteps = (maxSteps * finalStagePercent) / 100;
                if (iUpdateProgress < finalStageSteps) {
                    iUpdateProgress = finalStageSteps;
                }
                else {
                    iUpdateProgress++;
                }
            }
            else {
                iUpdateProgress++;
            }

            if (iUpdateProgress >= maxSteps) {
                this.OverallProgress(99);
            }
            else {
                this.OverallProgress((iUpdateProgress * 100) / maxSteps);
            }
        }

        private Thread iUpdateThread = null;
        private Mutex iMutex = new Mutex();
        private IUpdateFirmwareHandler iUserConsole = null;
        private Helper iHelper;
        private string iUrl;
        private string iVariant;
        private string iUglyName;
        private bool iNoTrust;
        private int iUpdateProgress = 0;

        private string iRoom;
        private string iName;
        private string iSoftwareUpdateVersion;
        private int iProgressMax = 0;
        private int iLastPercent = 0;
        private string iLastTitle = "";
        private string iLastMessage = "";
        private int iOverallProgress = 0;
        private bool iIsFaceDef = false;
        private bool iFallbackIncluded = true;
        private bool iFallbackChecked = false;
    }

    public class UpdateCheck
    {
        public EventHandler<EventArgs> EventUpdateCheckComplete;
        public EventHandler<EventArgsUpdateError> EventUpdateCheckError;

        public UpdateCheck() {
        }

        public void Start() {
            if (!InProgress) {
                UserLog.WriteLine("Update Check Started");
                Lock();
                iUpdateCheckComplete = false;
                iUpdateCheckFailed = false;
                iUpdateCheckThread = new Thread(UpdateCheckStart);
                Unlock();
                iUpdateCheckThread.IsBackground = true;
                iUpdateCheckThread.Priority = ThreadPriority.BelowNormal;
                iUpdateCheckThread.Start();
            }
        }

        public bool InProgress {
            get {
                Lock();
                bool inProgress = (iUpdateCheckThread != null);
                Unlock();
                return inProgress;
            }
        }

        public bool Failed {
            // only relevant if Update check has been started and is not in progress
            get {
                Lock();
                bool failed = iUpdateCheckFailed;
                Unlock();
                return failed;
            }
        }

        public void GetInfo(string aModel, bool aIsProxy, string aSoftwareVersion, string[] aBoardNumber, out bool aAvailable, out string aVersion, out string aUrl, out string aVariant, out string aReleaseNotesHtml) {
            aAvailable = false;
            aVersion = null;
            aUrl = null;
            aVariant = null;
            aReleaseNotesHtml = null;

            Lock();
            if (!iUpdateCheckComplete) {
                Unlock();
                return;
            }
            Unlock();

            UpdateCheck.UpdateInfo updateInfo = null;
            if (aModel != null) {
                if (aIsProxy) {
                    iUpdateInfoProxy.TryGetValue(aModel, out updateInfo);
                }
                else {
                    iUpdateInfo.TryGetValue(aModel, out updateInfo);
                }
            }

            if (updateInfo != null && aSoftwareVersion != null) {
                if (aIsProxy) {
                    aAvailable = (VersionSupport.CompareProxyVersions(updateInfo.Version, aSoftwareVersion) > 0);
                }
                else {
                    aAvailable = (VersionSupport.CompareVersions(updateInfo.Version, aSoftwareVersion) > 0);
                }
                aVersion = updateInfo.Version;
                aUrl = updateInfo.Url;
            }

            aVariant = GetDeviceVariant(aBoardNumber);

            if (aModel != null) {
                XmlDocument document = new XmlDocument();
                document.LoadXml(iReleaseNotesXml);
                aReleaseNotesHtml = "<html><head><meta content=\"text/html; charset=ISO-8859-1\" http-equiv=\"content-type\"><title>Release Notes</title></head><style type=\"text/css\">body {color:#FFF;background:#000;}a {color:#2D8FBF;text-decoration:none;}a:active {color:#939393;text-decoration:none;}</style><body>";
                foreach (XmlNode n in document.SelectNodes("/rss/channel/item")) {
                    string title = n["title"].InnerText;
                    string date = n["pubDate"].InnerText;
                    string details = n["description"].InnerText.Replace("![CDATA[", "").Replace("]]", "");
                    string link = n["link"] != null ? n["link"].InnerText : null;

                    if ((title.StartsWith("Linn DS") && (details.Contains(aModel + ",") || (details.Contains(aModel + ".")) || (details.Contains(aModel + " ")))) || (title.StartsWith(aModel + " "))) {
                        aReleaseNotesHtml += "<h4>" + (link != null ? "<a href=\"" + link + "\">" : "") + title + (link != null ? "</a>" : "") + "</h4>" + details + date + "<br/><hr/>";
                    }

                }
                aReleaseNotesHtml += "</body></html>";
                aReleaseNotesHtml = aReleaseNotesHtml.Replace("<br><br>", "<br>");
            }
        }

        private string GetDeviceVariant(string[] aBoardNumber)
        {
            VariantInfo result = null;
            string variantName = string.Empty;

            foreach (VariantInfo i in iVariantInfo)
            {
                foreach (string s in aBoardNumber)
                {
                    if (i.Pcas == s)
                    {
                        // can't just break when we find the first match as this wouldn't work for Renew products (it would match the base product first)
                        result = i;
                    }
                }

                if (result != null)
                {
                    if(string.IsNullOrEmpty(result.BasePcas))
                    {
                        // Pcas match found, can't exit here as it may be a Renew
                        variantName = result.Name;
                    }
                    else
                    {
                        foreach (string s in aBoardNumber)
                        {
                            if (i.BasePcas == s)
                            {
                                // Pcas match found and BasePcas match found = gauranteed to be a Renew product (so exit here)
                                return result.Name;
                            }
                        }
                    }
                }
            }

            return variantName;
        }

        internal class UpdateInfo
        {
            public UpdateInfo(string aModel, string aVersion, string aUrl) {
                iModel = aModel;
                iVersion = aVersion;
                iUrl = aUrl;
                UserLog.WriteLine(aModel + ": " + aVersion + " (" + Family + ") - " + aUrl);
            }

            public UpdateInfo(string aModel, string aVersion) {
                iModel = aModel;
                iVersion = aVersion;
                iUrl = null;
                UserLog.WriteLine(aModel + " (Proxy): " + aVersion);
            }

            public string Model {
                get { return iModel; }
            }

            public string Version {
                get { return iVersion; }
            }

            public string Family {
                get { return VersionSupport.Family(iVersion); }
            }

            public string Url {
                get { return iUrl; }
            }

            private string iModel;
            private string iVersion;
            private string iUrl;
        }

        internal class VariantInfo
        {
            public VariantInfo(string aPcas, string aName)
                : this(aPcas, null, aName) {
            }

            public VariantInfo(string aPcas, string aBasePcas, string aName) {
                UserLog.WriteLine("Variant: " + aPcas + (aBasePcas == null ? "" : " (Base " + aBasePcas + ")") + "-" + aName);
                iPcas = aPcas;
                iBasePcas = aBasePcas;
                iName = aName;
            }

            public string Pcas {
                get { return iPcas; }
            }

            public string BasePcas {
                get { return iBasePcas; }
            }

            public string Name {
                get { return iName; }
            }

            private string iPcas;
            private string iBasePcas;
            private string iName;
        }

        private void UpdateCheckStart() {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(kUpdatesAvailableUrl);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;

                try {
                    request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                }
                catch {
                    // not supported for all platforms
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                XmlDocument document = new XmlDocument();
                document.Load(response.GetResponseStream());

                XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
                nsManager.AddNamespace("linn", kLinnNamespace);

                // get update info
                foreach (XmlNode n in document.SelectNodes("/rss/channel/" + kReleasedDeviceTag, nsManager)) {
                    string model = n["linn:model"].InnerText;
                    string type = n["linn:type"].InnerText;
                    string version = n["linn:version"].InnerText;
                    //string family = n["linn:family"].InnerText;
                    string url = n["linn:url"].InnerText;

                    UpdateInfo updateInfo = null;
                    if (type == "Main") {
                        updateInfo = new UpdateInfo(model, version, url);
                        if (iUpdateInfo.ContainsKey(model)) {
                            iUpdateInfo[model] = updateInfo;
                        }
                        else {
                            iUpdateInfo.Add(model, updateInfo);
                        }
                    }
                    else if (type == "Proxy") {
                        updateInfo = new UpdateInfo(model, version);
                        if (iUpdateInfoProxy.ContainsKey(model)) {
                            iUpdateInfoProxy[model] = updateInfo;
                        }
                        else {
                            iUpdateInfoProxy.Add(model, updateInfo);
                        }
                    }
                }

                // get variant info
                foreach (XmlNode n in document.SelectNodes("/rss/channel/linn:variantList/linn:variant", nsManager)) {
                    string pcas = n["linn:pcas"].InnerText;
                    string basepcas = null;
                    if (n["linn:basepcas"] != null) {
                        basepcas = n["linn:basepcas"].InnerText;
                    }
                    string name = n["linn:name"].InnerText;
                    VariantInfo variantInfo = new VariantInfo(pcas, basepcas, name);
                    iVariantInfo.Add(variantInfo);
                }

                // get release notes
                request = (HttpWebRequest)WebRequest.Create(kReleaseNotesUrl);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;

                try {
                    request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                }
                catch {
                    // not supported for all platforms
                }

                response = (HttpWebResponse)request.GetResponse();

                document = new XmlDocument();
                document.Load(response.GetResponseStream());
                iReleaseNotesXml = document.OuterXml;

                Lock();
                iUpdateCheckThread = null;
                Unlock();

                iUpdateCheckComplete = true;
                iUpdateCheckFailed = false;

                if (EventUpdateCheckComplete != null) {
                    EventUpdateCheckComplete(this, EventArgs.Empty);
                }
            }
            catch (Exception e) {
                Lock();
                iUpdateCheckThread = null;
                Unlock();

                iUpdateCheckFailed = true;
                if (EventUpdateCheckError != null) {
                    EventUpdateCheckError(this, new EventArgsUpdateError(e.Message));
                }
            }
        }

        private void Lock() {
            iMutex.WaitOne();
        }

        private void Unlock() {
            iMutex.ReleaseMutex();
        }

        private const string kUpdatesAvailableUrl = "http://products.linn.co.uk/VersionInfo/LatestVersionInfo.xml";
        private const string kReleaseNotesUrl = "http://products.linn.co.uk/VersionInfo/ReleaseVersionInfo.xml";
        private const string kLinnNamespace = "http://products.linn.co.uk/VersionInfo/namespace";
        private const string kReleasedDeviceTag = "linn:release";

        private Thread iUpdateCheckThread = null;
        private bool iUpdateCheckComplete = false;
        private bool iUpdateCheckFailed = false;

        private SortedList<string, UpdateInfo> iUpdateInfo = new SortedList<string, UpdateInfo>(); // key = model
        private SortedList<string, UpdateInfo> iUpdateInfoProxy = new SortedList<string, UpdateInfo>(); // key = model
        private List<VariantInfo> iVariantInfo = new List<VariantInfo>(); // key = pcas number
        private string iReleaseNotesXml = null;
        private Mutex iMutex = new Mutex();
    }

    public class EventArgsUpdateError : EventArgs
    {
        public EventArgsUpdateError(string aErrorMessage) {
            iErrorMessage = aErrorMessage;
        }

        public string ErrorMessage {
            get { return iErrorMessage; }
        }

        private string iErrorMessage;
    }
}
