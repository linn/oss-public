using System;
using System.Collections.Generic;
using System.Threading;

using System.Data.SQLite;

namespace SneakyMedia.Database
{
    public class DatabaseSqlite : IDatabase
    {
        private class Tag : ITag
        {
            public Tag(ITag aTag)
            {
                iTag = aTag;
                iItems = new Dictionary<string, Dictionary<string, Item>>();
            }

            public void Add(Item aItem)
            {
                Dictionary<string, Item> mountitems;

                try
                {
                     mountitems = iItems[aItem.MountId];
                }
                catch (KeyNotFoundException)
                {
                    mountitems = new Dictionary<string, Item>();
                    iItems.Add(aItem.MountId, mountitems);
                }


                try
                {
                    mountitems.Add(aItem.ItemUri, aItem);
                }
                catch (ArgumentException)
                {
                    throw (new ItemAlreadyExistsError());
                }
            }

            public void Remove(Item aItem)
            {
                iItems[aItem.MountId].Remove(aItem.ItemUri);

                if (iItems[aItem.MountId].Keys.Count == 0)
                {
                    iItems.Remove(aItem.MountId);
                }
            }

            public string Ns
            {
                get
                {
                    return (iTag.Ns);
                }
            }

            public string Name
            {
                get
                {
                    return (iTag.Name);
                }
            }

            private ITag iTag;
            private Dictionary<string, Dictionary<string, Item>> iItems;
        }

        private class Item : IItem
        {
            public Item(Mount aMount, string aRelativeUri, IList<IMetadatum> aMetadata)
            {
                iMount = aMount;
                iRelativeUri = aRelativeUri;
                iMetadata = new List<IMetadatum>();
                iMetadata.AddRange(aMetadata);
            }

            public void Add(IMetadatum aMetadatum)
            {
                iMetadata.Add(aMetadatum);
            }

            public void Remove(IMetadatum aMetadatum)
            {
                iMetadata.Remove(aMetadatum);
            }

            public string MountId
            {
                get
                {
                    return (iMount.Id);
                }
            }

            public string ItemUri
            {
                get
                {
                    return (iRelativeUri);
                }
            }

            public IList<IMetadatum> Metadata
            {
                get
                {
                    return (iMetadata.AsReadOnly());
                }
            }

            Mount iMount;
            string iRelativeUri;
            List<IMetadatum> iMetadata;
        }

        private class Mount
        {
            public Mount(string aId, string aUri)
            {
                iId = aId;
                iUri = aUri;
                iItems = new Dictionary<string, Item>();
            }

            public void Add(Item aItem)
            {
                try
                {
                    iItems.Add(aItem.ItemUri, aItem);
                }
                catch (ArgumentException)
                {
                    throw (new ItemAlreadyExistsError());
                }
            }

            public void Add(IItem aItemId, IMetadatum aMetadatum)
            {
                try
                {
                    iItems[aItemId.ItemUri].Add(aMetadatum);
                }
                catch (KeyNotFoundException)
                {
                    throw (new ItemNotFoundError());
                }
            }

            public Item Remove(IItem aItemId)
            {
                Item item;

                try
                {
                    item = iItems[aItemId.ItemUri];
                    iItems.Remove(aItemId.ItemUri);
                    return (item);
                }
                catch (KeyNotFoundException)
                {
                    throw (new ItemNotFoundError());
                }
            }

            public void Remove(IItem aItemId, IMetadatum aMetadatum)
            {
                try
                {
                    iItems[aItemId.ItemUri].Remove(aMetadatum);
                }
                catch (KeyNotFoundException)
                {
                    throw (new ItemNotFoundError());
                }
            }

            public IList<IMetadatum> QueryItem(IItem aItemId)
            {
                try
                {
                    return (iItems[aItemId.ItemUri].Metadata);
                }
                catch (KeyNotFoundException)
                {
                    throw (new ItemNotFoundError());
                }
            }

            public IList<IItem> QueryItems()
            {
                List<IItem> items = new List<IItem>();

                foreach (Item item in iItems.Values)
                {
                    items.Add(item);
                }

                return (items);
            }

            public string Id
            {
                get
                {
                    return (iId);
                }
            }

            public string Uri
            {
                get
                {
                    return (iUri);
                }
                set
                {
                    iUri = value;
                }
            }

            public DateTime Scanned
            {
                get
                {
                    return (iScanned);
                }
                set
                {
                    iScanned = value;
                }
            }

            private string iId;
            private string iUri;
            private DateTime iScanned;
            private Dictionary<string, Item> iItems;
        }

        public DatabaseSqlite()
        {
            iMounts = new Dictionary<string, Mount>();
            iTags = new Dictionary<string, Dictionary<string, Tag>>();
        }

