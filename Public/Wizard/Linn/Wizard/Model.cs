
using System;
using System.Collections.Generic;

using Linn.ProductSupport;
using Linn.ProductSupport.Diagnostics;


namespace Linn.Wizard
{

    internal class XappTrackerId
    {
        private static string kTrackerIdDev = "UA-35626282-1";
        private static string kTrackerIdBeta = "UA-35636473-1";
        private static string kTrackerIdRelease = "UA-35622973-1";

        public XappTrackerId(Helper aHelper)
        {
            iId = kTrackerIdDev;
            if (aHelper.BuildType == EBuildType.Beta)
            {
                iId = kTrackerIdBeta;
            }
            else if (aHelper.BuildType == EBuildType.Release)
            {
                iId = kTrackerIdRelease;
            }
        }

        public string Id { get { return iId; } }
        private string iId;
    }

    // Class to provide global access to the model
    public class ModelInstance
    {
        public static void Create(Helper aHelper)
        {
            Assert.Check(iInstance == null);
            iInstance = new Model(aHelper);
        }

        public static Model Instance
        {
            get
            {
                Assert.Check(iInstance != null);
                return iInstance;
            }
        }

        private static Model iInstance;
    }


    // Model class for the entire wizard - contains all the models for each individual session
    public class Model
    {
        public Model(Helper aHelper)
        {
            iSessionModels = new Dictionary<Session, SessionModel>();

            // create network and diagnostics and run some tests
            iNetwork = new Network(aHelper);
            iDiagnostics = new Diagnostics();

            foreach (NetworkInterface netInterface in iNetwork.NetworkInterfaceList())
            {
                iDiagnostics.Run(ETest.eDhcp, netInterface.Info.IPAddress.ToString());
                iDiagnostics.Run(ETest.eInternet, netInterface.Info.IPAddress.ToString());
                iDiagnostics.Run(ETest.eUpnp, netInterface.Info.IPAddress.ToString());
            }
            iTrackerId = new XappTrackerId(aHelper);
        }

        public string TrackerId
        {
            get { return iTrackerId.Id; }
        }

        public SessionModel SessionModel(Session aSession)
        {
            Assert.Check(iSessionModels.ContainsKey(aSession));
            return iSessionModels[aSession];
        }

        public SessionModel CreateModel(Session aSession)
        {
            Assert.Check(!iSessionModels.ContainsKey(aSession));

            SessionModel model = new SessionModel(iNetwork);
            iSessionModels.Add(aSession, model);
            return model;
        }

        public Network Network
        {
            get { return iNetwork; }
        }

        public Diagnostics Diagnostics
        {
            get { return iDiagnostics; }
        }

        public void Close()
        {
            iDiagnostics.Shutdown();
            iNetwork.Close();
        }

        private Dictionary<Session, SessionModel> iSessionModels;
        private Network iNetwork;
        private Diagnostics iDiagnostics;
        private XappTrackerId iTrackerId;
    }


    // The model class containing all data specific to a single session
    public class SessionModel
    {
        public SessionModel(Network aNetwork)
        {
            SelectedProduct = string.Empty;
            SelectedBoxMacAddress = string.Empty;
            PendingSource = null;
            ConfiguredSources = new List<SourceConfig>();
            iNetwork = aNetwork;
        }

        public string SelectedProduct
        {
            get;
            set;
        }

        public string SelectedBoxMacAddress
        {
            get;
            set;
        }

        public Box SelectedBox
        {
            get { return (SelectedBoxMacAddress == string.Empty) ? null : iNetwork.Box(SelectedBoxMacAddress); }
        }

        public SourceConfig PendingSource
        {
            get;
            set;
        }

        public List<SourceConfig> ConfiguredSources
        {
            get;
            set;
        }

        public bool SongcastVisible
        {
            get { return iSongcastVisible; }
            set
            {
                iSongcastVisible = value;
                SelectedBox.BasicSetup.SetSourceVisible("Songcast", value);
            }
        }

