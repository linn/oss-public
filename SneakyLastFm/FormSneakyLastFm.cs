using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.Net;
using System.Drawing.Drawing2D;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Topology;
using Upnp;

namespace SneakyLastFm
{
    public partial class FormSneakyLastFm : Form
    {
        private Mutex iMutex = new Mutex(false);
        private Mutex iTrackMutex = new Mutex(false);

        private EventServerUpnp iEventServer;
        private SsdpListenerMulticast iListenerNotify;
        private House iHouse;

        private ModelSourceMediaRenderer iMediaRenderer;
        private ModelVolumeControl iVolumeControl;
        private List<SourceUpnp> iRenderers = new List<SourceUpnp>();
        
        private string iUsername = "";
        private string iPassword = "";
        private LastFmService iLastFmService = null;
        private string iLastError = "";
        private bool iLoginRequired = true;
        
        private List<LastFmTrack> iPlaylist = null;
        private int iTrackIndex = -1;
        private LastFmTrack iTrack = null;
        private int iVolume = 50;
        private bool iMute = false;
        
        private Point iLastMousePos;
        private double iLastAngle = 0.0;
        private double iAngle = 0.0;
        private Point iKnobPos = new Point(46, 83);
        private bool iIgnoreStopEvent = false;
        private SolidBrush iBrush = new SolidBrush(Color.FromArgb(255, 208, 31, 60));
        private Pen iPen = new Pen(Color.Black, 3);
        private Pen iKnobPen = new Pen(Color.Black, 2);
        
        private FormCoverArt iFormCoverArt = new FormCoverArt();
        private Settings iSettings = new Settings();

        public FormSneakyLastFm(IPAddress aInterface)
        {
            InitializeComponent();

            iSettings.Load();
            iUsername = iSettings.Username;
            iPassword = iSettings.Password;
            iUsernameTextBox.Text = (iUsername == "") ? "Username" : iUsername;
            iPasswordTextBox.UseSystemPasswordChar = (iPassword == "") ? false : true;
            iPasswordTextBox.Text = (iPassword == "") ? "Password" : iPassword;

            iEventServer = new EventServerUpnp();
            iListenerNotify = new SsdpListenerMulticast();
            iHouse = new House(iEventServer, iListenerNotify);
            iHouse.EventRoomAdded += EventRoomAdded;
            iHouse.EventRoomRemoved += EventRoomRemoved;

            iHouse.Start(aInterface);
        }

        private void EventRoomAdded(object sender, House.EventArgsRoom e)
        {
            e.Room.Lock();
            iMutex.WaitOne();
            foreach (SourceUpnp s in e.Room.SourceList)
            {
                if (iMediaRenderer == null)
                {
                    SetCurrentSource(s);
                }
                iRenderers.Add(s);
            }
            iMutex.ReleaseMutex();

            e.Room.EventSourceAdded += EventSourceAdded;
            e.Room.EventSourceRemoved += EventSourceRemoved;

            e.Room.Unlock();
        }

        private void EventRoomRemoved(object sender, House.EventArgsRoom e)
        {
            e.Room.EventSourceAdded -= EventSourceAdded;
            e.Room.EventSourceRemoved -= EventSourceRemoved;
        }

        private void EventSourceAdded(object sender, Room.EventArgsSource e)
        {
            if (e.Source is SourceUpnp)
            {
                iMutex.WaitOne();
                if (iMediaRenderer == null)
                {
                    SetCurrentSource(e.Source as SourceUpnp);
                }
                iRenderers.Add(e.Source as SourceUpnp);
                iMutex.ReleaseMutex();
            }
        }

        private void EventSourceRemoved(object sender, Room.EventArgsSource e)
        {
            iMutex.WaitOne();
            if (e.Source is SourceUpnp)
            {
                if (iMediaRenderer != null && e.Source == iMediaRenderer.Source)
                {
                    SelectNextSource();
                }
                iRenderers.Remove(e.Source as SourceUpnp);
            }
            iMutex.ReleaseMutex();
        }

