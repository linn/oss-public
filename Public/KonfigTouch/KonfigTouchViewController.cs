using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System;
using MonoTouch.Foundation;

namespace KonfigTouch
{
	public class TestInfo
	{
		public static string IPAddress = "10.2.11.171";
	}
	
	public partial class KonfigTouchViewController : UIViewController
	{
		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}
		
		static bool CurrentOrientationIsLandscape {
			get { return (UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeLeft || UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeRight); }
		}

		public KonfigTouchViewController ()
			: base (UserInterfaceIdiomIsPhone ? "KonfigTouchViewController_iPhone" : "KonfigTouchViewController_iPad", null)
		{
			iReleaseNotes = "<html><head><meta content=\"text/html; charset=ISO-8859-1\" http-equiv=\"content-type\"><title>Release Notes</title></head><style type=\"text/css\">body {color:#FFF;background:#000;}a {color:#2D8FBF;text-decoration:none;}a:active {color:#939393;text-decoration:none;}</style><body>";
			iReleaseNotes += "<h4><a href=\"http://docs.linn.co.uk/wiki/index.php/How_to_Upgrade_DS_Software\">Linn DS Software Update Released (Davaar 5)</a></h4>Updates are available for  Klimax DS, Klimax DSM, Akurate DS, Akurate DSM, Akurate Kontrol, Majik DS, Majik DS-I, Majik DSM, Sneaky Music DS, Sekrit DS-I and Renew DS. <br><br><ul><li>Provides support for <a href=\"http://oss.linn.co.uk/trac/wiki/Songcast\">Songcast app</a>, which allows you to send any audio from your computer to your Linn DS player and get better sound from your music services and web pages</li><li>Supports third party streaming protocol via the Net Aux source</li><li>Supports improved integration of DSM Systems with third-party surround sound systems (Setup documentation on <a href=\"http://docs.linn.co.uk/wiki/index.php/HDMI_Setup\">Linn Docs</a>)</li><li>Supports new <a href=\"http://docs.linn.co.uk/wiki/index.php/REM_020\">slimline handset</a></li><li>Various maintenance and bug fixes, see the <a href=\"http://docs.linn.co.uk/wiki/index.php/ReleaseNotes\">detailed release notes</a> for more information</li></ul>Tue, 20 Dec 2011 16:00:00 GMT<br/><hr/><h4><a href=\"http://www.linn.co.uk/ds_software\">Linn DS Software Update Released (Davaar 4)</a></h4>Updates are available for Akurate DSM and Majik DSM. <br><br><ul><li>Initial release for the new <a href=\"http://www.linn.co.uk/akuratedsm/\">Akurate DSM</a> and <a href=\"http://www.linn.co.uk/majikdsm/\">Majik DSM</a></li></ul>Tue, 15 Nov 2011 12:00:00 GMT<br/><hr/>";
			iReleaseNotes += "<h4><a href=\"http://docs.linn.co.uk/wiki/index.php/How_to_Upgrade_DS_Software\">Linn DS Software Update Released (Davaar 5)</a></h4>Updates are available for  Klimax DS, Klimax DSM, Akurate DS, Akurate DSM, Akurate Kontrol, Majik DS, Majik DS-I, Majik DSM, Sneaky Music DS, Sekrit DS-I and Renew DS. <br><br><ul><li>Provides support for <a href=\"http://oss.linn.co.uk/trac/wiki/Songcast\">Songcast app</a>, which allows you to send any audio from your computer to your Linn DS player and get better sound from your music services and web pages</li><li>Supports third party streaming protocol via the Net Aux source</li><li>Supports improved integration of DSM Systems with third-party surround sound systems (Setup documentation on <a href=\"http://docs.linn.co.uk/wiki/index.php/HDMI_Setup\">Linn Docs</a>)</li><li>Supports new <a href=\"http://docs.linn.co.uk/wiki/index.php/REM_020\">slimline handset</a></li><li>Various maintenance and bug fixes, see the <a href=\"http://docs.linn.co.uk/wiki/index.php/ReleaseNotes\">detailed release notes</a> for more information</li></ul>Tue, 20 Dec 2011 16:00:00 GMT<br/><hr/><h4><a href=\"http://www.linn.co.uk/ds_software\">Linn DS Software Update Released (Davaar 4)</a></h4>Updates are available for Akurate DSM and Majik DSM. <br><br><ul><li>Initial release for the new <a href=\"http://www.linn.co.uk/akuratedsm/\">Akurate DSM</a> and <a href=\"http://www.linn.co.uk/majikdsm/\">Majik DSM</a></li></ul>Tue, 15 Nov 2011 12:00:00 GMT<br/><hr/>";
			iReleaseNotes += "<h4><a href=\"http://docs.linn.co.uk/wiki/index.php/How_to_Upgrade_DS_Software\">Linn DS Software Update Released (Davaar 5)</a></h4>Updates are available for  Klimax DS, Klimax DSM, Akurate DS, Akurate DSM, Akurate Kontrol, Majik DS, Majik DS-I, Majik DSM, Sneaky Music DS, Sekrit DS-I and Renew DS. <br><br><ul><li>Provides support for <a href=\"http://oss.linn.co.uk/trac/wiki/Songcast\">Songcast app</a>, which allows you to send any audio from your computer to your Linn DS player and get better sound from your music services and web pages</li><li>Supports third party streaming protocol via the Net Aux source</li><li>Supports improved integration of DSM Systems with third-party surround sound systems (Setup documentation on <a href=\"http://docs.linn.co.uk/wiki/index.php/HDMI_Setup\">Linn Docs</a>)</li><li>Supports new <a href=\"http://docs.linn.co.uk/wiki/index.php/REM_020\">slimline handset</a></li><li>Various maintenance and bug fixes, see the <a href=\"http://docs.linn.co.uk/wiki/index.php/ReleaseNotes\">detailed release notes</a> for more information</li></ul>Tue, 20 Dec 2011 16:00:00 GMT<br/><hr/><h4><a href=\"http://www.linn.co.uk/ds_software\">Linn DS Software Update Released (Davaar 4)</a></h4>Updates are available for Akurate DSM and Majik DSM. <br><br><ul><li>Initial release for the new <a href=\"http://www.linn.co.uk/akuratedsm/\">Akurate DSM</a> and <a href=\"http://www.linn.co.uk/majikdsm/\">Majik DSM</a></li></ul>Tue, 15 Nov 2011 12:00:00 GMT<br/><hr/>";
			iReleaseNotes += "</body></html>";
			iReleaseNotes = iReleaseNotes.Replace("<br><br>", "<br>");
		
			iDeviceListTableViewController = new KonfigTouchTableViewController();
			iDeviceListTableViewController.EventCurrentDeviceSelected += CurrentDeviceUpdated;
			iDeviceListTableViewController.EventCurrentDeviceChanged += CurrentDeviceUpdated;
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
			
			iPopoverDevices = new KonfigTouchPopoverController(btnDevices, iDeviceListTableViewController);
			
			SetCurrentView(ViewType.eNoDeviceSelected);
			
			webViewConfigure.LoadError += ConfigurationLoadError;
			webViewConfigure.LoadStarted += ConfigurationLoadStarted;
			webViewConfigure.LoadFinished += ConfigurationLoadFinished;
			webViewConfigure.ShouldStartLoad += WebViewShouldStartLoad;
			webViewUpdate.ShouldStartLoad += WebViewShouldStartLoad;
			DisableBouncing(webViewConfigure);

			LoadConfig(); // should be done once device is slected (auto select)
			LoadReleaseNotes(); // should be retreived from device
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Release any retained subviews of the main view.
			// e.g. this.myOutlet = null;
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
		
		partial void btnUpdatePressed (MonoTouch.Foundation.NSObject sender) {
			if (btnUpdate.Title == kButtonUpdateTitle) {
				SetCurrentView(ViewType.eUpdate);
			}
			else {
				LoadConfig();
			}
		}
		
		partial void btnLinnLogoPressed (MonoTouch.Foundation.NSObject sender) {
			PresentModalViewUrl("http://www.linn.co.uk");
		}
		
		partial void btnShowUpdateDetailsPressed (MonoTouch.Foundation.NSObject sender) {
			textUpdateDetails.Hidden = !textUpdateDetails.Hidden;
		}
		
		partial void btnShowReleaseNotesPressed (MonoTouch.Foundation.NSObject sender) {
			webViewUpdate.Hidden = !webViewUpdate.Hidden;
		}

		partial void btnUpdateStartPressed (MonoTouch.Foundation.NSObject sender) {

		}
		
		partial void btnConfigureLoadRetryPressed (MonoTouch.Foundation.NSObject sender) {
			LoadConfig();
		}
		
		private void CurrentDeviceUpdated(object sender, EventArgsDevice e) {
			lblDeviceName.Text = e.Device;
			
			LoadConfig();
		}
		
		private void ConfigurationLoadStarted(object sender, EventArgs e) {
			SetCurrentView(ViewType.eLoading);
		}
		
		private void ConfigurationLoadFinished(object sender, EventArgs e) {
			SetCurrentView(ViewType.eConfigure);
		}
		
		private void ConfigurationLoadError(object sender, UIWebErrorArgs e) {
			SetCurrentView(ViewType.eConfigureLoadError);
		}
		
		private bool WebViewShouldStartLoad (UIWebView aWebView, MonoTouch.Foundation.NSUrlRequest aRequest, UIWebViewNavigationType aNavigationType) {
			// check for link clicked, open modal view controller if true
			if (aNavigationType == UIWebViewNavigationType.LinkClicked) {
				PresentModalViewUrl(aRequest.Url.AbsoluteString);
				return false;
			}
			else {
				return true;
			}
		}
		
		private void LoadConfig() {
			string url = "http://" + TestInfo.IPAddress + "/Config/Layouts/Default/index.html#service=Ds";
			NSUrlRequest req = new NSUrlRequest(new NSUrl(url), NSUrlRequestCachePolicy.UseProtocolCachePolicy, 5);
			webViewConfigure.LoadRequest(req);
		}
		
		private void LoadReleaseNotes() {
			webViewUpdate.LoadHtmlString(iReleaseNotes, null);
		}
		
		private void PresentModalViewUrl(string aUrl) {
			// open in modal popover view
			ModalWebViewController mvController = new ModalWebViewController(aUrl);
			mvController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
			mvController.ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;
			mvController.ModalInPopover = true;
			PresentModalViewController(mvController, true);
		}
		
		private void DisableBouncing(UIWebView aWebView) {
			foreach (UIView view in aWebView.Subviews) {
				if (view.GetType().ToString().Contains("UIScrollView")) {
					((UIScrollView)view).Bounces = false;
				}
			}
		}
		
		private void SetCurrentView(ViewType aView) {
			switch (aView) {
				case ViewType.eNoDeviceSelected : {
					viewConfigure.Hidden = true;
					viewConfigureLoadError.Hidden = true;
					viewLoading.Hidden = true;
					viewUpdate.Hidden = true;
					
					activityIndicatorLoading.StopAnimating();
					btnDevices.Enabled = true;
					btnUpdate.Title = kButtonUpdateTitle;
					btnUpdate.Enabled = false;
					break;
				}
				case ViewType.eConfigure : {
					viewConfigure.Hidden = false;
					viewConfigureLoadError.Hidden = true;
					viewLoading.Hidden = true;
					viewUpdate.Hidden = true;
				
					activityIndicatorLoading.StopAnimating();
					btnDevices.Enabled = true;
					btnUpdate.Title = kButtonUpdateTitle;
					btnUpdate.Enabled = true; // should be handled by device state
					break;
				}
				case ViewType.eConfigureLoadError : {
					viewConfigure.Hidden = true;
					viewConfigureLoadError.Hidden = false;
					viewLoading.Hidden = true;
					viewUpdate.Hidden = true;
					
					activityIndicatorLoading.StopAnimating();
					btnDevices.Enabled = true;
					btnUpdate.Title = kButtonUpdateTitle;
					btnUpdate.Enabled = false;
					break;
				}
				case ViewType.eLoading : {
					viewConfigure.Hidden = true;
					viewConfigureLoadError.Hidden = true;
					viewLoading.Hidden = false;
					viewUpdate.Hidden = true;
				
					activityIndicatorLoading.StartAnimating();
					btnDevices.Enabled = true;
					btnUpdate.Title = kButtonUpdateTitle;
					btnUpdate.Enabled = false;
					break;
				}
				case ViewType.eUpdate : {
					viewConfigure.Hidden = true;
					viewConfigureLoadError.Hidden = true;
					viewLoading.Hidden = true;
					viewUpdate.Hidden = false;
				
					activityIndicatorLoading.StopAnimating();
					iPopoverDevices.Dismiss();
					btnDevices.Enabled = false;
					btnUpdate.Title = kButtonUpdateDoneTitle;
					btnUpdate.Enabled = true;
					break;
				}
				default : {
					break;
				}
			}
		}
		
		private enum ViewType {
			eNoDeviceSelected,
			eConfigure,
			eConfigureLoadError,
			eLoading,
			eUpdate,
		}
		
		private string kButtonUpdateTitle = "Update";
		private string kButtonUpdateDoneTitle = "Done";
		private string iReleaseNotes = null;
		private KonfigTouchPopoverController iPopoverDevices = null;
		private KonfigTouchTableViewController iDeviceListTableViewController = null;
	}
	
