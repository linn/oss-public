using Linn;
using System.Xml;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System;
using System.Drawing;

namespace Linn {
namespace Gui {
namespace Scenegraph {
      
public class NodeText : NodeFont
{
    public NodeText() : base(UniqueName("NodeText")) {
    }
    
    public NodeText(string aName) : base(aName) {
    }
    
    public override Object Clone() {
        NodeText t = new NodeText();
        Clone(t);
        return t;
    }
    
    protected void Clone(NodeText aNodeText) {
        Clone((NodeFont)aNodeText);
        aNodeText.iText = iText;
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        XmlNodeList list = aXmlNode.SelectNodes("Text");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iText = list[0].InnerText;
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("Text");
        aWriter.WriteString(iText);
        aWriter.WriteEndElement();
    }
    
    public override void Draw(IRenderer aRenderer) {
        aRenderer.DrawText(this);
    }
    
    public override void Visit(Visitor aVisitor, bool aVisitNonActive) {
        aVisitor.AcceptText(this);
        base.Visit(aVisitor, aVisitNonActive);
    }
    
    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            MsgSetText setText = aMessage as MsgSetText;
            if(setText != null) {
                Text = setText.Text;
                Render(false);
                return true;
            }
        }
        if(base.ProcessMessage(aMessage)) {
            return true;
        }
        return false;
    }
    
    public string Text {
        get {
            return iText;
        }
        set {
            if(value != iText) {
                iText = value;
                ObserverUpdate();
            }
        }
    }
    
    private string iText = "";
}

} // Scenegraph
} // Gui
} // Linn
