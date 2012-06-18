using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

using System.Diagnostics;

namespace Kodegen
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length >= 2)
                {
                    string directory = Path.GetDirectoryName(args[0]);

                    if (directory.Length > 0)
                    {
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                    }

                    StreamWriter output = new StreamWriter(args[0]);

                    Template template = new Template(args[1]);

                    string[] arguments = new string[args.Length - 2];

                    Array.Copy(args, 2, arguments, 0, arguments.Length);

                    output.Write(template.Generate(arguments));

                    output.Flush();

                    output.Close();
                }
                else
                {
                    PrintUsage();
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.Write(e.Message);
                Debug.Write("\r\n");
#else
                Console.Write(e.Message);
                Console.Write("\r\n");
#endif
                System.Environment.Exit(1);
            }
        }

        private static void PrintUsage()
        {
            Console.Write("\n");
            Console.Write("Usage: Kodegen output-uri template-uri [arguments]\n");
            Console.Write("\n");
            Console.Write("Kodegen executes [template-uri] using [arguments], writing the results to [output-uri].\n");
            Console.Write("\n");
        }
    }
}
