﻿namespace LinnSetup
{
    partial class Config
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Config));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.webBrowser2 = new System.Windows.Forms.WebBrowser();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.buttonToggle = new System.Windows.Forms.ToolStripButton();
            this.uriLabel = new System.Windows.Forms.ToolStripLabel();
            this.buttonRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 0;
            // 
            // webBrowser2
            // 
            this.webBrowser2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser2.Location = new System.Drawing.Point(0, 25);
            this.webBrowser2.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser2.Name = "webBrowser2";
            this.webBrowser2.Size = new System.Drawing.Size(150, 125);
            this.webBrowser2.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonToggle,
            this.uriLabel,
            this.buttonRefresh});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(150, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // buttonToggle
            // 
            this.buttonToggle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonToggle.Image = ((System.Drawing.Image)(resources.GetObject("buttonToggle.Image")));
            this.buttonToggle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonToggle.Name = "buttonToggle";
            this.buttonToggle.Size = new System.Drawing.Size(23, 22);
            this.buttonToggle.Text = "Toggle";
            this.buttonToggle.Click += new System.EventHandler(this.buttonToggle_Click);
            // 
            // uriLabel
            // 
            this.uriLabel.Name = "uriLabel";
            this.uriLabel.Size = new System.Drawing.Size(0, 22);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("buttonRefresh.Image")));
            this.buttonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(23, 22);
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // Config
            // 
            this.Controls.Add(this.webBrowser2);
            this.Controls.Add(this.toolStrip1);
            this.Name = "Config";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.WebBrowser webBrowser2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel uriLabel;
        private System.Windows.Forms.ToolStripButton buttonToggle;
        private System.Windows.Forms.ToolStripButton buttonRefresh;

    }
}
