using System;
using System.IO;
using System.Net;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

using Linn.Kinsky;
using Linn.Topology;

using Upnp;

namespace KinskyDesktop.Widgets
{
    public partial class PlaylistWidget<T> : UserControl where T : MrItem
    {
        public enum EAdornment
        {
            eOff,
            eRed,
            eAmber,
            eGreen
        }

        private class NearestIndexResult
        {
            public NearestIndexResult(int aIndex, bool aInsertAfter, float aDistance)
            {
                iIndex = aIndex;
                iInsertAfter = aInsertAfter;
                iDistance = aDistance;
            }

            public int Index
            {
                get
                {
                    return iIndex;
                }
            }

            public bool InsertAfter
            {
                get
                {
                    return iInsertAfter;
                }
            }

            public float Distance
            {
                get
                {
                    return iDistance;
                }
            }

            public override string ToString()
            {
                return string.Format("iIndex={0},iInsertAfter={1},iDistance={2}", iIndex, iInsertAfter, iDistance);
            }

            private int iIndex;
            private bool iInsertAfter;
            private float iDistance;
        }

        private class TrackCollection
        {
            public TrackCollection(int aY, int aWidth, int aStartIndex, IArtworkCache aCache, PlaylistWidget<T> aParent, IViewSupport aViewSupport)
            {
                iViewSupport = aViewSupport;

                iParent = aParent;
                iCache = aCache;
                iClientRectangle = new Rectangle(0, aY, aWidth, 0);

                iCollapsed = false;
                iIsAlbum = false;
                iPlayingIndex = -1;
                iStartIndex = aStartIndex;

                iTracks = new List<T>();
                iAdornment = new List<EAdornment>();

                iCache.EventUpdated += EventUpdated;
            }

            public bool IsPartOfCollection(T aTrack)
            {
                string album = DidlLiteAdapter.Album(iTracks[0].DidlLite[0]);
                return (aTrack.DidlLite[0] is musicTrack && iTracks[0].DidlLite[0] is musicTrack && album != string.Empty && (DidlLiteAdapter.Album(aTrack.DidlLite[0]) == album));
            }

            public void Add(T aTrack, EAdornment aAdornment)
            {
                iIsAlbum = aTrack.DidlLite[0] is musicTrack && DidlLiteAdapter.Album(aTrack.DidlLite[0]) != string.Empty && (iTracks.Count > 0);

                if (iTracks.Count == 0)
                {
                    iArtworkUri = DidlLiteAdapter.ArtworkUri(aTrack.DidlLite[0]);
                }

                iTracks.Add(aTrack);
                iAdornment.Add(aAdornment);
                
                CalcHeight();
            }

            public bool Collapsed
            {
                get
                {
                    return iCollapsed;
                }
                set
                {
                    iCollapsed = value;
                    CalcHeight();
                }
            }

            public int Count
            {
                get
                {
                    return iTracks.Count;
                }
            }

            public void CalcHeight()
            {
                iClientRectangle.Height = kHeaderHeight + kHeaderSpacer + kHeaderSpacer;
                if (!iCollapsed)
                {
                    if (iIsAlbum)
                    {
                        iClientRectangle.Height += (iTracks.Count * kItemHeight);
                    }
                }
            }

            public int Top
            {
                get
                {
                    return iClientRectangle.Top;
                }
            }

            public int Bottom
            {
                get
                {
                    return iClientRectangle.Bottom;
                }
            }

            public int X
            {
                get
                {
                    return iClientRectangle.X;
                }
                set
                {
                    iClientRectangle.X = value;
                }
            }

            public int Y
            {
                get
                {
                    return iClientRectangle.Y;
                }
                set
                {
                    iClientRectangle.Y = value;
                }
            }

            public int Height
            {
                get
                {
                    return iClientRectangle.Height;
                }
            }

            public int Width
            {
                get
                {
                    return iClientRectangle.Width;
                }
                set
                {
                    iClientRectangle.Width = value;
                }
            }

            public T this[int aIndex]
            {
                get
                {
                    return iTracks[aIndex];
                }
                set
                {
                    iTracks[aIndex] = value;
                }
            }

            public int SetTrack(T aTrack, bool aFound)
            {
                if (!aFound)
                {
                    for(int i = 0; i < iTracks.Count; ++i)
                    {
                        if (iTracks[i] == aTrack)
                        {
                            iPlayingIndex = i;
                            return (iStartIndex + iPlayingIndex);
                        }
                    }
                }

                iPlayingIndex = -1;
                return -1;
            }

            public bool EnsureVisible(int aIndex, float aOffset, out float aNewOffset)
            {
                aNewOffset = aOffset;

                if (iStartIndex <= aIndex && aIndex < iStartIndex + iTracks.Count)
                {
                    float itemTop = iClientRectangle.Top + aOffset;
                    float itemBottom = itemTop + kHeaderHeight + kHeaderSpacer + kHeaderSpacer;

                    int index = aIndex - iStartIndex;
                    if (iIsAlbum)
                    {
                        itemTop = iClientRectangle.Top + kHeaderHeight + kHeaderSpacer + kHeaderSpacer + (kItemHeight * index) + aOffset;
                        itemBottom = itemTop + kItemHeight;
                    }

                    if (itemBottom > iParent.ClientRectangle.Bottom)
                    {
                        aNewOffset = aOffset + (iParent.ClientRectangle.Bottom - itemBottom);
                    }
                    else if (itemTop < 0)
                    {
                        aNewOffset = aOffset - itemTop;
                        if (index == 0 && iIsAlbum)
                        {
                            aNewOffset += kHeaderHeight + kHeaderSpacer + kHeaderSpacer;
                        }
                    }
                    
                    return true;
                }
                aOffset = 0;
                return false;
            }

