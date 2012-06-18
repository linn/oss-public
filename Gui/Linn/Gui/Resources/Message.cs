using Linn.Gui;
using System.Xml;
using Linn.Gui.Scenegraph;
using System.Windows.Forms;

namespace Linn {
namespace Gui {
namespace Resources {

public partial class Message : ISerialiseObject
{
    protected Message() {
    }
    
    protected Message(Plugin aPlugin)
    {
        iPlugin = new ReferencePlugin<Plugin>(aPlugin);
    }
    
    public virtual void Load(XmlNode aXmlNode) {
        XmlNodeList list = aXmlNode.SelectNodes("Fullname");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iPlugin = new ReferencePlugin<Plugin>(list[0].FirstChild.Value);
        }
    }
    
    public virtual void Link() {
        Trace.WriteLine(Trace.kGui, "Linking Message: " + iPlugin.Name);
        iPlugin.Link();
    }
    
    public virtual void Save(XmlTextWriter aWriter) {
        aWriter.WriteStartElement("Fullname");
        aWriter.WriteString(Plugin.Fullname);
        aWriter.WriteEndElement();
    }
    
    public virtual bool IsEqualTo(Message aMessage) {
        if(aMessage.GetType() == GetType()) {
            if(aMessage.Fullname == Fullname) {
                return true;
            }
        }
        return false;
    }

    public string Fullname {
        get {
            if(Plugin != null) {
                return Plugin.Fullname;
            } else {
                return "";
            }
        }
        set {
            if(Plugin == null && value != "") {
                iPlugin = new ReferencePlugin<Plugin>(value);
                iPlugin.Link();
            } else if(Plugin != null) {
                if(Plugin.Fullname != value) {
                    iPlugin = new ReferencePlugin<Plugin>(value);
                    iPlugin.Link();
                }
            }
        }
    }
    
    public string Namespace {
        get {
            return Plugin.Namespace;
        }
    }
    
    public string Name {
        get {
            return Plugin.Name;
        }
    }
    
    public Plugin Plugin {
        get {
            return iPlugin.Object;
        }
        set {
            iPlugin = new ReferencePlugin<Plugin>(value);
        }
    }
    
    ReferencePlugin<Plugin> iPlugin = new ReferencePlugin<Plugin>();
}

public sealed partial class MsgSetActive : Message
{
    public MsgSetActive() {
    }
    
    public MsgSetActive(Plugin aPlugin, bool aActive)
        : base(aPlugin)
    {
        iActive = aActive;
    }

    public override void Load(XmlNode aXmlNode)
    {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Active");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iActive = bool.Parse(list[0].FirstChild.Value);
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("Active");
        aWriter.WriteString(iActive.ToString());
        aWriter.WriteEndElement();
    }
    
    public override bool IsEqualTo(Message aMessage) {
        if(base.IsEqualTo(aMessage)) {
            if(((MsgSetActive)aMessage).Active == Active) {
                return true;
            }
        }
        return false;
    }

    public bool Active {
        get {
            return iActive;
        }
        set {
            iActive = value;
        }
    }
    
    private bool iActive = false;
}

public sealed partial class MsgActiveChanged : Message
{
    public MsgActiveChanged() {
    }
    
    public MsgActiveChanged(Plugin aPlugin, bool aOldActive, bool aNewActive)
        : base(aPlugin)
    {
        iOldActive = aOldActive;
        iNewActive = aNewActive;
    }

    public override void Load(XmlNode aXmlNode)
    {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("OldActive");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iOldActive = bool.Parse(list[0].FirstChild.Value);
        }
        
        list = aXmlNode.SelectNodes("NewActive");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iNewActive = bool.Parse(list[0].FirstChild.Value);
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("OldActive");
        aWriter.WriteString(iOldActive.ToString());
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("NewActive");
        aWriter.WriteString(iNewActive.ToString());
        aWriter.WriteEndElement();
    }
    
