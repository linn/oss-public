namespace LinnTopology
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
            this.components = new System.ComponentModel.Container();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.iMenuItemSettings = new System.Windows.Forms.MenuItem();
            this.MenuItemOptions = new System.Windows.Forms.MenuItem();
            this.MenuItemDebug = new System.Windows.Forms.MenuItem();
            this.iMenuItemHelp = new System.Windows.Forms.MenuItem();
            this.MenuItemAbout = new System.Windows.Forms.MenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageRoom = new System.Windows.Forms.TabPage();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.ListViewSource = new System.Windows.Forms.ListView();
            this.ListViewRoom = new System.Windows.Forms.ListView();
            this.tabPageLibrary = new System.Windows.Forms.TabPage();
            this.ListViewLibrary = new System.Windows.Forms.ListView();
            this.tabPageReprog = new System.Windows.Forms.TabPage();
            this.listViewReprog = new System.Windows.Forms.ListView();
            this.tabControl1.SuspendLayout();
            this.tabPageRoom.SuspendLayout();
            this.tabPageLibrary.SuspendLayout();
            this.tabPageReprog.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.iMenuItemSettings,
            this.iMenuItemHelp});
            // 
            // iMenuItemSettings
            // 
            this.iMenuItemSettings.Index = 0;
            this.iMenuItemSettings.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuItemOptions,
            this.MenuItemDebug});
            this.iMenuItemSettings.Text = "Tools";
            // 
            // MenuItemOptions
            // 
            this.MenuItemOptions.Index = 0;
            this.MenuItemOptions.Text = "Options...";
            // 
            // MenuItemDebug
            // 
            this.MenuItemDebug.Index = 1;
            this.MenuItemDebug.Text = "User Console";
            // 
            // iMenuItemHelp
            // 
            this.iMenuItemHelp.Index = 1;
            this.iMenuItemHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuItemAbout});
            this.iMenuItemHelp.Text = "Help";
            // 
            // MenuItemAbout
            // 
            this.MenuItemAbout.Index = 0;
            this.MenuItemAbout.Text = "About";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageRoom);
            this.tabControl1.Controls.Add(this.tabPageLibrary);
            this.tabControl1.Controls.Add(this.tabPageReprog);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(240, 268);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageRoom
            // 
            this.tabPageRoom.Controls.Add(this.splitter1);
            this.tabPageRoom.Controls.Add(this.ListViewSource);
            this.tabPageRoom.Controls.Add(this.ListViewRoom);
            this.tabPageRoom.Location = new System.Drawing.Point(4, 22);
            this.tabPageRoom.Name = "tabPageRoom";
            this.tabPageRoom.Size = new System.Drawing.Size(232, 242);
            this.tabPageRoom.TabIndex = 0;
            this.tabPageRoom.Text = "Room List";
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(100, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 242);
            this.splitter1.TabIndex = 0;
            this.splitter1.TabStop = false;
            // 
            // ListViewSource
            // 
            this.ListViewSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListViewSource.Location = new System.Drawing.Point(100, 0);
            this.ListViewSource.Name = "ListViewSource";
            this.ListViewSource.Size = new System.Drawing.Size(132, 242);
            this.ListViewSource.TabIndex = 0;
            this.ListViewSource.UseCompatibleStateImageBehavior = false;
            this.ListViewSource.View = System.Windows.Forms.View.List;
            // 
            // ListViewRoom
            // 
            this.ListViewRoom.Dock = System.Windows.Forms.DockStyle.Left;
            this.ListViewRoom.Location = new System.Drawing.Point(0, 0);
            this.ListViewRoom.Name = "ListViewRoom";
            this.ListViewRoom.Size = new System.Drawing.Size(100, 242);
            this.ListViewRoom.TabIndex = 0;
            this.ListViewRoom.UseCompatibleStateImageBehavior = false;
            this.ListViewRoom.View = System.Windows.Forms.View.List;
            // 
            // tabPageLibrary
            // 
            this.tabPageLibrary.Controls.Add(this.ListViewLibrary);
            this.tabPageLibrary.Location = new System.Drawing.Point(4, 22);
            this.tabPageLibrary.Name = "tabPageLibrary";
            this.tabPageLibrary.Size = new System.Drawing.Size(232, 242);
            this.tabPageLibrary.TabIndex = 1;
            this.tabPageLibrary.Text = "Library List";
            // 
            // ListViewLibrary
            // 
            this.ListViewLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListViewLibrary.FullRowSelect = true;
            this.ListViewLibrary.Location = new System.Drawing.Point(0, 0);
            this.ListViewLibrary.Name = "ListViewLibrary";
            this.ListViewLibrary.Size = new System.Drawing.Size(232, 242);
            this.ListViewLibrary.TabIndex = 0;
            this.ListViewLibrary.UseCompatibleStateImageBehavior = false;
            this.ListViewLibrary.View = System.Windows.Forms.View.List;
            // 
            // tabPageReprog
            // 
            this.tabPageReprog.Controls.Add(this.listViewReprog);
            this.tabPageReprog.Location = new System.Drawing.Point(4, 22);
            this.tabPageReprog.Name = "tabPageReprog";
            this.tabPageReprog.Size = new System.Drawing.Size(232, 242);
            this.tabPageReprog.TabIndex = 2;
            this.tabPageReprog.Text = "Reprog List";
            // 
            // listViewReprog
            // 
            this.listViewReprog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewReprog.Location = new System.Drawing.Point(0, 0);
            this.listViewReprog.Name = "listViewReprog";
            this.listViewReprog.Size = new System.Drawing.Size(232, 242);
            this.listViewReprog.TabIndex = 0;
            this.listViewReprog.UseCompatibleStateImageBehavior = false;
            this.listViewReprog.View = System.Windows.Forms.View.List;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.tabControl1);
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "LinnTopology";
            this.tabControl1.ResumeLayout(false);
            this.tabPageRoom.ResumeLayout(false);
            this.tabPageLibrary.ResumeLayout(false);
            this.tabPageReprog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageLibrary;
        private System.Windows.Forms.TabPage tabPageReprog;
        private System.Windows.Forms.TabPage tabPageRoom;
        private System.Windows.Forms.ListView listViewReprog;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.MenuItem iMenuItemSettings;
        private System.Windows.Forms.MenuItem iMenuItemHelp;
        public System.Windows.Forms.ListView ListViewLibrary;
        public System.Windows.Forms.ListView ListViewRoom;
        public System.Windows.Forms.ListView ListViewSource;
        public System.Windows.Forms.MenuItem MenuItemOptions;
        public System.Windows.Forms.MenuItem MenuItemDebug;
        public System.Windows.Forms.MenuItem MenuItemAbout;
    }
}