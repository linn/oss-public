
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


    public class MainPage : Page
    {
        public MainPage(Model aModel, IInvoker aInvoker, string aId, string aViewId)
            : base(aId, aViewId)
        {
            iModel = aModel;
            iInvoker = aInvoker;

            iModel.EventEnabledChanged += ModelEnabledChanged;
            iModel.EventReceiverListChanged += ModelReceiverListChanged;
            iModel.EventReceiverVolumeChanged += ModelReceiverVolumeChanged;

            iModel.Preferences.EventSelectedReceiverChanged += ModelSelectedReceiverChanged;
            iModel.Preferences.EventRotaryVolumeControlChanged += ModelRotaryVolumeControlChanged;
            iModel.Preferences.EventUseMusicLatencyChanged += ModelUseMusicLatencyChanged;
        }
        
        public event EventHandler EventShowConfig;
        public event EventHandler EventShowHelp;

        private delegate void DOnActivated(Session aSession);
        
        protected override void OnActivated(Session aSession)
        {
            if (iInvoker.TryBeginInvoke(new DOnActivated(OnActivated), aSession))
                return;

            // ensure that the Initialise event is only sent to the new session
            aSession.Send("Initialise", "");

            // these notifications strictly should only be sent to the new session but
            // there is no harm in sending to all
            UpdateDisplay();
            Send("RotaryVolumeControl", iModel.Preferences.RotaryVolumeControl);
            Send("UseMusicLatency", iModel.Preferences.UseMusicLatency);
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
            UpdateDisplay();
        }

        private void ModelReceiverListChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void ModelReceiverVolumeChanged(object sender, EventArgsReceiver e)
        {
            if (e.ReceiverUdn == iModel.Preferences.SelectedReceiverUdn)
            {
                UpdateDisplay();
            }
        }

        private void ModelSelectedReceiverChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void ModelRotaryVolumeControlChanged(object sender, EventArgs e)
        {
            Send("RotaryVolumeControl", iModel.Preferences.RotaryVolumeControl);
        }

        private void ModelUseMusicLatencyChanged(object sender, EventArgs e)
        {
            Send("UseMusicLatency", iModel.Preferences.UseMusicLatency);
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

        private void UpdateDisplay()
        {
            Receiver recv = iModel.Receiver(iModel.Preferences.SelectedReceiverUdn);
            if (recv != null && recv.IsOnline && recv.HasVolumeControl)
            {
                // receiver with volume control is available
                JsonObject info = new JsonObject();
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
                JsonObject info = new JsonObject();
                info.Add("SongcastOn", new JsonValueBool(iModel.Enabled));
                info.Add("Room", new JsonValueString(recv.Room));
                info.Add("Status", new JsonValueUint((uint)ReceiverStatus(recv)));
                Send("ReceiverOnlineNoVolume", info);
            }
            else if (recv != null && !recv.IsOnline)
            {
                // receiver is unavailable
                JsonObject info = new JsonObject();
                info.Add("SongcastOn", new JsonValueBool(iModel.Enabled));
                info.Add("Room", new JsonValueString(recv.Room));
                Send("ReceiverOffline", info);
            }
            else if (recv == null)
            {
                // no receiver is selected
                JsonObject info = new JsonObject();
                info.Add("SongcastOn", new JsonValueBool(iModel.Enabled));
                Send("ReceiverUnselected", info);
            }
        }
        
        private Model iModel;
        private IInvoker iInvoker;
    }
}


