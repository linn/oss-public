using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {
    
public class CommandSliderTextureChange : ICommand
{
    public CommandSliderTextureChange(NodeSlider aNodeSlider, string aNewSliderTexture) {
        iNodeSlider = aNodeSlider;
        iNewSliderTexture = aNewSliderTexture;
        iOldSliderTexture = aNodeSlider.Name;
    }
    
    public void Commit() {
        SetSliderTexture(iNewSliderTexture);
    }
    
    public void Undo() {
        SetSliderTexture(iOldSliderTexture);
    }
    
    public void Redo() {
        SetSliderTexture(iNewSliderTexture);
    }
    
    private void SetSliderTexture(string aSliderTexture) {
        iNodeSlider.SliderTexture.Name = aSliderTexture;
        iNodeSlider.SliderTexture.Link();
    }
    
    NodeSlider iNodeSlider = null;
    string iNewSliderTexture;
    string iOldSliderTexture;
}

public class CommandSliderPositionChange : ICommand
{
    public CommandSliderPositionChange(NodeSlider aNodeSlider, float aNewSliderPosition) {
        iNodeSlider = aNodeSlider;
        iNewSliderPosition = aNewSliderPosition;
        iOldSliderPosition = aNodeSlider.Position;
    }
    
    public void Commit() {
        SetSliderPosition(iNewSliderPosition);
    }
    
    public void Undo() {
        SetSliderPosition(iOldSliderPosition);
    }
    
    public void Redo() {
        SetSliderPosition(iNewSliderPosition);
    }
    
    private void SetSliderPosition(float aSliderPosition) {
        iNodeSlider.Position = aSliderPosition;
    }
    
    NodeSlider iNodeSlider = null;
    float iNewSliderPosition;
    float iOldSliderPosition;
}

public class CommandSliderOrientationChange : ICommand
{
    public CommandSliderOrientationChange(NodeSlider aNodeSlider, NodeSlider.EOrientation aNewSliderOrientation) {
        iNodeSlider = aNodeSlider;
        iNewSliderOrientation = aNewSliderOrientation;
        iOldSliderOrientation = aNodeSlider.Orientation;
    }
    
    public void Commit() {
        SetSliderOrientation(iNewSliderOrientation);
    }
    
    public void Undo() {
        SetSliderOrientation(iOldSliderOrientation);
    }
    
    public void Redo() {
        SetSliderOrientation(iNewSliderOrientation);
    }
    
    private void SetSliderOrientation(NodeSlider.EOrientation aSliderOrientation) {
        iNodeSlider.Orientation = aSliderOrientation;
    }
    
    NodeSlider iNodeSlider = null;
    NodeSlider.EOrientation iNewSliderOrientation;
    NodeSlider.EOrientation iOldSliderOrientation;
}

public class CommandSliderIndicatorNodeChange : ICommand
{
    public CommandSliderIndicatorNodeChange(NodeSlider aNodeSlider, string aNewSliderIndicatorNode) {
        iNodeSlider = aNodeSlider;
        iNewSliderIndicatorNode = aNewSliderIndicatorNode;
        if(aNodeSlider.IndicatorNode != null) {
            iOldSliderIndicatorNode = aNodeSlider.IndicatorNode.Fullname;
        } else {
            iOldSliderIndicatorNode = "";
        }
    }
    
    public void Commit() {
        SetSliderIndicatorNode(iNewSliderIndicatorNode);
    }
    
    public void Undo() {
        SetSliderIndicatorNode(iOldSliderIndicatorNode);
    }
    
    public void Redo() {
        SetSliderIndicatorNode(iNewSliderIndicatorNode);
    }
    
    private void SetSliderIndicatorNode(string aSliderIndicatorNode) {
        if(aSliderIndicatorNode != "") {
            NodeHit t = (NodeHit)PackageManager.Instance.PluginByName(aSliderIndicatorNode);
            iNodeSlider.IndicatorNode = t;
        } else {
            iNodeSlider.IndicatorNode = null;
        }
    }
    
    NodeSlider iNodeSlider = null;
    string iNewSliderIndicatorNode;
    string iOldSliderIndicatorNode;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
