using System;
using System.Threading;

using Linn.Kinsky;

using Upnp;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace KinskyTouch
{
    public interface IViewWidgetTimeDisplay
    {
        bool ShowingElapsed { get; }
        void ToggleTimeDisplay();

        void SetTargetSeconds(uint aSeconds);

        void StartSeek();
        void EndSeek();
    }

    internal class ViewWidgetTime : IViewWidgetMediaTime, IViewWidgetTimeDisplay
    {
        public ViewWidgetTime(UIControlWheel aControl, ViewHourGlass aViewHourGlass)
        {
            iControl = aControl;

            iViewHourGlass = aViewHourGlass;
        }

        public void Open()
        {
            lock(this)
            {
                iOpen = true;
            }
        }

        public void Close()
        {
            lock(this)
            {
                iControl.ViewBar.Value = 0;
                iControl.ViewBar.Text = string.Empty;

                iViewHourGlass.Stop();

                UIControl control = iControl as UIControl;
                control.BeginInvokeOnMainThread(delegate {
                    control.Enabled = false;
                });

                iOpen = false;
            }
        }

        public void Initialised()
        {
            lock(this)
            {
                if(iOpen)
                {
                    UIControl control = iControl as UIControl;
                    control.BeginInvokeOnMainThread(delegate {
                        control.Enabled = true;
                    });
                }
            }
        }

        public void SetAllowSeeking(bool aAllowSeeking)
        {
            lock(this)
            {
                iAllowSeeking = aAllowSeeking;
            }
        }

        public void SetTransportState(ETransportState aTransportState)
        {
            lock(this)
            {
                if(iOpen)
                {
                    iTransportState = aTransportState;

                    if(aTransportState == ETransportState.eStopped)
                    {
                        SetSeconds(0);
                    }
                    else
                    {
                        SetSeconds(iSeconds);
                    }

                    iControl.Dimmed = (aTransportState == ETransportState.ePaused);
                    if(aTransportState == ETransportState.eBuffering)
                    {
                        iViewHourGlass.Start();
                    }
                    else
                    {
                        iViewHourGlass.Stop();
                    }
                }
            }
        }

        public void SetDuration(uint aDuration)
        {
            lock(this)
            {
                if(iOpen)
                {
                    iControl.ViewBar.MaxValue = (float)aDuration;
                    iDuration = aDuration;

                    float seek = iDuration / 100.0f;
                    iSeekAmountPerStep = (uint)Math.Round(seek + 0.5f, MidpointRounding.AwayFromZero);
                }
            }
        }

        public void SetSeconds(uint aSeconds)
        {
            lock(this)
            {
                if(iOpen)
                {
                    iSeconds = aSeconds;

                    if(iTransportState != ETransportState.eBuffering && iTransportState != ETransportState.eStopped)
                    {
                        if(!iApplyTargetSeconds)
                        {
                            if(iShowRemaining && iDuration > 0)
                            {
                                iTime = new Time((int)-(iDuration - aSeconds));
                            }
                            else
                            {
                                iTime = new Time((int)aSeconds);
                            }

                            iControl.ViewBar.Text = iTime.ToPrettyString();
                        }
                        
                        iControl.ViewBar.Value = (float)iSeconds;
                    }
                    else
                    {
                        iControl.ViewBar.Value = 0;
                        iControl.ViewBar.Text = "";
                    }
                }
            }
        }

        public event EventHandler<EventArgsSeekSeconds> EventSeekSeconds;

        public bool ShowingElapsed
        {
            get
            {
                return !iShowRemaining;
            }
        }

        public void ToggleTimeDisplay()
        {
            lock(this)
            {
                iShowRemaining = !iShowRemaining;
                SetSeconds(iSeconds);
            }
        }

        public void SetTargetSeconds(uint aSeconds)
        {
            lock(this)
            {
                if(iOpen)
                {
                    if(!iApplyTargetSeconds)
                    {
                        iApplyTargetSeconds = true;

                        iControl.ViewBar.FontColour = UIColor.Yellow;
                        iControl.ViewBar.PreviewEnabled = true;
                    }

                    iTargetSeconds = aSeconds;

                    if(iTransportState != ETransportState.eBuffering)
                    {
                        if (iApplyTargetSeconds)
                        {
                            string t = string.Empty;
                            if (iShowRemaining)
                            {
                                Time time = new Time((int)(iTargetSeconds - iDuration));
                                t = time.ToPrettyString();
                            }
                            else
                            {
                                Time time = new Time((int)iTargetSeconds);
                                t = time.ToPrettyString();
                            }

                            iControl.ViewBar.PreviewValue = iTargetSeconds;
                            iControl.ViewBar.Text = t;
                        }
                    }
                }
            }
        }

        public void StartSeek()
        {
            lock(this)
            {
                if(iOpen && iAllowSeeking && iDuration > 0)
                {
                    Console.WriteLine("StartSeek: " + iTargetSeconds);
                    iSeeking = true;
                    iApplyTargetSeconds = false;
                }
            }
        }

        public void EndSeek()
        {
            lock(this)
            {
                iSeeking = false;
    
                if (iTransportState == ETransportState.eStopped)
                {
                    iControl.ViewBar.Text = string.Empty;
                }
    
                iControl.ViewBar.FontColour = UIColor.White;
                iControl.ViewBar.PreviewValue = 0;
                iControl.ViewBar.PreviewEnabled = false;
    
                iApplyTargetSeconds = false;
                iTargetSeconds = 0;
    
                SetSeconds(iSeconds);
            }
        }

        private void Clicked(object sender, EventArgs e)
        {
            ToggleTimeDisplay();
        }

        private void EventStartSeeking(object sender, EventArgs e)
        {
            StartSeek();
        }

        private void EventEndSeeking(object sender, EventArgs e)
        {
            lock(this)
            {
                if(iOpen)
                {
                    if(iSeeking)
                    {
                        Console.WriteLine("EventEndSeeking " + iTransportState + ", " + iApplyTargetSeconds + ", " + iTargetSeconds + ", " + iSeconds);
        
                        if(iApplyTargetSeconds)
                        {
                            iSeconds = iTargetSeconds;
        
                            if(EventSeekSeconds != null)
                            {
                                Console.WriteLine("EventEndSeeking: " + iTargetSeconds);
                                EventSeekSeconds(this, new EventArgsSeekSeconds(iTargetSeconds));
                            }
                        }
    
                        EndSeek();
                    }
                }
            }
        }

        private void EventCancelSeeking(object sender, EventArgs e)
        {
            lock(this)
            {
                if (iOpen)
                {
                    if(iSeeking)
                    {
                        EndSeek();
                    }
                }
            }
        }

        private void EventSeekForwards(object sender, EventArgs e)
        {
            lock(this)
            {
                if (iOpen && iSeeking)
                {
                    Console.WriteLine("EventSeekForwards");
                    if (!iApplyTargetSeconds)
                    {
                        iTargetSeconds = iSeconds;
                    }
    
                    iTargetSeconds += iSeekAmountPerStep;
    
                    if (iTargetSeconds > iDuration)
                    {
                        iTargetSeconds = iDuration;
                    }
    
                    SetTargetSeconds(iTargetSeconds);
                }
            }
        }

        private void EventSeekBackwards(object sender, EventArgs e)
        {
            lock(this)
            {
                if (iOpen && iSeeking)
                {
                    Console.WriteLine("EventSeekBackwards");
                    if (!iApplyTargetSeconds)
                    {
                        iTargetSeconds = iSeconds;
                    }
    
                    if (iTargetSeconds > iSeekAmountPerStep)
                    {
                        iTargetSeconds -= iSeekAmountPerStep;
                    }
                    else
                    {
                        iTargetSeconds = 0;
                    }
    
                    SetTargetSeconds(iTargetSeconds);
                }
            }
        }

        private bool iOpen;

        private Time iTime;
        private uint iSeconds;
        private uint iDuration;
        private ETransportState iTransportState;

        private bool iAllowSeeking;
        private bool iShowRemaining;

        private uint iSeekAmountPerStep;
        private bool iSeeking;
        private bool iApplyTargetSeconds;
        private uint iTargetSeconds;

        private UIControlWheel iControl;
        private ViewHourGlass iViewHourGlass;
    }

    internal interface IViewWidgetTimePopover
    {
        void Dismiss();
    }

    public partial class ViewWidgetTimeButtons : UIViewController, IViewWidgetMediaTime, IViewWidgetTimePopover
    {
        public ViewWidgetTimeButtons(string aNibName, NSBundle aBundle, IViewWidgetTimeDisplay aViewWidgetTimeDisplay)
            : base(aNibName, aBundle)
        {
            iViewWidgetTimeDisplay = aViewWidgetTimeDisplay;
            iTimer = new System.Threading.Timer(TimerUpdate);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            buttonFfwd.TouchDown += FfwdTouchDown;
            buttonFfwd.TouchUpInside += TouchUpInside;
            buttonFfwd.TouchUpOutside += TouchCancel;
            buttonFfwd.TouchDragOutside += TouchCancel;

            buttonFrwd.TouchDown += FrwdTouchDown;
            buttonFrwd.TouchUpInside += TouchUpInside;
            buttonFrwd.TouchUpOutside += TouchCancel;
            buttonFrwd.TouchDragOutside += TouchCancel;

            buttonTime.TouchDown += TimeTouchDown;

            lock(this)
            {
                buttonFfwd.Enabled = iAllowSeeking && (iDuration > 0);
                buttonFrwd.Enabled = iAllowSeeking && (iDuration > 0);
                buttonTime.Enabled = iAllowSeeking && (iDuration > 0);
            }
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            buttonFfwd.TouchDown -= FfwdTouchDown;
            buttonFfwd.TouchUpInside -= TouchCancel;
            buttonFfwd.TouchUpOutside -= TouchCancel;
            buttonFfwd.TouchDragOutside -= TouchCancel;

            buttonFrwd.TouchDown -= FrwdTouchDown;
            buttonFrwd.TouchUpInside -= TouchCancel;
            buttonFrwd.TouchUpOutside -= TouchCancel;
            buttonFrwd.TouchDragOutside -= TouchCancel;

            buttonTime.TouchDown -= TimeTouchDown;
        }

        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation aToInterfaceOrientation)
        {
            return true;
        }

        public void Open()
        {
            lock(this)
            {
                iOpen = true;
            }
        }

        public void Close()
        {
            lock(this)
            {
                iOpen = false;
            }
        }

        public void Initialised()
        {
            lock(this)
            {
                BeginInvokeOnMainThread(delegate {
                    if(buttonTime != null)
                    {
                        lock(this)
                        {
                            buttonFfwd.Enabled = iAllowSeeking && (iDuration > 0);
                            buttonFrwd.Enabled = iAllowSeeking && (iDuration > 0);
                            buttonTime.Enabled = iAllowSeeking && (iDuration > 0);
                        }
                    }
                });
            }
        }

        public void SetAllowSeeking(bool aAllowSeeking)
        {
            lock(this)
            {
                iAllowSeeking = aAllowSeeking;

                if(iOpen)
                {
                    BeginInvokeOnMainThread(delegate {
                        if(buttonTime != null)
                        {
                            lock(this)
                            {
                                buttonFfwd.Enabled = aAllowSeeking && (iDuration > 0);
                                buttonFrwd.Enabled = aAllowSeeking && (iDuration > 0);
                                buttonTime.Enabled = aAllowSeeking && (iDuration > 0);
                            }
                        }
                    });
                }
            }
        }

        public void SetTransportState(ETransportState aTransportState)
        {
        }

        public void SetDuration(uint aDuration)
        {
            lock(this)
            {
                iDuration = aDuration;

                float seek = aDuration / 100.0f;
                iSeekAmountPerStep = (uint)Math.Round(seek + 0.5f, MidpointRounding.AwayFromZero);

                if(iOpen)
                {
                    BeginInvokeOnMainThread(delegate {
                        if(buttonTime != null)
                        {
                            lock(this)
                            {
                                buttonFfwd.Enabled = iAllowSeeking && (aDuration > 0);
                                buttonFrwd.Enabled = iAllowSeeking && (aDuration > 0);
                                buttonTime.Enabled = iAllowSeeking && (aDuration > 0);
                            }
                        }
                    });
                }
            }
        }

        public void SetSeconds(uint aSeconds)
        {
            lock(this)
            {
                iSeconds = aSeconds;
            }
        }

        public event EventHandler<EventArgsSeekSeconds> EventSeekSeconds;

        public void Dismiss()
        {
            CancelAutoRepeat();
            iViewWidgetTimeDisplay.EndSeek();
        }

        public float RepeatInterval
        {
            set
            {
                iRepeatInterval = value;
            }
        }

        private void TimeTouchDown(object sender, EventArgs e)
        {
            iViewWidgetTimeDisplay.ToggleTimeDisplay();
        }

        private void FfwdTouchDown(object sender, EventArgs e)
        {
            lock(this)
            {
                iTargetSeconds = iSeconds;
            }

            iHeldButton = buttonFfwd as UIButton;

            iViewWidgetTimeDisplay.StartSeek();

            iTimer.Change((int)(kAutoRepeatDelay * 1000.0f), Timeout.Infinite);
        }

        private void FrwdTouchDown(object sender, EventArgs e)
        {
            lock(this)
            {
                iTargetSeconds = iSeconds;
            }

            iHeldButton = buttonFrwd as UIButton;

            iViewWidgetTimeDisplay.StartSeek();

            iTimer.Change((int)(kAutoRepeatDelay * 1000.0f), Timeout.Infinite);
        }

        private void TouchUpInside(object sender, EventArgs e)
        {
            bool open = false;
            lock(this)
            {
                open = iOpen;
            }

            if(open && EventSeekSeconds != null)
            {
                EventSeekSeconds(this, new EventArgsSeekSeconds(iTargetSeconds));
            }

            CancelAutoRepeat();
            iViewWidgetTimeDisplay.EndSeek();
        }

        private void TouchCancel(object sender, EventArgs e)
        {
            CancelAutoRepeat();
            iViewWidgetTimeDisplay.EndSeek();
        }

        private void CancelAutoRepeat()
        {
            iHeldButton = null;
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void TimerUpdate(object aObject)
        {
            BeginInvokeOnMainThread(delegate {
                bool open = false;
                lock(this)
                {
                    open = iOpen;
                }

                if(open)
                {
                    if(iHeldButton == buttonFfwd)
                    {
                        iTargetSeconds += iSeekAmountPerStep;

                        if(iTargetSeconds > iDuration)
                        {
                            iTargetSeconds = iDuration;
                        }
                    }
                    else if(iHeldButton == buttonFrwd)
                    {
                        if(iTargetSeconds > iSeekAmountPerStep)
                        {
                            iTargetSeconds -= iSeekAmountPerStep;
                        }
                        else
                        {
                            iTargetSeconds = 0;
                        }
                    }

                    iViewWidgetTimeDisplay.SetTargetSeconds(iTargetSeconds);

                    iTimer.Change((int)(iRepeatInterval * 1000.0f), Timeout.Infinite);
                }
            });
        }

        private const float kAutoRepeatDelay = 0.25f;

        private bool iOpen;
        private float iRepeatInterval;
        private UIButton iHeldButton;
        private System.Threading.Timer iTimer;

        private uint iSeconds;
        private uint iDuration;
        private bool iAllowSeeking;
        private uint iTargetSeconds;
        private uint iSeekAmountPerStep;

        private IViewWidgetTimeDisplay iViewWidgetTimeDisplay;
    }

    public partial class ViewWidgetTimeRotary : UIViewController, IViewWidgetMediaTime, IViewWidgetTimePopover
    {
        public ViewWidgetTimeRotary(string aNibName, NSBundle aBundle, IViewWidgetTimeDisplay aViewWidgetTimeDisplay)
            : base(aNibName, aBundle)
        {
            iViewWidgetTimeDisplay = aViewWidgetTimeDisplay;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            controlRotary.ImageWheel = KinskyTouch.Properties.ResourceManager.WheelLarge;
            controlRotary.ImageGrip = KinskyTouch.Properties.ResourceManager.WheelGripLarge;
            controlRotary.ImageWheelOver = KinskyTouch.Properties.ResourceManager.WheelLargeOver;

            controlRotary.EventClicked += Clicked;

            controlRotary.EventStartRotation += StartRotation;
            controlRotary.EventEndRotation += EndRotation;
            controlRotary.EventCancelRotation += CancelRotation;

            controlRotary.EventRotateClockwise += RotateClockwise;
            controlRotary.EventRotateAntiClockwise += RotateAntiClockwise;

            lock(this)
            {
                controlRotary.Enabled = iAllowSeeking && (iDuration > 0);
                SetCentreImage();
            }
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            controlRotary.EventClicked -= Clicked;

            controlRotary.EventStartRotation -= StartRotation;
            controlRotary.EventEndRotation -= EndRotation;
            controlRotary.EventCancelRotation -= CancelRotation;

            controlRotary.EventRotateClockwise -= RotateClockwise;
            controlRotary.EventRotateAntiClockwise -= RotateAntiClockwise;
        }

        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation aToInterfaceOrientation)
        {
            return true;
        }

        public void Open()
        {
            lock(this)
            {
                iOpen = true;
            }
        }

        public void Close()
        {
            lock(this)
            {
                BeginInvokeOnMainThread(delegate {
                    if(controlRotary != null)
                    {
                        controlRotary.Enabled = false;
                    }
                    if(imageViewElapsed != null && imageViewRemaining != null)
                    {
                        imageViewElapsed.Alpha = 0.0f;
                        imageViewRemaining.Alpha = 0.0f;
                    }
                });

                iOpen = false;
            }
        }

        public void Initialised()
        {
            lock(this)
            {
                BeginInvokeOnMainThread(delegate {
                    if(controlRotary != null)
                    {
                        lock(this)
                        {
                            controlRotary.Enabled = iAllowSeeking && (iDuration > 0);
                        }
                    }
                    if(imageViewElapsed != null && imageViewRemaining != null)
                    {
                        SetCentreImage();
                    }
                });
            }
        }

        public void SetAllowSeeking(bool aAllowSeeking)
        {
            lock(this)
            {
                iAllowSeeking = aAllowSeeking;

                if(iOpen)
                {
                    BeginInvokeOnMainThread(delegate {
                        if(controlRotary != null)
                        {
                            lock(this)
                            {
                                controlRotary.Enabled = aAllowSeeking && (iDuration > 0);
                            }
                        }
                    });
                }
            }
        }

        public void SetTransportState(ETransportState aTransportState)
        {
        }

        public void SetDuration(uint aDuration)
        {
            lock(this)
            {
                iDuration = aDuration;

                float seek = aDuration / 100.0f;
                iSeekAmountPerStep = (uint)Math.Round(seek + 0.5f, MidpointRounding.AwayFromZero);

                if(iOpen)
                {
                    BeginInvokeOnMainThread(delegate {
                        if(controlRotary != null)
                        {
                            lock(this)
                            {
                                controlRotary.Enabled = iAllowSeeking && (aDuration > 0);
                            }
                        }
                    });
                }
            }
        }

        public void SetSeconds(uint aSeconds)
        {
            lock(this)
            {
                iSeconds = aSeconds;
            }
        }

        public event EventHandler<EventArgsSeekSeconds> EventSeekSeconds;

        public void Dismiss()
        {
            iViewWidgetTimeDisplay.EndSeek();
        }

        private void SetCentreImage()
        {
            UIView.BeginAnimations("toggletime");
            UIView.SetAnimationDuration(0.15f);
            if(iViewWidgetTimeDisplay.ShowingElapsed)
            {
                imageViewElapsed.Alpha = 1.0f;
                imageViewRemaining.Alpha = 0.0f;
            }
            else
            {
                imageViewElapsed.Alpha = 0.0f;
                imageViewRemaining.Alpha = 1.0f;
            }
            UIView.CommitAnimations();
        }

        private void Clicked(object sender, EventArgs e)
        {
            iViewWidgetTimeDisplay.ToggleTimeDisplay();
            SetCentreImage();
        }

        private void StartRotation(object sender, EventArgs e)
        {
            lock(this)
            {
                iTargetSeconds = iSeconds;
            }

            iViewWidgetTimeDisplay.StartSeek();
        }

        private void EndRotation(object sender, EventArgs e)
        {
            bool open = false;
            lock(this)
            {
                open = iOpen;
            }

            if(open && EventSeekSeconds != null)
            {
                EventSeekSeconds(this, new EventArgsSeekSeconds(iTargetSeconds));
            }

            iViewWidgetTimeDisplay.EndSeek();
        }

        private void CancelRotation(object sender, EventArgs e)
        {
            iViewWidgetTimeDisplay.EndSeek();
        }

        private void RotateClockwise(object sender, EventArgs e)
        {
            lock(this)
            {
                iTargetSeconds += iSeekAmountPerStep;

                if(iTargetSeconds > iDuration)
                {
                    iTargetSeconds = iDuration;
                }
            }

            iViewWidgetTimeDisplay.SetTargetSeconds(iTargetSeconds);
        }

        private void RotateAntiClockwise(object sender, EventArgs e)
        {
            lock(this)
            {
                if(iTargetSeconds > iSeekAmountPerStep)
                {
                    iTargetSeconds -= iSeekAmountPerStep;
                }
                else
                {
                    iTargetSeconds = 0;
                }
            }

            iViewWidgetTimeDisplay.SetTargetSeconds(iTargetSeconds);
        }

        private bool iOpen;

        private uint iSeconds;
        private uint iDuration;
        private bool iAllowSeeking;
        private uint iTargetSeconds;
        private uint iSeekAmountPerStep;

        private IViewWidgetTimeDisplay iViewWidgetTimeDisplay;
    }
}

