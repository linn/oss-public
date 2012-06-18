using System;
using System.Net;
using System.Windows.Forms;

using Linn.Toolkit.WinForms;
using Linn.Topology;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn;

using Upnp;

namespace LinnSysTray
{
    public partial class LinnSysTrayForm : Form, IStack
    {
        private Helper iHelper;

        private EventServerUpnp iEventServer;
        private SsdpListenerMulticast iListenerNotify;
        private House iHouse;
        
        private Source iSource;
        private ModelSourceDiscPlayer iModelSourceDiscPlayer;
        private ModelSourceMediaRenderer iModelSourceMediaRenderer;

        public LinnSysTrayForm(Helper aHelper)
        {
            iHelper = aHelper;
            InitializeComponent();

            iHelper.Stack.SetStack(this);
            iHelper.Stack.SetStatusHandler(new StackStatusHandlerWinForms(iHelper.Title, notifyIcon));

            iEventServer = new EventServerUpnp();
            iListenerNotify = new SsdpListenerMulticast();
            iHouse = new House(iListenerNotify, iEventServer, new ModelFactory());

            iHouse.EventRoomAdded += EventRoomAdded;
            iHouse.EventRoomRemoved += EventRoomRemoved;

            notifyIcon.BalloonTipTitle = "Not subscribed";
            notifyIcon.BalloonTipText = "No infomation";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        }

        private void EventRoomAdded(object sender, EventArgsRoom e)
        {
            e.Room.EventSourceAdded += EventSourceAdded;
            e.Room.EventSourceRemoved += EventSourceRemoved;
        }

        private void EventRoomRemoved(object sender, EventArgsRoom e)
        {
            e.Room.EventSourceAdded -= EventSourceAdded;
            e.Room.EventSourceRemoved -= EventSourceRemoved;
        }

        private void EventSourceAdded(object sender, EventArgsSource e)
        {
            if (e.Source.Type == "Disc" || e.Source.Type == "Playlist" || e.Source.Type == "UpnpAv")
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Name = "sourceToolStripMenuItem" + e.Source.Room.Name + e.Source.Name + e.Source.Type;
                item.CheckOnClick = true;
                item.Size = new System.Drawing.Size(152, 22);
                item.Text = e.Source.Room.Name + ":" + e.Source.Name + " (" + e.Source.Type + ")";
                item.Tag = e.Source;
                item.Click += new System.EventHandler(sourceToolStripMenuItem_Click);

                BeginInvoke((MethodInvoker)delegate()
                {
                    selectDeviceToolStripMenuItem.DropDownItems.Add(item);
                });
            }
        }

        private void EventSourceRemoved(object sender, EventArgsSource e)
        {
            ToolStripMenuItem item = null;
            foreach (ToolStripMenuItem i in selectDeviceToolStripMenuItem.DropDownItems)
            {
                if (i.Tag == e.Source)
                {
                    item = i;
                    break;
                }
            }
            if (iSource == e.Source)
            {
                CloseSource();
            }

            BeginInvoke((MethodInvoker)delegate()
            {
                if (item != null)
                {
                    selectDeviceToolStripMenuItem.DropDownItems.Remove(item);
                }
            });
        }

        private void OpenSource()
        {
            if (iSource.Type == "Disc")
            {
                iModelSourceDiscPlayer = new ModelSourceDiscPlayerSdp(iSource);
                iModelSourceDiscPlayer.EventInitialised += EventInitialised;
                iModelSourceDiscPlayer.EventTrackIndexChanged += EventFromDiscPlayer;
                iModelSourceDiscPlayer.EventPlayStateChanged += EventFromDiscPlayer;
                iModelSourceDiscPlayer.Open();
            }
            else if (iSource.Type == "Playlist" || iSource.Type == "MediaRenderer")
            {
                iModelSourceMediaRenderer = ModelSourceMediaRenderer.Create(iSource);
                iModelSourceMediaRenderer.EventControlInitialised += EventInitialised;
                iModelSourceMediaRenderer.EventTrackChanged += EventFromMediaRenderer;
                iModelSourceMediaRenderer.EventTransportStateChanged += EventFromMediaRenderer;
                iModelSourceMediaRenderer.Open();
            }
        }

