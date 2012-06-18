using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.IO;

using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

namespace Linn.Doktor
{
    public class SupplyUpnp : Supply, ISsdpNotifyHandler
    {
        public class Interface
        {
            private const uint kSsdpSearchTime = 3;
            private const uint kSsdpSearchCount = 1;
            private const int kSsdpSearchDelay = 100; // milliseconds

            public Interface(IPAddress aAddress, ISsdpNotifyHandler aHandler)
            {
                iAddress = aAddress;
                iListenerUnicast = new SsdpListenerUnicast(aHandler);
                iListenerNotify = new SsdpListenerMulticast();
                iListenerNotify.Add(aHandler);
            }
            
            public void Rescan()
            {
                iListenerUnicast.SsdpMsearchAll(kSsdpSearchTime);
    
                uint retry = kSsdpSearchCount;
    
                while (--retry > 0)
                {
                    Thread.Sleep(kSsdpSearchDelay);
                    iListenerUnicast.SsdpMsearchRoot(kSsdpSearchTime);
                }
            }

            public void Open()
            {
                iListenerNotify.Start(iAddress);
                iListenerUnicast.Start(iAddress);
                Rescan();
            }
            
            public void Close()
            {
                iListenerNotify.Stop();
                iListenerUnicast.Stop();
            }
            
            private IPAddress iAddress;
            private SsdpListenerUnicast iListenerUnicast;
            private SsdpListenerMulticast iListenerNotify;
        }

        public SupplyUpnp()
        {
            iMutex = new Mutex();
            iInterfaces = new List<Interface>();
            iActive = new Dictionary<string, Node>();
            iWaiting = new Dictionary<string, Node>();
            iClosed = false;

            string host = Dns.GetHostName();

            IPHostEntry entry = Dns.GetHostEntry(host);

            foreach (IPAddress a in entry.AddressList)
            {
                if (a.AddressFamily == AddressFamily.InterNetwork)
                {
                    iInterfaces.Add(new Interface(a, this));
                }
            }
        }
        
        public override void Open()
        {
            foreach (Interface i in iInterfaces)
            {
                i.Open();
            }
        }
        
        public override void Close()
        {
            foreach (Interface i in iInterfaces)
            {
                i.Close();
            }

            Lock();

            iClosed = true;
            
            Unlock();            
        }
        
        public void Rescan()
        {
            foreach (Interface i in iInterfaces)
            {
                i.Rescan();
            }
        }

        public void NotifyRootAlive(byte[] aUuid, byte[] aLocation, uint aMaxAge)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid,0,aUuid.Length);
            string location = ASCIIEncoding.UTF8.GetString(aLocation, 0, aLocation.Length);
            Add(uuid, location);
        }

        public void NotifyUuidAlive(byte[] aUuid, byte[] aLocation, uint aMaxAge)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid,0,aUuid.Length);
            string location = ASCIIEncoding.UTF8.GetString(aLocation, 0, aLocation.Length);
            Add(uuid, location);
        }

        public void NotifyDeviceTypeAlive(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion, byte[] aLocation, uint aMaxAge)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid,0,aUuid.Length);
            string location = ASCIIEncoding.UTF8.GetString(aLocation, 0, aLocation.Length);
            Add(uuid, location);
        }

        public void NotifyServiceTypeAlive(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion, byte[] aLocation, uint aMaxAge)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid,0,aUuid.Length);
            string location = ASCIIEncoding.UTF8.GetString(aLocation, 0, aLocation.Length);
            Add(uuid, location);
        }

        public void NotifyRootByeBye(byte[] aUuid)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid,0,aUuid.Length);
            Remove(uuid);
        }

        public void NotifyUuidByeBye(byte[] aUuid)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid, 0, aUuid.Length);
            Remove(uuid);
        }

        public void NotifyDeviceTypeByeBye(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid, 0, aUuid.Length);
            Remove(uuid);
        }

        public void NotifyServiceTypeByeBye(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion)
        {
            string uuid = ASCIIEncoding.UTF8.GetString(aUuid, 0, aUuid.Length);
            Remove(uuid);
        }
        
        public const string kAttributeUuid = "Uuid";
        public const string kAttributeLocation = "Location";
        public const string kAttributeDeviceXml = "DeviceXml";

        private void Lock()
        {
            iMutex.WaitOne();
        }
        
        private void Unlock()
        {
            iMutex.ReleaseMutex();
        }

        public class RequestState
        {
            public RequestState(HttpWebRequest aRequest, Node aNode)
            {
                iRequest = aRequest;
                iNode = aNode;
            }
            
            public HttpWebRequest Request
            {
                get
                {
                    return (iRequest);
                }
            }
            
            public Node Node
            {
                get
                {
                    return (iNode);
                }
            }
                 
            HttpWebRequest iRequest;
            Node iNode;
        }
                
        private void RequestDeviceXml(string aUuid, string aLocation)
        {
            Node node = new Node("Upnp");
            
            node.Add(kAttributeUuid, aUuid);
            node.Add(kAttributeLocation, aLocation);
                    
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(aLocation);
            
            // Use a default WebProxy because this is on LAN, not Internet
            
            request.Proxy = new WebProxy();

            RequestState state = new RequestState(request, node);

            try
            {
                request.BeginGetResponse(new AsyncCallback(ResponseDeviceXml), state);
                iWaiting.Add(aUuid, node);
            }
            catch (WebException)
            {
            }
        }
        
        private void ResponseDeviceXml(IAsyncResult aResult)
        {
            Lock();
            
            bool closed = iClosed;
            
            Unlock();
            
            if (closed)
            {
                return;
            }
            
            RequestState state = aResult.AsyncState as RequestState;

            HttpWebRequest request = state.Request;

            Node node = state.Node;

            string uuid = node.Attributes[kAttributeUuid];
            
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());
    
                string xml = reader.ReadToEnd();
    
                Lock();
    
                iWaiting.Remove(uuid);
    
                node.Add(kAttributeDeviceXml, xml);
                
                iActive.Add(uuid, node);
                
                Unlock();
                    
                Add(node);
            }
            catch (WebException)
            {
                Lock();
    
                iWaiting.Remove(uuid);
                
                Unlock();
            }
        }

        private void Add(string aUuid, string aLocation)
        {
            Lock();

            Node node;

            iActive.TryGetValue(aUuid, out node);

            if (node == null)
            {
                iWaiting.TryGetValue(aUuid, out node);
                
                if (node == null)
                {
                    RequestDeviceXml(aUuid, aLocation);
                }
            }

            Unlock();
        }

        private void Remove(string aUuid)
        {
            Lock();
    
            Node node;

            iActive.TryGetValue(aUuid, out node);

            if (node != null)
            {
                iActive.Remove(aUuid);

                Unlock();
                
                Remove(node);
            }
            else
            {
                Unlock();
            }
        }

        private Mutex iMutex;
        private List<Interface> iInterfaces;
        private Dictionary<string, Node> iActive;
        private Dictionary<string, Node> iWaiting;
        private bool iClosed;
    }
}
