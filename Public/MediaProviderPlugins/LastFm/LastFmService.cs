using System.Net;
using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace OssKinskyMppLastFm
{
    internal interface ITrack
    {
        string Title { get; }
        string Album { get; }
        string Artist { get; }
        string Uri { get; }
        Image Artwork { get; }
        string ArtworkUri { get; }
        int Duration { get; }

        event EventHandler<EventArgs> EventArtworkDownloaded;
    }

    internal class Track : ITrack
    {
        public Track(string aTitle, string aAlbum, string aArtist, string aUri, string aArtworkUri, int aDuration)
        {
            iTitle = aTitle;
            iAlbum = aAlbum;
            iArtist = aArtist;
            iUri = aUri;
            iArtworkUri = aArtworkUri;
            iDuration = aDuration;

            iMutex = new Mutex(false);
            iArtworkDownloaded = false;
        }

        public string Title
        {
            get
            {
                return iTitle;
            }
        }

        public string Album
        {
            get
            {
                return iAlbum;
            }
        }

        public string Artist
        {
            get
            {
                return iArtist;
            }
        }

        public string Uri
        {
            get
            {
                return iUri;
            }
        }

        public Image Artwork
        {
            get
            {
                iMutex.WaitOne();
                if (!iArtworkDownloaded)
                {
                    iMutex.ReleaseMutex();

                    iArtworkDownloaded = true;
                    DownloadArtwork();
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
                return iArtwork;
            }
        }

        public string ArtworkUri
        {
            get
            {
                return iArtworkUri;
            }
        }

        public int Duration
        {
            get
            {
                return iDuration;
            }
        }

        public event EventHandler<EventArgs> EventArtworkDownloaded;

        private void DownloadArtwork()
        {
            try
            {
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(iArtworkUri);
                wreq.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                try
                {
                    wreq.BeginGetResponse(new AsyncCallback(GetResponseCallback), wreq);
                }
                catch (WebException)
                {
                }
            }
            catch (UriFormatException)
            {
            }
        }

        private void GetResponseCallback(IAsyncResult aResult)
        {
            try
            {
                HttpWebRequest wreq = aResult.AsyncState as HttpWebRequest;
                HttpWebResponse wresp = (HttpWebResponse)wreq.EndGetResponse(aResult);
                try
                {
                    Stream stream = wresp.GetResponseStream();
                    if (stream != null)
                    {
                        try
                        {
                            Bitmap image = new Bitmap(stream);

                            iMutex.WaitOne();

                            iArtwork = image;

                            iMutex.ReleaseMutex();

                            if (EventArtworkDownloaded != null)
                            {
                                EventArtworkDownloaded(this, EventArgs.Empty);
                            }
                        }
                        catch (ArgumentException)
                        {
                        }
                        stream.Close();
                    }
                }
                catch (ObjectDisposedException)
                {
                }
            }
            catch (WebException)
            {
            }
        }

        private string iTitle;
        private string iAlbum;
        private string iArtist;
        private string iUri;
        private string iArtworkUri;
        private int iDuration;

        private Mutex iMutex;
        private bool iArtworkDownloaded;
        private Image iArtwork;
    }

    internal class LastFmEventArgs : EventArgs
    {
        internal LastFmEventArgs()
        {
            iException = null;
        }

        internal LastFmEventArgs(Exception aException)
        {
            iException = aException;
        }

        protected void RaiseExceptionIfNecessary()
        {
            if (iException != null)
            {
                throw iException;
            }
        }

        private Exception iException;
    }

    internal class EventArgsHandshakeCompleted : LastFmEventArgs
    {
        internal EventArgsHandshakeCompleted(Exception aException)
            : base(aException)
        {
        }

        internal EventArgsHandshakeCompleted(string aResult, string aSession, string aStreamUrl, bool aSubscriber, string aFrameHack, string aBaseUrl, string aBasePath, string aInfoMessage, string aFingerPrintUploadUrl)
        {
            iResult = aResult;
            iSession = aSession;
            iStreamUrl = aStreamUrl;
            iSubscriber = aSubscriber;
            iFrameHack = aFrameHack;
            iBaseUrl = aBaseUrl;
            iBasePath = aBasePath;
            iInfoMessage = aInfoMessage;
            iFingerPrintUploadUrl = aFingerPrintUploadUrl;
        }

        public string Result
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iResult;
            }
        }

        public string Session
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iSession;
            }
        }

        public string SteamUrl
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iStreamUrl;
            }
        }

        public bool Subscriber
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iSubscriber;
            }
        }

        public string FrameHack
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iFrameHack;
            }
        }

        public string BaseUrl
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iBaseUrl;
            }
        }

        public string BasePath
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iBasePath;
            }
        }

        public string InfoMessage
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iInfoMessage;
            }
        }

        public string FingerPrintUploadUrl
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iFingerPrintUploadUrl;
            }
        }

        private string iResult;
        private string iSession;
        private string iStreamUrl;
        private bool iSubscriber;
        private string iFrameHack;
        private string iBaseUrl;
        private string iBasePath;
        private string iInfoMessage;
        private string iFingerPrintUploadUrl;
    }

    internal class EventArgsChangeStationCompleted : LastFmEventArgs
    {
        internal EventArgsChangeStationCompleted(Exception aException)
            : base(aException)
        {
        }

        internal EventArgsChangeStationCompleted(string aResult)
        {
            iResult = aResult;
        }

        public string Result
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iResult;
            }
        }

        private string iResult;
    }

    internal class EventArgsPlaylistCompleted : LastFmEventArgs
    {
        internal EventArgsPlaylistCompleted(Exception aException)
            : base(aException)
        {
        }

        internal EventArgsPlaylistCompleted(string aResult, IList<ITrack> aPlaylist)
        {
            iResult = aResult;
            iPlaylist = aPlaylist;
        }

        public string Result
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iResult;
            }
        }

        public IList<ITrack> Playlist
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iPlaylist;
            }
        }

        private string iResult;
        private IList<ITrack> iPlaylist;
    }

    internal class EventArgsCommandCompleted : LastFmEventArgs
    {
        internal EventArgsCommandCompleted(Exception aException)
            : base(aException)
        {
        }

        internal EventArgsCommandCompleted(string aResult, string aCommand)
        {
            iResult = aResult;
            iCommand = aCommand;
        }

        public string Result
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iResult;
            }
        }

        public string Command
        {
            get
            {
                RaiseExceptionIfNecessary();
                return iCommand;
            }
        }

        private string iResult;
        private string iCommand;
    }

    internal interface ILastFmService
    {
        void Handshake(string aUsername, string aPassword);
        void ChangeStation(string aUrl);
        void Playlist();

        void Command(string aCommand);
        void Skip();
        void Love();
        void Ban();

        event EventHandler<EventArgsHandshakeCompleted> EventHandshakeCompleted;
        event EventHandler<EventArgsChangeStationCompleted> EventChangeStationCompleted;
        event EventHandler<EventArgsPlaylistCompleted> EventPlaylistCompleted;
        event EventHandler<EventArgsCommandCompleted> EventCommandCompleted;
    }

    internal class LastFmService : ILastFmService
    {
        private class WebResponseState
        {
            public WebResponseState(WebRequest aRequest)
            {
                iRequest = aRequest;
            }

            public WebRequest WebRequest
            {
                get
                {
                    return iRequest;
                }
            }

            private WebRequest iRequest;
        }

        private class WebResponseStateCommand : WebResponseState
        {
            public WebResponseStateCommand(WebRequest aRequest, string aCommand)
                : base(aRequest)
            {
                iCommand = aCommand;
            }

            public string Command
            {
                get
                {
                    return iCommand;
                }
            }
            private string iCommand;
        }

        public void Handshake(string aUsername, string aPassword)
        {
            iUsername = aUsername;
            iPassword = aPassword;
            try
            {
                string uri = "http://ws.audioscrobbler.com/radio/handshake.php?version=" + kVersion + "&platform=" + Environment.OSVersion.Platform + "&username=" + iUsername + "&passwordmd5=" + PasswordMd5() + "&language=" + kLanguage + "&player=" + kPlayer;
                Console.WriteLine(uri);
                WebRequest request = WebRequest.Create(uri);
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                request.BeginGetResponse(HandshakeResponseCallback, new WebResponseState(request));
            }
            catch (System.Net.WebException e)
            {
                if (EventHandshakeCompleted != null)
                {
                    EventHandshakeCompleted(this, new EventArgsHandshakeCompleted(e));
                }
            }
        }

        public void ChangeStation(string aUrl)
        {
            try
            {
                string uri = "http://" + iBaseUrl + iBasePath + "/adjust.php?session=" + iSession + "&url=" + aUrl + "&lang=" + kLanguage;
                Console.WriteLine(uri);
                WebRequest request = WebRequest.Create(uri);
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                request.BeginGetResponse(ChangeStationResponseCallback, new WebResponseState(request));
            }
            catch (System.Net.WebException e)
            {
                if (EventChangeStationCompleted != null)
                {
                    EventChangeStationCompleted(this, new EventArgsChangeStationCompleted(e));
                }
            }
        }

        public void Playlist()
        {
            try
            {
                string uri = "http://" + iBaseUrl + iBasePath + "/xspf.php?sk=" + iSession + "&discovery=0&desktop=" + kVersion;
                Console.WriteLine(uri);
                WebRequest request = WebRequest.Create(uri);
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                request.BeginGetResponse(PlaylistResponseCallback, new WebResponseState(request));
            }
            catch (System.Net.WebException e)
            {
                if (EventPlaylistCompleted != null)
                {
                    EventPlaylistCompleted(this, new EventArgsPlaylistCompleted(e));
                }
            }
        }

        public void Command(string aCommand)
        {
            try
            {
                string uri = "http://" + iBaseUrl + iBasePath + "/control.php?session=" + iSession + "&command=" + aCommand;
                Console.WriteLine(uri);
                WebRequest request = WebRequest.Create(uri);
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                request.BeginGetResponse(CommandResponseCallback, new WebResponseStateCommand(request, aCommand));
            }
            catch (System.Net.WebException e)
            {
                if (EventCommandCompleted != null)
                {
                    EventCommandCompleted(this, new EventArgsCommandCompleted(e));
                }
            }
        }

        public void Skip()
        {
            Command("skip");
        }

        public void Love()
        {
            Command("love");
        }

        public void Ban()
        {
            Command("ban");
        }

        public event EventHandler<EventArgsHandshakeCompleted> EventHandshakeCompleted;
        public event EventHandler<EventArgsChangeStationCompleted> EventChangeStationCompleted;
        public event EventHandler<EventArgsPlaylistCompleted> EventPlaylistCompleted;
        public event EventHandler<EventArgsCommandCompleted> EventCommandCompleted;

        private void HandshakeResponseCallback(IAsyncResult aResult)
        {
            WebResponse response = null;
            StreamReader readStream = null;
            try
            {
                WebResponseState state = aResult.AsyncState as WebResponseState;
                response = state.WebRequest.EndGetResponse(aResult);
                Stream stream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                readStream = new StreamReader(stream, encode);

                string result = "OK";
                string streamUrl = "";
                string frameHack = "";
                string infoMsg = "";
                string fingerPrint = "";
                iSession = NextParameter(readStream);		// session
                if (iSession == "FAILED")
                {
                    result = NextParameter(readStream);		// msg
                }
                else
                {
                    streamUrl = NextParameter(readStream);			// stream url
                    iSubscriber = NextParameter(readStream) == "1";	// subscriber
                    frameHack = NextParameter(readStream);			// frame hack
                    iBaseUrl = NextParameter(readStream);		// base url
                    iBasePath = NextParameter(readStream);		// base path
                    infoMsg = NextParameter(readStream);			// info message
                    fingerPrint = NextParameter(readStream);			// finger print upload url
                }
                if (EventHandshakeCompleted != null)
                {
                    EventHandshakeCompleted(this, new EventArgsHandshakeCompleted(result, iSession, streamUrl, iSubscriber, frameHack, iBaseUrl, iBasePath, infoMsg, fingerPrint));
                }
            }
            catch (WebException e)
            {
                if (EventHandshakeCompleted != null)
                {
                    EventHandshakeCompleted(this, new EventArgsHandshakeCompleted(e));
                }
            }
            finally
            {
                if (readStream != null)
                {
                    readStream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        private void ChangeStationResponseCallback(IAsyncResult aResult)
        {
            WebResponse response = null;
            StreamReader readStream = null;
            try
            {
                WebResponseState state = aResult.AsyncState as WebResponseState;
                response = state.WebRequest.EndGetResponse(aResult);
                Stream stream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                readStream = new StreamReader(stream, encode);

                string result = NextParameter(readStream);
                if (EventChangeStationCompleted != null)
                {
                    EventChangeStationCompleted(this, new EventArgsChangeStationCompleted(result));
                }
            }
            catch (WebException e)
            {
                if (EventChangeStationCompleted != null)
                {
                    EventChangeStationCompleted(this, new EventArgsChangeStationCompleted(e));
                }
            }
            finally
            {
                if (readStream != null)
                {
                    readStream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        private void PlaylistResponseCallback(IAsyncResult aResult)
        {
            WebResponse response = null;
            StreamReader readStream = null;
            try
            {
                WebResponseState state = aResult.AsyncState as WebResponseState;
                response = state.WebRequest.EndGetResponse(aResult);
                Stream stream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                readStream = new StreamReader(stream, encode);

                XmlTextReader reader = new XmlTextReader(readStream);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);
                XmlNodeList list = xmlDoc.DocumentElement.SelectNodes("/playlist/trackList/track");
                string skips = xmlDoc.DocumentElement.SelectSingleNode("/playlist/link[@rel=\"http://www.last.fm/skipsLeft\"]").InnerText;
                Console.WriteLine("skips=" + skips);
                List<ITrack> playlist = new List<ITrack>();
                foreach (XmlNode node in list)
                {
                    string title = node.SelectSingleNode("title").InnerText;
                    string album = node.SelectSingleNode("album").InnerText;
                    string artist = node.SelectSingleNode("creator").InnerText;
                    string url = node.SelectSingleNode("location").InnerText;
                    string image = node.SelectSingleNode("image").InnerText;
                    int duration = int.Parse(node.SelectSingleNode("duration").InnerText) / 1000;
                    playlist.Add(new Track(title, album, artist, url, image, duration));
                }
                if (EventPlaylistCompleted != null)
                {
                    EventPlaylistCompleted(this, new EventArgsPlaylistCompleted("OK", playlist));
                }
            }
            catch (WebException e)
            {
                if (EventPlaylistCompleted != null)
                {
                    EventPlaylistCompleted(this, new EventArgsPlaylistCompleted(e));
                }
            }
            finally
            {
                if (readStream != null)
                {
                    readStream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        private void CommandResponseCallback(IAsyncResult aResult)
        {
            WebResponse response = null;
            StreamReader readStream = null;
            try
            {
                WebResponseStateCommand state = aResult.AsyncState as WebResponseStateCommand;
                response = state.WebRequest.EndGetResponse(aResult);
                Stream stream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                readStream = new StreamReader(stream, encode);

                string result = NextParameter(readStream);
                if (EventCommandCompleted != null)
                {
                    EventCommandCompleted(this, new EventArgsCommandCompleted(result, state.Command));
                }
            }
            catch (System.Net.WebException e)
            {
                if (EventCommandCompleted != null)
                {
                    EventCommandCompleted(this, new EventArgsCommandCompleted(e));
                }
            }
            finally
            {
                if (readStream != null)
                {
                    readStream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        private string NextParameter(StreamReader aStream)
        {
            string line = aStream.ReadLine();
            Console.WriteLine(line);
            return line.Split('=')[1];
        }

        private string PasswordMd5()
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(iPassword);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private const string kVersion = "3.1.5";
        private const string kLanguage = "en";
        private const string kPlayer = "KinskyDesktop";

        private string iUsername = "";
        private string iPassword = "";

        private string iSession = "";
        private bool iSubscriber = false;
        private string iBaseUrl = "";
        private string iBasePath = "";
    }
} // OssKinskyMppLastFm

