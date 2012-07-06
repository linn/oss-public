using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Linn.ProductSupport.Diagnostics
{
    public static class Extension
    {
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // Handles negative ends.
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start;

            // Return new array.
            T[] res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }
    }


    internal abstract class DhcpMessage
    {
        // DHCP protocol defined by RFC2131, with options defined by RFC1533
        // This implementation is hard-coded for use of ethernet hardware layer

        //------------------------------------
        // Handling of BOOTP options (RFC1533)
        //------------------------------------

        public enum EOption
        {
            // DHCP options (subset) - see RFC1533
            eSubnetMask = 0x01,
            eRouter = 0x03,
            eNameServer = 0x05,
            eDomainNameServer = 0x06,
            eHostName = 0x0C,
            eDomainName = 0x0F,
            eAllSubnetsLocal = 0x1B,
            eBroadcastAddress = 0x1C,
            eRequestedAddress = 0x32,
            eLeaseTime = 0x33,
            eOptionOverload = 0x34,
            eMessageType = 0x35,
            eServerIdentifier = 0x36,
            eParameterList = 0x37,
            eMessage = 0x38,
            eRenewTimeValue = 0x3a,
            eRebindingTimeValue = 0x3b
        }

        public enum EMessageType
        {
            // Parameter used with option 55 (0x35)
            eDiscover = 0x01,
            eOffer = 0x02,
            eRequest = 0x03,
            eDecline = 0x04,
            eAck = 0x05,
            eNak = 0x06,
            eRelease = 0x07
        }

        public void AddOption(EOption aOption, byte[] aValue)
        {
            List<byte> option = new List<byte>();
            option.Add((byte)aOption);
            option.Add((byte)aValue.Length);
            option.AddRange(aValue);
            iOptions.AddRange(option);
        }

        public byte[] GetOption(EOption aOption)
        {
            byte[] val = null;
            byte[] bytes = iOptions.ToArray();
            Queue<byte> data = new Queue<byte>(bytes);

            while (data.Count > 0)
            {
                EOption option = (EOption)data.Dequeue();
                short length = (short)data.Dequeue();
                byte[] value = new byte[length];
                for (short i = 0; i < length; i++)
                {
                    value[i] = data.Dequeue();
                }
                if (option == aOption)
                {
                    val = value;
                    break;
                }
            }
            return val;
        }

        public List<IPAddress> dns
        {
            get
            {
                List<IPAddress> addresses = new List<IPAddress>();
                byte[] name = GetOption(EOption.eDomainNameServer);
                for (int i = 0; i < name.Length; i += 4)
                {
                    addresses.Add(new IPAddress(name.Slice(i, i + 4)));
                }
                return addresses;
            }
        }

        public string domain
        {
            get
            {
                return Encoding.ASCII.GetString(GetOption(EOption.eDomainName));
            }
        }

        public int leaseTime
        {
            get
            {
                byte[] time = GetOption(EOption.eLeaseTime);
                return time[3] + 256 * time[2] + 65536 * time[1] + 16777216 * time[0];
            }
        }

        public byte[] leaseTimeBytes
        {
            get
            {
                byte[] time = GetOption(EOption.eLeaseTime);
                return time;
            }
        }

        public EMessageType messageType
        {
            get
            {
                byte[] type = GetOption(EOption.eMessageType);
                return (EMessageType)type[0];
            }
        }

        public IPAddress router
        {
            get
            {
                byte[] rtr = GetOption(EOption.eRouter);
                return new IPAddress(rtr);
            }
        }

        public IPAddress serverId
        {
            get
            {
                byte[] id = GetOption(EOption.eServerIdentifier);
                return new IPAddress(id);
            }
        }

        public IPAddress subnetMask
        {
            get
            {
                byte[] mask = GetOption(EOption.eSubnetMask);
                return new IPAddress(mask);
            }
        }

        //---------------------------------------------
        // Properties to access DHCP message and fields
        //---------------------------------------------

        public byte[] message   // raw DHCP message
        {
            get
            {
                List<byte> msg = new List<byte> { };
                msg.AddRange(iOp);
                msg.AddRange(iHtype);
                msg.AddRange(iHlen);
                msg.AddRange(iHops);
                msg.AddRange(iXid);
                msg.AddRange(iSecs);
                msg.AddRange(iFlags);
                msg.AddRange(iCiaddr);
                msg.AddRange(iYiaddr);
                msg.AddRange(iSiaddr);
                msg.AddRange(iGiaddr);
                msg.AddRange(iChaddr);
                msg.AddRange(iPadding);
                msg.AddRange(iSname);
                msg.AddRange(iFile);
                msg.AddRange(iMagic);
                msg.AddRange(iOptions);
                msg.Add(0xff);
                return msg.ToArray();
            }
        }

        public enum EOp         // message op code
        {
            eBootRequest = 0x01,
            eBootReply = 0x02
        }

        public EOp op
        {
            get
            {
                if (iOp[0] == (byte)EOp.eBootRequest)
                {
                    return EOp.eBootRequest;
                }
                else
                {
                    return EOp.eBootReply;
                }
            }
            set
            {
                iOp[0] = (byte)value;
            }
        }

        public byte[] xid       // transaction ID    
        {
            get
            {
                return iXid;
            }
            set
            {
                iXid = value;
            }
        }

        public string xidStr    // (as string)
        {
            get
            {
                return iXid[0].ToString("X2") + iXid[1].ToString("X2") + iXid[2].ToString("X2") + iXid[3].ToString("X2");
            }
        }

        public int secs         // elapsed seconds
        {
            get
            {
                return (iSecs[0] * 256 + iSecs[1]);
            }
            set
            {
                iSecs[0] = (byte)(value / 256);
                iSecs[1] = (byte)(value % 256);
            }
        }

        public string mac       // hardware address (MAC for ethernet)
        {
            get
            {
                string strMac = String.Empty;
                for (int i = 0; i < iChaddr.Length; i++)
                {
                    strMac += iChaddr[i].ToString("X2");
                    if (i != iChaddr.Length - 1)
                    {
                        strMac += ":";
                    }
                }
                return strMac;
            }
            set
            {
                List<byte> bytes = new List<byte>();
                string[] octets = value.Split(':');
                foreach (string octet in octets)
                {
                    bytes.Add(Convert.ToByte(octet, 16));
                }
                iChaddr = bytes.ToArray();
            }
        }

        public IPAddress ciaddr    // client IP address
        {
            get
            {
                return new IPAddress(iCiaddr);
            }
            set
            {
                iCiaddr = value.GetAddressBytes();
            }
        }

        public IPAddress yiaddr    // 'your' (client) IP address
        {
            get
            {
                return new IPAddress(iYiaddr);
            }
            set
            {
                iYiaddr = value.GetAddressBytes();
            }
        }

        public IPAddress siaddr    // next server IP address
        {
            get
            {
                return new IPAddress(iSiaddr);
            }
            set
            {
                iSiaddr = value.GetAddressBytes();
            }
        }

        public IPAddress giaddr    // relay agent IP address
        {
            get
            {
                return new IPAddress(iGiaddr);
            }
            set
            {
                iGiaddr = value.GetAddressBytes();
            }
        }

        //----------
        // Utilities
        //----------

        protected byte[] Fill(byte aValue, int aCount)
        {
            byte[] array = new byte[aCount];
            for (int i = 0; i < aCount; i++)
            {
                array[i] = aValue;
            }
            return array;
        }

        //-----------
        // Class data
        //-----------

        protected byte[] iOp = new byte[1];             // message type;
        protected byte[] iHtype = new byte[1];          // hardware address type
        protected byte[] iHlen = new byte[1];           // hardware address length
        protected byte[] iHops = new byte[1];           // hops
        protected byte[] iXid = new byte[4];            // transaction ID
        protected byte[] iSecs = new byte[2];           // client elapsed seconds
        protected byte[] iFlags = new byte[2];          // flags
        protected byte[] iCiaddr = new byte[4];         // client IP address
        protected byte[] iYiaddr = new byte[4];         // your IP address
        protected byte[] iSiaddr = new byte[4];         // server IP address
        protected byte[] iGiaddr = new byte[4];         // relay agent IP address
        protected byte[] iChaddr = new byte[6];         // client hardware address
        protected byte[] iPadding = new byte[10];       // client hardware address padding
        protected byte[] iSname = new byte[64];         // server host name
        protected byte[] iFile = new byte[128];         // boot file name
        protected byte[] iMagic = new byte[4];          // DHCP magic cookie   
        protected List<byte> iOptions = new List<byte>();
    }


    internal abstract class DhcpTxMessage : DhcpMessage
    {
        public DhcpTxMessage()
        {
            // Create new DHCP message (for transmission)

            Random random = new Random();

            op = EOp.eBootRequest;
            iHtype = new byte[] { 0x01 };               // ethernet
            iHlen = new byte[] { 0x06 };                // 6-byte MAC for ethernet
            iHops = new byte[] { 0x00 };                // always 0 for client
            random.NextBytes(iXid);                     // random transaction ID
            secs = 0;
            iFlags = new byte[] { 0x80, 0x00 };         // set BROADCAST bit
            ciaddr = IPAddress.Parse("0.0.0.0");
            yiaddr = IPAddress.Parse("0.0.0.0");
            siaddr = IPAddress.Parse("0.0.0.0");
            giaddr = IPAddress.Parse("0.0.0.0");
            iPadding = Fill(0x00, 10);                  // hardware address padding (10 bytes for ethernet)
            iSname = Fill(0x00, 64);                    // BOOTP legacy field
            iFile = Fill(0x00, 128);                    // BOOTP legacy field
            iMagic = new byte[] { 0x63, 0x82, 0x53, 0x63 };
        }
    }


    internal class DhcpDiscoverMessage : DhcpTxMessage
    {
        public DhcpDiscoverMessage()
        {
            // Create DHCP DISCOVER message (caller can modify defaults before sending if required)
            // defaults are set to mimic the message from a DS
            Random random = new Random();
            random.NextBytes(iChaddr);                                                      // spoofed MAC
            AddOption(EOption.eMessageType, new byte[] { (byte)EMessageType.eDiscover });   // DISCOVER message
            AddOption(EOption.eLeaseTime, new byte[] { 0xff, 0xff, 0xff, 0xff });           // infinite lease time
            AddOption(EOption.eParameterList, new byte[] {(byte)EOption.eSubnetMask,
                                            (byte)EOption.eRouter,
                                            (byte)EOption.eDomainNameServer,
                                            (byte)EOption.eHostName,
                                            (byte)EOption.eDomainName});
        }
    }


    internal class DhcpRequestMessage : DhcpTxMessage
    {
        public DhcpRequestMessage(DhcpMessage offer)
        {
            // Create DHCP REQUEST message (caller can modify defaults before sending if required)
            // defaults are set to mimic the message from a DS
            mac = offer.mac;
            secs = offer.secs;
            xid = offer.xid;
            AddOption(EOption.eMessageType, new byte[] { (byte)EMessageType.eRequest });
            AddOption(EOption.eServerIdentifier, offer.serverId.GetAddressBytes());
            AddOption(EOption.eRequestedAddress, offer.yiaddr.GetAddressBytes());
            AddOption(EOption.eLeaseTime, offer.leaseTimeBytes);
            AddOption(EOption.eParameterList, new byte[] {(byte)EOption.eSubnetMask,
                                            (byte)EOption.eRouter,
                                            (byte)EOption.eDomainNameServer,
                                            (byte)EOption.eHostName,
                                            (byte)EOption.eDomainName});
        }
    }


    internal class DhcpRenewMessage : DhcpTxMessage
    {
        public DhcpRenewMessage(DhcpMessage lease)
        {
            // RENEW message is a REQUEST message without server ID or requested address
            // and completed ciaddr
            mac = lease.mac;
            secs = 0;
            ciaddr = lease.yiaddr;
            xid = lease.xid;
            AddOption(EOption.eMessageType, new byte[] { (byte)EMessageType.eRequest });
            iFlags[0] = 0x00;         // clear BROADCAST bit
        }
    }


    internal class DhcpReleaseMessage : DhcpTxMessage
    {
        public DhcpReleaseMessage(DhcpMessage accept)
        {
            // Create DHCP RELEASE message
            mac = accept.mac;
            AddOption(EOption.eMessageType, new byte[] { (byte)EMessageType.eRelease });
            AddOption(EOption.eServerIdentifier, accept.serverId.GetAddressBytes());
            iFlags[0] = 0x00;         // clear BROADCAST bit
        }
    }


    internal class DhcpRxMessage : DhcpMessage
    {
        public DhcpRxMessage(byte[] aMessage)
        {
            try
            {
                // Create DHCP message from received data
                iOp = aMessage.Slice(0, 1);
                iHtype = aMessage.Slice(1, 2);
                iHlen = aMessage.Slice(2, 3);
                iHops = aMessage.Slice(3, 4);
                iXid = aMessage.Slice(4, 8);
                iSecs = aMessage.Slice(8, 10);
                iFlags = aMessage.Slice(10, 12);
                iCiaddr = aMessage.Slice(12, 16);
                iYiaddr = aMessage.Slice(16, 20);
                iSiaddr = aMessage.Slice(20, 24);
                iGiaddr = aMessage.Slice(24, 28);
                iChaddr = aMessage.Slice(28, 34);
                iPadding = aMessage.Slice(34, 44);
                iSname = aMessage.Slice(44, 108);
                iFile = aMessage.Slice(108, 236);
                iMagic = aMessage.Slice(236, 240);
                iOptions = new List<byte>(aMessage.Slice(240, -1));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());            // need to do something better here ......
            }
        }
    }
}
