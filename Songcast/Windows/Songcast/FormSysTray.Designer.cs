namespace Linn.Songcast
{
    partial class FormSysTray
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
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.linnSongcasterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleLinnSongcasterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reconnectSelectedReceiversToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.openLinnSongcasterPreferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Text = "Linn Songcaster: X";
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.linnSongcasterToolStripMenuItem,
            this.toggleLinnSongcasterToolStripMenuItem,
            this.reconnectSelectedReceiversToolStripMenuItem,
            this.toolStripMenuItem1,
            this.openLinnSongcasterPreferencesToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(253, 148);
            // 
            // linnSongcasterToolStripMenuItem
            // 
            this.linnSongcasterToolStripMenuItem.Enabled = false;
            this.linnSongcasterToolStripMenuItem.Name = "linnSongcasterToolStripMenuItem";
            this.linnSongcasterToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.linnSongcasterToolStripMenuItem.Text = "Linn Songcaster: X";
            // 
            // toggleLinnSongcasterToolStripMenuItem
            // 
            this.toggleLinnSongcasterToolStripMenuItem.Name = "toggleLinnSongcasterToolStripMenuItem";
            this.toggleLinnSongcasterToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.toggleLinnSongcasterToolStripMenuItem.Text = "Turn Linn Songcaster X";
            this.toggleLinnSongcasterToolStripMenuItem.Click += new System.EventHandler(this.toggleLinnSongcasterToolStripMenuItem_Click);
            // 
            // reconnectSelectedReceiversToolStripMenuItem
            // 
            this.reconnectSelectedReceiversToolStripMenuItem.Name = "reconnectSelectedReceiversToolStripMenuItem";
            this.reconnectSelectedReceiversToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.reconnectSelectedReceiversToolStripMenuItem.Text = "Reconnect Selected Receivers";
            this.reconnectSelectedReceiversToolStripMenuItem.Click += new System.EventHandler(this.reconnectSelectedReceiversToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(249, 6);
            // 
            // openLinnSongcasterPreferencesToolStripMenuItem
            // 
            this.openLinnSongcasterPreferencesToolStripMenuItem.Name = "openLinnSongcasterPreferencesToolStripMenuItem";
            this.openLinnSongcasterPreferencesToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.openLinnSongcasterPreferencesToolStripMenuItem.Text = "Open Linn Songcaster Preferences...";
            this.openLinnSongcasterPreferencesToolStripMenuItem.Click += new System.EventHandler(this.openLinnSongcasterPreferencesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(249, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // FormSysTray
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Name = "FormSysTray";
            this.ShowInTaskbar = false;
            this.Text = "Form";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem linnSongcasterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleLinnSongcasterToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openLinnSongcasterPreferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reconnectSelectedReceiversToolStripMenuItem;
    }
}

