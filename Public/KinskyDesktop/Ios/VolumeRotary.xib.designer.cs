// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace KinskyTouch {
	
	
	// Base type probably should be MonoTouch.UIKit.UIViewController or subclass
	[MonoTouch.Foundation.Register("ViewWidgetVolumeRotary")]
	public partial class ViewWidgetVolumeRotary {
		
		private MonoTouch.UIKit.UIView __mt_view;
		
		private UIControlRotary __mt_controlRotary;
		
		private MonoTouch.UIKit.UIImageView __mt_imageViewMuteOff;
		
		private MonoTouch.UIKit.UIImageView __mt_imageViewMuteOn;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("view")]
		private MonoTouch.UIKit.UIView view {
			get {
				this.__mt_view = ((MonoTouch.UIKit.UIView)(this.GetNativeField("view")));
				return this.__mt_view;
			}
			set {
				this.__mt_view = value;
				this.SetNativeField("view", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("controlRotary")]
		private UIControlRotary controlRotary {
			get {
				this.__mt_controlRotary = ((UIControlRotary)(this.GetNativeField("controlRotary")));
				return this.__mt_controlRotary;
			}
			set {
				this.__mt_controlRotary = value;
				this.SetNativeField("controlRotary", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("imageViewMuteOff")]
		private MonoTouch.UIKit.UIImageView imageViewMuteOff {
			get {
				this.__mt_imageViewMuteOff = ((MonoTouch.UIKit.UIImageView)(this.GetNativeField("imageViewMuteOff")));
				return this.__mt_imageViewMuteOff;
			}
			set {
				this.__mt_imageViewMuteOff = value;
				this.SetNativeField("imageViewMuteOff", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("imageViewMuteOn")]
		private MonoTouch.UIKit.UIImageView imageViewMuteOn {
			get {
				this.__mt_imageViewMuteOn = ((MonoTouch.UIKit.UIImageView)(this.GetNativeField("imageViewMuteOn")));
				return this.__mt_imageViewMuteOn;
			}
			set {
				this.__mt_imageViewMuteOn = value;
				this.SetNativeField("imageViewMuteOn", value);
			}
		}
	}
}