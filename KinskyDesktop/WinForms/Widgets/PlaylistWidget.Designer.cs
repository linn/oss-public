namespace KinskyDesktop.Widgets
{
    partial class PlaylistWidget<T>
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.scrollBarControl1 = new KinskyDesktop.Widgets.ScrollBarControl();
            this.SuspendLayout();
            // 
            // scrollBarControl1
            // 
            this.scrollBarControl1.Dock = System.Windows.Forms.DockStyle.Right;
            this.scrollBarControl1.ImageChannel = null;
            this.scrollBarControl1.ImageDownArrow = null;
            this.scrollBarControl1.ImageDownArrowMouse = null;
            this.scrollBarControl1.ImageDownArrowTouch = null;
            this.scrollBarControl1.ImageThumb = global::Linn.Kinsky.Properties.Resources.ThumbMiddle;
            this.scrollBarControl1.ImageThumbBottom = null;
            this.scrollBarControl1.ImageThumbBottomSpan = null;
            this.scrollBarControl1.ImageThumbMouse = global::Linn.Kinsky.Properties.Resources.ThumbMiddleMouse;
            this.scrollBarControl1.ImageThumbTop = null;
            this.scrollBarControl1.ImageThumbTopSpan = null;
            this.scrollBarControl1.ImageThumbTouch = global::Linn.Kinsky.Properties.Resources.ThumbMiddleTouch;
            this.scrollBarControl1.ImageUpArrow = null;
            this.scrollBarControl1.ImageUpArrowMouse = null;
            this.scrollBarControl1.ImageUpArrowTouch = null;
            this.scrollBarControl1.Location = new System.Drawing.Point(271, 0);
            this.scrollBarControl1.MouseOverShow = true;
            this.scrollBarControl1.Name = "scrollBarControl1";
            this.scrollBarControl1.Size = new System.Drawing.Size(20, 296);
            this.scrollBarControl1.TabIndex = 0;
            // 
            // PlaylistWidget
            // 
            this.Controls.Add(this.scrollBarControl1);
            this.DoubleBuffered = true;
            this.Name = "PlaylistWidget";
            this.Size = new System.Drawing.Size(291, 296);
            this.ResumeLayout(false);

        }

        #endregion

        private ScrollBarControl scrollBarControl1;
    }
}