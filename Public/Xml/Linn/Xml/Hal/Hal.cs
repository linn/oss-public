using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Linn.Xml
{
    namespace HalXml
    {
        [XmlRoot("hal")]
        public class Hal
        {
            private string iVersion;
            private Entity[] iEntities;
            
            static public Hal Load(string aUri) {
            
                XmlSerializer xml = new XmlSerializer(typeof(Hal));
                TextReader reader = new StreamReader(aUri);
                Hal hal = (Hal) xml.Deserialize(reader);
                reader.Close();
                return hal;
            }
            
            static public void Show(Hal aHal) {

                XmlSerializer xml = new XmlSerializer(typeof(Hal));
                xml.Serialize(Console.Out,aHal);
            }
            
            [XmlElement("entity")]
            public Entity[] Entities {
                get {
                    return iEntities;
                }
                set {
                    iEntities = value;
                }
            }
            
            [XmlAttribute("version")]
            public string Version {
                get {
                    return iVersion;
                }
                set {
                    iVersion = value;
                }
            }
            
            public class Entity {
                
                private string iInterface;
                private string iIndex;
                private string iImplementation;
                private string iArguments;
                private string iInclude;
                
                [XmlAttribute("interface")]
                public string Interface {
                    get {
                        return iInterface;
                    }
                    set {
                        iInterface = value;
                    }
                }
                
                [XmlAttribute("index")]
                public string Index {
                    get {
                        return iIndex;
                    }
                    set {
                        iIndex = value;
                    }
                }
                
                [XmlAttribute("implementation")]
                public string Implementation {
                    get {
                        return iImplementation;
                    }
                    set {
                        iImplementation = value;
                    }
                }
                
                [XmlAttribute("arguments")]
                public string Arguments {
                    get {
                        return iArguments;
                    }
                    set {
                        iArguments = value;
                    }
                }
                
                [XmlAttribute("include")]
                public string Include {
                    get {
                        return iInclude;
                    }
                    set {
                        iInclude = value;
                    }
                }
            }
        }
    }
}
