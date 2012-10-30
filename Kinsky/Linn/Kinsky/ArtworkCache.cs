using System;
using System.IO;
using System.Net;
using System.Drawing;
using System.Threading;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace Linn.Kinsky
{
    public interface IArtwork
    {
        Uri Uri { get; }
        bool Error { get; }
        Image Image { get; }
        Image ImageReflected { get; }
    }

    internal class Artwork : IArtwork
    {
        public Artwork(Uri aUri, Uri aUriDownload)
        {
            iUri = aUri;
            iUriDownload = aUriDownload;
            iError = false;
        }

        internal void Dispose()
        {
            if (iArtwork != null)
            {
                iArtwork.Dispose();
                iArtwork = null;
            }

            if (iArtworkReflected != null)
            {
                iArtworkReflected.Dispose();
                iArtworkReflected = null;
            }

            iError = false;
        }

        public Uri Uri
        {
            get
            {
                return iUri;
            }
        }

        public Uri UriDownload
        {
            get
            {
                return iUriDownload;
            }
        }

        public bool Error
        {
            get
            {
                return iError;
            }
            internal set
            {
                iError = value;
            }
        }

        public Image Image
        {
            get
            {
                return iArtwork;
            }
            internal set
            {
                iArtwork = value;
            }
        }

        public Image ImageReflected
        {
            get
            {
                return iArtworkReflected;
            }
            internal set
            {
                iArtworkReflected = value;
            }
        }

        private Uri iUri;
        private Uri iUriDownload;
        private bool iError;

        private Image iArtwork;
        private Image iArtworkReflected;
    }

    public class EventArgsArtwork : EventArgs
    {
        public EventArgsArtwork(IArtwork aArtwork)
        {
            Artwork = aArtwork;
        }

        public IArtwork Artwork;
    }

    public interface IArtworkCache
    {
        IArtwork Artwork(Uri aUri);
        void Clear();

        uint MaxImageCount { get; }
        uint MaxImageSize { get; }

        // NOTE: to be removed
        Image ReflectImage(Image aImage);
        Size ImageSize(Image aImage, uint aWidth, uint aHeight);

        event EventHandler<EventArgsArtwork> EventUpdated;
    }

    public class ArtworkCache : IArtworkCache
    {
        internal class WebRequestState
        {
            public WebRequestState(HttpWebRequest aRequest, Artwork aArtwork)
            {
                Request = aRequest;
                Artwork = aArtwork;
            }

            public HttpWebRequest Request;
            public Artwork Artwork;
        }

        public enum ECacheSize
        {
            eSmall,
            eMedium,
            eLarge
        }

        public ArtworkCache(ECacheSize aAlgorithm)
        {
            iMutex = new Mutex(false);
            iArtworkCache = new Dictionary<string, Artwork>();
            iArtworkCacheUsage = new List<string>();

            iMutexRequest = new Mutex(false);

            iSemaphore = new Semaphore(kMaxOutstandingRequests, kMaxOutstandingRequests);
            iPendingList = new List<Artwork>();

            iManualResetEvent = new ManualResetEvent(false);

            iThread = new Thread(Run);
            iThread.IsBackground = true;
            iThread.Name = "ArtworkCache";
            iThread.Priority = kThreadPriority;
            iThread.Start();

            Algorithm = aAlgorithm;
        }

        public ECacheSize Algorithm
        {
            get
            {
                return iAlgorithm;
            }
            set
            {
                iAlgorithm = value;
                switch (iAlgorithm)
                {
                    case ECacheSize.eSmall:
                        iMaxCacheSize = kMaxCacheSizeSmall;
                        iMaxImageSize = kMaxImageSizeSmall;
                        break;

                    case ECacheSize.eMedium:
                        iMaxCacheSize = kMaxCacheSizeMedium;
                        iMaxImageSize = kMaxImageSizeMedium;
                        break;

                    case ECacheSize.eLarge:
                        iMaxCacheSize = kMaxCacheSizeLarge;
                        iMaxImageSize = kMaxImageSizeLarge;
                        break;
                }
                RemoveStaleCacheItems();
            }
        }

        public uint MaxImageCount
        {
            get
            {
                return iMaxCacheSize;
            }
        }

        public uint MaxImageSize
        {
            get
            {
                return iMaxImageSize;
            }
        }

        public IArtwork Artwork(Uri aUri)
        {
            Assert.Check(aUri != null);

            string query = aUri.Query;
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(string.Format("(?'before'([?&]{0}=)[0-9][0-9]*)", kSizeQuery));
            if (r.IsMatch(query))
            {
                query = r.Replace(query, string.Format("?{0}={1}", kSizeQuery, iMaxImageSize));
            }

            Uri uri = aUri;
            if (!string.IsNullOrEmpty(aUri.Query))
            {
                uri = new Uri(aUri.AbsoluteUri.Replace(aUri.Query, query));
            }

            iMutex.WaitOne();

            Artwork value;
            if (iArtworkCache.TryGetValue(uri.AbsoluteUri, out value))
            {
                iArtworkCacheUsage.Remove(uri.AbsoluteUri);
                iArtworkCacheUsage.Add(uri.AbsoluteUri);

                iMutex.ReleaseMutex();

                return value;
            }

            value = new Artwork(aUri, uri);
            iArtworkCache.Add(value.UriDownload.AbsoluteUri, value);
            iArtworkCacheUsage.Add(value.UriDownload.AbsoluteUri);

            RemoveStaleCacheItems();

            iMutex.ReleaseMutex();

            Download(value);

            return value;
        }

        public void Clear()
        {
            iMutex.WaitOne();

            foreach (Artwork a in iArtworkCache.Values)
            {
                a.Dispose();
            }

            iPendingList.Clear();
            iArtworkCache.Clear();
            iArtworkCacheUsage.Clear();

            iMutex.ReleaseMutex();
        }

        public Image ReflectImage(Image aImage)
        {
            int height = (int)(aImage.Height * (kReflectivity / 255.0f));
            Bitmap dest = new Bitmap(aImage.Width, height, PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, aImage.Width, height);

            using (Graphics g = Graphics.FromImage(dest))
            {
                g.DrawImage(aImage, rect, new Rectangle(0, aImage.Height - height, aImage.Width, height), GraphicsUnit.Pixel);
                dest.RotateFlip(RotateFlipType.RotateNoneFlipY);
                using (LinearGradientBrush b = new LinearGradientBrush(rect, Color.FromArgb((int)(255 - kReflectivity), Color.Black), Color.FromArgb(255, Color.Black), 90, false))
                {
                    g.FillRectangle(b, rect);
                }
            }

            return dest;
        }

        public Size ImageSize(Image aImage, uint aWidth, uint aHeight)
        {
            Size size = new Size((int)aWidth, (int)aHeight);
            if (aImage.Width > aImage.Height)
            {
                size.Height = (int)(aWidth * (aImage.Height / (float)aImage.Width));
            }
            else if (aImage.Height > aImage.Width)
            {
                size.Width = (int)(aHeight * (aImage.Width / (float)aImage.Height));
            }
            return size;
        }

        public event EventHandler<EventArgsArtwork> EventUpdated;

        private void Run()
        {
            while (true)
            {
                iManualResetEvent.WaitOne();

                iMutexRequest.WaitOne();

                if (iPendingList.Count > 0)
                {
                    Artwork artwork = iPendingList[0];
                    iPendingList.RemoveAt(0);

                    //Console.WriteLine("ArtworkCache.Run: iPendingList.Count=" + iPendingList.Count);

                    iMutexRequest.ReleaseMutex();

                    iSemaphore.WaitOne();

                    ProcessRequest(artwork);
                }
                else
                {
                    iManualResetEvent.Reset();
                    //Console.WriteLine("ArtworkCache.Run: Waiting for next request; nothing to do");
                    iMutexRequest.ReleaseMutex();
                }
            }
        }

        internal void Download(Artwork aArtwork)
        {
            iMutexRequest.WaitOne();

            iPendingList.Remove(aArtwork);

            if (!aArtwork.Error && aArtwork.Image == null)
            {
                iPendingList.Insert(0, aArtwork);
                iManualResetEvent.Set();
            }

            iMutexRequest.ReleaseMutex();
        }

        private void ProcessRequest(Artwork aArtwork)
        {
            Trace.WriteLine(Trace.kKinsky, "ArtworkCache.ProcessRequest: Downloading " + aArtwork.UriDownload);

            try
            {
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(aArtwork.UriDownload);
                wreq.Proxy = new WebProxy();
                wreq.KeepAlive = false;
                wreq.Pipelined = false;
                wreq.AllowWriteStreamBuffering = true;
                WebRequestPool.QueueJob(new JobGetResponse(GetResponseCallback, new WebRequestState(wreq, aArtwork)));
            }
            catch (Exception e)
            {
                DownloadError(aArtwork, e.Message);
            }
        }

        private void GetResponseCallback(object aResult)
        {
            WebRequestState wreq = aResult as WebRequestState;
            Artwork artwork = wreq.Artwork;

            try
            {
                HttpWebResponse wresp = (HttpWebResponse)wreq.Request.GetResponse();
                try
                {
                    Stream stream = wresp.GetResponseStream();
                    if (stream != null)
                    {
                        try
                        {
                            Bitmap image = new Bitmap(stream);

                            try
                            {
                                Image scaledImage = ScaleImage(image);
                                Image reflectedImage = ReflectImage(scaledImage);

                                artwork.Image = scaledImage;
                                artwork.ImageReflected = reflectedImage;
                                artwork.Error = false;
                            }
                            catch (Exception e)
                            {
                                DownloadError(artwork, e.Message);
                                return;
                            }

                            image.Dispose();

                            if (EventUpdated != null)
                            {
                                EventUpdated(this, new EventArgsArtwork(artwork));
                            }

                            RequestCompleted(artwork);

                            return;
                        }
                        catch (Exception e)
                        {
                            DownloadError(artwork, e.Message);
                            return;
                        }
                        finally
                        {
                            stream.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    DownloadError(artwork, e.Message);
                    return;
                }
                finally
                {
                    if (wresp != null)
                    {
                        wresp.Close();
                    }
                }
            }
            catch (WebException e)
            {
                DownloadError(artwork, e.Message);
                return;
            }

            DownloadError(artwork, "Unknown error");
        }

        private void DownloadError(Artwork aArtwork, string aMessage)
        {
            UserLog.WriteLine("Failed to download " + aArtwork.UriDownload + " (" + aMessage + ")");
            Trace.WriteLine(Trace.kKinsky, "Failed to download " + aArtwork.UriDownload + " (" + aMessage + ")");

            RequestCompleted(aArtwork);

            iMutex.WaitOne();

            if (iArtworkCache.ContainsKey(aArtwork.UriDownload.AbsoluteUri))
            {
                iMutex.ReleaseMutex();

                aArtwork.Error = true;
            }
            else
            {
                iMutex.ReleaseMutex();
            }

            if (EventUpdated != null)
            {
                EventUpdated(this, new EventArgsArtwork(aArtwork));
            }
        }

        private void RequestCompleted(Artwork aArtwork)
        {
            iMutexRequest.WaitOne();

            if (iPendingList.Count > 0)
            {
                iManualResetEvent.Set();
            }

            iMutexRequest.ReleaseMutex();

            iSemaphore.Release();
        }

        private Bitmap ScaleImage(Bitmap aBitmap)
        {
            if (aBitmap.Width != iMaxImageSize && aBitmap.Height != iMaxImageSize)
            {
                Size size = ImageSize(aBitmap, iMaxImageSize, iMaxImageSize);

                Bitmap image = new Bitmap(size.Width, size.Height);

                Graphics g = Graphics.FromImage(image);
                g.DrawImage(aBitmap, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, aBitmap.Width, aBitmap.Height), GraphicsUnit.Pixel);
                g.Dispose();

                return image;
            }
            else
            {
                return aBitmap.Clone() as Bitmap;
            }
        }

        private void RemoveStaleCacheItems()
        {
            while (iArtworkCache.Count > iMaxCacheSize)
            {
                string uriToRemove = iArtworkCacheUsage[0];
                if (iArtworkCache[uriToRemove] != null)
                {
                    iArtworkCache[uriToRemove].Dispose();
                    iArtworkCache[uriToRemove] = null;
                }
                iArtworkCache.Remove(uriToRemove);
                iArtworkCacheUsage.RemoveAt(0);

                Trace.WriteLine(Trace.kKinsky, "ArtworkCache.RemoveStaleCacheItems: Removed " + uriToRemove);
            }

            Assert.Check(iArtworkCache.Count == iArtworkCacheUsage.Count);
            Assert.Check(iArtworkCacheUsage.Count <= iMaxCacheSize);
            Assert.Check(iArtworkCache.Count <= iMaxCacheSize);
        }

        private const string kSizeQuery = "size";

        private const uint kReflectivity = 90;

        private const uint kMaxCacheSizeSmall = 100;
        private const uint kMaxCacheSizeMedium = 3000;
        private const uint kMaxCacheSizeLarge = 5000;

        private const uint kMaxImageSizeSmall = 128;
        private const uint kMaxImageSizeMedium = 256;
        private const uint kMaxImageSizeLarge = 512;

        private const int kMaxOutstandingRequests = 2;
        private const ThreadPriority kThreadPriority = ThreadPriority.Normal;
        private Semaphore iSemaphore;
        private List<Artwork> iPendingList;

        private ECacheSize iAlgorithm;
        private uint iMaxCacheSize;
        private uint iMaxImageSize;

        private Mutex iMutex;
        private Dictionary<string, Artwork> iArtworkCache;
        private List<string> iArtworkCacheUsage;

        private Mutex iMutexRequest;
        private Thread iThread;

        private ManualResetEvent iManualResetEvent;
    }
} // Linn.Kinsky