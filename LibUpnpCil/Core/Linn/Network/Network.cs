using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Linn;

namespace Linn.Network
{
    // TcpStream

    public class NetworkError : Exception
    {
    }

    public class TcpServer
    {
        private const int kListenSlots = 100;

        private const int kMinPort = 49832;
        private const int kMaxPort = 49932;

        public TcpServer(IPAddress aInterface)
        {
            if (aInterface == IPAddress.Any)
            {
                IPHostEntry e = Dns.GetHostEntry(Dns.GetHostName());

                foreach (IPAddress i in e.AddressList)
                {
                    // find an IPV4 IPAddress

                    if (i.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Initialise(i);
                        return;
                    }
                }

                throw (new NetworkError());
            }
            else
            {
                Initialise(aInterface);
            }
        }

        private void Initialise(IPAddress aInterface)
        {
            for (int port = kMinPort; port <= kMaxPort; port++)
            {
                // On vista, a new socket has to be created for each attempted bind - if a socket is
                // created and then bound, and the bind fails, attempting to bind the same socket
                // to another port always fails (on vista).
                // Also, the creation of the socket goes outside the try..catch because we want
                // SocketExceptions that occur on creation to be handled by the client code. The handling
                // code for the SocketException here is specifically for the bind.

                try
                {
                    UserLog.Write("TcpServer+    " + aInterface + ":" + port + "...");
                    iListener = new TcpListener(aInterface, port);

                    iListener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

#if PocketPC
                    iListener.Start();
#else
                    iListener.Start(kListenSlots);
#endif
                    Endpoint = new IPEndPoint(aInterface, port);

                    UserLog.WriteLine("Success");
                    return;
                }
                catch (SocketException e)
                {
                    UserLog.WriteLine("Failed (" + e.Message + ")");
                }
            }

            throw (new NetworkError());
        }

        public void Accept(TcpSessionStream aSession)
        {
        	try
        	{
            	aSession.SetSocket(iListener.AcceptSocket());
            }
            catch (SocketException)
            {
                throw (new NetworkError());
            }
            catch (ObjectDisposedException)
            {
                throw (new NetworkError());
            }
        }

        public void Shutdown()
        {
            try
            {
                UserLog.Write("TcpServer-    " + Endpoint.Address + ":" + Endpoint.Port + "...");
                iListener.Stop();
                UserLog.WriteLine("Success");
            }
            catch (SocketException e)
            {
                UserLog.WriteLine("Failed (" + e.Message + ")");
                throw (new NetworkError());
            }
            catch (ObjectDisposedException e)
            {
                UserLog.WriteLine("Failed (" + e.Message + ")");
                throw (new NetworkError());
            }
        }

        public void Close()
        {
            iListener.Server.Close();
        }

        public IPEndPoint Endpoint;
        private TcpListener iListener;
    }

    public class TcpStream : IWriter, IReaderSource
    {
        protected TcpStream()
        {
        }

        // IWriter

        public void Write(byte aValue)
        {
            Assert.Check(false);
        }

        public void Write(byte[] aBuffer)
        {
            try
            {
                iSocket.Send(aBuffer);
            }
            catch (SocketException)
            {
                throw (new WriterError());
            }
            catch (ObjectDisposedException)
            {
                throw (new WriterError());
            }
        }

        public void WriteFlush()
        {
        }

        // IReaderSource

        public int Read(byte[] aBuffer, int aOffset, int aMaxBytes)
        {
            try
            {
                return (iSocket.Receive(aBuffer, aOffset, aMaxBytes, SocketFlags.None));
            }
            catch (SocketException)
            {
                throw (new ReaderError());
            }
            catch (ObjectDisposedException)
            {
                throw (new ReaderError());
            }
        }

        public void ReadFlush()
        {
        }

        // Socket Control

        public void Connect(EndPoint aEndpoint)
        {

            try
            {
                iSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                iSocket.Connect(aEndpoint);
            }
            catch (SocketException)
            {
                throw (new NetworkError());
            }
            catch (ObjectDisposedException)
            {
                throw (new NetworkError());
            }
        }

        public void BeginConnect(EndPoint aEndpoint, AsyncCallback aCallback)
        {

            try
            {
                iSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                iSocket.BeginConnect(aEndpoint, aCallback, null);
            }
            catch (SocketException)
            {
                throw (new NetworkError());
            }
            catch (ObjectDisposedException)
            {
                throw (new NetworkError());
            }
        }

        public void EndConnect(IAsyncResult aAsync)
        {
            try
            {
                iSocket.EndConnect(aAsync);
            }
            catch (SocketException)
            {
                throw (new NetworkError());
            }
            catch (ObjectDisposedException)
            {
                throw (new NetworkError());
            }
        }


        public void BeginWaitForData(AsyncCallback aCallback)
        {
            byte[] buffer = new byte[0];

            try
            {
                iSocket.BeginReceive(buffer, 0, 0, SocketFlags.None, aCallback, null);
            }
            catch (SocketException)
            {
                throw (new NetworkError());
            }
            catch (ObjectDisposedException)
            {
                throw (new NetworkError());
            }
        }

        public void EndWaitForData(IAsyncResult aAsync)
        {
            try
            {
                iSocket.EndReceive(aAsync);
            }
            catch (SocketException)
            {
                throw (new NetworkError());
            }
            catch (ObjectDisposedException)
            {
                throw (new NetworkError());
            }
        }

        public void Close()
        {
            if (iSocket != null)
            {
                iSocket.Close();
            }
        }

        protected Socket iSocket;
    }

