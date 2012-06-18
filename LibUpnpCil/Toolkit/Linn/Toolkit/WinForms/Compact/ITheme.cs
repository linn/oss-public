
using System;
using System.Drawing;
using System.Windows.Forms;


namespace Linn.Toolkit.WinForms
{
    public interface ITheme
    {
        void ApplyForm(Form aForm);
        void ApplyContainerControl(ContainerControl aContainerControl);
        void ApplyMainMenu(MainMenu aMainMenu);
        void ApplyMainMenuItem(MenuItem aMenuItem);
        void ApplyTabControl(TabControl aTabControl);
        void ApplyTabPage(TabPage aTabPage);
        void ApplyLabel(Label aLabel);
        void ApplyComboBox(ComboBox aComboBox);
        void ApplyCheckBox(CheckBox aCheckBox);
    }
}


