namespace KinskyJukebox
{
    partial class FormDevices
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
            this.deviceCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonDone = new System.Windows.Forms.Button();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.buttonSelectNone = new System.Windows.Forms.Button();
            this.locationTextBox = new System.Windows.Forms.TextBox();
            this.buttonTest = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // deviceCheckedListBox
            // 
            this.deviceCheckedListBox.FormattingEnabled = true;
            this.deviceCheckedListBox.HorizontalScrollbar = true;
            this.deviceCheckedListBox.Location = new System.Drawing.Point(12, 36);
            this.deviceCheckedListBox.Name = "deviceCheckedListBox";
            this.deviceCheckedListBox.Size = new System.Drawing.Size(543, 199);
            this.deviceCheckedListBox.Sorted = true;
            this.deviceCheckedListBox.TabIndex = 0;
            this.deviceCheckedListBox.SelectedIndexChanged += new System.EventHandler(this.deviceCheckedListBox_SelectedIndexChanged);
            // 
            // buttonApply
            // 
            this.buttonApply.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonApply.Location = new System.Drawing.Point(399, 241);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 1;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // buttonDone
            // 
            this.buttonDone.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonDone.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonDone.Location = new System.Drawing.Point(480, 241);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(75, 23);
            this.buttonDone.TabIndex = 2;
            this.buttonDone.Text = "Cancel";
            this.buttonDone.UseVisualStyleBackColor = true;
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonSelectAll.Location = new System.Drawing.Point(12, 241);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectAll.TabIndex = 3;
            this.buttonSelectAll.Text = "Select All";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // buttonSelectNone
            // 
            this.buttonSelectNone.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonSelectNone.Location = new System.Drawing.Point(93, 241);
            this.buttonSelectNone.Name = "buttonSelectNone";
            this.buttonSelectNone.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectNone.TabIndex = 4;
            this.buttonSelectNone.Text = "Select None";
            this.buttonSelectNone.UseVisualStyleBackColor = true;
            this.buttonSelectNone.Click += new System.EventHandler(this.buttonSelectNone_Click);
            // 
            // locationTextBox
            // 
            this.locationTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.locationTextBox.Location = new System.Drawing.Point(12, 10);
            this.locationTextBox.Name = "locationTextBox";
            this.locationTextBox.ReadOnly = true;
            this.locationTextBox.Size = new System.Drawing.Size(543, 20);
            this.locationTextBox.TabIndex = 5;
            // 
            // buttonTest
            // 
            this.buttonTest.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonTest.Enabled = false;
            this.buttonTest.Location = new System.Drawing.Point(206, 241);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(161, 23);
            this.buttonTest.TabIndex = 6;
            this.buttonTest.Text = "Test (send preset to device)";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // FormDevices
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(567, 276);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.locationTextBox);
            this.Controls.Add(this.buttonSelectNone);
            this.Controls.Add(this.buttonSelectAll);
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.deviceCheckedListBox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormDevices";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sync with Linn DS";
            this.Load += new System.EventHandler(this.FormDevices_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormDevices_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox deviceCheckedListBox;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.Button buttonSelectNone;
        private System.Windows.Forms.TextBox locationTextBox;
        private System.Windows.Forms.Button buttonTest;

    }
}