using System;

namespace Linn
{

    /// <summary>
    /// An exception which notifies client code that a method has been called on a different thread to that which the <see cref="Linn.Kinsky.IInvoker"/> executes on (generally the UI thread of the application).
    /// </summary>
    public class InvocationException : ApplicationException
    {
        /// <summary>
        /// Constructor for InvocationException.
        /// </summary>
        public InvocationException() : base("Must call this method on Invoker thread.") { }
    }

    /// <summary>
    /// Interface defining an Invoker object which allows Kinsky to ensure that all application code is executed safely within the context of a single Invoke thread (generally the UI thread of the application).
    /// </summary>
    public interface IInvoker
    {
        /// <summary>
        /// Returns true if the current thread is not the Invoke thread (generally the UI thread of the application).
        /// </summary>
        bool InvokeRequired { get; }

        /// <summary>
        /// Runs a method delegate on the Invoke thread of the application.
        /// </summary>
        /// <param name="aDelegate">The delegate to run on the Invoke thread.</param>
        /// <param name="aArgs">Optional parameters to pass to the delegate.</param>
        void BeginInvoke(Delegate aDelegate, params object[] aArgs);

        /// <summary>
        /// Runs the supplied method delegate on the Invoke thread of the application only if required.
        /// </summary>
        /// <param name="aDelegate">The delegate to run on the Invoke thread.</param>
        /// <param name="aArgs">Optional parameters to pass to the delegate.</param>
        /// <returns>True if BeginInvoke was called, otherwise False.</returns>
        bool TryBeginInvoke(Delegate aDelegate, params object[] aArgs);
    }
}