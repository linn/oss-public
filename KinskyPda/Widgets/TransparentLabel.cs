using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace KinskyPda
{
    public class TransparentLabel : TransparentControlBase
    {
        private ContentAlignment iAlignment = ContentAlignment.TopLeft;
        private StringFormat iFormat = null;
        
        public TransparentLabel()
        {
            iFormat = new StringFormat();
        }

        public ContentAlignment TextAlign
        {
            get { return iAlignment; }
            set
            {
                iAlignment = value;
                switch (iAlignment)
                {
                    case ContentAlignment.TopCenter:
                        iFormat.Alignment = StringAlignment.Center;
                        //iFormat.LineAlignment = StringAlignment.Center;
                        break;
                    case ContentAlignment.TopLeft:
                        iFormat.Alignment = StringAlignment.Near;
                        //iFormat.LineAlignment = StringAlignment.Near;
                        break;
                    case ContentAlignment.TopRight:
                        iFormat.Alignment = StringAlignment.Far;
                        //iFormat.LineAlignment = StringAlignment.Far;
                        break;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(ForeColor))
            {
                e.Graphics.DrawString(
                  Text,
                  Font,
                  brush,
                  new Rectangle(0, 0, Width, Height),
                  iFormat);
            }

            /*using (Pen p = new Pen(Color.White))
            {
                Rectangle r = new Rectangle(0, 0, Width, Height);
                r.Inflate(-1, -1);
                e.Graphics.DrawRectangle(p, r);
            }*/
        }
    }
}
