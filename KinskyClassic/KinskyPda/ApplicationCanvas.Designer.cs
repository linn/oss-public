using System;
using System.Windows.Forms;
using System.Drawing;

namespace Linn {
namespace KinskyPda {

public partial class ApplicationCanvas
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
    private void InitializeComponent() {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplicationCanvas));
        this.SuspendLayout();
        // 
        // ApplicationCanvas
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        this.ClientSize = new System.Drawing.Size(240, 294);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.Name = "ApplicationCanvas";
        this.Text = "LinnGuiPda";
        this.Deactivate += new System.EventHandler(this.ApplicationCanvas_Deactivate);
        this.Load += new System.EventHandler(this.ApplicationCanvas_Load);
        this.Closing += new System.ComponentModel.CancelEventHandler(this.ApplicationCanvas_Closing);
        this.ResumeLayout(false);

    }

    #endregion
}

} // KinskyPda
} // Linn
