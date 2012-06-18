using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Windows.Forms;

using Linn;
using Linn.Kinsky;
using Linn.Toolkit.WinForms;

namespace KinskyJukebox
{
    public partial class FormKinskyJukebox : Form
    {
        public FormKinskyJukebox(HelperKinskyJukebox aHelper) {
            InitializeComponent();
            warningLabel.Text = "Only " + MediaCollection.kPlaylistMaxTracks + " tracks will be used";
            this.warningBox.Image = Linn.Toolkit.WinForms.Properties.Resources.Warning;
            SetDoubleBuffered(listViewPlaylist);
            SetDoubleBuffered(listViewPresets);
            SetDoubleBuffered(treeViewPreset);
            SetDoubleBuffered(treeViewScan);
            iHelper = aHelper;
            this.Text = iHelper.Product;
            this.Icon = iHelper.Icon;
            this.restoreToolStripMenuItem.Image = Linn.Kinsky.Properties.Resources.KinskyLogo;
            panelNoDetails.Visible = true;
            panelBookmarkDetails.Visible = false;
            panelPresetDetails.Visible = false;
            panelTrackDetails.Visible = false;
            iTreeNodeComparer = new TreeNodeComparer();

            iFormUserLog = new FormUserLog(Icon.FromHandle(Properties.Resources.Console.GetHicon()));
            iFormUserLog.SetBackColour(Color.Black);
            iFormUserLog.SetForeColour(Color.White);

            // create image list for node icons
            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.TransparentColor = Color.Transparent;
            imageList.Images.Add(Properties.Resources.Bookmark);
            imageList.Images.Add(Properties.Resources.Preset);
            imageList.Images.Add(Properties.Resources.Track);
            imageList.Images.Add(Linn.Toolkit.WinForms.Properties.Resources.Warning);
            treeViewScan.ImageList = imageList;
            treeViewPreset.ImageList = imageList;
            TreeNode defaultNode = new TreeNode(kExampleBookmarkName);
            defaultNode.ToolTipText = "Count: " + defaultNode.Nodes.Count;
            defaultNode.ImageIndex = 0;
            defaultNode.SelectedImageIndex = 0;
            defaultNode.Name = "1";
            treeViewPreset.Nodes.Add(defaultNode);
        }

        static public void SetDoubleBuffered(System.Windows.Forms.Control aControl) {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(System.Windows.Forms.Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, aControl, new object[] { true });
        }

        private void FormKinskyJukebox_Load(object sender, EventArgs e) {
            SetupUserInterface();

            // delete any temp files left behind
            try {
                Directory.Delete(iHelper.TempDirectoryPath, true); // true is for delete recursively
            }
            catch (Exception) {
            }
            // create jukebox temp directory
            try {
                Directory.CreateDirectory(iHelper.TempDirectoryPath);
            }
            catch (Exception) {
            }
        }

        private void FormKinskyJukebox_Shown(object sender, EventArgs e) {
            // check for updates to Kinsky Jukebox (as long as the user has not disabled auto updates)
            if (iHelper.OptionPageUpdates.AutoUpdate) {
                CheckForUpdates(false);
            }
            
            // start the network stack
            iHelper.Stack.SetStatusHandler(new StackStatusHandlerWinForms(iHelper.Title, notifyIcon));
            iHelper.Stack.EventStatusChanged += EventStackStatusChanged; // server icon color changes red/green, notify icon changes
            iHelper.Stack.Start();

            // show the options dialog if specified by the user
            if (iHelper.Stack.StatusHandler.ShowOptions) {
                ShowOptionsDialog(true, false);
            }
        }

        private void FormKinskyJukebox_FormClosed(object sender, FormClosedEventArgs e) {
            iHelper.Stack.EventStatusChanged -= EventStackStatusChanged; // server icon color changes red/green, notify icon changes
            iHelper.Stack.Stop();
        }

        private void SetupUserInterface() {
            this.SizeChanged -= FormKinskyJukebox_SizeChanged;
            splitContainer1.SplitterMoved -= splitContainer1_SplitterMoved;
            splitContainer2.SplitterMoved -= splitContainer2_SplitterMoved;

            this.Size = iHelper.ApplicationOptions.WindowSize;

            if (iHelper.ApplicationOptions.WindowMaximised) {
                this.WindowState = FormWindowState.Maximized;
                try {
                    splitContainer1.SplitterDistance = iHelper.ApplicationOptions.LeftSplitterLocation;
                    splitContainer2.SplitterDistance = iHelper.ApplicationOptions.RightSplitterLocation;
                }
                catch { // use defaults if something has gone wrong
                }
            }
            else if (iHelper.ApplicationOptions.WindowMinimised && iHelper.OptionPageSetup.UseHttpServer) {
                // if using http server and previously minimized - start app in sys tray
                try {
                    splitContainer1.SplitterDistance = iHelper.ApplicationOptions.LeftSplitterLocation;
                    splitContainer2.SplitterDistance = iHelper.ApplicationOptions.RightSplitterLocation;
                }
                catch { // use defaults if something has gone wrong
                }
                this.WindowState = FormWindowState.Minimized;
            }
            else {
                this.WindowState = FormWindowState.Normal;
                try {
                    splitContainer1.SplitterDistance = iHelper.ApplicationOptions.LeftSplitterLocation;
                    splitContainer2.SplitterDistance = iHelper.ApplicationOptions.RightSplitterLocation;
                }
                catch { // use defaults if something has gone wrong
                }
            }

            this.SizeChanged += FormKinskyJukebox_SizeChanged;
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
            splitContainer2.SplitterMoved += splitContainer2_SplitterMoved;
        }

        private void ShowOptionsDialog(bool aStartOnNetwork, bool aStartOnSetup) {
            // add a new stack status change handler while the options page  is visible
            // leave the default one so the balloon tips still appear
            iHelper.Stack.EventStatusChanged += iHelper.Stack.StatusHandler.StackStatusOptionsChanged;

            // show the dialog
            FormUserOptions form = new FormUserOptions(iHelper.OptionPages);
            form.Icon = Icon.FromHandle(Properties.Resources.Options.GetHicon());
            if (aStartOnNetwork) {
                form.SetPageByName("Network");
            }
            else if (aStartOnSetup) {
                form.SetPageByName("Setup");
            }
            DialogResult result = form.ShowDialog();
            if (!aStartOnNetwork && !aStartOnSetup && iWizardInProgress) {
                // deal with wizard for normal case
                if (result == DialogResult.Cancel) {
                    iFormWizard.WizardCancel();
                }
                else {
                    iFormWizard.WizardConfigComplete(false);
                }
            }

            // cleanup
            form.Dispose();
            iHelper.Stack.EventStatusChanged -= iHelper.Stack.StatusHandler.StackStatusOptionsChanged;
        }

        private void FormKinskyJukebox_SizeChanged(object sender, EventArgs e) {
            if (this.WindowState == FormWindowState.Normal) {
                iHelper.ApplicationOptions.WindowSize = this.Size;
            }
            iHelper.ApplicationOptions.WindowMaximised = (this.WindowState == FormWindowState.Maximized);
            iHelper.ApplicationOptions.WindowMinimised = (this.WindowState == FormWindowState.Minimized);
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e) {
            this.ShowInTaskbar = true;
            if (iHelper.ApplicationOptions.WindowMaximised) {
                WindowState = FormWindowState.Maximized;
            }
            else {
                WindowState = FormWindowState.Normal;
            }
            try {
                notifyIcon.Visible = false;
            }
            catch {
                // not supported for all platforms
            }
        }

        private void FormKinskyJukebox_Resize(object sender, EventArgs e) {
            if (this.WindowState == FormWindowState.Minimized) {
                if (iHelper.OptionPageSetup.UseHttpServer) {
                    try {
                        notifyIcon.Visible = iHelper.IsWindows;
                        if (notifyIcon.Visible) {
                            notifyIcon.ShowBalloonTip(1000, iHelper.Title, "Http server will continue to run in the background", ToolTipIcon.Info);
                            this.ShowInTaskbar = false;
                        }
                    }
                    catch {
                        // not supported for all platforms
                    }
                }
            }
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e) {
            iHelper.ApplicationOptions.LeftSplitterLocation = splitContainer1.SplitterDistance;
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e) {
            iHelper.ApplicationOptions.RightSplitterLocation = splitContainer2.SplitterDistance;
        }

        private void httpServerStatus_MouseEnter(object sender, EventArgs e) {
            httpServerStatus.ToolTipText = "Http Server: ";
            if (iHelper.Stack.Status.State == EStackState.eOk) {
                httpServerStatus.ToolTipText += "Running";
                if (iHelper.OptionPageSetup.UseHttpServer) {
                    httpServerStatus.ToolTipText += ", Enabled";
                }
                else {
                    httpServerStatus.ToolTipText += ", Not Enabled";
                }
            }
            else {
                httpServerStatus.ToolTipText += "Not Running";
                if (iHelper.OptionPageSetup.UseHttpServer) {
                    httpServerStatus.ToolTipText += ", Enabled";
                }
                else {
                    httpServerStatus.ToolTipText += ", Not Enabled";
                }
            }
        }

