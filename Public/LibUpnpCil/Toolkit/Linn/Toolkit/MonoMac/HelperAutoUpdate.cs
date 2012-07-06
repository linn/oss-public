
using System;
using MonoMac.AppKit;
using MonoMac.Foundation;

using Linn;


namespace Linn.Toolkit
{
    public partial class HelperAutoUpdate : NSObject
    {
        partial void UpdateStarted(object sender, EventArgs e)
        {
            NSApplication.SharedApplication.Terminate(this);
        }
    }
}

