using System;
using System.Windows.Forms;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System.ComponentModel;
using System.Globalization;
using Linn.Gui.Editor.UndoRedo;

namespace Linn {
namespace Gui {
namespace Editor {
    
public class EditorPluginConverter : ExpandableObjectConverter 
{
    public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
    {
        if(destinationType == typeof(EditorPlugin)) {
            return true;
        }
        return base.CanConvertTo(context, destinationType);
    }
    
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType) 
    {
        if(destinationType == typeof(System.String) && value is EditorPlugin) {
            EditorPlugin editorPlugin = (EditorPlugin)value;
            return editorPlugin.Name;
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }
    
    public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType) 
    {
        if(sourceType == typeof(string))
            return true;
    
        return base.CanConvertFrom(context, sourceType);
    }
}

[TypeConverterAttribute(typeof(EditorPluginConverter))]
public class EditorPlugin : IPluginObserver, IDisposable
{
    public EditorPlugin(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) {
        iPlugin = aEditPlugin;
        iObserver = aObserver;
        iPlugin.AddObserver(this);
        iPlugin.AddObserver(iObserver);
        if (iPlugin.NextPlugin != null && iPlugin.NextPlugin != aOwner) {
            iNextEditorPlugin = EditorFactory.Instance.Create(aOwner, iPlugin.NextPlugin, aObserver);
        }
    }
    
    [CategoryAttribute("Plugin chain"),
     DescriptionAttribute("The plugin's chain.")]
    public EditorPlugin Plugin {
        get {
            return iNextEditorPlugin;
        }
    }
    
    public void Dispose() {
        iPlugin.RemoveObserver(this);
        iPlugin.RemoveObserver(iObserver);
        if(iNextEditorPlugin != null) {
            iNextEditorPlugin.Dispose();
        }
    }
    
    public virtual void Update(Plugin aPlugin) {
        Trace.WriteLine(Trace.kKinskyEd, "EditorPlugin: Update");
        //iProperty.Refresh();
    }
    
    [CategoryAttribute("Plugin properties"),
     DescriptionAttribute("The plugin's name.")]
    public string Name {
        get {
            return iPlugin.Name;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandNameChange(iPlugin, value));
        }
    }
    
    [CategoryAttribute("Plugin properties"),
     ReadOnlyAttribute(true),
     DescriptionAttribute("The plugin's fullname.")]
    public string Fullname {
        get {
            return iPlugin.Fullname;
        }
    }
    
    public Plugin EditPlugin() {
        return iPlugin;
    }

    protected Plugin iPlugin;
    private EditorPlugin iNextEditorPlugin;
    private IPluginObserver iObserver;
}

} // Editor
} // Gui
} // Linn
