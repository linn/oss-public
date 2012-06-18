using Android.Widget;
using Linn.Kinsky;
using System;
using Upnp;
using Linn;
using Android.Content;
using OssToolkitDroid;
using Android.Views;
using Android.Graphics;
using Android.Util;
using System.Collections.Generic;
using Android.Runtime;

namespace KinskyDroid
{

    public class ViewVolumeControl : LinearLayout
    {

        public ViewVolumeControl(IntPtr aHandle, JniHandleOwnership aHandleOwnership)
            : base(aHandle, aHandleOwnership)
        {
            //SetWillNotDraw(false);
            Init();
        }

        public ViewVolumeControl(Context aContext, IAttributeSet aAttributeSet)
            : base(aContext, aAttributeSet)
        {
            //SetWillNotDraw(false);
            Init();
        }

        private void Init()
        {
            iBackground = (this.Context.ApplicationContext as Stack).ResourceManager.GetBitmap(Resource.Drawable.Wheel);
            ImageView img = new ImageView(this.Context);
            img.SetImageBitmap(iBackground);
            AddView(img);
            //this.Width = iBackground.Width;
            //this.Height = iBackground.Height;
        }

        //protected override void OnDraw(Canvas canvas)
        //{
        //    base.OnDraw(canvas);
        //}

        private Bitmap iBackground;
    }





    public class RoomSourcePopupsMediator
    {
        public RoomSourcePopupsMediator(Context aContext, Stack aStack, Button aRoomButton, Button aSourceButton)
        {
            iContext = aContext;
            iRoomAdapter = new ViewWidgetRoomAdapter(aContext, aStack.RoomSelector, aStack.Invoker, aStack.IconResolver);
            iSourceAdapter = new ViewWidgetSourceAdapter(aContext, aStack.SourceSelector, aStack.Invoker, aStack.IconResolver);
            iRoomButton = aRoomButton;
            iSourceButton = aSourceButton;
            iRoomButton.Enabled = true;
            iSourceButton.Enabled = true;
            iRoomAdapter.SelectorButton = iRoomButton;
            iSourceAdapter.SelectorButton = iSourceButton;
            iRoomButton.Click += selectRoom_Click;
            iSourceButton.Click += selectSource_Click;
        }

        public void Close()
        {
            if (iPopup != null)
            {
                iPopup.Dismiss();
            }
            iRoomButton.Click -= selectRoom_Click;
            iSourceButton.Click -= selectSource_Click;
            iRoomAdapter.Close();
            iSourceAdapter.Close();
            iRoomButton.Enabled = false;
            iSourceButton.Enabled = false;
            iRoomButton.Text = string.Empty;
            iSourceButton.Text = string.Empty;
        }

        private void selectRoom_Click(object sender, EventArgs e)
        {
            LayoutInflater inflater = (LayoutInflater)iContext.GetSystemService(Context.LayoutInflaterService);
            View popupView = inflater.Inflate(Resource.Layout.RoomList, null, false);
            ListView listView = popupView.FindViewById<ListView>(Resource.Id.roomlist);
            iRoomAdapter.ListView = listView;
            iRoomAdapter.EventUserSelectedItem += iRoomAdapter_EventUserSelectedItem;
            iPopup = ShowPopup(popupView, sender as View);
            iPopup.EventDismissed += RoomPopup_EventDismissed;
        }

        private void iRoomAdapter_EventUserSelectedItem(object sender, EventArgs e)
        {
            Assert.Check(iPopup != null);
            iPopup.Dismiss();
        }

        private void RoomPopup_EventDismissed(object sender, EventArgs e)
        {
            iRoomAdapter.EventUserSelectedItem -= iRoomAdapter_EventUserSelectedItem;
            iRoomAdapter.ListView = null;
            (sender as Popup).EventDismissed -= RoomPopup_EventDismissed;
            iPopup = null;
        }

        private void selectSource_Click(object sender, EventArgs e)
        {
            LayoutInflater inflater = (LayoutInflater)iContext.GetSystemService(Context.LayoutInflaterService);
            View popupView = inflater.Inflate(Resource.Layout.SourceList, null, false);
            ListView listView = popupView.FindViewById<ListView>(Resource.Id.sourcelist);
            iSourceAdapter.ListView = listView;
            iSourceAdapter.EventUserSelectedItem += iSourceAdapter_EventUserSelectedItem;
            iPopup = ShowPopup(popupView, sender as View);
            iPopup.EventDismissed += SourcePopup_EventDismissed;
        }

        private void iSourceAdapter_EventUserSelectedItem(object sender, EventArgs e)
        {
            Assert.Check(iPopup != null);
            iPopup.Dismiss();
        }

