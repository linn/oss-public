using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {
    
public class CommandMsgReceiverChange : ICommand
{
    public CommandMsgReceiverChange(NodeInput aNodeInput, string aNewMsgReceiver) {
        iNodeInput = aNodeInput;
        iNewMsgReceiver = aNewMsgReceiver;
        if(aNodeInput.MsgReceiver != null) {
            iOldMsgReceiver = aNodeInput.MsgReceiver.Fullname;
        } else {
            iOldMsgReceiver = "";
        }
    }
    
    public void Commit() {
        SetMsgReceiver(iNewMsgReceiver);
    }
    
    public void Undo() {
        SetMsgReceiver(iOldMsgReceiver);
    }
    
    public void Redo() {
        SetMsgReceiver(iNewMsgReceiver);
    }
    
    private void SetMsgReceiver(string aMsgReceiver) {
        if(aMsgReceiver != "") {
            Node t = (Node)PackageManager.Instance.PluginByName(aMsgReceiver);
            iNodeInput.MsgReceiver = t;
        } else {
            iNodeInput.MsgReceiver = null;
        }
    }
    
    NodeInput iNodeInput = null;
    string iNewMsgReceiver;
    string iOldMsgReceiver;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
