using Linn.Gui.Scenegraph;
using System.Xml;
using Linn.Gui.Resources;
using System;
using System.Globalization;
using System.Drawing;

namespace Linn {
namespace Gui {
namespace Scenegraph {
    
public class NodeHit : Node
{
    public NodeHit() : base(UniqueName("NodeHit")) {
    }
    
    public NodeHit(string aName) : base(aName) {
    }
    
    public override Object Clone() {
        NodeHit h = new NodeHit();
        Clone(h);
        return h;
    }
    
    protected void Clone(NodeHit aNodeHit) {
        Clone((Node)aNodeHit);
        aNodeHit.iWidth = iWidth;
        aNodeHit.iHeight = iHeight;
        aNodeHit.iAllowHits = iAllowHits;
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        NumberFormatInfo nfi = new NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Width");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iWidth = float.Parse(list[0].FirstChild.Value, nfi);
        }
        
        list = aXmlNode.SelectNodes("Height");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iHeight = float.Parse(list[0].FirstChild.Value, nfi);
        }
        
        list = aXmlNode.SelectNodes("AllowHits");
        if(list != null) {
            if(list.Count > 0) {
                Assert.Check(list.Count == 1);
                iAllowHits = bool.Parse(list[0].FirstChild.Value);
            }
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        NumberFormatInfo nfi = new NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
                    
        aWriter.WriteStartElement("Width");
        aWriter.WriteString(iWidth.ToString(nfi));
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Height");
        aWriter.WriteString(iHeight.ToString(nfi));
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("AllowHits");
        aWriter.WriteString(iAllowHits.ToString());
        aWriter.WriteEndElement();
    }
    
    public override void Visit(Visitor aVisitor, bool aVisitNonActive) {
        aVisitor.AcceptHit(this);
        base.Visit(aVisitor, aVisitNonActive);
    }
    
    public float Width {
        get {
            return iWidth;
        }
        set {
            iWidth = value;
            OnSetWidth();
            ObserverUpdate();
        }
    }
    
    public virtual void OnSetWidth() {
    }
    
    public float Height {
        get {
            return iHeight;
        }
        set {
            iHeight = value;
            OnSetHeight();
            ObserverUpdate();
        }
    }
    
    public virtual void OnSetHeight() {
    }
    
    public bool AllowHits {
        get {
            return iAllowHits;
        }
        set {
            iAllowHits = value;
            ObserverUpdate();
        }
    }
    
    public override void Render() {
        Render(true);
    }
    
    public override void Render(bool aForce) {
        Rectangle r = new Rectangle((int)WorldSrt.Translation.X, (int)WorldSrt.Translation.Y, (int)Width, (int)Height);
        Renderer.Instance.Render(r, aForce);
    }
    
    public bool Inside(Vector3d aPosition) {
        Trace.WriteLine(Trace.kGui, "hitpos=" + aPosition + " w=" + iWidth + ", h=" + iHeight);
        if(aPosition.X >= 0 && aPosition.X < iWidth) {
            if(aPosition.Y >= 0 && aPosition.Y < iHeight) {
                //MsgHit msg = new MsgHit(Fullname, aPosition);
                //SendMessage(msg);
                return true;
            }
        }
        return false;
    }
    
    protected float iWidth = 0.0f;
    protected float iHeight = 0.0f;
    protected bool iAllowHits = true;
}
    
} // Scenegraph
} // Gui
} // Linn
