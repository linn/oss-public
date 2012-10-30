using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Net;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;
using Linn.Toolkit.WinForms;
using Linn.ProductSupport.Diagnostics;

using LinnSetup.Properties;

namespace LinnSetup
{
    public partial class FormLinnSetup : Form, IStack
    {
        public FormLinnSetup(HelperLinnSetup aHelper, Diagnostics aDiagnostics, AppletManager aManager) {
            iManager = aManager;
            iHelper = aHelper;
            iDiagnostics = aDiagnostics;

            iFormUserLog = new FormUserLog(iHelper.Icon);
            iFormUserLog.SetBackColour(Color.Black);
            iFormUserLog.SetForeColour(Color.White);

            InitializeComponent();

            this.WindowState = FormWindowState.Maximized;
            this.Text = iHelper.Product;
            this.Icon = iHelper.Icon;

            releaseNotesToolStripMenuItem.Image = Linn.Toolkit.WinForms.Properties.Resources.Rss;
            betaReleaseNotesToolStripMenuItem.Image = Linn.Toolkit.WinForms.Properties.Resources.Rss;

            // select view
            if (aHelper.ApplicationOptions.ViewDetails) {
                tableLayoutPanel1.Visible = false;
                listView1.Visible = true;
                deviceDetailsToolStripMenuItem.Text = "Icons";
            }

            // set up column sorting
            listView1.ListViewItemSorter = new ComparerListView(0); // default to sort by room

            //create tabs for applets
            for (int i = 0; i < iManager.AppletNames.Count; i++) {
                TabPage page = new TabPage(iManager.AppletNames[i]);
                page.Tag = i;
                tabControl1.TabPages.Add(page);
            }

            statusStrip1.Padding = new Padding(3, 0, 3, 0);
            Closed += EventFormClosedHandler;
        }

        private void MainForm_Shown(object sender, EventArgs e) {
            // start the network stack
            iHelper.Stack.SetStack(this);
            iHelper.Stack.SetStatusHandler(new StackStatusHandlerWinForms(iHelper.Title));
            iHelper.Stack.Start();

            // show the options dialog if specified by the user
            if (iHelper.Stack.StatusHandler.ShowOptions) {
                ShowOptionsDialog();
            }
        }

        private void EventFormClosedHandler(object sender, EventArgs e) {
            if (iDiagnostics != null) {
                iDiagnostics.Shutdown();
            }
            iHelper.Stack.Stop();
        }

        void IStack.Start(System.Net.IPAddress aIpAddress) {
            iEventServer = new EventServerUpnp();
            iListenerNotify = new SsdpListenerMulticast();
            iBoxes = new Boxes(iHelper, iEventServer, iListenerNotify);
            iBoxes.EventRoomAdded += RoomAddedHandler;
            iBoxes.EventRoomRemoved += RoomRemovedHandler;

            iEventServer.Start(aIpAddress);
            iListenerNotify.Start(aIpAddress);
            iBoxes.Start(aIpAddress);
        }

        void IStack.Stop() {
            iEventServer.Stop();
            iListenerNotify.Stop();
            iBoxes.Stop();
        }

        private void ShowOptionsDialog() {
            // add a new stack status change handler while the options page  is visible
            // leave the default one so the balloon tips still appear
            iHelper.Stack.EventStatusChanged += iHelper.Stack.StatusHandler.StackStatusOptionsChanged;

            // show the dialog
            FormUserOptions form = new FormUserOptions(iHelper.OptionPages);
            form.Icon = iHelper.Icon;
            form.ShowDialog();

            // cleanup
            form.Dispose();
            iHelper.Stack.EventStatusChanged -= iHelper.Stack.StatusHandler.StackStatusOptionsChanged;
        }

        public void RoomAddedHandler(object obj, EventArgsRoom e) {
            e.RoomArg.EventBoxAdded += BoxAddedHandler;
            e.RoomArg.EventBoxRemoved += BoxRemovedHandler;
        }

