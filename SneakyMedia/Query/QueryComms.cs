using System;
using System.Collections.Generic;

using SneakyMedia.Database;

namespace SneakyMedia.Query
{
    public abstract class ModuleQueryComms : Module
    {
        public const string kModuleType = "QueryComms";

        protected ModuleQueryComms(string aName, Version aVersion)
            : base(aName, aVersion, kModuleType)
        {
        }
    }
}

