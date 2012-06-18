using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace KinskyTouch
{
    public partial class CellBrowserHeaderFactory : NSObject
    {
        public CellBrowserHeaderFactory()
        {
        }

        public CellBrowserHeaderFactory(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public CellBrowserHeader Cell
        {
            get
            {
                return cellBrowserHeader;
            }
        }
    }

    public partial class CellBrowserHeader : CellLazyLoadImage
    {
        public CellBrowserHeader(IntPtr aInstance)
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

        public string Composer
        {
            set
            {
                BeginInvokeOnMainThread(delegate {
                    labelComposer.Text = value;
                });
            }
        }
    }
}