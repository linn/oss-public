using System.Collections.Generic;
using Linn.Gui.Resources;
using System.Drawing;
using System;
using System.Xml;
using System.Threading;

namespace Linn {
namespace Gui {
namespace Scenegraph {
    
public interface IListable : IDisposable
{
    void Highlight();
    void UnHighlight();
    void Select();
    void UnSelect();
    int State { get; set; }
    NodeHit NodeHit { set; get; }
}

public interface IListableRenderer
{
    NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted);
}

public class ListableRendererNull : IListableRenderer
{
    public NodeHit ToNodeHit(IListable aListable, float aWidth, float aHeight, NodeFont aNodeFont, uint aIndex, bool aHighlighted) {
        return null;
    }
}
    
public abstract class NodeListBase : NodeFont
{   
    public NodeListBase(string aName) : base(aName) {
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("LineCount");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iLineCount = uint.Parse(list[0].FirstChild.Value);
            iItemHeight = Height / iLineCount;
            for(uint i = 0; i < iLineCount; ++i) {
                Node n = new Node();
                //n.Namespace = Namespace;
                //n.Name = Name + ".Placeholder";
                AddCompositeChild(n);
            }
        }
        
        list = aXmlNode.SelectNodes("Highlight");
        if(list != null) {
            if(list.Count > 0) {
                Assert.Check(list.Count == 1);
                iHighlight = new ReferencePlugin<Node>(list[0].InnerText);
            }
        }
    }
    
    public override void Link() {
        base.Link();
        iHighlight.Link();
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("LineCount");
        aWriter.WriteString(iLineCount.ToString());
        aWriter.WriteEndElement();
        if(iHighlight.Object != null) {
            aWriter.WriteStartElement("Highlight");
            aWriter.WriteString(iHighlight.Object.Fullname);
            aWriter.WriteEndElement();
        }
    }
    
    public override void OnSetActive() {
        base.OnSetActive();
        UpdateHighlight();
    }
    
    public override void OnSetHeight() {
        base.OnSetHeight();
        iItemHeight = Height / iLineCount;
    }
    
    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            if(aMessage as MsgSetFocus != null) {
                MsgSetFocus msg = aMessage as MsgSetFocus;
                Focused = msg.Focus;
                return true;
            }
        }
        return base.ProcessMessage(aMessage);
    }
    
    public uint LineCount {
        get {
            return iLineCount;
        }
        set {
            iLineCount = value;
            iItemHeight = Height / iLineCount;
            for(int i = iCompositeChildrenList.Count; i < iLineCount; ++i) {
                NodeHit n = new NodeHit();
                n.Width = Width;
                n.Height = iItemHeight;
                //n.Namespace = Namespace;
                //n.Name = Name + ".Placeholder";
                AddCompositeChild(n);
            }
            ObserverUpdate();
        }
    }
    
    public Node Highlight {
        get {
            return iHighlight.Object;
        }
        set {
            iHighlight = new ReferencePlugin<Node>(value);
        }
    }
    
    public bool HighlightActive {
        get {
            return iHighlightActive;
        }
        set {
            if(iHighlightActive != value) {
                iHighlightActive = value;
                UpdateHighlight();
                ObserverUpdate();
            }
        }
    }
    
    public virtual bool Focused {
        get {
            return iFocused;
        }
        set {
            if(iFocused != value) {
                iFocused = value;
                UpdateHighlight();
                SendMessage(new MsgFocusChanged(this, !iFocused, iFocused));
            }
        }
    }
    
    public int HighlightedIndex {
        get {
            return iHighlightedIndex;
        }
    }
    
    public IListable HighlightedItem {
        get {
            return iHighlightedItem;
        }
    }
    
    protected virtual void UpdateHighlight() {
        if(iHighlight.Object != null) {
            iHighlight.Object.Active = Active && iHighlightActive && (iHighlightedIndex != -1) && iFocused;
        }
    }
    
    protected void SetLine(uint aIndex, NodeHit aNode) {
        Assert.Check(aIndex < iLineCount);
        Assert.Check(aNode != null);
        aNode.Namespace = Namespace;
        aNode.Active = Active;
        aNode.AllowHits = false;
        aNode.LocalTranslation.Z += 2;
        //ReplaceCompositeChild(aIndex, aNode);
        //aNode.Update(true, true);
        //RemoveCompositeChild(CompositeChild((int)aIndex));
        AddCompositeChild(aNode);
    }
    
    protected uint iLineCount = 0;
    protected float iItemHeight = 0.0f;
    protected IListable iHighlightedItem = null;
    protected int iHighlightedIndex = -1;
    protected bool iHighlightActive = true;
    protected ReferencePlugin<Node> iHighlight = new ReferencePlugin<Node>();
    protected bool iFocused = true;
}

public class EntryNotFound : System.Exception
{
    public EntryNotFound(uint aIndex) : base("Could not find entry " + aIndex) {}
}

public interface IListEntryProvider : IDisposable
{
    void SetList(IList aList);
    uint Count { get; }
    List<IListable> Entries(uint aStartIndex, uint aCount);
}

public class ListEntryProviderNull : IListEntryProvider
{
    public void Dispose() {}
    public void SetList(IList aList) {}
    
    public uint Count {
        get {
            return 0;
        }
    }
    
    public List<IListable> Entries(uint aStartIndex, uint aCount) {
        return new List<IListable>();
    }
}

public interface IStepCalculator
{
    uint SpeedToStep(float aSpeed, uint aListCount);
}

public class StepCalculatorDefault : IStepCalculator
{
    public uint SpeedToStep(float aSpeed, uint aListCount) {
        float inc = (uint)aSpeed;
        if(inc < 30) {
            inc = 1;
        } else if(inc < 70) {
            inc = ((inc - 26) / 2.0f);
        } else {
            inc = ((aListCount * 0.01f * (inc - 70)) + ((inc - 26) / 2.0f));
        }
        Trace.WriteLine(Trace.kGui, "speed = " + aSpeed + ": " + inc);
        Assert.Check(inc > 0);
        return (uint)inc;
    }
}

public class EventArgsRange : EventArgs 
{
    public EventArgsRange(uint aStartIndex, uint aCount) {
        iStartIndex = aStartIndex;
        iCount = aCount;
    }

    public uint StartIndex {
        get {
            return iStartIndex;
        }
    }

