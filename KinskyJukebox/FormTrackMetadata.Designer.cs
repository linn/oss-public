namespace KinskyJukebox
{
    partial class FormTrackMetadata
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.textBoxMetadata = new System.Windows.Forms.TextBox();
            this.pictureBoxAlbumArt = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAlbumArt)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxMetadata
            // 
            this.textBoxMetadata.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxMetadata.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxMetadata.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxMetadata.Location = new System.Drawing.Point(218, 12);
            this.textBoxMetadata.Multiline = true;
            this.textBoxMetadata.Name = "textBoxMetadata";
            this.textBoxMetadata.ReadOnly = true;
            this.textBoxMetadata.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxMetadata.Size = new System.Drawing.Size(337, 200);
            this.textBoxMetadata.TabIndex = 1;
            this.textBoxMetadata.WordWrap = false;
            // 
            // pictureBoxAlbumArt
            // 
            this.pictureBoxAlbumArt.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBoxAlbumArt.InitialImage = null;
            this.pictureBoxAlbumArt.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxAlbumArt.Name = "pictureBoxAlbumArt";
            this.pictureBoxAlbumArt.Size = new System.Drawing.Size(200, 200);
            this.pictureBoxAlbumArt.TabIndex = 2;
            this.pictureBoxAlbumArt.TabStop = false;
            // 
            // FormTrackMetadata
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(567, 225);
            this.Controls.Add(this.pictureBoxAlbumArt);
            this.Controls.Add(this.textBoxMetadata);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTrackMetadata";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Track Metadata";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormTrackMetadata_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormTrackMetadata_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAlbumArt)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxMetadata;
        private System.Windows.Forms.PictureBox pictureBoxAlbumArt;
    }
}