        public bool PlaylistVisible
        {
            get { return iPlaylistVisible; }
            set
            {
                iPlaylistVisible = value;
                SelectedBox.BasicSetup.SetSourceVisible("Playlist", value);
                SelectedBox.BasicSetup.SetSourceVisible("Radio", value);
            }
        }

        private Network iNetwork;
        private bool iSongcastVisible;
        private bool iPlaylistVisible;
    }


    // Class to store info about a source that has been/is being configured by the wizard
    public class SourceConfig
    {
        public SourceConfig()
        {
            Name = string.Empty;
            SystemName = string.Empty;
            IconName = string.Empty;
        }

        public string Name
        {
            get;
            set;
        }

        public string SystemName
        {
            get;
            set;
        }

        public string IconName
        {
            get;
            set;
        }
    }


    // IPageModel implementation for the first page in the wizard
    public class PageModelMain : IPageModel
    {
        public PageModelMain(Session aSession)
        {
            // Since this is the page model for the first page shown, this has the
            // responsibility of creating the underlying model for the session.
            iModel = ModelInstance.Instance.CreateModel(aSession);
        }

        public string ProductName
        {
            get { return iModel.SelectedProduct; }
            set { iModel.SelectedProduct = value; }
        }

        #region Implementation of IPageModel

        public void Opened()
        {
        }

        public void Reopened()
        {
        }

        public void Completed()
        {
        }

        #endregion

        private SessionModel iModel;
    }


    // IPageModel implementation for the room name page
    public class PageModelRoomName : IPageModel
    {
        public PageModelRoomName(Session aSession)
        {
            iModel = ModelInstance.Instance.SessionModel(aSession);
            iRoom = iModel.SelectedBox.Room;
            iTracker = aSession.Tracker;
        }

        public string RoomName
        {
            get { return iRoom; }
            set { iRoom = value; }
        }

        #region Implementation of IPageModel

        public void Opened()
        {
        }

        public void Reopened()
        {
        }

        public void Completed()
        {
            if (iRoom != iModel.SelectedBox.Room)
            {
                iModel.SelectedBox.BasicSetup.SetRoom(iRoom);
            }
            iTracker.TrackEvent("RoomName", iModel.SelectedProduct, iRoom, 0);
        }

        #endregion

        private SessionModel iModel;
        private string iRoom;
        private OpenHome.Xapp.Tracker iTracker;
    }


    // IPageModel implementation for the configuration of internal sources
    public class PageModelSourceInternal : IPageModel
    {
        public PageModelSourceInternal(Session aSession)
        {
            iModel = ModelInstance.Instance.SessionModel(aSession);
        }

        public void ShowSongcast()
        {
            iModel.SongcastVisible = true;
        }

        public void HideSongcast()
        {
            iModel.SongcastVisible = false;
        }

        public void ShowPlaylist()
        {
            iModel.PlaylistVisible = true;
        }

        public void HidePlaylist()
        {
            iModel.PlaylistVisible = false;
        }

        #region Implementation of IPageModel

        public void Opened()
        {
        }

        public void Reopened()
        {
        }

        public void Completed()
        {
        }

        #endregion

        private SessionModel iModel;
    }


    // IPageModel implementation for the source name page
    public class PageModelSourceName : IPageModel
    {
        public PageModelSourceName(Session aSession)
        {
            iModel = ModelInstance.Instance.SessionModel(aSession);

            // this is the first page of a new source config - create the config object and set as pending
            iModel.PendingSource = new SourceConfig();
        }

        public string SourceName
        {
            get { return iModel.PendingSource.Name; }
            set { iModel.PendingSource.Name = value; }
        }

        #region Implementation of IPageModel

        public void Opened()
        {
        }

        public void Reopened()
        {
        }

        public void Completed()
        {
        }

