
using System;
using System.Collections.Generic;

using Monobjc;
using Monobjc.Cocoa;


namespace Linn.Toolkit.Cocoa
{
    public class OptionPageMonobjc : IDisposable
    {
        public OptionPageMonobjc(IOptionPage aOptionsPage)
        {
            iTabViewItem = new NSTabViewItem();
            iTabViewItem.Label = aOptionsPage.Name;
            iTabViewItem.View.Frame = new NSRect(10, 291, 554, 258);

            iControls = new List<IOptionMonobjc>();

            float y = 240;
            float mid = iTabViewItem.View.Frame.Width * 0.5f;
            foreach (Option option in aOptionsPage.Options)
            {
                NSTextField label = new NSTextField();
                label.SetTitleWithMnemonic(new NSString(option.Name + ":"));
                label.IsSelectable = false;
                label.IsEditable = false;
                label.IsBordered = false;
                label.DrawsBackground = false;
                label.Alignment = NSTextAlignment.NSRightTextAlignment;
                label.SizeToFit();
                label.Frame = new NSRect(10, y - label.Frame.Height, mid - 20, label.Frame.Height);

                iTabViewItem.View.AddSubview(label);

                IOptionMonobjc o = null;

                if (option is OptionEnum || option is OptionNetworkInterface)
                {
                    o = new OptionEnumeratedMonobjc(new NSRect(mid, y, mid - 10, 20), option);
                }
                else if (option is OptionFilePath)
				{
                    o = new OptionFilePathMonobjc(new NSRect(mid, y, mid - 10, 20), option);
				}
                else if (option is OptionFolderPath)
				{
                    o = new OptionFolderPathMonobjc(new NSRect(mid, y, mid - 10, 20), option);
				}
				else if (option is OptionBool)
				{
                    o = new OptionBoolMonobjc(new NSRect(mid, y, mid - 10, 20), option);
				}
                else if (option is OptionListFolderPath)
				{
					o = new OptionListFolderPathMonobjc(new NSRect(mid, y, mid - 10, 20), option);
                }

                if (o != null)
                {
                    iTabViewItem.View.AddSubview(o.View);
                    y -= o.Height;
                    iControls.Add(o);
                }
            }
        }

        public NSTabViewItem TabViewItem
        {
            get { return iTabViewItem; }
        }

        public void Dispose()
        {
            foreach (IOptionMonobjc control in iControls)
            {
                control.Dispose();
            }
        }

        private NSTabViewItem iTabViewItem;
        private List<IOptionMonobjc> iControls;
    }
}
