using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Net;
using System.Xml;

using Upnp;

using Linn.Kinsky;
using Linn;

[assembly: ContentDirectoryFactoryType("OssKinskyMppShoutcast.ContentDirectoryFactoryShoutcast")]

namespace OssKinskyMppShoutcast
{
    public class ContentDirectoryFactoryShoutcast : IContentDirectoryFactory
    {
        public IContentDirectory Create(string aDataPath, IContentDirectorySupportV2 aSupport)
        {
            return (new ContentDirectoryShoutcast(aDataPath, aSupport));
        }
    }

    internal class ContentDirectoryShoutcast : IContentDirectory, IContainer, IDisposable
    {
        IContentDirectorySupportV2 iSupport;

        private OptionPage iOptionPage;

        private string iAlbumArtUri;

        private container iMetadata;

        private WebFetcher iWebFetcher;
        private WebFile iWebFile;
        
        private List<LiveStation> iStations;

        public ContentDirectoryShoutcast(string aDataPath, IContentDirectorySupportV2 aSupport)
        {
            iSupport = aSupport;
            iOptionPage = new OptionPage("Shoutcast");

            string installPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);

            iAlbumArtUri = aSupport.VirtualFileSystem.Uri(Path.Combine(installPath, "Shoutcast.png"));

            iStations = new List<LiveStation>();

            iMetadata = new album();
            iMetadata.Id = "shoutcast";
            iMetadata.Title = "Shoutcast";
            iMetadata.AlbumArtUri.Add(iAlbumArtUri);

            iWebFetcher = new WebFetcher(aDataPath);

            iWebFile = iWebFetcher.Create(new Uri("http://yp.shoutcast.com/sbin/newxml.phtml?search=radio&br=320&mt=audio/mpeg"), "stations.xml", 60);
            iWebFile.EventContentsChanged += WebFileContentsChanged;
            iWebFile.Open();
        }

        private void WebFileContentsChanged(object obj, EventArgs e)
        {
            bool added = false;

            try
            {
                XmlDocument document = new XmlDocument();
                StringReader reader = new StringReader(iWebFile.Contents);
                document.Load(reader);

                foreach (XmlNode station in document.SelectNodes("stationlist/station"))
                {
                    string name = GetContents(station, "@name");
                    string id = GetContents(station, "@id");

                    if (name != null && id != null)
                    {
                        LiveStation s = FindStation(id);

                        if (s == null)
                        {
                            s = new LiveStation(id, name, iAlbumArtUri, "http://yp.shoutcast.com/sbin/tunein-station.pls?id=" + id, 320);

                            added = true;

                            lock (this)
                            {
                                iStations.Add(s);
                            }
                        }
                    }
                }
            }
            catch (XmlException)
            {
            }

            if (added)
            {
                iMetadata.ChildCount = iStations.Count;

                if (EventContentAdded != null)
                {
                    EventContentAdded(this, EventArgs.Empty);
                }
            }
        }

        private string GetContents(XmlNode aNavigator, string aXpath)
        {
            XmlNode node = aNavigator.SelectSingleNode(aXpath);

            if (node == null)
            {
                return (null);
            }

            return (node.Value);
        }

        LiveStation FindStation(string aId)
        {
            foreach (LiveStation station in iStations)
            {
                if (station.Id == aId)
                {
                    return (station);
                }
            }

            return (null);
        }

        public string Name
        {
            get
            {
                return "Shoutcast";
            }
        }

        public string Company
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length != 0)
                {
                    return ((AssemblyCompanyAttribute)attributes[0]).Company;
                }
                else
                {
                    return "";
                }
            }
        }

        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public IContainer Root
        {
            get
            {
                return this;
            }
        }

        public IOptionPage OptionPage
        {
            get
            {
                return iOptionPage;
            }
        }
        
        public void Start()
        {
        }

        public void Stop()
        {
        }

        public uint Open()
        {
            lock (this)
            {
                return ((uint)iStations.Count);
            }
        }

        public DidlLite Items(uint aStartIndex, uint aCount)
        {
            int index = (int)aStartIndex;
            int count = (int)aCount;
            int required = index + count;
            int items = iStations.Count;

            if (required > items)
            {
                count -= required - items;
            }

            DidlLite didl = new DidlLite();

            lock (this)
            {
                foreach (LiveStation station in iStations.GetRange(index, count))
                {
                    didl.Add(station.Metadata);
                }
            }

            return (didl);
        }

        public void Refresh()
        {
            iWebFile.Refresh();
        }

        public IContainer ChildContainer(container aContainer)
        {
            return (null);
        }

        public container Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public bool HandleMove(DidlLite aDidlLite)
        {
            return (false);
        }

        public bool HandleInsert(DidlLite aDidlLite)
        {
            return (false);
        }

        public void Insert(string aAfterId, DidlLite aDidlLite)
        {
            throw new NotSupportedException();
        }

        public bool HandleDelete(DidlLite aDidlLite)
        {
            return (false);
        }

        public void Delete(string aId)
        {
            throw new NotSupportedException();
        }

        public bool HandleRename(upnpObject aObject)
        {
            return false;
        }

        public void Rename(string aId, string aTitle)
        {
            throw new NotSupportedException();
        }

        public DidlLite Search(string aSearchCriterea, uint aStartIndex, uint aCount)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> EventContentUpdated;
        public event EventHandler<EventArgs> EventContentAdded;
        public event EventHandler<EventArgsContentRemoved> EventContentRemoved;

        public void Dispose()
        {
            iWebFile.Dispose();
        }
    }

    internal class LiveStation
    {
        internal LiveStation(string aId, string aName, string aAlbumArtUri, string aAudioUri, int aKbps)
        {
            iId = aId;
            iName = aName;
            iAlbumArtUri = aAlbumArtUri;
            iAudioUri = aAudioUri;
            iKbps = aKbps * 125;

            iMetadata = new audioItem();
            iMetadata.Id = iId;
            iMetadata.Title = iName;
            iMetadata.AlbumArtUri.Add(iAlbumArtUri);

            resource res = new resource();
            res.Uri = iAudioUri;
            res.NrAudioChannels = 2;
            res.Bitrate = iKbps;
            res.ProtocolInfo = "http-get:*:audio/x-mpeg:*";

            iMetadata.Res.Add(res);
        }

        public string Id
        {
            get
            {
                return (iId);
            }
        }

        public audioItem Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        private string iId;
        private string iName;
        private string iAlbumArtUri;
        private string iAudioUri;
        private int iKbps;
        private audioItem iMetadata;
    }
}
