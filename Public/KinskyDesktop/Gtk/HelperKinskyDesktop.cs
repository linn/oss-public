using System;
using System.Threading;

using Gtk;

using Linn;
using Linn.Kinsky;

namespace KinskyDesktopGtk
{
    public class HelperKinskyDesktop : HelperKinsky
    {
        private class Invoker : IInvoker
        {
            public Invoker()
            {
                iMainThread = Thread.CurrentThread;
            }

            public bool InvokeRequired
            {
                get
                {
                    return Thread.CurrentThread != iMainThread;
                }
            }

            public void BeginInvoke(Delegate aDelegate, params object[] aArgs)
            {
                Application.Invoke(delegate {
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

            private Thread iMainThread;
        }

        public HelperKinskyDesktop(string[] aArgs)
            : base(aArgs, new Invoker())
        {
        }
    }
}

