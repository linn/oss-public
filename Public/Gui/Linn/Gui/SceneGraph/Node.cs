using Linn;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.Threading;

namespace Linn {
namespace Gui {
namespace Scenegraph {

public class DuplicateNode : System.Exception {
    public DuplicateNode(string aNode) : base(aNode) {}
}

internal class InvalidParent : System.Exception
{
}

public class Node : Plugin, IMessengerObserver, ICloneable
{
    public class NearToFar : IComparer<Node>
    {
        public int Compare(Node x, Node y) {
            return(x.WorldSrt.Translation.Z.CompareTo(y.WorldSrt.Translation.Z));
        }
    }
    
    public class FarToNear : IComparer<Node>
    {
        public int Compare(Node x, Node y) {
            return(y.WorldSrt.Translation.Z.CompareTo(x.WorldSrt.Translation.Z));
        }
    }

    public Node() : base(UniqueName("Node")) {
    }
    
    public Node(string aName) : base(aName) {
    }
    
    public override Object Clone() {
        Node n = new Node();        
        Clone(n);
        return n;
    }
    
    protected void Clone(Node aNode) {
        Clone((Plugin)aNode);
        aNode.iRoot = iRoot;
        aNode.iActive = iActive;
        aNode.iParent = iParent;
        aNode.iChildrenList.AddRange(iChildrenList);
        aNode.iCompositeChildrenList.AddRange(iCompositeChildrenList);
        aNode.iTranslatorIn = iTranslatorIn;    // shallow copy
        aNode.iTranslatorOut = iTranslatorOut;  // shallow copy
        aNode.iLocalSrt = (Srt)iLocalSrt.Clone();
        aNode.iWorldSrt = (Srt)iWorldSrt.Clone();
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Parent");
        if(list != null) {
            Assert.Check(list.Count == 1);
            if(list[0].FirstChild != null) {
                iParent = new ReferencePlugin<Node>(list[0].FirstChild.Value);
                /*if(list[0].Value == "") {
                    iRoot = true;
                }*/
            } else {
                iRoot = true;
            }
        }
        
        list = aXmlNode.SelectNodes("Active");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iActive = bool.Parse(list[0].FirstChild.Value);
        }
        
        list = aXmlNode.SelectNodes("Srt");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iLocalSrt.Load(list[0]);
        }
        
        list = aXmlNode.SelectNodes("InMessageTranslator");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iTranslatorIn.Load(list[0]);
        }

        list = aXmlNode.SelectNodes("OutMessageTranslator");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iTranslatorOut.Load(list[0]);
        }
        
