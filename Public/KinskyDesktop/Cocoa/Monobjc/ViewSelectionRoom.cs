
using System;

using Monobjc;
using Monobjc.Cocoa;

using Linn;
using Linn.Kinsky;


namespace KinskyDesktop
{
    // File's owner of the ViewSelectionRoom.nib file
    [ObjectiveCClass]
    public class ViewSelectionRoom : NSViewController, IViewPopover, IViewWidgetButton
    {
        private static readonly Class ThisClass = Class.GetClassFromType(typeof(ViewSelectionRoom));

        public ViewSelectionRoom() : base() {}
        public ViewSelectionRoom(IntPtr aInstance) : base(aInstance) {}

        public ViewSelectionRoom(IModelSelectionList<Linn.Kinsky.Room> aModel)
            : base()
        {
            iModel = aModel;
            NSBundle.LoadNibNamedOwner("ViewSelectionRoom.nib", this);
        }


        [ObjectiveCMessage("awakeFromNib")]
        public void AwakeFromNib()
        {
            // set appearance of view
            TextFieldTitle.TextColor = NSColor.WhiteColor;
            TextFieldTitle.Font = FontManager.FontLarge;

            ViewTable.RowHeight = 60.0f;
            ViewTable.BackgroundColor = NSColor.ClearColor;

            NSTableColumn nameColumn = ViewTable.TableColumns[1].CastAs<NSTableColumn>();
            TextFieldCellCentred nameCell = nameColumn.DataCell.CastAs<TextFieldCellCentred>();
            nameCell.TextColor = NSColor.WhiteColor;
            nameCell.Font = FontManager.FontSemiLarge;

            // setup model eventing
            iModel.EventChanged += ModelChanged;
            ModelMain.Instance.ViewMaster.ViewWidgetButtonStandby.Add(this);

            ModelChanged(this, EventArgs.Empty);

            if (SelectedIndex != -1)
            {
                ViewTable.ScrollRowToVisible(SelectedIndex);
            }

            // setup delegate - if this is done in IB, the methods can be called
            // before awakeFromNib which causes complications
            ViewTable.Delegate = this;
        }

        [ObjectiveCMessage("dealloc", SynchronizeFields = false)]
        public void Dealloc()
        {
            iModel.EventChanged -= ModelChanged;
            ModelMain.Instance.ViewMaster.ViewWidgetButtonStandby.Remove(this);

            View.Release();
            ArrayController.Release();

            this.SendMessageSuper(ThisClass, "dealloc");
        }


        #region IViewPopover implementation
        // no need to implement View and Release since the base class already implements them
        public event EventHandler<EventArgs> EventClose;
        #endregion IViewPopover implementation


        #region IViewWidgetButton implementation
        void IViewWidgetButton.Open()
        {
        }

        void IViewWidgetButton.Close()
        {
        }

        public event EventHandler<EventArgs> EventClick;
        #endregion IViewWidgetButton implementation


        #region NSTableView delegate functions
        [ObjectiveCMessage("tableView:willDisplayCell:forTableColumn:row:")]
        public void TableViewWillDisplayCell(NSTableView aView, Id aCell, NSTableColumn aTableColumn, int aRow)
        {
            NSString identifier = aTableColumn.Identifier.CastTo<NSString>();

            if (identifier.Compare(NSString.StringWithUTF8String("standby")) == NSComparisonResult.NSOrderedSame)
            {
                NSButtonCell cell = aCell.CastTo<NSButtonCell>();

                if (aRow == SelectedIndex)
                {
                    cell.Image = Properties.Resources.IconStandby;
                    cell.AlternateImage = Properties.Resources.IconStandbyOn;
                    cell.IsEnabled = true;
                }
                else
                {
                    cell.Image = null;
                    cell.AlternateImage = null;
                    cell.IsEnabled = false;
                }
            }
        }

        [ObjectiveCMessage("tableView:didClickCellAtColumn:row:")]
        public void TableViewDidClickRow(NSTableView aView, int aCol, int aRow)
        {
            // extended delegate function that is called on a mouse up - see
            // the TableViewClickable implementation below
            if (aRow == -1)
            {
                // ignore clicks on the table background
                return;
            }
            else if (SelectedIndex == aRow && aCol == 2)
            {
                // standby clicked for currently selected room
                EventHandler<EventArgs> ev = EventClick;
                if (ev != null)
                {
                    ev(this, EventArgs.Empty);
                }
            }
            else if (SelectedIndex != aRow)
            {
                // unselected room clicked
                iModel.SelectedItem = iModel.Items[aRow];
            }

            // always close the popover
            if (EventClose != null)
            {
                EventClose(this, EventArgs.Empty);
            }
        }
        #endregion NSTableView delegate functions


        #region NSArrayController bindings
        public NSMutableArray Rooms
        {
            [ObjectiveCMessage("rooms")]
            get { return iRooms; }
        }

        public NSIndexSet SelectionIndices
        {
            [ObjectiveCMessage("selectionIndices")]
            get
            {
                NSIndexSet selection = (SelectedIndex != -1)
                                     ? new NSIndexSet((uint)SelectedIndex)
                                     : new NSIndexSet();
                selection.Autorelease();
                return selection;
            }
            [ObjectiveCMessage("setSelectionIndices:")]
            set
            {
            }
        }
        #endregion NSArrayController bindings


        private void ModelChanged(object sender, EventArgs e)
        {
            // clear existing room list
            iRooms.Release();
            iRooms = new NSMutableArray();

            // add current list of rooms
            foreach (Linn.Kinsky.Room room in iModel.Items)
            {
                RoomData data = new RoomData();
                data.SetName(room.Name);

                iRooms.AddObject(data);
                data.Release();
            }

            // notify that there has been a change in the room list **before** the change in selection
            WillChangeValueForKey(NSString.StringWithUTF8String("rooms"));
            DidChangeValueForKey(NSString.StringWithUTF8String("rooms"));

            WillChangeValueForKey(NSString.StringWithUTF8String("selectionIndices"));
            DidChangeValueForKey(NSString.StringWithUTF8String("selectionIndices"));
        }

        private int SelectedIndex
        {
            get
            {
                return (iModel.SelectedItem != null) ? iModel.Items.IndexOf(iModel.SelectedItem) : -1;
            }
        }


        [ObjectiveCField]
        public NSTableView ViewTable;

        [ObjectiveCField]
        public NSTextField TextFieldTitle;

        [ObjectiveCField]
        public NSArrayController ArrayController;

        private IModelSelectionList<Linn.Kinsky.Room> iModel;
        private NSMutableArray iRooms = new NSMutableArray();
    }


    // Class for the data bindings
    [ObjectiveCClass]
    public class RoomData : NSObject
    {
        public RoomData() : base() {}
        public RoomData(IntPtr aInstance) : base(aInstance) {}

        public NSImage Image
        {
            [ObjectiveCMessage("image")]
            get { return Properties.Resources.IconRoom; }
        }

        public NSString Name
        {
            [ObjectiveCMessage("name")]
            get { return NSString.StringWithUTF8String(iName); }
        }

        public void SetName(string aName)
        {
            iName = aName;
        }

        private string iName = "asdassda";
    }
}



