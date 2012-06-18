using Linn.Gui.Resources;
using System;
using System.Drawing;
using System.Xml;
using System.Globalization;

namespace Linn {
namespace Gui {
namespace Scenegraph {

public class NodeSlider : NodePolygon
{
    public enum EOrientation {
        EO_Horizontal = 0,
        EO_Vertical = 1
    }
    
    public NodeSlider() : base(UniqueName("NodeSlider")) {
    }
    
    public override Object Clone() {
        NodeSlider s = new NodeSlider();
        Clone(s);
        return s;
    }
    
    protected void Clone(NodeSlider aNodeSlider) {
        Clone((NodePolygon)aNodeSlider);
        aNodeSlider.iPosition = iPosition;
        aNodeSlider.iSliderNode = (NodePolygon)iSliderNode.Clone();
        aNodeSlider.iIndicatorNode = new ReferencePlugin<NodeHit>(iIndicatorNode.Object);
        aNodeSlider.iTexture = iTexture;
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        XmlNodeList list;
        list = aXmlNode.SelectNodes("SliderTexture");
        if(list != null) {
            if(list.Count > 0) {
                Assert.Check(list.Count == 1);
                iTexture = new ReferenceTexture(list[0].InnerText);
            }
        }
        list = aXmlNode.SelectNodes("Value");
        if(list != null) {
            if(list.Count > 0) {
                Assert.Check(list.Count == 1);
                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
                iPosition = float.Parse(list[0].InnerText, nfi);
            }
        }
        list = aXmlNode.SelectNodes("IndicatorNode");
        if(list != null) {
            if(list.Count > 0) {
                Assert.Check(list.Count == 1);
                iIndicatorNode = new ReferencePlugin<NodeHit>(list[0].FirstChild.Value);
            }
        }
        list = aXmlNode.SelectNodes("Orientation");
        if(list != null) {
            if(list.Count > 0) {
                Assert.Check(list.Count == 1);
                iOrientation = (EOrientation)Enum.Parse(typeof(EOrientation), list[0].FirstChild.Value, true);
            }
        }
    }
    
    public override void Link() {
        base.Link();
        iTexture.Link();
        iIndicatorNode.Link();
        iSliderNode = (NodePolygon)PluginFactory.Instance.Create("NodePolygon");
        iSliderNode.Namespace = Namespace;
        iSliderNode.Name = Name + ".Slider";
        iSliderNode.Active = Active;
        iSliderNode.AllowHits = false;
        iSliderNode.ClampToTextureSize = false;
        iSliderNode.Texture(iTexture);
        iSliderNode.LocalTranslation = new Vector3d(0,0,1);
        iSliderNode.Width = Width;
        iSliderNode.Height = Height;
        AddCompositeChild(iSliderNode);
        iSliderNode.Update(true, true);
        UpdateSlider();
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        if(iTexture.Object != null) {
            aWriter.WriteStartElement("SliderTexture");
            aWriter.WriteString(iTexture.Object.Filename);
            aWriter.WriteEndElement();
        }
        aWriter.WriteStartElement("Value");
        NumberFormatInfo nfi = new NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
        aWriter.WriteString(iPosition.ToString(nfi));
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Orientation");
        aWriter.WriteString(iOrientation.ToString());
        aWriter.WriteEndElement();
        if(iIndicatorNode.Object != null) {
            aWriter.WriteStartElement("IndicatorNode");
            aWriter.WriteString(iIndicatorNode.Object.Fullname);
            aWriter.WriteEndElement();
        }
    }
    
    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            if(aMessage as MsgInputRotate != null) {
                float speed = ((MsgInputRotate)aMessage).Speed;
                float inc = speed * 0.001f;
                iPosition += inc;
                if(iPosition < 0.0f) {
                    iPosition = 0.0f;
                } else if(iPosition > 1.0f) {
                    iPosition = 1.0f;
                }
                Trace.WriteLine(Trace.kGui, aMessage + "(" + speed + "): " + inc);
                UpdateSlider();
                return true;
            }
            if(aMessage as MsgInputClick != null) {
                MsgInputClick msg = aMessage as MsgInputClick;
                CheckForUpdate(msg.Position);
                return true;
            }
            if(aMessage as MsgSetPosition != null) {
                MsgSetPosition msg = aMessage as MsgSetPosition;
                Position = msg.Position;
                return true;
            }
        }
        if(base.ProcessMessage(aMessage)) {
            return true;
        }
        return false;
    }
    
    public override void Hit(Vector3d aVector) {
        CheckForUpdate(aVector);
        base.Hit(aVector);
        if(iIndicatorNode.Object != null) {
            iIndicatorNode.Object.Hit(aVector);
        }
    }
    
    public override void Motion(Vector3d aVector) {
        UpdateSlider(aVector);
        base.Motion(aVector);
        if(iIndicatorNode.Object != null) {
            iIndicatorNode.Object.Motion(aVector);
        }
    }
    
    public override void UnHit() {
        base.UnHit();
        if(iIndicatorNode.Object != null) {
            iIndicatorNode.Object.UnHit();
        }
    }
    
