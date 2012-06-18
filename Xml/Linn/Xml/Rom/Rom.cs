using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Linn.Xml
{

    // Represents the contents of a Rom file, which is an XML file conforming
    // to the schema Rom.xsd, which describes a ROM to be reflashed

    public class Rom
    {
        public interface IStoreRegion
        {
            string Tag { get; }
            string Type { get; }
            IList<IEntry> EntryList { get; }
        }

        public interface IBinaryRegion
        {
            string Tag { get; }
            string Uri { get; }
        }

        public interface IEntry
        {
            string Tag { get; }
            string Source { get; }
            string Value { get; }
        }

        public static Rom Load(string aUri)
        {
            RomXml.Rom rom = RomXml.Rom.Load(aUri);

            if (rom != null)
            {
                return (new Rom(rom));
            }

            return (null);
        }

        private Rom(RomXml.Rom aRomLoader)
        {
            iRom = aRomLoader;
        }

        public IList<IStoreRegion> StoreRegionList
        {
            get
            {
                return (iRom.StoreRegionList);
            }
        }

        public IList<IBinaryRegion> BinaryRegionList
        {
            get
            {
                return (iRom.BinaryRegionList);
            }
        }

        public string Layout
        {
            get
            {
                return (iRom.Layout);
            }
        }

        public string Tags
        {
            get
            {
                return (iRom.Tags);
            }
        }

        public bool Fallback
        {
            get
            {
                return (iRom.Fallback);
            }
        }

        public string InstallRoot
        {
            get
            {
                return (iRom.InstallRoot);
            }
        }

        public IStoreRegion FindStoreRegion(string aTag)
        {
            return (iRom.FindStoreRegion(aTag));
        }

        public IBinaryRegion FindBinaryRegion(string aTag)
        {
            return (iRom.FindBinaryRegion(aTag));
        }

        private RomXml.Rom iRom;
    }

    namespace RomXml
    {
        [XmlRoot("Rom")]

        public class Rom
        {
            private string iTags;
            private string iLayout;
            private bool iFallback;
            private string iInstallRoot;

            private SortedList<string, Linn.Xml.Rom.IBinaryRegion> iBinaryRegionList;
            private SortedList<string, Linn.Xml.Rom.IStoreRegion> iStoreRegionList;

            public static Rom Load(string aUri)
            {
                Rom rom;

                XmlSerializer xml = new XmlSerializer(typeof(Rom));

                TextReader reader = new StreamReader(aUri);

                rom = (Rom)xml.Deserialize(reader);

                reader.Close();

                return (rom);
            }

            [XmlElement("Store", typeof(StoreRegion))]
            [XmlElement("Binary", typeof(BinaryRegion))]

            public object[] Items
            {
                get
                {
                    throw (new ApplicationException());
                }
                set
                {
                    iBinaryRegionList = new SortedList<string, Linn.Xml.Rom.IBinaryRegion>();
                    iStoreRegionList = new SortedList<string, Linn.Xml.Rom.IStoreRegion>();

                    foreach (object o in value)
                    {
                        if (o is StoreRegion)
                        {
                            StoreRegion region = o as StoreRegion;
                            iStoreRegionList.Add(region.Tag, region);
                        }
                        if (o is BinaryRegion)
                        {
                            BinaryRegion region = o as BinaryRegion;
                            iBinaryRegionList.Add(region.Tag, region);
                        }
                    }
                }
            }

            [XmlIgnore]

            public IList<Linn.Xml.Rom.IStoreRegion> StoreRegionList
            {
                get
                {
                    return iStoreRegionList.Values;
                }
            }

            [XmlIgnore]

            public IList<Linn.Xml.Rom.IBinaryRegion> BinaryRegionList
            {
                get
                {
                    return (iBinaryRegionList.Values);
                }
            }

            public Linn.Xml.Rom.IStoreRegion FindStoreRegion(string aTag)
            {
                Linn.Xml.Rom.IStoreRegion region;
                iStoreRegionList.TryGetValue(aTag, out region);
                return (region);
            }

            public Linn.Xml.Rom.IBinaryRegion FindBinaryRegion(string aTag)
            {
                Linn.Xml.Rom.IBinaryRegion region;
                iBinaryRegionList.TryGetValue(aTag, out region);
                return (region);
            }

            [XmlAttribute("layout")]

            public string Layout
            {
                get
                {
                    return (iLayout);
                }
                set
                {
                    iLayout = value;
                }
            }

            [XmlAttribute("tags")]

            public string Tags
            {
                get
                {
                    return (iTags);
                }
                set
                {
                    iTags = value;
                }
            }

            [XmlAttribute("fallback")]

            public bool Fallback
            {
                get
                {
                    return iFallback;
                }
                set
                {
                    iFallback = value;
                }
            }

            [XmlAttribute("installroot")]

            public string InstallRoot
            {
                get
                {
                    return this.iInstallRoot;
                }
                set
                {
                    iInstallRoot = value;
                }
            }

            public class BinaryRegion : Linn.Xml.Rom.IBinaryRegion
            {
                private string iTag;

                private BinaryRegionValue iBinaryRegionValue;

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

                [XmlElement("Value")]

                public BinaryRegionValue BinaryRegionValue
                {
                    get
                    {
                        throw (new ApplicationException());
                    }
                    set
                    {
                        iBinaryRegionValue = value;
                    }
                }

                [XmlIgnore]

                public string Uri
                {
                    get
                    {
                        return (iBinaryRegionValue.Value);
                    }
                }
            }

            public class BinaryRegionValue
            {
                private string iValue;

                [XmlAttribute("source")]

                public string Source
                {
                    get
                    {
                        throw (new ApplicationException());
                    }
                    set
                    {
                        if(value != "file")
                            throw (new ApplicationException());
                    }
                }

                [XmlText]

                public string Value
                {
                    get
                    {
                        return iValue;
                    }
                    set
                    {
                        iValue = value;
                    }
                }
            }

            public class StoreRegion : Linn.Xml.Rom.IStoreRegion
            {
                private string iTag;
                private string iType;
                private StoreRegionValue iStoreRegionValue;

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

                public string Type
                {
                    get
                    {
                        return this.iType;
                    }
                    set
                    {
                        this.iType = value;
                    }
                }

                [XmlElement("Value")]

                public StoreRegionValue StoreRegionValue
                {
                    get
                    {
                        return iStoreRegionValue;
                    }
                    set
                    {
                        iStoreRegionValue = value;
                    }
                }

                [XmlIgnore]

                public IList<Linn.Xml.Rom.IEntry> EntryList
                {
                    get
                    {
                        return (iStoreRegionValue.Store.EntryList);
                    }
                }
            }

            public class StoreRegionValue
            {
                private Store iStore;

                [XmlAttribute("source")]

                public string Source
                {
                    get
                    {
                        throw (new ApplicationException());
                    }
                    set
                    {
                        if(value != "inline")
                            throw (new ApplicationException());
                    }
                }

                [XmlElement("Store")]

                public Store Store
                {
                    get
                    {
                        return iStore;
                    }
                    set
                    {
                        iStore = value;
                    }
                }
            }

            public class Store
            {
                private List<Linn.Xml.Rom.IEntry> iEntryList;

                [XmlElement("Entry")]

                public Entry[] Items
                {
                    get
                    {
                        throw (new ApplicationException());
                    }
                    set
                    {
                        iEntryList = new List<Linn.Xml.Rom.IEntry>();

                        foreach (Entry e in value)
                        {
                            iEntryList.Add(e);
                        }
                    }
                }

                [XmlIgnore]

                public IList<Linn.Xml.Rom.IEntry> EntryList
                {
                    get
                    {
                        return (iEntryList.AsReadOnly());
                    }
                }
            }

            public class Entry : Linn.Xml.Rom.IEntry
            {
                private string iTag;

                private EntryValue iEntryValue;

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

                [XmlElement("Value")]

                public EntryValue EntryValue
                {
                    get
                    {
                        throw (new ApplicationException());
                    }
                    set
                    {
                        iEntryValue = value;
                    }
                }

                [XmlIgnore]

                public string Source
                {
                    get
                    {
                        return (iEntryValue.Source);
                    }
                }

                [XmlIgnore]

                public string Value
                {
                    get
                    {
                        return (iEntryValue.Value);
                    }
                }
            }

            public class EntryValue
            {
                private string iSource;
                private string iValue;

                [XmlAttribute("source")]

                public string Source
                {
                    get
                    {
                        if (iSource == null)
                        {
                            return ("inline");
                        }
                        return iSource;
                    }
                    set
                    {
                        iSource = value;
                    }
                }

                [XmlText]

                public string Value
                {
                    get
                    {
                        if (iValue == null)
                        {
                            return (String.Empty);
                        }
                        return iValue;
                    }
                    set
                    {
                        iValue = value;
                    }
                }
            }
        }
    }
}