    public uint Count {
        get {
            return iCount;
        }
    }

    private uint iStartIndex;
    private uint iCount;
}

public interface IList
{
    event EventHandler<EventArgsRange> EventAdded;
    event EventHandler<EventArgsRange> EventDeleted;
    event EventHandler<EventArgs> EventCleared;
    event EventHandler<EventArgs> EventRefreshed;

    void Add(uint aStartIndex, uint aCount);
    void Delete(uint aStartIndex, uint aCount);
    void Clear();
    void Refresh();
}

public class NodeList : NodeListBase, IList
{
    public new enum EAlignment {
        EA_Top = 0,
        EA_Centre = 1,
        EA_Bottom = 2,
        EA_None
    }

    public event EventHandler<EventArgsRange> EventAdded;
    public event EventHandler<EventArgsRange> EventDeleted;
    public event EventHandler<EventArgs> EventCleared;
    public event EventHandler<EventArgs> EventRefreshed;
    
    public NodeList() : base(UniqueName("NodeList")) {
    }
    
    public NodeList(string aName) : base(aName) {
    }
    
    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            if(aMessage as MsgInputRotate != null) {
                float speed = ((MsgInputRotate)aMessage).Speed;
                CalculateStep(speed);
                return true;
            }
            if(aMessage as MsgInputAxisZ != null) {
                float speed = ((MsgInputAxisZ)aMessage).Speed;
                if(speed > 0) {
                    ScrollUp((uint)(speed / 120.0f));
                } else if(speed < 0) {
                    ScrollDown((uint)-(speed / 120.0f));
                }
                return true;
            }
            if(aMessage as MsgInputSelect != null) {
                MsgInputSelect msg = aMessage as MsgInputSelect;
                SelectItem(msg.Position);
                return true;
            }
            if(aMessage as MsgSetHighlight != null) {
                MsgSetHighlight msg = aMessage as MsgSetHighlight;
                SetHighlight(msg.Index, msg.Alignment);
                return true;
            }
            if(aMessage as MsgSetTopEntry != null) {
                MsgSetTopEntry msg = aMessage as MsgSetTopEntry;
                bool update = false;
                LockUpdate();
                if (msg.Index != iTopEntry)
                {
                    iTopEntry = msg.Index;
                    update = true;
                }
                UnlockUpdate();
                if (update)
                {
                    Refresh();
                }
                return true;
            }
            if(aMessage as MsgScroll != null) {
                MsgScroll msg = aMessage as MsgScroll;
                if(msg.StepSize < 0) {
                    ScrollHighlightUp((uint)-msg.StepSize);
                } else if(msg.StepSize > 0) {
                    ScrollHighlightDown((uint)msg.StepSize);
                }
                return true;
            }
        }
        return base.ProcessMessage(aMessage);
    }
    
    public override void Visit(Visitor aVisitor, bool aVisitNonActive) {
        aVisitor.AcceptList(this);
        base.Visit(aVisitor, aVisitNonActive);
    }

    public void Add(uint aStartIndex, uint aCount) {
        System.Console.WriteLine(">>NodeList.Add");
        LockUpdate();
        System.Console.WriteLine(">NodeList.Add");
        Trace.WriteLine(Trace.kGui, "NodeList.Add: aStartIndex=" + aStartIndex + ", aCount=" + aCount);
        if(iProvider.Count > iLineCount) {
            if(iHighlightedItem != null) {
                if(aStartIndex <= iHighlightedIndex) {
                    iTopEntry += aCount;
                    iHighlightedIndex += (int)aCount;
                    SendMessage(new MsgHighlightUpdated(this, iHighlightedIndex, iHighlightedItem));
                }
            } else {
                if(aStartIndex <= iTopEntry && iTopEntry > 0) {
                    iTopEntry += aCount;
                }
            }
        } else {
            if(iHighlightedItem != null) {
                if(aStartIndex <= iHighlightedIndex) {
                    iHighlightedIndex += (int)aCount;
                    SendMessage(new MsgHighlightUpdated(this, iHighlightedIndex, iHighlightedItem));
                }
            }
        }
        Trace.WriteLine(Trace.kGui, "NodeList.Add: iHighlightedIndex=" + iHighlightedIndex + ", iTopEntry=" + iTopEntry);
        //Assert.Check(iTopEntry < iProvider.Count - iLineCount);
        Refresh();
        System.Console.WriteLine("<NodeList.Add");
        UnlockUpdate();
        System.Console.WriteLine("<<NodeList.Add");
        if (EventAdded != null) {
            EventAdded(this, new EventArgsRange(aStartIndex, aCount));
        }
    }
    
    public void Delete(uint aStartIndex, uint aCount) {
        //System.Console.WriteLine(">>NodeList.Delete");
        LockUpdate();
        //System.Console.WriteLine(">NodeList.Delete");
        Trace.WriteLine(Trace.kGui, "NodeList.Delete: aStartIndex=" + aStartIndex + ", aCount=" + aCount);
        if(iProvider.Count > iLineCount) {
            if(iHighlightedItem != null) {
                if(aStartIndex + aCount <= iHighlightedIndex) {
                    iTopEntry -= aCount;
                    iTopEntry = (iProvider.Count < iLineCount) ? 0 : (iTopEntry > iProvider.Count - iLineCount) ? iProvider.Count - iLineCount : iTopEntry;
                    iHighlightedIndex -= (int)aCount;
                    SendMessage(new MsgHighlightUpdated(this, iHighlightedIndex, iHighlightedItem));
                } else if(aStartIndex <= iHighlightedIndex && iHighlightedIndex < aStartIndex + aCount) {   // if item has been deleted
                    iTopEntry = (iProvider.Count < iLineCount) ? 0 : (iTopEntry > iProvider.Count - iLineCount) ? iProvider.Count - iLineCount : iTopEntry;
                    iHighlightedItem.UnHighlight();
                    MsgUnHighlight msg = new MsgUnHighlight(this, iHighlightedItem);
                    iHighlightedItem = null;
                    iHighlightedIndex = -1;
                    iHighlightOffset = -1;
                    SendMessage(msg);
                    if(iAutoSelect) {
                        iHighlightedIndex = (int)(aStartIndex + aCount - 1);
                        if(iHighlightedIndex > (int)iProvider.Count - 1) {
                            iHighlightedIndex = (int)(iProvider.Count - 1);
                        }
                        if(iHighlightedIndex != -1) {
                            List<IListable> listables = iProvider.Entries((uint)iHighlightedIndex, 1);
                            iHighlightedItem = listables[0];
                        }
                        SendMessage(new MsgHighlight(this, iHighlightedIndex, iHighlightedItem));
                    }
                } else if(iProvider.Count - iTopEntry < iLineCount) {
                    iTopEntry = iProvider.Count - iLineCount;
                }
            } else {
                if(aStartIndex <= iTopEntry && iTopEntry > 0) {
                    if(aStartIndex + aCount <= iTopEntry) {
                        iTopEntry -= aCount;
                    } else {
                        iTopEntry = aStartIndex;
                        if(iTopEntry == iProvider.Count) {
                            iTopEntry = (iProvider.Count < iLineCount) ? 0 : iProvider.Count - iLineCount;
                        }
                    }
                } else if(iProvider.Count - iTopEntry < iLineCount) {
                    iTopEntry = iProvider.Count - iLineCount;
                }
            }
        } else {
            if(iHighlightedItem != null) {
                if(aStartIndex + aCount <= iHighlightedIndex) {
                    iHighlightedIndex -= (int)aCount;
                    SendMessage(new MsgHighlightUpdated(this, iHighlightedIndex, iHighlightedItem));
                } else if(aStartIndex <= iHighlightedIndex && iHighlightedIndex < aStartIndex + aCount) {   // if item has been deleted
                    iHighlightedItem.UnHighlight();
                    MsgUnHighlight msg = new MsgUnHighlight(this, iHighlightedItem);
                    iHighlightedItem = null;
                    iHighlightedIndex = -1;
                    iHighlightOffset = -1;
                    SendMessage(msg);
                }
            }
            iTopEntry = 0;
        }
        Trace.WriteLine(Trace.kGui, "NodeList.Delete: iHighlightedIndex=" + iHighlightedIndex + ", iTopEntry=" + iTopEntry);
        Assert.Check(iTopEntry >= 0);
        Refresh();
        //System.Console.WriteLine("<NodeList.Delete");
        UnlockUpdate();
        //System.Console.WriteLine("<<NodeList.Delete");
        if (EventDeleted != null) {
            EventDeleted(this, new EventArgsRange(aStartIndex, aCount));
        }
    }
    
    public void Update() {
		Console.WriteLine(">Update");
        LockUpdate();
	    Console.WriteLine(">>Update");
        Lock();
		Console.WriteLine(">>>Update");
        if(iDirty) {
            if(iHighlight.Object != null) {
                iHighlight.Object.Active = false;
            }
            
            int count = CompositeChildren.Count;
            for(uint i = 0; i < count; ++i) {
                RemoveCompositeChild(CompositeChild(0));
            }
            Assert.Check(iCompositeChildrenList.Count == 0);
            
            float translationY = 0;
            List<IListable> listables = iProvider.Entries(iTopEntry, Math.Min(iLineCount, iProvider.Count - iTopEntry));
            for(uint i = 0; i < listables.Count; ++i) {
                IListable listable = listables[(int)i];
                Assert.Check(listable != null);
                bool highlighted = iTopEntry + i == iHighlightedIndex;
                if(highlighted) {
                    highlighted = false;
                    if(iHighlight.Object != null) {
                        iHighlight.Object.LocalTranslation = new Vector3d(iHighlight.Object.LocalTranslation.X, translationY, 1);
                        iHighlight.Object.Active = iHighlightActive && iActive && iFocused;
                        highlighted = iHighlight.Object.Active;
                        iHighlightOffset = (int)i;
                        if(iHighlightedItem != listable) {
                            iHighlightedItem = listable;
                            SendMessage(new MsgHighlightUpdated(this, iHighlightedIndex, iHighlightedItem));
                        }
                    }
                }
                NodeHit n = iRenderer.ToNodeHit(listable, Width, iItemHeight, this, iTopEntry + i, highlighted);
                if(n != null) {
                    n.LocalTranslation.Y = translationY;
                    SetLine(i, n);
                } else {
                    n = new NodeHit();
                    //n.Namespace = Namespace;
                    //n.Name = Name + ".Placeholder";
                    n.LocalTranslation.Y = translationY;
                    SetLine(i, n);
                }
                translationY += n.Height;
            }
            for(uint i = (uint)listables.Count; i < iLineCount; ++i) {
                NodeHit n = new NodeHit();
                n.Width = Width;
                n.Height = iItemHeight;
                //n.Namespace = Namespace;
                //n.Name = Name + ".Placeholder";
                n.LocalTranslation.Y = translationY;
                SetLine(i, n);
                translationY += n.Height;
            }
            iDirty = false;
        }
        Unlock();
        UnlockUpdate();
		Console.WriteLine("<Update");
    }
    
    public void Refresh() {
		Console.WriteLine(">Refresh");
        Refresh(true);
        if (EventRefreshed != null) {
            EventRefreshed(this, EventArgs.Empty);
        }
		Console.WriteLine("<Refresh");
    }
    
    public void Refresh(bool aRender) {
		Console.WriteLine(">Refresh(bool)");
        LockUpdate();
        iDirty = true;
        UnlockUpdate();
        Update();
        if(aRender) {
            Render(true);
        }
		Console.WriteLine("<Refresh(bool)");
    }
    
    public void Clear() {
        //System.Console.WriteLine(">>NodeList.Clear");
        LockUpdate();
        //System.Console.WriteLine(">NodeList.Clear");
        iTopEntry = 0;
        if(iHighlightedItem != null) {
            iHighlightedItem.UnHighlight();
            MsgUnHighlight msg = new MsgUnHighlight(this, iHighlightedItem);
            iHighlightedItem = null;
            iHighlightedIndex = -1;
            iHighlightOffset = -1;
            SendMessage(msg);
        }
        Refresh();
        //System.Console.WriteLine("<NodeList.Clear");
        UnlockUpdate();
        //System.Console.WriteLine("<<NodeList.Clear");
        if (EventCleared != null) {
            EventCleared(this, EventArgs.Empty);
        }
    }
    
    public IListEntryProvider ListEntryProvider {
        get {
            LockUpdate();
            IListEntryProvider provider = iProvider;
            UnlockUpdate();
            return provider;
        }
        set {
            LockUpdate();
            iProvider = value;
            iProvider.SetList(this);
            iTopEntry = 0;
            iHighlightOffset = -1;
            if(iHighlightedItem != null) {
                iHighlightedItem.UnHighlight();
                MsgUnHighlight msg = new MsgUnHighlight(this, iHighlightedItem);
                iHighlightedItem = null;
                iHighlightedIndex = -1;
                SendMessage(msg);
            }
            UnlockUpdate();
        }
    }
    
    public IListableRenderer ListableRenderer {
        get {
            LockUpdate();
            IListableRenderer renderer = iRenderer;
            UnlockUpdate();
            return renderer;
        }
        set {
            LockUpdate();
            iRenderer = value;
            UnlockUpdate();
        }
    }
    
    public IStepCalculator StepCalculator {
        get {
            LockUpdate();
            IStepCalculator calculator = iStepCalculator;
            UnlockUpdate();
            return calculator;
        }
        set {
            LockUpdate();
            iStepCalculator = value;
            UnlockUpdate();
        }
    }
    
    public void SetHighlight(int aIndex, EAlignment aAlignment) {
        //System.Console.WriteLine(">>NodeList.SetHighlight(int)");
        bool unhighlight = false;
        bool highlight = false;
        bool refresh = false;

        LockUpdate();
        //System.Console.WriteLine(">NodeList.SetHighlight(int)");
        IListable oldHighlightedItem = iHighlightedItem;
        IListable listable = null;
        if(aIndex < iProvider.Count) {
            if(aIndex > -1) {
                int topEntry = 0;
                List<IListable> listables = iProvider.Entries((uint)aIndex, 1);
                listable = listables[0];
                switch(aAlignment) {
                    case EAlignment.EA_Top:
                        iTopEntry = (uint)aIndex;
                        iHighlightOffset = 0;
                        break;
                    case EAlignment.EA_Centre:
                        topEntry = aIndex - (int)(Math.Ceiling(iLineCount / 2.0f) - 1);
                        iHighlightOffset = aIndex - topEntry;
                        break;
                    case EAlignment.EA_Bottom:
                        topEntry = aIndex + (int)(iLineCount - 1);
                        iHighlightOffset = aIndex - topEntry;
                        break;
                    case EAlignment.EA_None:
                        topEntry = (int)iTopEntry;
                        iHighlightOffset = aIndex - (int)iTopEntry;
                        break;
                }
                if(topEntry < 0) {
                    iTopEntry = 0;
                    iHighlightOffset = aIndex;
                } else if(iProvider.Count > iLineCount) {
                    if(topEntry > iProvider.Count - iLineCount) {
                        iTopEntry = iProvider.Count - iLineCount;
                        iHighlightOffset = aIndex - (int)iTopEntry;
                    } else {
                        iTopEntry = (uint)topEntry;
                    }
                } else {
                    iTopEntry = 0;
                }
            }
            if(listable != null) {
                if(iHighlightedItem != listable) {
                    if(oldHighlightedItem != null) {
                        oldHighlightedItem.UnHighlight();
                        unhighlight = true;
                    }
                    iHighlightedItem = listable;
                    iHighlightedIndex = aIndex;
                    listable.Highlight();
                    highlight = true;
                    refresh = true;
                }/* else {
                    if(iFocused) {
                        if(listable != null) {
                            SendMessage(new MsgSelect(Fullname, aIndex, listable));
                        }
                    }
                }*/
            } else {
                if(iHighlightedItem != null) {
                    iHighlightedItem.UnHighlight();
                    iHighlightedItem = null;
                    iHighlightedIndex = -1;
                    unhighlight = true;
                    refresh = true;
                }
            }
        }
        Focused = true;
        //Refresh();
        Trace.WriteLine(Trace.kGui, "NodeList.SelectItem: " + iHighlightedItem + " at " + iHighlightedIndex);
        //System.Console.WriteLine("<NodeList.SetHighlight(int)");
        UnlockUpdate();

        if (refresh)
        {
            Refresh();
        }

        if (unhighlight)
        {
            SendMessage(new MsgUnHighlight(this, oldHighlightedItem));
        }

        if (highlight)
        {
            SendMessage(new MsgHighlight(this, aIndex, listable));
        }
        //System.Console.WriteLine("<<NodeList.SetHighlight(int)");
    }
    
    public void SelectItem(Vector3d aVector) {
        //System.Console.WriteLine(">>NodeList.SelectItem(Vector3d)");
        LockUpdate();
        Focused = true;
        //System.Console.WriteLine(">NodeList.SelectItem(Vector3d)");
        List<IListable> listables = iProvider.Entries(iTopEntry, (uint)Math.Min(iCompositeChildrenList.Count, iProvider.Count));
        IListable oldHighlightedItem = iHighlightedItem;
        bool found = false;
        for(uint i = 0; i < listables.Count && !found; ++i) {
            IListable listable = listables[(int)i];
            NodeHit n = CompositeChild((int)i) as NodeHit;
            if(n != null) {
                //Console.WriteLine(aVector + ", " + iListItems[i].WorldSrt.Translation);
                //Console.WriteLine(iListItems[i].Width + ", " + iListItems[i].Height);
                if(n.Inside(aVector - n.WorldSrt.Translation)) {
                    if(oldHighlightedItem != listable) {
                        if(oldHighlightedItem != null) {
                            oldHighlightedItem.UnHighlight();
                            SendMessage(new MsgUnHighlight(this, oldHighlightedItem));
                        }
                        iHighlightedItem = listable;
                        iHighlightedIndex = (int)(iTopEntry + i);
                        iHighlightOffset = (int)i;
                        listable.Highlight();
                        SendMessage(new MsgHighlight(this, iHighlightedIndex, iHighlightedItem));
                        Refresh();
                    }
                    else
                    {
                        listable.Select();
                        SendMessage(new MsgSelect(this, (int)(iTopEntry + i), listable));
                        Refresh();
                    }
                    found = true;
                }
            }
        }
        if(!found) {
            if(oldHighlightedItem != null) {
                oldHighlightedItem.UnHighlight();
                SendMessage(new MsgUnHighlight(this, oldHighlightedItem));
                iHighlightedItem = null;
                iHighlightedIndex = -1;
                iHighlightOffset = -1;
            }
            SendMessage(new MsgHighlight(this, iHighlightedIndex, iHighlightedItem));
            Refresh();
        }
        /*if(iHighlightedItem != oldHighlightedItem) {
            Refresh();
        }*/
        Trace.WriteLine(Trace.kGui, "NodeList.SelectItem: " + iHighlightedItem + " at " + iHighlightedIndex);
        //System.Console.WriteLine("<NodeList.SelectItem(Vector3d)");
        UnlockUpdate();
        //System.Console.WriteLine("<<NodeList.SelectItem(Vector3d)");
    }
    
    public uint TopEntry {
        get {
            LockUpdate();
            uint topEntry = iTopEntry;
            UnlockUpdate();
            return topEntry;
        }
    }
    
    public override bool Focused {
        set {
            base.Focused = value;
            Refresh();
        }
    }
    
    public bool AutoSelect {
        get {
            return iAutoSelect;
        }
        set {
            iAutoSelect = value;
        }
    }
    
    private void CalculateStep(float aSpeed) {
        //System.Console.WriteLine(">>NodeList.CalculateStep");
        LockUpdate();
        //System.Console.WriteLine(">NodeList.CalculateStep");
        if(aSpeed > 0) {
            ScrollDown(iStepCalculator.SpeedToStep(aSpeed, iProvider.Count));
        } else if(aSpeed < 0) {
            ScrollUp(iStepCalculator.SpeedToStep(-aSpeed, iProvider.Count));
        }
        //System.Console.WriteLine("<NodeList.CalculateStep");
        UnlockUpdate();
        //System.Console.WriteLine("<<NodeList.CalculateStep");
    }
    
    private void ScrollUp(uint aStep) {
        LockUpdate();       
        int value = (int)iTopEntry - (int)aStep;
        if(value < 0) {
            iTopEntry = 0;
        } else {
            iTopEntry = (uint)value;
        }
        Focused = true;
        UnlockUpdate();
        SendMessage(new MsgTopEntry(this, iTopEntry));
        Refresh();
    }
    
    private void ScrollHighlightUp(uint aStep) {
        //System.Console.WriteLine(">>NodeList.ScrollHighlightUp");
        MsgUnHighlight unhighlightMsg = null;
        MsgHighlight highlightMsg = null;
        MsgHighlightUpdated updateHighlightMsg = null;
        LockUpdate();
        uint topEntryOld = iTopEntry;
        //System.Console.WriteLine(">NodeList.ScrollHighlightUp");
        //System.Console.WriteLine(">NodeList.ScrollHighlightUp: iTopEntry=" + iTopEntry + ", aStep=" + aStep);
        IListable oldHighlightedItem = iHighlightedItem;
        int newOffset = (iHighlightOffset > 0) ? iHighlightOffset - (int)aStep : -(int)aStep;
        if(iHighlightedItem != null) {
            if(iHighlightedIndex < iTopEntry || iHighlightedIndex > iTopEntry + iLineCount) {
                iTopEntry = (uint)iHighlightedIndex;
                iHighlightOffset = 0;
                int linecount = (iProvider.Count > iLineCount) ? (int)iLineCount : (int)iProvider.Count;
                int value = ((int)iProvider.Count - 1) - (linecount - 1);
                if(iTopEntry > value) {
                    iTopEntry = (value > 0) ? (uint)value : 0;
                    iHighlightOffset = iHighlightedIndex - (int)iTopEntry;
                }
                newOffset = (iHighlightOffset > 0) ? iHighlightOffset - (int)aStep : -(int)aStep;
            }
        }
        if(newOffset > 0) {
            iHighlightOffset = newOffset;
        } else {
            iHighlightOffset = 0;
            int value = (int)iTopEntry + newOffset;
            if(value < 0) {
                iTopEntry = 0;
            } else {
                iTopEntry = (uint)value;
            }
        }
        int index = iHighlightedIndex - (int)aStep;
        if(index < 0) {
            index = 0;
        }
        if(iProvider.Count > 0) {
            List<IListable> listables = iProvider.Entries((uint)index, 1);
            IListable listable = listables[0];
            if(oldHighlightedItem != listable) {
                if(oldHighlightedItem != null) {
                    oldHighlightedItem.UnHighlight();
                    unhighlightMsg = new MsgUnHighlight(this, oldHighlightedItem);
                }
                iHighlightedItem = listable;
                iHighlightedIndex = index;
                listable.Highlight();
                highlightMsg = new MsgHighlight(this, iHighlightedIndex, iHighlightedItem);
            }
        }
        if(iHighlightedItem != null && oldHighlightedItem == iHighlightedItem) {
            updateHighlightMsg = new MsgHighlightUpdated(this, iHighlightedIndex, iHighlightedItem);
        }
        Focused = true;
        //System.Console.WriteLine("<NodeList.ScrollHighlightUp");
        uint topEntryNew = iTopEntry;
        UnlockUpdate();
        if(unhighlightMsg != null) {
            SendMessage(unhighlightMsg);
        }
        if(highlightMsg != null) {
            SendMessage(highlightMsg);
        }
        if(updateHighlightMsg != null) {
            SendMessage(updateHighlightMsg);
        }
        if (topEntryNew != topEntryOld)
        {
            SendMessage(new MsgTopEntry(this, topEntryNew));
        }
        Refresh();
        //System.Console.WriteLine("<<NodeList.ScrollHighlightUp");
    }
    
    private void ScrollDown(uint aStep) {
        bool sendEvent = false;
        LockUpdate();
        if(iProvider.Count > iLineCount) {
            iTopEntry += aStep;
            int linecount = (iProvider.Count > iLineCount) ? (int)iLineCount : (int)iProvider.Count;
            int value = ((int)iProvider.Count - 1) - (linecount - 1);
            if(iTopEntry > value) {
                iTopEntry = (value > 0) ? (uint)value : 0;
            }
            sendEvent = true;
        }
        Focused = true;
        UnlockUpdate();
        if(sendEvent) {
            SendMessage(new MsgTopEntry(this, iTopEntry));
        }
        Refresh();
    }
    
    private void ScrollHighlightDown(uint aStep) {
        //System.Console.WriteLine(">>NodeList.ScrollHighlightDown");
        MsgUnHighlight unhighlightMsg = null;
        MsgHighlight highlightMsg = null;
        MsgHighlightUpdated updateHighlightMsg = null;
        LockUpdate();
        uint topEntryOld = iTopEntry;
        //System.Console.WriteLine(">NodeList.ScrollHighlightDown");
        Trace.WriteLine(Trace.kGui, ">NodeList.ScrollHighlightDown: iTopEntry=" + iTopEntry + ", aStep=" + aStep);
        //System.Console.WriteLine(">NodeList.ScrollHighlightDown: iTopEntry=" + iTopEntry + ", aStep=" + aStep + ", iHighlightOffset=" + iHighlightOffset + ", iHighlightedIndex=" + iHighlightedIndex);
        IListable oldHighlightedItem = iHighlightedItem;
        int linecount = (iProvider.Count > iLineCount) ? (int)iLineCount : (int)iProvider.Count;
        int newOffset = (iHighlightOffset < linecount) ? iHighlightOffset + (int)aStep : linecount;
        int index = iHighlightedIndex + (int)aStep;
        if(index > (int)iProvider.Count - 1) {
            index = (int)iProvider.Count - 1;
        }
        if(iHighlightedItem != null) {
            if(iHighlightedIndex < iTopEntry || iHighlightedIndex > iTopEntry + iLineCount) {
                iTopEntry = (uint)iHighlightedIndex;
                iHighlightOffset = 0;
                int value = ((int)iProvider.Count - 1) - (linecount - 1);
                if(iTopEntry > value) {
                    iTopEntry = (value > 0) ? (uint)value : 0;
                    iHighlightOffset = iHighlightedIndex - (int)iTopEntry;
                }
                newOffset = (iHighlightOffset < linecount) ? iHighlightOffset + (int)aStep : linecount;
            }
        }
        if(newOffset > linecount - 1) {
            iHighlightOffset = linecount - 1;
            iTopEntry += (uint)(newOffset - (linecount - 1));
            int value = ((int)iProvider.Count - 1) - (linecount - 1);
            if(iTopEntry > value) {
                iTopEntry = (value > 0) ? (uint)value : 0;
            }
        } else {
            if(index - newOffset < iTopEntry || index - newOffset > iTopEntry + iLineCount) {
                iTopEntry = (uint)(index - newOffset);
            }
            iHighlightOffset = newOffset;
        }
        //System.Console.WriteLine("NodeList.ScrollDown: iTopEntry=" + iTopEntry + ", iHighlightOffset=" + iHighlightOffset + ", index=" + index);
        if(index != -1) {
            List<IListable> listables = iProvider.Entries((uint)index, 1);
            IListable listable = listables[0];
            if(oldHighlightedItem != listable) {
                if(oldHighlightedItem != null) {
                    oldHighlightedItem.UnHighlight();
                    unhighlightMsg = new MsgUnHighlight(this, oldHighlightedItem);
                }
                iHighlightedItem = listable;
                iHighlightedIndex = index;
                listable.Highlight();
                highlightMsg = new MsgHighlight(this, iHighlightedIndex, iHighlightedItem);
            }
        }
        if(iHighlightedItem != null && oldHighlightedItem == iHighlightedItem) {
            updateHighlightMsg = new MsgHighlightUpdated(this, iHighlightedIndex, iHighlightedItem);
        }
        Focused = true;
        Trace.WriteLine(Trace.kGui, "NodeList.ScrollHighlightDown: iTopEntry=" + iTopEntry + ", iHighlightOffset=" + iHighlightOffset);
        //System.Console.WriteLine("NodeList.ScrollHighlightDown: iTopEntry=" + iTopEntry + ", iHighlightOffset=" + iHighlightOffset);
        //System.Console.WriteLine("<NodeList.ScrollHighlightDown");
        uint topEntryNew = iTopEntry;
        UnlockUpdate();
        if(unhighlightMsg != null) {
            SendMessage(unhighlightMsg);
        }
        if(highlightMsg != null) {
            SendMessage(highlightMsg);
        }
        if(updateHighlightMsg != null) {
            SendMessage(updateHighlightMsg);
        }
        if (topEntryNew != topEntryOld)
        {
            SendMessage(new MsgTopEntry(this, topEntryNew));
        }
        Refresh();
        //System.Console.WriteLine("<<NodeList.ScrollHighlightDown");
    }
    
    protected override void UpdateHighlight() {
        //System.Console.WriteLine(">>NodeList.UpdateHighlight");
        LockUpdate();
        //System.Console.WriteLine(">NodeList.UpdateHighlight");
        if(iHighlight.Object != null) {
            iHighlight.Object.Active = Active && iHighlightActive && (iHighlightedIndex != -1) && iFocused &&
                iHighlightedIndex >= iTopEntry && iHighlightedIndex < iTopEntry + iLineCount;
        }
        //System.Console.WriteLine("<NodeList.UpdateHighlight");
        UnlockUpdate();
        //System.Console.WriteLine("<<NodeList.UpdateHighlight");
    }
				
	private void LockUpdate() {
		//iUpdateMutex.WaitOne();
	}
				
	private void UnlockUpdate() {
		//iUpdateMutex.ReleaseMutex();
	}
    
    private IListEntryProvider iProvider = new ListEntryProviderNull();
    private IListableRenderer iRenderer = new ListableRendererNull();
    private IStepCalculator iStepCalculator = new StepCalculatorDefault();
    private uint iTopEntry = 0;
    private int iHighlightOffset = -1;
    private bool iDirty = true;
    private Mutex iUpdateMutex = new Mutex(false);
    private bool iAutoSelect = true;
}

