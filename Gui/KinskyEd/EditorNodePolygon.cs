using System.Windows.Forms;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.ComponentModel;
using Linn.Gui.Editor.UndoRedo;

namespace Linn {
namespace Gui {
namespace Editor {
    
internal class EditorNodePolygon : EditorNodeHit
{
    public EditorNodePolygon(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }
    
    /*[CategoryAttribute("NodeHit properties"),
     ReadOnlyAttribute(true),
     DescriptionAttribute("The node's width.")]
    public override float Width {
        get {
            return ((NodePolygon)iPlugin).Width;
        }
    }
    
    [CategoryAttribute("NodeHit properties"),
     ReadOnlyAttribute(true),
     DescriptionAttribute("The node's height.")]
    public override float Height {
        get {
            return ((NodePolygon)iPlugin).Height;
        }
    }*/
    
    [CategoryAttribute("NodePolygon properties"),
     DescriptionAttribute("Whether the polyon's size is dictated by the texture size.")]
    public bool ClampToTextureSize {
        get {
            return ((NodePolygon)iPlugin).ClampToTextureSize;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandClampToTextureSizeChange((NodePolygon)iPlugin, value));
        }
    }
}

} // Editor
} // Gui
} // Linn
