using System;
using System.Net;
using System.Threading;

using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

namespace Linn
{
    public class SystemEvents
    {
        private static PowerManager iPowerManager;

        public static event EventHandler<PowerModeChangedEventArgs> PowerModeChanged;

        static SystemEvents()
        {
            iPowerManager = new PowerManager();
            iPowerManager.PowerNotify += PowerNotifyHandler;
            iPowerManager.EnableNotifications();
        }

        ~SystemEvents()
        {
            iPowerManager.PowerNotify -= PowerNotifyHandler;
            iPowerManager.Dispose();
        }

        private static void PowerModeChangedHandler(object sender, PowerModeChangedEventArgs e)
        {
            if (PowerModeChanged != null)
            {
                PowerModeChanged(null, e);
            }
        }

        private static void PowerNotifyHandler(object sender, EventArgs e)
        {            
            PowerModes powerMode = PowerModes.eUnknown;
            PowerManager.PowerInfo info = iPowerManager.GetNextPowerInfo();

            Trace.WriteLine(Trace.kCore, info.ToString());

            if (info.Flags == PowerManager.SystemPowerStates.Suspend)
            {
                powerMode = PowerModes.eSuspend;
            }
            else if (info.Message == PowerManager.MessageTypes.Resume)
            {
                powerMode = PowerModes.eResume;
            }
            else if (info.Message == PowerManager.MessageTypes.Change)
            {
                powerMode = PowerModes.eStatusChange;
            }

            Trace.WriteLine(Trace.kCore, powerMode);

            if (PowerModeChanged != null)
            {
                PowerModeChanged(null, new PowerModeChangedEventArgs(powerMode));
            }
        }
    }

    public class PowerManager : IDisposable
    {
        public enum SystemPowerStates : uint
        {
            On = 0x00010000,
            Off = 0x00020000,
            Critical = 0x00040000,
            Boot = 0x00080000,
            Idle = 0x00100000,
            Suspend = 0x00200000,
            Reset = 0x00800000
        }

        public enum PowerReqFlags : uint
        {
            POWER_NAME = 0x00000001,
            POWER_FORCE = 0x00001000,
        }

        public enum DevicePowerStates
        {
            PwrDeviceUnspecified = -1,
            FullOn = 0,     // Full On: full power,  full functionality
            D0 = FullOn,
            LowOn,          // Low Power On: fully functional at low power/performance
            D1 = LowOn,
            StandBy,        // Standby: partially powered with automatic wake
            D2 = StandBy,
            Sleep,          // Sleep: partially powered with device initiated wake
            D3 = Sleep,
            Off,            // Off: unpowered
            D4 = Off,
            PwrDeviceMaximum
        }

        [FlagsAttribute()]
        public enum MessageTypes : uint
        {
            Transition = 0x00000001,
            Resume = 0x00000002,
            Change = 0x00000004,
            Status = 0x00000008
        }

        public enum ACLineStatus : byte
        {
            Offline = 0x00,
            OnLine = 0x01,
            Unknown = 0xff
        }

        [FlagsAttribute()]
        public enum BatteryFlags : byte
        {
            High = 0x01,
            Low = 0x02,
            Critical = 0x04,
            Charging = 0x08,
            Reserved1 = 0x10,
            Reserved2 = 0x20,
            Reserved3 = 0x40,
            NoBattery = 0x80,
            Unknown = High | Low | Critical | Charging | Reserved1 | Reserved2 | Reserved3 | NoBattery
        }

        private enum Wait : uint
        {
            Object = 0x00000000,
            Abandoned = 0x00000080,
            Failed = 0xffffffff,
        }

        private const uint POWER_NOTIFY_ALL = 0xFFFFFFFF;
        private const int INFINITE = -1;
        private const int MSGQUEUE_NOPRECOMMIT = 1;

        private AutoResetEvent powerThreadAbort;
        private bool abortPowerThread = false;
        private bool powerThreadRunning = false;
        public EventHandler<EventArgs> PowerNotify;
        
        private Queue powerQueue;
        private IntPtr hMsgQ = IntPtr.Zero;
        private IntPtr hReq = IntPtr.Zero;

        private bool bDisposed = false;

        [StructLayout(LayoutKind.Sequential)]
        private struct MessageQueueOptions
        {
            /// <summary>
            /// Size of the structure in bytes.
            /// </summary>
            public uint Size;

            /// <summary>
            /// Describes the behavior of the message queue. Set to MSGQUEUE_NOPRECOMMIT to 
            /// allocate message buffers on demand and to free the message buffers after 
            /// they are read, or set to MSGQUEUE_ALLOW_BROKEN to enable a read or write 
            /// operation to complete even if there is no corresponding writer or reader present.
            /// </summary>
            public uint Flags;

