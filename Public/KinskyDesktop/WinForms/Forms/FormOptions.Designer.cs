namespace KinskyDesktop
{
    partial class FormUserOptions
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
            this.iButtonOk = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.TreeViewOptions = new System.Windows.Forms.TreeView();
            this.PanelOptionsPage = new System.Windows.Forms.Panel();
            this.iButtonReset = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonClose
            // 
            this.ButtonClose.Location = new System.Drawing.Point(575, 3);
            // 
            // ButtonMaximize
            // 
            this.ButtonMaximize.Location = new System.Drawing.Point(555, 3);
            // 
            // ButtonMinimize
            // 
            this.ButtonMinimize.Location = new System.Drawing.Point(555, 3);
            // 
            // ButtonRestore
            // 
            this.ButtonRestore.Location = new System.Drawing.Point(555, 3);
            // 
            // iButtonOk
            // 
            this.iButtonOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.iButtonOk.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.iButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.iButtonOk.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.iButtonOk.ForeColor = System.Drawing.SystemColors.ControlText;
            this.iButtonOk.Location = new System.Drawing.Point(503, 261);
            this.iButtonOk.Name = "iButtonOk";
            this.iButtonOk.Size = new System.Drawing.Size(75, 25);
            this.iButtonOk.TabIndex = 1;
            this.iButtonOk.Text = "OK";
            this.iButtonOk.UseCompatibleTextRendering = true;
            this.iButtonOk.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.80325F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 31.44016F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 39.75659F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.tableLayoutPanel1.Controls.Add(this.TreeViewOptions, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.iButtonOk, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.PanelOptionsPage, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.iButtonReset, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 23);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85.62092F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.37908F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(594, 302);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // TreeViewOptions
            // 
            this.TreeViewOptions.BackColor = System.Drawing.Color.Black;
            this.TreeViewOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeViewOptions.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TreeViewOptions.ForeColor = System.Drawing.Color.White;
            this.TreeViewOptions.FullRowSelect = true;
            this.TreeViewOptions.HideSelection = false;
            this.TreeViewOptions.Indent = 10;
            this.TreeViewOptions.LineColor = System.Drawing.Color.White;
            this.TreeViewOptions.Location = new System.Drawing.Point(3, 3);
            this.TreeViewOptions.Name = "TreeViewOptions";
            this.TreeViewOptions.ShowLines = false;
            this.TreeViewOptions.Size = new System.Drawing.Size(134, 252);
            this.TreeViewOptions.TabIndex = 3;
            this.TreeViewOptions.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewOptions_AfterSelect);
            // 
            // PanelOptionsPage
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.PanelOptionsPage, 3);
            this.PanelOptionsPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelOptionsPage.Location = new System.Drawing.Point(143, 3);
            this.PanelOptionsPage.Name = "PanelOptionsPage";
            this.PanelOptionsPage.Size = new System.Drawing.Size(448, 252);
            this.PanelOptionsPage.TabIndex = 8;
            // 
            // iButtonReset
            // 
            this.iButtonReset.BackColor = System.Drawing.SystemColors.Control;
            this.iButtonReset.ForeColor = System.Drawing.SystemColors.ControlText;
            this.iButtonReset.Location = new System.Drawing.Point(143, 261);
            this.iButtonReset.Name = "iButtonReset";
            this.iButtonReset.Size = new System.Drawing.Size(75, 25);
            this.iButtonReset.TabIndex = 9;
            this.iButtonReset.Text = "Reset Page";
            this.iButtonReset.UseVisualStyleBackColor = false;
            this.iButtonReset.Click += new System.EventHandler(this.ButtonResetClick);
            // 
            // FormUserOptions
            // 
            this.AcceptButton = this.iButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.ClientSize = new System.Drawing.Size(600, 330);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormUserOptions";
            this.ShowIcon = true;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "User Options";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EventFormClosed);
            this.Controls.SetChildIndex(this.ButtonRestore, 0);
            this.Controls.SetChildIndex(this.ButtonMaximize, 0);
            this.Controls.SetChildIndex(this.ButtonMinimize, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.Controls.SetChildIndex(this.ButtonClose, 0);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button iButtonOk;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel PanelOptionsPage;
        public System.Windows.Forms.TreeView TreeViewOptions;
        private System.Windows.Forms.Button iButtonReset;




    }
}   // KinskyDesktop
