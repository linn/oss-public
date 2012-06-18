using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using Linn;
using Linn.Kinsky;

namespace KinskyDesktop
{
    partial class FormAboutBox : FormThemed
    {
        private IHelper iHelper;
        private string iVersion;
        private Image iKinskyLogo = KinskyDesktop.Properties.Resources.AboutBox;

        public FormAboutBox(IHelper aHelper)
        {
            iHelper = aHelper;
            iVersion = String.Format("Version {0}", iHelper.Version + " (" + iHelper.Family + ")");

            InitializeComponent();

            Size = new System.Drawing.Size(457, 280);
            Text = String.Format("About {0}", iHelper.Title);

            buttonOK.Location = new Point(366, 244);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawImage(iKinskyLogo, new Point(30, 40));
            using (Brush b = new SolidBrush(ForeColor))
            {
                Rectangle rect = new Rectangle(30, 195, iKinskyLogo.Width, Font.Height);
                e.Graphics.DrawString(iHelper.Product, Font, b, rect);
                rect.Y += Font.Height;
                e.Graphics.DrawString(iVersion, Font, b, rect);
                rect.Y += Font.Height;
                e.Graphics.DrawString(iHelper.Copyright, Font, b, rect);
                rect.Y += Font.Height;
                e.Graphics.DrawString(iHelper.Company, Font, b, rect);
                rect.Y += Font.Height;
                e.Graphics.DrawString(iHelper.Description, Font, b, rect);
            }
        }
    }
}
