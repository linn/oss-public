
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace Linn.Toolkit.WinForms
{
    public interface IOptionWinForms : IDisposable
    {
        Control Control { get; }
    }

    public class OptionWinFormsEnumerated : IOptionWinForms
    {
        public OptionWinFormsEnumerated(Option aOption)
        {
            iOption = aOption;
            iOption.EventAllowedChanged += AllowedValuesChanged;
            iOption.EventValueChanged += ValueChanged;
            iOption.EventEnabledChanged += EnabledChanged;

            iComboBox = new ComboBox();
            iComboBox.Name = "iComboBox";
            iComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            iComboBox.MaxDropDownItems = 10;

            foreach (string value in iOption.Allowed)
            {
                iComboBox.Items.Add(value);
            }
            iComboBox.SelectedItem = iOption.Value;
            iComboBox.Enabled = iOption.Enabled;

            iComboBox.SelectedIndexChanged += SelectedIndexChanged;
        }

        public Control Control
        {
            get { return iComboBox; }
        }

        public void Dispose()
        {
            iOption.EventAllowedChanged -= AllowedValuesChanged;
            iOption.EventValueChanged -= ValueChanged;
            iOption.EventEnabledChanged -= EnabledChanged;
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

        private void EnabledChanged(object sender, EventArgs e) {
            if (iComboBox.InvokeRequired) {
                iComboBox.Invoke(new EventHandler(EnabledChanged), new object[] { sender, e });
                return;
            }

            iComboBox.Enabled = iOption.Enabled;
        }

        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            iOption.Set(iComboBox.SelectedItem.ToString());
        }

        private Option iOption;
        private ComboBox iComboBox;
    }


    public abstract class OptionWinFormsPath : IOptionWinForms
    {
        protected OptionWinFormsPath(Option aOption, int aWidth)
        {
            iOption = aOption;
            iOption.EventValueChanged += ValueChanged;
            iOption.EventEnabledChanged += EnabledChanged;

            iControl = new UserControl();

            int buttonWidth = 50;
            int textBoxWidth = aWidth - buttonWidth;

            iTextBox = new TextBox();
            iTextBox.Text = iOption.Value;
            iTextBox.Enabled = iOption.Enabled;
            iTextBox.ReadOnly = true;
            iTextBox.Width = textBoxWidth;
            iTextBox.Location = new Point(0, 0);
            iTextBox.TabStop = false;
            iTextBox.MouseEnter += TextBoxMouseEnter;
            iTextBox.MouseLeave += TextBoxMouseLeave;
            iControl.Controls.Add(iTextBox);

            iToolTip = new ToolTip();

            iButton = new Button();
            iButton.BackColor = SystemColors.ButtonFace;
            iButton.ForeColor = SystemColors.ControlText;
            iButton.Text = "...";
            iButton.AutoSize = false;
            iButton.Width = buttonWidth;
            iButton.Height = iTextBox.Height;
            iButton.Location = new Point(textBoxWidth, 0);
            iButton.TabStop = true;
            iButton.TabIndex = 0;
            iButton.Enabled = iOption.Enabled;
            iButton.Click += ButtonClick;
            iControl.Controls.Add(iButton);

            iControl.Width = aWidth;
            iControl.Height = iTextBox.Height;
        }

        public Control Control
        {
            get { return iControl; }
        }

        public void Dispose()
        {
            iOption.EventValueChanged -= ValueChanged;
            iOption.EventEnabledChanged -= EnabledChanged;
        }

        protected abstract void ButtonClick(object sender, EventArgs e);

        private void ValueChanged(object sender, EventArgs e)
        {
            if (iTextBox.InvokeRequired)
            {
                iTextBox.Invoke(new EventHandler(ValueChanged), new object[] { sender, e });
                return;
            }

            iTextBox.Text = iOption.Value;
        }

        private void EnabledChanged(object sender, EventArgs e) {
            if (iTextBox.InvokeRequired) {
                iTextBox.Invoke(new EventHandler(EnabledChanged), new object[] { sender, e });
                return;
            }

            iTextBox.Enabled = iOption.Enabled;
            iButton.Enabled = iOption.Enabled;
        }

        private void TextBoxMouseEnter(object sender, EventArgs e)
        {
            iToolTip.Show(iTextBox.Text, iTextBox);
        }

        private void TextBoxMouseLeave(object sender, EventArgs e)
        {
            iToolTip.Hide(iTextBox);
        }

        protected Option iOption;
        private UserControl iControl;
        private TextBox iTextBox;
        private ToolTip iToolTip;
        private Button iButton;
    }


    public class OptionWinFormsFilePath : OptionWinFormsPath
    {
        public OptionWinFormsFilePath(Option aOption, int aWidth)
            : base(aOption, aWidth)
        {
        }

        protected override void ButtonClick(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = iOption.Value;
            dlg.InitialDirectory = iOption.Value;
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                iOption.Set(dlg.FileName);
            }
        }
    }


    public class OptionWinFormsFolderPath : OptionWinFormsPath
    {
        public OptionWinFormsFolderPath(Option aOption, int aWidth)
            : base(aOption, aWidth)
        {
        }

        protected override void ButtonClick(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = iOption.Value;
            dlg.ShowNewFolderButton = true;
            dlg.Description = iOption.Description;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                iOption.Set(dlg.SelectedPath);
            }
        }
    }

    public class OptionWinFormsFolderName : OptionWinFormsPath
    {
        public OptionWinFormsFolderName(Option aOption, int aWidth)
            : base(aOption, aWidth) {
        }

        protected override void ButtonClick(object sender, EventArgs e) {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = iOption.Value;
            dlg.ShowNewFolderButton = false;
            dlg.Description = iOption.Description;
            if (dlg.ShowDialog() == DialogResult.OK) {
                iOption.Set(System.IO.Path.GetFileName(dlg.SelectedPath));
            }
        }
    }


    public class OptionWinFormsBool : IOptionWinForms
    {
        public OptionWinFormsBool(Option aOption)
        {
            iOption = aOption as OptionBool;
            iOption.EventValueChanged += ValueChanged;
            iOption.EventEnabledChanged += EnabledChanged;

            iCheckBox = new CheckBox();
            iCheckBox.Text = "";
            iCheckBox.Checked = iOption.Native;
            iCheckBox.Enabled = iOption.Enabled;
            iCheckBox.CheckedChanged += CheckBoxChanged;
        }

        public Control Control
        {
            get { return iCheckBox; }
        }

        public void Dispose()
        {
            iOption.EventValueChanged -= ValueChanged;
            iOption.EventEnabledChanged -= EnabledChanged;
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

        private void EnabledChanged(object sender, EventArgs e) {
            if (iCheckBox.InvokeRequired) {
                iCheckBox.Invoke(new EventHandler(EnabledChanged), new object[] { sender, e });
                return;
            }

            iCheckBox.Enabled = iOption.Enabled;
        }

        private void CheckBoxChanged(object sender, EventArgs e)
        {
            iOption.Native = iCheckBox.Checked;
        }

        private OptionBool iOption;
        private CheckBox iCheckBox;
    }


    public class OptionWinFormsARGB : IOptionWinForms
    {
        public OptionWinFormsARGB(Option aOption)
        {
            iOption = aOption;
            iOption.EventValueChanged += ValueChanged;
            iOption.EventEnabledChanged += EnabledChanged;

            iButton = new ButtonColour();
            iButton.BackColor = SystemColors.Control;
            iButton.Colour = ToColor();
            iButton.Enabled = iOption.Enabled;
            iButton.Click += ButtonClick;
            iButton.AutoSize = true;
            iButton.Size = new Size(93, 23);
            iButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            iButton.UseVisualStyleBackColor = true;

            // wrapping the button in a UserControl and anchoring it to the left
            // and top edges of the parent prevents the button from being resizes
            // when the parent control is stretched to fit the option page
            iControl = new UserControl();
            iControl.Controls.Add(iButton);
            iControl.Width = iButton.Width;
            iControl.Height = iButton.Height;
        }

        public Control Control
        {
            get { return iControl; }
        }

        public void Dispose()
        {
            iOption.EventValueChanged -= ValueChanged;
            iOption.EventEnabledChanged -= EnabledChanged;
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            if (iButton.InvokeRequired)
            {
                iButton.Invoke(new EventHandler(ValueChanged), new object[] { sender, e });
                return;
            }

            iButton.Colour = ToColor();
        }

        private void EnabledChanged(object sender, EventArgs e) {
            if (iButton.InvokeRequired) {
                iButton.Invoke(new EventHandler(EnabledChanged), new object[] { sender, e });
                return;
            }

            iButton.Enabled = iOption.Enabled;
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = iButton.Colour;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                iOption.Set(ToARGB(dlg.Color));
            }
        }

        private Color ToColor()
        {
            return (Color.FromArgb(int.Parse(iOption.Value))); 
        }

        private string ToARGB(Color aColor)
        {
            return (aColor.ToArgb().ToString());
        }

        private Option iOption;
        private ButtonColour iButton;
        private UserControl iControl;
    }


    internal class ButtonColour : Button
    {
        public ButtonColour()
        {
            iSolidBrush = new SolidBrush(Color.Gray);
        }

        public Color Colour
        {
            get
            {
                return iSolidBrush.Color;
            }
            set
            {
                iSolidBrush.Dispose();
                iSolidBrush = new SolidBrush(value);
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            Graphics g = pevent.Graphics;

            Rectangle r = ClientRectangle;

            byte border = 4;
            byte right_border = 15;

            Rectangle rc = new Rectangle(r.Left + border, r.Top + border, r.Width - border - right_border - 1, r.Height - border * 2 - 1);

            g.FillRectangle(iSolidBrush, rc);

            g.DrawRectangle(Pens.Black, rc);

            //draw the arrow
            Point p1 = new Point(r.Width - 9, r.Height / 2 - 1);
            Point p2 = new Point(r.Width - 5, r.Height / 2 - 1);
            g.DrawLine(Pens.Black, p1, p2);

            p1 = new Point(r.Width - 8, r.Height / 2);
            p2 = new Point(r.Width - 6, r.Height / 2);
            g.DrawLine(Pens.Black, p1, p2);

            p1 = new Point(r.Width - 7, r.Height / 2);
            p2 = new Point(r.Width - 7, r.Height / 2 + 1);
            g.DrawLine(Pens.Black, p1, p2);

            //draw the divider line
            p1 = new Point(r.Width - 12, 4);
            p2 = new Point(r.Width - 12, r.Height - 5);
            g.DrawLine(SystemPens.ControlDark, p1, p2);

            p1 = new Point(r.Width - 11, 4);
            p2 = new Point(r.Width - 11, r.Height - 5);
            g.DrawLine(SystemPens.ControlLightLight, p1, p2);
        }

        private SolidBrush iSolidBrush;
    }


    public abstract class OptionWinFormsList : IOptionWinForms
    {
        protected OptionWinFormsList(Option aOption, int aWidth)
        {
            iOption = aOption;
            iList = new List<string>(StringListConverter.StringToList(iOption.Value));
            iOption.EventValueChanged += ValueChanged;
            iOption.EventEnabledChanged += EnabledChanged;

            iControl = new UserControl();
            iControl.Width = aWidth;

            int buttonWidth = 30;
            int buttonMargin = 10;
            int listWidth = aWidth - buttonWidth - buttonMargin*2;

            iListFolders = new ListView();
            iListFolders.Width = listWidth;
            iListFolders.Location = new Point(0, 0);
            iListFolders.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            iListFolders.TabStop = true;
            iListFolders.TabIndex = 0;
            iListFolders.MultiSelect = true;
            iListFolders.View = View.List;
            iListFolders.SelectedIndexChanged += ListFoldersSelectedIndexChanged;
            iListFolders.Enabled = iOption.Enabled;
            iControl.Controls.Add(iListFolders);

            iButtonAdd = new Button();
            iButtonAdd.BackColor = SystemColors.ButtonFace;
            iButtonAdd.ForeColor = SystemColors.ControlText;
            iButtonAdd.Text += "+";
            iButtonAdd.AutoSize = false;
            iButtonAdd.Width = buttonWidth;
            iButtonAdd.Height = buttonWidth;
            iButtonAdd.Location = new Point(listWidth + buttonMargin, 0);
            iButtonAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            iButtonAdd.TabStop = true;
            iButtonAdd.TabIndex = 1;
            iButtonAdd.Enabled = iOption.Enabled;
            iButtonAdd.Click += ButtonAddClick;
            iControl.Controls.Add(iButtonAdd);

            iButtonRemove = new Button();
            iButtonRemove.BackColor = SystemColors.ButtonFace;
            iButtonRemove.ForeColor = SystemColors.ControlText;
            iButtonRemove.Text += "-";
            iButtonRemove.AutoSize = false;
            iButtonRemove.Width = buttonWidth;
            iButtonRemove.Height = buttonWidth;
            iButtonRemove.Location = new Point(listWidth + buttonMargin, buttonWidth + buttonMargin);
            iButtonRemove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            iButtonRemove.TabStop = true;
            iButtonRemove.TabIndex = 1;
            iButtonRemove.Enabled = iOption.Enabled;
            iButtonRemove.Click += ButtonRemoveClick;
            iControl.Controls.Add(iButtonRemove);

            iControl.Height = iListFolders.Height;

            Populate();
        }

        public Control Control
        {
            get { return iControl; }
        }

        public void Dispose()
        {
            iOption.EventValueChanged -= ValueChanged;
            iOption.EventEnabledChanged -= EnabledChanged;
        }

        protected abstract void ButtonAddClick(object sender, EventArgs e);

        private void ButtonRemoveClick(object sender, EventArgs e)
        {
            // just remove the values from the option list and let the eventing
            // update the view

            int offset = 0;

            foreach (int index in iListFolders.SelectedIndices)
            {
                iList.RemoveAt(index + offset--);
            }

            iOption.Set(StringListConverter.ListToString(iList));
        }

        private void ListFoldersSelectedIndexChanged(object sender, EventArgs e)
        {
            iButtonRemove.Enabled = iListFolders.SelectedIndices.Count > 0;
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            if (iListFolders.InvokeRequired)
            {
                iListFolders.Invoke(new EventHandler(ValueChanged), new object[] { sender, e});
                return;
            }

            Populate();
        }

        private void EnabledChanged(object sender, EventArgs e) {
            if (iListFolders.InvokeRequired) {
                iListFolders.Invoke(new EventHandler(EnabledChanged), new object[] { sender, e });
                return;
            }

            iListFolders.Enabled = iOption.Enabled;
            iButtonAdd.Enabled = iOption.Enabled;
            iButtonRemove.Enabled = iOption.Enabled;
        }

        private void Populate()
        {
            iList = new List<string>(StringListConverter.StringToList(iOption.Value));
            iListFolders.Items.Clear();
            foreach (string val in iList)
            {
                iListFolders.Items.Add(new ListViewItem(val));
            }
        }

        protected void Add(string aValue)
        {
            iList.Add(aValue);
            iOption.Set(StringListConverter.ListToString(iList));
        }

        protected string Description
        {
            get
            {
                return (iOption.Description);
            }
        }

        private Option iOption;
        private List<string> iList;
        private UserControl iControl;
        private ListView iListFolders;
        private Button iButtonAdd;
        private Button iButtonRemove;
    }


    public class OptionWinFormsListFolderPath : OptionWinFormsList
    {
        public OptionWinFormsListFolderPath(Option aOption, int aWidth)
            : base(aOption, aWidth)
        {
        }

        protected override void ButtonAddClick(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            dlg.Description = Description;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Add(dlg.SelectedPath);
            }
        }
    }

    public class OptionWinFormsInt : IOptionWinForms
    {
        public OptionWinFormsInt(Option aOption) {
            iOption = aOption as OptionInt;
            iOption.EventValueChanged += ValueChanged;
            iOption.EventEnabledChanged += EnabledChanged;

            iTextBox = new TextBox();
            iTextBox.Text = iOption.Value;
            iTextBox.Enabled = iOption.Enabled;
            iTextBox.TextChanged += TextBoxChanged;
        }

        public Control Control {
            get { return iTextBox; }
        }

        public void Dispose() {
            iOption.EventValueChanged -= ValueChanged;
            iOption.EventEnabledChanged -= EnabledChanged;
        }

        private void ValueChanged(object sender, EventArgs e) {
            if (iTextBox.InvokeRequired) {
                iTextBox.Invoke(new EventHandler(ValueChanged), new object[] { sender, e });
                return;
            }
            iTextBox.Text = iOption.Value;
        }

        private void EnabledChanged(object sender, EventArgs e) {
            if (iTextBox.InvokeRequired) {
                iTextBox.Invoke(new EventHandler(EnabledChanged), new object[] { sender, e });
                return;
            }

            iTextBox.Enabled = iOption.Enabled;
        }

        private void TextBoxChanged(object sender, EventArgs e) {
            iOption.Set(iTextBox.Text);
        }

        private OptionInt iOption;
        private TextBox iTextBox;
    }

    public class OptionWinFormsUint : IOptionWinForms
    {
        public OptionWinFormsUint(Option aOption) {
            iOption = aOption as OptionUint;
            iOption.EventValueChanged += ValueChanged;
            iOption.EventEnabledChanged += EnabledChanged;

            iTextBox = new TextBox();
            iTextBox.Text = iOption.Value;
            iTextBox.Enabled = iOption.Enabled;
            iTextBox.TextChanged += TextBoxChanged;
        }

        public Control Control {
            get { return iTextBox; }
        }

        public void Dispose() {
            iOption.EventValueChanged -= ValueChanged;
            iOption.EventEnabledChanged -= EnabledChanged;
        }

        private void ValueChanged(object sender, EventArgs e) {
            if (iTextBox.InvokeRequired) {
                iTextBox.Invoke(new EventHandler(ValueChanged), new object[] { sender, e });
                return;
            }
            iTextBox.Text = iOption.Value;
        }

        private void EnabledChanged(object sender, EventArgs e) {
            if (iTextBox.InvokeRequired) {
                iTextBox.Invoke(new EventHandler(EnabledChanged), new object[] { sender, e });
                return;
            }

            iTextBox.Enabled = iOption.Enabled;
        }

        private void TextBoxChanged(object sender, EventArgs e) {
            iOption.Set(iTextBox.Text);
        }

        private OptionUint iOption;
        private TextBox iTextBox;
    }

    public class OptionWinFormsNumber : IOptionWinForms
    {
        public OptionWinFormsNumber(Option aOption) {
            iOption = aOption as OptionNumber;
            iOption.EventValueChanged += ValueChanged;
            iOption.EventEnabledChanged += EnabledChanged;

            iNumberBox = new NumericUpDown();

            string attribute = null;
            iOption.Attributes.TryGetValue("Min", out attribute);
            int min = 0;
            int.TryParse(attribute, out min);
            iOption.Attributes.TryGetValue("Max", out attribute);
            int max = 0;
            int.TryParse(attribute, out max);
            iNumberBox.Minimum = min;
            iNumberBox.Maximum = max;

            iNumberBox.Value = iOption.Native;
            iNumberBox.Enabled = iOption.Enabled;
            iNumberBox.ValueChanged += NumberBoxChanged;
        }

        public Control Control {
            get { return iNumberBox; }
        }

        public void Dispose() {
            iOption.EventValueChanged -= ValueChanged;
            iOption.EventEnabledChanged -= EnabledChanged;
        }

        private void ValueChanged(object sender, EventArgs e) {
            if (iNumberBox.InvokeRequired) {
                iNumberBox.Invoke(new EventHandler(ValueChanged), new object[] { sender, e });
                return;
            }
            iNumberBox.Value = iOption.Native;
        }

        private void EnabledChanged(object sender, EventArgs e) {
            if (iNumberBox.InvokeRequired) {
                iNumberBox.Invoke(new EventHandler(EnabledChanged), new object[] { sender, e });
                return;
            }

            iNumberBox.Enabled = iOption.Enabled;
        }

        private void NumberBoxChanged(object sender, EventArgs e) {
            iOption.Set(iNumberBox.Value.ToString());
        }

        private OptionNumber iOption;
        private NumericUpDown iNumberBox;
    }

    public class OptionWinFormsString : IOptionWinForms
    {
        public OptionWinFormsString(Option aOption) {
            iOption = aOption as OptionString;
            iOption.EventValueChanged += ValueChanged;
            iOption.EventEnabledChanged += EnabledChanged;

            iTextBox = new TextBox();
            iTextBox.Text = iOption.Value;
            iTextBox.Enabled = iOption.Enabled;
            iTextBox.TextChanged += TextBoxChanged;
        }

        public Control Control {
            get { return iTextBox; }
        }

        public void Dispose() {
            iOption.EventValueChanged -= ValueChanged;
            iOption.EventEnabledChanged -= EnabledChanged;
        }

        private void ValueChanged(object sender, EventArgs e) {
            if (iTextBox.InvokeRequired) {
                iTextBox.Invoke(new EventHandler(ValueChanged), new object[] { sender, e });
                return;
            }
            iTextBox.Text = iOption.Value;
        }

        private void EnabledChanged(object sender, EventArgs e) {
            if (iTextBox.InvokeRequired) {
                iTextBox.Invoke(new EventHandler(EnabledChanged), new object[] { sender, e });
                return;
            }

            iTextBox.Enabled = iOption.Enabled;
        }

        private void TextBoxChanged(object sender, EventArgs e) {
            iOption.Set(iTextBox.Text);
        }

        private OptionString iOption;
        private TextBox iTextBox;
    }

    public class OptionWinFormsUri : IOptionWinForms
    {
        public OptionWinFormsUri(Option aOption, int aWidth) {
            iOption = aOption as OptionUri;
            iOption.EventValueChanged += ValueChanged;
            iOption.EventEnabledChanged += EnabledChanged;

            string attribute = null;
            iOption.Attributes.TryGetValue("TestButton", out attribute);
            bool.TryParse(attribute, out iIncludeTestButton);

            iTextBox = new TextBox();
            iTextBox.Text = iOption.Value;
            iTextBox.Enabled = iOption.Enabled;
            iTextBox.TextChanged += TextBoxChanged;

            if (iIncludeTestButton) {
                iControl = new UserControl();

                int buttonWidth = 50;
                int textBoxWidth = aWidth - buttonWidth;

                iTextBox.Width = textBoxWidth;
                iTextBox.Location = new Point(0, 0);
                iControl.Controls.Add(iTextBox);

                iButton = new Button();
                iButton.BackColor = SystemColors.ButtonFace;
                iButton.ForeColor = SystemColors.ControlText;
                iButton.Text = "Test";
                iButton.AutoSize = false;
                iButton.Width = buttonWidth;
                iButton.Height = iTextBox.Height;
                iButton.Location = new Point(textBoxWidth, 0);
                iButton.TabStop = true;
                iButton.TabIndex = 0;
                iButton.Enabled = iOption.Enabled;
                iButton.Click += ButtonClick;
                iControl.Controls.Add(iButton);

                iControl.Width = aWidth;
                iControl.Height = iTextBox.Height;
            }
        }

        public Control Control {
            get {
                if (iIncludeTestButton) {
                    return iControl;
                }
                else {
                    return iTextBox;
                }
            }
        }

        public void Dispose() {
            iOption.EventValueChanged -= ValueChanged;
            iOption.EventEnabledChanged -= EnabledChanged;
        }

        private void ButtonClick(object sender, EventArgs e) {
            try {
                System.Diagnostics.Process.Start(iTextBox.Text);
            }
            catch (Exception exc) {
                MessageBox.Show("Could not open the URL in a browser:" + Environment.NewLine + exc.Message, "Invlaid URL", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void ValueChanged(object sender, EventArgs e) {
            if (iTextBox.InvokeRequired) {
                iTextBox.Invoke(new EventHandler(ValueChanged), new object[] { sender, e });
                return;
            }
            iTextBox.Text = iOption.Value;
        }

        private void EnabledChanged(object sender, EventArgs e) {
            if (iTextBox.InvokeRequired) {
                iTextBox.Invoke(new EventHandler(EnabledChanged), new object[] { sender, e });
                return;
            }

            iTextBox.Enabled = iOption.Enabled;
            if (iIncludeTestButton) {
                iButton.Enabled = iOption.Enabled;
            }
        }

        private void TextBoxChanged(object sender, EventArgs e) {
            try {
                Uri uri = new Uri(iTextBox.Text);
                if (uri.Scheme != Uri.UriSchemeHttp) {
                    throw new System.FormatException();
                }
                iTextBox.ForeColor = Color.Black;
                iOption.Set(iTextBox.Text);
            }
            catch {
                iTextBox.ForeColor = Color.Red;
            }
        }

        private OptionUri iOption;
        private UserControl iControl;
        private TextBox iTextBox;
        private Button iButton;
        private bool iIncludeTestButton = false;
    }
}


