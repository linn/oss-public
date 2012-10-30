using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.Net;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

namespace Linn.ProductSupport
{
    public class Tree
    {
        internal EventHandler<EventArgsRoom> EventRoomAdded;
        internal EventHandler<EventArgsRoom> EventRoomRemoved;

        public Tree(Helper aHelper) {
            iHelper = aHelper;
            // Check for updates
            iUpdateCheck = new UpdateCheck();
            iUpdateCheck.EventUpdateCheckComplete += UpdateCheckComplete;
            iUpdateCheck.EventUpdateCheckError += UpdateCheckError;
            iUpdateCheck.Start();
        }

        internal Box AddFallbackBox(BasicSetup aBasicSetup, Device aDevice, EventServerUpnp aEventServer, string aProductId, string[] aBoardId, string[] aBoardType, string[] aBoardDescription, string[] aBoardNumber, string aSoftwareVersion) {
            string model = aDevice.Model;
            if (model != null && model != "") {
                // Originally fallback model was 'Reprogram-Device', pull the device from this
                if (model.Contains("Reprogram-")) {
                    string[] list = model.Split('-');
                    if (list.Length > 1) {
                        model = list[1];
                    }
                }
            }
            if (model == null || model == "") {
                model = "Unknown";
            }
            return AddBox(aBasicSetup, model, "Unknown", "Reprogram", aDevice, aEventServer, Box.EState.eFallback, aProductId, aBoardId, aBoardType, aBoardDescription, aBoardNumber, aSoftwareVersion);
        }

        internal Box AddMainBox(BasicSetup aBasicSetup, Playback aPlayback, ServiceProduct aServiceProduct, EventServerUpnp aEventServer, string aProductId, string[] aBoardId, string[] aBoardType, string[] aBoardDescription, string[] aBoardNumber, string aSoftwareVersion) {
            return AddBox(aBasicSetup, aPlayback, aServiceProduct.ModelName, aServiceProduct.ProductRoom, aServiceProduct.ProductName, aServiceProduct.Device, aEventServer, Box.EState.eOn, aProductId, aBoardId, aBoardType, aBoardDescription, aBoardNumber, aServiceProduct.ProductImageUri, aSoftwareVersion, false);
        }

        internal Box AddProxyBox(ServiceProduct aServiceProduct, EventServerUpnp aEventServer, string[] aBoardType, string[] aBoardDescription, string[] aBoardNumber, string aSoftwareVersion) {
            return AddBox(aServiceProduct.ModelName, aServiceProduct.ProductRoom, aServiceProduct.ProductName, aServiceProduct.Device, aEventServer, Box.EState.eOn, aBoardType, aBoardDescription, aBoardNumber, aServiceProduct.ProductImageUri, aSoftwareVersion);
        }

        internal void RemoveBox(Box box) {
            Lock();
            Room room = iRoomList[box.Room];
            if (room.RemoveBox(box)) {
                iRoomList.Remove(box.Room);
                Unlock();
                if (EventRoomRemoved != null) {
                    EventRoomRemoved(this, new EventArgsRoom(room));
                }
            }
            else {
                Unlock();
            }
        }

        private Box AddBox(BasicSetup aBasicSetup, string aModel, string aRoom, string aName, Device aDevice, EventServerUpnp aEventServer, Box.EState aState, string aProductId, string[] aBoardId, string[] aBoardType, string[] aBoardDescription, string[] aBoardNumber, string aSoftwareVersion) {
            // fallback device
            return AddBox(aBasicSetup, null, aModel, aRoom, aName, aDevice, aEventServer, aState, aProductId, aBoardId, aBoardType, aBoardDescription, aBoardNumber, null, aSoftwareVersion, false);
        }

        private Box AddBox(string aModel, string aRoom, string aName, Device aDevice, EventServerUpnp aEventServer, Box.EState aState, string[] aBoardType, string[] aBoardDescription, string[] aBoardNumber, string aImageUri, string aSoftwareVersion) {
            // proxy device
            return AddBox(null, null, aModel, aRoom, aName, aDevice, aEventServer, aState, null, null, aBoardType, aBoardDescription, aBoardNumber, aImageUri, aSoftwareVersion, true);
        }

