using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Net;
using System.Net.Cache;
using System.IO;
using System.Globalization;
using System.Drawing;
using System.Threading;

using Linn;
using Linn.Kinsky;

namespace OssKinskyMppRadio
{
    public interface IFeedDownloadConsole
    {
        void WriteTitle(string aTitle);
        void WriteLogo(Image aLogo);
        void WriteStationCount(int aStationCount);
    }

    internal class FeedDownloadConsoleNull : IFeedDownloadConsole
    {
        public FeedDownloadConsoleNull()
        {
        }

        public void WriteTitle(string aTitle)
        {
        }

        public void WriteLogo(Image aLogo)
        {
        }

        public void WriteStationCount(int aStationCount)
        {
        }
    }

    public interface IAudio
    {
        string Uri { get; }
        string Type { get; }
        uint Bitrate { get; }
    }

    public interface ILocation
    {
        string Country { get; }
        string Region { get; }
        double Longitude { get; }
        double Latitude { get; }
    }

    public interface IEntry
    {
        string Title { get; }
        string Link { get; }
        Image Logo { get; }
        string LogoUri { get; }
        string Description { get; }
        string Category { get; }
        IAudio Audio { get; }
        ILocation Location { get; }
    }

    public interface IFeed
    {
        Uri Uri { get; }
        string Title { get; }
        bool Downloaded { get; }
        string Link { get; }
        Image Logo { get; }
        string LogoUri { get; }
        DateTime Updated { get; }
        IList<IEntry> EntryList { get; }
    }

    internal class Location : ILocation
    {
        public Location(string aCountry, string aRegion, double aLongitude, double aLatitude)
        {
            iCountry = aCountry;
            iRegion = aRegion;
            iLongitude = aLongitude;
            iLatitude = aLatitude;
        }

        public string Country
        {
            get
            {
                return (iCountry);
            }
        }

        public string Region
        {
            get
            {
                return (iRegion);
            }
        }

        public double Longitude
        {
            get
            {
                return (iLongitude);
            }
        }

        public double Latitude
        {
            get
            {
                return (iLatitude);
            }
        }

        private string iCountry;
        private string iRegion;
        private double iLongitude;
        private double iLatitude;
    }

    internal class Audio : IAudio
    {
        public Audio(string aUri, string aType, uint aBitrate)
        {
            iUri = aUri;
            iType = aType;
            iBitrate = aBitrate;
        }

        public string Uri
        {
            get
            {
                return (iUri);
            }
        }

        public string Type
        {
            get
            {
                return (iType);
            }
        }

        public uint Bitrate
        {
            get
            {
                return (iBitrate);
            }
        }

        private string iUri;
        private string iType;
        private uint iBitrate;
    }

    internal class Entry : IEntry
    {
        public Entry(string aTitle, string aLink, Image aLogo, string aLogoUri,  string aDescription, string aCategory, IAudio aAudio, ILocation aLocation)
        {
            iTitle = aTitle;
            iLink = aLink;
            iLogo = aLogo;
            iLogoUri = aLogoUri;
            iDescription = aDescription;
            iCategory = aCategory;
            iAudio = aAudio;
            iLocation = aLocation;
            iMutex = new Mutex();
        }

        public string Title
        {
            get
            {
                return (iTitle);
            }
        }

        public string Link
        {
            get
            {
                return (iLink);
            }
        }

        public Image Logo
        {
            get
            {
                return (iLogo);
            }
        }

        public string LogoUri
        {
            get
            {
                return (iLogoUri);
            }
        }

        public string Description
        {
            get
            {
                return (iDescription);
            }
        }

        public string Category
        {
            get
            {
                return (iCategory);
            }
        }

        public IAudio Audio
        {
            get
            {
                return (iAudio);
            }
        }

        public ILocation Location
        {
            get
            {
                return (iLocation);
            }
        }

        private string iTitle;
        private string iLink;
        private Image iLogo;
        private string iLogoUri;
        private string iDescription;
        private string iCategory;
        private IAudio iAudio;
        private ILocation iLocation;
        private Mutex iMutex;
    }

