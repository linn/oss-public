using System;
using System.Drawing;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Linn;
using Linn.Topology;
using Linn.Kinsky;

namespace KinskyTouch
{
    internal class ViewWidgetSelectorPopover<T> : UIPopoverControllerDelegate, IViewWidgetSelector<T> where T : class
    {
        public ViewWidgetSelectorPopover(HelperKinsky aHelper, UITableViewController aController, IViewWidgetSelector<T> aSelector, UIBarButtonItem aButtonItemOpen, UIBarButtonItem aButtonItemCancel)
        {
            iHelper = aHelper;
            iButton = aButtonItemOpen;
            iDefaultTitle = iButton.Title;

            iController = aController;

            aButtonItemOpen.Clicked += OpenClicked;
            aButtonItemCancel.Clicked += CancelClicked;

            aSelector.EventSelectionChanged += SelectionChanged;
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void InsertItem(int aIndex, T aItem)
        {
        }

        public void RemoveItem(T aItem)
        {
        }

        public void ItemChanged(T aItem)
        {
            lock(this)
            {
                if(aItem == iSelectedItem)
                {
                    SetSelected(aItem);
                }
            }
        }

        public void SetSelected(T aItem)
        {
            iButton.BeginInvokeOnMainThread(delegate {
                lock(this)
                {
                    if(aItem is Linn.Kinsky.Room)
                    {
                        Linn.Kinsky.Room room = aItem as Linn.Kinsky.Room;
                        iButton.Title = room.Name;
                    }
                    else if(aItem is Linn.Kinsky.Source)
                    {
                        Linn.Kinsky.Source source = aItem as Linn.Kinsky.Source;
                        iButton.Title = String.Format("{0}", source.Name);
                    }
                    else
                    {
                        iButton.Title = iDefaultTitle;
                        Dismiss();
                    }

                    iSelectedItem = aItem;
                }
            });
        }

        public event EventHandler<EventArgsSelection<T>> EventSelectionChanged;

        public override void DidDismiss(UIPopoverController aPopoverController)
        {
            Dismiss();
        }

        private void Dismiss()
        {
            if(iPopover != null)
            {
                iRefreshButton.Clicked -= RefreshClicked;
                iRefreshButton.Dispose();
                iRefreshButton = null;
    
                iPopover.Dismiss(true);
                iPopover.Dispose();
                iPopover = null;
            }
        }

        private void OpenClicked(object sender, EventArgs e)
        {
            if(iPopover == null)
            {
                UINavigationController navigationController = new UINavigationController(iController);
                iRefreshButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh);
                navigationController.NavigationBar.TopItem.RightBarButtonItem = iRefreshButton;
                iPopover = new UIPopoverController(navigationController);
                iPopover.Delegate = this;

                iRefreshButton.Clicked += RefreshClicked;

                navigationController.PopToRootViewController(false);
                //iPopover.SetPopoverContentSize(new SizeF(320, 600), true);
                iPopover.PresentFromBarButtonItem(iButton, UIPopoverArrowDirection.Any, true);
            }
            else
            {
                Dismiss();
            }
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            Dismiss();
        }

        private void SelectionChanged(object sender, EventArgsSelection<T> e)
        {
            if(iPopover != null)
            {
                iPopover.BeginInvokeOnMainThread(delegate {
                    Dismiss();
                });
            }
        }

        private void RefreshClicked(object sender, EventArgs e)
        {
            iHelper.Rescan();

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

        private string iDefaultTitle;
        private T iSelectedItem;
        private HelperKinsky iHelper;

        private UIBarButtonItem iButton;
        private UIBarButtonItem iRefreshButton;
        private UITableViewController iController;
        private UIPopoverController iPopover;
    }

    internal class ViewWidgetSelectorRoomNavigation : IViewWidgetSelector<Linn.Kinsky.Room>
    {
        private class Delegate : UINavigationControllerDelegate
        {
            public Delegate(ViewWidgetSelectorSource aViewWidgetSelectorSource)
            {
                iViewWidgetSelectorSource = aViewWidgetSelectorSource;
            }

            public override void WillShowViewController(UINavigationController aNavigationController, UIViewController aViewController, bool aAnimated)
            {
                if(aNavigationController.ViewControllers[0] == aNavigationController.TopViewController)
                {
                    iViewWidgetSelectorSource.Title = string.Empty;
                }
            }

            private ViewWidgetSelectorSource iViewWidgetSelectorSource;
        }

