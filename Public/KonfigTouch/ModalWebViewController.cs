using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace KonfigTouch
{
	public partial class ModalWebViewController : UIViewController
	{
		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public ModalWebViewController (string aInitialUrl)
			: base (UserInterfaceIdiomIsPhone ? "ModalWebViewController_iPhone" : "ModalWebViewController_iPad", null)
		{
			iInitialUrl = aInitialUrl;
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			webViewMain.LoadError += WebViewMainLoadError;
			webViewMain.LoadStarted += WebViewMainLoadStarted;
			webViewMain.LoadFinished += WebViewMainLoadFinished;
			webViewMain.ShouldStartLoad += WebViewMainShouldStartLoad;
			
			textFieldUrl.ShouldReturn += KeyboardReturnPressed;
			textFieldUrl.ShouldBeginEditing += KeyboardBeginEditing;
			textFieldUrl.ShouldEndEditing += KeyboardEndEditing;
			
			LoadUrl(iInitialUrl);
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			if (UserInterfaceIdiomIsPhone) {
				return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
			} else {
				return true;
			}
		}
		
		partial void btnDonePressed (MonoTouch.Foundation.NSObject sender) 
		{
			DismissModalViewControllerAnimated(true);
		}
		
		partial void btnOpenInSafariPressed (MonoTouch.Foundation.NSObject sender)
		{
			string link = textFieldUrl.Text;
			DismissModalViewControllerAnimated(false);
			
			// open in safari
			NSUrl url = new NSUrl(link);
            UIApplication.SharedApplication.OpenUrl(url);	
		}
		
		partial void btnBackPressed (MonoTouch.Foundation.NSObject sender)
		{
			if (webViewMain.CanGoBack) {
				webViewMain.GoBack();
			}
		}

		partial void btnForwardPressed (MonoTouch.Foundation.NSObject sender)
		{
			if (webViewMain.CanGoForward) {
				webViewMain.GoForward();
			}
		}
		
		partial void btnStopPressed (MonoTouch.Foundation.NSObject sender)
		{
			webViewMain.StopLoading();
		}

		partial void btnReloadPressed (MonoTouch.Foundation.NSObject sender)
		{
			if (webViewMain.Request != null && webViewMain.Request.Url.AbsoluteString != textFieldUrl.Text) {
				LoadUrl(textFieldUrl.Text);
			}
			else {
				webViewMain.Reload();
			}
		}

		private void WebViewMainLoadFinished (object sender, EventArgs e)
		{
			LoadUrlComplete();
			
			textFieldUrl.Text = ((UIWebView)sender).Request.Url.AbsoluteString; // required for back and forward requests (as ShouldStartLoad does not get called)
		}

		private void WebViewMainLoadStarted (object sender, EventArgs e)
		{
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

			btnStop.Enabled = true;
			btnReload.Enabled = false;
		}

		private void WebViewMainLoadError (object sender, UIWebErrorArgs e)
		{
			LoadUrlComplete();
			
			UIAlertView alert = new UIAlertView("Cannot Open Page",e.Error.LocalizedDescription,null,"OK",null);
            alert.Show(); 
		}
		
		private bool WebViewMainShouldStartLoad (UIWebView aWebView, MonoTouch.Foundation.NSUrlRequest aRequest, UIWebViewNavigationType aNavigationType) {
			textFieldUrl.Text = aRequest.Url.AbsoluteString; // if http is added programatically, this is the only way to reflect this if adderess is bad
			return true;
		}
		
		private bool KeyboardReturnPressed (UITextField aTextField) {  
			iKeyboardReturnPressed = true;
		    aTextField.ResignFirstResponder();  // dismiss keyboard
			LoadUrl (aTextField.Text); 
		    return true;  
		}  
		
		public bool KeyboardBeginEditing (UITextField textField) {
			iKeyboardReturnPressed = false;
			iTextBeforeEditing = textFieldUrl.Text;
			return true;  
		}
		
		public virtual bool KeyboardEndEditing (UITextField textField) {
			if (!iKeyboardReturnPressed) {
				textFieldUrl.Text = iTextBeforeEditing; // if return pressed, text will be overwritten
			}
			return true;  
		}
		
		private void LoadUrl(string aUrl) {
			NSUrl url = new NSUrl(aUrl);
			if (url.Scheme == null) {
				url = new NSUrl("http://" + aUrl);
			}
			
			NSUrlRequest req = new NSUrlRequest(url);
			webViewMain.LoadRequest(req);
		}
		
		private void LoadUrlComplete() {
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			
			btnStop.Enabled = false;
			btnReload.Enabled = true;
			
			btnBack.Enabled = webViewMain.CanGoBack;
			btnForward.Enabled = webViewMain.CanGoForward;
		}
		
		private string iInitialUrl;
		private string iTextBeforeEditing;
		private bool iKeyboardReturnPressed;
	}
}

