using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System.ComponentModel;
using System.Drawing;
using Linn.Gui.Editor.UndoRedo;

namespace Linn {
namespace Gui {
namespace Editor {
    
internal class EditorNodeFont : EditorNodeHit
{
    public EditorNodeFont(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }

    [CategoryAttribute("NodeFont properties"),
     DescriptionAttribute("The text's justification.")]
    public NodeFont.EJustification Justification {
        get {
            return ((NodeFont)iPlugin).Justification;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandJustificationChange((NodeFont)iPlugin, value));
        }
    }
    
    [CategoryAttribute("NodeFont properties"),
     DescriptionAttribute("The text's alignment.")]
    public NodeFont.EAlignment Alignment {
        get {
            return ((NodeFont)iPlugin).Alignment;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandAlignmentChange((NodeFont)iPlugin, value));
        }
    }
    
    [CategoryAttribute("NodeFont properties"),
     DescriptionAttribute("Specifies how to trim characters from a string that does not completely fit into the assigned rectangle.")]
    public NodeFont.ETrimming Trimming {
        get {
            return ((NodeFont)iPlugin).Trimming;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandTrimmingChange((NodeFont)iPlugin, value));
        }
    }
    
    [CategoryAttribute("NodeFont properties"),
     DescriptionAttribute("The text's font.")]
    public Font CurrFont {
        get {
            return ((NodeFont)iPlugin).CurrFont;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandFontChange((NodeFont)iPlugin, value));
        }
    }
    
    [CategoryAttribute("NodeFont properties"),
     DescriptionAttribute("The text's colour.")]
    public Color Colour {
        get {
            Colour colour = ((NodeFont)iPlugin).Colour;
            return Color.FromArgb(colour.A, colour.R, colour.G, colour.B);
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandColourChange((NodeFont)iPlugin, value));
        }
    }
}
    
} // Editor
} // Gui
} // Linn
