using System;
using System.Windows.Forms;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using Linn.Gui.Editor.UndoRedo;

namespace Linn {
namespace Gui {
namespace Editor {

internal sealed class EditorTranslatorList : CollectionBase
{
    public EditorTranslatorList(List<Translator> aTranslators) {
        iTranslators = aTranslators;
        foreach(Translator t in aTranslators) {
            Add(new TranslatorProxy(t));
        }
    }
    
    protected override void OnInsert(int index, object value) {
        Translator t = ((TranslatorProxy)value).Translator;
        if(iTranslators.IndexOf(t) == -1) {
            Trace.WriteLine(Trace.kKinskyEd, "OnInsert(" + index + ")");
            iTranslators.Add(t);
        }
    }
    
    protected override void OnClear() {
        Trace.WriteLine(Trace.kKinskyEd, "OnClear");
        iTranslators.Clear();
    }
    
    protected override void OnRemove(int index, object value) {
        Translator t = ((TranslatorProxy)value).Translator;
        if(iTranslators.IndexOf(t) != -1) {
            Trace.WriteLine(Trace.kKinskyEd, "OnRemove(" + index + ")");
            iTranslators.Remove(t);
        }
    }
    
    public int Add(TranslatorProxy aProxy) {
        return List.Add(aProxy);
    }
    
    public void Remove(TranslatorProxy aProxy) {
        List.Remove(aProxy);
    }

    public TranslatorProxy this[int index] {
        get {
            return (TranslatorProxy)List[index];
        }
        set {
            List[index] = value;
        }
    }
    
    private List<Translator> iTranslators;
}

internal sealed class TranslatorProxyConverter : ExpandableObjectConverter
{   
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType) {
        if(destType == typeof(string) && value is TranslatorProxy) {
            //TranslatorProxy t = (TranslatorProxy)value;
            return "Translator";
        }
        return base.ConvertTo(context, culture, value, destType);
    }
}

[TypeConverter(typeof(TranslatorProxyConverter))]
internal sealed class TranslatorProxy
{
    public TranslatorProxy() {
        iTranslator = new Translator(null, null);
        //iToMessage = EditorMessageFactory.Instance.Create(iTranslator.ToMessage);
        //iFromMessage = EditorMessageFactory.Instance.Create(iTranslator.FromMessage);
    }
    
    public TranslatorProxy(Translator aTranslator) {
        iTranslator = aTranslator;
        iToMessage = EditorMessageFactory.Instance.Create(aTranslator.ToMessage);
        iFromMessage = EditorMessageFactory.Instance.Create(aTranslator.FromMessage);
    }
    
    [CategoryAttribute("Translator properties"),
     DescriptionAttribute("The message to translate to."),
     TypeConverter(typeof(EditorMessageConverter))]
    public EditorMessage ToMessage {
        get {
            return iToMessage;
        }
        set {
            string fullname = iToMessage.Fullname;
            iToMessage = value;
            iToMessage.Fullname = fullname;
            iTranslator.ToMessage = value.Message;
            Trace.WriteLine(Trace.kKinskyEd, "ToMessage set");
        }
    }
    
    [CategoryAttribute("Translator properties"),
     DescriptionAttribute("The message to translate from."),
     TypeConverter(typeof(EditorMessageConverter))]
    public EditorMessage FromMessage {
        get {
            return iFromMessage;
        }
        set {
            string fullname = iFromMessage.Fullname;
            iFromMessage = value;
            iFromMessage.Fullname = fullname;
            iTranslator.FromMessage = value.Message;
            Trace.WriteLine(Trace.kKinskyEd, "FromMessage set");
        }
    }
    
    [BrowsableAttribute(false)]
    public Translator Translator {
        get {
            return iTranslator;
        }
    }
    
    Translator iTranslator;
    EditorMessage iToMessage = null;
    EditorMessage iFromMessage = null;
}
    
internal class EditorNode : EditorPlugin
{
    public EditorNode(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
        iInTranslatorTable = new EditorTranslatorList(((Node)iPlugin).TranslatorIn.Translators);
        iOutTranslatorTable = new EditorTranslatorList(((Node)iPlugin).TranslatorOut.Translators);
    }
    
    [CategoryAttribute("Node properties"),
     DescriptionAttribute("The node's translation along the x-axis.")]
    public float TranslateX {
        get {
            return ((Node)iPlugin).WorldTranslation.X;
        }
        set {
            Vector3d v = ((Node)iPlugin).WorldTranslation;
            UndoRedoManager.Instance.Commit(new CommandTranslationChange((Node)iPlugin, new Vector3d(value, v.Y, v.Z)));
        }
    }
    
    [CategoryAttribute("Node properties"),
     DescriptionAttribute("The node's translation along the y-axis.")]
    public float TranslateY {
        get {
            return ((Node)iPlugin).WorldTranslation.Y;
        }
        set {
            Vector3d v = ((Node)iPlugin).WorldTranslation;
            UndoRedoManager.Instance.Commit(new CommandTranslationChange((Node)iPlugin, new Vector3d(v.X, value,v.Z)));
        }
    }
    
    [CategoryAttribute("Node properties"),
     DescriptionAttribute("The node's translation along the z-axis.")]
    public float TranslateZ {
        get {
            return ((Node)iPlugin).WorldTranslation.Z;
        }
        set {
            Vector3d v = ((Node)iPlugin).WorldTranslation;
            UndoRedoManager.Instance.Commit(new CommandTranslationChange((Node)iPlugin, new Vector3d(v.X, v.Y, value)));
        }
    }
    
    [CategoryAttribute("Node properties"),
     DescriptionAttribute("Whether the node is visible.")]
    public bool Active {
        get {
            return ((Node)iPlugin).Active;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandActiveChange((Node)iPlugin, value));
        }
    }
    
    [CategoryAttribute("Node properties"),
     DescriptionAttribute("In message translation table.")]
    public EditorTranslatorList InTranslationTable {
        get {
            return iInTranslatorTable;
        }
    }
    
    [CategoryAttribute("Node properties"),
     DescriptionAttribute("Out message translation table.")]
    public EditorTranslatorList OutTranslationTable {
        get {
            return iOutTranslatorTable;
        }
    }
    
    private EditorTranslatorList iInTranslatorTable;
    private EditorTranslatorList iOutTranslatorTable;
}

} // Editor
} // Gui
} // Linn