        private Box AddBox(BasicSetup aBasicSetup, Playback aPlayback, string aModel, string aRoom, string aName, Device aDevice, EventServerUpnp aEventServer, Box.EState aState, string aProductId, string[] aBoardId, string[] aBoardType, string[] aBoardDescription, string[] aBoardNumber, string aImageUri, string aSoftwareVersion, bool aIsProxy) {
            Room room;
            Box box;

            Lock();

            if (iRoomList.TryGetValue(aRoom, out room)) {
                Unlock();
                box = new Box(iHelper, iUpdateCheck, aBasicSetup, aPlayback, aModel, room, aName, aDevice, aEventServer, aState, aProductId, aBoardId, aBoardType, aBoardDescription, aBoardNumber, aImageUri, aSoftwareVersion, aIsProxy);
            }
            else {
                room = new Room(aRoom);
                iRoomList[aRoom] = room;
                Unlock();

                if (EventRoomAdded != null) {
                    EventRoomAdded(this, new EventArgsRoom(room));
                }

                box = new Box(iHelper, iUpdateCheck, aBasicSetup, aPlayback, aModel, room, aName, aDevice, aEventServer, aState, aProductId, aBoardId, aBoardType, aBoardDescription, aBoardNumber, aImageUri, aSoftwareVersion, aIsProxy);
            }

            room.AddBox(box);

            return box;
        }

        internal void CheckForUpdates() {
            iUpdateCheck.Start();
        }

        internal bool UpdateCheckInProgress {
            get {
                return iUpdateCheck.InProgress;
            }
        }

        internal bool UpdateCheckFailed {
            get {
                return iUpdateCheck.Failed;
            }
        }

        private void UpdateCheckComplete(object obj, EventArgs e) {
            UserLog.WriteLine("Update Check Complete");
        }

        private void UpdateCheckError(object obj, EventArgsUpdateError e) {
            UserLog.WriteLine("Update Check Error: " + e.ErrorMessage);
        }

        private void Lock() {
            iMutex.WaitOne();
        }

        private void Unlock() {
            iMutex.ReleaseMutex();
        }

        public override string ToString() {
            Lock();
            string title = "Room Count: " + iRoomList.Count + ", Box Count: ";
            string roomsPlusBoxes = "";
            int boxCount = 0;
            foreach (KeyValuePair<string, Room> kvp in iRoomList) {
                roomsPlusBoxes += kvp.Value.ToString() + Environment.NewLine;
                boxCount += ((Room)kvp.Value).BoxCount;
            }
            title += boxCount + Environment.NewLine + Environment.NewLine;
            Unlock();

            return (title + roomsPlusBoxes);
        }

        private Mutex iMutex = new Mutex();
        private SortedList<string, Room> iRoomList = new SortedList<string, Room>();
        private UpdateCheck iUpdateCheck;
        private Helper iHelper;
    }

    public class Room
    {
        public event EventHandler<EventArgsBox> EventBoxAdded;
        public event EventHandler<EventArgsBox> EventBoxRemoved;

        internal Room(string aName) {
            iName = aName;
        }
        
        public string Name {
            get {
                return iName;
            }
        }

        public int BoxCount {
            get {
                return iBoxList.Count;
            }
        }

        internal void AddBox(Box box) {
            Lock();

            iBoxList[box.MacAddress] = box;

            Unlock();

            if (EventBoxAdded != null) {
                EventBoxAdded(this, new EventArgsBox(box));
            }
        }

        //return true if all boxes have been removed from the room, else returns false
        internal bool RemoveBox(Box box) {
            Lock();

            iBoxList.Remove(box.MacAddress);
            bool allItemsRemoved = (iBoxList.Count == 0);

            Unlock();

            if (EventBoxRemoved != null) {
                EventBoxRemoved(this, new EventArgsBox(box));
            }

            return allItemsRemoved;
        }

        public override string ToString() {
            string roomPlusBoxes = iName + Environment.NewLine + "-------------------------------" + Environment.NewLine;
            Lock();
            foreach (KeyValuePair<string, Box> kvp in iBoxList) {
                roomPlusBoxes += kvp.Value.ToString() + Environment.NewLine;
            }
            Unlock();
            return roomPlusBoxes;
        }

        private void Lock() {
            iMutex.WaitOne();
        }

