using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Linn.Xml
{

    // Represents the contents of a Layout file, which is an XML file conforming
    // to the schema Layout.xsd, which describes the layout of ROM sections
    // within in a set of flash devices.

    public class Tags
    {
        public enum ETagEntryType
        {
            eSigned,
            eUnsigned,
            eString,
            eBinary
        }

        public interface ITagEntry
        {
            uint Key { get; }
            string Tag { get; }
            ETagEntryType Type { get; }
            string Description { get; }
            uint Count { get; }
        }

        public static Tags Load(string aUri)
        {
            TagsXml.Tags tags;

            XmlSerializer xml = new XmlSerializer(typeof(TagsXml.Tags));

            TextReader reader = new StreamReader(aUri);

            tags = (TagsXml.Tags)xml.Deserialize(reader);

            reader.Close();

            return (new Tags(tags));
        }

        private Tags(TagsXml.Tags aTags)
        {
            iTags = aTags;
        }

        private class TagEntry : ITagEntry
        {
            public TagEntry(ITagEntry aTagEntry, uint aOffset)
            {
                iTagEntry = aTagEntry;
                iOffset = aOffset;
            }

            private ITagEntry iTagEntry;
            private uint iOffset;

            public uint Key
            {
                get
                {
                    return (iTagEntry.Key + iOffset);
                }
            }

            public string Tag
            {
                get
                {
                    return (iTagEntry.Tag);
                }
            }

            public ETagEntryType Type
            {
                get
                {
                    return (iTagEntry.Type);
                }
            }

            public string Description
            {
                get
                {
                    return (iTagEntry.Description);
                }
            }

            public uint Count
            { 
                get
                {
                    return (1);
                }
            }
        }
        
        public IList<ITagEntry> TagEntryList
        {
            get
            {
                return (iTags.TagEntryList);
            }
        }
        
        public ITagEntry Find(string aTag)
        {
            ITagEntry entry = iTags.Find(aTag);

            if (entry == null)
            {
                // Scan back for trailing number in name
                for (int i = aTag.Length; i > 0; i--)
                {
                    char c = aTag[i - 1];

                    if (c >= '0' && c <= '9')
                    {
                        continue;
                    }
                    else
                    {
                        string prefix = aTag.Substring(0, i);

                        entry = iTags.Find(prefix);

                        if (entry != null)
                        {
                            try
                            {
                                uint index = Convert.ToUInt32(aTag.Substring(i));

                                if (index < entry.Count)
                                {
                                    return (new TagEntry(entry, index));
                                }

                            }
                            catch (OverflowException)
                            {
                            }

                            return (null);
                        }
                    }
                }
            }

            return (entry);
        }

        TagsXml.Tags iTags;
    }

    namespace TagsXml
    {
        [XmlRoot("Tags")]

        public class Tags
        {
            public static Tags Load(string aUri)
            {
                Tags tags;

                XmlSerializer xml = new XmlSerializer(typeof(Tags));

                TextReader reader = new StreamReader(aUri);

                tags = (Tags)xml.Deserialize(reader);

                reader.Close();

                return (tags);
            }

            [XmlElement("Entry", typeof(TagEntry))]
            [XmlElement("Group", typeof(TagGroup))]

            public object[] Items
            {
                get
                {
                    throw (new ApplicationException());
                }
                set
                {
                    iEntryList = new SortedList<string, Linn.Xml.Tags.ITagEntry>();

                    foreach (object o in value)
                    {
                        Linn.Xml.Tags.ITagEntry e = o as Linn.Xml.Tags.ITagEntry;
                        iEntryList.Add(e.Tag, e);
                    }
                }
            }

            [XmlIgnore]

            public IList<Linn.Xml.Tags.ITagEntry> TagEntryList
            {
                get
                {
                    return (iEntryList.Values);
                }
            }

            public Linn.Xml.Tags.ITagEntry Find(string aTag)
            {
                Linn.Xml.Tags.ITagEntry entry;
                iEntryList.TryGetValue(aTag, out entry);
                return (entry);
            }

            private SortedList<string, Linn.Xml.Tags.ITagEntry> iEntryList;
        }

        public class TagEntry : Linn.Xml.Tags.ITagEntry
        {
            private string iTag;
            private Linn.Xml.Tags.ETagEntryType iType;
            private uint iKey;
            private string iDescription;

            [XmlElement("Tag")]

            public string Tag
            {
                get
                {
                    return iTag;
                }
                set
                {
                    iTag = value;
                }
            }

            [XmlElement("Type")]

            public string XmlType
            {
                get
                {
                    throw (new ApplicationException());
                }
                set
                {
                    switch (value)
                    {
                        case "TInt":
                            iType = Linn.Xml.Tags.ETagEntryType.eSigned;
                            break;
                        case "TUint":
                            iType = Linn.Xml.Tags.ETagEntryType.eUnsigned;
                            break;
                        case "String":
                            iType = Linn.Xml.Tags.ETagEntryType.eString;
                            break;
                        case "Binary":
                            iType = Linn.Xml.Tags.ETagEntryType.eBinary;
                            break;
                        default:
                            iType = Linn.Xml.Tags.ETagEntryType.eBinary;
                            break;
                    }
                }
            }

            [XmlIgnore]

            public Linn.Xml.Tags.ETagEntryType Type
            {
                get
                {
                    return (iType);
                }
            }

            [XmlElement("Key")]

            public uint Key
            {
                get
                {
                    return (iKey);
                }
                set
                {
                    iKey = value;
                }
            }

            [XmlElement("Desc")]

            public string Description
            {
                get
                {
                    return (iDescription);
                }
                set
                {
                    iDescription = value;
                }
            }

            [XmlIgnore]

            public virtual uint Count
            {
                get
                {
                    return (1);
                }
            }
        }

        public class TagGroup : TagEntry
        {
            private uint iCount;

            [XmlElement("Count")]

            public uint XmlCount
            {
                get
                {
                    throw (new ApplicationException());
                }
                set
                {
                    iCount = value;
                }
            }

            [XmlIgnore]

            public override uint Count
            {
                get
                {
                    return (iCount);
                }
            }
        }
    }
}