public class ListCache : IListEntryProvider, IList
{
    public ListCache(IListEntryProvider aProvider) {
        iCacheSize = kDefaultCacheSize;
        iCacheTimes = new List<uint>();
        iCache = new Dictionary<uint, IListable>();
        iProvider = aProvider;
        iProvider.SetList(this);
    }
    
    public ListCache(IListEntryProvider aProvider, uint aCacheSize) {
        iCacheSize = aCacheSize;
        iCacheTimes = new List<uint>();
        iCache = new Dictionary<uint, IListable>();
        iProvider = aProvider;
        iProvider.SetList(this);
    }
    
    public virtual void Dispose() {
        if(iProvider != null) {
            iProvider.Dispose();
        }
        //Clear();
    }

    public event EventHandler<EventArgsRange> EventAdded;
    public event EventHandler<EventArgsRange> EventDeleted;
    public event EventHandler<EventArgs> EventCleared;
    public event EventHandler<EventArgs> EventRefreshed;
    
    public void SetList(IList aList) {
        iList = aList;
    }
    
    public uint Count {
        get {
            return iProvider.Count;
        }
    }
    
    private IListable Entry(uint aIndex) {
        Trace.WriteLine(Trace.kGui, ">ListCache.Entry: aIndex=" + aIndex);
        IListable listable = null;
        try
        {
            Lock();
            if (iProvider != null)
            {
                if (!iCache.TryGetValue(aIndex, out listable))
                {
                    listable = CacheListableEntries(aIndex);
                }
            }
        }
        finally
        {
            Unlock();
        }
        return listable;
    }
    
