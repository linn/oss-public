using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Linn.Xml
{

    // Represents the contents of a RomCollection file, which is an XML file conforming
    // to the schema RomCollection.xsd, which describes the Bundle of ROM sections
    // within in a set of flash devices.

    public class RomCollection
    {
        public static RomCollection Load(string aUri)
        {
            RomCollectionXml.RomCollection collection = RomCollectionXml.RomCollection.Load(aUri);

            if (collection != null)
            {
                return (new RomCollection(collection));
            }

            return (null);
        }

        private RomCollection(RomCollectionXml.RomCollection aBundle)
        {
            iRomCollection = aBundle;
        }

        public string Main
        {
            get
            {
                return (iRomCollection.Main);
            }
        }

        public string Fallback
        {
            get
            {
                return (iRomCollection.Fallback);
            }
        }

        RomCollectionXml.RomCollection iRomCollection;
    }

    namespace RomCollectionXml
    {
        [XmlRoot("RomCollection")]

        public class RomCollection
        {
            public static RomCollection Load(string aUri)
            {
                RomCollection collection;

                XmlSerializer xml = new XmlSerializer(typeof(RomCollection));

                TextReader reader = new StreamReader(aUri);

                collection = (RomCollection)xml.Deserialize(reader);

                reader.Close();

                return (collection);
            }

            [XmlElement("Main")]

            public string Main
            {
                get
                {
                    return (iMain);
                }
                set
                {
                    iMain = value;
                }
            }

            [XmlElement("Fallback")]

            public string Fallback
            {
                get
                {
                    return (iFallback);
                }
                set
                {
                    iFallback = value;
                }
            }

            private string iMain;
            private string iFallback;
        }
    }
}
