using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;

using Linn.Kinsky;
using Linn;
using Upnp;

namespace OssKinskyMppLastFm
{
    internal interface IView
    {
        Control Control { get; }
        
        void Open();
        void Close();

        void SetUsernameAndPassword(string aUsername, string aPassword);

        void OnSizeClick();
        void OnViewClick();

        event EventHandler<EventArgsPlay> EventPlayNow;
        event EventHandler<EventArgsPlay> EventPlayNext;
        event EventHandler<EventArgsPlay> EventPlayLater;
    }

    internal partial class ViewWidgetMediaProviderLastFm : UserControl, IView
    {
        private IMediaProviderSupportV7 iSupport;
        private ILastFmService iService;
        private ViewContextMenu iContextMenu;

        private Mutex iMutex;

        private Brush iBrushBackColour;

        private string iUsername;
        private string iPassword;
        private bool iLoginRequired;

        public ViewWidgetMediaProviderLastFm(IMediaProviderSupportV7 aSupport, ILastFmService aService)
        {
            iMutex = new Mutex(false);

            iSupport = aSupport;
            iSupport.ViewSupport.EventSupportChanged += EventSupportChanged;

            iService = aService;

            InitializeComponent();

            SetViewColours();

            iContextMenu = new ViewContextMenu();
            listBox1.ContextMenuStrip = iContextMenu.ContextMenuStrip;

            iService.EventChangeStationCompleted += EventChangeStationCompleted;
            iService.EventCommandCompleted += EventCommandCompleted;
            iService.EventHandshakeCompleted += EventHandshakeCompleted;
            iService.EventPlaylistCompleted += EventPlaylistCompleted;

            iLoginRequired = true;
        }

        public Control Control
        {
            get
            {
                return this;
            }
        }

        public void Open()
        {
            iContextMenu.EventPlayNowClicked += EventContextMenu_PlayNow;
            iContextMenu.EventPlayNextClicked += EventContextMenu_PlayNext;
            iContextMenu.EventPlayLaterClicked += EventContextMenu_PlayLater;

            iSupport.PlaylistSupport.EventIsOpenChanged += EventPlaylistSupportIsOpenChanged;
            iSupport.PlaylistSupport.EventIsInsertingChanged += EventPlaylistSupportIsInsertingChanged;
        }

        public void Close()
        {
            iContextMenu.EventPlayNowClicked -= EventContextMenu_PlayNow;
            iContextMenu.EventPlayNextClicked -= EventContextMenu_PlayNext;
            iContextMenu.EventPlayLaterClicked -= EventContextMenu_PlayLater;

            iSupport.PlaylistSupport.EventIsOpenChanged -= EventPlaylistSupportIsOpenChanged;
            iSupport.PlaylistSupport.EventIsInsertingChanged -= EventPlaylistSupportIsInsertingChanged;
        }

        public void SetUsernameAndPassword(string aUsername, string aPassword)
        {
            if (iUsername != aUsername)
            {
                iLoginRequired = true;
                iUsername = aUsername;
            }
            if (iPassword != aPassword)
            {
                iLoginRequired = true;
                iPassword = aPassword;
            }

            if (iUsername == null || iUsername == string.Empty)
            {
                iLoginRequired = false;
            }

            if (iLoginRequired)
            {
                More.Enabled = false;
            }
        }

        public void OnSizeClick()
        {
        }

        public void OnViewClick()
        {
        }

        public event EventHandler<EventArgsPlay> EventPlayNow;
        public event EventHandler<EventArgsPlay> EventPlayNext;
        public event EventHandler<EventArgsPlay> EventPlayLater;

        private void EventContextMenu_PlayNow(object sender, EventArgs e)
        {
            if (EventPlayNow != null)
            {
                EventPlayNow(this, new EventArgsPlay(new MediaRetrieverNoRetrieve(SelectedUpnpObjects())));
            }
        }

        private void EventContextMenu_PlayNext(object sender, EventArgs e)
        {
            if (EventPlayNext != null)
            {
                EventPlayNext(this, new EventArgsPlay(new MediaRetrieverNoRetrieve(SelectedUpnpObjects())));
            }
        }

        private void EventContextMenu_PlayLater(object sender, EventArgs e)
        {
            if (EventPlayLater != null)
            {
                EventPlayLater(this, new EventArgsPlay(new MediaRetrieverNoRetrieve(SelectedUpnpObjects())));
            }
        }

