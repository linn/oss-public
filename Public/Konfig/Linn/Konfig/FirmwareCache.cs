using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;

namespace Linn.Konfig
{
    public class Firmware : IComparable<string>
    {
        public Firmware(VersionInfoReader.UpdateInfo aUpdateInfo, string aVariant)
        {
            iUpdateInfo = aUpdateInfo;
            iVariant = aVariant;
        }

        public string Model
        {
            get
            {
                return iUpdateInfo.Model;
            }
        }

        public string Variant
        {
            get
            {
                return iVariant;
            }
        }

        public string Version
        {
            get
            {
                return iUpdateInfo.Version;
            }
        }

        public Uri Uri
        {
            get
            {
                return iUpdateInfo.Uri;
            }
        }

        public Uri ReleaseNotesUri
        {
            get
            {
                return iUpdateInfo.ReleaseNotesUri;
            }
        }

        #region IComparable implementation
        public int CompareTo (string other)
        {
            return Version.CompareTo(other);
        }
        #endregion

        private VersionInfoReader.UpdateInfo iUpdateInfo;
        private string iVariant;
    }

    public class FirmwareRetriever : IDisposable
    {
        public FirmwareRetriever(Uri aUri, string aCachePath)
        {
            iUri = aUri;
            iCachePath = aCachePath;

            iDisposed = false;
            iLock = new object();
            iProgressList = new List<DDownloadProgress>();

            iEvent = new ManualResetEvent(false);

        }

        public void Dispose()
        {
            lock(iLock)
            {
                iDisposed = true;
            }

            iEvent.Set();
            iEvent.Dispose();
            iEvent = null;
        }

        public Uri Uri
        {
            get
            {
                return iUri;
            }
        }

        public delegate void DDownloadProgress(uint aPercent);
        public void WaitOne(DDownloadProgress aProgress)
        {
            lock(iLock)
            {
                iProgressList.Add(aProgress);
                aProgress(iProgress);
            }

            iEvent.WaitOne();
        }

        public bool Download(DDownloadProgress aProgress)
        {
            lock(iLock)
            {
                iProgressList.Add(aProgress);
                aProgress(0);
            }

            try
            {
                HttpWebRequest headRequest = (HttpWebRequest)WebRequest.Create(iUri);
                headRequest.Method = "HEAD";

                int fileSize = 0;
                using(HttpWebResponse headResponse = (HttpWebResponse)headRequest.GetResponse())
                {
                    fileSize = Int32.Parse(headResponse.Headers[HttpResponseHeader.ContentLength]);
                }

                string tempFilename = Path.Combine(iCachePath, Path.GetFileNameWithoutExtension(iUri.AbsolutePath) + ".part");
                string filename = Path.Combine(iCachePath, Path.GetFileName(iUri.AbsolutePath));

                int startingPoint = 0;
                if (File.Exists(tempFilename))
                {
                    startingPoint = (int)(new FileInfo(tempFilename).Length);
                }

                iProgress = (uint)((startingPoint / (float)fileSize) * 100.0f);
                lock(iLock)
                {
                    foreach(DDownloadProgress d in iProgressList)
                    {
                        d(iProgress);
                    }
                }

                lock(iLock)
                {
                    if(iDisposed)
                    {
                        return false;
                    }
                }

                if(fileSize > startingPoint)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(iUri);
                    request.Credentials = CredentialCache.DefaultCredentials;
                    if(request.Proxy != null)
                    {
                        request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    }
                    request.AddRange(startingPoint);

                    using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        using(Stream stream = response.GetResponseStream())
                        {
                            FileMode mode = FileMode.Append;
                            if(startingPoint == 0 || response.StatusCode != HttpStatusCode.PartialContent)
                            {
                                mode = FileMode.Create;
                            }

                            int total = startingPoint;
                            using(Stream file = new FileStream(tempFilename, mode, FileAccess.Write, FileShare.ReadWrite))
                            {
                                byte[] buffer = new byte[2048]; // read in 2K chuncks
                                int bytes;
                                do
                                {
                                    bytes = stream.Read(buffer, 0, buffer.Length);
                                    file.Write(buffer, 0, bytes);
                                    total += bytes;

                                    uint oldProgress = iProgress;
                                    iProgress = (uint)((total / (float)fileSize) * 100.0f);
                                    if(iProgress != oldProgress)
                                    {
                                        lock(iLock)
                                        {
                                            if(iDisposed)
                                            {
                                                return false;
                                            }

                                            foreach(DDownloadProgress d in iProgressList)
                                            {
                                                d(iProgress);
                                            }
                                        }
                                    }
                                }
                                while(bytes > 0);
                            }
                        }
                    }
                }

                File.Move(tempFilename, filename);

                return true;
            }
            catch(Exception e)
            {
                UserLog.WriteLine("Failed to download " + iUri + ":" + e);
            }

