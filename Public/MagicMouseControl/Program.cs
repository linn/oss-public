using System;

using MonoMac.AppKit;

namespace MagicMouseControl
{
    public static class Program
    {
        static void Main(string[] aArgs)
        {
            NSApplication.Init();
            NSApplication.Main(aArgs);
        }
    }
}

