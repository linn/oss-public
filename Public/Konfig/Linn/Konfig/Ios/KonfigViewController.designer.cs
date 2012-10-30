// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace Linn.Konfig
{
	[Register ("KonfigViewController")]
	partial class KonfigViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIWebView iWebView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (iWebView != null) {
				iWebView.Dispose ();
				iWebView = null;
			}
		}
	}
}
