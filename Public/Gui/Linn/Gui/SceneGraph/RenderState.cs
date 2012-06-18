using Linn.Gui.Resources;
using System.Xml.XPath;
using System.Xml;

namespace Linn {
namespace Gui {

public class RenderState : ISerialiseObject
{
    public void Load(XmlNode aXmlNode) {
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Texture");
        if(list != null) {
            if(list.Count > 0) {
                Assert.Check(list.Count == 1);
                iTexture = new ReferenceTexture(list[0].FirstChild.Value);
            }
        }
    }
    
    public void Link() {
        iTexture.Link();
    }
    
    public void Save(XmlTextWriter aWriter) {
        aWriter.WriteStartElement("RenderState");
        if(iTexture.Object != null) {
            aWriter.WriteStartElement("Texture");
            aWriter.WriteString(iTexture.Object.Filename);
            aWriter.WriteEndElement();
        }
        aWriter.WriteEndElement();
    }
    
    public ReferenceTexture Texture {
        get {
            return iTexture;
        }
        set {
            iTexture = value;
        }
    }
    
    private ReferenceTexture iTexture = new ReferenceTexture();
}
    
} // Gui
} // Linn
