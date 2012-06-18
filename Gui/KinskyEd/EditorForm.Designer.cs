using System;
using System.Drawing;
using System.Windows.Forms;

namespace Linn {
namespace Gui {
namespace Editor {

partial class EditorForm
{
    private System.Windows.Forms.MenuItem menuFile;
    private System.Windows.Forms.MenuItem menuEdit;
    private System.Windows.Forms.MenuItem menuView;
    private System.Windows.Forms.MenuItem menuHelp;
    private System.Windows.Forms.MenuItem menuFileNew;
    private System.Windows.Forms.MenuItem menuFileOpen;
    private System.Windows.Forms.MenuItem menuFileImport;
    private System.Windows.Forms.MenuItem menuFileClose;
    private System.Windows.Forms.MenuItem menuItem7;
    private System.Windows.Forms.MenuItem menuFileSave;
    private System.Windows.Forms.MenuItem menuFileSaveAs;
    private System.Windows.Forms.MenuItem menuItem12;
    private System.Windows.Forms.MenuItem menuFileExit;
    private System.Windows.Forms.MenuItem menuEditUndo;
    private System.Windows.Forms.MenuItem menuEditRedo;
    private System.Windows.Forms.MenuItem menuItem23;
    private System.Windows.Forms.MenuItem menuEditCut;
    private System.Windows.Forms.MenuItem menuEditCopy;
    private System.Windows.Forms.MenuItem menuEditPaste;
    private System.Windows.Forms.MenuItem menuEditDelete;
    private System.Windows.Forms.MenuItem menuViewScenegraph;
    private System.Windows.Forms.MenuItem menuViewPlugin;
    private System.Windows.Forms.MenuItem menuItem17;
    private System.Windows.Forms.MenuItem menuViewFullscreen;
    private System.Windows.Forms.MenuItem menuViewRefresh;
    private System.Windows.Forms.MenuItem menuItem18;
    private System.Windows.Forms.MenuItem menuViewPreferences;
    private System.Windows.Forms.MenuItem menuHelpContents;
    private System.Windows.Forms.MenuItem menuItem20;
    private System.Windows.Forms.MenuItem menuHelpAbout;
    private System.Windows.Forms.ToolStrip toolStrip;
    private System.Windows.Forms.ToolStripButton toolStripNew;
    private System.Windows.Forms.ToolStripButton toolStripOpen;
    private System.Windows.Forms.ToolStripButton toolStripSave;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripButton toolStripCut;
    private System.Windows.Forms.ToolStripButton toolStripCopy;
    private System.Windows.Forms.ToolStripButton toolStripPaste;
    private System.Windows.Forms.ToolStripButton toolStripDelete;
    private System.Windows.Forms.Splitter splitter1;
    private System.Windows.Forms.StatusBarPanel statusBarStatus;
    private System.Windows.Forms.StatusBarPanel statusBarMode;
    private System.Windows.Forms.StatusBarPanel statusBarLocation;
    private System.Windows.Forms.StatusBar statusBar1;
    private System.Windows.Forms.MainMenu mainMenu;
    private System.Windows.Forms.PropertyGrid PropertyPanel;
    private System.Windows.Forms.Panel PalettePanel;
    private System.Windows.Forms.Panel EditorPanel;
    private System.Windows.Forms.Button Node;
    private System.Windows.Forms.Button HitNode;
    private System.Windows.Forms.Button TextNode;
    private System.Windows.Forms.Button Surface;
    private System.Windows.Forms.Button AnimSurf;
    private System.Windows.Forms.Button PushBtn;
    private System.Windows.Forms.Button AnimPushBtn;
    private System.Windows.Forms.Button ToggleBtn;
    private System.Windows.Forms.Button AnimToggleBtn;
    private System.Windows.Forms.Button List;
    private System.Windows.Forms.Button NasList;
    private System.Windows.Forms.Button Input;
    private System.Windows.Forms.Button Slider;
    private System.ComponentModel.IContainer components;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
        if( disposing )
        {
            if (components != null) 
            {
                components.Dispose();
            }
        }
        base.Dispose( disposing );
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorForm));
        this.mainMenu = new System.Windows.Forms.MainMenu();
        this.menuFile = new System.Windows.Forms.MenuItem();
        this.menuFileNew = new System.Windows.Forms.MenuItem();
        this.menuFileOpen = new System.Windows.Forms.MenuItem();
        this.menuFileImport = new System.Windows.Forms.MenuItem();
        this.menuFileClose = new System.Windows.Forms.MenuItem();
        this.menuItem7 = new System.Windows.Forms.MenuItem();
        this.menuFileSave = new System.Windows.Forms.MenuItem();
        this.menuFileSaveAs = new System.Windows.Forms.MenuItem();
        this.menuItem12 = new System.Windows.Forms.MenuItem();
        this.menuFileExit = new System.Windows.Forms.MenuItem();
        this.menuEdit = new System.Windows.Forms.MenuItem();
        this.menuEditUndo = new System.Windows.Forms.MenuItem();
        this.menuEditRedo = new System.Windows.Forms.MenuItem();
        this.menuItem23 = new System.Windows.Forms.MenuItem();
        this.menuEditCut = new System.Windows.Forms.MenuItem();
        this.menuEditCopy = new System.Windows.Forms.MenuItem();
        this.menuEditPaste = new System.Windows.Forms.MenuItem();
        this.menuEditDelete = new System.Windows.Forms.MenuItem();
        this.menuView = new System.Windows.Forms.MenuItem();
        this.menuViewScenegraph = new System.Windows.Forms.MenuItem();
        this.menuViewPlugin = new System.Windows.Forms.MenuItem();
        this.menuItem17 = new System.Windows.Forms.MenuItem();
        this.menuViewFullscreen = new System.Windows.Forms.MenuItem();
        this.menuViewRefresh = new System.Windows.Forms.MenuItem();
        this.menuItem18 = new System.Windows.Forms.MenuItem();
        this.menuViewPreferences = new System.Windows.Forms.MenuItem();
        this.menuHelp = new System.Windows.Forms.MenuItem();
        this.menuHelpContents = new System.Windows.Forms.MenuItem();
        this.menuItem20 = new System.Windows.Forms.MenuItem();
        this.menuHelpAbout = new System.Windows.Forms.MenuItem();
        this.toolStrip = new System.Windows.Forms.ToolStrip();
        this.toolStripNew = new System.Windows.Forms.ToolStripButton();
        this.toolStripOpen = new System.Windows.Forms.ToolStripButton();
        this.toolStripSave = new System.Windows.Forms.ToolStripButton();
        this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
        this.toolStripCut = new System.Windows.Forms.ToolStripButton();
        this.toolStripCopy = new System.Windows.Forms.ToolStripButton();
        this.toolStripPaste = new System.Windows.Forms.ToolStripButton();
        this.toolStripDelete = new System.Windows.Forms.ToolStripButton();
        this.PropertyPanel = new System.Windows.Forms.PropertyGrid();
        this.splitter1 = new System.Windows.Forms.Splitter();
        this.PalettePanel = new System.Windows.Forms.Panel();
        this.Node = new System.Windows.Forms.Button();
        this.HitNode = new System.Windows.Forms.Button();
        this.TextNode = new System.Windows.Forms.Button();
        this.Surface = new System.Windows.Forms.Button();
        this.AnimSurf = new System.Windows.Forms.Button();
        this.PushBtn = new System.Windows.Forms.Button();
        this.AnimPushBtn = new System.Windows.Forms.Button();
        this.ToggleBtn = new System.Windows.Forms.Button();
        this.AnimToggleBtn = new System.Windows.Forms.Button();
        this.List = new System.Windows.Forms.Button();
        this.NasList = new System.Windows.Forms.Button();
        this.Input = new System.Windows.Forms.Button();
        this.Slider = new System.Windows.Forms.Button();
        this.EditorPanel = new System.Windows.Forms.Panel();
        this.statusBarStatus = new System.Windows.Forms.StatusBarPanel();
        this.statusBarMode = new System.Windows.Forms.StatusBarPanel();
        this.statusBarLocation = new System.Windows.Forms.StatusBarPanel();
        this.statusBar1 = new System.Windows.Forms.StatusBar();
        this.toolStrip.SuspendLayout();
        this.PalettePanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.statusBarStatus)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.statusBarMode)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.statusBarLocation)).BeginInit();
        this.SuspendLayout();
        // 
        // mainMenu
        // 
        this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFile,
            this.menuEdit,
            this.menuView,
            this.menuHelp});
        // 
        // menuFile
        // 
        this.menuFile.Index = 0;
        this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFileNew,
            this.menuFileOpen,
            this.menuFileImport,
            this.menuFileClose,
            this.menuItem7,
            this.menuFileSave,
            this.menuFileSaveAs,
            this.menuItem12,
            this.menuFileExit});
        this.menuFile.Text = "&File";
        // 
        // menuFileNew
        // 
        this.menuFileNew.Index = 0;
        this.menuFileNew.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
        this.menuFileNew.Text = "&New";
        this.menuFileNew.Click += new System.EventHandler(this.menuFileNew_Click);
        // 
        // menuFileOpen
        // 
        this.menuFileOpen.Index = 1;
        this.menuFileOpen.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
        this.menuFileOpen.Text = "&Open";
        this.menuFileOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
        // 
        // menuFileImport
        // 
        this.menuFileImport.Enabled = false;
        this.menuFileImport.Index = 2;
        this.menuFileImport.Text = "Import Layout...";
        this.menuFileImport.Click += new System.EventHandler(this.menuFileImport_Click);
        // 
        // menuFileClose
        // 
        this.menuFileClose.Enabled = false;
        this.menuFileClose.Index = 3;
        this.menuFileClose.Text = "&Close";
        this.menuFileClose.Click += new System.EventHandler(this.menuFileClose_Click);
        // 
        // menuItem7
        // 
        this.menuItem7.Index = 4;
        this.menuItem7.Text = "-";
        // 
        // menuFileSave
        // 
        this.menuFileSave.Enabled = false;
        this.menuFileSave.Index = 5;
        this.menuFileSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
        this.menuFileSave.Text = "&Save";
        this.menuFileSave.Click += new System.EventHandler(this.menuFileSave_Click);
        // 
        // menuFileSaveAs
        // 
        this.menuFileSaveAs.Enabled = false;
        this.menuFileSaveAs.Index = 6;
        this.menuFileSaveAs.Text = "Save &As...";
        this.menuFileSaveAs.Click += new System.EventHandler(this.menuFileSaveAs_Click);
        // 
        // menuItem12
        // 
        this.menuItem12.Index = 7;
        this.menuItem12.Text = "-";
        // 
        // menuFileExit
        // 
        this.menuFileExit.Index = 8;
        this.menuFileExit.Shortcut = System.Windows.Forms.Shortcut.CtrlX;
        this.menuFileExit.Text = "E&xit";
        this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
        // 
        // menuEdit
        // 
        this.menuEdit.Index = 1;
        this.menuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuEditUndo,
            this.menuEditRedo,
            this.menuItem23,
            this.menuEditCut,
            this.menuEditCopy,
            this.menuEditPaste,
            this.menuEditDelete});
        this.menuEdit.Text = "&Edit";
        // 
        // menuEditUndo
        // 
        this.menuEditUndo.Enabled = false;
        this.menuEditUndo.Index = 0;
        this.menuEditUndo.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
        this.menuEditUndo.Text = "Undo";
        this.menuEditUndo.Click += new System.EventHandler(this.menuEditUndo_Click);
        // 
        // menuEditRedo
        // 
        this.menuEditRedo.Enabled = false;
        this.menuEditRedo.Index = 1;
        this.menuEditRedo.Shortcut = System.Windows.Forms.Shortcut.CtrlY;
        this.menuEditRedo.Text = "Redo";
        this.menuEditRedo.Click += new System.EventHandler(this.menuEditRedo_Click);
        // 
        // menuItem23
        // 
        this.menuItem23.Index = 2;
        this.menuItem23.Text = "-";
        // 
        // menuEditCut
        // 
        this.menuEditCut.Enabled = false;
        this.menuEditCut.Index = 3;
        this.menuEditCut.Shortcut = System.Windows.Forms.Shortcut.CtrlX;
        this.menuEditCut.Text = "Cut";
        // 
        // menuEditCopy
        // 
        this.menuEditCopy.Enabled = false;
        this.menuEditCopy.Index = 4;
        this.menuEditCopy.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
        this.menuEditCopy.Text = "Copy";
        // 
        // menuEditPaste
        // 
        this.menuEditPaste.Enabled = false;
        this.menuEditPaste.Index = 5;
        this.menuEditPaste.Shortcut = System.Windows.Forms.Shortcut.CtrlV;
        this.menuEditPaste.Text = "Paste";
        // 
        // menuEditDelete
        // 
        this.menuEditDelete.Enabled = false;
        this.menuEditDelete.Index = 6;
        this.menuEditDelete.Shortcut = System.Windows.Forms.Shortcut.CtrlDel;
        this.menuEditDelete.Text = "Delete";
        this.menuEditDelete.Click += new System.EventHandler(this.menuEditDelete_Click);
        // 
        // menuView
        // 
        this.menuView.Index = 2;
        this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuViewScenegraph,
            this.menuViewPlugin,
            this.menuItem17,
            this.menuViewFullscreen,
            this.menuViewRefresh,
            this.menuItem18,
            this.menuViewPreferences});
        this.menuView.Text = "&View";
        // 
        // menuViewScenegraph
        // 
        this.menuViewScenegraph.Index = 0;
        this.menuViewScenegraph.Shortcut = System.Windows.Forms.Shortcut.CtrlG;
        this.menuViewScenegraph.Text = "View Scenegraph";
        this.menuViewScenegraph.Click += new System.EventHandler(this.menuViewScenegraph_Click);
        // 
        // menuViewPlugin
        // 
        this.menuViewPlugin.Checked = true;
        this.menuViewPlugin.Index = 1;
        this.menuViewPlugin.Shortcut = System.Windows.Forms.Shortcut.CtrlP;
        this.menuViewPlugin.Text = "Plugin Editor";
        this.menuViewPlugin.Click += new System.EventHandler(this.menuViewPlugin_Click);
        // 
        // menuItem17
        // 
        this.menuItem17.Index = 2;
        this.menuItem17.Text = "-";
        // 
        // menuViewFullscreen
        // 
        this.menuViewFullscreen.Index = 3;
        this.menuViewFullscreen.Shortcut = System.Windows.Forms.Shortcut.CtrlF;
        this.menuViewFullscreen.Text = "Fullscreen";
        this.menuViewFullscreen.Click += new System.EventHandler(this.menuViewFullscreen_Click);
        // 
        // menuViewRefresh
        // 
        this.menuViewRefresh.Index = 4;
        this.menuViewRefresh.Shortcut = System.Windows.Forms.Shortcut.F5;
        this.menuViewRefresh.Text = "Refresh";
        this.menuViewRefresh.Click += new System.EventHandler(this.menuViewRefresh_Click);
        // 
        // menuItem18
        // 
        this.menuItem18.Index = 5;
        this.menuItem18.Text = "-";
        // 
        // menuViewPreferences
        // 
        this.menuViewPreferences.Index = 6;
        this.menuViewPreferences.Text = "Preferences...";
        this.menuViewPreferences.Click += new System.EventHandler(this.menuViewPreferences_Click);
        // 
        // menuHelp
        // 
        this.menuHelp.Index = 3;
        this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuHelpContents,
            this.menuItem20,
            this.menuHelpAbout});
        this.menuHelp.Text = "&Help";
        // 
        // menuHelpContents
        // 
        this.menuHelpContents.Enabled = false;
        this.menuHelpContents.Index = 0;
        this.menuHelpContents.Shortcut = System.Windows.Forms.Shortcut.F1;
        this.menuHelpContents.Text = "Help Contents";
        // 
        // menuItem20
        // 
        this.menuItem20.Index = 1;
        this.menuItem20.Text = "-";
        // 
        // menuHelpAbout
        // 
        this.menuHelpAbout.Index = 2;
        this.menuHelpAbout.Text = "&About";
        this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
        // 
        // toolStrip
        // 
        this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
        this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripNew,
            this.toolStripOpen,
            this.toolStripSave,
            this.toolStripSeparator1,
            this.toolStripCut,
            this.toolStripCopy,
            this.toolStripPaste,
            this.toolStripDelete});
        this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
        this.toolStrip.Location = new System.Drawing.Point(0, 0);
        this.toolStrip.Name = "toolStrip";
        this.toolStrip.Size = new System.Drawing.Size(1125, 25);
        this.toolStrip.TabIndex = 1;
        this.toolStrip.Text = "toolStrip";
        this.toolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip_ItemClicked);
        // 
        // toolStripNew
        // 
        this.toolStripNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.toolStripNew.Image = ((System.Drawing.Image)(resources.GetObject("toolStripNew.Image")));
        this.toolStripNew.Name = "toolStripNew";
        this.toolStripNew.Size = new System.Drawing.Size(23, 22);
        this.toolStripNew.Text = "New";
        this.toolStripNew.Click += new System.EventHandler(this.toolStripNew_Click);
        // 
        // toolStripOpen
        // 
        this.toolStripOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.toolStripOpen.Image = ((System.Drawing.Image)(resources.GetObject("toolStripOpen.Image")));
        this.toolStripOpen.Name = "toolStripOpen";
        this.toolStripOpen.Size = new System.Drawing.Size(23, 22);
        this.toolStripOpen.Text = "Open";
        // 
        // toolStripSave
        // 
        this.toolStripSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.toolStripSave.Enabled = false;
        this.toolStripSave.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSave.Image")));
        this.toolStripSave.Name = "toolStripSave";
        this.toolStripSave.Size = new System.Drawing.Size(23, 22);
        this.toolStripSave.Text = "Save";
        // 
        // toolStripSeparator1
        // 
        this.toolStripSeparator1.Name = "toolStripSeparator1";
        this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
        // 
        // toolStripCut
        // 
        this.toolStripCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.toolStripCut.Enabled = false;
        this.toolStripCut.Image = ((System.Drawing.Image)(resources.GetObject("toolStripCut.Image")));
        this.toolStripCut.Name = "toolStripCut";
        this.toolStripCut.Size = new System.Drawing.Size(23, 22);
        this.toolStripCut.Text = "Cut";
        // 
        // toolStripCopy
        // 
        this.toolStripCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.toolStripCopy.Enabled = false;
        this.toolStripCopy.Image = ((System.Drawing.Image)(resources.GetObject("toolStripCopy.Image")));
        this.toolStripCopy.Name = "toolStripCopy";
        this.toolStripCopy.Size = new System.Drawing.Size(23, 22);
        this.toolStripCopy.Text = "Copy";
        // 
        // toolStripPaste
        // 
        this.toolStripPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.toolStripPaste.Enabled = false;
        this.toolStripPaste.Image = ((System.Drawing.Image)(resources.GetObject("toolStripPaste.Image")));
        this.toolStripPaste.Name = "toolStripPaste";
        this.toolStripPaste.Size = new System.Drawing.Size(23, 22);
        this.toolStripPaste.Text = "Paste";
        // 
        // toolStripDelete
        // 
        this.toolStripDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.toolStripDelete.Enabled = false;
        this.toolStripDelete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDelete.Image")));
        this.toolStripDelete.Name = "toolStripDelete";
        this.toolStripDelete.Size = new System.Drawing.Size(23, 22);
        this.toolStripDelete.Text = "Delete";
        // 
        // PropertyPanel
        // 
        this.PropertyPanel.Dock = System.Windows.Forms.DockStyle.Right;
        this.PropertyPanel.Location = new System.Drawing.Point(875, 25);
        this.PropertyPanel.Name = "PropertyPanel";
        this.PropertyPanel.Size = new System.Drawing.Size(250, 608);
        this.PropertyPanel.TabIndex = 4;
        this.PropertyPanel.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyPanel_PropertyValueChanged);
        // 
        // splitter1
        // 
        this.splitter1.BackColor = System.Drawing.SystemColors.Control;
        this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
        this.splitter1.Location = new System.Drawing.Point(872, 25);
        this.splitter1.Name = "splitter1";
        this.splitter1.Size = new System.Drawing.Size(3, 608);
        this.splitter1.TabIndex = 5;
        this.splitter1.TabStop = false;
        // 
        // PalettePanel
        // 
        this.PalettePanel.Controls.Add(this.Node);
        this.PalettePanel.Controls.Add(this.HitNode);
        this.PalettePanel.Controls.Add(this.TextNode);
        this.PalettePanel.Controls.Add(this.Surface);
        this.PalettePanel.Controls.Add(this.AnimSurf);
        this.PalettePanel.Controls.Add(this.PushBtn);
        this.PalettePanel.Controls.Add(this.AnimPushBtn);
        this.PalettePanel.Controls.Add(this.ToggleBtn);
        this.PalettePanel.Controls.Add(this.AnimToggleBtn);
        this.PalettePanel.Controls.Add(this.List);
        this.PalettePanel.Controls.Add(this.NasList);
        this.PalettePanel.Controls.Add(this.Input);
        this.PalettePanel.Controls.Add(this.Slider);
        this.PalettePanel.Dock = System.Windows.Forms.DockStyle.Left;
        this.PalettePanel.Location = new System.Drawing.Point(0, 25);
        this.PalettePanel.Name = "PalettePanel";
        this.PalettePanel.Size = new System.Drawing.Size(32, 608);
        this.PalettePanel.TabIndex = 6;
        // 
        // Node
        // 
        this.Node.Image = ((System.Drawing.Image)(resources.GetObject("Node.Image")));
        this.Node.Location = new System.Drawing.Point(0, 0);
        this.Node.Name = "Node";
        this.Node.Size = new System.Drawing.Size(32, 32);
        this.Node.TabIndex = 1;
        this.Node.Click += new System.EventHandler(this.buttonNode_Click);
        // 
        // HitNode
        // 
        this.HitNode.Image = ((System.Drawing.Image)(resources.GetObject("HitNode.Image")));
        this.HitNode.Location = new System.Drawing.Point(0, 32);
        this.HitNode.Name = "HitNode";
        this.HitNode.Size = new System.Drawing.Size(32, 32);
        this.HitNode.TabIndex = 1;
        this.HitNode.Click += new System.EventHandler(this.buttonHitNode_Click);
        // 
        // TextNode
        // 
        this.TextNode.Image = ((System.Drawing.Image)(resources.GetObject("TextNode.Image")));
        this.TextNode.Location = new System.Drawing.Point(0, 64);
        this.TextNode.Name = "TextNode";
        this.TextNode.Size = new System.Drawing.Size(32, 32);
        this.TextNode.TabIndex = 2;
        this.TextNode.Click += new System.EventHandler(this.buttonTextNode_Click);
        // 
        // Surface
        // 
        this.Surface.Location = new System.Drawing.Point(0, 96);
        this.Surface.Name = "Surface";
        this.Surface.Size = new System.Drawing.Size(32, 32);
        this.Surface.TabIndex = 3;
        this.Surface.Text = "Surface";
        this.Surface.Click += new System.EventHandler(this.buttonSurface_Click);
        // 
        // AnimSurf
        // 
        this.AnimSurf.Location = new System.Drawing.Point(0, 128);
        this.AnimSurf.Name = "AnimSurf";
        this.AnimSurf.Size = new System.Drawing.Size(32, 32);
        this.AnimSurf.TabIndex = 4;
        this.AnimSurf.Text = "AnimSurf";
        this.AnimSurf.Click += new System.EventHandler(this.buttonAnimSurf_Click);
        // 
        // PushBtn
        // 
        this.PushBtn.Location = new System.Drawing.Point(0, 160);
        this.PushBtn.Name = "PushBtn";
        this.PushBtn.Size = new System.Drawing.Size(32, 32);
        this.PushBtn.TabIndex = 5;
        this.PushBtn.Text = "PushBtn";
        this.PushBtn.Click += new System.EventHandler(this.buttonPushBtn_Click);
        // 
        // AnimPushBtn
        // 
        this.AnimPushBtn.Location = new System.Drawing.Point(0, 192);
        this.AnimPushBtn.Name = "AnimPushBtn";
        this.AnimPushBtn.Size = new System.Drawing.Size(32, 32);
        this.AnimPushBtn.TabIndex = 6;
        this.AnimPushBtn.Text = "AnimPushBtn";
        this.AnimPushBtn.Click += new System.EventHandler(this.buttonAnimPushBtn_Click);
        // 
        // ToggleBtn
        // 
        this.ToggleBtn.Location = new System.Drawing.Point(0, 224);
        this.ToggleBtn.Name = "ToggleBtn";
        this.ToggleBtn.Size = new System.Drawing.Size(32, 32);
        this.ToggleBtn.TabIndex = 7;
        this.ToggleBtn.Text = "ToggleBtn";
        this.ToggleBtn.Click += new System.EventHandler(this.buttonToggleBtn_Click);
        // 
        // AnimToggleBtn
        // 
        this.AnimToggleBtn.Location = new System.Drawing.Point(0, 256);
        this.AnimToggleBtn.Name = "AnimToggleBtn";
        this.AnimToggleBtn.Size = new System.Drawing.Size(32, 32);
        this.AnimToggleBtn.TabIndex = 8;
        this.AnimToggleBtn.Text = "AnimToggleBtn";
        this.AnimToggleBtn.Click += new System.EventHandler(this.buttonAnimToggleBtn_Click);
        // 
        // List
        // 
        this.List.Location = new System.Drawing.Point(0, 288);
        this.List.Name = "List";
        this.List.Size = new System.Drawing.Size(32, 32);
        this.List.TabIndex = 9;
        this.List.Text = "List";
        this.List.Click += new System.EventHandler(this.buttonList_Click);
        // 
        // NasList
        // 
        this.NasList.Location = new System.Drawing.Point(0, 0);
        this.NasList.Name = "NasList";
        this.NasList.Size = new System.Drawing.Size(75, 23);
        this.NasList.TabIndex = 10;
        // 
        // Input
        // 
        this.Input.Location = new System.Drawing.Point(0, 320);
        this.Input.Name = "Input";
        this.Input.Size = new System.Drawing.Size(32, 32);
        this.Input.TabIndex = 11;
        this.Input.Text = "Input";
        this.Input.Click += new System.EventHandler(this.buttonInput_Click);
        // 
        // Slider
        // 
        this.Slider.Image = ((System.Drawing.Image)(resources.GetObject("Slider.Image")));
        this.Slider.Location = new System.Drawing.Point(0, 352);
        this.Slider.Name = "Slider";
        this.Slider.Size = new System.Drawing.Size(32, 32);
        this.Slider.TabIndex = 12;
        this.Slider.Click += new System.EventHandler(this.buttonSlider_Click);
        // 
        // EditorPanel
        // 
        this.EditorPanel.AutoScroll = true;
        this.EditorPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.EditorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.EditorPanel.Location = new System.Drawing.Point(32, 25);
        this.EditorPanel.Name = "EditorPanel";
        this.EditorPanel.Size = new System.Drawing.Size(840, 608);
        this.EditorPanel.TabIndex = 7;
        this.EditorPanel.Click += new System.EventHandler(this.editorPanel_Click);
        // 
        // statusBarMode
        // 
        this.statusBarMode.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
        this.statusBarMode.Width = 909;
        // 
        // statusBar1
        // 
        this.statusBar1.Location = new System.Drawing.Point(0, 633);
        this.statusBar1.Name = "statusBar1";
        this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statusBarStatus,
            this.statusBarMode,
            this.statusBarLocation});
        this.statusBar1.ShowPanels = true;
        this.statusBar1.Size = new System.Drawing.Size(1125, 22);
        this.statusBar1.TabIndex = 3;
        this.statusBar1.Text = "statusBar1";
        // 
        // EditorForm
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(1125, 655);
        this.Controls.Add(this.EditorPanel);
        this.Controls.Add(this.PalettePanel);
        this.Controls.Add(this.splitter1);
        this.Controls.Add(this.PropertyPanel);
        this.Controls.Add(this.statusBar1);
        this.Controls.Add(this.toolStrip);
        this.HelpButton = true;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.KeyPreview = true;
        this.Menu = this.mainMenu;
        this.Name = "EditorForm";
        this.Text = "LinnGuiEd";
        this.Load += new System.EventHandler(this.EditorForm_Load);
        this.Closing += new System.ComponentModel.CancelEventHandler(this.EditorForm_Closing);
        this.toolStrip.ResumeLayout(false);
        this.toolStrip.PerformLayout();
        this.PalettePanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.statusBarStatus)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.statusBarMode)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.statusBarLocation)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
    #endregion
}

} // Editor
} // Gui
} // Linn