        private void SourcePopup_EventDismissed(object sender, EventArgs e)
        {
            iSourceAdapter.EventUserSelectedItem -= iSourceAdapter_EventUserSelectedItem;
            iSourceAdapter.ListView = null;
            (sender as Popup).EventDismissed -= SourcePopup_EventDismissed;
            iPopup = null;
        }

        private Popup ShowPopup(View aViewRoot, View aAnchor)
        {
            IWindowManager windowManager = iContext.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            int screenWidth = windowManager.DefaultDisplay.Width;
            Paint stroke = new Paint() { Color = Android.Graphics.Color.White, StrokeWidth = 1 };
            stroke.SetStyle(Paint.Style.Stroke);
            stroke.AntiAlias = true;
            Paint fill = new Paint();
            fill.Color = new Color(0, 0, 0, 200);
            fill.SetStyle(Paint.Style.Fill);
            Popup popup = new Popup(iContext, aViewRoot, aAnchor, screenWidth / 3, stroke, fill);
            popup.Show();
            return popup;
        }

        private Button iRoomButton;
        private Button iSourceButton;
        private Context iContext;
        private Popup iPopup;
        private ViewWidgetRoomAdapter iRoomAdapter;
        private ViewWidgetSourceAdapter iSourceAdapter;
    }

    public class ViewWidgetSourceAdapter : ViewWidgetSelectorAdapter<Source>
    {
        public ViewWidgetSourceAdapter(Context aContext, ViewWidgetSelector<Source> aViewWidgetSelector, IInvoker aInvoker, IconResolver aIconResolver)
            : base(aContext, aViewWidgetSelector, aInvoker, aIconResolver)
        {
        }

        protected override void AppendItemView(Context aContext, Source aItem, ViewCache aViewCache, ViewGroup aRoot)
        {
            aRoot.AddView(LayoutInflater.Inflate(Resource.Layout.SourceListItem, aRoot, false));
            UpdateView(aItem, aViewCache);
        }

        protected override void AppendPlaceholderView(Context aContext, ViewCache aViewCache, ViewGroup aRoot)
        {
            aRoot.AddView(LayoutInflater.Inflate(Resource.Layout.SourceListItem, aRoot, false));
            UpdateView(null, aViewCache);
        }


        protected override void RecycleItemView(Context aContext, Source aItem, ViewCache aViewCache)
        {
            UpdateView(aItem, aViewCache);
        }

        private void UpdateView(Source aSource, ViewCache aViewCache)
        {
            if (aSource != null)
            {
                aViewCache.FindViewById<TextView>(Resource.Id.sourcename).Text = aSource.Name;
                aViewCache.FindViewById<LazyLoadingImageView>(Resource.Id.sourceicon).SetImageBitmap(IconResolver.GetIcon(aSource).Image);
            }
            else
            {
                aViewCache.FindViewById<TextView>(Resource.Id.sourcename).Text = "";
                aViewCache.FindViewById<LazyLoadingImageView>(Resource.Id.sourceicon).SetImageBitmap(IconResolver.IconLoading.Image);
            }
        }

        protected override string GetSelectorText(Source aItem)
        {
            return aItem != null ? aItem.Name : "Select Source";
        }
    }

    public class ViewWidgetRoomAdapter : ViewWidgetSelectorAdapter<Room>
    {
        public ViewWidgetRoomAdapter(Context aContext, ViewWidgetSelector<Room> aViewWidgetSelector, IInvoker aInvoker, IconResolver aIconResolver)
            : base(aContext, aViewWidgetSelector, aInvoker, aIconResolver)
        {
        }

        protected override void AppendItemView(Context aContext, Room aItem, ViewCache aViewCache, ViewGroup aRoot)
        {
            aRoot.AddView(LayoutInflater.Inflate(Resource.Layout.RoomListItem, aRoot, false));
            UpdateView(aItem, aViewCache);
            ToggleButton standbyButton = aViewCache.FindViewById<ToggleButton>(Resource.Id.standbybutton);
            standbyButton.Click += standbyButton_Click;
        }

        protected override void AppendPlaceholderView(Context aContext, ViewCache aViewCache, ViewGroup aRoot)
        {
            aRoot.AddView(LayoutInflater.Inflate(Resource.Layout.RoomListItem, aRoot, false));
            UpdateView(null, aViewCache);
        }


        protected override void RecycleItemView(Context aContext, Room aItem, ViewCache aViewCache)
        {
            UpdateView(aItem, aViewCache);
        }

