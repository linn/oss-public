using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Cache;
using System.IO;
using System.Threading;

using SlimDX;
using SlimDX.DirectSound;
using SlimDX.Multimedia;

using Linn;
using Linn.Network;
using Linn.Ascii;

namespace OssKinskyMppSoundcard
{
    public interface ISource
    {
        string Name { get; }
        string Id { get; }
    }

    internal class Source : ISource
    {
        public Source(string aName, string aId)
        {
            iName = aName;
            iId = aId;
        }

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        public string Id
        {
            get
            {
                return (iId);
            }
        }

        private string iName;
        private string iId;
    }

    public class SoundDriver
    {
        public SoundDriver()
        {
            iMutex = new Mutex();
            iSourceList = new List<ISource>();
            iCapture = new DirectSoundCapture();

            DeviceCollection cdc = DirectSoundCapture.GetDevices();

            for (int i = 0; i < cdc.Count; i++)
            {
                DeviceInformation info = cdc[i];
                string name = info.Description;
                iSourceList.Add(new Source(info.Description, info.DriverGuid.ToString()));
            }
        }

        private void Lock()
        {
            iMutex.WaitOne();
        }

        private void Unlock()
        {
            iMutex.ReleaseMutex();
        }

        private void Initialise()
        {
            Unlock();
        }

        public void Start(IPAddress aInterface)
        {
            Lock();

            iHttpServer = new HttpServer(SourceList);
            iHttpServer.Start(aInterface);

            Unlock();
        }

        public string SourceUri(ISource aSource)
        {
            return (iHttpServer.Uri(aSource.Id));
        }

        public void Stop()
        {
            Lock();

            iHttpServer.Stop();

            Unlock();
        }

        public IList<ISource> SourceList
        {
            get
            {
                return (iSourceList.AsReadOnly());
            }
        }

        private Mutex iMutex;
        private HttpServer iHttpServer;
        private bool iInitialised;
        private List<ISource> iSourceList;
        private DirectSoundCapture iCapture;
    }

    public class HttpServer
    {
        private class HttpServerSession
        {
            internal class TcpSessionStream : TcpStream
            {
                internal TcpSessionStream()
                {
                }

                internal void SetSocket(Socket aSocket)
                {
                    iSocket = aSocket;
                }
            }

            public HttpServerSession(HttpServer aServer, IList<ISource> aSourceList, Socket aSocket)
            {
                iServer = aServer;
                iSourceList = aSourceList;
                iSocket = aSocket;

                iSession = new TcpSessionStream();
                iSession.SetSocket(iSocket);

                iReadBuffer = new Srb(kMaxReadBufferBytes, iSession);

                iWavFileHeader = new byte[44];

                iWavFileHeader[0] = 0x52; // ChunkId
                iWavFileHeader[1] = 0x49; // "RIFF"
                iWavFileHeader[2] = 0x46;
                iWavFileHeader[3] = 0x46;

                iWavFileHeader[4] = 0x24; // ChunkSize
                iWavFileHeader[5] = 0x00; // 36 + data size
                iWavFileHeader[6] = 0x00;
                iWavFileHeader[7] = 0x00;

                iWavFileHeader[8] = 0x57; // Format
                iWavFileHeader[9] = 0x41; // "WAVE"
                iWavFileHeader[10] = 0x56;
                iWavFileHeader[11] = 0x45;

                iWavFileHeader[12] = 0x66; // SubChunk1Id
                iWavFileHeader[13] = 0x6d; // "fmt "
                iWavFileHeader[14] = 0x74;
                iWavFileHeader[15] = 0x20;

                iWavFileHeader[16] = 0x10; // SubChunk1Size
                iWavFileHeader[17] = 0x00; // 16

                iWavFileHeader[18] = 0x00;
                iWavFileHeader[19] = 0x00;

                iWavFileHeader[20] = 0x01; // AudioFormat
                iWavFileHeader[21] = 0x00; // 1

                iWavFileHeader[22] = 0x02; // NumChannels
                iWavFileHeader[23] = 0x00; // 2

                iWavFileHeader[24] = 0x44; // SampleRate
                iWavFileHeader[25] = 0xac; // 44100
                iWavFileHeader[26] = 0x00;
                iWavFileHeader[27] = 0x00;

                iWavFileHeader[28] = 0x10; // ByteRate
                iWavFileHeader[29] = 0xb1; // 44100 * 2 * 2
                iWavFileHeader[30] = 0x02;
                iWavFileHeader[31] = 0x00;

                iWavFileHeader[32] = 0x04; // BlockAlign
                iWavFileHeader[33] = 0x00; // 4

                iWavFileHeader[34] = 0x10; // BitsPerSample
                iWavFileHeader[35] = 0x00; // 16

                iWavFileHeader[36] = 0x64; // SubChunk2Id
                iWavFileHeader[37] = 0x61; // "data"
                iWavFileHeader[38] = 0x74;
                iWavFileHeader[39] = 0x61;

                iWavFileHeader[40] = 0x00; // SubChunk2Size
                iWavFileHeader[41] = 0x00; // 0
                iWavFileHeader[42] = 0x00;
                iWavFileHeader[43] = 0x00;
            }

