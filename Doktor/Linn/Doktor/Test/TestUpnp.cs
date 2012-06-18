using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;

namespace Linn.Doktor
{
    public class TestUpnp : Test
    {
        public TestUpnp()
            : base("Upnp", "Upnp", "General UPnP Device Test")
        {
            iUnit = new ParameterUpnp("Unit", "The UPnP device to test");
            
            Add(iUnit);

            iSpecificationUda10 = new Specification("UPnP Forum", "UPnP Device Architecture 1.0");
            iSpecificationUda11 = new Specification("UPnP Forum", "UPnP Device Architecture 1.1");
        }
        
        protected override void Execute()
        {
            iBaseUri = iUnit.DeviceXmlUri;

            TestDevice();

            XmlNode device = iUnit.Device;

            if (device != null)
            {
                iServiceNo = 1;
                iServiceIdList = new List<string>();
                iEventUrlList = new List<string>();

                XmlNodeList nodes = device.SelectNodes("u:serviceList/u:service", iUnit.NsManager);

                foreach (XmlNode node in nodes)
                {
                    TestService(node);
                    iServiceNo++;
                }
            }
        }

        private void TestDevice()
        {
            // Case

            if (iUnit.DeviceXml == null)
            {
                Reference(iSpecificationUda10.Reference(25, 30));
                Reference(iSpecificationUda11.Reference(43, 48));
                High("Device description cannot be recognised as valid xml.");
                return;
            }

            // Case

            string name = iUnit.DeviceXml.DocumentElement.Name;

            if (name != "root")
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(44));
                Context("Element", name);
                High("Device xml root element should be 'root'.");
                return;
            }

            // Case

            string ns = iUnit.DeviceXml.DocumentElement.NamespaceURI;

