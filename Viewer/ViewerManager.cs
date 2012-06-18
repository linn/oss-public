using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;
using Linn;
using Zapp;

namespace Viewer
{
    class ViewerManager : IDisposable
    {
        /*This class is written assuming a DS will gracefully close any viewer connections when it is shutdown, i.e
         * by sending a RST TCP packet. As of r22907 of the mainline volkano branch, it doesn't do this, so 
         * ViewerManager's ConnectionClosed event is not fired as you would expect. However, ViewerManager signals 
         * that services are no longer available by eventing when the DS sends out a multicast Upnp message signaling 
         * its shutdown. We could use this signal to fire the ConnectionClosed event but it was decided that it would 
         * be better not to work around this bug in the DS and rather write the Viewer to cope with a graceful shutdown only.
         */

        public ViewerManager()
        {
            iCurrentIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            iManualResetEvent = new ManualResetEvent(false);

            iTcpListenerThread = new Thread(new ThreadStart(RunTcpServer));
            iTcpListenerThread.Name = "TcpListenerThread";
            iTcpListenerThread.IsBackground = true;
            iTcpListener = new TcpListener(IPAddress.Any, 0);
            iTcpListener.Start();
            iCurrentIPEndPoint.Port = ((IPEndPoint)iTcpListener.LocalEndpoint).Port;
            iManualResetEvent.Set();
            iState = TcpListenenerState.Accepting;

            iUdpClient = new UdpClient();

            iDeviceList = new DeviceListUpnp("linn.co.uk", "Volkano", 1, Dispatcher.CurrentDispatcher);
            iDeviceList.CollectionChanged += EventDeviceListCollectionChanged;
        }

        public void Connect(string aUglyName)
        {
            lock (iConnectLock)
            {
                lock (iDisposalLock)
                {
                    if (iDisposed)
                        throw new ObjectDisposedException("Viewer.ViewerManager");
                }
                if (!iTcpListenerThread.IsAlive)
                {
                    iTcpListenerThread.Start();
                }
                Disconnect();

                iCurrentUglyName = aUglyName;
                aUglyName += ".local"; //this turns the ugly name into a hostname.

                IPAddress[] deviceIPs = Dns.GetHostAddresses(aUglyName);
                iUdpClient.Connect(deviceIPs[0], iViewerControlPort);
                iCurrentIPEndPoint.Address = IPAddress.Parse(deviceIPs[0].ToString());
                iUdpResponseAsyncResult = iUdpClient.BeginReceive(new AsyncCallback(EventUdpDataReceived), null);

                byte[] iOpenConnectionBytes = GetOpenConnectionBytes();
                iUdpClient.Send(iOpenConnectionBytes, iOpenConnectionBytes.Length);
                iUdpDisconnectPending = true;
                CheckIfServicesAvailable();
            }
        }

        private void Disconnect()
        {
            lock (iDisposalLock)
            {
                if (iDisposed)
                    throw new ObjectDisposedException("Viewer.ViewerManager");
            }

            if (!iUdpDisconnectPending)
                return;

            iUdpClient.Connect(iCurrentIPEndPoint.Address, iViewerControlPort);
            iUdpClient.Send(GetCloseConnectionBytes(), GetCloseConnectionBytes().Length);
            iUdpDisconnectPending = false;
            CloseConnection();
        }

        private void CloseConnection()
        {
            lock (iTcpListenerThread)
            {
                if (iState == TcpListenenerState.Handling)
                {
                    iTcpClient.Close();
                    iTcpClient = null;
                    iState = TcpListenenerState.Accepting;
                }
            }
        }

        private byte[] GetOpenConnectionBytes()
        {
            byte[] iOpenMessage = new byte[5];

            iOpenMessage[0] = (byte)'o';
            iManualResetEvent.WaitOne();
            byte[] iPortBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(iCurrentIPEndPoint.Port));
            for (int i = 0; i < 4; i++)
                iOpenMessage[i + 1] = iPortBytes[i];

