
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using Linn;


namespace Linn.Songcast
{
    public class NetworkMonitor
    {
        public NetworkMonitor(IInvoker aInvoker, uint aLatencyMs)
        {
            iInvoker = aInvoker;
            iLatencyMs = aLatencyMs;
            iSessionId = 0;
        }
        
        public uint LatencyMs
        {
            get { return iLatencyMs; }
        }
        
        public static uint Performance(uint aLatency)
        {
            if (aLatency == LatencyMsExcellent) {
                return 0;
            }
            else if (aLatency == LatencyMsGood) {
                return 1;
            }
            else if (aLatency == LatencyMsPoor) {
                return 2;
            }
            else if (aLatency == LatencyMsVeryPoor) {
                return 3;
            }
            else {
                return 3;
            }
        }

        public int DetectProgressPercent
        {
            get { return (iSender != null) ? (int)iSender.ProgressPercent : -1; }
        }
        
        public void DetectLatency(IPAddress aAddress)
        {
            if (iSender != null)
            {
                iSender.EventFinished -= SenderFinished;
                iSender.EventProgressChanged -= SenderProgressChanged;
                iSender.Dispose();
                iSender = null;

                iRetriever.EventResultsChanged -= RetrieverResultsChanged;
                iRetriever.Dispose();
                iRetriever = null;
            }

            IPEndPoint ep = new IPEndPoint(aAddress, 8889);
            const uint periodMs = 50;
            const uint bytes = 1024;
            const uint iters = 600;
            iSessionId++;

            // create and start the sender
            iSender = new NetmonSender(iInvoker, ep, periodMs, bytes, iters, iSessionId);
            iSender.EventFinished += SenderFinished;
            iSender.EventProgressChanged += SenderProgressChanged;
            iSender.Start();

            // create and start the retriever
            iRetriever = new NetmonRetriever(iInvoker, ep, iSessionId);
            iRetriever.EventResultsChanged += RetrieverResultsChanged;
            iRetriever.Start();

            if (EventProgressChanged != null) {
                EventProgressChanged(this, EventArgs.Empty);
            }
        }
        
        public event EventHandler EventLatencyChanged;
        public event EventHandler EventProgressChanged;

        private void SenderProgressChanged(object sender, EventArgs e)
        {
            if (sender == iSender && EventProgressChanged != null) {
                EventProgressChanged(this, EventArgs.Empty);
            }
        }

        private void SenderFinished(object sender, EventArgs e)
        {
            if (sender != iSender) {
                return;
            }

            // dispose of the sender
            iSender.EventFinished -= SenderFinished;
            iSender.EventProgressChanged -= SenderProgressChanged;
            iSender.Dispose();
            iSender = null;

            // just skip the last couple of results for the retriever and dispose it
            NetmonRetriever.Results results = iRetriever.GetResults();
            iRetriever.EventResultsChanged -= RetrieverResultsChanged;
            iRetriever.Dispose();
            iRetriever = null;

            UserLog.WriteLine(String.Format("{0} Netmon Final: T:{1} O:{2} D:{3} M:{4} Min:{5} Max:{6} StdDev:{7}", DateTime.Now, results.Total, results.OutOfSeq, results.Duplicates, results.Missed, results.Min, results.Max, results.StdDev));
            
            // make sure latency is updated with the final results
            iLatencyMs = CalculateLatency(results);

            if (EventLatencyChanged != null) {
                EventLatencyChanged(this, EventArgs.Empty);
            }

            if (EventProgressChanged != null) {
                EventProgressChanged(this, EventArgs.Empty);
            }
        }

        private void RetrieverResultsChanged(object sender, EventArgs e)
        {
            if (sender != iRetriever) {
                return;
            }

            // get the current results from the retriever
            NetmonRetriever.Results results = iRetriever.GetResults();

            UserLog.WriteLine(String.Format("{0} Netmon: T:{1} O:{2} D:{3} M:{4} Min:{5} Max:{6} StdDev:{7}", DateTime.Now, results.Total, results.OutOfSeq, results.Duplicates, results.Missed, results.Min, results.Max, results.StdDev));

            // calculate the current latency for these results
            uint latencyMs = CalculateLatency(results);

            // update the latency only if is is greater than the current latency - this means that the latency will only change during the run if
            // if is larger than what it was at the start of the run
            if (latencyMs > iLatencyMs)
            {
                iLatencyMs = latencyMs;

                if (EventLatencyChanged != null) {
                    EventLatencyChanged(this, EventArgs.Empty);
                }
            }
        }

        private uint CalculateLatency(NetmonRetriever.Results aResults)
        {
            int dt = aResults.Max + aResults.Min;

            const uint ThresholdExcellent = 10;
            const uint ThresholdGood = 50;
            const uint ThresholdPoor = 100;

            if (dt < ThresholdExcellent)
            {
                return LatencyMsExcellent;
            }
            else if (dt < ThresholdGood)
            {
                return LatencyMsGood;
            }
            else if (dt < ThresholdPoor)
            {
                return LatencyMsPoor;
            }
            else
            {
                return LatencyMsVeryPoor;
            }
        }
        
