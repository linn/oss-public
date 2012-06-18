using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui.Editor.UndoRedo;
using System.IO;
using Linn.Gui.Editor;
using Linn.Gui;
using Linn;

namespace Linn {
namespace Gui {
namespace Editor {

public partial class ScenegraphForm : System.Windows.Forms.Form, IPluginObserver
{
    private EditorCanvas iEditorCanvas;
    private Package iLayout;
    private EditorSettings iSettings;
    private TreeNode iRefTreeNode;
    
    public ScenegraphForm(EditorForm aEditorForm, EditorSettings aSettings) {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();

        iImageList.TransparentColor = System.Drawing.Color.Transparent;
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/Ref.bmp")));
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/Node.bmp")));
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/NodeHit.bmp")));
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/NodeInput.bmp")));
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/NodeList.bmp")));
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/NodePolygon.bmp")));
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/NodeText.bmp")));

        iImageList.TransparentColor = System.Drawing.Color.Transparent; 
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/Ref.bmp"))); 
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/Node.bmp"))); 
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/NodeHit.bmp"))); 
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/NodeInput.bmp"))); 
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/NodeList.bmp"))); 
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/NodePolygon.bmp"))); 
        iImageList.Images.Add(Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/NodeText.bmp")));
         
        //
        // TODO: Add any constructor code after InitializeComponent call
        //
        iSettings = aSettings;
        iRefTreeNode = new TreeNode("Referenced Packages"); 
        iRefTreeNode.NodeFont = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
        CreateScenegraphTree();
    }

    private void ScenegraphForm_Load(object sender, EventArgs e) {
        Visible = iSettings.ScenegraphVisible;
        if(iSettings.ScenegraphLocation != new Point(-1, -1)) {
            Location = iSettings.ScenegraphLocation;
            Trace.WriteLine(Trace.kKinskyEd, "ScenegraphForm location: " + Location);
        }
        Size = iSettings.ScenegraphSize;
    }
    
    private void ScenegraphForm_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
         e.Cancel = true;
         Visible = false;
    }
    
    public void Update(Plugin aPlugin) {
        Trace.WriteLine(Trace.kKinskyEd, "Scenegraph: Updating " + aPlugin.Fullname);
        iTreeView.Nodes[0].Text = iLayout.Namespace;
        TreeNode node = FindNode((Node)aPlugin, true);
        if(aPlugin.Namespace == iLayout.Namespace) {
            if(aPlugin.Name != node.Text) {
                node.Text = aPlugin.Name;
            }
        } else {
            if(aPlugin.Fullname != node.Text) {
                node.Text = aPlugin.Fullname;
            }
        }
        for(int i = 0; i < ((Node)aPlugin).Children.Count; ++i) {
            if(FindNode(((Node)aPlugin).Child(i), true) == null) {
                AddNode(((Node)aPlugin).Child(i));
            }
        }
    }
    
    public EditorCanvas EditorCanvas {
        set {
            iEditorCanvas = value;
            CreateScenegraphTree();
        }
    }
    
    public void DeleteNode(Node aNode) {
        DeleteNode(iTreeView.Nodes[0], aNode);
    }
    
    private bool DeleteNode(TreeNode aTreeNode, Node aNode) {
        foreach(TreeNode node in aTreeNode.Nodes) {
            if(node.Tag == aNode) {
                aTreeNode.Nodes.Remove(node);
                return true;
            } else {
                if(DeleteNode(node, aNode)) {
                    return true;
                }
            }
        }
        return false;
    }
    
    public void AddNode(Node aNode) {
        Trace.WriteLine(Trace.kKinskyEd, "Scenegraph: Adding " + aNode.Fullname + "...");
        if(aNode.Parent == null) {
            TreeNode treeNode = CreateTreeNode(aNode);
            iTreeView.Nodes[0].Nodes.Add(treeNode);
            for(int i = 0; i < aNode.Children.Count; ++i) {
                AddNode(iTreeView.Nodes[0], aNode.Child(i));
            }
        } else {
            AddNode(iTreeView.Nodes[0], aNode);
        }
    }
    
    private bool AddNode(TreeNode aTreeNode, Node aNode) {
        foreach(TreeNode node in aTreeNode.Nodes) {
            if(node.Tag == aNode.Parent) {
                //Trace.WriteLine(Trace.kLinnGuiEd, "Scenegraph: Added");
                TreeNode treeNode = CreateTreeNode(aNode);
                node.Nodes.Add(treeNode);
                for(int i = 0; i < aNode.Children.Count; ++i) {
                    AddNode(node, aNode.Child(i));
                }
                return true;
            } else {
                if(AddNode(node, aNode)) {
                    return true;
                }
            }
        }
        return false;
    }
    
    public void AddReferencedPackage(Package aPackage) {
        if(aPackage.Namespace != iLayout.Namespace) {
            TreeNode node = new TreeNode(Path.GetFileName(aPackage.Filename), 0, 0);
            // not supported in mono yet...
            //node.ToolTipText = aPackage.Filename;
            iRefTreeNode.Nodes.Add(node);
            AddReferencedNodes(node, aPackage.Root);
        }
    }
    
    public void AddReferencedNodes(TreeNode aTreeNode, Node aNode) {
        Trace.WriteLine(Trace.kKinskyEd, "Scenegraph: Adding " + aNode.Fullname + "...");
        if(FindNode(aNode, false) != null) {
            // Don't list nodes in referenced packages if they are linked into the main scenegraph
            return;
        }
        if(aNode.Parent == null || aNode.Parent.Namespace != aNode.Namespace) {
            TreeNode treeNode = CreateTreeNode(aNode);
            aTreeNode.Nodes.Add(treeNode);
            for(int i = 0; i < aNode.Children.Count; ++i) {
                AddNode(aTreeNode, aNode.Child(i));
            }
        } else {
            AddNode(aTreeNode, aNode);
        }
    }
    