        private void Unlock() {
            iMutex.ReleaseMutex();
        }

        private Mutex iMutex = new Mutex();
        private SortedList<string, Box> iBoxList = new SortedList<string, Box>();
        private string iName;
    }

    public class Box
    {
        public event EventHandler<EventArgsBox> EventBoxChanged;

        public enum EState
        {
            eOn,
            eFallback,
            eOff
        };

        public static string kRs232Connected = "RS232 Controlled";
        public static string kRs232Disconnected = "RS232 connection not active";

        internal Box(Helper aHelper, UpdateCheck aUpdateCheck, BasicSetup aBasicSetup, Playback aPlayback, string aModel, Room aRoom, string aName, Device aDevice, EventServerUpnp aEventServer, EState aState, string aProductId, string[] aBoardId, string[] aBoardType, string[] aBoardDescription, string[] aBoardNumber, string aImageUri, string aSoftwareVersion, bool aIsProxy) {
            iHelper = aHelper;
            iModel = aModel;
            iRoom = aRoom;
            iName = aName;
            iDevice = aDevice;
            iEventServer = aEventServer;
            iState = aState;
            iImageUri = aImageUri;
            iSoftwareVersion = aSoftwareVersion;
            iUpdateCheck = aUpdateCheck;
            iBasicSetup = aBasicSetup;
            iUpdateFirmware = new UpdateFirmware(aHelper);
            iPlayback = aPlayback;
            iUpdateCheck.GetInfo(aModel, aIsProxy, aSoftwareVersion, aBoardNumber, out iSoftwareUpdateAvailable, out iSoftwareUpdateVersion, out iSoftwareUpdateUrl, out iSoftwareUpdateVariant, out iReleaseNotesHtml);
            iUpdateCheck.EventUpdateCheckComplete += UpdateCheckComplete;
            iUpdateCheck.EventUpdateCheckError += UpdateCheckError;
            iSysLogPretty = new SysLogPretty(aHelper.ExePath.FullName);
            iProductId = aProductId;
            iIsProxy = aIsProxy;
            if (aBoardId != null) {
                iBoardId = aBoardId;
            }
            if (aBoardType != null) {
                iBoardType = aBoardType;
            }
            if (aBoardDescription != null) {
                iBoardDescription = aBoardDescription;
            }
            if (aBoardNumber != null) {
                iBoardNumber = aBoardNumber;
            }
        }

        public EState State {
            get {
                return iState;
            }
        }

        public string StatusText {
            get {
                if (State == EState.eFallback) {
                    return "Fallback";
                }
                else if (State == EState.eOn) {
                    return "On";
                }
                else {
                    return "Off";
                }
            }
        }

        public IPAddress NetworkInterfaceIpAddress()
        {
            return (iEventServer.Interface);
        }


        public void Reboot() {
            if (BasicSetup != null) {
                BasicSetup.Reboot();
            }
        }

        public void SetStandby(bool aStandby) {
            if (Playback != null) {
                Playback.SetStandby(aStandby);
            }
        }

        public string Name {
            get {
                return iName;
            }
        }

        public string Model {
            get {
                if (iModel == null || iModel == "") {
                    //if device found is auskerry/bute it will not have a model property, use device xml model
                    iModel = iDevice.Model;
                    if (iModel == null || iModel == "") {
                        iModel = "Unknown";
                    }
                }

                return iModel;
            }
        }

        public string Room {
            get {
                return iRoom.Name;
            }
        }

        public string MacAddress {
            get {
                return (IsProxy ? ProxyMacAddress(iDevice.Udn) : VolkanoMacAddress(iDevice.Udn));
            }
        }

        public string IpAddress {
            get {
                return iDevice.IpAddress;
            }
        }

        public string Udn {
            get {
                return iDevice.Udn;
            }
        }

        public string ImageUri {
            get {
                return iImageUri;
            }
        }

        public string PresentationUri {
            get {
                return iDevice.PresentationUri;
            }
        }

        public string ConfigurationUri {
            get {
                return "http://" + IpAddress + "/Config/Layouts/Default/index.html" + (IsDs(Udn) ? "?service=Ds" : "") + (IsPreamp(Udn) ? "?service=Preamp" : "") + (IsCd(Udn) ? "?service=Cd" : "");
            }
        }

