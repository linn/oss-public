using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net;
using System.IO;

using Linn;
using Linn.Kinsky;
using Linn.Topology;
using Upnp;

namespace KinskyDesktop
{
    internal partial class FormStatus : FormThemed, IViewWidgetTrack, IViewWidgetPlaylist, IViewWidgetPlaylistRadio, IViewWidgetPlaylistReceiver, IViewWidgetMediaTime
    {
        private Mutex iMutex;
        private Linn.Timer iTimer;

        private IViewSupport iViewSupport;

        private Image iArtwork;
        private Image iReflectedArtwork;
        private Uri iArtworkUri;
        private string iTitle;
        private string iArtist;
        private string iAlbum;

        private Font iMicroFont;
        private Font iSmallFont;
        private Font iMediumFont;
        private Font iLargeFont;

        private bool iPlaylistOpen;
        private bool iRadioOpen;
        private IList<MrItem> iPlaylist;
        private MrItem iTrack;

        private IList<MrItem> iPresets;
        private int iPresetIndex;

        private IList<ModelSender> iSenders;
        private Channel iChannel;

        private int kItemWidth;
        private int iStartIndex;
        private int iOffset;
        private int iIndex;

        private int iTargetIndex;
        private float iPixelsPerMs;
        private Ticker iTicker;
        private float iLastTime;
        private Linn.Timer iScrollTimer;
        private Point iMousePosition;
        private bool iMouseDown;
        private float iDelta;

        private uint iBitrate;
        private uint iBitDepth;
        private float iSampleRate;
        private string iCodec;
        private bool iLossless;

        private ETransportState iTransportState;
        private uint iSeconds;
        private uint iDuration;

        private Image kImageNoArtwork;
        private Image kImageReflectedNoArtwork;
        private IArtworkCache iArtworkCache;

        public FormStatus(Form aForm, IArtworkCache aArtworkCache, IViewSupport aViewSupport)
        {
            iViewSupport = aViewSupport;

            iMutex = new Mutex(false);

            iPlaylist = new List<MrItem>();
            iTrack = null;

            iPresets = new List<MrItem>();
            iPresetIndex = -1;

            iStartIndex = 0;
            iOffset = 0;
            iIndex = -1;

            iTicker = new Ticker();
            iScrollTimer = new Linn.Timer();
            iScrollTimer.AutoReset = false;
            iScrollTimer.Interval = 20;
            iScrollTimer.Elapsed += ScrollTimerElapsed;

            iTimer = new Linn.Timer();
            iTimer.AutoReset = false;
            iTimer.Interval = 500;
            iTimer.Elapsed += TimerElapsed;

            iArtworkCache = aArtworkCache;
            iArtworkCache.EventUpdated += EventUpdated;

            kImageNoArtwork = Linn.Kinsky.Properties.Resources.NoAlbumArt;
            kImageReflectedNoArtwork = iArtworkCache.ReflectImage(kImageNoArtwork);

            iArtwork = kImageNoArtwork;
            iReflectedArtwork = kImageReflectedNoArtwork;

            iMicroFont = new Font(iViewSupport.FontSmall.FontFamily, ClientSize.Height / 50.0f, FontStyle.Regular);
            iSmallFont = new Font(iViewSupport.FontSmall.FontFamily, ClientSize.Height / 30.0f, FontStyle.Regular);
            iMediumFont = new Font(iViewSupport.FontMedium.FontFamily, ClientSize.Height / 25.0f, FontStyle.Regular);
            iLargeFont = new Font(iViewSupport.FontLarge.FontFamily, ClientSize.Height / 15.0f, FontStyle.Bold);

            InitializeComponent();

            kItemWidth = ClientSize.Width;
        }

        void IViewWidgetTrack.Open() { }
        void IViewWidgetPlaylist.Open()
        {
            iMutex.WaitOne();
            iPlaylistOpen = true;
            iMutex.ReleaseMutex();
        }
        void IViewWidgetPlaylistRadio.Open()
        {
            iMutex.WaitOne();
            iRadioOpen = true;
            iMutex.ReleaseMutex();
        }
        void IViewWidgetPlaylistReceiver.Open()
        {
            iMutex.WaitOne();
            iRadioOpen = true;
            iMutex.ReleaseMutex();
        }
        void IViewWidgetMediaTime.Open() { }

