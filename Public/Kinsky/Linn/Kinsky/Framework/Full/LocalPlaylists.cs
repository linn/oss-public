using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Linn;
using Upnp;

namespace Linn.Kinsky
{
    public delegate void LocalPlaylistFileNameGenerator(Playlist aPlaylist);
    public class LocalPlaylistSupport
    {
        public LocalPlaylistSupport(HelperKinsky aHelper, LocalPlaylistFileNameGenerator aNameGenerator)
        {
            // create the options page
            OptionPage optionPage = new OptionPage("Local Playlists");
            iOptionSaveDirectory = new OptionFolderPath("playlistpath", "Local playlist path", "Path where local playlists are saved", Path.Combine(aHelper.DataPath.FullName, "Playlists"));
            optionPage.Add(iOptionSaveDirectory);
            aHelper.AddOptionPage(optionPage);

            // listen for changes to the save directory option
            iOptionSaveDirectory.EventValueChanged += SaveDirectoryChanged;

            iCreator = new LocalPlaylistCreator(aNameGenerator);
            iRootContainer = new LocalPlaylistsRoot(iCreator, iOptionSaveDirectory);
        }

        public void Save(IList<upnpObject> aList)
        {
            // refresh the root container - if the playlist folder has not initialised, this will attempt
            // to reinitialise it
            iRootContainer.Refresh();

            iCreator.Create(iOptionSaveDirectory.Value, aList);
        }

        public string ContentDirectoryId
        {
            get { return iRootContainer.Metadata.Id; }
        }

        public IContainer ContentDirectoryRoot
        {
            get { return iRootContainer; }
        }

        private void SaveDirectoryChanged(object sender, EventArgs e)
        {
            iRootContainer.Refresh();
        }

        private OptionFolderPath iOptionSaveDirectory;
        private LocalPlaylistCreator iCreator;
        private LocalPlaylistsRoot iRootContainer;
    }

    internal class LocalPlaylistCreator
    {
        public LocalPlaylistCreator(LocalPlaylistFileNameGenerator aNameGenerator)
        {
            iNameGenerator = aNameGenerator;
        }

        public void Create(string aSaveDirectory, IList<upnpObject> aObjects)
        {
            if (aObjects.Count == 0)
                return;

            // generate a default file title for the new playlist
            DateTime now = DateTime.Now;
            string defaultTitle = String.Format("{0:00}", now.Year)
                                + String.Format("{0:00}", now.Month)
                                + String.Format("{0:00}", now.Day)
                                + "_"
                                + String.Format("{0:00}", now.Hour)
                                + String.Format("{0:00}", now.Minute)
                                + String.Format("{0:00}", now.Second);
			
			// create the playlist
            Playlist playlist = new Playlist(Path.Combine(aSaveDirectory, defaultTitle));
            int index = 0;
            foreach (upnpObject o in aObjects)
            {
                o.Restricted = false;
                playlist.Insert(index++, o);
            }

            iNameGenerator(playlist);
        }

        private LocalPlaylistFileNameGenerator iNameGenerator;
    }


    internal class LocalPlaylists : IDisposable
    {
        public LocalPlaylists(string aSaveDirectory)
        {
            iLock = new object();

            iSaveDirectory = aSaveDirectory;
            if (!Directory.Exists(iSaveDirectory))
            {
                Directory.CreateDirectory(iSaveDirectory);
            }

            // create the list of existing playlists in the save directory
            // - the only ways of changing this list is by
            //   a) changing the save directory, which causes a rescan
            //   b) creating, deleting or renaming an actual file - the file watcher
            //      will catch these
            // there is no explicit way of changing the list. This helps by simplifying
            // multi-threading issues
            iPlaylists = new SortedList<string, string>();
            Rescan();

            // create the file watcher
            iFileWatcher = new FileSystemWatcher();
            iFileWatcher.Path = iSaveDirectory;
            iFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            iFileWatcher.Filter = "*" + Playlist.kPlaylistExtension;

            iFileWatcher.Changed += FileWatcherChanged;
            iFileWatcher.Created += FileWatcherChanged;
            iFileWatcher.Deleted += FileWatcherChanged;
            iFileWatcher.Renamed += FileWatcherRenamed;
            iFileWatcher.Error += FileWatcherError;

            iFileWatcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            iFileWatcher.EnableRaisingEvents = false;

            iFileWatcher.Changed -= FileWatcherChanged;
            iFileWatcher.Created -= FileWatcherChanged;
            iFileWatcher.Deleted -= FileWatcherChanged;
            iFileWatcher.Renamed -= FileWatcherRenamed;
            iFileWatcher.Error -= FileWatcherError;
        }

