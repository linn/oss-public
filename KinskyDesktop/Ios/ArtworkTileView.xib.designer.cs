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
	
	
	// Base type probably should be MonoTouch.UIKit.UIView or subclass
	[MonoTouch.Foundation.Register("ArtworkTileView")]
	public partial class ArtworkTileView {
	}
	
	// Base type probably should be MonoTouch.UIKit.UIViewController or subclass
	[MonoTouch.Foundation.Register("ArtworkTileViewFactory")]
	public partial class ArtworkTileViewFactory {
		
		private ArtworkTileView __mt_view;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("view")]
		private ArtworkTileView view {
			get {
				this.__mt_view = ((ArtworkTileView)(this.GetNativeField("view")));
				return this.__mt_view;
			}
			set {
				this.__mt_view = value;
				this.SetNativeField("view", value);
			}
		}
	}
	
	// Base type probably should be MonoTouch.UIKit.UIView or subclass
	[MonoTouch.Foundation.Register("Tile")]
	public partial class Tile {
		
		private MonoTouch.UIKit.UIImageView __mt_viewImageArtworkBack;
		
		private MonoTouch.UIKit.UIImageView __mt_viewImageArtworkFront;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("viewImageArtworkBack")]
		private MonoTouch.UIKit.UIImageView viewImageArtworkBack {
			get {
				this.__mt_viewImageArtworkBack = ((MonoTouch.UIKit.UIImageView)(this.GetNativeField("viewImageArtworkBack")));
				return this.__mt_viewImageArtworkBack;
			}
			set {
				this.__mt_viewImageArtworkBack = value;
				this.SetNativeField("viewImageArtworkBack", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewImageArtworkFront")]
		private MonoTouch.UIKit.UIImageView viewImageArtworkFront {
			get {
				this.__mt_viewImageArtworkFront = ((MonoTouch.UIKit.UIImageView)(this.GetNativeField("viewImageArtworkFront")));
				return this.__mt_viewImageArtworkFront;
			}
			set {
				this.__mt_viewImageArtworkFront = value;
				this.SetNativeField("viewImageArtworkFront", value);
			}
		}
	}
}