using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Cache;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using Linn;
using Linn.Kinsky;
using Upnp;

namespace KinskyJukebox {

public class MediaCollection
{
    public readonly string[] AudioFileExtensions = { ".flac", ".mp", ".wav", ".aif", ".m4a", ".wma", ".ogg" };
    public const string kTuneInLocalQuery = "http://opml.radiotime.com/Browse.ashx?c=local&formats=mp3,wma,aac,wmvideo,ogg&partnerId=ah2rjr68";

    public enum SortType
    {
        eArtist,
        eAlbum,
        eGenre,
        eComposer,
        eArtistAlbum,
        eArtistAz,
        eTitleAz,
        eNew,
        eAll,
        eAlbumArtistAlbum,
        eAlbumArtist,
        eConductor,
    }

    public MediaCollection(HelperKinskyJukebox aHelper, DNodeCreated aNodeCreated, DProgressChanged aProgressChanged) {
        iCollectionDir = aHelper.OptionPageSetup.CollectionLocation;
        iNodeCreated = aNodeCreated;
        iProgressChanged = aProgressChanged;
        iHelper = aHelper;
    }

    public void Scan() {
        iCreateCollectionThread = new Thread(CreateCollection);
        iCreateCollectionThread.Name = "CreateCollection";
        iCreateCollectionThread.IsBackground = true;
        iCreateCollectionThread.Start();
        Linn.UserLog.WriteLine("Scan started");
    }

    public void QuickScan(string aLocation) {
        iQuickScanLocation = aLocation;
        iCreateCollectionThread = new Thread(CreateCollection);
        iCreateCollectionThread.Name = "CreateCollection";
        iCreateCollectionThread.IsBackground = true;
        iCreateCollectionThread.Start();
        Linn.UserLog.WriteLine("Quick Scan started");
    }

    public void StopScan() {
        Linn.UserLog.WriteLine("Scan stopped");
        iCreateCollectionThread.Abort();
    }

    public static bool IsCompilation(string aFileName, HelperKinskyJukebox aHelper) {
        if (aHelper.OptionPageSetup.CompilationsFolder != null && aHelper.OptionPageSetup.CompilationsFolder != "") {
            return Path.GetDirectoryName(aFileName).ToLowerInvariant().Contains(aHelper.OptionPageSetup.CompilationsFolder.ToLowerInvariant());
        }
        else {
            return (Path.GetDirectoryName(aFileName).ToLowerInvariant().Contains("compilation") ||
                    Path.GetDirectoryName(aFileName).ToLowerInvariant().Contains("various"));
        }
    }

