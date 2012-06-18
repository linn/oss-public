using System;
using System.IO;
using System.Xml;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using Linn;
using Linn.Kinsky;

namespace KinskyJukebox
{

public class Progress
{
    public enum State
    {
        eInProgress,
        eComplete,
        eSuccess,
        eFail,
    }
}

public class Presets
{
    public const uint kManifestVersion = 3;
    public static string kPresetDirectory = "/_Presets/";

    public class LoadManifestFailed : System.Exception
    {
        public LoadManifestFailed(string aMessage) : base("Could not load manifest file: " + aMessage) { }
    }

    public class NoPresetsCreated : System.Exception
    {
        public NoPresetsCreated() : base("No presets created (no music found in your Scan Directory).") { }
    }

    public static string DirectoryPath(OptionPageSetup aUserOptionsSetup) {
        return Path.GetFullPath(aUserOptionsSetup.CollectionLocation + kPresetDirectory);
    }

    public static string UriPath(HelperKinskyJukebox aHelper) {
        return TrackMetadata.GetUri(Presets.DirectoryPath(aHelper.OptionPageSetup), aHelper);
    }

    public Presets(HelperKinskyJukebox aHelper, TreeNodeCollection aUserCreatedNodes, DProgressChanged aProgressChanged, DNodeCreated aNodeCreated) {
        iPresetDir = DirectoryPath(aHelper.OptionPageSetup);
        iManifestPath = iPresetDir + "manifest.xml";
        iUserCreatedNodes = aUserCreatedNodes;
        iProgressChanged = aProgressChanged;
        iNodeCreated = aNodeCreated;
        iPlaylist = new Playlist();
        iHelper = aHelper;
    }

    public Presets(HelperKinskyJukebox aHelper, TreeNodeCollection aUserCreatedNodes, DProgressChanged aProgressChanged) :
        this(aHelper, aUserCreatedNodes, aProgressChanged, null) {
    }

    public void Export() {
        iExportThread = new Thread(Create);
        iExportThread.Name = "SavePresets";
        iExportThread.IsBackground = true;
        iExportThread.Start();
        Linn.UserLog.WriteLine("Save started");
    }

    public void StopExport() {
        Linn.UserLog.WriteLine("Save stopped");
        iExportThread.Abort();
    }

    public void Import() {
        iImportThread = new Thread(Load);
        iImportThread.Name = "ImportPresets";
        iImportThread.IsBackground = true;
        iReplaceIpAddress = false;
        iReplaceIpAddressMessage = false;
        iImportThread.Start();
        Linn.UserLog.WriteLine("Import started");
    }

    public void Import(OptionPageWizard.State aCorrectIpOption) {
        iImportThread = new Thread(Load);
        iImportThread.Name = "ImportPresets";
        iImportThread.IsBackground = true;

        if (aCorrectIpOption == OptionPageWizard.State.eAlways) {
            iReplaceIpAddress = true;
            iReplaceIpAddressMessage = true;
        }
        else if (aCorrectIpOption == OptionPageWizard.State.eNever) {
            iReplaceIpAddress = false;
            iReplaceIpAddressMessage = true;
        }
        else {
            // prompt
            iReplaceIpAddress = false;
            iReplaceIpAddressMessage = false;
        }

        iImportThread.Start();
        Linn.UserLog.WriteLine("Import started");
    }

    public void StopImport() {
        Linn.UserLog.WriteLine("Import stopped");
        iImportThread.Abort();
    }

    private void Create() {
        try {
            if (Directory.Exists(iPresetDir)) {
                iProgressChanged(0, "Clearing existing presets", Progress.State.eInProgress);
                Directory.Delete(iPresetDir, true);
            }
            iProgressChanged(0, "Creating presets directory", Progress.State.eInProgress);
            Directory.CreateDirectory(iPresetDir);
            iProgressChanged(0, "Creating manifest file", Progress.State.eInProgress);
            CreateManifest();
            iProgressChanged(0, "Creating presets", Progress.State.eInProgress);
            CreatePresets();
            if (iPresetCount == 0) {
                throw new NoPresetsCreated();
            }
        }
        catch (Exception e) {
            string message = "The following error was encountered:" + Environment.NewLine + Environment.NewLine + e.Message;
            if (e.GetType() == typeof(ThreadAbortException)) {
                message = "Operation was cancelled by the user" + Environment.NewLine;
            }
            Linn.UserLog.WriteLine("Save Failed: " + message);
            iProgressChanged(0, message, Progress.State.eFail);
            return;
        }
        finally {
            iProgressChanged(100, null, Progress.State.eComplete);
        }
        iProgressChanged(iPresetCount, null, Progress.State.eSuccess);
        Linn.UserLog.WriteLine("Save complete. Created " + iPresetCount + " Presets");
    }