        /*private void EventRoomUpdated(ModelRoom aRoom)
        {
            iMutex.WaitOne();
            if (aRoom == iCurrentSource.ModelSource.ModelRoom)
            {
                IModelPreamp preamp = iCurrentSource.ModelSource.ModelRoom.ModelDevicePreamp.ModelPreamp;
                preamp.EEventSubscribed += EventSubscribedPreamp;
                preamp.EEventUnSubscribed += EventUnSubscribedPreamp;
                //preamp.EEventVolume += EventVolume;
                preamp.EEventMute += EventMute;
                if (!preamp.Subscribe())
                {
                    preamp.EEventSubscribed -= EventSubscribedPreamp;
                    preamp.EEventUnSubscribed -= EventUnSubscribedPreamp;
                    //preamp.EEventVolume -= EventVolume;
                    preamp.EEventMute -= EventMute;
                }
            }
            iMutex.ReleaseMutex();
        }*/

        private void SetCurrentSource(SourceUpnp aSource)
        {
            Graphics g = null;
            ModelSourceMediaRenderer renderer = null;
            ModelVolumeControl volume = null;

            if (aSource != null)
            {
                renderer = new ModelSourceMediaRendererUpnpAv(aSource);
                volume = new ModelVolumeControlUpnpAv(aSource);

                renderer.EventControlInitialised += EventSubscribed;
                renderer.Open();

                volume.EventInitialised += EventSubscribedPreamp;
                volume.Open();
            }

            if (iMediaRenderer != null)
            {
                g = iMetadataPanel.CreateGraphics();
                iTrackMutex.WaitOne();
                
                iTrack = null;
                iCoverArtPictureBox.Image = null;
                DrawMetadata(g);
                
                iTrackMutex.ReleaseMutex();
                g.Dispose();

                iMediaRenderer.Close();
                iMediaRenderer.EventControlInitialised -= EventSubscribed;
                iMediaRenderer.EventTransportStateChanged -= EventTransportState;
            }

            if (iVolumeControl != null)
            {
                iVolumeControl.Close();
                iVolumeControl.EventInitialised -= EventSubscribedPreamp;
                iVolumeControl.EventMuteStateChanged -= EventMute;
                //iVolumeControl.EventVolumeChanged -= EventVolume;
            }

            iMediaRenderer = renderer;
            iVolumeControl = volume;

            g = iRenderersPanel.CreateGraphics();
            DrawCurrentSource(g, aSource);
            g.Dispose();
        }

        private void SelectNextSource()
        {
            iMutex.WaitOne();
            int currentIndex = -1;
            for (int i = 0; i < iRenderers.Count; ++i)
            {
                if (iRenderers[i] == iMediaRenderer.Source)
                {
                    currentIndex = i;
                    break;
                }
            }
            if (currentIndex > -1)
            {
                currentIndex++;
                currentIndex = currentIndex % iRenderers.Count;
                SetCurrentSource(iRenderers[currentIndex]);
            }
            else
            {
                SetCurrentSource(null);
            }
            iMutex.ReleaseMutex();
        }

        private void EventSubscribed(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if(sender == iMediaRenderer)
            {
                iMediaRenderer.EventTransportStateChanged += EventTransportState;
            }
            iMutex.ReleaseMutex();
            
            DrawVolume();
        }

        private void EventSubscribedPreamp(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (sender == iVolumeControl)
            {
                iVolume = (int)iVolumeControl.Volume;
            }
            iMutex.ReleaseMutex();

            DrawVolume();
        }

        private void EventTransportState(object sender, EventArgs e)
        {
            ModelSourceMediaRenderer renderer = sender as ModelSourceMediaRenderer;

            Console.WriteLine("TransportState=" + renderer.TransportState + ", iIgnoreStopEvent=" + iIgnoreStopEvent);

            if (renderer.TransportState == ModelSourceMediaRenderer.ETransportState.eStopped)
            {
                if (!iIgnoreStopEvent)
                {
                    Skip();
                }
            }
            
            DrawVolume();
            
            iIgnoreStopEvent = false;
        }

        private void EventVolume(object sender, EventArgs e)
        {
            ModelVolumeControl preamp = sender as ModelVolumeControl;
            iVolume = (int)preamp.Volume;
            DrawVolume();
        }

        private void EventMute(object sender, EventArgs e)
        {
            ModelVolumeControl preamp = sender as ModelVolumeControl;
            iMute = preamp.Mute;
            DrawVolume();
        }