        public ViewWidgetSelectorRoomNavigation(HelperKinsky aHelper, UINavigationController aNavigationController, UIScrollView aScrollView, ViewWidgetSelectorSource aViewWidgetSelectorSource, UIBarButtonItem aButtonRefresh, UIButton aButtonStandby)
        {
            iHelper = aHelper;

            iNavigationController = aNavigationController;
            iNavigationController.Delegate = new Delegate(aViewWidgetSelectorSource);

            iScrollView = aScrollView;
            iViewWidgetSelectorSource = aViewWidgetSelectorSource;

            iButtonRefresh = aButtonRefresh;
            iNavigationController.ViewControllers[0].NavigationItem.RightBarButtonItem = iButtonRefresh;

            iButtonStandby = aButtonStandby;
            iViewWidgetSelectorSource.NavigationItem.RightBarButtonItem = new UIBarButtonItem(iButtonStandby);

            iViewWidgetSelectorSource.EventSelectionChanged += SelectionChanged;
            iButtonRefresh.Clicked += RefreshClicked;
            iButtonStandby.TouchUpInside += StandbyClicked;
        }

        public void Open()
        {
        }

        public void Close()
        {
            iNavigationController.PopToRootViewController(true);
        }

        public void InsertItem (int aIndex, Linn.Kinsky.Room aItem)
        {
        }

        public void RemoveItem(Linn.Kinsky.Room aItem)
        {
        }

        public void ItemChanged(Linn.Kinsky.Room aItem)
        {
        }

        public void SetSelected(Linn.Kinsky.Room aItem)
        {
            iNavigationController.BeginInvokeOnMainThread(delegate {
                if(aItem != null)
                {
                    if(iNavigationController.ViewControllers.Length == 1)
                    {
                        iNavigationController.PushViewController(iViewWidgetSelectorSource, true);
                    }
                    iViewWidgetSelectorSource.Title = aItem.Name;
                }
            });
        }

        public event EventHandler<EventArgsSelection<Linn.Kinsky.Room>> EventSelectionChanged;

        private void SelectionChanged(object sender, EventArgsSelection<Linn.Kinsky.Source> e)
        {
            iScrollView.BeginInvokeOnMainThread(delegate {
                RectangleF rect = iScrollView.Frame;
                rect.Offset(rect.Width * 1, 0);
                iScrollView.ScrollRectToVisible(rect, true);
            });
        }

        private void RefreshClicked(object sender, EventArgs e)
        {
            iHelper.Rescan();

            UIActivityIndicatorView view = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White);
            view.Frame = new RectangleF(0.0f, 0.0f, 25.0f, 25.0f);

            UIBarButtonItem item = new UIBarButtonItem();
            item.CustomView = view;

            iNavigationController.ViewControllers[0].NavigationItem.RightBarButtonItem = item;

            view.StartAnimating();

            new System.Threading.Timer(delegate {
                iButtonRefresh.BeginInvokeOnMainThread(delegate {
                    view.StopAnimating();
                    iNavigationController.ViewControllers[0].NavigationItem.RightBarButtonItem = iButtonRefresh;
                });
            }, null, 3000, System.Threading.Timeout.Infinite);
        }

        private void StandbyClicked(object sender, EventArgs e)
        {
            iNavigationController.PopToRootViewController(true);
        }

        private HelperKinsky iHelper;

        private UINavigationController iNavigationController;
        private UIScrollView iScrollView;
        private ViewWidgetSelectorSource iViewWidgetSelectorSource;

        private UIBarButtonItem iButtonRefresh;
        private UIButton iButtonStandby;
    }

    [MonoTouch.Foundation.Register("ViewWidgetSelectorRoom")]
    internal class ViewWidgetSelectorRoom : UITableViewController, IViewWidgetSelector<Linn.Kinsky.Room>, IControllerRoomSelector
    {
        private class DataSource : UITableViewDataSource
        {
            public DataSource(UITableView aTableView, UIButton aButtonStandby)
            {
                iTableView = aTableView;
                iButtonStandby = aButtonStandby;
                iRooms = new List<Linn.Kinsky.Room>();
            }

