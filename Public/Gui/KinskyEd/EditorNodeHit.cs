using System;
using System.Windows.Forms;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.ComponentModel;
using Linn.Gui.Editor.UndoRedo;

namespace Linn {
namespace Gui {
namespace Editor {
    
internal class EditorNodeHit : EditorNode
{
    public EditorNodeHit(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }
        
    [CategoryAttribute("NodeHit properties"),
     DescriptionAttribute("The node's width.")]
    public virtual float Width {
        get {
            return ((NodeHit)iPlugin).Width;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandWidthChange((NodeHit)iPlugin, value));
        }
    }
    
    [CategoryAttribute("NodeHit properties"),
     DescriptionAttribute("The node's height.")]
    public virtual float Height {
        get {
            return ((NodeHit)iPlugin).Height;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandHeightChange((NodeHit)iPlugin, value));
        }
    }
    
    [CategoryAttribute("NodeHit properties"),
     DescriptionAttribute("Whether the node allows hit/unhit events.")]
    public virtual bool AllowHits {
        get {
            return ((NodeHit)iPlugin).AllowHits;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandAllowHitsChange((NodeHit)iPlugin, value));
        }
    }
}

} // Linn
} // Gui
} // Linn