        protected override void DestroyItemView(Context aContext, ViewCache aViewCache)
        {
            ToggleButton standbyButton = aViewCache.FindViewById<ToggleButton>(Resource.Id.standbybutton);
            standbyButton.Click -= standbyButton_Click;
            base.DestroyItemView(aContext, aViewCache);
        }

        private void UpdateView(Room aRoom, ViewCache aViewCache)
        {
            if (aRoom != null)
            {
                aViewCache.FindViewById<TextView>(Resource.Id.roomname).Text = aRoom.Name;
                aViewCache.FindViewById<LazyLoadingImageView>(Resource.Id.roomicon).SetImageBitmap(IconResolver.GetIcon(aRoom).Image);
                ToggleButton standbyButton = aViewCache.FindViewById<ToggleButton>(Resource.Id.standbybutton);
                int index = iViewWidgetSelector.IndexOf(aRoom);
                standbyButton.Visibility = index == iViewWidgetSelector.IndexOf(iViewWidgetSelector.SelectedItem) ? ViewStates.Visible : ViewStates.Gone;
                standbyButton.Checked = aRoom.Standby;
                standbyButton.Tag = new Java.Lang.Integer(index);
            }
            else
            {
                aViewCache.FindViewById<TextView>(Resource.Id.roomname).Text = "";
                aViewCache.FindViewById<LazyLoadingImageView>(Resource.Id.roomicon).SetImageBitmap(IconResolver.IconLoading.Image);
                aViewCache.FindViewById<ToggleButton>(Resource.Id.standbybutton).Visibility = ViewStates.Gone;
            }
        }

        void standbyButton_Click(object sender, EventArgs e)
        {
            int index = (sender as ToggleButton).Tag.JavaCast<Java.Lang.Integer>().IntValue();
            Room room = iViewWidgetSelector.Item(index);
            room.Standby = !room.Standby;
            (sender as ToggleButton).Checked = room.Standby;
            OnEventUserSelectedItem();
        }

        protected override string GetSelectorText(Room aItem)
        {
            return aItem != null ? aItem.Name : "Select Room";
        }
    }

    public abstract class ViewWidgetSelectorAdapter<T> : AsyncArrayAdapter<T, string>
    {
        public ViewWidgetSelectorAdapter(Context aContext, ViewWidgetSelector<T> aViewWidgetSelector, IInvoker aInvoker, IconResolver aIconResolver)
            : base(aContext, aViewWidgetSelector, aInvoker)
        {
            iInvoker = aInvoker;
            iViewWidgetSelector = aViewWidgetSelector;
            iViewWidgetSelector.EventDataChanged += iViewWidgetSelector_EventDataChanged;
            iViewWidgetSelector.EventSelectionChanged += iViewWidgetSelector_EventSelectionChanged;
            iIconResolver = aIconResolver;
        }

        void iViewWidgetSelector_EventSelectionChanged(object sender, EventArgsSelection<T> e)
        {
            Refresh();
        }

        public event EventHandler<EventArgs> EventUserSelectedItem;

        public ListView ListView
        {
            set
            {
                if (iListView != null)
                {
                    iListView.ItemClick -= iListView_ItemClick;
                    iListView.Adapter = null;
                    iListView = null;
                }
                if (value != null)
                {
                    iListView = value;
                    iListView.ItemClick += iListView_ItemClick;
                    iListView.Adapter = this;
                    iListView.SetSelection(iViewWidgetSelector.IndexOf(iViewWidgetSelector.SelectedItem));
                }
                Refresh();
            }
        }

        public Button SelectorButton
        {
            set
            {
                iSelectorButton = value;
                if (iSelectorButton != null)
                {
                    T selectedItem = iViewWidgetSelector.SelectedItem;
                    iSelectorButton.Text = GetSelectorText(selectedItem);
                }
            }
        }

        protected abstract string GetSelectorText(T aItem);

        protected IconResolver IconResolver
        {
            get
            {
                return iIconResolver;
            }
        }

        void iViewWidgetSelector_EventDataChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        void iListView_ItemClick(object sender, ItemEventArgs e)
        {
            iViewWidgetSelector.SelectedItem = iViewWidgetSelector.Item(e.Position);
            OnEventUserSelectedItem();
        }

        private void Refresh()
        {
            Assert.Check(!iInvoker.InvokeRequired);
            NotifyDataSetChanged(); 
            if (iSelectorButton != null)
            {
                iSelectorButton.Text = GetSelectorText(iViewWidgetSelector.SelectedItem);
            }
        }


