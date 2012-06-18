using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using Linn;
using Linn.Kinsky;
using KinskyWeb.Kinsky;

namespace KinskyWeb
{
    partial class FormAboutBox : Form
    {
        private Image iKinskyLogo = Resources.AboutBox;

        public FormAboutBox()
        {
            
            InitializeComponent();

            if (this.Parent == null)
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            Text = String.Format("About {0}", KinskyStack.GetDefault().Helper.Title);

            buttonOK.Location = new Point(366, 244);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawImage(iKinskyLogo, new Point(30, 40));
            using (Brush b = new SolidBrush(ForeColor))
            {
                Rectangle rect = new Rectangle(30, 195, iKinskyLogo.Width, Font.Height);
                e.Graphics.DrawString(KinskyStack.GetDefault().Helper.Product, Font, b, rect);
                rect.Y += Font.Height;
                e.Graphics.DrawString(KinskyStack.GetDefault().Helper.Version, Font, b, rect);
                rect.Y += Font.Height;
                e.Graphics.DrawString(KinskyStack.GetDefault().Helper.Copyright, Font, b, rect);
                rect.Y += Font.Height;
                e.Graphics.DrawString(KinskyStack.GetDefault().Helper.Company, Font, b, rect);
                rect.Y += Font.Height;
                e.Graphics.DrawString(KinskyStack.GetDefault().Helper.Description, Font, b, rect);
            }
        }
    }
}
