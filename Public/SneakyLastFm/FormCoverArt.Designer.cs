namespace SneakyLastFm
{
    partial class FormCoverArt
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.iCoverArtPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.iCoverArtPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // iCoverArtPictureBox
            // 
            this.iCoverArtPictureBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.iCoverArtPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iCoverArtPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iCoverArtPictureBox.Location = new System.Drawing.Point(0, 0);
            this.iCoverArtPictureBox.Name = "iCoverArtPictureBox";
            this.iCoverArtPictureBox.Size = new System.Drawing.Size(200, 200);
            this.iCoverArtPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.iCoverArtPictureBox.TabIndex = 0;
            this.iCoverArtPictureBox.TabStop = false;
            this.iCoverArtPictureBox.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            this.iCoverArtPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            // 
            // FormCoverArt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(200, 200);
            this.Controls.Add(this.iCoverArtPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormCoverArt";
            this.ShowInTaskbar = false;
            this.Text = "Form2";
            ((System.ComponentModel.ISupportInitialize)(this.iCoverArtPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox iCoverArtPictureBox;
    }
}