using System;
using System.Collections.Generic;
using System.Threading;

using SneakyMedia.Scanner;
using SneakyMedia.Database;
using SneakyMedia.Query;
using SneakyMedia.Browse;




namespace SneakyMedia.Database
{
    public interface IModule
    {
        string ModuleName { get; }
        Version ModuleVersion { get; }
        string ModuleType { get; }
        void OnCreate(IEngine aEngine);
        void OnLoad(IEngine aEngine);
    }

    public abstract class Module : IModule
    {
        protected Module(string aName, Version aVersion, string aType)
        {
            iModuleName = aName;
            iModuleVersion = aVersion;
            iModuleType = aType;
        }

        public string ModuleName
        {
            get
            {
                return (iModuleName);
            }
        }

        public Version ModuleVersion
        {
            get
            {
                return (iModuleVersion);
            }
        }

        public string ModuleType
        {
            get
            {
                return (iModuleType);
            }
        }

        protected IEngine Engine
        {
            get
            {
                return (iEngine);
            }
        }

        public void OnCreate(IEngine aEngine)
        {
            iEngine = aEngine;
            OnCreate();
        }

        public void OnLoad(IEngine aEngine)
        {
            iEngine = aEngine;
            OnLoad();
        }

        protected void CreateModule(IModule aModule)
        {
            iEngine.CreateModule(aModule);
        }

        protected abstract void OnCreate();

        protected abstract void OnLoad();

        private string iModuleName;
        private Version iModuleVersion;
        private string iModuleType;
        private IEngine iEngine;
    }

    public interface IEngine : IScanner, IDatabase, IQuery
    {
        void CreateModule(IModule aModule);
        IList<Mount> Mounts { get; }
        Mount CreateMount(string aId, string aUri);
        Mount FindMount(string aId);
        IScheme LoadScheme(string aId);
    }

    public class Engine : IEngine
    {
        public Engine()
        {
            iModules = new List<IModule>();

            // Create the Scanner

            iScanner = new ModuleScannerMain();

            CreateModule(iScanner);

            // Create the Database

            iDatabase = new DatabaseMySql();

            //CreateModule(iDatabase);

            // Create the Mounts

            iMounts = new List<Mount>();

            IList<string> mounts = iDatabase.QueryMounts();

            foreach (string mount in mounts)
            {
                iMounts.Add(new Mount(this, mount));
            }

            // Create the Query Modules

            CreateModule(new ModuleQueryGeneral());
            CreateModule(new ModuleQueryAv());

            // Create the Query Comms Modules

            CreateModule(new ModuleQueryCommsTelnet());

            // Initialise the schemes

            iSchemes = new Dictionary<string, IScheme>();

            // Create the Browse Comms Modules

            CreateModule(new ModuleBrowseTelnet());
            CreateModule(new ModuleBrowseWeb());
            CreateModule(new ModuleBrowseUpnp());
        }

        public static IEngine CreateEngine()
        {
            return (new Engine());
        }

        // IEngine

        public void CreateModule(IModule aModule)
        {
            iModules.Add(aModule);
            aModule.OnCreate(this);
        }

        public IList<Mount> Mounts
        {
            get
            {
                return (iMounts.AsReadOnly());
            }
        }

        public Mount CreateMount(string aId, string aUri)
        {
            Mount mount = new Mount(this, aId, aUri);
            iMounts.Add(mount);
            return (mount);
        }

        public Mount FindMount(string aId)
        {
            foreach (Mount mount in iMounts)
            {
                if (mount.MountId == aId)
                {
                    return (mount);
                }
            }
            throw (new MountNotFoundError());
        }

        public IScheme LoadScheme(string aId)
        {
            IScheme scheme;

            try
            {
                scheme = iSchemes[aId];
            }
            catch (KeyNotFoundException)
            {
                scheme = Browse.Scheme.LoadScheme(aId);

                iSchemes.Add(aId, scheme);
            }

            return (scheme);
        }

        // IScanner

        public IList<IMetadatum> Scan(string aUri)
        {
            return (iScanner.Scan(aUri));
        }

        // IDatabase

        public void AddMount(string aMountId, string aUri)
        {
            iDatabase.AddMount(aMountId, aUri);
        }

        public void UpdateMountUri(string aMountId, string aUri)
        {
            iDatabase.UpdateMountUri(aMountId, aUri);
        }

        public void UpdateMountScanned(string aMountId, DateTime aScanned)
        {
            iDatabase.UpdateMountScanned(aMountId, aScanned);
        }

        public string QueryMountUri(string aMountId)
        {
            return (iDatabase.QueryMountUri(aMountId));
        }

        public Nullable<DateTime> QueryLastScanned(string aMountId)
        {
            return (iDatabase.QueryLastScanned(aMountId));
        }

        public IList<string> QueryMounts()
        {
            return (iDatabase.QueryMounts());
        }

        public void Add(IItem aItem, IList<IMetadatum> aMetadata)
        {
            iDatabase.Add(aItem, aMetadata);
        }

        public void Remove(IItem aId)
        {
            iDatabase.Remove(aId);
        }

        public void Add(IItem aId, IMetadatum aMetadatum)
        {
            iDatabase.Add(aId, aMetadatum);
        }

        public void Remove(IItem aId, IMetadatum aMetadatum)
        {
            iDatabase.Remove(aId, aMetadatum);
        }

        public IList<IMetadatum> QueryItem(IItem aId)
        {
            return (iDatabase.QueryItem(aId));
        }

        public IList<IItem> QueryItems(string aMountId)
        {
            return (iDatabase.QueryItems(aMountId));
        }

        public IList<IList<string>> QueryItems(IList<IMetadatum> aSelect, uint aRandom, IList<ITag> aShow)
        {
            return (iDatabase.QueryItems(aSelect, aRandom, aShow));
        }

        public IList<IList<string>> SearchItems(string aSearch, uint aRandom, IList<ITag> aShow)
        {
            return (iDatabase.SearchItems(aSearch, aRandom, aShow));
        }

        public IList<IList<string>> Query(List<string> aQuery)
        {
            if (aQuery.Count > 0)
            {
                string module = aQuery[0];

                if (module.Length > 0)
                {
                    foreach (Module m in iModules)
                    {
                        if (m is ModuleQuery)
                        {
                            if (String.Compare(m.ModuleName, module, true) == 0)
                            {
                                return ((m as ModuleQuery).Query(aQuery.GetRange(1, aQuery.Count - 1)));
                            }
                        }
                    }
                }
            }

            throw (new QueryError("Query plugin not specified"));
        }

        private ModuleScannerMain iScanner;
        private IDatabase iDatabase;

        private List<IModule> iModules;
        private List<Mount> iMounts;
        private Dictionary<string, IScheme> iSchemes;
    }
}