    internal class Feed : IFeed
    {
        public Feed(Uri aUri, string aTitle)
        {
            iUri = aUri;
            iTitle = aTitle;
            iDownloaded = false;
            iMutex = new Mutex();
        }

        public Feed(Uri aUri, string aTitle, string aLink, Image aLogo, string aLogoUri, DateTime aUpdated)
        {
            iUri = aUri;
            iTitle = aTitle;
            iDownloaded = true;
            iLink = aLink;
            iLogo = aLogo;
            iLogoUri = aLogoUri;
            iUpdated = aUpdated;
            iEntryList = new List<IEntry>();
            iMutex = new Mutex();
        }

        public Uri Uri
        {
            get
            {
                return (iUri);
            }
        }

        public string Title
        {
            get
            {
                return (iTitle);
            }
        }

        public bool Downloaded
        {
            get
            {
                return (iDownloaded);
            }
        }

        private void CheckDownloaded()
        {
            if (!iDownloaded)
            {
                throw (new ApplicationException());
            }
        }

        public string Link
        {
            get
            {
                CheckDownloaded();
                return (iLink);
            }
        }

        public Image Logo
        {
            get
            {
                CheckDownloaded();
                return (iLogo);
            }
        }

        public string LogoUri
        {
            get
            {
                CheckDownloaded();
                return (iLogoUri);
            }
        }

        public DateTime Updated
        {
            get
            {
                CheckDownloaded();
                return (iUpdated);
            }
        }

        public IList<IEntry> EntryList
        {
            get
            {
                CheckDownloaded();
                iMutex.WaitOne();
                List<IEntry> list = new List<IEntry>(iEntryList);
                iMutex.ReleaseMutex();
                return (list.AsReadOnly());
            }
        }

        public void Add(IEntry aEntry)
        {
            iMutex.WaitOne();
            iEntryList.Add(aEntry);
            iMutex.ReleaseMutex();
        }

        private Uri iUri;
        private string iTitle;
        private bool iDownloaded;
        private string iLink;
        private Image iLogo;
        private string iLogoUri;
        private DateTime iUpdated;
        private List<IEntry> iEntryList;
        private Mutex iMutex;
    }

    public class Country
    {
        public static string Convert(string aCode)
        {
            if (iList == null)
            {
                iList = new SortedList<string, string>();

                var input = new StringReader(Properties.Resources.iso_3166_1_list_en);
                var reader = new XmlTextReader(input);
                var document = new XPathDocument(reader);
                var navigator = document.CreateNavigator();
                var nsmanager = new XmlNamespaceManager(navigator.NameTable);

                CultureInfo culture = Thread.CurrentThread.CurrentCulture;
                TextInfo converter = culture.TextInfo;

                foreach (XPathNavigator e in navigator.Select("/ISO_3166-1_List_en/ISO_3166-1_Entry"))
                {
                    string code = e.SelectSingleNode("ISO_3166-1_Alpha-2_Code_element", nsmanager).Value;
                    string country = e.SelectSingleNode("ISO_3166-1_Country_name", nsmanager).Value;
                    iList.Add(code, converter.ToTitleCase(country.ToLower()));
                }
            }

            try
            {
                return (iList[aCode]);
            }
            catch (KeyNotFoundException)
            {
            }

            return ("UNKNOWN");
        }

        private static SortedList<string, string> iList;
    }

