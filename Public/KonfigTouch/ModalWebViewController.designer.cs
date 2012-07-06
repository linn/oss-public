// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace KonfigTouch
{
	[Register ("ModalWebViewController")]
	partial class ModalWebViewController
	{
		[Outlet]
		MonoTouch.UIKit.UITextField textFieldUrl { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIWebView webViewMain { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem btnReload { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem btnStop { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem btnBack { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem btnForward { get; set; }

		[Action ("btnOpenInSafariPressed:")]
		partial void btnOpenInSafariPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("btnDonePressed:")]
		partial void btnDonePressed (MonoTouch.Foundation.NSObject sender);

		[Action ("btnBackPressed:")]
		partial void btnBackPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("btnForwardPressed:")]
		partial void btnForwardPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("btnReloadPressed:")]
		partial void btnReloadPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("btnStopPressed:")]
		partial void btnStopPressed (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (textFieldUrl != null) {
				textFieldUrl.Dispose ();
				textFieldUrl = null;
			}

			if (webViewMain != null) {
				webViewMain.Dispose ();
				webViewMain = null;
			}

			if (btnReload != null) {
				btnReload.Dispose ();
				btnReload = null;
			}

			if (btnStop != null) {
				btnStop.Dispose ();
				btnStop = null;
			}

			if (btnBack != null) {
				btnBack.Dispose ();
				btnBack = null;
			}

			if (btnForward != null) {
				btnForward.Dispose ();
				btnForward = null;
			}
		}
	}
}
