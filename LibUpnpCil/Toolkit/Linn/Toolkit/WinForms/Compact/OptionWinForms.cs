
using System;
using System.Drawing;
using System.Windows.Forms;


namespace Linn.Toolkit.WinForms
{
    public interface IOptionWinForms : IDisposable
    {
        Control Control { get; }
        void SetTheme(ITheme aTheme);
    }


    public class OptionWinFormsEnumerated : IOptionWinForms
    {
        public OptionWinFormsEnumerated(Option aOption)
        {
            iOption = aOption;
            iOption.EventAllowedChanged += AllowedValuesChanged;
            iOption.EventValueChanged += ValueChanged;

            iComboBox = new ComboBox();
            iComboBox.Name = "iComboBox";
            iComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            foreach (string value in iOption.Allowed)
            {
                iComboBox.Items.Add(value);
            }
            iComboBox.SelectedItem = iOption.Value;

            iComboBox.SelectedIndexChanged += SelectedIndexChanged;
        }

        public Control Control
        {
            get { return iComboBox; }
        }

        public void SetTheme(ITheme aTheme)
        {
            aTheme.ApplyComboBox(iComboBox);
        }

        public void Dispose()
        {
            iOption.EventAllowedChanged -= AllowedValuesChanged;
            iOption.EventValueChanged -= ValueChanged;
        }

        private void AllowedValuesChanged(object sender, EventArgs e)
        {
            if (iComboBox.InvokeRequired)
            {
                iComboBox.Invoke(new EventHandler(AllowedValuesChanged), new object[] { sender, e });
                return;
            }

            iComboBox.Items.Clear();
            foreach (string value in iOption.Allowed)
            {
                iComboBox.Items.Add(value);
            }
            ValueChanged(this, EventArgs.Empty);
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            if (iComboBox.InvokeRequired)
            {
                iComboBox.Invoke(new EventHandler(ValueChanged), new object[] { sender, e });
                return;
            }

            if (iComboBox.SelectedItem == null ||
                iComboBox.SelectedItem.ToString() != iOption.Value)
            {
                iComboBox.SelectedItem = iOption.Value;
            }
        }

        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            iOption.Set(iComboBox.SelectedItem.ToString());
        }

        private Option iOption;
        private ComboBox iComboBox;
    }


    public class OptionWinFormsBool : IOptionWinForms
    {
        public OptionWinFormsBool(Option aOption)
        {
            iOption = aOption as OptionBool;
            iOption.EventValueChanged += ValueChanged;

            iCheckBox = new CheckBox();
            iCheckBox.Text = "";
            iCheckBox.Checked = iOption.Native;
            iCheckBox.CheckStateChanged += CheckBoxChanged;
            iCheckBox.Height = 40;
        }

        public Control Control
        {
            get { return iCheckBox; }
        }

        public void SetTheme(ITheme aTheme)
        {
            aTheme.ApplyCheckBox(iCheckBox);
        }

        public void Dispose()
        {
            iOption.EventValueChanged -= ValueChanged;
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            if (iCheckBox.InvokeRequired)
            {
                iCheckBox.Invoke(new EventHandler(ValueChanged), new object[] { sender, e });
                return;
            }

            iCheckBox.Checked = iOption.Native;
        }

        private void CheckBoxChanged(object sender, EventArgs e)
        {
            iOption.Native = iCheckBox.Checked;
        }

        private OptionBool iOption;
        private CheckBox iCheckBox;
    }
}


