using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Linn.Toolkit.Mac
{
    public partial class CrashLogDumperWindow : MonoMac.AppKit.NSWindow
    {
        #region Constructors
        
        // Called when created from unmanaged code
        public CrashLogDumperWindow (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public CrashLogDumperWindow (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        
        // Shared initialization code
        void Initialize ()
        {
        }
        
        #endregion
    }
}

