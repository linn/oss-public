using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using Linn.ProductSupport;

namespace LinnSetup
{
    public partial class PlaybackTest : UserControl
    {
        public PlaybackTest(Target aTarget) {
            iTarget = aTarget;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e) {
            if (iTarget.Box.State == Box.EState.eOn && !iTarget.Box.IsProxy) {
                Enable();
            }
            else {
                Disable();
            }
            base.OnLoad(e);
        }

        public void Enable() {
            this.BeginInvoke(
              (MethodInvoker)delegate() {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
            });

            if (!iEnabled) {
                iEnabled = true;

                iTarget.Box.Playback.EventPlaybackInfo += PlaybackInfo;
                iTarget.Box.Playback.EventPlaybackError += PlaybackError;
                iTarget.Box.Playback.EventTransportStateChanged += TransportStateChanged;
                iTarget.Box.Playback.EventVolumeChanged += VolumeChanged;
                iTarget.Box.Playback.EventStandbyChanged += StandbyChanged;
                iTarget.Box.Playback.EventTrackInfoChanged += TrackInfoChanged;
            }
        }

        public void Disable() {
            this.BeginInvoke(
              (MethodInvoker)delegate() {
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                pictureBox1.Image = null;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            });

            if (iEnabled) {
                iEnabled = false;

                iTarget.Box.Playback.EventPlaybackInfo -= PlaybackInfo;
                iTarget.Box.Playback.EventPlaybackError -= PlaybackError;
                iTarget.Box.Playback.EventTransportStateChanged -= TransportStateChanged;
                iTarget.Box.Playback.EventVolumeChanged -= VolumeChanged;
                iTarget.Box.Playback.EventStandbyChanged -= StandbyChanged;
                iTarget.Box.Playback.EventTrackInfoChanged -= TrackInfoChanged;
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            textBox1.Text = "";
            textBox5.Text = "";

            // play default audio track
            iTarget.Box.Playback.Start();
        }

        private void button2_Click(object sender, EventArgs e) {
            textBox1.Text = "";
            textBox5.Text = "";

            iTarget.Box.Playback.Stop();
        }

        private void button3_Click(object sender, EventArgs e) {
            iTarget.Box.Playback.VolumeInc();
        }

        private void button4_Click(object sender, EventArgs e) {
            iTarget.Box.Playback.VolumeDec();
        }

        private void button5_Click(object sender, EventArgs e) {
            textBox1.Text = "";
            textBox5.Text = "";

            // play user selected file
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = "Select Track to Play";
            openFile.Filter = "audio files (*.flac, *.mp*, *.wav*, *.aif*, *.m4a, *.wma, *.ogg)|*.flac;*.mp*;*.wav*;*.aif*;*.m4a;*.wma;*.ogg";
            openFile.ShowDialog();
            if (File.Exists(openFile.FileName)) {
                iTarget.Box.Playback.Start(openFile.FileName);
            }
        }

        private void PlaybackInfo(object obj, EventArgsPlaybackInfo e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                textBox5.Text = e.PlaybackInfo;
            });
        }

        private void PlaybackError(object obj, EventArgsPlaybackError e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                textBox1.Text = e.ErrorMessage;
            });
        }

        private void TransportStateChanged(object obj, EventArgsTransportState e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                if (iTarget.Box.Playback.Standby) {
                    textBox2.Text = "Device in standby";
                }
                else {
                    textBox2.Text = e.TransportState;
                }
            });
        }

        private void VolumeChanged(object obj, EventArgsVolume e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                if (iTarget.Box.Playback.Standby) {
                    textBox3.Text = "Device in standby";
                }
                else {
                    textBox3.Text = e.Volume;
                }
            });
        }

        private void StandbyChanged(object obj, EventArgsStandby e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                if (e.Standby) {
                    textBox2.Text = "Device in standby";
                    textBox3.Text = "Device in standby";
                }
                else {
                    textBox2.Text = iTarget.Box.Playback.TransportState;
                    textBox3.Text = iTarget.Box.Playback.Volume.ToString();
                }
            });
        }

        private void TrackInfoChanged(object obj, EventArgsTrackInfo e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                textBox4.Text = "Title: " + e.Title + Environment.NewLine + "Artist: " + e.Artist + Environment.NewLine + "Album: " + e.Album + Environment.NewLine;
                try {
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.LoadAsync(e.AlbumArtUri);
                }
                catch (Exception) {
                    pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox1.Image = Linn.Kinsky.Properties.Resources.NoAlbumArt;
                }
            });
        }

        private Target iTarget;
        private bool iEnabled = false;
    }
}