        void IViewWidgetTrack.Close() { }
        void IViewWidgetPlaylist.Close()
        {
            iMutex.WaitOne();
            iPlaylist = new List<MrItem>();
            iIndex = -1;
            iPlaylistOpen = false;
            iMutex.ReleaseMutex();
        }
        void IViewWidgetPlaylistRadio.Close()
        {
            iMutex.WaitOne();
            iPresets = new List<MrItem>();
            iPresetIndex = -1;
            iRadioOpen = false;
            iMutex.ReleaseMutex();
        }
        void IViewWidgetPlaylistReceiver.Close()
        {
            iMutex.WaitOne();
            iPresets = new List<MrItem>();
            iPresetIndex = -1;
            iRadioOpen = false;
            iMutex.ReleaseMutex();
        }
        void IViewWidgetMediaTime.Close() { }

        public void Initialised()
        {
            Invalidate();
        }
		
		public void SetItem(upnpObject aObject)
        {
            iMutex.WaitOne();

            if (aObject != null)
            {
                iTitle = DidlLiteAdapter.Title(aObject);
                iArtist = DidlLiteAdapter.Artist(aObject);
                iAlbum = DidlLiteAdapter.Album(aObject);

                iArtworkUri = DidlLiteAdapter.ArtworkUri(aObject);

                if (iArtworkUri == null)
                {
                    SetNoArtwork();
                }
                else
                {
                    DownloadArtwork(iArtworkUri);
                }
            }
            else
            {
                iTitle = string.Empty;
                iArtist = string.Empty;
                iAlbum = string.Empty;

                iCodec = string.Empty;
                iBitDepth = 0;
                iSampleRate = 0;
                iBitrate = 0;
                iLossless = false;

                iArtworkUri = null;
                SetNoArtwork();
            }
            
            iMutex.ReleaseMutex();
        }

        private void DownloadArtwork(Uri aAlbumArtUri)
        {
            try
            {
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(aAlbumArtUri);
                wreq.Proxy = new WebProxy();
                wreq.KeepAlive = false;
                wreq.Pipelined = false;
                wreq.ContentLength = 0;
                wreq.AllowWriteStreamBuffering = true;
                WebRequestPool.QueueJob(new JobGetResponse(GetResponseCallback, wreq));
            }
            catch (Exception e)
            {
                Console.WriteLine("FormStatus.DownloadArtwork: " + e.Message);
            }
        }

