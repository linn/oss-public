using System;
using System.Windows.Forms;
using System.Threading;

using Linn;
using Linn.Kinsky;

namespace KinskyPda
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(string[] aArgs)
        {
            ThreadPool.SetMaxThreads(50, 500);

            HelperKinsky helper = new HelperKinsky(aArgs, new Invoker());
            helper.ProcessOptionsFileAndCommandLine();

            KinskyPda.FormKinskyPda form = new KinskyPda.FormKinskyPda(helper);
            Application.Run(form);
        }
		
		class Invoker : Control, IInvoker
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
}
