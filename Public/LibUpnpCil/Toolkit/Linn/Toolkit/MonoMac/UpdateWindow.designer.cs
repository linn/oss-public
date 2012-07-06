// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Linn.Toolkit.Mac
{
	[Register ("UpdateWindowController")]
	partial class UpdateWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField iText1 { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField iText2 { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton iButtonUpdate { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton iButtonClose { get; set; }

		[Outlet]
		MonoMac.AppKit.NSProgressIndicator iProgress { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton iButtonDetails { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView iImage { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton iButtonAutoCheck { get; set; }

		[Outlet]
		MonoMac.WebKit.WebView iWebView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (iText1 != null) {
				iText1.Dispose ();
				iText1 = null;
			}

			if (iText2 != null) {
				iText2.Dispose ();
				iText2 = null;
			}

			if (iButtonUpdate != null) {
				iButtonUpdate.Dispose ();
				iButtonUpdate = null;
			}

			if (iButtonClose != null) {
				iButtonClose.Dispose ();
				iButtonClose = null;
			}

			if (iProgress != null) {
				iProgress.Dispose ();
				iProgress = null;
			}

			if (iButtonDetails != null) {
				iButtonDetails.Dispose ();
				iButtonDetails = null;
			}

			if (iImage != null) {
				iImage.Dispose ();
				iImage = null;
			}

			if (iButtonAutoCheck != null) {
				iButtonAutoCheck.Dispose ();
				iButtonAutoCheck = null;
			}

			if (iWebView != null) {
				iWebView.Dispose ();
				iWebView = null;
			}
		}
	}

	[Register ("UpdateWindow")]
	partial class UpdateWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