    private void CreateCollection() {
        try {
            List<string> tagsArtist = new List<string>();
            List<string> tagsAlbum = new List<string>();
            List<string> tagsGenre = new List<string>();
            List<string> tagsComposer = new List<string>();
            List<string> tagsConductor = new List<string>();
            List<string> tagsArtistAlbum = new List<string>();
            List<string> tagsArtistAz = new List<string>();
            List<string> tagsTitleAz = new List<string>();
            List<string> tagsAlbumArtistAlbum = new List<string>();
            List<string> tagsAlbumArtist = new List<string>();
            List<TrackMetadata> trackList = new List<TrackMetadata>();

            iCurrent = 0;
            string directory = iCollectionDir;
            if (iQuickScanLocation != null) {
                directory = iQuickScanLocation;
            }
            // get all relevant audio files
            iProgressChanged(0, "Creating File List", Progress.State.eInProgress);
            DirectoryInfo di = new DirectoryInfo(directory);
            List<FileInfo> fi = new List<FileInfo>();
            fi.AddRange(di.GetFiles("*.*", SearchOption.AllDirectories));
            iProgressChanged(0, "Sorting File List", Progress.State.eInProgress);
            fi = fi.FindAll(delegate(FileInfo f) {
                return IsAudioFile(f);
            });
            iTotal += fi.Count;
            foreach (FileInfo f in fi) {
                // build the complete file list for this dir
                try {
                    string file = f.FullName;
                    TrackMetadata trackData = new TrackMetadata(file, iHelper);
                    trackList.Add(trackData);

                    tagsArtist.Add(MoveTheFromStart(trackData.Artist));
                    tagsAlbum.Add(MoveTheFromStart(trackData.Album));
                    tagsGenre.Add(MoveTheFromStart(trackData.Genre));
                    tagsComposer.Add(MoveTheFromStart(trackData.Composer));
                    tagsConductor.Add(MoveTheFromStart(trackData.Conductor));
                    tagsAlbumArtist.Add(MoveTheFromStart(trackData.AlbumArtist));
                    tagsAlbumArtistAlbum.Add(MoveTheFromStart(trackData.AlbumArtist) + "/" + trackData.Album);
                    if (trackData.IsCompilation) {
                        // for Artist-Album sort type, always label compilation album artists as various
                        // the track by track artist metadata will remain in tact
                        tagsArtistAlbum.Add(TrackMetadata.kCompilationsArtist + "/" + trackData.Album);
                    }
                    else {
                        tagsArtistAlbum.Add(MoveTheFromStart(trackData.Artist) + "/" + trackData.Album);
                    }

                    string trim = MoveTheFromStart(trackData.Artist).Trim(); // insure no whitespace at start
                    tagsArtistAz.Add(trim); // leave full artist until sorted, then drop to single letter

                    trim = MoveTheFromStart(trackData.Title).Trim(); // insure no whitespace at start
                    tagsTitleAz.Add(trim); // leave full title until sorted, then drop to single letter
                }
                catch (System.IO.PathTooLongException) {
                    try {
                        Linn.UserLog.WriteLine("WARNING: File ignored (path too long): " + f);
                    }
                    catch {
                        Linn.UserLog.WriteLine("WARNING: File ignored (path too long)");
                    }
                }

                // update the progress
                iCurrent++;
                iProgressChanged(iCurrent * 100 / iTotal, "Parsing: " + iCurrent + " of " + iTotal, Progress.State.eInProgress);
            }

            // initialise all nodes
            TreeNode nodeNewTracks = new TreeNode(SortTypeToString(SortType.eNew));
            TreeNode nodeArtistAlbum = new TreeNode(SortTypeToString(SortType.eArtistAlbum));
            TreeNode nodeArtist = new TreeNode(SortTypeToString(SortType.eArtist));
            TreeNode nodeAlbum = new TreeNode(SortTypeToString(SortType.eAlbum));
            TreeNode nodeGenre = new TreeNode(SortTypeToString(SortType.eGenre));
            TreeNode nodeComposer = new TreeNode(SortTypeToString(SortType.eComposer));
            TreeNode nodeConductor = new TreeNode(SortTypeToString(SortType.eConductor));
            TreeNode nodeArtistAz = new TreeNode(SortTypeToString(SortType.eArtistAz));
            TreeNode nodeTitleAz = new TreeNode(SortTypeToString(SortType.eTitleAz));
            TreeNode nodeAlbumArtistAlbum = new TreeNode(SortTypeToString(SortType.eAlbumArtistAlbum));
            TreeNode nodeAlbumArtist = new TreeNode(SortTypeToString(SortType.eAlbumArtist));
            TreeNode nodeAllTracks = new TreeNode(SortTypeToString(SortType.eAll));

            // create all nodes
            if (iHelper.OptionPageOrganisation.SortTypeArtistAlbum) {
                nodeArtistAlbum = CreateCollectionTag(SortType.eArtistAlbum, tagsArtistAlbum, trackList, nodeAllTracks, nodeNewTracks);
            }
            if (iHelper.OptionPageOrganisation.SortTypeArtist) {
                nodeArtist = CreateCollectionTag(SortType.eArtist, tagsArtist, trackList, nodeAllTracks, nodeNewTracks);
            }
            if (iHelper.OptionPageOrganisation.SortTypeAlbum) {
                nodeAlbum = CreateCollectionTag(SortType.eAlbum, tagsAlbum, trackList, nodeAllTracks, nodeNewTracks);
            }
            if (iHelper.OptionPageOrganisation.SortTypeGenre) {
                nodeGenre = CreateCollectionTag(SortType.eGenre, tagsGenre, trackList, nodeAllTracks, nodeNewTracks);
            }
            if (iHelper.OptionPageOrganisation.SortTypeComposer) {
                nodeComposer = CreateCollectionTag(SortType.eComposer, tagsComposer, trackList, nodeAllTracks, nodeNewTracks);
            }
            if (iHelper.OptionPageOrganisation.SortTypeConductor) {
                nodeConductor = CreateCollectionTag(SortType.eConductor, tagsConductor, trackList, nodeAllTracks, nodeNewTracks);
            }
            if (iHelper.OptionPageOrganisation.SortTypeArtistAz) {
                nodeArtistAz = CreateCollectionTag(SortType.eArtistAz, tagsArtistAz, trackList, nodeAllTracks, nodeNewTracks);
            }
            if (iHelper.OptionPageOrganisation.SortTypeTitleAz) {
                nodeTitleAz = CreateCollectionTag(SortType.eTitleAz, tagsTitleAz, trackList, nodeAllTracks, nodeNewTracks);
            }
            if (iHelper.OptionPageOrganisation.SortTypeAlbumArtistAlbum) {
                nodeAlbumArtistAlbum = CreateCollectionTag(SortType.eAlbumArtistAlbum, tagsAlbumArtistAlbum, trackList, nodeAllTracks, nodeNewTracks);
            }
            if (iHelper.OptionPageOrganisation.SortTypeAlbumArtist) {
                nodeAlbumArtist = CreateCollectionTag(SortType.eAlbumArtist, tagsAlbumArtist, trackList, nodeAllTracks, nodeNewTracks);
            }
            // add all nodes
            iProgressChanged(100, "Updating Music Collection", Progress.State.eInProgress);
            if (iHelper.OptionPageOrganisation.SortTypeNew) {
                iNodeCreated(nodeNewTracks);
            }
            if (iHelper.OptionPageOrganisation.SortTypeArtistAlbum) {
                iNodeCreated(nodeArtistAlbum);
            }
            if (iHelper.OptionPageOrganisation.SortTypeArtist) {
                iNodeCreated(nodeArtist);
            }
            if (iHelper.OptionPageOrganisation.SortTypeAlbum) {
                iNodeCreated(nodeAlbum);
            }
            if (iHelper.OptionPageOrganisation.SortTypeGenre) {
                iNodeCreated(nodeGenre);
            }
            if (iHelper.OptionPageOrganisation.SortTypeComposer) {
                iNodeCreated(nodeComposer);
            }
            if (iHelper.OptionPageOrganisation.SortTypeConductor) {
                iNodeCreated(nodeConductor);
            }
            if (iHelper.OptionPageOrganisation.SortTypeArtistAz) {
                iNodeCreated(nodeArtistAz);
            }
            if (iHelper.OptionPageOrganisation.SortTypeTitleAz) {
                iNodeCreated(nodeTitleAz);
            }
            if (iHelper.OptionPageOrganisation.SortTypeAlbumArtistAlbum) {
                iNodeCreated(nodeAlbumArtistAlbum);
            }
            if (iHelper.OptionPageOrganisation.SortTypeAlbumArtist) {
                iNodeCreated(nodeAlbumArtist);
            }
            if (iHelper.OptionPageOrganisation.SortTypeAll) {
                iNodeCreated(nodeAllTracks);
            }
            if (iHelper.OptionPageOrganisation.IncludeLocalRadio) {
                CreateLocalRadio();
            }
        }
        catch (Exception e) {
            string message = "The following error was encountered:" + Environment.NewLine + Environment.NewLine + e.Message;
            if (e.GetType() == typeof(ThreadAbortException)) {
                message = "Operation was cancelled by the user" + Environment.NewLine;
            }
            Linn.UserLog.WriteLine("Scan Failed: " + message);
            iProgressChanged(0, message, Progress.State.eFail);
            return;
        }
        finally {
            iProgressChanged(0, null, Progress.State.eComplete);
        }
        iProgressChanged(iTotal, null, Progress.State.eSuccess);
        Linn.UserLog.WriteLine("Scan complete. Total Tracks Scanned: " + iTotal);
    }

