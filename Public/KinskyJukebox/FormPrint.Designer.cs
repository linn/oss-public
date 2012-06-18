namespace KinskyJukebox
{
    partial class FormPrint
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
            this.printSectionCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.buttonPrintOK = new System.Windows.Forms.Button();
            this.buttonPrintCancel = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.buttonSelectNone = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.iPageLayoutAlbumArtOnlyLandscape = new System.Windows.Forms.RadioButton();
            this.iPageLayoutTrackDetailsLandscape = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.iPageLayoutAlbumArtOnlyPortrait = new System.Windows.Forms.RadioButton();
            this.iPageLayoutTrackDetailsPortrait = new System.Windows.Forms.RadioButton();
            this.iPagesPerSheet = new System.Windows.Forms.ComboBox();
            this.iPagesPerSheetLabel = new System.Windows.Forms.Label();
            this.iPrintOrderBooklet = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.iDocTypeRtf = new System.Windows.Forms.RadioButton();
            this.iDocTypePdf = new System.Windows.Forms.RadioButton();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // printSectionCheckedListBox
            // 
            this.printSectionCheckedListBox.CheckOnClick = true;
            this.printSectionCheckedListBox.FormattingEnabled = true;
            this.printSectionCheckedListBox.HorizontalScrollbar = true;
            this.printSectionCheckedListBox.Location = new System.Drawing.Point(3, 16);
            this.printSectionCheckedListBox.Name = "printSectionCheckedListBox";
            this.printSectionCheckedListBox.Size = new System.Drawing.Size(156, 199);
            this.printSectionCheckedListBox.TabIndex = 4;
            // 
            // buttonPrintOK
            // 
            this.buttonPrintOK.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonPrintOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonPrintOK.Location = new System.Drawing.Point(249, 290);
            this.buttonPrintOK.Name = "buttonPrintOK";
            this.buttonPrintOK.Size = new System.Drawing.Size(75, 29);
            this.buttonPrintOK.TabIndex = 11;
            this.buttonPrintOK.Text = "OK";
            this.buttonPrintOK.UseVisualStyleBackColor = true;
            this.buttonPrintOK.Click += new System.EventHandler(this.buttonPrintOK_Click);
            // 
            // buttonPrintCancel
            // 
            this.buttonPrintCancel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonPrintCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonPrintCancel.Location = new System.Drawing.Point(330, 290);
            this.buttonPrintCancel.Name = "buttonPrintCancel";
            this.buttonPrintCancel.Size = new System.Drawing.Size(75, 29);
            this.buttonPrintCancel.TabIndex = 12;
            this.buttonPrintCancel.Text = "Cancel";
            this.buttonPrintCancel.UseVisualStyleBackColor = true;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "pdf";
            this.saveFileDialog.FileName = "Presets";
            this.saveFileDialog.Filter = "Pdf File (*.pdf)|*.pdf|Rich Text File (*.rtf)|*.rtf";
            this.saveFileDialog.Title = "Print Catalog to a File (change the type to RTF if you want to edit the file)";
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonSelectAll.Location = new System.Drawing.Point(3, 219);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectAll.TabIndex = 5;
            this.buttonSelectAll.Text = "Select All";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // buttonSelectNone
            // 
            this.buttonSelectNone.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonSelectNone.Location = new System.Drawing.Point(84, 219);
            this.buttonSelectNone.Name = "buttonSelectNone";
            this.buttonSelectNone.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectNone.TabIndex = 6;
            this.buttonSelectNone.Text = "Select None";
            this.buttonSelectNone.UseVisualStyleBackColor = true;
            this.buttonSelectNone.Click += new System.EventHandler(this.buttonSelectNone_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Sections";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.iPageLayoutAlbumArtOnlyLandscape);
            this.panel1.Controls.Add(this.iPageLayoutTrackDetailsLandscape);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.iPageLayoutAlbumArtOnlyPortrait);
            this.panel1.Controls.Add(this.iPageLayoutTrackDetailsPortrait);
            this.panel1.Location = new System.Drawing.Point(15, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(224, 217);
            this.panel1.TabIndex = 7;
            // 
            // iPageLayoutAlbumArtOnlyLandscape
            // 
            this.iPageLayoutAlbumArtOnlyLandscape.AutoSize = true;
            this.iPageLayoutAlbumArtOnlyLandscape.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.iPageLayoutAlbumArtOnlyLandscape.Image = global::KinskyJukebox.Properties.Resources.AlbumArtLandscape;
            this.iPageLayoutAlbumArtOnlyLandscape.Location = new System.Drawing.Point(94, 119);
            this.iPageLayoutAlbumArtOnlyLandscape.Name = "iPageLayoutAlbumArtOnlyLandscape";
            this.iPageLayoutAlbumArtOnlyLandscape.Size = new System.Drawing.Size(125, 97);
            this.iPageLayoutAlbumArtOnlyLandscape.TabIndex = 3;
            this.iPageLayoutAlbumArtOnlyLandscape.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.iPageLayoutAlbumArtOnlyLandscape.UseVisualStyleBackColor = true;
            // 
            // iPageLayoutTrackDetailsLandscape
            // 
            this.iPageLayoutTrackDetailsLandscape.AutoSize = true;
            this.iPageLayoutTrackDetailsLandscape.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.iPageLayoutTrackDetailsLandscape.Image = global::KinskyJukebox.Properties.Resources.TrackLandscape;
            this.iPageLayoutTrackDetailsLandscape.Location = new System.Drawing.Point(94, 16);
            this.iPageLayoutTrackDetailsLandscape.Name = "iPageLayoutTrackDetailsLandscape";
            this.iPageLayoutTrackDetailsLandscape.Size = new System.Drawing.Size(125, 97);
            this.iPageLayoutTrackDetailsLandscape.TabIndex = 1;
            this.iPageLayoutTrackDetailsLandscape.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.iPageLayoutTrackDetailsLandscape.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Page Layout";
            // 
            // iPageLayoutAlbumArtOnlyPortrait
            // 
            this.iPageLayoutAlbumArtOnlyPortrait.AutoSize = true;
            this.iPageLayoutAlbumArtOnlyPortrait.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.iPageLayoutAlbumArtOnlyPortrait.Image = global::KinskyJukebox.Properties.Resources.AlbumArtPortrait;
            this.iPageLayoutAlbumArtOnlyPortrait.Location = new System.Drawing.Point(3, 119);
            this.iPageLayoutAlbumArtOnlyPortrait.Name = "iPageLayoutAlbumArtOnlyPortrait";
            this.iPageLayoutAlbumArtOnlyPortrait.Size = new System.Drawing.Size(125, 97);
            this.iPageLayoutAlbumArtOnlyPortrait.TabIndex = 2;
            this.iPageLayoutAlbumArtOnlyPortrait.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.iPageLayoutAlbumArtOnlyPortrait.UseVisualStyleBackColor = true;
            // 
            // iPageLayoutTrackDetailsPortrait
            // 
            this.iPageLayoutTrackDetailsPortrait.AutoSize = true;
            this.iPageLayoutTrackDetailsPortrait.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.iPageLayoutTrackDetailsPortrait.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.iPageLayoutTrackDetailsPortrait.Checked = true;
            this.iPageLayoutTrackDetailsPortrait.Image = global::KinskyJukebox.Properties.Resources.TrackPortrait;
            this.iPageLayoutTrackDetailsPortrait.Location = new System.Drawing.Point(3, 16);
            this.iPageLayoutTrackDetailsPortrait.Name = "iPageLayoutTrackDetailsPortrait";
            this.iPageLayoutTrackDetailsPortrait.Size = new System.Drawing.Size(125, 97);
            this.iPageLayoutTrackDetailsPortrait.TabIndex = 0;
            this.iPageLayoutTrackDetailsPortrait.TabStop = true;
            this.iPageLayoutTrackDetailsPortrait.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.iPageLayoutTrackDetailsPortrait.UseVisualStyleBackColor = true;
            // 
            // iPagesPerSheet
            // 
            this.iPagesPerSheet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.iPagesPerSheet.FormattingEnabled = true;
            this.iPagesPerSheet.Items.AddRange(new object[] {
            "1",
            "2",
            "4",
            "8",
            "9",
            "16",
            "25",
            "32",
            "36"});
            this.iPagesPerSheet.Location = new System.Drawing.Point(3, 3);
            this.iPagesPerSheet.MaxDropDownItems = 10;
            this.iPagesPerSheet.Name = "iPagesPerSheet";
            this.iPagesPerSheet.Size = new System.Drawing.Size(53, 21);
            this.iPagesPerSheet.TabIndex = 10;
            this.iPagesPerSheet.SelectedIndexChanged += new System.EventHandler(this.iPagesPerSheet_SelectedIndexChanged);
            // 
            // iPagesPerSheetLabel
            // 
            this.iPagesPerSheetLabel.AutoSize = true;
            this.iPagesPerSheetLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.iPagesPerSheetLabel.Location = new System.Drawing.Point(59, 6);
            this.iPagesPerSheetLabel.Name = "iPagesPerSheetLabel";
            this.iPagesPerSheetLabel.Size = new System.Drawing.Size(162, 13);
            this.iPagesPerSheetLabel.TabIndex = 10;
            this.iPagesPerSheetLabel.Text = "Pages per sheet (PDF only)";
            // 
            // iPrintOrderBooklet
            // 
            this.iPrintOrderBooklet.AutoSize = true;
            this.iPrintOrderBooklet.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.iPrintOrderBooklet.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.iPrintOrderBooklet.Location = new System.Drawing.Point(4, 30);
            this.iPrintOrderBooklet.Name = "iPrintOrderBooklet";
            this.iPrintOrderBooklet.Size = new System.Drawing.Size(220, 17);
            this.iPrintOrderBooklet.TabIndex = 9;
            this.iPrintOrderBooklet.Text = "Print as folding booklet (PDF only)";
            this.iPrintOrderBooklet.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.iDocTypeRtf);
            this.panel2.Controls.Add(this.iDocTypePdf);
            this.panel2.Location = new System.Drawing.Point(15, 235);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(224, 24);
            this.panel2.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(3, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Document Type";
            // 
            // iDocTypeRtf
            // 
            this.iDocTypeRtf.AutoSize = true;
            this.iDocTypeRtf.Location = new System.Drawing.Point(173, 2);
            this.iDocTypeRtf.Name = "iDocTypeRtf";
            this.iDocTypeRtf.Size = new System.Drawing.Size(46, 17);
            this.iDocTypeRtf.TabIndex = 8;
            this.iDocTypeRtf.Text = "RTF";
            this.iDocTypeRtf.UseVisualStyleBackColor = true;
            this.iDocTypeRtf.CheckedChanged += new System.EventHandler(this.iDocType_CheckedChanged);
            // 
            // iDocTypePdf
            // 
            this.iDocTypePdf.AutoSize = true;
            this.iDocTypePdf.Location = new System.Drawing.Point(121, 2);
            this.iDocTypePdf.Name = "iDocTypePdf";
            this.iDocTypePdf.Size = new System.Drawing.Size(46, 17);
            this.iDocTypePdf.TabIndex = 7;
            this.iDocTypePdf.TabStop = true;
            this.iDocTypePdf.Text = "PDF";
            this.iDocTypePdf.UseVisualStyleBackColor = true;
            this.iDocTypePdf.CheckedChanged += new System.EventHandler(this.iDocType_CheckedChanged);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.iPagesPerSheet);
            this.panel3.Controls.Add(this.iPagesPerSheetLabel);
            this.panel3.Controls.Add(this.iPrintOrderBooklet);
            this.panel3.Location = new System.Drawing.Point(15, 265);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(224, 53);
            this.panel3.TabIndex = 13;
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.printSectionCheckedListBox);
            this.panel4.Controls.Add(this.label1);
            this.panel4.Controls.Add(this.buttonSelectAll);
            this.panel4.Controls.Add(this.buttonSelectNone);
            this.panel4.Location = new System.Drawing.Point(245, 12);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(164, 247);
            this.panel4.TabIndex = 14;
            // 
            // FormPrint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(423, 331);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.buttonPrintCancel);
            this.Controls.Add(this.buttonPrintOK);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPrint";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Print Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormPrint_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox printSectionCheckedListBox;
        private System.Windows.Forms.Button buttonPrintOK;
        private System.Windows.Forms.Button buttonPrintCancel;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.Button buttonSelectNone;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton iPageLayoutAlbumArtOnlyPortrait;
        private System.Windows.Forms.RadioButton iPageLayoutTrackDetailsPortrait;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton iPageLayoutAlbumArtOnlyLandscape;
        private System.Windows.Forms.RadioButton iPageLayoutTrackDetailsLandscape;
        private System.Windows.Forms.ComboBox iPagesPerSheet;
        private System.Windows.Forms.Label iPagesPerSheetLabel;
        private System.Windows.Forms.CheckBox iPrintOrderBooklet;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton iDocTypeRtf;
        private System.Windows.Forms.RadioButton iDocTypePdf;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;

    }
}