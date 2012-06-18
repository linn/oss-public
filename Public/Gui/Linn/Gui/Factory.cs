using System.Xml;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using Linn;

namespace Linn {
namespace Gui {
    
public sealed class UnknownPlugin : System.Exception
{
}

sealed class UnknownMessage : System.Exception
{
}
    
public class PluginFactory
{
    public PluginFactory() {
        Trace.WriteLine(Trace.kGui, ">PluginFactory");
        if(iInstance != null) {
            throw new SingletonAlreadyExists();
        }
        iInstance = this;
    }
    
    public static PluginFactory Instance {
        get {
            if(iInstance == null) {
                throw new SingletonDoesntExist();
            }
            return iInstance;
        }
    }
    
    public virtual Plugin Create(XmlNode aXmlNode) {
        string classType = "";
        foreach(XmlAttribute attrib in aXmlNode.Attributes) {
            if(attrib.Name == "class") {
                classType = attrib.Value;
            }
        }
        Assert.Check(classType != "");
        Plugin newPlugin = Create(classType);
        newPlugin.Load(aXmlNode);
        return newPlugin;
    }
    
    public virtual Plugin Create(string aClassType) {
        Plugin newPlugin = null;
        if(aClassType == "Plugin") {
            newPlugin = new Plugin();
        } else if(aClassType == "Node") {
            newPlugin = new Node();
        } else if(aClassType == "NodeList") {
            newPlugin = new NodeList();
        } else if(aClassType == "NodeHit") {
            newPlugin = new NodeHit();
        } else if(aClassType == "NodeInput") {
            newPlugin = new NodeInput();
        } else if(aClassType == "NodeList") {
            newPlugin = new NodeList();
        } else if(aClassType == "NodeSlider") {
            newPlugin = new NodeSlider();
        } else if(aClassType == "NodePolygon") {
            newPlugin = new NodePolygon();
        } else if(aClassType == "NodeText") {
            newPlugin = new NodeText();
        } else if(aClassType == "NodeVolume") {
            newPlugin = new NodeVolume();
        } else if(aClassType == "Monostable") {
            newPlugin = new Monostable();
        } else if(aClassType == "Bistable") {
            newPlugin = new Bistable();
        } else if(aClassType == "Counter") {
            newPlugin = new Counter();
        } else if(aClassType == "TextureArray") {
            newPlugin = new TextureArray();
        } else if(aClassType == "TextureArrayFixed") {
            newPlugin = new TextureArrayFixed();
        }
        if(newPlugin != null) {
            return newPlugin;
        }
        throw new UnknownPlugin();
    }
    
    private static PluginFactory iInstance = null;
}

public sealed class MessageFactory
{
    public MessageFactory() {
        if(iInstance != null) {
            throw new SingletonAlreadyExists();
        }
        iInstance = this;
    }
    
    public static MessageFactory Instance {
        get {
            if(iInstance == null) {
                throw new SingletonDoesntExist();
            }
            return iInstance;
        }
    }
    
    public Message Create(XmlNode aXmlNode) {
        string classType = "";
        foreach(XmlAttribute attrib in aXmlNode.Attributes) {
            if(attrib.Name == "class") {
                classType = attrib.Value;
            }
        }
        Assert.Check(classType != "");
        Message newMessage = Create(classType);
        newMessage.Load(aXmlNode);
        return newMessage;
    }
    
    public Message Create(string aClassType) {
        Message newMessage = null;
        if(aClassType == "MsgSetActive") {
            newMessage = new MsgSetActive();
        } else if(aClassType == "MsgHit") {
            newMessage = new MsgHit();
        } else if(aClassType == "MsgSetText") {
            newMessage = new MsgSetText();
        } else if(aClassType == "MsgStateChanged") {
            newMessage = new MsgStateChanged();
        } else if(aClassType == "MsgSetState") {
            newMessage = new MsgSetState();
        } else if(aClassType == "MsgToggleState") {
            newMessage = new MsgToggleState();
        } else if(aClassType == "MsgCountEnd") {
            newMessage = new MsgCountEnd();
        } else if(aClassType == "MsgInputRotate") {
            newMessage = new MsgInputRotate();
        } else if(aClassType == "MsgInputAxisX") {
            newMessage = new MsgInputAxisX();
        } else if(aClassType == "MsgInputAxisY") {
            newMessage = new MsgInputAxisY();
        } else if(aClassType == "MsgInputAxisZ") {
            newMessage = new MsgInputAxisZ();
        } else if(aClassType == "MsgNext") {
            newMessage = new MsgNext();
        } else if(aClassType == "MsgPrevious") {
            newMessage = new MsgPrevious();
        }
        if(newMessage != null) {
            return newMessage;
        }
        Trace.WriteLine(Trace.kGui, "tying to instanciate " + aClassType + " message class");
        throw new UnknownMessage();
    }
    
    private static MessageFactory iInstance = null;
}
    
} // Gui
} // Linn