    private bool IsAudioFile(FileInfo aFile) {
        foreach (string ext in AudioFileExtensions) {
            if (aFile.Extension.ToLower().Contains(ext)) {
                return true;
            }
        }
        return false;
    }

    private TreeNode CreateCollectionTag(SortType aSortType, List<string> aTags, List<TrackMetadata> aTrackList, TreeNode aNodeAllTracks, TreeNode aNodeNewTracks) {
        TreeNode nodeAll = new TreeNode(SortTypeToString(aSortType));
        List<TrackMetadata> trackList = new List<TrackMetadata>(aTrackList);
        iProgressChanged(100, "Creating: " + SortTypeToString(aSortType), Progress.State.eInProgress);

        if (trackList.Count > 0) {
            // sort alphabetically by tag, retaining mapping to actual file name
            string[] tagArray = aTags.ToArray();
            TrackMetadata[] trackArray = trackList.ToArray();
            Array.Sort(tagArray, trackArray);

            // if sorting A to Z, have to insure remains sorted by tag rather than just letter
            // this is why we have waited to drop down to a single letter until after the sort
            if (aSortType == SortType.eArtistAz) {
                for (int i = 0; i < tagArray.Length; i++) {
                    // get first character
                    string letter = tagArray[i].ToUpper();
                    int loc = 0;
                    while (loc < letter.Length) {
                        if (Char.IsLetterOrDigit(letter.Substring(loc, 1)[0])) {
                            letter = letter.Substring(loc, 1);
                            break;
                        }
                        loc++;
                    }
                    if (!Char.IsLetter(letter[0])) {
                        if (Char.IsDigit(letter[0])) {
                            letter = "0-9";
                        }
                        else {
                            letter = "?";
                        }
                    }
                    tagArray[i] = (letter + " (Artist)");
                }
                Array.Sort(tagArray, trackArray);
            }
            else if (aSortType == SortType.eTitleAz) {
                for (int i = 0; i < tagArray.Length; i++) {
                    // get first character
                    string letter = tagArray[i].ToUpper(); 
                    int loc = 0;
                    while (loc < letter.Length) {
                        if (Char.IsLetterOrDigit(letter.Substring(loc, 1)[0])) {
                            letter = letter.Substring(loc, 1);
                            break;
                        }
                        loc++;
                    }
                    if (!Char.IsLetter(letter[0])) {
                        if (Char.IsDigit(letter[0])) {
                            letter = "0-9";
                        }
                        else {
                            letter = "?";
                        }
                    }
                    tagArray[i] = (letter + " (Title)");
                }
                Array.Sort(tagArray, trackArray);
            }

            trackList.Clear();
            TreeNode subNodeAll = null;
            string tagPrev = null;
            for (int i = 0; i < tagArray.Length; i++) {
                if (((tagArray[i] != tagPrev) && (i > 0)) || (i == (tagArray.Length - 1))) {
                    // if last tag, need to add last item
                    if (i == (tagArray.Length - 1)) {
                        trackList.Add(trackArray[i]); // will get redundantly added at the end as well - no consequence
                    }
                    // sort new file list alphapbetically
                    TrackMetadata[] sortedTrackList = trackList.ToArray();
                    string[] sorted = new string[trackList.Count];
                    for (int j = 0; j < trackList.Count; j++) {
                        string path = null;
                        if (aSortType != SortType.eTitleAz) {
                            // sort as Artist\Album\Disc#_Track#_Title 
                            if (trackList[j].IsCompilation) {
                                // insure compilations are ordered correctly, regardless of compilation option being set
                                path = TrackMetadata.kCompilationsArtist + "\\" + trackList[j].Album + "\\"; 
                            }
                            else {
                                path = trackList[j].Artist + "\\" + trackList[j].Album + "\\";
                            }
                            uint track = trackList[j].Track;
                            uint disc = trackList[j].Disc;
                            if (track > 0 || disc > 0) {
                                // insure tracks are ordered by track number if available
                                string name = trackList[j].Title.Trim();
                                if (track > 0) {
                                    name = track.ToString().PadLeft(3, '0') + "_" + name;
                                }
                                if (disc > 0) {
                                    name = disc.ToString().PadLeft(3, '0') + "_" + name;
                                }
                                path += name;
                            }
                        
                            // don't sort first part of path (Artist) by 'The '
                            // this means that for a search like genre, or artist a-z, tracks are ordered correctly and artist is still alphabetical
                            path = RemoveTheFromStart(path);
                        }
                        else {
                            // if sorting title A to Z, have to insure remains sorted by title tag only
                            // don't sort first part of path (Title) by 'The '
                            path = RemoveTheFromStart(trackList[j].Title).Trim();
                        }
                        sorted[j] = path; // get uri
                    }
                    Array.Sort(sorted, sortedTrackList);

                    // add all discoveries
                    if (i == 0) {
                        subNodeAll = new TreeNode(MoveTheToStart(tagArray[0])); // only discovered one track
                    }
                    else {
                        subNodeAll = new TreeNode(MoveTheToStart(tagArray[i - 1])); // discovered multiple tracks
                    }
                    subNodeAll.ImageIndex = 1;
                    subNodeAll.SelectedImageIndex = 1;
                    subNodeAll.Name = "1";
                    nodeAll.Nodes.Add(subNodeAll);
                    foreach (TrackMetadata data in sortedTrackList) {
                        TreeNode item = new TreeNode(data.Title);
                        item.Tag = data;
                        item.ToolTipText = data.ToString();
                        item.ImageIndex = 2;
                        item.SelectedImageIndex = 2;
                        if (data.AgeInDays >= 0 && data.AgeInDays <= iHelper.OptionPageOrganisation.NewMusicCutoffDays) {
                            item.ForeColor = kNewNodeColor;
                            subNodeAll.ForeColor = kNewNodeColor;
                        }
                        subNodeAll.Nodes.Add(item);
                    }
                    subNodeAll.ToolTipText = "Count: " + subNodeAll.Nodes.Count;
                    trackList.Clear();
                }
                trackList.Add(trackArray[i]);
                tagPrev = tagArray[i];
            }
        }
        if (nodeAll.Nodes.Count > 0) {
            if (!iSingleNodeCreated) {
                // create single bookmark node for new tracks
                aNodeNewTracks.Name = "1";
                aNodeNewTracks.ImageIndex = 0;
                aNodeNewTracks.SelectedImageIndex = 0;

                // create single bookmark node for all tracks
                aNodeAllTracks.Name = "1";
                aNodeAllTracks.ImageIndex = 0;
                aNodeAllTracks.SelectedImageIndex = 0;

                iSingleNodeCreated = true;
            }
            // create current node based on current sort type
            nodeAll.ToolTipText = "Count: " + nodeAll.Nodes.Count;
            nodeAll.ImageIndex = 0;
            nodeAll.SelectedImageIndex = 0;
            nodeAll.Name = "1";

            // create new tracks preset node based on sort type and add to new tracks bookmark node
            TreeNode subNodeNewAllTracks = new TreeNode("New: " + SortTypeToString(aSortType));
            subNodeNewAllTracks.ForeColor = kNewNodeColor;
            subNodeNewAllTracks.Name = "1";
            subNodeNewAllTracks.ImageIndex = 1;
            subNodeNewAllTracks.SelectedImageIndex = 1;
            aNodeNewTracks.Nodes.Add(subNodeNewAllTracks);
            foreach (TreeNode node in nodeAll.Nodes) {
                foreach (TreeNode subNode in node.Nodes) {
                    if (subNode.ForeColor == kNewNodeColor) {
                        subNodeNewAllTracks.Nodes.Add((TreeNode)subNode.Clone());
                    }
                }
            }
            aNodeNewTracks.ToolTipText = "Count: " + aNodeNewTracks.Nodes.Count;
            subNodeNewAllTracks.ToolTipText = "Count: " + subNodeNewAllTracks.Nodes.Count;

            // create all tracks preset node based on sort type and add to all tracks bookmark node
            TreeNode subnodeAllTracks = new TreeNode("All: " + SortTypeToString(aSortType));
            subnodeAllTracks.Name = "1";
            subnodeAllTracks.ImageIndex = 1;
            subnodeAllTracks.SelectedImageIndex = 1;
            aNodeAllTracks.Nodes.Add(subnodeAllTracks);
            foreach (TreeNode node in nodeAll.Nodes) {
                foreach (TreeNode subNode in node.Nodes) {
                    subnodeAllTracks.Nodes.Add((TreeNode)subNode.Clone());
                }
            }
            aNodeAllTracks.ToolTipText = "Count: " + aNodeAllTracks.Nodes.Count;
            subnodeAllTracks.ToolTipText = "Count: " + subnodeAllTracks.Nodes.Count;
        }
        iProgressChanged(100, null, Progress.State.eInProgress);
        return nodeAll;
    }

