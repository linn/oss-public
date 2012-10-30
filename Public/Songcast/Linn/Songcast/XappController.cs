
using System;
using System.Collections.Generic;

using OpenHome.Xapp;


namespace Linn.Songcast
{

    public class XappController
    {
        public XappController(Model aModel, IInvoker aInvoker)
        {
            iFramework = new Framework(OpenHome.Xen.Environment.AppPath + "/presentation");
            iFramework.AddCss("main/index.css");

            iPage = new MainPage(aModel, aInvoker, "main", "main");
            iFramework.AddPage(iPage);

            iWebServer = new WebServer(iFramework);
        }

        public string MainPageUri
        {
            get { return iWebServer.ResourceUri + iPage.UriPath; }
        }
        
        public MainPage MainPage
        {
            get { return iPage; }
        }

        private Framework iFramework;
        private WebServer iWebServer;
        private MainPage iPage;
    }


    public class LongPollException : Exception
    {
    }


    public class MainPage : Page, ITrackerSender
    {
        public MainPage(Model aModel, IInvoker aInvoker, string aId, string aViewId)
            : base(aId, aViewId)
        {
            iModel = aModel;
            iInvoker = aInvoker;

            iTracker = new Tracker(iModel.Preferences.TrackerAccount, this);
            iTrackerSessionId = Guid.NewGuid().ToString();

            iModel.EventEnabledChanged += ModelEnabledChanged;
            iModel.EventReceiverListChanged += ModelReceiverListChanged;
            iModel.EventReceiverVolumeControlChanged += ModelReceiverVolumeControlChanged;
            iModel.EventReceiverVolumeChanged += ModelReceiverVolumeChanged;
            iModel.EventSubnetListChanged += ModelSubnetListChanged;

            iModel.Preferences.EventSelectedReceiverChanged += ModelSelectedReceiverChanged;
            iModel.Preferences.EventRotaryVolumeControlChanged += ModelRotaryVolumeControlChanged;
            iModel.Preferences.EventUseMusicLatencyChanged += ModelUseMusicLatencyChanged;
            iModel.Preferences.EventUsageDataChanged += ModelUsageDataChanged;
        }

        void ITrackerSender.Send(string aName, JsonObject aValue)
        {
            Send(aName, aValue);
        }

        public event EventHandler EventShowConfig;
        public event EventHandler EventShowHelp;

        public void TrackPageVisibilityChange(bool aVisibility)
        {
            iTracker.TrackVariable(Tracker.EVariableIndex.Index1, "PageVisibility", aVisibility ? "Visible" : "Hidden", Tracker.EVariableScope.Page);
            iTracker.TrackPageView(this.ViewId);
        }

        private void TrackSessionId(string aSessionId)
        {
            iTracker.TrackVariable(Tracker.EVariableIndex.Index2, "SessionId", aSessionId, Tracker.EVariableScope.Session);
        }

        private void TrackReceiverCount(uint aCount)
        {
            iTracker.TrackVariable(Tracker.EVariableIndex.Index3, "ReceiverCount", aCount.ToString(), Tracker.EVariableScope.Session);
        }

        private void TrackSubnetCount(uint aCount)
        {
            iTracker.TrackVariable(Tracker.EVariableIndex.Index4, "SubnetCount", aCount.ToString(), Tracker.EVariableScope.Session);
        }

        private void TrackRotaryControls(bool aRotary)
        {
            iTracker.TrackVariable(Tracker.EVariableIndex.Index5, "RotaryControls", aRotary ? "Rotary" : "Rocker", Tracker.EVariableScope.Session);
        }

        private delegate void DOnActivated(Session aSession);
        
        protected override void OnActivated(Session aSession)
        {
            if (iInvoker.TryBeginInvoke(new DOnActivated(OnActivated), aSession))
                return;

            // ensure that the Initialise event is only sent to the new session
            aSession.Send("Initialise", "");

            // these notifications strictly should only be sent to the new session but
            // there is no harm in sending to all
            UpdateDisplay(true);
            Send("RotaryVolumeControl", iModel.Preferences.RotaryVolumeControl);
            Send("UseMusicLatency", iModel.Preferences.UseMusicLatency);

            iTracker.SetTracking(iModel.Preferences.UsageData);
            TrackPageVisibilityChange(false);
            TrackSessionId(iTrackerSessionId);
            TrackRotaryControls(iModel.Preferences.RotaryVolumeControl);
            TrackReceiverCount((uint)iModel.ReceiverList.Length);
            TrackSubnetCount((uint)iModel.SubnetList.Length);
        }
        
        private delegate void DOnReceive(Session aSession, string aName, string aValue);
        
        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            if (iInvoker.TryBeginInvoke(new DOnReceive(OnReceive), aSession, aName, aValue))
                return;
            