    private void Load() {
        try {
            iProgressChanged(0, "Loading Manifest File", Progress.State.eInProgress);
            try {
                LoadManifest();
                iProgressChanged(0, "Loading Generated Presets", Progress.State.eInProgress);
                LoadGeneratedPresets();
            }
            catch (LoadManifestFailed exc) {
                Linn.UserLog.WriteLine(exc.Message);
            }
            iProgressChanged(0, "Loading Miscellaneous Presets", Progress.State.eInProgress);
            LoadMiscellaneousPresets();
        }
        catch (Exception e) {
            string message = "The following error was encountered:" + Environment.NewLine + Environment.NewLine + e.Message;
            if (e.GetType() == typeof(ThreadAbortException)) {
                message = "Operation was cancelled by the user" + Environment.NewLine;
            }
            Linn.UserLog.WriteLine("Import Failed: " + message);
            iProgressChanged(0, message, Progress.State.eFail);
            return;
        }
        finally {
            iProgressChanged(100, null, Progress.State.eComplete);
        }
        iProgressChanged(0, null, Progress.State.eSuccess);
        Linn.UserLog.WriteLine("Import complete");
    }

    private void CreatePresets() {
        int currentPreset = 0;
        foreach (TreeNode node in iUserCreatedNodes) { // bookmarks
            foreach (TreeNode subNode in node.Nodes) { // presets
                currentPreset++;
                iProgressChanged(currentPreset * 100 / iPresetCount, "Creating preset " + currentPreset + " of " + iPresetCount, Progress.State.eInProgress);
                Playlist playlist = new Playlist(Path.GetFullPath(iPresetDir + currentPreset.ToString() + ".dpl"));
                if (subNode.Nodes.Count <= MediaCollection.kPlaylistMaxTracks || !iHelper.OptionPageSetup.Randomize) {
                    // if playlist too large, use first 1000 tracks
                    uint trackCount = 0;
                    foreach (TreeNode track in subNode.Nodes) { // tracks
                        TrackMetadata data = (TrackMetadata)track.Tag;
                        Upnp.upnpObject upnpTrack = data.GetUpnpMusicTrack(iHelper.OptionPageSetup, iHelper);
                        if (upnpTrack != null && upnpTrack is Upnp.musicTrack) {
                            trackCount++;
                            if (trackCount > MediaCollection.kPlaylistMaxTracks) {
                                break; // only save max playlist tracks
                            }
                            playlist.Insert(playlist.Tracks.Count, upnpTrack);
                        }
                    }
                }
                else {
                    // if playlust too large, use random 1000 tracks
                    List<int> randomPlaylist = RandomPlaylist(subNode.Nodes.Count);
                    for (int i = 0; i < MediaCollection.kPlaylistMaxTracks; i++) {
                        TreeNode track = subNode.Nodes[randomPlaylist[i]];
                        TrackMetadata data = (TrackMetadata)track.Tag;
                        Upnp.upnpObject upnpTrack = data.GetUpnpMusicTrack(iHelper.OptionPageSetup, iHelper);
                        if (upnpTrack != null && upnpTrack is Upnp.musicTrack) {
                            playlist.Insert(playlist.Tracks.Count, upnpTrack);
                        }
                    }
                }
                playlist.Save();
            }
        }
    }

    private List<int> RandomPlaylist(int aMaxValue) {
        Assert.Check(aMaxValue > MediaCollection.kPlaylistMaxTracks);
        List<int> indexList = new List<int>();
        for (int i = 0; i < aMaxValue; i++) {
            indexList.Add(i);
        }
        List<int> list = new List<int>();
        Random rnd = new Random();
        int number = 0;
        // create non-repeating list of random numbers
        for (int i = 0; i < MediaCollection.kPlaylistMaxTracks; i++) {
            number = rnd.Next(0, indexList.Count); // Next(min inclusive, max exclusive)
            list.Add(indexList[number]);
            indexList.RemoveAt(number);
        }

        return list;
    }

