using System;

using MonoMac.AppKit;

using Linn.Kinsky;

namespace MagicMouseControl
{
    public partial class MainView : NSView
    {
        public MainView()
        {
        }

        public MainView(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            iTracker = new InputTrackerDoubleTouch();
            iTracker.View = this;
            iTracker.EventBegin += Begin;

            AcceptsTouchEvents = true;
            BecomeFirstResponder();

            iHelper = new HelperKinsky(new string[] { });
            iHelper.ProcessOptionsFileAndCommandLine();
            iHelper.StartupRoom.Set("Sitting Room");

            ViewMaster view = new ViewMaster();

            iViewRoom = new ViewWidgetSelectorRoom();
            view.ViewWidgetSelectorRoom.Add(iViewRoom);

            iViewVolume = new ViewWidgetVolumeControl();
            view.ViewWidgetVolumeControl.Add(iViewVolume);

            Model model = new Model(view, new PlaySupport());
            iMediator = new Mediator(iHelper, model);

            iMediator.Open();

            iHelper.Stack.Start();
        }

        public override void MouseDown(NSEvent aEvent)
        {
            iViewVolume.Mute();
        }

        public override void SwipeWithEvent(NSEvent aEvent)
        {
            Console.WriteLine("Swipe");
        }

        public override void MagnifyWithEvent(NSEvent aEvent)
        {
            base.MagnifyWithEvent(aEvent);
        }

        public override void ScrollWheel(NSEvent aEvent)
        {
            if(Math.Abs(aEvent.DeltaY) > Math.Abs(aEvent.DeltaX))
            {
                if(aEvent.DeltaY > 0.0f)
                {
                    iViewVolume.IncrementVolume();
                }
                else if(aEvent.DeltaY < 0.0f)
                {
                    iViewVolume.DecrementVolume();
                }
            }
        }

        public override void TouchesBeganWithEvent(NSEvent aEvent)
        {
            iTracker.TouchesBeganWithEvent(aEvent);
        }

        public override void TouchesMovedWithEvent(NSEvent aEvent)
        {
            iTracker.TouchesMovedWithEvent(aEvent);
        }

        public override void TouchesEndedWithEvent(NSEvent aEvent)
        {
            iTracker.TouchesEndedWithEvent(aEvent);
        }

        public override void TouchesCancelledWithEvent(NSEvent aEvent)
        {
            iTracker.TouchesCancelledWithEvent(aEvent);
        }

        public void Close()
        {
            iMediator.Close();
            iHelper.Stack.Stop();
        }

        private void Begin(object sender, EventArgs e)
        {
            Console.WriteLine("gesture");
        }

        private InputTrackerDoubleTouch iTracker;
        private HelperKinsky iHelper;

        private Mediator iMediator;
        private ViewWidgetSelectorRoom iViewRoom;
        private ViewWidgetVolumeControl iViewVolume;
    }
}

