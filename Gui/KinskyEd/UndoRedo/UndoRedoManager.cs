using System.Collections.Generic;
using System;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {

public enum CommandDoneType {
    Commit = 0,
    Undo = 1,
    Redo = 2
}
    
public class CommandDoneEventArgs : EventArgs {
    public CommandDoneEventArgs(CommandDoneType aCommandDoneType) {
        iCommandDoneType = aCommandDoneType;
    }
    
    public CommandDoneType CommandDoneType {
        get {
            return iCommandDoneType;
        }
    }
    
    private CommandDoneType iCommandDoneType;
}
    
public class UndoRedoManager
{       
    private UndoRedoManager() {
    }
    
    public static UndoRedoManager Instance {
        get {
            if(iInstance == null) {
                iInstance = new UndoRedoManager();
            }
            return iInstance;
        }
    }
    
    public int MaxHistorySize {
        get {
            return iMaxHistorySize;
        }
        set {
            Assert.Check(value > -1);
            iMaxHistorySize = value;
            TruncateHistory();
        }
    }
    
    public bool CanUndo {
        get {
            return iCurrentIndex > -1;
        }
    }
    
    public bool CanRedo {
        get {
            return iCurrentIndex < iHistory.Count - 1;
        }
    }
    
    /*public IEnumerable<string> UndoCommands {
        get {
            for(int i = iCurrentIndex; i >= 0; --i) {
                yield return iHistory[i].Name;
            }
        }
    }
    
    public IEnumerable<string> RedoCommands {
        get {
            for(int i = iCurrentIndex + 1; i < iHistory.Count; ++i) {
                yield return iHistory[i].Name;
            }
        }
    }*/
    
    public void Undo() {
        if(CanUndo) {
            ICommand command = iHistory[iCurrentIndex];
            command.Undo();
            iCurrentIndex--;
            OnCommandDone(CommandDoneType.Undo);
        }
    }
    
    public void Redo() {
        if(CanRedo) {
            ICommand command = iHistory[iCurrentIndex+1];
            command.Redo();
            iCurrentIndex++;
            OnCommandDone(CommandDoneType.Redo);
        }
    }
    
    public void Commit(ICommand aCommand) {
        System.Console.WriteLine(">UndoRedoManager.Commit: aCommand=" + aCommand);
        Assert.Check(aCommand != null);
        try {
            aCommand.Commit();
        
            // remove all commands after insert position
            int count = iHistory.Count - iCurrentIndex - 1;
            iHistory.RemoveRange(iCurrentIndex + 1, count);
            
            iHistory.Add(aCommand);
            iCurrentIndex++;
            iCurrentCommand = null;
            TruncateHistory();
            OnCommandDone(CommandDoneType.Commit);
            System.Console.WriteLine("UndoRedoManager.Commit: iHistory.Count=" + iHistory.Count);
        } catch(Exception) {
            aCommand.Undo();
        }
    }
    
    public void FlushHistory() {
        iHistory.Clear();
        iCurrentIndex = -1;
    }
    
    public event EventHandler<CommandDoneEventArgs> CommandDone;
    private void OnCommandDone(CommandDoneType aCommandDoneType) {
        if(CommandDone != null) {
            CommandDone(this, new CommandDoneEventArgs(aCommandDoneType));
        }
    }
    
    private void TruncateHistory() {
        Assert.Check(iCurrentCommand == null);
        if(iHistory.Count > iMaxHistorySize) {
            int count = iHistory.Count - iMaxHistorySize;
            iHistory.RemoveRange(0, count);
            iCurrentIndex -= count;
        }
    }
    
    private static UndoRedoManager iInstance = null;
    private ICommand iCurrentCommand = null;
    private List<ICommand> iHistory = new List<ICommand>();
    private int iCurrentIndex = -1;
    private int iMaxHistorySize = 20;           // default to 20 history
}

} // UndoRedo
} // Editor
} // Gui
} // Linn
