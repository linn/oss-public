
using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Linn;


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
                try
                {
#if DEBUG || TRACE
                    Trace.WriteLine(Trace.kGui, string.Format("{0} INVOKING {1}", DateTime.Now.ToString(), this.GetCallInfo(aDelegate, aArgs)));
#endif
                    aDelegate.DynamicInvoke(aArgs);
#if DEBUG || TRACE
                    Trace.WriteLine(Trace.kGui, string.Format("{0} INVOKED {1}", DateTime.Now.ToString(), this.GetCallInfo(aDelegate, aArgs)));
#endif
                }
                catch (System.Exception ex)
                {
                    UserLog.WriteLine("Exception: " + ex);
                    UserLog.WriteLine("Invocation details: " + this.GetCallInfo(aDelegate, aArgs));
                    throw ex;
                }
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