        private SQLiteConnection iConnection;
        public void AddMount(string aMountId, string aUri)
        {
            try
            {
                iMounts.Add(aMountId, new Mount(aMountId, aUri));
            }
            catch (ArgumentException)
            {
                throw (new MountAlreadyExistsError());
            }
        }

        public void UpdateMountUri(string aMountId, string aUri)
        {
            try
            {
                Mount mount = iMounts[aMountId];
                mount.Uri = aUri;
            }
            catch (KeyNotFoundException)
            {
                throw (new MountNotFoundError());
            }
        }

        public void UpdateMountScanned(string aMountId, DateTime aScanned)
        {
            try
            {
                Mount mount = iMounts[aMountId];
                mount.Scanned = aScanned;
            }
            catch (KeyNotFoundException)
            {
                throw (new MountNotFoundError());
            }
        }

        public string QueryMountUri(string aMountId)
        {
            try
            {
                Mount mount = iMounts[aMountId];
                return (mount.Uri);
            }
            catch (KeyNotFoundException)
            {
                throw (new MountNotFoundError());
            }
        }

        public Nullable<DateTime> QueryLastScanned(string aMountId)
        {
            try
            {
                Mount mount = iMounts[aMountId];
                return (mount.Scanned);
            }
            catch (KeyNotFoundException)
            {
                throw (new MountNotFoundError());
            }
        }

        public IList<string> QueryMounts()
        {
            List<string> mounts = new List<string>();
            mounts.AddRange(iMounts.Keys);
            return (mounts);
        }

        public void Add(IItem aItemId, IList<IMetadatum> aMetadata)
        {
            Mount mount;

            try
            {
                mount = iMounts[aItemId.MountId];

            }
            catch (KeyNotFoundException)
            {
                throw (new MountNotFoundError());
            }

            // Create Item

            Item item = new Item(mount, aItemId.ItemUri, aMetadata);

            // Add item to the mount

            mount.Add(item);

            // Process tags
            /*
            foreach (IMetadatum entry in aMetadata)
            {
                Tag tag;

                try
                {
                    tag = iTags[entry.Tag.Ns][entry.Tag.Name];
                }
                catch (KeyNotFoundException)
                {
                    tag = new Tag(entry.Tag);

                    Dictionary<string, Tag> nstags;

                    try
                    {
                        nstags = iTags[tag.Ns];
                    }
                    catch (KeyNotFoundException)
                    {
                        nstags = new Dictionary<string, Tag>();
                        iTags.Add(tag.Ns, nstags);
                    }

                    nstags.Add(tag.Name, tag);
                }

                tag.Add(item);
            }
            */
        }

        public void Remove(IItem aItemId)
        {
            Item item;

            try
            {
                Mount mount = iMounts[aItemId.MountId];
                item = mount.Remove(aItemId);
            }
            catch (KeyNotFoundException)
            {
                throw (new MountNotFoundError());
            }

            // Process the tags

            foreach (IMetadatum entry in item.Metadata)
            {
                iTags[entry.Tag.Ns][entry.Tag.Name].Remove(item);
            }
        }

        public void Add(IItem aItemId, IMetadatum aMetadatum)
        {
            try
            {
                Mount mount = iMounts[aItemId.MountId];

                // Add metadata to the item

                mount.Add(aItemId, aMetadatum);
            }
            catch (KeyNotFoundException)
            {
                throw (new MountNotFoundError());
            }
        }

        public void Remove(IItem aItemId, IMetadatum aMetadatum)
        {
            try
            {
                Mount mount = iMounts[aItemId.MountId];

                // Remove metadata from the item

                mount.Remove(aItemId, aMetadatum);
            }
            catch (KeyNotFoundException)
            {
                throw (new MountNotFoundError());
            }
        }

        public IList<IMetadatum> QueryItem(IItem aItemId)
        {
            try
            {
                Mount mount = iMounts[aItemId.MountId];

                // Query the item

                return (mount.QueryItem(aItemId));
            }
            catch (KeyNotFoundException)
            {
                throw (new MountNotFoundError());
            }
        }

        public IList<IItem> QueryItems(string aMountId)
        {
            try
            {
                Mount mount = iMounts[aMountId];

                // Query the items

                return (mount.QueryItems());
            }
            catch (KeyNotFoundException)
            {
                throw (new MountNotFoundError());
            }
        }

        public IList<IList<string>> QueryItems(IList<IMetadatum> aSelect, uint aRandom, IList<ITag> aShow)
        {
            return (new List<IList<string>>());
        }

        public IList<IList<string>> SearchItems(string aSearch, uint aRandom, IList<ITag> aShow)
        {
            return (new List<IList<string>>());
        }

        private Dictionary<string, Mount> iMounts;
        private Dictionary<string, Dictionary<string, Tag>> iTags;
    }
}
