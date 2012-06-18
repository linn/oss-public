
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace Linn.Toolkit.WinForms
{
    public class OptionPageWinForms : IDisposable
    {
        public OptionPageWinForms(int aWidth, IOptionPage aOptionPage)
        {
            iLabels = new List<Label>();
            iOptions = new List<IOptionWinForms>();
            iOptionPage = aOptionPage;
            iControl = new ContainerControl();
            iControl.AutoScroll = true;

            // set the width of the page control before any children are added
            iControl.Width = aWidth;
            iControl.Dock = DockStyle.Fill;

            for (int i = 0; i < iOptionPage.Options.Count; i++)
            {
                Option option = iOptionPage.Options[i];

                // create the option control
                IOptionWinForms optionWinform = null;

                if (option is OptionEnum || option is OptionNetworkInterface)
                {
                    optionWinform = new OptionWinFormsEnumerated(option);
                }
                else if (option is OptionBool)
                {
                    optionWinform = new OptionWinFormsBool(option);
                }
                else if (option is OptionFilePath ||
                         option is OptionFolderPath ||
                         option is OptionColor ||
                         option is OptionListFolderPath)
                {
                    throw new NotImplementedException();
                }

                if (optionWinform != null)
                {
                    // create the label control
                    Label labelControl = new Label();
                    labelControl.Text = option.Name + ":";
                    labelControl.TextAlign = ContentAlignment.TopLeft;

                    iControl.Controls.Add(labelControl);
                    iControl.Controls.Add(optionWinform.Control);

                    iLabels.Add(labelControl);
                    iOptions.Add(optionWinform);
                }
            }

            // now update the layout of all controls
            UpdateLayout();
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

        public void SetTheme(ITheme aTheme)
        {
            aTheme.ApplyContainerControl(iControl);
            foreach (Label label in iLabels)
            {
                aTheme.ApplyLabel(label);
            }
            foreach (IOptionWinForms option in iOptions)
            {
                option.SetTheme(aTheme);
            }
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            // create a graphics object for measuring height of labels
            Graphics graphics = iControl.CreateGraphics();
            iControl.SuspendLayout();

            int width = iControl.Width - iControl.AutoScrollMargin.Width;
            int currentY = 0;

            for (int i = 0; i < iLabels.Count; i++)
            {
                Label label = iLabels[i];
                IOptionWinForms option = iOptions[i];

                const int leftMargin = 20;

                // calculate label text height
                SizeF sz = graphics.MeasureString(label.Text, label.Font);
                const int labelPadding = 10;
                label.Location = new Point(0, currentY + labelPadding);
                label.Width = width - leftMargin;
                label.Height = (int)sz.Height;

                // update height pointer
                currentY += label.Height + 2 * labelPadding;

                // set position of the option control
                const int optionPadding = 20;
                option.Control.Location = new Point(optionPadding, currentY);
                option.Control.Width = width - optionPadding - leftMargin;

                // update height pointer
                currentY += option.Control.Height;
            }

            iControl.ResumeLayout();
            graphics.Dispose();
        }

        private IOptionPage iOptionPage;
        private ContainerControl iControl;
        private List<Label> iLabels;
        private List<IOptionWinForms> iOptions;
    }
}


