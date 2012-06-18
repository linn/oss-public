using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Collections.Generic;
using System.Xml;

namespace Linn {
namespace Gui {
    
public class TextureArrayFixed : Plugin
{
    public TextureArrayFixed() : base("TextureArrayFixed") {
    }
    
    public TextureArrayFixed(string aName) : base(aName) {
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Texture");
        if(list != null) {
            foreach(XmlNode node in list) {
                ReferenceTexture texture = new ReferenceTexture(node.FirstChild.Value);
                iTextureList.Add(texture);
            }
        }
    }
    
    public override void Link() {
        base.Link();
        foreach(ReferenceTexture texture in iTextureList) {
            texture.Link();
        }
        // NOTE: should texture array class store index state???
        if(NextPlugin != null) {
            if(iTextureList.Count > 0) {
                NextPlugin.Texture(iTextureList[iTextureIndex]);
            } else {
                NextPlugin.Texture(new ReferenceTexture());
            }
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        foreach(ReferenceTexture t in iTextureList) {
            aWriter.WriteStartElement("Texture");
            aWriter.WriteString(t.Object.Filename);
            aWriter.WriteEndElement();
        }
    }
    
    public override void Vector3d(Vector3d aVector) {
        iTextureIndex = (int)aVector.X;
        if(iTextureIndex < 0){
            iTextureIndex = 0;
        } else if(iTextureIndex > iTextureList.Count - 1) {
            iTextureIndex = iTextureList.Count - 1;
        }
        if(NextPlugin != null) {
            if(iTextureList.Count > 0) {
                NextPlugin.Texture(iTextureList[iTextureIndex]);
            } else {
                NextPlugin.Texture(new ReferenceTexture());
            }
        }
    }
    
    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            MsgSetTexture setTexture = aMessage as MsgSetTexture;
            if(setTexture != null) {
                this[setTexture.Index] = setTexture.Texture;
            }
            return true;
        }
        if(base.ProcessMessage(aMessage)) {
            return true;
        }
        return false;
    }
    
    public ITexture this[int aIndex] {
        get {
            if(iTextureList.Count > 0) {
                return ((ReferenceTexture)iTextureList[aIndex]).Object;
            } else {
                return null;
            }
        }
        set {
            if(aIndex >= 0 && aIndex < iTextureList.Count)
            {
                ReferenceTexture texture = new ReferenceTexture(value);
                iTextureList[aIndex] = texture;
                if(aIndex == iTextureIndex) {
                    if(iTextureList.Count > 0) {
                        NextPlugin.Texture(iTextureList[iTextureIndex]);
                    } else {
                        NextPlugin.Texture(new ReferenceTexture());
                    }
                }
                ObserverUpdate();
            }
        }
    }
    
    public List<ReferenceTexture> Textures {
        get {
            return iTextureList;
        }
    }
    
    public void AddTexture(ITexture aTexture) {
        iTextureList.Add(new ReferenceTexture(aTexture));
        if(iTextureList.Count - 1 == iTextureIndex) {
            if(NextPlugin != null) {
                if(iTextureList.Count > 0) {
                    NextPlugin.Texture(iTextureList[iTextureIndex]);
                } else {
                    NextPlugin.Texture(new ReferenceTexture());
                }
            }
        }
        ObserverUpdate();
    }
    
    public void RemoveTexture(ITexture aTexture) {
        ReferenceTexture obj = null;
        foreach(ReferenceTexture t in iTextureList) {
            if(t.Object == aTexture) {
                obj = t;
                break;
            }
        }
        bool updateTexture = false;
        if(aTexture == this[iTextureIndex]) {
            updateTexture = true;
        }
        iTextureList.Remove(obj);
        if(iTextureList.Count < iTextureIndex) {
            iTextureIndex = iTextureList.Count - 1;
            if(NextPlugin != null) {
                if(iTextureList.Count > 0) {
                    NextPlugin.Texture(iTextureList[iTextureIndex]);
                } else {
                    NextPlugin.Texture(new ReferenceTexture());
                }
            }
        } else if(updateTexture) {
            if(NextPlugin != null) {
                if(iTextureList.Count > 0) {
                    NextPlugin.Texture(iTextureList[iTextureIndex]);
                } else {
                    NextPlugin.Texture(new ReferenceTexture());
                }
            }
        }
        ObserverUpdate();
    }
    
    public void Clear() {
        iTextureList.Clear();
        iTextureIndex = 0;
        NextPlugin.Texture(new ReferenceTexture());
        ObserverUpdate();
    }
    
    protected List<ReferenceTexture> iTextureList = new List<ReferenceTexture>();
    protected int iTextureIndex = 0;
}

public class TextureArray : TextureArrayFixed
{
    public TextureArray() : base("TextureArray") {
    }
    
    public TextureArray(string aName) : base(aName) {
    }
    
    public void SetNumTextures(int aNumTextures) {
        if(aNumTextures < iTextureList.Count) {
            iTextureList.RemoveRange(aNumTextures, iTextureList.Count - 1);
        } else {
            int numToAdd = aNumTextures - iTextureList.Count;
            for(int i = 0; i < numToAdd; ++i) {
                iTextureList.Add(new ReferenceTexture());
            }
        }
        if(iTextureIndex > aNumTextures) {
            iTextureIndex = iTextureList.Count - 1;
            if(iTextureList.Count > 0) {
                NextPlugin.Texture(iTextureList[iTextureIndex]);
            } else {
                NextPlugin.Texture(new ReferenceTexture());
            }
            ObserverUpdate();
        }
    }
}
    
} // Gui
} // Linn
