using System.Windows.Forms;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System.ComponentModel;
using Linn.Gui.Editor.UndoRedo;

namespace Linn {
namespace Gui {
namespace Editor {
    
internal class EditorNodeText : EditorNodeFont
{
    public EditorNodeText(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }

    [CategoryAttribute("NodeText properties"),
     DescriptionAttribute("The text the NodeText represents.")]
    public string Text {
        get {
            return ((NodeText)iPlugin).Text;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandTextChange((NodeText)iPlugin, value));
        }
    }   
}

} // Editor
} // Gui
} // Linn
