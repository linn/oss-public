namespace LinnSetup
{
    partial class FormTree
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
            this.textBoxTree = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxTree
            // 
            this.textBoxTree.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxTree.Cursor = System.Windows.Forms.Cursors.Default;
            this.textBoxTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTree.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTree.HideSelection = false;
            this.textBoxTree.Location = new System.Drawing.Point(0, 0);
            this.textBoxTree.Multiline = true;
            this.textBoxTree.Name = "textBoxTree";
            this.textBoxTree.ReadOnly = true;
            this.textBoxTree.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxTree.Size = new System.Drawing.Size(794, 468);
            this.textBoxTree.TabIndex = 0;
            this.textBoxTree.TabStop = false;
            // 
            // FormTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 468);
            this.Controls.Add(this.textBoxTree);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormTree";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tree";
            this.Load += new System.EventHandler(this.FormTree_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxTree;
    }
}