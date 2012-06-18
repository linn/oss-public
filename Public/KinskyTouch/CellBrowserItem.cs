using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace KinskyTouch
{
    public partial class CellBrowserItemFactory : NSObject
    {
        public CellBrowserItemFactory()
        {
        }

        public CellBrowserItemFactory(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public CellBrowserItem Cell
        {
            get
            {
                return cellBrowserItem;
            }
        }
    }

    public partial class CellBrowserItem : UITableViewCell
    {
        public CellBrowserItem(IntPtr aInstance)
            : base(aInstance)
        {
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

        public string TrackNumber
        {
            set
            {
                BeginInvokeOnMainThread(delegate {
                    labelTrackNumber.Text = value;
                });
            }
        }
    }
}