        public void Close()
        {
            if (iListView != null)
            {
                iListView.ItemClick -= iListView_ItemClick;
            }
            iViewWidgetSelector.EventDataChanged -= iViewWidgetSelector_EventDataChanged;
            iViewWidgetSelector.EventSelectionChanged -= iViewWidgetSelector_EventSelectionChanged;
            iViewWidgetSelector = null;
        }

        protected void OnEventUserSelectedItem()
        {
            EventHandler<EventArgs> eventUserSelectedItem = EventUserSelectedItem;
            if (eventUserSelectedItem != null)
            {
                eventUserSelectedItem(this, EventArgs.Empty);
            }
        }

        private ListView iListView;
        protected ViewWidgetSelector<T> iViewWidgetSelector;
        private IconResolver iIconResolver;
        private Button iSelectorButton;
        private IInvoker iInvoker;
    }

    public class ViewWidgetSelector<T> : IViewWidgetSelector<T>, IAsyncLoader<T>
    {
        public ViewWidgetSelector()
        {
            iItems = new List<T>();
        }

        public int IndexOf(T aItem)
        {
            return iItems.IndexOf(aItem);
        }

        public T SelectedItem
        {
            get
            {
                return iSelectedItem;
            }
            set
            {
                iSelectedItem = value;
                OnEventSelectionChanged();
            }
        }

        #region IAsyncLoader<T> Members

        public event EventHandler<EventArgs> EventDataChanged;

        public T Item(int aIndex)
        {
            Assert.Check(aIndex < iItems.Count);
            return iItems[aIndex];
        }

        public int Count
        {
            get { return iItems.Count; }
        }

        #endregion

        #region IViewWidgetSelector<T> Members

        public void Open()
        {
            iSelectedItem = default(T);
        }

        public void Close()
        {
            iItems.Clear();
        }

        public void InsertItem(int aIndex, T aItem)
        {
            iItems.Insert(aIndex, aItem);
        }

        public void RemoveItem(T aItem)
        {
            iItems.Remove(aItem);
        }

        public void ItemChanged(T aItem)
        {
            OnEventDataChanged();
        }

        public void SetSelected(T aItem)
        {
            iSelectedItem = aItem;
            OnEventDataChanged();
        }

        public event EventHandler<EventArgsSelection<T>> EventSelectionChanged;

        #endregion

        private void OnEventDataChanged()
        {
            EventHandler<EventArgs> eventDataChanged = EventDataChanged;
            if (eventDataChanged != null)
            {
                eventDataChanged(this, EventArgs.Empty);
            }
        }

        private void OnEventSelectionChanged()
        {
            EventHandler<EventArgsSelection<T>> eventSelectionChanged = EventSelectionChanged;
            if (eventSelectionChanged != null)
            {
                eventSelectionChanged(this, new EventArgsSelection<T>(iSelectedItem));
            }
        }

        private List<T> iItems;
        private T iSelectedItem;
    }

    public class ViewMain : RelativeLayout
    {
        public ViewMain(IntPtr aHandle, JniHandleOwnership aHandleOwnership)
            : base(aHandle, aHandleOwnership)
        {
            SetWillNotDraw(false);
        }

        public ViewMain(Context aContext, IAttributeSet aAttributeSet)
            : base(aContext, aAttributeSet)
        {
            SetWillNotDraw(false);
        }

        private void InitDrawables()
        {
            Stack stack = this.Context.ApplicationContext as Stack;
            iTopLeftImage = stack.ResourceManager.GetBitmap(Resource.Drawable.TopLeftEdge);
            iTopLeftSourceRect = new Rect(0, 0, iTopLeftImage.Width, iTopLeftImage.Height);

            iTopRightImage = stack.ResourceManager.GetBitmap(Resource.Drawable.TopRightEdge);
            iTopRightSourceRect = new Rect(0, 0, iTopRightImage.Width, iTopRightImage.Height);

            iTopFillerImage = stack.ResourceManager.GetBitmap(Resource.Drawable.TopFiller);
            iTopFillerSourceRect = new Rect(0, 0, iTopFillerImage.Width, iTopFillerImage.Height);

            iBottomLeftImage = stack.ResourceManager.GetBitmap(Resource.Drawable.BottomLeftEdge);
            iBottomLeftSourceRect = new Rect(0, 0, iBottomLeftImage.Width, iBottomLeftImage.Height);

            iBottomRightImage = stack.ResourceManager.GetBitmap(Resource.Drawable.BottomRightEdge);
            iBottomRightSourceRect = new Rect(0, 0, iBottomRightImage.Width, iBottomRightImage.Height);

            iBottomFillerImage = stack.ResourceManager.GetBitmap(Resource.Drawable.BottomFiller);
            iBottomFillerSourceRect = new Rect(0, 0, iBottomFillerImage.Width, iBottomFillerImage.Height);

            iLeftFillerImage = stack.ResourceManager.GetBitmap(Resource.Drawable.LeftFiller);
            iLeftFillerSourceRect = new Rect(0, 0, iLeftFillerImage.Width, iLeftFillerImage.Height);

            iRightFillerImage = stack.ResourceManager.GetBitmap(Resource.Drawable.RightFiller);
            iRightFillerSourceRect = new Rect(0, 0, iRightFillerImage.Width, iRightFillerImage.Height);

            iLogoImage = stack.ResourceManager.GetBitmap(Resource.Drawable.LinnLogo);
            iLogoSourceRect = new Rect(0, 0, iLogoImage.Width, iLogoImage.Height);

            iInitialised = true;
        }


