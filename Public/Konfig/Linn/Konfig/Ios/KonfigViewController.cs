using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Linn.Konfig
{
    public partial class KonfigViewController : UIViewController
    {
        public KonfigViewController () : base ("KonfigViewController", null)
        {
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
        }
        
        public override void ViewDidDisappear (bool animated)
        {
            base.ViewDidDisappear (animated);
            
            // Clear any references to subviews of the main view in order to
            // allow the Garbage Collector to collect them sooner.
            //
            // e.g. myOutlet.Dispose (); myOutlet = null;
            
            ReleaseDesignerOutlets ();
        }

        [Obsolete]
        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            return true;
        }
        
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.All;
        }

        public UIWebView WebView
        {
            get
            {
                return iWebView;
            }
        }
    }
}