        public string SaveDirectory
        {
            get { return iSaveDirectory; }
        }

        public uint Count
        {
            get
            {
                lock (iLock)
                {
                    return (uint)iPlaylists.Count;
                }
            }
        }

        public SortedList<string, string> GetPlaylists(uint aStartIndex, uint aCount)
        {
            lock (iLock)
            {
                IList<string> keys = iPlaylists.Keys;
                IList<string> vals = iPlaylists.Values;
                SortedList<string, string> list = new SortedList<string, string>();

                int startIndex = (int)aStartIndex;
                int count = (int)aCount;
                int endIndex = (startIndex + count < iPlaylists.Count) ? startIndex + count : iPlaylists.Count;
                for (int i = startIndex; i < endIndex; i++)
                {
                    list.Add(keys[i], vals[i]);
                }

                return list;
            }
        }

        private void Rescan()
        {
            iPlaylists.Clear();
            if (Directory.Exists(iSaveDirectory))
            {
                string[] files = Directory.GetFiles(iSaveDirectory, "*" + Playlist.kPlaylistExtension);
                foreach (string f in files)
                {
                    FileInfo i = new FileInfo(f);
                    iPlaylists.Add(i.FullName, i.Name.Replace(i.Extension, ""));
                }
            }
        }

        private void FileWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                // the file has been removed - remove the corresponding entry
                bool playlistRemoved = false;
                lock (iLock)
                {
                    if (iPlaylists.ContainsKey(e.FullPath))
                    {
                        iPlaylists.Remove(e.FullPath);
                        playlistRemoved = true;
                    }
                }