    // TcpSessionStream

    public class TcpSessionStream : TcpStream
    {
        public TcpSessionStream()
        {
        }

        public void SetSocket(Socket aSocket)
        {
            Assert.Check(aSocket != null);
            iSocket = aSocket;
        }
    }


    // TcpClientStream

    public class TcpClientStream : TcpStream
    {
        public TcpClientStream()
        {
        }
    }

    // UdpMulticastStream - Write multicast, Read unicast


    public class UdpMulticastStream : IWriter, IReaderSource
    {
        private const int kMinPort = 47832;
        private const int kMaxPort = 47932;

        public UdpMulticastStream(IPAddress aInterface, IPAddress aMulticast, int aPort, int aTtl)
        {
            Assert.Check(aTtl < 256);

            iMulticast = new IPEndPoint(aMulticast, aPort);

            iReadOpen = true;

            iWriteOpen = true;

            iSender = new IPEndPoint(IPAddress.Any, 0);

            for (int port = kMinPort; port <= kMaxPort; port++)
            {
                // On vista, a new socket has to be created for each attempted bind - if a socket is
                // created and then bound, and the bind fails, attempting to bind the same socket
                // to another port always fails (on vista).
                // Also, the creation of the socket goes outside the try..catch because we want
                // SocketExceptions that occur on creation to be handled by the client code. The handling
                // code for the SocketException here is specifically for the bind.

                Trace.WriteLine(Trace.kUpnp, DateTime.Now + ": UdpMulticastStream+    " + aInterface + ":" + port + "...");
                try
                {
                    iSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    iSocket.Bind(new IPEndPoint(aInterface, port));
                    iSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
                    iSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, aTtl);
                    
                    Trace.WriteLine(Trace.kUpnp, "Success");
                    return;
                }
                catch (SocketException e)
                {
                    Trace.WriteLine(Trace.kUpnp, "Failed (" + e.Message + ")");
                }
            }

            throw (new NetworkError());
        }

        public EndPoint Sender()
        {
            return (iSender);
        }

        // IWriter

        public void Write(byte aValue)
        {
            Assert.Check(false);
        }

        public void Write(byte[] aBuffer)
        {
            if (iWriteOpen)
            {
                try
                {
                    iSocket.SendTo(aBuffer, iMulticast);
                    iWriteOpen = false;
                }
                catch (SocketException)
                {
                    throw (new WriterError());
                }
                catch (ObjectDisposedException)
                {
                    throw (new WriterError());
                }
            }
            else
            {
                throw (new WriterError());
            }
        }

        public void WriteFlush()
        {
            iWriteOpen = true;
        }

        // IReaderSource

        public int Read(byte[] aBuffer, int aOffset, int aMaxBytes)
        {
            if (iReadOpen)
            {
                iReadOpen = false;

                int count = 0;

                while (count == 0)
                {
                    try
                    {
                        count = iSocket.ReceiveFrom(aBuffer, aOffset, aMaxBytes, SocketFlags.None, ref iSender);
                    }
                    catch (SocketException) { break; }
                    catch (ObjectDisposedException) { break; }

                }

                return (count);
            }

            throw (new ReaderError());
        }

        public void ReadFlush()
        {
            iReadOpen = true;
        }

        public void Shutdown()
        {
            try
            {
                iSocket.Blocking = false;
                iSocket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }

        public void Close()
        {
            try
            {
                iSocket.Close();
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }

        private IPEndPoint iMulticast;

        private Socket iSocket;

        private bool iReadOpen;
        private bool iWriteOpen;

        private EndPoint iSender;
    }

    // UdpMulticastReader - Read multicast

    public class UdpMulticastReader : IReaderSource
    {
        public UdpMulticastReader(IPAddress aInterface, IPAddress aMulticast, int aPort)
        {
            iMulticast = new IPEndPoint(aMulticast, aPort);

            try
            {

                iSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                iSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        // Unix - bind to multicast address
                        iSocket.Bind(iMulticast);
                        break;
                    default:
                        // Windows - bind to local interface
                        iSocket.Bind(new IPEndPoint(aInterface, aPort));
                        break;
                }


                // Join the multicast group
                MulticastOption option = new MulticastOption(aMulticast, aInterface);
                iSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, option);

                iReadOpen = true;

                iSender = new IPEndPoint(IPAddress.Any, 0);
            }
            catch (SocketException)
            {
                throw (new NetworkError());
            }
            catch (ObjectDisposedException)
            {
                throw (new NetworkError());
            }
        }

        public EndPoint Sender()
        {
            return (iSender);
        }

        // IReaderSource

        public int Read(byte[] aBuffer, int aOffset, int aMaxBytes)
        {
            if (iReadOpen)
            {
                iReadOpen = false;

                int count = 0;

                ManualResetEvent semaphore = new ManualResetEvent(false);

                while (count == 0)
                {
                    try
                    {
                        count = iSocket.ReceiveFrom(aBuffer, aOffset, aMaxBytes, SocketFlags.None, ref iSender);   
                    }
                    catch (SocketException) { break; }
                    catch (ObjectDisposedException) { break; }
                }

                return (count);
            }

            throw (new ReaderError());
        }

        public void ReadFlush()
        {
            iReadOpen = true;
        }

        public void Shutdown()
        {
            try
            {
                iSocket.Blocking = false;
                iSocket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }

        public void Close()
        {
            try
            {
                iSocket.Close();
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }

        private IPEndPoint iMulticast;

        private Socket iSocket;

        private bool iReadOpen;

        private EndPoint iSender;
    }
}