            public int MouseDown(MouseEventArgs e, bool aAdd)
            {
                iMouseDownIndex = TrackIndexAt(e.Location);

                /*if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                {
                    if (iMouseDownIndex == -2)
                    {
                        if(!aAdd || (aAdd && !iHeaderSelected))
                        {
                            iHeaderSelected = true;
                        }
                        else if (aAdd && iHeaderSelected)
                        {
                            iHeaderSelected = false;
                        }
                    }
                    else
                    {
                        iHeaderSelected = false;
                    }
                }*/

                return iMouseDownIndex;
            }

            public void MouseUp(MouseEventArgs e)
            {
            }

            public int MouseDoubleClick(MouseEventArgs e, bool aAdd)
            {
                int index = TrackIndexAt(e.Location);

                if (e.Button == MouseButtons.Left)
                {
                    if (index == iMouseDownIndex)
                    {
                        if (index == -2)
                        {
                            //Collapsed = !Collapsed;
                        }
                    }
                }

                return index;
            }

            public NearestIndexResult NearestIndex(Point aPoint)
            {
                int index = -1;
                bool after = false;
                float distance = int.MaxValue;

                if (!iIsAlbum)
                {
                    int yOffset = aPoint.Y - iClientRectangle.Top;
                    int halfHeight = (int)(kHeaderHeight * 0.5f);
                    int d = yOffset - halfHeight;
                    int abs = Math.Abs(d);
                    if (abs < distance)
                    {
                        after = (d < 0) ? false : true;
                        index = 0;
                        distance = abs;
                    }
                }
                else
                {
                    int yOffset = aPoint.Y - iClientRectangle.Top - kHeaderHeight;
                    int halfHeight = (int)(kItemHeight * 0.5f);
                    for (int i = 0; i < iTracks.Count; ++i)
                    {
                        int d = yOffset - halfHeight - (kItemHeight * i);
                        int abs = Math.Abs(d);
                        if (abs < distance)
                        {
                            after = (d < 0) ? false : true;
                            index = i;
                            distance = abs;
                        }
                    }
                }

                return new NearestIndexResult(index, after, distance);
            }

            public void Paint(float aOffset, Image aImagePlaying, PaintEventArgs e)
            {
                RectangleF dispRect = new RectangleF(X, Y + aOffset, Width, Height);
                if (e.Graphics.ClipBounds.IntersectsWith(dispRect))
                {
                    if (iIsAlbum)
                    {
                        DrawHeader(e.Graphics, (int)aOffset, /*iHeaderSelected*/false, EAdornment.eOff, false, (iStartIndex + 1).ToString(), DidlLiteAdapter.Album(iTracks[0].DidlLite[0]), DidlLiteAdapter.Artist(iTracks[0].DidlLite[0]), string.Empty, string.Empty);
                    }
                    else
                    {
                        if (iTracks[0].DidlLite[0] is musicTrack)
                        {
                            string subHeader1 = DidlLiteAdapter.Album(iTracks[0].DidlLite[0]);
                            string subHeader2 = DidlLiteAdapter.Artist(iTracks[0].DidlLite[0]);
                            string duration = DidlLiteAdapter.Duration(iTracks[0].DidlLite[0]);
                            DrawHeader(e.Graphics, (int)aOffset, iParent.SelectedIndices.Contains(iStartIndex), iAdornment[0], iPlayingIndex == 0, (iStartIndex + 1).ToString(), DidlLiteAdapter.Title(iTracks[0].DidlLite[0]), subHeader1, subHeader2, duration);
                        }
                        else
                        {
                            string subHeader1 = DidlLiteAdapter.Description(iTracks[0].DidlLite[0]);
                            string subHeader2 = DidlLiteAdapter.Genre(iTracks[0].DidlLite[0]);
                            string bitrate = DidlLiteAdapter.Bitrate(iTracks[0].DidlLite[0]);
                            DrawHeader(e.Graphics, (int)aOffset, iParent.SelectedIndices.Contains(iStartIndex), iAdornment[0], iPlayingIndex == 0, (iStartIndex + 1).ToString(), DidlLiteAdapter.Title(iTracks[0].DidlLite[0]), subHeader1, subHeader2, bitrate);
                        }
                    }

                    dispRect.Height = kItemHeight;
                    int y = (int)aOffset + kHeaderHeight + kHeaderSpacer + kHeaderSpacer;
                    for (int i = 0; i < iTracks.Count && !iCollapsed; ++i, y += kItemHeight)
                    {
                        dispRect.Y = Y + y;
                        if (e.Graphics.ClipBounds.IntersectsWith(dispRect))
                        {
                            if (iIsAlbum)
                            {
                                string title = DidlLiteAdapter.Title(iTracks[i].DidlLite[0]);
                                string duration = DidlLiteAdapter.Duration(iTracks[i].DidlLite[0]);
                                DrawItem(e.Graphics, y, iParent.SelectedIndices.Contains(iStartIndex + i), iPlayingIndex == i, (iStartIndex + i + 1).ToString(), title, duration);
                            }
                        }
                    }

                    if (iParent.InsertionMark.Index > -1)
                    {
                        int index = iParent.InsertionMark.Index;
                        index = iParent.InsertionMark.AppearsAfterItem ? index + 1 : index;

                        if (iStartIndex <= index && index < iStartIndex + iTracks.Count || (index == iStartIndex + iTracks.Count && iParent.InsertionMark.AppearsAfterItem))
                        {
                            if (iIsAlbum)
                            {
                                int offset = (int)((index - iStartIndex) * kItemHeight + Y + aOffset - 1);
                                if (!(iStartIndex == index && iParent.InsertionMark.AppearsAfterItem))
                                {
                                    offset += kHeaderHeight + kHeaderSpacer + kHeaderSpacer;
                                }
                                DrawInsertionMark(e.Graphics, offset);
                            }
                            else
                            {
                                int offset = (int)(Y + aOffset - 1);
                                if (iStartIndex == iParent.InsertionMark.Index && iParent.InsertionMark.AppearsAfterItem)
                                {
                                    offset += kHeaderHeight + kHeaderSpacer + kHeaderSpacer;
                                }
                                DrawInsertionMark(e.Graphics, offset);
                            }
                        }
                    }
                }
            }

