using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {
    
public class CommandLineCountChange : ICommand
{
    public CommandLineCountChange(NodeList aNodeList, uint aNewLineCount) {
        iNodeList = aNodeList;
        iNewLineCount = aNewLineCount;
        iOldLineCount = aNodeList.LineCount;
    }
    
    public void Commit() {
        SetLineCount(iNewLineCount);
    }
    
    public void Undo() {
        SetLineCount(iOldLineCount);
    }
    
    public void Redo() {
        SetLineCount(iNewLineCount);
    }
    
    private void SetLineCount(uint aLineCount) {
        iNodeList.LineCount = aLineCount;
    }
    
    NodeList iNodeList = null;
    uint iNewLineCount;
    uint iOldLineCount;
}

public class CommandHighlightChange : ICommand
{
    public CommandHighlightChange(NodeList aNodeList, string aNewHighlight) {
        iNodeList = aNodeList;
        iNewHighlight = aNewHighlight;
        if(aNodeList.Highlight != null) {
            iOldHighlight = aNodeList.Highlight.Fullname;
        } else {
            iOldHighlight = "";
        }
    }
    
    public void Commit() {
        SetHighlight(iNewHighlight);
    }
    
    public void Undo() {
        SetHighlight(iOldHighlight);
    }
    
    public void Redo() {
        SetHighlight(iNewHighlight);
    }
    
    private void SetHighlight(string aHighlight) {
        if(aHighlight != "") {
            Node t = (Node)PackageManager.Instance.PluginByName(aHighlight);
            iNodeList.Highlight = t;
        } else {
            iNodeList.Highlight = null;
        }
    }
    
    NodeList iNodeList = null;
    string iNewHighlight;
    string iOldHighlight;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
