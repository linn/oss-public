using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using OpenHome.Xapp;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;

namespace Linn.Wizard
{


    public class StreamPage : BasePage
    {
        public StreamPage(PageControl aPageControl, string aViewId, PageComponents aPageComponents, IPageNavigation aPageNavigation)
            : base(aPageControl, aViewId, aPageComponents, aPageNavigation) {

        }


        protected override void OnActivated(Session aSession) {

            base.OnActivated(aSession);
            
            Box box = iPageControl.SelectedBox;
            box.Playback.EventPlaybackInfo += PlaybackInfo;
            box.Playback.EventVolumeChanged += VolumeChanged;
            box.Playback.EventStandbyChanged += StandbyChanged;

            aSession.Send("MainTextStreamStatus", "Status: Click the play button to start streaming");

            if (box.Playback.Standby)
            {
                aSession.Send("MainTextVolume", "Volume: Device in standby");
            }
            else {
                aSession.Send("MainTextVolume", "Volume: " + box.Playback.Volume.ToString());
            }
        }


        protected override void OnDeactivated(Session aSession) {
            base.OnDeactivated(aSession);
            Box box = iPageControl.SelectedBox;

            box.Playback.Stop();
            box.Playback.EventPlaybackInfo -= PlaybackInfo;
            box.Playback.EventVolumeChanged -= VolumeChanged;
            box.Playback.EventStandbyChanged -= StandbyChanged;
        }


        protected override void OnReceive(Session aSession, string aName, string aValue) {

            Box box = iPageControl.SelectedBox;
            switch (aName) {

                case "Play":
                    box.Playback.Start();
                    break;

                case "Stop":
                    box.Playback.Stop();
                    break;

                case "VolumeInc":
                    box.Playback.VolumeInc();
                    break;

                case "VolumeDec":
                    box.Playback.VolumeDec();
                    break;

                default:
                    base.OnReceive(aSession, aName, aValue);
                    break;
            }
        }

        private void PlaybackInfo(object obj, EventArgsPlaybackInfo e) {
            Send("MainTextStreamStatus", "Status: " + e.PlaybackInfo);
        }

        private void VolumeChanged(object obj, EventArgsVolume e) {
            if (iPageControl.SelectedBox.Playback.Standby)
            {
                Send("MainTextVolume", "Volume: Device in standby");
            }
            else {
                Send("MainTextVolume", "Volume: " + e.Volume);
            }
        }

        private void StandbyChanged(object obj, EventArgsStandby e) {
            if (e.Standby) {
                Send("MainTextVolume", "Volume: Device in standby");
            }
            else {
                Send("MainTextVolume", "Volume: " + iPageControl.SelectedBox.Playback.Volume.ToString());
            }
        }

    }


}
