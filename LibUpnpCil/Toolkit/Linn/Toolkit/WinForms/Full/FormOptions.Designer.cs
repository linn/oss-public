namespace Linn.Toolkit.WinForms
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
            this.HelpBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // iButtonOk
            // 
            this.iButtonOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.iButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.iButtonOk.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.iButtonOk.Location = new System.Drawing.Point(516, 264);
            this.iButtonOk.Name = "iButtonOk";
            this.iButtonOk.Size = new System.Drawing.Size(75, 25);
            this.iButtonOk.TabIndex = 1;
            this.iButtonOk.Text = "OK";
            this.iButtonOk.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.84314F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72.15686F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tableLayoutPanel1.Controls.Add(this.TreeViewOptions, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.PanelOptionsPage, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.iButtonReset, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.iButtonOk, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.HelpBox, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85.9911F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.00891F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(600, 304);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // TreeViewOptions
            // 
            this.TreeViewOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TreeViewOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeViewOptions.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TreeViewOptions.FullRowSelect = true;
            this.TreeViewOptions.HideSelection = false;
            this.TreeViewOptions.Indent = 10;
            this.TreeViewOptions.Location = new System.Drawing.Point(3, 3);
            this.TreeViewOptions.Name = "TreeViewOptions";
            this.TreeViewOptions.ShowLines = false;
            this.TreeViewOptions.Size = new System.Drawing.Size(135, 255);
            this.TreeViewOptions.TabIndex = 3;
            this.TreeViewOptions.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewOptions_AfterSelect);
            // 
            // PanelOptionsPage
            // 
            this.PanelOptionsPage.BackColor = System.Drawing.SystemColors.Window;
            this.PanelOptionsPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel1.SetColumnSpan(this.PanelOptionsPage, 2);
            this.PanelOptionsPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelOptionsPage.Location = new System.Drawing.Point(144, 3);
            this.PanelOptionsPage.Name = "PanelOptionsPage";
            this.PanelOptionsPage.Size = new System.Drawing.Size(453, 255);
            this.PanelOptionsPage.TabIndex = 8;
            // 
            // iButtonReset
            // 
            this.iButtonReset.Location = new System.Drawing.Point(3, 264);
            this.iButtonReset.Name = "iButtonReset";
            this.iButtonReset.Size = new System.Drawing.Size(135, 25);
            this.iButtonReset.TabIndex = 9;
            this.iButtonReset.Text = "Restore Page Defaults";
            this.iButtonReset.UseVisualStyleBackColor = true;
            this.iButtonReset.Click += new System.EventHandler(this.ButtonResetClick);
            // 
            // HelpBox
            // 
            this.HelpBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.HelpBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.HelpBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.HelpBox.Location = new System.Drawing.Point(144, 264);
            this.HelpBox.Multiline = true;
            this.HelpBox.Name = "HelpBox";
            this.HelpBox.ReadOnly = true;
            this.HelpBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.HelpBox.Size = new System.Drawing.Size(360, 37);
            this.HelpBox.TabIndex = 10;
            this.HelpBox.TabStop = false;
            // 
            // FormUserOptions
            // 
            this.AcceptButton = this.iButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(600, 304);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormUserOptions";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "User Options";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EventFormClosed);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button iButtonOk;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel PanelOptionsPage;
        public System.Windows.Forms.TreeView TreeViewOptions;
        private System.Windows.Forms.Button iButtonReset;
        private System.Windows.Forms.TextBox HelpBox;
    }
}
