using Android.App;
using Android.Content;
using System.Reflection;
using Android.OS;
using System.Collections.Generic;
using System;
using Android.Runtime;
namespace Linn
{

    // must inherit from this class to provide an entry assembly
    public abstract class ApplicationDroid : Application
    {
        public ApplicationDroid(IntPtr aHandle, JniHandleOwnership aHandleOwnership) : base(aHandle, aHandleOwnership) { }

        public static ApplicationDroid Instance { get { return iInstance; } }
        public static Assembly EntryAssembly { get { return iEntryAssembly; } }

        public override void OnCreate()
        {
            base.OnCreate();
            iEntryAssembly = GetEntryAssembly();
            iInstance = this;
            iRunningActivities = new List<Activity>();
        }
        public bool IsRunning
        {
            get
            {
                return iRunningActivities.Count > 0;
            }
        }
        public void ActivityStarted(Activity aActivity)
        {
            Assert.Check(!iRunningActivities.Contains(aActivity));
            if (!IsRunning)
            {
                OnStart();
            }
            iRunningActivities.Add(aActivity);
        }
        public void ActivityStopped(Activity aActivity)
        {
            Assert.Check(iRunningActivities.Contains(aActivity));
            iRunningActivities.Remove(aActivity);
            if (!IsRunning)
            {
                OnStop();
            }
        }

        // base classes must override this method to provide the entry assembly
        protected abstract Assembly GetEntryAssembly();
        protected abstract void OnStart();
        protected abstract void OnStop();

        private static Assembly iEntryAssembly;
        private static ApplicationDroid iInstance;
        public List<Activity> iRunningActivities;
    }

    public class ObservableActivity : Activity
    {        
        protected override void OnStart()
        {
            base.OnStart();
            ApplicationDroid.Instance.ActivityStarted(this);
        }
        protected override void OnStop()
        {
            base.OnStop();
            ApplicationDroid.Instance.ActivityStopped(this);
        }
    }

}