        public void RoomRemovedHandler(object obj, EventArgsRoom e) {
            e.RoomArg.EventBoxAdded -= BoxAddedHandler;
            e.RoomArg.EventBoxRemoved -= BoxRemovedHandler;
        }

        public void BoxAddedHandler(object obj, EventArgsBox e) {
            e.BoxArg.EventBoxChanged += BoxChangedHandler;

            // made synchronous to handle starting from fallback case, box added and box changed called in succession, so targetMediator.Target.AddBoxChangeEvent is missed otherwise
            this.Invoke(
                 (MethodInvoker)delegate() {
                     if (listView1.Items.ContainsKey(e.BoxArg.MacAddress)) {
                         //if the box is being added due to a room change update the item
                         TargetMediator targetMediator = (TargetMediator)listView1.Items[e.BoxArg.MacAddress].Tag;
                         targetMediator.Target.AddBoxChangeEvent(e.BoxArg);

                         ChangeItem(e.BoxArg);
                     }
                     else {
                         AddItem(e);
                     }
                 }
            );
        }

        //when room name changes topology removes and adds the box the new box. this handler removed the event handler for the box
        //in the old room. the new box box changed handler will be added in AddBox.
        public void BoxRemovedHandler(object obj, EventArgsBox e) {
            e.BoxArg.EventBoxChanged -= BoxChangedHandler;
        }

        public void BoxChangedHandler(object obj, EventArgsBox e) {
            this.BeginInvoke(
                 (MethodInvoker)delegate() {
                     ChangeItem(e.BoxArg);
                 }
            );
        }

        private void ChangeItem(Box aBox) {
            ListViewItem item = listView1.Items[aBox.MacAddress];
            TargetMediator targetMediator = (TargetMediator)item.Tag;

            SetListViewItem(targetMediator, item);
            listView1.Sort();

            if (listView1.SelectedIndices.Count > 0 && item.Index == listView1.SelectedIndices[0]) {
                SetStatusBar(targetMediator);
            }
            if (iFormTree != null && !iFormTree.IsDisposed) {
                iFormTree.UpdateTree();
            }
        }

        private void AddItem(EventArgsBox e) {
            Target target = new Target(e.BoxArg);
            TargetMediator targetMediator = new TargetMediator(target, iManager);

            ListViewItem item = new ListViewItem(new string[] { "", "", "", "", "", "", "", "", "", "", "", "", "" });
            SetListViewItem(targetMediator, item);
            listView1.Items.Add(item);

            //on startup set selected item to the last user selected device
            if (e.BoxArg.MacAddress == iHelper.ApplicationOptions.LastBoxSelected) {
                item.Selected = true;
                pictureBox1.ImageLocation = e.BoxArg.ImageUri;
            }
            if (iFormTree != null && !iFormTree.IsDisposed) {
                iFormTree.UpdateTree();
            }
        }