    private void CreateLocalRadio() {
        string geoLocation = "Unknown";
        List<string> links = new List<string>();
        XmlTextReader reader = null;
        TreeNode nodeLocalRadio = new TreeNode();
        try {
            iProgressChanged(0, "Adding: Local Radio", Progress.State.eInProgress);
            // generate list of local locations and links based on geoId (from server IP)
            reader = new XmlTextReader(GetHttpResponse(kTuneInLocalQuery).GetResponseStream());
            reader.WhitespaceHandling = WhitespaceHandling.None;
            reader.MoveToContent();
            while (reader.Read()) {
                if (reader.Name == "outline" && reader.IsStartElement()) {
                    if (reader.GetAttribute("type") == "link") {
                        links.Add(reader.GetAttribute("URL"));
                    }
                    else if (reader.GetAttribute("type") == "audio") {
                        links.Add(kTuneInLocalQuery); // no links - just local stations (will generate 1 preset only)
                        break;
                    }
                }
                else if (reader.Name == "title" && reader.IsStartElement()) { // geoId location
                    reader.Read();
                    geoLocation = reader.Value;
                }
                else if (reader.Name == "body" && !reader.IsStartElement()) { // end tag
                    break;
                }
            }
            // create single bookmark node for local radio
            nodeLocalRadio.Text = "Local Radio (" + geoLocation + ") Powered by TuneIn Radio";
            nodeLocalRadio.Name = "1";
            nodeLocalRadio.ImageIndex = 0;
            nodeLocalRadio.SelectedImageIndex = 0;
            int i = 0;
            // generate list of stations based on location based links
            foreach (string link in links) {
                iProgressChanged(i++ * 100 / links.Count, "Adding: Local Radio", Progress.State.eInProgress);
                reader = new XmlTextReader(GetHttpResponse(link).GetResponseStream());
                reader.WhitespaceHandling = WhitespaceHandling.None;
                reader.MoveToContent();
                TreeNode nodeAllStations = new TreeNode();
                while (reader.Read()) {
                    if (reader.Name == "outline" && reader.IsStartElement()) {
                        if (reader.GetAttribute("type") == "audio") {
                            Upnp.musicTrack upnpTrack = new Upnp.musicTrack();
                            upnpTrack.Title = reader.GetAttribute("text");
                            Upnp.artist artist = new Upnp.artist();
                            artist.Artist = "Powered by TuneIn Radio";
                            upnpTrack.Artist.Add(artist);
                            string info = reader.GetAttribute("subtext") + " (Bitrate: " + reader.GetAttribute("bitrate") + "kbps, Reliability: " + reader.GetAttribute("reliability") + "%)";
                            upnpTrack.Album.Add(info);
                            Upnp.resource res = new Upnp.resource();
                            res.Uri = reader.GetAttribute("URL");
                            upnpTrack.Res.Add(res);
                            upnpTrack.ArtworkUri.Add(reader.GetAttribute("image"));
                            upnpTrack.AlbumArtUri.Add(reader.GetAttribute("image"));
                            TrackMetadata data = new TrackMetadata(upnpTrack);

                            TreeNode item = new TreeNode(data.Title);
                            item.Tag = data;
                            item.ToolTipText = data.ToString();
                            item.ImageIndex = 2;
                            item.SelectedImageIndex = 2;
                            nodeAllStations.Nodes.Add(item);
                        }
                    }
                    else if (reader.Name == "title" && reader.IsStartElement()) { // geoId location
                        reader.Read();
                        geoLocation = reader.Value;
                    }
                    else if (reader.Name == "body" && !reader.IsStartElement()) { // end tag
                        break;
                    }
                }
                // create region preset node and add all stations
                TreeNode subnodeRegion = new TreeNode(geoLocation);
                subnodeRegion.Name = "1";
                subnodeRegion.ImageIndex = 1;
                subnodeRegion.SelectedImageIndex = 1;
                foreach (TreeNode node in nodeAllStations.Nodes) {
                    subnodeRegion.Nodes.Add((TreeNode)node.Clone());
                }
                subnodeRegion.ToolTipText = "Count: " + subnodeRegion.Nodes.Count;
                nodeLocalRadio.Nodes.Add(subnodeRegion);
            }
            nodeLocalRadio.ToolTipText = "Count: " + nodeLocalRadio.Nodes.Count;
            iNodeCreated(nodeLocalRadio);
        }
        catch (Exception e) {
            UserLog.WriteLine("CreateLocalRadio Error: " + e);
        }
        finally {
            if (reader != null) {
                reader.Close();
            }
        }
    }

