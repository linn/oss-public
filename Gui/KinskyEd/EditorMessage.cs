using Linn.Gui.Resources;
using System.ComponentModel;
using System.Globalization;
using System;
using System.Collections.Generic;

namespace Linn {
namespace Gui {
namespace Editor {
    
internal class EditorMessageConverter : ExpandableObjectConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type t) {
        if(t == typeof(string)) {
            return true;
        }
        return base.CanConvertFrom(context, t);
    }
    
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo info, object value) {
        if(value is string) {
            try {
                string s = (string)value;
                return EditorMessageFactory.Instance.Create("Msg" + s);
            } catch(Exception e) {Trace.WriteLine(Trace.kKinskyEd, e.ToString());}
            // if we got this far, complain that we
            // couldn't parse the string
            //
            throw new ArgumentException("Can not convert '" + (string)value + "' to a type of Message");
        }
        return base.ConvertFrom(context, info, value);
    }
    
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType) {
        if(destType == typeof(string) && value is EditorMessage) {
            EditorMessage t = (EditorMessage)value;
            string className = t.Message.GetType().ToString();
            int index = className.LastIndexOf(".");
            if(index != -1) {
                className = className.Substring(index + 1);
                index = className.IndexOf("Msg");
                if(index != -1) {
                    className = className.Substring(index + 3);
                }
            }
            return className;
        }
        return base.ConvertTo(context, culture, value, destType);
    }
    
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
        return true;
    }
    
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
        return new StandardValuesCollection(new string[]{"CountStart", "CountStop", "CountEnd", "Hit", "Next", "Previous",
                "SetActive", "SetText", "SetState", "StateChanged", "ToggleState"});
    }
    
    /*public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
        return false;
    }*/
}

internal class TestConverter : StringConverter
{   
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
        return true;
    }
    
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
        List<string> list = new List<string>();
        foreach(Package pkg in PackageManager.Instance.Packages) {
            foreach(Plugin p in pkg.PluginList) {
                list.Add(p.Fullname);
            }
        }
        return new StandardValuesCollection(list.ToArray());
    }
}

public class EditorMessage
{   
    public EditorMessage(Linn.Gui.Resources.Message aMessage) {
        iMessage = aMessage;
    }
    
    [CategoryAttribute("Message properties"),
     DescriptionAttribute("The plugin the message is associated with."),
     TypeConverter(typeof(TestConverter))]
    public string Fullname {
        get {
            return iMessage.Fullname;
        }
        set {
            iMessage.Fullname = value;
        }
    }
    
    [BrowsableAttribute(false)]
    public Linn.Gui.Resources.Message Message {
        get {
            return iMessage;
        }
    }
    
    protected Linn.Gui.Resources.Message iMessage = null;
}

public sealed class EditorMsgSetActive : EditorMessage
{   
    public EditorMsgSetActive(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
    
    [CategoryAttribute("SetActive properties"),
     DescriptionAttribute("The active state of the named node.")]
    public bool Active {
        get {
            return ((MsgSetActive)iMessage).Active;
        }
        set {
            ((MsgSetActive)iMessage).Active = value;
        }
    }
}

public sealed class EditorMsgHit : EditorMessage
{   
    public EditorMsgHit(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
}

/*public sealed class EditorMsgSetTexture : EditorMessage
{  
    public MsgSetTexture(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
    
    public ITexture Texture {
        get {
            return iTexture.Object;
        }
    }
    
    public int Index {
        get {
            return iIndex;
        }
    }
}*/

public sealed class EditorMsgSetText : EditorMessage
{
    public EditorMsgSetText(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
    
    [CategoryAttribute("SetText properties"),
     DescriptionAttribute("The text of the named node.")]
    public string Text {
        get {
            return ((MsgSetText)iMessage).Text;
        }
        set {
            ((MsgSetText)iMessage).Text = value;
        }
    }
}

public sealed class EditorMsgStateChanged : EditorMessage
{
    public EditorMsgStateChanged(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
    
    [CategoryAttribute("StateChanged properties"),
     DescriptionAttribute("The old state of the named node.")]
    public bool OldState {
        get {
            return ((MsgStateChanged)iMessage).OldState;
        }
        set {
            ((MsgStateChanged)iMessage).OldState = value;
        }
    }
    
    [CategoryAttribute("StateChanged properties"),
     DescriptionAttribute("The new state of the named node.")]
    public bool NewState {
        get {
            return ((MsgStateChanged)iMessage).NewState;
        }
        set {
            ((MsgStateChanged)iMessage).NewState = value;
        }
    }
}

public sealed class EditorMsgSetState : EditorMessage
{
    public EditorMsgSetState(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
    
    [CategoryAttribute("SetState properties"),
     DescriptionAttribute("The state of the named node.")]
    public bool State {
        get {
            return ((MsgSetState)iMessage).State;
        }
        set {
            ((MsgSetState)iMessage).State = value;
        }
    }
}

public sealed class EditorMsgToggleState : EditorMessage
{
    public EditorMsgToggleState(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
}

public sealed class EditorMsgCountStart : EditorMessage
{
    public EditorMsgCountStart(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
}

public sealed class EditorMsgCountStop : EditorMessage
{
    public EditorMsgCountStop(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
}

public sealed class EditorMsgCountEnd : EditorMessage
{
    public EditorMsgCountEnd(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
}

/*public class MsgInputBase : Message
{
    public MsgInputBase() {
    }
    
    public MsgInputBase(string aFullname) : base(aFullname){
    }
    
    public float Speed {
        get {
            return iSpeed;
        }
        set {
            iSpeed = value;
        }
    }
    
    protected float iSpeed = 0;
}

public sealed class MsgInputClockwise : MsgInputBase
{
    public MsgInputClockwise() {
    }
    
    public MsgInputClockwise(string aFullname, float aSpeed) : base(aFullname) {
        iSpeed = aSpeed;
    }
}

public sealed class MsgInputAntiClockwise : MsgInputBase
{
    public MsgInputAntiClockwise() {
    }
    
    public MsgInputAntiClockwise(string aFullname, float aSpeed) : base(aFullname) {
        iSpeed = aSpeed;
    }
}

public sealed class MsgInputForwards : MsgInputBase
{
    public MsgInputForwards() {
    }
    
    public MsgInputForwards(string aFullname, float aSpeed) : base(aFullname) {
        iSpeed = aSpeed;
    }
}

public sealed class MsgInputBackwards : MsgInputBase
{
    public MsgInputBackwards() {
    }
    
    public MsgInputBackwards(string aFullname, float aSpeed) : base(aFullname) {
        iSpeed = aSpeed;
    }
}*/

public sealed class EditorMsgNext : EditorMessage
{
    public EditorMsgNext(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
}

public sealed class EditorMsgPrevious : EditorMessage
{
    public EditorMsgPrevious(Linn.Gui.Resources.Message aMessage) : base(aMessage) {
    }
}

} // Resources
} // Gui
} // Linn