        private void SetListViewItem(TargetMediator aTargetMediator, ListViewItem aItem) {
            aItem.SubItems[0].Text = aTargetMediator.Target.Box.Room;
            aItem.SubItems[1].Text = aTargetMediator.Target.Box.Name;
            aItem.SubItems[2].Text = aTargetMediator.Target.Box.Model;
            aItem.SubItems[3].Text = aTargetMediator.Target.Box.SoftwareVersion;
            if (iBoxes.UpdateCheckInProgress) {
                aItem.SubItems[4].Text = "Update Check In Progress";
            }
            else if (iBoxes.UpdateCheckFailed) {
                aItem.SubItems[4].Text = "Update Check Failed";
            }
            else if (aTargetMediator.Target.Box.SoftwareUpdateVersion == null) {
                aItem.SubItems[4].Text = "Device not found";
            }
            else {
                aItem.SubItems[4].Text = aTargetMediator.Target.Box.SoftwareUpdateString(true);
            }
            aItem.SubItems[5].Text = aTargetMediator.Target.Box.IpAddress;
            aItem.SubItems[6].Text = aTargetMediator.Target.Box.MacAddress;
            aItem.SubItems[7].Text = aTargetMediator.Target.Box.StatusText;
            aItem.SubItems[8].Text = aTargetMediator.Target.Box.ProductId;
            string boardTypes = "";
            for (uint i = 0; i < aTargetMediator.Target.Box.BoardType.Length; i++) {
                if (boardTypes.Length > 0 && aTargetMediator.Target.Box.BoardType[i].Length > 0) {
                    boardTypes += ", ";
                }
                boardTypes += aTargetMediator.Target.Box.BoardType[i];
            }
            string boardDescriptions = "";
            for (uint i = 0; i < aTargetMediator.Target.Box.BoardDescription.Length; i++) {
                if (boardDescriptions.Length > 0 && aTargetMediator.Target.Box.BoardDescription[i].Length > 0) {
                    boardDescriptions += ", ";
                }
                boardDescriptions += aTargetMediator.Target.Box.BoardDescription[i];
            }
            string boardIds = "";
            for (uint i = 0; i < aTargetMediator.Target.Box.BoardId.Length; i++) {
                if (boardIds.Length > 0 && aTargetMediator.Target.Box.BoardId[i].Length > 0) {
                    boardIds += ", ";
                }
                boardIds += aTargetMediator.Target.Box.BoardId[i];
            }
            string boardNumbers = "";
            for (uint i = 0; i < aTargetMediator.Target.Box.BoardNumber.Length; i++) {
                if (boardNumbers.Length > 0 && aTargetMediator.Target.Box.BoardNumber[i].Length > 0) {
                    boardNumbers += ", ";
                }
                boardNumbers += aTargetMediator.Target.Box.BoardNumber[i];
            }
            aItem.SubItems[9].Text = boardNumbers;
            aItem.SubItems[10].Text = boardTypes;
            aItem.SubItems[11].Text = boardDescriptions;
            aItem.SubItems[12].Text = boardIds;
            
            aItem.Tag = aTargetMediator;
            aItem.Name = aTargetMediator.Target.Box.MacAddress;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
            if (listView1.SelectedItems.Count == 0) {
                return;
            }

            TargetMediator targetMediator = (TargetMediator)listView1.SelectedItems[0].Tag;
            targetMediator.Activate(targetMediator.AppletSelectedIndex);

            SetSelectedTab(targetMediator.AppletSelectedIndex);
            ChangePanel(targetMediator);
            SetStatusBar(targetMediator);
            iHelper.ApplicationOptions.LastBoxSelected = targetMediator.Target.Box.MacAddress;
        }

        
        private bool AppletTabExists(int selectedIndex) {
            foreach (TabPage page in tabControl1.TabPages) {
                int appletIndex = (int)page.Tag;
                if (appletIndex == selectedIndex) {
                    return true;
                }
            }

            return false;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
            if (listView1.SelectedItems.Count == 0) {
                return;
            }

            int appletIndex = (int)tabControl1.SelectedTab.Tag;
            TargetMediator targetMediator = (TargetMediator)listView1.SelectedItems[0].Tag;

            //when the list view item changes the ui has already been modified 
            if (appletIndex == targetMediator.AppletSelectedIndex) {
                return;
            }

            targetMediator.Activate(appletIndex);
            ChangePanel(targetMediator);
            SetStatusBar(targetMediator);
        }

        private void SetSelectedTab(int appletIndex) {
            int selectedTabIndex;
            for (selectedTabIndex = 0; selectedTabIndex < tabControl1.TabPages.Count; selectedTabIndex++) {
                int tabAppletIndex = (int)tabControl1.TabPages[selectedTabIndex].Tag;
                if (tabAppletIndex == appletIndex) {
                    break;
                }
            }

            tabControl1.SelectedIndex = selectedTabIndex;
        }