            /// <summary>
            /// Number of messages in the queue.
            /// </summary>
            public uint MaxMessages;

            /// <summary>
            /// Number of bytes for each message, do not set to zero.
            /// </summary>
            public uint MaxMessage;

            /// <summary>
            /// Set to TRUE to request read access to the queue. Set to FALSE to request write 
            /// access to the queue.
            /// </summary>
            public uint ReadAccess;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PowerInfo
        {
            /// <summary>
            /// Defines the event type.
            /// </summary>
            /// <see cref="MessageTypes"/>
            public MessageTypes Message;

            /// <summary>
            /// One of the system power flags.
            /// </summary>
            /// <see cref="SystemPowerStates"/>
            public SystemPowerStates Flags;

            /// <summary>
            /// The byte count of SystemPowerState that follows. 
            /// </summary>
            public uint Length;

            /// <summary>
            /// Levels available in battery flag fields
            /// </summary>
            public uint NumLevels;

            /// <summary>
            /// Number of seconds of battery life remaining, 
            /// or 0xFFFFFFFF if remaining seconds are unknown.
            /// </summary>
            public uint BatteryLifeTime;

            /// <summary>
            /// Number of seconds of battery life when at full charge, 
            /// or 0xFFFFFFFF if full battery lifetime is unknown.
            /// </summary>
            public uint BatteryFullLifeTime;

            /// <summary>
            /// Number of seconds of backup battery life remaining, 
            /// or BATTERY_LIFE_UNKNOWN if remaining seconds are unknown.
            /// </summary>
            public uint BackupBatteryLifeTime;

            /// <summary>
            /// Number of seconds of backup battery life when at full charge, 
            /// or BATTERY_LIFE_UNKNOWN if full battery lifetime is unknown.
            /// </summary>
            public uint BackupBatteryFullLifeTime;

            /// <summary>
            /// AC power status. 
            /// </summary>
            /// <see cref="ACLineStatus"/>
            public ACLineStatus ACLineStatus;

            /// <summary>
            /// Battery charge status. 
            /// </summary>
            /// <see cref="BatteryFlags"/>
            public BatteryFlags BatteryFlag;

            /// <summary>
            /// Percentage of full battery charge remaining. 
            /// This member can be a value in the range 0 (zero) to 100, or 255 
            /// if the status is unknown. All other values are reserved.
            /// </summary>
            public byte BatteryLifePercent;

            /// <summary>
            /// Backup battery charge status. 
            /// </summary>
            public byte BackupBatteryFlag;

            /// <summary>
            /// Percentage of full backup battery charge remaining. 
            /// This value must be in the range of 0 to 100, or BATTERY_PERCENTAGE_UNKNOWN.
            /// </summary>
            public byte BackupBatteryLifePercent;

            public override string ToString()
            {
                return string.Format("Message={0},Flags={1},ACLineStatus={2},BatteryFlag={3},BatteryLifePercent={4}%", Message, Flags, ACLineStatus, BatteryFlag, BatteryLifePercent);
            }
        };

        ~PowerManager()
        {
            //Dispose();
        }

