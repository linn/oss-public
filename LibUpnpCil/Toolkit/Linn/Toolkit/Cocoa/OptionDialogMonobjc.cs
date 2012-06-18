
using System;
using System.Collections.Generic;

using Monobjc;
using Monobjc.Cocoa;


namespace Linn.Toolkit.Cocoa
{
    public class OptionDialogMonobjc
    {
        public OptionDialogMonobjc(IList<IOptionPage> aOptionPages)
        {
            // create a list of option pages
            iPages = new List<OptionPageMonobjc>();
            foreach (IOptionPage page in aOptionPages)
            {
                iPages.Add(new OptionPageMonobjc(page));
            }

            // create main window for the dialog
            iWindow = new NSWindow(new NSRect(0, 0, 800, 320),
                                   NSWindowStyleMasks.NSClosableWindowMask |
                                   NSWindowStyleMasks.NSTitledWindowMask,
                                   NSBackingStoreType.NSBackingStoreBuffered,
                                   false);
            iWindow.Title = NSString.StringWithUTF8String("User Options");
            iWindow.SetDelegate(d =>
            {
                d.WindowShouldClose += delegate(Id aSender) { return true; };
                d.WindowWillClose += delegate(NSNotification aNotification) { NSApplication.NSApp.AbortModal(); };
            });
            iWindow.IsReleasedWhenClosed = false;

            // create a view for the window content
            NSView view = new NSView();
            iWindow.ContentView = view;

            // create the tab view
            NSTabView tab = new NSTabView(new NSRect(13, 10, 774, 304));
            tab.AutoresizingMask = NSResizingFlags.NSViewMinXMargin | NSResizingFlags.NSViewMaxXMargin |
                                   NSResizingFlags.NSViewMinYMargin | NSResizingFlags.NSViewMaxYMargin;

            foreach (OptionPageMonobjc page in iPages)
            {
                tab.AddTabViewItem(page.TabViewItem);
            }

            view.AddSubview(tab);

            // view have been added to the window so they can be released
            view.Release();
            tab.Release();
        }
		
		public void Open()
		{
            // assert this is run once
            Assert.Check(iWindow != null);

            // run the window modally
            NSApplication.NSApp.RunModalForWindow(iWindow);

            // clean up
            foreach (OptionPageMonobjc page in iPages)
            {
                page.Dispose();
            }
            iWindow.Release();
            iWindow = null;
		}
		
		private NSWindow iWindow;
        private List<OptionPageMonobjc> iPages;
    }
}

