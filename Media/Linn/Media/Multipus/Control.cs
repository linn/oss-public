using System;
using System.Net;
using System.Threading;

using Linn;
using Linn.Network;

namespace Linn.Media.Multipus
{
    public class ControlMsgInvalid : Exception
    {
        public ControlMsgInvalid()
        {
        }
    }

    public class ControlMsg
    {
        public int ConstructBase(byte[] aData)
        {
            int pos = 0;

            iMpus = BitConverter.ToUInt32(aData, pos);
            iMpus = (uint)IPAddress.NetworkToHostOrder((int)iMpus);
            pos += 4;

            iVersionMajor = BitConverter.ToUInt32(aData, pos);
            iVersionMajor = (uint)IPAddress.NetworkToHostOrder((int)iVersionMajor);
            pos += 4;

            iVersionMinor = BitConverter.ToUInt32(aData, pos);
            iVersionMinor = (uint)IPAddress.NetworkToHostOrder((int)iVersionMinor);
            pos += 4;

            iCommand = BitConverter.ToUInt32(aData, pos);
            iCommand = (uint)IPAddress.NetworkToHostOrder((int)iCommand);
            pos += 4; 

            iUid = new byte[kUidBytes];
            Buffer.BlockCopy(aData, pos, iUid, 0, kUidBytes);
            pos += kUidBytes;

            iReqSeq = BitConverter.ToUInt32(aData, pos);
            iReqSeq = (uint)IPAddress.NetworkToHostOrder((int)iReqSeq);
            pos += 4; 

            if( iMpus != kMpus ) {
                throw (new ControlMsgInvalid());
            }
            if( iVersionMajor != kVersionMajor ) {
                throw (new ControlMsgInvalid());
            }
            if( iVersionMinor != kVersionMinor ) {
                throw (new ControlMsgInvalid());
            }

            return pos;
        }

        public string ToStringBase() 
        {
            string str = String.Format("Mpus: {0,8:X}\nMajor: {1:D}\nMinor: {2:D}\nCommand: {3:D}\nUid: ", iMpus, iVersionMajor, iVersionMinor, iCommand);
            str += BitConverter.ToString(iUid);
            str += String.Format("\nReqSeq: {0}\n", iReqSeq);
            return str;
        }
        protected uint iMpus;
        protected uint iVersionMajor;
        protected uint iVersionMinor;
        protected uint iCommand;
        protected byte[] iUid;
        protected uint iReqSeq;

        protected static readonly int kUidBytes = 8;
        protected static readonly uint kMpus = 0x4d707573; //Binary for "Mpus"
        protected static readonly int kVersionMajor = 0;
        protected static readonly int kVersionMinor = 1;
        protected static readonly int kCommandMasterSlave = 1; //Master to slave
        protected static readonly int kCommandSlaveMaster = 2; //Slave to Master
    }

    public class ControlMsgReq : ControlMsg
    {
        public ControlMsgReq(byte[] aData)
        {
            int pos = 0;
            Console.WriteLine("ControlMsgReq: {0}\n", aData.Length);    

            if(aData.Length != kBytes) {
                throw (new ControlMsgInvalid());
            }

            pos += ConstructBase(aData);

            if(iCommand != kCommandSlaveMaster) {
                throw (new ControlMsgInvalid());
            }

            iT1 = BitConverter.ToUInt32(aData, pos);
            iT1 = (uint)IPAddress.NetworkToHostOrder((int)iT1);
            pos += 4; 

            iT2 = BitConverter.ToUInt32(aData, pos);
            iT2 = (uint)IPAddress.NetworkToHostOrder((int)iT2);
            pos += 4; 

            iT3 = BitConverter.ToUInt32(aData, pos);
            iT3 = (uint)IPAddress.NetworkToHostOrder((int)iT3);
            pos += 4; 

            iT4 = BitConverter.ToUInt32(aData, pos);
            iT4 = (uint)IPAddress.NetworkToHostOrder((int)iT4);
            pos += 4; 
        }
        public override string ToString()
        {
            string str = String.Format("T1: {0:X}\nT2: {1:X}\nT3: {2:X}\nT4: {3:X}\n",iT1,iT2,iT3,iT4);
            return base.ToStringBase() + str;
        }

