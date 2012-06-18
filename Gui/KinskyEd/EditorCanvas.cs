using Linn;
using Linn.Gui;
using Linn.Gui.Scenegraph;
using System.Windows.Forms;
using System.Collections.Generic;
using Linn.Gui.Editor.UndoRedo;
using System;

namespace Linn {
namespace Gui {
namespace Editor {
    
public class EditorCanvas : CanvasGdiPlus
{   
    public EditorCanvas(EditorForm aEditorForm) {
        iEditorForm = aEditorForm;
    }
    
    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);
    }
    
    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        iEditorForm.UpdateLocation(e.X, e.Y);
    }
    
    public bool Editing {
        get {
            return iEditing;
        }
        set {
            iEditing = value;
        }
    }
    
    public Node EditNode {
        get {
            return iEditNode;
        }
        set {
            if(iEditNode != value) {
                iEditNode = value;
                iEditorForm.Selection = value;
            }
        }
    }
    
    protected override Node FindNode(Vector3d aPosition) {
        if(Editing) {
            List<Node> result = new VisitorSelectProxy(aPosition).Hit(iLayout.Root);
            if(result.Count > 0) {
                result.Sort(new Node.FarToNear());
                return((Node)result[0]);
            }
            return null;
        } else {
            return base.FindNode(aPosition);
        }
    }
    
    protected override void ProcessPaint(PaintEventArgs e) {
        base.ProcessPaint(e);
        if(iEditing && iEditNode != null) {
            ((RendererGdiEditor)Renderer.Instance).DrawSelectedNode(iEditNode);
        }
    }
    
    protected override void ProcessClick(EventArgs e) {
        if(!iEditing) {
            base.ProcessClick(e);
        }
    }
    
    protected override void ProcessDoubleClick(EventArgs e) {
        if(!iEditing) {
            base.ProcessDoubleClick(e);
        }
    }
    
    protected override void ProcessMouseDown(MouseEventArgs e) {
        if(!iEditing) {
            base.ProcessMouseDown(e);
        } else {
            EditNode = iCurrNode;
            if(iEditNode != null) {
                iSelectStart = new Vector3d(e.X, e.Y, iEditNode.WorldTranslation.Z);
                iNodeStart = iEditNode.WorldTranslation;
                iLastPosition = iSelectStart;
            }
        }
    }
    
    protected override void ProcessMouseUp(MouseEventArgs e) {
        if(!iEditing) {
            base.ProcessMouseUp(e);
        } else {
            iCurrNode = null;
            if(iEditNode != null) {
                Vector3d v = new Vector3d(e.X, e.Y, iEditNode.WorldTranslation.Z);
                Vector3d diff = iSelectStart - v;
                iEditNode.WorldTranslation = iNodeStart;
                UndoRedoManager.Instance.Commit(new CommandTranslationChange(iEditNode, iEditNode.WorldTranslation - diff));
            }
        }
    }
    
    protected override void ProcessMouseMotion(MouseEventArgs e) {
        if(!iEditing) {
            base.ProcessMouseMotion(e);
        } else if(iEditing && e.Button == MouseButtons.Left && iEditNode != null) {
            Vector3d v = new Vector3d(e.X, e.Y, iEditNode.WorldTranslation.Z);
            Vector3d diff = iLastPosition - v;
            iEditNode.WorldTranslation = iEditNode.WorldTranslation - diff; 
            iLastPosition = v;
            Renderer.Instance.Render();
        }
    }
    
    private EditorForm iEditorForm = null;
    private bool iEditing = false;
    private Node iEditNode = null;
    private Vector3d iSelectStart;
    private Vector3d iNodeStart;
    private Vector3d iLastPosition;
}

} // Editor
} // Gui
} // Linn
