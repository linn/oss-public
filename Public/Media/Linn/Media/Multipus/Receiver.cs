using System;
using System.Net;
using System.Threading;

using Linn;
using Linn.Network;

namespace Linn.Media.Multipus
{
    public class FrameErrorHandler : IFrameErrorHandler
    {
        public void NotMpus(Frame aFrame)
        {
            Console.WriteLine("FrameErrorHandler: NotMpus\n");
        }
        public void LateOrDuplicate(Frame aFrame)
        {
            Console.WriteLine("FrameErrorHandler::LateOrDuplicate\n");
        }
        public void Missing(long aLastGoodTimestampNs, long aNextGoodTimestampNs, uint aMissingFrames)
        {
            Console.WriteLine("FrameErrorHandler::Missed {0} frames in {1} time", aMissingFrames, aNextGoodTimestampNs - aLastGoodTimestampNs);
        }

    }

    public class Receiver
    {
        public Receiver(IPAddress aInterface, IPAddress aMulticast, int aPort)
        {
            iMulticastReader = new UdpMulticastReader(aInterface, aMulticast, aPort);
            iSrb = new Srb(kMaxRead, iMulticastReader);
            iFrameSupplyUdp = new FrameSupplyUdp(iSrb);
            iFrameWriterDisk = new FrameWriterDisk(iFrameSupplyUdp);
            iFrameFilter = new FrameFilter(iFrameWriterDisk, new FrameErrorHandler());
            
            iThread = new Thread(new ThreadStart(Run));
            iThread.IsBackground = true;
            iThread.Name = "Multipus Control Receiver";
        }

        public void Start()
        {
            iThread.Start();
        }

        private void Run()
        {
            Console.WriteLine("Receiver::Run");
            while (true)
            {
                Frame frame = iFrameFilter.Read();
            }
        }

        private int kMaxRead = 65000;
        private UdpMulticastReader iMulticastReader;
        private Srb iSrb;
        private FrameSupplyUdp iFrameSupplyUdp;
        private FrameWriterDisk iFrameWriterDisk;
        private FrameFilter iFrameFilter;
        private Thread iThread;
    }
}

