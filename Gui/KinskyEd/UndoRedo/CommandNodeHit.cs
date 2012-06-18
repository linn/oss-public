using Linn.Gui.Scenegraph;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {
    
public class CommandWidthChange : ICommand
{
    public CommandWidthChange(NodeHit aNodeHit, float aNewWidth) {
        iNodeHit = aNodeHit;
        iNewWidth = aNewWidth;
        iOldWidth = aNodeHit.Width;
    }
    
    public void Commit() {
        SetWidth(iNewWidth);
    }
    
    public void Undo() {
        SetWidth(iOldWidth);
    }
    
    public void Redo() {
        SetWidth(iNewWidth);
    }
    
    private void SetWidth(float aWidth) {
        iNodeHit.Width = aWidth;
    }
    
    NodeHit iNodeHit = null;
    float iNewWidth;
    float iOldWidth;
}

public class CommandHeightChange : ICommand
{
    public CommandHeightChange(NodeHit aNodeHit, float aNewHeight) {
        iNodeHit = aNodeHit;
        iNewHeight = aNewHeight;
        iOldHeight = aNodeHit.Height;
    }
    
    public void Commit() {
        SetHeight(iNewHeight);
    }
    
    public void Undo() {
        SetHeight(iOldHeight);
    }
    
    public void Redo() {
        SetHeight(iNewHeight);
    }
    
    private void SetHeight(float aHeight) {
        iNodeHit.Height = aHeight;
    }
    
    NodeHit iNodeHit = null;
    float iNewHeight;
    float iOldHeight;
}

public class CommandAllowHitsChange : ICommand
{
    public CommandAllowHitsChange(NodeHit aNodeHit, bool aNewAllowHits) {
        iNodeHit = aNodeHit;
        iNewAllowHits = aNewAllowHits;
        iOldAllowHits = aNodeHit.AllowHits;
    }
    
    public void Commit() {
        SetAllowHits(iNewAllowHits);
    }
    
    public void Undo() {
        SetAllowHits(iOldAllowHits);
    }
    
    public void Redo() {
        SetAllowHits(iNewAllowHits);
    }
    
    private void SetAllowHits(bool aAllowHits) {
        iNodeHit.AllowHits = aAllowHits;
    }
    
    NodeHit iNodeHit = null;
    bool iNewAllowHits;
    bool iOldAllowHits;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