            public void SetRoom(Linn.Kinsky.Room aRoom)
            {
                lock(this)
                {
                    if(aRoom != iRoom)
                    {
                        Linn.Kinsky.Room oldRoom = iRoom;
                        iRoom = aRoom;
    
                        iTableView.BeginUpdates();
    
                        int oldIndex = iRooms.IndexOf(oldRoom);
                        if(oldIndex > -1)
                        {
                            NSIndexPath path = NSIndexPath.FromRowSection(oldIndex, 0);
                            UITableViewCell cell = iTableView.CellAt(path);
                            if(cell != null)
                            {
                                cell.AccessoryView = null;
                            }
                            iTableView.ReloadRows(new NSIndexPath[] { path }, UITableViewRowAnimation.Fade);
                        }
    
                        int newIndex = iRooms.IndexOf(aRoom);
                        if(newIndex > -1)
                        {
                            NSIndexPath path = NSIndexPath.FromRowSection(newIndex, 0);
                            /*UITableViewCell cell = iTableView.CellAt(NSIndexPath.FromRowSection(newIndex, 0));
                            if(cell != null)
                            {
                                cell.AccessoryView = iButtonStandby;
                            }*/
                            iTableView.ReloadRows(new NSIndexPath[] { path }, UITableViewRowAnimation.Fade);
                        }
    
                        iTableView.EndUpdates();
                    }
                }
            }

            public void InsertItem(int aIndex, Linn.Kinsky.Room aRoom)
            {
                lock(this)
                {
                    iTableView.BeginUpdates();
                    iRooms.Insert(aIndex, aRoom);
                    iTableView.InsertRows(new NSIndexPath[] { NSIndexPath.FromRowSection(aIndex, 0) }, UITableViewRowAnimation.Fade);
                    iTableView.EndUpdates();
                }
            }