            private void DrawInsertionMark(Graphics aGraphics, int aOffset)
            {
                Rectangle line = new Rectangle(X, aOffset, Width, 2);
                aGraphics.FillRectangle(Brushes.White, line);

                Point[] leftTriangle = new Point[3] {
                            new Point(X, aOffset - 4),
                            new Point(X + 7, aOffset),
                            new Point(X, aOffset + 4)
                        };
                aGraphics.FillPolygon(Brushes.White, leftTriangle);

                Point[] rightTriangle = new Point[3] {
                            new Point(Width, aOffset - 4),
                            new Point(Width - 8, aOffset),
                            new Point(Width, aOffset + 4)
                        };
                aGraphics.FillPolygon(Brushes.White, rightTriangle);
            }

            private void DrawHeader(Graphics aGraphics, int aOffset, bool aSelected, EAdornment aAdornmentState, bool aPlaying, string aIndex, string aHeader, string aSubHeader1, string aSubHeader2, string aBitrate)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;

                Brush brushHeader = iParent.BrushForeColour;
                Brush brushMutedText = iParent.BrushForeColourMuted;

                if (aSelected)
                {
                    Rectangle bounds = new Rectangle(kPlayingIconWidth + 1, Top + aOffset, kSelectedIconWidth, kHeaderHeight + kHeaderSpacer + kHeaderSpacer - 1);
                    aGraphics.FillRectangle(iParent.BrushHighlightBackColour, bounds);

                    brushHeader = iParent.BrushHighlightForeColour;
                    brushMutedText = iParent.BrushHighlightForeColour;
                }
                else
                {
                    Rectangle bounds = new Rectangle(0, Top + aOffset, Width, kHeaderHeight + kHeaderSpacer + kHeaderSpacer);
                    aGraphics.FillRectangle(iParent.BrushBackColour, bounds);
                }

                IArtwork artwork = null;
                Image image = null;
                if (iArtworkUri != null)
                {
                    artwork = iCache.Artwork(iArtworkUri);
                    image = artwork.Image;
                }
                if (image != null)
                {
                    DrawArtwork(aGraphics, image, aOffset + kHeaderSpacer);
                }
                else
                {
                    DrawArtwork(aGraphics, kImageNoArtwork, aOffset + kHeaderSpacer);
                }

                if (!iIsAlbum)
                {
                    Image playing = iParent.ImagePlaying;
                    float y = Top + aOffset + kHeaderSpacer;

                    if (aAdornmentState != EAdornment.eOff)
                    {
                        Image adornment = null;
                        switch (aAdornmentState)
                        {
                            case EAdornment.eRed:
                                adornment = iParent.ImageAdornmentRed;
                                break;
                            case EAdornment.eAmber:
                                adornment = iParent.ImageAdornmentAmber;
                                break;
                            case EAdornment.eGreen:
                                adornment = iParent.ImageAdornmentGreen;
                                break;
                            default:
                                break;
                        }
                        if(adornment != null)
                        {
                            SizeF size = ImageSize(adornment, kPlayingIconWidth, kPlayingIconWidth);
                            aGraphics.DrawImage(adornment, 0, y, size.Width, size.Height);
                            y += size.Height + kHeaderSpacer;
                        }
                    }

                    if (playing != null && aPlaying)
                    {
                        SizeF size = ImageSize(playing, kPlayingIconWidth, kPlayingIconWidth);
                        aGraphics.DrawImage(playing, 0, y, size.Width, size.Height);
                    }

                    Rectangle rect = new Rectangle(kPlayingIconWidth + kSelectedIconWidth, Top + aOffset + kHeaderSpacer, kNumberWidth, iViewSupport.FontSmall.Height);
                    format.Alignment = StringAlignment.Center;
                    aGraphics.DrawString(aIndex, iViewSupport.FontSmall, brushMutedText, rect, format);
                }

                format.Alignment = StringAlignment.Near;
                Size bitrateSize = TextRenderer.MeasureText(aBitrate, iViewSupport.FontSmall);

                int offset = kHeaderHeight + kPlayingIconWidth + kSelectedIconWidth;
                Rectangle r = new Rectangle(offset + kNumberWidth, Top + aOffset + kHeaderSpacer, (int)(Width - offset - kNumberWidth - bitrateSize.Width), iViewSupport.FontMedium.Height);

                if (aHeader != string.Empty)
                {
                    aGraphics.DrawString(aHeader, iViewSupport.FontMedium, brushHeader, r, format);
                    r.Y += iViewSupport.FontMedium.Height;
                }

                if (aSubHeader1 != string.Empty)
                {
                    aGraphics.DrawString(aSubHeader1, iViewSupport.FontSmall, brushHeader, r, format);
                    r.Y += iViewSupport.FontSmall.Height;
                }

                if (aSubHeader2 != string.Empty)
                {
                    aGraphics.DrawString(aSubHeader2, iViewSupport.FontSmall, brushHeader, r, format);
                }

                if (aBitrate != string.Empty)
                {
                    format.Alignment = StringAlignment.Far;
                    r = new Rectangle(r.Right, Top + aOffset + kHeaderSpacer, (int)bitrateSize.Width, iViewSupport.FontSmall.Height);
                    aGraphics.DrawString(aBitrate, iViewSupport.FontSmall, brushMutedText, r, format);
                }
            }

            private void DrawArtwork(Graphics aGraphics, Image aArtwork, int aOffset)
            {
                SizeF size = ImageSize(aArtwork, kHeaderHeight - 1, kHeaderHeight - 1);
                aGraphics.DrawImage(aArtwork, (int)(kPlayingIconWidth + kSelectedIconWidth  + kNumberWidth + ((kHeaderHeight - size.Width) * 0.5f)), (int)(Top + aOffset), size.Width, size.Height);
            }

