namespace KinskyDesktop
{
    partial class FormThemed
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.ButtonMinimize = new KinskyDesktop.Widgets.ButtonControl();
            this.ButtonMaximize = new KinskyDesktop.Widgets.ButtonControl();
            this.ButtonClose = new KinskyDesktop.Widgets.ButtonControl();
            this.ButtonRestore = new KinskyDesktop.Widgets.ButtonControl();
            this.SuspendLayout();
            // 
            // ButtonMinimize
            // 
            this.ButtonMinimize.BackColor = System.Drawing.Color.Transparent;
            this.ButtonMinimize.ImageDisabled = global::Linn.Kinsky.Properties.Resources.WindowsMinimize;
            this.ButtonMinimize.ImageMouseOver = global::Linn.Kinsky.Properties.Resources.WindowsMinimizeMouse;
            this.ButtonMinimize.ImageStateInitial = global::Linn.Kinsky.Properties.Resources.WindowsMinimize;
            this.ButtonMinimize.ImageTouched = global::Linn.Kinsky.Properties.Resources.WindowsMinimizeTouch;
            this.ButtonMinimize.Location = new System.Drawing.Point(253, 3);
            this.ButtonMinimize.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonMinimize.Name = "ButtonMinimize";
            this.ButtonMinimize.Size = new System.Drawing.Size(20, 16);
            this.ButtonMinimize.TabIndex = 2;
            this.ButtonMinimize.EventClick += new System.EventHandler<System.EventArgs>(this.ButtonMinimize_EventClick);
            // 
            // ButtonMaximize
            // 
            this.ButtonMaximize.BackColor = System.Drawing.Color.Transparent;
            this.ButtonMaximize.ImageDisabled = global::Linn.Kinsky.Properties.Resources.WindowsMaximize;
            this.ButtonMaximize.ImageMouseOver = global::Linn.Kinsky.Properties.Resources.WindowsMaximizeMouse;
            this.ButtonMaximize.ImageStateInitial = global::Linn.Kinsky.Properties.Resources.WindowsMaximize;
            this.ButtonMaximize.ImageTouched = global::Linn.Kinsky.Properties.Resources.WindowsMaximizeTouch;
            this.ButtonMaximize.Location = new System.Drawing.Point(273, 3);
            this.ButtonMaximize.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonMaximize.Name = "ButtonMaximize";
            this.ButtonMaximize.Size = new System.Drawing.Size(20, 16);
            this.ButtonMaximize.TabIndex = 1;
            this.ButtonMaximize.EventClick += new System.EventHandler<System.EventArgs>(this.ButtonMaximize_EventClick);
            // 
            // ButtonClose
            // 
            this.ButtonClose.BackColor = System.Drawing.Color.Transparent;
            this.ButtonClose.ImageDisabled = global::Linn.Kinsky.Properties.Resources.WindowsClose;
            this.ButtonClose.ImageMouseOver = global::Linn.Kinsky.Properties.Resources.WindowsCloseMouse;
            this.ButtonClose.ImageStateInitial = global::Linn.Kinsky.Properties.Resources.WindowsClose;
            this.ButtonClose.ImageTouched = global::Linn.Kinsky.Properties.Resources.WindowsCloseTouch;
            this.ButtonClose.Location = new System.Drawing.Point(293, 3);
            this.ButtonClose.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonClose.Name = "ButtonClose";
            this.ButtonClose.Size = new System.Drawing.Size(20, 16);
            this.ButtonClose.TabIndex = 0;
            this.ButtonClose.EventClick += new System.EventHandler<System.EventArgs>(this.ButtonClose_EventClick);
            // 
            // ButtonRestore
            // 
            this.ButtonRestore.BackColor = System.Drawing.Color.Transparent;
            this.ButtonRestore.ImageDisabled = global::Linn.Kinsky.Properties.Resources.WindowsRestore;
            this.ButtonRestore.ImageMouseOver = global::Linn.Kinsky.Properties.Resources.WindowsRestoreMouse;
            this.ButtonRestore.ImageStateInitial = global::Linn.Kinsky.Properties.Resources.WindowsRestore;
            this.ButtonRestore.ImageTouched = global::Linn.Kinsky.Properties.Resources.WindowsRestoreTouch;
            this.ButtonRestore.Location = new System.Drawing.Point(273, 3);
            this.ButtonRestore.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonRestore.Name = "ButtonRestore";
            this.ButtonRestore.Size = new System.Drawing.Size(20, 16);
            this.ButtonRestore.TabIndex = 3;
            this.ButtonRestore.Visible = false;
            this.ButtonRestore.EventClick += new System.EventHandler<System.EventArgs>(this.ButtonRestore_EventClick);
            // 
            // FormThemed
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.ClientSize = new System.Drawing.Size(318, 331);
            this.Controls.Add(this.ButtonMinimize);
            this.Controls.Add(this.ButtonMaximize);
            this.Controls.Add(this.ButtonClose);
            this.Controls.Add(this.ButtonRestore);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormThemed";
            this.ShowIcon = false;
            this.Text = "FormThemed";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.ResumeLayout(false);

        }

        protected KinskyDesktop.Widgets.ButtonControl ButtonClose;
        protected KinskyDesktop.Widgets.ButtonControl ButtonMaximize;
        protected KinskyDesktop.Widgets.ButtonControl ButtonMinimize;
        protected KinskyDesktop.Widgets.ButtonControl ButtonRestore;
    }
}