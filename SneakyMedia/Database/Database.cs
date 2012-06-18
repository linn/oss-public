using System;
using System.Collections.Generic;
using System.Threading;

namespace SneakyMedia.Database
{
    public class MountNotFoundError : Exception
    {
        public MountNotFoundError()
        {
        }
    }

    public class MountAlreadyExistsError : Exception
    {
        public MountAlreadyExistsError()
        {
        }
    }

    public class ItemNotFoundError : Exception
    {
        public ItemNotFoundError()
        {
        }
    }

    public class ItemAlreadyExistsError : Exception
    {
        public ItemAlreadyExistsError()
        {
        }
    }

    public class TagNotFoundError : Exception
    {
        public TagNotFoundError()
        {
        }
    }

    public class TagAlreadyExistsError : Exception
    {
        public TagAlreadyExistsError()
        {
        }
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

    public interface IItem
    {
        string MountId { get; }
        string ItemUri { get; }
    }

    public interface IMount
    {
        string MountUri { get; }
        DateTime LastScanned { get; }
    }

    public enum EWhere
    {
        eMetadatum,
        eMountId,
        eSearch,
        eRandom
    }

    public interface IDatabase
    {
        void AddMount(string aMountId, string aUri);
        void UpdateMountUri(string aMountId, string aUri);
        void UpdateMountScanned(string aMountId, DateTime aScanned);
        string QueryMountUri(string aMountId);
        Nullable<DateTime> QueryLastScanned(string aMountId);
        IList<string> QueryMounts();
        void Add(IItem aItem, IList<IMetadatum> aMetadata);
        void Remove(IItem aItem);
        void Add(IItem aItem, IMetadatum aMetadatum);
        void Remove(IItem aItem, IMetadatum aMetadatum);
        IList<IMetadatum> QueryItem(IItem aItem);
        IList<IItem> QueryItems(string aMountId);
        IList<IList<string>> QueryItems(IList<IMetadatum> aWhere, uint aRandom, IList<ITag> aShow);
        IList<IList<string>> SearchItems(string aSearch, uint aRandom, IList<ITag> aShow);
    }
}