            private void DrawItem(Graphics aGraphics, int aOffset, bool aSelected, bool aPlaying, string aIndex, string aTitle, string aDuration)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;

                Brush brushText = iParent.BrushForeColour;
                Brush brushMutedText = iParent.BrushForeColourMuted;

                if (aSelected)
                {
                    Rectangle bounds = new Rectangle(kPlayingIconWidth + 1, Top + aOffset, kSelectedIconWidth, kItemHeight - 1);
                    aGraphics.FillRectangle(iParent.BrushHighlightBackColour, bounds);

                    brushText = iParent.BrushHighlightForeColour;
                    brushMutedText = iParent.BrushHighlightForeColour;
                }
                else
                {
                    Rectangle bounds = new Rectangle(0, Top + aOffset, Width, kItemHeight - 1);
                    aGraphics.FillRectangle(iParent.BrushBackColour, bounds);
                }

                Size durationSize = TextRenderer.MeasureText(aDuration, iViewSupport.FontSmall);

                Image playing = iParent.ImagePlaying;
                if (playing != null && aPlaying)
                {
                    SizeF size = ImageSize(playing, kPlayingIconWidth, kPlayingIconWidth);
                    aGraphics.DrawImage(playing, 0, Top + aOffset, size.Width, size.Height);
                }

                Rectangle rect = new Rectangle(kPlayingIconWidth + kSelectedIconWidth, Top + (int)(((kItemHeight - 1) - iViewSupport.FontSmall.Height) * 0.5f) + aOffset, kNumberWidth, iViewSupport.FontSmall.Height);
                format.Alignment = StringAlignment.Center;
                aGraphics.DrawString(aIndex, iViewSupport.FontSmall, brushMutedText, rect, format);

                rect = new Rectangle(rect.Right, rect.Top, (int)(Width - durationSize.Width - kNumberWidth - kPlayingIconWidth - kSelectedIconWidth), iViewSupport.FontSmall.Height);
                format.Alignment = StringAlignment.Near;
                aGraphics.DrawString(aTitle, iViewSupport.FontSmall, brushText, rect, format);

                format.Alignment = StringAlignment.Far;
                rect = new Rectangle(rect.Right, rect.Top, (int)durationSize.Width, iViewSupport.FontSmall.Height);
                aGraphics.DrawString(aDuration, iViewSupport.FontSmall, brushMutedText, rect, format);
            }

            private SizeF ImageSize(Image aImage, int aWidth, int aHeight)
            {
                SizeF size = new SizeF(aWidth, aHeight);
                if (aImage != null)
                {
                    if (aImage.Width > aImage.Height)
                    {
                        size.Height = aWidth * (aImage.Height / (float)aImage.Width);
                    }
                    else if (aImage.Height > aImage.Width)
                    {
                        size.Width = aHeight * (aImage.Width / (float)aImage.Height);
                    }
                }
                return size;
            }

            private int TrackIndexAt(Point aLocation)
            {
                if (iClientRectangle.Contains(aLocation))
                {
                    int yOffset = aLocation.Y - iClientRectangle.Top;

                    if (yOffset < kHeaderHeight)
                    {
                        if (iIsAlbum)
                        {
                            return -2;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return (int)((yOffset - (kHeaderHeight + kHeaderSpacer + kHeaderSpacer)) / (float)kItemHeight);
                    }
                }

                return -1;
            }

            private void EventUpdated(object sender, EventArgsArtwork e)
            {
                if (iArtworkUri != null)
                {
                    if (e.Artwork.Uri.AbsoluteUri == iArtworkUri.AbsoluteUri)
                    {
                        iParent.Invalidate();
                    }
                }
            }

            public static int kHeaderHeight = 74;
            public static int kItemHeight = 18;
            public static int kPlayingIconWidth = 14;
            public static int kSelectedIconWidth = 8;
            public static int kNumberWidth = 36;
            public static int kHeaderSpacer = 3;

            private static readonly Image kImageNoArtwork = Linn.Kinsky.Properties.Resources.NoAlbumArt;

            private IViewSupport iViewSupport;

            private Rectangle iClientRectangle;

            private int iMouseDownIndex;

            private int iStartIndex;
            private int iPlayingIndex;

            private bool iIsAlbum;
            private bool iCollapsed;

            private PlaylistWidget<T> iParent;

            private Uri iArtworkUri;

            private IArtworkCache iCache;
            private List<T> iTracks;
            private List<EAdornment> iAdornment;
        }

        internal PlaylistWidget(IArtworkCache aArtworkCache, IViewSupport aViewSupport)
        {
            iMutex = new Mutex(false);

            iArtworkCache = aArtworkCache;

            iViewSupport = aViewSupport;
            iViewSupport.EventSupportChanged += EventChanged;

            iCollections = new List<TrackCollection>();
            iTracks = new List<T>();
            iTrackIndex = -1;

            iDragging = false;
            iDragFinished = false;
            iFirstSelectedIndex = -1;
            iLastSelectedIndex = -1;
            iSelectedIndices = new List<int>();

            iBeginUpdate = false;

            iBrushBackColour = new SolidBrush(iViewSupport.BackColour);
            iBrushForeColour = new SolidBrush(iViewSupport.ForeColour);
            iBrushForeColourMuted = new SolidBrush(iViewSupport.ForeColourMuted);
            iBrushHighlightBackColour = new SolidBrush(iViewSupport.HighlightBackColour);
            iBrushHighlightForeColour = new SolidBrush(iViewSupport.HighlightForeColour);

            iInsertionMark = new PlaylistWidgetInsertionMark<T>(this);

            InitializeComponent();

            scrollBarControl1.EventScroll += EventScroll;
            scrollBarControl1.EventValueChanged += EventScroll;
            scrollBarControl1.DoubleClick += EventScrollbarDoubleClick;

            EventChanged(null, EventArgs.Empty);
        }

