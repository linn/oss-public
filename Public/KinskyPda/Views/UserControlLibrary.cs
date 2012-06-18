
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Linn;
using Linn.Kinsky;
using KinskyPda.Widgets;


namespace KinskyPda.Views
{
    internal class UserControlBrowser : UserControl
    {
        public UserControlBrowser()
        {
            // create the breadcrumb widget
            iWidgetBreadcrumb = new WidgetBreadcrumb();
            iWidgetBreadcrumb.Bounds = LayoutManager.Instance.TopBounds;
            iWidgetBreadcrumb.BackColor = ViewSupport.Instance.BackColour;
            iWidgetBreadcrumb.Font = ViewSupport.Instance.FontLarge;
            iWidgetBreadcrumb.ForeColor = ViewSupport.Instance.ForeColourBright;
            iWidgetBreadcrumb.Dock = DockStyle.Top;

            // create the main browser panel
            iWidgetBrowser = new Panel();
            iWidgetBrowser.Bounds = LayoutManager.Instance.CentreBounds;
            iWidgetBrowser.Dock = DockStyle.Fill;

            // create the background panel for the buttons
            iPanelButtons = new PanelNoBackgroundPaint();
            iPanelButtons.BackgroundImage = TextureManager.Instance.BlankControl;
            iPanelButtons.BackColor = ViewSupport.Instance.BackColour;
            iPanelButtons.Bounds = LayoutManager.Instance.BottomBounds;
            iPanelButtons.Dock = DockStyle.Bottom;

            // create the parent control for the buttons
            iButtonParent = new TransparentControlBase();
            iButtonParent.Bounds = LayoutManager.Instance.PanelToolsBounds;
            iButtonParent.Parent = iPanelButtons;

            // create the "up" button
            iButtonUp = ImageButtonFactory.CreateLeft(TextureManager.Instance.UpDirectoryTouch,
                                                      TextureManager.Instance.UpDirectory);
            iButtonUp.Enabled = false;
            iButtonUp.Parent = iButtonParent;
            iButtonUp.Click += ButtonUpClick;

            // create the "play now" button
            iButtonPlayNow = ImageButtonFactory.CreateCentre(TextureManager.Instance.InsertPlayTouch,
                                                             TextureManager.Instance.InsertPlay);
            iButtonPlayNow.Enabled = false;
            iButtonPlayNow.Parent = iButtonParent;

            // create the "play later" button
            iButtonPlayLater = ImageButtonFactory.CreateRight(TextureManager.Instance.InsertQueueTouch,
                                                              TextureManager.Instance.InsertQueue);
            iButtonPlayLater.Enabled = false;
            iButtonPlayLater.Parent = iButtonParent;

            this.SuspendLayout();

            // set some parameters for the parent view control
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;

            // add the widgets to the parent control - the iWidgetBrowser needs to be
            // added first otherwise it will appear underneath the other widgets - adding
            // it first means that it appears of the correct size
            iWidgetBrowser.Parent = this;
            iWidgetBreadcrumb.Parent = this;
            iPanelButtons.Parent = this;

            this.ResumeLayout(false);

            // setup some events
            iWidgetBreadcrumb.EventLocationChanged += BreadcrumbLocationChanged;
            GotFocus += EventGotFocus;
	    }

        public IControllerLocation ControllerLocation
        {
            set { iControllerLocation = value; }
        }

        public ContextMenu PContextMenu
        {
            get { return iViewBrowser.ContextMenu; }
        }

        public WidgetBreadcrumb ViewWidgetBreadcrumb
        {
            get { return iWidgetBreadcrumb; }
        }

        public ImageButton ButtonPlayNow
        {
            get { return iButtonPlayNow; }
        }

        public ImageButton ButtonPlayLater
        {
            get { return iButtonPlayLater; }
        }

        public void SetViewBrowser(ViewBrowser aViewBrowser)
        {
            if (iViewBrowser != null)
                iViewBrowser.Close();

            iViewBrowser = aViewBrowser;
            this.SuspendLayout();
            iWidgetBrowser.Controls.Clear();
            iViewBrowser.SetParent(iWidgetBrowser);
            this.ResumeLayout();
            iViewBrowser.Open();
        }

        public void Load(KinskyPda.PlaylistSupport aPlaylistSupport)
        {
            aPlaylistSupport.EventIsInsertAllowedChanged += new EventHandler<EventArgs>(PlaylistSupportIsInsertAllowedChanged);

            SetInsertAllowed(aPlaylistSupport.IsInsertAllowed());
        }

        private void EventGotFocus(object sender, EventArgs e)
        {
            if (Focused)
            {
                iViewBrowser.Focus();
            }
        }

        private void SetInsertAllowed(bool aIsInsertAllowed)
        {
            iButtonPlayLater.Enabled = aIsInsertAllowed;
            iButtonPlayNow.Enabled = aIsInsertAllowed;
        }

        private void PlaylistSupportIsInsertAllowedChanged(object sender, EventArgs e)
        {
            PlaylistSupport support = (PlaylistSupport)sender;
            SetInsertAllowed(support.IsInsertAllowed());
        }

        private void BreadcrumbLocationChanged(object sender, EventArgs e)
        {
            Assert.Check(sender == iWidgetBreadcrumb);

            // location in the breadcrumb widget has changed
            iButtonPlayLater.Enabled = false;
            iButtonPlayNow.Enabled = false;
            iButtonUp.Enabled = (!iWidgetBreadcrumb.IsHome);
        }

        private void ButtonUpClick(object sender, EventArgs e)
        {
            if (iControllerLocation != null)
                iControllerLocation.Up(1);
        }

        private WidgetBreadcrumb iWidgetBreadcrumb;
        private Panel iWidgetBrowser;
        private PanelNoBackgroundPaint iPanelButtons;
        private TransparentControlBase iButtonParent;
        private ImageButton iButtonUp;
        private ImageButton iButtonPlayNow;
        private ImageButton iButtonPlayLater;
        private ViewBrowser iViewBrowser;
        private IControllerLocation iControllerLocation;
    }
}
