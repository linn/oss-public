using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Linn
{
    public interface IImage<ImageType>
    {
        ImageType Image { get; }
        int SizeBytes { get; }
    }

    public interface IImageLoader<ImageType>
    {
        IImage<ImageType> LoadImage(Uri aUri, int aDownscaleImageSize);
    }

    public class EventArgsImage<ImageType> : EventArgs
    {
        public EventArgsImage(string aUri, IImage<ImageType> aImage)
        {
            Uri = aUri;
            Image = aImage;
        }

        public string Uri;
        public IImage<ImageType> Image;
    }

    public interface IImageCache<ImageType>
    {
        void Clear();
        event EventHandler<EventArgsImage<ImageType>> EventImageAdded;
        IImage<ImageType> Image(Uri aUri);
        uint MaxCacheSize { get; }
        void Add(string aUri, IImage<ImageType> aImage);
        void Remove(string aUri);
        bool Contains(string aUri);
    }

    public interface IImageUriConverter
    {
        Uri Convert(Uri aUri);
    }

    public class ThreadedImageCache<ImageType> : IImageCache<ImageType>
    {
        public ThreadedImageCache(int aMaxSize, int aDownscaleImageSize, int aThreadCount, IImage<ImageType> aErrorImage, IImageLoader<ImageType> aImageLoader)
        {
            iLockObject = new object();
            iErrorImage = aErrorImage;
            iDownscaleImageSize = aDownscaleImageSize;
            iMaxCacheSize = aMaxSize;
            iImageCache = new Dictionary<string, IImage<ImageType>>();
            iImageCacheUsage = new List<string>();
            iImageLoader = aImageLoader;

            iEvent = new ManualResetEvent(false);
            iPendingRequests = new List<ImageRequest>();

            iThreads = new List<Thread>();

            for (int i = 0; i < aThreadCount; i++)
            {
                Thread t = new Thread(ProcessRequests);
                t.IsBackground = true;
                t.Name = "ImageCache" + i;
                t.Start();
                iThreads.Add(t);
            }
        }

        public IImage<ImageType> Image(Uri aUri)
        {
            lock (iLockObject)
            {
                IImage<ImageType> image;
                string key = aUri.OriginalString;
                if (!iImageCache.TryGetValue(key, out image))
                {
                    ImageRequest request = iPendingRequests.Find((i) => { return (i.Uri == key); });
                    if (request != null)
                    {
                        iPendingRequests.Remove(request);
                    }
                    iPendingRequests.Insert(0, new ImageRequest() { Uri = key });

                    image = null;

                    iEvent.Set();
                }
                else
                {
                    iImageCacheUsage.Remove(key);
                    iImageCacheUsage.Add(key);
                }

                return image;
            }
        }

        public bool Contains(string aUri)
        {
            lock (iLockObject)
            {
                return iImageCache.ContainsKey(aUri);
            }
        }

        public void Clear()
        {
            lock (iLockObject)
            {
                iPendingRequests.Clear();
                iImageCache.Clear();
                iImageCacheUsage.Clear();
            }
        }

        public void Add(string aUri, IImage<ImageType> aImage)
        {
            lock (iLockObject)
            {
                if (!iImageCache.ContainsKey(aUri))
                {
                    int imageSize = aImage.SizeBytes;
                    iCurrentCacheSize += imageSize;
                    iImageCache.Add(aUri, aImage);
                    iImageCacheUsage.Add(aUri);
                    RemoveStaleCacheItems();
                }
            }
        }

        public void Remove(string aUri)
        {
            lock (iLockObject)
            {
                if (iImageCache.ContainsKey(aUri))
                {
                    IImage<ImageType> image = iImageCache[aUri] as IImage<ImageType>;
                    iImageCache.Remove(aUri);
                    iImageCacheUsage.Remove(aUri);
                    iCurrentCacheSize -= image.SizeBytes;
                }
            }
        }

        public uint MaxCacheSize
        {
            get
            {
                return (uint)iMaxCacheSize;
            }
        }
        public event EventHandler<EventArgsImage<ImageType>> EventImageAdded;

        private void ProcessRequests()
        {
            while (true)
            {
                iEvent.WaitOne();

                Monitor.Enter(iLockObject);

                if (iPendingRequests.Count > 0)
                {
                    ImageRequest request = iPendingRequests[0];
                    iPendingRequests.Remove(request);

                    Monitor.Exit(iLockObject);
                    IImage<ImageType> img = iErrorImage;
                    bool failed = true;
                    try
                    {
                        img = iImageLoader.LoadImage(new Uri(request.Uri), iDownscaleImageSize);
                        failed = false;
                    }
                    catch (Exception ex)
                    {
                        UserLog.WriteLine("Error downloading image: " + request.Uri + ", " + ex.ToString());
                    }
                    if (!failed)
                    {
                        Add(request.Uri, img);
                    }

                    EventHandler<EventArgsImage<ImageType>> handler = EventImageAdded;
                    if (handler != null)
                    {
                        handler(this, new EventArgsImage<ImageType>(request.Uri, img));
                    }
                }
                else
                {
                    iEvent.Reset();
                    Monitor.Exit(iLockObject);
                }
            }
        }


        private void RemoveStaleCacheItems()
        {
            while (iCurrentCacheSize > iMaxCacheSize)
            {
                string uriToRemove = iImageCacheUsage[0];
                Remove(uriToRemove);
                Trace.WriteLine(Trace.kKinsky, "ImageCache.RemoveStaleCacheItems: Removed " + uriToRemove);
            }

            Assert.Check(iImageCache.Count == iImageCacheUsage.Count);
            Assert.Check(iCurrentCacheSize <= iMaxCacheSize);
        }

        private object iLockObject;
        private int iMaxCacheSize;
        private Dictionary<string, IImage<ImageType>> iImageCache;
        private List<string> iImageCacheUsage;
        private ManualResetEvent iEvent;
        private List<Thread> iThreads;
        private List<ImageRequest> iPendingRequests;
        private int iCurrentCacheSize;
        private int iDownscaleImageSize;
        private IImage<ImageType> iErrorImage;
        private IImageLoader<ImageType> iImageLoader;

        private class ImageRequest
        {
            public string Uri { get; set; }
        }
    }

    public abstract class AbstractStreamImageLoader<ImageType> : IImageLoader<ImageType>
    {

        public AbstractStreamImageLoader(IImageUriConverter aUriConverter)
        {
            iUriConverter = aUriConverter;
        }

        public IImage<ImageType> LoadImage(Uri aUri, int aDownscaleImageSize)
        {
            using (WebClient web = new WebClient())
            {
                if (iUriConverter != null)
                {
                    aUri = iUriConverter.Convert(aUri);
                }
                using (Stream imgData = web.OpenRead(aUri))
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        byte[] buffer = new byte[8192];
                        int count = imgData.Read(buffer, 0, buffer.Length);
                        while (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                            count = imgData.Read(buffer, 0, buffer.Length);
                        }
                        memory.Seek(0, SeekOrigin.Begin);
                        return CreateImageFromStream(memory, aDownscaleImageSize != 0, aDownscaleImageSize);
                    }
                }
            }
        }

        protected abstract IImage<ImageType> CreateImageFromStream(MemoryStream aStream, bool aDownscaleImage, int aDownscaleImageSize);
        private IImageUriConverter iUriConverter;
    }

    public class DefaultUriConverter : IImageUriConverter
    {
        #region IImageUriConverter Members

        public Uri Convert(Uri aUri)
        {
            return aUri;
        }

        #endregion
    }

    public class ArtworkDownscalingUriConverter : IImageUriConverter
    {
        public ArtworkDownscalingUriConverter(int aDownscaleSize)
        {
            iDownscaleSize = aDownscaleSize;
            iDownscaleRegex = new Regex(string.Format("(?'before'([?&]{0}=)[0-9][0-9]*)", kSizeQuery), RegexOptions.Compiled);
        }

        #region IImageUriConverter Members

        public Uri Convert(Uri aUri)
        {
            string query = aUri.Query;
            if (iDownscaleRegex.IsMatch(query))
            {
                query = iDownscaleRegex.Replace(query, string.Format("?{0}={1}", "Size", iDownscaleSize));
            }
            if (!string.IsNullOrEmpty(aUri.Query))
            {
                return new Uri(aUri.AbsoluteUri.Replace(aUri.Query, query));
            }
            return aUri;
        }

        #endregion

        private Regex iDownscaleRegex;
        private int iDownscaleSize;
        private const string kSizeQuery = "size";
    }



}