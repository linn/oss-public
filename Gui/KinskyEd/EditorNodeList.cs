using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System.ComponentModel;
using Linn.Gui.Editor.UndoRedo;

namespace Linn {
namespace Gui {
namespace Editor {
    
internal class EditorNodeList : EditorNodeFont
{
    public EditorNodeList(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }
    
    [CategoryAttribute("NodeList properties"),
     DescriptionAttribute("The number of lines in the list.")]
    public uint LineCount {
        get {
            return ((NodeList)iPlugin).LineCount;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandLineCountChange((NodeList)iPlugin, value));
        }
    }
    
    [CategoryAttribute("NodeList properties"),
     DescriptionAttribute("The node to use a highlight."),
     TypeConverter(typeof(NodeConverter))]
    public string Highlight {
        get {
            if(((NodeList)iPlugin).Highlight != null) {
                return ((NodeList)iPlugin).Highlight.Fullname;
            } else {
                return "";
            }
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandHighlightChange((NodeList)iPlugin, value));
        }
    }
}
    
} // Editor
} // Gui
} // Linn
