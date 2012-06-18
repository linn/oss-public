using System;
using System.Collections.Generic;

using SneakyMedia.Database;

namespace SneakyMedia.Query
{
    public class QueryError : Exception
    {
        public QueryError(string aMessage)
            : base(aMessage)
        {
        }
    }

    public interface IQuery
    {
        IList<IList<string>> Query(List<string> aQuery);
    }

    public abstract class ModuleQuery : Module, IQuery
    {
        public const string kModuleType = "Query";

        protected ModuleQuery(string aName, Version aVersion)
            : base(aName, aVersion, kModuleType)
        {
        }

        public abstract IList<IList<string>> Query(List<string> aQuery);
    }
}

