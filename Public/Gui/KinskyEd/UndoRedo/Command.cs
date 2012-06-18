namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {
    
public interface ICommand
{
    void Commit();
    void Undo();
    void Redo();
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
