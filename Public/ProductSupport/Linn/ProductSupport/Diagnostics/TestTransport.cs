// Check transport layer communications (TCP and UDP)

// Checks for operation of the comms, and will additionally get information on network operation
// (latency/packet loss etc.). For any testing which involves timing measurements will use the
// assumption that code processing time is negligible compared to the network timings being measured
// (which holds true except when measuring timings on wired networks, in which case there should be 
// no timing issues anyway) 

// TCP messages received by DS on port 0xec01 (60417) are echoed back as TCP messages
// UNIcast UDP messages received by DS on port 0xec02 (60418) are echoed as UNIcast UDP messages
// UNIcast UDP messages received by DS on port 0xec03 (60419) are echoed back as MULTIcast UDP messages to 239.250.250.12:13966
// MULTIcast UDP messages received by DS on port 0xec04 (60420) are echoed back as UNIcast UDP messages to port 12967


using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Linn.ControlPoint;
using Linn.Control.Nct;

namespace Linn.ProductSupport.Diagnostics
{
    internal enum EProtocol
    {
        eTcp,
        eUdp,
        eUdpMulti
    }

    //-----------------------------------------------------------------------------
    // Transport level diagnostics testing
    //-----------------------------------------------------------------------------
    internal class TestTransport : TestBase
    {
        private static int iTimeout = 1000;

        public TestTransport(EProtocol aTxProtocol, EProtocol aRxProtocol, string aLocalAddr, string aRemoteAddr, Logger aLog, ETest aTest)
            : base(aLocalAddr, aLog, aTest)
        {
            if (aRemoteAddr == null)
            {
                iAbortMsg = "DS device undefined";
                iAbortTest = true;
                return;
            }
            try
            {
                if ((aTxProtocol == EProtocol.eTcp) && (aRxProtocol == EProtocol.eTcp))
                {
                    iConx = new TcpConx(aLocalAddr, aRemoteAddr);
                }
                else if ((aTxProtocol == EProtocol.eUdp) && (aRxProtocol == EProtocol.eUdp))
                {
                    iConx = new UdpConx(aLocalAddr, aRemoteAddr);
                }
                else if ((aTxProtocol == EProtocol.eUdp) && (aRxProtocol == EProtocol.eUdpMulti))
                {
                    iConx = new UdpMultiRespondConx(aLocalAddr, aRemoteAddr);
                }
                else if ((aTxProtocol == EProtocol.eUdpMulti) && (aRxProtocol == EProtocol.eUdp))
                {
                    iConx = new UdpMultiSenderConx(aLocalAddr, aRemoteAddr);
                }
                else
                {
                    Assert.Check(false, "Invalid test configuration");
                }
            }
            catch (Exception e)
            {
                iAbortMsg = e.ToString();
                iAbortTest = true;
            }
        }

        public override void ExecuteTest()
        {
            if (iAbortTest)
            {
                iLog.Fail(iAbortMsg);
            }
            else
            {
                iConx.StartRx(DataRxCb);
                if (!iKill)
                {
                    Execute(100, 50);       // 100 un-fragmented packets
                }
                if (!iKill)
                {
                    Execute(100, 2000);     // 100 fragmented packets
                }
                iConx.StopRx();
            }
        }

        public override void Shutdown()
        {
            if (iConx != null)
            {
                iConx.Shutdown();
            }
        }

        protected void Execute(int aLoops, int aMessageLength)
        {
            iStart = 0;
            iPeak = 0;
            iMessageCounter = 0;
            iMissingMsg = 0;
            iTxMessageQ.Clear();
            iRxMessageQ.Clear();

            iLog.Subtest(aLoops.ToString() + " loops of " + aMessageLength.ToString() + " bytes between " + iConx.localEp + " and " + iConx.remoteEp);
            try
            {
                iTimer.Reset();

                for (int loop = 0; loop < aLoops; loop++)
                {
                    string message = TestMessage(aMessageLength);
                    iTxMessageQ.Enqueue(message);
                    iStart = iTimer.Peek();
                    iConx.Transmit(message);
                    if (!CheckMessageRx())
                    {
                        break;
                    }
                }
                long elapsed = iTimer.Peek();
                ProcessResults(aLoops, elapsed);
            }
            catch (Exception e)
            {
                iLog.Fail(e.ToString());
            }
        }

        public void DataRxCb(string aMessage)
        {
            // called by receiver thread on receipt of data
            iMessageRx.Set();
            iRxMessageQ.Enqueue(aMessage);

            long now = iTimer.Peek();
            long latency = now - iStart;
            if (latency > iPeak)
            {
                iPeak = latency;
            }
        }

