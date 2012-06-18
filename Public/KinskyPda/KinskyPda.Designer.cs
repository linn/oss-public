using System.Drawing;

namespace KinskyPda
{
    partial class FormKinskyPda
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
            this.iMainMenu = new System.Windows.Forms.MainMenu();
            this.iMenuItemLeft = new System.Windows.Forms.MenuItem();
            this.iMenuItemRight = new System.Windows.Forms.MenuItem();
            this.userControlPlay = new KinskyPda.Views.UserControlPlay();
            this.userControlSource1 = new KinskyPda.Views.UserControlSource();
            this.userControlBrowser1 = new KinskyPda.Views.UserControlBrowser();
            this.SuspendLayout();
            // 
            // iMainMenu
            // 
            this.iMainMenu.MenuItems.Add(this.iMenuItemLeft);
            this.iMainMenu.MenuItems.Add(this.iMenuItemRight);
            // 
            // iMenuItemLeft
            // 
            this.iMenuItemLeft.Text = "Left";
            this.iMenuItemLeft.Click += new System.EventHandler(this.MenuItemLeftClick);
            // 
            // iMenuItemRight
            // 
            this.iMenuItemRight.Text = "Right";
            this.iMenuItemRight.Click += new System.EventHandler(this.MenuItemRightClick);
            // 
            // userControlPlay
            // 
            this.userControlPlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userControlPlay.Location = new System.Drawing.Point(0, 0);
            this.userControlPlay.Name = "userControlPlay";
            this.userControlPlay.Size = new System.Drawing.Size(240, 294);
            this.userControlPlay.TabIndex = 0;
            // 
            // userControlSource1
            // 
            this.userControlSource1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userControlSource1.Location = new System.Drawing.Point(0, 0);
            this.userControlSource1.Name = "userControlSource1";
            this.userControlSource1.Size = new System.Drawing.Size(240, 294);
            this.userControlSource1.TabIndex = 1;
            this.userControlSource1.Visible = false;
            // 
            // userControlBrowser1
            // 
            this.userControlBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userControlBrowser1.Location = new System.Drawing.Point(0, 0);
            this.userControlBrowser1.Name = "userControlBrowser1";
            this.userControlBrowser1.Size = new System.Drawing.Size(240, 294);
            this.userControlBrowser1.TabIndex = 2;
            this.userControlBrowser1.Visible = false;
            // 
            // KinskyPDA
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = Color.FromArgb(22, 22, 22);
            this.ClientSize = new System.Drawing.Size(240, 294);
            this.Controls.Add(this.userControlPlay);
            this.Controls.Add(this.userControlSource1);
            this.Controls.Add(this.userControlBrowser1);
            this.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(0, 0);
            this.Menu = this.iMainMenu;
            this.Name = "Kinsky";
            this.Text = "Kinsky";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FormLoad);
            this.Closed += new System.EventHandler(this.FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private KinskyPda.Views.UserControlBrowser userControlBrowser1;
        private KinskyPda.Views.UserControlSource userControlSource1;
        private KinskyPda.Views.UserControlPlay userControlPlay;
        private System.Windows.Forms.MainMenu iMainMenu;
        private System.Windows.Forms.MenuItem iMenuItemLeft;
        private System.Windows.Forms.MenuItem iMenuItemRight;
    }
}