        private void EventPlaylistSupportIsOpenChanged(object sender, EventArgs e)
        {
            if (iSupport.PlaylistSupport.IsOpen())
            {
                listBox1.BeginInvoke((MethodInvoker)delegate()
                {
                    if (listBox1.SelectedIndices.Count > 0)
                    {
                        iContextMenu.SetPlayEnabled(!iSupport.PlaylistSupport.IsInserting());
                    }
                });
            }
            else
            {
                iContextMenu.SetPlayEnabled(false);
            }
        }

        private void EventPlaylistSupportIsInsertingChanged(object sender, EventArgs e)
        {
            listBox1.BeginInvoke((MethodInvoker)delegate()
            {
                if (listBox1.SelectedIndices.Count > 0)
                {
                    iContextMenu.SetPlayEnabled(iSupport.PlaylistSupport.IsOpen() && !iSupport.PlaylistSupport.IsInserting());
                }
            });
        }

        private IList<upnpObject> SelectedUpnpObjects()
        {
            List<upnpObject> items = new List<upnpObject>();
            for (int i = 0; i < listBox1.SelectedIndices.Count; ++i)
            {
                ITrack track = listBox1.Items[listBox1.SelectedIndices[i]] as ITrack;
                items.Add(UpnpObject(track));
            }
            return items;
        }

        private upnpObject UpnpObject(ITrack aTrack)
        {
            musicTrack track = new musicTrack();
            
            artist artist = new artist();
            artist.Artist = aTrack.Artist;
            artist.Role = "Performer";
            track.Artist.Add(artist);

            resource resource = new resource();
            Time time = new Time(aTrack.Duration);
            resource.Duration = time.ToString();
            resource.Uri = aTrack.Uri;
            track.Res.Add(resource);

            track.Album.Add(aTrack.Album);
            track.AlbumArtUri.Add(aTrack.ArtworkUri);

            track.Title = aTrack.Title;

            return track;
        }

        private void EventChangeStationCompleted(object sender, EventArgsChangeStationCompleted e)
        {
            if (e.Result == "OK")
            {
                iService.Playlist();
            }
            else
            {
                More.BeginInvoke((MethodInvoker)delegate()
                {
                    More.Enabled = false;
                });
            }
        }

        private void EventCommandCompleted(object sender, EventArgsCommandCompleted e)
        {
        }

        private void EventHandshakeCompleted(object sender, EventArgsHandshakeCompleted e)
        {
            if (e.Result == "OK")
            {
                More.BeginInvoke((MethodInvoker)delegate()
                {
                    More.Enabled = true;
                });
                UserLog.WriteLine(DateTime.Now + ": last.fm login as " + iUsername + " successful");
                ChangeStation();
            }
            else
            {
                More.BeginInvoke((MethodInvoker)delegate()
                {
                    More.Enabled = false;
                });
                iLoginRequired = true;
                UserLog.WriteLine(DateTime.Now + ": last.fm login as " + iUsername + " unsuccessful (" + e.Result + ")");
            }
        }

        private void EventPlaylistCompleted(object sender, EventArgsPlaylistCompleted e)
        {
            if (e.Result == "OK")
            {
                listBox1.BeginInvoke((MethodInvoker)delegate()
                {
                    iMutex.WaitOne();
                    listBox1.BeginUpdate();

                    foreach (ITrack t in e.Playlist)
                    {
                        t.EventArtworkDownloaded += EventArtworkDownloaded;
                        listBox1.Items.Add(t);
                        Console.WriteLine(t.Title + ", " + t.Artist + ", " + t.Album + ", " + t.Uri + ", " + t.Duration);
                    }

                    listBox1.EndUpdate();
                    iMutex.ReleaseMutex();
                });
            }
        }

        private void EventArtworkDownloaded(object sender, EventArgs e)
        {
            listBox1.Invalidate();
        }

        private void ChangeStation()
        {
            iService.ChangeStation("lastfm://artist/" + Channel.Text + "/similarartists");
        }

        private void Channel_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (iLoginRequired)
                {
                    iLoginRequired = false;
                    iService.Handshake(iUsername, iPassword);
                }
                else
                {
                    ChangeStation();
                }
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            //{
            //    e.Graphics.FillRectangle(iBrushHighlightBackColour, e.Bounds);
            //}
            //else
            //{
                e.Graphics.FillRectangle(iBrushBackColour, e.Bounds);
            //}

