
using System;
using System.Drawing;
using System.Windows.Forms;

using Linn;
using Linn.Toolkit.WinForms;


namespace KinskyPda
{
    internal class Theme : ITheme
    {
        public void ApplyForm(Form aForm)
        {
            aForm.BackColor = ViewSupport.Instance.BackColour;
            aForm.ForeColor = ViewSupport.Instance.ForeColour;
            aForm.Font = ViewSupport.Instance.FontLarge;
            aForm.WindowState = FormWindowState.Maximized;
        }

        public void ApplyContainerControl(ContainerControl aContainerControl)
        {
            aContainerControl.BackColor = ViewSupport.Instance.BackColour;
        }

        public void ApplyMainMenu(MainMenu aMainMenu)
        {
        }

        public void ApplyMainMenuItem(MenuItem aMenuItem)
        {
        }

        public void ApplyTabControl(TabControl aTabControl)
        {
            aTabControl.Font = ViewSupport.Instance.FontSmall;
        }

        public void ApplyTabPage(TabPage aTabPage)
        {
            aTabPage.BackColor = ViewSupport.Instance.BackColour;
        }

        public void ApplyLabel(Label aLabel)
        {
            aLabel.BackColor = ViewSupport.Instance.BackColour;
            aLabel.ForeColor = ViewSupport.Instance.ForeColour;
            aLabel.Font = ViewSupport.Instance.FontMedium;
        }

        public void ApplyComboBox(ComboBox aComboBox)
        {
            aComboBox.Font = ViewSupport.Instance.FontMedium;
        }

        public void ApplyCheckBox(CheckBox aCheckBox)
        {
        }
    }
}

