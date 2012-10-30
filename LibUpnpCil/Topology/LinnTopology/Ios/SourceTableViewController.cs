using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Linn.Topology;

namespace LinnTopology
{
    [MonoTouch.Foundation.Register("SourceTableViewController")]
    public class SourceTableViewController : UITableViewController
    {
        private class DataSource : UITableViewDataSource
        {
            public DataSource(SourceTableViewController aController)
            {
                iController = aController;
                iSources = new List<ISource>();
            }

            public void SetRoom(IRoom aRoom)
            {
                lock(this)
                {
                    if(iRoom != null)
                    {
                        iRoom.EventSourceAdded -= SourceAdded;
                        iRoom.EventSourceRemoved -= SourceRemoved;
                    }

                    iRoom = aRoom;
                    iRoom.EventSourceAdded += SourceAdded;
                    iRoom.EventSourceRemoved += SourceRemoved;

                    iSources = new List<ISource>(iRoom.Sources);

                    iController.BeginInvokeOnMainThread(delegate {
                        iController.TableView.ReloadData();
                    });
                }
            }

            public override int RowsInSection(UITableView aTableView, int aSection)
            {
                lock(this)
                {
                    return iSources.Count;
                }
            }

            public override UITableViewCell GetCell(UITableView aTableView, NSIndexPath aIndexPath)
            {
                UITableViewCell cell = aTableView.DequeueReusableCell(kCellIdentifier);
                if(cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Default, kCellIdentifier);
                }

                lock(this)
                {
                    cell.TextLabel.Text = string.Format("{0}:{1}", iSources[aIndexPath.Row].Group, iSources[aIndexPath.Row].Name);
                }
                
                return cell;
            }

            public ISource SourceAt(int aIndex)
            {
                lock(this)
                {
                    return iSources[aIndex];
                }
            }

            private void SourceAdded(object sender, EventArgsSource e)
            {
                lock(this)
                {
                    iSources = new List<ISource>(iRoom.Sources);
                    iController.BeginInvokeOnMainThread(delegate {
                        iController.TableView.ReloadData();
                    });
                }
            }

            private void SourceRemoved(object sender, EventArgsSource e)
            {
                lock(this)
                {
                    iSources = new List<ISource>(iRoom.Sources);
                    iController.BeginInvokeOnMainThread(delegate {
                        iController.TableView.ReloadData();
                    });
                }
            }

            private SourceTableViewController iController;
            private IRoom iRoom;

            private List<ISource> iSources;
        }

        private class TableDelegate : UITableViewDelegate
        {

            private SourceTableViewController.DataSource iDataSource;

            public TableDelegate(SourceTableViewController.DataSource aDataSource)
            {
                iDataSource = aDataSource;
            }

            public override void RowSelected(UITableView aTableView, NSIndexPath aIndexPath)
            {
                iDataSource.SourceAt(aIndexPath.Row).Select();
            }
        }

        public SourceTableViewController(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public void SetRoom(IRoom aRoom)
        {
            iDataSource.SetRoom(aRoom);

            if(iPopoverController != null)
            {
                iPopoverController.Dismiss(true);
            }
        }

        public void SetPopOverController(UIPopoverController aPopoverController)
        {
            iPopoverController = aPopoverController;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            Title = "Sources";

            iDataSource = new SourceTableViewController.DataSource(this);
            TableView.DataSource = iDataSource;
            TableView.Delegate = new TableDelegate(iDataSource);
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

        private static NSString kCellIdentifier = new NSString("Source");

        private SourceTableViewController.DataSource iDataSource;
        private UIPopoverController iPopoverController;
    }
}