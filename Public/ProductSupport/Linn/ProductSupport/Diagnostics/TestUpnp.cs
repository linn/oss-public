// Perform UPnP M-Search to get list of all UPnP devices advertised on network
// Problematic devices show up by responding to M-Search, but failing to respond
// to request for device XML

using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Threading;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;

namespace Linn.ProductSupport.Diagnostics
{
    internal class TestUpnp : TestBase
    {
        public const int iMsearchTime = 5;

        public TestUpnp(string aInterface, Logger aLog, ETest aTest)
            : base(aInterface, aLog, aTest)
        {
        }

        public override void ExecuteTest()
        {
            List<Device> deviceList = SearchForDevices();

            foreach (Device device in deviceList)
            {
                Console.WriteLine(device.location);
                iLog.Subtest("Fetching device XML from " + device.location);
                UpnpDeviceInfo info = new UpnpDeviceInfo(device, this);
                string err = info.Update();

                if (err.Length != 0)
                {
                    iLog.Fail(err);
                }
                else
                {
                    iLog.Info("Device at " + info.Address);
                    iLog.Info("Manufacturer:- " + info.Manufacturer);
                    iLog.Info("Model:- " + info.Model);
                    iLog.Info("UPnP Name:- " + info.Name);
                    iLog.Pass("");
                }
                if (iKill)
                {
                    break;
                }
            }
        }

        private List<Device> SearchForDevices()
        {
            SsdpHandler ssdpHandler = new SsdpHandler();
            SsdpListenerUnicast ssdp = new SsdpListenerUnicast(ssdpHandler);
            System.Net.IPAddress ip = System.Net.IPAddress.Parse(iInterface);

            iLog.Subtest("Searching for UPnP devices for " + iMsearchTime.ToString() + "s");
            try
            {
                ssdp.Start(ip);
                ssdp.SsdpMsearchRoot(iMsearchTime);
                System.Threading.Thread.Sleep(iMsearchTime * 1000 + 2000);
                ssdp.Stop();
                ssdpHandler.DeviceList.Sort(CompareDevs);
                iLog.Pass("Found " + ssdpHandler.DeviceList.Count + " devices");
            }
            catch (Exception e)
            {
                iLog.Fail(e.ToString());
                ssdp.Stop();
            }
            return (ssdpHandler.DeviceList);
        }

        private int CompareDevs(Device x, Device y)
        {
            return x.location.CompareTo(y.location);
        }
    }


    internal class Device : IEquatable<Device>
    {
        public Device(byte[] aUuid, byte[] aLocation)
        {
            iLocation = System.Text.Encoding.ASCII.GetString(aLocation);
            iUuid = System.Text.Encoding.ASCII.GetString(aUuid);
        }

        public string location
        {
            get
            {
                return iLocation;
            }
        }

        public string uuid
        {
            get
            {
                return iUuid;
            }
        }

        public bool Equals(Device aOther)
        {
            if (aOther.location == iLocation)
            {
                return true;
            }
            return false;
        }

        private string iLocation;
        private string iUuid;
    }


    internal class SsdpHandler : ISsdpNotifyHandler
    {
        public SsdpHandler()
        {
            iDeviceList = new List<Device>();
        }

        public List<Device> DeviceList
        {
            get
            {
                return (iDeviceList);
            }
        }

        public void NotifyRootAlive(byte[] aUuid, byte[] aLocation, uint aMaxAge)
        {
            Device device = new Device(aUuid, aLocation);
            if (iDeviceList.Contains(device) == false)
            {
                iDeviceList.Add(device);
            }
        }

        // ISsdpNotifyHandler
        public void NotifyUuidAlive(byte[] aUuid, byte[] aLocation, uint aMaxAge) { }
        public void NotifyDeviceTypeAlive(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion, byte[] aLocation, uint aMaxAge) { }
        public void NotifyServiceTypeAlive(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion, byte[] aLocation, uint aMaxAge) { }
        public void NotifyRootByeBye(byte[] aUuid) { }
        public void NotifyUuidByeBye(byte[] aUuid) { }
        public void NotifyDeviceTypeByeBye(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion) { }
        public void NotifyServiceTypeByeBye(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion) { }

        private List<Device> iDeviceList;
    }


    internal class UpnpDeviceInfo
    {
        public const string iUpnpNs = "urn:schemas-upnp-org:device-1-0";

        public UpnpDeviceInfo(Device aDevice, TestBase aTest)
        {
            iUri = aDevice.location;
            iTest = aTest;
            iUuid = aDevice.uuid;
            iAddress = "";
            iManufacturer = "";
            iModel = "";
            iName = "";
            iKill = false;
            iOpened = false;
            iUpdated = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        public string Update()
        {
            string err = "";
            DeviceUpnp dev = new DeviceUpnp(iUuid, iUri);

            iUpdated.Reset();
            dev.Open();
            dev.EventOpened += new EventHandler<EventArgs>(EvOpen);
            dev.EventOpenFailed += new EventHandler<EventArgs>(EvOpenFailed);
            iTest.EventKill += new EventHandler<EventArgs>(EvKill);
            iUpdated.WaitOne();

            if (!iKill)
            {
                if (iOpened)
                {
                    try
                    {
                        XmlDocument document = new XmlDocument();
                        XmlNamespaceManager nsmanager = new XmlNamespaceManager(document.NameTable);

                        iAddress = dev.Location.Split(':')[1].Substring(2);
                        iModel = dev.Model;
                        iName = dev.Name;

                        document.LoadXml(dev.DeviceXml);
                        nsmanager.AddNamespace("u", iUpnpNs);
                        iManufacturer = document.SelectSingleNode("/u:root/u:device/u:manufacturer", nsmanager).InnerText;
                    }
                    catch (Exception e)
                    {
                        err = "Error parsing device XML:- " + e;
                    }
                }
                else
                {
                    err = "Error retrieving device XML";
                }
            }
            return err;
        }

        private void EvOpen(object sender, EventArgs e)
        {
            iOpened = true;
            iUpdated.Set();
        }

        private void EvOpenFailed(object sender, EventArgs e)
        {
            iOpened = false;
            iUpdated.Set();
        }

        private void EvKill(object sender, EventArgs e)
        {
            iKill = true;
            iUpdated.Set();
        }

        public string Address
        {
            get
            {
                return (iAddress);
            }
        }

        public string Manufacturer
        {
            get
            {
                return (iManufacturer);
            }
        }

        public string Model
        {
            get
            {
                return (iModel);
            }
        }

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        private string iUri;
        private TestBase iTest;
        private string iUuid;
        private string iAddress;
        private string iManufacturer;
        private string iModel;
        private string iName;
        private bool iKill;
        private bool iOpened;
        private EventWaitHandle iUpdated;
    }
}