        private void EventStackStatusChanged(object sender, EventArgsStackStatus e) {
            if (this.InvokeRequired) {
                // no need to block
                this.BeginInvoke(new EventHandler<EventArgsStackStatus>(EventStackStatusChanged), new object[] { sender, e });
                return;
            }
            if (e.Status.State == EStackState.eOk) {
                httpServerStatus.Image = Properties.Resources.Server;
                try {
                    notifyIcon.Icon = iHelper.Icon;
                    notifyIcon.Text = iHelper.Title + " (Http Server Connected)";
                }
                catch {
                    // not supported for all platforms
                }
            }
            else {
                httpServerStatus.Image = Properties.Resources.ServerFail;
                try {
                    notifyIcon.Icon = Icon.FromHandle(Linn.Toolkit.WinForms.Properties.Resources.Warning.GetHicon());
                    notifyIcon.Text = iHelper.Title + " (Http Server Disconnected)";
                }
                catch {
                    // not supported for all platforms
                }
            }
        }

        public void AddCollectionNode(TreeNode aNode) {
            if (this.InvokeRequired) {
                // no need to block
                this.BeginInvoke(new DEventAddNode(AddCollectionNode), new object[] { aNode });
                return;
            }
            // add root element
            treeViewScan.Nodes.Add(aNode);
        }

        public void AddImportedNode(TreeNode aNode) {
            if (this.InvokeRequired) {
                // no need to block
                this.BeginInvoke(new DEventAddNode(AddImportedNode), new object[] { aNode });
                return;
            }
            // add root element
            if (aNode.Text == Presets.kMiscNodeName) {
                foreach (TreeNode node in treeViewPreset.Nodes) {
                    if (node.Text == Presets.kMiscNodeName) {
                        // lump all misc presets together
                        foreach (TreeNode newNode in aNode.Nodes) {
                            node.Nodes.Add(newNode);
                        }
                        return;
                    }
                }
            }
            treeViewPreset.Nodes.Add(aNode);
        }

        public bool ScanDataExists() {
            return (treeViewScan.Nodes.Count > 0);
        }

        public bool PresetDataExists() {
            RemoveExampleBookmark();
            return (treeViewPreset.Nodes.Count > 0);
        }