        #endregion

        private SessionModel iModel;
    }


    // IPageModel implementation for the source type selection page
    public class PageModelSourceType : IPageModel
    {
        public PageModelSourceType(Session aSession)
        {
            iNextPage = string.Empty;
        }

        public string NextPage
        {
            get { return iNextPage; }
            set { iNextPage = value; }
        }

        #region Implementation of IPageModel

        public void Opened()
        {
        }

        public void Reopened()
        {
        }

        public void Completed()
        {
        }

        #endregion

        private string iNextPage;
    }


    // IPageModel implementation for the source input selection page
    public class PageModelSourceInput : IPageModel
    {
        public PageModelSourceInput(Session aSession)
        {
            iModel = ModelInstance.Instance.SessionModel(aSession);
        }

        public string SourceInput
        {
            get { return iModel.PendingSource.SystemName; }
            set { iModel.PendingSource.SystemName = value; }
        }

        #region Implementation of IPageModel

        public void Opened()
        {
        }

        public void Reopened()
        {
        }

        public void Completed()
        {
        }

        #endregion

        private SessionModel iModel;
    }


    // IPageModel implementation for the source icon screen
    public class PageModelSourceIcon : IPageModel
    {
        public PageModelSourceIcon(Session aSession)
        {
            iModel = ModelInstance.Instance.SessionModel(aSession);

            // at this point the pending source's system name is known - icon information can now be determined
            SortedList<string, SourceInfo> sourceInfos = iModel.SelectedBox.BasicSetup.SourceInfoList;
            SourceInfo sourceInfo = sourceInfos[iModel.PendingSource.SystemName];

            // set the current icon
            iModel.PendingSource.IconName = "source-icon-" + sourceInfo.Icon.Name;

            // set the list of allowed icons
            List<string> allowedIconNames = new List<string>();

            foreach (SourceIcon icon in sourceInfo.AllowedIcons)
            {
                allowedIconNames.Add("source-icon-" + icon.Name);
            }

            iAllowedIconNames = allowedIconNames.ToArray();
        }

        public string SourceIcon
        {
            get { return iModel.PendingSource.IconName; }
            set { iModel.PendingSource.IconName = value; }
        }

        public string[] AllowedSourceIcons
        {
            get { return iAllowedIconNames; }
        }

        #region Implementation of IPageModel

        public void Opened()
        {
        }

        public void Reopened()
        {
        }

        public void Completed()
        {
        }

        #endregion

        private SessionModel iModel;
        private string[] iAllowedIconNames;
    }


    // IPageModel implementation for the source test page
    public class PageModelSourceTest : IPageModel
    {
        
        public PageModelSourceTest(Session aSession)
        {
            iModel = ModelInstance.Instance.SessionModel(aSession);
            iSource = iModel.PendingSource;
        }

        public string SourceName
        {
            get { return iSource.Name; }
        }

        public string SourceInput
        {
            get { return iSource.SystemName; }
        }

        public string SourceIcon
        {
            get { return iSource.IconName; }
        }

        public void StartTest()
        {
            // select this source on the ds - use the system name to set to avoid race conditions with
            // setting the name
            iModel.SelectedBox.Playback.SetSourceIndexByName("[" + iSource.SystemName + "]");
        }

        public void StopTest()
        {
            iModel.SelectedBox.SetStandby(true);
        }

        public void VolumeInc()
        {
            iModel.SelectedBox.Playback.VolumeInc();
        }

        public void VolumeDec()
        {
            iModel.SelectedBox.Playback.VolumeDec();
        }

        #region Implementation of IPageModel

        public void Opened()
        {
            // set the source config on the ds
            iModel.SelectedBox.BasicSetup.SetSourceVisible(iSource.SystemName, true);
            iModel.SelectedBox.BasicSetup.SetSourceName(iSource.SystemName, iSource.Name);

            if (!string.IsNullOrEmpty(iSource.IconName))
            {
                Assert.Check(iSource.IconName.StartsWith("source-icon-"));
                iModel.SelectedBox.BasicSetup.SetSourceIcon(iSource.SystemName, iSource.IconName.Substring(12));
            }
        }

