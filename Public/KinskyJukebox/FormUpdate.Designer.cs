namespace KinskyJukebox
{
    partial class FormUpdate
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
            this.buttonSkip = new System.Windows.Forms.Button();
            this.buttonYes = new System.Windows.Forms.Button();
            this.updateMessageBox = new System.Windows.Forms.TextBox();
            this.buttonNo = new System.Windows.Forms.Button();
            this.updateProgressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // buttonSkip
            // 
            this.buttonSkip.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonSkip.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSkip.Location = new System.Drawing.Point(386, 231);
            this.buttonSkip.Name = "buttonSkip";
            this.buttonSkip.Size = new System.Drawing.Size(97, 45);
            this.buttonSkip.TabIndex = 3;
            this.buttonSkip.Text = "Skip";
            this.buttonSkip.UseVisualStyleBackColor = true;
            this.buttonSkip.Click += new System.EventHandler(this.buttonSkip_Click);
            // 
            // buttonYes
            // 
            this.buttonYes.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonYes.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonYes.Location = new System.Drawing.Point(12, 231);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.Size = new System.Drawing.Size(97, 45);
            this.buttonYes.TabIndex = 1;
            this.buttonYes.Text = "Yes";
            this.buttonYes.UseVisualStyleBackColor = true;
            this.buttonYes.Click += new System.EventHandler(this.buttonYes_Click);
            // 
            // updateMessageBox
            // 
            this.updateMessageBox.BackColor = System.Drawing.SystemColors.Window;
            this.updateMessageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updateMessageBox.Location = new System.Drawing.Point(12, 12);
            this.updateMessageBox.Multiline = true;
            this.updateMessageBox.Name = "updateMessageBox";
            this.updateMessageBox.ReadOnly = true;
            this.updateMessageBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.updateMessageBox.Size = new System.Drawing.Size(471, 213);
            this.updateMessageBox.TabIndex = 6;
            this.updateMessageBox.TabStop = false;
            // 
            // buttonNo
            // 
            this.buttonNo.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonNo.Location = new System.Drawing.Point(115, 231);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(97, 45);
            this.buttonNo.TabIndex = 2;
            this.buttonNo.Text = "No";
            this.buttonNo.UseVisualStyleBackColor = true;
            // 
            // updateProgressBar
            // 
            this.updateProgressBar.Location = new System.Drawing.Point(12, 231);
            this.updateProgressBar.Name = "updateProgressBar";
            this.updateProgressBar.Size = new System.Drawing.Size(368, 45);
            this.updateProgressBar.TabIndex = 8;
            this.updateProgressBar.Visible = false;
            // 
            // FormUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(495, 288);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.updateMessageBox);
            this.Controls.Add(this.buttonYes);
            this.Controls.Add(this.buttonSkip);
            this.Controls.Add(this.updateProgressBar);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormUpdate";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kinsky Jukebox Update";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSkip;
        private System.Windows.Forms.Button buttonYes;
        private System.Windows.Forms.TextBox updateMessageBox;
        private System.Windows.Forms.Button buttonNo;
        private System.Windows.Forms.ProgressBar updateProgressBar;

    }
}