using Linn.Gui.Resources;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {

public class CommandFilenameChange : ICommand
{
    public CommandFilenameChange(ReferenceTexture aTexture, string aNewFilename) {
        iTexture = aTexture;
        iNewFilename = aNewFilename;
        iOldFilename = aTexture.Name;
    }
    
    public void Commit() {
        SetFilename(iNewFilename);
    }
    
    public void Undo() {
        SetFilename(iOldFilename);
    }
    
    public void Redo() {
        SetFilename(iNewFilename);
    }
    
    private void SetFilename(string aFilename) {
        System.Console.WriteLine(">CommandFilenameChange.SetFilename: aFilename=" + aFilename);
        iTexture.Name = aFilename;
        iTexture.Link();
    }
    
    ReferenceTexture iTexture = null;
    string iNewFilename;
    string iOldFilename;
}

public class CommandAddTextureChange : ICommand
{
    public CommandAddTextureChange(TextureArrayFixed aTexture, ReferenceTexture aNewAddTexture) {
        iTexture = aTexture;
        iNewAddTexture = aNewAddTexture;
    }
    
    public void Commit() {
        AddTexture();
    }
    
    public void Undo() {
        RemoveTexture();
    }
    
    public void Redo() {
        AddTexture();
    }
    
    private void AddTexture() {
        iTexture.AddTexture(iNewAddTexture.Object);
    }
    
    private void RemoveTexture() {
        iTexture.RemoveTexture(iNewAddTexture.Object);
    }
    
    TextureArrayFixed iTexture = null;
    ReferenceTexture iNewAddTexture;
}

public class CommandRemoveTextureChange : ICommand
{
    public CommandRemoveTextureChange(TextureArrayFixed aTexture, ReferenceTexture aNewRemoveTexture) {
        iTexture = aTexture;
        iNewRemoveTexture = aNewRemoveTexture;
    }
    
    public void Commit() {
        RemoveTexture();
    }
    
    public void Undo() {
        AddTexture();
    }
    
    public void Redo() {
        RemoveTexture();
    }
    
    private void RemoveTexture() {
        iTexture.RemoveTexture(iNewRemoveTexture.Object);
    }
    
    private void AddTexture() {
        iTexture.AddTexture(iNewRemoveTexture.Object);
    }
    
    TextureArrayFixed iTexture = null;
    ReferenceTexture iNewRemoveTexture;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
