using System.Windows.Forms;
using System;
using Linn;

namespace Linn.Toolkit.WinForms
{
    public class Invoker : Control, IInvoker
    {
        #region IInvoker Members


        void IInvoker.BeginInvoke(Delegate aDelegate, params object[] aArgs)
        {
                try
                {
#if DEBUG || TRACE
                    Trace.WriteLine(Trace.kGui, string.Format("{0} INVOKING {1}", DateTime.Now.ToString(), this.GetCallInfo(aDelegate, aArgs)));
#endif
                    base.BeginInvoke(aDelegate, aArgs);
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
            
        }

        bool IInvoker.TryBeginInvoke(Delegate aDelegate, params object[] aArgs)
        {
            if (base.InvokeRequired)
            {
                base.BeginInvoke(aDelegate, aArgs);
                return true;
            }
            return false;
        }

        #endregion
    }
}