using Linn.Gui.Resources;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {
    
public class CommandNamespaceChange : ICommand
{
    public CommandNamespaceChange(Package aLayout, string aNewNamespace) {
        iLayout = aLayout;
        iNewNamespace = aNewNamespace;
        iOldNamespace = aLayout.Namespace;
    }
    
    public void Commit() {
        SetNamespace(iNewNamespace);
    }
    
    public void Undo() {
        SetNamespace(iOldNamespace);
    }
    
    public void Redo() {
        SetNamespace(iNewNamespace);
    }
    
    private void SetNamespace(string aNamespace) {
        iLayout.Namespace = aNamespace;
    }
    
    Package iLayout = null;
    string iNewNamespace;
    string iOldNamespace;
}

public class CommandNameChange : ICommand
{
    public CommandNameChange(Plugin aPlugin, string aNewName) {
        iPlugin = aPlugin;
        iNewName = aNewName;
        iOldName = aPlugin.Name;
    }
    
    public void Commit() {
        SetName(iNewName);
    }
    
    public void Undo() {
        SetName(iOldName);
    }
    
    public void Redo() {
        SetName(iNewName);
    }
    
    private void SetName(string aName) {
        string oldName = iPlugin.Name;
        iPlugin.Name = aName;
        Plugin plugin = iPlugin.NextPlugin;
        while(plugin != null && plugin != iPlugin) {
            plugin.Name = plugin.Name.Replace(oldName, aName);
            plugin = plugin.NextPlugin;
        }
    }
    
    Plugin iPlugin = null;
    string iNewName;
    string iOldName;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
