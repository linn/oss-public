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
	[MonoTouch.Foundation.Register("CellPlaylistFactory")]
	public partial class CellPlaylistFactory {
		
		private CellPlaylist __mt_cellPlaylist;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("cellPlaylist")]
		private CellPlaylist cellPlaylist {
			get {
				this.__mt_cellPlaylist = ((CellPlaylist)(this.GetNativeField("cellPlaylist")));
				return this.__mt_cellPlaylist;
			}
			set {
				this.__mt_cellPlaylist = value;
				this.SetNativeField("cellPlaylist", value);
			}
		}
	}
	
	// Base type probably should be MonoTouch.UIKit.UITableViewCell or subclass
	[MonoTouch.Foundation.Register("CellPlaylist")]
	public partial class CellPlaylist {
		
		private MonoTouch.UIKit.UIImageView __mt_imageViewArtwork;
		
		private MonoTouch.UIKit.UILabel __mt_labelTitle;
		
		private MonoTouch.UIKit.UILabel __mt_labelDurationBitrate;
		
		private MonoTouch.UIKit.UIImageView __mt_imageViewPlaying;
		
		private MonoTouch.UIKit.UILabel __mt_labelAlbum;
		
		private MonoTouch.UIKit.UILabel __mt_labelArtist;
		
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
		
		[MonoTouch.Foundation.Connect("labelDurationBitrate")]
		private MonoTouch.UIKit.UILabel labelDurationBitrate {
			get {
				this.__mt_labelDurationBitrate = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("labelDurationBitrate")));
				return this.__mt_labelDurationBitrate;
			}
			set {
				this.__mt_labelDurationBitrate = value;
				this.SetNativeField("labelDurationBitrate", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("imageViewPlaying")]
		private MonoTouch.UIKit.UIImageView imageViewPlaying {
			get {
				this.__mt_imageViewPlaying = ((MonoTouch.UIKit.UIImageView)(this.GetNativeField("imageViewPlaying")));
				return this.__mt_imageViewPlaying;
			}
			set {
				this.__mt_imageViewPlaying = value;
				this.SetNativeField("imageViewPlaying", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("labelAlbum")]
		private MonoTouch.UIKit.UILabel labelAlbum {
			get {
				this.__mt_labelAlbum = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("labelAlbum")));
				return this.__mt_labelAlbum;
			}
			set {
				this.__mt_labelAlbum = value;
				this.SetNativeField("labelAlbum", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("labelArtist")]
		private MonoTouch.UIKit.UILabel labelArtist {
			get {
				this.__mt_labelArtist = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("labelArtist")));
				return this.__mt_labelArtist;
			}
			set {
				this.__mt_labelArtist = value;
				this.SetNativeField("labelArtist", value);
			}
		}
	}
}