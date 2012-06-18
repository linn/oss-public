using Linn.Gui.Scenegraph;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {

public class CommandTranslationChange : ICommand
{
    public CommandTranslationChange(Node aNode, Vector3d aNewPosition) {
        iNode = aNode;
        iNewPosition = aNewPosition;
        iOldPosition = aNode.WorldTranslation;
    }
    
    public void Commit() {
        SetTranslation(iNewPosition);
    }
    
    public void Undo() {
        SetTranslation(iOldPosition);
    }
    
    public void Redo() {
        SetTranslation(iNewPosition);
    }
    
    private void SetTranslation(Vector3d aPosition) {
        iNode.WorldTranslation = aPosition;
    }
    
    Node iNode = null;
    Vector3d iNewPosition;
    Vector3d iOldPosition;
}

public class CommandActiveChange : ICommand
{
    public CommandActiveChange(Node aNode, bool aNewActive) {
        iNode = aNode;
        iNewActive = aNewActive;
        iOldActive = aNode.Active;
    }
    
    public void Commit() {
        SetActive(iNewActive);
    }
    
    public void Undo() {
        SetActive(iOldActive);
    }
    
    public void Redo() {
        SetActive(iNewActive);
    }
    
    private void SetActive(bool aActive) {
        iNode.Active = aActive;
    }
    
    Node iNode = null;
    bool iNewActive;
    bool iOldActive;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
