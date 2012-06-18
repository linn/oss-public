using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;

using Linn;
using Linn.Kinsky;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace KinskyTouch
{
    internal interface IImageXmlIdListener
    {
        uint ImageId { get; set; }
    }

    partial class SaveViewController : UITableViewController, IImageXmlIdListener
    {
        internal class Saver : UIAlertViewDelegate
        {
            public Saver(ISaveSupport aSaveSupport)
            {
                iSaveSupport = aSaveSupport;
            }

            public string DefaultName
            {
                get
                {
                    return iSaveSupport.DefaultName;
                }
            }

            public void Save(string aFilename, string aDescription, uint aImageId)
            {
                iFilename = aFilename;
                iDescription = aDescription;
                iImageId = aImageId;

                if(iSaveSupport.Exists(aFilename))
                {
                    UIAlertView alert = new UIAlertView("Confirm Replacement", aFilename + " already exists. Do you want to replace it?", this, "No", new string[] { "Yes" });
                    alert.Show();
                }
                else
                {
                    Save();
                }
            }

            public override void Canceled(UIAlertView aAlertView)
            {
            }

            public override void Clicked(UIAlertView aAlertview, int aButtonIndex)
            {
                if(aButtonIndex != aAlertview.CancelButtonIndex)
                {
                    Save();
                }
            }

            private void Save()
            {
                try
                {
                    iSaveSupport.Save(iFilename, iDescription, iImageId);
                }
                catch (PlaylistManagerNotFoundException)
                {
                    UIAlertView alert = new UIAlertView("Save Failed", "Could not find PlaylistManager", null, "OK");
                    alert.Show();
                }
                catch (Exception e)
                {
                    UserLog.WriteLine(DateTime.Now + ": Could not save playlist file: " + e);
    
                    UIAlertView alert = new UIAlertView("Save Failed", e.Message, null, "OK");
                    alert.Show();
                }
            }

            private string iFilename;
            private string iDescription;
            private uint iImageId;

            private ISaveSupport iSaveSupport;
        }

        class DataSource : UITableViewDataSource
        {
            public class Delegate : NSUrlConnectionDelegate
            {
                public Delegate(UIImageView aImageView)
                {
                    iImageView = aImageView;
                    iData = new NSMutableData();
                }

                public override void ReceivedData (NSUrlConnection connection, NSData data)
                {
                    iData.AppendData(data);
                }

                public override void FailedWithError(NSUrlConnection connection, NSError error)
                {
                    iData = null;
                    iImageView.Image = KinskyTouch.Properties.ResourceManager.AlbumError;
                }

                public override void FinishedLoading(NSUrlConnection connection)
                {
                    UIImage temp = new UIImage(iData);
                    iImageView.Image = temp;
                }

                private UIImageView iImageView;
                private NSMutableData iData;
            }

            public DataSource(ISaveSupport aSaveSupport, UITextField aTextFieldName, UITextField aTextFieldDescription, UIImageView aImageView, IImageXmlIdListener aImageXmlListener)
            {
                iSaveSupport = aSaveSupport;
                iTextFieldName = aTextFieldName;
                iTextFieldDescription = aTextFieldDescription;
                iImageView = aImageView;
                iListener = aImageXmlListener;
            }

            public override int NumberOfSections(UITableView aTableView)
            {
                return 2;
            }

            public override int RowsInSection(UITableView aTableView, int aSection)
            {
                if(aSection == 0)
                {
                    return 1;
                }
                if(iSaveSupport.SaveLocation == LocalPlaylists.kRootId)
                {
                    return 1;
                }
                else
                {
                    return 3;
                }
            }

            public override UITableViewCell GetCell(UITableView aTableView, NSIndexPath aIndexPath)
            {
                UITableViewCell cell = null;

                if(aIndexPath.Section == 0)
                {
                    cell = aTableView.DequeueReusableCell(kLocationCellIdentifier);
                    if(cell == null)
                    {
                        cell = new UITableViewCell(UITableViewCellStyle.Value1, kLocationCellIdentifier);
                        cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    }
    
                    cell.TextLabel.Text = iSaveSupport.SaveLocation;
                }
                else
                {
                    if(aIndexPath.Row == 0)
                    {
                        cell = aTableView.DequeueReusableCell(kFilenameCellIdentifier);
                        if(cell == null)
                        {
                            cell = new UITableViewCell(UITableViewCellStyle.Value1, kFilenameCellIdentifier);
                            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                            cell.TextLabel.Text = "Name";
                            iTextFieldName.Frame = new System.Drawing.RectangleF(115, 0, cell.ContentView.Frame.Width - 115, cell.ContentView.Frame.Height);
                            cell.ContentView.Add(iTextFieldName);
                        }
                    }
                    else if(aIndexPath.Row == 1)
                    {
                        cell = aTableView.DequeueReusableCell(kDetailsCellIdentifier);
                        if(cell == null)
                        {
                            cell = new UITableViewCell(UITableViewCellStyle.Value1, kDetailsCellIdentifier);
                            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                            cell.TextLabel.Text = "Description";
                            iTextFieldDescription.Frame = new System.Drawing.RectangleF(115, 0, cell.ContentView.Frame.Width - 115, cell.ContentView.Frame.Height);
                            cell.ContentView.Add(iTextFieldDescription);
                        }
                    }
                    else
                    {
                        cell = aTableView.DequeueReusableCell(kImageCellIdentifier);
                        if(cell == null)
                        {
                            cell = new UITableViewCell(UITableViewCellStyle.Value1, kImageCellIdentifier);
                            cell.TextLabel.Text = "Image";
                            iImageView.Frame = new System.Drawing.RectangleF(115, 0, cell.ContentView.Frame.Height, cell.ContentView.Frame.Height);
                            cell.ContentView.Add(iImageView);
                        }

                        if(iListener.ImageId == 0)
                        {
                            iImageView.Image = KinskyTouch.Properties.ResourceManager.Playlist;
                        }
                        else
                        {
                            if(iConnection != null)
                            {
                                iConnection.Cancel();
                            }
                            NSUrl url = new NSUrl(iSaveSupport.ImageList[iListener.ImageId].AbsoluteUri);
                            NSUrlRequest request = new NSUrlRequest(url);
                            iConnection = new NSUrlConnection(request, new Delegate(iImageView), true);
        
                            iImageView.Image = KinskyTouch.Properties.ResourceManager.Loading;
                        }

                        if(iSaveSupport.ImageList.Count > 0)
                        {
                            cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                        }
                        else
                        {
                            cell.Accessory = UITableViewCellAccessory.None;
                        }
                    }
                }

                return cell;
            }

            private static readonly string kLocationCellIdentifier = "SaveLocation";
            private static readonly string kFilenameCellIdentifier = "SaveFilename";
            private static readonly string kDetailsCellIdentifier = "SaveDetails";
            private static readonly string kImageCellIdentifier = "SaveImage";

            private ISaveSupport iSaveSupport;
            private UITextField iTextFieldName;
            private UITextField iTextFieldDescription;
            private UIImageView iImageView;
            private IImageXmlIdListener iListener;

            private NSUrlConnection iConnection;
        }

        class Delegate : UITableViewDelegate
        {
            public Delegate(UITableViewController aController, ISaveSupport aSaveSupport, IImageXmlIdListener aListener)
            {
                iController = aController;
                iSaveSupport = aSaveSupport;
                iListener = aListener;
            }

            public override NSIndexPath WillSelectRow(UITableView aTableView, NSIndexPath aIndexPath)
            {
                if(aIndexPath.Section == 1 && aIndexPath.Row < 2)
                {
                    return null;
                }

                return aIndexPath;
            }

            public override void RowSelected(UITableView aTableView, NSIndexPath aIndexPath)
            {
                if(aIndexPath.Section == 0)
                {
                    iController.NavigationController.PushViewController(new SaveLocationViewController(iSaveSupport), true);
                }
                else if(aIndexPath.Section == 1 && aIndexPath.Row == 2 && iSaveSupport.ImageList.Count > 0)
                {
                    iController.NavigationController.PushViewController(new PlaylistImageViewController(iSaveSupport, iListener), true);
                }
            }

            private UITableViewController iController;
            private ISaveSupport iSaveSupport;
            private IImageXmlIdListener iListener;
        }

        public SaveViewController(Saver aSaver, ISaveSupport aSaveSupport, string aNibName, NSBundle aBundle)
            : base(aNibName, aBundle)
        {
            iSaver = aSaver;
            iSaveSupport = aSaveSupport;
        }

        public SaveViewController(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.DataSource = new DataSource(iSaveSupport, textFieldFilename, textFieldDescription, imageView, this);
            TableView.Delegate = new Delegate(this, iSaveSupport, this);

            textFieldFilename.Text = iSaver.DefaultName;
            textFieldFilename.BecomeFirstResponder();

            iButtonSave = new UIBarButtonItem();
            iButtonSave.Title = "Save";

            iButtonCancel = new UIBarButtonItem();
            iButtonCancel.Title = "Cancel";

            iButtonSave.Clicked += SaveClicked;
            iButtonCancel.Clicked += CancelClicked;

            Title = "Save Playlist";
            iImageId = 0;

            NavigationItem.LeftBarButtonItem = iButtonCancel;
            NavigationItem.RightBarButtonItem = iButtonSave;

            iSaveSupport.EventSaveLocationChanged += SaveLocationChanged;
            iSaveSupport.EventImageListChanged += ImageListChanged;
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            iButtonSave.Clicked -= SaveClicked;
            iButtonCancel.Clicked -= CancelClicked;

            iButtonSave.Dispose();
            iButtonCancel.Dispose();

            iSaveSupport.EventSaveLocationChanged -= SaveLocationChanged;
            iSaveSupport.EventImageListChanged -= ImageListChanged;
        }

        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            return true;
        }

        public uint ImageId
        {
            get
            {
                return iImageId;
            }
            set
            {
                iImageId = value;

                BeginInvokeOnMainThread(delegate {
                    TableView.ReloadData();
                });
            }
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            DismissModalViewControllerAnimated(true);

            iSaver.Save(textFieldFilename.Text, textFieldDescription.Text, iImageId);
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            DismissModalViewControllerAnimated(true);
        }

        private void SaveLocationChanged(object sender, EventArgs e)
        {
            iImageId = 0;

            BeginInvokeOnMainThread(delegate {
                TableView.ReloadData();
            });
        }

        private void ImageListChanged(object sender, EventArgs e)
        {
            BeginInvokeOnMainThread(delegate {
                TableView.ReloadData();
            });
        }

        private Saver iSaver;
        private uint iImageId;
        private ISaveSupport iSaveSupport;

        private UIBarButtonItem iButtonSave;
        private UIBarButtonItem iButtonCancel;
    }

    internal class SaveLocationViewController : UITableViewController
    {
        class DataSource : UITableViewDataSource
        {
            public DataSource(ISaveSupport aSaveSupport)
            {
                iSaveSupport = aSaveSupport;
            }

            public override int NumberOfSections(UITableView aTableView)
            {
                return 1;
            }

            public override int RowsInSection(UITableView aTableView, int aSection)
            {
                return iSaveSupport.SaveLocations.Count;
            }

            public override UITableViewCell GetCell(UITableView aTableView, NSIndexPath aIndexPath)
            {
                UITableViewCell cell = aTableView.DequeueReusableCell(kCellIdentifier);
                if(cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Default, kCellIdentifier);
                }

                string saveLocation = iSaveSupport.SaveLocations[aIndexPath.Row];
                cell.TextLabel.Text = saveLocation;
                cell.Accessory = (iSaveSupport.SaveLocation == saveLocation) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

                return cell;
            }

            private static readonly string kCellIdentifier = "SaveLocationItem";

            private ISaveSupport iSaveSupport;
        }

        class Delegate : UITableViewDelegate
        {
            public Delegate(ISaveSupport aSaveSupport)
            {
                iSaveSupport = aSaveSupport;
            }

            public override void RowSelected(UITableView aTableView, NSIndexPath aIndexPath)
            {
                iSaveSupport.SaveLocation = iSaveSupport.SaveLocations[aIndexPath.Row];
                aTableView.DeselectRow(aIndexPath, true);
            }

            private ISaveSupport iSaveSupport;
        }

        public SaveLocationViewController(ISaveSupport aSaveSupport)
            : base(UITableViewStyle.Grouped)
        {
            iSaveSupport = aSaveSupport;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            iSaveSupport.EventSaveLocationChanged += SaveLocationChanged;
            iSaveSupport.EventSaveLocationsChanged += SaveLocationsChanged;

            Title = "Save Location";

            TableView.DataSource = new DataSource(iSaveSupport);
            TableView.Delegate = new Delegate(iSaveSupport);
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            iSaveSupport.EventSaveLocationChanged -= SaveLocationChanged;
            iSaveSupport.EventSaveLocationsChanged -= SaveLocationsChanged;
        }

        private void SaveLocationsChanged(object sender, EventArgs e)
        {
            BeginInvokeOnMainThread(delegate {
                TableView.ReloadData();
            });
        }

        private void SaveLocationChanged(object sender, EventArgs e)
        {
            BeginInvokeOnMainThread(delegate {
                TableView.ReloadData();
            });
        }

        private ISaveSupport iSaveSupport;
    }

    internal class PlaylistImageViewController : UITableViewController
    {
        class DataSource : UITableViewDataSource
        {
            public DataSource(ISaveSupport aSaveSupport, IImageXmlIdListener aListener)
            {
                iSaveSupport = aSaveSupport;
                iListener = aListener;
            }

            public override int NumberOfSections(UITableView aTableView)
            {
                return 1;
            }

            public override int RowsInSection(UITableView aTableView, int aSection)
            {
                return iSaveSupport.ImageList.Count;
            }

            public override UITableViewCell GetCell(UITableView aTableView, NSIndexPath aIndexPath)
            {
                UITableViewCell cell = aTableView.DequeueReusableCell(kCellIdentifier);
                if(cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Default, kCellIdentifier);
                }

                cell.TextLabel.Text = "Image " + aIndexPath.Row;

                List<uint> list = new List<uint>(iSaveSupport.ImageList.Keys);
                if(iListener.ImageId == list[aIndexPath.Row])
                {
                    cell.Accessory = UITableViewCellAccessory.Checkmark;
                }
                else
                {
                    cell.Accessory = UITableViewCellAccessory.None;
                }

                return cell;
            }

            private static readonly string kCellIdentifier = "PlaylistImageItem";

            private ISaveSupport iSaveSupport;
            private IImageXmlIdListener iListener;
        }

        class Delegate : UITableViewDelegate
        {
            public Delegate(ISaveSupport aSaveSupport, IImageXmlIdListener aListener)
            {
                iSaveSupport = aSaveSupport;
                iListener = aListener;
            }

            public override void RowSelected(UITableView aTableView, NSIndexPath aIndexPath)
            {
                IList<uint> list = new List<uint>(iSaveSupport.ImageList.Keys);
                iListener.ImageId = list[aIndexPath.Row];
                aTableView.ReloadData();
            }

            private ISaveSupport iSaveSupport;
            private IImageXmlIdListener iListener;
        }

        public PlaylistImageViewController(ISaveSupport aSaveSupport, IImageXmlIdListener aListener)
            : base(UITableViewStyle.Grouped)
        {
            iSaveSupport = aSaveSupport;
            iListener = aListener;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            iSaveSupport.EventImageListChanged += ImageListChanged;

            Title = "Playlist Images";

            TableView.DataSource = new DataSource(iSaveSupport, iListener);
            TableView.Delegate = new Delegate(iSaveSupport, iListener);
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
            iSaveSupport.EventImageListChanged -= ImageListChanged;
        }

        private void ImageListChanged(object sender, EventArgs e)
        {
            BeginInvokeOnMainThread(delegate {
                TableView.ReloadData();
            });
        }

        private ISaveSupport iSaveSupport;
        private IImageXmlIdListener iListener;
    }
}