        protected override void OnDraw(Canvas canvas)
        {
            if (!iInitialised)
            {
                InitDrawables();
            }
            Rect topLeftDestRect = new Rect(0, 0, iTopLeftImage.Width, iTopLeftImage.Height);
            canvas.DrawBitmap(iTopLeftImage, iTopLeftSourceRect, topLeftDestRect, null);
            Rect topRightDestRect = new Rect(canvas.Width - iTopRightImage.Width, 0, canvas.Width, iTopRightImage.Height);
            canvas.DrawBitmap(iTopRightImage, iTopRightSourceRect, topRightDestRect, null);


            Rect bottomLeftDestRect = new Rect(0, canvas.Height - iBottomLeftImage.Height, iBottomLeftImage.Width, canvas.Height);
            canvas.DrawBitmap(iBottomLeftImage, iBottomLeftSourceRect, bottomLeftDestRect, null);
            Rect bottomRightDestRect = new Rect(canvas.Width - iBottomRightImage.Width, canvas.Height - iBottomRightImage.Height, canvas.Width, canvas.Height);
            canvas.DrawBitmap(iBottomRightImage, iBottomRightSourceRect, bottomRightDestRect, null);

            if (canvas.Width > iTopLeftImage.Width + iTopRightImage.Width)
            {
                Rect topFillRect = new Rect(iTopLeftImage.Width, 0, canvas.Width - iTopRightImage.Width, iTopFillerImage.Height);
                canvas.DrawBitmap(iTopFillerImage, iTopFillerSourceRect, topFillRect, null);
            }
            if (canvas.Width > iBottomLeftImage.Width + iBottomRightImage.Width)
            {
                Rect bottomFillRect = new Rect(iBottomLeftImage.Width, canvas.Height - iBottomLeftImage.Height, canvas.Width - iBottomRightImage.Width, canvas.Height);
                canvas.DrawBitmap(iBottomFillerImage, iBottomFillerSourceRect, bottomFillRect, null);
            }
            if (canvas.Height > iTopLeftImage.Height + iBottomLeftImage.Height)
            {
                Rect leftFillRect = new Rect(0, iTopLeftImage.Height, iLeftFillerImage.Width, canvas.Height - iBottomLeftImage.Height);
                canvas.DrawBitmap(iLeftFillerImage, iLeftFillerSourceRect, leftFillRect, null);
            }
            if (canvas.Height > iTopRightImage.Height + iBottomRightImage.Height)
            {
                Rect rightFillRect = new Rect(canvas.Width - iRightFillerImage.Width, iTopRightImage.Height, canvas.Width, canvas.Height - iBottomRightImage.Height);
                canvas.DrawBitmap(iRightFillerImage, iRightFillerSourceRect, rightFillRect, null);
            }

            Rect logoDestRect = new Rect((canvas.Width / 2) - (iLogoImage.Width / 2), canvas.Height - iLogoImage.Height - kLogoMargin, (canvas.Width / 2) + (iLogoImage.Width / 2), canvas.Height - kLogoMargin);
            canvas.DrawBitmap(iLogoImage, iLogoSourceRect, logoDestRect, null);

            base.OnDraw(canvas);

        }

        private Bitmap iTopLeftImage;
        private Rect iTopLeftSourceRect;

        private Bitmap iTopRightImage;
        private Rect iTopRightSourceRect;

        private Bitmap iTopFillerImage;
        private Rect iTopFillerSourceRect;

        private Bitmap iBottomLeftImage;
        private Rect iBottomLeftSourceRect;

        private Bitmap iBottomRightImage;
        private Rect iBottomRightSourceRect;

        private Bitmap iBottomFillerImage;
        private Rect iBottomFillerSourceRect;

