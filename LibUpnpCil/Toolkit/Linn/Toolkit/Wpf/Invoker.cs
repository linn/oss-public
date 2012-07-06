
using System;
using System.Windows.Threading;


namespace Linn.Toolkit.Wpf
{
    public class Invoker : IInvoker
    {
        public Invoker(Dispatcher aDispatcher)
        {
            iDispatcher = aDispatcher;
        }

        public bool InvokeRequired
        {
            get { return !iDispatcher.CheckAccess(); }
        }
        
        public void BeginInvoke(Delegate aDelegate, params object[] aArgs)
        {
            iDispatcher.BeginInvoke(aDelegate, aArgs);
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

        private Dispatcher iDispatcher;
    }
}

