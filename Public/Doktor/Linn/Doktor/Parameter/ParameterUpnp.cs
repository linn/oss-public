using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;

namespace Linn.Doktor
{

    public class ParameterUpnp : ParameterNodal
    {
        public ParameterUpnp(string aName, string aDescription)
            : base(aName, "Upnp", aDescription)
        {
        }

        protected ParameterUpnp(string aName, string aType, string aDescription)
            : base(aName, aType, aDescription)
        {
        }

        protected override IList<string> Recognise(INode aNode)
        {
            if (aNode.Type == "Upnp")
            {
                iDeviceXmlUri = new Uri(aNode.Attributes[SupplyUpnp.kAttributeLocation]);

                iUdn = aNode.Attributes[SupplyUpnp.kAttributeUuid];

                string xml = aNode.Attributes[SupplyUpnp.kAttributeDeviceXml];                

                StringReader reader = new StringReader(xml);
                
                iDeviceXml = new XmlDocument();
    
                try
                {
                    iDeviceXml.Load(reader);
                }
                catch (XmlException)
                {
                    return (null);
                }

                iNsManager = new XmlNamespaceManager(iDeviceXml.NameTable);
                iNsManager.AddNamespace("u", "urn:schemas-upnp-org:device-1-0");
                iNsManager.AddNamespace("s", "urn:schemas-upnp-org:service-1-0");

                iRoot = iDeviceXml.SelectSingleNode("/u:root/u:device", iNsManager);

                if (iRoot != null)
                {
                    iDevice = FindDevice(iRoot, iUdn);

                    if (iDevice != null)
                    {
                        XmlNode name = iDevice.SelectSingleNode("u:friendlyName", iNsManager);

                        if (name != null)
                        {
                            iFriendlyName = name.FirstChild.Value;

                            List<string> list = new List<string>();
                            
                            list.Add(iFriendlyName);
                            list.Add(iUdn);
                            
                            return (list);
                        }
                    }
                }
            }
            
            return (null);
        }

        public Uri DeviceXmlUri
        {
            get
            {
                return (iDeviceXmlUri);
            }
        }

        public XmlDocument DeviceXml
        {
            get
            {
                return (iDeviceXml);
            }
        }
        
        public XmlNamespaceManager NsManager
        {
            get
            {
                return (iNsManager);
            }
        }

        public XmlNode Root
        {
            get
            {
                return (iRoot);
            }
        }

        public XmlNode Device
        {
            get
            {
                return (iDevice);
            }
        }

        public string Udn
        {
            get
            {
                return (iUdn);
            }
        }

        public string FriendlyName
        {
            get
            {
                return (iFriendlyName);
            }
        }

        private string FindUdn(XmlNode aDevice)
        {
            const string kUuid = "uuid:";

            XmlNode node = aDevice.SelectSingleNode("u:UDN", iNsManager);

            if (node != null)
            {
                string value = node.FirstChild.Value;

                if (value.StartsWith(kUuid))
                {
                    return (value.Substring(kUuid.Length));
                }
            }

            return (null);
        }

        // Find device with the given UDN in the passsed in device or one of its embedded devices

        private XmlNode FindDevice(XmlNode aDevice, string aUdn)
        {
            if (FindUdn(aDevice) == aUdn)
            {
                return (aDevice);
            }

            XmlNodeList nodes = aDevice.SelectNodes("u:deviceList/u:device", iNsManager);

            foreach (XmlNode node in nodes)
            {
                XmlNode device = FindDevice(node, aUdn);

                if (device != null)
                {
                    return (device);
                }
            }

            return (null);
        }

        Uri iDeviceXmlUri;
        XmlDocument iDeviceXml;
        XmlNamespaceManager iNsManager;
        XmlNode iRoot;
        XmlNode iDevice;
        string iUdn;
        string iFriendlyName;
    }
}
