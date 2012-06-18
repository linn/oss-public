using System.Windows.Forms;
using System;

namespace Linn.Toolkit.WinForms
{
    public class Invoker : Control, IInvoker
    {
        #region IInvoker Members


        void IInvoker.BeginInvoke(Delegate aDelegate, params object[] aArgs)
        {
            base.BeginInvoke(aDelegate, aArgs);
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