        private void CloseSource()
        {
            BeginInvoke((MethodInvoker)delegate()
                {
                    notifyIcon.Text = "LinnSysTray\nStatus: Not connected";
                    previousToolStripMenuItem.Enabled = false;
                    playToolStripMenuItem.Enabled = false;
                    pauseToolStripMenuItem.Enabled = false;
                    stopToolStripMenuItem.Enabled = false;
                    nextToolStripMenuItem.Enabled = false;
                }
            );

            if (iModelSourceDiscPlayer != null)
            {
                iModelSourceDiscPlayer.Close();
                iModelSourceDiscPlayer.EventInitialised -= EventInitialised;
                iModelSourceDiscPlayer.EventTrackIndexChanged -= EventFromDiscPlayer;
                iModelSourceDiscPlayer.EventPlayStateChanged -= EventFromDiscPlayer;
                iModelSourceDiscPlayer = null;
                iSource = null;
            }
            else if (iModelSourceMediaRenderer != null)
            {
                iModelSourceMediaRenderer.Close();
                iModelSourceMediaRenderer.EventControlInitialised -= EventInitialised;
                iModelSourceMediaRenderer.EventTrackChanged -= EventFromMediaRenderer;
                iModelSourceMediaRenderer.EventTransportStateChanged -= EventFromMediaRenderer;
                iModelSourceMediaRenderer = null;
                iSource = null;
            }
        }

