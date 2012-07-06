using System;

using Android.App;
using Android.Views;
using Android.OS;

using Linn;
using Android.Content.Res;
using Android.Content.PM;

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
                if (iStack.IsTabletView)
                {
                    SetContentView(Resource.Layout.DummyTablet);
                }
                else
                {
                    SetContentView(Resource.Layout.DummyPhone);
                }
                iStack.StackStarted += StackStarted;
                iStack.StackStopped += StackStopped;
            }
            catch (Exception e)
            {
                UserLog.WriteLine("Exception in OnCreate()");
                throw e;
            }
        }

        protected override void OnPause()
        {
            Console.WriteLine(DateTime.Now + ": OnPause()");
            //iRunning = false;
            //iStack.StopStack();
            //if (!iStack.IsScreenOn)
            //{
            //    iStack.StopStack();
            //}
            base.OnPause();
        }

        protected override void OnResume()
        {
            Console.WriteLine(DateTime.Now + ": OnResume()");
            //iRunning = true;
            //if (iStack.IsScreenOn)
            //{
            //    iStack.StartStack();
            //}
            base.OnResume();
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
            Console.WriteLine("OnConfigurationChanged()");
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