                // if the file was removed from the list, send event
                PlaylistRemoved ev = EventPlaylistRemoved;
                if (playlistRemoved && ev != null)
                {
                    ev(e.FullPath);
                }
            }
            else
            {
                // some unknown change has happened - rescan the save directory
                lock (iLock)
                {
                    Rescan();
                }

                // send the event
                PlaylistsChanged ev = EventPlaylistsChanged;
                if (ev != null)
                {
                    ev();
                }
            }
        }

        private void FileWatcherRenamed(object sender, RenamedEventArgs e)
        {
            Trace.WriteLine(Trace.kKinsky, "PlaylistWatcher.FileSystemWatcherRenamed: e.OldFullPath=" + e.OldFullPath + ", e.FullPath=" + e.FullPath);

            // a file has been renamed - rename the corresponding entry
            bool playlistRenamed = false;
            lock (iLock)
            {
                if (iPlaylists.ContainsKey(e.OldFullPath))
                {
                    iPlaylists.Remove(e.OldFullPath);
                    FileInfo i = new FileInfo(e.FullPath);
                    iPlaylists.Add(i.FullName, i.Name.Replace(i.Extension, ""));
                    playlistRenamed = true;
                }
            }

            if (playlistRenamed)
            {
                PlaylistRemoved evRem = EventPlaylistRemoved;
                if (evRem != null)
                {
                    evRem(e.OldFullPath);
                }
                PlaylistAdded evAdd = EventPlaylistAdded;
                if (evAdd != null)
                {
                    evAdd(e.FullPath);
                }
            }
        }

        private void FileWatcherError(object sender, ErrorEventArgs e)
        {
            Exception ex = e.GetException();
            UserLog.WriteLine(DateTime.Now + ": LocalPlaylists.FileWatcherError (" + ex + ") - file system changes will not be detected");
            Trace.WriteLine(Trace.kKinsky, ex);
        }

        public delegate void PlaylistAdded(string aFilename);
        public delegate void PlaylistRemoved(string aFilename);
        public delegate void PlaylistsChanged();
        public event PlaylistAdded EventPlaylistAdded;
        public event PlaylistRemoved EventPlaylistRemoved;
        public event PlaylistsChanged EventPlaylistsChanged;

        private object iLock;
        private string iSaveDirectory;
        private SortedList<string, string> iPlaylists;
        private FileSystemWatcher iFileWatcher;
    }


    // Top level local playlist container that wraps either a
    // LocalPlaylistRootOk or LocalPlaylistRootError container - these
    // wrapped containers are **not** children of this container
    internal class LocalPlaylistsRoot : IContainer
    {
        public LocalPlaylistsRoot(LocalPlaylistCreator aCreator, OptionFolderPath aOptionSaveDirectory)
        {
            iLock = new object();
            iCreator = aCreator;
            iOptionSaveDirectory = aOptionSaveDirectory;

            // initialise as being in its error state
            iPlaylists = null;
            iWrapped = new LocalPlaylistsRootError(kRootId);
            iWrapped.EventContentUpdated += ContentUpdated;
            iWrapped.EventContentAdded += ContentAdded;
            iWrapped.EventContentRemoved += ContentRemoved;

            Refresh();
        }

        public uint Open()
        {
            Refresh();

            return iWrapped.Open();
        }

        public void Refresh()
        {
            EventHandler<EventArgs> ev = null;

            lock (iLock)
            {
                if (iPlaylists == null)
                {
                    // current playlist folder has not been initialised - attempt initialisation again
                    InitialisePlaylistFolder();

                    // notify only if the playlist folder has been successfully initialised
                    if (iPlaylists != null)
                    {
                        ev = EventContentUpdated;
                    }
                }
                else if (iPlaylists.SaveDirectory != iOptionSaveDirectory.Value)
                {
                    // current playlist folder has changed
                    InitialisePlaylistFolder();

                    ev = EventContentUpdated;
                }
                else if (!Directory.Exists(iOptionSaveDirectory.Value))
                {
                    // current playlist folder is no longer available
                    InitialisePlaylistFolder();

                    ev = EventContentUpdated;
                }
            }

            if (ev != null)
            {
                ev(this, EventArgs.Empty);
            }
        }

        public IContainer ChildContainer(container aContainer)
        {
            return iWrapped.ChildContainer(aContainer);
        }

        public container Metadata
        {
            get { return iWrapped.Metadata; }
        }

        public bool HandleMove(DidlLite aDidlLite)
        {
            return iWrapped.HandleMove(aDidlLite);
        }

        public bool HandleInsert(DidlLite aDidlLite)
        {
            return iWrapped.HandleInsert(aDidlLite);
        }

        public void Insert(string aAfterId, DidlLite aDidlLite)
        {
            iWrapped.Insert(aAfterId, aDidlLite);
        }

        public bool HandleDelete(DidlLite aDidlLite)
        {
            return iWrapped.HandleDelete(aDidlLite);
        }

        public void Delete(string aId)
        {
            iWrapped.Delete(aId);
        }

        public bool HandleRename(upnpObject aObject)
        {
            return iWrapped.HandleRename(aObject);
        }

        public void Rename(string aId, string aTitle)
        {
            iWrapped.Rename(aId, aTitle);
        }

        public DidlLite Items(uint aStartIndex, uint aCount)
        {
            return iWrapped.Items(aStartIndex, aCount);
        }

        public DidlLite Search(string aSearchCriterea, uint aStartIndex, uint aCount)
        {
            return iWrapped.Search(aSearchCriterea, aStartIndex, aCount);
        }

        public event EventHandler<EventArgs> EventContentUpdated;
        public event EventHandler<EventArgs> EventContentAdded;
        public event EventHandler<EventArgsContentRemoved> EventContentRemoved;

        private void ContentUpdated(object sender, EventArgs e)
        {
            EventHandler<EventArgs> ev = EventContentUpdated;
            if (ev != null)
            {
                ev(this, e);
            }
        }

        private void ContentAdded(object sender, EventArgs e)
        {
            EventHandler<EventArgs> ev = EventContentAdded;
            if (ev != null)
            {
                ev(this, e);
            }
        }

        private void ContentRemoved(object sender, EventArgsContentRemoved e)
        {
            EventHandler<EventArgsContentRemoved> ev = EventContentRemoved;
            if (ev != null)
            {
                ev(this, e);
            }
        }

        private void InitialisePlaylistFolder()
        {
            if (iPlaylists != null)
            {
                iPlaylists.Dispose();
                iPlaylists = null;
            }

            try
            {
                iPlaylists = new LocalPlaylists(iOptionSaveDirectory.Value);
            }
            catch (Exception e)
            {
                UserLog.WriteLine(DateTime.Now + ": failed to initialise local playlist folder: " + e);
                iPlaylists = null;
            }

            iWrapped.EventContentUpdated -= ContentUpdated;
            iWrapped.EventContentAdded -= ContentAdded;
            iWrapped.EventContentRemoved -= ContentRemoved;

            if (iPlaylists != null)
            {
                iWrapped = new LocalPlaylistsRootOk(kRootId, iPlaylists, iCreator);
            }
            else
            {
                iWrapped = new LocalPlaylistsRootError(kRootId);
            }

            iWrapped.EventContentUpdated += ContentUpdated;
            iWrapped.EventContentAdded += ContentAdded;
            iWrapped.EventContentRemoved += ContentRemoved;
        }

        private const string kRootId = "Local Playlists";

        private object iLock;
        private IContainer iWrapped;
        private LocalPlaylists iPlaylists;
        private LocalPlaylistCreator iCreator;
        private OptionFolderPath iOptionSaveDirectory;
    }


    // Root container when an error has occurred with the playlist folder
    internal class LocalPlaylistsRootError : IContainer
    {
        public LocalPlaylistsRootError(string aRootId)
        {
            iMetadata = new playlistContainer();
            iMetadata.Id = aRootId;
            iMetadata.Title = aRootId;
            iMetadata.ChildCount = 1;
            iMetadata.Restricted = true;
            iMetadata.Searchable = true;
        }

        public uint Open()
        {
            return 1;
        }

        public void Refresh()
        {
        }

        public IContainer ChildContainer(container aContainer)
        {
            return null;
        }

        public container Metadata
        {
            get { return iMetadata; }
        }

        public bool HandleMove(DidlLite aDidlLite)
        {
            return false;
        }

        public bool HandleInsert(DidlLite aDidlLite)
        {
            return false;
        }

        public void Insert(string aAfterId, DidlLite aDidlLite)
        {
        }

        public bool HandleDelete(DidlLite aDidlLite)
        {
            return false;
        }

        public void Delete(string aId)
        {
        }

        public bool HandleRename(upnpObject aObject)
        {
            return false;
        }

        public void Rename(string aId, string aTitle)
        {
        }

        public DidlLite Items(uint aStartIndex, uint aCount)
        {
            item child = new item();
            child.Id = iMetadata.Id + "#1";
            child.ParentId = iMetadata.Id;
            child.Restricted = true;
            child.Title = "Error with Local Playlist folder";
            child.WriteStatus = "PROTECTED";

            DidlLite didl = new DidlLite();
            didl.Add(child);
            return didl;
        }

        public DidlLite Search(string aSearchCriterea, uint aStartIndex, uint aCount)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> EventContentUpdated { add { } remove { } }
        public event EventHandler<EventArgs> EventContentAdded { add { } remove { } }
        public event EventHandler<EventArgsContentRemoved> EventContentRemoved { add { } remove { } }

        private container iMetadata;
    }


    // Root container used when all is ok
    internal class LocalPlaylistsRootOk : IContainer
    {
        public LocalPlaylistsRootOk(string aRootId, LocalPlaylists aPlaylists, LocalPlaylistCreator aCreator)
        {
            iPlaylists = aPlaylists;
            iPlaylists.EventPlaylistAdded += PlaylistAdded;
            iPlaylists.EventPlaylistRemoved += PlaylistRemoved;
            iPlaylists.EventPlaylistsChanged += PlaylistsChanged;
            iCreator = aCreator;

            iMetadata = new playlistContainer();
            iMetadata.Id = aRootId;
            iMetadata.Title = aRootId;
            iMetadata.ChildCount = (int)iPlaylists.Count;
            iMetadata.Restricted = false;
            iMetadata.Searchable = true;
        }

        public uint Open()
        {
            return iPlaylists.Count;
        }

        public void Refresh()
        {
        }

        public IContainer ChildContainer(container aContainer)
        {
            return new ContainerPlaylist(aContainer);
        }

        public container Metadata
        {
            get { return iMetadata; }
        }

        public bool HandleMove(DidlLite aDidlLite)
        {
            return false;
        }

        public bool HandleInsert(DidlLite aDidlLite)
        {
            foreach (upnpObject i in aDidlLite)
            {
                if (i is playlistItem)
                {
                    return true;
                }
            }

            return false;
        }

        public void Insert(string aAfterId, DidlLite aDidlLite)
        {
            // create the playlist file on disk - let the filewatcher in the
            // LocalPlaylists class handle the rest
            try
            {
                iCreator.Create(iPlaylists.SaveDirectory, aDidlLite);
            }
            catch (Exception) { }
        }

        public bool HandleDelete(DidlLite aDidlLite)
        {
            return true;
        }

        public void Delete(string aId)
        {
            // the id of the item is simply the playlist filename - delete it
            // and let the filewatcher in the LocalPlaylists class handle the rest
            try
            {
                File.Delete(aId);
                UserLog.WriteLine(DateTime.Now + ": LocalPlaylistsRoot.Delete " + aId);
            }
            catch (Exception) { }
        }

        public bool HandleRename(upnpObject aObject)
        {
            return true;
        }

        public void Rename(string aId, string aTitle)
        {
            // the id of the item is the original filename of the playlist and aTitle
            // is the new file name (without extension)
            FileInfo oldFileInfo = new FileInfo(aId);
            string newFilename = Path.Combine(oldFileInfo.Directory.FullName, aTitle + Playlist.kPlaylistExtension);
            string oldFilename = aId;

            // rename the file and let the filewatcher in the LocalPlaylists class handle
            // the rest
            try
            {
                File.Move(oldFilename, newFilename);
                UserLog.WriteLine(DateTime.Now + ": LocalPlaylistsRoot.Rename " + oldFilename + " to " + newFilename);
            }
            catch (Exception) { }
        }

        public DidlLite Items(uint aStartIndex, uint aCount)
        {
            DidlLite didl = new DidlLite();

            SortedList<string, string> playlists = iPlaylists.GetPlaylists(aStartIndex, aCount);
            foreach (KeyValuePair<string, string> s in playlists)
            {
                playlistContainer playlist = new playlistContainer();
                resource resource = new resource();
                playlist.Res.Add(resource);

                playlist.Id = s.Key;
                playlist.ParentId = iMetadata.Id;
                playlist.Title = s.Value;
                playlist.WriteStatus = "PROTECTED";
                playlist.Restricted = false;
                playlist.Searchable = true;

                resource.Uri = s.Key;

                didl.Add(playlist);
            }

            return didl;
        }

        public DidlLite Search(string aSearchCriterea, uint aStartIndex, uint aCount)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> EventContentUpdated;
        public event EventHandler<EventArgs> EventContentAdded;
        public event EventHandler<EventArgsContentRemoved> EventContentRemoved;

        private void PlaylistAdded(string aFilename)
        {
            iMetadata.ChildCount = (int)iPlaylists.Count;

            if (EventContentAdded != null)
            {
                EventContentAdded(this, EventArgs.Empty);
            }
        }

        private void PlaylistRemoved(string aFilename)
        {
            iMetadata.ChildCount = (int)iPlaylists.Count;

            if (EventContentRemoved != null)
            {
                EventContentRemoved(this, new EventArgsContentRemoved(aFilename));
            }
        }

        private void PlaylistsChanged()
        {
            iMetadata.ChildCount = (int)iPlaylists.Count;

            if (EventContentUpdated != null)
            {
                EventContentUpdated(this, EventArgs.Empty);
            }
        }

        private LocalPlaylists iPlaylists;
        private LocalPlaylistCreator iCreator;
        private container iMetadata;
    }


    // container for a local playlist
    internal class ContainerPlaylist : IContainer
    {
        public ContainerPlaylist(container aContainer)
        {
            iMutex = new Mutex(false);
            iMetadata = aContainer;
            iPlaylist = new Playlist(aContainer.Id);
        }

        public uint Open()
        {
            iMutex.WaitOne();

            iPlaylist.Load();
            uint count = (uint)iPlaylist.Tracks.Count;
            
            iMutex.ReleaseMutex();

            return count;
        }

        public void Refresh() { }

        public IContainer ChildContainer(container aContainer)
        {
            throw new NotImplementedException();
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
			return true;
		}

        public bool HandleInsert(DidlLite aDidlLite)
        {
            return true;
        }

        public void Insert(string aAfterId, DidlLite aDidlLite)
        {
            iMutex.WaitOne();

            int index = 0;
            for (int i = 0; i < iPlaylist.Tracks.Count; ++i)
            {
                if (iPlaylist.Tracks[i].Id == aAfterId)
                {
                    index = i + 1;
                    break;
                }
            }

            foreach (upnpObject o in aDidlLite)
            {
                if (o is item)
                {
                    iPlaylist.Insert(index, o);
                    ++index;
                }
            }
            
            iPlaylist.Save();

            iMetadata.ChildCount = iPlaylist.Tracks.Count;

            iMutex.ReleaseMutex();

            if (EventContentAdded != null)
            {
                EventContentAdded(this, EventArgs.Empty);
            }
        }
		
		public bool HandleDelete(DidlLite aDidlLite)
		{
			return true;
		}

        public void Delete(string aId)
        {
            iMutex.WaitOne();
            foreach (upnpObject o in iPlaylist.Tracks)
            {
                if (o.Id == aId)
                {
                    iPlaylist.Remove(o);
                    iPlaylist.Save();

                    iMetadata.ChildCount = iPlaylist.Tracks.Count;

                    iMutex.ReleaseMutex();

                    if (EventContentRemoved != null)
                    {
                        EventContentRemoved(this, new EventArgsContentRemoved(aId));
                    }

                    return;
                }
            }
            iMutex.ReleaseMutex();
        }

        public bool HandleRename(upnpObject aObject)
        {
            return false;
        }

        public void Rename(string aId, string aTitle)
        {
            throw new NotSupportedException();
        }

        public DidlLite Items(uint aStartIndex, uint aCount)
        {
            iMutex.WaitOne();

            DidlLite didl = new DidlLite();

            uint endIndex = aStartIndex + aCount;
            for (uint i = aStartIndex; i < endIndex && i < iPlaylist.Tracks.Count; ++i)
            {
                didl.Add(iPlaylist.Tracks[(int)i]);
            }

            iMutex.ReleaseMutex();

            return didl;
        }

        public DidlLite Search(string aSearchCriterea, uint aStartIndex, uint aCount)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> EventContentUpdated { add { } remove { } }
        public event EventHandler<EventArgs> EventContentAdded;
        public event EventHandler<EventArgsContentRemoved> EventContentRemoved;

        private Mutex iMutex;

        private container iMetadata;
        
        private Playlist iPlaylist;
    }
}


