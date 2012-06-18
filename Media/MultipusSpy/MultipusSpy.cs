using System;
using System.Net;
using Linn;
using Linn.Media.Multipus;
using System.Threading;

class Program
{
    static void Main(string[] aArgs)
    {
        
        Console.WriteLine("Starting Receiver");

        IHelper helper = new Helper(aArgs);
        helper.ProcessCommandLine();
        
        byte[] addr = {239, 253, 149, 202}; 
        IPAddress ip = new IPAddress(addr);
        Receiver recv = new Receiver(helper.Interface.Interface.Info.IPAddress, ip, 51972);
        recv.Start();
        while (true)
        {
        }
    }
}