    public class Lrf
    {
        public IFeed Download(Uri aUri, IFeedDownloadConsole aConsole)
        {
            Stream stream = iHttpClient.Request(aUri);

            if (stream == null)
            {
                return (null);
            }

            try
            {
                var reader = new XmlTextReader(stream);
                var document = new XPathDocument(reader);
                var navigator = document.CreateNavigator();
                var nsmanager = new XmlNamespaceManager(navigator.NameTable);

                nsmanager.AddNamespace("radio", "http://oss.linn.co.uk/Feeds/Radio");

                string title = Select(navigator, nsmanager, "/radio:radiofeed/radio:title");

                if (title == null)
                {
                    return (null);
                }

                aConsole.WriteTitle(title);

                string link = Select(navigator, nsmanager, "/radio:radiofeed/radio:link");

                if (link == null)
                {
                    return (null);
                }

                string logouri = Select(navigator, nsmanager, "/radio:radiofeed/radio:logo");

                if (logouri == null)
                {
                    return (null);
                }

                Image logo = RequestImage(logouri);

                if (logo == null)
                {
                    return (null);
                }

                aConsole.WriteLogo(logo);

                string updatedstring = Select(navigator, nsmanager, "/radio:radiofeed/radio:updated");

                if (updatedstring == null)
                {
                    return (null);
                }

                DateTime updated;

                try
                {
                    updated = DateTime.Parse(updatedstring, CultureInfo.InvariantCulture.DateTimeFormat);
                }
                catch (FormatException)
                {
                    return (null);
                }

                Feed feed = new Feed(aUri, title, link, logo, logouri, updated);

                foreach (XPathNavigator i in navigator.Select("/radio:radiofeed/radio:station", nsmanager))
                {
                    string ititle = Select(i, nsmanager, "radio:title");

                    if (ititle == null)
                    {
                        continue;
                    }

                    string ilink = Select(i, nsmanager, "radio:link");

                    if (ilink == null)
                    {
                        continue;
                    }

                    string ilogouri = Select(i, nsmanager, "radio:logo");

                    if (ilogouri == null)
                    {
                        continue;
                    }

                    Image ilogo = RequestImage(ilogouri);

                    if (ilogo == null)
                    {
                        continue;
                    }

                    string idescription = Select(i, nsmanager, "radio:description");

                    if (idescription == null)
                    {
                        continue;
                    }

                    string icategory = Select(i, nsmanager, "radio:category");

                    if (icategory == null)
                    {
                        continue;
                    }

                    string iaudiouri = Select(i, nsmanager, "radio:audio/radio:uri");

                    if (iaudiouri == null)
                    {
                        continue;
                    }

                    string iaudiotype = Select(i, nsmanager, "radio:audio/radio:type");

                    if (iaudiotype == null)
                    {
                        continue;
                    }

                    string iaudiobitrate = Select(i, nsmanager, "radio:audio/radio:bitrate");

                    if (iaudiobitrate == null)
                    {
                        continue;
                    }

                    uint bitrate;

                    try
                    {
                        bitrate = Convert.ToUInt32(iaudiobitrate);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }
                    catch (OverflowException)
                    {
                        continue;
                    }

                    string icode = Select(i, nsmanager, "radio:location/radio:country");

                    if (icode == null)
                    {
                        continue;
                    }

                    string icountry = Country.Convert(icode);

                    string iregion = Select(i, nsmanager, "radio:location/radio:region");

                    if (iregion == null)
                    {
                        continue;
                    }

                    string ilongitudes = Select(i, nsmanager, "radio:location/radio:longitude");

                    if (ilongitudes == null)
                    {
                        continue;
                    }

                    double ilongitude;

                    try
                    {
                        ilongitude = Convert.ToDouble(ilongitudes, iXmlCulture);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }
                    catch (OverflowException)
                    {
                        continue;
                    }

                    string ilatitudes = Select(i, nsmanager, "radio:location/radio:latitude");

                    if (ilatitudes == null)
                    {
                        continue;
                    }

                    double ilatitude;

                    try
                    {
                        ilatitude = Convert.ToDouble(ilatitudes, iXmlCulture);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }
                    catch (OverflowException)
                    {
                        continue;
                    }

                    Audio iaudio = new Audio(iaudiouri, iaudiotype, bitrate);
                    Location ilocation = new Location(icountry, iregion, ilongitude, ilatitude);

                    Entry entry = new Entry(ititle, ilink, ilogo, ilogouri, idescription, icategory, iaudio, ilocation);

                    feed.Add(entry);

                    aConsole.WriteStationCount(feed.EntryList.Count);
                }

                return (feed);
            }
            catch (XmlException)
            {
                return (null);
            }
            finally
            {
                stream.Close();
            }
        }

