using System;
using System.Windows.Forms;
using System.Drawing;

namespace Linn {
namespace Kinsky {

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
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplicationCanvas));
        this.iFadeInTimer = new System.Windows.Forms.Timer(this.components);
        this.iFadeOutTimer = new System.Windows.Forms.Timer(this.components);
        this.SuspendLayout();
        // 
        // iFadeInTimer
        // 
        this.iFadeInTimer.Interval = 1;
        this.iFadeInTimer.Tick += new System.EventHandler(this.iFadeInTimer_Tick);
        // 
        // iFadeOutTimer
        // 
        this.iFadeOutTimer.Interval = 1;
        this.iFadeOutTimer.Tick += new System.EventHandler(this.iFadeOutTimer_Tick);
        // 
        // ApplicationCanvas
        // 
        this.ClientSize = new System.Drawing.Size(292, 273);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.Name = "ApplicationCanvas";
        this.Opacity = 0;
        this.Text = "LinnGui";
        this.Deactivate += new System.EventHandler(this.ApplicationCanvas_Deactivate);
        this.Load += new System.EventHandler(this.ApplicationCanvas_Load);
        this.Closing += new System.ComponentModel.CancelEventHandler(this.ApplicationCanvas_Closing);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Timer iFadeInTimer;
    private System.Windows.Forms.Timer iFadeOutTimer;
}

} // Kinsky
} // Linn
