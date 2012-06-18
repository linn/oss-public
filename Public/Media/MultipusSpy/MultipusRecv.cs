using System;
using System.Net;
using Linn;
using Linn.Network;
using System.Threading;

class Program
{
    static void Main(string[] aArgs)
    {
        Console.WriteLine("Starting Receiver");

        AppNetwork app = new AppNetwork(aArgs);
        app.Start();
        
        byte[] addr = {239, 255, 19, 72}; 
        IPAddress ip = new IPAddress(addr);

        UdpMulticastReader reader = new UdpMulticastReader(app.Interface, ip, 51972);

        Thread.Sleep(500000);
    }
}

