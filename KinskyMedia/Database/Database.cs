using System;
using System.Collections.Generic;
using System.Threading;

namespace KinskyMedia.Database
{
    public class DatabaseError : Exception
    {
        protected DatabaseError(string aMessage) : base("Database error: " + aMessage)
        {
        }
    }
    
    public class ItemNotFound : DatabaseError
    {
        public ItemNotFound(string aRecordType) : base(aRecordType + " not found")
        {
        }
    }

    public class ItemAlreadyExists : DatabaseError
    {
        public ItemAlreadyExists(string aRecordType) : base(aRecordType + " already exists")
        {
        }
    }

    public interface IMount
    {
        string Id { get; }
        string Uri { get; }
        Nullable<DateTime> LastScanned { get; }
    }

    public interface IItem
    {
        IMount Mount { get; }
        string Uri { get; }
    }

    public interface ITag
    {
        string Ns { get; }
        string Name { get; }
    }

    public interface IMetadatum
    {
        ITag Tag { get; }
        string Value { get; }
    }

    public interface IDatabase
    {
        // Mount
        
        IList<IMount> MountList();
        void RemoveMount(IMount aMount);
        IMount CreateMount(string aId, string aUri);
        void UpdateMountUri(IMount aMount, string aUri);
        void UpdateMountLastScanned(IMount aMount, DateTime aLastScanned);

        // Tag
        
        IList<ITag> TagList();
        void RemoveTag(ITag aTag);
        ITag CreateTag(string aNs, string aName);

        // Item
        
        IList<IItem> ItemList(IMount aMount);
        IList<IItem> ItemList(IList<IMetadatum> aFilter);
        void RemoveItem(IItem aItem);
        IItem CreateItem(IMount aMount, string aUri);
        
        // Metadatum
        
        IList<IMetadatum> MetadatumList(IItem aItem);
        void RemoveMetadatum(IItem aItem, IMetadatum aMetadatum);
        IMetadatum CreateMetadatum(IItem aItem, ITag aTag, string aValue);
    }
}