    private void CheckForUpdate(Vector3d aVector) {
        bool inside = false;
        if(iIndicatorNode.Object != null) {
            switch(iOrientation) {
            case(EOrientation.EO_Horizontal):
                if(iIndicatorNode.Object.WorldTranslation.X < aVector.X && aVector.X < iIndicatorNode.Object.WorldTranslation.X + iIndicatorNode.Object.Width) {
                    inside = true;
                }
                break;
            case(EOrientation.EO_Vertical):
                if(iIndicatorNode.Object.WorldTranslation.Y < aVector.Y && aVector.Y < iIndicatorNode.Object.WorldTranslation.Y + iIndicatorNode.Object.Height) {
                    inside = true;
                }
                break;
            }
        }
        if(!inside) {
            UpdateSlider(aVector);
        }
    }
    
    private void UpdateSlider(Vector3d aVector) {
        float pos = 0;
        switch(iOrientation) {
        case(EOrientation.EO_Horizontal):
            if(iIndicatorNode.Object != null) {
                pos = ((aVector.X - WorldTranslation.X) - (iIndicatorNode.Object.Width * 0.5f)) / (Width - iIndicatorNode.Object.Width);
            } else {
                pos = (aVector.X - WorldTranslation.X) / Width;
            }
            Trace.WriteLine(Trace.kGui, "width=" + Width + ", x=" + WorldTranslation.X + ", hit x=" + aVector.X + "-> %=" + pos*100.0f);
            break;
        case(EOrientation.EO_Vertical):
            if(iIndicatorNode.Object != null) {
                pos = ((aVector.Y - WorldTranslation.Y) - (iIndicatorNode.Object.Height * 0.5f)) / (Height - iIndicatorNode.Object.Height);
            } else {
                pos = (aVector.Y - WorldTranslation.Y) / Height;
            }
            Trace.WriteLine(Trace.kGui, "height=" + Height + ", y=" + WorldTranslation.Y + ", hit y=" + aVector.Y + "-> %=" + pos*100.0f);
            break;
        default:
            Assert.Check(true);
            break;
        }
        if(pos < 0) {
            pos = 0;
        } else if(pos > 1) {
            pos = 1;
        }
        Position = pos;
    }
    
    private void UpdateSlider() {
        switch(iOrientation) {
        case(EOrientation.EO_Horizontal):
            if(iIndicatorNode.Object != null) {
                iSliderNode.Width = (Width - iIndicatorNode.Object.Width) * iPosition + (iIndicatorNode.Object.Width * 0.5f);
                Vector3d v = iIndicatorNode.Object.LocalTranslation;
                float x = (Width - iIndicatorNode.Object.Width) * iPosition;
                float y = (0.5f * Height) - (0.5f * iIndicatorNode.Object.Height);
                iIndicatorNode.Object.LocalTranslation = new Vector3d(x, y, v.Z);
            } else {
                iSliderNode.Width = Width * iPosition;
            }
            break;
        case(EOrientation.EO_Vertical):
            if(iIndicatorNode.Object != null) {
                iSliderNode.Height = (Height - iIndicatorNode.Object.Height) * iPosition + (iIndicatorNode.Object.Height * 0.5f);
                Vector3d v = iIndicatorNode.Object.LocalTranslation;
                float x = (0.5f * Width) - (0.5f * iIndicatorNode.Object.Width);
                float y = (Height - iIndicatorNode.Object.Height) * iPosition;
                iIndicatorNode.Object.LocalTranslation = new Vector3d(x, y, v.Z);
            } else {
                iSliderNode.Height = Height * iPosition;
            }
            break;
        default:
            Assert.Check(true);
            break;
        }
        Render(false);
    }
    
    public override void OnSetWidth() {
        base.OnSetWidth();
        iSliderNode.Width = iWidth;
        UpdateSlider();
    }
    
    public override void OnSetHeight() {
        base.OnSetHeight();
        iSliderNode.Height = iHeight;
        UpdateSlider();
    }
    
    public float Position {
        get {
            return iPosition;
        }
        set {
            if(value != iPosition) {
                if(value < 0) {
                    System.Console.WriteLine("WARNING: NodeSlider.Position: value=" + value);
                    value = 0;
                } else if(value > 1) {
                    System.Console.WriteLine("WARNING: NodeSlider.Position: value=" + value);
                    value = 1;
                }
                Assert.Check(value >= 0 && value <= 1);
                float oldPosition = iPosition;
                iPosition = value;
                UpdateSlider();
                SendMessage(new MsgPositionChanged(this, oldPosition, iPosition));
                ObserverUpdate();
            }
        }
    }
    
    public EOrientation Orientation {
        get {
            return iOrientation;
        }
        set {
            if(value != iOrientation) {
                iOrientation = value;
                iSliderNode.Width = iWidth;
                iSliderNode.Height = iHeight;
                UpdateSlider();
                ObserverUpdate();
            }
        }
    }
    
    public ReferenceTexture SliderTexture {
        get {
            return iTexture;
        }
    }
    
    public NodeHit IndicatorNode {
        get {
            return iIndicatorNode.Object;
        }
        set {
            iIndicatorNode = new ReferencePlugin<NodeHit>(value);
            UpdateSlider();
        }
    }
    
    private EOrientation iOrientation = EOrientation.EO_Horizontal;
    private float iPosition = 0;            // 0-100 range
    private NodePolygon iSliderNode = null;
    private ReferenceTexture iTexture = new ReferenceTexture();
    private ReferencePlugin<NodeHit> iIndicatorNode = new ReferencePlugin<NodeHit>();
}

} // Scenegraph
} // Gui
} // Linn

