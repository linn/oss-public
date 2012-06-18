using System.Xml;
using System.Collections.Generic;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System;

namespace Linn {
namespace Gui {
namespace Scenegraph {
    
public class NodeInput : NodePolygon
{   
    public NodeInput() : base(UniqueName("NodeInput")) {
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("MsgReceiver");
        if(list != null) {
            if(list.Count > 0) {
                Assert.Check(list.Count == 1);
                iMsgReceiver = new ReferencePlugin<Node>(list[0].InnerText);
            }
        }
    }
    
    public override void Link() {
        base.Link();
        iMsgReceiver.Link();
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        if(iMsgReceiver.Object != null) {
            aWriter.WriteStartElement("MsgReceiver");
            aWriter.WriteString(iMsgReceiver.Object.Fullname);
            aWriter.WriteEndElement();
        }
    }
    
    public override void Click(Vector3d aVector) {
        if(!iCommandSent && iMsgReceiver.Object != null) {
            Trace.WriteLine(Trace.kGui, "NodeInput: cmd=SELECT");
            Messenger.Instance.PresentationMessage(new MsgInputClick(iMsgReceiver.Object, aVector));
            SendMessage(new MsgInputClick(this, aVector));
        }
    }
    
    public override void DoubleClick(Vector3d aVector) {
        if(!iCommandSent && iMsgReceiver.Object != null) {
            Messenger.Instance.PresentationMessage(new MsgInputDoubleClick(iMsgReceiver.Object, aVector));
            SendMessage(new MsgInputDoubleClick(this, aVector));
        }
    }
    
    public override void Wheel(float aDirection) {
        Messenger.Instance.PresentationMessage(new MsgInputAxisZ(iMsgReceiver.Object, aDirection));
        SendMessage(new MsgInputAxisZ(this, aDirection));
    }
    
    public override void Hit(Vector3d aVector) {
        iCommandSent = false;
        base.Hit(aVector);
    }
    
    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            if(aMessage as MsgInputRotate != null) {
                iCommandSent = true;
                if(iMsgReceiver.Object != null) {
                    MsgInputRotate m = aMessage as MsgInputRotate;
                    Messenger.Instance.PresentationMessage(new MsgInputRotate(iMsgReceiver.Object, m.Speed));
                    return true;
                }
            }
        }
        if(base.ProcessMessage(aMessage)) {
            return true;
        }
        return false;
    }
    
    public Node MsgReceiver {
        get {
            return iMsgReceiver.Object;
        }
        set {
            iMsgReceiver = new ReferencePlugin<Node>(value);
        }
    }

    private bool iCommandSent = false;
    private ReferencePlugin<Node> iMsgReceiver = new ReferencePlugin<Node>();
}

} // Scenegraph
} // Gui
} // Linn
