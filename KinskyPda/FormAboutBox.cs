
using System;
using System.Drawing;
using System.Windows.Forms;

using Linn;


namespace KinskyPda
{
    public partial class FormAboutBox : Form
    {
        public FormAboutBox(IHelper aHelper)
        {
            InitializeComponent();

            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = ViewSupport.Instance.BackColour;

            iPanelLower = new PanelNoBackgroundPaint();
            iPanelLower.BackColor = ViewSupport.Instance.BackColour;
            iPanelLower.BackgroundImage = TextureManager.Instance.BlankControl;
            iPanelLower.Bounds = LayoutManager.Instance.BottomBounds;
            iPanelLower.Parent = this;

            iPanel = new PanelNoBackgroundPaint();
            iPanel.BackColor = ViewSupport.Instance.BackColour;
            iPanel.BackgroundImage = TextureManager.Instance.Background;
            iPanel.Dock = DockStyle.Fill;
            iPanel.Parent = this;

            iTitle = CreateLabel("About " + aHelper.Title, ViewSupport.Instance.FontLarge);
            iProduct = CreateLabel(aHelper.Product, ViewSupport.Instance.FontSmall);
            iVersion = CreateLabel("Version " + aHelper.Version + " (" + aHelper.Family + ")", ViewSupport.Instance.FontSmall);
            iCopyright = CreateLabel(aHelper.Copyright, ViewSupport.Instance.FontSmall);
            iCompany = CreateLabel(aHelper.Company, ViewSupport.Instance.FontSmall);
            iDescription = CreateLabel(aHelper.Description, ViewSupport.Instance.FontSmall);

            int currentY = 0;
            int padding = 10;
            int height = 30;
            int leftMargin = 20;
            iTitle.Bounds = new Rectangle(0, currentY, LayoutManager.Instance.AllBounds.Width - leftMargin, 40);
            currentY += iTitle.Height + padding;

            iProduct.Bounds = new Rectangle(padding, currentY, LayoutManager.Instance.AllBounds.Width - leftMargin - padding, height);
            currentY += iProduct.Height + padding;

            iVersion.Bounds = new Rectangle(padding, currentY, LayoutManager.Instance.AllBounds.Width - leftMargin - padding, height);
            currentY += iVersion.Height + padding;

            iCopyright.Bounds = new Rectangle(padding, currentY, LayoutManager.Instance.AllBounds.Width - leftMargin - padding, height);
            currentY += iCopyright.Height + padding;

            iCompany.Bounds = new Rectangle(padding, currentY, LayoutManager.Instance.AllBounds.Width - leftMargin - padding, height);
            currentY += iCompany.Height + padding;

            iDescription.Bounds = new Rectangle(padding, currentY, LayoutManager.Instance.AllBounds.Width - leftMargin - padding, height);
            currentY += iDescription.Height + padding;

            // Set the text of the left menu item (which does nothing) to a space. If the text is empty, the buttons screw up on
            // older PDAs (Windows 2003) so that the "Done" button cannot be seen meaning that the about box cannot be closed.
            this.iMenuItemLeft.Text = " ";

            this.ResumeLayout();
        }

        private void MenuItemDoneClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private TransparentLabel CreateLabel(string aText, Font aFont)
        {
            TransparentLabel label = new TransparentLabel();
            label.ForeColor = ViewSupport.Instance.ForeColour;
            label.Font = aFont;
            label.Text = aText;
            label.Parent = iPanel;

            return label;
        }

        private PanelNoBackgroundPaint iPanel;
        private PanelNoBackgroundPaint iPanelLower;
        private TransparentLabel iTitle;
        private TransparentLabel iProduct;
        private TransparentLabel iVersion;
        private TransparentLabel iCopyright;
        private TransparentLabel iCompany;
        private TransparentLabel iDescription;
    }
}