        list = aXmlNode.SelectNodes("Children");
        if(list != null) {
            Assert.Check(list.Count == 1);
            list = list[0].SelectNodes("Child");
            foreach(XmlNode node in list) {
                ReferencePlugin<Node> child = new ReferencePlugin<Node>(node.FirstChild.Value);
                iChildrenList.Add(child);
                // TODO: check for duplicate children
            }
        }
    }
    
    public override void Link() {
        //Trace.WriteLine(Trace.kGui, "Linking Node: " + Fullname);
        base.Link();
        bool refNode = (Parent != null);
        iParent.Link();
        // we need to add nodes that are child nodes in external packages
        if(Parent != null && !refNode) {
            if(Parent.Namespace != Namespace) {
                Node node = Parent;
                Parent = null;
                node.AddChild(this);
            }
        }
        iTranslatorIn.Link();
        iTranslatorOut.Link();
        foreach(ReferencePlugin<Node> child in iChildrenList) {
            child.Link();
            // when need to set the correct parent for root nodes that
            // are referenced in external packages
            if(child.Object.iParent.Name == "") {
                child.Object.Parent = this;
            }
        }
        Update(false, true);
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        aWriter.WriteStartElement("Parent");
        if(Parent != null && !iRoot) {
            aWriter.WriteString(Parent.Fullname);
        }
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Active");
        aWriter.WriteString(iActive.ToString());
        aWriter.WriteEndElement();
        
        iLocalSrt.Save(aWriter);
        
        aWriter.WriteStartElement("InMessageTranslator");
        iTranslatorIn.Save(aWriter);
        aWriter.WriteEndElement();
        
        aWriter.WriteStartElement("OutMessageTranslator");
        iTranslatorOut.Save(aWriter);
        aWriter.WriteEndElement();
        
        aWriter.WriteStartElement("Children");
        foreach(ReferencePlugin<Node> node in iChildrenList) {
            aWriter.WriteStartElement("Child");
            aWriter.WriteString(node.Object.Fullname);
            aWriter.WriteEndElement();
        }   
        aWriter.WriteEndElement();
    }
    
    public bool Active {
        get {
            return iActive;
        }
        set {
            bool update = false;
            Lock();
            if(iActive != value) {
                iActive = value;
                Update(false, false);
                foreach(ReferencePlugin<Node> child in iChildrenList) {
                    child.Object.Active = value;
                }
                foreach(ReferencePlugin<Node> child in iCompositeChildrenList) {
                    child.Object.Active = value;
                }
                update = true;
            }
            Unlock();
            if(update) {
                OnSetActive();
            }
        }
    }
    
    public virtual void OnSetActive() {
        MsgActiveChanged msg = new MsgActiveChanged(this, !iActive, iActive);
        SendMessage(msg);
    }
    
    public Node Parent {
        get {
            return iParent.Object;
        }
        set {
            Lock();
            iParent = new ReferencePlugin<Node>(value);
            Unlock();
            ObserverUpdate();
        }
    }
    
    public bool IsRoot {
        get {
            return iRoot;
        }
    }
    
    public Srt LocalSrt {
        get {
            return iLocalSrt;
        }
    }
    
    public Srt WorldSrt {
        get {
            return iWorldSrt;
        }
    }
    
    public virtual Vector3d LocalTranslation {
        get {
            return iLocalSrt.Translation;
        }
        set {
            Lock();
            iLocalSrt.Translation = value;
            Update(true, false);
            Unlock();
        }
    }
    
    public virtual Vector3d WorldTranslation {
        get {
            return iWorldSrt.Translation;
        }
        set {
            Lock();
            iWorldSrt.Translation = value;
            //Trace.WriteLine(Trace.kGui, "Node.WorldTranslation: parent = " + Parent);
            if(Parent != null) {
                iLocalSrt.Translation = iWorldSrt.Translation - Parent.WorldTranslation;
                //Trace.WriteLine(Trace.kGui, Fullname + ": WorldTrans = " + iWorldSrt.Translation);
                //Trace.WriteLine(Trace.kGui, Fullname + ": Parent WorldTrans = " + Parent.WorldTranslation);
                //Trace.WriteLine(Trace.kGui, Fullname + ": LocalTrans = " + iLocalSrt.Translation);
            }
            else {
                iLocalSrt.Translation = value;
                //Trace.WriteLine(Trace.kGui, Fullname + ": WorldTrans = " + iWorldSrt.Translation);
                //Trace.WriteLine(Trace.kGui, Fullname + ": LocalTrans = " + iLocalSrt.Translation);
            }
            Update(true, false);
            Unlock();
        }
    }
    
    public MessageTranslator TranslatorIn {
        get {
            return iTranslatorIn;
        }
    }
    
    public MessageTranslator TranslatorOut {
        get {
            return iTranslatorOut;
        }
    }
    
    public void AddChild(Node aChild) {
        if(aChild.Parent == this) {
            throw new DuplicateNode(aChild.Fullname);
        }
        
        if(aChild.Parent != null) {
            aChild.Parent.RemoveChild(aChild);
        }
        
        aChild.Parent = this;
        Lock();
        iChildrenList.Add(new ReferencePlugin<Node>(aChild));
        //aChild.WorldTranslation = aChild.WorldTranslation;
        aChild.Update(true, true);
        Unlock();
        ObserverUpdate();
    }
    
    protected void AddCompositeChild(Node aChild) {
        if(aChild.Parent == this) {
            throw new DuplicateNode(aChild.Fullname);
        }
        
        if(aChild.Parent != null) {
            aChild.Parent.RemoveChild(aChild);
        }
        
        aChild.Parent = this;
        Lock();
        iCompositeChildrenList.Add(new ReferencePlugin<Node>(aChild));
        //aChild.WorldTranslation = aChild.WorldTranslation;
        aChild.Update(true, true);
        Unlock();
        ObserverUpdate();
    }
    
    public void RemoveChild(Node aChild) {
        if(aChild.Parent != this) {
            throw new InvalidParent();
        }
        Lock();
        ReferencePlugin<Node> obj = null;
        foreach(ReferencePlugin<Node> node in iChildrenList) {
            if(node.Object == aChild) {
                obj = node;
                break;
            }
        }
        iChildrenList.Remove(obj);
        aChild.Parent = null;
        Unlock();
        ObserverUpdate();
    }
    
    protected void RemoveCompositeChild(Node aChild) {
        if(aChild.Parent != this) {
            throw new InvalidParent();
        }
        Lock();
        ReferencePlugin<Node> obj = null;
        foreach(ReferencePlugin<Node> node in iCompositeChildrenList) {
            if(node.Object == aChild) {
                obj = node;
                break;
            }
        }
        iCompositeChildrenList.Remove(obj);
        aChild.Parent = null;
        Unlock();
        ObserverUpdate();
    }
    
    protected void ReplaceCompositeChild(uint aIndex, Node aChild) {
        if(aChild.Parent == this) {
            throw new DuplicateNode(aChild.Fullname);
        }
        if(aChild.Parent != null) {
            aChild.Parent.RemoveChild(aChild);
        }
        aChild.Parent = this;
        Lock();
        iCompositeChildrenList[(int)aIndex].Object.Parent = null;
        iCompositeChildrenList[(int)aIndex] = new ReferencePlugin<Node>(aChild);
        Unlock();
        ObserverUpdate();
    }
    
    public List<ReferencePlugin<Node>> Children {
        get {
            return iChildrenList;
        }
    }
    
    public List<ReferencePlugin<Node>> CompositeChildren {
        get {
            return iCompositeChildrenList;
        }
    }
    
    public Node Child(int aIndex) {
        Lock();
        ReferencePlugin<Node> result = iChildrenList[aIndex];
        Unlock();
        return result.Object;
    }
    
    public Node CompositeChild(int aIndex) {
        Lock();
        ReferencePlugin<Node> result = iCompositeChildrenList[aIndex];
        Unlock();
        return result.Object;
    }
    
    public virtual void Update(bool aRecurse, bool aForce) {
        bool update = false;
        if(iActive == true || aForce == true) {
            if(Parent != null) {
                Vector3d parentPos = Parent.WorldSrt.Translation;
                iWorldSrt.Translation = parentPos + iLocalSrt.Translation;
            } else {
                iWorldSrt.Translation = iLocalSrt.Translation;
            }
            //Trace.WriteLine(Trace.kGui, "Updated " + Fullname + ": LocalTrans = " + iLocalSrt.Translation + ", WorldTrans = " + iWorldSrt.Translation);
            if(aRecurse == true) {
                ReferencePlugin<Node>[] list;
                Lock();
                list = iChildrenList.ToArray();
                Unlock();
                foreach(ReferencePlugin<Node> node in list) {
                    node.Object.Update(true, aForce);
                }
                Lock();
                list = iCompositeChildrenList.ToArray();
                Unlock();
                foreach(ReferencePlugin<Node> node in list) {
                    node.Object.Update(true, aForce);
                }
            }
            update = true;
        }
        if(update) {
            ObserverUpdate();
        }
    }
    
    public virtual void Render() {
    }
    
    public virtual void Render(bool aForce) {
    }
    
    public virtual void BeginDraw() {
    }
    
    public virtual void Draw(IRenderer aRenderer) {
    }
    
    public virtual void EndDraw() {
    }
    
    public virtual void Visit(Visitor aVisitor, bool aVisitNonActive) {
        aVisitor.AcceptNode(this);
        ReferencePlugin<Node>[] list;
        Lock();
        list = iChildrenList.ToArray();
        Unlock();
        foreach(ReferencePlugin<Node> node in list) {
            if(node.Object.Active || aVisitNonActive) {
                node.Object.Visit(aVisitor, aVisitNonActive);
            }
        }
        Lock();
        list = iCompositeChildrenList.ToArray();
        Unlock();
        foreach(ReferencePlugin<Node> node in list) {
            if(node.Object.Active || aVisitNonActive) {
                node.Object.Visit(aVisitor, aVisitNonActive);
            }
        }
    }
    
    public override List<string> PackageDependencies() {
        Lock();
        List<string> deps = base.PackageDependencies();
        foreach(ReferencePlugin<Node> n in iChildrenList) {
            Trace.WriteLine(Trace.kGui, "Node.PackageDependencies: " + n.Object.Namespace);
            deps.Add(n.Object.Namespace);
        }
        foreach(Translator t in iTranslatorIn.Translators) {
            Trace.WriteLine(Trace.kGui, "Node.PackageDependencies: " + t.ToMessage.Namespace);
            deps.Add(t.ToMessage.Namespace);
            Trace.WriteLine(Trace.kGui, "Node.PackageDependencies: " + t.FromMessage.Namespace);
            deps.Add(t.FromMessage.Namespace);
        }
        foreach(Translator t in iTranslatorOut.Translators) {
            Trace.WriteLine(Trace.kGui, "Node.PackageDependencies: " + t.ToMessage.Namespace);
            deps.Add(t.ToMessage.Namespace);
            Trace.WriteLine(Trace.kGui, "Node.PackageDependencies: " + t.FromMessage.Namespace);
            deps.Add(t.FromMessage.Namespace);
        }
        Unlock();
        return deps;
    }
    
    public override void Vector3d(Vector3d aVector) {
        if(iActive) {
            base.Vector3d(aVector);
            ReferencePlugin<Node>[] list;
            Lock();
            list = iChildrenList.ToArray();
            Unlock();
            foreach(ReferencePlugin<Node> node in list) {
                node.Object.Vector3d(aVector);
            }
            Lock();
            list = iCompositeChildrenList.ToArray();
            Unlock();
            foreach(ReferencePlugin<Node> node in list) {
                node.Object.Vector3d(aVector);
            }
        }
    }
    
