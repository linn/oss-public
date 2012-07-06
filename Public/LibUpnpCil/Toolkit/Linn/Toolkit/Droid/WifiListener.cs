using Linn;
using Android.Content;
using Android.Runtime;
namespace OssToolkitDroid
{

    [BroadcastReceiver]
    public class WifiListener : BroadcastReceiver
    {

        private IHelper iHelper;
        private Scheduler iScheduler;
        // default constructor to satisfy framework requirements, should not be used
        public WifiListener()
            : base()
        {
            Assert.Check(false);
        }

        public WifiListener(System.IntPtr aIntPtr, JniHandleOwnership aHandleOwnership)
            : base()
        {
            iLockObject = new object();
        }

        public WifiListener(IHelper aHelper)
            : base()
        {
            iHelper = aHelper;
            iScheduler = new Scheduler("WifiStateChangeScheduler", 1);
            iLockObject = new object();
        }

        public new void Dispose()
        {
            lock (iLockObject)
            {
                iScheduler.Stop();
                iScheduler = null;
            }
            base.Dispose();
        }

        public override void OnReceive(Context aContext, Intent aIntent)
        {
            Refresh(aContext);
            aIntent.Dispose();
        }

        public void Refresh(Context aContext)
        {
            lock (iLockObject)
            {
                if (iScheduler != null)
                {
                    iScheduler.Schedule(() =>
                    {
                        NetworkInfo.RefreshWifiInfo(aContext);
                        if (iHelper != null)
                        {
                            iHelper.Interface.NetworkChanged();
                        }
                    });
                }
            }
        }

        private object iLockObject;
    }
}