	internal class KonfigTouchPopoverController : UIPopoverControllerDelegate {
		
		internal KonfigTouchPopoverController(UIBarButtonItem aButtonItemOpen, KonfigTouchTableViewController aController) {
            iButton = aButtonItemOpen;
            iController = aController;
			iDefaultTitle = aButtonItemOpen.Title;
			
			aButtonItemOpen.Clicked += OpenClicked;
			aController.EventCurrentDeviceSelected += CurrentDeviceSelected;
		}

		private void OpenClicked(object sender, EventArgs e) {
			if(iPopover == null) {
                UINavigationController navigationController = new UINavigationController(iController);
                iRefreshButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh);
				iEditButton = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
				iEditDoneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done);
				navigationController.NavigationBar.TopItem.Title = iDefaultTitle;
                navigationController.NavigationBar.TopItem.RightBarButtonItem = iRefreshButton;
				navigationController.NavigationBar.TopItem.LeftBarButtonItem = iEditButton;
                iPopover = new UIPopoverController(navigationController);
                iPopover.Delegate = this;

                iRefreshButton.Clicked += RefreshClicked;
				iEditButton.Clicked += EditClicked;
				iEditDoneButton.Clicked += EditClicked;

                navigationController.PopToRootViewController(false);
                iPopover.SetPopoverContentSize(new SizeF(380, iPopover.PopoverContentSize.Height), true);
                iPopover.PresentFromBarButtonItem(iButton, UIPopoverArrowDirection.Any, true);
            }
            else {
                Dismiss();
            }
		}
		
		public override void DidDismiss(UIPopoverController aPopoverController) {
            Dismiss();
        }
		
		public void Dismiss() {
            if(iPopover != null) {
                iRefreshButton.Clicked -= RefreshClicked;
                iRefreshButton.Dispose();
                iRefreshButton = null;
				
				iEditButton.Clicked -= EditClicked;
                iEditButton.Dispose();
                iEditButton = null;
				
				iEditDoneButton.Clicked -= EditClicked;
                iEditDoneButton.Dispose();
                iEditDoneButton = null;
    
                iPopover.Dismiss(true);
                iPopover.Dispose();
                iPopover = null;
				
				iController.SetEditing(false);
            }
        }
		
		private void CurrentDeviceSelected(object sender, EventArgsDevice e) {
            if(iPopover != null) {
                iPopover.BeginInvokeOnMainThread(delegate {
                    Dismiss();
                });
            }
        }
		
		private void RefreshClicked(object sender, EventArgs e) {
            //iHelper.Rescan();

            UIActivityIndicatorView view = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White);
            view.Frame = new RectangleF(0.0f, 0.0f, 25.0f, 25.0f);

            UIBarButtonItem item = new UIBarButtonItem();
            item.CustomView = view;

            UINavigationController navigationController = iPopover.ContentViewController as UINavigationController;
            navigationController.ViewControllers[0].NavigationItem.RightBarButtonItem = item;

            view.StartAnimating();

            new System.Threading.Timer(delegate {
                iRefreshButton.BeginInvokeOnMainThread(delegate {
                    view.StopAnimating();
                    navigationController.ViewControllers[0].NavigationItem.RightBarButtonItem = iRefreshButton;
                });
            }, null, 3000, System.Threading.Timeout.Infinite);
        }
		
		private void EditClicked(object sender, EventArgs e) {
			UINavigationController navigationController = iPopover.ContentViewController as UINavigationController;
			if (iController.TableView.Editing) {
				iController.SetEditing(false);
				navigationController.ViewControllers[0].NavigationItem.LeftBarButtonItem = iEditButton;
			}
			else {
				iController.SetEditing(true);
				navigationController.ViewControllers[0].NavigationItem.LeftBarButtonItem = iEditDoneButton;
			}
		}
		
		private UIBarButtonItem iButton;
        private UIBarButtonItem iRefreshButton;
		private UIBarButtonItem iEditButton;
		private UIBarButtonItem iEditDoneButton;
        private KonfigTouchTableViewController iController;
        private UIPopoverController iPopover;
		private string iDefaultTitle;
	}
	
	public class KonfigTouchTableViewController : UITableViewController
	{
		public event EventHandler<EventArgsDevice> EventCurrentDeviceSelected;
		public event EventHandler<EventArgsDevice> EventCurrentDeviceChanged;
		
		public KonfigTouchTableViewController() : base(UITableViewStyle.Plain) {
		}
		 
		public override void ViewDidLoad () {
			base.ViewDidLoad();
			
			iDataSource = new KonfigTouchTableDataSource(this);
			this.TableView.DataSource = iDataSource;
			this.TableView.Delegate = new KonfigTouchTableDelegate(iDataSource);
			
			this.TableView.SectionHeaderHeight = 25.0f;
			this.TableView.RowHeight = 60.0f;
            this.TableView.BackgroundColor = UIColor.Black;
            this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            this.TableView.ShowsHorizontalScrollIndicator = false;
            this.TableView.IndicatorStyle = UIScrollViewIndicatorStyle.White;   
		}
		
		public override void ViewWillAppear(bool aAnimated) {
            base.ViewWillAppear(aAnimated);

            if(iDataSource != null) {
				NSIndexPath path = iDataSource.CurrentDeviceIndex;
                if(path != null) {
                    this.TableView.ScrollToRow(path, UITableViewScrollPosition.Middle, true);
                }
            }
        }
		
		public void CurrentDeviceSelected(string aDevice) {
			if(EventCurrentDeviceSelected != null) {
                EventCurrentDeviceSelected(this, new EventArgsDevice(aDevice));
            }
		}
		
		public void CurrentDeviceChanged(string aDevice) {
			if(EventCurrentDeviceChanged != null) {
                EventCurrentDeviceChanged(this, new EventArgsDevice(aDevice));
            }
		}
		
		public void SetEditing(bool aEditing) {
			iDataSource.SetEditing(aEditing);
			this.TableView.SetEditing(aEditing, true);
		}
		
		private KonfigTouchTableDataSource iDataSource;
	}
	
	public class KonfigTouchTableDelegate : UITableViewDelegate
	{      
		public KonfigTouchTableDelegate(KonfigTouchTableDataSource aDataSource) { 
			iDataSource = aDataSource;
		}
		 
		public override void RowSelected (UITableView aTableView, NSIndexPath aIndexPath) {
			//aTableView.DeselectRow(aIndexPath, true);
			iDataSource.SetCurrentDevice(aIndexPath);
		}
		
		public override UITableViewCellEditingStyle EditingStyleForRow (UITableView aTableView, NSIndexPath aIndexPath) {
			return UITableViewCellEditingStyle.None;
		}
		
		public override bool ShouldIndentWhileEditing (UITableView aTableView, NSIndexPath aIndexPath) {
			return false;
		}
		
		private KonfigTouchTableDataSource iDataSource;
	}
	
	public class KonfigTouchTableDataSource : UITableViewDataSource
	{
		public KonfigTouchTableDataSource(KonfigTouchTableViewController aController) {
			iController = aController;
			iTableView = aController.TableView;
			iData = new List<SectionData>();
			iSectionDataComparer = new SectionData.Comparer();
			 
			SectionData section1 = new SectionData("section1");
			section1.Data.Add("LR Device C");
			section1.Data.Add("LR Device A");
			section1.Data.Add("The Nice Akurate DSM");
			section1.Title = "Johnathon's Bedroom";
			iData.Add(section1);
			 
			SectionData section2 = new SectionData("section2");
			section2.Data.Add("K Device 1");
			section2.Data.Add("K Device 2");
			section2.Title = "Kitchen";
			iData.Add(section2);
			
			ArrangeDeviceList();
		}
		
		public void SetCurrentDevice(NSIndexPath aIndex) {
			string newDevice = DeviceAt(aIndex);
			iController.CurrentDeviceSelected(newDevice);
			if (iCurrentDevice != newDevice) {
				ChangedCheckedDevice(iCurrentDevice, newDevice);
				iCurrentDevice = newDevice;
			}
		}
		
		public NSIndexPath CurrentDeviceIndex {
			get {
				if (iCurrentDevice != null) {
					return GetIndexFromDevice(iCurrentDevice);
				}
				return null;
			}
		}
		
		private string DeviceAt(NSIndexPath aIndexPath) {
            lock(this) {
				SectionData sectionData = iData[aIndexPath.Section];
				string row = sectionData.Data[aIndexPath.Row];
				
                return sectionData.Title + " (" + row + ")";
            }
        }
		
		private void ChangedCheckedDevice(string aOldDevice, string aNewDevice) {
			if (aOldDevice != null) {
				NSIndexPath oldIndex = GetIndexFromDevice(aOldDevice);
				if (oldIndex != null) {
					UITableViewCell oldCell = iTableView.CellAt(oldIndex);
					if (oldCell != null) {
						oldCell.AccessoryView = AccessoryViewUpdateAvailableNotSelected;
						//oldCell.Accessory = UITableViewCellAccessory.None;
						oldCell.TextLabel.TextColor = UIColor.Gray;
					}
				}
			}
			if (aNewDevice != null) {
				NSIndexPath newIndex = GetIndexFromDevice(aNewDevice);
				if (newIndex != null) {
					UITableViewCell newCell = iTableView.CellAt(newIndex);
					if (newCell != null) {
						newCell.AccessoryView = AccessoryViewUpdateAvailableSelected;
						//newCell.Accessory = UITableViewCellAccessory.Checkmark;
						newCell.TextLabel.TextColor = UIColor.White;
					}
				}
			}
		}
		
		private NSIndexPath GetIndexFromDevice(string aDevice) {
			for (int i = 0; i < iData.Count; i++) { // sections
				for (int j = 0; j < iData[i].Data.Count; j++) { // rows
					NSIndexPath index = NSIndexPath.FromRowSection(j, i);
					if (DeviceAt(index) == aDevice) {
						return index;
					}
				}
			}
			return null;
		}
		
		public void SetEditing(bool aEditing) {
			iTableView.BeginUpdates();
			if (aEditing) {
				SectionData sectionNewRoomStart = new SectionData("sectionNewRoomStart");
				sectionNewRoomStart.Title = kNewRoomTitle;
				iData.Add(sectionNewRoomStart);
				iTableView.InsertSections(NSIndexSet.FromIndex(iData.Count - 1), UITableViewRowAnimation.Fade);
				iTableView.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
			}
			else if (iTableView.Editing) {
				int index = iData.Count - 1;
				iData.RemoveAt(index);
				iTableView.DeleteSections(NSIndexSet.FromIndex(index), UITableViewRowAnimation.Fade);
				iTableView.BackgroundColor = UIColor.Black;
			}
			else {
				iTableView.BackgroundColor = UIColor.Black;
			}
			iTableView.EndUpdates();
		}
		 
		public override string TitleForHeader(UITableView aTableView, int aSection) {
			return iData[aSection].Title;
		}
		 
		public override int RowsInSection(UITableView aTableview, int aSection) {
			return iData[aSection].Data.Count;
		}
		 
		public override int NumberOfSections(UITableView aTableView)
		{
			return iData.Count;
		}
		 
		public override UITableViewCell GetCell(UITableView aTableView, NSIndexPath aIndexPath)
		{
			SectionData sectionData = iData[aIndexPath.Section];
			string cellId = sectionData.CellId;
			string row = sectionData.Data[aIndexPath.Row];
			 
			UITableViewCell cell = aTableView.DequeueReusableCell(cellId);      
			if (cell == null ) {
				cell = new UITableViewCell(UITableViewCellStyle.Default,cellId);		
				
				cell.BackgroundColor = UIColor.Black;
                cell.SelectionStyle = UITableViewCellSelectionStyle.Gray;
				cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(14);
				
				cell.ImageView.Image = KonfigTouch.Properties.ResourceManager.DeviceImagePlaceholder;
				string url = "http://" + TestInfo.IPAddress + "/images/Icon.png";
				NSUrl nsUrl = new NSUrl(url);
				NSUrlRequest nsReq = new NSUrlRequest(nsUrl);
				GetImageFromDeviceDelegate del = new GetImageFromDeviceDelegate(aTableView, aIndexPath);
				NSUrlConnection nsCon = new NSUrlConnection(nsReq, del);
				nsCon.Start();
			}
			
			if (iCurrentDevice == DeviceAt (aIndexPath)) {
				// currently selected device
				cell.AccessoryView = AccessoryViewUpdateAvailableSelected;
				//cell.Accessory = UITableViewCellAccessory.Checkmark;
				cell.TextLabel.TextColor = UIColor.White;
			}
			else {
				cell.AccessoryView = AccessoryViewUpdateAvailableNotSelected;
				//cell.Accessory = UITableViewCellAccessory.None;
				cell.TextLabel.TextColor = UIColor.Gray;
			}
			 
			cell.TextLabel.Text = row;
			return cell;
		}		

		public override void CommitEditingStyle (UITableView aTableView, UITableViewCellEditingStyle aEditingStyle, NSIndexPath aIndexPath) {
			if (aEditingStyle == UITableViewCellEditingStyle.Delete) {
				//iData[aIndexPath.Section].Data.RemoveAt(aIndexPath.Row);
				//aTableView.DeleteRows(new [] { aIndexPath }, UITableViewRowAnimation.Fade);
			}
		}
		 
		public override bool CanMoveRow (UITableView aTableView, NSIndexPath aIndexPath) {
			return true;
		}
		
		public override bool CanEditRow (UITableView aTableView, NSIndexPath aIndexPath) {
			return true;
		}
		 
		public override void MoveRow (UITableView aTableView, NSIndexPath aSourceIndexPath, NSIndexPath aDestinationIndexPath) {
			bool reloadTable = false;
			if (aDestinationIndexPath.Section == (iData.Count - 1)) {
				// added to 'New Room...' section --> get new room name from user - handle move after new room name entered (allows cancel as well)
				PromptForRoomNameOnMove(aSourceIndexPath, aDestinationIndexPath);
			}
			else {
				string originalDevice = DeviceAt(aSourceIndexPath);
				string item = iData[aSourceIndexPath.Section].Data[aSourceIndexPath.Row];
				iData[aSourceIndexPath.Section].Data.RemoveAt(aSourceIndexPath.Row);
				iData[aDestinationIndexPath.Section].Data.Insert(aDestinationIndexPath.Row, item);
				
				UpdateCurrentDeviceOnMove(originalDevice, DeviceAt(aDestinationIndexPath));
				reloadTable = true;
			}
			
			for (int i = 0; i < (iData.Count - 1); i++) {
				// if any section, other than the 'New Room...' section, has no devices, remove it
				if (iData[i].Data.Count == 0) {
					iData.RemoveAt(i);
					reloadTable = true;
					break;
				}
			}
			
			if (reloadTable) {
				iTableView.ReloadData();
			}
		}
		
		private void UpdateCurrentDeviceOnMove(string aOldDevice, string aNewDevice) {
			if (iCurrentDevice == aOldDevice) {
				// currently selected device has moved
				iController.CurrentDeviceChanged(aNewDevice);
				iCurrentDevice = aNewDevice;
			}
			ArrangeDeviceList();
		}
		
		private void ArrangeDeviceList() {
			// arrange sections
			iData.Sort(iSectionDataComparer);
			// arrange rows
			for (int i = 0; i < iData.Count; i++) {
				iData[i].Data.Sort();
			}
		}
		
		private UIImageView AccessoryViewUpdateAvailableSelected {
			get {
				return new UIImageView(new UIImage("UpdateAvailable.png"));
			}
		}
		
		private UIImageView AccessoryViewUpdateAvailableNotSelected {
			get {
				return new UIImageView(new UIImage("UpdateAvailableDark.png"));
			}
		}
		
		private void PromptForRoomNameOnMove(NSIndexPath aSourceIndexPath, NSIndexPath aDestinationIndexPath) {
			iMoveDeviceSourceIndexPath = aSourceIndexPath;
			iMoveDeviceDestinationIndexPath = aDestinationIndexPath;
			
			TextFieldAlertView roomNameAlertView = new TextFieldAlertView("Please Enter a Room Name", "Room Name", "20 characters max. Can't be blank.");
			roomNameAlertView.Dismissed += RoomNameAlertViewDismissed;
			roomNameAlertView.Show ();
		}
		
		private void RoomNameAlertViewDismissed (object sender, UIButtonEventArgs e) {
			string newRoomName = ((TextFieldAlertView) sender).EnteredText;
			newRoomName = newRoomName.Trim(); // remove any whitespace from beginning and end of text
			if (newRoomName.Length > 20) {
				newRoomName = newRoomName.Remove(20);
			}
			
			if(e.ButtonIndex == 1 && newRoomName.Length > 0) {
				int matchingRoomSectionIndex = -1;
				
				for (int i = 0; i < (iData.Count - 1); i++) {
					// new room name matches an existing room name
					if (iData[i].Title == newRoomName) {
						matchingRoomSectionIndex = i;
						break;
					}
				}
				
				if (matchingRoomSectionIndex < 0) {
					// rename old 'New Room...' section
					iData[iMoveDeviceDestinationIndexPath.Section].Title = newRoomName;
					
					// create new 'New Room...' section
					SectionData section = new SectionData("sectionNewRoom" + iData.Count.ToString());
					section.Title = kNewRoomTitle; 
	                iData.Insert(iData.Count, section);
					
					// move selected device to new room section
					string originalDevice = DeviceAt(iMoveDeviceSourceIndexPath);
					string item = iData[iMoveDeviceSourceIndexPath.Section].Data[iMoveDeviceSourceIndexPath.Row];
					iData[iMoveDeviceSourceIndexPath.Section].Data.RemoveAt(iMoveDeviceSourceIndexPath.Row);
					iData[iMoveDeviceDestinationIndexPath.Section].Data.Insert(iMoveDeviceDestinationIndexPath.Row, item);
					
					UpdateCurrentDeviceOnMove(originalDevice, DeviceAt(iMoveDeviceDestinationIndexPath));
				}
				else if (matchingRoomSectionIndex != iMoveDeviceSourceIndexPath.Section) {
					// move selected device to matching room section (guess at top row of matching section)
					string originalDevice = DeviceAt(iMoveDeviceSourceIndexPath);
					string item = iData[iMoveDeviceSourceIndexPath.Section].Data[iMoveDeviceSourceIndexPath.Row];
					iData[iMoveDeviceSourceIndexPath.Section].Data.RemoveAt(iMoveDeviceSourceIndexPath.Row);
					NSIndexPath destIndex = NSIndexPath.FromRowSection(0, matchingRoomSectionIndex);
					iData[destIndex.Section].Data.Insert(destIndex.Row, item);

					UpdateCurrentDeviceOnMove(originalDevice, DeviceAt(destIndex));
				}
				
				for (int i = 0; i < (iData.Count - 1); i++) {
					// if any section, other than the 'New Room...' section, has no devices, remove it
					if (iData[i].Data.Count == 0) {
						iData.RemoveAt(i);
						break;
					}
				}
				
				// reload table to reflect new sections
				iTableView.ReloadData();
			}
			else {
				// Cancel - reload table to insure move is cancelled
				iTableView.ReloadData();
			}
						
			iMoveDeviceSourceIndexPath = null;
			iMoveDeviceDestinationIndexPath = null;
		}
		
		private class GetImageFromDeviceDelegate : NSUrlConnectionDelegate {
			public GetImageFromDeviceDelegate(UITableView aTableView, NSIndexPath aIndexPath) {
				iTableView = aTableView;
				iIndexPath = aIndexPath;
			}
	
			public override void ReceivedData (NSUrlConnection aConnection, NSData aData) {
				if (iImageData == null) {
					iImageData = new NSMutableData();
				}
				iImageData.AppendData(aData);
				
				if (iImageData.Length == iExpectedContentLength) {
					UITableViewCell cell = iTableView.CellAt(iIndexPath);
					if (cell != null) {
						UIImage image = new UIImage(iImageData);
						cell.ImageView.Image = image;
					}
				}
			}
			
			public override void ReceivedResponse (NSUrlConnection connection, NSUrlResponse response) {
				iExpectedContentLength = response.ExpectedContentLength;
			}
			
			private UITableView iTableView = null;
			private NSIndexPath iIndexPath = null;
			private long iExpectedContentLength = 0;
			private NSMutableData iImageData = null;
		}
		
		internal class SectionData
		{
			public string Title { get;set; }
			public string CellId { get;set; }
			public List<string> Data { get;set; }
		 
			public SectionData(string aCellId)
			{
				Title = "";
				CellId = aCellId;
				Data = new List<string>();
			}
			
			public class Comparer: IComparer<SectionData>
			{
				public int Compare(SectionData x, SectionData y) {
					if (x.Title == kNewRoomTitle) {
						return 1;
					}
					else if (y.Title == kNewRoomTitle) {
						return -1;
					}
					else {
						return (x.Title.CompareTo(y.Title));
					}
				}
			}
		}
		
		private List<SectionData> iData;
		private UITableView iTableView;
		private KonfigTouchTableViewController iController;
		private SectionData.Comparer iSectionDataComparer;
		private string iCurrentDevice = null;
		private const string kNewRoomTitle = "New Room...";
		private NSIndexPath iMoveDeviceSourceIndexPath = null;
		private NSIndexPath iMoveDeviceDestinationIndexPath = null;
	}

	
	public class EventArgsDevice : EventArgs
    {
        public EventArgsDevice(string aDevice) {
            iDevice = aDevice;
        }

        public string Device {
            get { return iDevice; }
        }

        private string iDevice;
    }
	
	public class TextFieldAlertView : UIAlertView
	{
		public TextFieldAlertView(string aTitle) : this(false, aTitle) {
		}
		
		public TextFieldAlertView(string aTitle, string aMessage) : this(false, aTitle, "", aMessage) {
		}
		
		public TextFieldAlertView(string aTitle, string aPlaceholderText, string aMessage) : this(false, aTitle, aPlaceholderText, aMessage) {
		}
		
		public TextFieldAlertView(bool aSecureTextEntry, string aTitle) : this(aSecureTextEntry, aTitle, "", kHiddenStringMessage) {	
		}
		
		public TextFieldAlertView(bool aSecureTextEntry, string aTitle, string aPlaceholderText, string aMessage) {	
			iPlaceholderText = aPlaceholderText;
			iSecureTextEntry = aSecureTextEntry;
			this.Title = aTitle;
			this.Message = aMessage;
			if (aMessage.Length == 0) {
				this.Message = kHiddenStringMessage;
			}
			this.AddButton("Cancel");
			this.AddButton("OK");
			this.WillPresent += AlertViewWillPresent;
			
			iTextField = ComposeTextFieldControl(iSecureTextEntry, iPlaceholderText);
			iTextField.ShouldReturn += KeyboardReturnPressed;
		}

		public string EnteredText {
			get {
				return iTextField.Text;
			}
		}

		public override void LayoutSubviews () {
			base.LayoutSubviews ();

			// add the text field to the alert view
			this.AddSubview(iTextField);
			
			// make the alert view bigger to handle the text field and the message
			if (this.Message != kHiddenStringMessage) {
				AdjustControlSize();
			}
		}

		private UITextField ComposeTextFieldControl(bool aSecureTextEntry, string aPlaceholderText) {
			UITextField textField = new UITextField (new System.Drawing.RectangleF (12f, 45f, 260f, 25f));
			textField.BackgroundColor = UIColor.White;
			textField.UserInteractionEnabled = true;
			textField.AutocorrectionType = UITextAutocorrectionType.No;
			textField.AutocapitalizationType = UITextAutocapitalizationType.Words;
			textField.ReturnKeyType = UIReturnKeyType.Done;
			textField.SecureTextEntry = aSecureTextEntry;
			textField.Placeholder = aPlaceholderText;
			
			return textField;
		}
		
		private void AdjustControlSize() {
			if (this.Frame.Y > 0) {
				float tfExtH = iTextField.Frame.Size.Height + 10.0f;
				
				var frame = new RectangleF(this.Frame.X, this.Frame.Y - tfExtH/2, this.Frame.Size.Width, this.Frame.Size.Height + tfExtH);
				this.Frame = frame;
			
				foreach(var view in this.Subviews) {
					if(view is UIControl) {
						view.Frame = new RectangleF(view.Frame.X, view.Frame.Y + tfExtH, view.Frame.Size.Width, view.Frame.Size.Height);
					}
				}
			}
		}
		
		private bool KeyboardReturnPressed(UITextField aTextField) {
			this.DismissWithClickedButtonIndex(1, true);
			return true;
		}
	
		private void AlertViewWillPresent (object sender, EventArgs e) {
			// load the keybord automatically with the alert view
			iTextField.BecomeFirstResponder();
		}
		
		private UITextField iTextField;
		private bool iSecureTextEntry;
		private string iPlaceholderText;
		private const string kHiddenStringMessage = " ";
	}
}