            public void Start()
            {
                Assert.Check(iThread == null);
                iThread = new Thread(new ThreadStart(Run));
                iThread.IsBackground = true;
                iThread.Name = "DirectSoundServerSession";
                iThread.Start();
            }

            public void Stop()
            {
                Assert.Check(iThread != null);
                iThread.Abort();
                iThread.Join();
            }

            private void Run()
            {
                try
                {
                    try
                    {
                        // GET [capture-device-id] HTTP/1.1
                        // Host: x.x.x.x
                        // Connection: close
                        // Range: bytes=0-

                        // get the request header

                        SortedList<string, string> headers = new SortedList<string, string>();

                        string method = String.Empty;
                        string uri = String.Empty;
                        string version = String.Empty;
                        bool first = true;

                        while (true)
                        {
                            byte[] line;

                            line = Ascii.Trim(iReadBuffer.ReadUntil(Ascii.kAsciiLf));

                            int bytes = line.Length;

                            if (bytes == 0)
                            {
                                if (first)
                                {
                                    continue; // a blank line before first header - ignore (RFC 2616 section 4.1)
                                }

                                break;
                            }

                            if (Ascii.IsWhitespace(line[0]))
                            {
                                continue; // a line starting with spaces is a continuation line
                            }

                            Parser parser = new Parser(line);

                            if (first)
                            { // method
                                method = Encoding.ASCII.GetString(parser.Next());
                                uri = Encoding.ASCII.GetString(parser.Next());
                                version = Encoding.ASCII.GetString(Ascii.Trim(parser.Remaining()));
                                first = false;
                            }
                            else
                            { // header
                                string field = Encoding.ASCII.GetString(parser.Next(Ascii.kAsciiColon));
                                string value = Encoding.ASCII.GetString(Ascii.Trim(parser.Remaining()));
                                if (!String.IsNullOrEmpty(field))
                                {
                                    headers.Add(field, value);
                                }
                            }
                        }

                        if (method != kMethodGet)
                        {
                            SendResponse("405 Method Not Supported");
                        }

                        if (uri.StartsWith("/"))
                        {
                            uri = uri.Substring(1);
                        }

                        // find capture device

                        foreach (ISource source in iSourceList)
                        {
                            if (source.Id == uri)
                            {
                                StreamSource(source);

                                try
                                {
                                    iSocket.Close();
                                }
                                catch (SocketException)
                                {
                                }

                                iServer.SessionClosed(this);
                                return;
                            }
                        }

                        SendResponse("404 Not Found");
                    }
                    catch (SocketException)
                    {
                    }
                }
                catch (ThreadAbortException)
                {
                }

                try
                {
                    iSocket.Close();
                }
                catch (SocketException)
                {
                }

                iServer.SessionClosed(this);
            }

