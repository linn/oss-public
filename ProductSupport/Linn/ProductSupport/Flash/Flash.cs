using System;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;
using Linn.Xml;

namespace Linn.ProductSupport.Flash
{
    public interface IConsole
    {
        void Newline();
        void Title(string aMessage);
        void Write(string aMessage);
        void ProgressOpen(int aMax);
        void ProgressSetValue(int aValue);
        void ProgressClose();
    }

    public class FlashException : Exception
    {
        public FlashException(string aMessage)
            : base("FAILED " + aMessage)
        {
        }

        public FlashException()
            : base("FAILED")
        {
        }
    }

    public class FactoryDefaulter
    {
        public FactoryDefaulter(IPAddress aInterface, IConsole aConsole, string aUglyName)
        {
            iInterface = aInterface;
            iConsole = aConsole;
            iUglyName = aUglyName;
            iFallbackFinder = new Finder(iInterface, iConsole, aUglyName);
            iNoExec = false;
            iWait = false;
        }

        public bool Execute() {
            string temp = null;
            return Execute(out temp);
        }

        public bool Execute(out string aFailMessage)
        {
            try
            {
                DoExecute();

                iConsole.Newline();

                iConsole.Write(iUglyName + " set to factory defaults successfully");
                iConsole.Newline();
                aFailMessage = "";
                return (true);
            }
            catch (FlashException e)
            {
                iConsole.Write(e.Message);
                iConsole.Newline();
                aFailMessage = e.Message;
                return (false);
            }
        }

        private void ReportTitle(string aTitle)
        {
            iConsole.Title(aTitle);
        }

        private void ReportSuccess()
        {
            iConsole.Write("ok");
            iConsole.Newline();
        }

        private void ReportYes()
        {
            iConsole.Write("yes");
            iConsole.Newline();
        }

        private void ReportNo()
        {
            iConsole.Write("no");
            iConsole.Newline();
        }

        private void ReportSkipped()
        {
            iConsole.Write("skipped");
            iConsole.Newline();
        }

        private void DoExecute()
        {
            ReportTitle("Reboot to fallback");

            if (!iFallbackFinder.EstablishFallback())
            {
                throw (new FlashException());
            }

            iServiceFlash = new ServiceFlash(iFallbackFinder.Device);

            iFallbackHandler = new Fallback(iConsole, iServiceFlash);

            CollectRomDirMain();
            CollectRwStore();
            DefaultRwStore();
            WriteRwStore();

            if (!iNoExec)
            {
                ReportTitle("Reboot to main");

                if (!iFallbackFinder.SetBootMode("Main"))
                {
                    throw (new FlashException());
                }

                if (!iFallbackFinder.IssueReboot())
                {
                    throw (new FlashException());
                }

                if (iWait)
                {
                    if (!iFallbackFinder.WaitForOffOn())
                    {
                        ReportSkipped();
                    }

                    if (!iFallbackFinder.FindDevice())
                    {
                        throw (new FlashException());
                    }
                }
            }
        }