        private void SetMetadata(LastFmTrack aTrack) {
            iTrackMutex.WaitOne();
            iTrack = aTrack;
            if (aTrack != null && aTrack.Image != "")
            {
                try
                {
                    iCoverArtPictureBox.Load(aTrack.Image);                    
                }
                catch (System.Net.WebException)
                {
                    iCoverArtPictureBox.Image = null;
                }
                iFormCoverArt.SetCoverArt(aTrack.Image);
            }
            else
            {
                iCoverArtPictureBox.Image = null;
            }
            Graphics g = iMetadataPanel.CreateGraphics();
            DrawMetadata(g);
            g.Dispose();
            iTrackMutex.ReleaseMutex();
            
            iMutex.WaitOne();
            if (iMediaRenderer != null && aTrack != null)
            {
                musicTrack track = new musicTrack();
                track.Title = aTrack.Title;
                track.Creator = "Last.fm";

                artist artist = new artist();
                artist.Artist = aTrack.Artist;
                artist.Role = "performer";
                track.Artist.Add(artist);
                track.Album.Add(aTrack.Album);
                track.AlbumArtUri = aTrack.Image;
                track.Restricted = true;

                Time time = new Time(aTrack.Duration);
                resource resource = new resource();
                resource.Duration = time.ToString();
                resource.ProtocolInfo = "http-get:*:audio/mpeg:*";
                resource.Uri = aTrack.Url;

                track.Res.Add(resource);

                List<upnpObject> result = new List<upnpObject>();
                result.Add(track);
                int index = (int)iMediaRenderer.PlaylistTrackCount;
                iMediaRenderer.PlaylistInsert(index, result);

                Console.WriteLine("SetMetadata: TransportState=" + iMediaRenderer.TransportState);
                if (iMediaRenderer.TransportState != ModelSourceMediaRenderer.ETransportState.eStopped)
                {
                    iIgnoreStopEvent = true;
                }
            }
            iMutex.ReleaseMutex();
        }

        private void MuteOrPause()
        {
            iMutex.WaitOne();
            if (iMediaRenderer != null)
            {
                if (iVolumeControl != null && (iMediaRenderer.TransportState == ModelSourceMediaRenderer.ETransportState.ePlaying))
                {
                    iMute = !iMute;
                    iVolumeControl.SetMute(iMute);//!preamp.Mute;
                }
                else
                {   // wont get here with Linn MRs as we always expose volume control even when it doesnt work!
                    if (iMediaRenderer.TransportState == ModelSourceMediaRenderer.ETransportState.ePlaying)
                    {
                        iIgnoreStopEvent = true;
                        iMediaRenderer.Pause();
                    }
                    else
                    {
                        if (iTrack != null)
                        {
                            iMediaRenderer.Play();
                        }
                    }
                }
                DrawVolume();
            }
            iMutex.ReleaseMutex();
        }

        private void IncrementVolume()
        {
            iMutex.WaitOne();
            if (iVolume < 100)
            {
                iVolume++;
                if (iVolumeControl != null)
                {
                    iVolumeControl.SetVolume((uint)iVolume);//(uint)(iVolume + 1);
                }
                DrawVolume();
            }
            iMutex.ReleaseMutex();
        }

        private void DecrementVolume()
        {
            iMutex.WaitOne();
            if (iVolume > 0)
            {
                iVolume--;
                if (iVolumeControl != null)
                {
                    iVolumeControl.SetVolume((uint)iVolume);//(uint)(iVolume - 1);
                }
                DrawVolume();
            }
            iMutex.ReleaseMutex();
        }

        private void iUsernameTextBox_TextChanged(object sender, EventArgs e)
        {
	        iUsername = iUsernameTextBox.Text;
            iLoginRequired = true;
            if (iLastError != "")
            {
                iLastError = "";
                SetMetadata(null);
            }
        }

        private void iPasswordTextBox_TextChanged(object sender, EventArgs e)
        {
	        iPassword = iPasswordTextBox.Text;
            iLoginRequired = true;
            if (iLastError != "")
            {
                iLastError = "";
                SetMetadata(null);
            }
        }

        private void iSearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
	        if(e.KeyChar == (char)Keys.Enter && iMediaRenderer != null) {
                if (iLoginRequired)
                {
                    iLastError = "";
                    iLastFmService = new LastFmService();
                    iLastFmService.HandshakeCompleted += HandshakeCompleted;
                    iLastFmService.ChangeStationCompleted += ChangeStationCompleted;
                    iLastFmService.PlaylistCompleted += PlaylistCompleted;
                    iLastFmService.CommandCompleted += CommandCompleted;
                    iLastFmService.Handshake(iUsername, iPassword);
                }
                else
                {
                    ChangeStation();
                }
	        }
        }