//  public override void Matrix(Matrix aMatrix) {
//  }

    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            MsgSetActive setActive = aMessage as MsgSetActive;
            if(setActive != null) {
                Active = setActive.Active;
                Render(false);
                return true;
            }
        }
        return false;
    }

    public virtual void Receive(Message aMessage) {
        if(ProcessMessage(aMessage)) {      // process message as it might be an active message
            Lock();
            iTranslatorIn.SendMessage(aMessage);
            Unlock();
            return;
        }
        if(Active) {        // If the node is active pass the message on to its plugin chain
            if(base.ProcessMessage(aMessage)) {
                Lock();
                iTranslatorIn.SendMessage(aMessage);
                Unlock();
                return;
            }
        }
        ReferencePlugin<Node>[] list;
        Lock();
        list = iChildrenList.ToArray();
        Unlock();
        foreach(ReferencePlugin<Node> node in list) {
            node.Object.Receive(aMessage);
        }
        Lock();
        list = iCompositeChildrenList.ToArray();
        Unlock();
        foreach(ReferencePlugin<Node> node in list) {
            node.Object.Receive(aMessage);
        }
    }
    
    public override void SendMessage(Message aMessage) {
		//Console.WriteLine(">SendMessage");
        Lock();
		//Console.WriteLine(">>SendMessage");
        iTranslatorOut.SendMessage(aMessage);
        Unlock();
		//Console.WriteLine("<<SendMessage");
        Messenger.Instance.ApplicationMessage(aMessage);
		//Console.WriteLine("<SendMessage");
    }
    
    public void ResetRootHint() {
        iRoot = false;
    }

    public Node Search(string aName)
    {
        VisitorSearch search = new VisitorSearch(aName);
        Node node = search.Search(this);
        Assert.Check(node != null);
        return (node);
    }
				
	protected void Lock() {
		//iMutex.WaitOne();
	}
	
	protected void Unlock() {
		//iMutex.ReleaseMutex();
	}
    
    private bool iRoot = false;
    protected Mutex iMutex = new Mutex(false);
    protected bool iActive = true;
    protected ReferencePlugin<Node> iParent = new ReferencePlugin<Node>();
    protected List<ReferencePlugin<Node>> iChildrenList = new List<ReferencePlugin<Node>>();
    protected List<ReferencePlugin<Node>> iCompositeChildrenList = new List<ReferencePlugin<Node>>();
    // TODO: message translators for in and out
    protected MessageTranslator iTranslatorIn = new MessageTranslator();
    protected MessageTranslator iTranslatorOut = new MessageTranslator();
    protected Srt iLocalSrt = new Srt();
    protected Srt iWorldSrt = new Srt();    
}

} // Scenegraph
} // Gui
} // Linn