    private void CreateManifest() {
        XmlDocument document = new XmlDocument();
        XmlElement manifest = document.CreateElement("linn", "Jukebox", "urn:linn-co-uk/jukebox");
        manifest.SetAttribute("version", kManifestVersion.ToString());

        XmlElement presetImageBase = document.CreateElement("linn", "ImageBase", "urn:linn-co-uk/jukebox");
        string imageBase = iHelper.OptionPageSetup.CollectionHttpLocation;
        if (!(imageBase.EndsWith("/") || imageBase.EndsWith("\\"))) {
            imageBase += "/";
        }
        imageBase = imageBase.Replace("\\", "/");
        XmlText presetImageBaseText = document.CreateTextNode(imageBase);
        presetImageBase.AppendChild(presetImageBaseText);
        manifest.AppendChild(presetImageBase);

        iPresetCount = 0;

        foreach (TreeNode node in iUserCreatedNodes) {
            XmlElement bookmark = document.CreateElement("linn", "Bookmark", "urn:linn-co-uk/jukebox");

            XmlElement bookmarkName = document.CreateElement("linn", "Name", "urn:linn-co-uk/jukebox");
            XmlText bookmarkNameText = document.CreateTextNode(node.Text);
            bookmarkName.AppendChild(bookmarkNameText);

            XmlElement bookmarkNumber = document.CreateElement("linn", "Number", "urn:linn-co-uk/jukebox");
            XmlText bookmarkNumberText = document.CreateTextNode((iPresetCount + 1).ToString());
            bookmarkNumber.AppendChild(bookmarkNumberText);

            bookmark.AppendChild(bookmarkName);
            bookmark.AppendChild(bookmarkNumber);
            manifest.AppendChild(bookmark);

            foreach (TreeNode subNode in node.Nodes) {
                XmlElement preset = document.CreateElement("linn", "Preset", "urn:linn-co-uk/jukebox");

                XmlElement presetName = document.CreateElement("linn", "Name", "urn:linn-co-uk/jukebox");
                XmlText presetNameText = document.CreateTextNode(subNode.Text);
                presetName.AppendChild(presetNameText);
                preset.AppendChild(presetName);

                XmlElement presetNumber = document.CreateElement("linn", "Number", "urn:linn-co-uk/jukebox");
                XmlText presetNumberText = document.CreateTextNode(subNode.Name);
                presetNumber.AppendChild(presetNumberText);
                preset.AppendChild(presetNumber);

                string imagePath = null;
                try {
                    TrackMetadata data = (TrackMetadata)subNode.Nodes[0].Tag;
                    string rel = data.AlbumArtPath.Remove(0, iHelper.OptionPageSetup.CollectionLocation.Length);
                    if (rel.StartsWith("/") || rel.StartsWith("\\")) {
                        rel = rel.Remove(0, 1);
                    }
                    imagePath = rel.Replace("\\", "/");
                }
                catch (Exception) {
                    imagePath = null;
                }
                if (imagePath != null && imagePath.Trim() != "") {
                    XmlElement presetImage = document.CreateElement("linn", "Image", "urn:linn-co-uk/jukebox");
                    XmlText presetImageText = document.CreateTextNode(imagePath);
                    presetImage.AppendChild(presetImageText);
                    preset.AppendChild(presetImage);
                }

                iPresetCount = int.Parse(subNode.Name);  
                manifest.AppendChild(preset);
            }
        }

        document.AppendChild(manifest);
        document.Save(iManifestPath);
    }

    private void LoadManifest() {
        XmlTextReader reader = null;
        try {
            iPresets.Clear();
            iBookmarks.Clear();
            reader = new XmlTextReader(iManifestPath);
            reader.WhitespaceHandling = WhitespaceHandling.None;
            reader.MoveToContent();
            if (reader.Name == "linn:Jukebox" && reader.IsStartElement()) {
                while (reader.Read()) {
                    if (reader.Name == "linn:Bookmark" && reader.IsStartElement()) {
                        AddNextEntry(reader, iBookmarks);
                    }
                    else if (reader.Name == "linn:Preset" && reader.IsStartElement()) {
                        AddNextEntry(reader, iPresets);
                    }
                    else if (reader.Name == "linn:Jukebox" && !reader.IsStartElement()) { // end tag
                        break;
                    }
                }
            }
        }
        catch (Exception e) {
            throw new LoadManifestFailed(e.Message);
        }
        finally {
            if (reader != null) {
                reader.Close();
            }
        }
    }

