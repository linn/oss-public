using System.Net;
using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Collections.Generic;

namespace SneakyLastFm {

public class LastFmTrack
{
    public LastFmTrack(string aTitle, string aAlbum, string aArtist, string aUrl, string aImage, int aDuration) {
        iTitle = aTitle;
		iAlbum = aAlbum;
		iArtist = aArtist;
		iUrl = aUrl;
		iImage = aImage;
        iDuration = aDuration;
    }

    public string Title {
        get {
	       return iTitle;
	   }
    }

    public string Album {
        get {
	       return iAlbum;
	   }
    }

    public string Artist {
        get {
	       return iArtist;
	   }
    }

    public string Url {
        get {
	       return iUrl;
	   }
    }

    public string Image {
        get {
	       return iImage;
	   }
    }

    public int Duration {
        get {
            return iDuration;
        }
    }

    private string iTitle;
    private string iAlbum;
    private string iArtist;
    private string iUrl;
    private string iImage;
    private int iDuration;
}

public class LastFmService
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

    public event HandshakeCompletedEventHandler HandshakeCompleted;

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
            if (HandshakeCompleted != null)
            {
                HandshakeCompleted(this, new HandshakeCompletedEventArgs(e));
            }
        }
    }

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
            if (HandshakeCompleted != null)
            {
                HandshakeCompleted(this, new HandshakeCompletedEventArgs(result, iSession, streamUrl, iSubscriber, frameHack,
                    iBaseUrl, iBasePath, infoMsg, fingerPrint));
            }
        }
        catch (WebException e)
        {
            if (HandshakeCompleted != null)
            {
                HandshakeCompleted(this, new HandshakeCompletedEventArgs(e));
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

    public event ChangeStationCompletedEventHandler ChangeStationCompleted;

    public void ChangeStation(string aUrl) {
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
            if (ChangeStationCompleted != null)
            {
                ChangeStationCompleted(this, new ChangeStationCompletedEventArgs(e));
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
            if (ChangeStationCompleted != null)
            {
                ChangeStationCompleted(this, new ChangeStationCompletedEventArgs(result));
            }
        }
        catch (WebException e)
        {
            if (ChangeStationCompleted != null)
            {
                ChangeStationCompleted(this, new ChangeStationCompletedEventArgs(e));
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

    public event PlaylistCompletedEventHandler PlaylistCompleted;

    public void Playlist() {
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
            if (PlaylistCompleted != null)
            {
                PlaylistCompleted(this, new PlaylistCompletedEventArgs(e));
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
            List<LastFmTrack> playlist = new List<LastFmTrack>();
            foreach (XmlNode node in list)
            {
                string title = node.SelectSingleNode("title").InnerText;
                string album = node.SelectSingleNode("album").InnerText;
                string artist = node.SelectSingleNode("creator").InnerText;
                string url = node.SelectSingleNode("location").InnerText;
                string image = node.SelectSingleNode("image").InnerText;
                int duration = int.Parse(node.SelectSingleNode("duration").InnerText) / 1000;
                playlist.Add(new LastFmTrack(title, album, artist, url, image, duration));
            }
            if (PlaylistCompleted != null)
            {
                PlaylistCompleted(this, new PlaylistCompletedEventArgs("OK", playlist));
            }
        }
        catch (WebException e)
        {
            if (PlaylistCompleted != null)
            {
                PlaylistCompleted(this, new PlaylistCompletedEventArgs(e));
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

    public event CommandCompletedEventHandler CommandCompleted;

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
            if (CommandCompleted != null)
            {
                CommandCompleted(this, new CommandCompletedEventArgs(e));
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
            if (CommandCompleted != null)
            {
                CommandCompleted(this, new CommandCompletedEventArgs(result, state.Command));
            }
        }
        catch (System.Net.WebException e)
        {
            if (CommandCompleted != null)
            {
                CommandCompleted(this, new CommandCompletedEventArgs(e));
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

    private string NextParameter(StreamReader aStream) {
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
		for (int i = 0; i < hash.Length; i++) {
		    sb.Append(hash[i].ToString("x2"));
		}
		return sb.ToString();
    }

    private const string kVersion = "1.1.0.0";
    private const string kLanguage = "en";
    private const string kPlayer = "SneakyLastFm";

    private string iUsername = "";
    private string iPassword = "";

    private string iSession = "";
    private bool iSubscriber = false;
    private string iBaseUrl = "";
    private string iBasePath = "";
}

public class LastFmEventArgs : EventArgs
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

public delegate void HandshakeCompletedEventHandler(object sender, HandshakeCompletedEventArgs e);

public class HandshakeCompletedEventArgs : LastFmEventArgs
{
    internal HandshakeCompletedEventArgs(Exception aException)
        : base(aException)
    {
    }

    internal HandshakeCompletedEventArgs(string aResult, string aSession, string aStreamUrl, bool aSubscriber, string aFrameHack,
        string aBaseUrl, string aBasePath, string aInfoMessage, string aFingerPrintUploadUrl)
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

public delegate void ChangeStationCompletedEventHandler(object sender, ChangeStationCompletedEventArgs e);

public class ChangeStationCompletedEventArgs : LastFmEventArgs
{
    internal ChangeStationCompletedEventArgs(Exception aException)
        : base(aException)
    {
    }

    internal ChangeStationCompletedEventArgs(string aResult)
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

public delegate void PlaylistCompletedEventHandler(object sender, PlaylistCompletedEventArgs e);

public class PlaylistCompletedEventArgs : LastFmEventArgs
{
    internal PlaylistCompletedEventArgs(Exception aException)
        : base(aException)
    {
    }

    internal PlaylistCompletedEventArgs(string aResult, List<LastFmTrack> aPlaylist)
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

    public List<LastFmTrack> Playlist
    {
        get
        {
            RaiseExceptionIfNecessary();
            return iPlaylist;
        }
    }

    private string iResult;
    private List<LastFmTrack> iPlaylist;
}

public delegate void CommandCompletedEventHandler(object sender, CommandCompletedEventArgs e);

public class CommandCompletedEventArgs : LastFmEventArgs
{
    internal CommandCompletedEventArgs(Exception aException)
        : base(aException)
    {
    }

    internal CommandCompletedEventArgs(string aResult, string aCommand)
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

} // SneakyLastFm

