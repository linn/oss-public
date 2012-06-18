using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

using KinskyMedia.Database;
using KinskyMedia.Scanner;

namespace KinskyMedia
{
    public class Program : IConsole
    {
        [STAThread]

        static void Main(string[] aArgs)
        {
            Program app = new Program();

            try
            {
                app.Run(aArgs);
            }
            catch (DatabaseError e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        public Program()
        {
        }

        public void Run(string[] aArgs)
        {
            if (aArgs.Length == 0)
            {
                Start();
            }
            else
            {
                switch (aArgs[0])
                {
                    case "mount":
                        if (aArgs.Length > 1)
                        {
                            if (aArgs.Length > 2)
                            {
                                Mount(aArgs[1], aArgs[2]);
                                return;
                            }
                            else
                            {
                                Console.WriteLine("Mount uri not specified");
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Mount id not specified");
                            return;
                        }
                    default:
                        Console.WriteLine("Invalid command");
                        return;
                }
            }
        }

        private void Mount(string aId, string aUri)
        {
            Engine engine = new Engine(this);

            engine.AddMount(aId, aUri);
        }


        private void Start()
        {
        }

        public void Write(string aMessage)
        {
            Console.Write(aMessage);
        }
    }
}