        private void GetResponseCallback(object aResult)
        {
            HttpWebRequest wreq = aResult as HttpWebRequest;
            HttpWebResponse wresp = null;
            Stream stream = null;

            try
            {
                wresp = (HttpWebResponse)wreq.GetResponse();
                stream = wresp.GetResponseStream();

                if (stream != null)
                {

                    Bitmap image = new Bitmap(stream);

                    iMutex.WaitOne();

                    if (iArtworkUri != null && wreq.RequestUri.AbsoluteUri == iArtworkUri.AbsoluteUri)
                    {
                        iArtwork = image;
                        iReflectedArtwork = iArtworkCache.ReflectImage(image);

                        iMutex.ReleaseMutex();

                        Update();
                    }
                    else
                    {
                        SetNoArtwork();

                        iMutex.ReleaseMutex();

                        image.Dispose();

                        Update();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("FormStatus.GetResponseCallback: " + e.Message);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }

                if (wresp != null)
                {
                    wresp.Close();
                }
            }
        }

        private void SetNoArtwork()
        {
            if (iArtwork != null && iArtwork != kImageNoArtwork)
            {
                iArtwork.Dispose();
            }
            iArtwork = kImageNoArtwork;

            if (iReflectedArtwork != null && iReflectedArtwork != kImageReflectedNoArtwork)
            {
                iReflectedArtwork.Dispose();
            }
            iReflectedArtwork = kImageReflectedNoArtwork;
        }

        public void SetMetatext(upnpObject aObject)
        {
            iMutex.WaitOne();

            if (aObject != null)
            {
                iAlbum = DidlLiteAdapter.Title(aObject);
                iArtist = string.Empty;
            }

            iMutex.ReleaseMutex();
        }

        public void SetBitrate(uint aBitrate)
        {
            iMutex.WaitOne();
            iBitrate = aBitrate;
            iMutex.ReleaseMutex();
        }

        public void SetSampleRate(float aSampleRate)
        {
            iMutex.WaitOne();
            iSampleRate = aSampleRate;
            iMutex.ReleaseMutex();
        }

        public void SetBitDepth(uint aBitDepth)
        {
            iMutex.WaitOne();
            iBitDepth = aBitDepth;
            iMutex.ReleaseMutex();
        }

        public void SetCodec(string aCodec)
        {
            iMutex.WaitOne();
            iCodec = aCodec;
            iMutex.ReleaseMutex();
        }

        public void SetLossless(bool aLossless)
        {
            iMutex.WaitOne();
            iLossless = aLossless;
            iMutex.ReleaseMutex();
        }

        public new void Update()
        {
            iTimer.Stop();
            iTimer.Start();
        }

        public void SetPlaylist(IList<MrItem> aPlaylist)
        {
            iMutex.WaitOne();
            iPlaylist = aPlaylist;
            SetTrack(iTrack);
            iMutex.ReleaseMutex();

            Invalidate();
        }

        public void SetTrack(MrItem aTrack)
        {
            iMutex.WaitOne();
            iTrack = aTrack;
            iIndex = -1;
            for (int i = 0; i < iPlaylist.Count; ++i)
            {
                if (iPlaylist[i] == aTrack)
                {
                    iIndex = i;
                    iTargetIndex = i;
                    break;
                }
            }
            if (iIndex > -1)
            {
                iPixelsPerMs = (iOffset - (iTargetIndex * kItemWidth)) / 200.0f;

                if (Math.Abs(iPixelsPerMs) > 0)
                {
                    iTicker.Reset();
                    iLastTime = 0;
                    iScrollTimer.Start();
                }
            }
            iMutex.ReleaseMutex();
        }

        public void Save()
        {
        }

        public void Delete()
        {
        }

        public void SetPresets(IList<MrItem> aPresets)
        {
            iMutex.WaitOne();
            iPresets = aPresets;
            SetPreset(iPresetIndex);
            iMutex.ReleaseMutex();

            Invalidate();
        }

        public void SetChannel(Channel aChannel)
        {
            iMutex.WaitOne();
            
            iChannel = aChannel;

            iTargetIndex = -1;
            if (iSenders != null && iChannel != null)
            {
                for (int i = 0; i < iSenders.Count; ++i)
                {
                    if (iSenders[i].Metadata.Xml == iChannel.DidlLite.Xml)
                    {
                        iTargetIndex = i;
                        break;
                    }
                }
            }

            if (iTargetIndex > -1)
            {
                iPixelsPerMs = (iOffset - (iTargetIndex * kItemWidth)) / 200.0f;

                if (Math.Abs(iPixelsPerMs) > 0)
                {
                    iTicker.Reset();
                    iLastTime = 0;
                    iScrollTimer.Start();
                }
            }
            else
            {
                iOffset = 0;
            }

            iMutex.ReleaseMutex();
        }

        public void SetPreset(int aPresetIndex)
        {
            iMutex.WaitOne();
            iPresetIndex = aPresetIndex;
            iTargetIndex = aPresetIndex;

            if (iPresetIndex > -1)
            {
                iPixelsPerMs = (iOffset - (iTargetIndex * kItemWidth)) / 200.0f;

                if (Math.Abs(iPixelsPerMs) > 0)
                {
                    iTicker.Reset();
                    iLastTime = 0;
                    iScrollTimer.Start();
                }
            }
            else
            {
                iOffset = 0;
            }

            iMutex.ReleaseMutex();
        }

        public void SetSenders(IList<ModelSender> aSenders)
        {
            iMutex.WaitOne();
            iSenders = aSenders;
            SetChannel(iChannel);
            iMutex.ReleaseMutex();

            Invalidate();
        }

        public void SetTransportState(ETransportState aTransportState)
        {
            iMutex.WaitOne();
            iTransportState = aTransportState;
            iMutex.ReleaseMutex();
            Invalidate();
        }

        public void SetAllowSeeking(bool aAllowSeeking)
        {
        }

        public void SetDuration(uint aDuration)
        {
            iMutex.WaitOne();
            iDuration = aDuration;
            iSeconds = 0;
            iMutex.ReleaseMutex();
        }

        public void SetSeconds(uint aSeconds)
        {
            iMutex.WaitOne();
            iSeconds = aSeconds;
            if (iIndex > -1)
            {
                if (iPlaylist[iIndex].DidlLite == iTrack.DidlLite)
                {
                    Invalidate();
                }
            }
            else if(iPlaylist.Count == 0)
            {
                Invalidate();
            }
            iMutex.ReleaseMutex();
        }

        public event EventHandler<EventArgsSeekSeconds> EventSeekSeconds { add { } remove { } }

        public event EventHandler<EventArgsSeekTrack> EventSeekTrack { add { } remove { } }

        public event EventHandler<EventArgsPlaylistInsert> EventPlaylistInsert { add { } remove { } }
        public event EventHandler<EventArgsPlaylistMove> EventPlaylistMove { add { } remove { } }
        public event EventHandler<EventArgsPlaylistDelete> EventPlaylistDelete { add { } remove { } }

        public event EventHandler<EventArgs> EventPlaylistDeleteAll { add { } remove { } }

        public event EventHandler<EventArgsSetPreset> EventSetPreset { add { } remove { } }
        public event EventHandler<EventArgsSetChannel> EventSetChannel { add { } remove { } }

        private void EventUpdated(object sender, EventArgsArtwork e)
        {
            iMutex.WaitOne();

            for (int i = iStartIndex; i < iStartIndex + 2 && i < iPlaylist.Count; ++i)
            {
                upnpObject upnpObject = iPlaylist[i].DidlLite[0];
                Uri uri = DidlLiteAdapter.ArtworkUri(upnpObject);
                if (uri.AbsoluteUri == e.Artwork.Uri.AbsoluteUri)
                {
                    Invalidate();
                }
            }

            for (int i = iStartIndex; i < iStartIndex + 2 && i < iPresets.Count; ++i)
            {
                upnpObject upnpObject = iPresets[i].DidlLite[0];
                Uri uri = DidlLiteAdapter.ArtworkUri(upnpObject);
                if (uri.AbsoluteUri == e.Artwork.Uri.AbsoluteUri)
                {
                    Invalidate();
                }
            }

            iMutex.ReleaseMutex();
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void ScrollTimerElapsed(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            float time = iTicker.MilliSeconds;
            float ms = time - iLastTime;
            iLastTime = time;
            SetOffsetRelative((int)(iPixelsPerMs * ms));

            if (iPixelsPerMs < 0)
            {
                int itemOffset = iTargetIndex * kItemWidth;
                if (iOffset > itemOffset)
                {
                    SetOffsetAbsolute(itemOffset);
                }
            }
            else
            {
                int itemOffset = iTargetIndex * kItemWidth;
                if (iOffset < itemOffset)
                {
                    SetOffsetAbsolute(itemOffset);
                }
            }

            if (iOffset != iTargetIndex * kItemWidth)
            {
                iScrollTimer.Start();
            }
            else
            {
                iPixelsPerMs = 0;
            }

            iMutex.ReleaseMutex();

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();

            bool processed = false;
            if (e.Button == MouseButtons.Left)
            {
                if (ClientRectangle.Contains(e.Location))
                {
                    iMouseDown = true;
                    iMousePosition = PointToClient(Control.MousePosition);
                    iDelta = 0;

                    processed = true;
                }
            }

            if (!processed)
            {
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (iMouseDown)
            {
                Point point = PointToClient(Control.MousePosition);
                int delta = point.X - iMousePosition.X;

                SetOffsetRelative((int)(delta * 1.5f));

                iDelta += (delta * 1.5f);

                iMousePosition = point;
                
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            iMouseDown = false;

            if (Math.Abs(iDelta) > 0)
            {
                if (iDelta > 0)
                {
                    iTargetIndex = iStartIndex;
                }
                else
                {
                    iTargetIndex = (int)Math.Ceiling(iOffset / (float)kItemWidth);
                }

                iPixelsPerMs += (iDelta / Math.Abs(iDelta)) * (ClientSize.Width / 200.0f);
                iTicker.Reset();
                iLastTime = 0;
                iScrollTimer.Start();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta > 0)
            {
                if (Math.Abs(iPixelsPerMs) > 0)
                {
                    --iTargetIndex;
                }
                else
                {
                    iTargetIndex = iStartIndex - 1;
                }
                if (iTargetIndex < 0)
                {
                    iTargetIndex = 0;
                }
                iPixelsPerMs += ClientSize.Width / 200.0f;
            }
            else
            {
                if (Math.Abs(iPixelsPerMs) > 0)
                {
                    ++iTargetIndex;
                }
                else
                {
                    iTargetIndex = iStartIndex + 1;
                }
                if (iTargetIndex > iPlaylist.Count + iPresets.Count - 1)
                {
                    iTargetIndex = iPlaylist.Count + iPresets.Count - 1;
                }
                iPixelsPerMs += -ClientSize.Width / 200.0f;
            }

            if (Math.Abs(iPixelsPerMs) > 0)
            {
                iTicker.Reset();
                iLastTime = 0;
                iScrollTimer.Start();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (iPlaylistOpen)
            {
                PaintPlaylist(e);
            }
            else if (iRadioOpen && iPresetIndex > -1)
            {
                PaintPresets(e);
            }
            else
            {
                PaintTrack(e);
            }
        }

        private void PaintPlaylist(PaintEventArgs e)
        {
            e.Graphics.Clip = new Region(ClientRectangle);

            iMutex.WaitOne();

            e.Graphics.FillRectangle(Brushes.Black, ClientRectangle);

            int height = (int)(ClientSize.Height * 0.5f);
            int width = height;

            int offsetY = (int)((ClientSize.Height - (height * 1.5f)) * 0.5f);

            for (int i = iStartIndex; i < iStartIndex + 2 && i < iPlaylist.Count; ++i)
            {
                int offsetX = ClientRectangle.X + -iOffset + (i * kItemWidth);

                upnpObject o = iPlaylist[i].DidlLite[0];
                PaintItem(e.Graphics, width, height, offsetX, offsetY, DidlLiteAdapter.ArtworkUri(o), iTitle, iAlbum, iArtist, DidlLiteAdapter.Genre(o), iIndex == i);
            }

            iMutex.ReleaseMutex();
        }

        private void PaintPresets(PaintEventArgs e)
        {
            e.Graphics.Clip = new Region(ClientRectangle);

            iMutex.WaitOne();

            e.Graphics.FillRectangle(Brushes.Black, ClientRectangle);

            int height = (int)(ClientSize.Height * 0.5f);
            int width = height;

            int offsetY = (int)((ClientSize.Height - (height * 1.5f)) * 0.5f);

            for (int i = iStartIndex; i < iStartIndex + 2 && i < iPresets.Count; ++i)
            {
                int offsetX = ClientRectangle.X + -iOffset + (i * kItemWidth);

                upnpObject o = iPresets[i].DidlLite[0];
                PaintItem(e.Graphics, width, height, offsetX, offsetY, DidlLiteAdapter.ArtworkUri(o), iTitle, iAlbum, iArtist, DidlLiteAdapter.Genre(o), i == iPresetIndex);
            }

            iMutex.ReleaseMutex();
        }

        private void PaintTrack(PaintEventArgs e)
        {
            iMutex.WaitOne();

            e.Graphics.FillRectangle(Brushes.Black, ClientRectangle);

            Color fontColour = iViewSupport.ForeColourBright;
            bool drawTime = (iTransportState == ETransportState.ePlaying || iTransportState == ETransportState.ePaused);

            int height = (int)(ClientSize.Height * 0.5f);
            int width = height;

            int offsetY = (int)((ClientSize.Height - (height * 1.5f)) * 0.5f);
            int offsetX = ClientRectangle.X;

            /*musicTrack track = new musicTrack();
            artist artist = new artist();
            artist.Artist = iArtist;

            track.Title = iTitle;
            track.Album.Add(iAlbum);
            track.Artist.Add(artist);

            PaintItem(e.Graphics, width, height, offsetX, offsetY, track, true);*/

            width = (int)(iArtwork.Width * (height / (float)iArtwork.Height));
            int offset = (int)((height - width) * 0.5f) + 20;
            e.Graphics.DrawImage(iArtwork, offsetX + offset, offsetY, width, height);

            int reflectionHeight = (int)(iReflectedArtwork.Height * (width / (float)iReflectedArtwork.Width));
            e.Graphics.DrawImage(iReflectedArtwork, offsetX + offset, offsetY + height, width, reflectionHeight);

            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.EndEllipsis;
            int x = offsetX + height + 30;
            int y = offsetY;
            width = Width - (height + 30);
            Size size = TextRenderer.MeasureText(e.Graphics, iTitle, iLargeFont);
            TextRenderer.DrawText(e.Graphics, iTitle, iLargeFont, new Rectangle(x, y, width, size.Height), fontColour, flags);
            y += (int)(size.Height + (height * 0.1f));

            size = TextRenderer.MeasureText(e.Graphics, iArtist, iMediumFont);
            TextRenderer.DrawText(e.Graphics, iArtist, iMediumFont, new Rectangle(x, y, width, size.Height), fontColour, flags);
            y += size.Height;

            size = TextRenderer.MeasureText(e.Graphics, iAlbum, iSmallFont);
            TextRenderer.DrawText(e.Graphics, iAlbum, iSmallFont, new Rectangle(x, y, width, size.Height), fontColour, flags);
            y += size.Height;

            //size = TextRenderer.MeasureText(e.Graphics, iGenre, iSmallFont);
            //TextRenderer.DrawText(e.Graphics, iGenre, iSmallFont, new Rectangle(x, y, width, size.Height), fontColour, flags);
            y += size.Height;

            //if (drawTime && !iStopped)
            {
                y = height + offsetY - iSmallFont.Height;
                //int yOffset = (int)(Height * 0.12f);

                /*using (Pen p = new Pen(iViewSupport.ForeColourBright))
                {
                    //Rectangle rect = new Rectangle(x, y, width - 30, size.Height);
                    //e.Graphics.DrawRectangle(p, rect);
                    int startOffset = (i == 0) ? 20 : 0;
                    int endOffset = (i == (iPlaylist.Count - 1)) ? 20 : 0;
                    if (i == 0)
                    {
                        e.Graphics.DrawLine(p, new Point(startOffset + offsetX, ClientSize.Height - iSmallFont.Height - yOffset), new Point(startOffset + offsetX, ClientSize.Height - yOffset));
                    }
                    if (i == (iPlaylist.Count - 1))
                    {
                        e.Graphics.DrawLine(p, new Point(ClientSize.Width - endOffset + offsetX, ClientSize.Height - iSmallFont.Height - yOffset), new Point(ClientSize.Width - endOffset + offsetX, ClientSize.Height - yOffset));
                    }
                    e.Graphics.DrawLine(p, new Point(startOffset + offsetX, ClientSize.Height - iSmallFont.Height - yOffset), new Point(offsetX + ClientSize.Width - endOffset, ClientSize.Height - iSmallFont.Height - yOffset));
                    e.Graphics.DrawLine(p, new Point(startOffset + offsetX, ClientSize.Height - yOffset), new Point(offsetX + ClientSize.Width - endOffset, ClientSize.Height - yOffset));
                }*/
                /*using (Brush b = new SolidBrush(iViewSupport.HighlightBackColour))
                {
                    int startOffset = (i == 0) ? 21 : 0;
                    int endOffset = (i == (iPlaylist.Count - 1)) ? 21 : 0;
                    if (i < iIndex)
                    {
                        Rectangle rect = new Rectangle(startOffset + offsetX, ClientSize.Height - iSmallFont.Height - (yOffset - 1), ClientSize.Width - startOffset, iSmallFont.Height - 1);
                        e.Graphics.FillRectangle(b, rect);
                    }
                    else if (drawTime && iDuration > 0)
                    {
                        Rectangle rect = new Rectangle(startOffset + offsetX, ClientSize.Height - iSmallFont.Height - yOffset + 1, (int)((ClientSize.Width - startOffset - endOffset) * (iSeconds / (float)iDuration)), iSmallFont.Height - 1);
                        e.Graphics.FillRectangle(b, rect);
                    }
                }*/

                Time time = new Time((int)iSeconds);
                Time duration = new Time((int)iDuration);

                string t = (drawTime ? time.ToPrettyString() : "");
                if (duration.SecondsTotal > 0)
                {
                    t += (drawTime ? " / " : "") + duration.ToPrettyString();
                }

                Size tSize = TextRenderer.MeasureText(e.Graphics, t, iMicroFont);
                TextRenderer.DrawText(e.Graphics, t, iMicroFont, new Rectangle(x, y - tSize.Height, tSize.Width, iMicroFont.Height), fontColour, TextFormatFlags.Default);

                /*size = TextRenderer.MeasureText(e.Graphics, duration.ToPrettyString(), iMicroFont);
                TextRenderer.DrawText(e.Graphics, duration.ToPrettyString(), iMicroFont, new Rectangle(x + width - 30 - size.Width, y - size.Height, size.Width, size.Height), fontColour, TextFormatFlags.Right);*/

                if (iCodec != string.Empty)
                {
                    string info = iBitrate.ToString() + " kbps, " + iSampleRate.ToString() + " kHz" + (iLossless ? " / " + iBitDepth.ToString() + " bits, " : ", ") + iCodec;
                    Size infoSize = TextRenderer.MeasureText(e.Graphics, info, iMicroFont);
                    TextRenderer.DrawText(e.Graphics, info, iMicroFont, new Rectangle(x, y - iMicroFont.Height - iMicroFont.Height, infoSize.Width, infoSize.Height), fontColour, TextFormatFlags.Default);
                }
            }

            iMutex.ReleaseMutex();
        }

        private void PaintItem(Graphics aGraphics, int aWidth, int aHeight, int aOffsetX, int aOffsetY, Uri aArtworkUri, string aTitle, string aAlbum, string aArtist, string aGenre, bool aCurrent)
        {
            int height = aHeight;
            int width = height;

            int offsetY = aOffsetY;
            int offsetX = aOffsetX;

            Rectangle itemRect = new Rectangle(offsetX, ClientRectangle.Y, kItemWidth, ClientSize.Height);
            if (ClientRectangle.IntersectsWith(itemRect))
            {
                bool drawTime = false;
                Color fontColour = iViewSupport.ForeColour;
                if (aCurrent)
                {
                    fontColour = iViewSupport.ForeColourBright;
                    drawTime = (iTransportState == ETransportState.ePlaying || iTransportState == ETransportState.ePaused);
                }

                Uri uri = aArtworkUri;

                IArtwork artwork = null;
                Image image = kImageNoArtwork;
                if (uri != null)
                {
                    artwork = iArtworkCache.Artwork(uri);

                    if (artwork.Image != null)
                    {
                        image = artwork.Image;
                    }
                }

                width = (int)(image.Width * (height / (float)image.Height));
                int offset = (int)((height - width) * 0.5f) + 20;
                aGraphics.DrawImage(image, offsetX + offset, offsetY, width, height);

                image = null;
                if (artwork != null)
                {
                    image = artwork.ImageReflected;
                }
                if (image == null)
                {
                    image = kImageReflectedNoArtwork;
                }
                int reflectionHeight = (int)(image.Height * (width / (float)image.Width));
                aGraphics.DrawImage(image, offsetX + offset, offsetY + height, width, reflectionHeight);

                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.EndEllipsis;
                int x = offsetX + height + 30;
                int y = offsetY;
                width = Width - (height + 30);
                Size size = TextRenderer.MeasureText(aGraphics, aTitle, iLargeFont);
                TextRenderer.DrawText(aGraphics, aTitle, iLargeFont, new Rectangle(x, y, width, size.Height), fontColour, flags);
                y += (int)(size.Height + (height * 0.1f));

                if (aArtist.Length > 0)
                {
                    size = TextRenderer.MeasureText(aGraphics, aArtist, iMediumFont);
                    TextRenderer.DrawText(aGraphics, aArtist, iMediumFont, new Rectangle(x, y, width, size.Height), fontColour, flags);
                    y += size.Height;
                }

                if (aAlbum.Length > 0)
                {
                    size = TextRenderer.MeasureText(aGraphics, aAlbum, iSmallFont);
                    TextRenderer.DrawText(aGraphics, aAlbum, iSmallFont, new Rectangle(x, y, width, size.Height), fontColour, flags);
                    y += size.Height;
                }

                if (aGenre.Length > 0)
                {
                    size = TextRenderer.MeasureText(aGraphics, aGenre, iSmallFont);
                    TextRenderer.DrawText(aGraphics, aGenre, iSmallFont, new Rectangle(x, y, width, size.Height), fontColour, flags);
                    y += size.Height;
                }

                y = height + offsetY - iSmallFont.Height;
                //int yOffset = (int)(Height * 0.12f);

                /*using (Pen p = new Pen(iViewSupport.ForeColourBright))
                {
                    //Rectangle rect = new Rectangle(x, y, width - 30, size.Height);
                    //e.Graphics.DrawRectangle(p, rect);
                    int startOffset = (i == 0) ? 20 : 0;
                    int endOffset = (i == (iPlaylist.Count - 1)) ? 20 : 0;
                    if (i == 0)
                    {
                        e.Graphics.DrawLine(p, new Point(startOffset + offsetX, ClientSize.Height - iSmallFont.Height - yOffset), new Point(startOffset + offsetX, ClientSize.Height - yOffset));
                    }
                    if (i == (iPlaylist.Count - 1))
                    {
                        e.Graphics.DrawLine(p, new Point(ClientSize.Width - endOffset + offsetX, ClientSize.Height - iSmallFont.Height - yOffset), new Point(ClientSize.Width - endOffset + offsetX, ClientSize.Height - yOffset));
                    }
                    e.Graphics.DrawLine(p, new Point(startOffset + offsetX, ClientSize.Height - iSmallFont.Height - yOffset), new Point(offsetX + ClientSize.Width - endOffset, ClientSize.Height - iSmallFont.Height - yOffset));
                    e.Graphics.DrawLine(p, new Point(startOffset + offsetX, ClientSize.Height - yOffset), new Point(offsetX + ClientSize.Width - endOffset, ClientSize.Height - yOffset));
                }*/
                /*using (Brush b = new SolidBrush(iViewSupport.HighlightBackColour))
                {
                    int startOffset = (i == 0) ? 21 : 0;
                    int endOffset = (i == (iPlaylist.Count - 1)) ? 21 : 0;
                    if (i < iIndex)
                    {
                        Rectangle rect = new Rectangle(startOffset + offsetX, ClientSize.Height - iSmallFont.Height - (yOffset - 1), ClientSize.Width - startOffset, iSmallFont.Height - 1);
                        e.Graphics.FillRectangle(b, rect);
                    }
                    else if (drawTime && iDuration > 0)
                    {
                        Rectangle rect = new Rectangle(startOffset + offsetX, ClientSize.Height - iSmallFont.Height - yOffset + 1, (int)((ClientSize.Width - startOffset - endOffset) * (iSeconds / (float)iDuration)), iSmallFont.Height - 1);
                        e.Graphics.FillRectangle(b, rect);
                    }
                }*/

                if (aCurrent)
                {
                    Time time = new Time((int)iSeconds);
                    Time duration = new Time((int)iDuration);

                    string t = (drawTime ? time.ToPrettyString() : "");
                    if (duration.SecondsTotal > 0)
                    {
                        t += (drawTime ? " / " : "") + duration.ToPrettyString();
                    }

                    Size tSize = TextRenderer.MeasureText(aGraphics, t, iMicroFont);
                    TextRenderer.DrawText(aGraphics, t, iMicroFont, new Rectangle(x, y - iMicroFont.Height, tSize.Width, tSize.Height), fontColour, TextFormatFlags.Default);

                    /*size = TextRenderer.MeasureText(e.Graphics, duration.ToPrettyString(), iMicroFont);
                    TextRenderer.DrawText(e.Graphics, duration.ToPrettyString(), iMicroFont, new Rectangle(x + width - 30 - size.Width, y - size.Height, size.Width, size.Height), fontColour, TextFormatFlags.Right);*/

                    if (iCodec != string.Empty)
                    {
                        string info = iBitrate.ToString() + " kbps, " + iSampleRate.ToString() + " kHz" + (iLossless ? " / " + iBitDepth.ToString() + " bits, " : ", ") + iCodec;
                        Size infoSize = TextRenderer.MeasureText(aGraphics, info, iMicroFont);
                        TextRenderer.DrawText(aGraphics, info, iMicroFont, new Rectangle(x, y - iMicroFont.Height - iMicroFont.Height, infoSize.Width, infoSize.Height), fontColour, TextFormatFlags.Default);
                    }
                }
            }
        }

        private void SetOffsetRelative(int aDelta)
        {
            iOffset -= aDelta;
            SetValidBounds();
        }

        private void SetOffsetAbsolute(int aOffset)
        {
            iOffset = aOffset;
            SetValidBounds();
        }

        private void SetValidBounds()
        {
            if (iOffset < 0)
            {
                iOffset = 0;
            }
            if (iOffset > kItemWidth * (iPlaylist.Count + iPresets.Count - 1))
            {
                iOffset = kItemWidth * (iPlaylist.Count + iPresets.Count - 1);
            }

            iStartIndex = iOffset / kItemWidth;

            if (iStartIndex > iPlaylist.Count + iPresets.Count - 1)
            {
                iStartIndex = iPlaylist.Count + iPresets.Count - 1;
            }
            if (iStartIndex < 0)
            {
                iStartIndex = 0;
            }
        }

        private void FormStatus_Resize(object sender, EventArgs e)
        {
            if (ClientSize.Width > 0 && ClientSize.Height > 0)
            {
                iMutex.WaitOne();

                iSmallFont.Dispose();
                iMediumFont.Dispose();
                iLargeFont.Dispose();

                iMicroFont = new Font(iViewSupport.FontSmall.FontFamily, ClientSize.Height / 50.0f, FontStyle.Regular);
                iSmallFont = new Font(iViewSupport.FontSmall.FontFamily, ClientSize.Height / 35.0f, FontStyle.Regular);
                iMediumFont = new Font(iViewSupport.FontMedium.FontFamily, ClientSize.Height / 30.0f, FontStyle.Regular);
                iLargeFont = new Font(iViewSupport.FontLarge.FontFamily, ClientSize.Height / 20.0f, FontStyle.Bold);

                float ratio = ClientSize.Width / (float)kItemWidth;
                kItemWidth = ClientSize.Width;
                SetOffsetAbsolute((int)Math.Round(iOffset * ratio));

                iMutex.ReleaseMutex();

                Invalidate();
            }
        }

        private void FormStatus_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
