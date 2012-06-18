namespace MediaServerBrowser
{
    partial class iForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu iMainMenu;

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
            this.iMainMenu = new System.Windows.Forms.MainMenu();
            this.iListView = new System.Windows.Forms.ListView();
            this.iButtonBack = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // iListView
            // 
            this.iListView.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.iListView.FullRowSelect = true;
            this.iListView.Location = new System.Drawing.Point(0, 0);
            this.iListView.Name = "iListView";
            this.iListView.Size = new System.Drawing.Size(240, 246);
            this.iListView.TabIndex = 1;
            this.iListView.View = System.Windows.Forms.View.List;
            this.iListView.ItemActivate += new System.EventHandler(this.iListView_ItemActivate);
            // 
            // iButtonBack
            // 
            this.iButtonBack.Location = new System.Drawing.Point(0, 245);
            this.iButtonBack.Name = "iButtonBack";
            this.iButtonBack.Size = new System.Drawing.Size(240, 23);
            this.iButtonBack.TabIndex = 2;
            this.iButtonBack.Text = "Back";
            this.iButtonBack.Click += new System.EventHandler(this.iButtonBack_Click);
            // 
            // iForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.iButtonBack);
            this.Controls.Add(this.iListView);
            this.Menu = this.iMainMenu;
            this.Name = "iForm";
            this.Text = "MediaServerBrowser";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView iListView;
        private System.Windows.Forms.Button iButtonBack;

    }
}

