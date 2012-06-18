using System;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using Linn.Kinsky;

namespace KinskyTouch
{
    public partial class CellSenderController : NSObject
    {
        public CellSenderController(IControllerRoomSelector aController)
        {
            iController = aController;
        }

        public void Initialise()
        {
            cellSender.ButtonRoom.TouchUpInside += TouchUpInside;
        }

        public CellSender Cell
        {
            get
            {
                return cellSender;
            }
        }

        public void SetRoom(Room aRoom)
        {
            iRoom = aRoom;
            SetButtonState();
        }

        public void SetPlaying(bool aPlaying)
        {
            iPlaying = aPlaying;
            SetButtonState();
        }

        private void TouchUpInside(object sender, EventArgs e)
        {
            iController.Select(iRoom);
        }

        private void SetButtonState()
        {
            cellSender.ButtonRoom.Hidden = !iPlaying || (iRoom == null);
        }

        private IControllerRoomSelector iController;
        private bool iPlaying;
        private Room iRoom;
    }

    partial class CellSender : CellLazyLoadImage
    {
        public CellSender(IntPtr aInstance)
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

                controller.SetPlaying(aPlaying);
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

        public UIButton ButtonRoom
        {
            get
            {
                return buttonRoom;
            }
        }

        public void SetRoom(Room aRoom)
        {
            controller.SetRoom(aRoom);
        }
    }
}