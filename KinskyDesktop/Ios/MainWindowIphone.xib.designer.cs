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
	
	
	// Base type probably should be MonoTouch.Foundation.NSObject or subclass
	[MonoTouch.Foundation.Register("AppDelegateIphone")]
	public partial class AppDelegateIphone {
		
		private MonoTouch.UIKit.UIWindow __mt_window;
		
		private MonoTouch.UIKit.UITableView __mt_tableViewSource;
		
		private MonoTouch.UIKit.UINavigationController __mt_navigationController;
		
		private SourceToolbarIphone __mt_sourceToolbar;
		
		private ViewWidgetSelectorSource __mt_viewControllerSources;
		
		private HelperKinskyTouch __mt_helper;
		
		private MonoTouch.UIKit.UIButton __mt_buttonRepeat;
		
		private MonoTouch.UIKit.UIButton __mt_buttonShuffle;
		
		private MonoTouch.UIKit.UIView __mt_viewBrowser;
		
		private MonoTouch.UIKit.UIImageView __mt_imageViewArtwork;
		
		private MonoTouch.UIKit.UIButton __mt_buttonCentre;
		
		private MonoTouch.UIKit.UIButton __mt_buttonLeft;
		
		private MonoTouch.UIKit.UIButton __mt_buttonRight;
		
		private UIControlWheel __mt_controlRotaryTime;
		
		private UIControlWheel __mt_controlRotaryVolume;
		
		private ViewHourGlassIphone __mt_viewHourGlass;
		
		private UIViewInfoIphone __mt_viewInfo;
		
		private MonoTouch.UIKit.UIScrollView __mt_scrollView;
		
		private MonoTouch.UIKit.UIPageControl __mt_pageControl;
		
		private ViewWidgetSelectorRoom __mt_viewControllerRooms;
		
		private MonoTouch.UIKit.UIViewController __mt_viewControllerBrowser;
		
		private MonoTouch.UIKit.UINavigationController __mt_navigationControllerRoomSource;
		
		private MonoTouch.UIKit.UIControl __mt_controlTime;
		
		private MonoTouch.UIKit.UIControl __mt_controlVolume;
		
		private UIViewControllerNowPlaying __mt_viewControllerNowPlaying;
		
		private UIViewControllerKinskyTouchIphone __mt_viewController;
		
		private MonoTouch.UIKit.UIButton __mt_buttonArtwork;
		
		private MonoTouch.UIKit.UIBarButtonItem __mt_buttonRefresh;
		
		private MonoTouch.UIKit.UIImageView __mt_imageViewPlaylistAux;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("window")]
		private MonoTouch.UIKit.UIWindow window {
			get {
				this.__mt_window = ((MonoTouch.UIKit.UIWindow)(this.GetNativeField("window")));
				return this.__mt_window;
			}
			set {
				this.__mt_window = value;
				this.SetNativeField("window", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("tableViewSource")]
		private MonoTouch.UIKit.UITableView tableViewSource {
			get {
				this.__mt_tableViewSource = ((MonoTouch.UIKit.UITableView)(this.GetNativeField("tableViewSource")));
				return this.__mt_tableViewSource;
			}
			set {
				this.__mt_tableViewSource = value;
				this.SetNativeField("tableViewSource", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("navigationController")]
		private MonoTouch.UIKit.UINavigationController navigationController {
			get {
				this.__mt_navigationController = ((MonoTouch.UIKit.UINavigationController)(this.GetNativeField("navigationController")));
				return this.__mt_navigationController;
			}
			set {
				this.__mt_navigationController = value;
				this.SetNativeField("navigationController", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("sourceToolbar")]
		private SourceToolbarIphone sourceToolbar {
			get {
				this.__mt_sourceToolbar = ((SourceToolbarIphone)(this.GetNativeField("sourceToolbar")));
				return this.__mt_sourceToolbar;
			}
			set {
				this.__mt_sourceToolbar = value;
				this.SetNativeField("sourceToolbar", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewControllerSources")]
		private ViewWidgetSelectorSource viewControllerSources {
			get {
				this.__mt_viewControllerSources = ((ViewWidgetSelectorSource)(this.GetNativeField("viewControllerSources")));
				return this.__mt_viewControllerSources;
			}
			set {
				this.__mt_viewControllerSources = value;
				this.SetNativeField("viewControllerSources", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("helper")]
		private HelperKinskyTouch helper {
			get {
				this.__mt_helper = ((HelperKinskyTouch)(this.GetNativeField("helper")));
				return this.__mt_helper;
			}
			set {
				this.__mt_helper = value;
				this.SetNativeField("helper", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("buttonRepeat")]
		private MonoTouch.UIKit.UIButton buttonRepeat {
			get {
				this.__mt_buttonRepeat = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("buttonRepeat")));
				return this.__mt_buttonRepeat;
			}
			set {
				this.__mt_buttonRepeat = value;
				this.SetNativeField("buttonRepeat", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("buttonShuffle")]
		private MonoTouch.UIKit.UIButton buttonShuffle {
			get {
				this.__mt_buttonShuffle = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("buttonShuffle")));
				return this.__mt_buttonShuffle;
			}
			set {
				this.__mt_buttonShuffle = value;
				this.SetNativeField("buttonShuffle", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewBrowser")]
		private MonoTouch.UIKit.UIView viewBrowser {
			get {
				this.__mt_viewBrowser = ((MonoTouch.UIKit.UIView)(this.GetNativeField("viewBrowser")));
				return this.__mt_viewBrowser;
			}
			set {
				this.__mt_viewBrowser = value;
				this.SetNativeField("viewBrowser", value);
			}
		}
		
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
		
		[MonoTouch.Foundation.Connect("buttonCentre")]
		private MonoTouch.UIKit.UIButton buttonCentre {
			get {
				this.__mt_buttonCentre = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("buttonCentre")));
				return this.__mt_buttonCentre;
			}
			set {
				this.__mt_buttonCentre = value;
				this.SetNativeField("buttonCentre", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("buttonLeft")]
		private MonoTouch.UIKit.UIButton buttonLeft {
			get {
				this.__mt_buttonLeft = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("buttonLeft")));
				return this.__mt_buttonLeft;
			}
			set {
				this.__mt_buttonLeft = value;
				this.SetNativeField("buttonLeft", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("buttonRight")]
		private MonoTouch.UIKit.UIButton buttonRight {
			get {
				this.__mt_buttonRight = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("buttonRight")));
				return this.__mt_buttonRight;
			}
			set {
				this.__mt_buttonRight = value;
				this.SetNativeField("buttonRight", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("controlRotaryTime")]
		private UIControlWheel controlRotaryTime {
			get {
				this.__mt_controlRotaryTime = ((UIControlWheel)(this.GetNativeField("controlRotaryTime")));
				return this.__mt_controlRotaryTime;
			}
			set {
				this.__mt_controlRotaryTime = value;
				this.SetNativeField("controlRotaryTime", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("controlRotaryVolume")]
		private UIControlWheel controlRotaryVolume {
			get {
				this.__mt_controlRotaryVolume = ((UIControlWheel)(this.GetNativeField("controlRotaryVolume")));
				return this.__mt_controlRotaryVolume;
			}
			set {
				this.__mt_controlRotaryVolume = value;
				this.SetNativeField("controlRotaryVolume", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewHourGlass")]
		private ViewHourGlassIphone viewHourGlass {
			get {
				this.__mt_viewHourGlass = ((ViewHourGlassIphone)(this.GetNativeField("viewHourGlass")));
				return this.__mt_viewHourGlass;
			}
			set {
				this.__mt_viewHourGlass = value;
				this.SetNativeField("viewHourGlass", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewInfo")]
		private UIViewInfoIphone viewInfo {
			get {
				this.__mt_viewInfo = ((UIViewInfoIphone)(this.GetNativeField("viewInfo")));
				return this.__mt_viewInfo;
			}
			set {
				this.__mt_viewInfo = value;
				this.SetNativeField("viewInfo", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("scrollView")]
		private MonoTouch.UIKit.UIScrollView scrollView {
			get {
				this.__mt_scrollView = ((MonoTouch.UIKit.UIScrollView)(this.GetNativeField("scrollView")));
				return this.__mt_scrollView;
			}
			set {
				this.__mt_scrollView = value;
				this.SetNativeField("scrollView", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("pageControl")]
		private MonoTouch.UIKit.UIPageControl pageControl {
			get {
				this.__mt_pageControl = ((MonoTouch.UIKit.UIPageControl)(this.GetNativeField("pageControl")));
				return this.__mt_pageControl;
			}
			set {
				this.__mt_pageControl = value;
				this.SetNativeField("pageControl", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewControllerRooms")]
		private ViewWidgetSelectorRoom viewControllerRooms {
			get {
				this.__mt_viewControllerRooms = ((ViewWidgetSelectorRoom)(this.GetNativeField("viewControllerRooms")));
				return this.__mt_viewControllerRooms;
			}
			set {
				this.__mt_viewControllerRooms = value;
				this.SetNativeField("viewControllerRooms", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewControllerBrowser")]
		private MonoTouch.UIKit.UIViewController viewControllerBrowser {
			get {
				this.__mt_viewControllerBrowser = ((MonoTouch.UIKit.UIViewController)(this.GetNativeField("viewControllerBrowser")));
				return this.__mt_viewControllerBrowser;
			}
			set {
				this.__mt_viewControllerBrowser = value;
				this.SetNativeField("viewControllerBrowser", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("navigationControllerRoomSource")]
		private MonoTouch.UIKit.UINavigationController navigationControllerRoomSource {
			get {
				this.__mt_navigationControllerRoomSource = ((MonoTouch.UIKit.UINavigationController)(this.GetNativeField("navigationControllerRoomSource")));
				return this.__mt_navigationControllerRoomSource;
			}
			set {
				this.__mt_navigationControllerRoomSource = value;
				this.SetNativeField("navigationControllerRoomSource", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("controlTime")]
		private MonoTouch.UIKit.UIControl controlTime {
			get {
				this.__mt_controlTime = ((MonoTouch.UIKit.UIControl)(this.GetNativeField("controlTime")));
				return this.__mt_controlTime;
			}
			set {
				this.__mt_controlTime = value;
				this.SetNativeField("controlTime", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("controlVolume")]
		private MonoTouch.UIKit.UIControl controlVolume {
			get {
				this.__mt_controlVolume = ((MonoTouch.UIKit.UIControl)(this.GetNativeField("controlVolume")));
				return this.__mt_controlVolume;
			}
			set {
				this.__mt_controlVolume = value;
				this.SetNativeField("controlVolume", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewControllerNowPlaying")]
		private UIViewControllerNowPlaying viewControllerNowPlaying {
			get {
				this.__mt_viewControllerNowPlaying = ((UIViewControllerNowPlaying)(this.GetNativeField("viewControllerNowPlaying")));
				return this.__mt_viewControllerNowPlaying;
			}
			set {
				this.__mt_viewControllerNowPlaying = value;
				this.SetNativeField("viewControllerNowPlaying", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewController")]
		private UIViewControllerKinskyTouchIphone viewController {
			get {
				this.__mt_viewController = ((UIViewControllerKinskyTouchIphone)(this.GetNativeField("viewController")));
				return this.__mt_viewController;
			}
			set {
				this.__mt_viewController = value;
				this.SetNativeField("viewController", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("buttonArtwork")]
		private MonoTouch.UIKit.UIButton buttonArtwork {
			get {
				this.__mt_buttonArtwork = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("buttonArtwork")));
				return this.__mt_buttonArtwork;
			}
			set {
				this.__mt_buttonArtwork = value;
				this.SetNativeField("buttonArtwork", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("buttonRefresh")]
		private MonoTouch.UIKit.UIBarButtonItem buttonRefresh {
			get {
				this.__mt_buttonRefresh = ((MonoTouch.UIKit.UIBarButtonItem)(this.GetNativeField("buttonRefresh")));
				return this.__mt_buttonRefresh;
			}
			set {
				this.__mt_buttonRefresh = value;
				this.SetNativeField("buttonRefresh", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("imageViewPlaylistAux")]
		private MonoTouch.UIKit.UIImageView imageViewPlaylistAux {
			get {
				this.__mt_imageViewPlaylistAux = ((MonoTouch.UIKit.UIImageView)(this.GetNativeField("imageViewPlaylistAux")));
				return this.__mt_imageViewPlaylistAux;
			}
			set {
				this.__mt_imageViewPlaylistAux = value;
				this.SetNativeField("imageViewPlaylistAux", value);
			}
		}
	}
	
	// Base type probably should be MonoTouch.UIKit.UIViewController or subclass
	[MonoTouch.Foundation.Register("ViewWidgetBrowserRootIphone")]
	public partial class ViewWidgetBrowserRootIphone {
		
		private MonoTouch.UIKit.UIButton __mt_buttonHome;
		
		private MonoTouch.UIKit.UIButton __mt_buttonRetry;
		
		private MonoTouch.UIKit.UIView __mt_viewError;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("buttonHome")]
		private MonoTouch.UIKit.UIButton buttonHome {
			get {
				this.__mt_buttonHome = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("buttonHome")));
				return this.__mt_buttonHome;
			}
			set {
				this.__mt_buttonHome = value;
				this.SetNativeField("buttonHome", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("buttonRetry")]
		private MonoTouch.UIKit.UIButton buttonRetry {
			get {
				this.__mt_buttonRetry = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("buttonRetry")));
				return this.__mt_buttonRetry;
			}
			set {
				this.__mt_buttonRetry = value;
				this.SetNativeField("buttonRetry", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewError")]
		private MonoTouch.UIKit.UIView viewError {
			get {
				this.__mt_viewError = ((MonoTouch.UIKit.UIView)(this.GetNativeField("viewError")));
				return this.__mt_viewError;
			}
			set {
				this.__mt_viewError = value;
				this.SetNativeField("viewError", value);
			}
		}
	}
	
	// Base type probably should be MonoTouch.UIKit.UIViewController or subclass
	[MonoTouch.Foundation.Register("UIViewControllerNowPlaying")]
	public partial class UIViewControllerNowPlaying {
		
		private MonoTouch.UIKit.UIImageView __mt_imageViewArtwork;
		
		private MonoTouch.UIKit.UIView __mt_viewPlaylist;
		
		private MonoTouch.UIKit.UIButton __mt_buttonArtwork;
		
		private MonoTouch.UIKit.UIButton __mt_buttonList;
		
		private MonoTouch.UIKit.UINavigationBar __mt_navigationBar;
		
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
		
		[MonoTouch.Foundation.Connect("viewPlaylist")]
		private MonoTouch.UIKit.UIView viewPlaylist {
			get {
				this.__mt_viewPlaylist = ((MonoTouch.UIKit.UIView)(this.GetNativeField("viewPlaylist")));
				return this.__mt_viewPlaylist;
			}
			set {
				this.__mt_viewPlaylist = value;
				this.SetNativeField("viewPlaylist", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("buttonArtwork")]
		private MonoTouch.UIKit.UIButton buttonArtwork {
			get {
				this.__mt_buttonArtwork = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("buttonArtwork")));
				return this.__mt_buttonArtwork;
			}
			set {
				this.__mt_buttonArtwork = value;
				this.SetNativeField("buttonArtwork", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("buttonList")]
		private MonoTouch.UIKit.UIButton buttonList {
			get {
				this.__mt_buttonList = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("buttonList")));
				return this.__mt_buttonList;
			}
			set {
				this.__mt_buttonList = value;
				this.SetNativeField("buttonList", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("navigationBar")]
		private MonoTouch.UIKit.UINavigationBar navigationBar {
			get {
				this.__mt_navigationBar = ((MonoTouch.UIKit.UINavigationBar)(this.GetNativeField("navigationBar")));
				return this.__mt_navigationBar;
			}
			set {
				this.__mt_navigationBar = value;
				this.SetNativeField("navigationBar", value);
			}
		}
	}
	
	// Base type probably should be MonoTouch.UIKit.UIView or subclass
	[MonoTouch.Foundation.Register("UIViewInfoIphone")]
	public partial class UIViewInfoIphone {
	}
	
	// Base type probably should be MonoTouch.UIKit.UIToolbar or subclass
	[MonoTouch.Foundation.Register("SourceToolbarIphone")]
	public partial class SourceToolbarIphone {
	}
	
	// Base type probably should be MonoTouch.UIKit.UIViewController or subclass
	[MonoTouch.Foundation.Register("UIViewControllerKinskyTouchIphone")]
	public partial class UIViewControllerKinskyTouchIphone {
		
		private MonoTouch.UIKit.UIViewController __mt_viewControllerBrowser;
		
		private UIViewControllerNowPlaying __mt_viewControllerNowPlaying;
		
		private MonoTouch.UIKit.UINavigationController __mt_viewControllerRoomSource;
		
		private MonoTouch.UIKit.UIPageControl __mt_pageControl;
		
		private MonoTouch.UIKit.UIScrollView __mt_scrollView;
		
		private MonoTouch.UIKit.UINavigationController __mt_navigationControllerBrowser;
		
		private MonoTouch.UIKit.UINavigationController __mt_navigationControllerRoomSource;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("viewControllerBrowser")]
		private MonoTouch.UIKit.UIViewController viewControllerBrowser {
			get {
				this.__mt_viewControllerBrowser = ((MonoTouch.UIKit.UIViewController)(this.GetNativeField("viewControllerBrowser")));
				return this.__mt_viewControllerBrowser;
			}
			set {
				this.__mt_viewControllerBrowser = value;
				this.SetNativeField("viewControllerBrowser", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewControllerNowPlaying")]
		private UIViewControllerNowPlaying viewControllerNowPlaying {
			get {
				this.__mt_viewControllerNowPlaying = ((UIViewControllerNowPlaying)(this.GetNativeField("viewControllerNowPlaying")));
				return this.__mt_viewControllerNowPlaying;
			}
			set {
				this.__mt_viewControllerNowPlaying = value;
				this.SetNativeField("viewControllerNowPlaying", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("viewControllerRoomSource")]
		private MonoTouch.UIKit.UINavigationController viewControllerRoomSource {
			get {
				this.__mt_viewControllerRoomSource = ((MonoTouch.UIKit.UINavigationController)(this.GetNativeField("viewControllerRoomSource")));
				return this.__mt_viewControllerRoomSource;
			}
			set {
				this.__mt_viewControllerRoomSource = value;
				this.SetNativeField("viewControllerRoomSource", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("pageControl")]
		private MonoTouch.UIKit.UIPageControl pageControl {
			get {
				this.__mt_pageControl = ((MonoTouch.UIKit.UIPageControl)(this.GetNativeField("pageControl")));
				return this.__mt_pageControl;
			}
			set {
				this.__mt_pageControl = value;
				this.SetNativeField("pageControl", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("scrollView")]
		private MonoTouch.UIKit.UIScrollView scrollView {
			get {
				this.__mt_scrollView = ((MonoTouch.UIKit.UIScrollView)(this.GetNativeField("scrollView")));
				return this.__mt_scrollView;
			}
			set {
				this.__mt_scrollView = value;
				this.SetNativeField("scrollView", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("navigationControllerBrowser")]
		private MonoTouch.UIKit.UINavigationController navigationControllerBrowser {
			get {
				this.__mt_navigationControllerBrowser = ((MonoTouch.UIKit.UINavigationController)(this.GetNativeField("navigationControllerBrowser")));
				return this.__mt_navigationControllerBrowser;
			}
			set {
				this.__mt_navigationControllerBrowser = value;
				this.SetNativeField("navigationControllerBrowser", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("navigationControllerRoomSource")]
		private MonoTouch.UIKit.UINavigationController navigationControllerRoomSource {
			get {
				this.__mt_navigationControllerRoomSource = ((MonoTouch.UIKit.UINavigationController)(this.GetNativeField("navigationControllerRoomSource")));
				return this.__mt_navigationControllerRoomSource;
			}
			set {
				this.__mt_navigationControllerRoomSource = value;
				this.SetNativeField("navigationControllerRoomSource", value);
			}
		}
	}
	
	// Base type probably should be MonoTouch.UIKit.UIView or subclass
	[MonoTouch.Foundation.Register("ViewHourGlassIphone")]
	public partial class ViewHourGlassIphone {
	}
}
