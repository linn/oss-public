using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SneakyLastFm
{
    public partial class FormCoverArt : Form
    {
        public FormCoverArt()
        {
            InitializeComponent();
        }

        public void SetCoverArt(string aUrl)
        {
            try
            {
                iCoverArtPictureBox.Load(aUrl);
            }
            catch (System.Net.WebException)
            {
                iCoverArtPictureBox.Image = null;
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            Hide();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Hide();
        }
    }
}
