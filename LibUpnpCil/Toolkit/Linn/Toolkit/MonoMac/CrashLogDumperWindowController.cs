using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Linn.Toolkit.Mac
{
    public partial class CrashLogDumperWindowController : MonoMac.AppKit.NSWindowController, ICrashLogDumper
    {
        #region Constructors
        
        // Called when created from unmanaged code
        public CrashLogDumperWindowController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public CrashLogDumperWindowController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        
        // Call to load from the XIB/NIB file
        public CrashLogDumperWindowController (NSImage aImage, string aTitle, string aProduct, string aVersion) : base ("CrashLogDumperWindow")
        {
            iImage = aImage;
            iTitle = aTitle;
            iProduct = aProduct;
            iVersion = aVersion;

            Initialize ();
        }
        
        // Shared initialization code
        void Initialize ()
        {
        }
        
        #endregion

        public override void AwakeFromNib()
        {
            Image.Image = iImage;
            HeaderText.StringValue = iTitle + " has encountered a problem and needs to close. We are sorry for the inconvenience";
            FooterText.StringValue = "We have created an error report that you can send to help us improve " + iTitle + ".";

            ButtonDetails.Activated += ToggleDetails;
            ButtonCancel.Activated += Cancel;
            ButtonSend.Activated += Send;

            Window.Title = iTitle;
            Window.WillClose += WindowWillClose;

            Window.SetFrame(new RectangleF(Window.Frame.X, Window.Frame.Y, Window.Frame.Width, 217), false, false);
        }

        public void Dump(CrashLog aCrashLog)
        {
            InvokeOnMainThread(delegate {
                iReportText = aCrashLog.ToString();
                Details.Editable = true;
                Details.InsertText(new NSString(iReportText));
                Details.ScrollRangeToVisible(new NSRange(0, 0));
                Details.SetSelectedRange(new NSRange(0, 0));
                Details.Editable = false;
    
                // show the window modally
                Window.Center();
                Window.MakeKeyAndOrderFront(this);
    
                NSApplication.SharedApplication.RunModalForWindow(Window);
    
                Window.OrderOut(this);
            });
        }

        private void WindowWillClose(object sender, EventArgs e)
        {
            NSApplication.SharedApplication.StopModal();
        }

        private void Cancel(object sender, EventArgs e)
        {
            Close ();
        }

        private void Send(object sender, EventArgs e)
        {
            DebugReport report = new DebugReport("Crash log generated by " + iProduct + " ver " + iVersion);
            report.Post(iTitle, iReportText);
            Close ();
        }

        private void ToggleDetails(object sender, EventArgs e)
        {
            RectangleF rect = Window.Frame;
            if(rect.Height == 535)
            {
                rect = new RectangleF(rect.X, rect.Y + 318, rect.Width, 217);
            }
            else
            {
                rect = new RectangleF(rect.X, rect.Y - 318, rect.Width, 535);
            }
            Window.SetFrame(rect, true, true);
        }

        private NSImage iImage;
        private string iTitle;
        private string iProduct;
        private string iVersion;
        private string iReportText;
    }
}