        public void BeginUpdate()
        {
            iMutex.WaitOne();

            iBeginUpdate = true;

            iMutex.ReleaseMutex();
        }

        public void EndUpdate()
        {
            iMutex.WaitOne();

            iBeginUpdate = false;
            ClipOffsetToRange();
            SetScrollbar(iHeight, (int)iOffset);

            iMutex.ReleaseMutex();

            BeginInvoke((MethodInvoker)delegate()
            {
                Refresh();
                scrollBarControl1.Refresh();
            });
        }

        public void Add(T aTrack, EAdornment aAdornment)
        {
            iMutex.WaitOne();

            if (iLastCollection != null)
            {
                if (!iLastCollection.IsPartOfCollection(aTrack))
                {
                    CreateCollection(iLastCollection.Bottom, iTracks.Count, iArtworkCache);
                }
            }
            else
            {
                CreateCollection(0, iTracks.Count, iArtworkCache);
            }

            iLastCollection.Add(aTrack, aAdornment);
            iTracks.Add(aTrack);

            iHeight = iLastCollection.Top + iLastCollection.Height;

            int height = iHeight;
            int offset = (int)iOffset;

            iMutex.ReleaseMutex();

            if (!iBeginUpdate)
            {
                SetScrollbar(height, offset);
                Invalidate();
            }
        }

        public void Clear()
        {
            iMutex.WaitOne();

            iLastCollection = null;
            iCollections.Clear();
            iTracks.Clear();
            iSelectedIndices.Clear();
            iHeight = 0;
            iTrackIndex = -1;

            if (!iBeginUpdate)
            {
                iOffset = 0;
            }

            int height = iHeight;
            int offset = (int)iOffset;

            iMutex.ReleaseMutex();

            if (!iBeginUpdate)
            {
                SetScrollbar(height, offset);
                Invalidate();
            }
        }

        public void SetTrack(T aTrack)
        {
            iMutex.WaitOne();

            bool found = false;
            foreach (TrackCollection c in iCollections)
            {
                int index = c.SetTrack(aTrack, found);
                if (index > -1)
                {
                    iTrackIndex = index;
                    found = true;
                }
            }

            iMutex.ReleaseMutex();

            if (found || aTrack == null)
            {
                Invalidate();
            }
        }

        public IList<int> SelectedIndices
        {
            get
            {
                return iSelectedIndices.AsReadOnly();
            }
        }

        public IList<T> Tracks
        {
            get
            {
                return iTracks.AsReadOnly();
            }
        }

        public Image ImagePlaying
        {
            get
            {
                return iImagePlaying;
            }
            set
            {
                iImagePlaying = value;
            }
        }

        public Image ImageAdornmentRed
        {
            get
            {
                return  iImageAdornmentRed;
            }
            set
            {
                iImageAdornmentRed = value;
            }
        }

        public Image ImageAdornmentAmber
        {
            get
            {
                return iImageAdornmentAmber;
            }
            set
            {
                iImageAdornmentAmber = value;
            }
        }

        public Image ImageAdornmentGreen
        {
            get
            {
                return iImageAdornmentGreen;
            }
            set
            {
                iImageAdornmentGreen = value;
            }
        }

        public Brush BrushBackColour
        {
            get
            {
                return iBrushBackColour;
            }
        }

        public Brush BrushForeColour
        {
            get
            {
                return iBrushForeColour;
            }
        }

        public Brush BrushForeColourMuted
        {
            get
            {
                return iBrushForeColourMuted;
            }
        }

        public Brush BrushHighlightBackColour
        {
            get
            {
                return iBrushHighlightBackColour;
            }
        }

        public Brush BrushHighlightForeColour
        {
            get
            {
                return iBrushHighlightForeColour;
            }
        }

        public PlaylistWidgetInsertionMark<T> InsertionMark
        {
            get
            {
                return iInsertionMark;
            }
        }

        public ScrollBarControl ScrollBar
        {
            get
            {
                return scrollBarControl1;
            }
        }

        public void EnsureVisible(int aIndex)
        {
            iMutex.WaitOne();

            foreach (TrackCollection c in iCollections)
            {
                float newOffset;
                if (c.EnsureVisible(aIndex, iOffset, out newOffset))
                {
                    iOffset = newOffset;
                    SetScrollbar(iHeight, (int)iOffset);
                    break;
                }
            }

            iMutex.ReleaseMutex();

            Invalidate(true);
        }

        public event EventHandler<EventArgs> EventSelectedIndexChanged;
        public event EventHandler<EventArgs> EventItemActivated;
        public event EventHandler<ItemDragEventArgs> ItemDrag;

