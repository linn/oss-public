using Linn;
using Android.Content;
using Android.OS;
using System;
namespace OssToolkitDroid
{




    [BroadcastReceiver]
    public class ActionUserPresentListener : BroadcastReceiver
    {
        private Scheduler iScheduler;
        public event EventHandler<EventArgs> EventUserPresent;

        // default constructor to satisfy framework requirements, should not be used
        public ActionUserPresentListener()
            : base()
        {
            Assert.Check(false);
        }

        public ActionUserPresentListener(Context aContext)
            : base()
        {
            iScheduler = new Scheduler("ActionUserPresentListenerScheduler", 1);
        }


        public override void OnReceive(Context aContext, Intent aIntent)
        {
            iScheduler.Schedule(() =>
            {
                UserLog.WriteLine("ActionUserPresentListener.OnReceive");
                OnEventUserPresent();
            });
        }

        private void OnEventUserPresent()
        {
            EventHandler<EventArgs> del = EventUserPresent;
            if (del != null)
            {
                del(this, EventArgs.Empty);
            }
        }
    }




    [BroadcastReceiver]
    public class ScreenStateListener : BroadcastReceiver
    {
        private bool iIsScreenOn;
        private Scheduler iScheduler;
        public event EventHandler<EventArgsScreenState> EventScreenStateChanged;

        // default constructor to satisfy framework requirements, should not be used
        public ScreenStateListener()
            : base()
        {
            Assert.Check(false);
        }

        public ScreenStateListener(Context aContext)
            : base()
        {
            iScheduler = new Scheduler("ScreenStateScheduler", 1);
            iIsScreenOn = IsScreenOn(aContext);
        }


        public override void OnReceive(Context aContext, Intent aIntent)
        {
            iScheduler.Schedule(() =>
            {
                bool isScreenOn = IsScreenOn(aContext);
                UserLog.WriteLine("ScreenState.OnReceive: " + isScreenOn);
                OnEventScreenStateChanged(isScreenOn);
                aIntent.Dispose();
            });
        }

        public static bool IsScreenOn(Context aContext)
        {
            return ((PowerManager)aContext.GetSystemService(Context.PowerService)).IsScreenOn;
        }

        private void OnEventScreenStateChanged(bool aIsScreenOn)
        {
            EventHandler<EventArgsScreenState> del = EventScreenStateChanged;
            if (del != null)
            {
                del(this, new EventArgsScreenState(aIsScreenOn));
            }
        }
    }

    public class EventArgsScreenState : EventArgs
    {

        public EventArgsScreenState(bool aIsScreenOn)
        {
            iIsScreenOn = aIsScreenOn;
        }

        public bool IsScreenOn { get { return iIsScreenOn; } }

        private bool iIsScreenOn;
    }




}