            private void StreamSource(ISource aSource)
            {
                SendResponse("200 OK");

                iSocket.Send(iWavFileHeader);

                const int kAudioChunkBytes = 144 * 1024;
                const int kAudioChunks = 4;

                CaptureBuffer capture = CreateCaptureBuffer(aSource, kAudioChunks * kAudioChunkBytes);

                int offset = 0;

                NotificationPosition[] notifications = new NotificationPosition[kAudioChunks];
                WaitHandle[] handles = new WaitHandle[kAudioChunks];

                for (uint i = 0; i < kAudioChunks; i++)
                {
                    NotificationPosition notification = new NotificationPosition();
                    notification.Offset = offset;
                    notification.Event = new ManualResetEvent(false);
                    handles[i] = notification.Event;
                    notifications[i] = notification;
                    offset += kAudioChunkBytes;
                }

                capture.SetNotificationPositions(notifications);

                // Rotate notifications

                for (uint i = 0; i < kAudioChunks - 1; i++)
                {
                    WaitHandle a = handles[i];
                    handles[i] = handles[i + 1];
                    handles[i + 1] = a;
                }

                byte[] audio = new byte[kAudioChunkBytes];

                capture.Start(true);

                try
                {
                    while (true)
                    {
                        int x = WaitHandle.WaitAny(handles);
                        ManualResetEvent manual = handles[x] as ManualResetEvent;
                        manual.Reset();
                        capture.Read<byte>(audio, 0, kAudioChunkBytes, notifications[x].Offset, false);
                        iSocket.Send(audio);
                    }
                }
                catch(SocketException)
                {
                }

                capture.Stop();
            }

            private CaptureBuffer CreateCaptureBuffer(ISource aSource, int aBytes)
            {
                var format = new WaveFormat
                {
                    SamplesPerSecond = 44100,
                    BitsPerSample = 16,
                    Channels = 2,
                    FormatTag = WaveFormatTag.Pcm,
                    BlockAlignment = 4,
                    AverageBytesPerSecond = 44100 * 4 // 2 channels, 2 bytes per sample
                };

                var desc = new CaptureBufferDescription
                {
                    Format = format,
                    BufferBytes = aBytes
                };

                DirectSoundCapture capture = new DirectSoundCapture();

                return (new CaptureBuffer(capture, desc));
            }

            /*
            private bool Range(string[] aRequest, ref long aStartByte, ref long aEndByte)
            {
                foreach (string h in aRequest)
                {
                    string[] request = h.Split(':');
                    if (request[0] == kHeaderRange)
                    {
                        Console.WriteLine(h);

                        if (request.Length > 1)
                        {
                            string bytes = request[1].Replace("bytes=", "");
                            string[] splitBytes = bytes.Split('-');
                            try
                            {
                                if (splitBytes.Length > 0)
                                {
                                    aStartByte = long.Parse(splitBytes[0]);
                                }
                                if (splitBytes.Length > 1 && splitBytes[1] != string.Empty)
                                {
                                    aEndByte = long.Parse(splitBytes[1]);
                                }

                                Trace.WriteLine(Trace.kKinsky, "HttpServerSession: Range " + aStartByte + " - " + aEndByte);

                                return true;
                            }
                            catch (FormatException)
                            {
                            }
                        }
                    }
                }

                return false;
            }
            */

            private void SendResponse(string aStatus)
            {
                string response =  "HTTP/1.1 " + aStatus + "\r\n";
                response += "Server: KinskySoundcard\r\n";
                response += "Content-Length: 0\r\n";
                response += "Connection: close\r\n";
                response += "\r\n";

                byte[] buffer = Encoding.ASCII.GetBytes(response);
                iSocket.Send(buffer);
            }

            /*
            private void SendResponseGet(bool aPartialGet, string aFilename, long aStartBytes, long aEndBytes, long aTotalBytes)
            {
                string response = string.Empty;

                if (aPartialGet)
                {
                    response += "HTTP/1.1 206 Partial Content\r\n";
                    response += "Server: Kinsky\r\n";
                    //response += "Content-Disposition: attachment; filename=" + aFilename + "\r\n";
                    response += "Accept-Ranges: bytes\r\n";
                    response += "Content-Range: bytes " + aStartBytes.ToString() + "-" + aEndBytes.ToString() + "/" + aTotalBytes.ToString() + "\r\n";
                    response += "Content-Length: " + (aEndBytes - aStartBytes).ToString() + "\r\n";
                    response += "Connection: close\r\n";
                    response += "\r\n";
                }
                else
                {
                    response += "HTTP/1.1 200 OK\r\n";
                    response += "Server: Kinsky\r\n";
                    //response += "Content-Disposition: attachment; filename=" + aFilename + "\r\n";
                    response += "Accept-Ranges: bytes\r\n";
                    response += "Content-Length: " + aTotalBytes.ToString() + "\r\n";
                    response += "Connection: close\r\n";
                    response += "\r\n";
                }

                byte[] buffer = Encoding.ASCII.GetBytes(response);
                iSocket.Send(buffer);
            }
            */

