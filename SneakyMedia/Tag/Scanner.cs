using System;
using System.Collections.Generic;

using SneakyMedia.Database;

namespace SneakyMedia.Scanner
{
    public interface IScanner
    {
        IList<IMetadatum> Scan(string aUri);
    }

    public abstract class ModuleScanner : Module, IScanner
    {
        public const string kModuleScannerType = "Scanner";

        protected ModuleScanner(string aName, Version aVersion)
            : base(aName, aVersion, kModuleScannerType)
        {
        }

        public abstract IList<IMetadatum> Scan(string aUri);
    }

    public class ModuleScannerMain : ModuleScanner
    {
        public ModuleScannerMain()
            : base("Main", new Version(1, 0, 0, 0))
        {
            iScanners = new List<IScanner>();
        }

        protected override void OnCreate()
        {
            Create(new ModuleScannerXiph());
            Create(new ModuleScannerTagLib());
        }

        protected override void OnLoad()
        {
        }

        private void Create(ModuleScanner aScanner)
        {
            CreateModule(aScanner);
            iScanners.Add(aScanner);
        }

        public override IList<IMetadatum> Scan(string aUri)
        {
            // Use the list of scanners in force when the scan request was made

            foreach (IScanner scanner in iScanners)
            {
                IList<IMetadatum> tags = scanner.Scan(aUri);

                if (tags != null)
                {
                    return (tags);
                }
            }

            return (new List<IMetadatum>());
        }

        private List<IScanner> iScanners;
    }
}

