using System;

using Linn.Kinsky;

namespace MagicMouseControl
{
    public class ViewWidgetVolumeControl : IViewWidgetVolumeControl
    {
        public ViewWidgetVolumeControl()
        {
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void SetVolume(uint aVolume)
        {
        }

        public void SetMute(bool aMute)
        {
            lock(this)
            {
                iMute = aMute;
            }
        }

        public void SetVolumeLimit(uint aVolumeLimit)
        {
        }

        public event EventHandler<EventArgs> EventVolumeIncrement;
        public event EventHandler<EventArgs> EventVolumeDecrement;
        public event EventHandler<EventArgsVolume> EventVolumeChanged;
        public event EventHandler<EventArgsMute> EventMuteChanged;

        public void IncrementVolume()
        {
            if(EventVolumeIncrement != null)
            {
                EventVolumeIncrement(this, EventArgs.Empty);
            }
        }

        public void DecrementVolume()
        {
            if(EventVolumeDecrement != null)
            {
                EventVolumeDecrement(this, EventArgs.Empty);
            }
        }

        public void Mute()
        {
            bool mute;
            lock(this)
            {
                mute = iMute;
            }

            if(EventMuteChanged != null)
            {
                EventMuteChanged(this, new EventArgsMute(!mute));
            }
        }

        private bool iMute;
    }
}

