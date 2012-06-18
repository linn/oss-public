using System;
using System.Collections.Generic;

using SneakyMedia.Database;

namespace SneakyMedia.Scanner
{
    public class Tag : ITag
    {
        protected Tag(string aNs, string aName)
        {
            iNs = aNs;
            iName = aName;
        }

        public static bool IsSearchable(ITag aTag)
        {
            if (aTag.Ns != HypertagAv.kNs)
            {
                return (false);
            }

            if (aTag.Name == HypertagAv.kNameAlbum)
            {
                return (true);
            }

            if (aTag.Name == HypertagAv.kNameArtist)
            {
                return (true);
            }

            if (aTag.Name == HypertagAv.kNameTitle)
            {
                return (true);
            }

            return (false);
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

    public class Hypertag : Tag
    {
        public Hypertag(string aNs, string aName)
            : base(aNs, aName)
        {
        }
    }

    public class HypertagAv : Hypertag
    {
        public const string kNs = "av";

        public const string kNameTitle = "Title";
        public const string kNameVersion = "Version";
        public const string kNameAlbum = "Album";
        public const string kNameTrackNumber = "TrackNumber";
        public const string kNameArtist = "Artist";
        public const string kNamePerformer = "Performer";
        public const string kNameCopyright = "Copyright";
        public const string kNameLicense = "License";
        public const string kNameOrganisation = "Organisation";
        public const string kNameDescription = "Description";
        public const string kNameGenre = "Genre";
        public const string kNameDate = "Date";
        public const string kNameLocation = "Location";
        public const string kNameContact = "Contact";
        public const string kNameIsrc = "Isrc";

        public const string kNameBitsPerSample = "BitsPerSample";
        public const string kNameChannels = "Channels";
        public const string kNameSamples = "Samples";
        public const string kNameSampleRate = "SampleRate";

        public const string kNameBeatsPerMinute = "BeatsPerMinute";
        public const string kNameComment = "Comment";
        public const string kNameComposer = "Composer";
        public const string kNameConductor = "Conductor";
        public const string kNameDiscNumber = "DiscNumber";
        public const string kNameDiscCount = "DiscCount";
        public const string kNameGrouping = "Grouping";
        public const string kNameLyrics = "Lyrics";
        public const string kNameTrackCount = "TrackCount";

        public HypertagAv(string aName)
            : base(kNs, aName)
        {
        }
    }

    public class HypertagText : Hypertag
    {
        public const string kNs = "text";

        public HypertagText(string aName)
            : base(kNs, aName)
        {
        }
    }

    public class HypertagUser : Hypertag
    {
        public const string kNs = "user";

        public HypertagUser(string aName)
            : base(kNs, aName)
        {
        }
    }

    public class TagLegacy : Tag
    {
        public TagLegacy(string aNs, string aName)
            : base(aNs, aName)
        {
        }
    }

    public class TagGeneral : ITag
    {
        public TagGeneral(string aQualifiedName)
        {
            string[] split = aQualifiedName.Split(new char[] { ':' });

            if (split.Length == 1)
            {
                iNs = HypertagAv.kNs;
                iName = aQualifiedName;
            }
            else
            {
                iName = split[split.Length - 1];
                iNs = aQualifiedName.Substring(0, aQualifiedName.Length - iName.Length - 1);
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
}

