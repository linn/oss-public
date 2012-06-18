using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

using KinskyMedia.Database;
using KinskyMedia.Scanner;

namespace KinskyMedia
{
    public interface IConsole
    {
        void Write(string aMessage);
    }

    public interface IFileProcessor
    {
        void Process(string aUri);
    }

    public class Mounter : IFileProcessor
    {
        public Mounter(IConsole aConsole, IDatabase aDatabase, IScanner aScanner, IMount aMount)
        {
            iConsole = aConsole;
            iDatabase = aDatabase;
            iScanner = aScanner;
            iMount = aMount;
        }

        public void Process(string aUri)
        {
            iConsole.Write(aUri + "\n");

            IList<IMetadatum> metadata = iScanner.Scan(iMount.Uri + "/" + aUri);

            if (metadata != null)
            {
                //IItem item = iDatabase.CreateItem(iMount, aUri);

                foreach (IMetadatum metadatum in metadata)
                {
                    iConsole.Write("---- " + metadatum.Tag.Ns + ":" + metadatum.Tag.Name + " " + metadatum.Value + "\n");

                    //iDatabase.CreateMetadatum(item, metadatum.Tag, metadatum.Value);
                }
            }
        }

        private IConsole iConsole;
        private IDatabase iDatabase;
        private ITagManager iTagManager;
        private IScanner iScanner;
        private IMount iMount;
    }

    public class DirectoryScanner
    {
        public DirectoryScanner(string aUri)
        {
            if ((File.GetAttributes(aUri) & FileAttributes.Directory) != 0)
            {
                iDirectory = new DirectoryInfo(aUri);
            }
        }

        public void Process(IFileProcessor aProcessor)
        {
            if (iDirectory != null)
            {
                iProcessor = aProcessor;
                Scan(iDirectory, "");
            }
        }

        public bool IsDirectory
        {
            get
            {
                return (iDirectory != null);
            }
        }

        private void Scan(DirectoryInfo aInfo, string aUri)
        {
            FileInfo[] files = aInfo.GetFiles();

            foreach (FileInfo file in files)
            {
                iProcessor.Process(aUri + file.Name);
            }

            DirectoryInfo[] dirs = aInfo.GetDirectories();

            foreach (DirectoryInfo dir in dirs)
            {
                if ((dir.Attributes & FileAttributes.ReparsePoint) == 0)
                {
                    Scan(dir, aUri + dir.Name + "/");
                }
            }
        }
        
        DirectoryInfo iDirectory;
        IFileProcessor iProcessor;
    }

    public class Engine : IConsole
    {
        public Engine(IConsole aConsole)
        {
            iConsole = aConsole;
            iDatabase = new DatabaseDb4o();
            iTagManager = new TagManager(iDatabase);
            iScanner = new ScannerManager();
            iScanner.ManageTags(iTagManager);
        }

        public void AddMount(string aId, string aUri)
        {
            DirectoryScanner dir = new DirectoryScanner(aUri);

            if (!dir.IsDirectory)
            {
                WriteLine("Specified mount uri is not a folder");
                return;
            }

            IMount mount = iDatabase.CreateMount(aId, aUri);

            Mounter mounter = new Mounter(iConsole, iDatabase, iScanner, mount);

            dir.Process(mounter);
        }

        public void Write(string aMessage)
        {
            iConsole.Write(aMessage);
        }

        public void WriteLine(string aMessage)
        {
            iConsole.Write(aMessage + "\n");
        }


        private IConsole iConsole;
        private IDatabase iDatabase;
        private ITagManager iTagManager;
        private IScanner iScanner;
    }
}
