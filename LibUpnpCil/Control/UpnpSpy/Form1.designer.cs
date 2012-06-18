namespace UpnpSpy
{
    partial class Form1
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
            this.listViewSsdp = new System.Windows.Forms.ListView();
            this.columnHeaderTime = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderKind = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderLocation = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDomain = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderType = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderVersion = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderAge = new System.Windows.Forms.ColumnHeader();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listBoxDevices = new System.Windows.Forms.ListBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageDetails = new System.Windows.Forms.TabPage();
            this.tabPageSsdp = new System.Windows.Forms.TabPage();
            this.iMainMenu = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageSsdp.SuspendLayout();
            this.iMainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewSsdp
            // 
            this.listViewSsdp.AllowColumnReorder = true;
            this.listViewSsdp.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderTime,
            this.columnHeaderKind,
            this.columnHeaderLocation,
            this.columnHeaderDomain,
            this.columnHeaderType,
            this.columnHeaderVersion,
            this.columnHeaderAge});
            this.listViewSsdp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSsdp.FullRowSelect = true;
            this.listViewSsdp.GridLines = true;
            this.listViewSsdp.Location = new System.Drawing.Point(3, 3);
            this.listViewSsdp.Name = "listViewSsdp";
            this.listViewSsdp.Size = new System.Drawing.Size(795, 444);
            this.listViewSsdp.TabIndex = 0;
            this.listViewSsdp.UseCompatibleStateImageBehavior = false;
            this.listViewSsdp.View = System.Windows.Forms.View.Details;
            this.listViewSsdp.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewSsdp_MouseDoubleClick);
            this.listViewSsdp.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewSsdp_ColumnClick);
            // 
            // columnHeaderTime
            // 
            this.columnHeaderTime.Tag = "Uuid";
            this.columnHeaderTime.Text = "Time";
            this.columnHeaderTime.Width = 123;
            // 
            // columnHeaderKind
            // 
            this.columnHeaderKind.Tag = "Kind";
            this.columnHeaderKind.Text = "Kind";
            this.columnHeaderKind.Width = 77;
            // 
            // columnHeaderLocation
            // 
            this.columnHeaderLocation.Tag = "Location";
            this.columnHeaderLocation.Text = "Location";
            this.columnHeaderLocation.Width = 257;
            // 
            // columnHeaderDomain
            // 
            this.columnHeaderDomain.Tag = "Domain";
            this.columnHeaderDomain.Text = "Domain";
            this.columnHeaderDomain.Width = 89;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Tag = "Type";
            this.columnHeaderType.Text = "Type";
            this.columnHeaderType.Width = 135;
            // 
            // columnHeaderVersion
            // 
            this.columnHeaderVersion.Tag = "Version";
            this.columnHeaderVersion.Text = "Version";
            this.columnHeaderVersion.Width = 49;
            // 
            // columnHeaderAge
            // 
            this.columnHeaderAge.Tag = "Age";
            this.columnHeaderAge.Text = "Age";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listBoxDevices);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(1054, 476);
            this.splitContainer1.SplitterDistance = 241;
            this.splitContainer1.TabIndex = 1;
            // 
            // listBoxDevices
            // 
            this.listBoxDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxDevices.FormattingEnabled = true;
            this.listBoxDevices.Location = new System.Drawing.Point(0, 0);
            this.listBoxDevices.Name = "listBoxDevices";
            this.listBoxDevices.Size = new System.Drawing.Size(241, 472);
            this.listBoxDevices.Sorted = true;
            this.listBoxDevices.TabIndex = 0;
            this.listBoxDevices.SelectedIndexChanged += new System.EventHandler(this.listBoxDevices_SelectedIndexChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageDetails);
            this.tabControl1.Controls.Add(this.tabPageSsdp);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(809, 476);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPageDetails
            // 
            this.tabPageDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.tabPageDetails.Location = new System.Drawing.Point(4, 22);
            this.tabPageDetails.Name = "tabPageDetails";
            this.tabPageDetails.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDetails.Size = new System.Drawing.Size(801, 450);
            this.tabPageDetails.TabIndex = 1;
            this.tabPageDetails.Text = "Details";
            this.tabPageDetails.UseVisualStyleBackColor = true;
            // 
            // tabPageSsdp
            // 
            this.tabPageSsdp.Controls.Add(this.listViewSsdp);
            this.tabPageSsdp.Location = new System.Drawing.Point(4, 22);
            this.tabPageSsdp.Name = "tabPageSsdp";
            this.tabPageSsdp.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSsdp.Size = new System.Drawing.Size(801, 450);
            this.tabPageSsdp.TabIndex = 0;
            this.tabPageSsdp.Text = "Ssdp";
            this.tabPageSsdp.UseVisualStyleBackColor = true;
            // 
            // iMainMenu
            // 
            this.iMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem});
            this.iMainMenu.Location = new System.Drawing.Point(0, 0);
            this.iMainMenu.Name = "iMainMenu";
            this.iMainMenu.Size = new System.Drawing.Size(1054, 24);
            this.iMainMenu.TabIndex = 2;
            this.iMainMenu.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.optionsToolStripMenuItem.Text = "Options...";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.MenuItemOptionsClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1054, 500);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.iMainMenu);
            this.MainMenuStrip = this.iMainMenu;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.EventFormLoad);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EventFormClosed);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageSsdp.ResumeLayout(false);
            this.iMainMenu.ResumeLayout(false);
            this.iMainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewSsdp;
        private System.Windows.Forms.ColumnHeader columnHeaderKind;
        private System.Windows.Forms.ColumnHeader columnHeaderTime;
        private System.Windows.Forms.ColumnHeader columnHeaderDomain;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderVersion;
        private System.Windows.Forms.ColumnHeader columnHeaderLocation;
        private System.Windows.Forms.ColumnHeader columnHeaderAge;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageSsdp;
        private System.Windows.Forms.TabPage tabPageDetails;
        private System.Windows.Forms.ListBox listBoxDevices;
        private System.Windows.Forms.MenuStrip iMainMenu;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
    }
}

