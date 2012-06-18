using Linn.Gui.Scenegraph;
using System;
using Linn.Gui.Resources;
using Linn.Gui;
using System.Collections.Generic;
using System.Windows.Forms;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ControllerMediaServer : ControllerNodeList
{ 
    public ControllerMediaServer(Node aRoot, Library aLibrary) : base(aRoot, "Library.DirectoryList") {
        iModel = aLibrary;
        
        iSearchClearTimer = new Timer();
        iSearchClearTimer.Interval = kKeyTimeOut;
        iSearchClearTimer.Elapsed += ClearSearchString;
        iSearchClearTimer.AutoReset = false;
        
        iView = new ViewMediaServer(aRoot, this, aLibrary);
        Messenger.Instance.EEventAppMessage += Receive;
        iModel.EEventDirectory += EventDirectory;
        SetFocus(iList.Focused);
    }
    
    public override void Dispose() {
        base.Dispose();
        Messenger.Instance.EEventAppMessage -= Receive;
        iModel.EEventDirectory -= EventDirectory;
        if(iView != null) {
            iView.Dispose();
            //iView = null;
        }
    }
    
    public Library Model {
        get {
            return iModel;
        }
    }
    
    public override void Receive(Linn.Gui.Resources.Message aMessage) {
        base.Receive(aMessage);
        if(aMessage.Fullname == "Library.Root") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == true) {
                    iView.Enable();
                    iView.SetInsert(InsertAllowed() && !iInserting);
                }
            }
        }
        if(aMessage.Fullname == "Library.DirectoryHighlight") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == true) {
                    iView.SetInsert(InsertAllowed() && !iInserting);
                }
            }
        }
        if(aMessage.Fullname == "Library.BackButton.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    Messenger.Instance.PresentationMessage(new MsgSetFocus("Library.DirectoryList", true));
                    iModel.UpDirectory();
                }
            }
        }
        if(aMessage.Fullname == "Library.DirectoryList") {
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
                iView.Highlight(highlightMsg.Index, obj);
            }
            MsgHighlightUpdated highlightUpdatedMsg = aMessage as MsgHighlightUpdated;
            if(highlightUpdatedMsg != null) {
                ListableUpnpObject obj = highlightUpdatedMsg.Listable as ListableUpnpObject;
                Assert.Check(obj != null);
                iHighlightedItem = obj;
                iView.HighlightUpdated(highlightUpdatedMsg.Index, obj);
                iView.SetInsert(InsertAllowed() && !iInserting);
            }
            MsgUnHighlight unHighlightMsg = aMessage as MsgUnHighlight;
            if(unHighlightMsg != null) {
                ListableUpnpObject obj = unHighlightMsg.Listable as ListableUpnpObject;
                Assert.Check(obj != null);
                iHighlightedItem = null;
                iHighlightedIndex = -1;
                iView.UnHighlight(obj);
            }
            MsgTopEntry topEntry = aMessage as MsgTopEntry;
            if(topEntry != null) {
                iView.SetScrollbar(topEntry.Index);
            }
        }
        if(aMessage.Fullname == "Library.Highlight") {
            MsgActiveChanged msgActiveChanged = aMessage as MsgActiveChanged;
            if(msgActiveChanged != null) {
                if(msgActiveChanged.NewActive == true) {
                    iView.SetInsert(InsertAllowed());
                }
            }
        }
        if(aMessage.Fullname == "Library.DirectoryList") {
            MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;
            if(focusMsg != null) {
                SetFocus(focusMsg.NewFocus);
            }
        }
        if(aMessage.Fullname == "CurrentPlaylist.Playlist") {
            MsgFocusChanged focusMsg = aMessage as MsgFocusChanged;
            if(focusMsg != null) {
                if(focusMsg.NewFocus) {
                    Messenger.Instance.PresentationMessage(new MsgSetFocus("Library.DirectoryList", false));
                }
            }
        }
        if(aMessage.Fullname == "Library.DirectoryScrollbar") {
            SetFocus(true);
            MsgPositionChanged positionMsg = aMessage as MsgPositionChanged;
            if(positionMsg != null) {
                iView.SetTopEntry(positionMsg.NewPosition);
            }
        }
        if(aMessage.Fullname == "") {
            MsgKeyDown keyDown = aMessage as MsgKeyDown;
            if(keyDown != null) {
                if(iHasFocus) {
                    iSearchClearTimer.Stop();
                    string action = iKeyBindings.Action(keyDown.Key);
                    if(action == "Back") {
                        Messenger.Instance.PresentationMessage(new MsgSetState("Library.BackButton.Monostable", true));
                        Renderer.Instance.Render();
                    }
                    if(action == "") {
                        if(keyDown.Key == Keys.Back) {
                            if(iSearchString.Length > 0) {
                                iSearchString = iSearchString.Remove(iSearchString.Length - 1, 1);
                                Trace.WriteLine(Trace.kLinnGuiMediaServer, "ControllerMediaServer.Receive: iSearchString=" + iSearchString);
                                FindEntryByString(iSearchString);
                            }
                        } else if((keyDown.Key & Keys.Alt) != Keys.Alt && (keyDown.Key & Keys.Control) != Keys.Control) {
                            iSearchString += (char)keyDown.Key;
                            Trace.WriteLine(Trace.kLinnGuiMediaServer, "ControllerMediaServer.Receive: iSearchString=" + iSearchString);
                            FindEntryByString(iSearchString);
                        }
                    }
                    iSearchClearTimer.Start();
                }
            }
            MsgKeyUp keyUp = aMessage as MsgKeyUp;
            if(keyUp != null) {
                string action = iKeyBindings.Action(keyUp.Key);
                if(action == "Back" && iHasFocus) {
                    Messenger.Instance.PresentationMessage(new MsgSetState("Library.BackButton.Monostable", false));
                    Renderer.Instance.Render();
                }
            }
        }
    }
    
    public void SetFocus(bool aFocus) {
        iHasFocus = aFocus;
        iView.SetFocus(aFocus);
    }
    
    public void SetInserting(bool aInserting) {
        iInserting = aInserting;
        iView.SetInsert(InsertAllowed() && !iInserting);
        Renderer.Instance.Render();
    }
    
    public List<Item> RetrieveItems(uint aNumItems, string aProtocolInfo) {
        if(InsertAllowed()) {
            return iModel.RetrieveItems(iHighlightedItem.Object, aNumItems, aProtocolInfo);
        }
        return new List<Item>();
    }
    
    private void Select(int aIndex, ListableUpnpObject aObject) {
        // NOTE: can this be moved into ListableUpnpObject.Select()???
        if(aObject.Object as Container != null) {   // only go down a directory if object is a container
            iModel.DownDirectory(aIndex, aObject);
            iHighlightedItem = null;
            iHighlightedIndex = -1;
        }
    }
    
    private void EventDirectory() {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, ">ControllerMediaServer.EventDirectory");
        if(iModel.DirInfo != null) {
            iHighlightedIndex = iModel.DirInfo.Index;
            if(iHighlightedIndex != -1) {
                iHighlightedItem = iModel.ListEntryProvider.Entry((uint)iHighlightedIndex) as ListableUpnpObject;
                Assert.Check(iHighlightedItem != null);
                iView.SetInsert(InsertAllowed() && !iInserting);
            }
        }
        Trace.WriteLine(Trace.kLinnGuiMediaServer, "<ControllerMediaServer.EventDirectory");
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
                (obj as Item != null && obj.Res.Count > 0))) {          // allow all items with a resource
                Trace.WriteLine(Trace.kLinnGuiMediaServer, "ControllerMediaServer.SetInsert: insert allowed (" + obj + ")");
                return true;
            } else {
                Trace.WriteLine(Trace.kLinnGuiMediaServer, "ControllerMediaServer.SetInsert: insert NOT allowed (" + obj + ")");
                return false;
            }
        }
        return false;
    }
    
    private void ClearSearchString(object aSender, EventArgs aArgs) {
        iSearchString = "";
    }
    
    private void FindEntryByString(string aSearchString) {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, ">ControllerMediaServer.FindEntryByString");
        UpnpObject obj = null;
        DirInfo info = iModel.DirInfo;
        if(info.Object != null) {
            obj = info.Object.Object;
        }
        if((obj as MusicAlbum != null) || (obj as PlaylistContainer != null) || (obj as StorageFolder != null)) {
            // if our parent is a music album, a playlist container or a storage folder we dont allow quick key search
            return; 
        }
        uint count = iModel.ListEntryProviderL2.Count;
        if(count > 0) {
            // get first stab at index for letter
            uint index = (uint)Math.Round((float)count / 2.0f);
            int result = FindEntryByString(aSearchString, index, 0, count);
            Trace.WriteLine(Trace.kLinnGuiMediaServer, "ControllerMediaServer.FindEntryByString: result=" + result);
            if(result != -1 && result != iHighlightedIndex) {
                Messenger.Instance.PresentationMessage(new MsgSetHighlight("Library.DirectoryList", result, NodeList.EAlignment.EA_Centre));
                iHighlightedIndex = result;
                iHighlightedItem = iModel.ListEntryProviderL2.Entry((uint)iHighlightedIndex) as ListableUpnpObject;
                Assert.Check(iHighlightedItem != null);
                iView.HighlightUpdated(iHighlightedIndex, iHighlightedItem);
                iView.SetInsert(InsertAllowed() && !iInserting);
            }
        }
    }
    
    private int FindEntryByString(string aSearchString, uint aIndex, uint aRangeMin, uint aRangeMax) {
        Trace.WriteLine(Trace.kLinnGuiMediaServer, "ControllerMediaServer.FindEntryByString: aIndex=" + aIndex + ", aRangeMin=" + aRangeMin + ", aRangeMax=" + aRangeMax);
        IListable listable = iModel.ListEntryProviderL2.Entry(aIndex);
        ListableUpnpObject obj = listable as ListableUpnpObject;
        if(obj != null && obj.Object.Title.Length > 0) {
            if(string.Compare(obj.Object.Title, 0, aSearchString, 0, aSearchString.Length, true) < 0) {
                uint index = (uint)Math.Round(((float)(aRangeMax - aIndex - 0.5f) / 2.0f) + aIndex);
                Trace.WriteLine(Trace.kLinnGuiMediaServer, "ControllerMediaServer.FindEntryByString(<): index=" + index);
                if(index == aIndex) {   // we can't find an item starting with the required letter, return first after desired letter
                    return (int)aIndex;
                }
                return FindEntryByString(aSearchString, index, aIndex + 1, aRangeMax);
            } else {
                if((string.Compare(obj.Object.Title, 0, aSearchString, 0, aSearchString.Length, true) == 0 && aIndex == aRangeMin) || (aRangeMax - aRangeMin == 0)) {
                    return (int)aIndex;
                }
                uint index = (uint)Math.Round(((float)(aIndex - aRangeMin - 0.5f) / 2.0f) + aRangeMin);
                Trace.WriteLine(Trace.kLinnGuiMediaServer, "ControllerMediaServer.FindEntryByString(=>): index=" + index);
                if(index == aIndex) {   // we can't find an item starting with the required letter, return first after desired letter
                    return (int)aIndex;
                }
                return FindEntryByString(aSearchString, index, aRangeMin, (string.Compare(obj.Object.Title, 0, aSearchString, 0, aSearchString.Length, true) == 0) ? aIndex + 1 : aIndex);
            }
        }
        return -1;
    }
    
    private ViewMediaServer iView;
    private Library iModel;
    private ListableUpnpObject iHighlightedItem;
    private int iHighlightedIndex = -1;
    private bool iInserting = false;
    private bool iHasFocus = false;
    private Timer iSearchClearTimer;
    private string iSearchString = "";
    private const int kKeyTimeOut = 1000;
}
    
} // Kinsky
} // Linn