        public void Dispose()
        {
            if (!bDisposed)
            {
                // Try disabling notifications and ending the thread
                DisableNotifications();
                bDisposed = true;

                // SupressFinalize to take this object off the finalization queue 
                // and prevent finalization code for this object from executing a second time.
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Sets the system power state to the requested value.
        /// </summary>
        /// <param name="systemState">The system power state to set the device to.</param>
        /// <returns>Win32 error code</returns>
        /// <remarks>Should be used with extreme care since it may result in an unexpected 
        /// application or system behavior.</remarks>
        public int SetSystemPowerState(SystemPowerStates systemState)
        {
            uint nError = 0;

            nError = CESetSystemPowerState(
                IntPtr.Zero,
                (uint)systemState,
                0);

            return (int)nError;
        }

        /// <summary>
        /// Returns the current system power state currently in effect.
        /// </summary>
        /// <param name="systemStateName">Receives the system power state name</param>
        /// <param name="systemState">Receives the system power state</param>
        /// <returns>Win32 error code</returns>
        public int GetSystemPowerState(StringBuilder systemStateName, out SystemPowerStates systemState)
        {
            uint nError = 0;

            nError = CEGetSystemPowerState(systemStateName, (uint)systemStateName.Capacity, out systemState);

            return (int)nError;
        }

        /// <summary>
        /// Requests that the Power Manager change the power state of a device.
        /// </summary>
        /// <param name="deviceName">Specifies the device name, for example, COM1:.</param>
        /// <param name="deviceState">Indicates the device power state</param>
        /// <returns>Win32 error code</returns>
        /// <remarks>Should be used with extreme care since it may result in an unexpected 
        /// application or system behavior.</remarks>
        public int DevicePowerNotify(string deviceName, DevicePowerStates deviceState)
        {
            uint nError = 0;

            nError = CEDevicePowerNotify(deviceName, (uint)deviceState, (uint)PowerReqFlags.POWER_NAME);

            return (int)nError;
        }

        /// <summary>
        /// Activates notification events. An application can now register to PowerNotify and be 
        /// notified when a power notification is received.
        /// </summary>
        public void EnableNotifications()
        {
            // Set the message queue options
            MessageQueueOptions Options = new MessageQueueOptions();

            // Size in bytes ( 5 * 4)
            Options.Size = (uint)Marshal.SizeOf(Options);
            // Allocate message buffers on demand and to free the message buffers after they are read
            Options.Flags = MSGQUEUE_NOPRECOMMIT;
            // Number of messages in the queue.
            Options.MaxMessages = 32;
            // Number of bytes for each message, do not set to zero.
            Options.MaxMessage = 512;
            // Set to true to request read access to the queue.
            Options.ReadAccess = 1; // True

            // Create the queue and request power notifications on it
            hMsgQ = CECreateMsgQueue("PowerNotifications", ref Options);

            hReq = CERequestPowerNotifications(hMsgQ, POWER_NOTIFY_ALL);

            // If the above succeed
            if (hMsgQ != IntPtr.Zero && hReq != IntPtr.Zero)
            {
                powerQueue = new Queue();

                // Create an event so that we can kill the thread when we want
                powerThreadAbort = new AutoResetEvent(false);

                // Create the power watcher thread
                Thread thread = new Thread(PowerNotifyThread);
                thread.Name = "PowerNotifyThread";
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.BelowNormal;
                thread.Start();
            }
        }

        /// <summary>
        /// Disables power notification events.
        /// </summary>
        public void DisableNotifications()
        {
            // If we are already closed just exit
            if (!powerThreadRunning)
                return;

            // Stop receiving power notifications
            if (hReq != IntPtr.Zero)
                CEStopPowerNotifications(hReq);

            // Attempt to end the PowerNotifyThread
            abortPowerThread = true;
            powerThreadAbort.Set();

            // Wait for the thread to stop
            int count = 0;
            while (powerThreadRunning)
            {
                Thread.Sleep(100);

                // If it did not stop it time record this and give up
                if (count++ > 50)
                    break;
            }
        }

        /// <summary>
        /// Obtain the next PowerInfo structure
        /// </summary>
        public PowerInfo GetNextPowerInfo()
        {
            // Get the next item from the queue in a thread safe manner
            lock (powerQueue.SyncRoot)
                return (PowerInfo)powerQueue.Dequeue();
        }

        /// <summary>
        /// Worker thread that creates and reads a message queue for power notifications
        /// </summary>
        private void PowerNotifyThread()
        {
            powerThreadRunning = true;

            // Keep going util we are asked to quit
            while (!abortPowerThread)
            {
                IntPtr[] Handles = new IntPtr[2];

                Handles[0] = hMsgQ;
                Handles[1] = powerThreadAbort.Handle;

                // Wait on two handles because the message queue will never
                // return from a read unless messages are posted.
                Wait res = (Wait)CEWaitForMultipleObjects(
                                         (uint)Handles.Length,
                                         Handles,
                                         false,
                                         INFINITE);

                // Exit the loop if an abort was requested
                if (abortPowerThread)
                    break;

                // Else
                switch (res)
                {
                    // This must be an error - Exit loop and thread
                    case Wait.Abandoned:
                        abortPowerThread = true;
                        break;

                    // Timeout - Continue after a brief sleep
                    case Wait.Failed:
                        Thread.Sleep(500);
                        break;

                    // Read the message from the queue
                    case Wait.Object:
                        {
                            // Create a new structure to read into
                            PowerInfo Power = new PowerInfo();

                            uint PowerSize = (uint)Marshal.SizeOf(Power);
                            uint BytesRead = 0;
                            uint Flags = 0;

                            // Read the message
                            if (CEReadMsgQueue(hMsgQ, ref Power, PowerSize,
                                                ref BytesRead, 0, ref Flags))
                            {
                                // Set value to zero if percentage is not known
                                if ((Power.BatteryLifePercent < 0) || (Power.BatteryLifePercent > 100))
                                    Power.BatteryLifePercent = 0;

                                if ((Power.BackupBatteryLifePercent < 0) || (Power.BackupBatteryLifePercent > 100))
                                    Power.BackupBatteryLifePercent = 0;

                                // Add the power structure to the queue so that the 
                                // UI thread can get it
                                lock (powerQueue.SyncRoot)
                                    powerQueue.Enqueue(Power);

                                // Fire an event to notify the UI
                                if (PowerNotify != null)
                                    PowerNotify(this, null);
                            }

                            break;
                        }
                }
            }

            // Close the message queue
            if (hMsgQ != IntPtr.Zero)
                CECloseMsgQueue(hMsgQ);

            powerThreadRunning = false;
        }

        [DllImport("coredll.dll", EntryPoint = "RequestPowerNotifications")]
        private static extern IntPtr CERequestPowerNotifications(IntPtr hMsgQ, uint Flags);

        [DllImport("coredll.dll", EntryPoint = "StopPowerNotifications")]
        private static extern bool CEStopPowerNotifications(IntPtr hReq);

        [DllImport("coredll.dll", EntryPoint = "SetDevicePower")]
        private static extern uint CESetDevicePower(string Device, uint dwDeviceFlags, uint DeviceState);

        [DllImport("coredll.dll", EntryPoint = "GetDevicePower")]
        private static extern uint CEGetDevicePower(string Device, uint dwDeviceFlags, uint DeviceState);

        [DllImport("coredll.dll", EntryPoint = "DevicePowerNotify")]
        private static extern uint CEDevicePowerNotify(string Device, uint DeviceState, uint Flags);

        [DllImport("coredll.dll", EntryPoint = "SetSystemPowerState")]
        private static extern uint CESetSystemPowerState(IntPtr sState, uint StateFlags, uint Options);

        [DllImport("coredll.dll", EntryPoint = "GetSystemPowerState")]
        private static extern uint CEGetSystemPowerState(StringBuilder Buffer, uint Length, out SystemPowerStates Flags);

        [DllImport("coredll.dll", EntryPoint = "CreateMsgQueue")]
        private static extern IntPtr CECreateMsgQueue(string Name, ref MessageQueueOptions Options);

        [DllImport("coredll.dll", EntryPoint = "CloseMsgQueue")]
        private static extern bool CECloseMsgQueue(IntPtr hMsgQ);

        [DllImport("coredll.dll", EntryPoint = "ReadMsgQueue")]
        private static extern bool CEReadMsgQueue(IntPtr hMsgQ, ref PowerInfo Power, uint BuffSize, ref uint BytesRead, uint Timeout, ref uint Flags);

        [DllImport("coredll.dll", EntryPoint = "WaitForMultipleObjects", SetLastError = true)]
        private static extern int CEWaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool fWaitAll, int dwMilliseconds);
    }
}

namespace System.Net.NetworkInformation
{
    internal class NetworkChange
    {
        private const int kThreadSleepPeriod = 1000;

