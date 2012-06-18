using Linn;
using System.Xml;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System;

namespace Linn {
namespace Gui {
namespace Scenegraph {
    
public class NodePolygon : NodeHit
{
    public NodePolygon() : base(UniqueName("NodePolygon")) {
    }
    
    public NodePolygon(string aName) : base(aName) {
    }
    
    public override Object Clone() {
        NodePolygon p = new NodePolygon();
        Clone(p);
        return p;
    }
    
    protected void Clone(NodePolygon aNodePolygon) {
        Clone((NodeHit)aNodePolygon);
        aNodePolygon.iRenderState = iRenderState;
        aNodePolygon.iClampToTextureSize = iClampToTextureSize;
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("RenderState");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iRenderState.Load(list[0]);
        }
        
        list = aXmlNode.SelectNodes("ClampToTextureSize");
        if(list != null) {
            if(list.Count > 0) {
                Assert.Check(list.Count == 1);
                iClampToTextureSize = bool.Parse(list[0].FirstChild.Value);
            }
        }
    }
    
    public override void Link() {
        //Trace.WriteLine(Trace.kGui, "Linking NodePolygon: " + Fullname);
        base.Link();
        iRenderState.Link();
        if(iRenderState.Texture.Object != null && iClampToTextureSize == true) {
            if(iRenderState.Texture.Object.Loaded()) {
                iWidth = iRenderState.Texture.Object.Width;
                iHeight = iRenderState.Texture.Object.Height;
            }
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        iRenderState.Save(aWriter);
        
        aWriter.WriteStartElement("ClampToTextureSize");
        aWriter.WriteString(iClampToTextureSize.ToString());
        aWriter.WriteEndElement();
    }
    
    public override void Draw(IRenderer aRenderer) {
        /*if(iRenderState.Texture.Object != null) {
            if(iRenderState.Texture.Object.Width > 0 && iRenderState.Texture.Object.Height > 0) {
                if(iRenderState.Texture.Object.Width != iWidth || iRenderState.Texture.Object.Height != iHeight) {
                    iWidth = iRenderState.Texture.Object.Width;
                    iHeight = iRenderState.Texture.Object.Height;
                    ObserverUpdate();
                }
            }
        }*/
        aRenderer.DrawPolygon(this);
    }
    
    public override void Visit(Visitor aVisitor, bool aVisitNonActive) {
        aVisitor.AcceptPolygon(this);
        base.Visit(aVisitor, aVisitNonActive);
    }
    
    public override void Texture(ReferenceTexture aTexture) {
        iRenderState.Texture = aTexture;
        if(iRenderState.Texture.Object != null && iClampToTextureSize == true) {
            if(iRenderState.Texture.Object.Loaded()) {
                iWidth = iRenderState.Texture.Object.Width;
                iHeight = iRenderState.Texture.Object.Height;
                ObserverUpdate();
            }
        }
    }
    
    public RenderState CurrRenderState {
        get {
            return iRenderState;
        }
    }
    
    public bool ClampToTextureSize {
        get {
            return iClampToTextureSize;
        }
        set {
            iClampToTextureSize = value;
            if(iRenderState.Texture.Object != null && iClampToTextureSize == true) {
                iWidth = iRenderState.Texture.Object.Width;
                iHeight = iRenderState.Texture.Object.Height;
            }
            ObserverUpdate();
        }
    }
    
    public override void OnSetWidth() {
        base.OnSetWidth();
        if(iRenderState.Texture.Object != null && iClampToTextureSize) {
            iWidth = iRenderState.Texture.Object.Width;
        }
    }
    
    public override void OnSetHeight() {
        base.OnSetHeight();
        if(iRenderState.Texture.Object != null && iClampToTextureSize) {
            iHeight = iRenderState.Texture.Object.Height;
        }
    }
    
    private RenderState iRenderState = new RenderState();
    private bool iClampToTextureSize = true;
}

} // Scenegraph
} // Gui
} // Linn
