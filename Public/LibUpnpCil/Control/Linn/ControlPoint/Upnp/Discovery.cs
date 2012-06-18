using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Threading;

using Linn.Control;
using Linn.Control.Ssdp;

namespace Linn.ControlPoint.Upnp
{

    public class DeviceUpnp : Device
    {

        public static Device CreateLinnMediaRendererDevice(Device aHost)
        {
            string udn = aHost.Udn;

            udn = udn.Substring(0, udn.Length - 2);
            udn += "71";


            Uri host = new Uri(aHost.Find(kKeyUpnpLocation));
            Uri location = new Uri(host, "/MediaRenderer/device.xml");

            DeviceUpnp device = new DeviceUpnp(udn, location.AbsoluteUri);

            return (device);
        }

        private static readonly string kKeyUpnpLocation = "UpnpLocation";
        private static readonly string kKeyUpnpIpAddress = "UpnpIpAddress";
        private static readonly string kKeyUpnpDeviceXml = "UpnpDeviceXml";
        private static readonly string kKeyUpnpFriendlyName = "UpnpFriendName";
        private static readonly string kKeyUpnpModel = "UpnpModel";
        private static readonly string kKeyUpnpPresentationUri = "UpnpPresentationUri";

        private static readonly string kVolkanoRelativeControlUri = "control";
        private static readonly string kVolkanoRelativeSubscriptionUri = "event";

        private static readonly int kDeviceXmlTimeout = 5000; // milliseconds

        public DeviceUpnp(string aLocation)
        {
            Uri location = new Uri(aLocation);
            Add(kKeyUpnpLocation, aLocation);
            Add(kKeyUpnpIpAddress, location.Host);

            string xml = DeviceXml;
            if(!string.IsNullOrEmpty(xml))
            {
                StringReader reader = new StringReader(xml);
                XmlDocument document = new XmlDocument();

                document.Load(reader);
    
                XmlNamespaceManager nsmanager = new XmlNamespaceManager(document.NameTable);
                nsmanager.AddNamespace("u", "urn:schemas-upnp-org:device-1-0");

                // get UDN from the root device

                XmlNode udn = document.SelectSingleNode("/u:root/u:device/u:UDN", nsmanager);
    
                if (udn != null && udn.FirstChild != null)
                {
                    Add(kKeyUdn, udn.FirstChild.Value.Substring(5));
                }
            }
        }

        public DeviceUpnp(string aUdn, string aLocation)
            : base(aUdn)
        {
            Uri location = new Uri(aLocation);
            Add(kKeyUpnpLocation, aLocation);
            Add(kKeyUpnpIpAddress, location.Host);
        }

        public override Device RelatedDevice(string aUdn)
        {
            Device device = new DeviceUpnp(aUdn, Find(kKeyUpnpLocation));

            string xml = Find(kKeyUpnpDeviceXml);

            if (xml != null)
            {
                device.Add(kKeyUpnpDeviceXml, xml);
            }

            return (device);
        }

        public override string DeviceXml
        {
            get
            {
                // See if we have the device xml cached already

                string xml = Find(kKeyUpnpDeviceXml);

                if (xml == null)
                {
                    // ... no, better get and cache the device xml

                    HttpWebResponse response = null;
                    StreamReader reader = null;

                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Find(kKeyUpnpLocation));

                        // Use a HTTP 1.0 client because of bug 80017 in the mono bug database
                        // Use a default WebProxy to avoid proxy authentication errors
                        request.Proxy = new WebProxy();
                        request.ProtocolVersion = HttpVersion.Version10;
                        request.KeepAlive = false;
                        request.Timeout = kDeviceXmlTimeout;
                        request.ReadWriteTimeout = kDeviceXmlTimeout;

                        response = (HttpWebResponse)request.GetResponse();
                        reader = new StreamReader(response.GetResponseStream());

                        xml = reader.ReadToEnd();

                        Add(kKeyUpnpDeviceXml, xml);
                    }
                    catch (WebException e)
                    {
                        UserLog.WriteLine(DateTime.Now + ": DeviceUpnp.DeviceXml: " + Location + ": " + e.Message + (e.InnerException != null ? (", " + e.InnerException.Message) : ""));
                        Console.WriteLine(DateTime.Now + ": DeviceUpnp.DeviceXml: " + Location + ": " + e.Message + (e.InnerException != null ? (", " + e.InnerException.Message) : ""));
                        return (null);
                    }
                    catch (ArgumentException)
                    {
                        // Possible if two threads are trying to get the device XML ar the same time
                        // One will win and the other will cause this exception on the Add. Just
                        // ignore the exception and carry on with the collected XML.
                        return (null);
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }

                        if (response != null)
                        {
                            response.Close();
                        }
                    }
                }

