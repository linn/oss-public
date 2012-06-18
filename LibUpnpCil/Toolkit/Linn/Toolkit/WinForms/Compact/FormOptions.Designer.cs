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
            this.iTabControl = new System.Windows.Forms.TabControl();
            this.iMainMenu = new System.Windows.Forms.MainMenu();
            this.iMenuItemReset = new System.Windows.Forms.MenuItem();
            this.iMenuItemDone = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // iTabControl
            // 
            this.iTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iTabControl.Location = new System.Drawing.Point(0, 0);
            this.iTabControl.Name = "iTabControl";
            this.iTabControl.SelectedIndex = 0;
            this.iTabControl.Size = new System.Drawing.Size(240, 268);
            this.iTabControl.TabIndex = 0;
            // 
            // iMainMenu
            // 
            this.iMainMenu.MenuItems.Add(this.iMenuItemReset);
            this.iMainMenu.MenuItems.Add(this.iMenuItemDone);
            // 
            // iMenuItemReset
            // 
            this.iMenuItemReset.Text = "Reset";
            this.iMenuItemReset.Click += new System.EventHandler(this.MenuItemResetClick);
            // 
            // iMenuItemDone
            // 
            this.iMenuItemDone.Text = "Done";
            this.iMenuItemDone.Click += new System.EventHandler(this.MenuItemDoneClick);
            // 
            // FormUserOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.SteelBlue;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.ControlBox = false;
            this.Controls.Add(this.iTabControl);
            this.Menu = this.iMainMenu;
            this.MinimizeBox = false;
            this.Name = "FormUserOptions";
            this.Text = "Options";
            this.Closed += new System.EventHandler(this.EventFormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MainMenu iMainMenu;
        private System.Windows.Forms.MenuItem iMenuItemReset;
        private System.Windows.Forms.MenuItem iMenuItemDone;
        protected System.Windows.Forms.TabControl iTabControl;
    }
}