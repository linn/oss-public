using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace KinskyJukebox
{
    public partial class FormTrackMetadata : Form
    {
        public FormTrackMetadata(TrackMetadata aMetadata) {
            InitializeComponent();
            this.Icon = Icon.FromHandle(Properties.Resources.Track.GetHicon());
            textBoxMetadata.Text = aMetadata.ToString();
            iAlbumArtFileName = aMetadata.AlbumArtPath;
            Refresh();
            pictureBoxAlbumArt.Select();
        }

        private void FormTrackMetadata_Paint(object sender, PaintEventArgs e) {
            try {
                pictureBoxAlbumArt.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBoxAlbumArt.LoadAsync(iAlbumArtFileName);
            }
            catch (Exception) {
                pictureBoxAlbumArt.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBoxAlbumArt.Image = Linn.Kinsky.Properties.Resources.NoAlbumArt;
            }
        }

        private string iAlbumArtFileName = null;

        private void FormTrackMetadata_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) {
                this.Close();
            }
        }
    }
}
