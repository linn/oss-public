using System;
using System.ServiceProcess;

namespace Linn
{
	/// <summary>
	/// Description of ServiceController.
	/// </summary>
    internal static class ServiceInfo
    {
        internal static string Status {
            get {
                ServiceController ssdp = new ServiceController("SSDPSRV");
                return ssdp.Status.ToString();
      	    }
        }

        internal static string DisplayName {
            get {
                 ServiceController ssdp = new ServiceController("SSDPSRV");
                 return ssdp.DisplayName;
            }
        }
    }
}
