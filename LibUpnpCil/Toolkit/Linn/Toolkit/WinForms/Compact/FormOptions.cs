using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Linn.Toolkit.WinForms
{
    public partial class FormUserOptions : Form
    {
        public FormUserOptions(IList<IOptionPage> aOptionPages)
        {
            InitializeComponent();

            foreach (IOptionPage optionPage in aOptionPages)
            {
                // create the controls for the options page
                OptionPageWinForms page = new OptionPageWinForms(iTabControl.Width, optionPage);

                // add the tab page
                TabPage tabPage = new TabPage();
                tabPage.Name = optionPage.Name;
                tabPage.Text = optionPage.Name;
                tabPage.Tag = page;
                tabPage.Controls.Add(page.Control);
                iTabControl.TabPages.Add(tabPage);
            }

            if (aOptionPages.Count > 0)
            {
                iTabControl.SelectedIndex = 0;
            }
        }

        public void SetTheme(ITheme aTheme)
        {
            aTheme.ApplyForm(this);
            aTheme.ApplyMainMenu(iMainMenu);
            aTheme.ApplyMainMenuItem(iMenuItemDone);
            aTheme.ApplyMainMenuItem(iMenuItemReset);

            aTheme.ApplyTabControl(iTabControl);
            foreach (TabPage page in iTabControl.TabPages)
            {
                aTheme.ApplyTabPage(page);
                OptionPageWinForms optionPage = page.Tag as OptionPageWinForms;
                optionPage.SetTheme(aTheme);
            }
        }

        public void SetPageByName(string aPageName)
        {
            for (int i = 0; i < iTabControl.TabPages.Count; i++)
            {
                if (iTabControl.TabPages[i].Name == aPageName)
                {
                    iTabControl.SelectedIndex = i;
                }
            }
        }

        private void EventFormClosed(object sender, EventArgs e)
        {
            foreach (TabPage page in iTabControl.TabPages)
            {
                OptionPageWinForms optionPage = page.Tag as OptionPageWinForms;
                optionPage.Dispose();
            }
        }

        private void MenuItemResetClick(object sender, EventArgs e)
        {
            if (iTabControl.SelectedIndex != -1)
            {
                OptionPageWinForms optionPage = iTabControl.TabPages[iTabControl.SelectedIndex].Tag as OptionPageWinForms;
                foreach (Option o in optionPage.OptionPage.Options)
                {
                    o.ResetToDefault();
                }
            }
        }

        private void MenuItemDoneClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}