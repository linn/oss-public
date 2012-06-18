namespace TestWebServicesAsync
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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.iMenuItemSettings = new System.Windows.Forms.MenuItem();
            this.iMenuItemOption = new System.Windows.Forms.MenuItem();
            this.iMenuItemDebug = new System.Windows.Forms.MenuItem();
            this.iMenuItemHelp = new System.Windows.Forms.MenuItem();
            this.iMenuItemAbout = new System.Windows.Forms.MenuItem();
            this.iText = new System.Windows.Forms.TextBox();
            this.iResult = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.iMenuItemSettings);
            this.mainMenu1.MenuItems.Add(this.iMenuItemHelp);
            // 
            // iMenuItemSettings
            // 
            this.iMenuItemSettings.MenuItems.Add(this.iMenuItemOption);
            this.iMenuItemSettings.MenuItems.Add(this.iMenuItemDebug);
            this.iMenuItemSettings.Text = "Settings";
            // 
            // iMenuItemOption
            // 
            this.iMenuItemOption.Text = "Options";
            this.iMenuItemOption.Click += new System.EventHandler(this.MenuItemOptionsClick);
            // 
            // iMenuItemDebug
            // 
            this.iMenuItemDebug.Text = "User Console";
            this.iMenuItemDebug.Click += new System.EventHandler(this.MenuItemDebugClick);
            // 
            // iMenuItemHelp
            // 
            this.iMenuItemHelp.MenuItems.Add(this.iMenuItemAbout);
            this.iMenuItemHelp.Text = "Help";
            // 
            // iMenuItemAbout
            // 
            this.iMenuItemAbout.Text = "About";
            this.iMenuItemAbout.Click += new System.EventHandler(this.MenuItemAboutClick);
            // 
            // iText
            // 
            this.iText.Location = new System.Drawing.Point(70, 45);
            this.iText.Name = "iText";
            this.iText.ReadOnly = true;
            this.iText.Size = new System.Drawing.Size(100, 21);
            this.iText.TabIndex = 0;
            // 
            // iResult
            // 
            this.iResult.Location = new System.Drawing.Point(3, 107);
            this.iResult.Name = "iResult";
            this.iResult.ReadOnly = true;
            this.iResult.Size = new System.Drawing.Size(234, 21);
            this.iResult.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.iResult);
            this.Controls.Add(this.iText);
            this.Menu = this.mainMenu1;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "TestWebServicesAsync";
            this.Closed += new System.EventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem iMenuItemSettings;
        private System.Windows.Forms.MenuItem iMenuItemHelp;
        private System.Windows.Forms.MenuItem iMenuItemDebug;
        private System.Windows.Forms.MenuItem iMenuItemOption;
        private System.Windows.Forms.MenuItem iMenuItemAbout;
        private System.Windows.Forms.TextBox iText;
        private System.Windows.Forms.TextBox iResult;
    }
}