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
using Linn.Kinsky;
using Android.Graphics;

namespace KinskyDroid
{

    [Activity(Label = "Linn Kinsky",
        Theme = "@android:style/Theme.NoTitleBar",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenLayout)]
    public class MainActivity : ObservableActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                iStack = this.Application as Stack;
                SetContentView(Resource.Layout.Main);
                iStack.StackStarted += StackStarted;
                iStack.StackStopped += StackStopped;
                Button saveButton = FindViewById<Button>(Resource.Id.savebutton);
                ImageButton deleteButton = FindViewById<ImageButton>(Resource.Id.deletebutton);
                ToggleButton editButton = FindViewById<ToggleButton>(Resource.Id.editmodebutton);
                editButton.Click += (d, e) =>
                {
                    saveButton.Visibility = editButton.Checked ? ViewStates.Gone : ViewStates.Visible;
                    deleteButton.Visibility = editButton.Checked ? ViewStates.Visible : ViewStates.Gone;
                };
            }
            catch (Exception ex)
            {
                UserLog.WriteLine("OnCreate:: " + ex);
            }
        }


        private void StackStarted(object sender, EventArgs e)
        {
            iStack.Invoker.BeginInvoke((Action)(() =>
            {
                StartUI();
            }));
        }

        private void StackStopped(object sender, EventArgs e)
        {
            iStack.Invoker.BeginInvoke((Action)(() =>
            {
                StopUI();
            }));
        }

        private void StartUI()
        {
            if (!iStarted)
            {
                iBrowser = new ViewWidgetBrowser(this.ApplicationContext, iStack.Location, iStack.Invoker, iStack.ImageCache, iStack.IconResolver, FindViewById<Button>(Resource.Id.backbutton), FindViewById<TextView>(Resource.Id.locationdisplay));

                (FindViewById(Resource.Id.browser) as RelativeLayout).AddView(iBrowser);
                Button selectRoom = FindViewById<Button>(Resource.Id.selectroombutton);
                Button selectSource = FindViewById<Button>(Resource.Id.selectsourcebutton);
                iRoomSourcePopupsMediator = new RoomSourcePopupsMediator(ApplicationContext, iStack, selectRoom, selectSource);
                iStarted = true;
            }
        }

        private void StopUI()
        {
            if (iStarted)
            {
                (FindViewById(Resource.Id.browser) as RelativeLayout).RemoveView(iBrowser);
                iRoomSourcePopupsMediator.Close();
                iStarted = false;
            }
        }

        protected override void OnDestroy()
        {
            iStack.StackStarted -= StackStarted;
            iStack.StackStopped -= StackStopped;
            base.OnDestroy();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            Assert.Check(!iStack.Invoker.InvokeRequired);
            UserLog.WriteLine("OnConfigurationChanged()");
            base.OnConfigurationChanged(newConfig);
            // restart ui
            SetContentView(Resource.Layout.Main);
            if (iStack.IsRunning && iStarted)
            {
                StopUI();
                StartUI();
            }
        }


        private Stack iStack;
        private ViewWidgetBrowser iBrowser;
        private RoomSourcePopupsMediator iRoomSourcePopupsMediator;
        private bool iStarted;
    }



}


