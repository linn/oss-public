namespace LinnSetup
{
    partial class Diagnostic
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Diagnostic));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.diagnosticComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.diagnosticInputBox = new System.Windows.Forms.ToolStripTextBox();
            this.buttonRefresh = new System.Windows.Forms.ToolStripButton();
            this.textBoxDiagnosticResult = new System.Windows.Forms.TextBox();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.diagnosticComboBox,
            this.diagnosticInputBox,
            this.buttonRefresh});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(335, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // diagnosticComboBox
            // 
            this.diagnosticComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.diagnosticComboBox.Name = "diagnosticComboBox";
            this.diagnosticComboBox.Size = new System.Drawing.Size(750, 21);
            this.diagnosticComboBox.SelectedIndexChanged += new System.EventHandler(this.diagnosticComboBox_SelectedIndexChanged);
            // 
            // diagnosticInputBox
            // 
            this.diagnosticInputBox.Name = "diagnosticInputBox";
            this.diagnosticInputBox.Size = new System.Drawing.Size(100, 21);
            this.diagnosticInputBox.ToolTipText = "input";
            this.diagnosticInputBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.diagnosticInputBox_KeyPress);
            this.diagnosticInputBox.DoubleClick += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("buttonRefresh.Image")));
            this.buttonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(23, 20);
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // textBoxDiagnosticResult
            // 
            this.textBoxDiagnosticResult.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxDiagnosticResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDiagnosticResult.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxDiagnosticResult.Location = new System.Drawing.Point(0, 25);
            this.textBoxDiagnosticResult.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxDiagnosticResult.Multiline = true;
            this.textBoxDiagnosticResult.Name = "textBoxDiagnosticResult";
            this.textBoxDiagnosticResult.ReadOnly = true;
            this.textBoxDiagnosticResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDiagnosticResult.Size = new System.Drawing.Size(335, 152);
            this.textBoxDiagnosticResult.TabIndex = 1;
            // 
            // Diagnostic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxDiagnosticResult);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "Diagnostic";
            this.Size = new System.Drawing.Size(335, 177);
            this.Load += new System.EventHandler(this.Diagnostic_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripComboBox diagnosticComboBox;
        private System.Windows.Forms.TextBox textBoxDiagnosticResult;
        private System.Windows.Forms.ToolStripTextBox diagnosticInputBox;
        private System.Windows.Forms.ToolStripButton buttonRefresh;
    }
}