        internal void NearestIndex(Point aPoint, out int aIndex, out bool aInsertAfter)
        {
            int index = 0;
            Point p = new Point(aPoint.X, (int)(aPoint.Y - iOffset));
            NearestIndexResult result = new NearestIndexResult(-1, false, int.MaxValue);

            iMutex.WaitOne();

            foreach (TrackCollection c in iCollections)
            {
                NearestIndexResult r = c.NearestIndex(p);
                if (r.Distance < result.Distance)
                {
                    result = new NearestIndexResult(r.Index + index, r.InsertAfter, r.Distance);
                }
                index += c.Count;
            }

            if (iDragging && iLastSelectedIndex > -1 && iSelectedIndices.Count == 1)
            {
                if (result.Index == iLastSelectedIndex ||
                    result.Index == iLastSelectedIndex - 1 && result.InsertAfter ||
                    result.Index == iLastSelectedIndex + 1 && !result.InsertAfter)
                {
                    result = new NearestIndexResult(-1, false, result.Distance);
                }
            }
            if (iCollections.Count == 0)
            {
                result = new NearestIndexResult(0, false, 0.0f);
            }

            iMutex.ReleaseMutex();

            aIndex = result.Index;
            aInsertAfter = result.InsertAfter;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            iMutex.WaitOne();

            foreach (TrackCollection c in iCollections)
            {
                c.Paint(iOffset, iImagePlaying, e);
            }

            iMutex.ReleaseMutex();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            iMutex.WaitOne();

            foreach (TrackCollection c in iCollections)
            {
                c.Width = ClientSize.Width - scrollBarControl1.Width;
            }

            ClipOffsetToRange();

            int height = iHeight;
            int offset = (int)iOffset;

            iMutex.ReleaseMutex();
            
            SetScrollbar(height, offset);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            int index = 0;
            bool changed = false;
            bool itemSelected = false;
            bool add = ((Form.ModifierKeys & Keys.Control) == Keys.Control);
            bool range = ((Form.ModifierKeys & Keys.Shift) == Keys.Shift);
            MouseEventArgs eArgs = new MouseEventArgs(e.Button, e.Clicks, e.X, (int)(e.Y - iOffset), e.Delta);

            iStartKeyRangeSelect = true;

            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
            {
                return;
            }

            iPointMouseDown = e.Location;
            iLastSelectedIndex = -1;
            iDragFinished = false;
            iSelectLastIndex = false;

            iMutex.WaitOne();

            foreach (TrackCollection c in iCollections)
            {
                int i = c.MouseDown(eArgs, add);
                if (i > -1)
                {
                    if (range && iFirstSelectedIndex > -1)
                    {
                        SelectRange(iFirstSelectedIndex, index + i);
                        iLastSelectedIndex = index + i;
                    }
                    else
                    {
                        bool removed = false;
                        if (add)
                        {
                            removed = iSelectedIndices.Remove(index + i);
                        }
                        else
                        {
                            removed = iSelectedIndices.Contains(index + i);
                            if (!removed)
                            {
                                if (e.Button == MouseButtons.Left)
                                {
                                    iSelectedIndices.Clear();
                                }
                            }
                            else
                            {
                                iSelectedIndices.Remove(index + i);
                                if (e.Button == MouseButtons.Left)
                                {
                                    iSelectLastIndex = true;
                                }
                            }
                        }

                        if (!add && !removed && e.Button == MouseButtons.Right)
                        {
                            iSelectedIndices.Clear();
                        }

                        if (!(add && removed && e.Button == MouseButtons.Left))
                        {
                            iSelectedIndices.Add(index + i);
                            iLastSelectedIndex = index + i;
                        }
                    }
                    changed = true;
                    itemSelected = true;
                }
                else if (i == -2)
                {
                    if (!add && !range)
                    {
                        iSelectedIndices.Clear();
                    }
                    
                    if (range)
                    {
                        SelectRange(iFirstSelectedIndex, index);
                    }

                    bool allSelected = true;
                    for (int j = 0; j < c.Count; ++j)
                    {
                        if (add)
                        {
                            if (!iSelectedIndices.Remove(index + j))
                            {
                                allSelected = false;
                                break;
                            }
                        }
                        else
                        {
                            iSelectedIndices.Add(index + j);
                        }
                    }

                    if ((add || range) && !allSelected || e.Button == MouseButtons.Right)
                    {
                        for (int j = 0; j < c.Count; ++j)
                        {
                            iSelectedIndices.Add(index + j);
                        }
                    }

                    iLastSelectedIndex = index;
                    itemSelected = true;
                    changed = true;
                }
                index += c.Count;
            }

            if (iSelectedIndices.Count > 0 && !itemSelected)
            {
                iSelectedIndices.Clear();
                changed = true;
            }

            if (iSelectedIndices.Count > 0)
            {
                iFirstSelectedIndex = iSelectedIndices[0];
            }
            else
            {
                iFirstSelectedIndex = -1;
            }

            iMutex.ReleaseMutex();

            if (changed)
            {
                Invalidate();

                if (EventSelectedIndexChanged != null)
                {
                    EventSelectedIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        private void SelectRange(int aFirstSelectedIndex, int aIndex)
        {
            iSelectedIndices.Clear();

            if (aIndex <= aFirstSelectedIndex)
            {
                for (int j = aIndex; j <= aFirstSelectedIndex; ++j)
                {
                    iSelectedIndices.Add(j);
                }
            }
            else
            {
                for (int j = aFirstSelectedIndex; j <= aIndex; ++j)
                {
                    iSelectedIndices.Add(j);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            int y = 0;
            int index = 0;
            bool changed = false;
            MouseEventArgs eArgs = new MouseEventArgs(e.Button, e.Clicks, e.X, (int)(e.Y - iOffset), e.Delta);

            if (iSelectLastIndex)
            {
                iSelectedIndices.Clear();
                iSelectedIndices.Add(iLastSelectedIndex);

                changed = true;

                if (EventSelectedIndexChanged != null)
                {
                    EventSelectedIndexChanged(this, EventArgs.Empty);
                }
            }

            iMutex.WaitOne();

            foreach (TrackCollection c in iCollections)
            {
                c.Y = y;
                bool collapsed = c.Collapsed;
                c.MouseUp(eArgs);
                if (c.Collapsed != collapsed)
                {
                    changed = true;
                    if (!c.Collapsed)
                    {
                        float bottom = c.Bottom + iOffset;
                        if (bottom > Height)
                        {
                            if (c.Height <= Height)
                            {
                                iOffset -= bottom - Height;
                            }
                            else
                            {
                                iOffset = -c.Top;
                            }
                        }
                    }
                }
                y += c.Height;
                index += c.Count;
            }

            iHeight = y;

            ClipOffsetToRange();

            //iLastSelectedIndex = -1;

            int height = iHeight;
            int offset = (int)iOffset;

            iMutex.ReleaseMutex();

            if (changed)
            {
                SetScrollbar(height, offset);
                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            float lines = e.Delta / 120.0f;

            iMutex.WaitOne();

            iOffset += TrackCollection.kItemHeight * lines;

            ClipOffsetToRange();

            scrollBarControl1.Value = (int)-iOffset;

            Console.WriteLine(scrollBarControl1.Value);

            iMutex.ReleaseMutex();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            bool add = ((Form.ModifierKeys & Keys.Control) == Keys.Control);
            MouseEventArgs eArgs = new MouseEventArgs(e.Button, e.Clicks, e.X, (int)(e.Y - iOffset), e.Delta);

            iMutex.WaitOne();

            int index = 0;
            foreach (TrackCollection c in iCollections)
            {
                //int oldHeight = c.Height;
                int i = c.MouseDoubleClick(eArgs, add);
                if (i > -1)
                {
                    iMutex.ReleaseMutex();

                    if (EventItemActivated != null)
                    {
                        EventItemActivated(this, EventArgs.Empty);
                    }

                    return;
                }
                else if (i == -2)
                {
                    /*iHeight -= (oldHeight - c.Height);

                    ClipOffsetToRange();

                    int height = iHeight;
                    int offset = (int)iOffset;

                    iMutex.ReleaseMutex();

                    SetScrollbar(height, offset);

                    Invalidate(true);*/
                    iMutex.ReleaseMutex();

                    if (EventItemActivated != null)
                    {
                        EventItemActivated(this, EventArgs.Empty);
                    }

                    return;
                }
                index += c.Count;
            }

            iMutex.ReleaseMutex();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right || iSelectedIndices.Count == 0)
            {
                return;
            }

            double d = Math.Pow(e.Location.X - iPointMouseDown.X, 2) + Math.Pow(e.Location.Y - iPointMouseDown.Y, 2);

            if (!iDragFinished && d > kStartDragDistance)
            {
                iDragging = true;
                if (ItemDrag != null)
                {
                    ItemDrag(this, new ItemDragEventArgs(e.Button));
                }
                iDragging = false;
                iDragFinished = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            iMutex.WaitOne();

            switch (keyData)
            {
                case Keys.Return:
                    if (iSelectedIndices.Count > 0)
                    {
                        iMutex.ReleaseMutex();

                        if (EventItemActivated != null)
                        {
                            EventItemActivated(this, EventArgs.Empty);
                        }

                        iMutex.WaitOne();
                    }
                    break;

                case Keys.Up:
                case Keys.Shift | Keys.Up:
                    if (iStartKeyRangeSelect)
                    {
                        iFirstSelectedIndex = iLastSelectedIndex;
                        iStartKeyRangeSelect = false;
                    }

                    if (iLastSelectedIndex > 0)
                    {
                        iLastSelectedIndex = iLastSelectedIndex - 1;
                    }

                    if (keyData == (Keys.Shift | Keys.Up))
                    {
                        SelectRange(iFirstSelectedIndex, iLastSelectedIndex);
                    }
                    else
                    {
                        iStartKeyRangeSelect = true;
                        SelectRange(iLastSelectedIndex, iLastSelectedIndex);
                    }

                    EnsureVisible(iLastSelectedIndex);
                    break;

                case Keys.Down:
                case Keys.Shift | Keys.Down:
                    if (iStartKeyRangeSelect)
                    {
                        iFirstSelectedIndex = iLastSelectedIndex;
                        iStartKeyRangeSelect = false;
                    }

                    if (iLastSelectedIndex < iTracks.Count - 1)
                    {
                        iLastSelectedIndex = iLastSelectedIndex + 1;
                    }

                    if (keyData == (Keys.Shift | Keys.Down))
                    {
                        SelectRange(iFirstSelectedIndex, iLastSelectedIndex);
                    }
                    else
                    {
                        iStartKeyRangeSelect = true;
                        SelectRange(iLastSelectedIndex, iLastSelectedIndex);
                    }

                    EnsureVisible(iLastSelectedIndex);
                    break;

                case Keys.PageUp:
                case Keys.Shift | Keys.PageUp:
                    if (iStartKeyRangeSelect)
                    {
                        iFirstSelectedIndex = iLastSelectedIndex;
                        iStartKeyRangeSelect = false;
                    }

                    iLastSelectedIndex = iLastSelectedIndex - 6;
                    if (iLastSelectedIndex < 0)
                    {
                        iLastSelectedIndex = 0;
                    }

                    if (keyData == (Keys.Shift | Keys.PageUp))
                    {
                        SelectRange(iFirstSelectedIndex, iLastSelectedIndex);
                    }
                    else
                    {
                        iStartKeyRangeSelect = true;
                        SelectRange(iLastSelectedIndex, iLastSelectedIndex);
                    }

                    EnsureVisible(iLastSelectedIndex);
                    break;


                case Keys.PageDown:
                case Keys.Shift | Keys.PageDown:
                    if (iStartKeyRangeSelect)
                    {
                        iFirstSelectedIndex = iLastSelectedIndex;
                        iStartKeyRangeSelect = false;
                    }

                    iLastSelectedIndex = iLastSelectedIndex + 6;
                    if (iLastSelectedIndex > iTracks.Count - 1)
                    {
                        iLastSelectedIndex = iTracks.Count - 1;
                    }

                    if (keyData == (Keys.Shift | Keys.PageDown))
                    {
                        SelectRange(iFirstSelectedIndex, iLastSelectedIndex);
                    }
                    else
                    {
                        iStartKeyRangeSelect = true;
                        SelectRange(iLastSelectedIndex, iLastSelectedIndex);
                    }

                    EnsureVisible(iLastSelectedIndex);
                    break;
            }

            iMutex.ReleaseMutex();

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ClipOffsetToRange()
        {
            if (-iOffset > iHeight - Height)
            {
                iOffset = -(iHeight - Height);
            }
            if (-iOffset < 0)
            {
                iOffset = 0;
            }
        }

        private void SetScrollbar(int aHeight, int aOffset)
        {
            if (scrollBarControl1.InvokeRequired)
            {
                scrollBarControl1.BeginInvoke((MethodInvoker)delegate()
                {
                    SetScrollbar(aHeight, aOffset);
                });
            }
            else
            {
                int max = aHeight - DisplayRectangle.Height;
                if(max < 0)
                {
                    max = 0;
                }
                scrollBarControl1.Maximum = max;
                scrollBarControl1.Minimum = 0;
                float ratio = (scrollBarControl1.Maximum > 0) ? DisplayRectangle.Height / (float)scrollBarControl1.Maximum : 0;
                scrollBarControl1.LargeChange = (int)(DisplayRectangle.Height * ratio);
                scrollBarControl1.SmallChange = (int)(TrackCollection.kItemHeight * ratio);
                scrollBarControl1.Value = -aOffset;

                scrollBarControl1.Invalidate();
            }
        }

        private void CreateCollection(int aY, int aStartIndex, IArtworkCache aArtworkCache)
        {
            iLastCollection = new TrackCollection(aY, ClientSize.Width - scrollBarControl1.Width, aStartIndex, aArtworkCache, this, iViewSupport);
            iCollections.Add(iLastCollection);
        }

        private void EventScroll(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            iOffset = -scrollBarControl1.Value;

            if (iDragging)
            {
                iInsertionMark.Update(PointToClient(MousePosition));
            }

            iMutex.ReleaseMutex();
            
            Invalidate();
        }

        private void EventScrollbarDoubleClick(object sender, EventArgs e)
        {
            if (iTrackIndex > -1)
            {
                EnsureVisible(iTrackIndex);
            }
        }

        private void EventChanged(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            if (iBrushBackColour != null)
            {
                iBrushBackColour.Dispose();
                iBrushBackColour = new SolidBrush(iViewSupport.BackColour);
            }
            if (iBrushForeColour != null)
            {
                iBrushForeColour.Dispose();
                iBrushForeColour = new SolidBrush(iViewSupport.ForeColour);
            }
            if (iBrushForeColourMuted != null)
            {
                iBrushForeColourMuted.Dispose();
                iBrushForeColourMuted = new SolidBrush(iViewSupport.ForeColourMuted);
            }
            if (iBrushHighlightBackColour != null)
            {
                iBrushHighlightBackColour.Dispose();
                iBrushHighlightBackColour = new SolidBrush(iViewSupport.HighlightBackColour);
            }
            if (iBrushHighlightForeColour != null)
            {
                iBrushHighlightForeColour.Dispose();
                iBrushHighlightForeColour = new SolidBrush(iViewSupport.HighlightForeColour);
            }
            BackColor = iViewSupport.BackColour;

            TrackCollection.kHeaderHeight = iViewSupport.FontMedium.Height * 2 + 40;
            TrackCollection.kItemHeight = iViewSupport.FontSmall.Height + 4;

            Graphics g = CreateGraphics();
            TrackCollection.kNumberWidth = (int)g.MeasureString("10000", iViewSupport.FontSmall).Width;
            g.Dispose();
            g = null;

            TrackCollection prev = null;
            foreach (TrackCollection c in iCollections)
            {
                c.CalcHeight();

                if (prev != null)
                {
                    c.Y = prev.Bottom;
                }

                prev = c;
            }

            if (prev != null)
            {
                int oldHeight = iHeight;
                iHeight = prev.Top + prev.Height;
                iOffset = (int)(iOffset * (oldHeight / (float)iHeight));
            }

            int height = iHeight;
            int offset = (int)iOffset;

            iMutex.ReleaseMutex();

            SetScrollbar(height, offset);
            Invalidate();
        }

        private const float kStartDragDistance = 25;

        private Mutex iMutex;

        private IArtworkCache iArtworkCache;
        private IViewSupport iViewSupport;

        private Brush iBrushBackColour;
        private Brush iBrushForeColour;
        private Brush iBrushForeColourMuted;
        private Brush iBrushHighlightBackColour;
        private Brush iBrushHighlightForeColour;

        private TrackCollection iLastCollection;

        private bool iBeginUpdate;

        private Point iPointMouseDown;
        private bool iDragging;
        private bool iDragFinished;

        private bool iStartKeyRangeSelect;
        private int iFirstSelectedIndex;
        private int iLastSelectedIndex;
        private bool iSelectLastIndex;

        private List<int> iSelectedIndices;

        private List<T> iTracks;
        private int iTrackIndex;

        private PlaylistWidgetInsertionMark<T> iInsertionMark;
        private Image iImagePlaying;
        private Image iImageAdornmentRed;
        private Image iImageAdornmentAmber;
        private Image iImageAdornmentGreen;

        private float iOffset;
        private int iHeight;
        private List<TrackCollection> iCollections;
    }

    public class PlaylistWidgetInsertionMark<T> where T : MrItem
    {
        public PlaylistWidgetInsertionMark(PlaylistWidget<T> aPlaylistWidget)
        {
            iPlaylistWidget = aPlaylistWidget;
            iIndex = -1;
            iAppearsAfterItem = false;
        }

        public bool AppearsAfterItem
        {
            get
            {
                return iAppearsAfterItem;
            }
            set
            {
                iAppearsAfterItem = value;
                iPlaylistWidget.Invalidate(false);
            }
        }

        public int Index
        {
            get
            {
                return iIndex;
            }
            set
            {
                iIndex = value;
                iPlaylistWidget.Invalidate(false);
            }
        }

        public void Update(Point aPoint)
        {
            iPlaylistWidget.NearestIndex(aPoint, out iIndex, out iAppearsAfterItem);
            iPlaylistWidget.Refresh();
        }

        private PlaylistWidget<T> iPlaylistWidget;
        private int iIndex;
        private bool iAppearsAfterItem;
    }
} // Linn.Kinsky