    private void LoadGeneratedPresets() {
        if (iNodeCreated != null) {
            // store list of bookmark pointers
            List<int> bookmarkLinks = new List<int>();
            foreach (KeyValuePair<int, string> lkvp in iBookmarks) {
                bookmarkLinks.Add(lkvp.Key);
            }
            bookmarkLinks.Add(iPresets.Count + 1);
            int count = 0;
            // create nodes
            foreach (KeyValuePair<int, string> bkvp in iBookmarks) {
                TreeNode bookmark = new TreeNode(bkvp.Value);
                int[] links = bookmarkLinks.ToArray();
                for (int i = links[count]; i < links[count + 1]; i++) {
                    string name = "";
                    if (iPresets.TryGetValue(i, out name)) {
                        iProgressChanged(i * 100 / iPresets.Count, "Loading preset " + i + " of " + iPresets.Count, Progress.State.eInProgress);
                        TreeNode preset = new TreeNode(name);
                        // track nodes
                        LoadPlaylistTracks(iPresetDir + i.ToString() + ".dpl", preset);
                        // preset nodes
                        preset.ToolTipText = "Count: " + preset.Nodes.Count;
                        preset.ImageIndex = 1;
                        preset.SelectedImageIndex = 1;
                        preset.Name = "1";
                        bookmark.Nodes.Add(preset);
                    }
                }
                count++;
                // bookmark nodes
                bookmark.ToolTipText = "Count: " + bookmark.Nodes.Count;
                bookmark.ImageIndex = 0;
                bookmark.SelectedImageIndex = 0;
                bookmark.Name = "1";
                iNodeCreated(bookmark);
            }
        }
    }

    private void LoadMiscellaneousPresets() {
        if (iNodeCreated != null && Directory.Exists(iPresetDir)) {
            // find all dpl files that were not loaded as generated presets
            string[] files = Directory.GetFiles(iPresetDir, "*.dpl*", SearchOption.TopDirectoryOnly); // recursive
            List<string> misc = new List<string>();
            foreach (string file in files) {
                try {
                    // numeric but not included in manifest - not a generated preset
                    if (uint.Parse(Path.GetFileNameWithoutExtension(file)) > iPresets.Count) {
                        misc.Add(file);
                    }
                }
                catch (FormatException) {
                    // non numeric - not a generated preset
                    misc.Add(file);
                }
            }
            string[] miscFiles = misc.ToArray();
            Array.Sort(miscFiles);
            // add all dpl files that were not loaded as generated presets
            if (miscFiles.Length > 0) {
                TreeNode bookmark = new TreeNode(kMiscNodeName);
                for (int i = 0; i < miscFiles.Length; i++) {
                    iProgressChanged((i + 1) * 100 / miscFiles.Length, "Loading misc preset " + (i + 1) + " of " + miscFiles.Length, Progress.State.eInProgress);
                    TreeNode preset = new TreeNode(Path.GetFileNameWithoutExtension(miscFiles[i]));
                    // track nodes
                    LoadPlaylistTracks(miscFiles[i], preset);
                    // preset nodes
                    preset.ToolTipText = "Count: " + preset.Nodes.Count;
                    preset.ImageIndex = 1;
                    preset.SelectedImageIndex = 1;
                    preset.Name = "1";
                    bookmark.Nodes.Add(preset);
                }
                // bookmark node
                bookmark.ToolTipText = "Count: " + bookmark.Nodes.Count;
                bookmark.ImageIndex = 0;
                bookmark.SelectedImageIndex = 0;
                bookmark.Name = "1";
                iNodeCreated(bookmark);
            }
        }
    }
    