        private Bitmap iLeftFillerImage;
        private Rect iLeftFillerSourceRect;

        private Bitmap iRightFillerImage;
        private Rect iRightFillerSourceRect;

        private Bitmap iLogoImage;
        private Rect iLogoSourceRect;

        private bool iInitialised;
        private const int kLogoMargin = 7;
    }

    public class ViewWidgetBrowser : ViewFlipper
    {

        public ViewWidgetBrowser(Context aContext, Location aLocation, IInvoker aInvoker, AndroidImageCache aImageCache, IconResolver aIconResolver, Button aBackButton, TextView aLocationDisplay)
            : base(aContext)
        {
            Assert.Check(!aInvoker.InvokeRequired);
            iLocationDisplay = aLocationDisplay;
            iBackButton = aBackButton;
            iImageCache = aImageCache;
            iIconResolver = aIconResolver;
            this.LayoutParameters = new LayoutParams(LayoutParams.FillParent, LayoutParams.FillParent);
            iLocation = aLocation;
            iInvoker = aInvoker;

            for (int i = 0; i < iLocation.Containers.Count; i++)
            {
                IContainer container = iLocation.Containers[i];
                container.EventContentAdded += ContentAdded;
                container.EventContentRemoved += ContentRemoved;
                container.EventContentUpdated += ContentUpdated;
                iCurrent = new BrowserListView(Context, container, iInvoker, this, iImageCache, iIconResolver);
                AddView(iCurrent);
                ShowNext();
            }
            this.FocusableInTouchMode = true;
            this.RequestFocus();
            aBackButton.Click += new EventHandler(aBackButton_Click);
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (iLocation.Containers.Count > 0)
            {
                int index = iLocation.Containers.Count - 1;
                iBackButton.Text = index == 0 ? "" : iLocation.Containers[index - 1].Metadata.Title;
                iBackButton.Enabled = index > 0;
                iLocationDisplay.Text = iLocation.Containers[index].Metadata.Title;
                iBackButton.Visibility = iLocation.Containers.Count > 1 ? ViewStates.Visible : ViewStates.Gone;
            }
            else
            {
                iBackButton.Text = string.Empty;
                iBackButton.Enabled = false;
                iLocationDisplay.Text = string.Empty;
                iBackButton.Visibility = ViewStates.Gone;
            }
        }

        private void aBackButton_Click(object sender, EventArgs e)
        {
            Assert.Check(!iInvoker.InvokeRequired);
            if (CanGoUp())
            {
                Up(1);
            }
        }

        public bool CanGoUp()
        {
            return iLocation.Containers.Count > 1;
        }

        public void Up(uint aLevels)
        {
            if (iInvoker.TryBeginInvoke((Action<uint>)(Up), aLevels))
                return;
            Up(aLevels, true);
        }

        public void Up(uint aLevels, bool aAnimate)
        {
            if (iInvoker.TryBeginInvoke((Action<uint, bool>)(Up), aLevels, aAnimate))
                return;

            for (uint i = 0; i < aLevels && iLocation.Containers.Count > 1; ++i)
            {
                IContainer container = iLocation.Current;
                container.EventContentAdded -= ContentAdded;
                container.EventContentRemoved -= ContentRemoved;
                container.EventContentUpdated -= ContentUpdated;

                iLocation = iLocation.PreviousLocation();
                ShowPrevious();
                RemoveView(iCurrent);
                iCurrent = this.CurrentView as BrowserListView;
            }
            UpdateControls();
        }

        public void Down(container aContainer)
        {
            if (iInvoker.TryBeginInvoke((Action<container>)(Down), aContainer))
                return;
            Down(aContainer, true);
        }

        public void Down(container aContainer, bool aAnimate)
        {
            if (iInvoker.TryBeginInvoke((Action<container, bool>)(Down), aContainer, aAnimate))
                return;

            IContainer container = iLocation.Current.ChildContainer(aContainer);
            if (container != null)
            {
                iLocation = new Location(iLocation, container);

                container.EventContentAdded += ContentAdded;
                container.EventContentRemoved += ContentRemoved;
                container.EventContentUpdated += ContentUpdated;
                iCurrent = new BrowserListView(Context, iLocation.Current, iInvoker, this, iImageCache, iIconResolver);
                AddView(iCurrent);
                ShowNext();
            }
            UpdateControls();
        }

