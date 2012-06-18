using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

using SneakyMedia.Database;
using SneakyMedia.Query;

namespace SneakyMountViewer
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                IEngine engine = Engine.CreateEngine();

                List<string> query = new List<string>(args);

                try
                {
                    IList<IList<string>> result = engine.Query(query);

                    foreach (IList<string> row in result)
                    {
                        foreach (string column in row)
                        {
                            Console.Write(" | ");
                            Console.Write(column);
                        }
                        Console.WriteLine(" |");
                    }
                }
                catch (QueryError e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                IEngine engine = Engine.CreateEngine();

                while (true)
                {
                    string line = Console.ReadLine();

                    string[] split = line.Split(new char[] { ' ' });

                    List<string> query = new List<string>(args);

                    System.Collections.IEnumerator enumerator = split.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        string s = (string)enumerator.Current;

                        if (s.StartsWith("\""))
                        {
                            s = s.Substring(1, s.Length - 1);

                            if (s.EndsWith("\""))
                            {
                                s = s.Substring(0, s.Length - 1);
                            }
                            else
                            {
                                while (enumerator.MoveNext())
                                {
                                    s += " ";
                                    s += (string)enumerator.Current;

                                    if (s.EndsWith("\""))
                                    {
                                        s = s.Substring(0, s.Length - 1);
                                        break;
                                    }
                                }
                            }
                        }
                        query.Add(s);
                    }

                    try
                    {
                        IList<IList<string>> result = engine.Query(query);

                        foreach (IList<string> row in result)
                        {
                            foreach (string column in row)
                            {
                                Console.Write(" | ");
                                Console.Write(column);
                            }
                            Console.WriteLine(" |");
                        }
                    }
                    catch (QueryError e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        private static void PrintUsage()
        {
            Console.Write("\n");
            Console.Write("Usage: SneakyMediaQuery [query]\n");
            Console.Write("\n");
            Console.Write("Query the SneakyMedia database\n");
            Console.Write("\n");
        }
    }
}