        private static Thread iThread;
        private static IPHostEntry iIpHostEntry;

        public static event EventHandler<EventArgs> NetworkAddressChanged;
        public static event EventHandler<EventArgs> NetworkAvailabilityChanged { add { } remove { } }

        static NetworkChange()
        {
            iThread = new Thread(CheckNetworkAvailability);
            iThread.Name = "NetworkChange";
            iThread.IsBackground = true;
            iThread.Priority = ThreadPriority.BelowNormal;

            iIpHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            iThread.Start();
        }

        ~NetworkChange()
        {
            iThread.Abort();
            iThread.Join();

            iIpHostEntry = null;
        }

        private static void CheckNetworkAvailability()
        {
            while (true)
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

                if (hostEntry.AddressList.Length != iIpHostEntry.AddressList.Length)
                {
                    iIpHostEntry = hostEntry;

                    if (NetworkAddressChanged != null)
                    {
                        NetworkAddressChanged(null, EventArgs.Empty);
                    }
                }
                else
                {
                    for (int i = 0; i < iIpHostEntry.AddressList.Length; ++i)
                    {
                        if (iIpHostEntry.AddressList[i].ToString() != hostEntry.AddressList[i].ToString())
                        {
                            iIpHostEntry = hostEntry;

                            if (NetworkAddressChanged != null)
                            {
                                NetworkAddressChanged(null, EventArgs.Empty);
                            }

                            break;
                        }
                    }
                }

                Thread.Sleep(kThreadSleepPeriod);
            }
        }
    }
}