        private const uint LatencyMsExcellent = 50;
        private const uint LatencyMsGood = 100;
        private const uint LatencyMsPoor = 200;
        private const uint LatencyMsVeryPoor = 500;
        
        private IInvoker iInvoker;
        private uint iLatencyMs;
        private uint iSessionId;
        private NetmonSender iSender;
        private NetmonRetriever iRetriever;
    }


    // class for a netmon sender that sends packets to the receiver
    public class NetmonSender
    {
        public NetmonSender(IInvoker aInvoker, IPEndPoint aEndpoint, uint aPeriodMs, uint aBytes, uint aIterations, uint aId)
        {
            iInvoker = aInvoker;
            iEndpoint = aEndpoint;
            iPeriodMs = aPeriodMs;
            iBytes = aBytes;
            iIterations = aIterations;
            iId = aId;
            iProgressPercent = 0;
            iAbort = false;
        }

        public void Start()
        {
            if (iThread == null) {
                iThread = new Thread(Run);
                iThread.IsBackground = true;
                iThread.Name = "NTMS";
                iThread.Start();
            }
        }
        
        public void Dispose()
        {
            if (iThread != null)
            {
                iAbort = true;
                iThread.Join();
                iThread = null;
            }
        }

        public uint ProgressPercent
        {
            get { return iProgressPercent; }
        }

        public event EventHandler EventProgressChanged;
        public event EventHandler EventFinished;

        private void Run()
        {
            // create a buffer for sending data - the first 16 bytes will be filled with the netmon header
            // info and the rest is unimportant
            byte[] buffer = new byte[iBytes];

            // create the udp socket for sending
            UdpClient udpClient = new UdpClient();

            // create a diagnostics stopwatch to perform timestamping - start it at t=0
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Reset();
            stopWatch.Start();

            // start the send loop
            for (uint i = 0; i < iIterations; i++)
            {
                // sleep until the next packet is due
                uint dueTimeMs = (i + 1) * iPeriodMs;
                int fireInMs = (int)dueTimeMs - (int)stopWatch.ElapsedMilliseconds;
                if (fireInMs > 0)
                {
                    Thread.Sleep(fireInMs);
                }

                // fill buffer for next packet
                if (!iAbort)
                {
                    NetmonEvent ev = new NetmonEvent(iId, i, (uint)(stopWatch.ElapsedMilliseconds * 1000));
                    ev.Write(buffer);
                }
                else
                {
                    // an empty packet signals that no more will be sent
                    Array.Clear(buffer, 0, 12);
                }

                // send the packet
                try {
                    udpClient.Send(buffer, buffer.Length, iEndpoint);
                }
                catch (Exception) {
                    // Error occurred in sending - quite probably due to hibernation.
                    // Stop this sender and make sure the Finished event is called to let the
                    // client code know.
                    break;
                }

                if (iAbort) {
                    break;
                }

                uint prog = ((i + 1) * 100) / iIterations;
                if (prog != iProgressPercent)
                {
                    iProgressPercent = prog;
                    iInvoker.BeginInvoke(new Action(UpdateProgress));
                }
            }

            udpClient.Close();
            
            if (!iAbort) {
                iInvoker.BeginInvoke(new Action(Finished));
            }
        }

        private void UpdateProgress()
        {
            if (EventProgressChanged != null) {
                EventProgressChanged(this, EventArgs.Empty);
            }
        }

        private void Finished()
        {
            if (EventFinished != null) {
                EventFinished(this, EventArgs.Empty);
            }
        }
        
        private IInvoker iInvoker;
        private readonly IPEndPoint iEndpoint;
        private readonly uint iPeriodMs;
        private readonly uint iBytes;
        private readonly uint iIterations;
        private readonly uint iId;
        private uint iProgressPercent;
        private bool iAbort;
        private Thread iThread;
    }


    // Class to retrieve results from the netmon receiver
    public class NetmonRetriever
    {
        public NetmonRetriever(IInvoker aInvoker, IPEndPoint aEndpoint, uint aId)
        {
            iInvoker = aInvoker;
            iEndpoint = aEndpoint;
            iId = aId;
            
            iLock = new object();
            iResults = new Results();
        }

        public void Start()
        {
            if (iThread == null)
            {
                iThread = new Thread(Run);
                iThread.IsBackground = true;
                iThread.Name = "NTMR";
                iThread.Start();
            }
        }
        
        public void Dispose()
        {
            if (iThread != null)
            {
                iClient.Close();
                iThread = null;
            }
        }

