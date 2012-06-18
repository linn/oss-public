namespace KinskyDesktop
{
    partial class FormSelector
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
            this.doubleBufferedTableLayoutPanel1 = new KinskyDesktop.Widgets.DoubleBufferedTableLayoutPanel();
            this.ListViewSelector = new KinskyDesktop.Widgets.ListViewKinsky();
            this.Title = new System.Windows.Forms.ColumnHeader();
            this.doubleBufferedTableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonClose
            // 
            this.ButtonClose.Location = new System.Drawing.Point(540, 3);
            this.ButtonClose.Size = new System.Drawing.Size(23, 17);
            this.ButtonClose.TabStop = false;
            // 
            // ButtonMaximize
            // 
            this.ButtonMaximize.Location = new System.Drawing.Point(520, 3);
            this.ButtonMaximize.TabStop = false;
            // 
            // ButtonMinimize
            // 
            this.ButtonMinimize.Location = new System.Drawing.Point(520, 3);
            this.ButtonMinimize.TabStop = false;
            // 
            // ButtonRestore
            // 
            this.ButtonRestore.Location = new System.Drawing.Point(520, 3);
            this.ButtonRestore.TabStop = false;
            // 
            // doubleBufferedTableLayoutPanel1
            // 
            this.doubleBufferedTableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.doubleBufferedTableLayoutPanel1.ColumnCount = 1;
            this.doubleBufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.doubleBufferedTableLayoutPanel1.Controls.Add(this.ListViewSelector, 0, 0);
            this.doubleBufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.doubleBufferedTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.doubleBufferedTableLayoutPanel1.Name = "doubleBufferedTableLayoutPanel1";
            this.doubleBufferedTableLayoutPanel1.Padding = new System.Windows.Forms.Padding(2, 20, 2, 2);
            this.doubleBufferedTableLayoutPanel1.RowCount = 1;
            this.doubleBufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.doubleBufferedTableLayoutPanel1.Size = new System.Drawing.Size(568, 342);
            this.doubleBufferedTableLayoutPanel1.TabIndex = 4;
            this.doubleBufferedTableLayoutPanel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tableLayoutPanel1_MouseMove);
            this.doubleBufferedTableLayoutPanel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tableLayoutPanel1_MouseDown);
            this.doubleBufferedTableLayoutPanel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tableLayoutPanel1_MouseUp);
            // 
            // ListViewSelector
            // 
            this.ListViewSelector.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.ListViewSelector.BackColor = System.Drawing.Color.Black;
            this.ListViewSelector.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListViewSelector.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Title});
            this.ListViewSelector.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListViewSelector.ForeColorBright = System.Drawing.SystemColors.WindowText;
            this.ListViewSelector.ForeColorMuted = System.Drawing.SystemColors.WindowText;
            this.ListViewSelector.FullRowSelect = true;
            this.ListViewSelector.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ListViewSelector.HideSelection = false;
            this.ListViewSelector.HighlightBackColour = System.Drawing.Color.Empty;
            this.ListViewSelector.HighlightForeColour = System.Drawing.Color.Empty;
            this.ListViewSelector.LargeIconSize = new System.Drawing.Size(128, 128);
            this.ListViewSelector.Location = new System.Drawing.Point(4, 22);
            this.ListViewSelector.Margin = new System.Windows.Forms.Padding(2);
            this.ListViewSelector.MultiSelect = false;
            this.ListViewSelector.Name = "ListViewSelector";
            this.ListViewSelector.OwnerDraw = true;
            this.ListViewSelector.ShowGroups = false;
            this.ListViewSelector.Size = new System.Drawing.Size(560, 316);
            this.ListViewSelector.SmallIconSize = new System.Drawing.Size(25, 25);
            this.ListViewSelector.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.ListViewSelector.TabIndex = 0;
            this.ListViewSelector.TileHeight = 128;
            this.ListViewSelector.TileSize = new System.Drawing.Size(560, 128);
            this.ListViewSelector.UseCompatibleStateImageBehavior = false;
            this.ListViewSelector.ItemActivate += new System.EventHandler(this.ListViewSelector_ItemActivate);
            this.ListViewSelector.DragDrop += new System.Windows.Forms.DragEventHandler(this.ListViewSelector_DragDrop);
            this.ListViewSelector.DragOver += new System.Windows.Forms.DragEventHandler(this.ListViewSelector_DragOver);
            // 
            // Title
            // 
            this.Title.Text = "Title";
            // 
            // FormSelector
            // 
            this.AllowThemeResize = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 342);
            this.Controls.Add(this.doubleBufferedTableLayoutPanel1);
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSelector";
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Selector";
            this.Shown += new System.EventHandler(this.FormSelector_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSelector_FormClosing);
            this.Controls.SetChildIndex(this.doubleBufferedTableLayoutPanel1, 0);
            this.Controls.SetChildIndex(this.ButtonRestore, 0);
            this.Controls.SetChildIndex(this.ButtonMaximize, 0);
            this.Controls.SetChildIndex(this.ButtonMinimize, 0);
            this.Controls.SetChildIndex(this.ButtonClose, 0);
            this.doubleBufferedTableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Widgets.DoubleBufferedTableLayoutPanel doubleBufferedTableLayoutPanel1;
        public Widgets.ListViewKinsky ListViewSelector;
        private System.Windows.Forms.ColumnHeader Title;

    }
}