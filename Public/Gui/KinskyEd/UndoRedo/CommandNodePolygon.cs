using Linn.Gui.Scenegraph;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {
    
public class CommandClampToTextureSizeChange : ICommand
{
    public CommandClampToTextureSizeChange(NodePolygon aNodePolygon, bool aNewClampToTextureSize) {
        iNodePolygon = aNodePolygon;
        iNewClampToTextureSize = aNewClampToTextureSize;
        iOldClampToTextureSize = aNodePolygon.ClampToTextureSize;
    }
    
    public void Commit() {
        SetClampToTextureSize(iNewClampToTextureSize);
    }
    
    public void Undo() {
        SetClampToTextureSize(iOldClampToTextureSize);
    }
    
    public void Redo() {
        SetClampToTextureSize(iNewClampToTextureSize);
    }
    
    private void SetClampToTextureSize(bool aClampToTextureSize) {
        iNodePolygon.ClampToTextureSize = aClampToTextureSize;
    }
    
    NodePolygon iNodePolygon = null;
    bool iNewClampToTextureSize;
    bool iOldClampToTextureSize;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
