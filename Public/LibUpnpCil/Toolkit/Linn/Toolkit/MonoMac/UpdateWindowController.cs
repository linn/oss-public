using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Linn.Toolkit.Mac
{
    public partial class UpdateWindowController : MonoMac.AppKit.NSWindowController
    {
        #region Constructors
        
        // Called when created from unmanaged code
        public UpdateWindowController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public UpdateWindowController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        // Call to load from the XIB/NIB file
        public UpdateWindowController (NSImage aLogo) : base ("UpdateWindow")
        {
            iLogo = aLogo;
            
            Initialize ();
        }
        
        // Shared initialization code
        void Initialize ()
        {
        }
        
        #endregion
        
        public NSTextField Text1
        {
            get { return iText1; }
        }
        
        public NSTextField Text2
        {
            get { return iText2; }
        }

        public NSButton ButtonUpdate
        {
            get { return iButtonUpdate; }
        }
        
        public NSButton ButtonClose
        {
            get { return iButtonClose; }
        }
        
        public NSButton ButtonDetails
        {
            get { return iButtonDetails; }
        }
        
        public NSButton ButtonAutoCheck
        {
            get { return iButtonAutoCheck; }
        }
        
        public NSProgressIndicator Progress
        {
            get { return iProgress; }
        }
        
        public NSImageView Image
        {
            get { return iImage; }
        }
        
        public MonoMac.WebKit.WebView WebView
        {
            get { return iWebView; }
        }
        
        public override void AwakeFromNib()
        {
            Image.Image = iLogo;

            ButtonDetails.Activated += ButtonDetailsClicked;
            ButtonClose.Activated += ButtonCloseClicked;

            // set the initial height of the window
            iHeightWithDetails = Window.Frame.Height;
            RectangleF rect = Window.Frame;
            rect = new RectangleF(rect.X, rect.Y + iDetailsHeight, rect.Width, iHeightWithDetails-iDetailsHeight);
            Window.SetFrame(rect, false);
        }

        private void ButtonDetailsClicked(object sender, EventArgs e)
        {
            RectangleF rect = Window.Frame;
            
            if (rect.Height == iHeightWithDetails)
            {
                rect = new RectangleF(rect.X, rect.Y + iDetailsHeight, rect.Width, iHeightWithDetails-iDetailsHeight);
            }
            else
            {
                rect = new RectangleF(rect.X, rect.Y - iDetailsHeight, rect.Width, iHeightWithDetails);
            }
            
            Window.SetFrame(rect, true, true);            
        }

        private void ButtonCloseClicked(object sender, EventArgs e)
        {
            Close();
        }

        
        private NSImage iLogo;
        private float iHeightWithDetails;
        private const float iDetailsHeight = 300;
    }


    // Implementation of the abstract ViewAutoUpdateStandard class in the Linn.Toolkit namespace
    public class ViewAutoUpdateStandard : Linn.Toolkit.ViewAutoUpdateStandard
    {
        public ViewAutoUpdateStandard(NSImage aLogo)
        {
            iView = new UpdateWindowController(aLogo);
            iView.LoadWindow();
        }

        #region Implementation of abstract interface
        
        protected override string Text1
        {
            set { iView.Text1.StringValue = value; }
        }
        
        protected override string Text2
        {
            set { iView.Text2.StringValue = value; }
        }
        
        protected override string ButtonCloseText
        {
            set { iView.ButtonClose.Title = value; }
        }

        protected override void StartProgress(bool aIsIndeterminate)
        {
            iView.Progress.Indeterminate = aIsIndeterminate;
            iView.Progress.StartAnimation(iView);
        }
        
        protected override void StopProgress()
        {
            iView.Progress.StopAnimation(iView);
            iView.Progress.Indeterminate = false;
        }

        protected override bool ProgressHidden
        {
            set { iView.Progress.Hidden = value; }
        }
        
        protected override int ProgressValue
        {
            set { iView.Progress.DoubleValue = value; }
        }

        protected override bool ButtonDetailsEnabled
        {
            set { iView.ButtonDetails.Enabled = value; }
        }
        
        protected override bool ButtonUpdateEnabled
        {
            set { iView.ButtonUpdate.Enabled = value; }
        }
        
        protected override bool ButtonCloseEnabled
        {
            set { iView.ButtonClose.Enabled = value; }
        }

        protected override void SetButtonUpdateAsDefault()
        {
            iView.ButtonAutoCheck.KeyEquivalent = string.Empty;
            iView.ButtonClose.KeyEquivalent = string.Empty;
            iView.ButtonDetails.KeyEquivalent = string.Empty;
            iView.ButtonUpdate.KeyEquivalent = "\r";
        }
        
        protected override void SetButtonCloseAsDefault()
        {
            iView.ButtonAutoCheck.KeyEquivalent = string.Empty;
            iView.ButtonUpdate.KeyEquivalent = string.Empty;
            iView.ButtonDetails.KeyEquivalent = string.Empty;
            iView.ButtonClose.KeyEquivalent = "\r";
        }

        protected override bool ButtonAutoCheckHidden
        {
            set { iView.ButtonAutoCheck.Hidden = value; }
        }

        protected override bool WindowHidden
        {
            set {
                if (!value) {
                    iView.Window.Center();
                    iView.Window.MakeKeyAndOrderFront(iView);
                    NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
                }
                else {
                    iView.Window.OrderOut(iView);
                }
            }
        }
        
        protected override string WebViewUri
        {
            set { iView.WebView.MainFrameUrl = value; }
        }

        protected override bool ShowCompatibilityBreak(string aButtonUpdate, string aButtonCancel, string aMessage, string aInformation)
        {
            NSAlert alert = new NSAlert();

            alert.AddButton(aButtonUpdate);
            alert.AddButton(aButtonCancel);
            alert.MessageText = aMessage;
            alert.InformativeText = aInformation;
            alert.AlertStyle = NSAlertStyle.Warning;

            NSAlertButtonReturn returnCode = (NSAlertButtonReturn)alert.RunModal();

            return (returnCode == NSAlertButtonReturn.First);
        }
        
        public override bool ButtonAutoCheckOn
        {
            get { return iView.ButtonAutoCheck.State == NSCellStateValue.On; }
            set { iView.ButtonAutoCheck.State = value ? NSCellStateValue.On : NSCellStateValue.Off; }
        }        

        public override event EventHandler EventClosed
        {
            add { iView.Window.WillClose += value; }
            remove { iView.Window.WillClose -= value; }
        }
        
        public override event EventHandler EventButtonUpdateClicked
        {
            add { iView.ButtonUpdate.Activated += value; }
            remove { iView.ButtonUpdate.Activated -= value; }
        }
        
        public override event EventHandler EventButtonAutoCheckClicked
        {
            add { iView.ButtonAutoCheck.Activated += value; }
            remove { iView.ButtonAutoCheck.Activated -= value; }
        }

        #endregion

        private UpdateWindowController iView;
    }
}
