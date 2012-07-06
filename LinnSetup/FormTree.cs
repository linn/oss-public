using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Linn.ProductSupport;

namespace LinnSetup
{
    public partial class FormTree : Form
    {
        public FormTree(Icon aIcon, Boxes aBoxes)
        {
            iBoxes = aBoxes;
            InitializeComponent();
            Icon = aIcon;
        }

        private void FormTree_Load(object sender, EventArgs e) {
            UpdateTree();
        }

        public void UpdateTree() {
            this.BeginInvoke(
                 (MethodInvoker)delegate() {
                     textBoxTree.Text = iBoxes.ToString();
                 }
            );
        }

        private Boxes iBoxes;
    }
}
