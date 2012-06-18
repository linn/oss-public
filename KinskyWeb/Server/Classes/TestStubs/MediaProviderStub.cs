using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Upnp;
namespace KinskyWeb.TestStubs
{
    public class MediaProviderStub
    {

        private static XElement iStubXML;

        public MediaProviderStub()
        {
            if (iStubXML == null)
            {
                iStubXML = XElement.Load(
                                    Path.Combine(
                                    Path.GetDirectoryName(
                                    Assembly.GetExecutingAssembly().Location),
                                    "TestFiles/FakeMediaServer.xml"));
            }

        }

        public container GetRoot()
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("didl", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
            namespaceManager.AddNamespace("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/");
            namespaceManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            namespaceManager.AddNamespace("ldl", "urn:linn-co-uk/DIDL-Lite");

            XElement containerElement = iStubXML.XPathSelectElement("device[@objectID='0']/metadata/didl:DIDL-Lite", namespaceManager);
            DidlLite result = new DidlLite(containerElement.ToString());
            return result[0] as container;
        }

        public string GetTitle(string containerID)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("didl", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
            namespaceManager.AddNamespace("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/");
            namespaceManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            namespaceManager.AddNamespace("ldl", "urn:linn-co-uk/DIDL-Lite");

            XElement titleElement = iStubXML.XPathSelectElement(String.Format("device[@objectID='{0}']/dc:title", containerID), namespaceManager);
            if (titleElement != null)
            {
                return titleElement.ToString();
            }
            else
            {
                return null;
            }
        }

        public DidlLite GetChildren(string containerID)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("didl", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
            namespaceManager.AddNamespace("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/");
            namespaceManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            namespaceManager.AddNamespace("ldl", "urn:linn-co-uk/DIDL-Lite");

            XElement containerElement = iStubXML.XPathSelectElement(String.Format("device[@objectID='{0}']/directChildren/didl:DIDL-Lite", containerID), namespaceManager);
            if (containerElement != null)
            {
                return new DidlLite(containerElement.ToString());
            }
            else
            {
                return new DidlLite();
            }
        }
    }
}