        public void Browse(Location aLocation)
        {
            if (iInvoker.TryBeginInvoke((Action<Location>)(Browse), aLocation))
                return;

            Location commonAncestor = iLocation.CommonAncestor(aLocation);
            int levelsUp = iLocation.Containers.Count - commonAncestor.Containers.Count;
            Up((uint)levelsUp, false);

            for (int i = commonAncestor.Containers.Count; i < aLocation.Containers.Count; i++)
            {
                container next = aLocation.Containers[i].Metadata;
                Down(next, false);
            }

            iCurrent.Refresh();
        }

        private void ContentAdded(object sender, EventArgs e)
        {
            if (iInvoker.TryBeginInvoke((Action<object, EventArgs>)(ContentAdded), sender, e))
                return;
            if (sender == iLocation.Current)
            {
                iCurrent.Refresh();
            }
        }

        private void ContentRemoved(object sender, EventArgsContentRemoved e)
        {
            if (iInvoker.TryBeginInvoke((Action<object, EventArgsContentRemoved>)(ContentRemoved), sender, e))
                return;

            iCurrent.Refresh();
        }

        private void ContentUpdated(object sender, EventArgs e)
        {
            if (iInvoker.TryBeginInvoke((Action<object, EventArgs>)(ContentUpdated), sender, e))
                return;
            iCurrent.Refresh();
        }

        private BrowserListView iCurrent;
        private Location iLocation;
        private IInvoker iInvoker;
        private AndroidImageCache iImageCache;
        private IconResolver iIconResolver;
        private Button iBackButton;
        private TextView iLocationDisplay;
    }

    public class BrowserListView : ListView, IAsyncLoader<upnpObject>
    {
        public BrowserListView(Context aContext, IContainer aContainer, IInvoker aInvoker, ViewWidgetBrowser aParent, AndroidImageCache aImageCache, IconResolver aIconResolver)
            : base(aContext)
        {
            iImageCache = aImageCache;
            iIconResolver = aIconResolver;
            iContainer = aContainer;
            iParent = aParent;
            iInvoker = aInvoker;
            CreateContentCollector();
            this.ItemClick += BrowserListView_ItemClick;
            this.LayoutParameters = new LayoutParams(LayoutParams.FillParent, LayoutParams.FillParent);
            //this.ItemLongClick += BrowserListView_ItemLongClick;
        }

        void iContentCollector_EventItemsFailed(object sender, EventArgsItemsFailed e)
        {
            //throw new NotImplementedException();
        }

        void iContentCollector_EventItemsLoaded(object sender, EventArgsItemsLoaded<upnpObject> e)
        {
            // precache images
            foreach (upnpObject item in e.Items)
            {
                Icon<Bitmap> icon = iIconResolver.GetIcon(item);
                if (icon.IsUri && !iImageCache.Contains(icon.ImageUri.OriginalString))
                {
                    iImageCache.Image(icon.ImageUri);
                }
            }
            EventHandler<EventArgs> evtDataChanged = EventDataChanged;
            if (evtDataChanged != null)
            {
                evtDataChanged(this, EventArgs.Empty);
            }
        }

        void iContentCollector_EventOpened(object sender, EventArgs e)
        {
            iInvoker.BeginInvoke((Action)(() =>
            {
                if (sender == iContentCollector)
                {
                    iAdaptorCount = iContentCollector.Count;
                    Adapter = new BrowserListAdaptor(Context, this, iInvoker, iImageCache, iIconResolver);
                    (Adapter as BrowserListAdaptor).NotifyDataSetChanged();
                }
            }));
        }

        private void CreateContentCollector()
        {
            Assert.Check(!iInvoker.InvokeRequired);
            if (iContentCollector != null)
            {
                Adapter = null;
                iContentCollector.EventOpened -= iContentCollector_EventOpened;
                iContentCollector.EventItemsLoaded -= iContentCollector_EventItemsLoaded;
                iContentCollector.EventItemsFailed -= iContentCollector_EventItemsFailed;
                iContentCollector.Dispose();
                iContentCollector = null;
            }
            iContentCollector = ContentCollectorMaster.Create(iContainer, new DictionaryBackedContentCache<upnpObject>(kCacheSize), kRangeSize, kThreadCount, kReadAheadRanges);
            iContentCollector.EventOpened += iContentCollector_EventOpened;
            iContentCollector.EventItemsLoaded += iContentCollector_EventItemsLoaded;
            iContentCollector.EventItemsFailed += iContentCollector_EventItemsFailed;
        }

        void BrowserListView_ItemLongClick(object sender, ItemEventArgs e)
        {
            upnpObject item = (Adapter as BrowserListAdaptor)[e.Position];
            if (item != null)
            {
                Toast.MakeText(Context, "Long click: " + item.Title, ToastLength.Long).Show();
            }
        }

