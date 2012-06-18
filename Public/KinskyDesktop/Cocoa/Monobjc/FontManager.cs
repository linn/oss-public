
using System;

using Monobjc;
using Monobjc.Cocoa;


namespace KinskyDesktop
{
    public static class FontManager
    {
        public static NSFont FontSmall
        {
            get { return NSFont.SystemFontOfSize(10.0f); }
        }

        public static NSFont FontMedium
        {
            get { return NSFont.SystemFontOfSize(12.0f); }
        }

        public static NSFont FontSemiLarge
        {
            get { return NSFont.SystemFontOfSize(13.0f); }
        }

        public static NSFont FontLarge
        {
            get { return NSFont.SystemFontOfSize(16.0f); }
        }
    }
}

