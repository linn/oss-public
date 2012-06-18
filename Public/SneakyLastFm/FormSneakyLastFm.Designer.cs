using System.Windows.Forms;

namespace SneakyLastFm
{
    partial class FormSneakyLastFm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSneakyLastFm));
            this.iVolumePanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.iLogoPictureBox = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.iMetadataPanel = new System.Windows.Forms.Panel();
            this.iSearchTextBox = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.iRightSidePanel = new System.Windows.Forms.Panel();
            this.iCloseButton = new System.Windows.Forms.Button();
            this.iLeftSidePanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.iCoverArtPictureBox = new System.Windows.Forms.PictureBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.iRenderersPanel = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.iUsernameTextBox = new System.Windows.Forms.TextBox();
            this.iPasswordTextBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iLogoPictureBox)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.iRightSidePanel.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iCoverArtPictureBox)).BeginInit();
            this.panel5.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // iVolumePanel
            // 
            this.iVolumePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.iVolumePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iVolumePanel.Location = new System.Drawing.Point(23, 22);
            this.iVolumePanel.Margin = new System.Windows.Forms.Padding(0);
            this.iVolumePanel.Name = "iVolumePanel";
            this.iVolumePanel.Size = new System.Drawing.Size(104, 104);
            this.iVolumePanel.TabIndex = 17;
            this.iVolumePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.iVolumePanel_OnPaint);
            this.iVolumePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iVolumePanel_MouseDown);
            this.iVolumePanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iVolumePanel_MouseUp);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.iRightSidePanel, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.iLeftSidePanel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.iRenderersPanel, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel5, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.panel6, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 2F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 2F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(750, 200);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.tableLayoutPanel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.panel1.Controls.Add(this.iLogoPictureBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(30, 2);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(150, 45);
            this.panel1.TabIndex = 8;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // iLogoPictureBox
            // 
            this.iLogoPictureBox.Image = global::SneakyLastFm.Properties.Resources.lastfm_logo;
            this.iLogoPictureBox.Location = new System.Drawing.Point(0, 2);
            this.iLogoPictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.iLogoPictureBox.Name = "iLogoPictureBox";
            this.iLogoPictureBox.Size = new System.Drawing.Size(145, 42);
            this.iLogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.iLogoPictureBox.TabIndex = 0;
            this.iLogoPictureBox.TabStop = false;
            this.iLogoPictureBox.Click += new System.EventHandler(this.iLogoPictureBox_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.iMetadataPanel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.iSearchTextBox, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.panel2, 0, 4);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(180, 49);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 77F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 2F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(390, 150);
            this.tableLayoutPanel2.TabIndex = 9;
            this.tableLayoutPanel2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.tableLayoutPanel2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // iMetadataPanel
            // 
            this.iMetadataPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.iMetadataPanel.Location = new System.Drawing.Point(0, 47);
            this.iMetadataPanel.Margin = new System.Windows.Forms.Padding(0);
            this.iMetadataPanel.Name = "iMetadataPanel";
            this.iMetadataPanel.Size = new System.Drawing.Size(390, 77);
            this.iMetadataPanel.TabIndex = 0;
            this.iMetadataPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.iMetadataPanel_Paint);
            this.iMetadataPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iMetadataPanel_MouseDown);
            // 
            // iSearchTextBox
            // 
            this.iSearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.iSearchTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.iSearchTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.iSearchTextBox.Font = new System.Drawing.Font("Arial", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.iSearchTextBox.ForeColor = System.Drawing.Color.White;
            this.iSearchTextBox.Location = new System.Drawing.Point(0, 24);
            this.iSearchTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.iSearchTextBox.Name = "iSearchTextBox";
            this.iSearchTextBox.Size = new System.Drawing.Size(390, 23);
            this.iSearchTextBox.TabIndex = 2;
            this.iSearchTextBox.Text = "Type an artist to start listening";
            this.iSearchTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.iSearchTextBox.Leave += new System.EventHandler(this.iSearchTextBox_Leave);
            this.iSearchTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.iSearchTextBox_KeyPress);
            this.iSearchTextBox.Enter += new System.EventHandler(this.iSearchTextBox_Enter);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Black;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 148);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(390, 2);
            this.panel2.TabIndex = 3;
            this.panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.panel2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // iRightSidePanel
            // 
            this.iRightSidePanel.BackColor = System.Drawing.SystemColors.WindowText;
            this.iRightSidePanel.Controls.Add(this.iCloseButton);
            this.iRightSidePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iRightSidePanel.Location = new System.Drawing.Point(720, 2);
            this.iRightSidePanel.Margin = new System.Windows.Forms.Padding(0);
            this.iRightSidePanel.Name = "iRightSidePanel";
            this.tableLayoutPanel1.SetRowSpan(this.iRightSidePanel, 3);
            this.iRightSidePanel.Size = new System.Drawing.Size(30, 198);
            this.iRightSidePanel.TabIndex = 10;
            this.iRightSidePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.iRightSidePanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // iCloseButton
            // 
            this.iCloseButton.BackColor = System.Drawing.Color.Black;
            this.iCloseButton.FlatAppearance.BorderSize = 0;
            this.iCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.iCloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Black;
            this.iCloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.iCloseButton.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.iCloseButton.ForeColor = System.Drawing.Color.White;
            this.iCloseButton.Location = new System.Drawing.Point(4, 2);
            this.iCloseButton.Name = "iCloseButton";
            this.iCloseButton.Size = new System.Drawing.Size(23, 23);
            this.iCloseButton.TabIndex = 0;
            this.iCloseButton.TabStop = false;
            this.iCloseButton.Text = "X";
            this.iCloseButton.UseVisualStyleBackColor = false;
            this.iCloseButton.Click += new System.EventHandler(this.iCloseButton_Click);
            // 
            // iLeftSidePanel
            // 
            this.iLeftSidePanel.BackColor = System.Drawing.SystemColors.WindowText;
            this.iLeftSidePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iLeftSidePanel.Location = new System.Drawing.Point(0, 2);
            this.iLeftSidePanel.Margin = new System.Windows.Forms.Padding(0);
            this.iLeftSidePanel.Name = "iLeftSidePanel";
            this.tableLayoutPanel1.SetRowSpan(this.iLeftSidePanel, 3);
            this.iLeftSidePanel.Size = new System.Drawing.Size(30, 198);
            this.iLeftSidePanel.TabIndex = 11;
            this.iLeftSidePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.iLeftSidePanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.iCoverArtPictureBox, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.panel4, 0, 3);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(30, 49);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 2F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(150, 151);
            this.tableLayoutPanel3.TabIndex = 12;
            this.tableLayoutPanel3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.tableLayoutPanel3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // iCoverArtPictureBox
            // 
            this.iCoverArtPictureBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.iCoverArtPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iCoverArtPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iCoverArtPictureBox.Location = new System.Drawing.Point(25, 24);
            this.iCoverArtPictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.iCoverArtPictureBox.Name = "iCoverArtPictureBox";
            this.iCoverArtPictureBox.Size = new System.Drawing.Size(100, 100);
            this.iCoverArtPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.iCoverArtPictureBox.TabIndex = 5;
            this.iCoverArtPictureBox.TabStop = false;
            this.iCoverArtPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iCoverArtPictureBox_MouseDown);
            this.iCoverArtPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel3.SetColumnSpan(this.panel4, 3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 148);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(150, 3);
            this.panel4.TabIndex = 6;
            this.panel4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.panel4.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // iRenderersPanel
            // 
            this.iRenderersPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.iRenderersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iRenderersPanel.Location = new System.Drawing.Point(180, 2);
            this.iRenderersPanel.Margin = new System.Windows.Forms.Padding(0);
            this.iRenderersPanel.Name = "iRenderersPanel";
            this.iRenderersPanel.Size = new System.Drawing.Size(390, 45);
            this.iRenderersPanel.TabIndex = 18;
            this.iRenderersPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.iRenderersPanel_Paint);
            this.iRenderersPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iRenderersPanel_MouseDown);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.panel5.Controls.Add(this.iUsernameTextBox);
            this.panel5.Controls.Add(this.iPasswordTextBox);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(570, 2);
            this.panel5.Margin = new System.Windows.Forms.Padding(0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(150, 45);
            this.panel5.TabIndex = 19;
            this.panel5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.panel5.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            // 
            // iUsernameTextBox
            // 
            this.iUsernameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.iUsernameTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.iUsernameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.iUsernameTextBox.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.iUsernameTextBox.ForeColor = System.Drawing.Color.White;
            this.iUsernameTextBox.Location = new System.Drawing.Point(47, 10);
            this.iUsernameTextBox.Name = "iUsernameTextBox";
            this.iUsernameTextBox.Size = new System.Drawing.Size(100, 13);
            this.iUsernameTextBox.TabIndex = 3;
            this.iUsernameTextBox.Text = "Username";
            this.iUsernameTextBox.TextChanged += new System.EventHandler(this.iUsernameTextBox_TextChanged);
            this.iUsernameTextBox.Leave += new System.EventHandler(this.iUsernameTextBox_Leave);
            this.iUsernameTextBox.Enter += new System.EventHandler(this.iUsernameTextBox_Enter);
            // 
            // iPasswordTextBox
            // 
            this.iPasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.iPasswordTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.iPasswordTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.iPasswordTextBox.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.iPasswordTextBox.ForeColor = System.Drawing.Color.White;
            this.iPasswordTextBox.Location = new System.Drawing.Point(47, 27);
            this.iPasswordTextBox.Name = "iPasswordTextBox";
            this.iPasswordTextBox.Size = new System.Drawing.Size(100, 13);
            this.iPasswordTextBox.TabIndex = 4;
            this.iPasswordTextBox.Text = "Password";
            this.iPasswordTextBox.TextChanged += new System.EventHandler(this.iPasswordTextBox_TextChanged);
            this.iPasswordTextBox.Leave += new System.EventHandler(this.iPasswordTextBox_Leave);
            this.iPasswordTextBox.Enter += new System.EventHandler(this.iPasswordTextBox_Enter);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.iVolumePanel, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.panel3, 0, 3);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(570, 49);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 4;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 2F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(150, 151);
            this.tableLayoutPanel4.TabIndex = 20;
            this.tableLayoutPanel4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.tableLayoutPanel4.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel4.SetColumnSpan(this.panel3, 3);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 148);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(150, 3);
            this.panel3.TabIndex = 18;
            this.panel3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.panel3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            // 
            // panel6
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.panel6, 3);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(30, 0);
            this.panel6.Margin = new System.Windows.Forms.Padding(0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(690, 2);
            this.panel6.TabIndex = 21;
            // 
            // FormSneakyLastFm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(31)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(750, 200);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(7000, 200);
            this.MinimumSize = new System.Drawing.Size(750, 200);
            this.Name = "FormSneakyLastFm";
            this.Text = "SneakyLastFm";
            this.Deactivate += new System.EventHandler(this.iForm_Deactivate);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseUp);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.iForm_FormClosed);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iForm_MouseDown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iLogoPictureBox)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.iRightSidePanel.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.iCoverArtPictureBox)).EndInit();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox iCoverArtPictureBox;
        private System.Windows.Forms.Panel iVolumePanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox iLogoPictureBox;
        private System.Windows.Forms.TextBox iPasswordTextBox;
        private System.Windows.Forms.TextBox iUsernameTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel iMetadataPanel;
        private System.Windows.Forms.TextBox iSearchTextBox;
        private System.Windows.Forms.Panel iRightSidePanel;
        private System.Windows.Forms.Panel iLeftSidePanel;
        private TableLayoutPanel tableLayoutPanel3;
        private Panel iRenderersPanel;
        private Panel panel5;
        private Button iCloseButton;
        private TableLayoutPanel tableLayoutPanel4;
        private Panel panel2;
        private Panel panel3;
        private Panel panel4;
        private Panel panel6;

    }
}

