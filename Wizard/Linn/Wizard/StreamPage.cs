using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;

namespace Linn.Wizard
{
    public class StreamPage : BasePage
    {
        public StreamPage(PageControl aPageControl, PageDefinitions.Page aPageDefinintion)
            : base(aPageControl, aPageDefinintion) {

        }


        protected override void OnActivated(Session aSession) {

            base.OnActivated(aSession);

            Box box = aSession.Model.SelectedBox;
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
            Box box = aSession.Model.SelectedBox;

            box.Playback.Stop();
            box.Playback.EventPlaybackInfo -= PlaybackInfo;
            box.Playback.EventVolumeChanged -= VolumeChanged;
            box.Playback.EventStandbyChanged -= StandbyChanged;
        }


        protected override void OnReceive(Session aSession, string aName, string aValue) {

            Box box = aSession.Model.SelectedBox;
            switch (aName) {

                case "Play":
                    box.Playback.Start(Path.Combine(OpenHome.Xen.Environment.AppPath, "AudioTrack.flac"));
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
            Playback pb = obj as Playback;
            if (pb.Standby)
            {
                Send("MainTextVolume", "Volume: Device in standby");
            }
            else {
                Send("MainTextVolume", "Volume: " + e.Volume);
            }
        }

        private void StandbyChanged(object obj, EventArgsStandby e) {
            Playback pb = obj as Playback;
            if (e.Standby) {
                Send("MainTextVolume", "Volume: Device in standby");
            }
            else {
                Send("MainTextVolume", "Volume: " + pb.Volume.ToString());
            }
        }

    }


}
