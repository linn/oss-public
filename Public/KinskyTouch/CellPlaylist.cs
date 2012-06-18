using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace KinskyTouch
{
    public partial class CellPlaylistFactory : NSObject
    {
        public CellPlaylistFactory()
        {
        }

        public CellPlaylistFactory(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public CellPlaylist Cell
        {
            get
            {
                return cellPlaylist;
            }
        }
    }

    public partial class CellPlaylist : CellLazyLoadImage
    {
        public CellPlaylist(IntPtr aInstance)
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

        public void SetPlaying(bool aPlaying, bool aAnimated)
        {
            BeginInvokeOnMainThread(delegate {
                if(aAnimated)
                {
                    if(aPlaying)
                    {
                        UIView.Transition(imageViewArtwork, imageViewPlaying, 0.5f, UIViewAnimationOptions.LayoutSubviews | UIViewAnimationOptions.TransitionFlipFromRight, null);
                    }
                    else
                    {
                        UIView.Transition(imageViewPlaying, imageViewArtwork, 0.5f, UIViewAnimationOptions.LayoutSubviews | UIViewAnimationOptions.TransitionFlipFromLeft, null);
                    }
                }
                else
                {
                    if(aPlaying)
                    {
                        UIView.Transition(imageViewArtwork, imageViewPlaying, 0.0f, UIViewAnimationOptions.LayoutSubviews | UIViewAnimationOptions.TransitionNone, null);
                    }
                    else
                    {
                        UIView.Transition(imageViewPlaying, imageViewArtwork, 0.0f, UIViewAnimationOptions.LayoutSubviews | UIViewAnimationOptions.TransitionNone, null);
                    }
                }
            });
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

        public string Artist
        {
            set
            {
                BeginInvokeOnMainThread(delegate {
                    labelArtist.Text = value;
                });
            }
        }

        public string Album
        {
            set
            {
                BeginInvokeOnMainThread(delegate {
                    labelAlbum.Text = value;
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