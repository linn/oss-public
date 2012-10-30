using System;
using System.Collections.Generic;
using System.Xml;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;

using OpenHome.Xapp;

namespace Linn.Konfig
{
    public enum ERecognisedType
    {
        eDs,
        eMediaServer,
        eMediaRenderer,
        eSonos
    }

    public delegate void RecogniserHandler(CpDeviceRecognised aDevice);

    public interface IRecogniser
    {
        CpDeviceRecognised Recognise(CpDevice aDevice, RecogniserHandler aRecognised);
    }

    public abstract class CpDeviceRecognised : IDisposable, IComparable<CpDeviceRecognised>
    {
        public CpDeviceRecognised(CpDevice aDevice)
        {
            iDevice = aDevice;
            iDevice.AddRef();
        }

        public virtual void Dispose()
        {
            iDevice.RemoveRef();
            iDevice = null;
        }

        public CpDevice Device
        {
            get
            {
                return iDevice;
            }
        }

        public string Udn
        {
            get
            {
                return iDevice.Udn();
            }
        }

        public int CompareTo(CpDeviceRecognised aDevice)
        {
            if (Udn == aDevice.Udn)
            {
                return 0;
            }

            if (Type != aDevice.Type)
            {
                return (Type < aDevice.Type) ? -1 : 1;
            }

            int cmp = string.Compare(Fullname, aDevice.Fullname);
            if (cmp == 0)
            {
                return Udn.CompareTo(aDevice.Udn);
            }

            return cmp;
        }

        public EventHandler<EventArgs> Changed;

        protected void OnChanged()
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        public abstract string Fullname { get; }
        public abstract ERecognisedType Type { get; }
        public abstract string PresentationUri { get; }
        public abstract JsonObject Json { get; }

        private CpDevice iDevice;
    }

    public class CpDeviceRecognisedLinn : CpDeviceRecognised
    {
        public CpDeviceRecognisedLinn(CpDevice aDevice, RecogniserHandler aRecognised)
            : base(aDevice)
        {
            try
            {
                iLock = new object();

                iRecognised = aRecognised;

                iInAppConfig = false;
                iPresentationUri = string.Empty;

                iProductService = new CpProxyAvOpenhomeOrgProduct1(aDevice);
                iProductService.SetPropertyInitialEvent(InitialEvent);
                iProductService.SetPropertyProductRoomChanged(ProductRoomChanged);
                iProductService.SetPropertyProductNameChanged(ProductNameChanged);
                iProductService.SetPropertyModelNameChanged(ModelNameChanged);
                iProductService.SetPropertyProductImageUriChanged(ProductImageUriChanged);
                iProductService.SetPropertyAttributesChanged(ProductAttributesChanged);
                iProductService.Subscribe();
            }
            catch
            {
                aDevice.RemoveRef();
                throw;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (iProductService != null)
            {
                iProductService.Dispose();
                iProductService = null;
            }
        }

        #region CpDeviceRecognised implementation
        public override string Fullname
        {
            get
            {
                return string.Format("{0}:{1}", iProductRoom, iProductName);
            }
        }

        public override ERecognisedType Type
        {
            get
            {
                return ERecognisedType.eDs;
            }
        }

        public override string PresentationUri
        {
            get
            {
                return iPresentationUri;
            }
        }

        public override JsonObject Json
        {
            get
            {
                lock (iLock)
                {
                    JsonObject info = new JsonObject();
                    info.Add("Room", new JsonValueString(iProductRoom));
                    if (iProductName != iModelName)
                    {
                        info.Add("Name", new JsonValueString(string.Format(" ({0})", iProductName)));
                    }
                    else
                    {
                        info.Add("Name", new JsonValueString(string.Empty));
                    }
                    info.Add("Model", new JsonValueString(iModelName));
                    info.Add("ImageUri", new JsonValueString(iProductImageUri));
                    info.Add("InAppConfig", new JsonValueBool(iInAppConfig));

                    return info;
                }
            }
        }
        #endregion

        private void InitialEvent()
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceRecognisedLinn: InitialEvent+             Udn{" + Udn + "}");

            lock (iLock)
            {
                iProductRoom = iProductService.PropertyProductRoom();
                iProductName = iProductService.PropertyProductName();
                iModelName = iProductService.PropertyModelName();
                iProductImageUri = iProductService.PropertyProductImageUri();
                SetPresentationUri(iProductService.PropertyAttributes());
                if (string.IsNullOrEmpty(iProductImageUri))
                {
                    iProductImageUri = kDefaultProductImageUri;
                }
            }

            iRecognised(this);
        }

