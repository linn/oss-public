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
	[MonoTouch.Foundation.Register("CellDefaultFactory")]
	public partial class CellDefaultFactory {
		
		private CellDefault __mt_cellDefault;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("cellDefault")]
		private CellDefault cellDefault {
			get {
				this.__mt_cellDefault = ((CellDefault)(this.GetNativeField("cellDefault")));
				return this.__mt_cellDefault;
			}
			set {
				this.__mt_cellDefault = value;
				this.SetNativeField("cellDefault", value);
			}
		}
	}
	
	// Base type probably should be MonoTouch.UIKit.UITableViewCell or subclass
	[MonoTouch.Foundation.Register("CellDefault")]
	public partial class CellDefault {
		
		private MonoTouch.UIKit.UIImageView __mt_imageViewArtwork;
		
		private MonoTouch.UIKit.UILabel __mt_labelTitle;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("imageViewArtwork")]
		private MonoTouch.UIKit.UIImageView imageViewArtwork {
			get {
				this.__mt_imageViewArtwork = ((MonoTouch.UIKit.UIImageView)(this.GetNativeField("imageViewArtwork")));
				return this.__mt_imageViewArtwork;
			}
			set {
				this.__mt_imageViewArtwork = value;
				this.SetNativeField("imageViewArtwork", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("labelTitle")]
		private MonoTouch.UIKit.UILabel labelTitle {
			get {
				this.__mt_labelTitle = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("labelTitle")));
				return this.__mt_labelTitle;
			}
			set {
				this.__mt_labelTitle = value;
				this.SetNativeField("labelTitle", value);
			}
		}
	}
}