        private Image RequestImage(string aUri)
        {
            Stream stream = null;
            try
            {
                Uri uri = new Uri(aUri);

                stream = iHttpClient.Request(uri);

                if (stream != null)
                {
                    return (new Bitmap(stream));
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return (null);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        public Lrf(IHttpClient aHttpClient)
        {
            iHttpClient = aHttpClient;
            iMutex = new Mutex();
            iFeedList = new SortedList<string, IFeed>();
            iUriList = new List<string>();
            iSemaphore = new ManualResetEvent(false);
            iFeedDownloadConsoleNull = new FeedDownloadConsoleNull();
            iXmlCulture = NumberFormatInfo.InvariantInfo;
        }

        public void Add(Uri aUri, string aTitle)
        {
            Add(new Feed(aUri, aTitle));
        }

        public void Add(IFeed aFeed)
        {
            iMutex.WaitOne();

            try
            {
                iFeedList.Add(aFeed.Title, aFeed);
            }
            catch (ArgumentException)
            {
                iFeedList[aFeed.Title] = aFeed;
            }

            if (!aFeed.Downloaded)
            {
                iSemaphore.Set();
            }

            iMutex.ReleaseMutex();

            if (EventChanged != null)
            {
                EventChanged(this, EventArgs.Empty);
            }
        }

        public void Remove(IFeed aFeed)
        {
            iMutex.WaitOne();

            if (iFeedList.Remove(aFeed.Title))
            {
                iMutex.ReleaseMutex();

                if (EventChanged != null)
                {
                    EventChanged(this, EventArgs.Empty);
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        public void Start()
        {
            iMutex.WaitOne();

            if (iThread != null)
            {
                iMutex.ReleaseMutex();
                throw (new ApplicationException());
            }

            iThread = new Thread(new ThreadStart(Run));
            iThread.Priority = ThreadPriority.BelowNormal;
            iThread.IsBackground = true;
            iThread.Start();

            iMutex.ReleaseMutex();
        }

        public void Stop()
        {
            iMutex.WaitOne();

            if (iThread == null)
            {
                iMutex.ReleaseMutex();
                throw (new ApplicationException());
            }

            iThread.Abort();
            iThread.Join();
            iThread = null;

            iMutex.ReleaseMutex();
        }

        private IFeed FindFeedToDownload()
        {
            iMutex.WaitOne();

            foreach (IFeed f in iFeedList.Values)
            {
                if (!f.Downloaded)
                {
                    iMutex.ReleaseMutex();

                    return (f);
                }
            }

            iSemaphore.Reset();

            iMutex.ReleaseMutex();

            return (null);
        }

        private void Run()
        {
            try
            {
                iSemaphore.WaitOne();

                while (true)
                {
                    IFeed candidate = FindFeedToDownload();

                    if (candidate == null)
                    {
                        break;
                    }

                    IFeed feed = Download(candidate.Uri, iFeedDownloadConsoleNull);

                    if (feed != null)
                    {
                        Add(feed);
                    }
                    else
                    {
                        Remove(candidate);
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
        }

        private string Select(XPathNavigator aNavigator, XmlNamespaceManager aNsManager, string aPath)
        {
            var n = aNavigator.SelectSingleNode(aPath, aNsManager);

            if (n == null)
            {
                return (null);
            }

            return (n.Value);
        }

        public event EventHandler<EventArgs> EventChanged;

        public IList<IFeed> FeedList
        {
            get
            {
                iMutex.WaitOne();
                List<IFeed> list = new List<IFeed>(iFeedList.Values);
                iMutex.ReleaseMutex();
                return (list.AsReadOnly());
            }
        }

        private IHttpClient iHttpClient;
        private Mutex iMutex;
        private Thread iThread;
        private SortedList<string, IFeed> iFeedList;
        private List<string> iUriList;
        private ManualResetEvent iSemaphore;
        private FeedDownloadConsoleNull iFeedDownloadConsoleNull;
        private IFormatProvider iXmlCulture;
    }
}


