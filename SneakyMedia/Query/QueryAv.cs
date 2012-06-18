using System;
using System.Collections.Generic;

using SneakyMedia.Scanner;
using SneakyMedia.Database;

namespace SneakyMedia.Query
{
    public class ModuleQueryAv : ModuleQuery
    {
        public ModuleQueryAv()
            : base ("Av", new Version(1, 0, 0, 0))
        {
        }

        protected override void OnCreate()
        {
        }

        protected override void OnLoad()
        {
        }

        public override IList<IList<string>> Query(List<string> aQuery)
        {
            string search = null;
            List<IMetadatum> where = new List<IMetadatum>();
            uint random = 0;
            List<ITag> show = new List<ITag>();

            IEnumerator<string> list = aQuery.GetEnumerator();

            while (list.MoveNext())
            {
                string clause = list.Current.ToUpperInvariant();

                if (clause == "SEARCH")
                {
                    if (where.Count != 0)
                    {
                        throw (new QueryError("Cannot mix SEARCH and WHERE clauses"));
                    }

                    if (!list.MoveNext())
                    {
                        throw (new QueryError("SEARCH incomplete"));
                    }

                    search = list.Current;
                }
                else if (clause == "WHERE")
                {
                    if (search != null)
                    {
                        throw (new QueryError("Cannot mix SEARCH and WHERE clauses"));
                    }

                    if (!list.MoveNext())
                    {
                        throw (new QueryError("WHERE tag not specified"));
                    }

                    ITag tag = new HypertagAv(list.Current);

                    if (!list.MoveNext())
                    {
                        throw (new QueryError("WHERE value not specified"));
                    }

                    string value = list.Current;

                    where.Add(new Metadatum(tag, list.Current));
                }
                else if (clause == "RANDOM")
                {
                    if (random != 0)
                    {
                        throw (new QueryError("Cannot specify more than one RANDOM value"));
                    }

                    if (!list.MoveNext())
                    {
                        throw (new QueryError("RANDOM value not specified"));
                    }

                    try
                    {
                        random = uint.Parse(list.Current);
                    }
                    catch (FormatException)
                    {
                        throw (new QueryError("RANDOM value invalid"));
                    }

                    if (random == 0)
                    {
                        throw (new QueryError("RANDOM value cannot be zero"));
                    }
                }
                else if (clause == "SHOW")
                {
                    if (!list.MoveNext())
                    {
                        throw (new QueryError("SHOW incomplete"));
                    }

                    show.Add(new HypertagAv(list.Current));
                }
                else
                {
                    throw (new QueryError("Invalid Clause"));
                }
            }

            if (show.Count == 0)
            {
                throw (new QueryError("No SHOW clause specified"));
            }

            if (search != null)
            {
                return (Engine.SearchItems(search, random, show));
            }
            else
            {
                return (Engine.QueryItems(where, random, show));
            }
        }
    }
}

