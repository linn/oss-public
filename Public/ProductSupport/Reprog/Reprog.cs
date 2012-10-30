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
        optParser.Usage = "usage: Reprog.exe [options] [target ugly name] [rom file | bundle file]";

        OptionParser.OptionBool optFallback = new OptionParser.OptionBool("-f", "--fallback", "Target fallback rather than main");
        OptionParser.OptionBool optNoExec = new OptionParser.OptionBool(null, "--noexec", "Do not reboot target after reprogramming");
        OptionParser.OptionBool optWait = new OptionParser.OptionBool("-w", "--wait", "Wait to discover rebooted target after reprogramming (ignored with --noexec)");
        OptionParser.OptionBool optNoTrust = new OptionParser.OptionBool(null, "--notrust", "Reprogram to factory fresh paying no attention to current flash contents");
        OptionParser.OptionBool optBootstrap = new OptionParser.OptionBool(null, "--bootstrap", "Additionally reprogram the boostrap");
        OptionParser.OptionBool optUnsafe = new OptionParser.OptionBool(null, "--unsafe", "Program directly to flash / no 2 phase programming.");
        OptionParser.OptionString optEmulator = new OptionParser.OptionString("-e", "--emulator", "", "Flash emulator name (SrecA)", "Emulator name");
        OptionParser.OptionString optOutput = new OptionParser.OptionString("-o", "--output", "", "Flash emulator output filename", "Output filename");
        
        
        optParser.AddOption(optFallback);
        optParser.AddOption(optNoExec);
        optParser.AddOption(optWait);
        optParser.AddOption(optNoTrust);
        optParser.AddOption(optBootstrap);
        optParser.AddOption(optUnsafe);
        optParser.AddOption(optEmulator);
        optParser.AddOption(optOutput);

        helper.Start();

        if (optParser.PosArgs.Count != 2)
        {
            Console.WriteLine(optParser.Help());
            System.Environment.Exit(1);
        }

        string uglyname = optParser.PosArgs[0];
        string xmlfile = optParser.PosArgs[1];

        // create the console

        IConsole console = new FlashInfoConsole();

        // create the reprogrammer

        Reprogrammer reprog = new Reprogrammer(helper.IpAddress, console, uglyname, xmlfile);

        reprog.Fallback = optFallback.Value;
        reprog.NoExec = optNoExec.Value;
        reprog.Wait = optWait.Value;
        reprog.NoTrust = optNoTrust.Value;
        reprog.Bootstrap = optBootstrap.Value;
        reprog.Emulator = optEmulator.Value;
        reprog.Output = optOutput.Value;
        reprog.Unsafe = optUnsafe.Value;

        // reprog

        if (!reprog.Execute())
        {
            reprog.Close();
            System.Environment.Exit(1);
        }

        reprog.Close();
        System.Environment.Exit(0);
    }
}