            ITrack track = null;
            iMutex.WaitOne();
            if (e.Index > -1 && e.Index < listBox1.Items.Count)
            {
                track = listBox1.Items[e.Index] as ITrack;
            }
            iMutex.ReleaseMutex();

            if (track != null)
            {
                if (track.Artwork != null)
                {
                    e.Graphics.DrawImage(track.Artwork, e.Bounds.Left, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);
                }

                TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
                int y = e.Bounds.Top;

                Size size = TextRenderer.MeasureText(track.Title, Font);
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    TextRenderer.DrawText(e.Graphics, track.Title, Font, new Rectangle(e.Bounds.Left + e.Bounds.Height + 4, y, e.Bounds.Width - 104, size.Height), iSupport.ViewSupport.HighlightBackColour, flags);
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, track.Title, Font, new Rectangle(e.Bounds.Left + e.Bounds.Height + 4, y, e.Bounds.Width - 104, size.Height), listBox1.ForeColor, flags);
                }
                y += size.Height;

                size = TextRenderer.MeasureText(track.Artist, Font);
                TextRenderer.DrawText(e.Graphics, track.Artist, Font, new Rectangle(e.Bounds.Left + e.Bounds.Height + 4, y, e.Bounds.Width - 104, size.Height), listBox1.ForeColor, flags);
                y += size.Height;

                size = TextRenderer.MeasureText(track.Album, Font);
                TextRenderer.DrawText(e.Graphics, track.Album, Font, new Rectangle(e.Bounds.Left + e.Bounds.Height + 4, y, e.Bounds.Width - 104, size.Height), listBox1.ForeColor, flags);
                y += size.Height;

                Time time = new Time(track.Duration);
                size = TextRenderer.MeasureText(time.ToPrettyString(), Font);
                TextRenderer.DrawText(e.Graphics, time.ToPrettyString(), Font, new Rectangle(e.Bounds.Left + e.Bounds.Height + 4, y, e.Bounds.Width - 104, size.Height), listBox1.ForeColor, flags);
            }
        }

        private void More_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            if (iLoginRequired)
            {
                iLoginRequired = false;
                iService.Handshake(iUsername, iPassword);
            }
            else
            {
                iService.Playlist();
            }
        }

        private void EventSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndices.Count > 0)
            {
                iContextMenu.SetPlayEnabled(iSupport.PlaylistSupport.IsOpen() && !iSupport.PlaylistSupport.IsInserting());
            }
            else
            {
                iContextMenu.SetPlayEnabled(false);
            }
        }

        private void Channel_Enter(object sender, EventArgs e)
        {
            if (Channel.Text == "Enter an artist name to start listening")
            {
                Channel.Text = "";
            }
        }

        private void Channel_Leave(object sender, EventArgs e)
        {
            if (Channel.Text == "")
            {
                Channel.Text = "Enter an artist name to start listening";
            }
        }

        private void EventSupportChanged(object sender, EventArgs e)
        {
            SetViewColours();
        }

        private void SetViewColours()
        {
            BackColor = iSupport.ViewSupport.BackColour;
            ForeColor = iSupport.ViewSupport.ForeColour;
            Font = iSupport.ViewSupport.FontMedium;

            Channel.BackColor = iSupport.ViewSupport.BackColour;
            Channel.ForeColor = iSupport.ViewSupport.ForeColour;
            Channel.Font = iSupport.ViewSupport.FontMedium;

            More.BackColor = iSupport.ViewSupport.BackColour;
            More.ForeColor = iSupport.ViewSupport.ForeColour;
            More.Font = iSupport.ViewSupport.FontSmall;

            listBox1.BackColor = iSupport.ViewSupport.BackColour;
            listBox1.ForeColor = iSupport.ViewSupport.ForeColour;
            listBox1.Font = iSupport.ViewSupport.FontMedium;

            if (iBrushBackColour != null)
            {
                iBrushBackColour.Dispose();
                iBrushBackColour = null;
            }
            iBrushBackColour = new SolidBrush(iSupport.ViewSupport.BackColour);
        }
    }
} // OssKinskyMppLastFm