    public virtual List<IListable> Entries(uint aStartIndex, uint aCount) {
        List<IListable> result = new List<IListable>();
        for(uint i = aStartIndex; i < aStartIndex + aCount; ++i) {
            result.Add(Entry(i));
        }
        return result;
    }
    
    public void Add(uint aStartIndex, uint aCount) {
        Lock();
        Dictionary<uint, IListable> newCache = new Dictionary<uint, IListable>();
        for(uint i = 0; i < iCacheTimes.Count; ++i) {
            uint key = iCacheTimes[(int)i];
            IListable listable = iCache[key];
            if(key > (int)aStartIndex - 1) {
                uint newKey = key + aCount;
                iCache.Remove(key);
                iCacheTimes[(int)i] = newKey;
                newCache.Add(newKey, listable);
            } else {
                newCache.Add(key, listable);
            }
        }
        iCache = newCache;
        Unlock();
        if(iList != null) {
            iList.Add(aStartIndex, aCount);
        }
        if (EventAdded != null) {
            EventAdded(this, new EventArgsRange(aStartIndex, aCount));
        }
    }
    
    public void Delete(uint aStartIndex, uint aCount) {
        Lock();
        for(uint i = aStartIndex; i < aStartIndex + aCount; ++i) {
            iCache.Remove(i);
            iCacheTimes.Remove(i);
        }
        Dictionary<uint, IListable> newCache = new Dictionary<uint, IListable>();
        for(uint i = 0; i < iCacheTimes.Count; ++i) {
            uint key = iCacheTimes[(int)i];
            IListable listable = iCache[key];
            if(key > aStartIndex + aCount - 1) {
                uint newKey = key - aCount;
                iCacheTimes[(int)i] = newKey;
                newCache.Add(newKey, listable);
            } else {
                newCache.Add(key, listable);
            }
        }
        iCache = newCache;
        Unlock();
        if(iList != null) {
            iList.Delete(aStartIndex, aCount);
        }
        if (EventDeleted != null) {
            EventDeleted(this, new EventArgsRange(aStartIndex, aCount));
        }
    }
    