        public string ConfigurationAppUri {
            get {
                return "http://" + IpAddress + "/App/Config/Layouts/Default/index.html" + (IsDs(Udn) ? "?service=Ds" : "") + (IsPreamp(Udn) ? "?service=Preamp" : "") + (IsCd(Udn) ? "?service=Cd" : "");
            }
        }

        public void UpdateFirmware(IUpdateFirmwareHandler aHandler, bool aDeviceRecovery) {
            iUpdateFirmware.Start(SoftwareUpdateUrl, SoftwareUpdateVariant, UglyName(Udn), Room, Name, SoftwareUpdateVersion, aDeviceRecovery, aHandler);
        }

        public void RestoreFactoryDefaults(IUpdateFirmwareHandler aHandler) {
            iUpdateFirmware.RestoreFactoryDefaults(UglyName(Udn), Room, Name, aHandler);
        }

        public bool UpdateFirmwareInProgress {
            get {
                return iUpdateFirmware.InProgress;
            }
        }

        public bool UpdateCheckInProgress {
            get {
                return iUpdateCheck.InProgress;
            }
        }

        public bool UpdateCheckFailed {
            get {
                return iUpdateCheck.Failed;
            }
        }

        public bool UpdateCheckDeviceNotFound {
            get {
                return (SoftwareUpdateVersion == null);
            }
        }

        public EventHandler<EventArgs> UpdateCheckCompleteEvent {
            get {
                return iUpdateCheck.EventUpdateCheckComplete;
            }
            set {
                iUpdateCheck.EventUpdateCheckComplete += value;
            }
        }

        public EventHandler<EventArgsUpdateError> UpdateCheckErrorEvent {
            get {
                return iUpdateCheck.EventUpdateCheckError;
            }
            set {
                iUpdateCheck.EventUpdateCheckError += value;
            }
        }

        public void CheckForUpdates() {
            iUpdateCheck.Start();
        }

        public bool SoftwareUpdateAvailable {
            get {
                if (iSoftwareUpdateUrl != null && iSoftwareUpdateVariant != null && iSoftwareUpdateVersion != null) {
                    if (IsFallBack(Udn)) {
                        return true;
                    }
                    else {
                        return iSoftwareUpdateAvailable;
                    }
                }
                return false;
            }
        }

        public string SoftwareUpdateVersion {
            get {
                if (iIsProxy) {
                    return "Dealer Update (" + iSoftwareUpdateVersion + ")";
                }
                else {
                    return VersionSupport.SoftwareVersionPretty(iSoftwareUpdateVersion, true);
                }
            }
        }

        public string SoftwareUpdateUrl {
            get {
                return iSoftwareUpdateUrl;
            }
        }

        public string SoftwareUpdateVariant {
            get {
                return iSoftwareUpdateVariant;
            }
        }

        public string ReleaseNotesHtml {
            get {
                return iReleaseNotesHtml;
            }
        }

        public string SoftwareUpdateString(bool aShowAll) {
            if (SoftwareUpdateAvailable || aShowAll) {
                return SoftwareUpdateVersion + (aShowAll ? " [Available: " + SoftwareUpdateAvailable.ToString() + ", Url: " + SoftwareUpdateUrl + ", Variant: " + SoftwareUpdateVariant + "]" : "");
            }
            else {
                return "";
            }
        }

        public string SoftwareVersion {
            get {
                if (IsFallBack(Udn)) {
                    return "Fallback (" + iSoftwareVersion + ")";
                }
                else {
                    return VersionSupport.SoftwareVersionPretty(iSoftwareVersion, true);
                }
            }
        }

        public Playback Playback {
            get {
                return iPlayback;
            }
        }

        public string ProductId {
            get {
                return iProductId;
            }
        }

        public string[] BoardType {
            get {
                return iBoardType;
            }
        }

        public string[] BoardId {
            get {
                return iBoardId;
            }
        }

        public string[] BoardDescription {
            get {
                return iBoardDescription;
            }
        }

        public string[] BoardNumber {
            get {
                return iBoardNumber;
            }
        }