    private HttpWebResponse GetHttpResponse(string aRequest) {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(aRequest);
        request.Credentials = CredentialCache.DefaultCredentials;
        request.Proxy.Credentials = CredentialCache.DefaultCredentials;
        try {
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
        }
        catch {
            // not supported for all platforms
        }
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        return response;
    }

    private string RemoveTheFromStart(string aString) {
        return RemoveTheFromStart(aString, false);
    }

    private string MoveTheFromStart(string aString) {
        return RemoveTheFromStart(aString, true);
    }

    private string RemoveTheFromStart(string aString, bool aAddReference) {
        string newString = aString;
        if (aString.StartsWith("The ", StringComparison.CurrentCultureIgnoreCase)) {
            newString = aString.Remove(0, 4).Trim();
            if (aAddReference) {
                newString += "<KJ:" + aString.Substring(0,3) + ">";
            }
        }
        return newString;
    }

    private string MoveTheToStart(string aString) {
        string newString = aString;
        int index = aString.IndexOf("<KJ:The>", StringComparison.CurrentCultureIgnoreCase);
        if (index >= 0) {
            newString = aString.Substring(index + 4, 3) + " " + aString.Remove(index, 8);
        }
        return newString;
    }

    public static string SortTypeToString(SortType aSortType) {
        switch (aSortType) {
            case SortType.eArtist: { return "Artist"; }
            case SortType.eAlbum: { return "Album"; }
            case SortType.eGenre: { return "Genre"; }
            case SortType.eComposer: { return "Composer"; }
            case SortType.eConductor: { return "Conductor"; }
            case SortType.eArtistAlbum: { return "Artist / Album"; }
            case SortType.eArtistAz: { return "Arist A to Z"; }
            case SortType.eTitleAz: { return "Title A to Z"; }
            case SortType.eNew: { return "New"; }
            case SortType.eAll: { return "All"; }
            case SortType.eAlbumArtist: { return "Album Artist"; }
            case SortType.eAlbumArtistAlbum: { return "Album Artist / Album"; }
            default: { return ""; }
        }
    }