                return (xml);
            }
        }

        private XmlNode DeviceXmlExplicit()
        {
            string xml = DeviceXml;

            if (xml != null)
            {
                return (Upnp.DeviceXmlExplicit(xml, Udn));
            }

            return (null);
        }

        public override void Open()
        {
            // Used to collect the device xml asynchronously

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Find(kKeyUpnpLocation));

            // Use a HTTP 1.0 client because of bug 80017 in the mono bug database
            // Use a default WebProxy to avoid proxy authentication errors
            request.Proxy = new WebProxy();
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.Timeout = kDeviceXmlTimeout;
            request.ReadWriteTimeout = kDeviceXmlTimeout;

            try
            {
                WebRequestPool.QueueJob(new JobGetResponse(OpenResponse, request));
            }
            catch (WebException)
            {
            }
        }

        private void OpenResponse(object aResult)
        {
            HttpWebResponse response = null;
            StreamReader reader = null;

            try
            {
                HttpWebRequest request = aResult as HttpWebRequest;
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());

                string xml = reader.ReadToEnd();

                Add(kKeyUpnpDeviceXml, xml);
                Opened();
            }
            catch (WebException e)
            {
                UserLog.WriteLine(DateTime.Now + ": DeviceUpnp.OpenResponse: " + Location + ": " + e.Message + (e.InnerException != null ? (", " + e.InnerException.Message) : ""));
                Console.WriteLine(DateTime.Now + ": DeviceUpnp.OpenResponse: " + Location + ": " + e.Message + (e.InnerException != null ? (", " + e.InnerException.Message) : ""));
            }
            catch (ArgumentException)
            {
                // Possible if two threads are trying to get the device XML at the same time
                // One will win and the other will cause this exception on the Add. Just
                // ignore the exception and carry on with the collected XML.
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                if (response != null)
                {
                    response.Close();
                }
            }
        }

        public override string Name
        {
            get
            {
                // See if we have the name cached already

                string name = Find(kKeyUpnpFriendlyName);

                if (name == null)
                {
                    // ... no, better get and cache the presentation uri

                    name = Upnp.FriendlyName(DeviceXmlExplicit());

                    if (name != null)
                    {
                        Add(kKeyUpnpFriendlyName, name);
                    }
                }

                return (name);
            }
        }

        public override string Model
        {
            get
            {
                // See if we have the model cached already

                string model = Find(kKeyUpnpModel);

                if (model == null)
                {
                    // ... no, better get and cache the model

                    XmlNode device = DeviceXmlExplicit();

                    if (device != null)
                    {
                        XmlNamespaceManager nsmanager = new XmlNamespaceManager(device.OwnerDocument.NameTable);
                        nsmanager.AddNamespace("u", "urn:schemas-upnp-org:device-1-0");

                        XmlNode node = device.SelectSingleNode("u:modelName", nsmanager);

                        if (node != null)
                        {
                            model = node.FirstChild.Value;
                            Add(kKeyUpnpModel, model);
                        }
                    }
                }

                return (model);
            }
        }

        public override string Location
        {
            get
            {
                return (Find(kKeyUpnpLocation));
            }
        }

        public override string IpAddress
        {
            get
            {
                return (Find(kKeyUpnpIpAddress));
            }
        }

        public override string PresentationUri
        {
            get
            {
                // See if we have the presentation uri cached already

                string presentation = Find(kKeyUpnpPresentationUri);

                if (presentation == null)
                {
                    // ... no, better get and cache the presentation uri

                    presentation = Upnp.PresentationUri(DeviceXmlExplicit());

                    if (presentation != null)
                    {
                        Add(kKeyUpnpPresentationUri, presentation);
                    }
                }

                return (presentation);
            }
        }

        public override uint HasService(ServiceType aType)
        {
            return (Upnp.HasService(DeviceXmlExplicit(), aType));
        }

        public override bool HasAction(ServiceType aType, string aAction)
        {
            // not yet implemented ... requires collecting the service xml
            return (false);
        }

        public override bool HasState(ServiceType aType, string aState)
        {
            // not yet implemented ... requires collecting the service xml
            return (false);
        }

        public override ServiceLocation FindServiceLocation(ServiceType aType)
        {
            try
            {
                if (IsLinn)
                {
                    // this is a volkano device, so try to infer control and subscription uris
                    Uri uri = new Uri(Find(kKeyUpnpLocation));

                    if (uri.AbsolutePath != "/")
                    {
                        // this is a bute or post-bute volkano device from which the control and
                        // subscription uris can be inferred

                        Uri serviceuri = new Uri(uri, aType.Type + "/");
                        Uri controluri = new Uri(serviceuri, kVolkanoRelativeControlUri);
                        Uri subscriptionuri = new Uri(serviceuri, kVolkanoRelativeSubscriptionUri);

                        return (new ServiceLocationUpnp(controluri, subscriptionuri));
                    }
                }

                // this is not a volkano device or it is a pre-bute volkano device, so
                // must inspect the device xml in order to find the control and subscription uris

                string servicetype = Upnp.ServiceTypeToString(aType);

                XmlNode device = DeviceXmlExplicit();

                if (device != null)
                {
                    XmlNamespaceManager nsmanager = new XmlNamespaceManager(device.OwnerDocument.NameTable);
                    nsmanager.AddNamespace("u", "urn:schemas-upnp-org:device-1-0");

                    // collect the URLBase if there is one

                    Uri rooturi;

                    XmlNode root = device.SelectSingleNode("/u:root/u:URLBase", nsmanager);

                    try
                    {
                        if (root == null)
                        {
                            rooturi = new Uri(Find(kKeyUpnpLocation));
                        }
                        else
                        {
                            rooturi = new Uri(root.FirstChild.Value);
                        }
                    }
                    catch (UriFormatException)
                    {
                        throw (new DeviceException(104, "URLBase malformed"));
                    }

                    // look in the root device for the service

                    XmlNodeList list;

                    list = device.SelectNodes("u:serviceList/u:service", nsmanager);

                    foreach (XmlNode n in list)
                    {
                        XmlNode type = n.SelectSingleNode("u:serviceType", nsmanager);

                        if (type != null)
                        {
                            ServiceType found = new ServiceType(type.FirstChild.Value);
                            ServiceType mine = new ServiceType(servicetype);

                            if (mine.IsSupportedBy(found))
                            {
                                return (GetServiceLocation(n, nsmanager, rooturi));
                            }
                        }
                    }
                }

                throw (new DeviceException(101, servicetype + " not found in device xml"));
            }
            catch (Exception ex)
            {
                // logging for ticket #876
                UserLog.WriteLine("Unhandled exception in FindServiceLocation : " + ex);
                throw ex;
            }
        }

        private ServiceLocation GetServiceLocation(XmlNode aNode, XmlNamespaceManager aNamespaceManager, Uri aRootUri)
        {
            XmlNode control = aNode.SelectSingleNode("u:controlURL", aNamespaceManager);
            XmlNode subscription = aNode.SelectSingleNode("u:eventSubURL", aNamespaceManager);

            if (control != null && subscription != null)
            {
                try
                {
                    Uri controluri = new Uri(aRootUri, control.FirstChild.Value);
                    Uri subscriptionuri = null;
                    if (subscription.FirstChild != null)
                    {
                        subscriptionuri = new Uri(aRootUri, subscription.FirstChild.Value);
                    }
                    return (new ServiceLocationUpnp(controluri, subscriptionuri));
                }
                catch (UriFormatException)
                {
                    throw (new DeviceException(102, "Malformed control and/or subscription uri"));
                }
            }
            throw (new DeviceException(103, "Control and/or subscription uri not found in device xml"));
        }
    }


    public class DeviceListUpnp : DeviceList, ISsdpNotifyHandler
    {
        enum ESearchType
        {
            eAll,
            eService,
            eDevice
        }

        private const string kDomainUpnpOrg = "upnp.org";
        private const string kDomainSchemasUpnpOrg = "schemas.upnp.org";
        private const uint kSsdpSearchTime = 3;
        private const uint kSsdpSearchCount = 1;
        private const int kSsdpSearchDelay = 100;

        public DeviceListUpnp(ServiceType aType)
        {
            iType = aType;
            iListenerUnicast = new SsdpListenerUnicast(this);
            iTimers = new List<Timer>();
        }

        public DeviceListUpnp(ServiceType aType, ISsdpNotifyProvider aListener)
            : this(aType)
        {
            aListener.Add(this);
        }

        public override void Rescan()
        {
            UserLog.WriteLine(DateTime.Now + ": Upnp List Rescan Type{" + iType + "}");
            Trace.WriteLine(Trace.kUpnp, "Upnp List Rescan Type{" + iType + "}");

            string sdomain = iType.Domain;

            if (sdomain == kDomainUpnpOrg)
            {
                sdomain = kDomainSchemasUpnpOrg;
            }

            sdomain = sdomain.Replace('.', '-');

            byte[] domain = ASCIIEncoding.UTF8.GetBytes(sdomain);
            byte[] type = ASCIIEncoding.UTF8.GetBytes(iType.Type);
            uint version = iType.Version;

            bool mSearchSent = true;
            try
            {
                iListenerUnicast.SsdpMsearchServiceType(domain, type, version, kSsdpSearchTime);
            }
            catch (Linn.WriterError) { mSearchSent = false; }

            uint retry = kSsdpSearchCount;

            while (--retry > 0)
            {
                Thread.Sleep(kSsdpSearchDelay);

                try
                {
                    iListenerUnicast.SsdpMsearchServiceType(domain, type, version, kSsdpSearchTime);
                }
                catch (Linn.WriterError) { mSearchSent = false; }
            }
            if (mSearchSent)
            {
                Timer t = new Timer(kSsdpSearchTime * 1000);
                t.Elapsed += RescanElapsed;
                t.Start();
                lock (iTimers)
                {
                    iTimers.Add(t);
                }
            }
        }

        private void RescanElapsed(object sender, EventArgs e)
        {
            base.RemoveExpiredDevices(kSsdpSearchTime * 1000 * 3);
            Timer t = sender as Timer;
            lock (iTimers)
            {
                iTimers.Remove(t);
            }
            t.Stop();
            t.Elapsed -= RescanElapsed;
            t.Dispose();
        }

        public override void Start(IPAddress aInterface)
        {
            Trace.WriteLine(Trace.kUpnp, "Upnp List Start  Type{" + iType + "}");
            iListenerUnicast.Start(aInterface);
            Rescan();
        }

        public override void Stop()
        {
            iListenerUnicast.Stop();
            Clear();
        }

        public void NotifyRootAlive(byte[] aUuid, byte[] aLocation, uint aMaxAge)
        {
        	string uuid = ASCIIEncoding.UTF8.GetString(aUuid,0,aUuid.Length);

            Trace.WriteLine(Trace.kUpnp, "Alive Root       Uuid{" + uuid + "}");
        }

        public void NotifyUuidAlive(byte[] aUuid, byte[] aLocation, uint aMaxAge)
        {
           string uuid = ASCIIEncoding.UTF8.GetString(aUuid,0,aUuid.Length);

            Trace.WriteLine(Trace.kUpnp, "Alive Uuid       Uuid{" + uuid + "}");
        }

        public void NotifyDeviceTypeAlive(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion, byte[] aLocation, uint aMaxAge)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid,0,aUuid.Length);

            Trace.WriteLine(Trace.kUpnp, "Alive Device     Uuid{" + uuid + "}");
        }

        public void NotifyServiceTypeAlive(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion, byte[] aLocation, uint aMaxAge)
        {
        	
            string domain = ASCIIEncoding.UTF8.GetString(aDomain,0,aDomain.Length).Replace('-', '.');
            string type = ASCIIEncoding.UTF8.GetString(aType,0,aType.Length);

            if (domain == kDomainSchemasUpnpOrg)
            {
                domain = kDomainUpnpOrg;
            }

            string uuid = ASCIIEncoding.UTF8.GetString(aUuid,0,aUuid.Length);

            ServiceType st = new ServiceType(domain, type, aVersion);

            Trace.WriteLine(Trace.kUpnp, "Alive Service    Uuid{" + uuid + "}" + st);

            if (iType.IsSupportedBy(st))
            {
                string location = ASCIIEncoding.UTF8.GetString(aLocation, 0, aLocation.Length);
                try {
                    Add(new DeviceUpnp(uuid, location));
                }
                catch (UriFormatException e) {
                    UserLog.WriteLine(DateTime.Now + ": Uuid{" + uuid + "}Location{" + location + "}: " + e.Message);
                }
            }
        }

        public void NotifyRootByeBye(byte[] aUuid)
        {
			string uuid = ASCIIEncoding.UTF8.GetString(aUuid, 0, aUuid.Length);

            Trace.WriteLine(Trace.kUpnp, "ByeBye Root      Uuid{" + uuid + "}");

            Remove(uuid);
        }

        public void NotifyUuidByeBye(byte[] aUuid)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid, 0, aUuid.Length);

            Trace.WriteLine(Trace.kUpnp, "ByeBye Uuid      Uuid{" + uuid + "}");

            Remove(uuid);
        }

        public void NotifyDeviceTypeByeBye(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid, 0, aUuid.Length);

            Trace.WriteLine(Trace.kUpnp, "ByeBye Device    Uuid{" + uuid + "}");

            Remove(uuid);
        }

        public void NotifyServiceTypeByeBye(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion)
        {
           string uuid = ASCIIEncoding.UTF8.GetString(aUuid, 0, aUuid.Length);

            Trace.WriteLine(Trace.kUpnp, "ByeBye Service   Uuid{" + uuid + "}");

            Remove(uuid);
        }


        private ServiceType iType;
        private SsdpListenerUnicast iListenerUnicast;
        private List<Timer> iTimers;
    }

}
