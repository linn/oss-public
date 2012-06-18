namespace KinskyPda
{
    partial class FormAboutBox
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
            this.iMenuItemLeft = new System.Windows.Forms.MenuItem();
            this.iMenuItemDone = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // iMainMenu
            // 
            this.iMainMenu.MenuItems.Add(this.iMenuItemLeft);
            this.iMainMenu.MenuItems.Add(this.iMenuItemDone);
            // 
            // iMenuItemLeft
            // 
            this.iMenuItemLeft.Text = "";
            // 
            // iMenuItemDone
            // 
            this.iMenuItemDone.Text = "Done";
            this.iMenuItemDone.Click += new System.EventHandler(this.MenuItemDoneClick);
            // 
            // FormAboutBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 294);
            this.ControlBox = false;
            this.Location = new System.Drawing.Point(0, 0);
            this.Menu = this.iMainMenu;
            this.Name = "FormAboutBox";
            this.Text = "FormAboutBox";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem iMenuItemLeft;
        private System.Windows.Forms.MenuItem iMenuItemDone;
    }
}