using Linn;
using Android.Content;
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
        public WifiListener(IHelper aHelper)
            : base()
        {
            iHelper = aHelper;
            iScheduler = new Scheduler("WifiStateChangeScheduler", 1);
        }

        public override void OnReceive(Context aContext, Intent aIntent)
        {
            Refresh(aContext);
        }

        public void Refresh(Context aContext)
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