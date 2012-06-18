using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Globalization;
using System.Net.Cache;
using System.Threading;
using System.Windows.Forms;
using Linn;

namespace KinskyJukebox
{
    public partial class FormUpdate : Form
    {
        public FormUpdate(HelperKinskyJukebox aHelper, DEventCheckForUpdatesComplete aEventCheckForUpdatesComplete, DEventDownloadUpdateComplete aEventDownloadUpdateComplete) {
            iHelper = aHelper;
            iEventCheckForUpdatesComplete = aEventCheckForUpdatesComplete;
            iEventDownloadUpdateComplete = aEventDownloadUpdateComplete;
            InitializeComponent();
            this.Icon = Icon.FromHandle(Properties.Resources.Update.GetHicon());
        }

        private void buttonYes_Click(object sender, EventArgs e) {
            try {
                if (iHelper.IsWindows) {
                    iDownloadThread = new Thread(DownloadLatestVersion);
                    iDownloadThread.Name = "Download";
                    iDownloadThread.IsBackground = true;
                    iDownloadThread.Start();
                }
                else {
                    LoadWebPage();
                }
            }
            catch (Exception exc) {
                UserLog.WriteLine("Download update failed (init): " + exc.Message);
                BasicControlSetup("Download update failed" + Environment.NewLine + "The following error was encountered during initialisation: " + exc.Message);
                iEventDownloadUpdateComplete(false);
            }
        }

        private void buttonSkip_Click(object sender, EventArgs e) {
            if (buttonSkip.Text == "Skip") {
                iHelper.OptionPageUpdates.AutoUpdate = false;
            }
            else if (buttonSkip.Text == "Stop") {
                Linn.UserLog.WriteLine("Download stopped");
                iDownloadThread.Abort();
            }
        }

        private void SetProgress(int aPercent, string aMessage) {
            if (this.InvokeRequired) {
                this.Invoke(new DEventSetProgress(SetProgress), new object[] { aPercent, aMessage });
                return;
            }
            if (aPercent >= 0 && aPercent <= 100) {
                updateProgressBar.Value = aPercent;
                updateMessageBox.Text = aMessage;
            }
        }

        private void BasicControlSetup(string aMessage) {
            if (this.InvokeRequired) {
                this.Invoke(new DEventBasicMessage(BasicControlSetup), new object[] { aMessage });
                return;
            }
            SetProgress(0, aMessage);
            buttonSkip.Text = "OK";
            buttonSkip.DialogResult = DialogResult.No;
            buttonSkip.Visible = true;
            buttonYes.Visible = false;
            buttonNo.Visible = false;
            updateProgressBar.Visible = false;
            this.ControlBox = true;
        }

        private void UserInputControlSetup(string aMessage, bool aAllowSkip) {
            if (this.InvokeRequired) {
                this.Invoke(new DEventUserInput(UserInputControlSetup), new object[] { aMessage, aAllowSkip });
                return;
            }
            SetProgress(0, aMessage);
            if (aAllowSkip) {
                buttonSkip.Text = "Skip";
            }
            else {
                buttonSkip.Text = "Cancel";
            }
            buttonSkip.DialogResult = DialogResult.No;
            buttonSkip.Visible = true;
            buttonYes.Visible = true;
            buttonNo.Visible = true;
            updateProgressBar.Visible = false;
            this.ControlBox = true;
            this.Visible = false;
        }

        private void DownloadControlSetup() {
            if (this.InvokeRequired) {
                this.Invoke(new DEventDownload(DownloadControlSetup));
                return;
            }
            buttonSkip.Text = "Stop";
            buttonSkip.DialogResult = DialogResult.None;
            buttonSkip.Visible = true;
            buttonYes.Visible = false;
            buttonNo.Visible = false;
            updateProgressBar.Visible = true;
            this.ControlBox = false;
        }

        public void CheckForUpdates(bool aUserRequested) {
            iUserRequestedUpdate = aUserRequested;
            // compile version info from xml file
            try {
                iLatestVersion = null;
                iLatestUrl = null;
                iParseThread = new Thread(ParseVersionInfo);
                iParseThread.Name = "ParseVersionInfo";
                iParseThread.IsBackground = true;
                iParseThread.Start((string)kLatestVersionInfoUrl);
            }
            catch (Exception exc) {
                UserLog.WriteLine("Check for Updates Failed (init): " + exc.Message);
                if (iUserRequestedUpdate) {
                    BasicControlSetup("Check for Updates Failed" + Environment.NewLine + "The following error was encountered during initialisation: " + exc.Message);
                    iEventCheckForUpdatesComplete(true);
                }
            }
        }

