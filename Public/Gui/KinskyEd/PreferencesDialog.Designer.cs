using System;
using System.Drawing;
using System.Windows.Forms;

namespace Linn {
namespace Gui {
namespace Editor {

partial class PreferencesDialog
{
    private System.Windows.Forms.Label packageLabel;
    private System.Windows.Forms.Label textureLabel;
    private System.Windows.Forms.TextBox packageTextBox;
    private System.Windows.Forms.TextBox textureTextBox;
    private System.Windows.Forms.Button packageButton;
    private System.Windows.Forms.Button textureButton;
    private System.Windows.Forms.Button saveButton;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button cancelButton;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
        if( disposing )
        {
            if(components != null)
            {
                components.Dispose();
            }
        }
        base.Dispose( disposing );
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.packageLabel = new System.Windows.Forms.Label();
        this.textureLabel = new System.Windows.Forms.Label();
        this.packageTextBox = new System.Windows.Forms.TextBox();
        this.textureTextBox = new System.Windows.Forms.TextBox();
        this.packageButton = new System.Windows.Forms.Button();
        this.textureButton = new System.Windows.Forms.Button();
        this.saveButton = new System.Windows.Forms.Button();
        this.groupBox1 = new System.Windows.Forms.GroupBox();
        this.cancelButton = new System.Windows.Forms.Button();
        this.groupBox1.SuspendLayout();
        this.SuspendLayout();
        // 
        // packageLabel
        // 
        this.packageLabel.Location = new System.Drawing.Point(8, 24);
        this.packageLabel.Name = "packageLabel";
        this.packageLabel.Size = new System.Drawing.Size(96, 24);
        this.packageLabel.TabIndex = 0;
        this.packageLabel.Text = "Package Cache:";
        this.packageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // textureLabel
        // 
        this.textureLabel.Location = new System.Drawing.Point(8, 64);
        this.textureLabel.Name = "textureLabel";
        this.textureLabel.Size = new System.Drawing.Size(88, 24);
        this.textureLabel.TabIndex = 1;
        this.textureLabel.Text = "Texture Cache:";
        this.textureLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // packageTextBox
        // 
        this.packageTextBox.Location = new System.Drawing.Point(104, 24);
        this.packageTextBox.Name = "packageTextBox";
        this.packageTextBox.Size = new System.Drawing.Size(344, 20);
        this.packageTextBox.TabIndex = 2;
        // 
        // textureTextBox
        // 
        this.textureTextBox.Location = new System.Drawing.Point(104, 64);
        this.textureTextBox.Name = "textureTextBox";
        this.textureTextBox.Size = new System.Drawing.Size(344, 20);
        this.textureTextBox.TabIndex = 3;
        // 
        // packageButton
        // 
        this.packageButton.Location = new System.Drawing.Point(456, 24);
        this.packageButton.Name = "packageButton";
        this.packageButton.Size = new System.Drawing.Size(64, 24);
        this.packageButton.TabIndex = 4;
        this.packageButton.Text = "Browse";
        this.packageButton.Click += new System.EventHandler(this.packageButtonBrowse_Click);
        // 
        // textureButton
        // 
        this.textureButton.Location = new System.Drawing.Point(456, 64);
        this.textureButton.Name = "textureButton";
        this.textureButton.Size = new System.Drawing.Size(64, 24);
        this.textureButton.TabIndex = 5;
        this.textureButton.Text = "Browse";
        this.textureButton.Click += new System.EventHandler(this.textureButtonBrowse_Click);
        // 
        // saveButton
        // 
        this.saveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.saveButton.Location = new System.Drawing.Point(8, 120);
        this.saveButton.Name = "saveButton";
        this.saveButton.Size = new System.Drawing.Size(272, 24);
        this.saveButton.TabIndex = 6;
        this.saveButton.Text = "Save";
        this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
        // 
        // groupBox1
        // 
        this.groupBox1.Controls.Add(this.textureTextBox);
        this.groupBox1.Controls.Add(this.packageTextBox);
        this.groupBox1.Controls.Add(this.textureLabel);
        this.groupBox1.Controls.Add(this.packageLabel);
        this.groupBox1.Controls.Add(this.textureButton);
        this.groupBox1.Controls.Add(this.packageButton);
        this.groupBox1.Location = new System.Drawing.Point(8, 8);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(528, 104);
        this.groupBox1.TabIndex = 7;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Caches";
        // 
        // cancelButton
        // 
        this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.cancelButton.Location = new System.Drawing.Point(280, 120);
        this.cancelButton.Name = "cancelButton";
        this.cancelButton.Size = new System.Drawing.Size(256, 24);
        this.cancelButton.TabIndex = 8;
        this.cancelButton.Text = "Cancel";
        // 
        // PreferencesDialog
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(544, 149);
        this.Controls.Add(this.cancelButton);
        this.Controls.Add(this.saveButton);
        this.Controls.Add(this.groupBox1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "PreferencesDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Preferences Dialog";
        this.groupBox1.ResumeLayout(false);
        this.groupBox1.PerformLayout();
        this.ResumeLayout(false);

    }
    #endregion
}


} // Editor
} // Gui
} // Linn