        public string BoardInfoString {
            get {
                string boardInfo = "";

                if (iBoardNumber.Length > 0 && iBoardNumber.Length == iBoardType.Length && iBoardNumber.Length == iBoardId.Length && iBoardNumber.Length == iBoardDescription.Length) {
                    boardInfo = "Num: Board Type | Board ID | Board Description" + Environment.NewLine;
                    boardInfo += "----------------------------------------------" + Environment.NewLine;

                    for (uint i = 0; i < iBoardNumber.Length; i++) {
                        boardInfo += iBoardNumber[i] + ": " + iBoardType[i] + " | " + iBoardId[i] + " | " + iBoardDescription[i] + Environment.NewLine;
                        if (iBoardDescription[i].Length > 0) {
                            boardInfo += "";
                        }
                    }
                }

                return boardInfo;
            }
        }

        public BasicSetup BasicSetup {
            get {
                return iBasicSetup;
            }
        }

        public void GetSysLog(SysLogPretty.DSysLogComplete aCallback) {
            iSysLogPretty.Refresh(iDevice, aCallback);
        }

        public ServiceDiagnostics ServiceDiagnostics {
            get {
                return new ServiceDiagnostics(iDevice, iEventServer);
            }
        }

        public bool IsProxy {
            get {
                return iIsProxy;
            }
        }

        public static bool IsFallBack(string aUdn) {
            return (aUdn.Substring(32, 2) == "00");
        }

        public static bool IsPreamp(string aUdn) {
            return (aUdn.Substring(34, 2) == "33");
        }

        public static bool IsCd(string aUdn) {
            return (aUdn.Substring(34, 2) == "2f");
        }

        public static bool IsDs(string aUdn) {
            return (aUdn.Substring(34, 2) == "3f");
        }

        public static string VolkanoMacAddress(string aUdn) {
            return (aUdn.Substring(9, 14));
        }

        public static string ProxyMacAddress(string aUdn) {
            return (VolkanoMacAddress(aUdn) + " RS232" + (Box.IsPreamp(aUdn) ? " Preamp" : "") + (Box.IsCd(aUdn) ? " Cd" : ""));
        }

        public static string UglyName(string aUdn) {
            return "linn-" + aUdn.Substring(16, 7).Replace("-", "");
        }

        public override string ToString() {
            return (IsProxy ? "RS232 -> " : "") + Room + " - " + Name;
        }

        internal void SetFallback(Device aDevice) {
            SetState(aDevice, EState.eFallback);
        }

        //room name is not passed as if the room name had been modified the box would be removed and added to the tree
        internal void SetOn(Device aDevice, string aModel, string aName, BasicSetup aBasicSetup, Playback aPlayback) {
            Lock();

            iModel = aModel;
            iName = aName;
            iBasicSetup = aBasicSetup;
            iPlayback = aPlayback;

            Unlock();

            SetState(aDevice, EState.eOn);
        }

        internal void SetDetails(string aModel, string aName) {
            Lock();

            iModel = aModel;
            iName = aName;

            Unlock();

            if (EventBoxChanged != null) {
                EventBoxChanged(this, new EventArgsBox(this));
            }
        }

        internal void SetVersionDetails(string aSoftwareVersion, string aProductId) {
            Lock();

            iSoftwareVersion = aSoftwareVersion;
            iProductId = aProductId;
            iUpdateCheck.GetInfo(iModel, iIsProxy, iSoftwareVersion, iBoardNumber, out iSoftwareUpdateAvailable, out iSoftwareUpdateVersion, out iSoftwareUpdateUrl, out iSoftwareUpdateVariant, out iReleaseNotesHtml);

            Unlock();

            if (EventBoxChanged != null) {
                EventBoxChanged(this, new EventArgsBox(this));
            }
        }

        internal void SetProxyVersionDetails(string[] aBoardType, string[] aBoardDescription, string[] aBoardNumber, string aSoftwareVersion) {
            Lock();

            iSoftwareVersion = aSoftwareVersion;
            iBoardType = aBoardType;
            iBoardDescription = aBoardDescription;
            iBoardNumber = aBoardNumber;
            iUpdateCheck.GetInfo(iModel, iIsProxy, iSoftwareVersion, iBoardNumber, out iSoftwareUpdateAvailable, out iSoftwareUpdateVersion, out iSoftwareUpdateUrl, out iSoftwareUpdateVariant, out iReleaseNotesHtml);

            Unlock();

            if (EventBoxChanged != null) {
                EventBoxChanged(this, new EventArgsBox(this));
            }
        }

