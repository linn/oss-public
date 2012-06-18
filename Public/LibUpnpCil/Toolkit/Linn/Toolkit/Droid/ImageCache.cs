using Linn;
using System.IO;
using Android.Graphics;
using Android.Widget;
using System;
using Android.Runtime;
using Android.Util;
using Android.Content;

namespace OssToolkitDroid
{

    public class AndroidImageCache : ThreadedImageCache<Bitmap>
    {
        public AndroidImageCache(int aMaxSize, int aDownscaleImageSize, int aThreadCount, IImage<Bitmap> aErrorImage, IImageLoader<Bitmap> aImageLoader, IInvoker aInvoker)
            : base(aMaxSize, aDownscaleImageSize, aThreadCount, aErrorImage, aImageLoader)
        {
            Assert.Check(aInvoker != null);
            iInvoker = aInvoker;
        }
        public IInvoker Invoker { get { return iInvoker; } }
        private IInvoker iInvoker;
    }

    public class AndroidImageLoader : AbstractStreamImageLoader<Bitmap>
    {

        public AndroidImageLoader(IImageUriConverter aUriConverter) : base(aUriConverter) { }

        protected override IImage<Bitmap> CreateImageFromStream(MemoryStream aStream, bool aDownscaleImage, int aDownscaleImageSize)
        {
            Bitmap b = BitmapFactory.DecodeByteArray(aStream.GetBuffer(), 0, (int)aStream.Length);
            if (b == null)
            {
                throw new Exception("ImageLoader: DecodeByteArray returned null...");
            }
            if (aDownscaleImage)
            {
                Size s = CalculateScaledDimensions(b.Width, b.Height, aDownscaleImageSize);
                Bitmap scaled = Bitmap.CreateScaledBitmap(b, (int)s.Width, (int)s.Height, true);
                if (scaled == null)
                {
                    throw new Exception("ImageLoader: CreateScaledBitmap returned null...");
                }
                // we may have upscaled, so ensure the scaled image is smaller than the original
                if (b.RowBytes * b.Height > scaled.RowBytes * scaled.Height)
                {
                    b = scaled;
                }
            }
            AndroidImageWrapper result = new AndroidImageWrapper(b);
            return result;
        }

        private Size CalculateScaledDimensions(int width, int height, int aDownscaleSize)
        {
            int scaledWidth = 0, scaledHeight = 0;
            if (width > height)
            {
                if (height > 0)
                {
                    double ratio = (double)width / (double)height;
                    scaledWidth = aDownscaleSize;
                    scaledHeight = (int)(aDownscaleSize / ratio);
                }
            }
            else
            {
                if (width > 0)
                {
                    double ratio = (double)height / (double)width;
                    scaledHeight = aDownscaleSize;
                    scaledWidth = (int)(aDownscaleSize / ratio);
                }
            }
            return new Size(scaledWidth, scaledHeight);
        }

        private class Size
        {
            public Size(int aWidth, int aHeight)
            {
                this.Width = aWidth;
                this.Height = aHeight;
            }
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }

    public class AndroidImageWrapper : IImage<Bitmap>
    {
        public AndroidImageWrapper(Bitmap aImage)
        {
            Assert.Check(aImage != null);
            iImage = aImage;
            iSizeBytes = aImage.RowBytes * aImage.Height;
        }

        #region IImage Members

        public Bitmap Image
        {
            get
            {
                return iImage;
            }
        }

        public int SizeBytes
        {
            get
            {
                return iSizeBytes;
            }
        }

        #endregion

        private int iSizeBytes;
        private Bitmap iImage;
    }

    public class LazyLoadingImageView : ImageView
    {
        public LazyLoadingImageView(IntPtr aHandle, JniHandleOwnership aHandleOwnership)
            : base(aHandle, aHandleOwnership)
        {
            iLockObject = new object();
        }

        public LazyLoadingImageView(Context aContext, IAttributeSet aAttributeSet)
            : base(aContext, aAttributeSet)
        {
            iLockObject = new object();
        }

        public void LoadImage(AndroidImageCache aImageCache, Uri aUri)
        {
            RemoveCacheHandler();
            Assert.Check(aUri != null);
            Assert.Check(aImageCache != null);
            AddCacheHandler(aImageCache);
            iUri = aUri.OriginalString;
            IImage<Bitmap> image = aImageCache.Image(aUri);
            if (image != null)
            {
                SetImageBitmap(image.Image);
            }
        }

        public override void SetImageURI(Android.Net.Uri uri)
        {
            throw new NotImplementedException("Please use LoadImage() method");
        }

        public override void SetImageResource(int resId)
        {
            RemoveCacheHandler();
            base.SetImageResource(resId);
        }

        public override void SetImageDrawable(Android.Graphics.Drawables.Drawable drawable)
        {
            RemoveCacheHandler();
            base.SetImageDrawable(drawable);
        }

        public override void SetImageBitmap(Bitmap bm)
        {
            RemoveCacheHandler();
            base.SetImageBitmap(bm);
        }

        private void AddCacheHandler(AndroidImageCache aImageCache)
        {
            lock (iLockObject)
            {
                Assert.Check(iImageCache == null);
                iImageCache = aImageCache;
                iImageCache.EventImageAdded += ImageAddedHandler;
            }
        }

        private void RemoveCacheHandler()
        {
            lock (iLockObject)
            {
                if (iImageCache != null)
                {
                    iImageCache.EventImageAdded -= ImageAddedHandler;
                    iImageCache = null;
                }
            }
        }


        private void ImageAddedHandler(object sender, EventArgsImage<Bitmap> e)
        {
            lock (iLockObject)
            {
                if (iImageCache != null)
                {
                    iImageCache.Invoker.BeginInvoke((Action)(() =>
                    {
                        if (e.Uri == iUri)
                        {
                            SetImageBitmap(e.Image.Image);
                        }
                    }));
                }
            }
        }

        private string iUri;
        private AndroidImageCache iImageCache;
        private object iLockObject;
    }
}