    public void Clear() {
        Lock();
        iCache.Clear();
        iCacheTimes.Clear();
        Unlock();
        if(iList != null) {
            iList.Clear();
        }
        if (EventCleared != null) {
            EventCleared(this, EventArgs.Empty);
        }
    }
    
    public virtual void Refresh() {
        Lock();
        iCache.Clear();
        iCacheTimes.Clear();
        Unlock();
        if(iList != null) {
            iList.Refresh();
        }
        if (EventRefreshed != null) {
            EventRefreshed(this, EventArgs.Empty);
        }
    }
    
    protected IListable CacheListableEntries(uint aIndex) {
        uint cacheRange = iCacheRange > Count ? (uint)Math.Round(Count * 0.5f) : iCacheRange;
        cacheRange = cacheRange == 0 ? 1 : cacheRange;
        uint offset = (((int)aIndex - cacheRange) < 0) ? (uint)-((int)aIndex - cacheRange) : 0;
        uint startIndex = (((int)aIndex - cacheRange) < 0) ? 0 : aIndex - cacheRange;
        uint endIndex = ((aIndex + cacheRange + offset) > Count) ? Count : aIndex + cacheRange + offset;
        uint count = ((int)endIndex - (int)startIndex) < 0 ? 0 : endIndex - startIndex;
        Trace.WriteLine(Trace.kGui, "ListCache.CacheListableEntries: count=" + count + ", iCacheSize=" + iCacheSize + ", aIndex=" + aIndex + ", cacheRange=" + cacheRange + ", Count=" + Count + ", startIndex=" + startIndex + ", endIndex=" + endIndex + ", iProvider=" + iProvider);
        count = count > iCacheSize ? iCacheSize : count;
        // It is possible for the playlist to have changed before the cache has a chance to retrieve a particular entry,
        // it is therefore very possible to have a count == 0
        if(count > 0) {
            List<IListable> result = CacheListableEntries(startIndex, count);
            int index = (int)aIndex - (int)startIndex;
            if (index < result.Count) {
                return result[index];
            }
        }
        return null;
    }
    