        private bool CheckMessageRx()
        {
            bool rc = true;
            if (!iMessageRx.WaitOne(iTimeout))
            {
                iLog.Warn("Failed to receive response (timed out)");
                iMissingMsg += 1;
                if (iMissingMsg > 4)
                {
                    iLog.Warn("Abandoning test after 5 consecutive lost messages");
                    rc = false;
                }
            }
            else
            {
                iMissingMsg = 0;
            }
            return rc;
        }

        private void ProcessResults(int aLoops, long aElapsed)
        {
            // could check data in tx vs rx lists, but extremely unlikely to see issues other
            // than missing packets which can be more easily detected using message count

            int sentMessages = iTxMessageQ.Count;
            int lostMessages = iTxMessageQ.Count - iRxMessageQ.Count;
            long aveLatency = 0;

            iLog.Info("Sent messages:- " + sentMessages.ToString());
            iLog.Info("Lost messages:- " + lostMessages.ToString());
            if (sentMessages - lostMessages > 0)
            {
                aveLatency = (aElapsed - (iTimeout * lostMessages)) / (sentMessages - lostMessages);
                iLog.Info("Average latency:- " + aveLatency.ToString() + "ms");
                iLog.Info("Peak latency:- " + iPeak.ToString() + "ms");
            }

            if (lostMessages > 0)
            {
                iLog.Fail("Messages lost");
            }
            else if (aveLatency > 250)
            {
                iLog.Fail("High average latancy - " + aveLatency.ToString() + "ms");
            }
            else if (iPeak > 1000)
            {
                iLog.Fail("High peak latancy - " + iPeak.ToString() + "ms");
            }
            else
            {
                iLog.Pass("");
            }

        }

        private string TestMessage(int aLength)
        {
            iMessageCounter += 1;
            string s = "abcdefghijklmnopqrstuvwxyz";
            string message = iMessageCounter.ToString("D8");
            while (message.Length < aLength)
            {
                message += s;
            }
            message = message.Remove(aLength - 3);
            message += "EOD";
            return message;
        }

        private bool iAbortTest = false;
        private string iAbortMsg = "";
        private long iStart = 0;
        private long iPeak = 0;
        private int iMessageCounter = 0;
        private int iMissingMsg = 0;
        private Conx iConx = null;
        private HiResTimer iTimer = new HiResTimer();
        private EventWaitHandle iMessageRx = new EventWaitHandle(false, EventResetMode.AutoReset);
        protected Queue<string> iRxMessageQ = new Queue<string>();
        protected Queue<string> iTxMessageQ = new Queue<string>();
    }


    //-----------------------------------------------------------------------------
    // Conx class hierarchy - socket level comms for transport diagnostics
    //-----------------------------------------------------------------------------
    internal abstract class Conx
    {
        protected const int kDsTcpPort = 60417;             // ports and ip addresses for diagnostic
        protected const int kDsUdpPort = 60418;             // comms with DS. These are hard-coded on
        protected const int kDsMultiRespPort = 60419;       // the device side
        protected const int kDsMultiSendPort = 60420;
        protected const string kMultiRcvAddr = "239.250.250.12";
        protected const string kMultiSendAddr = "239.255.255.250";      // UPnP address 
        protected const int kMultiRcvPort = 13966;
        protected const int kMultiAckPort = 13967;
        protected const string kStopRx = "STOP_RX";

        // The MultiSend uses UPnP address because the device side is limited to 4 simultaneous multicast group
        // memberships, and is already at theis limit (if multicast songcast is enabled)

        public delegate void RxDataCb(string aMessage);

        public Conx()
        {
            iRxStarted = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        public void Shutdown()
        {
            iRxSock.Close();
            if (iTxSock != iRxSock)
            {
                iTxSock.Close();
            }
        }

        public virtual void Transmit(string aMessage)
        {
            iTxSock.SendTo(Encoding.ASCII.GetBytes(aMessage), iRemoteEp);
        }

        public void StartRx(RxDataCb aRxCb)
        {
            iRxThread = new Thread(new ThreadStart(Receive));
            iRxThread.Start();
            iRxCallback = aRxCb;
            iRxStarted.WaitOne();   // wait for thread to start-up
        }

        public virtual void StopRx()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            sock.SendTo(Encoding.ASCII.GetBytes(kStopRx), iRxSock.LocalEndPoint);
            iRxThread.Join();
            Thread.Sleep(1000);     // delay to allow socket to be fully flushed
        }

