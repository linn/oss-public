using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Linn.Topology;

namespace LinnTopology
{
    [MonoTouch.Foundation.Register("RoomTableViewController")]
    public class RoomTableViewController : UITableViewController
    {
        private class DataSource : UITableViewDataSource
        {
            public DataSource(RoomTableViewController aController, House aHouse)
            {
                iRooms = new SortedList<string, IRoom>();

                iController = aController;
                iHouse = aHouse;

                iHouse.EventRoomAdded += RoomAdded;
                iHouse.EventRoomRemoved += RoomRemoved;
            }

            public override int RowsInSection(UITableView aTableView, int aSection)
            {
                lock(this)
                {
                    return iRooms.Count;
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
                    cell.TextLabel.Text = iRooms.Values[aIndexPath.Row].Name;
                }
                
                return cell;
            }

            public IRoom RoomAt(int aIndex)
            {
                lock(this)
                {
                    return iRooms.Values[aIndex];
                }
            }

            private void RoomAdded(object sender, EventArgsRoom e)
            {
                iController.BeginInvokeOnMainThread(delegate {
                    lock(this)
                    {
                        iRooms.Add(e.Room.Name, e.Room);
                        int index = iRooms.IndexOfValue(e.Room);
                        iController.TableView.InsertRows(new NSIndexPath[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);
                    }
                });
            }

            private void RoomRemoved(object sender, EventArgsRoom e)
            {
                iController.BeginInvokeOnMainThread(delegate {
                    lock(this)
                    {
                        int index = iRooms.IndexOfValue(e.Room);
                        iRooms.Remove(e.Room.Name);
                        iController.TableView.DeleteRows(new NSIndexPath[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);
                    }
                });
            }

            private RoomTableViewController iController;
            private House iHouse;
            private SortedList<string, IRoom> iRooms;
        }

        private class TableDelegate : UITableViewDelegate
        {

            private RoomTableViewController.DataSource iDataSource;
            private SourceTableViewController iController;

            public TableDelegate(SourceTableViewController aController, RoomTableViewController.DataSource aDataSource)
            {
                iController = aController;
                iDataSource = aDataSource;
            }

            public override void RowSelected(UITableView aTableView, NSIndexPath aIndexPath)
            {
                iController.SetRoom(iDataSource.RoomAt(aIndexPath.Row));
            }
        }

        public RoomTableViewController(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public void SetHouse(House aHouse)
        {
            iDataSource = new RoomTableViewController.DataSource(this, aHouse);
            TableView.DataSource = iDataSource;
        }

        public void SetSourceTableViewController(SourceTableViewController aSourceController)
        {
            TableView.Delegate = new TableDelegate(aSourceController, iDataSource);
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad();
            Title = "Rooms";
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

        private static NSString kCellIdentifier = new NSString("Room");

        private RoomTableViewController.DataSource iDataSource;
    }
}