    protected List<IListable> CacheListableEntries(uint aStartIndex, uint aCount) {
        Trace.WriteLine(Trace.kGui, ">ListCache.CacheListableEntries: aStartIndex=" + aStartIndex + ", aCount=" + aCount);
        List<IListable> result = iProvider.Entries(aStartIndex, aCount);
        Lock();
        for(uint i = aStartIndex; i < aStartIndex + aCount; ++i) {
            int index = (int)(i - aStartIndex);
            if (index < result.Count) {
                IListable value;
                if(!iCache.TryGetValue(i, out value)) {
                    if(result[index] != null) {
                        iCache.Add(i, result[index]);
                        if (iCacheTimes.Count < iCacheSize) {
                            iCacheTimes.Add(i);
                        } else {    // replace oldest cache item with new cache item
                            Trace.WriteLine(Trace.kGui, "ListCache.CacheListableEntries: Cache full, replacing " + iCacheIndex + ": " + iCacheTimes[(int)iCacheIndex] + " with " + i);
                            iCache.Remove(iCacheTimes[(int)iCacheIndex]);
                            iCacheTimes[(int)iCacheIndex] = i;
                        }
                        iCacheIndex++;
                        iCacheIndex %= iCacheSize;
                    }
                }
            } else {
                break;
            }
        }
        Unlock();
        Trace.WriteLine(Trace.kGui, "<ListCache.CacheListableEntries");
        return result;
    }
				
