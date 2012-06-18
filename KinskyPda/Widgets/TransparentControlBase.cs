using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace KinskyPda
{
    public class TransparentControlBase : Control
    {
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Parent is PanelNoBackgroundPaint)
            {
                PanelNoBackgroundPaint backgroundPanel = (PanelNoBackgroundPaint)Parent;

                e.Graphics.DrawImage(
                  backgroundPanel.BackgroundImage,
                  0,
                  0,
                  Bounds,
                  GraphicsUnit.Pixel);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Refresh();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            Invalidate();
        }
    }
}