        public struct Results
        {
            public uint Total;
            public int Min;
            public int Max;
            public int StdDev;
            public uint OutOfSeq;
            public uint Duplicates;
            public uint Missed;
        }

        public Results GetResults()
        {
            lock (iLock)
            {
                return iResults;
            }
        }
        
        public event EventHandler EventResultsChanged;

        private void Run()
        {
            NetworkStream stream;
            try
            {
                // create tcp client, connect and get the stream
                iClient = new TcpClient();
                iClient.Connect(iEndpoint);            
                stream = iClient.GetStream();
            }
            catch (Exception)
            {
                // could not connect to client
                if (iClient != null) {
                    iClient.Close();
                }
                ResultsChanged();
                return;
            }

            uint txTimebase = 0;
            uint rxTimebase = 0;
            uint lastFrame = 0;
            int av = 0;
            int av2 = 0;
            int avNum = 0;
            byte[] buffer = new byte[16];

            // start retrieving the netmon results
            while (true)
            {
                // retrieve a 16-byte netmon packet
                try
                {
                    int bytesRecv = 0;
                    while (bytesRecv < 16)
                    {
                        bytesRecv += stream.Read(buffer, bytesRecv, 16 - bytesRecv);
                    }
                }
                catch (Exception)
                {
                    ResultsChanged();
                    break;
                }

                // analyse the results of this packet
                NetmonEvent ev = new NetmonEvent(buffer);

                // skip packets not for this id
                if (ev.Id != iId) {
                    continue;
                }

                // initialise some bookkeeping if this is the first packet
                if (iResults.Total == 0)
                {
                    txTimebase = ev.TxTimestamp;
                    rxTimebase = ev.RxTimestamp;
                    lastFrame = ev.Frame;
                    
                    lock (iLock)
                    {
                        iResults.Total++;
                    }
                    continue;
                }
                
                // this is not the first packet - start calculating results
                bool changed = false;
                lock (iLock)
                {
                    if (ev.Frame < lastFrame)
                    {
                        iResults.OutOfSeq++;
                        changed = true;
                    }
                    else if (ev.Frame == lastFrame)
                    {
                        iResults.Duplicates++;
                        changed = true;
                    }
                    else if (ev.Frame > lastFrame + 1)
                    {
                        iResults.Missed = iResults.Missed + ev.Frame - lastFrame - 1;
                        changed = true;
                    }
                    else
                    {
                        uint txDt = ev.TxTimestamp - txTimebase;
                        uint rxDt = ev.RxTimestamp - rxTimebase;
                        
                        int dt = ((int)rxDt - (int)txDt) / 1000;
                        
                        if (dt > iResults.Max)
                        {
                            iResults.Max = dt;
                            changed = true;
                        }
                        if (dt < iResults.Min)
                        {
                            iResults.Min = dt;
                            changed = true;
                        }

                        av += dt;
                        av2 += dt * dt;
                        avNum++;

                        iResults.StdDev = (av2/avNum) - ((av*av)/(avNum*avNum));
                    }
                    
                    lastFrame = ev.Frame;
                    iResults.Total++;
                }
                
                if (changed) {
                    ResultsChanged();
                }
            }
        }
        
        private void ResultsChanged()
        {
            if (iInvoker.TryBeginInvoke(new Action(ResultsChanged))) {
                return;
            }

            if (EventResultsChanged != null) {
                EventResultsChanged(this, EventArgs.Empty);
            }
        }

        private IInvoker iInvoker;
        private readonly IPEndPoint iEndpoint;
        private readonly uint iId;
        private Thread iThread;
        private TcpClient iClient;
        private object iLock;
        private Results iResults;
    }

    
    // class describing a netmon packet sent over the network
    public class NetmonEvent
    {
        public NetmonEvent(uint aId, uint aFrame, uint aTxTimestamp)
        {
            Id = aId;
            Frame = aFrame;
            TxTimestamp = aTxTimestamp;
            RxTimestamp = aId;
        }

        public NetmonEvent(byte[] aBuffer)
        {
            Id = BigEndianConverter.BigEndianToUint32(aBuffer, 0);
            Frame = BigEndianConverter.BigEndianToUint32(aBuffer, 4);
            TxTimestamp = BigEndianConverter.BigEndianToUint32(aBuffer, 8);
            RxTimestamp = BigEndianConverter.BigEndianToUint32(aBuffer, 12);
        }

        public void Write(byte[] aBuffer)
        {
            System.IO.MemoryStream s = new System.IO.MemoryStream(aBuffer, true);
            BitWriter w = new BitWriter(s, true);
            
            w.WriteUint32(Id);
            w.WriteUint32(Frame);
            w.WriteUint32(TxTimestamp);
            w.WriteUint32(RxTimestamp);
        }

        public readonly uint Id;
        public readonly uint Frame;
        public readonly uint TxTimestamp;
        public readonly uint RxTimestamp;
    }
}