            public void RemoveItem(Linn.Kinsky.Room aRoom)
            {
                lock(this)
                {
                    int index = iRooms.IndexOf(aRoom);
                    iTableView.BeginUpdates();
                    iRooms.Remove(aRoom);
                    iTableView.DeleteRows(new NSIndexPath[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);
                    iTableView.EndUpdates();
                }
            }

            public void Clear()
            {
                lock(this)
                {
                    iRooms.Clear();
                    iTableView.ReloadData();
                }
            }

            public NSIndexPath IndexPathFor(Linn.Kinsky.Room aRoom)
            {
                lock(this)
                {
                    int index = iRooms.IndexOf(aRoom);
                    if(index > -1)
                    {
                        return NSIndexPath.FromRowSection(index, 0);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public Linn.Kinsky.Room RoomAt(int aIndex)
            {
                lock(this)
                {
                    return iRooms[aIndex];
                }
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
                CellDefault cell = aTableView.DequeueReusableCell(kCellIdentifier) as CellDefault;
                if(cell == null)
                {
                    CellDefaultFactory factory = new CellDefaultFactory();
                    NSBundle.MainBundle.LoadNib("CellDefault", factory, null);
                    cell = factory.Cell;
                }

                lock(this)
                {
                    Linn.Kinsky.Room room = iRooms[aIndexPath.Row];

                    cell.BackgroundColor = UIColor.Black;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Gray;
                    cell.TextLabel.TextColor = UIColor.White;

                    cell.Title = room.Name;
                    cell.Image = KinskyTouch.Properties.ResourceManager.Room;
                    cell.AccessoryView = (iRoom == room) ? iButtonStandby : null;
                    //cell.Accessory = (iRoom == room) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
                    /*cell.AccessoryView = null;
                    if(iRoom == room)
                    {
                        UIButton buttonStandby = new UIButton(new RectangleF(0, 0, 29, 29));
                        buttonStandby.ShowsTouchWhenHighlighted = true;
                        buttonStandby.SetImage(new UIImage("StandbyOn.png"), UIControlState.Normal);
                        buttonStandby.SetImage(new UIImage("StandbyDown.png"), UIControlState.Highlighted);
                        buttonStandby.SetImage(new UIImage("Standby.png"), UIControlState.Selected);
                        cell.AccessoryView = buttonStandby;
                    }*/
                }

                return cell;
            }

            private UIButton iButtonStandby;
            private UITableView iTableView;
            private List<Linn.Kinsky.Room> iRooms;
            private Linn.Kinsky.Room iRoom;
        }

        private class Delegate : UITableViewDelegate
        {
            public Delegate(IControllerRoomSelector aController, DataSource aDataSource)
            {
                iController = aController;
                iDataSource = aDataSource;
            }

            public override void RowSelected(UITableView aTableView, NSIndexPath aIndexPath)
            {
                aTableView.DeselectRow(aIndexPath, true);
                iController.Select(iDataSource.RoomAt(aIndexPath.Row));
            }

            private IControllerRoomSelector iController;
            private DataSource iDataSource;
        }

        public ViewWidgetSelectorRoom(IntPtr aInstance)
            : base(aInstance)
        {
            iRooms = new List<Linn.Kinsky.Room>();
        }


        public ViewWidgetSelectorRoom(UIButton aButtonStandby)
        {
            iButtonStandby = aButtonStandby;
            iRooms = new List<Linn.Kinsky.Room>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = 73.0f;
            TableView.BackgroundColor = UIColor.Black;
            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            TableView.ShowsHorizontalScrollIndicator = false;
            TableView.IndicatorStyle = UIScrollViewIndicatorStyle.White;

            lock(this)
            {
                iDataSource = new DataSource(TableView, iButtonStandby);
                TableView.DataSource = iDataSource;
                TableView.Delegate = new Delegate(this, iDataSource);

                for(int i = 0; i < iRooms.Count; ++i)
                {
                    iDataSource.InsertItem(i, iRooms[i]);
                }
                iDataSource.SetRoom(iRoom);
            }

            Title = "Rooms";
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            lock(this)
            {
                iDataSource.Dispose();
                iDataSource = null;
            }
        }

        public override void ViewWillAppear(bool aAnimated)
        {
            base.ViewWillAppear(aAnimated);

            if(iDataSource != null)
            {
                NSIndexPath path = iDataSource.IndexPathFor(iRoom);
                if(path != null)
                {
                    TableView.ScrollToRow(path, UITableViewScrollPosition.Middle, false);
                }
            }
        }

        public void SetStandbyButton(UIButton aButtonStandby)
        {
            iButtonStandby = aButtonStandby;
        }

        public void Open()
        {
            lock(this)
            {
                iOpen = true;
            }
        }

        public void Close()
        {
            lock(this)
            {
                BeginInvokeOnMainThread(delegate {
                    if(iDataSource != null)
                    {
                        iDataSource.Clear();
                    }
                    iRooms.Clear();
                });

                iOpen = false;
            }
        }

        public void InsertItem(int aIndex, Linn.Kinsky.Room aItem)
        {
            lock(this)
            {
                if(iOpen)
                {
                    BeginInvokeOnMainThread(delegate {
                        iRooms.Insert(aIndex, aItem);
                        if(iDataSource != null)
                        {
                            iDataSource.InsertItem(aIndex, aItem);
                        }
                    });
                }
            }
        }

        public void RemoveItem(Linn.Kinsky.Room aItem)
        {
            lock(this)
            {
                if(iOpen)
                {
                    BeginInvokeOnMainThread(delegate {
                        iRooms.Remove(aItem);
                        if(iDataSource != null)
                        {
                            iDataSource.RemoveItem(aItem);
                        }
                    });
                }
            }
        }

        public void ItemChanged(Linn.Kinsky.Room aItem)
        {
        }

        public void SetSelected(Linn.Kinsky.Room aItem)
        {
            SetLabel(aItem);
        }

        public void Select(Linn.Kinsky.Room aRoom)
        {
            SetLabel(aRoom);

            if(EventSelectionChanged != null)
            {
                EventSelectionChanged(this, new EventArgsSelection<Linn.Kinsky.Room>(aRoom));
            }
        }
        
        public event EventHandler<EventArgsSelection<Linn.Kinsky.Room>> EventSelectionChanged;

        private void SetLabel(Linn.Kinsky.Room aRoom)
        {
            BeginInvokeOnMainThread(delegate {
                iRoom = aRoom;
                if(iDataSource != null)
                {
                    iDataSource.SetRoom(aRoom);
                }
            });
        }

        private static NSString kCellIdentifier = new NSString("CellDefault");

        private bool iOpen;

        private UIButton iButtonStandby;
        private DataSource iDataSource;
        private List<Linn.Kinsky.Room> iRooms;
        private Linn.Kinsky.Room iRoom;

        //private UITableView iTableView;
        //private UILabel iLabel;
    }

    [MonoTouch.Foundation.Register("ViewWidgetSelectorSource")]
    internal class ViewWidgetSelectorSource : UITableViewController, IViewWidgetSelector<Linn.Kinsky.Source>, IControllerSourceSelector
    {
        private class DataSource : UITableViewDataSource
        {
            public DataSource(UITableView aTableView)
            {
                iTableView = aTableView;
                iSources = new List<Linn.Kinsky.Source>();
            }

            public void SetSource(Linn.Kinsky.Source aSource)
            {
                lock(this)
                {
                    Linn.Kinsky.Source oldSource = iSource;
                    iSource = aSource;

                    if(oldSource != null)
                    {
                        int index = iSources.IndexOf(oldSource);
                        if(index > -1)
                        {
                            iTableView.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);
                        }
                    }

                    if(aSource != null)
                    {
                        int index = iSources.IndexOf(aSource);
                        if(index > -1)
                        {
                            iTableView.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);
                        }
                    }
                }
            }

            public void InsertItem(int aIndex, Linn.Kinsky.Source aSource)
            {
                lock(this)
                {
                    iTableView.BeginUpdates();
                    iSources.Insert(aIndex, aSource);
                    iTableView.InsertRows(new NSIndexPath[] { NSIndexPath.FromRowSection(aIndex, 0) }, UITableViewRowAnimation.Fade);
                    iTableView.EndUpdates();
                }
            }

            public void RemoveItem(Linn.Kinsky.Source aSource)
            {
                lock(this)
                {
                    int index = iSources.IndexOf(aSource);
                    if(index > -1)
                    {
                        iTableView.BeginUpdates();
                        iSources.Remove(aSource);
                        iTableView.DeleteRows(new NSIndexPath[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);
                        iTableView.EndUpdates();
                    }
                }
            }

            public void ItemChanged(Linn.Kinsky.Source aSource)
            {
                lock(this)
                {
                    int index = iSources.IndexOf(aSource);
                    if(index > -1)
                    {
                        iTableView.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);
                    }
                }
            }

            public void Clear()
            {
                lock(this)
                {
                    iSources.Clear();
                    iTableView.ReloadData();
                }
            }

            public NSIndexPath IndexPathFor(Linn.Kinsky.Source aSource)
            {
                lock(this)
                {
                    if(aSource != null)
                    {
                        int index = iSources.IndexOf(aSource);
                        if(index > -1)
                        {
                            return NSIndexPath.FromRowSection(index, 0);
                        }
                    }

                    return null;
                }
            }

            public Linn.Kinsky.Source SourceAt(NSIndexPath aPathIndex)
            {
                lock(this)
                {
                    return iSources[aPathIndex.Row];

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
                CellDefault cell = aTableView.DequeueReusableCell(kCellIdentifier) as CellDefault;
                if(cell == null)
                {
                    CellDefaultFactory factory = new CellDefaultFactory();
                    NSBundle.MainBundle.LoadNib("CellDefault", factory, null);
                    cell = factory.Cell;
                }

                lock(this)
                {
                    cell.BackgroundColor = UIColor.Black;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Gray;
                    //cell.TextLabel.TextColor = UIColor.White;

                    Linn.Kinsky.Source source = iSources[aIndexPath.Row];
                    cell.Title = source.Name;
                    cell.Image = GetSourceImage(source);
                    cell.Accessory = (iSource == source) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
                }

                return cell;
            }

            private UIImage GetSourceImage(Linn.Kinsky.Source aSource)
            {
                switch(aSource.Type)
                {
                    case Linn.Kinsky.Source.kSourceAux:
                    case Linn.Kinsky.Source.kSourceAnalog:
                    case Linn.Kinsky.Source.kSourceSpdif:
                    case Linn.Kinsky.Source.kSourceToslink:
                        return KinskyTouch.Properties.ResourceManager.SourceExternal;

                    case Linn.Kinsky.Source.kSourceDisc:
                        return KinskyTouch.Properties.ResourceManager.SourceDisc;

                    case Linn.Kinsky.Source.kSourceDs:
                        return KinskyTouch.Properties.ResourceManager.SourcePlaylist;

                    case Linn.Kinsky.Source.kSourceRadio:
                    case Linn.Kinsky.Source.kSourceTuner:
                        return KinskyTouch.Properties.ResourceManager.SourceRadio;

                    case Linn.Kinsky.Source.kSourceUpnpAv:
                        return KinskyTouch.Properties.ResourceManager.SourceUpnpAv;

                    case Linn.Kinsky.Source.kSourceReceiver:
                        return KinskyTouch.Properties.ResourceManager.SourceSongcast;

                    default:
                        return KinskyTouch.Properties.ResourceManager.SourceExternal;
                }
            }

            private UITableView iTableView;

            private List<Linn.Kinsky.Source> iSources;
            private Linn.Kinsky.Source iSource;
        }

        private class Delegate : UITableViewDelegate
        {
            public Delegate(IControllerSourceSelector aController, ViewWidgetSelectorSource.DataSource aDataSource)
            {
                iController = aController;
                iDataSource = aDataSource;
            }

            public override void RowSelected(UITableView aTableView, NSIndexPath aIndexPath)
            {
                aTableView.DeselectRow(aIndexPath, true);
                iController.Select(iDataSource.SourceAt(aIndexPath));
            }

            private IControllerSourceSelector iController;
            private ViewWidgetSelectorSource.DataSource iDataSource;
        }

        public ViewWidgetSelectorSource()
        {
            iSources = new List<Linn.Kinsky.Source>();
        }

        public ViewWidgetSelectorSource(IntPtr aInstance)
            : base(aInstance)
        {
            iSources = new List<Linn.Kinsky.Source>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.SectionHeaderHeight = 42.0f;
            TableView.RowHeight = 73.0f;
            TableView.BackgroundColor = UIColor.Black;
            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            TableView.ShowsHorizontalScrollIndicator = false;
            TableView.IndicatorStyle = UIScrollViewIndicatorStyle.White;

            lock(this)
            {
                iDataSource = new DataSource(TableView);
                TableView.DataSource = iDataSource;
                iDelegate = new Delegate(this, iDataSource);
                TableView.Delegate = iDelegate;

                for(int i = 0; i < iSources.Count; ++i)
                {
                    iDataSource.InsertItem(i, iSources[i]);
                }

                iDataSource.SetSource(iSource);
            }

            Title = "Sources";
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            lock(this)
            {
                iDataSource.Dispose();
                iDataSource = null;
                iDelegate = null;
            }
        }

        public override void ViewWillAppear(bool aAnimated)
        {
            base.ViewWillAppear(aAnimated);

            lock(this)
            {
                if(iDataSource != null)
                {
                    NSIndexPath path = iDataSource.IndexPathFor(iSource);
                    if(path != null)
                    {
                        TableView.ScrollToRow(path, UITableViewScrollPosition.Middle, false);
                    }
                }
            }
        }

        public void Open()
        {
            lock(this)
            {
                iOpen = true;
            }
        }

        public void Close()
        {
            lock(this)
            {
                BeginInvokeOnMainThread(delegate {
                    lock(this)
                    {
                        iSources.Clear();
                        if(iDataSource != null)
                        {
                            iDataSource.Clear();
                        }
                    }
                });

                iOpen = false;
            }
        }

        public void InsertItem(int aIndex, Linn.Kinsky.Source aItem)
        {
            lock(this)
            {
                if(iOpen)
                {
                    BeginInvokeOnMainThread(delegate {
                        lock(this)
                        {
                            iSources.Insert(aIndex, aItem);
                            if(iDataSource != null)
                            {
                                iDataSource.InsertItem(aIndex, aItem);
                            }
                        }
                    });
                }
            }
        }

        public void RemoveItem(Linn.Kinsky.Source aItem)
        {
            lock(this)
            {
                if(iOpen)
                {
                    BeginInvokeOnMainThread(delegate {
                        lock(this)
                        {
                            iSources.Remove(aItem);
                            if(iDataSource != null)
                            {
                                iDataSource.RemoveItem(aItem);
                            }
                        }
                    });
                }
            }
        }

        public void ItemChanged(Linn.Kinsky.Source aItem)
        {
            lock(this)
            {
                if(iOpen)
                {
                    BeginInvokeOnMainThread(delegate {
                        lock(this)
                        {
                            if(iDataSource != null)
                            {
                                iDataSource.ItemChanged(aItem);
                            }
                        }
                    });
                }
            }
        }

        public void SetSelected(Linn.Kinsky.Source aItem)
        {
            SetLabel(aItem);
        }

        public void Select(Linn.Kinsky.Source aSource)
        {
            SetLabel(aSource);

            if(EventSelectionChanged != null)
            {
                EventSelectionChanged(this, new EventArgsSelection<Linn.Kinsky.Source>(aSource));
            }
        }

        public event EventHandler<EventArgsSelection<Linn.Kinsky.Source>> EventSelectionChanged;

        private void SetLabel(Linn.Kinsky.Source aSource)
        {
            BeginInvokeOnMainThread(delegate {
                lock(this)
                {
                    if(iDataSource != null)
                    {
                        iDataSource.SetSource(aSource);
                    }
                }
            });

            iSource = aSource;
        }

        private static NSString kCellIdentifier = new NSString("CellDefault");

        private bool iOpen;

        private DataSource iDataSource;
        private Delegate iDelegate;

        private List<Linn.Kinsky.Source> iSources;
        private Linn.Kinsky.Source iSource;
    }
}