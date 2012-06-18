using System;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn.Topology;
using System.Threading;
using Linn.Kinsky;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using Linn.Control.Upnp.ControlPoint;

namespace Linn {
namespace KinskyPda {

public class ControllerMediaServer : IDisposable, IMessengerObserver
{
    public ControllerMediaServer(Node aRoot, ModelMediaServer aModelMediaServer) {
        iModelMediaServer = aModelMediaServer;
        iViewMediaServer = new ViewMediaServer(aRoot, aModelMediaServer);
        iKeyBindings = new KeyBindings();
        iDisposeMutex = new Mutex(false);
        iSearchClearTimer = new Timer();
        iSearchClearTimer.Interval = kKeyTimeOut;
        iSearchClearTimer.Elapsed += ClearSearchString;
        iSearchClearTimer.AutoReset = false;
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        iDisposeMutex.WaitOne();
        Messenger.Instance.EEventAppMessage -= Receive;
        if(iViewMediaServer != null) {
            iViewMediaServer.Dispose();
        }
        iDisposed = true;
        iDisposeMutex.ReleaseMutex();
    }
    
    public void Receive(Linn.Gui.Resources.Message aMessage) {
        iDisposeMutex.WaitOne();
        if(iDisposed) return;
        if(aMessage.Fullname == "Library.ContainerHighlight") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == true) {
                    iViewMediaServer.SetInsert(InsertAllowed() && !iInserting);
                }
            }
        }
        if(aMessage.Fullname == "StatusBar.Back.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    Back();
                }
            }
        }
        if(aMessage.Fullname == "Library.ContainerList") {
            MsgSelect selectMsg = aMessage as MsgSelect;
            if(selectMsg != null) {
                ListableUpnpObject obj = selectMsg.Listable as ListableUpnpObject;
                Assert.Check(obj != null);
                if(obj != null) {
                    Select(selectMsg.Index, obj);
                }
            }
            MsgHighlight highlightMsg = aMessage as MsgHighlight;
            if(highlightMsg != null && highlightMsg.Listable != null) {
                ListableUpnpObject obj = highlightMsg.Listable as ListableUpnpObject;
                Assert.Check(obj != null);
                iHighlightedItem = obj;
                iHighlightedIndex = highlightMsg.Index;
            }
            MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;
            if(highlightUpdatedMsg != null) {
                ListableUpnpObject obj = highlightUpdatedMsg.Listable as ListableUpnpObject;
                Assert.Check(obj != null);
                iHighlightedItem = obj;
                iViewMediaServer.SetInsert(InsertAllowed() && !iInserting);
            }
            MsgUnHighlight unHighlightMsg = aMessage as MsgUnHighlight;
            if(unHighlightMsg != null) {
                ListableUpnpObject obj = unHighlightMsg.Listable as ListableUpnpObject;
                Assert.Check(obj != null);
                iHighlightedItem = null;
                iHighlightedIndex = -1;
            }
        }
        if(aMessage.Fullname == "Library.Insert.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    Insert(false);
                }
            }
        }
        if(aMessage.Fullname == "") {
            MsgKeyDown keyDown = aMessage as MsgKeyDown;
            if(keyDown != null) {
                iSearchClearTimer.Stop();
                string action = iKeyBindings.Action(keyDown.Key);
                /*if(action == "Back") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("Library.BackButton.Monostable", true));
                    Renderer.Instance.Render();
                }*/
                if(action == "") {
                    if(keyDown.Key == Keys.Back) {
                        if(iSearchString.Length > 0) {
                            iSearchString = iSearchString.Remove(iSearchString.Length - 1, 1);
                            Trace.WriteLine(Trace.kMediaServer, "ControllerMediaServer.Receive: iSearchString=" + iSearchString);
                            FindEntryByString(iSearchString);
                        }
                    } else if((keyDown.Key & Keys.Alt) != Keys.Alt && (keyDown.Key & Keys.Control) != Keys.Control) {
                        iSearchString += (char)keyDown.Key;
                        Trace.WriteLine(Trace.kMediaServer, "ControllerMediaServer.Receive: iSearchString=" + iSearchString);
                        FindEntryByString(iSearchString);
                    }
                }
                iSearchClearTimer.Start();
            }
        }
        iDisposeMutex.ReleaseMutex();
    }
    
    public ModelSourceMediaRenderer ModelSourceMediaRenderer {
        get {
            return iModelSourceMediaRenderer;
        }
        set {
            iDisposeMutex.WaitOne();
            if(iDisposed) return;
            iModelSourceMediaRenderer = value;
            iViewMediaServer.SetInsert(InsertAllowed() && !iInserting);
            iDisposeMutex.ReleaseMutex();
        }
    }
    
    public ModelMediaServer ModelMediaServer {
        get {
            return iModelMediaServer;
        }
    }
    
    public bool InsertAllowed() {
	    if(iHighlightedItem != null) {
	        UpnpObject obj = iHighlightedItem.Object;
	        if((obj as PlaylistContainer != null ||                     // allow playlists
	            obj as MusicGenre != null ||                            // allow genre containers
	            obj as MusicArtist != null ||                           // allow artist containers
	            obj as MusicAlbum != null ||                            // allow album containers
	            obj as StorageSystem != null ||                         // allow storage system
	            obj as StorageFolder != null ||                         // allow storage folders
	            (obj as Item != null && obj.Res.Count > 0)) &&          // allow all items with a resource
	            iModelSourceMediaRenderer != null) {                    // only if we are subscribed to a media renderer        
	            Trace.WriteLine(Trace.kMediaServer, "ControllerMediaServer.SetInsert: insert allowed (" + obj + ")");
	            return true;
	        } else {
	            Trace.WriteLine(Trace.kMediaServer, "ControllerMediaServer.SetInsert: insert NOT allowed (" + obj + ")");
	            return false;
	        }
	    }
	    return false;
	}
    
    private void EventDirectory() {
	    Trace.WriteLine(Trace.kMediaServer, ">ControllerMediaServer.EventDirectory");
	    if(iModelMediaServer.DirInfo != null) {
	        iHighlightedIndex = iModelMediaServer.DirInfo.Index;
	        if(iHighlightedIndex != -1) {
	            iHighlightedItem = iModelMediaServer.ListEntryProvider.Entries((uint)iHighlightedIndex, 1)[0] as ListableUpnpObject;
	            Assert.Check(iHighlightedItem != null);
	            iViewMediaServer.SetInsert(InsertAllowed() && !iInserting);
	        }
	    }
	    Trace.WriteLine(Trace.kMediaServer, "<ControllerMediaServer.EventDirectory");
	}
    
    private void Select(int aIndex, ListableUpnpObject aObject) {
        // NOTE: can this be moved into ListableUpnpObject.Select()???
        if(aObject.Object as Linn.Topology.Container != null) {   // only go down a directory if object is a container
            iModelMediaServer.DownDirectory(aIndex, aObject);
        }
    }
    
    private void Back() {
        iModelMediaServer.UpDirectory();
    }
    
    private void ClearSearchString(object aSender, EventArgs aArgs) {
        iSearchString = "";
    }
    
    private void FindEntryByString(string aSearchString) {
        Trace.WriteLine(Trace.kMediaServer, ">ControllerMediaServer.FindEntryByString");
        UpnpObject obj = null;
        DirInfo info = iModelMediaServer.DirInfo;
        if(info.Object != null) {
            obj = info.Object.Object;
        }
        if((obj as MusicAlbum != null) || (obj as PlaylistContainer != null) || (obj as StorageFolder != null)) {
            // if our parent is a music album, a playlist container or a storage folder we dont allow quick key search
            return; 
        }
        uint count = iModelMediaServer.ListEntryProviderL2.Count;
        if(count > 0) {
            // get first stab at index for letter
            uint index = (uint)Math.Round((float)count / 2.0f);
            int result = FindEntryByString(aSearchString, index, 0, count);
            Trace.WriteLine(Trace.kMediaServer, "ControllerMediaServer.FindEntryByString: result=" + result);
            if(result != -1) {
                Messenger.Instance.PresentationMessage(new MsgSetHighlight("Library.ContainerList", result, NodeList.EAlignment.EA_Centre));
                iViewMediaServer.SetInsert(InsertAllowed() && !iInserting);
            }
        }
    }
    
    private int FindEntryByString(string aSearchString, uint aIndex, uint aRangeMin, uint aRangeMax) {
        Trace.WriteLine(Trace.kMediaServer, "ControllerMediaServer.FindEntryByString: aIndex=" + aIndex + ", aRangeMin=" + aRangeMin + ", aRangeMax=" + aRangeMax);
        IListable listable = iModelMediaServer.ListEntryProviderL2.Entries(aIndex, 1)[0];
        ListableUpnpObject obj = listable as ListableUpnpObject;
        if(obj != null && obj.Object.Title.Length > 0) {
            if(string.Compare(obj.Object.Title, 0, aSearchString, 0, aSearchString.Length, true) < 0) {
                uint index = (uint)Math.Round(((float)(aRangeMax - aIndex - 0.5f) / 2.0f) + aIndex);
                Trace.WriteLine(Trace.kMediaServer, "ControllerMediaServer.FindEntryByString(<): index=" + index);
                if(index == aIndex) {   // we can't find an item starting with the required letter, return first after desired letter
                    return (int)aIndex;
                }
                return FindEntryByString(aSearchString, index, aIndex + 1, aRangeMax);
            } else {
                if((string.Compare(obj.Object.Title, 0, aSearchString, 0, aSearchString.Length, true) == 0 && aIndex == aRangeMin) || (aRangeMax - aRangeMin == 0)) {
                    return (int)aIndex;
                }
                uint index = (uint)Math.Round(((float)(aIndex - aRangeMin - 0.5f) / 2.0f) + aRangeMin);
                Trace.WriteLine(Trace.kMediaServer, "ControllerMediaServer.FindEntryByString(=>): index=" + index);
                if(index == aIndex) {   // we can't find an item starting with the required letter, return first after desired letter
                    return (int)aIndex;
                }
                return FindEntryByString(aSearchString, index, aRangeMin, (string.Compare(obj.Object.Title, 0, aSearchString, 0, aSearchString.Length, true) == 0) ? aIndex + 1 : aIndex);
            }
        }
        return -1;
    }
    
    private void Insert(bool aPlayAfterInsert) {
        /*if(iInsertWorker == null) {
            iPlayAfterInsert = aPlayAfterInsert;
            ModelSourceMediaRendererLinn mediaRendererLinn = iModelSourceMediaRenderer as ModelSourceMediaRendererLinn;
            ModelSourceMediaRendererUpnpAv mediaRenderer = iModelSourceMediaRenderer as ModelSourceMediaRendererUpnpAv;
            if(mediaRendererLinn != null) {
                iInsertIndex = mediaRendererLinn.ModelPlaylist.TrackCount;
            } else if(mediaRenderer != null) {
                iInsertIndex = mediaRenderer.ModelPlaylist.TrackCount;
            }
            iInsertWorker = new BackgroundWorker();
            iInsertWorker.DoWork += DoInsertWork;
            iInsertWorker.RunWorkerCompleted += RunWorkerCompleted;
            iInsertWorker.RunWorkerAsync();
        } else {
            Trace.WriteLine(Trace.kMediaServer, "ControllerMediaServer.Insert: Command ignored - already inserting!");
        }*/
    }
    
    /*private void DoInsertWork(object sender, DoWorkEventArgs args) {
        ModelSourceMediaRendererLinn mediaRendererLinn = iModelSourceMediaRenderer as ModelSourceMediaRendererLinn;
        ModelSourceMediaRendererUpnpAv mediaRenderer = iModelSourceMediaRenderer as ModelSourceMediaRendererUpnpAv;
        uint maxTracksAllowed = 1000;
        if(mediaRendererLinn != null) {
            maxTracksAllowed = mediaRendererLinn.ModelPlaylist.MaxTracksAllowed;
        }
        List<Item> items = iModelMediaServer.RetrieveItems(iHighlightedItem.Object, maxTracksAllowed, iModelSourceMediaRenderer.ProtocolInfo);
        Trace.WriteLine(Trace.kMediaServer, "ControllerMediaServer.DoInsertWork: items.Count=" + items.Count);
        if(items.Count > 0) {
            try {
                if(mediaRendererLinn != null) {
                    mediaRendererLinn.ModelPlaylist.Insert(iInsertIndex, items);
	            } else if(mediaRenderer != null) {
	                mediaRenderer.ModelPlaylist.Insert(iInsertIndex, items);
	            }
                if(iPlayAfterInsert) {
                    iModelSourceMediaRenderer.SeekTracks((int)iInsertIndex, "Absolute");
                }
            } catch(ControlRequest.InvokeError e) {
                System.Console.WriteLine("ControllerMediaServer.DoInsertWork:\n" + e);
            } catch(ControlRequest.InvokeFailed e) {
                System.Console.WriteLine("ControllerMediaServer.DoInsertWork:\n" + e);
            } catch(Exception e) {
                System.Console.WriteLine("ControllerMediaServer.DoInsertWork:\n" + e);
                throw e;
            }
        }
    }*/
    
    /*private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
        Trace.WriteLine(Trace.kMediaServer, ">ControllerMediaServer.RunWorkerCompleted");
        if(e.Error != null) {
            System.Console.WriteLine("ControllerMediaServer.RunWorkerCompleted:\n" + e.Error.ToString());
            throw new System.Exception(e.Error.ToString());
        }
        iInsertWorker = null;
    }*/
    
    private Mutex iDisposeMutex = null;
    private bool iDisposed = false;
    private ModelMediaServer iModelMediaServer = null;
    private ViewMediaServer iViewMediaServer = null;
    private KeyBindings iKeyBindings = null;
    private Timer iSearchClearTimer = null;
    private string iSearchString = "";
    private const int kKeyTimeOut = 1000;
    private ListableUpnpObject iHighlightedItem = null;
    private int iHighlightedIndex = -1;
    private bool iInserting = false;
    private ModelSourceMediaRenderer iModelSourceMediaRenderer = null;
    //private BackgroundWorker iInsertWorker = null;
    private bool iPlayAfterInsert = false;
    private uint iInsertIndex = 0;
}

} // KinskyPda
} // Linn
