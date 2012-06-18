using System;
using System.Net;
using System.Text;
using System.IO;

using System.Diagnostics;
using System.Threading;

using Linn;
using Linn.ControlPoint;
using Linn.Topology;

public class FindDevice
{
    [STAThread]
    static void Main(string[] aArgs)
    {
        Helper helper = new Helper(aArgs);

        OptionParser optParser = helper.OptionParser;
        optParser.Usage = "usage: FindDevice [options] [device name]";
        OptionParser.OptionInt optTimeout = new OptionParser.OptionInt("-w", "--wait", 5, "Time to wait for finding the device (s)", "TIMEOUT");
        optParser.AddOption(optTimeout);

        helper.ProcessCommandLine();
        
        if (optParser.PosArgs.Count != 1) {
            Console.WriteLine(optParser.Help());
            return;
        }

        string uglyname = optParser.PosArgs[0];
        int timeout = optTimeout.Value;

        DeviceFinder finder = new DeviceFinder(uglyname);

        Device device;
        
        try
        {
            device = finder.Find(helper.Interface.Interface.Info.IPAddress, timeout * 1000);
        }
        catch (DeviceFinderException)
        {
            Console.WriteLine("Device not found");
            return;
        }

        helper.Dispose();

        Console.WriteLine("Udn: " + device.Udn);

        System.Environment.Exit(0);
    }
}