    private void LoadPlaylistTracks(string aPlaylist, TreeNode aPreset) {
        string pl = Path.GetFullPath(aPlaylist);
        uint trackCount = 0;
        if (File.Exists(pl)) {
            iPlaylist.Load(pl);
            if (iPlaylist.Tracks.Count > 0) {
                foreach (Upnp.upnpObject track in iPlaylist.Tracks) {
                    if (track is Upnp.musicTrack) {
                        trackCount++;
                        if (trackCount > MediaCollection.kPlaylistMaxTracks) {
                            return; // only import max playlist tracks
                        }
                        // offer to update IP address (IpV4 only) for imported tracks if it is different from the user setup IP address
                        if (track.Res.Count > 0) {
                            Uri imported = new Uri(track.Res[0].Uri);
                            Uri stored;
                            if (!iHelper.OptionPageSetup.UseHttpServer) {
                                stored = new Uri(iHelper.OptionPageSetup.CollectionHttpLocation);
                            }
                            else {
                                stored = new Uri(iHelper.HttpServer.Uri("FILE"));
                            }
                            if (stored.Scheme == Uri.UriSchemeHttp && stored.HostNameType == UriHostNameType.IPv4 && imported.HostNameType == UriHostNameType.IPv4) {
                                if (stored.Host != imported.Host) {
                                    if (!iReplaceIpAddressMessage) {
                                        DialogResult result = MessageBox.Show("Would you like to update the imported IP addresses to match the user configured IP address?" + Environment.NewLine + Environment.NewLine + "User configured IP address: " + stored.Host + Environment.NewLine + "Imported IP address: " + imported.Host + Environment.NewLine, "Import: IP Address Mismatch Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                                        if (result == DialogResult.Yes) {
                                            iReplaceIpAddress = true;
                                        }
                                        iReplaceIpAddressMessage = true;
                                    }
                                    if (iReplaceIpAddress) {
                                        track.Res[0].Uri = track.Res[0].Uri.Replace(imported.Host, stored.Host);
                                        if (track.AlbumArtUri.Count > 0) {
                                            track.AlbumArtUri[0] = track.AlbumArtUri[0].Replace(imported.Host, stored.Host);
                                        }
                                        if (track.ArtworkUri.Count > 0) {
                                            track.ArtworkUri[0] = track.ArtworkUri[0].Replace(imported.Host, stored.Host);
                                        }
                                    }
                                }
                            }
                        }

                        // track nodes
                        TrackMetadata data = new TrackMetadata(track);
                        TreeNode item = new TreeNode(data.Title);
                        item.Tag = data;
                        item.ToolTipText = data.ToString();
                        item.ImageIndex = 2;
                        item.SelectedImageIndex = 2;
                        aPreset.Nodes.Add(item);
                    }
                }
            }
        }
    }

    private void AddNextEntry(XmlTextReader aReader, SortedDictionary<int, string> aDictionary) {
        int number = 0;
        string name = "";
        string endTag = aReader.Name;
        while (aReader.Read()) {
            if (aReader.Name == "linn:Name" && aReader.IsStartElement()) {
                aReader.Read();
                name = aReader.Value;
            }
            else if (aReader.Name == "linn:Number" && aReader.IsStartElement()) {
                aReader.Read();
                number = int.Parse(aReader.Value);
            }
            else if (aReader.Name == endTag && !aReader.IsStartElement()) { // end tag
                break;
            }
        }
        if (aDictionary.ContainsKey(number)) {
            aDictionary.Remove(number);
        }
        aDictionary.Add(number, name);
    }

    public delegate void DProgressChanged(int aPercent, string aMessage, Progress.State aProgressState);
    public delegate void DNodeCreated(TreeNode aNode);
    public static string kMiscNodeName = "Miscellaneous";
    private Thread iExportThread = null;
    private Thread iImportThread = null;
    private string iPresetDir = "";
    private string iManifestPath = "";
    private DProgressChanged iProgressChanged;
    private DNodeCreated iNodeCreated;
    private TreeNodeCollection iUserCreatedNodes = null;
    private int iPresetCount = 0;
    private bool iReplaceIpAddress = false;
    private bool iReplaceIpAddressMessage = false;
    private SortedDictionary<int, string> iPresets = new SortedDictionary<int, string>();
    private SortedDictionary<int, string> iBookmarks = new SortedDictionary<int, string>();
    private Playlist iPlaylist = null;
    private HelperKinskyJukebox iHelper;
}

}   // namespace KinskyJukebox

