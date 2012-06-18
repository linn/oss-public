using System;
using System.Collections.Generic;
using System.Threading;

using Db4objects.Db4o;

namespace KinskyMedia.Database
{
    // Tables
    
    internal class Mount : IMount
    {
        public Mount(string aId, string aUri)
        {
            iId = aId;
            iUri = aUri;
        }
        
        public static string RecordType
        {
            get
            {
                return ("Mount");
            }
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
        }
        
        public Nullable<DateTime> LastScanned
        {
            get
            {
                return (iLastScanned);
            }
        }
        
        public void SetUri(string aUri)
        {
            iUri = aUri;
        }
        
        public void SetLastScanned(DateTime aLastScanned)
        {
            iLastScanned = aLastScanned;
        }

        private string iId;
        private string iUri;
        private Nullable<DateTime> iLastScanned;
    }
    
    internal class Tag : ITag
    {
        public Tag(string aNs, string aName)
        {
            iNs = aNs;
            iName = aName;
        }
        
        public static string RecordType
        {
            get
            {
                return ("Tag");
            }
        }
        
        public string Ns
        {
            get
            {
                return (iNs);
            }
        }
        
        public string Name
        {
            get
            {
                return (iName);
            }
        }

        private string iNs;
        private string iName;
    }
    
    internal class Item : IItem
    {
        public Item(Mount aMount, string aUri)
        {
            iMount = aMount;
            iUri = aUri;
        }
        
        public static string RecordType
        {
            get
            {
                return ("Item");
            }
        }
        
        public IMount Mount
        {
            get
            {
                return (iMount);
            }
        }
        
        public string Uri
        {
            get
            {
                return (iUri);
            }
        }

        public bool References(Mount aMount)
        {
            return (iMount == aMount); 
        } 

        private Mount iMount;
        private string iUri;
    }
    
    internal class Metadatum : IMetadatum
    {
        public Metadatum(Item aItem, Tag aTag, string aValue)
        {
            iItem = aItem;
            iTag = aTag;
            iValue = aValue;
        }
        
        public static string RecordType
        {
            get
            {
                return ("Metadatum");
            }
        }
        
        public IItem Item
        {
            get
            {
                return (iItem);
            }
        }
        
        public ITag Tag
        {
            get
            {
                return (iTag);
            }
        }
        
        public string Value
        {
            get
            {
                return (iValue);
            }
        }
        
        public bool References(Item aItem)
        {
            return (iItem == aItem); 
        } 

        public bool References(Tag aTag)
        {
            return (iTag == aTag); 
        } 

        internal Item iItem;
        internal Tag iTag;
        internal string iValue;
    }
    
    public class DatabaseDb4o : IDatabase
    {
        public DatabaseDb4o()
        {
            iDb = Db4oFactory.OpenFile("KinskyMedia.db4o");
        }
        
        // Mount
        
        private Mount GetMount(IMount aMount)
        {
            Mount mount = aMount as Mount;
            
            if (mount != null) {
                return (mount);
            }
            
            throw (new ItemNotFound(Mount.RecordType));
        }
        
        public IList<IMount> MountList()
        {
            IList<Mount> query = iDb.Query<Mount>(delegate(Mount aMount) {
                return (true);
            });
            
            List<IMount> list = new List<IMount>();
            
            foreach(Mount mount in query) {
                list.Add(mount);
            }

            return (list);
        }
        
        public void RemoveMount(IMount aMount)
        {
        }
        
        public IMount CreateMount(string aId, string aUri)
        {
            IList<Mount> query = iDb.Query<Mount>(delegate(Mount aMount) {
                return (aMount.Id == aId);
            });
            
            if (query.Count > 0) {
                throw (new ItemAlreadyExists(Mount.RecordType));
            }
            
            Mount mount = new Mount(aId, aUri);
            
            iDb.Store(mount);
            
            return (mount);
        }
        
        public void UpdateMountUri(IMount aMount, string aUri)
        {
            Mount mount = GetMount(aMount);
            mount.SetUri(aUri);
            iDb.Store(mount);
        }

        public void UpdateMountLastScanned(IMount aMount, DateTime aLastScanned)
        {
            Mount mount = GetMount(aMount);
            mount.SetLastScanned(aLastScanned);
            iDb.Store(mount);
        }
        
        // Tag
        
        private Tag GetTag(ITag aTag)
        {
            Tag tag = aTag as Tag;
            
            if (tag != null) {
                return (tag);
            }
            
            throw (new ItemNotFound(Tag.RecordType));
        }
        
        public IList<ITag> TagList()
        {
            IList<Tag> query = iDb.Query<Tag>(delegate(Tag aTag) {
                return (true);
            });
            
            List<ITag> list = new List<ITag>();
            
            foreach(Tag tag in query) {
                list.Add(tag);
            }

            return (list);
        }
        
        public void RemoveTag(ITag aTag)
        {
        }
        
        public ITag CreateTag(string aNs, string aName)
        {
            IList<Tag> query = iDb.Query<Tag>(delegate(Tag aTag) {
                return (aTag.Ns == aNs && aTag.Name == aName);
            });
            
            if (query.Count > 0) {
                throw (new ItemAlreadyExists(Tag.RecordType));
            }
            
            Tag tag = new Tag(aNs, aName);
            
            iDb.Store(tag);
            
            return (tag);
        }

        // Item
        
        private Item GetItem(IItem aItem)
        {
            Item item = aItem as Item;
            
            if (item != null) {
                return (item);
            }
            
            throw (new ItemNotFound(Item.RecordType));
        }
        
        public IList<IItem> ItemList(IMount aMount)
        {
            Mount mount = GetMount(aMount);
            
            IList<Item> query = iDb.Query<Item>(delegate(Item aItem) {
                return (aItem.References(mount));
            });
            
            List<IItem> list = new List<IItem>();
            
            foreach(Item item in query) {
                list.Add(item);
            }

            return (list);
        }
        
        public IList<IItem> ItemList(IList<IMetadatum> aFilter)
        {
            List<IItem> list = new List<IItem>();

            return (list);
        }
        
        public void RemoveItem(IItem aItem)
        {
        }
        
        public IItem CreateItem(IMount aMount, string aUri)
        {
            Mount mount = GetMount(aMount);
            
            IList<Item> query = iDb.Query<Item>(delegate(Item aItem) {
                return (aItem.References(mount) && aItem.Uri == aUri);
            });
            
            if (query.Count > 0) {
                throw (new ItemAlreadyExists(Item.RecordType));
            }
            
            Item item = new Item(mount, aUri);
            
            iDb.Store(item);
            
            return (item);
        }
        
        // Metadatum
        
        public IList<IMetadatum> MetadatumList(IItem aItem)
        {
            Item item = GetItem(aItem);
            
            IList<Metadatum> query = iDb.Query<Metadatum>(delegate(Metadatum aMetadatum) {
                return (aMetadatum.References(item));
            });
            
            List<IMetadatum> list = new List<IMetadatum>();
            
            foreach(Metadatum metadatum in query) {
                list.Add(metadatum);
            }

            return (list);
        }
        
        public void RemoveMetadatum(IItem aItem, IMetadatum aMetadatum)
        {
        }
        
        // Duplicates allowed on metadata

        public IMetadatum CreateMetadatum(IItem aItem, ITag aTag, string aValue)
        {
            Item item = GetItem(aItem);
            Tag tag = GetTag(aTag);
            
            Metadatum metadatum = new Metadatum(item, tag, aValue);
            
            iDb.Store(metadatum);
            
            return (metadatum);
        }
        
        private IObjectContainer iDb;
    }
}