    public delegate void DProgressChanged(int aPercent, string aMessage, Progress.State aProgressState);
    public delegate void DNodeCreated(TreeNode aNode);
    private Thread iCreateCollectionThread = null;
    private string iCollectionDir = "";
    private int iTotal = 0;
    private int iCurrent = 0;
    private DProgressChanged iProgressChanged;
    private DNodeCreated iNodeCreated;
    private HelperKinskyJukebox iHelper = null;
    private bool iSingleNodeCreated = false;
    private string iQuickScanLocation = null;

    public static Color kNewNodeColor = Color.Green;
    public static uint kPlaylistMaxTracks = 1000;
}

public class TrackMetadata
{
    public TrackMetadata(string aFile, HelperKinskyJukebox aHelper) {
        // File Info
        FilePath = Path.GetFullPath(aFile);
        try {
            AlbumArtPath = FindArtwork(Path.GetDirectoryName(FilePath));
        }
        catch (Exception) {
        }
        try {
            AgeInDays = (DateTime.Today - File.GetCreationTime(aFile)).Days;
        }
        catch (Exception) {
        }

        // Check for compilation
        try {
            IsCompilation = MediaCollection.IsCompilation(FilePath, aHelper);
        }
        catch (Exception) {
        }

        bool artistVarious = (aHelper.OptionPageOrganisation.IgnoreCompilations && IsCompilation);

        try {
            // populate metadata from tags if it is there
            TagLib.File tagFile = TagLib.File.Create(aFile);
            Title = tagFile.Tag.Title;
            Album = tagFile.Tag.Album;
            Artist = tagFile.Tag.FirstPerformer;
            AlbumArtist = tagFile.Tag.FirstAlbumArtist;
            Genre = tagFile.Tag.FirstGenre;
            Composer = tagFile.Tag.FirstComposer;
            Conductor = tagFile.Tag.Conductor;
            Track = tagFile.Tag.Track;
            Disc = tagFile.Tag.Disc;
            DiscCount = tagFile.Tag.DiscCount;
            Year = tagFile.Tag.Year.ToString();
            Duration = new Upnp.Time((int)tagFile.Properties.Duration.TotalSeconds).ToString();
            MimeType = tagFile.MimeType;
        }
        catch (Exception) {
        }

        // deal with missing data and unwanted data
        if (Title == null) { Title = Path.GetFileNameWithoutExtension(aFile); }
        if (Album == null) { Album = "Unknown"; }
        if (Artist == null) { Artist = "Unknown"; }
        else if (artistVarious) { Artist = kCompilationsArtist; }
        if (AlbumArtist == null) { AlbumArtist = "Unknown"; }
        if (Genre == null) { Genre = "Unknown"; }
        if (Composer == null) { Composer = "Unknown"; }
        if (Year == null || Year.Length != 4) { Year = "Unknown"; }
        if (Duration == null) { Duration = "Unknown"; }
        if (Conductor == null) { Conductor = "Unknown"; }
        if (MimeType == null) { MimeType = "Unknown"; }
        if (aHelper.OptionPageOrganisation.SortByYear) {
            Album = Year + "/" + Album;
        }
    }