        private void HandshakeCompleted(object aSender, HandshakeCompletedEventArgs aArgs)
        {
            try
            {
                if (aArgs.Result == "OK")
                {
                    ChangeStation();
                }
                else
                {
                    iLastError = aArgs.Result;
                    SetMetadata(null);
                }
            }
            catch (WebException e)
            {
                iLastError = e.Message;
                SetMetadata(null);
            }
        }

        private void iSkipButton_Click(object sender, EventArgs e)
        {
            Skip();
        }

        private void Skip()
        {
            if (iLastFmService != null)
            {
                iLastError = "";
                iLastFmService.Skip();
            }
        }

        private void CommandCompleted(object aSender, CommandCompletedEventArgs aArgs)
        {
            try 
            {
                if (aArgs.Result == "OK")
                {
                    if (aArgs.Command == "skip")
                    {
                        iTrackMutex.WaitOne();
                        iTrackIndex++;
                        if (iTrackIndex == iPlaylist.Count || iPlaylist.Count == 0)
                        {
                            Playlist();
                        }
                        else
                        {
                            iTrack = iPlaylist[iTrackIndex];
                            SetMetadata(iTrack);
                        }
                        iTrackMutex.ReleaseMutex();
                    }
                }
                else
                {
                    iLastError = aArgs.Result;
                    SetMetadata(null);
                }
            }
            catch (WebException e)
            {
                iLastError = e.Message;
                SetMetadata(null);
            }
        }

        private void ChangeStation()
        {
            if (iLastFmService != null)
            {
                iLastError = "";
                iLastFmService.ChangeStation("lastfm://artist/" + iSearchTextBox.Text + "/similarartists");
            }
        }

        private void ChangeStationCompleted(object aSender, ChangeStationCompletedEventArgs aArgs)
        {
            try
            {
                if (aArgs.Result == "OK")
                {
                    Playlist();
                }
                else
                {
                    iLastError = aArgs.Result;
                    SetMetadata(null);
                }
            }
            catch (WebException e)
            {
                iLastError = e.Message;
                SetMetadata(null);
            }
        }

        private void Playlist()
        {
            if (iLastFmService != null)
            {
                iTrackMutex.WaitOne();
                iTrackIndex = 0;
                iLastError = "";
                iTrackMutex.ReleaseMutex();
                iLastFmService.Playlist();
            }
        }

        private void PlaylistCompleted(object aSender, PlaylistCompletedEventArgs aArgs)
        {
            iTrackMutex.WaitOne();
            try
            {
                iPlaylist = aArgs.Playlist;
                Console.WriteLine("iPlaylist.Count=" + iPlaylist.Count);
                if (iPlaylist.Count > 0)
                {
                    SetMetadata(iPlaylist[iTrackIndex]);
                }
                else
                {
                    SetMetadata(null);
                }
            }
            catch (WebException e)
            {
                iLastError = e.Message;
                SetMetadata(null);
            }
            iTrackMutex.ReleaseMutex();
        }

        //
        //  Rendering routines for Volume Panel, Metadata Panel, Renderer Panel
        //