        protected virtual void Receive()
        {
            byte[] rxData = new byte[4096];
            iRxStarted.Set();       // signal that thread is operational

            while (true)
            {
                try
                {
                    int bytes = iRxSock.Receive(rxData);
                    string rxMessage = Encoding.ASCII.GetString(rxData.Slice(0, bytes));
                    if (rxMessage == kStopRx)
                    {
                        break;
                    }
                    iRxCallback(rxMessage);
                }
                catch
                {
                    break;
                }
            }
        }

        public string localEp
        {
            get
            {
                return iLocalEp.ToString();
            }
        }

        public string remoteEp
        {
            get
            {
                return iRemoteEp.ToString();
            }
        }

        protected Socket iRxSock;
        protected Socket iTxSock;
        protected IPEndPoint iLocalEp;
        protected IPEndPoint iRemoteEp;
        protected RxDataCb iRxCallback;
        protected EventWaitHandle iRxStarted;
        protected Thread iRxThread;
    }


    internal class TcpConx : Conx
    {
        public TcpConx(string aLocalAddr, string aRemoteAddr)
        {
            iLocalEp = new IPEndPoint(IPAddress.Parse(aLocalAddr), 0);
            iRemoteEp = new IPEndPoint(IPAddress.Parse(aRemoteAddr), kDsTcpPort);
            iRxSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            iRxSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            iRxSock.Bind(iLocalEp);
            iRxSock.Connect(iRemoteEp);
            iTxSock = iRxSock;
        }

        public override void Transmit(string aMessage)
        {
            iTxSock.Send(Encoding.ASCII.GetBytes(aMessage));
        }

        public override void StopRx()
        {
            try
            {
                iTxSock.Send(Encoding.ASCII.GetBytes(kStopRx));
                iRxThread.Join();
            }
            catch
            {
            }
            Thread.Sleep(1000);     // delay to allow socket to be fully flushed
        }
    }


    internal class UdpConx : Conx
    {
        public UdpConx(string aLocalAddr, string aRemoteAddr)
        {
            iLocalEp = new IPEndPoint(IPAddress.Parse(aLocalAddr), 0);
            iRemoteEp = new IPEndPoint(IPAddress.Parse(aRemoteAddr), kDsUdpPort);
            iRxSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            iRxSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            iRxSock.Bind(iLocalEp);
            iTxSock = iRxSock;
        }
    }


    internal class UdpMultiRespondConx : Conx
    {
        public UdpMultiRespondConx(string aLocalAddr, string aRemoteAddr)
        {
            iLocalEp = new IPEndPoint(IPAddress.Parse(aLocalAddr), kMultiRcvPort);
            IPAddress multiGrp = IPAddress.Parse(kMultiRcvAddr);
            IPEndPoint multiEp = new IPEndPoint(multiGrp, kMultiRcvPort);

            iRxSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                iRxSock.Bind(multiEp);
            }
            else
            {
                iRxSock.Bind(iLocalEp);
            }
            iRxSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            iRxSock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multiGrp, IPAddress.Parse(aLocalAddr)));

            iRemoteEp = new IPEndPoint(IPAddress.Parse(aRemoteAddr), kDsMultiRespPort);
            iTxSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            iTxSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
        }
    }


    internal class UdpMultiSenderConx : Conx
    {
        public UdpMultiSenderConx(string aLocalAddr, string aRemoteAddr)
        {
            iRemoteAddr = aRemoteAddr;
            iLocalEp = new IPEndPoint(IPAddress.Parse(aLocalAddr), kMultiAckPort);
            iRemoteEp = new IPEndPoint(IPAddress.Parse(kMultiSendAddr), kDsMultiSendPort);

            iRxSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            iRxSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            iRxSock.Bind(iLocalEp);
            iTxSock = iRxSock;
        }

        protected override void Receive()
        {
            // MulticastSender specific variant filters out responses from all devices except
            // the DUT (as all DS on network will respond to the multicast trigger messages.
            byte[] rxData = new byte[4096];
            EndPoint remoteEp = new IPEndPoint(0, 0);
            iRxStarted.Set();   // signal to StartRx that thread is operational

            while (true)
            {
                try
                {
                    int bytes = iRxSock.ReceiveFrom(rxData, ref remoteEp);
                    IPEndPoint ep = (IPEndPoint)remoteEp;

                    if (ep.Address.Equals(((IPEndPoint)(iRxSock.LocalEndPoint)).Address))
                    {
                        if (Encoding.ASCII.GetString(rxData.Slice(0, bytes)) == kStopRx)
                        {
                            break;
                        }
                    }

                    if (ep.Address.ToString().Equals(iRemoteAddr))
                    {
                        string rxMessage = Encoding.ASCII.GetString(rxData.Slice(0, bytes));
                        if (bytes > 2)
                        {
                            iRxCallback(rxMessage);
                        }
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        private string iRemoteAddr;
    }
}
