using System;
using System.Collections.Generic;

using KinskyMedia.Database;

namespace KinskyMedia.Scanner
{
    public class TagAv
    {
        public const string Ns = "av";

        public const string NameTitle = "Title";
        public const string NameVersion = "Version";
        public const string NameAlbum = "Album";
        public const string NameTrackNumber = "TrackNumber";
        public const string NameTrackCount = "TrackCount";
        public const string NameArtist = "Artist";
        public const string NamePerformer = "Performer";
        public const string NameCopyright = "Copyright";
        public const string NameLicense = "License";
        public const string NameOrganisation = "Organisation";
        public const string NameDescription = "Description";
        public const string NameGenre = "Genre";
        public const string NameDate = "Date";
        public const string NameLocation = "Location";
        public const string NameContact = "Contact";
        public const string NameIsrc = "Isrc";

        public const string NameBitsPerSample = "BitsPerSample";
        public const string NameChannels = "Channels";
        public const string NameSamples = "Samples";
        public const string NameSampleRate = "SampleRate";

        public const string NameBeatsPerMinute = "BeatsPerMinute";
        public const string NameComment = "Comment";
        public const string NameComposer = "Composer";
        public const string NameConductor = "Conductor";
        public const string NameDiscNumber = "DiscNumber";
        public const string NameDiscCount = "DiscCount";
        public const string NameGrouping = "Grouping";
        public const string NameLyrics = "Lyrics";
    }

    public class TagText
    {
        public const string kNs = "text";
    }

    public class TagUser
    {
        public const string kNs = "user";
    }

    public class Metadatum : IMetadatum
    {
        public Metadatum(ITag aTag, string aValue)
        {
            iTag = aTag;
            iValue = aValue;
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

        private ITag iTag;
        private string iValue;
    }

    public interface ITagManager
    {
        ITag Find(string aNs, string aName);
        ITag Create(string aNs, string aName);
    }

    public class TagManager : ITagManager
    {
        public TagManager(IDatabase aDatabase)
        {
            iDatabase = aDatabase;
            iTags = new List<ITag>(iDatabase.TagList());
        }

        public ITag Find(string aNs, string aName)
        {
            foreach (ITag tag in iTags)
            {
                if (tag.Ns == aNs && tag.Name == aName)
                {
                    return (tag);
                }
            }

            return (null);
        }

        public ITag Create(string aNs, string aName)
        {
            ITag tag = Find(aNs, aName);

            if (tag == null)
            {
                tag = iDatabase.CreateTag(aNs, aName);
                iTags.Add(tag);
            }

            return (tag);
        }

        private IDatabase iDatabase;
        List<ITag> iTags;
    }
}

