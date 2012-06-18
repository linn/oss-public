
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;


namespace Linn.Toolkit.WinForms
{
    public class OptionPageWinForms : IDisposable
    {
        public OptionPageWinForms(int aWidth, IOptionPage aOptionPage) 
            : this(aWidth, aOptionPage, null){
        }

        public OptionPageWinForms(int aWidth, IOptionPage aOptionPage, Control aHelpTextControl)
        {
            iOptions = new List<IOptionWinForms>();
            iOptionPage = aOptionPage;
            iControl = new ContainerControl();
            iControl.AutoScroll = true;
            iHelpTextControl = aHelpTextControl;

            // set the width of the page control before any children are added
            iControl.Width = aWidth;

            int labelWidth = aWidth / 2;
            int optionWidth = aWidth - labelWidth;
            int currentY = 0;

            for (int i=0 ; i<iOptionPage.Options.Count ; i++)
            {
                Option option = iOptionPage.Options[i];

                // create the label for the option
                Label labelControl = new Label();
                labelControl.AutoSize = false;
                labelControl.Text = option.Name;
                labelControl.TextAlign = ContentAlignment.MiddleLeft;

                // create the option control
                IOptionWinForms optionWinform = null;

                if (option is OptionEnum || option is OptionNetworkInterface || option is OptionBoolEnum)
                {
                    optionWinform = new OptionWinFormsEnumerated(option);
                }
                else if (option is OptionFilePath)
                {
                    optionWinform = new OptionWinFormsFilePath(option, optionWidth);
                }
                else if (option is OptionFolderPath)
                {
                    optionWinform = new OptionWinFormsFolderPath(option, optionWidth);
                }
                else if (option is OptionFolderName)
                {
                    optionWinform = new OptionWinFormsFolderName(option, optionWidth);
                }
                else if (option is OptionUri)
                {
                    optionWinform = new OptionWinFormsUri(option, optionWidth);
                }
                else if (option is OptionBool)
                {
                    optionWinform = new OptionWinFormsBool(option);
                }
                else if (option is OptionColor)
                {
                    optionWinform = new OptionWinFormsARGB(option);
                }
                else if (option is OptionListFolderPath)
                {
                    optionWinform = new OptionWinFormsListFolderPath(option, optionWidth);
                }
                else if (option is OptionInt)
                {
                    optionWinform = new OptionWinFormsInt(option);
                }
                else if (option is OptionUint) {
                    optionWinform = new OptionWinFormsUint(option);
                }
                else if (option is OptionNumber) {
                    optionWinform = new OptionWinFormsNumber(option);
                }
                else if (option is OptionString) {
                    optionWinform = new OptionWinFormsString(option);
                }

                // calculate positioning of the controls
                if (optionWinform != null)
                {
                    iOptions.Add(optionWinform);
                    Control optionControl = optionWinform.Control;

                    labelControl.Location = new Point(0, currentY);
                    labelControl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    labelControl.TabStop = false;
                    labelControl.Width = labelWidth;
                    labelControl.Height = optionControl.Height;

                    optionControl.Location = new Point(labelWidth, currentY);
                    optionControl.Width = optionWidth;
                    optionControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                    optionControl.TabStop = true;
                    optionControl.TabIndex = i;

                    if (aHelpTextControl != null) {
                        labelControl.Tag = optionControl;
                        labelControl.Click += SelectOptionLabel;
                        optionControl.Tag = labelControl;
                        optionControl.Name = option.Description;
                        optionControl.Enter += SelectOption;
                        optionControl.Leave += DeselectOption;
                    }

                    iControl.Controls.Add(labelControl);
                    iControl.Controls.Add(optionControl);

                    currentY += optionControl.Height;
                }
            }
        }

        private void SelectOptionLabel(object sender, EventArgs e) {
            //Select corresponding option control
            Control optionControl = (Control)((Control)sender).Tag;
            foreach (Control control in optionControl.Controls) {
                if (control.TabStop) {
                    control.Select();
                    return;
                }
            }
            optionControl.Select();
        }

        private void SelectOption(object sender, EventArgs e) {
            //Highlight corresponding label control, show help text
            Control labelControl = (Control)((Control)sender).Tag;
            labelControl.BackColor = SystemColors.Highlight;
            labelControl.ForeColor = SystemColors.HighlightText;
            iHelpTextControl.Text = ((Control)sender).Name;
        }

        private void DeselectOption(object sender, EventArgs e) {
            //Unhighlight corresponding label control
            Control labelControl = (Control)((Control)sender).Tag;
            labelControl.BackColor = SystemColors.Window;
            labelControl.ForeColor = SystemColors.WindowText;
        }

        public IOptionPage OptionPage
        {
            get { return iOptionPage; }
        }

        public Control Control
        {
            get { return iControl; }
        }

        public void Dispose()
        {
            foreach (IOptionWinForms option in iOptions)
            {
                option.Dispose();
            }
        }

        private IOptionPage iOptionPage;
        private ContainerControl iControl;
        private List<IOptionWinForms> iOptions;
        private Control iHelpTextControl;
    }
}


