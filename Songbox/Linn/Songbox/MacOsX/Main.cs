using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace Linn.Songbox
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

