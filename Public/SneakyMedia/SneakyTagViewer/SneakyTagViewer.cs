using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using SneakyMedia.Database;

namespace SneakyTagViewer
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Program program = new Program(args);
            }
            else
            {
                PrintUsage();
            }
        }

        private Program(string[] args)
        {
            iEngine = Engine.CreateEngine();
            MountDirectory(args[0]);
        }

        private IEngine iEngine;

        private void MountDirectory(string aDirectory)
        {
            MountDirectory(new DirectoryInfo(aDirectory), String.Empty);
        }

        private void MountDirectory(DirectoryInfo aInfo, string aDirectory)
        {
            FileInfo[] files = aInfo.GetFiles();

            try
            {
                foreach (FileInfo file in files)
                {

                    Console.WriteLine(String.Format("File: {0}", aDirectory + file.Name));

                    IList<IMetadatum> tags = iEngine.Scan(file.FullName);

                    Console.WriteLine(String.Empty);

                    string format = "{0, -30} {1, -30} {2}";

                    Console.WriteLine(String.Format(format, "Namespace", "Name", "Value"));
                    Console.WriteLine(String.Format(format, "---------", "----", "-----"));

                    foreach (IMetadatum entry in tags)
                    {
                        Console.WriteLine(String.Format(format, entry.Tag.Ns, entry.Tag.Ns, entry.Value));
                    }

                    Console.WriteLine(String.Empty);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            DirectoryInfo[] dirs = aInfo.GetDirectories();

            try
            {
                foreach (DirectoryInfo dir in dirs)
                {
                    if ((dir.Attributes & FileAttributes.ReparsePoint) == 0)
                    {
                        MountDirectory(dir, aDirectory + dir.Name + "/");
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static void PrintUsage()
        {
            Console.Write("\n");
            Console.Write("Usage: SneakyTagViewer [directory]\n");
            Console.Write("\n");
            Console.Write("Displays the tags associated with the files in the specified directory\n");
            Console.Write("\n");
        }
    }
}
