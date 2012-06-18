namespace LinnDiagnostics
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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.comboBoxRefresh = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.textBoxDiagnostic = new System.Windows.Forms.ToolStripTextBox();
            this.textBoxResult = new System.Windows.Forms.TextBox();
            this.tabControlGeneral = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.tabPageActions = new System.Windows.Forms.TabPage();
            this.textBoxActions = new System.Windows.Forms.TextBox();
            this.buttonActionsStart = new System.Windows.Forms.Button();
            this.tabPageEvents = new System.Windows.Forms.TabPage();
            this.tabPageThreads = new System.Windows.Forms.TabPage();
            this.comboBoxDevice = new System.Windows.Forms.ComboBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPageDevice = new System.Windows.Forms.TabPage();
            this.listBoxDevice = new System.Windows.Forms.ListBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.tabControlGeneral.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.tabPageActions.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPageDevice.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.CanOverflow = false;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel3,
            this.comboBoxRefresh,
            this.toolStripLabel2,
            this.textBoxDiagnostic});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(3, 3);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(658, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(45, 22);
            this.toolStripLabel3.Text = "Refresh";
            // 
            // comboBoxRefresh
            // 
            this.comboBoxRefresh.Items.AddRange(new object[] {
            "None",
            "1 second",
            "3 seconds",
            "5 seconds",
            "10 seconds",
            "30 seconds",
            "1 minute",
            "10 minutes"});
            this.comboBoxRefresh.Name = "comboBoxRefresh";
            this.comboBoxRefresh.Size = new System.Drawing.Size(121, 25);
            this.comboBoxRefresh.SelectedIndexChanged += new System.EventHandler(this.comboBoxRefreshSelectedIndexChanged);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(56, 22);
            this.toolStripLabel2.Text = "Diagnostic";
            // 
            // textBoxDiagnostic
            // 
            this.textBoxDiagnostic.Name = "textBoxDiagnostic";
            this.textBoxDiagnostic.Size = new System.Drawing.Size(300, 25);
            this.textBoxDiagnostic.TextChanged += new System.EventHandler(this.textBoxDiagnosticTextChanged);
            // 
            // textBoxResult
            // 
            this.textBoxResult.AcceptsReturn = true;
            this.textBoxResult.AcceptsTab = true;
            this.textBoxResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxResult.BackColor = System.Drawing.SystemColors.Info;
            this.textBoxResult.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxResult.Location = new System.Drawing.Point(0, 31);
            this.textBoxResult.Multiline = true;
            this.textBoxResult.Name = "textBoxResult";
            this.textBoxResult.ReadOnly = true;
            this.textBoxResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxResult.Size = new System.Drawing.Size(664, 441);
            this.textBoxResult.TabIndex = 1;
            this.textBoxResult.WordWrap = false;
            // 
            // tabControlGeneral
            // 
            this.tabControlGeneral.Controls.Add(this.tabPageGeneral);
            this.tabControlGeneral.Controls.Add(this.tabPageActions);
            this.tabControlGeneral.Controls.Add(this.tabPageEvents);
            this.tabControlGeneral.Controls.Add(this.tabPageThreads);
            this.tabControlGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlGeneral.Location = new System.Drawing.Point(0, 0);
            this.tabControlGeneral.Name = "tabControlGeneral";
            this.tabControlGeneral.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tabControlGeneral.SelectedIndex = 0;
            this.tabControlGeneral.Size = new System.Drawing.Size(672, 498);
            this.tabControlGeneral.TabIndex = 2;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add(this.toolStrip1);
            this.tabPageGeneral.Controls.Add(this.textBoxResult);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(664, 472);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // tabPageActions
            // 
            this.tabPageActions.Controls.Add(this.textBoxActions);
            this.tabPageActions.Controls.Add(this.buttonActionsStart);
            this.tabPageActions.Location = new System.Drawing.Point(4, 22);
            this.tabPageActions.Name = "tabPageActions";
            this.tabPageActions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageActions.Size = new System.Drawing.Size(664, 472);
            this.tabPageActions.TabIndex = 1;
            this.tabPageActions.Text = "Actions";
            this.tabPageActions.UseVisualStyleBackColor = true;
            // 
            // textBoxActions
            // 
            this.textBoxActions.BackColor = System.Drawing.SystemColors.Info;
            this.textBoxActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxActions.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxActions.Location = new System.Drawing.Point(3, 81);
            this.textBoxActions.Multiline = true;
            this.textBoxActions.Name = "textBoxActions";
            this.textBoxActions.Size = new System.Drawing.Size(658, 388);
            this.textBoxActions.TabIndex = 3;
            // 
            // buttonActionsStart
            // 
            this.buttonActionsStart.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonActionsStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonActionsStart.Location = new System.Drawing.Point(3, 3);
            this.buttonActionsStart.Name = "buttonActionsStart";
            this.buttonActionsStart.Size = new System.Drawing.Size(658, 78);
            this.buttonActionsStart.TabIndex = 2;
            this.buttonActionsStart.Text = "Start";
            this.buttonActionsStart.UseVisualStyleBackColor = true;
            this.buttonActionsStart.Click += new System.EventHandler(this.buttonActionsStartClick);
            // 
            // tabPageEvents
            // 
            this.tabPageEvents.Location = new System.Drawing.Point(4, 22);
            this.tabPageEvents.Name = "tabPageEvents";
            this.tabPageEvents.Size = new System.Drawing.Size(664, 472);
            this.tabPageEvents.TabIndex = 2;
            this.tabPageEvents.Text = "Events";
            this.tabPageEvents.UseVisualStyleBackColor = true;
            // 
            // tabPageThreads
            // 
            this.tabPageThreads.Location = new System.Drawing.Point(4, 22);
            this.tabPageThreads.Name = "tabPageThreads";
            this.tabPageThreads.Size = new System.Drawing.Size(664, 472);
            this.tabPageThreads.TabIndex = 3;
            this.tabPageThreads.Text = "Threads";
            this.tabPageThreads.UseVisualStyleBackColor = true;
            // 
            // comboBoxDevice
            // 
            this.comboBoxDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDevice.FormattingEnabled = true;
            this.comboBoxDevice.Location = new System.Drawing.Point(0, 502);
            this.comboBoxDevice.Margin = new System.Windows.Forms.Padding(0);
            this.comboBoxDevice.Name = "comboBoxDevice";
            this.comboBoxDevice.Size = new System.Drawing.Size(891, 21);
            this.comboBoxDevice.TabIndex = 3;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControlGeneral);
            this.splitContainer1.Size = new System.Drawing.Size(891, 498);
            this.splitContainer1.SplitterDistance = 215;
            this.splitContainer1.TabIndex = 4;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPageDevice);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(215, 498);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPageDevice
            // 
            this.tabPageDevice.Controls.Add(this.listBoxDevice);
            this.tabPageDevice.Location = new System.Drawing.Point(4, 22);
            this.tabPageDevice.Name = "tabPageDevice";
            this.tabPageDevice.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDevice.Size = new System.Drawing.Size(207, 472);
            this.tabPageDevice.TabIndex = 0;
            this.tabPageDevice.Text = "Device";
            this.tabPageDevice.UseVisualStyleBackColor = true;
            // 
            // listBoxDevice
            // 
            this.listBoxDevice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxDevice.FormattingEnabled = true;
            this.listBoxDevice.Location = new System.Drawing.Point(3, 3);
            this.listBoxDevice.Name = "listBoxDevice";
            this.listBoxDevice.Size = new System.Drawing.Size(201, 459);
            this.listBoxDevice.TabIndex = 0;
            this.listBoxDevice.SelectedIndexChanged += new System.EventHandler(this.listBoxDeviceSelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(891, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.optionsToolStripMenuItem.Text = "&Options...";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(891, 522);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.comboBoxDevice);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Form1";
            this.Text = "LinnDiagnostics";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EventFormClosed);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControlGeneral.ResumeLayout(false);
            this.tabPageGeneral.ResumeLayout(false);
            this.tabPageGeneral.PerformLayout();
            this.tabPageActions.ResumeLayout(false);
            this.tabPageActions.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPageDevice.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripTextBox textBoxDiagnostic;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripComboBox comboBoxRefresh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.TextBox textBoxResult;
        private System.Windows.Forms.TabControl tabControlGeneral;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.TabPage tabPageActions;
        private System.Windows.Forms.TabPage tabPageEvents;
        private System.Windows.Forms.TabPage tabPageThreads;
        private System.Windows.Forms.ComboBox comboBoxDevice;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox listBoxDevice;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPageDevice;
        private System.Windows.Forms.TextBox textBoxActions;
        private System.Windows.Forms.Button buttonActionsStart;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;


    }
}

