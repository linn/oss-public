// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace KonfigTouch
{
	[Register ("KonfigTouchViewController")]
	partial class KonfigTouchViewController
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblDeviceName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView viewConfigureLoadError { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIWebView webViewConfigure { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView viewLoading { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblConfigureLoadError { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIActivityIndicatorView activityIndicatorLoading { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIWebView webViewUpdate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView viewMain { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView viewUpdate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView viewConfigure { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView viewUpdateReleaseNotes { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView viewUpdateProgress { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIProgressView progressUpdate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnShowUpdateDetails { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnShowReleaseNotes { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIToolbar toolbarTop { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIToolbar toolbarBottom { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextView textUpdateDetails { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem btnDevices { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem btnUpdate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnUpdateStart { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem btnLinnLogo { get; set; }

		[Action ("btnConfigureLoadRetryPressed:")]
		partial void btnConfigureLoadRetryPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("btnLinnLogoPressed:")]
		partial void btnLinnLogoPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("btnShowUpdateDetailsPressed:")]
		partial void btnShowUpdateDetailsPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("btnUpdateStartPressed:")]
		partial void btnUpdateStartPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("btnShowReleaseNotesPressed:")]
		partial void btnShowReleaseNotesPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("btnUpdatePressed:")]
		partial void btnUpdatePressed (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (lblDeviceName != null) {
				lblDeviceName.Dispose ();
				lblDeviceName = null;
			}

			if (viewConfigureLoadError != null) {
				viewConfigureLoadError.Dispose ();
				viewConfigureLoadError = null;
			}

			if (webViewConfigure != null) {
				webViewConfigure.Dispose ();
				webViewConfigure = null;
			}

			if (viewLoading != null) {
				viewLoading.Dispose ();
				viewLoading = null;
			}

			if (lblConfigureLoadError != null) {
				lblConfigureLoadError.Dispose ();
				lblConfigureLoadError = null;
			}

			if (activityIndicatorLoading != null) {
				activityIndicatorLoading.Dispose ();
				activityIndicatorLoading = null;
			}

			if (webViewUpdate != null) {
				webViewUpdate.Dispose ();
				webViewUpdate = null;
			}

			if (viewMain != null) {
				viewMain.Dispose ();
				viewMain = null;
			}

			if (viewUpdate != null) {
				viewUpdate.Dispose ();
				viewUpdate = null;
			}

			if (viewConfigure != null) {
				viewConfigure.Dispose ();
				viewConfigure = null;
			}

			if (viewUpdateReleaseNotes != null) {
				viewUpdateReleaseNotes.Dispose ();
				viewUpdateReleaseNotes = null;
			}

			if (viewUpdateProgress != null) {
				viewUpdateProgress.Dispose ();
				viewUpdateProgress = null;
			}

			if (progressUpdate != null) {
				progressUpdate.Dispose ();
				progressUpdate = null;
			}

			if (btnShowUpdateDetails != null) {
				btnShowUpdateDetails.Dispose ();
				btnShowUpdateDetails = null;
			}

			if (btnShowReleaseNotes != null) {
				btnShowReleaseNotes.Dispose ();
				btnShowReleaseNotes = null;
			}

			if (toolbarTop != null) {
				toolbarTop.Dispose ();
				toolbarTop = null;
			}

			if (toolbarBottom != null) {
				toolbarBottom.Dispose ();
				toolbarBottom = null;
			}

			if (textUpdateDetails != null) {
				textUpdateDetails.Dispose ();
				textUpdateDetails = null;
			}

			if (btnDevices != null) {
				btnDevices.Dispose ();
				btnDevices = null;
			}

			if (btnUpdate != null) {
				btnUpdate.Dispose ();
				btnUpdate = null;
			}

			if (btnUpdateStart != null) {
				btnUpdateStart.Dispose ();
				btnUpdateStart = null;
			}

			if (btnLinnLogo != null) {
				btnLinnLogo.Dispose ();
				btnLinnLogo = null;
			}
		}
	}
}
