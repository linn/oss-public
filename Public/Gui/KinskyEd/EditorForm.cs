using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Linn.Gui.Resources;
using Linn.Gui;
using Linn.Gui.Scenegraph;
using System.Collections;
using Linn.Gui.Editor.UndoRedo;
using Linn.Gui.Editor;
using Linn;

namespace Linn {
namespace Gui {
namespace Editor {

public partial class EditorForm : Form, IPluginObserver
{
    private EditorCanvas LayoutPanel;
    private ScenegraphForm iScenegraphForm;
    private System.Drawing.Size iWindowClientSize;
    private System.Drawing.Point iWindowLocation;
    private EditorPlugin iPropertyEditor;
    private EditorSettings iSettings;
    private string iExeDir;
    private bool iModified;

    public EditorForm() {
        LayoutPanel = new EditorCanvas(this);
        new RendererGdiEditor(LayoutPanel);
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor"));
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Default"));

        iExeDir = Application.StartupPath;
        Trace.WriteLine(Trace.kKinskyEd, "Exe dir: " + iExeDir);
        
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        
        //
        // TODO: Add any constructor code after InitializeComponent call
        //
        UndoRedoManager.Instance.CommandDone += CommandDone;
        iModified = false;
        iSettings = new EditorSettings(Application.StartupPath);
        iScenegraphForm = new ScenegraphForm(this, iSettings);
        iScenegraphForm.EditorCanvas = LayoutPanel;
    }
    
    private void EditorForm_Load(object sender, EventArgs e) {
        iSettings.LoadSettings();
        TextureManager.Instance.AddPath(iSettings.DefaultTextureCache);
        TextureManager.Instance.AddPath(iSettings.TextureCache);
        PackageManager.Instance.AddPath(iSettings.PackageCache);
        if(iSettings.EditorLocation != new Point(-1, -1)) {
            Location = iSettings.EditorLocation;
        }
        Size = iSettings.EditorSize;
        iScenegraphForm.Visible = iSettings.ScenegraphVisible;
    }
    
    private void EditorForm_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        if(!SaveIfRequired()) {
            e.Cancel = true;
            return;
        }
        Assert.Check(iSettings != null);
        Trace.WriteLine(Trace.kKinskyEd, "iSettings: " + iSettings + ", iScenegraphForm: " + iScenegraphForm);
        iSettings.EditorLocation = Location;
        iSettings.EditorSize = Size;
        iSettings.ScenegraphLocation = iScenegraphForm.Location;
        iSettings.ScenegraphSize = iScenegraphForm.Size;
        iSettings.ScenegraphVisible = iScenegraphForm.Visible;
        iSettings.SaveSettings();
    }
    
    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);
    }
    