        internal void SetBoardDetails(string[] aBoardId, string[] aBoardType, string[] aBoardDescription, string[] aBoardNumber) {
            Lock();

            iBoardId = aBoardId;
            iBoardType = aBoardType;
            iBoardDescription = aBoardDescription;
            iBoardNumber = aBoardNumber;
            iUpdateCheck.GetInfo(iModel, iIsProxy, iSoftwareVersion, iBoardNumber, out iSoftwareUpdateAvailable, out iSoftwareUpdateVersion, out iSoftwareUpdateUrl, out iSoftwareUpdateVariant, out iReleaseNotesHtml);

            Unlock();

            if (EventBoxChanged != null) {
                EventBoxChanged(this, new EventArgsBox(this));
            }
        }

        internal void SetOff() {
            Lock();

            iState = EState.eOff;

            Unlock();

            if (EventBoxChanged != null) {
                EventBoxChanged(this, new EventArgsBox(this));
            }
        }

        private void UpdateCheckComplete(object obj, EventArgs e) {
            Lock();

            bool changed = false;
            bool available;
            string url, version, variant, releaseNotesHtml;

            iUpdateCheck.GetInfo(iModel, iIsProxy, iSoftwareVersion, iBoardNumber, out available, out version, out url, out variant, out releaseNotesHtml);

            if (iSoftwareUpdateAvailable != available) {
                iSoftwareUpdateAvailable = available;
                changed = true;
            }

            if (iSoftwareUpdateVersion != version) {
                iSoftwareUpdateVersion = version;
                changed = true;
            }

            if (iSoftwareUpdateUrl != url) {
                iSoftwareUpdateUrl = url;
                changed = true;
            }

            if (iSoftwareUpdateVariant != variant) {
                iSoftwareUpdateVariant = variant;
                changed = true;
            }

            if (iReleaseNotesHtml != releaseNotesHtml) {
                iReleaseNotesHtml = releaseNotesHtml;
                changed = true;
            }

            Unlock();

            if (EventBoxChanged != null && changed) {
                EventBoxChanged(this, new EventArgsBox(this));
            }
        }

        private void UpdateCheckError(object obj, EventArgs e) {
            if (EventBoxChanged != null) {
                EventBoxChanged(this, new EventArgsBox(this));
            }
        }

        private void SetState(Device aDevice, EState aState) {
            Lock();

            iDevice = aDevice;
            iState = aState;

            Unlock();

            if (EventBoxChanged != null) {
                EventBoxChanged(this, new EventArgsBox(this));
            }
        }

        private void Lock() {
            iMutex.WaitOne();
        }

        private void Unlock() {
            iMutex.ReleaseMutex();
        }

        private Mutex iMutex = new Mutex();
        private Helper iHelper;
        private EventServerUpnp iEventServer;
        private Device iDevice;
        private EState iState;
        private string iName;
        private string iModel;
        private bool iSoftwareUpdateAvailable;
        private string iSoftwareUpdateVersion;
        private string iSoftwareUpdateUrl;
        private string iSoftwareUpdateVariant;
        private string iSoftwareVersion;
        private string iProductId;
        private string iReleaseNotesHtml;
        private string[] iBoardId = new string[] { "" };
        private string[] iBoardType = new string[] { "" };
        private string[] iBoardDescription = new string[] { "" };
        private string[] iBoardNumber = new string[] { "" };
        private string iImageUri;
        private bool iIsProxy = false;
        private Room iRoom;
        private UpdateCheck iUpdateCheck;
        private UpdateFirmware iUpdateFirmware;
        private BasicSetup iBasicSetup;
        private Playback iPlayback;
        private SysLogPretty iSysLogPretty;
    }

    public class EventArgsRoom : EventArgs
    {
        public EventArgsRoom(Room aRoom) {
            iRoom = aRoom;
        }

        public Room RoomArg {
            get {
                return iRoom;
            }
        }

        private Room iRoom;
    }

    public class EventArgsBox : EventArgs
    {
        public EventArgsBox(Box aBox) {
            iBox = aBox;
        }

        public Box BoxArg {
            get {
                return iBox;
            }
        }

        private Box iBox;
    }
}



