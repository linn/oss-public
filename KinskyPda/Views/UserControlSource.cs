
using System;
using System.Drawing;
using System.Windows.Forms;

using Linn.Kinsky;
using KinskyPda.Widgets;

namespace KinskyPda.Views
{
    public class UserControlSource : UserControl
    {
        public UserControlSource()
        {
            // create the room selector
            iWidgetSelectorRoom = new WidgetSelectorRoom();
            iWidgetSelectorRoom.BackColor = ViewSupport.Instance.BackColour;
            iWidgetSelectorRoom.Font = ViewSupport.Instance.FontLarge;
            iWidgetSelectorRoom.ForeColor = ViewSupport.Instance.ForeColourBright;
            iWidgetSelectorRoom.Bounds = LayoutManager.Instance.TopBounds;
            iWidgetSelectorRoom.Dock = DockStyle.Top;

            // create the source selector
            iWidgetSelectorSource = new WidgetSelectorSource();
            iWidgetSelectorSource.BackColor = ViewSupport.Instance.BackColour;
            iWidgetSelectorSource.Dock = DockStyle.Fill;

            // create the button panel
            iPanelButtons = new PanelNoBackgroundPaint();
            iPanelButtons.BackgroundImage = TextureManager.Instance.BlankControl;
            iPanelButtons.BackColor = ViewSupport.Instance.BackColour;
            iPanelButtons.Bounds = LayoutManager.Instance.BottomBounds;
            iPanelButtons.Dock = DockStyle.Bottom;

            // create the parent control for the buttons
            iButtonParent = new TransparentControlBase();
            iButtonParent.Bounds = LayoutManager.Instance.PanelToolsBounds;
            iButtonParent.Parent = iPanelButtons;

            // create the standby button
            ImageButton buttonStandby = ImageButtonFactory.CreateLeft(TextureManager.Instance.StandbyTouch,
                                                                      TextureManager.Instance.Standby);
            buttonStandby.Parent = iButtonParent;
            buttonStandby.Enabled = false;
            iButtonStandby = new WidgetButtonStandby(buttonStandby);

            // create the refresh button
            iButtonRefresh = ImageButtonFactory.CreateRight(TextureManager.Instance.RefreshTouch,
                                                            TextureManager.Instance.Refresh);
            iButtonRefresh.Click += ButtonRefreshClick;
            iButtonRefresh.Parent = iButtonParent;

            // create the context menu
            iContextMenu = new ContextMenuSource();
            iWidgetSelectorSource.ContextMenu = iContextMenu;

            // add top level controls to the view
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(192F, 192F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            iWidgetSelectorSource.Parent = this;
            iWidgetSelectorRoom.Parent = this;
            iPanelButtons.Parent = this;

            this.ResumeLayout(false);

            GotFocus += EventGotFocus;
        }

        public void Load(HelperKinsky aHelper)
        {
            iHelper = aHelper;
        }

        public ContextMenu PContextMenu
        {
            get { return iContextMenu; }
        }

        public IViewWidgetSelector<Linn.Kinsky.Room> ViewWidgetSelectorRoom
        {
            get { return iWidgetSelectorRoom; }
        }

        public IViewWidgetButton ViewWidgetButtonStandby
        {
            get { return iButtonStandby; }
        }

        public IViewWidgetSelector<Linn.Kinsky.Source> ViewWidgetSelectorSource
        {
            get { return iWidgetSelectorSource; }
        }

        private void EventGotFocus(object sender, EventArgs e)
        {
            if (Focused)
            {
                iWidgetSelectorSource.Focus();
            }
        }

        private void ButtonRefreshClick(object sender, EventArgs e)
        {
            iHelper.Rescan();
        }

        private WidgetSelectorSource iWidgetSelectorSource;
        private WidgetSelectorRoom iWidgetSelectorRoom;
        private PanelNoBackgroundPaint iPanelButtons;
        private TransparentControlBase iButtonParent;
        private WidgetButtonStandby iButtonStandby;
        private ImageButton iButtonRefresh;
        private ContextMenuSource iContextMenu;
        private HelperKinsky iHelper;
    }


    internal class ContextMenuSource : ContextMenu
    {
        private MenuItem AddMenuItem(string aText)
        {
            MenuItem item = new MenuItem();
            item.Text = aText;
            this.MenuItems.Add(item);
            return item;
        }

        public ContextMenuSource()
        {
            iMenuItemOptions = AddMenuItem("Options...");
            iMenuItemAbout = AddMenuItem("About");
            iMenuItemDebug = AddMenuItem("Debug");
            AddMenuItem("-");
            iMenuItemExit = AddMenuItem("Exit");
        }

        private MenuItem iMenuItemOptions;
        private MenuItem iMenuItemAbout;
        private MenuItem iMenuItemDebug;
        private MenuItem iMenuItemExit;
    }
}
