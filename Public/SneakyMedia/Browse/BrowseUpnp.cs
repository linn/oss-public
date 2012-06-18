using System;
using System.Collections.Generic;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using SneakyMedia.Database;

namespace SneakyMedia.Browse
{
    public class ModuleBrowseUpnp : ModuleBrowse
    {
        public const int kPort = 50002;
        public const int kSlots = 10;

        public ModuleBrowseUpnp()
            : base("Upnp", new Version(1, 0, 0, 0))
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
            private Socket iSocket;
            private StreamReader iReader;
            private StreamWriter iWriter;
            private ContentDirectory iContentDirectory;

            public Session(IEngine aEngine, Socket aSocket)
            {
                iSocket = aSocket;
                iReader = new StreamReader(new NetworkStream(iSocket, false));
                iWriter = new StreamWriter(new NetworkStream(iSocket, true));
                iWriter.AutoFlush = true;
                iContentDirectory = new ContentDirectory(aEngine);
                new Thread(Process).Start();
            }

            private void Process()
            {
                try
                {
                    // inputs

                    string objectId = "0";
                    string browseFlag = ContentDirectory.kBrowseDirectChildren;
                    string filter = "*";
                    uint startingIndex = 0;
                    uint requestedCount = 0;
                    string sortCriteria = "";

                    // outputs

                    string result;
                    uint numberReturned;
                    uint totalMatches;
                    string updateId;

                    while (true)
                    {
                        // read command

                        string line = iReader.ReadLine();

                        iWriter.WriteLine(String.Empty);

                        if (String.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        // process arguments

                        objectId = line;

                        // execute browse

                        iContentDirectory.Browse(objectId, browseFlag, filter, startingIndex, requestedCount, sortCriteria, out result, out numberReturned, out totalMatches, out updateId);

                        // display results

                        iWriter.WriteLine("Number returned: {0}", numberReturned);
                        iWriter.WriteLine("TotalMatches   : {0}", totalMatches);
                        iWriter.WriteLine("Update Id      : {0}", updateId);
                        iWriter.WriteLine(String.Empty);
                        iWriter.WriteLine(result);
                        iWriter.WriteLine(String.Empty);
                    }
                }
                catch (IOException)
                {
                }

                iSocket.Close();
            }
        }

        public class ContentDirectory
        {
            public const string kBrowseMetadata = "BrowseMetadata";
            public const string kBrowseDirectChildren = "BrowseDirectChildren";

            public ContentDirectory(IEngine aEngine)
            {
                iEngine = aEngine;
                iBrowser = new Browser(iEngine, "Upnp");
            }

            public void Browse(string aObjectId, string aBrowseFlag, string aFilter, uint aStartingIndex, uint aRequestedCount, string aSortCriteria, out string aResult, out uint aNumberReturned, out uint aTotalMatches, out string aUpdateId)
            {
                // is this the root id

                if (IsRoot(aObjectId))
                {
                    aNumberReturned = 5;
                    aTotalMatches = 5;
                    aUpdateId = "xxxx";
                    aResult = "The Root Level Result";

                    //<DIDL-Lite xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/" xmlns:dlna="urn:schemas-dlna-org:metadata-1-0/" xmlns:pv="http://www.pv.com/pvns/" xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/"><container id="0" parentID="-1" childCount="3" restricted="1" searchable="1"><upnp:searchClass includeDerived="1">object</upnp:searchClass><dc:title>root</dc:title><res protocolInfo="http-get:*:audio/x-mpegurl:*">http://172.20.0.161:8100/m3u/0.m3u</res><upnp:class>object.container</upnp:class></container></DIDL-Lite>                    
                    
                    return;
                }

                aNumberReturned = 10;
                aTotalMatches = 10;
                aUpdateId = "xxxx";
                aResult = "The Result";
            }

            private bool IsRoot(string aObjectId)
            {
                uint value;

                try
                {
                    value = uint.Parse(aObjectId);
                }
                catch (FormatException)
                {
                    return (false);
                }
                catch (OverflowException)
                {
                    return (false);
                }

                return (value == 0);
            }

            private IEngine iEngine;
            private IBrowser iBrowser;
        }
    }
}

