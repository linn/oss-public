// Check for correct DHCP operation on network, including lease rebind. Any issues
// shown up here should be resolved before continuing with any further diagnostics or
// setup as DHCP issues are usually disasterous to correct operation of DS (or any
// DHCP based) networked system.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Linn.ProductSupport.Diagnostics
{
    internal class TestDhcp : TestBase
    {
        // DHCP messages use port 68 on client and port 67 on server(s) 

        public TestDhcp(string aInterface, Logger aLog, ETest aTest)
            : base(aInterface, aLog, aTest)
        { }

        public override void ExecuteTest()
        {
            List<DhcpMessage> offers;
            List<DhcpMessage> leases;
            IPEndPoint client = new IPEndPoint(IPAddress.Parse(iInterface), 68);
            iRxMessages = new List<DhcpMessage>();
            iShutdown = false;

            try
            {
                iUdpClient = new UdpClient(client);
                iUdpClient.EnableBroadcast = true;

                StartDhcpReceiver();
                if (!iKill)
                {
                    offers = TestDiscover();
                    if (!iKill)
                    {
                        leases = TestRequest(offers);
                        // if (!iKill)
                        //{
                        //    TestRenew();     DS doesn't do renew, only rebind
                        //}
                        if (!iKill)
                        {
                            TestRebind(leases);
                            if (!iKill)
                            {
                                TestRelease(leases);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                iLog.Fail(e.ToString());
            }
            finally
            {
                if (iRxThread != null)
                {
                    StopDhcpReceiver();
                }
            }

        }

        private List<DhcpMessage> TestDiscover()
        {
            iLog.Subtest("DHCP Discover");
            List<DhcpMessage> offers = new List<DhcpMessage>();

            DhcpDiscoverMessage discover = new DhcpDiscoverMessage();
            iRxMessages.Clear();
            int bytesSent = SendMessage(discover, "255.255.255.255");
            if (bytesSent != discover.message.Length)
            {
                iLog.Fail("Message NOT sent");
            }
            else
            {
                Thread.Sleep(5000);
                foreach (DhcpMessage message in iRxMessages)    // process responses
                {
                    if (message.xidStr == discover.xidStr)
                    {
                        if (message.messageType == DhcpMessage.EMessageType.eOffer)
                        {
                            iLog.Info("Received DHCP offer of " + message.yiaddr.ToString() + " from " + message.serverId.ToString());
                            offers.Add(message);
                        }
                        else
                        {
                            iLog.Warn("Unexpected " + message.messageType + " from " + message.serverId.ToString());
                        }
                    }
                }
                if (offers.Count == 0)                          // fail on no OFFERS
                {
                    iLog.Fail("No DHCP offers received");
                }
                else if (offers.Count == 1)                     // pass on ONE OFFER
                {
                    iLog.Pass("Single DHCP offer received");
                }
                else
                {
                    List<IPAddress> subnets = new List<IPAddress>();
                    foreach (DhcpMessage offer in offers)
                    {
                        IPAddress subnet = GetSubnet(offer.yiaddr, offer.subnetMask);
                        if (subnets.Contains(subnet))           // warn on >1 OFFER on same subnet
                        {
                            iLog.Warn("Multiple DHCP offers from SAME subnet " + subnet.ToString());
                        }
                        else if (subnets.Count > 0)             // fail on OFFERs from >1 subnets
                        {
                            string netStr = subnet.ToString();
                            foreach (IPAddress ip in subnets)
                            {
                                netStr += " " + ip.ToString();
                            }
                            iLog.Fail("Multiple DHCP offers from DIFFERENT subnets " + netStr);
                        }
                        subnets.Add(subnet);
                    }
                }
            }
            return offers;
        }

        private List<DhcpMessage> TestRequest(List<DhcpMessage> offers)
        {
            List<DhcpMessage> leases = new List<DhcpMessage>();

            foreach (DhcpMessage offer in offers)
            {
                iLog.Subtest("DHCP Request " + offer.serverId.ToString());
                DhcpRequestMessage request = new DhcpRequestMessage(offer);
                iRxMessages.Clear();
                int bytesSent = SendMessage(request, "255.255.255.255");
                if (bytesSent != request.message.Length)
                {
                    iLog.Fail("Message NOT sent");
                }
                else
                {
                    Thread.Sleep(5000);
                    bool passed = false;
                    foreach (DhcpMessage message in iRxMessages)    // process responses
                    {
                        if (message.xidStr == request.xidStr)
                        {
                            if (message.messageType == DhcpMessage.EMessageType.eAck)
                            {
                                iLog.Info("My IP Address " + message.yiaddr.ToString());
                                iLog.Info("Router " + message.router);
                                iLog.Info("Subnet Mask " + message.subnetMask.ToString());
                                iLog.Info("Lease Time " + message.leaseTime.ToString());
                                List<IPAddress> dns = message.dns;
                                foreach (IPAddress address in dns)
                                {
                                    iLog.Info("Domain Name Server " + address.ToString());
                                }
                                iLog.Info("Domain Name  " + message.domain);
                                iLog.Pass("REQUEST was ACKd");
                                leases.Add(message);
                                passed = true;
                                break;
                            }
                        }
                    }
                    if (!passed)
                    {
                        iLog.Fail("No ACK to REQUEST");
                    }
                }
            }
            return leases;
        }

        private void TestRebind(List<DhcpMessage> leases)
        {
            foreach (DhcpMessage lease in leases)
            {
                iLog.Subtest("DHCP Rebind to " + lease.yiaddr.ToString());
                DhcpRenewMessage renew = new DhcpRenewMessage(lease);
                iRxMessages.Clear();
                int bytesSent = SendMessage(renew, "255.255.255.255");  // rebind is a broadcast renew
                if (bytesSent != renew.message.Length)
                {
                    iLog.Fail("Message NOT sent");
                }
                else
                {
                    Thread.Sleep(5000);
                    bool passed = false;
                    foreach (DhcpMessage message in iRxMessages)        // process responses
                    {
                        if (message.xidStr == renew.xidStr)
                        {
                            if (message.messageType == DhcpMessage.EMessageType.eAck)
                            {
                                passed = true;
                            }
                        }
                    }
                    if (passed)
                    {
                        iLog.Pass("REBIND was ACKd");
                    }
                    else
                    {
                        iLog.Fail("No ACK to REBIND");
                    }
                }
            }
        }

        private void TestRelease(List<DhcpMessage> leases)
        {
            foreach (DhcpMessage lease in leases)
            {
                iLog.Subtest("DHCP Release " + lease.mac + " from " + lease.serverId.ToString());
                DhcpReleaseMessage request = new DhcpReleaseMessage(lease);
                iRxMessages.Clear();
                int bytesSent = SendMessage(request, lease.serverId.ToString());
                if (bytesSent != request.message.Length)
                {
                    iLog.Fail("Message NOT sent");
                }
                else
                {
                    iLog.Pass("Message sent");
                }
            }
        }

        private int SendMessage(DhcpMessage aMessage, string aDestination)
        {
            int bytesSent = 0;
            iLog.Info("Tx " + aMessage.messageType.ToString() + " from " + aMessage.mac + " with XID " + aMessage.xidStr);
            try
            {
                IPEndPoint server = new IPEndPoint(IPAddress.Parse(aDestination), 67);
                bytesSent = iUdpClient.Send(aMessage.message, aMessage.message.Length, server);
            }
            catch (Exception e)
            {
                iLog.Fail(e.ToString());
            }
            return bytesSent;
        }

        private void StartDhcpReceiver()
        {
            iRxThread = new Thread(new ThreadStart(DhcpReceiver));
            iRxThread.Start();
        }

        private void StopDhcpReceiver()
        {
            iShutdown = true;
            iUdpClient.Close();
            iRxThread.Join();
        }

        private void DhcpReceiver()
        {
            DhcpMessage message;
            while (!iShutdown)
            {
                message = ReceiveMessage();
                if (message != null)
                {
                    iRxMessages.Add(message);

                }
            }
        }

        private DhcpMessage ReceiveMessage()
        {
            DhcpRxMessage message = null;
            try
            {
                IPEndPoint server = new IPEndPoint(IPAddress.Any, 0);
                byte[] response = iUdpClient.Receive(ref server);
                message = new DhcpRxMessage(response);
                iLog.Info("Rx " + message.messageType.ToString() + " from " + message.serverId.ToString() + " for " + message.mac + " with XID " + message.xidStr);
            }
            catch (Exception e)
            {
                if (!iShutdown)
                {
                    iLog.Fail(e.ToString());
                }
            }
            return message;
        }

        private IPAddress GetSubnet(IPAddress address, IPAddress subnetMask)
        {
            byte[] addrBytes = address.GetAddressBytes();
            byte[] maskBytes = subnetMask.GetAddressBytes();
            byte[] subnetBytes = new byte[addrBytes.Length];

            for (int i = 0; i < subnetBytes.Length; i++)
            {
                subnetBytes[i] = (byte)(addrBytes[i] & maskBytes[i]);
            }
            return new IPAddress(subnetBytes);
        }

        private bool iShutdown;
        private UdpClient iUdpClient;
        private Thread iRxThread;
        private List<DhcpMessage> iRxMessages;
    }
}