            if (ns != "urn:schemas-upnp-org:device-1-0")
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(44));
                Context("Namespace", ns);
                High("Device xml root element must be in the 'urn:schemas-upnp-org:device-1-0' namespace.");
                return;
            }

            // Case

            XmlNode node = iUnit.DeviceXml.SelectSingleNode("/u:root/u:specVersion", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(44));
                High("Device xml does not contain the mandatory <specVersion> element.");
            }
            else
            {
                // Case

                string spec = null;

                node = iUnit.DeviceXml.SelectSingleNode("/u:root/u:specVersion/u:major", iUnit.NsManager);

                if (node != null)
                {
                    string major = Value(node);

                    node = iUnit.DeviceXml.SelectSingleNode("/u:root/u:specVersion/u:minor", iUnit.NsManager);

                    if (node != null)
                    {
                        string minor = Value(node);

                        spec = major + "." + minor;
                    }
                }

                if (spec == null)
                {
                    Reference(iSpecificationUda10.Reference(27));
                    Reference(iSpecificationUda11.Reference(44));
                    High("Device xml does not specify the UPnP specification version using <major> and <minor> xml elements.");
                }
                else
                {
                    // Case

                    if (spec != "1.0" && spec != "1.1")
                    {
                        Reference(iSpecificationUda10.Reference(27));
                        Reference(iSpecificationUda11.Reference(44));
                        Context("Version", spec);
                        Medium("Device xml does not conform to UPnP specification version 1.0, or 1.1.");
                    }
                }
            }

            // Case

            XmlNodeList nodes = iUnit.DeviceXml.SelectNodes("/u:root/u:device", iUnit.NsManager);

            if (nodes.Count < 1)
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(44));
                High("Device xml <root> element does not contain the mandatory <device> element.");
                return;
            }

            if (nodes.Count > 1)
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(44));
                Context("DeviceCount", nodes.Count.ToString());
                Medium("Device xml <root> element must contain one and only one <device> element.");
            }

            // Case

            if (iUnit.Device == null)
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(44));
                Context("Udn", iUnit.Udn);
                High("Device xml does not contain a <device> element for this device's UDN.");
                return;
            }

            // Case

            node = iUnit.Device.SelectSingleNode("u:deviceType", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(44));
                High("Device does not contain the mandatory <deviceType> element.");
            }
            else
            {
                // Case

                string type = Value(node);

                string[] parts = type.Split(new char[] { ':' });

                uint ver;

                if (parts.Length < 5 || parts[0] != "urn" || parts[2] != "device" || !uint.TryParse(parts[4], out ver))
                {
                    Reference(iSpecificationUda10.Reference(27));
                    Reference(iSpecificationUda11.Reference(44));
                    Context("Type", type);
                    High("Device type must be of the form 'urn:DOMAIN-NAME:device:DEVICE-TYPE:VER'.");
                }
                else
                {
                    // Case

                    if (parts[1].Contains("."))
                    {
                        Reference(iSpecificationUda10.Reference(27));
                        Reference(iSpecificationUda11.Reference(44));
                        Context("Type", type);
                        Low("Device type domain must have periods replaced with hyphens in accordance with RFC 2141.");
                    }

                    // Case

                    if (parts[3].Length > 64)
                    {
                        Reference(iSpecificationUda10.Reference(27));
                        Reference(iSpecificationUda11.Reference(44));
                        Context("Type", type);
                        Low("Device type suffix must be less than or equal to 64 characters in length, not counting the version suffix and separating colon.");
                    }
                }
            }

            // Case

            node = iUnit.Device.SelectSingleNode("u:friendlyName", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(45));
                High("Device does not contain the mandatory <friendlyName> element.");
            }
            else
            {
                // Case

                string friendly = Value(node);

                if (friendly.Length > 64)
                {
                    Reference(iSpecificationUda10.Reference(27));
                    Reference(iSpecificationUda11.Reference(45));
                    Context("FriendlyName", friendly);
                    Low("Friendly name must be less than or equal to 64 characters in length.");
                }
            }

            // Case

            node = iUnit.Device.SelectSingleNode("u:manufacturer", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(45));
                Medium("Device does not contain the mandatory <manufacturer> element.");
            }
            else
            {
                // Case

                string manufacturer = Value(node);

                if (manufacturer.Length > 64)
                {
                    Reference(iSpecificationUda10.Reference(27));
                    Reference(iSpecificationUda11.Reference(45));
                    Context("Manufacturer", manufacturer);
                    Low("Manufacturer must be less than or equal to 64 characters in length.");
                }
            }

            // Case

            node = iUnit.Device.SelectSingleNode("u:modelName", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(45));
                Medium("Device does not contain the mandatory <modelName> element.");
            }
            else
            {
                // Case

                string model = Value(node);

                if (model.Length > 32)
                {
                    Reference(iSpecificationUda10.Reference(27));
                    Reference(iSpecificationUda11.Reference(45));
                    Context("Model", model);
                    Low("Model name must be less than or equal to 32 characters in length.");
                }
            }

            // Case

            node = iUnit.Device.SelectSingleNode("u:modelDescription", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(45));
                Low("Device does not contain the recommended <modelDescription> element.");
            }
            else
            {
                // Case

                string modeldesc = Value(node);

                if (modeldesc.Length > 128)
                {
                    Reference(iSpecificationUda10.Reference(27));
                    Reference(iSpecificationUda11.Reference(45));
                    Context("ModelDescription", modeldesc);
                    Low("Model description must be less than or equal to 128 characters in length.");
                }
            }

            // Case

            node = iUnit.Device.SelectSingleNode("u:modelNumber", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(27));
                Reference(iSpecificationUda11.Reference(45));
                Low("Device does not contain the recommended <modelNumber> element.");
            }
            else
            {
                // Case

                string modelno = Value(node);

                if (modelno.Length > 32)
                {
                    Reference(iSpecificationUda10.Reference(27));
                    Reference(iSpecificationUda11.Reference(45));
                    Context("ModelNumber", modelno);
                    Low("Model number must be less than or equal to 32 characters in length.");
                }
            }

            // Case

            node = iUnit.Device.SelectSingleNode("u:serialNumber", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(28));
                Reference(iSpecificationUda11.Reference(45));
                Low("Device does not contain the recommended <serialNumber> element.");
            }
            else
            {
                // Case

                string serialno = Value(node);

                if (serialno.Length > 64)
                {
                    Reference(iSpecificationUda10.Reference(28));
                    Reference(iSpecificationUda11.Reference(45));
                    Context("SerialNumber", serialno);
                    Low("Serial number must be less than or equal to 64 characters in length.");
                }
            }

            // Case

            node = iUnit.Device.SelectSingleNode("u:presentationURL", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(29));
                Reference(iSpecificationUda11.Reference(47));
                Low("Device does not contain the recommended <presentationURL> element.");
            }
            else
            {
                // Case

                string url = Value(node);

                if (RelativeUri(url) == null)
                {
                    Reference(iSpecificationUda10.Reference(29));
                    Reference(iSpecificationUda11.Reference(47));
                    Context("PresentationUrl", url);
                    Medium("Presentation url must be a valid URL.");
                }
            }
        }

        private void TestService(XmlNode aNode)
        {
            iServiceId = "UNKNOWN";
            iServiceType = "UNKNOWN";

            // Case

            XmlNode node = aNode.SelectSingleNode("u:serviceType", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(28));
                Reference(iSpecificationUda11.Reference(46));
                Context("Service", iServiceNo);
                High("Service description does not contain the mandatory <serviceType> element.");
            }
            else
            {
                // Cass

                iServiceType = Value(node);

                string[] parts = iServiceType.Split(new char[] { ':' });

                uint ver;

                if (parts.Length < 5 || parts[0] != "urn" || parts[2] != "service" || !uint.TryParse(parts[4], out ver))
                {
                    Reference(iSpecificationUda10.Reference(28));
                    Reference(iSpecificationUda11.Reference(46));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    High("Service type must be of the form 'urn:DOMAIN-NAME:service:SERVICE-TYPE:VER'.");
                }
                else
                {
                    // Case

                    if (parts[1].Contains("."))
                    {
                        Reference(iSpecificationUda10.Reference(28));
                        Reference(iSpecificationUda11.Reference(46));
                        Context("Service", iServiceNo);
                        Context("ServiceType", iServiceType);
                        Low("Service type domain must have periods replaced with hyphens in accordance with RFC 2141.");
                    }

                    // Case

                    if (parts[3].Length > 64)
                    {
                        Reference(iSpecificationUda10.Reference(28));
                        Reference(iSpecificationUda11.Reference(46));
                        Context("Service", iServiceNo);
                        Context("ServiceType", iServiceType);
                        Low("Service type suffix must be less than or equal to 64 characters in length, not counting the version suffix and separating colon.");
                    }
                }
            }

            // Case

            node = aNode.SelectSingleNode("u:serviceId", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(29));
                Reference(iSpecificationUda11.Reference(46));
                Context("Service", iServiceNo);
                High("Service does not contain the mandatory <serviceId> element.");
            }
            else
            {
                // Case

                iServiceId = Value(node);

                string[] parts = iServiceId.Split(new char[] { ':' });

                if (parts.Length < 4 || parts[0] != "urn" || parts[2] != "serviceId")
                {
                    Reference(iSpecificationUda10.Reference(29));
                    Reference(iSpecificationUda11.Reference(46));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    Context("Id", iServiceId);
                    High("Service id must be of the form 'urn:DOMAIN-NAME:serviceId:SERVICE-ID'.");
                }
                else
                {
                    // Case

                    if (parts[1].Contains("."))
                    {
                        Reference(iSpecificationUda10.Reference(29));
                        Reference(iSpecificationUda11.Reference(46));
                        Context("Service", iServiceNo);
                        Context("ServiceType", iServiceType);
                        Context("Id", iServiceId);
                        Low("Service id domain must have periods replaced with hyphens in accordance with RFC 2141.");
                    }

                    // Case

                    if (parts[3].Length > 64)
                    {
                        Reference(iSpecificationUda10.Reference(29));
                        Reference(iSpecificationUda11.Reference(46));
                        Context("Service", iServiceNo);
                        Context("ServiceType", iServiceType);
                        Context("Id", iServiceId);
                        Low("Service id suffix must be less than or equal to 64 characters in length, not counting the version suffix and separating colon.");
                    }

                    // Case

                    if (iServiceIdList.Contains(iServiceId))
                    {
                        Reference(iSpecificationUda10.Reference(29));
                        Reference(iSpecificationUda11.Reference(46));
                        Context("Service", iServiceNo);
                        Context("ServiceType", iServiceType);
                        Context("Id", iServiceId);
                        Medium("Service id is not unique within this device.");
                    }
                    else
                    {
                        iServiceIdList.Add(iServiceId);
                    }
                }
            }

            // Case

            node = aNode.SelectSingleNode("u:controlURL", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(29));
                Reference(iSpecificationUda11.Reference(46));
                Context("Service", iServiceNo);
                High("Service does not contain the mandatory <controlURL> element.");
            }
            else
            {
                // Case

                string url = Value(node);

                if (RelativeUri(url) == null)
                {
                    Context("Service", iServiceNo);
                    Context("ServiceControlUrl", url);
                    High("Service control url must be a valid URL.");
                }
            }

            // Case

            iEventUrl = String.Empty;

            node = aNode.SelectSingleNode("u:eventSubURL", iUnit.NsManager);

            if (node == null)
            {
                Context("Service", iServiceNo);
                Reference(iSpecificationUda10.Reference(29));
                Reference(iSpecificationUda11.Reference(46));
                High("Service does not contain the mandatory <eventSubURL> element.");
            }
            else
            {
                // Case

                iEventUrl = Value(node);

                if (iEventUrl.Length > 0)
                {
                    Uri eventuri = RelativeUri(iEventUrl);

                    if (eventuri == null)
                    {
                        Reference(iSpecificationUda10.Reference(29));
                        Reference(iSpecificationUda11.Reference(46));
                        Context("Service", iServiceNo);
                        Context("EventSubscriptionUrl", iEventUrl);
                        High("Event subscription url is not a valid URL.");
                    }
                    else
                    {
                        // Case

                        string absolute = eventuri.AbsolutePath;

                        if (iEventUrlList.Contains(absolute))
                        {
                            Reference(iSpecificationUda10.Reference(29));
                            Reference(iSpecificationUda11.Reference(46));
                            Context("Service", iServiceNo);
                            Context("EventSubscriptionUrl", iEventUrl);
                            Medium("Event subscription url is not unique for this device.");
                        }
                        else
                        {
                            iEventUrlList.Add(absolute);
                        }
                    }
                }
            }

            // Case

            node = aNode.SelectSingleNode("u:SCPDURL", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(29));
                Reference(iSpecificationUda11.Reference(46));
                Context("Service", iServiceNo);
                High("Service does not contain the mandatory <SCPDURL> element.");
            }
            else
            {
                // Case

                string url = Value(node);

                Uri scpd = RelativeUri(url);

                if (scpd == null)
                {
                    Reference(iSpecificationUda10.Reference(29));
                    Reference(iSpecificationUda11.Reference(46));
                    Context("Service", iServiceNo);
                    Context("ServiceDescriptionUrl", url);
                    High("Service description url must be a valid URL.");
                }
                else
                {
                    TestServiceXml(scpd);
                }
            }
        }

        private class StateVariable
        {
            public StateVariable(string aName, string aType, bool aEvented)
            {
                iName = aName;
                iType = aType;
                iEvented = aEvented;
            }

            public string Name
            {
                get
                {
                    return (iName);
                }
            }
            public string Type
            {
                get
                {
                    return (iType);
                }
            }

            public bool Evented
            {
                get
                {
                    return (iEvented);
                }
            }

            string iName;
            string iType;
            bool iEvented;
        }

        private class Argument
        {
            public Argument(string aName, bool aIn, bool aReturnValue)
            {
                iName = aName;
                iIn = aIn;
                iReturnValue = aReturnValue;
            }

            public string Name
            {
                get
                {
                    return (iName);
                }
            }
            public bool In
            {
                get
                {
                    return (iIn);
                }
            }

            public bool ReturnValue
            {
                get
                {
                    return (iReturnValue);
                }
            }

            string iName;
            bool iIn;
            bool iReturnValue;
        }

        private void TestServiceXml(Uri aUri)
        {
            string xml = null;

            // Case

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(aUri);

            // Use a default WebProxy because this is on LAN, not Internet

            request.Proxy = new WebProxy();

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());

                xml = reader.ReadToEnd();
            }
            catch (Exception)
            {
                Reference(iSpecificationUda10.Reference(30, 36));
                Reference(iSpecificationUda11.Reference(48, 59));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("ServiceDescriptionUrl", aUri.AbsolutePath);
                High("Unable to read the service description.");
                return;
            }

            // Case

            XmlDocument document = new XmlDocument();

            try
            {
                document.Load(new StringReader(xml));
            }
            catch (XmlException)
            {
                Reference(iSpecificationUda10.Reference(30, 36));
                Reference(iSpecificationUda11.Reference(48, 59));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("ServiceDescriptionUrl", aUri.AbsolutePath);
                High("Service description not recognisable as a valiod xml document (UDA 1.0 p31, UDA 1.1, p50).");
                return;
            }

            // Case

            string name = document.DocumentElement.Name;

            if (name != "scpd")
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(50));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Element", name);
                High("Service description root element should be 'scpd'.");
                return;
            }

            // Case

            string ns = document.DocumentElement.NamespaceURI;

            if (ns != "urn:schemas-upnp-org:service-1-0")
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(50));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Namespace", ns);
                Medium("Service description root element should be in the 'urn:schemas-upnp-org:service-1-0' namespace.");
                return;
            }

            // Case

            XmlNode node = document.SelectSingleNode("/s:scpd/s:specVersion", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(50));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                High("Service description root element does not contain the mandatory <specVersion> element.");
            }
            else
            {
                // Case

                string spec = null;

                node = document.SelectSingleNode("/s:scpd/s:specVersion/s:major", iUnit.NsManager);

                if (node != null)
                {
                    string major = Value(node);

                    node = document.SelectSingleNode("/s:scpd/s:specVersion/s:minor", iUnit.NsManager);

                    if (node != null)
                    {
                        string minor = Value(node);

                        spec = major + "." + minor;
                    }
                }

                if (spec == null)
                {
                    Reference(iSpecificationUda10.Reference(32));
                    Reference(iSpecificationUda11.Reference(50));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    High("UPnP specification version is not correctly described using <major> and <minor> xml elements.");
                }
                else
                {
                    // Case

                    if (spec != "1.0" && spec != "1.1")
                    {
                        Reference(iSpecificationUda10.Reference(32));
                        Reference(iSpecificationUda11.Reference(50));
                        Context("Service", iServiceNo);
                        Context("ServiceType", iServiceType);
                        Context("Version", spec);
                        Medium("UPnP specification version for must be 1.0, 1.1.");
                    }
                }
            }

            // Case

            iStateVariableList = new List<StateVariable>();

            node = document.SelectSingleNode("/s:scpd/s:serviceStateTable", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                High("Service description does not contain the mandatory <serviceStateTable> xml element.");
            }
            else
            {
                // Case

                XmlNodeList nodes = document.SelectNodes("/s:scpd/s:serviceStateTable/s:stateVariable", iUnit.NsManager);

                if (nodes.Count == 0)
                {
                    Reference(iSpecificationUda10.Reference(33));
                    Reference(iSpecificationUda11.Reference(51));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    High("Service state table contains no state variables. A service must specify at least one state variable.");
                }
                else
                {
                    // Test state variables

                    iStateVariableNo = 1;

                    foreach (XmlNode n in nodes)
                    {
                        TestStateVariable(n);
                        iStateVariableNo++;
                    }
                }
            }

            bool events = false;

            foreach (StateVariable sv in iStateVariableList)
            {
                if (sv.Evented)
                {
                    events = true;
                }
            }


            if (!events)
            {
                // Case

                if (iEventUrl.Length != 0)
                {
                    Reference(iSpecificationUda10.Reference(29));
                    Reference(iSpecificationUda11.Reference(46));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    Context("EventSubscriptionUrl", iEventUrl);
                    Medium("A service must specify a blank event subscription url if it contains no evented state variables.");
                }
            }
            else
            {
                // Case

                if (iEventUrl.Length == 0)
                {
                    Reference(iSpecificationUda10.Reference(29));
                    Reference(iSpecificationUda11.Reference(46));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    Medium("A service must specify an event subscription url if it contains evented state variables.");
                }
                else
                {
                    TestSubscription();
                }
            }

            // Test actions

            XmlNodeList actions = document.SelectNodes("/s:scpd/s:actionList/s:action", iUnit.NsManager);

            iActionNo = 1;
            iActionList = new List<string>();

            foreach (XmlNode n in actions)
            {
                TestAction(n);
                iActionNo++;
            }
        }

        private void TestSubscription()
        {
        /*
            // Case

            Uri serveruri;

            Uri uri = RelativeUri(iEventUrl);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            request.Method = "SUBSCRIBE";
            //request.Headers.Add("HOST: " + uri.Host + ":" + uri.Port);
            //request.Headers.Add("CALLBACK: " + serveruri.AbsolutePath);
            request.Headers.Add("NT: upnp:event");
            request.Headers.Add("TIMEOUT: Second-30");

            // Use a default WebProxy because this is on LAN, not Internet

            request.Proxy = new WebProxy();

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // HTTP/1.1 200 OK
                // DATE: when response was generated
                // SERVER: OS/version UPnP/1.1 product/version
                // SID: uuid:subscription-UUID
                // CONTENT-LENGTH: 0
                // TIMEOUT: Second-actual subscription duration


                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Reference(iSpecificationUda10.Reference(30, 36));
                    Reference(iSpecificationUda11.Reference(48, 59));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    Context("EventSubscriptionUrl", iEventUrl);
                    Context("StatusCode", response.StatusCode.ToString());
                    High("Unable to subscribe to the service.");
                    return;
                }

                StreamReader reader = new StreamReader(response.GetResponseStream());

                xml = reader.ReadToEnd();
            }
            catch (Exception e)
            {
                Reference(iSpecificationUda10.Reference(30, 36));
                Reference(iSpecificationUda11.Reference(48, 59));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("EventSubscriptionUrl", iEventUrl);
                Context("Exception", e.Message);
                High("Unable to subscribe to the service.");
                return;
            }
        */
        }

        private bool IsValidNameCharacter(char aChar, bool aFirst)
        {
            if (aChar >= 'a' && aChar <= 'z')
            {
                return (true);
            }

            if (aChar >= 'A' && aChar <= 'Z')
            {
                return (true);
            }

            if (aChar >= '0' && aChar <= '9')
            {
                return (true);
            }

            if (aChar == '_')
            {
                return (true);
            }

            if (aChar > 0x7f)
            {
                return (true);
            }

            if (!aFirst && aChar == '.')
            {
                return (true);
            }

            return (false);
        }

        private void TestStateVariable(XmlNode aNode)
        {
            bool evented = true;

            XmlNode node = aNode.SelectSingleNode("@sendEvents", iUnit.NsManager);

            if (node != null)
            {
                if (Value(node) == "no")
                {
                    evented = false;
                }
            }

            // Case

            node = aNode.SelectSingleNode("s:name", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(52));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("StateVariable", iStateVariableNo);
                Medium("State variable does not contain the mandatory <name> element.");
                return;
            }

            string name = Value(node);

            // Case

            if (name.Length == 0)
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(52));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("StateVariable", iStateVariableNo);
                Medium("The name of a state variable cannot be blank.");
                return;
            }

            // Case

            if (name.Length >= 32)
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(52));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("StateVariable", iStateVariableNo);
                Context("StateVariableName", name);
                Low("The name of a state variable must be less than 32 characters in length.");
            }

            // Case

            if (name.Contains("-"))
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(52));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("StateVariable", iStateVariableNo);
                Context("StateVariableName", name);
                Low("The name of a state variable must not contain a hyphen.");
            }

            // Case

            if (!IsValidNameCharacter(name[0], true))
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(52));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("StateVariable", iStateVariableNo);
                Context("StateVariableName", name);
                Low("The first character of the name of a state variable must be a USASCII letter (A-Z, a-z), USASCII digit (0-9), an underscore, or a non-experimental Unicode letter or digit greater than U+007F.");
            }

            // Case

            for (int i = 0; i < name.Length - 1; i++)
            {
                if (!IsValidNameCharacter(name[i + 1], false))
                {
                    Reference(iSpecificationUda10.Reference(33));
                    Reference(iSpecificationUda11.Reference(52));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    Context("StateVariable", iStateVariableNo);
                    Context("StateVariableName", name);
                    Low("Each character after the first character in the name of a state variable must be a USASCII letter (A-Z, a-z), USASCII digit (0-9), an underscore, a period, a Unicode combiningchar, an extender, or a non-experimental Unicode letter or digit greater than U+007F.");
                    break;
                }
            }

            // Case

            if (name.Length >= 3 && name.Substring(0, 3).ToUpper() == "XML")
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(52));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("StateVariable", iStateVariableNo);
                Context("StateVariableName", name);
                Low("The first three letters of the name of a state variable must not be 'XML' in any combination of case.");
            }

            // Case

            node = aNode.SelectSingleNode("s:dataType", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(52));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("StateVariable", iStateVariableNo);
                Context("StateVariableName", name);
                High("State variable does not contain the mandatory <dataType> element.");
            }

            string type = Value(node);

            // Case

            switch (type)
            {
                case "ui1":
                case "ui2":
                case "ui4":
                case "i1":
                case "i2":
                case "i4":
                case "int":
                case "r4":
                case "r8":
                case "number":
                case "fixed.14.4":
                case "float":
                case "char":
                case "string":
                case "date":
                case "dateTime":
                case "dateTime.tz":
                case "time":
                case "time.tz":
                case "boolean":
                case "bin.base64":
                case "bin.hex":
                case "uri":
                case "uuid":
                    break;
                default:
                    Reference(iSpecificationUda10.Reference(33, 34));
                    Reference(iSpecificationUda11.Reference(52, 54));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    Context("StateVariable", iStateVariableNo);
                    Context("StateVariableName", name);
                    Context("StateVariableType", type);
                    Medium("State variable has an invalid data type.");
                    return;
            }

            iStateVariableList.Add(new StateVariable(name, type, evented));
        }

        private void TestAction(XmlNode aNode)
        {
            // Case

            XmlNode node = aNode.SelectSingleNode("s:name", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                High("Action does not contain the mandatory <name> element.");
                return;
            }

            iAction = Value(node);

            // Case

            if (iAction.Length == 0)
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Medium("The name of an action cannot be blank.");
                return;
            }

            // Case

            if (iAction.Length >= 32)
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Low("The name of an action must be less than 32 characters in length.");
            }

            // Case

            if (iAction.Contains("-"))
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Low("The name of an action must not contain a hyphen.");
            }

            // Case

            if (!IsValidNameCharacter(iAction[0], true))
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Low("The first character of the name of an action must be a USASCII letter (A-Z, a-z), USASCII digit (0-9), an underscore, or a non-experimental Unicode letter or digit greater than U+007F.");
            }

            // Case

            for (int i = 0; i < iAction.Length - 1; i++)
            {
                if (!IsValidNameCharacter(iAction[i + 1], false))
                {
                    Reference(iSpecificationUda10.Reference(32));
                    Reference(iSpecificationUda11.Reference(51));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    Context("Action", iActionNo);
                    Context("ActionName", iAction);
                    Low("Each character after the first character in the name of an action must be a USASCII letter (A-Z, a-z), USASCII digit (0-9), an underscore, a period, a Unicode combiningchar, an extender, or a non-experimental Unicode letter or digit greater than U+007F.");
                    break;
                }
            }

            if (iAction.Length >= 3 && iAction.Substring(0, 3).ToUpper() == "XML")
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Low("The first three letters of the name of an action must not be 'XML' in any combination of case.");
            }

            // Case

            if (iActionList.Contains(iAction))
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Low("The name of each action within a service must be unique.");
            }
            else
            {
                iActionList.Add(iAction);
            }

            // Test arguments

            XmlNodeList nodes = aNode.SelectNodes("s:argumentList/s:argument", iUnit.NsManager);

            iArgumentNo = 1;
            iArgumentList = new List<Argument>();

            foreach (XmlNode n in nodes)
            {
                TestArgument(n);
                iArgumentNo++;
            }
        }

        private void TestArgument(XmlNode aNode)
        {
            XmlNode node = aNode.SelectSingleNode("s:name", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Context("Argument", iArgumentNo);
                High("Argument does not contain the mandatory <name> element.");
                return;
            }

            string name = Value(node);

            if (name.Length == 0)
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Context("Argument", iArgumentNo);
                High("The name of an argument cannot be blank.");
                return;
            }

            if (name.Length >= 32)
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Context("Argument", iArgumentNo);
                Context("ArgumentName", name);
                Low("The name of an argument must be less than 32 characters in length (UDA 1.0 p33, UDA 1.1, p51).");
            }

            // Case

            if (name.Contains("-"))
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Context("Argument", iArgumentNo);
                Context("ArgumentName", name);
                Low("The name of an argument must not contain a hyphen.");
            }

            // Case

            if (!IsValidNameCharacter(name[0], true))
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Context("Argument", iArgumentNo);
                Context("ArgumentName", name);
                Low("The first character of the name of an argument must be a USASCII letter (A-Z, a-z), USASCII digit (0-9), an underscore, or a non-experimental Unicode letter or digit greater than U+007F.");
            }

            // Case

            for (int i = 0; i < name.Length - 1; i++)
            {
                if (!IsValidNameCharacter(name[i + 1], false))
                {
                    Reference(iSpecificationUda10.Reference(32));
                    Reference(iSpecificationUda11.Reference(51));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    Context("Action", iActionNo);
                    Context("ActionName", iAction);
                    Context("Argument", iArgumentNo);
                    Context("ArgumentName", name);
                    Low("Each character after the first character in the name of an argument must be a USASCII letter (A-Z, a-z), USASCII digit (0-9), an underscore, a period, a Unicode combiningchar, an extender, or a non-experimental Unicode letter or digit greater than U+007F.");
                    break;
                }
            }

            // Case

            if (name.Length >= 3 && name.Substring(0, 3).ToUpper() == "XML")
            {
                Reference(iSpecificationUda10.Reference(32));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Context("Argument", iArgumentNo);
                Context("ArgumentName", name);
                Low("The first three letters of the name of a state variable must not be 'XML' in any combination of case.");
            }

            // Case

            string direction = null;

            node = aNode.SelectSingleNode("s:direction", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Context("Argument", iArgumentNo);
                Context("ArgumentName", name);
                High("Argument does not contain the mandatory <direction> element (UDA 1.0 p33, UDA 1.1, p51).");
            }
            else
            {
                // Case

                direction = Value(node);

                if (direction != "in" && direction != "out")
                {
                    Reference(iSpecificationUda10.Reference(33));
                    Reference(iSpecificationUda11.Reference(51));
                    Context("Service", iServiceNo);
                    Context("ServiceType", iServiceType);
                    Context("Action", iActionNo);
                    Context("ActionName", iAction);
                    Context("Argument", iArgumentNo);
                    Context("ArgumentName", name);
                    Context("ArgumentDirection", direction);
                    High("The direction of an argument must be either 'in' or 'out'.");
                }
                else
                {
                    // Case

                    if (direction == "in")
                    {
                        foreach (Argument argument in iArgumentList)
                        {
                            if (!argument.In)
                            {
                                Reference(iSpecificationUda10.Reference(33));
                                Reference(iSpecificationUda11.Reference(51));
                                Context("Service", iServiceNo);
                                Context("ServiceType", iServiceType);
                                Context("Action", iActionNo);
                                Context("ActionName", iAction);
                                Context("Argument", iArgumentNo);
                                Context("ArgumentName", name);
                                Context("ArgumentDirection", direction);
                                Medium("All 'in' arguments must be listed before 'out' arguments (UDA 1.0 p33, UDA 1.1, p51).");
                                break;
                            }
                        }
                    }
                }
            }

            bool retval = false;

            node = aNode.SelectSingleNode("s:retval", iUnit.NsManager);

            if (node != null)
            {
                retval = true;
            }

            // Case

            node = aNode.SelectSingleNode("s:relatedStateVariable", iUnit.NsManager);

            if (node == null)
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Context("Argument", iArgumentNo);
                Context("ArgumentName", name);
                High("Argument does not contain the mandatory <relatedStateVariable> element (UDA 1.0 p32, UDA 1.1, p51).");
                return;
            }

            string rsv = Value(node);

            bool valid = false;

            foreach (StateVariable sv in iStateVariableList)
            {
                if (sv.Name == rsv)
                {
                    valid = true;
                    break;
                }
            }

            if (!valid)
            {
                Reference(iSpecificationUda10.Reference(33));
                Reference(iSpecificationUda11.Reference(51));
                Context("Service", iServiceNo);
                Context("ServiceType", iServiceType);
                Context("Action", iActionNo);
                Context("ActionName", iAction);
                Context("Argument", iArgumentNo);
                Context("ArgumentName", name);
                Context("RelatedStateVariable", rsv);
                High("Argument specifies a related state variable that does not appear in the service state table (UDA 1.0 p32, UDA 1.1, p51).");
                return;
            }

            if (direction != null)
            {
                iArgumentList.Add(new Argument(name, direction == "in", retval));
            }
        }

        private Uri RelativeUri(string aUrl)
        {
            try
            {
                return (new Uri(iBaseUri, aUrl));
            }
            catch (UriFormatException)
            {
            }

            return (null);
        }

        private string Value(XmlNode aNode)
        {
            if (!aNode.HasChildNodes)
            {
                return (string.Empty);
            }

            return (aNode.FirstChild.Value);
        }

        private ParameterUpnp iUnit;
        private Specification iSpecificationUda10;
        private Specification iSpecificationUda11;

        private Uri iBaseUri;
        private uint iServiceNo;
        private string iServiceType;
        private string iServiceId;
        private string iEventUrl;
        private uint iStateVariableNo;
        private uint iActionNo;
        private string iAction;
        private uint iArgumentNo;
        private List<string> iServiceIdList;
        private List<string> iEventUrlList;
        private List<StateVariable> iStateVariableList;
        private List<string> iActionList;
        private List<Argument> iArgumentList;
    }
}
