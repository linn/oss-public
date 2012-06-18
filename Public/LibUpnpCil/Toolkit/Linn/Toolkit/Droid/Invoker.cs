using Linn;
using Android.OS;
using Java.Lang;
using Android.App;
using Android.Content;
using System;

namespace OssToolkitDroid
{

    public class Invoker : IInvoker
    {
        public Invoker(Context aContext)
        {
            iContext = aContext;
            iHandler = new Handler(iContext.MainLooper);
        }

        #region IInvoker Members

        public bool InvokeRequired
        {
            get
            {
                return iContext.MainLooper.Thread.Id != Thread.CurrentThread().Id;
            }
        }

        public void BeginInvoke(System.Delegate aDelegate, params object[] aArgs)
        {
            iHandler.Post((Action)(()=>{
                aDelegate.DynamicInvoke(aArgs);
            }));
        }

        public bool TryBeginInvoke(System.Delegate aDelegate, params object[] aArgs)
        {
            if (InvokeRequired)
            {
                BeginInvoke(aDelegate, aArgs);
                return true;
            }
            return false;
        }

        #endregion
        private Context iContext;
        private Handler iHandler;
    }

}