        private void ProductRoomChanged()
        {
            lock (iLock)
            {
                iProductRoom = iProductService.PropertyProductRoom();
            }

            OnChanged();
        }

        private void ProductNameChanged()
        {
            lock (iLock)
            {
                iProductName = iProductService.PropertyProductName();
            }

            OnChanged();
        }

        private void ProductImageUriChanged()
        {
            lock (iLock)
            {
                iProductImageUri = iProductService.PropertyProductImageUri();
                if (string.IsNullOrEmpty(iProductImageUri))
                {
                    iProductImageUri = kDefaultProductImageUri;
                }
            }

            OnChanged();
        }

        private void ModelNameChanged()
        {
            lock (iLock)
            {
                iModelName = iProductService.PropertyModelName();
            }

            OnChanged();
        }

        private void ProductAttributesChanged()
        {
            SetPresentationUri(iProductService.PropertyAttributes());
        }

        private void SetPresentationUri(string aAttributes)
        {
            iInAppConfig = false;
            iPresentationUri = string.Empty;

            string[] splitAttributes = aAttributes.Split(' ');
            foreach (string attribute in splitAttributes)
            {
                if (attribute.StartsWith("App"))
                {
                    string[] splitAttribute = attribute.Split(new char[] { ':' }, 2);
                    if (splitAttribute.Length == 2)
                    {
                        string[] splitType = splitAttribute[1].Split(new char[] { '=' }, 2);
                        if (splitType.Length == 2)
                        {
                            iPresentationUri = splitType[1];
                            iInAppConfig = true;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(iPresentationUri))
            {
                string presentationUri;
                if (Device.GetAttribute("Upnp.PresentationUrl", out presentationUri))
                {
                    iPresentationUri = presentationUri.Replace("index.html", kConfigurationUri);
                }
                else
                {
                    string location;
                    if (Device.GetAttribute("Upnp.Location", out location))
                    {
                        Uri uri = new Uri(location);

                        string version;
                        if (Device.GetAttribute("Upnp.Service.av-openhome-org.Volume", out version))
                        {
                            iPresentationUri = string.Format("{0}://{1}/{2}", uri.Scheme, uri.Host, kProxyPreampConfigurationUri);
                        }
                        else if (Device.GetAttribute("Upnp.Service.linn-co-uk.Sdp", out version))
                        {
                            iPresentationUri = string.Format("{0}://{1}/{2}", uri.Scheme, uri.Host, kProxyCdConfigurationUri);
                        }
                    }
                }
                /*else
                {
                    iPresentationUri = string.Empty;
                }*/
            }
        }

        private static readonly string kDefaultProductImageUri = "images/NotFound.png";

        private static readonly string kConfigurationUri = "Config/Layouts/Default/index.html#service=Ds";
        private static readonly string kProxyPreampConfigurationUri = "Config/Layouts/Default/index.html#service=Preamp";
        private static readonly string kProxyCdConfigurationUri = "Config/Layouts/Default/index.html#service=Cd";

        private RecogniserHandler iRecognised;
        private CpProxyAvOpenhomeOrgProduct1 iProductService;

        private object iLock;
        private bool iInAppConfig;

        private string iProductRoom;
        private string iProductName;
        private string iModelName;
        private string iProductImageUri;
        private string iPresentationUri;
    }

    public class RecogniserLinn : IRecogniser
    {
        #region IRecogniser implementation
        public CpDeviceRecognised Recognise(CpDevice aDevice, RecogniserHandler aRecognised)
        {
            if (aDevice.Udn().StartsWith(kUdnLinnPrefix))
            {
                string xml;
                if (aDevice.GetAttribute("Upnp.DeviceXml", out xml))
                {
                    XmlNameTable table = new NameTable();
                    XmlNamespaceManager manager = new XmlNamespaceManager(table);
                    manager.AddNamespace("ns", "urn:schemas-upnp-org:device-1-0");

                    XmlDocument doc = new XmlDocument(manager.NameTable);
                    try
                    {
                        doc.LoadXml(xml);
                    }
                    catch (Exception ex)
                    {
                        UserLog.WriteLine("RecogniserLinn.  Exception caught recognising device xml: " + ex);
                        UserLog.WriteLine(xml);
                        throw;
                    }

                    XmlNodeList nodes = doc.SelectNodes("/ns:root/ns:device/ns:serviceList/ns:service/ns:serviceType", manager);
                    foreach (XmlNode n in nodes)
                    {
                        if (n.FirstChild != null && n.FirstChild.InnerText == kProductServiceType)
                        {
                            UserLog.WriteLine(DateTime.Now + ": RecogniserLinn: Device+             Udn{" + aDevice.Udn() + "}");

                            return new CpDeviceRecognisedLinn(aDevice, aRecognised);
                        }
                    }
                }
            }

            return null;
        }
        #endregion

        private static readonly string kUdnLinnPrefix = "4c494e4e-";
        private static readonly string kProductServiceType = "urn:av-openhome-org:service:Product:1";
    }

    public class CpDeviceRecognisedSonos : CpDeviceRecognised
    {
        public CpDeviceRecognisedSonos(CpDevice aDevice, RecogniserHandler aRecognised)
            : base(aDevice)
        {
            try
            {
                iRecognised = aRecognised;

                string xml;
                if (aDevice.GetAttribute("Upnp.DeviceXml", out xml))
                {
                    XmlNameTable table = new NameTable();
                    XmlNamespaceManager manager = new XmlNamespaceManager(table);
                    manager.AddNamespace("ns", "urn:schemas-upnp-org:device-1-0");

                    XmlDocument doc = new XmlDocument(manager.NameTable);
                    try
                    {
                        doc.LoadXml(xml);
                    }
                    catch (Exception ex)
                    {
                        UserLog.WriteLine("CpDeviceRecognisedSonos.  Exception caught recognising device xml: " + ex);
                        UserLog.WriteLine(xml);
                        throw;
                    }

                    string baseUri = string.Empty;
                    XmlNode node = doc.SelectSingleNode("/ns:root/ns:URLBase", manager);
                    if (node == null)
                    {
                        string location;
                        if (aDevice.GetAttribute("Upnp.Location", out location))
                        {
                            Uri uri = new Uri(location);
                            baseUri = string.Format("{0}://{1}", uri.Scheme, uri.Authority);
                        }
                    }
                    else if (node.FirstChild != null)
                    {
                        baseUri = node.FirstChild.InnerText;
                    }

                    XmlNodeList nodes = doc.SelectNodes("/ns:root/ns:device/ns:iconList/ns:icon", manager);
                    int width = 0;
                    int height = 0;
                    foreach (XmlNode n in nodes)
                    {
                        bool found = false;
                        XmlNode w = n.SelectSingleNode("ns:width", manager);
                        XmlNode h = n.SelectSingleNode("ns:height", manager);
                        XmlNode u = n.SelectSingleNode("ns:url", manager);

                        if (w != null && w.FirstChild != null)
                        {
                            try
                            {
                                int i = int.Parse(w.FirstChild.InnerText);
                                if (i > width)
                                {
                                    width = i;
                                    height = i;
                                    found = true;
                                }
                            }
                            catch (FormatException) { }
                        }

                        if (h != null && h.FirstChild != null)
                        {
                            try
                            {
                                int i = int.Parse(h.FirstChild.InnerText);
                                if (i > height)
                                {
                                    width = i;
                                    height = i;
                                    found = true;
                                }
                            }
                            catch (FormatException) { }
                        }

                        if (found && u != null && u.FirstChild != null)
                        {
                            iProductImageUri = baseUri;
                            iProductImageUri += u.FirstChild.InnerText;
                        }
                    }
                }

                string name;
                if (Device.GetAttribute("Upnp.FriendlyName", out name))
                {
                    iZoneName = name;
                }
                else
                {
                    iZoneName = string.Empty;
                }

                iRecognised(this);
            }
            catch
            {
                aDevice.RemoveRef();
                throw;
            }
        }

        #region implemented abstract members of Linn.Konfig.CpDeviceRecognised
        public override string Fullname
        {
            get
            {
                return iZoneName;
            }
        }

        public override ERecognisedType Type
        {
            get
            {
                return ERecognisedType.eSonos;
            }
        }

        public override string PresentationUri
        {
            get
            {
                string uri;
                if (Device.GetAttribute("Upnp.PresentationUrl", out uri))
                {
                    return uri;
                }

                return string.Empty;
            }
        }

        public override JsonObject Json
        {
            get
            {
                JsonObject info = new JsonObject();
                info.Add("ZoneName", new JsonValueString(iZoneName));
                info.Add("ImageUri", new JsonValueString(iProductImageUri));

                return info;
            }
        }
        #endregion

        private RecogniserHandler iRecognised;
        private string iZoneName;
        private string iProductImageUri;
    }

    public class RecogniserSonos : IRecogniser
    {
        #region IRecogniser implementation
        public CpDeviceRecognised Recognise(CpDevice aDevice, RecogniserHandler aRecognised)
        {
            string version;
            if (aDevice.GetAttribute("Upnp.Service.schemas-upnp-org.DeviceProperties", out version))
            {
                UserLog.WriteLine(DateTime.Now + ": RecogniserSonos: Device+             Udn{" + aDevice.Udn() + "}");
                try
                {
                    return new CpDeviceRecognisedSonos(aDevice, aRecognised);
                }
                catch(Exception ex)
                {
                    UserLog.WriteLine(DateTime.Now + "RecogniserSonos:  Exception caught recognising device xml: " + ex);
                    return null;
                }
            }

            return null;
        }
        #endregion
    }

    public class CpDeviceRecognisedMediaServer : CpDeviceRecognised
    {
        public CpDeviceRecognisedMediaServer(CpDevice aDevice, RecogniserHandler aRecognised)
            : base(aDevice)
        {
            try
            {
                iRecognised = aRecognised;

                iProductImageUri = kProductImageUri;

                string xml;
                if (aDevice.GetAttribute("Upnp.DeviceXml", out xml))
                {
                    XmlNameTable table = new NameTable();
                    XmlNamespaceManager manager = new XmlNamespaceManager(table);
                    manager.AddNamespace("ns", "urn:schemas-upnp-org:device-1-0");

                    XmlDocument doc = new XmlDocument(manager.NameTable);
                    doc.LoadXml(xml);

                    string baseUri = string.Empty;
                    XmlNode node = doc.SelectSingleNode("/ns:root/ns:URLBase", manager);
                    if (node == null)
                    {
                        string location;
                        if (aDevice.GetAttribute("Upnp.Location", out location))
                        {
                            Uri uri = new Uri(location);
                            baseUri = string.Format("{0}://{1}", uri.Scheme, uri.Authority);
                        }
                    }
                    else if (node.FirstChild != null)
                    {
                        baseUri = node.FirstChild.InnerText;
                    }

                    iBaseUri = baseUri;

                    XmlNodeList nodes = doc.SelectNodes("/ns:root/ns:device/ns:iconList/ns:icon", manager);
                    int width = 0;
                    int height = 0;
                    foreach (XmlNode n in nodes)
                    {
                        bool found = false;
                        XmlNode w = n.SelectSingleNode("ns:width", manager);
                        XmlNode h = n.SelectSingleNode("ns:height", manager);
                        XmlNode u = n.SelectSingleNode("ns:url", manager);

                        if (w != null && w.FirstChild != null)
                        {
                            try
                            {
                                int i = int.Parse(w.FirstChild.InnerText);
                                if (i > width)
                                {
                                    width = i;
                                    height = i;
                                    found = true;
                                }
                            }
                            catch (FormatException) { }
                        }

                        if (h != null && h.FirstChild != null)
                        {
                            try
                            {
                                int i = int.Parse(h.FirstChild.InnerText);
                                if (i > height)
                                {
                                    width = i;
                                    height = i;
                                    found = true;
                                }
                            }
                            catch (FormatException) { }
                        }

                        if (found && u != null && u.FirstChild != null)
                        {
                            iProductImageUri = baseUri;
                            if (!iProductImageUri.EndsWith("/") && !u.FirstChild.InnerText.StartsWith("/"))
                            {
                                iProductImageUri += "/";
                            }
                            iProductImageUri += u.FirstChild.InnerText;
                        }
                    }
                }

                string name;
                if (Device.GetAttribute("Upnp.FriendlyName", out name))
                {
                    iFriendlyName = name;
                }
                else
                {
                    iFriendlyName = string.Empty;
                }

                iRecognised(this);
            }
            catch
            {
                throw;
            }
        }

        #region implemented abstract members of Linn.Konfig.CpDeviceRecognised
        public override string Fullname
        {
            get
            {
                return iFriendlyName;
            }
        }

        public override ERecognisedType Type
        {
            get
            {
                return ERecognisedType.eMediaServer;
            }
        }

        public override string PresentationUri
        {
            get
            {
                string uri;
                if (Device.GetAttribute("Upnp.PresentationUrl", out uri))
                {
                    if (uri.EndsWith("/Upnp/resource"))
                    {
                        uri += "/";
                    }
                    else if (uri == "/")
                    {
                        uri = iBaseUri + uri;
                    }
                    return uri;
                }

                return string.Empty;
            }
        }

        public override JsonObject Json
        {
            get
            {
                JsonObject info = new JsonObject();
                info.Add("Name", new JsonValueString(iFriendlyName));
                info.Add("ImageUri", new JsonValueString(iProductImageUri));

                return info;
            }
        }
        #endregion

        private static readonly string kProductImageUri = "images/NotFound.png";

        private RecogniserHandler iRecognised;
        private string iFriendlyName;
        private string iProductImageUri;
        private string iBaseUri;
    }

    public class RecogniserMediaServer : IRecogniser
    {
        #region IRecogniser implementation
        public CpDeviceRecognised Recognise(CpDevice aDevice, RecogniserHandler aRecognised)
        {
            string xml;
            if (aDevice.GetAttribute("Upnp.DeviceXml", out xml))
            {
                XmlNameTable table = new NameTable();
                XmlNamespaceManager manager = new XmlNamespaceManager(table);
                manager.AddNamespace("ns", "urn:schemas-upnp-org:device-1-0");

                XmlDocument doc = new XmlDocument(manager.NameTable);
                try
                {
                    doc.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    UserLog.WriteLine("RecogniserMediaServer.  Exception caught recognising device xml: " + ex);
                    UserLog.WriteLine(xml);
                    return null;
                }

                XmlNode n = doc.SelectSingleNode("/ns:root/ns:device/ns:deviceType", manager);
                if (n != null && n.FirstChild != null && n.FirstChild.InnerText == kDeviceType)
                {
                    UserLog.WriteLine(DateTime.Now + ": RecogniserMediaServer: Device+             Udn{" + aDevice.Udn() + "}");

                    return new CpDeviceRecognisedMediaServer(aDevice, aRecognised);
                }
            }

            return null;
        }
        #endregion

        private static readonly string kDeviceType = "urn:schemas-upnp-org:device:MediaServer:1";
    }

    public class CpDeviceRecogniserList : IDisposable
    {
        public CpDeviceRecogniserList(IList<IRecogniser> aRecognisers, ChangedHandler aAdded, ChangedHandler aRemoved)
        {
            iAdded = aAdded;
            iRemoved = aRemoved;
            iRecognisers = aRecognisers;

            iDisposed = false;
            iLock = new object();
            iDeviceListPending = new List<CpDeviceRecognised>();
            iDeviceListRecognised = new List<CpDeviceRecognised>();

            iDeviceListAll = new CpDeviceListUpnpAll(Added, Removed);
        }

        public void Dispose()
        {
            iDeviceListAll.Dispose();
            iDeviceListAll = null;

            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceRecogniserList");
                }

                foreach (CpDeviceRecognised d in iDeviceListPending)
                {
                    d.Dispose();
                }

                foreach (CpDeviceRecognised d in iDeviceListRecognised)
                {
                    d.Dispose();
                }

                iDeviceListPending.Clear();
                iDeviceListRecognised.Clear();

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("CpDeviceRecogniserList");
                }

                iDeviceListAll.Refresh();
            }
        }