    public void CreateScenegraphTree() {
        iLayout = (iEditorCanvas == null) ? null : iEditorCanvas.CurrLayout;
        Trace.WriteLine(Trace.kKinskyEd, "Creating scenegraph tree " + iLayout);
        // Suppress repainting the TreeView until all the objects have been created.
        iTreeView.BeginUpdate();

        // Clear the TreeView each time the method is called.
        iTreeView.Nodes.Clear();
        iRefTreeNode.Nodes.Clear();

        if(iLayout != null) {
            iTreeView.Nodes.Add(new System.Windows.Forms.TreeNode(iLayout.Namespace, 1, 1));
            AddNode(iLayout.Root);
            iTreeView.Nodes.Add(iRefTreeNode);
        }
        foreach(Package p in PackageManager.Instance.Packages) {
            AddReferencedPackage(p);
        }
        
        // Begin repainting the TreeView.
        iTreeView.EndUpdate();
    }
    
    private TreeNode CreateTreeNode(Node aNode) {
        int imageIndex = NodeImageIndex(aNode);
        TreeNode treeNode;
        if(aNode.Namespace == iLayout.Namespace) {
            treeNode = new TreeNode(aNode.Name, imageIndex, imageIndex);
        } else {
            treeNode = new TreeNode(aNode.Fullname, imageIndex, imageIndex);
        }
        treeNode.Tag = aNode;
        if(aNode.Namespace != iLayout.Namespace) {
            treeNode.NodeFont = new Font("Microsoft Sans Serif", 8, FontStyle.Italic);
        }
        aNode.AddObserver(this);
        return treeNode;
    }
    
    private int NodeImageIndex(Node aNode) {
        if(aNode as NodeText != null) {
            return 6;
        } else if(aNode as NodeList != null) {
            return 4;
        } else if(aNode as NodeInput != null) {
            return 3;
        } else if(aNode as NodePolygon != null) {
            return 5;
        } else if(aNode as NodeHit != null) {
            return 2;
        } else {
            return 1;
        }
    }
    
    private TreeNode FindNode(Node aNode, bool aSearchRefNodes) {
        //Trace.WriteLine(Trace.kLinnGuiEd, "Scenegraph: Searching for " + aNode.Fullname + "...");
        TreeNode n = FindNode(iTreeView.Nodes[0], aNode);
        if(n != null || !aSearchRefNodes) {
            return n;
        }
        return FindNode(iRefTreeNode, aNode);
    }
    
    private TreeNode FindNode(TreeNode aTreeNode, Node aNode) {
        foreach(TreeNode node in aTreeNode.Nodes) {
            if(node.Tag == aNode) {
                //Trace.WriteLine(Trace.kLinnGuiEd, "Scenegraph: Found.");
                return node;
            } else {
                TreeNode t = FindNode(node, aNode);
                if(t != null) {
                    return t;
                }
            }
        }
        return null;
    }
    
    public Node Selection {
        get {
            return (Node)iTreeView.SelectedNode.Tag;
        }
        set {
            if(value != null) {
                iTreeView.SelectedNode = FindNode(value, true);
            } else {
                iTreeView.SelectedNode = iTreeView.Nodes[0];
            }
        }
    }
    
    private void treeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e) {
        if(iEditorCanvas != null) {
            iEditorCanvas.EditNode = (Node)e.Node.Tag;
        }
    }
    
    private void treeView_AfterLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e) {
        if(e.Label != null) {
            if(e.Label.Length > 0) {
                if((Node)e.Node.Tag != null) {
                    if(((Node)e.Node.Tag).Namespace == iLayout.Namespace) {
                        UndoRedoManager.Instance.Commit(new CommandNameChange((Plugin)e.Node.Tag, e.Label));
                    } else {
                        e.CancelEdit = true;
                    }
                } else {
                    UndoRedoManager.Instance.Commit(new CommandNamespaceChange(iLayout, e.Label));
                }
                e.Node.EndEdit(false);
            } else {
                /* Cancel the label edit action, inform the user, and 
                   place the node in edit mode again. */
                e.CancelEdit = true;
                MessageBox.Show("Invalid Node name - The Node's name cannot be blank", "Node Name Edit");
                e.Node.BeginEdit();
            }
        }
    }
    
    private void treeView_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e) {
        DoDragDrop(e.Item, DragDropEffects.Move);
    }
    
    private void treeView_DragEnter(object sender, System.Windows.Forms.DragEventArgs e) {
        e.Effect = DragDropEffects.Move;
    }

    private void treeView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e) {
        TreeNode NewNode;
        if(e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false)) {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode DestinationNode = ((TreeView)sender).GetNodeAt(pt);
            NewNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
            if(NewNode.Tag as Node != null && DestinationNode.Tag as Node != null && DestinationNode != NewNode) {
                Vector3d trans = ((Node)NewNode.Tag).WorldTranslation;
                ((Node)DestinationNode.Tag).AddChild((Node)NewNode.Tag);
                ((Node)NewNode.Tag).WorldTranslation = trans;
                DestinationNode.Nodes.Add((TreeNode)NewNode.Clone());
                DestinationNode.Expand();
                //Remove Original Node
                NewNode.Remove();
                // Render as node could have been moved from a referenced package
                Renderer.Instance.Render();
            }
        }
    }
}

} // Editor
} // Gui
} // Linn

