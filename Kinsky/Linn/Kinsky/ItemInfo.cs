using System.Collections.Generic;
using Upnp;
using System.Collections.ObjectModel;
using System;

namespace Linn.Kinsky
{
    public class ItemInfo
    {
    
        public ItemInfo(upnpObject aItem) : this(aItem, null) { }
        public ItemInfo(upnpObject aItem, upnpObject aParent)
        {
            iItem = aItem;
            iParent = aParent;
            iParsed = false;
            iLock = new object();
        }
    
    
        private string GetItemType(upnpObject aItem)
        {
            //todo:
            return aItem.Class;
        }
    
        public ReadOnlyCollection<KeyValuePair<string, string>> AllItems
        {
            get
            {
                lock (iLock)
                {
                    if (!iParsed)
                    {
                        foreach (KeyValuePair<string, string> s in Parse()) { }
                    }
                }
                return iFullInfoList.AsReadOnly();
            }
        }
    
        public ReadOnlyCollection<KeyValuePair<string, string>> DisplayItems
        {
            get
            {
                lock (iLock)
                {
                    if (!iParsed)
                    {
                        foreach (KeyValuePair<string, string> s in Parse()) { }
                    }
                }
                return iDisplayInfoList.AsReadOnly();
            }
        }
    
        public KeyValuePair<string, string>? DisplayItem(int aIndex)
        {
            lock (iLock)
            {
                if (!iParsed)
                {
                    int index = 0;
                    foreach (KeyValuePair<string, string> s in Parse())
                    {
                        if (index == aIndex)
                        {
                            return s;
                        }
                        index++;
                    }
                }
                else
                {
                    if (aIndex < iDisplayInfoList.Count)
                    {
                        return iDisplayInfoList[aIndex];
                    }
                }
            }
            return null;
        }
    
        private IEnumerable<KeyValuePair<string, string>> Parse()
        {
            List<KeyValuePair<string, string>> displayList = null;
            List<KeyValuePair<string, string>> fullList = null;
            lock (iLock)
            {
                if (iParsed)
                {
                    displayList = iDisplayInfoList;
                    fullList = iFullInfoList;
                }
            }
    
            if (displayList == null)
            {
                displayList = new List<KeyValuePair<string, string>>();
                fullList = new List<KeyValuePair<string, string>>();
    
                if (iItem != null)
                {
                    string type = GetItemType(iItem);
                    fullList.Add(new KeyValuePair<string, string>("Type", type));
    
                    string title = DidlLiteAdapter.Title(iItem);
                    if (title != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Title", title));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Title", title));
                    }
    
                    string album = DidlLiteAdapter.Album(iItem);
                    // if parent is a musicAlbum and album is blank, try to get album info from parent
                    if (album == string.Empty && iParent is musicAlbum)
                    {
                        album = DidlLiteAdapter.Album(iParent);
                    }
                    // don't display album field if we are a musicAlbum as it will be same as Title
                    if (album != string.Empty && !(iItem is musicAlbum))
                    {
                        // only insert album info for display if parent not a musicAlbum
                        if (!(iParent is musicAlbum))
                        {
                            displayList.Add(new KeyValuePair<string, string>("Album", album));
                            yield return displayList[displayList.Count - 1];
                        }
                        fullList.Add(new KeyValuePair<string, string>("Album", album));
                    }
    
                    string artist = DidlLiteAdapter.Artist(iItem);
                    string albumArtist = DidlLiteAdapter.AlbumArtist(iItem);
    
                    // don't display artist field if we are a person, as it will be same as title
                    if (artist != string.Empty && !(iItem is person))
                    {
                        // only display artist field when:
                        // our parent is not a music album 
                        // or we are a music album and album artist field is blank
                        // our parent is a music album but artist field is different from album artist field
    
                        if (!(iParent is musicAlbum) ||
                            (iItem is musicAlbum && albumArtist == string.Empty) ||
                            (iParent is musicAlbum && albumArtist != artist))
                        {
                            displayList.Add(new KeyValuePair<string, string>("Artist", artist));
                            yield return displayList[displayList.Count - 1];
                        }
                        fullList.Add(new KeyValuePair<string, string>("Artist", artist));
                    }
    
                    if (albumArtist != string.Empty)
                    {
                        // only display albumartist field when:
                        // we are a musicAlbum and album artist is different from artist
                        // or artist field is blank
    
                        if ((iItem is musicAlbum && albumArtist != artist) ||
                            artist == string.Empty)
                        {
                            displayList.Add(new KeyValuePair<string, string>("Album Artist", albumArtist));
                            yield return displayList[displayList.Count - 1];
                        }
                        fullList.Add(new KeyValuePair<string, string>("Album Artist", albumArtist));
                    }
    
                    string count = DidlLiteAdapter.Count(iItem);
                    if (count != string.Empty && count != "0")
                    {
                        displayList.Add(new KeyValuePair<string, string>("Count", count));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Count", count));
                    }
    
                    string genre = DidlLiteAdapter.Genre(iItem);
                    if (genre != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Genre", genre));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Genre", genre));
                    }
    