    protected override void OnResize(EventArgs e) {
        base.OnResize(e);
        if(LayoutPanel != null) {
            int x = (int)((EditorPanel.ClientSize.Width * 0.5f) - (LayoutPanel.ClientSize.Width * 0.5f));
            int y = (int)((EditorPanel.ClientSize.Height * 0.5f) - (LayoutPanel.ClientSize.Height * 0.5f));
            LayoutPanel.Location = new System.Drawing.Point(x, y);
        }
    }
    
    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if(e.Control) {
            if(LayoutPanel != null) {
                if(!LayoutPanel.Editing) {
                    statusBarStatus.Text = "EDIT";
                    LayoutPanel.Editing = true;
                    Renderer.Instance.Render();
                }
            }
        }
    }
    
    protected override void OnKeyUp(KeyEventArgs e) {
        base.OnKeyUp(e);
        if(!e.Control) {
            if(LayoutPanel != null) {
                statusBarStatus.Text = "EXECUTE";
                LayoutPanel.Editing = false;
            }
        }
        Renderer.Instance.Render();
    }
    
    private void editorPanel_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            if(LayoutPanel.Editing) {
                LayoutPanel.EditNode = null;
            }
        }
    }
    
    private void menuFileNew_Click(object sender, System.EventArgs e) {
        FileNew();
    }
    
    private void menuFileOpen_Click(object sender, System.EventArgs e) {
        FileOpen();
    }
    
    private void menuFileImport_Click(object sender, System.EventArgs e) {
        FileImport();
    }
    
    private void menuFileClose_Click(object sender, System.EventArgs e) {
        if(!SaveIfRequired()) {
            return;
        }
        if(LayoutPanel != null) {
            foreach(Plugin p in LayoutPanel.CurrLayout.PluginList) {
                p.RemoveObserver(this);
            }
            LayoutPanel.EditNode = null;
            LayoutPanel.CurrLayout = null;
            CentreLayoutPanel();
            PackageManager.Instance.FlushCache();
            TextureManager.Instance.FlushCache();
            iScenegraphForm.CreateScenegraphTree();
            Text = "KinskyEd";
        }
        menuFileClose.Enabled = false;
        menuFileSave.Enabled = false;
        menuFileSaveAs.Enabled = false;
        menuFileImport.Enabled = false;
        toolStripSave.Enabled = false;
        statusBarStatus.Text = "";
        statusBarLocation.Text = "";
    }
    
    private void menuFileSave_Click(object sender, System.EventArgs e) {
        if(menuFileSaveAs.Enabled == true) {
            FileSave();
        }
    }
    
    private void menuFileSaveAs_Click(object sender, System.EventArgs e) {
        if(menuFileSaveAs.Enabled == true) {
            FileSaveAs();
        }
    }
    
    private void menuFileExit_Click(object sender, System.EventArgs e) {
        Close();
    }
    
    private void menuEditUndo_Click(object sender, System.EventArgs e) {
        UndoRedoManager.Instance.Undo();
        PropertyPanel.Refresh();
        Renderer.Instance.Render();
    }
    
    private void menuEditRedo_Click(object sender, System.EventArgs e) {
        UndoRedoManager.Instance.Redo();
        PropertyPanel.Refresh();
        Renderer.Instance.Render();
    }
    
    private void menuEditDelete_Click(object sender, System.EventArgs e) {
        if(menuEditDelete.Enabled == true) {
            DeleteSelection();
        }
    }
    
    private void menuViewScenegraph_Click(object sender, System.EventArgs e) {
        iScenegraphForm.Visible = true;
        iScenegraphForm.Location = iSettings.ScenegraphLocation;
    }
    
    private void menuViewPlugin_Click(object sender, System.EventArgs e) {
        menuViewPlugin.Checked = !menuViewPlugin.Checked;
        PropertyPanel.Visible = menuViewPlugin.Checked;
        splitter1.Visible = menuViewPlugin.Checked;
    }
    
    private void menuViewFullscreen_Click(object sender, System.EventArgs e) {
        toolStrip.SuspendLayout();
        PalettePanel.SuspendLayout();
        EditorPanel.SuspendLayout();
        SuspendLayout();
        menuViewFullscreen.Checked = !menuViewFullscreen.Checked;
        if(menuViewFullscreen.Checked) {
            iWindowClientSize = ClientSize;
            iWindowLocation = Location;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Location = new System.Drawing.Point(0, 0);
            Rectangle rect = Screen.GetBounds(this);
            Trace.WriteLine(Trace.kKinskyEd, "menuViewFullscreen_Click: " + rect.Width + "," + rect.Height);
            ClientSize = new System.Drawing.Size(rect.Width, rect.Height);
        } else {
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            ClientSize = iWindowClientSize;
            Location = iWindowLocation;
        }
        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        PalettePanel.ResumeLayout(false);
        PalettePanel.PerformLayout();
        EditorPanel.ResumeLayout(false);
        EditorPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
    
    private void menuViewPreferences_Click(object sender, System.EventArgs e) {
        PreferencesDialog dialog = new PreferencesDialog();
        if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
            Trace.WriteLine(Trace.kKinskyEd, "Updating cache settings");
            iSettings.TextureCache = TextureManager.Instance.RootDirectory;
            iSettings.PackageCache = PackageManager.Instance.RootDirectory;
            RefreshTextures();
        }
    }
    
    private void menuViewRefresh_Click(object sender, System.EventArgs e) {
        RefreshTextures();
    }
    
    private void menuHelpAbout_Click(object sender, System.EventArgs e) {
        EditorAboutBox aboutDialog = new EditorAboutBox();
        aboutDialog.ShowDialog();
        aboutDialog.Dispose();
    }
    
    private void RefreshTextures() {
        if(LayoutPanel != null) {
            TextureManager.Instance.Refresh();
            if (LayoutPanel.CurrLayout != null)
            {
                LayoutPanel.CurrLayout.Root.Update(true, true);
                Renderer.Instance.Render();
            }
        }
    }   
    
    private void toolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
        if(e.ClickedItem == toolStripNew) {
            FileNew();
        } else if(e.ClickedItem == toolStripOpen) {
            FileOpen();
        } else if(e.ClickedItem == toolStripSave && toolStripSave.Enabled == true) {
            FileSave();
        } else if(e.ClickedItem == toolStripDelete && toolStripDelete.Enabled == true) {
            DeleteSelection();
        }
    }
    
    private void buttonNode_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            AddNode(new Node());
        }
    }
    
    private void buttonHitNode_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            NodeHit hit = new NodeHit();
            hit.Width = 50;
            hit.Height = 50;
            AddNode(hit);
        }
    }
    
    private void buttonTextNode_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            NodeText text = new NodeText();
            text.Width = 100;
            text.Height = 20;
            text.Text = "TextNode";
            AddNode(text);
        }
    }
    
    private void buttonSurface_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            Package package = new Package(iExeDir + "/../../share/Linn/Gui/Editor/Widgets/", "Surface.xml");
            AddWidget(package);
        }
    }
    
    private void buttonAnimSurf_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            Package package = new Package(iExeDir + "/../../share/Linn/Gui/Editor/Widgets/", "AnimatedSurface.xml");
            AddWidget(package);
        }
    }
    
    private void buttonPushBtn_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            Package package = new Package(iExeDir + "/../../share/Linn/Gui/Editor/Widgets/", "PushButton.xml");
            AddWidget(package);
        }
    }
    
    private void buttonAnimPushBtn_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            Package package = new Package(iExeDir + "/../../share/Linn/Gui/Editor/Widgets/", "AnimatedPushButton.xml");
            AddWidget(package);
        }
    }
    
    private void buttonToggleBtn_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            Package package = new Package(iExeDir + "/../../share/Linn/Gui/Editor/Widgets/", "ToggleButton.xml");
            AddWidget(package);
        }
    }
    
    private void buttonAnimToggleBtn_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            Package package = new Package(iExeDir + "/../../share/Linn/Gui/Editor/Widgets/", "AnimatedToggleButton.xml");
            AddWidget(package);
        }
    }
    
    private void buttonList_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            Package package = new Package(iExeDir + "/../../share/Linn/Gui/Editor/Widgets/", "List.xml");
            AddWidget(package);
        }
    }
    
    private void buttonInput_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            Package package = new Package(iExeDir + "/../../share/Linn/Gui/Editor/Widgets/", "Input.xml");
            AddWidget(package);
        }
    }
    
    private void buttonSlider_Click(object sender, System.EventArgs e) {
        if(LayoutPanel != null) {
            Package package = new Package(iExeDir + "/../../share/Linn/Gui/Editor/Widgets/", "Slider.xml");
            AddWidget(package);
        }
    }
    
    private void propertyPanel_PropertyValueChanged(object sender, System.Windows.Forms.PropertyValueChangedEventArgs e) {
        Trace.WriteLine(Trace.kKinskyEd, "PropertyValueChanged -> Rendering");
        Renderer.Instance.Render();
    }
    
    private void CommandDone(object aObject, CommandDoneEventArgs aArgs) {
        System.Console.WriteLine("EditorForm.CommandDone: aArgs=" + aArgs.CommandDoneType);
        System.Console.WriteLine("EditorForm.CommandDone: CanUndo=" + UndoRedoManager.Instance.CanUndo + ", CanRedo=" + UndoRedoManager.Instance.CanRedo);
        this.menuEditUndo.Enabled = UndoRedoManager.Instance.CanUndo;
        this.menuEditRedo.Enabled = UndoRedoManager.Instance.CanRedo;
        iModified = UndoRedoManager.Instance.CanUndo;
        if(iModified) {
            Text = "KinskyEd - *" + LayoutPanel.CurrLayout.Filename;
        } else {
            Text = "KinskyEd - " + LayoutPanel.CurrLayout.Filename;
        }
    }
    
    public void Update(Plugin aPlugin) {
        Trace.WriteLine(Trace.kKinskyEd, "Editor form update by " + aPlugin.Fullname);
        if(aPlugin == LayoutPanel.CurrLayout.Root) {
            Trace.WriteLine(Trace.kKinskyEd, "Editor resized");
            NodeHit node = aPlugin as NodeHit;
            Assert.Check(node != null);
            //Assert.Check(node.Parent == null);
            LayoutPanel.Size = new System.Drawing.Size((int)((NodeHit)aPlugin).Width, (int)((NodeHit)aPlugin).Height);
            CentreLayoutPanel();
        }
        PropertyPanel.Refresh();
    }
    
    private void AddNode(Node aNode) {
        try {
            LayoutPanel.CurrLayout.AddNode(aNode);
            if(LayoutPanel.EditNode != null) {
                LayoutPanel.EditNode.AddChild(aNode);
            } else {
                LayoutPanel.CurrLayout.Root.AddChild(aNode);
            }
            // NOTE: Node added to scenegraph through AddChild call
            /*if(iScenegraphForm != null) {
                iScenegraphForm.AddNode(aNode);
            }*/
            LayoutPanel.EditNode = aNode;
            VisitorGreatestZ vistor = new VisitorGreatestZ();
            aNode.WorldTranslation = new Vector3d(10, 10, vistor.Search(LayoutPanel.CurrLayout.Root) + 1);
            Renderer.Instance.Render();
        } catch(Exception e) {
            MessageBox.Show("Failed to add " + aNode.GetType() + " node.\n" + e.Message,
                "Node add Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
    
    private void AddWidget(Package aPackage) {
        PackageManager.Instance.Packages.Add(aPackage);
        aPackage.Link();
        if(aPackage.Type == "Widget") {
            List<Plugin> pluginList = aPackage.PluginList;
            Node root = null;
            foreach(Plugin p in pluginList) {
                if(p as Node != null) {
                    if(((Node)p).Parent == null) {
                        root = (Node)p;
                    }
                }
            }
            if(root != null) {
                root.ResetRootHint();
                AddNode(root);
                UpdateNodeNamespace(root);
            }
        }
        PackageManager.Instance.Packages.Remove(aPackage);
    }
    
    private void UpdateNodeNamespace(Node aNode) {
        string oldName = aNode.Name;
        aNode.Name = aNode.Name;
        string newName = aNode.Name;
        Trace.WriteLine(Trace.kKinskyEd, "oldName=" + oldName + ", newName=" + newName + ", ns=" + aNode.Namespace);
        Plugin plugin = aNode.NextPlugin;
        while(plugin != null && plugin != aNode) {
            plugin.Name = plugin.Name.Replace(oldName, newName);
            plugin = plugin.NextPlugin;
        }
        for(int i = 0; i < aNode.Children.Count; ++i) {
            UpdateNodeNamespace(aNode.Child(i));
        }
    }
    
    private bool SaveIfRequired() {
        if(iModified) {
            DialogResult result = MessageBox.Show("Do you want to save changes to your layout?", "Layout has been modified", MessageBoxButtons.YesNoCancel);
            if(result == DialogResult.Yes) {
                return FileSave();
            } else if(result == DialogResult.Cancel) {
                return false;
            }
            iModified = false;
        }
        return true;
    }
    
    private void FileNew() {
        if(!SaveIfRequired()) {
            return;
        }
        NewLayoutDialog newLayout = new NewLayoutDialog();
        if(newLayout.ShowDialog() == DialogResult.OK) {
            TextureManager.Instance.FlushCache();
            PackageManager.Instance.FlushCache();
            //EditorCanvas canvas = new EditorCanvas(this);
            Package package = new Package();
            NodeHit root = (NodeHit)PluginFactory.Instance.Create("NodeHit");
            package.Namespace = newLayout.LayoutNamespace;
            root.Name = "Root";
            root.Width = newLayout.LayoutWidth;
            root.Height = newLayout.LayoutHeight;
            package.Root = root;
            package.AddNode(root);
            PackageManager.Instance.Packages.Add(package);
            LayoutPanel.CurrLayout = package;
            iScenegraphForm.CreateScenegraphTree();
            AddCanvasToEditor(LayoutPanel);
            CentreLayoutPanel();
            Messenger.Instance.Root = root;
            root.AddObserver(this);
            Renderer.Instance.Render();
            Selection = root;
            iModified = true;
            Text = "KinskyEd - *" + package.Namespace;
        }
    }
    
    private void FileOpen() {
        if(!SaveIfRequired()) {
            return;
        }
        System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
        openFileDialog.InitialDirectory = PackageManager.Instance.RootDirectory;
        openFileDialog.Filter = "layout files (*.xml)|*.xml|All files (*.*)|*.*";
        openFileDialog.FilterIndex = 1;
        if(openFileDialog.ShowDialog() == DialogResult.OK) {
            Trace.WriteLine(Trace.kKinskyEd, "Loading: " + openFileDialog.FileName);
            string filename = openFileDialog.FileName;
            try {
                AddCanvasToEditor(LayoutPanel);
                LayoutPanel.Load(filename);
                iScenegraphForm.CreateScenegraphTree();
                CentreLayoutPanel();
                Messenger.Instance.Root = LayoutPanel.CurrLayout.Root;
                foreach(Plugin p in LayoutPanel.CurrLayout.PluginList) {
                    p.AddObserver(this);
                }
                Selection = null;
                Text = "KinskyEd - " + openFileDialog.FileName;
            } catch(Exception e) {
                MessageBox.Show("Failed to open " + openFileDialog.FileName + "\n\n" + e.ToString(),
                    "Layout open Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
    
    private void FileImport() {
        System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
        openFileDialog.InitialDirectory = PackageManager.Instance.RootDirectory;
        openFileDialog.Filter = "layout files (*.xml)|*.xml|All files (*.*)|*.*";
        openFileDialog.FilterIndex = 1;
        if(openFileDialog.ShowDialog() == DialogResult.OK) {
            Trace.WriteLine(Trace.kKinskyEd, "Importing: " + openFileDialog.FileName);
            string filename = openFileDialog.FileName;
            try {
                Package package = PackageManager.Instance.Import(filename);
                //LayoutPanel.CurrLayout.Root.AddChild(package.Root);
                iScenegraphForm.AddReferencedPackage(package);
                Renderer.Instance.Render();
            } catch(Exception e) {
                MessageBox.Show("Failed to import " + openFileDialog.FileName + "\n\n" + e.ToString(),
                    "Layout import Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
    
    private bool FileSave() {
        try {
            Trace.Write(Trace.kKinskyEd, "Saving...");
            LayoutPanel.CurrLayout.Save();
            Text = "KinskyEd - " + LayoutPanel.CurrLayout.Filename;
            iModified = false;
            Trace.WriteLine(Trace.kKinskyEd, "Done");
            return true;
        } catch(Linn.Gui.Resources.InvalidFilename) {
            return FileSaveAs();
        }
    }
    
    private bool FileSaveAs() {
        System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
        saveFileDialog.InitialDirectory = PackageManager.Instance.RootDirectory;
        saveFileDialog.Filter = "layout files (*.xml)|*.xml|All files (*.*)|*.*";
        saveFileDialog.FilterIndex = 1;
        if(saveFileDialog.ShowDialog() == DialogResult.OK) {
            Trace.Write(Trace.kKinskyEd, "Saving: " + saveFileDialog.FileName + "...");
            string filename = saveFileDialog.FileName;
            try {
                LayoutPanel.CurrLayout.Save(filename);
                iModified = false;
                Text = "KinskyEd - " + filename;
                Trace.WriteLine(Trace.kKinskyEd, "Done");
                return true;
            } catch(Exception e) {
                MessageBox.Show("Failed to save " + filename + "\n\n" + e.ToString(),
                    "Layout save Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        return false;
    }
    
    public void UpdateLocation(int aX, int aY) {
        statusBarLocation.Text = "x=" + aX + ", y=" + aY;
    }
    
    private void AddCanvasToEditor(EditorCanvas aEditorCanvas) {
        EditorPanel.Controls.Remove(LayoutPanel);
        LayoutPanel.TabIndex = 0;
        EditorPanel.Controls.Add(LayoutPanel);
        CentreLayoutPanel();
        /*if(LayoutPanel != null) {
            EditorPanel.Controls.Remove(LayoutPanel);
        }
        LayoutPanel = aEditorCanvas;
        LayoutPanel.TabIndex = 0;
        EditorPanel.Controls.Add(LayoutPanel);
        CentreLayoutPanel();*/
        menuFileClose.Enabled = true;
        menuFileSave.Enabled = true;
        menuFileSaveAs.Enabled = true;
        menuFileImport.Enabled = true;
        toolStripSave.Enabled = true;
        if(statusBarStatus.Text == "") {
            statusBarStatus.Text = "EXECUTE";
        }
        //iScenegraphForm.EditorCanvas = LayoutPanel;
    }
    
    public Node Selection {
        get {
            return iScenegraphForm.Selection;
        }
        set {
            iScenegraphForm.Selection = value;
            
            if(value != null) {
                value.Update(true, true);
                if(iPropertyEditor != null) {
                    iPropertyEditor.Dispose();
                }
                iPropertyEditor = EditorFactory.Instance.Create(value, this);
                PropertyPanel.SelectedObject = iPropertyEditor;
                menuEditDelete.Enabled = true;
                toolStripDelete.Enabled = true;
            } else {
                PropertyPanel.SelectedObject = null;
                menuEditDelete.Enabled = false;
                toolStripDelete.Enabled = false;
            }
        }
    }
    
    private void DeleteSelection() {
        if(LayoutPanel.EditNode.Parent != null) {
            RemoveAsNodeObserver(LayoutPanel.EditNode);
            LayoutPanel.CurrLayout.DeleteNode(LayoutPanel.EditNode);
            if(iScenegraphForm != null) {
                iScenegraphForm.DeleteNode(LayoutPanel.EditNode);
            }
            LayoutPanel.EditNode = null;
            Renderer.Instance.Render();
        } else {
            MessageBox.Show("You cannot delete the ROOT node.", "Node Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
    
    private void CentreLayoutPanel() {
        int width = (LayoutPanel.ClientSize.Width > EditorPanel.ClientSize.Width) ? ClientSize.Width + (LayoutPanel.ClientSize.Width - EditorPanel.ClientSize.Width) + 10 : ClientSize.Width;
        int height = (LayoutPanel.ClientSize.Height > EditorPanel.ClientSize.Height) ? ClientSize.Height + (LayoutPanel.ClientSize.Height - EditorPanel.ClientSize.Height) + 10 : ClientSize.Height;
        Trace.WriteLine(Trace.kKinskyEd, "EditorForm.CentreLayoutPanel: ClientSize=" + ClientSize);
        Trace.WriteLine(Trace.kKinskyEd, "EditorForm.CentreLayoutPanel: LayoutPanel w=" + width + ", h=" + height);
        ClientSize = new System.Drawing.Size(width, height);
        Trace.WriteLine(Trace.kKinskyEd, "EditorForm.CentreLayoutPanel: EditorPanel.ClientSize=" + EditorPanel.ClientSize);
        Trace.WriteLine(Trace.kKinskyEd, "EditorForm.CentreLayoutPanel: LayoutPanel.ClientSize=" + LayoutPanel.ClientSize);
        int x = (int)((EditorPanel.ClientSize.Width * 0.5f) - (LayoutPanel.ClientSize.Width * 0.5f));
        int y = (int)((EditorPanel.ClientSize.Height * 0.5f) - (LayoutPanel.ClientSize.Height * 0.5f));
        Trace.WriteLine(Trace.kKinskyEd, "EditorForm.CentreLayoutPanel: LayoutPanel x=" + x + ", y=" + y);
        LayoutPanel.Location = new System.Drawing.Point(x, y);
    }
    
    private void RemoveAsNodeObserver(Node aNode) {
        aNode.RemoveObserver(this);
        for(int i = 0; i < aNode.Children.Count; ++i) {
            RemoveAsNodeObserver(aNode.Child(i));
        }
    }

    private void toolStripNew_Click(object sender, EventArgs e)
    {

    }
}

} // Editor
} // Gui
} // Linn
