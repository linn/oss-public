using System;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace MagicMouseControl
{
    public partial class MainWindow : NSObject
    {
        public MainWindow()
        {
        }

        // Called when created from unmanaged code
        public MainWindow(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            Window.MakeKeyAndOrderFront(this);
        }

        public void Close()
        {
            View.Close();
        }
    }
}
