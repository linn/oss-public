
using System;
using System.Threading;


namespace Linn
{

    // Linn Timer class - implemented to be largely the same as the
    // System.Timers.Timer class. We have our own because the System
    // one is not present in the .NET compact framework.
    // The ElapsedEventArgs class is also not present - the event
    // handler in this implementation is a simple EventHandler (rather
    // than an ElapsedEventHandler)
    public class Timer : IDisposable
    {
        public Timer()
        {
            iTimer = new System.Threading.Timer(ElapsedEventInternal, null, Timeout.Infinite, Timeout.Infinite);
        }
        public Timer(double aInterval)
        {
            iInterval = (int)aInterval;
            iTimer = new System.Threading.Timer(ElapsedEventInternal, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            if(iTimer != null) {
                iTimer.Dispose();
            }
        }

        public bool AutoReset
        {
            get { return iAutoReset; }
            set {
                if (iEnabled)
                {
                    if (value)
                    {
                        iTimer.Change(iInterval, iInterval);
                    }
                    else
                    {
                        iTimer.Change(iInterval, Timeout.Infinite);
                    }
                }
                iAutoReset = value;
            }
        }
        public bool Enabled
        {
            get { return iEnabled; }
            set
            {
                System.Console.WriteLine("Enabled: value=" + value);
                if (value)
                {
                    if (iAutoReset)
                    {
                        iTimer.Change(iInterval, iInterval);
                    }
                    else
                    {
                        iTimer.Change(iInterval, Timeout.Infinite);
                    }
                }
                iEnabled = value;
            }
        }
        public double Interval
        {
            get { return iInterval; }
            set {
                if (iEnabled)
                {
                    if (iAutoReset)
                    {
                        iTimer.Change((int)value, (int)value);
                    }
                    else
                    {
                        iTimer.Change((int)value, Timeout.Infinite);
                    }
                }
                iInterval = (int)value;
            }
        }
        public void Start()
        {
            if (iAutoReset)
            {
                iTimer.Change(iInterval, iInterval);
            }
            else
            {
                iTimer.Change(iInterval, Timeout.Infinite);
            }
            iEnabled = true;
        }
        public void Stop()
        {
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);
            iEnabled = false;
        }
        public event EventHandler Elapsed;

        private void ElapsedEventInternal(object aSender)
        {
            if (!iAutoReset)
            {
                iEnabled = false;
            }
            Elapsed(this, new EventArgs());
        }
        private bool iAutoReset = true;
        private bool iEnabled = false;
        private int iInterval = 0;
        private System.Threading.Timer iTimer = null;
    }

}   // namespace Linn