        void BrowserListView_ItemClick(object sender, ItemEventArgs e)
        {
            upnpObject item = (Adapter as BrowserListAdaptor)[e.Position];
            if (item != null)
            {
                Activate(item);
            }
        }

        private void Activate(upnpObject aItem)
        {
            if (aItem is container)
            {
                iParent.Down(aItem as container);
            }
            else
            {
                Toast.MakeText(Context, "Activated: " + aItem.Title, ToastLength.Long).Show();
            }
        }

        internal void Refresh()
        {
            CreateContentCollector();
        }

        private IContainer iContainer;
        private IContentCollector<upnpObject> iContentCollector;

        #region IAsyncLoader<upnpObject> Members

        public event EventHandler<EventArgs> EventDataChanged;

        public upnpObject Item(int aIndex)
        {
            return iContentCollector.Item(aIndex, ERequestPriority.Foreground);
        }

        int IAsyncLoader<upnpObject>.Count
        {
            get
            {
                return iAdaptorCount;
            }
        }


        #endregion

        private IInvoker iInvoker;
        private int iAdaptorCount;
        private ViewWidgetBrowser iParent;
        private AndroidImageCache iImageCache;
        private IconResolver iIconResolver;
        private const int kCacheSize = 500;
        private const int kRangeSize = 10;
        private const int kThreadCount = 2;
        private const int kReadAheadRanges = 5;
    }


    public class BrowserListAdaptor : AsyncArrayAdapter<upnpObject, string>
    {
        public BrowserListAdaptor(Context aContext, IAsyncLoader<upnpObject> aLoader, IInvoker aInvoker, AndroidImageCache aImageCache, IconResolver aIconResolver)
            : base(aContext, aLoader, aInvoker)
        {
            iImageCache = aImageCache;
            iIconResolver = aIconResolver;
            iInvoker = aInvoker;
        }
        
        protected override void AppendItemView(Context aContext, upnpObject aItem, ViewCache aViewCache, ViewGroup aRoot)
        {
            aRoot.AddView(LayoutInflater.Inflate(Resource.Layout.BrowserItem, aRoot, false));
            PopulateView(aItem, aViewCache);
        }

        protected override void AppendPlaceholderView(Context aContext, ViewCache aViewCache, ViewGroup aRoot)
        {
            aRoot.AddView(LayoutInflater.Inflate(Resource.Layout.BrowserItem, aRoot, false));
            PopulatePlaceholder(aViewCache);
        }

        protected override void RecycleItemView(Context aContext, upnpObject aItem, ViewCache aViewCache)
        {
            PopulateView(aItem, aViewCache);
        }

        private void PopulatePlaceholder(ViewCache aViewCache)
        {
            LazyLoadingImageView imageView = aViewCache.FindViewById<LazyLoadingImageView>(Resource.Id.browseritemicon);
            Bitmap placeholder = iIconResolver.IconLoading.Image;
            imageView.SetImageBitmap(placeholder);
            TextView firstLine = aViewCache.FindViewById<TextView>(Resource.Id.browseritemfirstline);
            firstLine.Text = string.Empty;
            TextView secondLine = aViewCache.FindViewById<TextView>(Resource.Id.browseritemsecondLine);
            secondLine.Text = string.Empty;
        }

        private void PopulateView(upnpObject aItem, ViewCache aViewCache)
        {
            LazyLoadingImageView imageView = aViewCache.FindViewById<LazyLoadingImageView>(Resource.Id.browseritemicon);
            Bitmap placeholder = iIconResolver.IconLoading.Image;
            imageView.SetImageBitmap(placeholder);
            Icon<Bitmap> icon = iIconResolver.GetIcon(aItem);
            if (icon.IsUri)
            {
                imageView.LoadImage(iImageCache, icon.ImageUri);
            }
            else
            {
                imageView.SetImageBitmap(icon.Image);
            }
            TextView firstLine = aViewCache.FindViewById<TextView>(Resource.Id.browseritemfirstline);
            firstLine.Text = DidlLiteAdapter.Title(aItem);
            TextView secondLine = aViewCache.FindViewById<TextView>(Resource.Id.browseritemsecondLine);
            string artist = DidlLiteAdapter.Artist(aItem);
            string album = DidlLiteAdapter.Album(aItem);
            //TODO: abstract this out into a Kinsky class and refine algorithm to prioritise display
            string secondLineText = string.Format("{0} {1} {2}", artist, artist != string.Empty && album != string.Empty ? " / " : "", album);
            secondLine.Text = secondLineText;
        }

        private AndroidImageCache iImageCache;
        private IconResolver iIconResolver;
        private IInvoker iInvoker;
    }


}