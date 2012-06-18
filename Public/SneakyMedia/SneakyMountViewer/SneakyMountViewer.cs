using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

using SneakyMedia.Database;

namespace SneakyMountViewer
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args.Length > 1)
                {
                    IEngine engine = Engine.CreateEngine();

                    Mount mount;

                    try
                    {
                        mount = engine.CreateMount(args[0], args[1]);

                        Console.WriteLine("Creating mount {0} at {1}", mount.MountId, mount.MountUri);
                        Console.WriteLine(String.Empty);

                        mount.EventScan += EventScan;

                        DateTime start = DateTime.Now;

                        mount.Scan();

                        DateTime end = DateTime.Now;

                        WriteMount(engine, mount);

                        TimeSpan duration = end.Subtract(start);

                        Console.WriteLine("Time to mount: {0}", duration);

                        /*
                        start = DateTime.Now;

                        mount.Scan();

                        end = DateTime.Now;

                        duration = end.Subtract(start);

                        Console.WriteLine("Time to remount: {0}", duration);
                        */
                    }
                    catch (MountAlreadyExistsError)
                    {
                        Console.WriteLine("Mount {0} already exists", args[0]);
                        Console.WriteLine(String.Empty);
                    }
                }
                else
                {
                    IEngine engine = Engine.CreateEngine();

                    Mount mount;
                        
                    try
                    {
                        mount = engine.FindMount(args[0]);

                        Console.WriteLine("Viewing mount {0} at {1}", mount.MountId, mount.MountUri);
                        Console.WriteLine(String.Empty);

                        WriteMount(engine, mount);
                    }
                    catch (MountNotFoundError)
                    {
                        Console.WriteLine("Mount {0} not found", args[0]);
                        Console.WriteLine(String.Empty);
                    }
                }
            }
            else
            {
                PrintUsage();
            }
        }

        private static void EventScan(object obj, EventArgs e)
        {
            Mount mount = obj as Mount;
            Console.WriteLine(mount.ScanUri);
        }

        private static void WriteMount(IEngine aEngine, Mount aMount)
        {
            string format = "{0, -30} {1, -30} {2}";

            IList<IItem> items = aEngine.QueryItems(aMount.MountId);

            foreach (IItem item in items)
            {
                Console.WriteLine("Item: {0}", item.ItemUri);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Format(format, "Namespace", "Name", "Value"));
                Console.WriteLine(String.Format(format, "---------", "----", "-----"));

                IList<IMetadatum> tags = aEngine.QueryItem(item);

                if (tags != null)
                {
                    foreach (IMetadatum entry in tags)
                    {
                        Console.WriteLine(String.Format(format, entry.Tag.Ns, entry.Tag.Name, entry.Value));
                    }
                }
                else
                {
                    Console.WriteLine("No tags");
                }

                Console.WriteLine(String.Empty);
            }
        }

        private static void PrintUsage()
        {
            Console.Write("\n");
            Console.Write("Usage: SneakyMountViewer [directory]\n");
            Console.Write("\n");
            Console.Write("Displays the tags associated with the specified mount point\n");
            Console.Write("\n");
        }
    }
}