            return iOpenMessage;
        }

        private void EventUdpDataReceived(IAsyncResult aIAsyncResult)
        {
            lock (iDisposalLock)
            {
                /*If the object is disposed whilst we are waiting for udp data this method will be called
                 * automatically. Therefore throwing an ObjectDisposedException isn't useful so we just return.
                 */
                if (iDisposed)
                    return;
            }

            IPEndPoint temp = new IPEndPoint(iCurrentIPEndPoint.Address, iViewerControlPort);
            byte[] udpClientResponse;
            try
            {
                udpClientResponse = iUdpClient.EndReceive(aIAsyncResult, ref temp);
            }
            catch (SocketException)
            {
                return;
            }
            if (udpClientResponse.Length > 0)
            {
                char response = (char)udpClientResponse[0];
                if (response == 'n')
                {
                    if (ConnectionRefused != null)
                    {
                        ConnectionRefused(this, EventArgs.Empty);
                    }
                }
                else
                {
                    //Ignore accepted ('y') responses here as the ConnectionAccepted
                    //event already handles informing the user of a successful connection.
                } 
            }
        }

        private void EventDeviceListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (DeviceUpnp i in e.NewItems)
                {
                    if (GetUglyName(i.Udn).Equals(iCurrentUglyName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (ServicesAvailable != null)
                            ServicesAvailable(this, new DeviceEventArgs(i));
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (DeviceUpnp i in e.OldItems)
                {
                    if (GetUglyName(i.Udn).Equals(iCurrentUglyName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (ServicesUnavailable != null)
                            ServicesUnavailable(this, new DeviceEventArgs(i));
                    }
                }
            }
            else
            {
                //Current implementation does not support other actions, such as reset.
                throw new ArgumentException("Unsupported Action in NotifyCollectionChangedEventArgs");
            }
        }

        private void CheckIfServicesAvailable()
        {
            //lock object to prevent iDeviceList updating it's list until
            //we are finished enumerating.
            lock (iDeviceList)
            {
                foreach (DeviceUpnp i in iDeviceList)
                {
                    if (GetUglyName(i.Udn).Equals(iCurrentUglyName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (ServicesAvailable != null)
                            ServicesAvailable(this, new DeviceEventArgs(i));
                    }
                }
            }
        }

        private byte[] GetCloseConnectionBytes()
        {
            byte[] iOpenMessage = new byte[5];

            iOpenMessage[0] = (byte)'c';
            byte[] iPortBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(iCurrentIPEndPoint.Port));
            for (int i = 0; i < 4; i++)
                iOpenMessage[i + 1] = iPortBytes[i];

            return iOpenMessage;
        }

        private void RunTcpServer()
        {
            while (true)
            {
                try
                {
                    TcpClient iClient = iTcpListener.AcceptTcpClient();
                    lock (iTcpListenerThread)
                    {
                        iTcpClient = iClient;
                        iState = TcpListenenerState.Handling;
                    }
                }
                //The following 3 exceptions are thrown due to iTcpListener being disposed as ViewerManager is shutting down.
                //We interpret these exceptions as a signal to shutdown the iTcpListenerThread.
                catch (SocketException)
                {
                    return;
                }
                catch (NullReferenceException)
                {
                    return;
                }
                catch (InvalidOperationException)
                {
                    return;
                }

                if (ConnectionAccepted != null)
                {
                    ConnectionAccepted(this, EventArgs.Empty);
                }

                iViewerRecords = new List<ViewerRecord>();
                iViewerOutputSrb = new Srb(iSrbMaxBufferSize, new TcpReaderSource(iTcpClient.GetStream()));
                while (true)
                {
                    try
                    {
                        ViewerRecord record = new ViewerRecord(iViewerOutputSrb);
                        lock (iViewerRecords)
                        {
                            iViewerRecords.Add(record);
                        }
                        if (ViewerOutputAvailable != null)
                        {
                            ViewerOutputAvailable(this, EventArgs.Empty);
                        }
                    }
                    catch (ReaderError)
                    {
                        //remote host has disconnected.
                        if (ConnectionClosed != null)
                        {
                            ConnectionClosed(this, EventArgs.Empty);
                        }
                        break;
                    }
                }
                CloseConnection();
            }
        }

        private byte[] ReadDataFromTcpClient(int aNumberOfBytesToRead)
        {
            NetworkStream iStream = iTcpClient.GetStream();
            byte[] iBytes = new byte[aNumberOfBytesToRead];
            int bytesRead;
            while ((bytesRead = iStream.Read(iBytes, 0, aNumberOfBytesToRead)) < aNumberOfBytesToRead)
            {
                aNumberOfBytesToRead -= bytesRead;
            }
            return iBytes;
        }

        public IPAddress GetClientIPAddress()
        {
            return IPAddress.Parse(iCurrentIPEndPoint.Address.ToString());
        }

        public int GetLocalTcpListenPort()
        {
            return iCurrentIPEndPoint.Port;
        }

        public List<ViewerRecord> ViewerOutput
        {
            get
            {
                lock (iViewerRecords)
                {
                    List<ViewerRecord> output = iViewerRecords;
                    iViewerRecords = new List<ViewerRecord>();
                    return (output);
                }
            }
        }

        public int AvailableRecords
        {
            get
            {
                if (iViewerRecords != null)
                    return iViewerRecords.Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// An ugly name uniquely identifies a Linn volkano device. It can be used to perform an mDNS lookup to obtain
        /// a device's IP address.
        /// </summary>
        /// <param name="aUdn">The udn whose uglyname you want.</param>
        /// <returns>The uglyname corresponding to the given udn.</returns>
        public static string GetUglyName(string aUdn)
        {
            string uglyName = "linn-";

            //Get index of second '-'; where hex block portion of the ugly name is.
            int index = aUdn.IndexOf('-', 0);
            index = aUdn.IndexOf('-', index + 1);

            //skip first two hex bytes.
            index += 3;
            uglyName += aUdn.Substring(index, 2);
            //skip 3rd '-' character.
            index += 3;
            uglyName += aUdn.Substring(index, 4);
            return uglyName;
        }

        /// <summary>
        /// Validates an uglyname. Does not guarantee that the given uglyname is actually in use.
        /// </summary>
        /// <param name="aUglyName">The uglyname you wanted validated</param>
        /// <returns>returns true if the uglyname is valid, otherwise false.</returns>
        public static bool IsValidUglyName(string aUglyName)
        {
            if (aUglyName.Length != 11)
                return false;
            else if (!aUglyName.StartsWith("linn-", StringComparison.OrdinalIgnoreCase))
                return false;

            string hexPart = aUglyName.Substring(aUglyName.IndexOf('-') + 1);
            string validHexChars = "1234567890ABCDEFabcdef";
            foreach (char c in hexPart)
            {
                if (!validHexChars.Contains(c.ToString()))
                    return false;
            }
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (iDisposalLock)
            {
                if (!iDisposed)
                {
                    if (disposing)
                    {
                        Disconnect();
                        iUdpClient.Close();

                        if (iTcpListenerThread.IsAlive)
                        {
                            iTcpListener.Stop();
                            iTcpListenerThread.Join();
                        }
                    }
                    iDeviceList.Dispose();
                    iDisposed = true;
                }
            }
        }

        ~ViewerManager()
        {
            Dispose(false);
        }

        private Thread iTcpListenerThread;
        private TcpListener iTcpListener;
        private enum TcpListenenerState { Accepting, Handling };
        private TcpListenenerState iState;
        //true if a Udp connect datagram has been sent but no corresponding disconnect packet has been sent yet.
        private bool iUdpDisconnectPending = false;
        //keeps track of the local TCP listen port and the remote device IP.
        private IPEndPoint iCurrentIPEndPoint;
        private string iCurrentUglyName;
        private ManualResetEvent iManualResetEvent;
        private Object iConnectLock = new Object();
        private Srb iViewerOutputSrb;
        private int iSrbMaxBufferSize = 1000000; // ~ 1MB - should be plenty, can be tweaked if the Viewer seems sluggish.
        private List<ViewerRecord> iViewerRecords;
        private UdpClient iUdpClient;
        private IAsyncResult iUdpResponseAsyncResult;
        private TcpClient iTcpClient;
        private int iViewerControlPort = 50000;
        private bool iDisposed = false;
        private object iDisposalLock = new Object();
        private DeviceListUpnp iDeviceList;
        public EventHandler<EventArgs> ViewerOutputAvailable;
        public EventHandler<EventArgs> ConnectionRefused;
        public EventHandler<EventArgs> ConnectionAccepted;
        public EventHandler<EventArgs> ConnectionClosed;
        public EventHandler<DeviceEventArgs> ServicesAvailable;
        /*A user of this class might want to connect to multiple devices simultanously, 
         * so we pass a DeviceEventArgs here instead of just EventArgs.Empty*/
        public EventHandler<DeviceEventArgs> ServicesUnavailable;
    }

    class ViewerRecord
    {
        internal ViewerRecord(Srb iViewerOutputSrb)
        {
            byte[] id = iViewerOutputSrb.Read(4);
            byte[] timestamp = iViewerOutputSrb.Read(4);
            byte[] length = iViewerOutputSrb.Read(4);

            iId = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(id, 0));
            iTimestamp = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(timestamp, 0));
            iPayloadLength = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(length, 0));
            iPayload = iViewerOutputSrb.Read((int)iPayloadLength);
        }

        internal ViewerRecord(BinaryReader aBinaryInput)
        {
            iId = aBinaryInput.ReadUInt32();
            iTimestamp = aBinaryInput.ReadUInt32();
            iPayloadLength = aBinaryInput.ReadUInt32();

            iPayload = aBinaryInput.ReadBytes((int)iPayloadLength);
        }

        public int Size
        {
            get
            {
                return iPayload.Length + 3*sizeof(uint);
            }
        }

        public uint Id
        {
            get
            {
                return (iId);
            }
        }

        public uint Timestamp
        {
            get
            {
                return (iTimestamp);
            }
        }

        public uint PayloadLength
        {
            get
            {
                return iPayloadLength;
            }
        }

        public byte[] Payload
        {
            get
            {
                return (iPayload);
            }
        }

        public override string ToString()
        {
            return System.Text.Encoding.UTF8.GetString(iPayload);
        }

        private uint iId;
        private uint iTimestamp;
        private uint iPayloadLength;
        private byte[] iPayload;
    }

    class TcpReaderSource : IReaderSource
    {
        public TcpReaderSource(NetworkStream aNetworkStream)
        {
            iNetworkStream = aNetworkStream;
        }

        public int Read(byte[] aBuffer, int aOffset, int aMaxBytes)
        {
            try
            {
                int bytesRead = iNetworkStream.Read(aBuffer, aOffset, aMaxBytes);
                if (bytesRead == 0)
                    throw new ReaderError();
                else
                    return bytesRead;
            }
            catch (ObjectDisposedException)
            {
                throw new ReaderError();
            }
            catch (IOException)
            {
                throw new ReaderError();
            }
        }   

        public void ReadFlush()
        {
            //this has no effect as NetworkStream is not buffered, only
            //here to implement a required method from IReaderSource
            iNetworkStream.Flush();
        }

        NetworkStream iNetworkStream;
    }

    public class DeviceEventArgs : EventArgs
    {
        public DeviceEventArgs(DeviceUpnp aDevice)
        {
            iDevice = aDevice;
        }

        public DeviceUpnp Device
        {
            get
            {
                return iDevice;
            }
        }

        private DeviceUpnp iDevice;
    }
}
