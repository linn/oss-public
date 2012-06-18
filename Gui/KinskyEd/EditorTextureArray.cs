using System;
using System.Windows.Forms;
using Linn.Gui.Resources;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Linn.Gui.Editor.UndoRedo;

namespace Linn {
namespace Gui {
namespace Editor {

internal sealed class EditorTextureList : CollectionBase
{
    public EditorTextureList(TextureArrayFixed aPlugin, List<ReferenceTexture> aTextures) {
        iTextures = aTextures;
        iPlugin = aPlugin;
        foreach(ReferenceTexture t in aTextures) {
            Add(new TextureProxy(t));
        }
    }
    
    protected override void OnInsert(int index, object aProxy) {
        ReferenceTexture t = ((TextureProxy)aProxy).ReferenceTexture;
        if(iTextures.IndexOf(t) == -1) {
            Trace.WriteLine(Trace.kKinskyEd, "OnInsert(" + index + ")");
            //iTextures.Add(t);
            UndoRedoManager.Instance.Commit(new CommandAddTextureChange(iPlugin, t));
            Renderer.Instance.Render();
        }
    }
    
    protected override void OnClear() {
        Trace.WriteLine(Trace.kKinskyEd, "OnClear");
        //iTextures.Clear();
        iPlugin.Clear();
        Renderer.Instance.Render();
    }
    
    protected override void OnRemove(int index, object aProxy) {
        ReferenceTexture t = ((TextureProxy)aProxy).ReferenceTexture;
        if(iTextures.IndexOf(t) != -1) {
            Trace.WriteLine(Trace.kKinskyEd, "OnRemove(" + index + ")");
            //iTextures.Remove(t);
            UndoRedoManager.Instance.Commit(new CommandRemoveTextureChange(iPlugin, t));
            Renderer.Instance.Render();
        }
    }
    
    public void Add(TextureProxy aProxy) {
        List.Add(aProxy);
    }
    
    public void Remove(TextureProxy aProxy) {
        List.Remove(aProxy);
    }

    public TextureProxy this[int index] {
        get {
            return (TextureProxy)this.List[index];
        }
    }
    
    private List<ReferenceTexture> iTextures;
    private TextureArrayFixed iPlugin;
}

internal sealed class TextureProxyConverter : ExpandableObjectConverter
{   
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType) {
        if(destType == typeof(string) && value is TextureProxy) {
            TextureProxy t = (TextureProxy)value;
            return t.Filename;
        }
        return base.ConvertTo(context, culture, value, destType);
    }
}

[TypeConverter(typeof(TextureProxyConverter))]
internal sealed class TextureProxy
{
    public TextureProxy() {
        iTexture = new ReferenceTexture();
    }
    
    public TextureProxy(ReferenceTexture aTexture) {
        iTexture = aTexture;
    }
    
    [CategoryAttribute("Texture properties"),
     DescriptionAttribute("The filename containing the texture's image.")]
    public string Filename {
        get {
            return iTexture.Name;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandFilenameChange(iTexture, value));
            Renderer.Instance.Render();
            Trace.WriteLine(Trace.kKinskyEd, "linked texture");
        }
    }
    
    [BrowsableAttribute(false)]
    public ReferenceTexture ReferenceTexture {
        get {
            return iTexture;
        }   
    }
    
    ReferenceTexture iTexture;
}

internal class EditorTextureArrayFixed : EditorPlugin
{   
    public EditorTextureArrayFixed(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
        iTextures = new EditorTextureList((TextureArrayFixed)iPlugin, ((TextureArrayFixed)iPlugin).Textures);
    }

    [CategoryAttribute("TextureArray properties"),
     DescriptionAttribute("The filenames of the array's textures.")]
    public EditorTextureList Textures {
        get {
            return iTextures;
        }
        set {
            iTextures = value;
        }
    }
    
    protected EditorTextureList iTextures = null;
}

internal class EditorTextureArray : EditorTextureArrayFixed
{
    public EditorTextureArray(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }
}

} // Editor
} // Gui
} // Linn
