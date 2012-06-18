using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

using Linn;
using Linn.Kinsky;
using Linn.Toolkit.WinForms;

namespace KinskyDesktop
{
    public partial class FormUserOptions : FormThemed
    {
        public FormUserOptions(IList<IOptionPage> aOptionPages)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;

            iButtonOk.Select();

            Size = new System.Drawing.Size(600, 330);

            Size panelSize = PanelOptionsPage.Size;

            foreach (IOptionPage optionPage in aOptionPages)
            {
                OptionPageWinForms page = new OptionPageWinForms(panelSize.Width, optionPage);

                TreeNode node = new TreeNode();
                node.Name = optionPage.Name;
                node.Text = optionPage.Name;
                node.Tag = page;
                TreeViewOptions.Nodes.Add(node);
            }

            if (aOptionPages.Count > 0)
            {
                TreeViewOptions.SelectedNode = TreeViewOptions.Nodes[0];
            }
        }

        public void SetPageByName(string aPageName)
        {
            TreeViewOptions.SelectedNode = TreeViewOptions.Nodes[aPageName];
        }

        private void TreeViewOptions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                OptionPageWinForms page = e.Node.Tag as OptionPageWinForms;
                Control control = page.Control;
                control.Size = PanelOptionsPage.Size;
                control.Dock = DockStyle.Fill;

                PanelOptionsPage.SuspendLayout();
                PanelOptionsPage.Controls.Clear();
                PanelOptionsPage.Controls.Add(control);
                PanelOptionsPage.ResumeLayout(true);
            }
            else
            {
                if (TreeViewOptions.Nodes.Count > 0)
                {
                    TreeViewOptions.SelectedNode = TreeViewOptions.Nodes[0];
                }
            }
        }

        private void ButtonResetClick(object sender, EventArgs e)
        {
            if (TreeViewOptions.SelectedNode != null)
            {
                OptionPageWinForms winformPage = TreeViewOptions.SelectedNode.Tag as OptionPageWinForms;
                IOptionPage page = winformPage.OptionPage;
                foreach (Option o in page.Options)
                {
                    o.ResetToDefault();
                }
            }
        }

        private void EventFormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (TreeNode node in TreeViewOptions.Nodes)
            {
                OptionPageWinForms page = node.Tag as OptionPageWinForms;
                page.Dispose();
            }
        }
    }
}


