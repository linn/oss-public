using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Linn.Konfig
{
    public class VersionInfoReader : IDisposable
    {
        public enum EUpdateType
        {
            Nightly = 0x00,
            Development = 0x01,
            Beta = 0x02,
            Stable = 0x04
        }

        public class UpdateInfo
        {
            public UpdateInfo(string aModel, string aType, string aVersion, string aFamily, EUpdateType aUpdateType)
            {
                iModel = aModel;
                iType = aType;
                iVersion = aVersion;
                iFamily = aFamily;
                iUri = null;
                iUpdateType = aUpdateType;
            }

            public UpdateInfo(string aModel, string aType, string aVersion, string aFamily, Uri aUri, EUpdateType aUpdateType)
            {
                iModel = aModel;
                iType = aType;
                iVersion = aVersion;
                iFamily = aFamily;
                iUri = aUri;
                iUpdateType = aUpdateType;
            }

            public string Model
            {
                get
                {
                    return iModel;
                }
            }

            public string Type
            {
                get
                {
                    return iType;
                }
            }

            public string Version
            {
                get
                {
                    return iVersion;
                }
            }

            public string Family
            {
                get
                {
                    return iFamily;
                }
            }

            public Uri Uri
            {
                get
                {
                    return iUri;
                }
            }

            public Uri ReleaseNotesUri
            {
                get
                {
                    switch (UpdateType)
                    {
                        case EUpdateType.Stable:
                            return kReleaseNotesStable;
                        case EUpdateType.Beta:
                            return kReleaseNotesBeta;
                        default:
                            return null;
                    }
                }
            }

            public EUpdateType UpdateType
            {
                get
                {
                    return iUpdateType;
                }
            }

            private static readonly Uri kReleaseNotesStable = new Uri("http://products.linn.co.uk/VersionInfo/ReleaseVersionInfo.xml");
            private static readonly Uri kReleaseNotesBeta = new Uri("http://products.linn.co.uk/VersionInfo/BetaVersionInfo.xml");

            private string iModel;
            private string iType;
            private string iVersion;
            private string iFamily;
            private Uri iUri;
            private EUpdateType iUpdateType;
        }

        public class VariantInfo
        {
            public VariantInfo(string aPcas, string aBasePcas, string aName)
            {
                iPcas = aPcas;
                iBasePcas = aBasePcas;
                iName = aName;
            }

            public string Pcas
            {
                get
                {
                    return iPcas;
                }
            }

            public string BasePcas
            {
                get
                {
                    return iBasePcas;
                }
            }

            public string Name
            {
                get
                {
                    return iName;
                }
            }

            private string iPcas;
            private string iBasePcas;
            private string iName;
        }

        public const string kDefaultUpdatesUrl = "http://products.linn.co.uk/VersionInfo/LatestVersionInfo.xml";

        public VersionInfoReader(string aUpdateFeedUri, uint aUpdateInterval, string aApplicationDataPath, EUpdateType aUpdateTypes, FirmwareCache aCache)
        {
            iLock = new object();
            iDisposed = false;

            iCache = aCache;

            iUpdateFeedUri = aUpdateFeedUri;
            iUpdateInterval = aUpdateInterval;
            iApplicationDataPath = aApplicationDataPath;
            iUpdateTypes = aUpdateTypes;
            iTimer = new System.Threading.Timer(TimerElapsed);

            iUpdateInfoList = new List<UpdateInfo>();
            iVariantInfoList = new List<VariantInfo>();
        }

        public void Dispose()
        {
            Stop();
            iTimer = null;

            lock (iLock)
            {
                iDisposed = true;
            }
        }

        public void Start()
        {
            iTimer.Change(0, iUpdateInterval);
        }

        public void Stop()
        {
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void SetUpdateTypes(EUpdateType aUpdateTypes)
        {
            Stop();

            lock (iLock)
            {
                iUpdateTypes = aUpdateTypes;
                iUpdateInfoList.Clear();
                iVariantInfoList.Clear();
            }

            Start();
        }

        private void TimerElapsed(object aObject)
        {
            CheckForUpdates();
        }

        public Firmware GetFirmware(string aModel, ReadOnlyCollection<string> aPcbNumberList)
        {
            lock (iLock)
            {
                string variant = GetDeviceVariant(aPcbNumberList);
                if (string.IsNullOrEmpty(variant))
                {
                    return null;
                }

                UpdateInfo info = null;

                foreach (UpdateInfo i in iUpdateInfoList)
                {
                    if (i.Model == aModel)
                    {
                        if (info == null || i.Version.CompareTo(info.Version) > 0)
                        {
                            info = i;
                        }
                    }
                }

                if (info != null)
                {
                    return new Firmware(info, variant);
                }

                return null;
            }
        }

        public string GetDeviceVariant(ReadOnlyCollection<string> aPcbNumberList)
        {
            lock (iLock)
            {
                VariantInfo result = null;
                string variantName = string.Empty;

                foreach (VariantInfo i in iVariantInfoList)
                {
                    foreach (string s in aPcbNumberList)
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
                            foreach (string s in aPcbNumberList)
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
        }

        public EventHandler<EventArgs> EventVersionInfoAvailable;

        private void CheckForUpdates()
        {
            WebResponse response = null;
            Stream stream = null;
            try
            {
                WebRequest request = WebRequest.Create(iUpdateFeedUri);
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
                settings.DtdProcessing = DtdProcessing.Prohibit;
                XmlReader xmlReader = XmlTextReader.Create(stream, settings);
                document.Load(xmlReader);

                string filename = Path.Combine(iApplicationDataPath, Path.GetFileName(iUpdateFeedUri));
                if(File.Exists(filename))
                {
                    string oldFileContents = File.ReadAllText(filename);
                    StringWriter writer = new StringWriter();
                    document.WriteTo(new XmlTextWriter(writer));
                    // if VersionInfo file has changed save the new one out for future comparisions and clear cached firmware
                    if(oldFileContents != writer.ToString())
                    {
                        UserLog.WriteLine("LatestVersionInfo.xml has changed -> clearing firmware cache");
                        File.WriteAllText(filename, writer.ToString());
                        iCache.Clear();
                    }
                }
                else
                {
                    document.Save(filename);
                }

                XmlNamespaceManager xmlNsMan = new XmlNamespaceManager(document.NameTable);
                xmlNsMan.AddNamespace("linn", "http://products.linn.co.uk/VersionInfo/namespace");

                KeyValuePair<EUpdateType, string>[] versionTypeNodeNames = new KeyValuePair<EUpdateType, string>[] 
                {
                    new KeyValuePair<EUpdateType, string>(EUpdateType.Stable, "linn:release"),
                    new KeyValuePair<EUpdateType, string>(EUpdateType.Beta, "linn:beta"),
                    new KeyValuePair<EUpdateType, string>(EUpdateType.Development, "linn:devel"),
                    new KeyValuePair<EUpdateType, string>(EUpdateType.Nightly, "linn:nightly")
                };

                bool disposed = false;
                lock (iLock)
                {
                    foreach(KeyValuePair<EUpdateType, string> versionType in versionTypeNodeNames)
                    {
                        if ((iUpdateTypes & versionType.Key) == versionType.Key)
                        {
                            XmlNodeList nodes = document.SelectNodes("/rss/channel/" + versionType.Value, xmlNsMan);
                            foreach(XmlNode n in nodes)
                            {
                                string model = n["linn:model"].InnerText;
                                string type = n["linn:type"].InnerText;
                                string version = n["linn:version"].InnerText;
                                string family = n["linn:family"].InnerText;
                                string url = n["linn:url"].InnerText;

                                if(type == "Main")
                                {
                                    UpdateInfo updateInfo = new UpdateInfo(model, type, version, family, new Uri(url), versionType.Key);
                                    iUpdateInfoList.Add(updateInfo);
                                }
                                else if(type == "Proxy")
                                {
                                    UpdateInfo updateInfo = new UpdateInfo(model, type, version, family, versionType.Key);
                                    iUpdateInfoList.Add(updateInfo);
                                }
                            }
                        }
                    }

                    XmlNodeList variantNodes = document.SelectNodes("/rss/channel/linn:variantList/linn:variant", xmlNsMan);
                    foreach(XmlNode n in variantNodes)
                    {
                        string pcas = n["linn:pcas"].InnerText;
                        string basepcas = null;
                        if(n["linn:basepcas"] != null) {
                            basepcas = n["linn:basepcas"].InnerText;
                        }
                        string name = n["linn:name"].InnerText;

                        iVariantInfoList.Add(new VariantInfo(pcas, basepcas, name));
                    }
                
                    disposed = iDisposed; 
                }
                if(!disposed)
                {
                    if(EventVersionInfoAvailable != null)
                    {
                        EventVersionInfoAvailable(this, EventArgs.Empty);
                    }
                }

            }
            catch (Exception ex)
            {
                UserLog.WriteLine(string.Format("Error caught checking for firmware updates: {0}", ex.ToString()));
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
        }

        private object iLock;
        private bool iDisposed;

        private FirmwareCache iCache;

        private string iUpdateFeedUri;
        private uint iUpdateInterval;
        private string iApplicationDataPath;
        private EUpdateType iUpdateTypes;
        private System.Threading.Timer iTimer;

        private List<UpdateInfo> iUpdateInfoList;
        private List<VariantInfo> iVariantInfoList;
    }
}