            return false;
        }

        private bool iDisposed;
        private object iLock;
        private uint iProgress;
        private List<DDownloadProgress> iProgressList;

        private Uri iUri;
        private string iCachePath;

        private ManualResetEvent iEvent;
    }

    public interface IFirmwareManager
    {
        void SetUpdating(bool aUpdating);
        string XmlFilename(Firmware aFirmware, FirmwareRetriever.DDownloadProgress aProgress);
    }

    public class FirmwareCache : IFirmwareManager, IDisposable
    {
        public FirmwareCache (string aApplicationDataPath)
        {
            iLock = new object();

            iUpdatingCount = 0;
            iEventUpdating = new ManualResetEvent(true);
            iEventCacheClear = new ManualResetEvent(true);

            iCachePath = Path.Combine(aApplicationDataPath, "Cache");
            if(!Directory.Exists(iCachePath))
            {
                Directory.CreateDirectory(iCachePath);
            }
            iCachePathTemp = Path.Combine(iCachePath, "Temp");

            iRetrieverList = new List<FirmwareRetriever>();
        }

        public void Dispose()
        {
            iEventUpdating.WaitOne();

            lock(iLock)
            {
                foreach(FirmwareRetriever r in iRetrieverList)
                {
                    r.Dispose();
                }

                iRetrieverList.Clear();
            }

            if(Directory.Exists(iCachePathTemp))
            {
                string[] fileList = Directory.GetFiles(iCachePathTemp, "*");
                string[] dirList = Directory.GetDirectories(iCachePathTemp, "*");
                foreach (string file in fileList)
                {
                    File.Delete(file);
                }
                foreach (string dir in dirList)
                {
                    Directory.Delete(dir, true); // true is for delete recursively
                }
            }
        }

        public void Clear()
        {
            iEventCacheClear.Reset();
            iEventUpdating.WaitOne();

            try
            {
                string[] fileList = Directory.GetFiles(iCachePath, "*");
                foreach (string file in fileList)
                {
                    File.Delete(file);
                }

                string[] dirList = Directory.GetDirectories(iCachePath, "*");

                foreach (string dir in dirList) 
                {
                    Directory.Delete(dir, true); // true is for delete recursively
                }
            }
            catch (Exception) { }

            iEventCacheClear.Set();
        }

        /*public FirmwareRetriever Cache(Firmware aFirmware)
        {
            lock(iLock)
            {
                foreach(FirmwareRetriever r in iRetrieverList)
                {
                    if(r.Uri == aFirmware.Uri)
                    {
                        return r;
                    }
                }

                FirmwareRetriever retriever = new FirmwareRetriever(aFirmware.Uri, iCachePath);
                iRetrieverList.Add(retriever);

                return retriever;
            }
        }*/

        public void SetUpdating(bool aUpdating)
        {
            lock(iLock)
            {
                if(aUpdating)
                {
                    ++iUpdatingCount;
                    iEventUpdating.Reset();
                }
                else
                {
                    --iUpdatingCount;
                    if(iUpdatingCount == 0)
                    {
                        iEventUpdating.Set();
                    }
                }
            }
        }

        public string XmlFilename(Firmware aFirmware, FirmwareRetriever.DDownloadProgress aProgress)
        {
            iEventCacheClear.WaitOne();

            bool cached = false;
            bool found = false;
            FirmwareRetriever retriever = null;
            lock(iLock)
            {
                if(IsCached(aFirmware))
                {
                    cached = true;
                    aProgress(100);
                }

                if(!cached)
                {
                    foreach(FirmwareRetriever r in iRetrieverList)
                    {
                        if(r.Uri == aFirmware.Uri)
                        {
                            retriever = r;
                            found = true;
                            break;
                        }
                    }

                    if(!found)
                    {
                        retriever = new FirmwareRetriever(aFirmware.Uri, iCachePath);
                        iRetrieverList.Add(retriever);
                    }
                }
            }

            if(!cached)
            {
                if(found)
                {
                    retriever.WaitOne(aProgress);
                }
                else
                {
                    bool downloaded = retriever.Download(aProgress);

                    lock(iLock)
                    {
                        if(iRetrieverList.Remove(retriever))
                        {
                            retriever.Dispose();
                        }
                    }

                    if(!downloaded)
                    {
                        return null;
                    }
                }
            }

            lock (iLock)
            {
                string filename = FlashSupport.GetRomFilename(iCachePathTemp, Path.Combine(iCachePath, Path.GetFileName(aFirmware.Uri.AbsolutePath)), aFirmware.Variant);
                if (string.IsNullOrEmpty(filename))
                {
                    File.Delete(Path.Combine(iCachePath, Path.GetFileName(aFirmware.Uri.AbsolutePath)));
                }
                return filename;
            }
        }

        private bool IsCached(Firmware aFirmware)
        {
            return File.Exists(Path.Combine(iCachePath, Path.GetFileName(aFirmware.Uri.AbsolutePath)));
        }

        private object iLock;
        private string iCachePath;
        private string iCachePathTemp;
        private List<FirmwareRetriever> iRetrieverList;

        private uint iUpdatingCount;
        private ManualResetEvent iEventUpdating;
        private ManualResetEvent iEventCacheClear;
    }
}