	private void Lock() {
		//iMutex.WaitOne();
	}
				
    
	private void Unlock() {
		//iMutex.ReleaseMutex();
	}
    
    protected const uint kDefaultCacheSize = 100;
    protected IList iList = null;
    protected IListEntryProvider iProvider = null;
    protected uint iCacheRange = 20;
    protected uint iCacheSize = 0;
    protected uint iCacheIndex = 0;
    protected Dictionary<uint, IListable> iCache = null;
    protected List<uint> iCacheTimes = null;
    protected Mutex iMutex = new Mutex(false);
}

public interface IListableFinder
{
    bool Find(IListable aListable);
}

public sealed class IListablePredicate
{
    public IListablePredicate(IListable aListable) {
        iListable = aListable;
    }
    
    public bool Find(IListable aListable) {
        return iListable == aListable;
    }
    
    private IListable iListable = null;
}


public class ListSorted : IListEntryProvider
{
    public void Dispose() {
        Trace.WriteLine(Trace.kGui, ">ListSorted.Dispose: iEntries.Count=" + iEntries.Count);
        Lock();
        iEntries.Clear();
        Unlock();
    }
    
    public void SetList(IList aList) {
        iList = aList;
    }
    
    public uint Count {
        get {
            Lock();
            uint count = (uint)iEntries.Count;
            Unlock();
            return count;
        }
    }
    