        private void sourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item.CheckState == CheckState.Checked)
            {
                if (iSource != null)
                {
                    foreach (ToolStripMenuItem i in selectDeviceToolStripMenuItem.DropDownItems)
                    {
                        if (i.Tag == iSource)
                        {
                            i.CheckState = CheckState.Unchecked;
                        }
                    }
                }
                iSource = item.Tag as Source;
                OpenSource();
            }
            else
            {
                CloseSource();
            }
        }

        private void EventInitialised(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate()
                {
                    notifyIcon.Text = "Linn SysTray\n" + iSource.Room.Name + ":" + iSource.Name + "\n" + "State: Connected";
                    previousToolStripMenuItem.Enabled = true;
                    playToolStripMenuItem.Enabled = true;
                    pauseToolStripMenuItem.Enabled = true;
                    stopToolStripMenuItem.Enabled = true;
                    nextToolStripMenuItem.Enabled = true;
                }
            );
        }

        private void EventFromMediaRenderer(object sender, EventArgs e)
        {
            string title = iSource.Room.Name + ":" + iSource.Name;
            string metadata = "";
            iModelSourceMediaRenderer.Lock();
            MrItem item = null;
            if (iModelSourceMediaRenderer.TrackIndex != -1)
            {
                item = iModelSourceMediaRenderer.PlaylistItem((uint)iModelSourceMediaRenderer.TrackIndex);
            }
            iModelSourceMediaRenderer.Unlock();

            if (item != null && item.DidlLite[0] is musicTrack)
            {
                musicTrack t = item.DidlLite[0] as musicTrack;
                metadata += "Title: " + t.Title + "\n";
                metadata += "Artist: ";
                if (t.Album.Count > 0)
                {
                    metadata += t.Artist[0].Artist;
                }
                metadata += "\n";
                metadata += "Album: ";
                if (t.Album.Count > 0)
                {
                    metadata += t.Album[0];
                }
                metadata += "\n\n";
            }
            else
            {
                metadata += "No track playing\n\n";
            }

            metadata += "Track: " + (iModelSourceMediaRenderer.TrackIndex + 1);
            if(iModelSourceMediaRenderer.PlaylistTrackCount > 0)
            {
                metadata += " of " + iModelSourceMediaRenderer.PlaylistTrackCount;
            }
            metadata += "\n";

            metadata += "State: ";
            switch (iModelSourceMediaRenderer.TransportState)
            {
                case ModelSourceMediaRenderer.ETransportState.eBuffering:
                    metadata += "Buffering";
                    break;
                case ModelSourceMediaRenderer.ETransportState.ePaused:
                    metadata += "Paused";
                    break;
                case ModelSourceMediaRenderer.ETransportState.ePlaying:
                    metadata += "Playing";
                    break;
                case ModelSourceMediaRenderer.ETransportState.eStopped:
                    metadata += "Stopped";
                    break;
                case ModelSourceMediaRenderer.ETransportState.eUnknown:
                    metadata += "Unknown";
                    break;
            }
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = metadata;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ShowBalloonTip(3000);
        }

        private void EventFromDiscPlayer(object sender, EventArgs e)
        {
            string title = iSource.Room.Name + ":" + iSource.Name;
            string metadata = "";
            if (iModelSourceDiscPlayer.TrackIndex > -1)
            {
                metadata += "Title: Track " + (iModelSourceDiscPlayer.TrackIndex + 1) + "\n\n";
            }
            else
            {
                metadata += "No track playing\n\n";
            }

            metadata += "Track: " + (iModelSourceDiscPlayer.TrackIndex + 1);
            if (iModelSourceDiscPlayer.TrackCount > 0)
            {
                metadata += " of " + iModelSourceDiscPlayer.TrackCount;
            }
            metadata += "\n";

            metadata += "State: ";
            switch (iModelSourceDiscPlayer.PlayState)
            {
                case ModelSourceDiscPlayer.EPlayState.ePaused:
                    metadata += "Paused";
                    break;
                case ModelSourceDiscPlayer.EPlayState.ePlaying:
                    metadata += "Playing";
                    break;
                case ModelSourceDiscPlayer.EPlayState.eStopped:
                    metadata += "Stopped";
                    break;
                case ModelSourceDiscPlayer.EPlayState.eSuspended:
                    metadata += "Suspended";
                    break;
                case ModelSourceDiscPlayer.EPlayState.eUnknown:
                    metadata += "Unknown";
                    break;
            }
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = metadata;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ShowBalloonTip(3000);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void previousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (iModelSourceDiscPlayer != null)
            {
                iModelSourceDiscPlayer.Previous();
            }
            else if (iModelSourceMediaRenderer != null)
            {
                iModelSourceMediaRenderer.Previous();
            }
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (iModelSourceDiscPlayer != null)
            {
                iModelSourceDiscPlayer.Play();
            }
            else if (iModelSourceMediaRenderer != null)
            {
                iModelSourceMediaRenderer.Play();
            }
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (iModelSourceDiscPlayer != null)
            {
                iModelSourceDiscPlayer.Pause();
            }
            else if (iModelSourceMediaRenderer != null)
            {
                iModelSourceMediaRenderer.Pause();
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (iModelSourceDiscPlayer != null)
            {
                iModelSourceDiscPlayer.Stop();
            }
            else if (iModelSourceMediaRenderer != null)
            {
                iModelSourceMediaRenderer.Stop();
            }
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (iModelSourceDiscPlayer != null)
            {
                iModelSourceDiscPlayer.Next();
            }
            else if (iModelSourceMediaRenderer != null)
            {
                iModelSourceMediaRenderer.Next();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon.ShowBalloonTip(3000);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowOptionsDialog(false);
        }

        private void ShowOptionsDialog(bool aShowNetworkPage)
        {
            // add a new stack status change handler while the options page  is visible
            // leave the default one so the balloon tips still appear
            iHelper.Stack.EventStatusChanged += iHelper.Stack.StatusHandler.StackStatusOptionsChanged;

            FormUserOptions optionsDialog = new FormUserOptions(iHelper.OptionPages);
            if (aShowNetworkPage)
            {
                optionsDialog.SetPageByName("Network");
            }
            optionsDialog.ShowDialog(this);
            optionsDialog.Dispose();

            iHelper.Stack.EventStatusChanged -= iHelper.Stack.StatusHandler.StackStatusOptionsChanged;
        }

        private void LinnSysTrayForm_Load(object sender, EventArgs e)
        {
            iHelper.Stack.Start();

            // show the options dialog if specified by the user
            if (iHelper.Stack.StatusHandler.ShowOptions) {
                ShowOptionsDialog(true);
            }
        }

        private void LinnSysTrayForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseSource();
            iHelper.Stack.Stop();
        }

        void IStack.Start(IPAddress aIpAddress)
        {
            iEventServer.Start(aIpAddress);
            iListenerNotify.Start(aIpAddress);
            iHouse.Start(aIpAddress);
        }

        void IStack.Stop()
        {
            iHouse.Stop();
            iListenerNotify.Stop();
            iEventServer.Stop();
        }
    }
}