        private void ParseVersionInfo(object aUri) {
            XmlTextReader reader = null;
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create((string)aUri);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                try {
                    request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                }
                catch {
                    // not supported for all platforms
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                reader = new XmlTextReader(response.GetResponseStream());
                reader.WhitespaceHandling = WhitespaceHandling.None;
                reader.MoveToContent();
                while (reader.Read()) {
                    if (reader.Name == kReleasedDeviceTag && reader.IsStartElement()) {
	                    PopulateVersionInfo(reader, kReleaseTypeStandard);
	                }
                    else if (iHelper.OptionPageUpdates.BetaVersions && reader.Name == kBetaDeviceTag && reader.IsStartElement()) {
                        PopulateVersionInfo(reader, kReleaseTypeBeta);
                    }
                    else if (reader.Name == kLatestVersionInfoFileTag && !reader.IsStartElement()) { // end tag
                        break;
                    }
                }
                if (CompareVersions(iLatestVersion, iHelper.Version)) {
                    string message = "Found a newer version of " + iHelper.Title + Environment.NewLine + "Existing Version: " + iHelper.Version + Environment.NewLine + "Latest Version: " + iLatestVersion + iLatestReleaseType + Environment.NewLine + Environment.NewLine + "Would you like to update to the latest version?";
                    if (iUserRequestedUpdate) {
                        UserInputControlSetup(message, false);
                    }
                    else {
                        UserInputControlSetup(message + Environment.NewLine + "(Click skip to turn off the automatic update check on startup)", true);
                    }
                    iEventCheckForUpdatesComplete(true);
                }
                else if (iUserRequestedUpdate) {
                    BasicControlSetup("No updates found");
                    iEventCheckForUpdatesComplete(true);
                }
                else {
                    iEventCheckForUpdatesComplete(false);
                }
            }
            catch (Exception e) {
                UserLog.WriteLine("Check for Updates Failed (parse xml error): " + e.Message);
                if (iUserRequestedUpdate) {
                    BasicControlSetup("Check for Updates Failed" + Environment.NewLine + "The following error was encountered while gathering the latest version information: " + e.Message);
                    iEventCheckForUpdatesComplete(true);
                }
            }
            finally {
                if (reader != null) {
                    reader.Close();
                }
            }
        }

        private void PopulateVersionInfo(XmlTextReader aReader, string aReleaseType) {
            string model = NextValue(aReader);
            if (model == iHelper.Title) {
                string type = NextValue(aReader);
                string version = NextValue(aReader);
                string family = NextValue(aReader);
                string url = NextValue(aReader);
                UserLog.WriteLine(model + " (" + type + "): " + version + " (" + family + ") - " + url + " <" + aReleaseType + ">");
                if (CompareVersions(version, iLatestVersion)) {
                    iLatestVersion = version;
                    iLatestUrl = url;
                    iLatestReleaseType = aReleaseType;
                }
            }
        }

        private string NextValue(XmlTextReader aReader) {
            aReader.Read(); // start tag
            aReader.Read(); // data
            string value = aReader.Value;
            aReader.Read(); // end tag
            return value;
        }

        private bool CompareVersions(string aLeftVersion, string aRightVersion) {
            // returns true if aLeftVersion > aRightVersion
            float leftMajor, leftMinor, rightMajor, rightMinor;
            ToNumber(aLeftVersion, out leftMajor, out leftMinor);
            ToNumber(aRightVersion, out rightMajor, out rightMinor);
            if (leftMajor > rightMajor || (leftMajor == rightMajor && leftMinor > rightMinor)) {
                return true;
            }
            return false;
        }

        private void ToNumber(string aVersion, out float aMajor, out float aMinor) {
            aMajor = 0;
            aMinor = 0;
            string[] list;
            try {
                list = aVersion.Split('.');
                if (list.Length > 2) {
                    aMajor = (float.Parse(list[0], Nfi) * 1000) + float.Parse(list[1], Nfi); // new style volkano version number A.B.C (major)
                    aMinor = float.Parse(list[2], Nfi);
                }
            }
            catch (Exception) {
            }
        }

