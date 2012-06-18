using System;
using System.Collections.Generic;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using SneakyMedia.Scanner;
using SneakyMedia.Database;

namespace SneakyMedia.Query
{
    public class ModuleQueryCommsTelnet : ModuleQueryComms
    {
        public const int kPort = 50000;
        public const int kSlots = 10;

        public ModuleQueryCommsTelnet()
            : base ("Telnet", new Version(1, 0, 0, 0))
        {
        }

        protected override void OnCreate()
        {
            new Thread(Listen).Start();
        }

        protected override void OnLoad()
        {
        }

        private void Listen()
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, kPort));
            server.Listen(kSlots);

            while (true)
            {
                Socket session = server.Accept();
                new Session(Engine, session);
            }
        }

        internal class Session
        {
            IEngine iEngine;
            Socket iSocket;
            public StreamReader iReader;
            public StreamWriter iWriter;

            public Session(IEngine aEngine, Socket aSocket)
            {
                iEngine = aEngine;
                iSocket = aSocket;
                iReader = new StreamReader(new NetworkStream(iSocket, false));
                iWriter = new StreamWriter(new NetworkStream(iSocket, true));
                iWriter.AutoFlush = true;
                new Thread(Process).Start();
            }

            private void Process()
            {
                try
                {
                    while (true)
                    {
                        string line = iReader.ReadLine();

                        if (line == null)
                        {
                            break;
                        }

                        string[] split = line.Split(new char[] { ' ' });

                        List<string> query = new List<string>();

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
                            IList<IList<string>> result = iEngine.Query(query);

                            int x = 1;
                            int y = 1;

                            foreach (IList<string> row in result)
                            {
                                foreach (string column in row)
                                {
                                    iWriter.WriteLine("{0}.{1} {2}", y, x, column);
                                    x++;
                                }
                                y++;
                                x = 1;
                                iWriter.WriteLine(String.Empty);
                            }
                        }
                        catch (QueryError e)
                        {
                            iWriter.WriteLine(e.Message);
                        }
                    }
                }
                catch (IOException)
                {
                }

                iSocket.Close();
            }
        }
    }
}