    public List<IListable> Entries(uint aStartIndex, uint aCount) {
        List<IListable> list = new List<IListable>();
        try {
            Lock();
            list = iEntries.GetRange((int)aStartIndex, (int)aCount);
        } catch(ArgumentException) {
            for(uint i = 0; i < aCount; ++i) {
                list.Add(null);
            }
        } finally {
            Unlock();
        }
        return list;
    }
    
    public void Add(IListable aListable, Comparison<IListable> aComparer) {
        //System.Console.WriteLine(">>ListSorted.Add");
        Lock();
        //System.Console.WriteLine(">ListSorted.Add");
        iEntries.Add(aListable);
        iEntries.Sort(aComparer);
        //System.Console.WriteLine("<ListSorted.Add");
        Unlock();
        //System.Console.WriteLine("<<ListSorted.Add");
        if(iList != null) {
            iList.Add((uint)iEntries.FindIndex(new IListablePredicate(aListable).Find), 1);
        }
    }
    
    public void Delete(IListable aListable) {
        Lock();
        uint index = (uint)iEntries.FindIndex(new IListablePredicate(aListable).Find);
        iEntries.Remove(aListable);
        Unlock();
        if(iList != null) {
            iList.Delete(index, 1);
        }
    }
    
    public void Clear() {
        Lock();
        iEntries.Clear();
        Unlock();
        if(iList != null) {
            iList.Clear();
        }
    }
    
    public IListable Find(IListableFinder aListableFinder) {
        //System.Console.WriteLine(">>ListSorted.Find");
        Lock();
        //System.Console.WriteLine(">ListSorted.Find");
        IListable listable = iEntries.Find(aListableFinder.Find);
        //System.Console.WriteLine("<ListSorted.Find");
        Unlock();
        //System.Console.WriteLine("<<ListSorted.Find");
        return listable;
    }
				
	private void Lock() {
		//iMutex.WaitOne();
	}
				
    
	private void Unlock() {
		//iMutex.ReleaseMutex();
	}
    
    private IList iList = null;
    private List<IListable> iEntries = new List<IListable>();
    private Mutex iMutex = new Mutex(false);
}

public class ListUnsorted : IListEntryProvider
{
    public void Dispose() {
        Lock();
        iEntries.Clear();
        Unlock();
    }
    
    public void SetList(IList aList) {
        iList = aList;
    }
    
    public uint Count {
        get {
            Lock();
            uint count = (uint)iEntries.Count;
            Unlock();
            return count;
        }
    }
    
    public List<IListable> Entries(uint aStartIndex, uint aCount) {
        List<IListable> list = new List<IListable>();
        try {
            Lock();
            list = iEntries.GetRange((int)aStartIndex, (int)aCount);
        } catch(ArgumentException) {
            for(uint i = 0; i < aCount; ++i) {
                list.Add(null);
            }
        } finally {
            Unlock();
        }
        return list;
    }
    
    public void Add(IListable aListable) {
        Lock();
        iEntries.Add(aListable);
        Unlock();
        if(iList != null) {
            iList.Add((uint)iEntries.FindIndex(new IListablePredicate(aListable).Find), 1);
        }
    }
    
    public void Add(uint aStartIndex, List<IListable> aListables) {
        Lock();
        iEntries.InsertRange((int)aStartIndex, aListables);
        Unlock();
        if(iList != null) {
            iList.Add(aStartIndex, (uint)aListables.Count);
        }
    }
    
    public void Delete(IListable aListable) {
        Lock();
        uint index = (uint)iEntries.FindIndex(new IListablePredicate(aListable).Find);
        iEntries.Remove(aListable);
        Unlock();
        if(iList != null) {
            iList.Delete(index, 1);
        }
    }
    
    public void Delete(uint aStartIndex, uint aCount) {
        Lock();
        iEntries.RemoveRange((int)aStartIndex, (int)aCount);
        Unlock();
        if(iList != null) {
            iList.Delete(aStartIndex, aCount);
        }
    }
    
    public void Clear() {
        Lock();
        iEntries.Clear();
        Unlock();
        if(iList != null) {
            iList.Clear();
        }
    }
    
    public IListable Find(IListableFinder aListableFinder) {
        Lock();
        IListable listable = iEntries.Find(aListableFinder.Find);
        Unlock();
        return listable;
    }
				
	private void Lock() {
		Console.WriteLine("here");
		iMutex.WaitOne();
		Console.WriteLine("blah: " + ++reference);
		iMutex.ReleaseMutex();
		//iMutex.WaitOne();
	}
				
    
	private void Unlock() {
		/*iMutex.WaitOne();
		Console.WriteLine(--reference);
		iMutex.ReleaseMutex();*/
		//iMutex.ReleaseMutex();
	}
				
	private int reference = 0;
    private IList iList = null;
    private List<IListable> iEntries = new List<IListable>();
    private Mutex iMutex = new Mutex(false);
}
    
} // Scenegraph
} // Gui
} // Linn