        private void ChangePanel(TargetMediator aTarget) {
            Control ui = aTarget.CurrentApplet.Ui;
            ui.Name = "Ui";
            ui.Dock = DockStyle.Fill;
            tabControl1.SelectedTab.Controls.RemoveByKey("Ui");
            tabControl1.SelectedTab.Controls.Add(ui);
        }

        private void SetStatusBar(TargetMediator aTarget) {
            nameStatus.Text = aTarget.ToString();
            nameStatus.BackColor = aTarget.StatusColor;
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e) {
            listView1.ListViewItemSorter = new ComparerListView(e.Column);
            listView1.Sort();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

        private void refreshDeviceListToolStripMenuItem_Click(object sender, EventArgs e) {
            iBoxes.Rescan();
        }

        private void treeToolStripMenuItem_Click(object sender, EventArgs e) {
            if (iFormTree == null || iFormTree.IsDisposed) {
                iFormTree = new FormTree(iHelper.Icon, iBoxes);
            }
            iFormTree.Show();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowOptionsDialog();
        }

        private void debugConsoleToolStripMenuItem_Click(object sender, EventArgs e) {
            if (iFormUserLog.IsDisposed) {
                iFormUserLog = new FormUserLog(iHelper.Icon);
            }
            iFormUserLog.Show();
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e) {
            iBoxes.CheckForUpdates();
        }

        private void toolsToolStripMenuItem_DropDownOpening(object sender, EventArgs e) {
            checkForUpdatesToolStripMenuItem.Enabled = !iBoxes.UpdateCheckInProgress;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            FormAboutBox aboutBox = new FormAboutBox(iHelper);
            aboutBox.ShowDialog();
            aboutBox.Dispose();
        }

        private void deviceDetailsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (deviceDetailsToolStripMenuItem.Text == "Details") {
                tableLayoutPanel1.Visible = false;
                listView1.Visible = true;
                deviceDetailsToolStripMenuItem.Text = "Icons";
                iHelper.ApplicationOptions.ViewDetails = true;
            }
            else {
                tableLayoutPanel1.Visible = true;
                listView1.Visible = false;
                deviceDetailsToolStripMenuItem.Text = "Details";
                iHelper.ApplicationOptions.ViewDetails = false;
            }
        }

        private void onlineHelpToolStripMenuItem_Click(object sender, EventArgs e) {
            string url = "http://docs.linn.co.uk/wiki/index.php/";
            try {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception) {
                MessageBox.Show("Could not load: " + url, "Load URL Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void releaseNotesToolStripMenuItem_Click(object sender, EventArgs e) {
            string url = "http://products.linn.co.uk/VersionInfo/ReleaseVersionInfo.xml";
            try {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception) {
                MessageBox.Show("Could not load: " + url, "Load URL Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void betaReleaseNotesToolStripMenuItem_Click(object sender, EventArgs e) {
            string url = "http://products.linn.co.uk/VersionInfo/BetaVersionInfo.xml";
            try {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception) {
                MessageBox.Show("Could not load: " + url, "Load URL Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void tuneInRadioWebsiteToolStripMenuItem_Click(object sender, EventArgs e) {
            string url = "http://tunein.com/";
            try {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception) {
                MessageBox.Show("Could not load: " + url, "Load URL Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void rebootDeviceToolStripMenuItem_Click(object sender, EventArgs e) {
            TargetMediator targetMediator = (TargetMediator)listView1.SelectedItems[0].Tag;
            targetMediator.Target.Box.Reboot();
        }

        private void turnDeviceOnToolStripMenuItem_Click(object sender, EventArgs e) {
            TargetMediator targetMediator = (TargetMediator)listView1.SelectedItems[0].Tag;
            targetMediator.Target.Box.SetStandby(false);
        }

        private EventServerUpnp iEventServer;
        private SsdpListenerMulticast iListenerNotify;
        private Boxes iBoxes = null;
        private AppletManager iManager;
        private HelperLinnSetup iHelper;
        private Diagnostics iDiagnostics;
        private FormUserLog iFormUserLog = null;
        private FormTree iFormTree = null;
    }
}