            if (aName == "Clicked" && aValue == "standby")
            {
                iModel.Enabled = !iModel.Enabled;
            }
            else if (aName == "Clicked" && aValue == "configuration")
            {
                if (EventShowConfig != null) {
                    EventShowConfig(this, EventArgs.Empty);
                }
            }
            else if (aName == "Clicked" && aValue == "help")
            {
                if (EventShowHelp != null) {
                    EventShowHelp(this, EventArgs.Empty);
                }
            }
            else if (aName == "Clicked" && aValue == "stop")
            {
                if (iModel.Enabled)
                {
                    Receiver recv = iModel.Receiver(iModel.Preferences.SelectedReceiverUdn);
                    if (recv != null && recv.IsOnline)
                    {
                        recv.Stop();
                    }
                }
            }
            else if (aName == "Clicked" && aValue == "play")
            {
                if (iModel.Enabled)
                {
                    Receiver recv = iModel.Receiver(iModel.Preferences.SelectedReceiverUdn);
                    if (recv != null && recv.IsOnline)
                    {
                        recv.Play();
                    }
                }
            }
            else if (aName == "Clicked" && aValue == "music")
            {
                iModel.Preferences.UseMusicLatency = true;
            }
            else if (aName == "Clicked" && aValue == "video")
            {
                iModel.Preferences.UseMusicLatency = false;
            }
            else if (aName == "ClockwiseRotate")
            {
                if (iModel.Enabled)
                {
                    Receiver recv = iModel.Receiver(iModel.Preferences.SelectedReceiverUdn);
                    if (recv != null && recv.IsOnline && recv.HasVolumeControl)
                    {
                        recv.VolumeInc();
                    }
                }
            }
            else if (aName == "AntiClockwiseRotate")
            {
                if (iModel.Enabled)
                {
                    Receiver recv = iModel.Receiver(iModel.Preferences.SelectedReceiverUdn);
                    if (recv != null && recv.IsOnline && recv.HasVolumeControl)
                    {
                        recv.VolumeDec();
                    }
                }
            }
            else if (aName == "Clicked" && aValue == "")
            {
                if (iModel.Enabled)
                {
                    Receiver recv = iModel.Receiver(iModel.Preferences.SelectedReceiverUdn);
                    if (recv != null && recv.IsOnline && recv.HasVolumeControl)
                    {
                        recv.SetMute(!recv.Mute);
                    }
                }
            }
            else if (aName == "LongPollError")
            {
                UserLog.WriteLine(DateTime.Now + " LongPollError: " + aValue);
                throw new LongPollException();
            }
        }

        private void ModelEnabledChanged(object sender, EventArgs e)
        {
            UpdateDisplay(true);
        }

        private void ModelReceiverListChanged(object sender, EventArgs e)
        {
            TrackReceiverCount((uint)iModel.ReceiverList.Length);
            UpdateDisplay(true);
        }

        private void ModelSubnetListChanged(object sender, EventArgs e)
        {
            TrackSubnetCount((uint)iModel.SubnetList.Length);
        }

        private void ModelReceiverVolumeControlChanged(object sender, EventArgsReceiver e)
        {
            if (e.ReceiverUdn == iModel.Preferences.SelectedReceiverUdn)
            {
                UpdateDisplay(true);
            }
        }

        private void ModelReceiverVolumeChanged(object sender, EventArgsReceiver e)
        {
            if (e.ReceiverUdn == iModel.Preferences.SelectedReceiverUdn)
            {
                UpdateDisplay(false);
            }
        }

        private void ModelSelectedReceiverChanged(object sender, EventArgs e)
        {
            UpdateDisplay(true);
        }

        private void ModelRotaryVolumeControlChanged(object sender, EventArgs e)
        {
            Send("RotaryVolumeControl", iModel.Preferences.RotaryVolumeControl);
        }

        private void ModelUseMusicLatencyChanged(object sender, EventArgs e)
        {
            Send("UseMusicLatency", iModel.Preferences.UseMusicLatency);
        }

        private void ModelUsageDataChanged(object sender, EventArgs e)
        {
            iTracker.SetTracking(iModel.Preferences.UsageData);
        }

        private OpenHome.Songcast.EReceiverStatus ReceiverStatus(Receiver aReceiver)
        {
            if (!iModel.Enabled)
            {
                return OpenHome.Songcast.EReceiverStatus.eDisconnected;
            }
            else
            {
                return aReceiver.Status;
            }
        }

        private void UpdateDisplay(bool aLog)
        {
            Receiver recv = iModel.Receiver(iModel.Preferences.SelectedReceiverUdn);

            JsonObject info = new JsonObject();

            if (recv != null && recv.IsOnline && recv.HasVolumeControl)
            {
                // receiver with volume control is available
                info.Add("SongcastOn", new JsonValueBool(iModel.Enabled));
                info.Add("Room", new JsonValueString(recv.Room));
                info.Add("Status", new JsonValueUint((uint)ReceiverStatus(recv)));
                info.Add("Volume", new JsonValueUint(recv.Volume));
                info.Add("VolumeLimit", new JsonValueUint(recv.VolumeLimit));
                info.Add("Mute", new JsonValueBool(recv.Mute));
                Send("ReceiverOnlineWithVolume", info);
            }
            else if (recv != null && recv.IsOnline)
            {
                // receiver without volume control is available
                info.Add("SongcastOn", new JsonValueBool(iModel.Enabled));
                info.Add("Room", new JsonValueString(recv.Room));
                info.Add("Status", new JsonValueUint((uint)ReceiverStatus(recv)));
                Send("ReceiverOnlineNoVolume", info);
            }
            else if (recv != null && !recv.IsOnline)
            {
                // receiver is unavailable
                info.Add("SongcastOn", new JsonValueBool(iModel.Enabled));
                info.Add("Room", new JsonValueString(recv.Room));
                Send("ReceiverOffline", info);
            }
            else if (recv == null)
            {
                // no receiver is selected
                info.Add("SongcastOn", new JsonValueBool(iModel.Enabled));
                Send("ReceiverUnselected", info);
            }

            if (aLog)
            {
                UserLog.WriteLine(DateTime.Now + " : Linn.Songcast.MainPage.UpdateDisplay " + info.Serialise());
            }
        }
        
        private Model iModel;
        private IInvoker iInvoker;
        private Tracker iTracker;
        private string iTrackerSessionId;

    }
}


