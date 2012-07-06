using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Linn;
using Linn.ControlPoint.Upnp;
using Linn.Control.Ssdp;
using Linn.Topology;
using Android.Net.Wifi;
using Android.Content.Res;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using OssToolkitDroid;
using Android.Content.PM;

namespace LinnTopologyDroid
{

    [Activity(Label = "Linn Topology",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
    public class TabsActivity : TabActivity
    {
        protected override void OnStart()
        {
            base.OnStart();
            ApplicationDroid.Instance.ActivityStarted(this);
        }
        protected override void OnStop()
        {
            base.OnStop();
            ApplicationDroid.Instance.ActivityStopped(this);
        }

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.Main);
                InitaliseTabs();
            }
            catch (Exception ex)
            {
                UserLog.WriteLine("OnCreate:: " + ex);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            TabHost tabHost = this.TabHost;
            tabHost.ClearAllTabs();
            SetContentView(Resource.Layout.Main);
            InitaliseTabs();
        }


        private void InitaliseTabs()
        {
            Resources res = this.ApplicationContext.Resources;
            TabHost tabHost = this.TabHost;
            tabHost.AddTab(tabHost.NewTabSpec("tabRoomSourceList").SetIndicator(res.GetString(Resource.String.roomlist)).SetContent(new Intent(this, typeof(RoomSourceListActivity))));
            tabHost.AddTab(tabHost.NewTabSpec("tabLibraryList").SetIndicator(res.GetString(Resource.String.mediaserverlist)).SetContent(new Intent(this, typeof(LibraryListActivity))));
            tabHost.CurrentTab = 0;
        }
    }


    [Activity(Label = "Rooms")]
    public class RoomSourceListActivity : ObservableActivity
    {

