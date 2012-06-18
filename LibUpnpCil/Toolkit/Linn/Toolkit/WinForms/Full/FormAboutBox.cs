using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Linn;

namespace Linn.Toolkit.WinForms
{
    public partial class FormAboutBox : Form
    {
        public FormAboutBox(IHelper aHelper) {
            InitializeComponent();
            Text = String.Format("About {0}", aHelper.Title);
            labelProductName.Text = aHelper.Product;
            labelVersion.Text = String.Format("Version {0}", aHelper.Version + " (" + aHelper.Family + ")");
            labelCopyright.Text = aHelper.Copyright;
            labelCompanyName.Text = aHelper.Company;
            textBoxDescription.Text = aHelper.Description;
        }
    }
}