            private HttpServer iServer;
            private IList<ISource> iSourceList;
            private Socket iSocket;

            private byte[] iWavFileHeader;

            private const int kMaxReadBufferBytes = 8000;
            private const string kMethodGet = "GET";
            private const string kHeaderRange = "Range";

            private HttpServerSession.TcpSessionStream iSession;
            private Srb iReadBuffer;

            private bool iExiting;
            private Thread iThread;
        }

        public HttpServer(IList<ISource> aSourceList)
        {
            iSourceList = aSourceList;
            iMutex = new Mutex();
            iSessions = new List<HttpServerSession>();
        }

        public void Start(IPAddress aInterface)
        {
            iThread = new Thread(new ThreadStart(Run));
            iThread.IsBackground = true;
            iThread.Name = "HttpServer";

            IPEndPoint endpoint = new IPEndPoint(aInterface, kPort);
            iBaseUri = "http://" + endpoint.ToString() + "/";

            try
            {
                iListener = new TcpListener(aInterface, kPort);
                iListener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                iListener.Start(kListenSlots);
                iThread.Start();
            }
            catch (SocketException e)
            {
                UserLog.WriteLine("Starting HTTP Server at " + iBaseUri + "...Failed (" + e.Message + ")");
            }
        }

        public void Stop()
        {
            Assert.Check(iThread != null);

            iThread.Abort();
            iThread.Join();

            try
            {
                iListener.Stop();
                UserLog.WriteLine("Stopping HTTP Server at " + iBaseUri + "...Success");
            }
            catch (SocketException e)
            {
                UserLog.WriteLine("Stopping HTTP Server at " + iBaseUri + "...Failed (" + e.Message + ")");
            }

            Assert.Check(iSessions.Count == 0);
        }

        public string Uri(string aId)
        {
            return iBaseUri + aId;
        }

        private void Run()
        {
            UserLog.WriteLine("Starting HTTP Server at " + iBaseUri + "...Success");

            try
            {
                while (true)
                {
                    Socket socket = iListener.AcceptSocket();

                    HttpServerSession session = new HttpServerSession(this, iSourceList, socket);

                    iMutex.WaitOne();
                    iSessions.Add(session);
                    int count = iSessions.Count;
                    Trace.WriteLine(Trace.kKinsky, "SoundcardServer.Run: Socket accepted from " + socket.RemoteEndPoint);
                    Trace.WriteLine(Trace.kKinsky, "SoundcardServer.Run: " + iSessions.Count + " active sessions");
                    session.Start();
                    iMutex.ReleaseMutex();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (SocketException)
            {
            }

            iMutex.WaitOne();

            foreach (HttpServerSession session in iSessions)
            {
                session.Stop();
            }

            iMutex.ReleaseMutex();

            UserLog.WriteLine("HTTP server stopped");
        }

        private void SessionClosed(HttpServerSession aSession)
        {
            iMutex.WaitOne();

            iSessions.Remove(aSession);

            int count = iSessions.Count;

            iMutex.ReleaseMutex();

            Trace.WriteLine(Trace.kKinsky, "HttpServer.EventSessionFinished: " + iSessions.Count + " active sessions");
        }

        private const int kListenSlots = 100;
        private const int kPort = 50009;

        private IList<ISource> iSourceList;
        private Mutex iMutex;
        private TcpListener iListener;
        private Thread iThread;
        private string iBaseUri;

        private List<HttpServerSession> iSessions;
    }
}


