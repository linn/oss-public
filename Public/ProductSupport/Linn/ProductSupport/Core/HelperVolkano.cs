using System;
using System.Net;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;


namespace Linn.ProductSupport
{
	public class HelperVolkano : Helper
    {
        // console applications should use helper volkano
        // command line options used only (nothisng saved to disk)
        public HelperVolkano(string[] aArgs) : base(aArgs) {
            iIpAddress = IPAddress.None;
        }

        public void Start() {
            ProcessCommandLine();
            string adapterName = "None";

            if (CommandLineInterface != null) {
                // valid interface specified on command line
                iIpAddress = CommandLineInterface.IPAddress;
                adapterName = "command line interface: " + CommandLineInterface.Name;
            }
            else {
                string hostName = Dns.GetHostName();

                // If running on eng server - set the interface to send multicast on
                if (hostName == "ascit" || hostName == "eng") {
                    iIpAddress = IPAddress.Parse("10.2.7.70");
                    adapterName = "server interface: " + hostName;
                }
                else {
                    IList<NetworkInfoModel> ifaces = NetworkInfo.GetAllNetworkInterfaces();
                    foreach (NetworkInfoModel iface in ifaces) {
                        // select first interface that is detected
                        iIpAddress = iface.IPAddress;
                        adapterName = "first interface discovered: " + iface.Name;
                        break;
                    }
                }
            }
            Console.WriteLine("Application started using " + adapterName + " (" + iIpAddress.ToString() + ")");
        }

        public IPAddress IpAddress {
            get {
                return iIpAddress;
            }
        }

        private IPAddress iIpAddress;
    }
}