        private void CollectRomDirMain()
        {
            RomDirInfo info = new RomDirInfo(iConsole, iServiceFlash);

            ReportTitle("Collect main rom directory");

            iConsole.Write("Collecting rom directory info ...... ");

            info.ReadRomDirInfo();

            iConsole.Write("Finding flash device ............... ");

            IFlash flash = iFallbackHandler.Find(info.RomDirMainFlashId);

            iConsole.Write("Reading rom directory .............. ");

            byte[] data = iFallbackHandler.Read(flash, info.RomDirMainOffset, info.RomDirMainBytes);

            iConsole.Write("Sorting rom directory .............. ");

            iOnDeviceRomDir = RomDir.Create(data, 0);

            if (iOnDeviceRomDir == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();
        }

        private void CollectRwStore()
        {
            ReportTitle("Collect read/write store");

            iConsole.Write("Finding rom directory entry ........ ");

            iOnDeviceRwStoreEntry = iOnDeviceRomDir.Find(Linn.Tags.uStoreReadWrite);

            if (iOnDeviceRwStoreEntry == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Finding flash device ............... ");

            iOnDeviceRwStoreFlash = iFallbackHandler.Find(iOnDeviceRwStoreEntry.FlashId);

            iConsole.Write("Reading read/write store ........... ");

            byte[] data = iFallbackHandler.Read(iOnDeviceRwStoreFlash, iOnDeviceRwStoreEntry.Offset, iOnDeviceRwStoreEntry.Bytes);

            iConsole.Write("Sorting read/write store entries ... ");

            iOnDeviceRwStore = Store.CreateRwStore(data, iOnDeviceRwStoreFlash.SectorBytes);

            if (iOnDeviceRwStore == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            return;
        }

        private void DefaultRwStore()
        {
            foreach (RomDirEntry e in iOnDeviceRomDir.EntryList)
            {
                if (e.Type == Linn.Tags.uStoreReadWriteDefaults)
                {
                    ReportTitle("Setting default entries for store " + e.Key);

                    iConsole.Write("Finding flash device ............... ");

                    IFlash flash = iFallbackHandler.Find(e.FlashId);

                    iConsole.Write("Reading default store .............. ");

                    byte[] data = iFallbackHandler.Read(flash, e.Offset, e.Bytes);

                    iConsole.Write("Sorting default store .............. ");

                    Store store = Store.CreateRoStore(data);

                    if (store == null)
                    {
                        throw (new FlashException());
                    }

                    ReportSuccess();

                    iConsole.Write("Applying default store ............. ");

                    foreach (StoreEntry s in store.StoreEntryList)
                    {
                        StoreEntry entry = iOnDeviceRwStore.Find(s.Key);

                        if (entry == null)
                        {
                            throw (new FlashException());
                        }

                        entry.Data = s.Data;
                    }

                    ReportSuccess();
                }
            }
        }

        private void WriteRwStore()
        {
            ReportTitle("Reprogram StoreReadWrite");

            iConsole.Write("Erasing sectors .................... ");

            uint storeoffset = iOnDeviceRwStoreEntry.Offset;
            uint storebytes = iOnDeviceRwStoreEntry.Bytes;

            iFallbackHandler.Erase(iOnDeviceRwStoreFlash, storeoffset, storebytes);

            uint offset = 0;

            uint sectors = storebytes / iOnDeviceRwStoreFlash.SectorBytes;

            for (int i = 0; i < sectors; i++)
            {
                string num = (i + 1).ToString();
                string dots = new string('.', 17 - num.Length);
                iConsole.Write("Formatting sector " + num + " " + dots + " ");
                iFallbackHandler.Write(iOnDeviceRwStoreFlash, storeoffset + offset, BigEndian.Bytes(0xcccccccc));
                offset += iOnDeviceRwStoreFlash.SectorBytes;
            }

            iConsole.Write("Creating image ..................... ");

            byte[] data = iOnDeviceRwStore.CreateRwStore(iOnDeviceRwStoreFlash.SectorBytes);

            if (data.Length > storebytes)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Writing image ...................... ");

            iFallbackHandler.Write(iOnDeviceRwStoreFlash, storeoffset, data);
        }

        public bool NoExec
        {
            get
            {
                return (iNoExec);
            }
            set
            {
                iNoExec = value;
            }
        }

        public bool Wait
        {
            get
            {
                return (iWait);
            }
            set
            {
                iWait = value;
            }
        }

        public void Close()
        {
            iFallbackFinder.Close();
        }

        private string iUglyName;
        private IPAddress iInterface;
        private IConsole iConsole;
        private Finder iFallbackFinder;
        private ServiceFlash iServiceFlash;
        private Fallback iFallbackHandler;

        private RomDir iOnDeviceRomDir;
        private Store iOnDeviceRwStore;
        private RomDirEntry iOnDeviceRwStoreEntry;
        private IFlash iOnDeviceRwStoreFlash;

        private bool iNoExec;
        private bool iWait;
    }

    public class Reprogrammer
    {
        public Reprogrammer(IPAddress aInterface, IConsole aConsole, string aUglyName, string aXmlFile)
        {
            iInterface = aInterface;
            iConsole = aConsole;
            iUglyName = aUglyName;
            iXmlFile = aXmlFile;
            iCrc32 = new Crc32();
            iDoFallback = false;
            iDoWriteFallbackFpga = true;
            iDoNoExec = false;
            iDoWait = false;
            iDoNoTrust = false;
            iPhase = "UNKNOWN";
            iUnsafe = false;
        }

        public bool Execute() {
            string temp = null;
            return Execute(out temp);
        }

        public bool Execute(out string aFailMessage) {
            try {
                DoExecute();

                iConsole.Newline();

                iConsole.Write(iUglyName + " reprogrammed successfully");
                iConsole.Newline();
                aFailMessage = "";
                return (true);
            }
            catch (FlashException e) {
                iConsole.Write(e.Message);
                iConsole.Newline();
                aFailMessage = e.Message;
                return (false);
            }
        }

        private void ReportTitle(string aTitle)
        {
            iConsole.Title("[" + iPhase + "] " + aTitle);
        }

        private void ReportSuccess()
        {
            iConsole.Write("ok");
            iConsole.Newline();
        }

        private void ReportYes()
        {
            iConsole.Write("yes");
            iConsole.Newline();
        }

        private void ReportNo()
        {
            iConsole.Write("no");
            iConsole.Newline();
        }

        private void ReportSkipped()
        {
            iConsole.Write("skipped");
            iConsole.Newline();
        }

        private void DoExecute()
        {
            iPhase = "INIT";

            AnalyseXml();

            if (iIsCollection)
            {
                ReprogCollection();
            }
            else
            {
                ValidateReprogFiles();

                ValidateLayout();

                ValidateRom();

                if (Fallback)
                {
                    iConsole.Write("Checking for valid fallback file ... ");

                    if (!iRom.Fallback)
                    {
                        throw (new FlashException());
                    }

                    ReportSuccess();

                    ReprogFallback();
                }
                else
                {
                    if (iRom.Fallback)
                    {
                        ReprogMainFallback();
                    }
                    else
                    {
                        ReprogMain();
                    }
                }
            }
        }

        private void AnalyseXml()
        {
            ReportTitle("Analyse xml file");

            iConsole.Write("Validating uri ..................... ");

            try
            {
                Uri current = new Uri(System.Environment.CurrentDirectory + "/");
                iXmlUri = new Uri(current, iXmlFile);
            }
            catch (UriFormatException)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Checking for rom collection file ... ");

            string path = Uri.UnescapeDataString(iXmlUri.AbsolutePath);

            try
            {
                iCollection = RomCollection.Load(path);

                ReportYes();

                iCollectionUri = iXmlUri;

                iIsCollection = true;
            }
            catch (Exception)
            {
                ReportNo();

                iConsole.Write("Checking for rom file .............. ");

                try
                {
                    iRom = Rom.Load(path);

                    ReportYes();

                    iIsCollection = false;
                }
                catch (Exception)
                {
                    throw (new FlashException("Unable to load " + path));
                }
            }
        }

        private void ValidateReprogFiles()
        {
            ReportTitle("Validate reprogramming files");

            iConsole.Write("Locating install root .............. ");

            try
            {
                iRomInstallRoot = new Uri(iXmlUri, iRom.InstallRoot.Substring(1)); // get rid of initial '/'

                ReportSuccess();
            }
            catch (UriFormatException)
            {
                throw (new FlashException());
            }

            iConsole.Write("Loading tags file .................. ");

            try
            {
                Uri uri = new Uri(iRomInstallRoot, iRom.Tags.Substring(1)); // get rid of initial '/'
                iTagsUri = Uri.UnescapeDataString(uri.AbsolutePath);

            }
            catch (UriFormatException)
            {
                throw (new FlashException());
            }


            try
            {
                iTags = Linn.Xml.Tags.Load(iTagsUri);

                ReportSuccess();
            }
            catch (Exception)
            {
                throw (new FlashException("Unable to load " + iTagsUri));
            }

            iConsole.Write("Loading layout file ................ ");

            try
            {
                Uri uri = new Uri(iRomInstallRoot, iRom.Layout.Substring(1)); // get rid of initial '/'
                iLayoutUri = Uri.UnescapeDataString(uri.AbsolutePath);
            }
            catch (UriFormatException)
            {
                throw (new FlashException());
            }

            try
            {
                iLayout = Layout.Load(iLayoutUri);

                ReportSuccess();
            }
            catch (Exception)
            {
                throw (new FlashException("Unable to load " + iLayoutUri));
            }
        }

        private void LoadRomFile()
        {
            ReportTitle("Load rom file");

            iConsole.Write("Validating uri ..................... ");

            try
            {
                iXmlUri = new Uri(iCollectionUri, iXmlFile);

                ReportSuccess();
            }
            catch (UriFormatException)
            {
                throw (new FlashException());
            }

            string path = Uri.UnescapeDataString(iXmlUri.AbsolutePath);

            iConsole.Write("Loading rom file ................... ");

            try
            {
                iRom = Rom.Load(path);

                ReportSuccess();
            }
            catch (Exception)
            {
                throw (new FlashException("Unable to load " + path));
            }
        }

        private void ReprogCollection()
        {
            ReportTitle("Checking emulation mode");

            iConsole.Write("Checking emulator not specified .... ");

            if (!String.IsNullOrEmpty(iEmulator))
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Checking output not specified ...... ");

            if (!String.IsNullOrEmpty(iOutput))
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iPhase = "FIND";

            iFinder = new Finder(iInterface, iConsole, iUglyName);

            ReportTitle("Find fallback application");

            if (!iFinder.FindDevice())
            {
                throw (new FlashException());
            }

            iConsole.Write("Checking device in fallback mode ... ");

            if (iFinder.DeviceIsInFallback)
            {
                ReportYes();
            }
            else
            {
                ReportNo();

                iConsole.Write("Checking device running fallback ... ");

                if (iFinder.CheckRunningFallback())
                {
                    ReportYes();
                }
                else
                {
                    ReportNo();

                    RebootFallback();
                }
            }

            iServiceFlash = new ServiceFlash(iFinder.Device);
            iRomDirInfo = new RomDirInfo(iConsole, iServiceFlash);
            iFallback = new Fallback(iConsole, iServiceFlash);

            if (CheckIdenticalFallback())
            {
                if (!iFinder.DeviceIsInFallback)
                {
                    RebootFallback();
                }

                iPhase = "M->M";

                ReportTitle("Reprogram main");

                iXmlFile = iCollection.Main;

                LoadRomFile();

                ValidateReprogFiles();

                ValidateLayout();

                ValidateRom();

                ReprogRomMain();

                if (!iDoNoExec)
                {
                    IssueRebootToMain();
                }

                return;
            }

            if (iFinder.DeviceIsInFallback)
            {
                iPhase = "F->M";

                iXmlFile = iCollection.Fallback;

                LoadRomFile();

                ValidateReprogFiles();

                ValidateLayout();

                ValidateRom();

                ReprogRomMainFallback();

                RebootMain();
            }

            iPhase = "F->F";

            iXmlFile = iCollection.Fallback;

            LoadRomFile();

            ValidateReprogFiles();

            ValidateLayout();

            ValidateRom();

            ReprogRomFallback();

            RebootFallback();

            iPhase = "M->M";

            iXmlFile = iCollection.Main;

            LoadRomFile();

            ValidateReprogFiles();

            ValidateLayout();

            ValidateRom();

            ReprogRomMain();

            if (!iDoNoExec)
            {
                IssueRebootToMain();
            }
        }

        private bool CheckIdenticalFallback()
        {
            iXmlFile = iCollection.Fallback;

            LoadRomFile();

            ValidateReprogFiles();

            ValidateLayout();

            ValidateRom();

            ReportTitle("Check fallback reprogramming necessary");

            CollectRomDirFallback();

            if (iDoWriteFallbackFpga)
            {
                if (!CheckIdenticalBinary("FallbackFpga"))
                {
                    return (false);
                }
            }

            if (!CheckIdenticalBinary("FallbackApp"))
            {
                return (false);
            }

            CreateRoStores();

            if (!CheckIdenticalRoStores("StoreReadOnlyFallback"))
            {
                return (false);
            }

            if (iDoBootstrap)
            {
                if (!CheckIdenticalBinary("Bootstrap"))
                {
                    return (false);
                }
            }

            return (true);
        }

        private void RebootMain()
        {
            ReportTitle("Reboot to main");

            if (!iFinder.EstablishMain())
            {
                throw (new FlashException());
            }

            iServiceFlash = new ServiceFlash(iFinder.Device);
            iRomDirInfo = new RomDirInfo(iConsole, iServiceFlash);
            iFallback = new Fallback(iConsole, iServiceFlash);
        }

        private void RebootFallback()
        {
            ReportTitle("Reboot to fallback");

            if (!iFinder.EstablishFallback())
            {
                throw (new FlashException());
            }

            iServiceFlash = new ServiceFlash(iFinder.Device);
            iRomDirInfo = new RomDirInfo(iConsole, iServiceFlash);
            iFallback = new Fallback(iConsole, iServiceFlash);
        }

        private void IssueRebootToMain()
        {
            ReportTitle("Reboot to main");

            if (!iFinder.SetBootMode("Main"))
            {
                throw (new FlashException());
            }

            if (!iFinder.IssueReboot())
            {
                throw (new FlashException());
            }

            if (iDoWait)
            {
                if (!iFinder.WaitForOffOn())
                {
                    ReportSkipped();
                }

                if (!iFinder.FindDevice())
                {
                    throw (new FlashException());
                }
            }
        }

        private void IssueRebootToFallback()
        {
            ReportTitle("Reboot to fallback");

            if (!iFinder.SetBootMode("Fallback"))
            {
                throw (new FlashException());
            }

            if (!iFinder.IssueReboot())
            {
                throw (new FlashException());
            }

            if (iDoWait)
            {
                if (!iFinder.WaitForOffOn())
                {
                    ReportSkipped();
                }

                if (!iFinder.FindDevice())
                {
                    throw (new FlashException());
                }
            }
        }

        private bool AnalyseEmulator()
        {
            ReportTitle("Checking emulation mode");

            iConsole.Write("Checking if emulator specified ... ");

            if (String.IsNullOrEmpty(iEmulator))
            {
                ReportNo();
                return (false);
            }

            ReportYes();

            iConsole.Write("Checking output file specified ... ");

            if (String.IsNullOrEmpty(iOutput))
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Creating emulator ... ");

            iFallback = CreateEmulator(iEmulator, iOutput);

            ReportSuccess();

            iDoNoTrust = true;
            iDoNoExec = true;
            iDoWait = false;

            return (true);
        }

        IFallback CreateEmulator(string aEmulator, string aOutput)
        {
            if (aEmulator == "SrecA")
            {
                return (new FallbackEmulatorSrecA(iConsole, aOutput));
            }

            throw (new FlashException());
        }

        void ReprogRomMain()
        {
            iRomDir = new RomDir();

            if (!iDoNoTrust)
            {
                CollectRomDirMain();
                CollectRwStore();
            }
            else
            {
                CreateEmptyRwStore();
            }

            CreateEraseCountStore();

            MergeRwStore();

            CreateRoStores();

            if (!NoTrust)
            {
                ClearRomDir("MainRomDir");
            }

            WriteBinary("MainFpga", "MainFpga");
            WriteBinary("MainApp", "MainApp");
            WriteCrash("MainCrash");
            WriteRoStores("StoreReadOnly");
            WriteRwStore();

            // Add additional binary regions. Location of the fpga bitstream
            // region in the romdir is fixed (core cpld is hardcoded). This
            // ordering appears to be enforced in _this_ function, so we can't
            // really do _all_ binary regions this way.

            foreach (Rom.IBinaryRegion b in iRom.BinaryRegionList)
            {
                // Trim "u" from start of tag.

                string tag = b.Tag.Substring(1);

                // If we haven't written the binaryregion yet (ie above), then add.

                if ( tag != "MainFpga" && tag != "MainApp" )
                {
                    WriteBinary(tag, tag);
                }
            }

            WriteRomDir("MainRomDir");

            iFallback.Close();
        }

        void ReprogRomMainFallback()
        {
            iRomDir = new RomDir();

            if (!iDoNoTrust)
            {
                CollectRomDirMain();
                CollectRwStore();
            }
            else
            {
                CreateEmptyRwStore();
            }

            CreateRoStores();

            if (!iDoNoTrust)
            {
                ClearRomDir("MainRomDir");
            }

            if (iDoWriteFallbackFpga)
            {
                WriteBinary("FallbackFpga", "MainFpga");
            }
            WriteBinary("FallbackApp", "MainApp");
            WriteCrash("MainCrash");
            WriteRoStores("StoreReadOnly");

            iRwStore = iOnDeviceRwStore;

            WriteRwStore();
            WriteRomDir("MainRomDir");

            iFallback.Close();
        }

        void ReprogRomFallback()
        {
            iRomDir = new RomDir();

            if (!iDoNoTrust)
            {
                CollectRomDirFallback();
            }

            CreateRoStores();

            if (!NoTrust)
            {
                ClearRomDir("FallbackRomDir");
            }

            if (iDoWriteFallbackFpga)
            {
                WriteBinary("FallbackFpga", "FallbackFpga");
            }
            WriteBinary("FallbackApp", "FallbackApp");
            WriteCrash("FallbackCrash");
            WriteRoStores("StoreReadOnlyFallback");

            if (iDoBootstrap)
            {
                WriteBinary("Bootstrap", "Bootstrap");
            }
            else
            {
                AddRomDirEntry("Bootstrap");
            }

            WriteRomDir("FallbackRomDir");

            iFallback.Close();
        }

        private void ReprogMain()
        {
            iPhase = "M->M";

            if (!AnalyseEmulator())
            {
                iFinder = new Finder(iInterface, iConsole, iUglyName);

                RebootFallback();
            }

            ReprogRomMain();

            if (!iDoNoExec)
            {
                IssueRebootToMain();
            }
        }

        private void ReprogMainFallback()
        {
            iPhase = "F->M";

            if (!AnalyseEmulator())
            {
                iFinder = new Finder(iInterface, iConsole, iUglyName);

                RebootFallback();
            }

            ReprogRomMainFallback();

            if (!iDoNoExec)
            {
                IssueRebootToMain();
            }
        }

        private void ReprogFallback()
        {
            iPhase = "F->F";

            if (!AnalyseEmulator())
            {
                iFinder = new Finder(iInterface, iConsole, iUglyName);

                if ( iUnsafe )
                    RebootFallback();
                else
                    RebootMain();
            }

            ReprogRomFallback();

            if (!iDoNoExec)
            {
                IssueRebootToFallback();
            }
        }

        private void CollectRomDirMain()
        {
            ReportTitle("Collect main rom directory");

            iConsole.Write("Collecting rom directory info ...... ");

            iRomDirInfo.ReadRomDirInfo();

            iConsole.Write("Finding flash device ............... ");

            IFlash flash = iFallback.Find(iRomDirInfo.RomDirMainFlashId);

            iConsole.Write("Reading rom directory .............. ");

            byte[] data = iFallback.Read(flash, iRomDirInfo.RomDirMainOffset, iRomDirInfo.RomDirMainBytes);

            iConsole.Write("Sorting rom directory .............. ");

            iOnDeviceRomDir = RomDir.Create(data, 0);

            if (iOnDeviceRomDir == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();
        }


        private void CollectRomDirFallback()
        {
            ReportTitle("Collect fallback rom directory");

            iConsole.Write("Collecting rom directory info ...... ");

            iRomDirInfo.ReadRomDirInfo();

            iConsole.Write("Finding flash device ............... ");

            IFlash flash = iFallback.Find(iRomDirInfo.RomDirFallbackFlashId);

            iConsole.Write("Reading rom directory .............. ");

            byte[] data = iFallback.Read(flash, iRomDirInfo.RomDirFallbackOffset, iRomDirInfo.RomDirFallbackBytes);

            iConsole.Write("Sorting rom directory .............. ");

            iOnDeviceRomDir = RomDir.Create(data, 0);

            if (iOnDeviceRomDir == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();
        }

        private void CollectRwStore()
        {
            ReportTitle("Collect read/write store");

            iConsole.Write("Finding rom directory entry ........ ");

            uint key = iTags.Find("uStoreReadWrite").Key;

            RomDirEntry entry = iOnDeviceRomDir.Find(key);

            if (entry != null)
            {
                ReportSuccess();

                iConsole.Write("Finding flash device ............... ");

                IFlash flash = iFallback.Find(entry.FlashId);

                iConsole.Write("Reading read/write store ........... ");

                byte[] data = iFallback.Read(flash, entry.Offset, entry.Bytes);

                iConsole.Write("Sorting read/write store entries ... ");

                iOnDeviceRwStore = Store.CreateRwStore(data, flash.SectorBytes);

                if (iOnDeviceRwStore == null)
                {
                    throw (new FlashException());
                }

                ReportSuccess();

                return;
            }
            else
            {
                ReportNo();
            }
        }

        private void CreateEmptyRwStore()
        {
            ReportTitle("Create read/write store");

            iConsole.Write("Creating empty read/write store .... ");

            iOnDeviceRwStore = new Store();

            ReportSuccess();
        }

        private void CreateEraseCountStore()
        {
            ReportTitle("Prepare sector erase counters");

            iConsole.Write("Enumerating flash devices .......... ");

            List<string> flashtags = new List<string>();

            foreach (Layout.ISection s in iLayout.SectionList)
            {
                if (!flashtags.Contains(s.Flash))
                {
                    flashtags.Add(s.Flash);
                }
            }

            ReportSuccess();

            iEraseCountStore = new Store();

            foreach (string f in flashtags)
            {
                String dots = new String('.', 27 - f.Length);

                iConsole.Write("Finding " + f + " " + dots + " ");

                uint id = iTags.Find(f).Key;

                IFlash flash = iFallback.Find(id);

                iConsole.Write("Preparing erase counters ........... ");

                for (uint i = 0; i < flash.SectorCount; i++)
                {
                    string tag = f + "EraseCount" + i;

                    Linn.Xml.Tags.ITagEntry entry = iTags.Find(tag);

                    if (entry == null)
                    {
                        throw (new FlashException());
                    }

                    iEraseCountStore.Add(StoreEntry.Create(entry.Key, 0));
                }

                ReportSuccess();
            }
        }

        private void MergeRwStore()
        {
            ReportTitle("Merge new read/write store");

            iConsole.Write("Merging store entries .............. ");

            int max = 0;

            foreach (Rom.IStoreRegion s in iRom.StoreRegionList)
            {
                if (iTags.Find(s.Type).Key == Linn.Tags.uStoreReadWriteDefaults)
                {
                    max += s.EntryList.Count;
                }
            }

            max += iEraseCountStore.StoreEntryList.Count;

            iConsole.ProgressOpen(max);

            iRwStore = new Store();

            int count = 0;

            // add erase count entries

            foreach (StoreEntry e in iEraseCountStore.StoreEntryList)
            {
                StoreEntry entry = iOnDeviceRwStore.Find(e.Key);

                if (entry != null)
                {
                    // found in on-device store
                    iRwStore.Add(entry);
                }
                else
                {
                    // not found in on-device store
                    iRwStore.Add(e);
                }

                iConsole.ProgressSetValue(++count);
            }

            // add other entries

            foreach (Rom.IStoreRegion s in iRom.StoreRegionList)
            {
                if (iTags.Find(s.Type).Key == Linn.Tags.uStoreReadWriteDefaults)
                {
                    foreach (Rom.IEntry e in s.EntryList)
                    {
                        Linn.Xml.Tags.ITagEntry t = iTags.Find(e.Tag);

                        StoreEntry entry = iOnDeviceRwStore.Find(t.Key);

                        if (entry != null)
                        {
                            // found in on-device store
                            iRwStore.Add(entry);
                        }
                        else
                        {
                            // not found in on-device store

                            // migration policy start

                            // 1. use old preamp source names for the first hundred product source names

                            if(t.Key >= Linn.Tags.uProductSourceName0 && t.Key < Linn.Tags.uProductSourceName0 + 100)
                            {
                                StoreEntry old = iOnDeviceRwStore.Find(t.Key - Linn.Tags.uProductSourceName0 + Linn.Tags.uPreampSourceName0);

                                if (old != null)
                                {
                                    iRwStore.Add(new StoreEntry(t.Key, old.Data));
                                    iConsole.ProgressSetValue(++count);
                                    continue;
                                }

                            }

                            // 2. use old preamp source visibles for the first hundred product source visibles

                            if(t.Key >= Linn.Tags.uProductSourceVisible0 && t.Key < Linn.Tags.uProductSourceVisible0 + 100)
                            {
                                StoreEntry old = iOnDeviceRwStore.Find(t.Key - Linn.Tags.uProductSourceVisible0 + Linn.Tags.uPreampSourceVisible0);

                                if (old != null)
                                {
                                    iRwStore.Add(new StoreEntry(t.Key, old.Data));
                                    iConsole.ProgressSetValue(++count);
                                    continue;
                                }
                            }

                            // 3. use old product specific VolumeControlEnabled value for new generic DigitalVolumeControlEnabled entry

                            if(t.Key == Linn.Tags.uDigitalVolumeControlEnabled)
                            {
                                StoreEntry old;

                                // check for existance KlimaxDs volume control store entry
                                old = iOnDeviceRwStore.Find(Linn.Tags.uKlimaxDsVolumeControlEnabled);

                                if (old == null) // not a KlimaxDs - check for AkurateDs
                                {
                                    old = iOnDeviceRwStore.Find(Linn.Tags.uAkurateDsVolumeControlEnabled);
                                }
                                if (old == null) // not an AkurateDs - check for MajikDs
                                {
                                    old = iOnDeviceRwStore.Find(Linn.Tags.uMajikDsVolumeControlEnabled);
                                }
                                if (old == null) // not a MajikDs - check for SneakyMusicDs
                                {
                                    old = iOnDeviceRwStore.Find(Linn.Tags.uVolumeControlEnabled);
                                }

                                if (old != null) // found one of the above
                                {
                                    iRwStore.Add(new StoreEntry(t.Key, old.Data));
                                    iConsole.ProgressSetValue(++count);
                                    continue;
                                }
                            }

                            // 4. use old Digital Audio Output Raw value for new Digital Audio Output Mode entry

                            if(t.Key == Linn.Tags.uDigitalAudioOutputMode)
                            {
                                StoreEntry old = iOnDeviceRwStore.Find(Linn.Tags.uDigitalAudioOutputRaw);

                                if (old != null)
                                {
                                    iRwStore.Add(new StoreEntry(t.Key, old.Data));
                                    iConsole.ProgressSetValue(++count);
                                    continue;
                                }
                            }


                            // migration policy end

                            if (t.Type == Linn.Xml.Tags.ETagEntryType.eSigned)
                            {
                                try
                                {
                                    entry = StoreEntry.Create(t.Key, Convert.ToInt32(e.Value));
                                    iRwStore.Add(entry);
                                }
                                catch (FormatException)
                                {
                                    throw (new FlashException("Invalid tag " + e.Tag));
                                }
                                catch (OverflowException)
                                {
                                    throw (new FlashException("Invalid tag " + e.Tag));
                                }
                            }
                            else if (t.Type == Linn.Xml.Tags.ETagEntryType.eUnsigned)
                            {
                                try
                                {
                                    entry = StoreEntry.Create(t.Key, Convert.ToUInt32(e.Value));
                                    iRwStore.Add(entry);
                                }
                                catch (FormatException)
                                {
                                    throw (new FlashException("Invalid tag " + e.Tag));
                                }
                                catch (OverflowException)
                                {
                                    throw (new FlashException("Invalid tag " + e.Tag));
                                }
                            }
                            else if (t.Type == Linn.Xml.Tags.ETagEntryType.eString)
                            {
                                entry = StoreEntry.Create(t.Key, e.Value);
                                iRwStore.Add(entry);
                            }
                            else if (t.Type == Linn.Xml.Tags.ETagEntryType.eBinary)
                            {
                                if (e.Source != "file")
                                {
                                    throw (new FlashException("Invalid tag " + e.Tag));
                                }

                                Uri uri = new Uri(iRomInstallRoot, e.Value.Substring(1)); // get rid of initial '/'
                                string path = Uri.UnescapeDataString(uri.AbsolutePath);
                                FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                                entry = StoreEntry.Create(t.Key, file);
                                iRwStore.Add(entry);
                            }
                        }
                    }

                    iConsole.ProgressSetValue(++count);
                }
            }

            iConsole.ProgressClose();

            ReportSuccess();
        }

        public class StoreRo : Store
        {
            public StoreRo(uint aKey, uint aType)
            {
                iKey = aKey;
                iType = aType;
            }

            public uint Key
            {
                get
                {
                    return (iKey);
                }
            }

            public uint Type
            {
                get
                {
                    return (iType);
                }
            }

            private uint iKey;
            private uint iType;
        }

        private void CreateRoStores()
        {
            ReportTitle("Create read only stores");

            iConsole.Write("Collecting read only stores ........ ");

            int max = 0;

            uint keyro = iTags.Find("uStoreReadOnly").Key;
            uint keyrwd = iTags.Find("uStoreReadWriteDefaults").Key;

            foreach (Rom.IStoreRegion s in iRom.StoreRegionList)
            {
                uint type = iTags.Find(s.Type).Key;

                if (type == keyro || type == keyrwd)
                {
                    max += s.EntryList.Count;
                }
            }

            iConsole.ProgressOpen(max);

            iRoStores = new List<StoreRo>();

            int count = 0;

            foreach (Rom.IStoreRegion s in iRom.StoreRegionList)
            {
                uint key = iTags.Find(s.Tag).Key;
                uint type = iTags.Find(s.Type).Key;

                if (type == keyro || type == keyrwd)
                {
                    StoreRo store = new StoreRo(key, type);

                    foreach (Rom.IEntry e in s.EntryList)
                    {
                        Linn.Xml.Tags.ITagEntry t = iTags.Find(e.Tag);

                        StoreEntry entry;

                        if (t.Type == Linn.Xml.Tags.ETagEntryType.eSigned)
                        {
                            try
                            {
                                entry = StoreEntry.Create(t.Key, Convert.ToInt32(e.Value));
                                store.Add(entry);
                            }
                            catch (FormatException)
                            {
                                throw (new FlashException("Invalid tag " + e.Tag));
                            }
                            catch (OverflowException)
                            {
                                throw (new FlashException("Invalid tag " + e.Tag));
                            }
                        }
                        else if (t.Type == Linn.Xml.Tags.ETagEntryType.eUnsigned)
                        {
                            try
                            {
                                entry = StoreEntry.Create(t.Key, Convert.ToUInt32(e.Value));
                                store.Add(entry);
                            }
                            catch (FormatException)
                            {
                                throw (new FlashException("Invalid tag " + e.Tag));
                            }
                            catch (OverflowException)
                            {
                                throw (new FlashException("Invalid tag " + e.Tag));
                            }
                        }
                        else if (t.Type == Linn.Xml.Tags.ETagEntryType.eString)
                        {
                            entry = StoreEntry.Create(t.Key, e.Value);
                            store.Add(entry);
                        }
                        else if (t.Type == Linn.Xml.Tags.ETagEntryType.eBinary)
                        {
                            if (e.Source != "file")
                            {
                                throw (new FlashException("Invalid tag " + e.Tag));
                            }

                            Uri uri = new Uri(iRomInstallRoot, e.Value.Substring(1)); // get rid of initial '/'
                            string path = Uri.UnescapeDataString(uri.AbsolutePath);
                            FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                            entry = StoreEntry.Create(t.Key, file);
                            store.Add(entry);
                        }
                    }

                    iRoStores.Add(store);

                    iConsole.ProgressSetValue(++count);
                }
            }

            iConsole.ProgressClose();
            ReportSuccess();
        }

        private bool CheckIdenticalBinary(string aName)
        {
            ReportTitle("Check identical " + aName);

            string name = "u" + aName;

            iConsole.Write("Finding file ....................... ");

            Rom.IBinaryRegion region = iRom.FindBinaryRegion(name);

            if (region == null)
            {
                throw (new FlashException());
            }

            Uri uri = new Uri(iRomInstallRoot, region.Uri.Substring(1)); // get rid of initial '/'
            string path = Uri.UnescapeDataString(uri.AbsolutePath);

            FileStream file;

            try
            {
                file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (FileNotFoundException)
            {
                throw (new FlashException());
            }
            catch (DirectoryNotFoundException)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Generating crc ..................... ");

            uint crc = iCrc32.Compute(file);

            ReportSuccess();

            iConsole.Write("Finding layout section ............. ");

            Layout.ISection section = iLayout.Find(name);

            if (section == null)
            {
                throw (new FlashException());
            }

            uint id = iTags.Find(section.Flash).Key;

            ReportSuccess();

            uint key = iTags.Find(name).Key;
            uint type = iTags.Find("uStoreBinary").Key;

            iConsole.Write("Finding current section ............ ");

            RomDirEntry dir = iOnDeviceRomDir.Find(key);

            if (dir != null)
            {
                ReportSuccess();

                iConsole.Write("Checking identical ................. ");

                if (dir.Type == type &&
                    dir.Offset == section.Offset &&
                    dir.FlashId == id &&
                    dir.Bytes == file.Length &&
                    dir.Crc == crc)
                {
                    ReportYes();

                    return (true);
                }
            }

            ReportNo();

            return (false);
        }

        private void WriteBinary(string aSource, string aTarget)
        {
            string source = "u" + aSource;
            string target = "u" + aTarget;

            ReportTitle("Reprogram " + aSource);

            iConsole.Write("Finding file ....................... ");

            Rom.IBinaryRegion region = iRom.FindBinaryRegion(source);

            if (region == null)
            {
                throw (new FlashException());
            }

            Uri uri = new Uri(iRomInstallRoot, region.Uri.Substring(1)); // get rid of initial '/'
            string path = Uri.UnescapeDataString(uri.AbsolutePath);

            FileStream file;

            try
            {
                file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (FileNotFoundException)
            {
                throw (new FlashException());
            }
            catch (DirectoryNotFoundException)
            {
                throw (new FlashException());
            }

            string suffix = Path.GetExtension(path); // This includes the '.'

            byte[] data = null;
            bool writeDirectFromFile = false;

            switch ( suffix )
            {
                // All suffixes that require conversion to binary should
                // be handled here (until plugin system arrives :)

                case ".jed":
                    FuseMap f = new FuseMap(path);
                    data = f.AsBinary();
                    break;

                case ".bin":
                    writeDirectFromFile = true;
                    break;

                default:
                    throw (new FlashException());
            }

            uint byteCount = 0;

            ReportSuccess();

            iConsole.Write("Generating crc ..................... ");

            uint crc;

            if ( writeDirectFromFile )
            {
                crc = iCrc32.Compute(file);
                byteCount = (uint) file.Length;
            }
            else
            {
                crc = iCrc32.Compute(data);
                byteCount = (uint) data.Length;
            }

            ReportSuccess();

            iConsole.Write("Finding layout section ............. ");

            Layout.ISection section = iLayout.Find(target);

            if (section == null)
            {
                throw (new FlashException());
            }

            uint id = iTags.Find(section.Flash).Key;

            ReportSuccess();

            uint key = iTags.Find(target).Key;
            uint type = iTags.Find("uStoreBinary").Key;

            RomDirEntry entry = new RomDirEntry(key, id, section.Offset, byteCount, type, crc);

            iRomDir.Add(entry);

            if (!iDoNoTrust)
            {
                iConsole.Write("Finding current section ............ ");

                RomDirEntry dir = iOnDeviceRomDir.Find(key);

                if (dir != null)
                {
                    ReportSuccess();

                    iConsole.Write("Checking identical ................. ");

                    if (dir.Type == type &&
                        dir.Offset == section.Offset &&
                        dir.FlashId == id &&
                        dir.Bytes == byteCount &&
                        dir.Crc == crc)
                    {
                        ReportYes();
                        return;
                    }

                    ReportNo();
                }
                else
                {
                    ReportNo();
                }
            }

            iConsole.Write("Checking size ...................... ");

            if (byteCount > section.Bytes)
            {
                throw (new FlashException("Image: " + byteCount + " Section: " + section.Bytes));
            }

            ReportSuccess();

            iConsole.Write("Finding flash device ............... ");

            IFlash flash = iFallback.Find(id);

            iConsole.Write("Erasing sectors .................... ");

            iFallback.Erase(flash, section.Offset, section.Bytes);

            iConsole.Write("Writing image ...................... ");

            if ( writeDirectFromFile )
            {
                file.Position = 0;
                iFallback.Write(flash, section.Offset, file);
            }
            else
            {
                iFallback.Write(flash, section.Offset, data);
            }
        }

        private void WriteRwStore()
        {
            ReportTitle("Reprogram StoreReadWrite");

            iConsole.Write("Finding flash device ............... ");

            Layout.ISection section = iLayout.Find("uStoreReadWrite");

            if (section == null)
            {
                throw (new FlashException());
            }

            uint id = iTags.Find(section.Flash).Key;

            IFlash flash = iFallback.Find(id);

            iConsole.Write("Erasing sectors .................... ");

            iFallback.Erase(flash, section.Offset, section.Bytes);

            uint offset = 0;

            uint sectors = section.Bytes / flash.SectorBytes;

            for (int i = 0; i < sectors; i++)
            {
                string num = (i + 1).ToString();
                string dots = new string('.', 17 - num.Length);
                iConsole.Write("Formatting sector " + num + " " + dots + " ");
                iFallback.Write(flash, section.Offset + offset, BigEndian.Bytes(0xcccccccc));
                offset += flash.SectorBytes;
            }

            iConsole.Write("Creating image ..................... ");

            byte[] data = iRwStore.CreateRwStore(flash.SectorBytes);

            if (data.Length > section.Bytes)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Writing image ...................... ");

            iFallback.Write(flash, section.Offset, data);

            uint key = iTags.Find("uStoreReadWrite").Key;

            RomDirEntry entry = new RomDirEntry(key, id, section.Offset, section.Bytes, key);

            iRomDir.Add(entry);
        }

        private bool CheckIdenticalRoStores(string aName)
        {
            string tag = "u" + aName;

            ReportTitle("Check identical " + aName);

            iConsole.Write("Finding layout section ............. ");

            Layout.ISection section = iLayout.Find(tag);

            if (section == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            uint id = iTags.Find(section.Flash).Key;

            int offset;

            offset = 0;

            iConsole.Write("Checking identical ................. ");

            // check every store in rom file is present and correct on the device

            foreach (StoreRo s in iRoStores)
            {
                byte[] data = s.CreateRoStore();

                uint crc = iCrc32.Compute(data);

                RomDirEntry dir = iOnDeviceRomDir.Find(s.Key);

                if (dir == null ||
                    dir.Type != s.Type ||
                    dir.Offset != section.Offset + offset ||
                    dir.FlashId != id ||
                    dir.Bytes != data.Length ||
                    dir.Crc != crc)
                {
                    ReportNo();

                    return (false);
                }

                offset += data.Length;
            }

            // check no other ro or rwd store beyond these

            uint keyro = iTags.Find("uStoreReadOnly").Key;
            uint keyrwd = iTags.Find("uStoreReadWriteDefaults").Key;

            foreach (RomDirEntry e in iOnDeviceRomDir.EntryList)
            {
                if (e.Type == keyro || e.Type == keyrwd)
                {
                    if (e.FlashId != id || e.Offset >= section.Offset + offset)
                    {
                        ReportNo();

                        return (false);
                    }
                }
            }

            ReportYes();

            return (true);
        }

        private void AddRomDirEntry(string aName)
        {
            string tag = "u" + aName;

            ReportTitle("Add rom directory entry for " + aName);

            iConsole.Write("Finding layout section ............. ");

            Layout.ISection section = iLayout.Find(tag);

            if (section == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Adding entry ....................... ");

            uint key = iTags.Find(tag).Key;
            uint id = iTags.Find(section.Flash).Key;

            RomDirEntry entry = new RomDirEntry(key, id, section.Offset, section.Bytes, key);

            iRomDir.Add(entry);

            ReportSuccess();
        }

        private void WriteRoStores(string aName)
        {
            string tag = "u" + aName;

            ReportTitle("Reprogram " + aName);

            iConsole.Write("Finding layout section ............. ");

            Layout.ISection section = iLayout.Find(tag);

            if (section == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            uint id = iTags.Find(section.Flash).Key;

            int offset;

            bool identical = false;

            if (!iDoNoTrust)
            {
                offset = 0;

                identical = true;

                iConsole.Write("Checking identical ................. ");

                // check every store in rom file is present and correct on the device

                foreach (StoreRo s in iRoStores)
                {
                    byte[] data = s.CreateRoStore();

                    uint crc = iCrc32.Compute(data);

                    RomDirEntry dir = iOnDeviceRomDir.Find(s.Key);

                    if (dir == null ||
                        dir.Type != s.Type ||
                        dir.Offset != section.Offset + offset ||
                        dir.FlashId != id ||
                        dir.Bytes != data.Length ||
                        dir.Crc != crc)
                    {
                        identical = false;
                        break;
                    }

                    offset += data.Length;
                }

                // check no other ro or rwd store beyond these

                uint keyro = iTags.Find("uStoreReadOnly").Key;
                uint keyrwd = iTags.Find("uStoreReadWriteDefaults").Key;

                foreach (RomDirEntry e in iOnDeviceRomDir.EntryList)
                {
                    if (e.Type == keyro || e.Type == keyrwd)
                    {
                        if (e.FlashId != id || e.Offset >= section.Offset + offset)
                        {
                            identical = false;
                            break;
                        }
                    }
                }

                if (identical)
                {
                    ReportYes();
                }
                else
                {
                    ReportNo();
                }
            }

            offset = 0;

            foreach (StoreRo s in iRoStores)
            {
                byte[] data = s.CreateRoStore();

                uint crc = iCrc32.Compute(data);

                RomDirEntry entry = new RomDirEntry(s.Key, id, section.Offset + (uint)offset, (uint)data.Length, s.Type, crc);

                iRomDir.Add(entry);

                offset += data.Length;
            }

            if (!identical)
            {
                iConsole.Write("Checking size ...................... ");

                if (offset > section.Bytes)
                {
                    throw (new FlashException());
                }

                ReportSuccess();

                iConsole.Write("Finding flash device ............... ");

                IFlash flash = iFallback.Find(id);

                iConsole.Write("Erasing sectors .................... ");

                iFallback.Erase(flash, section.Offset, section.Bytes);

                byte[] stores = new byte[offset];

                offset = 0;

                foreach (StoreRo s in iRoStores)
                {
                    byte[] data = s.CreateRoStore();

                    Array.Copy(data, 0, stores, offset, data.Length);

                    offset += data.Length;
                }

                iConsole.Write("Writing image ...................... ");

                iFallback.Write(flash, section.Offset, stores);
            }
        }

        private void ClearRomDir(string aName)
        {
            string tag = "u" + aName;

            ReportTitle("Clear " + aName);

            iConsole.Write("Finding flash device ............... ");

            Layout.ISection section = iLayout.Find(tag);

            if (section == null)
            {
                throw (new FlashException());
            }

            uint id = iTags.Find(section.Flash).Key;

            IFlash flash = iFallback.Find(id);

            iConsole.Write("Creating image ..................... ");

            RomDir romdir = new RomDir();

            uint keyrw = iTags.Find("uStoreReadWrite").Key;

            foreach (RomDirEntry e in iOnDeviceRomDir.EntryList)
            {
                if (e.Type != keyrw)
                {
                    romdir.Add(new RomDirEntry(0, 0, 0, 0, 0));
                }
                else
                {
                    romdir.Add(e);
                }
            }

            byte[] data = romdir.Create();

            ReportSuccess();

            iConsole.Write("Writing image ...................... ");

            iFallback.Write(flash, section.Offset, data);
        }

        private void WriteRomDir(string aName)
        {
            string tag = "u" + aName;

            ReportTitle("Reprogram " + aName);

            iConsole.Write("Finding flash device ............... ");

            Layout.ISection section = iLayout.Find(tag);

            if (section == null)
            {
                throw (new FlashException());
            }

            uint id = iTags.Find(section.Flash).Key;

            IFlash flash = iFallback.Find(id);

            iConsole.Write("Erasing sectors .................... ");

            iFallback.Erase(flash, section.Offset, section.Bytes);

            iConsole.Write("Creating image ..................... ");

            byte[] data = iRomDir.Create();

            if (data.Length > section.Bytes)
            {
                throw (new FlashException("Rom directory too large"));
            }

            ReportSuccess();

            iConsole.Write("Writing image ...................... ");

            iFallback.Write(flash, section.Offset, data);
        }

        private void WriteCrash(string aName)
        {
            string tag = "u" + aName;

            ReportTitle("Reprogram " + aName);

            iConsole.Write("Finding flash device ............... ");

            Layout.ISection section = iLayout.Find(tag);

            if (section == null)
            {
                throw (new FlashException());
            }

            uint id = iTags.Find(section.Flash).Key;

            IFlash flash = iFallback.Find(id);

            iConsole.Write("Erasing sectors .................... ");

            iFallback.Erase(flash, section.Offset, section.Bytes);

            uint key = iTags.Find(tag).Key;
            uint type = iTags.Find("uStoreBinary").Key;

            RomDirEntry entry = new RomDirEntry(key, id, section.Offset, section.Bytes, type);

            iRomDir.Add(entry);
        }

        private void ValidateTag(string aTag)
        {
            if (iTags.Find(aTag) == null)
            {
                iConsole.ProgressClose();

                throw (new FlashException("Unable to find tag " + aTag));
            }
        }

        private void ValidateLayout()
        {
            iConsole.Write("Validating layout file ............. ");

            int items = 0;

            iConsole.ProgressOpen(iLayout.SectionList.Count * 2);

            foreach (Layout.ISection s in iLayout.SectionList)
            {
                ValidateTag(s.Flash);
                iConsole.ProgressSetValue(++items);
                ValidateTag(s.Tag);
                iConsole.ProgressSetValue(++items);
            }

            iConsole.ProgressClose();
            ReportSuccess();
        }

        private void ValidateFile(string aRelativeUri)
        {
            Uri uri = new Uri(iRomInstallRoot, aRelativeUri.Substring(1)); // get rid of initial '/'

            string path = Uri.UnescapeDataString(uri.AbsolutePath);

            try
            {
                FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                file.Close();
                return;
            }
            catch (FileNotFoundException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }

            iConsole.ProgressClose();

            throw (new FlashException("Unable open file " + path));
        }

        private void ValidateRom()
        {
            iConsole.Write("Validating rom file ................ ");

            int items = 0;

            int max;

            max = iRom.BinaryRegionList.Count * 2;

            foreach (Rom.IStoreRegion s in iRom.StoreRegionList)
            {
                max += 2;

                foreach (Rom.IEntry e in s.EntryList)
                {
                    max++;

                    if (e.Source == "file")
                    {
                        max++;
                    }
                }
            }

            iConsole.ProgressOpen(max);

            foreach (Rom.IBinaryRegion b in iRom.BinaryRegionList)
            {
                ValidateTag(b.Tag);
                iConsole.ProgressSetValue(++items);
                ValidateFile(b.Uri);
                iConsole.ProgressSetValue(++items);
            }

            foreach (Rom.IStoreRegion s in iRom.StoreRegionList)
            {
                ValidateTag(s.Tag);
                iConsole.ProgressSetValue(++items);
                ValidateTag(s.Type);
                iConsole.ProgressSetValue(++items);

                foreach (Rom.IEntry e in s.EntryList)
                {
                    ValidateTag(e.Tag);
                    iConsole.ProgressSetValue(++items);

                    if (e.Source == "file")
                    {
                        ValidateFile(e.Value);
                        iConsole.ProgressSetValue(++items);
                    }
                }
            }

            iConsole.ProgressClose();
            ReportSuccess();
        }

        public void Close()
        {
            if (iFinder != null)
            {
                iFinder.Close();
            }
        }

        public bool Fallback
        {
            get
            {
                return (iDoFallback);
            }
            set
            {
                iDoFallback = value;
            }
        }

        public bool WriteFallbackFpga
        {
            get
            {
                return (iDoWriteFallbackFpga);
            }
            set
            {
                iDoWriteFallbackFpga = value;
            }
        }

        public bool NoExec
        {
            get
            {
                return (iDoNoExec);
            }
            set
            {
                iDoNoExec = value;
            }
        }

        public bool Wait
        {
            get
            {
                return (iDoWait);
            }
            set
            {
                iDoWait = value;
            }
        }

        public bool NoTrust
        {
            get
            {
                return (iDoNoTrust);
            }
            set
            {
                iDoNoTrust = value;
            }
        }

        public bool Bootstrap
        {
            get
            {
                return (iDoBootstrap);
            }
            set
            {
                iDoBootstrap = value;
            }
        }

        public bool Unsafe
        {
            get
            {
                return (iUnsafe);
            }
            set
            {
                iUnsafe = value;
            }
        }

        public string Emulator
        {
            get
            {
                return (iEmulator);
            }
            set
            {
                iEmulator = value;
            }
        }

        public string Output
        {
            get
            {
                return (iOutput);
            }
            set
            {
                iOutput = value;
            }
        }

        private string iUglyName;
        private string iXmlFile;
        private IPAddress iInterface;
        private IConsole iConsole;
        private Finder iFinder;
        private ServiceFlash iServiceFlash;
        private RomDirInfo iRomDirInfo;
        private IFallback iFallback;
        private Crc32 iCrc32;
        private RomDir iOnDeviceRomDir;
        private Store iOnDeviceRwStore;
        private Store iRwStore;
        private List<StoreRo> iRoStores;
        private Store iEraseCountStore;
        private RomDir iRomDir;

        private bool iDoFallback;
        private bool iDoWriteFallbackFpga;
        private bool iDoNoExec;
        private bool iDoWait;
        private bool iDoNoTrust;
        private bool iDoBootstrap;
        private bool iUnsafe;

        private string iEmulator;
        private string iOutput;

        private bool iIsCollection;
        private Uri iXmlUri;
        private Uri iCollectionUri;
        private RomCollection iCollection;
        private Rom iRom;
        private Uri iRomInstallRoot;
        private string iLayoutUri;
        private Layout iLayout;
        private string iTagsUri;
        private Linn.Xml.Tags iTags;

        private string iPhase;
    }

    public class FpgaWriter
    {
        public FpgaWriter(IPAddress aInterface, IConsole aConsole, string aUglyName, string aBitstream)
        {
            iInterface = aInterface;
            iConsole = aConsole;
            iUglyName = aUglyName;
            iBitstream = aBitstream;
            iFallbackFinder = new Finder(iInterface, iConsole, aUglyName);
            iCrc32 = new Crc32();
            iFallback = false;
            iNoExec = false;
            iWait = false;
        }

        public bool Execute()
        {
            try
            {
                DoExecute();

                iConsole.Newline();

                iConsole.Write(iUglyName + " fpga written successfully");
                iConsole.Newline();

                return (true);
            }
            catch (FlashException e)
            {
                iConsole.Write(e.Message);
                iConsole.Newline();
                return (false);
            }
        }

        private void ReportTitle(string aTitle)
        {
            iConsole.Title(aTitle);
        }

        private void ReportSuccess()
        {
            iConsole.Write("ok");
            iConsole.Newline();
        }

        private void ReportYes()
        {
            iConsole.Write("yes");
            iConsole.Newline();
        }

        private void ReportNo()
        {
            iConsole.Write("no");
            iConsole.Newline();
        }

        private void ReportSkipped()
        {
            iConsole.Write("skipped");
            iConsole.Newline();
        }

        private void DoExecute()
        {
            if (Fallback)
            {
            	if (Hdmi)
            	{
		            iConsole.Write("No HDMI fpga in fallback ........... ");
	                throw (new FlashException());
            	}
            	
                RebootMain();

                CollectRomDirFallback();

                if (WriteBinary(Linn.Tags.uFallbackFpga))
                {
                    WriteRomDirFallback();
                }

                if (!iNoExec)
                {
                    IssueRebootToFallback();
                }
            }
            else
            {
                RebootFallback();

                CollectRomDirMain();

				if (Hdmi)
				{
	                if (WriteBinary(Linn.Tags.uHdmiFpga))
	                {
	                    WriteRomDirMain();
	                }
				}
				else
				{
	                if (WriteBinary(Linn.Tags.uMainFpga))
	                {
	                    WriteRomDirMain();
	                }
                }

                if (!iNoExec)
                {
                    IssueRebootToMain();
                }
            }
        }

        private void RebootMain()
        {
            ReportTitle("Reboot to main");

            if (!iFallbackFinder.EstablishMain())
            {
                throw (new FlashException());
            }

            iServiceFlash = new ServiceFlash(iFallbackFinder.Device);
            iRomDirInfo = new RomDirInfo(iConsole, iServiceFlash);
            iFallbackHandler = new Fallback(iConsole, iServiceFlash);
        }

        private void RebootFallback()
        {
            ReportTitle("Reboot to fallback");

            if (!iFallbackFinder.EstablishFallback())
            {
                throw (new FlashException());
            }

            iServiceFlash = new ServiceFlash(iFallbackFinder.Device);
            iRomDirInfo = new RomDirInfo(iConsole, iServiceFlash);
            iRomDirInfo = new RomDirInfo(iConsole, iServiceFlash);
            iFallbackHandler = new Fallback(iConsole, iServiceFlash);
        }

        private void IssueRebootToMain()
        {
            ReportTitle("Reboot to main");

            if (!iFallbackFinder.SetBootMode("Main"))
            {
                throw (new FlashException());
            }

            if (!iFallbackFinder.IssueReboot())
            {
                throw (new FlashException());
            }

            if (iWait)
            {
                if (!iFallbackFinder.WaitForOffOn())
                {
                    ReportSkipped();
                }

                if (!iFallbackFinder.FindDevice())
                {
                    throw (new FlashException());
                }
            }
        }

        private void IssueRebootToFallback()
        {
            ReportTitle("Reboot to fallback");

            if (!iFallbackFinder.SetBootMode("Fallback"))
            {
                throw (new FlashException());
            }

            if (!iFallbackFinder.IssueReboot())
            {
                throw (new FlashException());
            }

            if (iWait)
            {
                if (!iFallbackFinder.WaitForOffOn())
                {
                    ReportSkipped();
                }


                if (!iFallbackFinder.FindDevice())
                {
                    throw (new FlashException());
                }
            }
        }

        private void CollectRomDirMain()
        {
            ReportTitle("Collect main rom directory");

            iConsole.Write("Collecting rom directory info ...... ");

            iRomDirInfo.ReadRomDirInfo();

            iConsole.Write("Finding flash device ............... ");

            IFlash flash = iFallbackHandler.Find(iRomDirInfo.RomDirMainFlashId);

            iConsole.Write("Reading rom directory .............. ");

            byte[] data = iFallbackHandler.Read(flash, iRomDirInfo.RomDirMainOffset, iRomDirInfo.RomDirMainBytes);

            iConsole.Write("Sorting rom directory .............. ");

            iOnDeviceRomDir = RomDir.Create(data, 0);

            if (iOnDeviceRomDir == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();
        }


        private void CollectRomDirFallback()
        {
            ReportTitle("Collect fallback rom directory");

            iConsole.Write("Collecting rom directory info ...... ");

            iRomDirInfo.ReadRomDirInfo();

            iConsole.Write("Finding flash device ............... ");

            IFlash flash = iFallbackHandler.Find(iRomDirInfo.RomDirFallbackFlashId);

            iConsole.Write("Reading rom directory .............. ");

            byte[] data = iFallbackHandler.Read(flash, iRomDirInfo.RomDirFallbackOffset, iRomDirInfo.RomDirFallbackBytes);

            iConsole.Write("Sorting rom directory .............. ");

            iOnDeviceRomDir = RomDir.Create(data, 0);

            if (iOnDeviceRomDir == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();
        }

        private bool WriteBinary(uint aKey)
        {
            ReportTitle("Write bitstream");

            iConsole.Write("Finding file ....................... ");

            FileStream file;

            try
            {
                file = File.Open(iBitstream, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (FileNotFoundException)
            {
                throw (new FlashException());
            }
            catch (DirectoryNotFoundException)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Generating crc ..................... ");

            uint crc = iCrc32.Compute(file);

            ReportSuccess();

            iConsole.Write("Finding rom directory entry ........ ");

            RomDirEntry dir = iOnDeviceRomDir.Find(aKey);

            if (dir == null)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Checking identical ................. ");

            if (dir.Crc == crc)
            {
                ReportYes();
                return (false);
            }

            dir.Crc = crc;

            ReportNo();

            iConsole.Write("Checking size ...................... ");

            if (file.Length != dir.Bytes)
            {
                throw (new FlashException());
            }

            ReportSuccess();

            iConsole.Write("Finding flash device ............... ");

            IFlash flash = iFallbackHandler.Find(dir.FlashId);

            iConsole.Write("Erasing sectors .................... ");

            uint sectorbytes = ((dir.Bytes + flash.SectorBytes - 1) / flash.SectorBytes) * flash.SectorBytes;

            iFallbackHandler.Erase(flash, dir.Offset, sectorbytes);

            iConsole.Write("Writing image ...................... ");

            file.Position = 0;

            iFallbackHandler.Write(flash, dir.Offset, file);

            return (true);
        }

        private void WriteRomDirMain()
        {
            ReportTitle("Write main rom directory");

            iConsole.Write("Finding flash device ............... ");

            IFlash flash = iFallbackHandler.Find(iRomDirInfo.RomDirMainFlashId);

            iConsole.Write("Erasing sectors .................... ");

            iFallbackHandler.Erase(flash, iRomDirInfo.RomDirMainOffset, iRomDirInfo.RomDirMainBytes);

            iConsole.Write("Creating image ..................... ");

            byte[] data = iOnDeviceRomDir.Create();

            ReportSuccess();

            iConsole.Write("Writing image ...................... ");

            iFallbackHandler.Write(flash, iRomDirInfo.RomDirMainOffset, data);
        }

        private void WriteRomDirFallback()
        {
            ReportTitle("Write main rom directory");

            iConsole.Write("Finding flash device ............... ");

            IFlash flash = iFallbackHandler.Find(iRomDirInfo.RomDirFallbackFlashId);

            iConsole.Write("Erasing sectors .................... ");

            iFallbackHandler.Erase(flash, iRomDirInfo.RomDirFallbackOffset, iRomDirInfo.RomDirFallbackBytes);

            iConsole.Write("Creating image ..................... ");

            byte[] data = iOnDeviceRomDir.Create();

            ReportSuccess();

            iConsole.Write("Writing image ...................... ");

            iFallbackHandler.Write(flash, iRomDirInfo.RomDirFallbackOffset, data);
        }

        public void Close()
        {
            iFallbackFinder.Close();
        }

        public bool Fallback
        {
            get
            {
                return (iFallback);
            }
            set
            {
                iFallback = value;
            }
        }        

        public bool NoExec
        {
            get
            {
                return (iNoExec);
            }
            set
            {
                iNoExec = value;
            }
        }

        public bool Wait
        {
            get
            {
                return (iWait);
            }
            set
            {
                iWait = value;
            }
        }
        
        public bool Hdmi
        {
            get
            {
                return (iHdmi);
            }
            set
            {
                iHdmi = value;
            }
        }

        private string iUglyName;
        private string iBitstream;
        private IPAddress iInterface;
        private IConsole iConsole;
        private Finder iFallbackFinder;
        private ServiceFlash iServiceFlash;
        private RomDirInfo iRomDirInfo;
        private Fallback iFallbackHandler;
        private Crc32 iCrc32;
        private RomDir iOnDeviceRomDir;

        private bool iFallback;
        private bool iNoExec;
        private bool iWait;
        private bool iHdmi;
    }
}

