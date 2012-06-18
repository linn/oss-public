
using System;

namespace Linn
{
    public static class SystemEvents
    {
        public static event EventHandler<PowerModeChangedEventArgs> PowerModeChanged;

        static SystemEvents()
        {
            Microsoft.Win32.SystemEvents.PowerModeChanged += EventPowerModeChanged;
        }

        private static void EventPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            PowerModes powerMode = PowerModes.eUnknown;
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.Suspend:
                    powerMode = PowerModes.eSuspend;
                    break;
                case Microsoft.Win32.PowerModes.Resume:
                    powerMode = PowerModes.eResume;
                    break;
                case Microsoft.Win32.PowerModes.StatusChange:
                    powerMode = PowerModes.eStatusChange;
                    break;
            }

            UserLog.WriteLine(DateTime.Now + ": Power mode event - " + e.Mode);

            if (PowerModeChanged != null)
            {
                PowerModeChanged(null, new PowerModeChangedEventArgs(powerMode));
            }
        }
    }
}