        public void toolStripButtonScan_Click(object sender, EventArgs e) {
            if (!VerifyCollectionLocation()) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return;
            }
            if (!VerifyHttpLocation()) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return;
            }
            // remove any old collection data
            treeViewScan.Nodes.Clear();
            ScanProgressBar.Value = 0;
            ScanLabel.Text = "";
            if (!iWizardInProgress) {
                statusStripScan.Visible = true;
                toolStripButtonScan.Enabled = false;
                toolStripButtonQuickScan.Enabled = false;
                toolStripButtonWizard.Enabled = false;
                scanCollectionToolStripMenuItem.Enabled = false;
                quickScanToolStripMenuItem.Enabled = false;
                oneClickWizardToolStripMenuItem.Enabled = false;
            }
            // scan for new data
            iLastScanFull = true;
            iCollection = new MediaCollection(iHelper, AddCollectionNode, UpdateScanProgress);
            iCollection.Scan();
        }

        public void toolStripButtonQuickScan_Click(object sender, EventArgs e) {
            if (!VerifyCollectionLocation()) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return;
            }
            if (!VerifyHttpLocation()) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return;
            }
            // get 'Quick' location (must be within verified collection location
            string location = null;
            FolderBrowserDialog browse = new FolderBrowserDialog();
            browse.Description = "Please select the directory for the quick scan." + Environment.NewLine + "This must reside within the directory you have configured as your Scan Directory (root directory of your music collection).";
            browse.ShowNewFolderButton = false;
            browse.SelectedPath = iHelper.OptionPageSetup.CollectionLocation;
            if (browse.ShowDialog() == DialogResult.OK) {
                location = browse.SelectedPath.Trim();
            }
            if (!Directory.Exists(location)) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return; // if quick location not found, exit
            }
            if (!location.StartsWith(iHelper.OptionPageSetup.CollectionLocation)) {
                string failMessage = "Directory must reside within the directory you have configured as your Scan Directory (root directory of your music collection)." + Environment.NewLine + "You can change this setting under Tools/Options/Setup.";
                if (iWizardInProgress) {
                    iFormWizard.WizardFailed(failMessage);
                }
                else {
                    MessageBox.Show(failMessage, "Quick Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                }
                return;
            }
            // remove any old collection data
            treeViewScan.Nodes.Clear();
            ScanProgressBar.Value = 0;
            ScanLabel.Text = "";
            if (!iWizardInProgress) {
                statusStripScan.Visible = true;
                toolStripButtonScan.Enabled = false;
                toolStripButtonQuickScan.Enabled = false;
                toolStripButtonWizard.Enabled = false;
                scanCollectionToolStripMenuItem.Enabled = false;
                quickScanToolStripMenuItem.Enabled = false;
                oneClickWizardToolStripMenuItem.Enabled = false;
            }
            // scan for new data
            iLastScanFull = false;
            iCollection = new MediaCollection(iHelper, AddCollectionNode, UpdateScanProgress);
            iCollection.QuickScan(location);
        }

        private void oneClickWizardToolStripMenuItem_Click(object sender, EventArgs e) {
            Linn.UserLog.WriteLine("Wizard started");
            iWizardInProgress = true;
            iFormWizard = new FormWizard(iHelper, this);
            iFormWizard.ShowDialog();
            iWizardInProgress = false;
            iFormWizard.Dispose();
        }

        private void toolStripButtonAdd_Click(object sender, EventArgs e) {
            if (treeViewScan.SelectedNode != null) {
                TreeNode newNode = (TreeNode)treeViewScan.SelectedNode.Clone();
                if (treeViewPreset.SelectedNode == null) { 
                    if (treeViewScan.SelectedNode.Level == 0) {
                        // add selected bookmark
                        treeViewPreset.Nodes.Add(newNode);
                    }
                    else {
                        return;
                    }
                }
                else if (treeViewPreset.SelectedNode.Level == 0) {
                    if (treeViewScan.SelectedNode.Level == 0) {
                        // insert selected bookmark
                        treeViewPreset.Nodes.Insert(treeViewPreset.SelectedNode.Index, newNode);
                    }
                    else if (treeViewScan.SelectedNode.Level == 1) {
                        treeViewPreset.SelectedNode.Nodes.Add(newNode);
                    }
                    else {
                        return;
                    }
                }
                else if (treeViewPreset.SelectedNode.Level == 1) {
                    if (treeViewScan.SelectedNode.Level == 1) {
                        foreach (TreeNode node in treeViewScan.SelectedNode.Nodes) {
                            newNode = (TreeNode)node.Clone();
                            treeViewPreset.SelectedNode.Nodes.Add(newNode);
                        }
                    }
                    else if (treeViewScan.SelectedNode.Level == 2) {
                        treeViewPreset.SelectedNode.Nodes.Add(newNode);
                    }
                    else {
                        return;
                    }
                }
                else if (treeViewPreset.SelectedNode.Level == 2) {
                    if (treeViewScan.SelectedNode.Level == 1) {
                        foreach (TreeNode node in treeViewScan.SelectedNode.Nodes) {
                            newNode = (TreeNode)node.Clone();
                            treeViewPreset.SelectedNode.Parent.Nodes.Insert(treeViewPreset.SelectedNode.Index, newNode);
                        }
                    }
                    else if (treeViewScan.SelectedNode.Level == 2) {
                        treeViewPreset.SelectedNode.Parent.Nodes.Insert(treeViewPreset.SelectedNode.Index, newNode);
                    }
                    else {
                        return;
                    }
                }
                else {
                    return;
                }
                AssignNumbers();
            }
        }

        public void ScanStopButton_Click(object sender, EventArgs e) {
            iCollection.StopScan();
        }

        public void SynchStopButton_Click(object sender, EventArgs e) {
            try {
                iPresets.StopExport();
            }
            catch { // not created or not running
            }
            try {
                iPresets.StopImport();
            }
            catch { // not created or not running
            }
            try {
                iPrint.StopPrint();
            }
            catch { // not created or not running
            }
        }

        public void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvokeRequired) {
                this.Invoke(new MethodInvoker(this.Close));
            }
            else {
                this.Close();
            }
        }

        public void optionsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowOptionsDialog(false, false);
        }

        private void debugConsoleToolStripMenuItem_Click(object sender, EventArgs e) {
            if (iFormUserLog.IsDisposed) {
                iFormUserLog = new FormUserLog(Icon.FromHandle(Properties.Resources.Console.GetHicon()));
            }
            iFormUserLog.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            FormAboutBox aboutBox = new FormAboutBox(iHelper);
            aboutBox.ShowDialog();
            aboutBox.Dispose();
        }

        private void treeViewScan_ItemDrag(object sender, ItemDragEventArgs e) {
            DoDragDrop(e.Item, DragDropEffects.Copy);
        }

        private void treeViewPreset_ItemDrag(object sender, ItemDragEventArgs e) {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeViewPreset_DragEnter(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.All;
        }

        private void treeViewPreset_DragDrop(object sender, DragEventArgs e) {
            TreeNode sourceNode;
            iDragDropLastNodeOver = null;
            iDragDropNullRefresh = true;

            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false)) {
                Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                TreeNode destinationNode = ((TreeView)sender).GetNodeAt(pt);
                sourceNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                TreeNode newNode = (TreeNode)sourceNode.Clone();

                if (destinationNode == null) {
                    if (sourceNode.Level == 0) {
                        // add new bookmark to end of root (copy or move)
                        treeViewPreset.Nodes.Add(newNode);
                    }
                    else if (sourceNode.Level == 1 && sourceNode.TreeView.Name == "treeViewPreset") {
                        // move preset to end of current node
                        sourceNode.Parent.Nodes.Add(newNode);
                    }
                    else if (sourceNode.Level == 2 && sourceNode.TreeView.Name == "treeViewPreset") {
                        // move track to end of current node
                        sourceNode.Parent.Nodes.Add(newNode);
                    }
                    else {
                        return;
                    }
                }
                else {
                    if (destinationNode.Level == 0) { // bookmarks
                        if (sourceNode.Level == 0) {
                             // drop a bookmark into a bookmark - add to root (copy or move)
                            treeViewPreset.Nodes.Insert(destinationNode.Index, newNode);
                        }
                        else if (sourceNode.Level == 1) {
                            // move a preset node into a bookmark
                            if (sourceNode.TreeView.Name == "treeViewPreset" && destinationNode.Index == sourceNode.Parent.Index + 1) {
                                // add to end of source node (if it is the next node)
                                sourceNode.Parent.Nodes.Add(newNode);
                            }
                            else {
                                // add to end of destination node
                                destinationNode.Nodes.Add(newNode);
                            }
                        }
                        else if (sourceNode.Level == 2 && sourceNode.TreeView.Name == "treeViewPreset") {
                            // move a track node into a bookmark
                            if (destinationNode.Index == sourceNode.Parent.Parent.Index + 1) {
                                // add to end of source node (if it is the next node)
                                sourceNode.Parent.Nodes.Add(newNode);
                            }
                            else {
                                return;
                            }
                        }
                        else {
                            return;
                        }
                    }
                    else if (destinationNode.Level == 1) { // presets
                        if (sourceNode.Level == 0) {
                            // only allow bookmarks to be added to root
                            return;
                        }
                        else if (sourceNode.Level == 1) {
                            if (sourceNode.TreeView.Name == "treeViewScan") {
                                // drop a preset into a preset from scan side - add source preset to dest preset (track by track)
                                foreach (TreeNode node in sourceNode.Nodes) {
                                    newNode = (TreeNode)node.Clone();
                                    destinationNode.Nodes.Add(newNode);
                                }
                            }
                            else {
                                // drop a preset into a preset from preset side - put into that preset's bookmark (numerically)
                                destinationNode.Parent.Nodes.Insert(destinationNode.Index, newNode);
                            }
                        }
                        else if (sourceNode.Level == 2) {
                            // drop a track into a preset
                            if (sourceNode.TreeView.Name == "treeViewPreset" && destinationNode.Index == sourceNode.Parent.Index + 1) {
                                // add to end of source node (if it is the next node)
                                sourceNode.Parent.Nodes.Add(newNode);
                            }
                            else {
                                // add to end of destination node
                                destinationNode.Nodes.Add(newNode);
                            }
                        }
                        else {
                            return;
                        }
                    }
                    else if (destinationNode.Level == 2) { // tracks
                        if (sourceNode.Level == 0) {
                            // only allow bookmarks to be added to root
                            return;
                        }
                        else if (sourceNode.Level == 1) {
                            // drop preset into track - add all tracks from selected preset into destination preset
                            if (sourceNode.TreeView.Name == "treeViewScan") {
                                foreach (TreeNode node in sourceNode.Nodes) {
                                    newNode = (TreeNode)node.Clone();
                                    destinationNode.Parent.Nodes.Insert(destinationNode.Index, newNode);
                                }
                            }
                            else {
                                // not allowed from within preset section
                                return;
                            }
                        }
                        else if (sourceNode.Level == 2) {
                            // drop a track into a track - put into that track's preset
                            destinationNode.Parent.Nodes.Insert(destinationNode.Index, newNode);
                        }
                        else {
                            return;
                        }
                    }
                    else {
                        return;
                    }
                }

                if (e.AllowedEffect == DragDropEffects.Move) {
                    sourceNode.Remove();
                }
                AssignNumbers();
                treeViewPreset.SelectedNode = newNode; // insure node being dragged is selected
            }
        }

        private void treeViewPreset_DragOver(object sender, DragEventArgs e) {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode destinationNode = ((TreeView)sender).GetNodeAt(pt);
            TreeNode sourceNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");

            if (destinationNode == null) {
                if (iDragDropNullRefresh) {
                    iDragDropNullRefresh = false;
                    iDragDropLastNodeOver = null;
                    treeViewPreset.Refresh();
                    if (sourceNode.Level == 0) {
                        if (treeViewPreset.Nodes.Count > 0) {
                            DrawBottomPlaceholder(treeViewPreset.Nodes[treeViewPreset.Nodes.Count - 1]);
                        }
                    }
                    else if (sourceNode.Level == 1 && sourceNode.TreeView.Name == "treeViewPreset") {
                        DrawBottomPlaceholder(sourceNode.Parent.LastNode);
                    }
                    else if (sourceNode.Level == 2 && sourceNode.TreeView.Name == "treeViewPreset") {
                        DrawBottomPlaceholder(sourceNode.Parent.LastNode);
                    }
                    else {
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                    e.Effect = DragDropEffects.Copy | DragDropEffects.Move;
                }
            }
            else {
                iDragDropNullRefresh = true;
                ScrollOnDrag(sender, e);
                if (iDragDropLastNodeOver != destinationNode) {
                    iDragDropLastNodeOver = destinationNode;
                    treeViewPreset.Refresh();
                    if (destinationNode.Level == 0) { // bookmarks
                        if (sourceNode.Level == 0) {
                        }
                        else if (sourceNode.Level == 1) {
                            if (sourceNode.TreeView.Name == "treeViewPreset" && destinationNode.Index == sourceNode.Parent.Index + 1) {
                                DrawBottomPlaceholder(sourceNode.Parent.LastNode);
                            }
                            e.Effect = DragDropEffects.Copy | DragDropEffects.Move;
                            return;
                        }
                        else if (sourceNode.Level == 2 && sourceNode.TreeView.Name == "treeViewPreset") {
                            if (destinationNode.Index == sourceNode.Parent.Parent.Index + 1) {
                                DrawBottomPlaceholder(sourceNode.Parent.LastNode);
                                e.Effect = DragDropEffects.Copy | DragDropEffects.Move;
                                return;
                            }
                            else {
                                e.Effect = DragDropEffects.None;
                                return;
                            }
                        }
                        else {
                            e.Effect = DragDropEffects.None;
                            return;
                        }
                    }
                    else if (destinationNode.Level == 1) { // presets
                        if (sourceNode.Level == 1) {
                            if (sourceNode.TreeView.Name == "treeViewScan") {
                                // from scan side - insure preset is added to dest preset
                                e.Effect = DragDropEffects.Copy | DragDropEffects.Move;
                                return;
                            }
                            // from preset side - insure preset order is changed
                        }
                        else if (sourceNode.Level == 2) {
                            if (sourceNode.TreeView.Name == "treeViewPreset" && destinationNode.Index == sourceNode.Parent.Index + 1) {
                                DrawBottomPlaceholder(sourceNode.Parent.LastNode);
                            }
                            e.Effect = DragDropEffects.Copy | DragDropEffects.Move;
                            return;
                        }
                        else {
                            e.Effect = DragDropEffects.None;
                            return;
                        }
                    }
                    else if (destinationNode.Level == 2) { // tracks
                        if (sourceNode.TreeView.Name == "treeViewScan") {
                            if (sourceNode.Level <= 0 || sourceNode.Level > 2) {
                                e.Effect = DragDropEffects.None;
                                return;
                            }
                        }
                        else {
                            if (sourceNode.Level <= 1 || sourceNode.Level > 2) {
                                e.Effect = DragDropEffects.None;
                                return;
                            }
                        }
                    }
                    else {
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                    e.Effect = DragDropEffects.Copy | DragDropEffects.Move;
                    DrawTopPlaceholder(destinationNode);
                }
            }
        }

        private void treeViewPreset_DragLeave(object sender, EventArgs e) {
            iDragDropLastNodeOver = null;
            iDragDropNullRefresh = true;
            treeViewPreset.Refresh();
        }

        private void DrawTopPlaceholder(TreeNode aNodeOver) {
            Graphics g = this.treeViewPreset.CreateGraphics();
            int leftPos = aNodeOver.Bounds.Left;
            int rightPos = this.treeViewPreset.Width - 28; // 8 for triangles, 20 for scrollbar
            Point[] leftTriangle = new Point[3] { new Point(leftPos, aNodeOver.Bounds.Top - 4), new Point(leftPos, aNodeOver.Bounds.Top + 4), new Point(leftPos + 4, aNodeOver.Bounds.Top) };
            Point[] rightTriangle = new Point[3] { new Point(rightPos, aNodeOver.Bounds.Top - 4), new Point(rightPos, aNodeOver.Bounds.Top + 4), new Point(rightPos - 4, aNodeOver.Bounds.Top) };
            g.FillPolygon(System.Drawing.Brushes.Black, leftTriangle);
            g.FillPolygon(System.Drawing.Brushes.Black, rightTriangle);
            g.DrawLine(new System.Drawing.Pen(Color.Black, 2), new Point(leftPos, aNodeOver.Bounds.Top), new Point(rightPos, aNodeOver.Bounds.Top));
        }

        private void DrawBottomPlaceholder(TreeNode aNodeOver) {
            Graphics g = this.treeViewPreset.CreateGraphics();
            int leftPos = aNodeOver.Bounds.Left;
            int rightPos = this.treeViewPreset.Width - 28; // 8 for triangles, 20 for scrollbar
            Point[] leftTriangle = new Point[3] { new Point(leftPos, aNodeOver.Bounds.Bottom - 4), new Point(leftPos, aNodeOver.Bounds.Bottom + 4), new Point(leftPos + 4, aNodeOver.Bounds.Bottom) };
            Point[] rightTriangle = new Point[3] { new Point(rightPos, aNodeOver.Bounds.Bottom - 4), new Point(rightPos, aNodeOver.Bounds.Bottom + 4), new Point(rightPos - 4, aNodeOver.Bounds.Bottom) };
            g.FillPolygon(System.Drawing.Brushes.Black, leftTriangle);
            g.FillPolygon(System.Drawing.Brushes.Black, rightTriangle);
            g.DrawLine(new System.Drawing.Pen(Color.Black, 2), new Point(leftPos, aNodeOver.Bounds.Bottom), new Point(rightPos, aNodeOver.Bounds.Bottom));
        }

        private void ScrollOnDrag(object sender, DragEventArgs e) {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode node = ((TreeView)sender).GetNodeAt(pt);
            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - iScrollOnDragTicks);
            
            if (node != null) {
                //scroll up
                if (pt.Y < (((TreeView)sender).ItemHeight) - ((TreeView)sender).Top) {
                    // if within one node of top, scroll quickly
                    if (node.PrevVisibleNode != null) {
                        node = node.PrevVisibleNode;
                    }
                    node.EnsureVisible();
                    iScrollOnDragTicks = DateTime.Now.Ticks;
                }
                else if (pt.Y < ((((TreeView)sender).ItemHeight * 2) - ((TreeView)sender).Top)) {
                    // if within two nodes of the top, scroll slowly
                    if (ts.TotalMilliseconds > 250) {
                        node = node.PrevVisibleNode;
                        if (node.PrevVisibleNode != null) {
                            node = node.PrevVisibleNode;
                        }
                        node.EnsureVisible();
                        iScrollOnDragTicks = DateTime.Now.Ticks;
                    }
                }
                //scroll down
                else if (pt.Y > (((TreeView)sender).Bottom - (((TreeView)sender).ItemHeight + 20))) {
                    // if within one node of bottom, scroll quickly (20 for scrollbar)
                    if (node.NextVisibleNode != null) {
                        node = node.NextVisibleNode;
                    }
                    node.EnsureVisible();
                    iScrollOnDragTicks = DateTime.Now.Ticks;
                }
                else if (pt.Y > (((TreeView)sender).Bottom - (((TreeView)sender).ItemHeight * 2 + 20))) {
                    // if within two nodes of the bottom, scroll slowly (20 for scrollbar)
                    if (ts.TotalMilliseconds > 250) {
                        node = node.NextVisibleNode;
                        if (node.NextVisibleNode != null) {
                            node = node.NextVisibleNode;
                        }
                        node.EnsureVisible();
                        iScrollOnDragTicks = DateTime.Now.Ticks;
                    }
                }
            }
        }

        private void treeViewPreset_AfterSelect(object sender, TreeViewEventArgs e) {
            if (treeViewPreset.SelectedNode == null || treeViewPreset.SelectedNode.Level > 2) {
                toolStripDetailsLabel.Text = "Details";
                renameToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
                copyToolStripMenuItem.Enabled = false;
                pasteToolStripMenuItem.Enabled = (iCopiedNode != null && iCopiedNode.Level == 0);
                countToolStripMenuItemPreset.Text = "No selection";
                statusInfoRelative.Text = countToolStripMenuItemPreset.Text;
                panelNoDetails.Visible = true;
                panelBookmarkDetails.Visible = false;
                panelPresetDetails.Visible = false;
                panelTrackDetails.Visible = false;
            }
            else {
                deleteToolStripMenuItem.Enabled = true;
                copyToolStripMenuItem.Enabled = true;

                if (treeViewPreset.SelectedNode.Level == 0) {
                    toolStripDetailsLabel.Text = "Bookmark Details";
                    renameToolStripMenuItem.Enabled = true;
                    pasteToolStripMenuItem.Enabled = (iCopiedNode != null && (iCopiedNode.Level == 0 || iCopiedNode.Level == 1));
                    textBoxBookmark.Text = treeViewPreset.SelectedNode.Text;
                    countToolStripMenuItemPreset.Text = "Count: " + treeViewPreset.SelectedNode.Nodes.Count.ToString();
                    statusInfoRelative.Text = countToolStripMenuItemPreset.Text;
                    panelNoDetails.Visible = false;
                    panelBookmarkDetails.Visible = true;
                    panelPresetDetails.Visible = false;
                    panelTrackDetails.Visible = false;
                    listViewPresets.Items.Clear();
                    foreach (TreeNode node in treeViewPreset.SelectedNode.Nodes) {
                        ListViewItem item = new ListViewItem(node.Text);
                        item.SubItems.Add(node.Name);
                        item.Tag = node;
                        listViewPresets.Items.Add(item);
                    }
                }
                else if (treeViewPreset.SelectedNode.Level == 1) {
                    toolStripDetailsLabel.Text = "Preset Details";
                    renameToolStripMenuItem.Enabled = true;
                    warningPanel.Visible = (treeViewPreset.SelectedNode.Nodes.Count > MediaCollection.kPlaylistMaxTracks);
                    pasteToolStripMenuItem.Enabled = (iCopiedNode != null && (iCopiedNode.Level == 1 || iCopiedNode.Level == 2));
                    textBoxPreset.Text = treeViewPreset.SelectedNode.Text;
                    textBoxPresetNumber.Text = treeViewPreset.SelectedNode.Name;
                    countToolStripMenuItemPreset.Text = "Count: " + treeViewPreset.SelectedNode.Nodes.Count.ToString();
                    statusInfoRelative.Text = countToolStripMenuItemPreset.Text;
                    panelNoDetails.Visible = false;
                    panelBookmarkDetails.Visible = false;
                    panelPresetDetails.Visible = true;
                    panelTrackDetails.Visible = false;
                    listViewPlaylist.Items.Clear();
                    foreach (TreeNode node in treeViewPreset.SelectedNode.Nodes) {
                        TrackMetadata data = (TrackMetadata)node.Tag;
                        if (data != null) {
                            ListViewItem item = new ListViewItem((node.Index + 1).ToString());
                            item.SubItems.Add(data.Title);
                            item.SubItems.Add(data.Artist);
                            item.SubItems.Add(data.Album);
                            item.SubItems.Add(data.Track.ToString());
                            item.SubItems.Add(data.Genre);
                            item.SubItems.Add(data.Composer);
                            item.SubItems.Add(data.Conductor);
                            item.SubItems.Add(data.AlbumArtist);
                            item.SubItems.Add(data.Year);
                            item.SubItems.Add(data.Duration);
                            item.SubItems.Add(data.Disc.ToString());
                            item.Tag = data;
                            listViewPlaylist.Items.Add(item);
                        }
                    }
                }
                else if (treeViewPreset.SelectedNode.Level == 2) {
                    // show track details
                    toolStripDetailsLabel.Text = "Track Details";
                    renameToolStripMenuItem.Enabled = false;
                    pasteToolStripMenuItem.Enabled = (iCopiedNode != null && (iCopiedNode.Level == 1 || iCopiedNode.Level == 2));
                    countToolStripMenuItemPreset.Text = "Index: " + (treeViewPreset.SelectedNode.Index + 1).ToString();
                    statusInfoRelative.Text = countToolStripMenuItemPreset.Text;
                    TrackMetadata data = (TrackMetadata)treeViewPreset.SelectedNode.Tag;
                    textBoxMetadata.Text = data.ToString();
                    panelAlbumArt.Tag = data;
                    panelNoDetails.Visible = false;
                    panelBookmarkDetails.Visible = false;
                    panelPresetDetails.Visible = false;
                    panelTrackDetails.Visible = true;
                    panelTrackDetails.Refresh();
                }
            }
        }

        private void treeViewPreset_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
            treeViewPreset_AfterSelect(this, null);
        }

        private void listViewPlaylist_DoubleClick(object sender, EventArgs e) {
            if (listViewPlaylist.SelectedItems[0] != null) {
                FormTrackMetadata form = new FormTrackMetadata((TrackMetadata)listViewPlaylist.SelectedItems[0].Tag);
                form.ShowDialog();
                form.Dispose();
            }
        }

        private void panelTrackDetails_Paint(object sender, PaintEventArgs e) {
            if ((treeViewPreset.SelectedNode != null && treeViewPreset.SelectedNode.Level == 2) ||
                (treeViewScan.SelectedNode != null && treeViewScan.SelectedNode.Level == 2)) {
                if (panelAlbumArt.Tag != null) {
                    // insure album art panel is always square
                    int sizeDiff = panelTrackDetails.Size.Width - panelTrackDetails.Size.Height;
                    if (sizeDiff > 0) {
                        panelAlbumArt.Size = new Size(panelTrackDetails.Size.Height, panelTrackDetails.Size.Height);
                    }
                    else {
                        panelAlbumArt.Size = new Size(panelTrackDetails.Size.Width, panelTrackDetails.Size.Width);
                    }
                    int newHeight = panelTrackDetails.Size.Height - panelAlbumArt.Size.Height;
                    if (newHeight > 6) {
                        newHeight -= 6;
                    }
                    textBoxMetadata.Size = new Size(panelTrackDetails.Size.Width, newHeight);
                    textBoxMetadata.Location = new Point(0, panelAlbumArt.Size.Height + 6);
                    TrackMetadata data = (TrackMetadata)panelAlbumArt.Tag;
                    try {
                        pictureBoxAlbumArt.SizeMode = PictureBoxSizeMode.StretchImage;
                        pictureBoxAlbumArt.LoadAsync(data.AlbumArtPath);
                    }
                    catch (Exception) {
                        pictureBoxAlbumArt.SizeMode = PictureBoxSizeMode.CenterImage;
                        pictureBoxAlbumArt.Image = Linn.Kinsky.Properties.Resources.NoAlbumArt;
                    }
                }
            }
        }

        private void deleteButton_Click(object sender, EventArgs e) {
            DeleteSelectedNode();
        }

        private void alphabetizeToolStripMenuItem_Click(object sender, EventArgs e) {
            // treeViewPreset.Sort sorts every node and subnode
            // this will only sort at the level selected
            if (treeViewPreset.SelectedNode == null) {
                AlphabetizePresetNodes(treeViewPreset.Nodes);
                AssignNumbers();
            }
            else if (treeViewPreset.SelectedNode.Level <= 2) {
                AlphabetizePresetNodes(treeViewPreset.SelectedNode.Nodes);
                // if on Title A to Z node - alphabatize sub nodes as well
                if (treeViewPreset.SelectedNode.Level == 0 && 
                    treeViewPreset.SelectedNode.Text == MediaCollection.SortTypeToString(MediaCollection.SortType.eTitleAz)) {
                    foreach (TreeNode node in treeViewPreset.SelectedNode.Nodes) {
                        AlphabetizePresetNodes(node.Nodes);
                    }
                }
                AssignNumbers();
            }
        }

        private void AlphabetizePresetNodes(TreeNodeCollection aNodes) {
            List<TreeNode> list = new List<TreeNode>();
            foreach (TreeNode childNode in aNodes) {
                list.Add(childNode);
            }
            list.Sort(iTreeNodeComparer);
            treeViewPreset.BeginUpdate();
            aNodes.Clear();
            foreach (TreeNode childNode in list) {
                aNodes.Add(childNode);
            }
            treeViewPreset.EndUpdate();
        }

        public class TreeNodeComparer : IComparer<TreeNode>
        {
            public int Compare(TreeNode x, TreeNode y) {
                return (x.Text.CompareTo(y.Text));
            }
        }

        private void toolStripButtonAddBookmark_Click(object sender, EventArgs e) {
            TreeNode newBookmark = new TreeNode("New Bookmark " + (treeViewPreset.Nodes.Count + 1));
            newBookmark.Name = (treeViewPreset.Nodes.Count + 1).ToString();
            newBookmark.ImageIndex = 0;
            newBookmark.SelectedImageIndex = 0;
            if (treeViewPreset.SelectedNode != null && treeViewPreset.SelectedNode.Level == 0) {
                treeViewPreset.Nodes.Insert(treeViewPreset.SelectedNode.Index, newBookmark);
            }
            else {
                treeViewPreset.Nodes.Add(newBookmark);
            }
            AssignNumbers();
            treeViewPreset.SelectedNode = newBookmark;
            RenameNode();
        }

        private void toolStripButtonAddPreset_Click(object sender, EventArgs e) {
            if (treeViewPreset.SelectedNode != null) {
                if (treeViewPreset.SelectedNode.Level == 0) {
                    TreeNode newPreset = new TreeNode("New Preset " + (treeViewPreset.SelectedNode.Nodes.Count + 1));
                    newPreset.Name = (treeViewPreset.SelectedNode.Nodes.Count + 1).ToString();
                    newPreset.ImageIndex = 1;
                    newPreset.SelectedImageIndex = 1;
                    treeViewPreset.SelectedNode.Nodes.Add(newPreset);
                    AssignNumbers();
                    treeViewPreset.SelectedNode = newPreset;
                    RenameNode();
                    return;
                }
                else if (treeViewPreset.SelectedNode.Level == 1) {
                    TreeNode newPreset = new TreeNode("New Preset " + (treeViewPreset.SelectedNode.Parent.Nodes.Count + 1));
                    newPreset.Name = (treeViewPreset.SelectedNode.Parent.Nodes.Count + 1).ToString();
                    newPreset.ImageIndex = 1;
                    newPreset.SelectedImageIndex = 1;
                    treeViewPreset.SelectedNode.Parent.Nodes.Insert(treeViewPreset.SelectedNode.Index, newPreset);
                    AssignNumbers();
                    treeViewPreset.SelectedNode = newPreset;
                    RenameNode();
                    return;
                }
            }
            MessageBox.Show("Presets can only be added to active bookmarks", "Add Preset Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e) {
            RenameNode();
        }

        private void treeViewPreset_MouseDown(object sender, MouseEventArgs e) {
            treeViewPreset.SelectedNode = treeViewPreset.GetNodeAt(e.X, e.Y);

            if (treeViewPreset.SelectedNode == null || treeViewPreset.SelectedNode.Level > 2) {
                toolStripDetailsLabel.Text = "Details";
                renameToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
                copyToolStripMenuItem.Enabled = false;
                pasteToolStripMenuItem.Enabled = (iCopiedNode != null && iCopiedNode.Level == 0);
                countToolStripMenuItemPreset.Text = "No selection";
                statusInfoRelative.Text = countToolStripMenuItemPreset.Text;
                panelNoDetails.Visible = true;
                panelBookmarkDetails.Visible = false;
                panelPresetDetails.Visible = false;
                panelTrackDetails.Visible = false;
            }
        }

        private void treeViewScan_MouseDown(object sender, MouseEventArgs e) {
            treeViewScan.SelectedNode = treeViewScan.GetNodeAt(e.X, e.Y);
            if (treeViewScan.SelectedNode == null) {
                countToolStripMenuItemScan.Text = "No selection";
                statusInfoRelative.Text = countToolStripMenuItemScan.Text;
                addToolStripMenuItem.Enabled = false;
                toolStripButtonAdd.Enabled = false;
                playToolStripMenuItem.Enabled = false;
                toolStripButtonPlay.Enabled = false;
            }
        }

        private void treeViewPreset_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
            if (e.Label != null && e.Label.Trim().Length > 0) {
                e.Node.EndEdit(false);
                treeViewPreset.SelectedNode.Text = e.Label;
                ReselectCurrentNode();
            }
            else {
                e.Node.EndEdit(true);
            }
            treeViewPreset.LabelEdit = false;
        }

        private void treeViewScan_DoubleClick(object sender, EventArgs e) {
            if (treeViewScan.SelectedNode != null && treeViewScan.SelectedNode.Level == 2) {
                TrackMetadata data = (TrackMetadata)treeViewScan.SelectedNode.Tag;
                try {
                    if (data != null) {
                        System.Diagnostics.Process.Start(data.FilePath);
                    }
                }
                catch (Exception exc) {
                    Linn.UserLog.WriteLine("Could not play track: " + data.FilePath + ":" + Environment.NewLine + exc.Message);
                    MessageBox.Show("Could not open: " + data.FilePath + ":" + Environment.NewLine + exc.Message, "Could not open file", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        public void syncButton_Click(object sender, EventArgs e) {
            RemoveExampleBookmark();
            if (iLastScanFull) {
                // smart add - replace existing nodes and add new nodes (use with full scan)
                AddAll();
            }
            else {
                // append to matching nodes or create neww node if no match (use with quick scan only)
                if (treeViewScan.Nodes.Count > 0) {
                    TreeNode newNode = null;
                    foreach (TreeNode bookmark in treeViewScan.Nodes) { // scanned bookmarks
                        bool bookmarkFound = false;
                        foreach (TreeNode bookmarkCurrent in treeViewPreset.Nodes) { // current bookmarks
                            if (bookmarkCurrent.Text == bookmark.Text) { // bookmark match
                                foreach (TreeNode preset in bookmark.Nodes) { // scanned prests
                                    bool presetFound = false;
                                    foreach (TreeNode presetCurrent in bookmarkCurrent.Nodes) { // current presets
                                        if (presetCurrent.Text == preset.Text) { // preset match
                                            foreach (TreeNode track in preset.Nodes) { // scanned tracks
                                                bool trackFound = false;
                                                foreach (TreeNode trackCurrent in presetCurrent.Nodes) { // current tracks
                                                    if (trackCurrent.Text == track.Text) { // track match
                                                        trackFound = true;
                                                        break;
                                                    }
                                                }
                                                if (!trackFound) {
                                                    newNode = (TreeNode)track.Clone();
                                                    presetCurrent.Nodes.Add(newNode);
                                                }
                                            }
                                            presetFound = true;
                                            break;
                                        }
                                    }
                                    if (!presetFound) {
                                        newNode = (TreeNode)preset.Clone();
                                        bookmarkCurrent.Nodes.Add(newNode);
                                    }
                                }
                                bookmarkFound = true;
                                break;
                            }
                        }
                        if (!bookmarkFound) {
                            newNode = (TreeNode)bookmark.Clone();
                            treeViewPreset.Nodes.Add(newNode);
                        }
                    }
                    AssignNumbers();
                }
            }
            if (iWizardInProgress) {
                iFormWizard.WizardSyncComplete();
            }
        }

        public void exportToLinnDSToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!VerifyCollectionLocation()) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return;
            }
            if (!VerifyHttpLocation()) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return;
            }
            FormDevices form = null;
            DialogResult result;
            try {
                form = new FormDevices(iHelper);
                result = form.ShowDialog();
            }
            catch {
                result = DialogResult.Cancel;
            }
            if (iWizardInProgress) {
                if (result == DialogResult.Cancel) {
                    iFormWizard.WizardCancel();
                }
                else {
                    iFormWizard.WizardSyncWithDsComplete(false);
                }
            }
            if (form != null) {
                form.Dispose();
            }
        }

        public void printToolStripMenuItem_Click(object sender, EventArgs e) {
            // clear existing progress info
            synchProgressBar.Value = 0;
            synchLabel.Text = "";

            iPrint = new FormPrint(iHelper, treeViewPreset, false, iPresetCount, PrintingProgress);
            DialogResult result = iPrint.ShowDialog();
            if (result == DialogResult.Cancel) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
            }
            iPrint.Dispose();
        } 

        private void printPreviewButton_Click(object sender, EventArgs e) {
            // clear existing progress info
            synchProgressBar.Value = 0;
            synchLabel.Text = "";

            iPrint = new FormPrint(iHelper, treeViewPreset, true, iPresetCount, PrintingProgress);
            iPrint.ShowDialog();
            iPrint.Dispose();
        }

        public void importButton_Click(object sender, EventArgs e) {
            if (!VerifyCollectionLocation()) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return;
            }
            if (!VerifyHttpLocation()) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return;
            }
            RemoveExampleBookmark();
            if (iWizardInProgress) {
                if ((bool)sender) {
                    treeViewPreset.Nodes.Clear();
                }
            }
            else if (treeViewPreset.Nodes.Count > 0) {
                DialogResult result = MessageBox.Show("Do you want to clear the presets window before importing your saved presets?", "Import: Clear Presets Window", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes) {
                    treeViewPreset.Nodes.Clear();
                }
            }
            // clear existing
            synchProgressBar.Value = 0;
            synchLabel.Text = "";
            if (!iWizardInProgress) {
                statusStripSynch.Visible = true;
                SetSynchFunctionsEnabled(false);
            }
            // import presets
            iPresets = new Presets(iHelper, treeViewPreset.Nodes, UpdateImportProgress, AddImportedNode);
            if (!iWizardInProgress) {
                iPresets.Import();
            }
            else {
                iPresets.Import(iHelper.OptionPageWizard.CorrectIp);
            }
        }

        public void exportButton_Click(object sender, EventArgs e) {
            if (!VerifyCollectionLocation()) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return;
            }
            if (!VerifyHttpLocation()) {
                if (iWizardInProgress) {
                    iFormWizard.WizardCancel();
                }
                return;
            }
            // clear existing
            synchProgressBar.Value = 0;
            synchLabel.Text = "";
            if (!iWizardInProgress) {
                statusStripSynch.Visible = true;
                SetSynchFunctionsEnabled(false);
            }
            // export presets
            iPresets = new Presets(iHelper, treeViewPreset.Nodes, UpdateExportProgress);
            iPresets.Export();
        }

        public bool VerifyHttpLocation() {
            // verify http location
            bool uriDone = false;
            bool configRequired = false;
            while (!uriDone) {
                try {
                    if (iHelper.OptionPageSetup.UseHttpServer) {
                        if (iHelper.Stack.Status.State == EStackState.eOk) {
                            uriDone = true;
                        }
                        else {
                            configRequired = true;
                            DialogResult result = MessageBox.Show("Please insure you have selected a valid network interface (Tools/Options/Network)", "Kinsky Jukebox Http Server Not Running", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand);
                            if (result != DialogResult.OK) {
                                return false;
                            }
                            ShowOptionsDialog(true, false);
                        }
                    }
                    else {
                        Uri httpLocation = new Uri(iHelper.OptionPageSetup.CollectionHttpLocation);
                        if (httpLocation.Scheme != Uri.UriSchemeHttp) {
                            throw new Linn.Network.NetworkError();
                        }
                        uriDone = !httpLocation.IsFile;
                    }
                }
                catch (Exception) {
                    configRequired = true;
                    DialogResult result = MessageBox.Show("The URL of your Scan Directory (web accessible address of scan directory) is either invalid or has not been configured." + Environment.NewLine + "Click 'OK' to configure this setting." + Environment.NewLine + Environment.NewLine + "If you are using a NAS based webserver, please enter a valid http location (i.e. http://<ip address of nas>/<music share>)." + Environment.NewLine + "Click the 'Test' button to insure this location opens in a web browser and points to your Scan Directory (root directory of your music collection)." + Environment.NewLine + Environment.NewLine + "Otherwise, you can allow Kinsky Jukebox to run as an Http server by ticking this option box." + Environment.NewLine + "Kinsky Jukebox would need to remain running during music playback as it would be serving the music to your DS device.", "URL of Scan Directory Not Configured", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                    if (result != DialogResult.OK) {
                        return false;
                    }
                    ShowOptionsDialog(false, true);
                }
            }
            if (iWizardInProgress) {
                iFormWizard.WizardScanDirectoryUrlConfigComplete(configRequired);
            }
            return true;
        }

        public bool VerifyCollectionLocation() {
            if (!Directory.Exists(iHelper.OptionPageSetup.CollectionLocation)) {
                // get new collection location if stored location can not be found
                SelectNewMediaCollectionLocation();
                if (!Directory.Exists(iHelper.OptionPageSetup.CollectionLocation)) {
                    return false; // if new location not found, exit
                }
            }
            if (iWizardInProgress) {
                iFormWizard.WizardScanDirectoryConfigComplete();
            }
            return true;
        }

        private void SelectNewMediaCollectionLocation() {
            // get collection directory
            FolderBrowserDialog browse = new FolderBrowserDialog();
            browse.Description = "Please select your Scan Directory (root directory of your music collection)." + Environment.NewLine + "The existing directory is either invalid or has not been configured.";
            browse.ShowNewFolderButton = false;
            string location = null;
            DialogResult result = browse.ShowDialog();
            if (result == DialogResult.OK) {
                location = browse.SelectedPath.Trim();
                iHelper.OptionPageSetup.CollectionLocation = location;
            }
        }

        private void UpdateScanProgress(int aPercent, string aMessage, Progress.State aProgressState) {
            if (this.InvokeRequired) {
                // no need to block
                this.BeginInvoke(new DEventProgress(UpdateScanProgress), new object[] { aPercent, aMessage, aProgressState });
                return;
            }
            if (aProgressState == Progress.State.eSuccess) {
                statusInfoTotal.Text = "Total Tracks Scanned: " + aPercent;
            }
            if (iWizardInProgress) {
                iFormWizard.ScanProgress(aPercent, aMessage, aProgressState);
            }
            else if (aProgressState == Progress.State.eFail) {
                MessageBox.Show(aMessage, "Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
            else if (aProgressState == Progress.State.eInProgress) {
                if ((aPercent >= 0) && (aPercent <= 100)) {
                    ScanProgressBar.Value = aPercent;
                }
                if (aMessage != null && ScanLabel.Text != aMessage) {
                    ScanLabel.Text = aMessage;
                }
            }
            else if (aProgressState == Progress.State.eComplete) {
                statusStripScan.Visible = false;
                toolStripButtonScan.Enabled = true;
                toolStripButtonQuickScan.Enabled = true;
                toolStripButtonWizard.Enabled = true;
                scanCollectionToolStripMenuItem.Enabled = true;
                quickScanToolStripMenuItem.Enabled = true;
                oneClickWizardToolStripMenuItem.Enabled = true;
            }
        }

        private void UpdateExportProgress(int aPercent, string aMessage, Progress.State aProgressState) {
            if (this.InvokeRequired) {
                // no need to block
                this.BeginInvoke(new DEventProgress(UpdateExportProgress), new object[] { aPercent, aMessage, aProgressState });
                return;
            }
            if (iWizardInProgress) {
                iFormWizard.ExportProgress(aPercent, aMessage, aProgressState);
            }
            else if (aProgressState == Progress.State.eFail) {
                MessageBox.Show(aMessage, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
            else if (aProgressState == Progress.State.eSuccess) {
                DialogResult result = MessageBox.Show("Preset URL Prefix: " + Presets.UriPath(iHelper) + Environment.NewLine + Environment.NewLine + "Would you like to Sync with your Linn DS?", "Created " + aPercent + " Presets", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                if (result == DialogResult.Yes) {
                    exportToLinnDSToolStripMenuItem_Click(this, EventArgs.Empty);
                }
            }
            else if (aProgressState == Progress.State.eInProgress) {
                if ((aPercent >= 0) && (aPercent <= 100)) {
                    synchProgressBar.Value = aPercent;
                }
                if (aMessage != null && synchLabel.Text != aMessage) {
                    synchLabel.Text = aMessage;
                }
            }
            else if (aProgressState == Progress.State.eComplete) {
                statusStripSynch.Visible = false;
                SetSynchFunctionsEnabled(true);
            }
        }

        private void UpdateImportProgress(int aPercent, string aMessage, Progress.State aProgressState) {
            if (this.InvokeRequired) {
                // no need to block
                this.BeginInvoke(new DEventProgress(UpdateImportProgress), new object[] { aPercent, aMessage, aProgressState });
                return;
            }
            if (iWizardInProgress) {
                iFormWizard.ImportProgress(aPercent, aMessage, aProgressState);
            }
            else if (aProgressState == Progress.State.eFail) {
                MessageBox.Show(aMessage, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            
            }
            else if (aProgressState == Progress.State.eInProgress) {
                if ((aPercent >= 0) && (aPercent <= 100)) {
                    synchProgressBar.Value = aPercent;
                }
                if (aMessage != null && synchLabel.Text != aMessage) {
                    synchLabel.Text = aMessage;
                }
            }
            else if (aProgressState == Progress.State.eComplete) {
                statusStripSynch.Visible = false;
                SetSynchFunctionsEnabled(true);
                AssignNumbers();
            }
        }

        private void PrintingProgress(int aPercent, string aMessage, Progress.State aProgressState) {
            if (this.InvokeRequired) {
                // no need to block
                this.BeginInvoke(new DEventProgress(PrintingProgress), new object[] { aPercent, aMessage, aProgressState });
                return;
            }
            if (iWizardInProgress) {
                iFormWizard.PrintingProgress(aPercent, aMessage, aProgressState);
            }
            else if (aProgressState == Progress.State.eFail) {
                MessageBox.Show(aMessage, "Print Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
            else if (aProgressState != Progress.State.eSuccess) {
                if ((aPercent >= 0) && (aPercent <= 100)) {
                    synchProgressBar.Value = aPercent;
                }
                if (aMessage != null && synchLabel.Text != aMessage) {
                    synchLabel.Text = aMessage;
                }
                if (aProgressState == Progress.State.eComplete) {
                    statusStripSynch.Visible = false;
                    SetSynchFunctionsEnabled(true);
                }
                else if (!statusStripSynch.Visible) {
                    statusStripSynch.Visible = true;
                    SetSynchFunctionsEnabled(false);
                }
            }
        }

        private void SetSynchFunctionsEnabled(bool aEnabled) {
            exportButton.Enabled = aEnabled;
            importButton.Enabled = aEnabled;
            syncButton.Enabled = aEnabled;
            toolStripButtonDsExport.Enabled = aEnabled;
            printButton.Enabled = aEnabled;
            printPreviewButton.Enabled = aEnabled;
            exportPresetsToolStripMenuItem.Enabled = aEnabled;
            importPresetsToolStripMenuItem.Enabled = aEnabled;
            syncToolStripMenuItem.Enabled = aEnabled;
            exportToLinnDSToolStripMenuItem.Enabled = aEnabled;
            printToolStripMenuItem.Enabled = aEnabled;
            printPreviewToolStripMenuItem.Enabled = aEnabled;
        }

        private void treeViewScan_AfterSelect(object sender, TreeViewEventArgs e) {
            addToolStripMenuItem.Enabled = false;
            toolStripButtonAdd.Enabled = false;
            if (treeViewScan.SelectedNode == null || treeViewScan.SelectedNode.Level > 2) {
                countToolStripMenuItemScan.Text = "No selection";
                statusInfoRelative.Text = countToolStripMenuItemScan.Text;
                playToolStripMenuItem.Enabled = false;
                toolStripButtonPlay.Enabled = false;
            }
            else if (treeViewScan.SelectedNode.Level == 0) {
                countToolStripMenuItemScan.Text = treeViewScan.SelectedNode.ToolTipText;
                statusInfoRelative.Text = countToolStripMenuItemScan.Text;
                playToolStripMenuItem.Enabled = false;
                toolStripButtonPlay.Enabled = false;

                if (treeViewPreset.SelectedNode == null || treeViewPreset.SelectedNode.Level == 0) {
                    addToolStripMenuItem.Enabled = true;
                    toolStripButtonAdd.Enabled = true;
                }
            }
            else if (treeViewScan.SelectedNode.Level == 1) {
                countToolStripMenuItemScan.Text = treeViewScan.SelectedNode.ToolTipText;
                statusInfoRelative.Text = countToolStripMenuItemScan.Text;
                playToolStripMenuItem.Enabled = false;
                toolStripButtonPlay.Enabled = false;

                if (treeViewPreset.SelectedNode != null) {
                    if (treeViewPreset.SelectedNode.Level >= 0 && treeViewPreset.SelectedNode.Level <= 2) {
                        addToolStripMenuItem.Enabled = true;
                        toolStripButtonAdd.Enabled = true;
                    }
                }
            }
            else if (treeViewScan.SelectedNode.Level == 2) {
                countToolStripMenuItemScan.Text = "Index: " + (treeViewScan.SelectedNode.Index + 1).ToString();
                statusInfoRelative.Text = countToolStripMenuItemScan.Text;
                playToolStripMenuItem.Enabled = true;
                toolStripButtonPlay.Enabled = true;

                if (treeViewPreset.SelectedNode != null) {
                    if (treeViewPreset.SelectedNode.Level == 1 || treeViewPreset.SelectedNode.Level == 2) {
                        addToolStripMenuItem.Enabled = true;
                        toolStripButtonAdd.Enabled = true;
                    }
                }

                // show track details
                toolStripDetailsLabel.Text = "Track Details";
                TrackMetadata data = (TrackMetadata)treeViewScan.SelectedNode.Tag;
                textBoxMetadata.Text = data.ToString();
                panelAlbumArt.Tag = data;
                panelNoDetails.Visible = false;
                panelBookmarkDetails.Visible = false;
                panelPresetDetails.Visible = false;
                panelTrackDetails.Visible = true;
                panelTrackDetails.Refresh();
            }
        }

        private void treeViewScan_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
            treeViewScan_AfterSelect(this, null);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
            iCopiedNode = treeViewPreset.SelectedNode;
            treeViewPreset_AfterSelect(this, null);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e) {
            if (iCopiedNode != null) {
                TreeNode newNode = (TreeNode)iCopiedNode.Clone();
                if (treeViewPreset.SelectedNode == null) {
                    if (iCopiedNode.Level == 0) {
                        // copy bookmark to empty location
                        treeViewPreset.Nodes.Add(newNode);
                    }
                    else {
                        return; // can't copy preset/track to empty location
                    }
                }
                else if (treeViewPreset.SelectedNode.Level == 0) {
                    if (iCopiedNode.Level == 0) {
                        // copy bookmark to bookmark 
                        treeViewPreset.Nodes.Insert(treeViewPreset.SelectedNode.Index, newNode);
                    }
                    else if (iCopiedNode.Level == 1) {
                        // copy preset to bookmark 
                        treeViewPreset.SelectedNode.Nodes.Add(newNode);
                    }
                    else {
                        return; // can't copy track to active bookmark
                    }
                }
                else if (treeViewPreset.SelectedNode.Level == 1) {
                    if (iCopiedNode.Level == 1) {
                        // copy preset to preset - add all tracks from selected preset into destination preset
                        foreach (TreeNode node in iCopiedNode.Nodes) {
                            newNode = (TreeNode)node.Clone();
                            treeViewPreset.SelectedNode.Nodes.Add(newNode);
                        }
                    }
                    else if (iCopiedNode.Level == 2) {
                        // copy track to preset
                        treeViewPreset.SelectedNode.Nodes.Add(newNode);
                    }
                    else {
                        return; // can't copy bookmark to active preset
                    }
                }
                else if (treeViewPreset.SelectedNode.Level == 2) {
                    if (iCopiedNode.Level == 1) {
                        // copy preset to track - add all tracks from selected preset into destination preset
                        foreach (TreeNode node in iCopiedNode.Nodes) {
                            newNode = (TreeNode)node.Clone();
                            treeViewPreset.SelectedNode.Parent.Nodes.Insert(treeViewPreset.SelectedNode.Index, newNode);
                        }
                    }
                    else if (iCopiedNode.Level == 2) {
                        // copy track to track
                        treeViewPreset.SelectedNode.Parent.Nodes.Insert(treeViewPreset.SelectedNode.Index, newNode);
                    }
                    else {
                        return; // can't copy bookmark to active track
                    }
                }
                else {
                    return; // no active preset
                }
                AssignNumbers();
                treeViewPreset.SelectedNode = newNode; // insure node being sent is selected
            }
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e) {
            CheckForUpdates(true);
        }

        private void CheckForUpdates(bool aUserRequested) {
            Linn.UserLog.WriteLine("Check for updates");
            iFormUpdate = new FormUpdate(iHelper, EventCheckForUpdatesComplete, EventDownloadUpdateComplete);
            iFormUpdate.CheckForUpdates(aUserRequested); // starts update check in a seaparate thread, calls back when complete
        }

        private void EventCheckForUpdatesComplete(bool aShow) {
            this.Invoke((MethodInvoker)delegate() {
                if (aShow) {
                    iFormUpdate.ShowDialog();
                    iFormUpdate.Dispose();
                }
            });
        }

        private void EventDownloadUpdateComplete(bool aClose) {
            this.Invoke((MethodInvoker)delegate() {
                if (aClose) {
                    iFormUpdate.Dispose();
                    this.Close();
                }
            });
        }

        private void resetUserInterfaceToolStripMenuItem_Click(object sender, EventArgs e) {
            iHelper.ApplicationOptions.ResetToDefaults();
            SetupUserInterface();
        }

        private void onlineHelpToolStripMenuItem_Click(object sender, EventArgs e) {
            Linn.UserLog.WriteLine("Online help");
            string url = "http://oss.linn.co.uk/trac/wiki/KinskyJukeboxManual";
            try {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception exc) {
                Linn.UserLog.WriteLine("Online Help Failed: " + exc.Message);
                MessageBox.Show("Could not load: " + url, "Online Help Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void textBoxPreset_TextChanged(object sender, EventArgs e) {
            if (treeViewPreset.SelectedNode != null && treeViewPreset.SelectedNode.Level == 1) {
                if (textBoxPreset.Text != null && textBoxPreset.Text.Length > 0) {
                    treeViewPreset.SelectedNode.Text = textBoxPreset.Text;
                }
            }
        }


        private void textBoxBookmark_TextChanged(object sender, EventArgs e) {
            if (treeViewPreset.SelectedNode != null && treeViewPreset.SelectedNode.Level == 0) {
                if (textBoxBookmark.Text != null && textBoxBookmark.Text.Length > 0) {
                    treeViewPreset.SelectedNode.Text = textBoxBookmark.Text;
                }
            }
        }

        private void DeleteSelectedNode() {
            if (treeViewPreset.SelectedNode != null) {
                treeViewPreset.Nodes.Remove(treeViewPreset.SelectedNode);
                AssignNumbers();
                ReselectCurrentNode();
            }
        }

        private void ReselectCurrentNode() {
            treeViewPreset_AfterSelect(this, null);
        }

        private void RenameNode() {
            if (treeViewPreset.SelectedNode != null && treeViewPreset.SelectedNode.Level < 2) {
                if (!treeViewPreset.SelectedNode.IsEditing) {
                    treeViewPreset.LabelEdit = true;
                    treeViewPreset.SelectedNode.BeginEdit();
                }
            }
        }

        private void RemoveExampleBookmark() {
            // remove example bookmark node if still there (and has no presets)
            if (treeViewPreset.Nodes.Count == 1 && treeViewPreset.Nodes[0].Nodes.Count == 0 && treeViewPreset.Nodes[0].Text == kExampleBookmarkName) {
                treeViewPreset.Nodes.Clear();
            }
        }

        public void AssignNumbers() {
            iBookmarkCount = 0;
            iPresetCount = 0;
            if (treeViewPreset.Nodes.Count > 0) {
                foreach (TreeNode bookmark in treeViewPreset.Nodes) {
                    bookmark.Name = (++iBookmarkCount).ToString();
                    if (bookmark.Nodes.Count > 0) {
                        foreach (TreeNode preset in bookmark.Nodes) {
                            preset.Name = (++iPresetCount).ToString();
                            if (preset.Nodes.Count > MediaCollection.kPlaylistMaxTracks) {
                                if (preset.ImageIndex != 3) {
                                    preset.ImageIndex = 3;
                                    preset.SelectedImageIndex = 3;
                                }
                            }
                            else if (preset.ImageIndex != 1) {
                                preset.ImageIndex = 1;
                                preset.SelectedImageIndex = 1;
                            }
                        }
                    }
                }
            }
        }

        private void AddAll() {
            // smart add - replace existing nodes and add new nodes (use with full scan)
            // if bookmark already exists then replace it, otherwise add it to the end
            if (treeViewScan.Nodes.Count > 0) {
                foreach (TreeNode bookmark in treeViewScan.Nodes) {
                    TreeNode newNode = (TreeNode)bookmark.Clone();
                    bool bookmarkFound = false;
                    foreach (TreeNode bookmarkCurrent in treeViewPreset.Nodes) {
                        if (bookmarkCurrent.Text == bookmark.Text) {
                            // replace if already exists
                            treeViewPreset.Nodes.Insert(bookmarkCurrent.Index, newNode);
                            treeViewPreset.Nodes.Remove(bookmarkCurrent);
                            bookmarkFound = true;
                            break;
                        }
                    }
                    if (!bookmarkFound) {
                        // add to end
                        treeViewPreset.Nodes.Add(newNode);
                    }
                }
                AssignNumbers();
            }
        }

        private const string kExampleBookmarkName = "My Presets";
        private delegate void DEventProgress(int aPercent, string aMessage, Progress.State aProgressState);
        private delegate void DEventAddNode(TreeNode aNode);
        private HelperKinskyJukebox iHelper;
        private TreeNode iCopiedNode = null;
        private long iScrollOnDragTicks = 0;
        private TreeNode iDragDropLastNodeOver = null;
        private bool iDragDropNullRefresh = true;
        private bool iWizardInProgress = false;
        private uint iBookmarkCount = 0;
        private uint iPresetCount = 0;
        private MediaCollection iCollection = null;
        private Presets iPresets = null;
        private TreeNodeComparer iTreeNodeComparer = null;
        private bool iLastScanFull = false;
        private FormPrint iPrint = null;
        private FormWizard iFormWizard = null;
        private FormUpdate iFormUpdate = null;
        private FormUserLog iFormUserLog = null;
    }
}