        public delegate void ChangedHandler(CpDeviceRecogniserList aList, CpDeviceRecognised aDevice);

        private void Added(CpDeviceList aList, CpDevice aDevice)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceRecogniserList: Device+             Udn{" + aDevice.Udn() + "}");

            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                foreach (IRecogniser r in iRecognisers)
                {
                    CpDeviceRecognised recognisedDevice = r.Recognise(aDevice, RecogniserAdded);
                    if (recognisedDevice != null)
                    {
                        // check to see if device has been added immediately
                        if (!iDeviceListRecognised.Contains(recognisedDevice))
                        {
                            iDeviceListPending.Add(recognisedDevice);
                        }

                        return;
                    }
                }
            }

            UserLog.WriteLine(DateTime.Now + ": CpDeviceRecogniserList: Device not recognised       Udn{" + aDevice.Udn() + "}");
        }

        private void RecogniserAdded(CpDeviceRecognised aDevice)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceRecogniserList: DeviceRecognised+            Udn{" + aDevice.Udn + "}");

            lock (iLock)
            {
                if (iDisposed)
                {
                    aDevice.Dispose();

                    return;
                }

                iDeviceListPending.Remove(aDevice);

                iDeviceListRecognised.Add(aDevice);
            }

            if (iAdded != null)
            {
                iAdded(this, aDevice);
            }
        }

        private void Removed(CpDeviceList aList, CpDevice aDevice)
        {
            UserLog.WriteLine(DateTime.Now + ": CpDeviceRecogniserList: Device-             Udn{" + aDevice.Udn() + "}");

            CpDeviceRecognised removedDevice = null;

            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                foreach (CpDeviceRecognised d in iDeviceListPending)
                {
                    if (d.Udn == aDevice.Udn())
                    {
                        iDeviceListPending.Remove(d);

                        d.Dispose();

                        return;
                    }
                }

                foreach (CpDeviceRecognised d in iDeviceListRecognised)
                {
                    if (d.Udn == aDevice.Udn())
                    {
                        removedDevice = d;

                        iDeviceListRecognised.Remove(d);

                        break;
                    }
                }
            }

            if (removedDevice != null)
            {
                if (iRemoved != null)
                {
                    iRemoved(this, removedDevice);
                }

                removedDevice.Dispose();
            }
        }

        private CpDeviceListUpnpAll iDeviceListAll;
        private List<CpDeviceRecognised> iDeviceListPending;
        private List<CpDeviceRecognised> iDeviceListRecognised;

        private bool iDisposed;
        private object iLock;
        private IList<IRecogniser> iRecognisers;
        private ChangedHandler iAdded;
        private ChangedHandler iRemoved;
    }
}

