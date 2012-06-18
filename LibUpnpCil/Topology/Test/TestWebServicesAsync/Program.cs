using System;
using System.Windows.Forms;

using Linn;

namespace TestWebServicesAsync
{	
    static class Program
    {	
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        private static void Main(string[] aArgs)
        {
            Helper helper = new Helper(aArgs);

            OptionParser.OptionUint iterations = new OptionParser.OptionUint("-n", "--num", 100, "Sets the number of iterations to perform.", "ITERATIONS");
            helper.OptionParser.AddOption(iterations);

            helper.ProcessCommandLine();

            if (!helper.OptionParser.HelpSpecified())
            {
                if (helper.OptionParser.PosArgs.Count < 1)
                {
                    Console.WriteLine(helper.OptionParser.Help());
                }
                else
                {
                    Application.Run(new Form1(helper, helper.OptionParser.PosArgs[0], iterations.Value));
                }
            }
            
            helper.Dispose();
        }
    }
}
