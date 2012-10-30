using System;
using System.Net;
using System.Text;
using System.IO;

using Linn;
using Linn.ProductSupport;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

using Linn.ProductSupport.Flash;

public class Reprog
{
    class FlashInfoConsole : IConsole
    {
        internal FlashInfoConsole()
        {
            iProgressString = String.Empty;
        }

        public void Title(string aMessage)
        {
            String underline = new string('-', aMessage.Length);
            Console.WriteLine(String.Empty);
            Console.WriteLine(aMessage);
            Console.WriteLine(underline);
        }

        public void Write(string aMessage)
        {
            Console.Write(aMessage);
        }

        public void Newline()
        {
            Console.WriteLine(String.Empty);
        }

        public void ProgressOpen(int aMax)
        {
            iProgressMax = aMax;
            iProgressString = String.Format("0/{0}", iProgressMax);
            Console.Write(iProgressString);
        }

        public void ProgressSetValue(int aValue)
        {
            Backspace(iProgressString.Length);
            iProgressString = String.Format("{0}/{1}", aValue, iProgressMax);
            Console.Write(iProgressString);
        }

        public void ProgressClose()
        {
            Backspace(iProgressString.Length);
        }

        private void Backspace(int aCount)
        {
            if (aCount > 0)
            {
                string backspace = new string('\b', (int)aCount);
                string space = new string(' ', (int)aCount);
                Console.Write(backspace);
                Console.Write(space);
                Console.Write(backspace);
            }
        }

        private int iProgressMax;
        private string iProgressString;
    }

    [STAThread]

    static void Main(string[] aArgs)
    {
        HelperVolkano helper = new HelperVolkano(aArgs);
        OptionParser optParser = helper.OptionParser;
        optParser.Usage = "usage: FacDef.exe [options] [target ugly name]";

        OptionParser.OptionBool optNoExec = new OptionParser.OptionBool(null, "--noexec", "Do not reboot target after reprogramming");
        OptionParser.OptionBool optWait = new OptionParser.OptionBool("-w", "--wait", "Wait to discover rebooted target after reprogramming (ignored with --noexec)");

        optParser.AddOption(optNoExec);
        optParser.AddOption(optWait);

        helper.Start();

        if (optParser.PosArgs.Count != 1)
        {
            Console.WriteLine(optParser.Help());
            System.Environment.Exit(1);
        }

        string uglyname = optParser.PosArgs[0];

        // create the console

        IConsole console = new FlashInfoConsole();

        // create the reprogrammer

        FactoryDefaulter defaulter = new FactoryDefaulter(helper.IpAddress, console, uglyname);

        defaulter.NoExec = optNoExec.Value;
        defaulter.Wait = optWait.Value;

        if (!defaulter.Execute())
        {
            defaulter.Close();
            System.Environment.Exit(1);
        }

        defaulter.Close();
    }
}
