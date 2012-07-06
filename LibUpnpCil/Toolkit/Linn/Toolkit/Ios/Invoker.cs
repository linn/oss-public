using System;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Linn.Toolkit.Ios
{
    public class Invoker : IInvoker
    {
        public void BeginInvoke(Delegate aDelegate, params object[] aArgs)
        {
            UIApplication.SharedApplication.BeginInvokeOnMainThread(delegate {
                aDelegate.DynamicInvoke(aArgs);
            });
        }
    
        public bool TryBeginInvoke(Delegate aDelegate, params object[] aArgs)
        {
            if(InvokeRequired)
            {
                BeginInvoke(aDelegate, aArgs);
                return true;
            }
    
            return false;
        }
    
        public bool InvokeRequired
        {
            get
            {
                return !NSThread.Current.IsMainThread;
            }
        }
    }
}