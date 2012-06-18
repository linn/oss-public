using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace KinskyTouch
{
    public partial class CellBrowserFactory : NSObject
    {
        public CellBrowserFactory()
        {
        }

        public CellBrowserFactory(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public CellBrowser Cell
        {
            get
            {
                return cellBrowser;
            }
        }
    }

    public partial class CellBrowser : CellLazyLoadImage
    {
        public CellBrowser(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public override UIImage Image
        {
            get
            {
                return imageViewArtwork.Image;
            }
            
            set
            {
                BeginInvokeOnMainThread(delegate {
                    imageViewArtwork.Image = value;
                });
            }
        }

        public string Title
        {
            set
            {
                BeginInvokeOnMainThread(delegate {
                    labelTitle.Text = value;
                });
            }
        }

        public string ArtistAlbum
        {
            set
            {
                BeginInvokeOnMainThread(delegate {
                    labelArtistAlbum.Text = value;
                });
            }
        }

        public string DurationBitrate
        {
            set
            {
                BeginInvokeOnMainThread(delegate {
                    if(string.IsNullOrEmpty(value))
                    {
                        float width = ContentView.Frame.Right - labelTitle.Frame.X;
                        labelTitle.Frame = new System.Drawing.RectangleF(labelTitle.Frame.X, labelTitle.Frame.Y, width, labelTitle.Frame.Height);
                    }
                    else
                    {
                        float width = labelDurationBitrate.Frame.X - labelTitle.Frame.X;
                        labelTitle.Frame = new System.Drawing.RectangleF(labelTitle.Frame.X, labelTitle.Frame.Y, width, labelTitle.Frame.Height);
                    }
                    labelDurationBitrate.Text = value;
                });
            }
        }
    }
}