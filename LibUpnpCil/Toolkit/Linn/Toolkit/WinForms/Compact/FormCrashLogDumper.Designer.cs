namespace Linn.Toolkit.WinForms
{
    partial class FormCrashLogDumper
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
            this.iTextBoxDetails = new System.Windows.Forms.TextBox();
            this.iLabelTitle = new System.Windows.Forms.Label();
            this.iLabelMessage = new System.Windows.Forms.Label();
            this.iPanelSeparator = new System.Windows.Forms.Panel();
            this.iMainMenu = new System.Windows.Forms.MainMenu();
            this.iMenuItemSend = new System.Windows.Forms.MenuItem();
            this.iMenuItemNoSend = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // iTextBoxDetails
            // 
            this.iTextBoxDetails.Location = new System.Drawing.Point(3, 113);
            this.iTextBoxDetails.Multiline = true;
            this.iTextBoxDetails.Name = "iTextBoxDetails";
            this.iTextBoxDetails.ReadOnly = true;
            this.iTextBoxDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.iTextBoxDetails.Size = new System.Drawing.Size(234, 176);
            this.iTextBoxDetails.TabIndex = 0;
            // 
            // iLabelTitle
            // 
            this.iLabelTitle.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.iLabelTitle.ForeColor = System.Drawing.Color.Black;
            this.iLabelTitle.Location = new System.Drawing.Point(3, 9);
            this.iLabelTitle.Name = "iLabelTitle";
            this.iLabelTitle.Size = new System.Drawing.Size(234, 46);
            this.iLabelTitle.Text = "XXX has encountered a problem and needs to close. We are sorry for the inconvenie" +
                "nce";
            // 
            // iLabelMessage
            // 
            this.iLabelMessage.ForeColor = System.Drawing.Color.Black;
            this.iLabelMessage.Location = new System.Drawing.Point(3, 62);
            this.iLabelMessage.Name = "iLabelMessage";
            this.iLabelMessage.Size = new System.Drawing.Size(234, 48);
            this.iLabelMessage.Text = "We have created an error report that you can send to help us improve XXX.";
            // 
            // iPanelSeparator
            // 
            this.iPanelSeparator.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.iPanelSeparator.BackColor = System.Drawing.Color.Black;
            this.iPanelSeparator.Location = new System.Drawing.Point(3, 58);
            this.iPanelSeparator.Name = "iPanelSeparator";
            this.iPanelSeparator.Size = new System.Drawing.Size(234, 1);
            // 
            // iMainMenu
            // 
            this.iMainMenu.MenuItems.Add(this.iMenuItemSend);
            this.iMainMenu.MenuItems.Add(this.iMenuItemNoSend);
            // 
            // iMenuItemSend
            // 
            this.iMenuItemSend.Text = "Send";
            this.iMenuItemSend.Click += new System.EventHandler(this.MenuItemSendClick);
            // 
            // iMenuItemNoSend
            // 
            this.iMenuItemNoSend.Text = "Don\'t Send";
            this.iMenuItemNoSend.Click += new System.EventHandler(this.MenuItemNoSendClick);
            // 
            // FormCrashLogDumper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(240, 292);
            this.Controls.Add(this.iPanelSeparator);
            this.Controls.Add(this.iLabelMessage);
            this.Controls.Add(this.iLabelTitle);
            this.Controls.Add(this.iTextBoxDetails);
            this.Location = new System.Drawing.Point(0, 0);
            this.Menu = this.iMainMenu;
            this.MinimizeBox = false;
            this.Name = "FormCrashLogDumper";
            this.Text = "XXX";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox iTextBoxDetails;
        private System.Windows.Forms.Label iLabelTitle;
        private System.Windows.Forms.Label iLabelMessage;
        private System.Windows.Forms.Panel iPanelSeparator;
        private System.Windows.Forms.MainMenu iMainMenu;
        private System.Windows.Forms.MenuItem iMenuItemSend;
        private System.Windows.Forms.MenuItem iMenuItemNoSend;
    }
}