        private Stack iStack;
        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.RoomSourceList);
                InitaliseLayout();
                iStack = Application as Stack;
                iStack.StackStarted += StackStarted;
                iStack.StackStopped += StackStopped;
                RefreshRooms();
            }
            catch (Exception ex)
            {
                UserLog.WriteLine("OnCreate:: " + ex);
            }
        }

        private void StackStarted(object sender, EventArgs args)
        {
            RunOnUiThread(() =>
            {
                iStack.House.RoomAdded += RoomAdded;
                iStack.House.RoomRemoved += RoomRemoved;
                RefreshRooms();
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            iStack.StackStarted -= StackStarted;
            iStack.StackStopped -= StackStopped;
        }

        private void StackStopped(object sender, EventArgs args)
        {
            RunOnUiThread(() =>
            {
                iStack.House.RoomAdded -= RoomAdded;
                iStack.House.RoomRemoved -= RoomRemoved;
            });
        }

        private void RefreshRooms()
        {
            iRoomListAdapter.Clear();
            foreach (IRoom room in iStack.House.Rooms)
            {
                iRoomListAdapter.Add(room);
            }
            iRoomListAdapter.NotifyDataSetChanged();
        }

        private void RoomAdded(object sender, EventArgsRoom e)
        {
            RunOnUiThread(() =>
            {
                iRoomListAdapter.Add(e.Room);
                iRoomListAdapter.NotifyDataSetChanged();
            });
        }

        private void RoomRemoved(object sender, EventArgsRoom e)
        {
            RunOnUiThread(() =>
            {
                iRoomListAdapter.Remove(e.Room);
                iRoomListAdapter.NotifyDataSetChanged();
            });
        }

        private void InitaliseLayout()
        {
            Resources res = this.ApplicationContext.Resources;
            iRoomList = (ListView)FindViewById(Resource.Id.roomlist);
            iRoomList.ItemClick += RoomListItemClick;
            if (iRoomListAdapter == null)
            {
                iRoomListAdapter = new RoomArrayAdapter(this, Resource.Layout.Room, new List<IRoom>());
            }
            iRoomList.Adapter = iRoomListAdapter;
            iSourceList = (ListView)FindViewById(Resource.Id.sourcelist);
            iSourceList.ItemClick += SourceListItemClick;
            if (iSourceListAdapter == null)
            {
                iSourceListAdapter = new SourceArrayAdapter(this, Resource.Layout.Source, new List<ISource>());
            }
            iSourceList.Adapter = iSourceListAdapter;
        }

        void SourceListItemClick(object sender, Android.Widget.AdapterView.ItemClickEventArgs e)
        {
            ISource source = iSourceListAdapter.GetItem(e.Position);
            source.Select();
        }

        void RoomListItemClick(object sender, Android.Widget.AdapterView.ItemClickEventArgs e)
        {
            e.View.Selected = true;
            iRoomListAdapter.NotifyDataSetChanged();
            if (iCurrentRoom != null)
            {
                iCurrentRoom.EventSourceAdded -= EventSourceAdded;
                iCurrentRoom.EventSourceRemoved -= EventSourceRemoved;
                iCurrentRoom.EventCurrentChanged -= EventCurrentChanged;
            }
            iCurrentRoom = iRoomListAdapter.GetItem(e.Position);
            iCurrentRoom.EventSourceAdded += EventSourceAdded;
            iCurrentRoom.EventSourceRemoved += EventSourceRemoved;
            iCurrentRoom.EventCurrentChanged += EventCurrentChanged;
            iSourceListAdapter.Clear();
            foreach (ISource source in iCurrentRoom.Sources)
            {
                iSourceListAdapter.Add(source);
            }
            iSourceListAdapter.NotifyDataSetChanged();

        }

        private void EventSourceAdded(object sender, EventArgsSource e)
        {
            RunOnUiThread(() =>
            {
                iSourceListAdapter.Add(e.Source);
                iSourceListAdapter.NotifyDataSetChanged();
            });
        }
        private void EventSourceRemoved(object sender, EventArgsSource e)
        {
            RunOnUiThread(() =>
            {
                iSourceListAdapter.Remove(e.Source);
                iSourceListAdapter.NotifyDataSetChanged();
            });
        }
        private void EventCurrentChanged(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                iSourceListAdapter.NotifyDataSetChanged();
            });
        }


        private RoomArrayAdapter iRoomListAdapter;
        private ListView iRoomList;
        private SourceArrayAdapter iSourceListAdapter;
        private ListView iSourceList;
        private IRoom iCurrentRoom;
    }

    public class RoomArrayAdapter : ArrayAdapter<IRoom>
    {

        private Context context;

        public RoomArrayAdapter(Context context, int textViewResourceId, IList<IRoom> items)
            : base(context, textViewResourceId, items)
        {
            this.context = context;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                View view = convertView;
                if (view == null)
                {
                    LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                    view = inflater.Inflate(Resource.Layout.Room, null);
                }
                IRoom item = this.GetItem(position);
                if (item != null)
                {
                    CheckedTextView itemView = (CheckedTextView)view.FindViewById(Resource.Id.room);
                    if (itemView != null)
                    {
                        itemView.Text = String.Format("{0}", item.Name);
                    }

                }

                return view;
            }
            catch (Exception e)
            {
                UserLog.WriteLine("GetView::Exception -> " + e);
                return null;
            }
        }

    }

    public class SourceArrayAdapter : ArrayAdapter<ISource>
    {

        private Context context;

        public SourceArrayAdapter(Context context, int textViewResourceId, IList<ISource> items)
            : base(context, textViewResourceId, items)
        {
            this.context = context;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                View view = convertView;
                if (view == null)
                {
                    LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                    view = inflater.Inflate(Resource.Layout.Source, null);
                }
                ISource item = this.GetItem(position);
                if (item != null)
                {
                    TextView itemView = (TextView)view.FindViewById(Resource.Id.source);
                    if (itemView != null)
                    {
                        itemView.Text = String.Format("{0}", item.Name);
                    }
                    ImageView imageView = (ImageView)view.FindViewById(Resource.Id.sourceimage);
                    if (imageView != null)
                    {
                        imageView.Visibility = item.Room.Current == item ? ViewStates.Visible : ViewStates.Invisible;
                    }
                }

                return view;
            }
            catch (Exception e)
            {
                UserLog.WriteLine("GetView::Exception -> " + e);
                return null;
            }
        }

    }

    [Activity(Label = "Library")]
    public class LibraryListActivity : Activity
    {
        private Stack iStack;
        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.MediaServerList);
                InitaliseLayout();
                iStack = Application as Stack;
                iStack.StackStarted += StackStarted;
                iStack.StackStopped += StackStopped;
                RefreshServers();
            }
            catch (Exception ex)
            {
                UserLog.WriteLine("OnCreate:: " + ex);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            iStack.StackStarted -= StackStarted;
            iStack.StackStopped -= StackStopped;
        }

        private void StackStarted(object sender, EventArgs args)
        {
            RunOnUiThread(() =>
            {
                iStack.Library.MediaServerAdded += MediaServerAdded;
                iStack.Library.MediaServerRemoved += MediaServerRemoved;
                RefreshServers();
            });
        }

        private void StackStopped(object sender, EventArgs args)
        {
            RunOnUiThread(() =>
            {
                iStack.Library.MediaServerAdded -= MediaServerAdded;
                iStack.Library.MediaServerRemoved -= MediaServerRemoved;
            });

        }

        private void RefreshServers()
        {
            iLibraryListAdapter.Clear();
            foreach (MediaServer s in iStack.Library.MediaServers)
            {
                iLibraryListAdapter.Add(s);
            }
            iLibraryListAdapter.NotifyDataSetChanged();
        }


        private void MediaServerAdded(object sender, EventArgsMediaServer e)
        {
            RunOnUiThread(() =>
            {
                iLibraryListAdapter.Add(e.MediaServer);
                iLibraryListAdapter.NotifyDataSetChanged();
            });
        }

        private void MediaServerRemoved(object sender, EventArgsMediaServer e)
        {
            RunOnUiThread(() =>
            {
                iLibraryListAdapter.Remove(e.MediaServer);
                iLibraryListAdapter.NotifyDataSetChanged();
            });
        }

        private void InitaliseLayout()
        {
            Resources res = this.ApplicationContext.Resources;
            iLibraryList = (ListView)FindViewById(Resource.Id.mediaserverlist);
            if (iLibraryListAdapter == null)
            {
                iLibraryListAdapter = new MediaServerArrayAdapter(this, Resource.Layout.MediaServer, new List<MediaServer>());
            }
            iLibraryList.Adapter = iLibraryListAdapter;
        }

        private MediaServerArrayAdapter iLibraryListAdapter;
        private ListView iLibraryList;
    }

    public class MediaServerArrayAdapter : ArrayAdapter<MediaServer>
    {

        private Context context;

        public MediaServerArrayAdapter(Context context, int textViewResourceId, IList<MediaServer> items)
            : base(context, textViewResourceId, items)
        {
            this.context = context;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                View view = convertView;
                if (view == null)
                {
                    LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                    view = inflater.Inflate(Resource.Layout.MediaServer, null);
                }
                MediaServer item = this.GetItem(position);
                if (item != null)
                {
                    TextView itemView = (TextView)view.FindViewById(Resource.Id.mediaserver);
                    if (itemView != null)
                    {
                        itemView.Text = String.Format("{0}", item.Name);
                    }
                }

                return view;
            }
            catch (Exception e)
            {
                UserLog.WriteLine("SetView::Exception -> " + e);
                return null;
            }
        }

    }

}