                    string releaseYear = DidlLiteAdapter.ReleaseYear(iItem);
                    if (releaseYear != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Release Year", releaseYear));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Release Year", releaseYear));
                    }
    
                    string originalTrackNumber = DidlLiteAdapter.OriginalTrackNumber(iItem);
                    if (originalTrackNumber != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Original Track No.", originalTrackNumber));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Original Track No.", originalTrackNumber));
                    }
    
                    string composer = DidlLiteAdapter.Composer(iItem);
                    if (composer != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Composer", composer));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Composer", composer));
                    }
    
                    string conductor = DidlLiteAdapter.Conductor(iItem);
                    if (conductor != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Conductor", conductor));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Conductor", conductor));
                    }
    
                    string actor = DidlLiteAdapter.Actor(iItem);
                    if (actor != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Actor", actor));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Actor", actor));
                    }
    
                    string director = DidlLiteAdapter.Director(iItem);
                    if (director != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Director", director));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Director", director));
                    }
    
                    string publisher = DidlLiteAdapter.Publisher(iItem);
                    if (publisher != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Publisher", publisher));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Publisher", publisher));
                    }
    
                    string contributer = DidlLiteAdapter.Contributor(iItem);
                    if (contributer != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Contributor", contributer));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Contributor", contributer));
                    }
    
                    string duration = DidlLiteAdapter.Duration(iItem);
                    if (duration != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Duration", duration));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Duration", duration));
                    }
    
                    string size = DidlLiteAdapter.Size(iItem);
                    if (size != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Size", size));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Size", size));
                    }
    
                    string bitrate = DidlLiteAdapter.Bitrate(iItem);
                    if (bitrate != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Bitrate", bitrate));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Bitrate", bitrate));
                    }
    
                    string sampleRate = DidlLiteAdapter.SampleRate(iItem);
                    if (sampleRate != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("SampleRate", sampleRate));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("SampleRate", sampleRate));
                    }
    
                    string bitDepth = DidlLiteAdapter.BitDepth(iItem);
                    if (bitDepth != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Bit Depth", bitDepth));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Bit Depth", bitDepth));
                    }
    
                    string mimeType = DidlLiteAdapter.MimeType(iItem);
                    if (mimeType != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Mime Type", mimeType));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Mime Type", mimeType));
                    }
    
                    string protocolInfo = DidlLiteAdapter.ProtocolInfo(iItem);
                    if (protocolInfo != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Protocol Info", protocolInfo));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Protocol Info", protocolInfo));
                    }
    
                    string description = DidlLiteAdapter.Description(iItem);
                    if (description != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Description", description));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Description", description));
                    }
    
                    string info = DidlLiteAdapter.Info(iItem);
                    if (info != string.Empty)
                    {
                        displayList.Add(new KeyValuePair<string, string>("Info", description));
                        yield return displayList[displayList.Count - 1];
                        fullList.Add(new KeyValuePair<string, string>("Info", description));
                    }
    
                    string uri = DidlLiteAdapter.Uri(iItem);
                    if (uri != string.Empty)
                    {
                        fullList.Add(new KeyValuePair<string, string>("Uri", uri));
                    }
    
                    Uri artworkUri = DidlLiteAdapter.ArtworkUri(iItem);
                    if (artworkUri != null)
                    {
                        fullList.Add(new KeyValuePair<string, string>("Artwork Uri", artworkUri.OriginalString));
                    }
                }
    
                lock (iLock)
                {
                    if (!iParsed)
                    {
                        iFullInfoList = fullList;
                        iDisplayInfoList = displayList;
                        iParsed = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < iDisplayInfoList.Count; i++)
                {
                    yield return iDisplayInfoList[i];
                }
            }
        }
    
        private List<KeyValuePair<string, string>> iFullInfoList;
        private List<KeyValuePair<string, string>> iDisplayInfoList;
        private bool iParsed;
        private object iLock;
        private upnpObject iItem;
        private upnpObject iParent;
    
    }
}