        public uint iT1;
        public uint iT2;
        public uint iT3;
        public uint iT4;

        private static readonly int kBytes = 44;
        
    }

    public class ControlMsgResp : ControlMsg
    {
        public ControlMsgResp(byte[] aData)
        {
            if(aData.Length != kBytes) {
                throw (new ControlMsgInvalid());
            }

            int pos = ConstructBase(aData);

            if(iCommand != kCommandMasterSlave) {
                throw (new ControlMsgInvalid());
            }

            iT4 = BitConverter.ToUInt32(aData, pos);
            iT4 = (uint)IPAddress.NetworkToHostOrder((int)iT4);
            pos += 4;
            
            iSample = BitConverter.ToUInt64(aData, pos);
            iSample = (ulong)IPAddress.NetworkToHostOrder((long)iSample);
            pos += 8;
            
            iSampleClock = BitConverter.ToUInt32(aData, pos);
            iSampleClock = (uint)IPAddress.NetworkToHostOrder((int)iSampleClock);
            pos += 4;

            iState = BitConverter.ToUInt32(aData, pos);
            iState = (uint)IPAddress.NetworkToHostOrder((int)iState);
            pos += 4;

            iT1Seq = BitConverter.ToUInt32(aData, pos);
            iT1Seq = (uint)IPAddress.NetworkToHostOrder((int)iT1Seq);
            pos += 4;

            iT1Prev = BitConverter.ToUInt32(aData, pos);
            iT1Prev = (uint)IPAddress.NetworkToHostOrder((int)iT1Prev);
            pos += 4;

            iClockFreq = BitConverter.ToUInt32(aData, pos);
            iClockFreq = (uint)IPAddress.NetworkToHostOrder((int)iClockFreq);
            pos += 4;
        }

        public override string ToString()
        {
            string str = String.Format("T4: {0:X}\nSample: {1}\nSampleClock: {2}\nState: {3}\nT1Seq: {4:X}\nT1Prev: {5:X}\nClockFreq: {6}",iT4, iSample, iSampleClock, iState, iT1Seq, iT1Prev, iClockFreq);
            return base.ToStringBase() + str;
        }
        
        public uint iT4;
        public ulong iSample;
        public uint iSampleClock;
        public uint iState;
        public uint iT1Seq;
        public uint iT1Prev;
        public uint iClockFreq;

        public  static readonly int kBytes = 60;
    }


    public class ControlReceiver
    {
        public ControlReceiver(IPAddress aInterface, IPAddress aMulticast)
        {
            iReader = new UdpMulticastReader(aInterface, aMulticast, 51979);

            iBuf = new byte[kMaxBufBytes];

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
            while(true) {
                Console.WriteLine("ControlReceiver::Run");
                int read = iReader.Read(iBuf, 0, kMaxBufBytes);
                iReader.ReadFlush();

                byte[] buf = new byte[read];
                Array.Copy(iBuf, buf, read);

                try {
                    ControlMsgReq req = new ControlMsgReq(buf);
                    Console.WriteLine("ControlReceiver::Run received req");
                    Console.WriteLine(req.ToString());
                }
                catch(ControlMsgInvalid) {
                    Console.WriteLine("ControlReceiver::Run req invalid");
                    
                    try {
                        ControlMsgResp resp = new ControlMsgResp(buf);
                        Console.WriteLine("ControlReceiver::Run received resp");
                        Console.WriteLine(resp.ToString());
                    }
                    catch(ControlMsgInvalid) {
                        Console.WriteLine("ControlReceiver::Run resp invalid");
                    }
                }
            }
        }

        private static readonly int kMaxBufBytes = ControlMsgResp.kBytes;
        private byte[] iBuf;
        private UdpMulticastReader iReader;
        private Thread iThread;
    }
}