    public override bool IsEqualTo(Message aMessage) {
        if(base.IsEqualTo(aMessage)) {
            if(((MsgActiveChanged)aMessage).OldActive == OldActive) {
                if(((MsgActiveChanged)aMessage).NewActive == NewActive) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool OldActive {
        get {
            return iOldActive;
        }
        set {
            iOldActive = value;
        }
    }
    
    public bool NewActive {
        get {
            return iNewActive;
        }
        set {
            iNewActive = value;
        }
    }
    
    private bool iOldActive = false;
    private bool iNewActive = false;
}

public sealed partial class MsgHit : Message
{
    public MsgHit() {
        iPosition = new Vector3d();
    }
    
    public MsgHit(Plugin aPlugin, Vector3d aPosition)
        : base(aPlugin)
    {
        iPosition = aPosition;
    }

    public override void Load(XmlNode aXmlNode)
    {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Position");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iPosition.Load(list[0]);
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("Position");
        iPosition.Save(aWriter);
        aWriter.WriteEndElement();
    }
    
    public override bool IsEqualTo(Message aMessage) {
        if(base.IsEqualTo(aMessage)) {
            //if(((MsgHit)aMessage).Position == Position) {
                return true;
            //}
        }
        return false;
    }
    
    public Vector3d Position {
        get {
            return iPosition;
        }
    }
    
    private Vector3d iPosition = null;
}

public sealed partial class MsgNext : Message
{
    public MsgNext() {
    }
    
    public MsgNext(Plugin aPlugin)
        : base(aPlugin)
    {
    }
}

public sealed partial class MsgPrevious : Message
{
    public MsgPrevious() {
    }
    
    public MsgPrevious(Plugin aPlugin)
        : base(aPlugin)
    {
    }
}

public sealed partial class MsgSetTexture : Message
{
    public MsgSetTexture() {
    }
    
    public MsgSetTexture(Plugin aPlugin, ITexture aTexture, int aIndex)
        : base(aPlugin)
    {
        iTexture = new ReferenceTexture(aTexture);
        iIndex = aIndex;
    }

    public override void Load(XmlNode aXmlNode)
    {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Texture");
        if(list != null) {
            Assert.Check(list.Count == 1);
            ReferenceTexture texture = new ReferenceTexture();
            texture.Name = list[0].FirstChild.Value;
            iTexture = texture;
        }
        
        list = aXmlNode.SelectNodes("Index");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iIndex = int.Parse(list[0].FirstChild.Value);
        }
    }
    
    public override void Link() {
        base.Link();
        iTexture.Link();
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("Texture");
        aWriter.WriteString(iTexture.Object.Filename);
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Index");
        aWriter.WriteString(iIndex.ToString());
        aWriter.WriteEndElement();
    }
    
    public override bool IsEqualTo(Message aMessage) {
        if(base.IsEqualTo(aMessage)) {
            if(((MsgSetTexture)aMessage).Texture == Texture) {
                return true;
            }
        }
        return false;
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
    
    private ReferenceTexture iTexture = null;
    private int iIndex = 0;
}

public sealed partial class MsgSetText : Message
{
    public MsgSetText() {
    }
    
    public MsgSetText(Plugin aPlugin, string aText)
        : base(aPlugin)
    {
        iText = aText;
    }

    public override void Load(XmlNode aXmlNode)
    {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Text");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iText = list[0].FirstChild.Value;
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("Text");
        aWriter.WriteString(iText);
        aWriter.WriteEndElement();
    }
    
    public override bool IsEqualTo(Message aMessage) {
        if(base.IsEqualTo(aMessage)) {
            if(((MsgSetText)aMessage).Text == Text) {
                return true;
            }
        }
        return false;
    }

    public string Text {
        get {
            return iText;
        }
        set {
            iText = value;
        }
    }
    
    private string iText = "";
}

public sealed partial class MsgStateChanged : Message
{
    public MsgStateChanged() {
    }
    
    public MsgStateChanged(Plugin aPlugin, bool aOldState, bool aNewState)
        : base(aPlugin)
    {
        iOldState = aOldState;
        iNewState = aNewState;
    }

    public override void Load(XmlNode aXmlNode)
    {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("OldState");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iOldState = bool.Parse(list[0].FirstChild.Value);
        }
        
        list = aXmlNode.SelectNodes("NewState");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iNewState = bool.Parse(list[0].FirstChild.Value);
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("OldState");
        aWriter.WriteString(iOldState.ToString());
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("NewState");
        aWriter.WriteString(iNewState.ToString());
        aWriter.WriteEndElement();
    }
    
    public override bool IsEqualTo(Message aMessage) {
        if(base.IsEqualTo(aMessage)) {
            if(((MsgStateChanged)aMessage).OldState == OldState) {
                if(((MsgStateChanged)aMessage).NewState == NewState) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool OldState {
        get {
            return iOldState;
        }
        set {
            iOldState = value;
        }
    }

    public bool NewState {
        get {
            return iNewState;
        }
        set {
            iNewState = value;
        }
    }
    
    private bool iOldState;
    private bool iNewState;
}

public sealed partial class MsgSetState : Message
{
    public MsgSetState() {
    }
    
    public MsgSetState(Plugin aPlugin, bool aState)
        : base(aPlugin)
    {
        iState = aState;
    }

    public override void Load(XmlNode aXmlNode)
    {
        base.Load(aXmlNode);
        
        XmlNodeList list = aXmlNode.SelectNodes("State");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iState = bool.Parse(list[0].FirstChild.Value);
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("State");
        aWriter.WriteString(iState.ToString());
        aWriter.WriteEndElement();
    }
    
    public override bool IsEqualTo(Message aMessage) {
        if(base.IsEqualTo(aMessage)) {
            if(((MsgSetState)aMessage).State == State) {
                return true;
            }
        }
        return false;
    }
    
    public bool State {
        get {
            return iState;
        }
        set {
            iState = value;
        }
    }
    
    private bool iState = false;
}

public sealed partial class MsgToggleState : Message
{
    public MsgToggleState() {
    }
    
    public MsgToggleState(Plugin aPlugin)
        : base(aPlugin)
    {
    }
}

public sealed partial class MsgCountStart : Message
{
    public MsgCountStart() {
    }
    
    public MsgCountStart(Plugin aPlugin)
        : base(aPlugin)
    {
    }
}

public sealed partial class MsgCountStop : Message
{
    public MsgCountStop() {
    }
    
    public MsgCountStop(Plugin aPlugin)
        : base(aPlugin)
    {
    }
}

public sealed partial class MsgCountEnd : Message
{
    public MsgCountEnd() {
    }
    
    public MsgCountEnd(Plugin aPlugin)
        : base(aPlugin)
    {
    }
}

public class MsgInputBase : Message
{
    public MsgInputBase() {
    }
    
    public MsgInputBase(Plugin aPlugin, float aSpeed)
        : base(aPlugin)
    {
        iSpeed = aSpeed;
    }

    public float Speed
    {
        get {
            return iSpeed;
        }
    }
    
    protected float iSpeed = 0;
}

public sealed partial class MsgInputRotate : MsgInputBase
{
    public MsgInputRotate() {
    }
    
    public MsgInputRotate(Plugin aPlugin, float aSpeed)
        : base(aPlugin, aSpeed)
    {
    }
}

public sealed partial class MsgInputAxisX : MsgInputBase
{
    public MsgInputAxisX() {
    }
    
    public MsgInputAxisX(Plugin aPlugin, float aSpeed)
        : base(aPlugin, aSpeed)
    {
    }
}

public sealed partial class MsgInputAxisY : MsgInputBase
{
    public MsgInputAxisY() {
    }
    
    public MsgInputAxisY(Plugin aPlugin, float aSpeed)
        : base(aPlugin, aSpeed)
    {
    }
}

public sealed partial class MsgInputAxisZ : MsgInputBase
{
    public MsgInputAxisZ() {
    }
    
    public MsgInputAxisZ(Plugin aPlugin, float aSpeed)
        : base(aPlugin, aSpeed)
    {
    }
}

public class MsgInputSelect : Message
{
    public MsgInputSelect() {
    }
    
    public MsgInputSelect(Plugin aPlugin, Vector3d aPosition)
        : base(aPlugin)
    {
        iPosition = aPosition;
    }

    public Vector3d Position
    {
        get {
            return iPosition;
        }
    }
    
    private Vector3d iPosition = new Vector3d();
}

public sealed partial class MsgInputClick : MsgInputSelect
{
    public MsgInputClick() {
    }
    
    public MsgInputClick(Plugin aPlugin, Vector3d aPosition)
        : base(aPlugin, aPosition)
    {
    }
}

public sealed partial class MsgInputDoubleClick : MsgInputSelect
{
    public MsgInputDoubleClick() {
    }
    
    public MsgInputDoubleClick(Plugin aPlugin, Vector3d aPosition)
        : base(aPlugin, aPosition)
    {
    }
}

public abstract class MsgKey : Message
{
    public MsgKey() {
    }
    
    public MsgKey(Keys aKey) {
        iKey = aKey;
    }

    public Keys Key
    {
        get {
            return iKey;
        }
    }
    
    private Keys iKey = Keys.None;
}

public sealed partial class MsgKeyDown : MsgKey
{
    public MsgKeyDown() {
    }
    
    public MsgKeyDown(Keys aKey) : base(aKey) {
    }
}

public sealed partial class MsgKeyUp : MsgKey
{
    public MsgKeyUp() {
    }
    
    public MsgKeyUp(Keys aKey) : base(aKey) {
    }
}

public class MsgKeyPress : Message
{
    public MsgKeyPress() {
    }
    
    public MsgKeyPress(char aChar) {
        iChar = aChar;
    }
    
    public char Char {
        get {
            return iChar;
        }
    }
    
    private char iChar;
}

public abstract partial class MsgSelection : Message
{
    protected MsgSelection() {
    }
    
    protected MsgSelection(Plugin aPlugin, int aIndex, IListable aListable)
        : base(aPlugin)
    {
        iIndex = aIndex;
        iListable = aListable;
    }

    public IListable Listable
    {
        get {
            return iListable;
        }
    }
    
    public int Index {
        get {
            return iIndex;
        }
    }
    
    protected IListable iListable = null;
    protected int iIndex = 0;
}

public class MsgSelect : MsgSelection
{
    public MsgSelect() {
    }
    
    public MsgSelect(Plugin aPlugin, int aIndex, IListable aListable)
        : base(aPlugin, aIndex, aListable)
    {
    }
}

public class MsgHighlight : MsgSelection
{
    public MsgHighlight() {
    }
    
    public MsgHighlight(Plugin aPlugin, int aIndex, IListable aListable)
        : base(aPlugin, aIndex, aListable)
    {
    }
}

public class MsgHighlightUpdated : MsgSelection
{
    public MsgHighlightUpdated() {
    }
    
    public MsgHighlightUpdated(Plugin aPlugin, int aIndex, IListable aListable)
        : base(aPlugin, aIndex, aListable)
    {
    }
}

public class MsgUnHighlight : Message
{
    public MsgUnHighlight() {
    }
    
    public MsgUnHighlight(Plugin aPlugin, IListable aListable)
        : base(aPlugin)
    {
        iListable = aListable;
    }

    public IListable Listable
    {
        get {
            return iListable;
        }
    }
    
    protected IListable iListable = null;
}

public sealed partial class MsgSetHighlight : Message
{
    public MsgSetHighlight() {
    }
    
    public MsgSetHighlight(Plugin aPlugin, int aIndex, NodeList.EAlignment aAlignment)
        : base(aPlugin)
    {
        iIndex = aIndex;
        iAlignment = aAlignment;
    }

    public int Index
    {
        get {
            return iIndex;
        }
        set {
            iIndex = value;
        }
    }
    
    public NodeList.EAlignment Alignment {
        get {
            return iAlignment;
        }
        set {
            iAlignment = value;
        }
    }
    
    private int iIndex = -1;
    private NodeList.EAlignment iAlignment = NodeList.EAlignment.EA_None;
}

public sealed partial class MsgScroll : Message
{
    public MsgScroll() {
    }
    
    public MsgScroll(Plugin aPlugin, int aStepSize)
        : base(aPlugin)
    {
        iStepSize = aStepSize;
    }

    public int StepSize
    {
        get {
            return iStepSize;
        }
        set {
            iStepSize = value;
        }
    }
    
    private int iStepSize = 0;
}

public sealed partial class MsgTopEntry : Message
{
    public MsgTopEntry() {
    }
    
    public MsgTopEntry(Plugin aPlugin, uint aIndex)
        : base(aPlugin)
    {
        iIndex = aIndex;
    }

    public uint Index
    {
        get {
            return iIndex;
        }
        set {
            iIndex = value;
        }
    }
    
    private uint iIndex = 0;
}

public sealed partial class MsgSetTopEntry : Message
{
    public MsgSetTopEntry() {
    }
    
    public MsgSetTopEntry(Plugin aPlugin, uint aIndex)
        : base(aPlugin)
    {
        iIndex = aIndex;
    }

    public uint Index
    {
        get {
            return iIndex;
        }
        set {
            iIndex = value;
        }
    }
    
    private uint iIndex = 0;
}

public sealed partial class MsgFocusChanged : Message
{
    public MsgFocusChanged() {
    }
    
    public MsgFocusChanged(Plugin aPlugin, bool aOldFocus, bool aNewFocus)
        : base(aPlugin)
    {
        iOldFocus = aOldFocus;
        iNewFocus = aNewFocus;
    }

    public override void Load(XmlNode aXmlNode)
    {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("OldFocus");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iOldFocus = bool.Parse(list[0].FirstChild.Value);
        }
        
        list = aXmlNode.SelectNodes("NewFocus");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iNewFocus = bool.Parse(list[0].FirstChild.Value);
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("OldFocus");
        aWriter.WriteString(iOldFocus.ToString());
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("NewFocus");
        aWriter.WriteString(iNewFocus.ToString());
        aWriter.WriteEndElement();
    }
    
    public override bool IsEqualTo(Message aMessage) {
        if(base.IsEqualTo(aMessage)) {
            if(((MsgFocusChanged)aMessage).OldFocus == OldFocus) {
                if(((MsgFocusChanged)aMessage).NewFocus == NewFocus) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool OldFocus {
        get {
            return iOldFocus;
        }
        set {
            iOldFocus = value;
        }
    }
    
    public bool NewFocus {
        get {
            return iNewFocus;
        }
        set {
            iNewFocus = value;
        }
    }
    
    private bool iOldFocus = false;
    private bool iNewFocus = false;
}

public sealed partial class MsgSetFocus : Message
{
    public MsgSetFocus() {
    }
    
    public MsgSetFocus(Plugin aPlugin, bool aFocus)
        : base(aPlugin)
    {
        iFocus = aFocus;
    }

    public override void Load(XmlNode aXmlNode)
    {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Active");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iFocus = bool.Parse(list[0].FirstChild.Value);
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("Focus");
        aWriter.WriteString(iFocus.ToString());
        aWriter.WriteEndElement();
    }
    
    public override bool IsEqualTo(Message aMessage) {
        if(base.IsEqualTo(aMessage)) {
            if(((MsgSetFocus)aMessage).Focus == Focus) {
                return true;
            }
        }
        return false;
    }

    public bool Focus {
        get {
            return iFocus;
        }
        set {
            iFocus = value;
        }
    }
    
    private bool iFocus = false;
}

public class MsgPositionChanged : Message
{
    public MsgPositionChanged() {
    }
    
    public MsgPositionChanged(Plugin aPlugin, float aOldPosition, float aNewPosition)
        : base(aPlugin)
    {
        iOldPosition = aOldPosition;
        iNewPosition = aNewPosition;
    }

    public float OldPosition
    {
        get {
            return iOldPosition;
        }
        set {
            iOldPosition = value;
        }
    }
    
    public float NewPosition {
        get {
            return iNewPosition;
        }
        set{
            iNewPosition = value;
        }
    }
    
    private float iOldPosition = 0;
    private float iNewPosition = 0;
}

public sealed partial class MsgSetPosition : Message
{
    public MsgSetPosition() {
    }
    
    public MsgSetPosition(Plugin aPlugin, float aPosition)
        : base(aPlugin)
    {
        iPosition = aPosition;
    }

    public float Position
    {
        get {
            return iPosition;
        }
        set {
            iPosition = value;
        }
    }
    
    private float iPosition = 0;
}

} // Resources
} // Gui
} // Linn