    public TrackMetadata(Upnp.upnpObject aMusicTrack) {
        Assert.Check(aMusicTrack is Upnp.musicTrack);

        // File Info
        if (aMusicTrack.Res.Count > 0) {
            FilePath = aMusicTrack.Res[0].Uri;
        }
        try {
            AlbumArtPath = DidlLiteAdapter.ArtworkUri(aMusicTrack).AbsoluteUri;
        }
        catch { // invlid or missing album art
        }

        // populate metadata from upnp music track if it is there
        try {
            Title = aMusicTrack.Title;
            if (((Upnp.musicTrack)aMusicTrack).Album.Count > 0) {
                Album = ((Upnp.musicTrack)aMusicTrack).Album[0];
            }
            if (((Upnp.musicTrack)aMusicTrack).Artist.Count > 0) {
                Artist = ((Upnp.musicTrack)aMusicTrack).Artist[0].Artist;
                foreach (Upnp.artist artist in ((Upnp.musicTrack)aMusicTrack).Artist) {
                    if (artist.Role == null || artist.Role == "" || artist.Role == "Performer") {
                        Artist = artist.Artist;
                    }
                    else if (artist.Role == "Composer") {
                        Composer = artist.Artist;
                    }
                    else if (artist.Role == "AlbumArtist") {
                        AlbumArtist = artist.Artist;
                    }
                    else if (artist.Role == "Conductor") {
                        Conductor = artist.Artist;
                    }
                }
            }
            if (((Upnp.musicTrack)aMusicTrack).Genre.Count > 0) {
                Genre = ((Upnp.musicTrack)aMusicTrack).Genre[0];
            }
            Track = (uint)(((Upnp.musicTrack)aMusicTrack).OriginalTrackNumber);
            if (aMusicTrack is Upnp.musicTrack) {
                Disc = (uint)(((Upnp.musicTrack)aMusicTrack).OriginalDiscNumber);
                DiscCount = (uint)(((Upnp.musicTrack)aMusicTrack).OriginalDiscCount);
            }
            Year = ((Upnp.musicTrack)aMusicTrack).Date;
            if (aMusicTrack.Res.Count > 0) {
                Duration = aMusicTrack.Res[0].Duration;
                MimeType = aMusicTrack.Res[0].ProtocolInfo;
            }
        }
        catch (Exception) {
        }

        // deal with missing data and unwanted data
        if (Title == null) { Title = "Unknown"; }
        if (Album == null) { Album = "Unknown"; }
        if (Artist == null) { Artist = "Unknown"; }
        if (Genre == null) { Genre = "Unknown"; }
        if (Composer == null) { Composer = "Unknown"; }
        if (Conductor == null) { Conductor = "Unknown"; }
        if (AlbumArtist == null) { AlbumArtist = "Unknown"; }
        if (Year == null) { Year = "Unknown"; }
        if (Duration == null) { Duration = "Unknown"; }
        if (MimeType == null) { MimeType = "Unknown"; }
    }

    public override string ToString() {
        string str = "";
        str += "Title: " + Title + Environment.NewLine;
        str += "Artist: " + Artist + Environment.NewLine;
        str += "Album: " + Album + Environment.NewLine;
        str += "Genre: " + Genre + Environment.NewLine;
        str += "Composer: " + Composer + Environment.NewLine;
        str += "Conductor: " + Conductor + Environment.NewLine;
        str += "AlbumArtist: " + AlbumArtist + Environment.NewLine;
        str += "Track Number: " + Track + Environment.NewLine;
        str += "Disc Number: " + Disc + " of " + DiscCount + Environment.NewLine;
        str += "Year: " + Year + Environment.NewLine;
        str += "Duration: " + Duration + Environment.NewLine;
        str += "File: " + FilePath + Environment.NewLine;
        str += "Album Art: " + AlbumArtPath + Environment.NewLine;
        return str;
    }

