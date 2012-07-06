
using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace Linn.Songcast
{
    class MainClass
    {
        static void Main (string [] args)
        {
            NSApplication.Init ();
            NSApplication.Main (args);
        }
    }
}    

