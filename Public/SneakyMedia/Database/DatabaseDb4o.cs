using System;
using System.Collections.Generic;
using System.Threading;

using System.Data.SqlClient;
using System.Data.SqlTypes;

using SneakyMedia.Scanner;

using Db4objects.Db4o;

namespace SneakyMedia.Database
{
    public static class Extensions
    {
        public static IEnumerable<IMetadatum> Reversed(this IList<IMetadatum> aList)
        {
            int i = aList.Count;

            while (--i >= 0)
            {
                yield return aList[i];
            }
        }
    }
    
    // Tables
    
    internal class Mount
    {
        private Mount(string aId, string aUri)
        {
            iId = aId;
            iUri = aUri;
        }
        
        public static Mount Create(IObjectContainer aDb, string aId, string aUri)
        {
            IList <Mount> query = aDb.Query<Mount>(delegate(Mount aMount) {
                return (aMount.iId == aId);
            });
            
            if (query.Count > 0) {
                throw (new MountAlreadyExistsError());
            }
            
            Mount mount = new Mount(aId, aUri);
            
            aDb.Store(mount);
            
            return (mount);
        }
        
        public static Mount Find(IObjectContainer aDb, string aId)
        {
            IList <Mount> query = aDb.Query<Mount>(delegate(Mount aMount) {
                return (aMount.iId == aId);
            });

            if (query.Count == 0) {
                throw (new MountNotFoundError());
            }
            
            Assert.Check(query.Count == 1);
            
            return (query[0]);
        }
        
        public static void UpdateUri(IObjectContainer aDb, string aId, string aUri)
        {
            Mount mount = Find(aDb, aId);
            
            mount.iUri = aUri;
            
            aDb.Store(mount);
        }

        public static void UpdateLastScanned(IObjectContainer aDb, string aId, DateTime aLastScanned)
        {
            Mount mount = Find(aDb, aId);
            
            mount.iLastScanned = aLastScanned;
            
            aDb.Store(mount);
        }
        
        public static string QueryUri(IObjectContainer aDb, string aId)
        {
            Mount mount = Find(aDb, aId);
            
            return (mount.iUri);
        }
        
        public static IList<string> QueryIdList(IObjectContainer aDb)
        {
            IList<Mount> query = aDb.Query<Mount>(delegate(Mount aMount) {
                return (true);
            });
            
            List<string> list = new List<string>();
            
            foreach(Mount mount in query) {
                list.add(mount.iId);
            }

            return (list);
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
        
        public DateTime LastScanned
        {
            get
            {
                return (iLastScanned);
            }
        }

        private string iId;
        private string iUri;
        private DateTime iLastScanned;
    }
    
    class Item
    {
        private Item(Mount aMount, string Uri)
        {
            iMount = aMount;
            iUri = aUri;
        }
        
        public static Item Create(IObjectContainer aDb, Mount aMount, string aUri)
        {
            IList <Item> query = aDb.Query<Item>(delegate(Item aItem) {
                return (aItem.iMount.iId == aMount.iId && aItem.iUri == aUri);
            });
            
            if (query.Count > 0) {
                throw (new ItemAlreadyExistsError());
            }
            
            Item item = new Item(aMount, aUri);
            
            aDb.Store(item);
            
            return (item);
        }
        
        public Mount Mount
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

        private Mount iMount;
        private string iUri;
    }
    
    class Tag
    {
        public static Tag Find(IObjectContainer aDb, string aNs, string aName)
        {
            IList <Tag> query = aDb.Query<Tag>(delegate(Tag aTag) {
                return (aTag.iNs == aNs && aTag.iName == aName);
            });

            if (query.Count == 0) {
                throw (new TagNotFoundError());
            }
            
            Assert.Check(query.Count == 1);
            
            return (query[0]);
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
    
    class Metadatum
    {
        private Metadatum(Item aItem, Tag aTag, string aValue)
        {
            iItem = aItem;
            iTag = aTag;
            iValue = aValue;
        }
        
        public static Metadatum Create(IObjectContainer aDb, Item aItem, Tag aTag, string aValue)
        {
            IList <Metadatum> query = aDb.Query<Metadatum>(delegate(Metadatum aMetadatum) {
                return (aMetadatum.iItem == aItem && aMetadatum.iTag == aTag);
            });
            
            if (query.Count > 0) {
                throw (new MetadatumAlreadyExistsError());
            }
            
            Metadatum metadatum = new Metadatum(aItem, aTag, aValue);
            
            aDb.Store(metadatum);
            
            return (metadatum);
        }
        
        public Item Item
        {
            get
            {
                return (iItem);
            }
        }
        
        public Tag Tag
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

        private Item iItem;
        private Tag iTag;
        private string iValue;
    }
    
    public class DatabaseDb4o : IDatabase
    {
        public DatabaseDb4o()
        {
            iDb = Db4oFactory.OpenFile("KinskyMedia.db4o");
        }

        public void AddMount(string aMountId, string aMountUri)
        {
            Mount.Create(aMountId, aMountUri);
        }

        public void UpdateMountUri(string aMountId, string aMountUri)
        {
            Mount.UpdateUri(iDb, aMountId, aMountUri);        
        }

        public void UpdateMountScanned(string aMountId, DateTime aLastScanned)
        {
            Mount.UpdateLastScanned(iDb, aMountId, aLastScanned);
        }

        public string QueryMountUri(string aMountId)
        {
            return (Mount.QueryUri(aMountId));
       }

        public Nullable<DateTime> QueryLastScanned(string aMountId)
        {
            return (Mount.QueryLastScanned(aMountId));
        }

        public IList<string> QueryMounts()
        {
            return (Mount.QueryIdList(iDb));
        }

        public void Add(IItem aItem, IList<IMetadatum> aMetadata)
        {
            string id = aItem.MountId;
            string uri = aItem.ItemUri;
            
            Mount mount = Mount.Find(iDb, id);
            
            Item item = Item.Create(iDb, mount, uri);
            
            foreach (IMetadatum metadatum in aMetadata)
            {
                string ns = metadatum.Tag.Ns;
                string name = metadatum.Tag.Name;
                string value = metadatum.Value;
                
                Tag tag = Tag.Find(ns, name);
                
                Metadatum = Metadatum.Create(item, tag, value);
            }
        }

        public void Remove(IItem aItemId)
        {
        }

        public void Add(IItem aItem, IMetadatum aMetadatum)
        {
        }

        public void Remove(IItem aItemId, IMetadatum aMetadatum)
        {
        }

        public IList<IMetadatum> QueryItem(IItem aItemId)
        {
            return (new List<IMetadatum>());
        }

        public IList<IItem> QueryItems(string aMountId)
        {
            return (new List<IItem>());
        }

        public IList<IList<string>> SearchItems(string aSearch, uint aRandom, IList<ITag> aShow)
        {
            List<IList<string>> result = new List<IList<string>>();
            return (result);
        }

        public IList<IList<string>> QueryItems(IList<IMetadatum> aWhere, uint aRandom, IList<ITag> aShow)
        {
            List<IList<string>> result = new List<IList<string>>();
            return (result);
        }

        private IObjectContainer iDb;
    }
}
