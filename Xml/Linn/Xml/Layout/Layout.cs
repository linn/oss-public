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

    public class Layout
    {
        public interface ISection
        {
            string Tag { get; }
            string Flash { get; }
            uint Offset { get; }
            uint Bytes { get; }
        }

        public static Layout Load(string aUri)
        {
            LayoutXml.Layout layout = LayoutXml.Layout.Load(aUri);

            if (layout != null)
            {
                return (new Layout(layout));
            }

            return (null);
        }

        private Layout(LayoutXml.Layout aLayout)
        {
            iLayout = aLayout;
        }

        public IList<ISection> SectionList
        {
            get
            {
                return (iLayout.SectionList);
            }
        }

        public ISection Find(string aTag)
        {
            return (iLayout.Find(aTag));
        }

        LayoutXml.Layout iLayout;
    }

    namespace LayoutXml
    {
        [XmlRoot("Layout")]

        public class Layout
        {
            public static Layout Load(string aUri)
            {
                Layout layout;

                XmlSerializer xml = new XmlSerializer(typeof(Layout));

                TextReader reader = new StreamReader(aUri);

                layout = (Layout)xml.Deserialize(reader);

                reader.Close();

                return (layout);
            }

            [XmlElement("Section")]

            public Section[] Items
            {
                get
                {
                    throw (new ApplicationException());
                }
                set
                {
                    iSectionList = new SortedList<string, Linn.Xml.Layout.ISection>();

                    foreach (Section s in value)
                    {
                        iSectionList.Add(s.Tag, s);
                    }
                }
            }

            [XmlIgnore]

            public IList<Linn.Xml.Layout.ISection> SectionList
            {
                get
                {
                    return (iSectionList.Values);
                }
            }

            public Linn.Xml.Layout.ISection Find(string aTag)
            {
                Linn.Xml.Layout.ISection section;
                iSectionList.TryGetValue(aTag, out section);
                return (section);
            }

            private SortedList<string, Linn.Xml.Layout.ISection> iSectionList;
        }

        public class Section : Linn.Xml.Layout.ISection
        {
            private string iTag;
            private string iFlash;
            private uint iOffset;
            private uint iBytes;

            [XmlAttribute("tag")]

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

            [XmlElement("Flash")]

            public string Flash
            {
                get
                {
                    return iFlash;
                }
                set
                {
                    iFlash = value;
                }
            }

            [XmlElement("Offset")]

            public string XmlOffset
            {
                get
                {
                    throw (new ApplicationException());
                }
                set
                {
                    if (value.StartsWith("0x"))
                    {
                        iOffset = Convert.ToUInt32(value.Substring(2), 16);
                    }
                    else
                    {
                        throw (new FormatException("Offset not in hex"));
                    }
                }
            }

            [XmlIgnore]

            public uint Offset
            {
                get
                {
                    return iOffset;
                }
                set
                {
                    iOffset = value;
                }
            }

            [XmlElement("Bytes")]

            public string XmlBytes
            {
                get
                {
                    throw (new ApplicationException());
                }
                set
                {
                    if (value.StartsWith("0x"))
                    {
                        iBytes = Convert.ToUInt32(value.Substring(2), 16);
                    }
                    else
                    {
                        throw (new FormatException("Bytes not in hex"));
                    }
                }
            }

            [XmlIgnore]

            public uint Bytes
            {
                get
                {
                    return iBytes;
                }
                set
                {
                    iBytes = value;
                }
            }
        }
    }
}
