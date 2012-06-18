using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;
using Linn.Gui.Editor.UndoRedo;

namespace Linn {
namespace Gui {
namespace Editor {
    
internal class NodeConverter : StringConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
        return true;
    }
    
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
        List<string> list = new List<string>();
        foreach(Package pkg in PackageManager.Instance.Packages) {
            foreach(Plugin p in pkg.PluginList) {
                if(p as Node != null) {
                    list.Add(p.Fullname);
                }
            }
        }
        return new StandardValuesCollection(list.ToArray());
    }
}
    
internal class EditorNodeInput : EditorNodePolygon
{
    public EditorNodeInput(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }
    
    [CategoryAttribute("NodeInput properties"),
     DescriptionAttribute("The node who will receive the user input."),
     TypeConverter(typeof(NodeConverter))]
    public string MessageReceiver {
        get {
            if(((NodeInput)iPlugin).MsgReceiver != null) {
                return ((NodeInput)iPlugin).MsgReceiver.Fullname;
            } else {
                return "";
            }
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandMsgReceiverChange((NodeInput)iPlugin, value));
        }
    }
}

} // Editor
} // Gui
} // Linn
