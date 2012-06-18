using System;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using Linn;
using Linn.Kinsky;
using Linn.Toolkit.Ios;

namespace KinskyTouch
{
    [MonoTouch.Foundation.Register("HelperKinskyTouch")]
    partial class HelperKinskyTouch : NSObject
    {
        private class Invoker : IInvoker
        {
            public void BeginInvoke(Delegate aDelegate, params object[] aArgs)
            {
                UIApplication.SharedApplication.BeginInvokeOnMainThread(delegate {
                    aDelegate.DynamicInvoke(aArgs);
                });
            }

            public bool TryBeginInvoke(Delegate aDelegate, params object[] aArgs)
            {
                if(InvokeRequired)
                {
                    BeginInvoke(aDelegate, aArgs);
                    return true;
                }

                return false;
            }

            public bool InvokeRequired
            {
                get
                {
                    return !NSThread.Current.IsMainThread;
                }
            }
        }

        public HelperKinskyTouch(IntPtr aInstance)
            : base(aInstance)
        {
            iHelper = new HelperKinsky(new string[] {}, new Invoker());

            UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
            NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.BatteryStateDidChangeNotification, delegate {
                EventOptionAutoLockValueChanged(this, EventArgs.Empty);
            });

            iCrashLogDumper = new CrashLogDumper(iHelper.Title, iHelper.Product, iHelper.Version);
            iHelper.AddCrashLogDumper(iCrashLogDumper);

            iOptionPageGeneral = new OptionPageGeneral("General");
            iHelper.AddOptionPage(iOptionPageGeneral);

            iOptionInsertMode = new OptionInsertMode();
            iHelper.AddOption(iOptionInsertMode);

            iOptionPageGeneral.OptionAutoLock.EventValueChanged += EventOptionAutoLockValueChanged;
            iOptionPageGeneral.OptionAutoSendCrashLog.EventValueChanged += EventOptionAutoSendCrashLogValueChanged;

            iHelper.ProcessOptionsFileAndCommandLine();

            EventOptionAutoLockValueChanged(this, EventArgs.Empty);
            EventOptionAutoSendCrashLogValueChanged(this, EventArgs.Empty);
        }

        public HelperKinsky Helper
        {
            get
            {
                return iHelper;
            }
        }

        public OptionBool OptionExtendedTrackInfo
        {
            get
            {
                return iOptionPageGeneral.OptionExtendedTrackInfo;
            }
        }

        public OptionBool OptionEnableLargeControls
        {
            get
            {
                return iOptionPageGeneral.OptionEnableLargeControls;
            }
        }

        public OptionBool OptionEnableRocker
        {
            get
            {
                return iOptionPageGeneral.OptionEnableRocker;
            }
        }

        public OptionBool OptionGroupTracks
        {
            get
            {
                return iOptionPageGeneral.OptionGroupTracks;
            }
        }

        public OptionEnum OptionInsertMode
        {
            get
            {
                return iOptionInsertMode;
            }
        }

        private void EventOptionAutoLockValueChanged(object sender, EventArgs e)
        {
            if(iOptionPageGeneral.OptionAutoLock.Value == "Always")
            {
                UIApplication.SharedApplication.IdleTimerDisabled = true;
            }
            else if(iOptionPageGeneral.OptionAutoLock.Value == "When charging")
            {
                if(UIDevice.CurrentDevice.BatteryState == UIDeviceBatteryState.Unplugged)
                {
                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                }
                else
                {
                    UIApplication.SharedApplication.IdleTimerDisabled = true;
                }
            }
            else
            {
                UIApplication.SharedApplication.IdleTimerDisabled = false;
            }
        }

        private void EventOptionAutoSendCrashLogValueChanged(object sender, EventArgs e)
        {
            iCrashLogDumper.SetAutoSend(iOptionPageGeneral.OptionAutoSendCrashLog.Native);
        }

        private HelperKinsky iHelper;
        private CrashLogDumper iCrashLogDumper;
        private OptionPageGeneral iOptionPageGeneral;
        private OptionEnum iOptionInsertMode;
    }
}