    public Upnp.upnpObject GetUpnpMusicTrack(OptionPageSetup aUserOptionsSetup, HelperKinskyJukebox aHelper) {
        Upnp.upnpObject upnpMusicTrack = null;

        if (DiscCount > 0) {
            upnpMusicTrack = new Upnp.musicTrack();
        }
        else {
            upnpMusicTrack = new Upnp.musicTrack();
        }
        upnpMusicTrack.Id = FilePath;
        upnpMusicTrack.Title = Title;
        ((Upnp.musicTrack)upnpMusicTrack).Album.Add(Album);
        ((Upnp.musicTrack)upnpMusicTrack).Genre.Add(Genre);
        ((Upnp.musicTrack)upnpMusicTrack).OriginalTrackNumber = (int)Track;
        if (upnpMusicTrack is Upnp.musicTrack) {
            ((Upnp.musicTrack)upnpMusicTrack).OriginalDiscNumber = (int)Disc;
            ((Upnp.musicTrack)upnpMusicTrack).OriginalDiscCount = (int)DiscCount;
        }
        ((Upnp.musicTrack)upnpMusicTrack).Date = Year;

        Upnp.artist performer = new Upnp.artist();
        performer.Artist = Artist;
        performer.Role = "Performer";
        ((Upnp.musicTrack)upnpMusicTrack).Artist.Add(performer);

        Upnp.artist composer = new Upnp.artist();
        composer.Role = "Composer";
        composer.Artist = Composer;
        ((Upnp.musicTrack)upnpMusicTrack).Artist.Add(composer);

        Upnp.artist albumArtist = new Upnp.artist();
        albumArtist.Artist = AlbumArtist;
        albumArtist.Role = "AlbumArtist";
        ((Upnp.musicTrack)upnpMusicTrack).Artist.Add(albumArtist);

        Upnp.artist conductor = new Upnp.artist();
        conductor.Artist = Conductor;
        conductor.Role = "Conductor";
        ((Upnp.musicTrack)upnpMusicTrack).Artist.Add(conductor);

        Upnp.resource resource = new Upnp.resource();
        resource.Duration = Duration;
        resource.Uri = GetUri(FilePath, aHelper);
        resource.ProtocolInfo = "http-get:*:" + MimeType + ":*";
        upnpMusicTrack.Res.Add(resource);

        try {
            string art = GetUri(AlbumArtPath, aHelper);
            upnpMusicTrack.AlbumArtUri.Add(art);
            upnpMusicTrack.ArtworkUri.Add(art);
        }
        catch (Exception) { // if AlbumArtPath is invalid, just don't add artwork
        }

        return upnpMusicTrack;
    }

    public static string GetUri(string aFile, HelperKinskyJukebox aHelper) {
        Uri uri = null;
        if (Uri.TryCreate(aFile, UriKind.Absolute, out uri)) {
            if (uri.Scheme == Uri.UriSchemeHttp){
                return aFile; // already a valid uri
            }
        }

        if (aHelper.OptionPageSetup.UseHttpServer) {
            return aHelper.HttpServer.Uri(aFile);
        }
        else {
            string rel = aFile.Remove(0, aHelper.OptionPageSetup.CollectionLocation.Length);
            if (rel.StartsWith("/") || rel.StartsWith("\\")) {
                rel = rel.Remove(0, 1);
            }
            string path = aHelper.OptionPageSetup.CollectionHttpLocation;
            if (!(path.EndsWith("/") || path.EndsWith("\\"))) {
                path += "/";
            }
            rel = rel.Replace("\\", "/");
            path += rel;
            uri = new Uri(path);
            return uri.AbsoluteUri;
        }
    }

    private string FindArtwork(string aDirectory) {
        try {
            foreach (string ext in kImageFileExtensions) {
                string[] images = Directory.GetFiles(aDirectory, ext, SearchOption.TopDirectoryOnly); // non-recursive
                if (images.Length > 0) {
                    return Path.GetFullPath(images[0]);
                }
            }
        }
        catch (Exception) {
        }
        return null;
    }

    public static string kCompilationsArtist = "Various";
    private readonly string[] kImageFileExtensions = { "*older.jpg", "*.png", "*.jpg" };

    public string Title = null;
    public string Artist = null;
    public string AlbumArtist = null;
    public string Album = null;
    public string Genre = null;
    public string Composer = null;
    public string Conductor = null;
    public uint Track = 0;
    public uint Disc = 0;
    public uint DiscCount = 0;
    public string Year = null;
    public string Duration = null;
    public int AgeInDays = -1;
    public string FilePath = null;
    public string AlbumArtPath = null;
    public string MimeType = null;
    public bool IsCompilation = false;
}

}   // namespace KinskyJukebox

