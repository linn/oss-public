using System;
using System.Windows.Forms;
using System.Globalization;

namespace Linn {
namespace Gui {
namespace Editor {

partial class NewLayoutDialog
{
    private System.Windows.Forms.TextBox nsTextBox;
    private System.Windows.Forms.GroupBox propertiesBox;
    private System.Windows.Forms.Label heightLabel;
    private System.Windows.Forms.TextBox heightTextBox;
    private System.Windows.Forms.TextBox widthTextBox;
    private System.Windows.Forms.Label widthLabel;
    private System.Windows.Forms.Label nsLabel;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button okButton;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
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
    private void InitializeComponent() {
        this.nsTextBox = new System.Windows.Forms.TextBox();
        this.propertiesBox = new System.Windows.Forms.GroupBox();
        this.cancelButton = new System.Windows.Forms.Button();
        this.okButton = new System.Windows.Forms.Button();
        this.heightLabel = new System.Windows.Forms.Label();
        this.heightTextBox = new System.Windows.Forms.TextBox();
        this.widthTextBox = new System.Windows.Forms.TextBox();
        this.widthLabel = new System.Windows.Forms.Label();
        this.nsLabel = new System.Windows.Forms.Label();
        this.propertiesBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // nsTextBox
        // 
        this.nsTextBox.Location = new System.Drawing.Point(82, 28);
        this.nsTextBox.Name = "nsTextBox";
        this.nsTextBox.Size = new System.Drawing.Size(198, 20);
        this.nsTextBox.TabIndex = 0;
        this.nsTextBox.Text = "NewLayout";
        // 
        // propertiesBox
        // 
        this.propertiesBox.Controls.Add(this.cancelButton);
        this.propertiesBox.Controls.Add(this.okButton);
        this.propertiesBox.Controls.Add(this.heightLabel);
        this.propertiesBox.Controls.Add(this.heightTextBox);
        this.propertiesBox.Controls.Add(this.widthTextBox);
        this.propertiesBox.Controls.Add(this.widthLabel);
        this.propertiesBox.Controls.Add(this.nsLabel);
        this.propertiesBox.Controls.Add(this.nsTextBox);
        this.propertiesBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.propertiesBox.Location = new System.Drawing.Point(0, 0);
        this.propertiesBox.Name = "propertiesBox";
        this.propertiesBox.Size = new System.Drawing.Size(292, 169);
        this.propertiesBox.TabIndex = 1;
        this.propertiesBox.TabStop = false;
        this.propertiesBox.Text = "Properties";
        // 
        // cancelButton
        // 
        this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.cancelButton.Location = new System.Drawing.Point(130, 133);
        this.cancelButton.Name = "cancelButton";
        this.cancelButton.Size = new System.Drawing.Size(75, 23);
        this.cancelButton.TabIndex = 7;
        this.cancelButton.Text = "Cancel";
        this.cancelButton.UseVisualStyleBackColor = true;
        // 
        // okButton
        // 
        this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.okButton.Location = new System.Drawing.Point(211, 133);
        this.okButton.Name = "okButton";
        this.okButton.Size = new System.Drawing.Size(75, 23);
        this.okButton.TabIndex = 6;
        this.okButton.Text = "OK";
        this.okButton.UseVisualStyleBackColor = true;
        this.okButton.Click += new System.EventHandler(this.okButton_Click);
        // 
        // heightLabel
        // 
        this.heightLabel.AutoSize = true;
        this.heightLabel.Location = new System.Drawing.Point(12, 99);
        this.heightLabel.Name = "heightLabel";
        this.heightLabel.Size = new System.Drawing.Size(41, 13);
        this.heightLabel.TabIndex = 5;
        this.heightLabel.Text = "Height:";
        // 
        // heightTextBox
        // 
        this.heightTextBox.Location = new System.Drawing.Point(82, 96);
        this.heightTextBox.Name = "heightTextBox";
        this.heightTextBox.Size = new System.Drawing.Size(198, 20);
        this.heightTextBox.TabIndex = 4;
        this.heightTextBox.Text = "480";
        // 
        // widthTextBox
        // 
        this.widthTextBox.Location = new System.Drawing.Point(82, 63);
        this.widthTextBox.Name = "widthTextBox";
        this.widthTextBox.Size = new System.Drawing.Size(198, 20);
        this.widthTextBox.TabIndex = 3;
        this.widthTextBox.Text = "640";
        // 
        // widthLabel
        // 
        this.widthLabel.AutoSize = true;
        this.widthLabel.Location = new System.Drawing.Point(12, 66);
        this.widthLabel.Name = "widthLabel";
        this.widthLabel.Size = new System.Drawing.Size(38, 13);
        this.widthLabel.TabIndex = 2;
        this.widthLabel.Text = "Width:";
        // 
        // nsLabel
        // 
        this.nsLabel.AutoSize = true;
        this.nsLabel.Location = new System.Drawing.Point(12, 31);
        this.nsLabel.Name = "nsLabel";
        this.nsLabel.Size = new System.Drawing.Size(67, 13);
        this.nsLabel.TabIndex = 1;
        this.nsLabel.Text = "Namespace:";
        // 
        // NewLayoutDialog
        // 
        this.AcceptButton = this.okButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.cancelButton;
        this.ClientSize = new System.Drawing.Size(292, 169);
        this.Controls.Add(this.propertiesBox);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "NewLayoutDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "New Layout Dialog";
        this.propertiesBox.ResumeLayout(false);
        this.propertiesBox.PerformLayout();
        this.ResumeLayout(false);

    }

    #endregion
}

} // Editor
} // Gui
} // Linn
