
using System;
using MonoMac.AppKit;
using MonoMac.Foundation;


namespace Linn.Toolkit.Mac
{
    public class Invoker : IInvoker
    {
        public bool InvokeRequired
        {
            get
            {
                return !NSThread.Current.IsMainThread;
            }
        }
        
        public void BeginInvoke(Delegate aDelegate, params object[] aArgs)
        {
            NSApplication.SharedApplication.BeginInvokeOnMainThread(delegate()
            {
                aDelegate.DynamicInvoke(aArgs);
            });
        }
        
        public bool TryBeginInvoke(Delegate aDelegate, params object[] aArgs)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(aDelegate, aArgs);
                return true;
            }
            return false;
        }
    }
}


