using System;

using Linn;
using Linn.Control.Nct;

namespace NctClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] aArgs)
        {
            Helper helper = new Helper(aArgs);
            OptionParser optParser = helper.OptionParser;
            OptionParser.OptionString option1 = new OptionParser.OptionString("-1", "--protocol", "UDP", "Protocol (UDP or TCP)", "PROTOCOL");
            OptionParser.OptionString option2 = new OptionParser.OptionString("-2", "--clientaddr", "127.0.0.1", "Client IP address", "CLIENTADDRESS");
            OptionParser.OptionInt    option3 = new OptionParser.OptionInt("-3", "--clientport", 51936, "Client port", "CLIENTPORT");
            OptionParser.OptionString option4 = new OptionParser.OptionString("-4", "--serveraddr", "127.0.0.1", "Server IP address", "SERVERADDRESS");
            OptionParser.OptionInt    option5 = new OptionParser.OptionInt("-5", "--serverport", 51936, "Server port", "SERVERPORT");
            OptionParser.OptionInt    option6 = new OptionParser.OptionInt("-6", "--size", 4, "Size of each packet (bytes)", "PACKETSIZE");
            OptionParser.OptionInt    option7 = new OptionParser.OptionInt("-7", "--total", 20, "Total number of packets to send", "PACKETTOTAL");
            OptionParser.OptionInt    option8 = new OptionParser.OptionInt("-8", "--delay", 0, "Delay between packets (0.1ms steps)", "PACKETDELAY");
            optParser.AddOption(option1);
            optParser.AddOption(option2);
            optParser.AddOption(option3);
            optParser.AddOption(option4);
            optParser.AddOption(option5);
            optParser.AddOption(option6);
            optParser.AddOption(option7);
            helper.ProcessCommandLine();

            Client test = new Client(option1.Value, option2.Value, option3.Value, option4.Value, option5.Value, option6.Value, option7.Value, option8.Value);

            // run the test ...
            test.ClientServerTest();
            helper.Dispose();
        }
    }
}