        public void Reopened()
        {
            // the user has clicked back and reopened this page - remove the source from the list
            // of configured sources and reset the pending source to this one
            Assert.Check(iModel.ConfiguredSources.Contains(iSource));
            iModel.ConfiguredSources.Remove(iSource);
            iModel.PendingSource = iSource;
        }

        public void Completed()
        {
            // the user has clicked through to the next page - this source has now been officially configured
            Assert.Check(!iModel.ConfiguredSources.Contains(iSource));
            iModel.ConfiguredSources.Add(iSource);
            iModel.PendingSource = null;

            // test has finished - put ds back into standby
            iModel.SelectedBox.SetStandby(true);
        }

        #endregion

        private SessionModel iModel;
        private SourceConfig iSource;
    }


    // IPageModel implementation for the Use page
    public class PageModelUse : IPageModel
    {
        public PageModelUse(Session aSession)
        {
            iModel = ModelInstance.Instance.SessionModel(aSession);
            iTracker = aSession.Tracker;
        }

        public string YesToSongcast
        {
            get { return iModel.SongcastVisible ? "yes" : "no"; }
        }

        public string YesToPlaylist
        {
            get { return iModel.PlaylistVisible ? "yes" : "no"; }
        }

        public string HasConnectedSources
        {
            get { return iModel.ConfiguredSources.Count > 0 ? "yes" : "no"; }
        }

        #region Implementation of IPageModel

        public void Opened()
        {
            // The 'Use' page has been opened - this means the source config has finished and all unconfigured sources can be hidden
            SortedList<string, SourceInfo> sourceInfos = iModel.SelectedBox.BasicSetup.SourceInfoList;

            List<string> sourcesToHide = new List<string>();

            foreach (SourceInfo sourceInfo in sourceInfos.Values)
            {
                bool isConfigured = false;

                foreach (SourceConfig source in iModel.ConfiguredSources)
                {
                    if (sourceInfo.SystemName == source.SystemName)
                    {
                        isConfigured = true;
                        break;
                    }
                }


                // only back panel sources are currently automatically hidden
                if (!isConfigured
                    && sourceInfo.SystemName != "Front Aux"
                    && sourceInfo.SystemName != "Net Aux"
                    && sourceInfo.SystemName != "Playlist"
                    && sourceInfo.SystemName != "Radio"
                    && sourceInfo.SystemName != "Songcast"
                    && sourceInfo.SystemName != "UPnP AV")
                {
                    sourcesToHide.Add(sourceInfo.SystemName);
                }

                
            }
            foreach (string sourceToHide in sourcesToHide)
            {
                iModel.SelectedBox.BasicSetup.SetSourceVisible(sourceToHide, false);
            }

            TrackSourceInfo(sourceInfos, sourcesToHide);
        }

        public void Reopened()
        {
        }

        public void Completed()
        {
        }

        private void TrackSourceInfo(SortedList<string, SourceInfo> sourceInfos, List<string> sourcesToHide)
        {
            foreach (SourceInfo sourceInfo in sourceInfos.Values)
            {
                string productSourceName = string.Format("{0}:{1}", iModel.SelectedProduct, sourceInfo.SystemName);
                iTracker.TrackEvent("SourceName", productSourceName, sourceInfo.Name, 0);
                bool visible = sourceInfo.Visible && !sourcesToHide.Contains(sourceInfo.SystemName);
                iTracker.TrackEvent("SourceVisibility", productSourceName, visible ? "Visible" : "Hidden", 0);
            }
        }

        #endregion

        private SessionModel iModel;
        private OpenHome.Xapp.Tracker iTracker;
    }
}


