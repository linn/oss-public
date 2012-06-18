using System;
using System.Drawing;
using System.Windows.Forms;

namespace Linn {
namespace Gui {
namespace Editor {

partial class ScenegraphForm
{
    private System.Windows.Forms.TreeView iTreeView;
    private System.Windows.Forms.ImageList iImageList;
    private System.ComponentModel.IContainer iComponents;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
        if( disposing )
        {
            if(iComponents != null)
            {
                iComponents.Dispose();
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScenegraphForm));
        this.iTreeView = new System.Windows.Forms.TreeView();
        this.iImageList = new System.Windows.Forms.ImageList(this.components);
        this.SuspendLayout();
        // 
        // iTreeView
        // 
        this.iTreeView.AllowDrop = true;
        this.iTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iTreeView.FullRowSelect = true;
        this.iTreeView.HideSelection = false;
        this.iTreeView.ImageIndex = 0;
        this.iTreeView.ImageList = this.iImageList;
        this.iTreeView.LabelEdit = true;
        this.iTreeView.Location = new System.Drawing.Point(0, 0);
        this.iTreeView.Name = "iTreeView";
        this.iTreeView.SelectedImageIndex = 0;
        this.iTreeView.ShowLines = false;
        this.iTreeView.Size = new System.Drawing.Size(366, 548);
        this.iTreeView.TabIndex = 0;
        this.iTreeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView_AfterLabelEdit);
        this.iTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_DragDrop);
        this.iTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
        this.iTreeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_DragEnter);
        this.iTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
        // 
        // iImageList
        //
        this.iImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
        this.iImageList.ImageSize = new System.Drawing.Size(16, 16);
        this.iImageList.TransparentColor = System.Drawing.Color.Transparent;
        // 
        // ScenegraphForm
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(366, 548);
        this.Controls.Add(this.iTreeView);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MinimumSize = new System.Drawing.Size(200, 100);
        this.Name = "ScenegraphForm";
        this.Text = "Scenegraph Browser";
        this.Load += new System.EventHandler(this.ScenegraphForm_Load);
        this.Closing += new System.ComponentModel.CancelEventHandler(this.ScenegraphForm_Closing);
        this.ResumeLayout(false);

    }
    #endregion

    private System.ComponentModel.IContainer components;
}

} // Editor
} // Gui
} // Linn

