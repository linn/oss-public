using Linn.Gui.Scenegraph;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {
    
public class CommandTextChange : ICommand
{
    public CommandTextChange(NodeText aNodeText, string aNewText) {
        iNodeText = aNodeText;
        iNewText = aNewText;
        iOldText = aNodeText.Text;
    }
    
    public void Commit() {
        SetText(iNewText);
    }
    
    public void Undo() {
        SetText(iOldText);
    }
    
    public void Redo() {
        SetText(iNewText);
    }
    
    private void SetText(string aText) {
        iNodeText.Text = aText;
    }
    
    NodeText iNodeText = null;
    string iNewText;
    string iOldText;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