        private NumberFormatInfo Nfi {
            get {
                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
                return nfi;
            }
        }

        private void DownloadLatestVersion() {
            string message = "";
            try {
                message = "Download";
                DownloadControlSetup();
                string file = Path.Combine(iHelper.TempDirectoryPath, Guid.NewGuid().ToString() + ".exe");
                DownloadFile(iLatestUrl, file);
                message = "Load";
                System.Diagnostics.Process.Start(file);
                iEventDownloadUpdateComplete(true);
            }
            catch (Exception exc) {
                if (exc.GetType() == typeof(ThreadAbortException)) {
                    message += " update failed" + Environment.NewLine + "Operation was cancelled by the user";
                }
                else {
                    message += " update failed" + Environment.NewLine + "The following error was encountered: " + exc.Message;
                    Linn.UserLog.WriteLine(message);
                }
                BasicControlSetup(message);
                iEventDownloadUpdateComplete(false);
            }
        }

        private void LoadWebPage() {
            string message = "";
            try {
                string url = "http://oss.linn.co.uk/trac/wiki/KinskyJukebox";
                message = "Load web page: " + url;
                BasicControlSetup(message);
                System.Diagnostics.Process.Start(url);
                iEventDownloadUpdateComplete(true);
            }
            catch (Exception exc) {
                message += " failed" + Environment.NewLine + "The following error was encountered: " + exc.Message;
                Linn.UserLog.WriteLine(message);
                BasicControlSetup(message);
                iEventDownloadUpdateComplete(false);
            }
        }

        private void DownloadFile(string aUrl, string aLocalFilename) {
            int bytesdone = 0;
            Stream rStream = null;
            Stream lStream = null;
            HttpWebResponse response = null;
            int fileSize = 0;
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(aUrl);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;

                if (request != null) {
                    response = (HttpWebResponse)request.GetResponse();
                }
                if (response != null) {
                    fileSize = (int)response.ContentLength;
                    rStream = response.GetResponseStream();
                    lStream = File.Create(aLocalFilename);
                    byte[] buffer = new byte[2048]; // read in 2K chunks
                    int bytesRead;
                    int currPercent = 0;
                    int lastPercent = 0;
                    do {
                        bytesRead = rStream.Read(buffer, 0, buffer.Length);
                        lStream.Write(buffer, 0, bytesRead);
                        bytesdone += bytesRead;
                        currPercent = (bytesdone * 100) / fileSize;
                        if (currPercent != lastPercent) {
                            SetProgress(currPercent, "Downloading " + iHelper.Title + " " + iLatestVersion + iLatestReleaseType + " .............. " + currPercent + "%");
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
            UserLog.WriteLine("Downloaded File: " + aUrl + "->" + aLocalFilename + "(" + bytesdone / 1000 + " kb)");
        }

        public delegate void DEventCheckForUpdatesComplete(bool aShow);
        public delegate void DEventDownloadUpdateComplete(bool aClose);

        private HelperKinskyJukebox iHelper;
        private delegate void DEventSetProgress(int aPercent, string aMessage);
        private delegate void DEventBasicMessage(string aMessage);
        private delegate void DEventUserInput(string aMessage, bool aAllowSkip);
        private delegate void DEventDownload();
        
        private const string kLatestVersionInfoUrl = "http://products.linn.co.uk/VersionInfo/LatestVersionInfo.xml";
        private const string kLatestVersionInfoFileTag = "channel";
        private const string kReleasedDeviceTag = "linn:release";
        private const string kBetaDeviceTag = "linn:beta";
        private const string kReleaseTypeStandard = "";
        private const string kReleaseTypeBeta = " (Beta: pre release)";
        private Thread iParseThread;
        private Thread iDownloadThread;
        private string iLatestVersion = null;
        private string iLatestUrl = null;
        private string iLatestReleaseType = null;
        private bool iUserRequestedUpdate = false;
        private DEventCheckForUpdatesComplete iEventCheckForUpdatesComplete = null;
        private DEventDownloadUpdateComplete iEventDownloadUpdateComplete = null;
    }
}
