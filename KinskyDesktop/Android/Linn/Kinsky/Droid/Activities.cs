using System;

using Android.App;
using Android.Views;
using Android.OS;

using Linn;
using Android.Content.Res;
using Android.Content.PM;
using Android.Widget;

namespace KinskyDroid
{

    [Activity(Label = "Kinsky",
        Theme = "@android:style/Theme.NoTitleBar",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenLayout)]
    public class MainActivity : ObservableActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            Console.WriteLine(DateTime.Now + ": OnCreate()");
            try
            {
                base.OnCreate(bundle);
                iStack = this.Application as Stack;
                double containerWidth, width;
                if (iStack.IsTabletView)
                {
                    SetContentView(Resource.Layout.DummyTablet);
                    containerWidth = Math.Min(iStack.Resources.DisplayMetrics.WidthPixels, iStack.Resources.DisplayMetrics.HeightPixels) / 2;
                    width = containerWidth;
                }
                else
                {
                    RequestedOrientation = ScreenOrientation.Portrait;
                    SetContentView(Resource.Layout.DummyPhone);
                    containerWidth = iStack.Resources.DisplayMetrics.WidthPixels;
                    width = Math.Min(containerWidth, 600);
                    new ToolbarLayoutPhone(FindViewById<ViewGroup>(Resource.Id.trackartworkcontainer),
                                           FindViewById<ViewGroup>(Resource.Id.playlistbuttons),                                           
                                           FindViewById<ViewGroup>(Resource.Id.trackcontrols), null, null)
                    .Layout(iStack.Resources.DisplayMetrics.HeightPixels);
                }
                
                new ControlsLayout(FindViewById<RelativeLayout>(Resource.Id.volumeandtransportcontrolscontainer),
                                   FindViewById<RelativeLayout>(Resource.Id.volumeandtransportcontrols),
                                   FindViewById<TransportControls>(Resource.Id.transportcontrols),
                                   FindViewById<DisplayControl>(Resource.Id.timedisplay),
                                   FindViewById<DisplayControl>(Resource.Id.volumedisplay))
                .Layout(containerWidth, width);
                iStack.StackStarted += StackStarted;
                iStack.StackStopped += StackStopped;
            }
            catch (Exception e)
            {
                UserLog.WriteLine("Exception in OnCreate()");
                throw e;
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (iViewKinsky != null)
            {
                if (iViewKinsky.OnKeyDown(keyCode, e))
                {
                    return true;
                }
            }
            return base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            if (iViewKinsky != null)
            {
                if (iViewKinsky.OnKeyUp(keyCode, e))
                {
                    return true;
                }
            }
            return base.OnKeyUp(keyCode, e);
        }

        private void StackStarted(object sender, EventArgs e)
        {
            iStack.Invoker.BeginInvoke((Action)(() =>
            {
                if (iViewKinsky == null)
                {
                    iViewKinsky = CreateViewKinsky();
                }
                iViewKinsky.Open();
                iStackStarted = true;
            }));
        }

        private void StackStopped(object sender, EventArgs e)
        {
            iStack.Invoker.BeginInvoke((Action)(() =>
            {
                if (iViewKinsky != null)
                {
                    iViewKinsky.Close();
                }
                iStackStarted = false;
            }));
        }

        protected override void OnDestroy()
        {
            Console.WriteLine(DateTime.Now + ": OnDestroy()");
            iStack.StackStarted -= StackStarted;
            iStack.StackStopped -= StackStopped;
            if (iViewKinsky != null)
            {
                iViewKinsky.Dispose();
                iViewKinsky = null;
            }
            base.OnDestroy();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            Assert.Check(!iStack.Invoker.InvokeRequired);
            UserLog.WriteLine("OnConfigurationChanged()");
            base.OnConfigurationChanged(newConfig);
            // restart ui
            if (iViewKinsky != null)
            {
                iViewKinsky.Dispose();
                iViewKinsky = CreateViewKinsky();
                if (iStackStarted)
                {
                    iViewKinsky.Open();
                }
            }
        }

        private ViewKinsky CreateViewKinsky()
        {
            if (iStack.IsTabletView)
            {
                return new ViewKinskyTablet(iStack, this, iStack.ViewMaster, iStack.ResourceManager, iStack.IconResolver);
            }
            else
            {
                return new ViewKinskyPhone(iStack, this, iStack.ViewMaster, iStack.ResourceManager, iStack.IconResolver);
            }
        }

        private Stack iStack;
        private ViewKinsky iViewKinsky;
        private bool iStackStarted;
    }


}


