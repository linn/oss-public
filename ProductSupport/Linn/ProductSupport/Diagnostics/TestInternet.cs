// TestInternet - verify internet connectivity and DNS operation
//
// Checks ping of Google by IP and name
// Checks ping of Linn by IP and name
// Checks HTTP GET of file from Linn

using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Linn.ProductSupport.Diagnostics
{
    internal class TestInternet : TestBase
    {
        protected static string kLinnIp = "89.16.185.167";
        protected static string kLinnName = "kiboko.linn.co.uk";
        protected static string kGoogleIp = "173.194.34.162";
        protected static string kGoogleName = "google.com";
        protected static string kTestUri = "http://products.linn.co.uk/VersionInfo/ReleaseVersionInfo.xml";
        protected static string kTitle = "<title>Linn Software Releases</title>";

        public TestInternet(string aInterface, Logger aLog, ETest aTest)
            : base(aInterface, aLog, aTest)
        {
        }

        public override void ExecuteTest()
        {
            if (!iKill)
            {
                PingAddr(kGoogleIp, 100);
            }
            if (!iKill)
            {
                PingAddr(kGoogleName, 100);
            }
            if (!iKill)
            {
                PingAddr(kLinnIp, 100);
            }
            if (!iKill)
            {
                PingAddr(kLinnName, 100);
            }
            if (!iKill)
            {
                HttpGet(kTestUri);
            }
        }

        protected void PingAddr(string aAddr, int aLoops)
        {
            int loop;
            long time = 0;
            long maxTime = 0;
            int ttl = 0;
            int bytes = 0;
            int lost = 0;
            int missing = 0;
            bool ok = true;
            Ping ping = new Ping();
            PingReply reply;

            iLog.Subtest("Ping " + aAddr + " for " + aLoops.ToString() + " loops");
            try
            {
                for (loop = 0; loop < aLoops; loop++)
                {
                    reply = ping.Send(aAddr);

                    if (reply.Status == IPStatus.Success)
                    {
                        time += reply.RoundtripTime;
                        ttl = reply.Options.Ttl;
                        bytes = reply.Buffer.Length;
                        missing = 0;
                        if (reply.RoundtripTime > maxTime)
                        {
                            maxTime = reply.RoundtripTime;
                        }
                    }
                    else
                    {
                        lost += 1;
                        missing += 1;
                        if (missing > 5)
                        {
                            // if lost > 5 sequential abandon ....  whats the timeout here
                            iLog.Fail("Abandoning test after 5 consecutive failed pings");
                            loop = aLoops;      // force loop exit
                            ok = false;
                        }
                    }
                }
                if (ok)
                {
                    long aveTime = time / (aLoops - lost);
                    iLog.Info("Loops:- " + aLoops.ToString());
                    iLog.Info("Lost Packets:- " + lost.ToString());
                    iLog.Info("Ave time:- " + aveTime.ToString());
                    iLog.Info("Max time:- " + maxTime.ToString());
                    iLog.Info("Ttl:- " + ttl.ToString());
                    iLog.Info("Packet size:- " + bytes.ToString());

                    if (aveTime > 500)
                    {
                        iLog.Fail("VERY slow connection (Ave round-trip delay of " + aveTime.ToString() + ")");
                    }
                    else if (aveTime > 200)
                    {
                        iLog.Warn("SLOW connection (Ave round-trip delay of " + aveTime.ToString() + ")");
                    }
                    else if (lost > 0)
                    {
                        iLog.Warn(lost.ToString() + " lost packets");
                    }
                    else
                    {
                        iLog.Pass("");
                    }
                }
            }
            catch (PingException e)
            {
                iLog.Fail("DNS failure - " + e.ToString());
            }
            catch (Exception e)
            {
                iLog.Fail(e.ToString());
            }
        }

        protected void HttpGet(string aUri)
        {
            int count = 0;
            byte[] buffer = new byte[1024];
            string str = null;

            iLog.Subtest("HTTP GET from " + aUri);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(aUri);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream stream = response.GetResponseStream();
                count = stream.Read(buffer, 0, buffer.Length);
                stream.Close();

                if (count != 0)
                {
                    str = Encoding.ASCII.GetString(buffer, 0, count);
                    if (!str.Contains(kTitle))
                    {
                        iLog.Fail("Unexpected data: " + str);
                    }
                    else
                    {
                        iLog.Pass("");
                    }
                }
                else
                {
                    iLog.Fail("No data returned");
                }
            }
            catch (Exception e)
            {
                iLog.Fail("No response" + e);
            }
        }
    }
}