        private void iMetadataPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            DrawMetadata(g);
        }

        private void DrawMetadata(Graphics aGraphics)
        {
            iTrackMutex.WaitOne();
            DrawMetadata(aGraphics, iTrack == null ? "No track playing" : iTrack.Title, iTrack == null ? iLastError : iTrack.Album + " - " + iTrack.Artist);
            iTrackMutex.ReleaseMutex();
        }

        private void DrawMetadata(Graphics aGraphics, string aTitle, string aAlbumArtist)
        {
            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;
            
            Rectangle rect = new Rectangle(0, 9, iMetadataPanel.ClientSize.Width, 34);
            aGraphics.Clip = new Region(rect);
            aGraphics.Clear(Color.FromArgb(255, 208, 31, 60));
            
            Font font = new Font("Arial", 12, FontStyle.Bold);
            aGraphics.DrawString(aTitle, font, Brushes.White, rect, format);

            rect = new Rectangle(0, 43, iMetadataPanel.ClientSize.Width, 34);
            aGraphics.Clip = new Region(rect);
            aGraphics.Clear(Color.FromArgb(255, 208, 31, 64));
            aGraphics.DrawString(aAlbumArtist, new Font("Arial", 12, FontStyle.Bold), Brushes.White, rect, format);
        }

        private void iRenderersPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            iMutex.WaitOne();
            if (iMediaRenderer != null)
            {
                DrawCurrentSource(g, iMediaRenderer.Source);
            }
            else
            {
                DrawCurrentSource(g, null);
            }
            iMutex.ReleaseMutex();
        }

        private void DrawCurrentSource(Graphics aGraphics, Source aSource)
        {
            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;
            Rectangle rect = new Rectangle(0, 12, iRenderersPanel.ClientSize.Width, 32);
            aGraphics.Clip = new Region(rect);
            aGraphics.Clear(Color.FromArgb(255, 208, 31, 60));
            aGraphics.DrawString(aSource == null ? "No renderers found" : aSource.Room.Name + ":" + aSource.Name, new Font("Arial", 12, FontStyle.Bold), Brushes.White, rect, format);
        }

        private void iVolumePanel_OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle rect = new Rectangle(1, 1, 100, 100);
            g.FillEllipse(Brushes.White, rect);
            g.DrawEllipse(iPen, rect);

            rect = new Rectangle(26, 26, 50, 50);
            g.FillEllipse(iBrush, rect);
            g.DrawEllipse(iPen, rect);

            DrawKnob(g);
            DrawVolume(g);
        }

        private void DrawKnob(Graphics aGraphics)
        {
            Rectangle rect = new Rectangle(iKnobPos.X, iKnobPos.Y, 10, 10);
            aGraphics.Clip = new Region(new Rectangle(iKnobPos.X - 1, iKnobPos.Y - 1, 14, 14));
            aGraphics.FillEllipse(iBrush, rect);
            aGraphics.DrawEllipse(iKnobPen, rect);
        }

        private void DrawVolume()
        {
            Graphics g = iVolumePanel.CreateGraphics();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            DrawVolume(g);
            g.Dispose();
        }

        private void DrawVolume(Graphics aGraphics)
        {
            Rectangle rect = new Rectangle(32, 39, 39, 26);
            aGraphics.Clip = new Region(rect);
            aGraphics.Clear(Color.FromArgb(255, 208, 31, 60));
            iMutex.WaitOne();
            if (iMediaRenderer != null && iMediaRenderer.TransportState == ModelSourceMediaRenderer.ETransportState.ePlaying)
            {
                if (iVolumeControl != null)
                {
                    iMutex.ReleaseMutex();

                    if (iMute)
                    {
                        aGraphics.DrawImage(global::SneakyLastFm.Properties.Resources.audio_volume_muted, 42, 38, 22, 26);
                    }
                    else
                    {
                        StringFormat format = new StringFormat();
                        format.LineAlignment = StringAlignment.Center;
                        format.Alignment = StringAlignment.Center;
                        aGraphics.DrawString(iVolume.ToString(), new Font("Arial", 14, FontStyle.Bold), Brushes.White, rect, format);
                    }
                }
            }
            else if (iMediaRenderer != null && iMediaRenderer.TransportState == ModelSourceMediaRenderer.ETransportState.ePaused)
            {
                iMutex.ReleaseMutex();

                rect = new Rectangle(40, 41, 8, 20);
                aGraphics.FillRectangle(Brushes.White, rect);
                rect = new Rectangle(55, 41, 8, 20);
                aGraphics.FillRectangle(Brushes.White, rect);
            }
            else
            {
                iMutex.ReleaseMutex();

                rect = new Rectangle(42, 41, 20, 20);
                aGraphics.FillRectangle(Brushes.White, rect);
            }
        }

        //
        // Mouse movement for Volume Control, Window movement
        //

        private void iForm_MouseDown(object sender, MouseEventArgs e)
        {
            Capture = true;
            if (e.Clicks == 2)
            {
                ShowLargeCoverArt();
            }
            iLastMousePos = MousePosition;
            MouseMove += iForm_MouseMove;
        }

        private void iForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (Capture)
            {
                MouseMove -= iForm_MouseMove;
                iVolumePanel.MouseMove -= iForm_MouseMove;
                Capture = false;
            }
        }

        private void iForm_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouse = PointToScreen(new Point(e.X, e.Y));
            Point delta = new Point(mouse.X - iLastMousePos.X, mouse.Y - iLastMousePos.Y);
            iLastMousePos = mouse;

            Point pos = new Point(Left + delta.X, Top + delta.Y);

            // limit position

            Rectangle work = Screen.PrimaryScreen.Bounds;

            if (pos.X < 0)
            {
                pos.X = 0;
            }

            double maxX = work.Width - Width;

            if (pos.X > maxX)
            {
                pos.X = (int)maxX;
            }

            if (pos.Y < 0)
            {
                pos.Y = 0;
            }

            double maxY = work.Height - Height;

            if (pos.Y > maxY)
            {
                pos.Y = (int)maxY;
            }

            Left = pos.X;
            Top = pos.Y;
        }

        private void iCoverArtPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 2)
            {
                if (iCoverArtPictureBox.Image != null)
                {
                    ShowLargeCoverArt();
                }
            }
            else
            {
                Capture = true;
                iLastMousePos = MousePosition;
                MouseMove += iForm_MouseMove;
            }
        }

        private void ShowLargeCoverArt()
        {
            float x = 30;//(iCoverArtPictureBox.Width / 2.0f) - (iFormCoverArt.Width / 2.0f);
            float y = 0;//(iCoverArtPictureBox.Height / 2.0f) - (iFormCoverArt.Height / 2.0f);
            iFormCoverArt.Show();
            iFormCoverArt.Location = PointToScreen(new Point((int)x, (int)y));
        }

        private void iMetadataPanel_MouseDown(object sender, MouseEventArgs e)
        {
            Rectangle rect = new Rectangle(3, 11, iMetadataPanel.ClientSize.Width, 40);
            if (rect.Contains(new Point(e.X, e.Y)))
            {
                Skip();
            }
            else
            {
                Capture = true;
                iLastMousePos = MousePosition;
                MouseMove += iForm_MouseMove;
            }
        }

        private void iVolumePanel_MouseDown(object sender, MouseEventArgs e)
        {
            float distSquared = ((50 - e.X) * (50 - e.X)) + ((50 - e.Y) * (50 - e.Y));
            if (distSquared < 50 * 50) // inside outer circle
            {
                if (distSquared < 25 * 25) // inside inner circle
                {
                    MuteOrPause();
                }
                else
                {
                    iVolumePanel.Capture = true;
                    iVolumePanel.MouseMove += iVolumePanel_MouseMove;
                    UpdateAngle();
                }
            }
            else
            {
                Capture = true;
                iLastMousePos = MousePosition;
                MouseMove += iForm_MouseMove;
            }
        }

        private void iVolumePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (iVolumePanel.Capture)
            {
                iVolumePanel.MouseMove -= iVolumePanel_MouseMove;
                iVolumePanel.Capture = false;
            }
        }

        private void iVolumePanel_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateAngle();
        }

        private void UpdateAngle()
        {
            Point mouse = iVolumePanel.PointToClient(MousePosition);

            double angle = ((Math.Atan2(mouse.Y - 50, mouse.X - 50) * 180) / Math.PI) + 270;

            if (angle > 360)
            {
                angle -= 360;
            }

            iAngle = angle;

            double delta = angle - iLastAngle;

            // normalise angle delta

            if (delta > 180)
            {
                delta -= 360;
            }

            if (delta < -180)
            {
                delta += 360;
            }

            // update volume according to angle delta

            if (delta > 30)
            {
                IncrementVolume();
                iLastAngle = angle;
            }

            if (delta < -30)
            {
                DecrementVolume();
                iLastAngle = angle;
            }

            Matrix mx = new Matrix();
            mx.Rotate((float)iAngle);
            Point[] position = new Point[] { new Point(0, 37) };
            mx.TransformPoints(position);

            Graphics g = iVolumePanel.CreateGraphics();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(iKnobPos.X - 2, iKnobPos.Y - 2, 15, 15);
            g.Clip = new Region(rect);
            g.FillRectangle(Brushes.White, rect);

            iKnobPos = position[0];
            iKnobPos.X += 46;
            iKnobPos.Y += 46;

            DrawKnob(g);

            g.Dispose();
        }

        private void iRightSidePanel_MouseDown(object sender, MouseEventArgs e)
        {
            iLastMousePos = MousePosition;
            iRightSidePanel.Capture = true;
            iRightSidePanel.MouseMove += iSidePanel_MouseMove;
        }

        private void iRightSidePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (iRightSidePanel.Capture)
            {
                iRightSidePanel.MouseMove -= iSidePanel_MouseMove;
                iRightSidePanel.Capture = false;
            }
        }

        private void iLeftSidePanel_MouseDown(object sender, MouseEventArgs e)
        {
            iLastMousePos = MousePosition;
            iLeftSidePanel.Capture = true;
            iLeftSidePanel.MouseMove += iSidePanel_MouseMove;
        }

        private void iLeftSidePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (iLeftSidePanel.Capture)
            {
                iLeftSidePanel.MouseMove -= iSidePanel_MouseMove;
                iLeftSidePanel.Capture = false;
            }
        }

        private void iSidePanel_MouseMove(object sender, MouseEventArgs e)
        {
            /*Point mouse = PointToScreen(new Point(e.X, e.Y));
            Point delta = new Point(mouse.X - iLastMousePos.X, mouse.Y - iLastMousePos.Y);
            iLastMousePos = mouse;

            Width = Width + delta.X;
            Console.WriteLine(Width);*/
        }

        private void iRenderersPanel_MouseDown(object sender, MouseEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 13, iRenderersPanel.ClientSize.Width, 30);
            if (rect.Contains(new Point(e.X, e.Y)))
            {
                SelectNextSource();
            }
            else
            {
                Capture = true;
                iLastMousePos = MousePosition;
                MouseMove += iForm_MouseMove;
            }
        }

        private void iUsernameTextBox_Enter(object sender, EventArgs e)
        {
            if (iUsernameTextBox.Text == "Username")
            {
                iUsernameTextBox.Text = "";
            }
        }

        private void iPasswordTextBox_Enter(object sender, EventArgs e)
        {
            if (iPasswordTextBox.Text == "Password")
            {
                iPasswordTextBox.Text = "";
                iPasswordTextBox.UseSystemPasswordChar = true;
            }
        }

        private void iUsernameTextBox_Leave(object sender, EventArgs e)
        {
            iSettings.Username = iUsernameTextBox.Text;
            iSettings.Save();
            if (iUsernameTextBox.Text == "")
            {
                iUsernameTextBox.Text = "Username";
            }
        }

        private void iPasswordTextBox_Leave(object sender, EventArgs e)
        {
            iSettings.Password = iPasswordTextBox.Text;
            iSettings.Save();
            if (iPasswordTextBox.Text == "")
            {
                iPasswordTextBox.Text = "Password";
                iPasswordTextBox.UseSystemPasswordChar = false;
            }
        }

        private void iSearchTextBox_Enter(object sender, EventArgs e)
        {
            if (iSearchTextBox.Text == "Type an artist to start listening")
            {
                iSearchTextBox.Text = "";
            }
        }

        private void iSearchTextBox_Leave(object sender, EventArgs e)
        {
            if (iSearchTextBox.Text == "")
            {
                iSearchTextBox.Text = "Type an artist to start listening";
            }
        }

        private void iLogoPictureBox_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.last.fm");
        }

        private void iForm_Deactivate(object sender, EventArgs e)
        {
            if (Capture)
            {
                MouseMove -= iForm_MouseMove;
                iVolumePanel.MouseMove -= iForm_MouseMove;
                Capture = false;
            }
            if (iVolumePanel.Capture)
            {
                iVolumePanel.MouseMove -= iVolumePanel_MouseMove;
                iVolumePanel.Capture = false;
            }
            if (iLeftSidePanel.Capture)
            {
                iLeftSidePanel.MouseMove -= iSidePanel_MouseMove;
                iLeftSidePanel.Capture = false;
            }
            if (iRightSidePanel.Capture)
            {
                iRightSidePanel.MouseMove -= iSidePanel_MouseMove;
                iRightSidePanel.Capture = false;
            }
        }

        //
        // Handle closing application
        //

        private void iCloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void iForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SetCurrentSource(null);
            if (iLastFmService != null)
            {
                iLastFmService.HandshakeCompleted -= HandshakeCompleted;
            }

            iHouse.Stop();
        }
    }
}
