using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

using SneakyMedia.Database;

namespace SneakyMedia.Scanner
{
    public class ItemId : IItem
    {
        public ItemId(Mount aMount, string aRelativeUri)
        {
            iMount = aMount;
            iRelativeUri = aRelativeUri;
        }

        public string MountId
        {
            get
            {
                return (iMount.MountId);
            }
        }

        public string ItemUri
        {
            get
            {
                return (iRelativeUri);
            }
        }

        Mount iMount;
        string iRelativeUri;
    }

    public class Mount
    {
        // Construct existing mount

        public Mount(IEngine aEngine, string aId)
        {
            iEngine = aEngine;

            iMountId = aId;
            iMountUri = iEngine.QueryMountUri(iMountId);
            iLastScanned = iEngine.QueryLastScanned(iMountId);

            iMutex = new Mutex();

            iScanning = false;
        }

        // Create new mount

        public Mount(IEngine aEngine, string aMountId, string aMountUri)
        {
            iEngine = aEngine;

            iMountId = aMountId;
            iMountUri = aMountUri;

            if ((File.GetAttributes(iMountUri) & FileAttributes.Directory) == 0)
            {
                throw (new ArgumentException());
            }

            iEngine.AddMount(iMountId, iMountUri);

            iMutex = new Mutex();

            iScanning = false;
        }

        public string MountId
        {
            get
            {
                return (iMountId);
            }
        }

        public string MountUri
        {
            get
            {
                return (iMountUri);
            }
        }

        public Nullable<DateTime> LastScanned
        {
            get
            {
                return (iLastScanned);
            }
        }

        public void Scan()
        {
            iMutex.WaitOne();

            // Check if already scanning

            if (iScanning)
            {
                iMutex.ReleaseMutex();
                throw (new ApplicationException());
            }

            iScanning = true;

            iScanBegin = DateTime.Now;

            if (iLastScanned.HasValue)
            {

                iRemovedItems = new List<IItem>();

                iRemovedItems.AddRange(iEngine.QueryItems(iMountId));

                iMutex.ReleaseMutex();

                RescanDirectory(iMountUri);

                foreach (ItemId item in iRemovedItems)
                {
                    iEngine.Remove(item);
                }
            }
            else // scanning the first time
            {
                iMutex.ReleaseMutex();

                ScanDirectory(iMountUri);
            }

            iEngine.UpdateMountScanned(iMountId, iScanBegin.Value);

            iMutex.WaitOne();

            iScanning = false;

            iMutex.ReleaseMutex();
        }

        internal IList<IMetadatum> Scan(string aUri)
        {
            return (iEngine.Scan(aUri));
        }

        public string ScanUri
        {
            get
            {
                return (iScanUri);
            }
        }

        public EventHandler<EventArgs> EventScan;

        private void ScanDirectory(string aDirectory)
        {
            ScanDirectory(new DirectoryInfo(aDirectory), String.Empty);
        }

        private void ScanDirectory(DirectoryInfo aInfo, string aDirectory)
        {
            FileInfo[] files = aInfo.GetFiles();

            try
            {
                foreach (FileInfo file in files)
                {
                    iScanUri = aDirectory + file.Name;

                    if (EventScan != null)
                    {
                        EventScan(this, EventArgs.Empty);
                    }

                    ItemId itemid = new ItemId(this, iScanUri);

                    IList<IMetadatum> metadata = Scan(file.FullName);

                    iEngine.Add(itemid, metadata);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            DirectoryInfo[] dirs = aInfo.GetDirectories();

            try
            {
                foreach (DirectoryInfo dir in dirs)
                {
                    if ((dir.Attributes & FileAttributes.ReparsePoint) == 0)
                    {
                        ScanDirectory(dir, aDirectory + dir.Name + "/");
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private void RescanDirectory(string aDirectory)
        {
            RescanDirectory(new DirectoryInfo(aDirectory), String.Empty);
        }

        private void RescanDirectory(DirectoryInfo aInfo, string aDirectory)
        {
            FileInfo[] files = aInfo.GetFiles();

            try
            {
                foreach (FileInfo file in files)
                {
                    iScanUri = aDirectory + file.Name;

                    if (EventScan != null)
                    {
                        EventScan(this, EventArgs.Empty);
                    }

                    /*
                    // Remove this from the RemovedItems list

                    foreach(IItem item in iRemovedItems)
                    {
                        if (item.ItemUri == iScanUri)
                        {
                            iRemovedItems.Remove(item);
                            break;
                        }
                    }

                    if (!iLastScanned.HasValue || file.LastWriteTimeUtc > iLastScanned) // file has changed since last scan
                    {

                        Scan(file.FullName);

                        ItemId itemid = new ItemId(this, iScanUri);

                        IList<IMetadatum> oldtags;
                        IList<IMetadatum> newtags;
                        
                        try 
                        {
                            oldtags = iEngine.QueryItem(itemid);
                            newtags = iEngine.Scan(file.FullName);

                            foreach(Metadatum x in newtags)
                            {
                                foreach(Metadatum y in oldtags)
                                {
                                    if (x.Tag.Ns == y.Tag.Ns)
                                    {
                                        if (x.Tag.Name == y.Tag.Name)
                                        {
                                            IList<string> oldvalue = y.Value;
                                            IList<string> newvalue = x.Value;

                                            bool changed = false;

                                            foreach (string v in newvalue)
                                            {
                                                try
                                                {
                                                    oldvalue.Remove(v);
                                                }
                                                catch (ArgumentException)
                                                {
                                                    changed = true;
                                                }
                                            }

                                            if (!changed)
                                            {
                                                if (oldvalue.Count != 0)
                                                {
                                                    changed = true;
                                                }
                                            }

                                            if (changed)
                                            {
                                                iEngine.Remove(itemid, y);
                                                iEngine.Add(itemid, x);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        catch (ItemNotFoundError) // new item
                        {
                            iEngine.Add(itemid, Scan(file.FullName));
                        }
                    }
                    */
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            DirectoryInfo[] dirs = aInfo.GetDirectories();

            try
            {
                foreach (DirectoryInfo dir in dirs)
                {
                    if ((dir.Attributes & FileAttributes.ReparsePoint) == 0)
                    {
                        RescanDirectory(dir, aDirectory + dir.Name + "/");
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        IEngine iEngine;
        string iMountId;
        string iMountUri;
        Nullable<DateTime> iLastScanned;
        Mutex iMutex;
        Uri iAbsoluteUri;
        List<IItem> iRemovedItems;
        Nullable<DateTime> iScanBegin;
        bool iScanning;
        string iScanUri;
    }
}
