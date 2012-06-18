using Linn.Gui;
using System.Xml;
using System.Collections.Generic;

namespace Linn {
namespace Gui {
namespace Resources {
    
public sealed class Translator : ISerialiseObject
{
    public Translator() {
    }
    
    public Translator(Message aFromMessage, Message aToMessage) {
        iFromMessage = aFromMessage;
        iToMessage = aToMessage;
    }
    
    public void Load(XmlNode aXmlNode) {
        XmlNodeList list;
        list = aXmlNode.SelectNodes("FromMessage");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iFromMessage = MessageFactory.Instance.Create(list[0]);
        }
        
        list = aXmlNode.SelectNodes("ToMessage");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iToMessage = MessageFactory.Instance.Create(list[0]);
        }
    }
    
    public void Link() {
        //Trace.WriteLine(Trace.kGui, "Linking Translator");
        //Trace.WriteLine(Trace.kGui, "iFromMessage=" + iFromMessage + ", iToMessage=" + iToMessage);
        iFromMessage.Link();
        iToMessage.Link();
    }
    
    public void Save(XmlTextWriter aWriter) {
        aWriter.WriteStartElement("FromMessage");
        string type = iFromMessage.GetType().ToString();
        aWriter.WriteAttributeString("class", type.Substring(type.LastIndexOf('.') + 1));
        iFromMessage.Save(aWriter);
        aWriter.WriteEndElement();

        aWriter.WriteStartElement("ToMessage");
        type = iToMessage.GetType().ToString();
        aWriter.WriteAttributeString("class", type.Substring(type.LastIndexOf('.') + 1));
        iToMessage.Save(aWriter);
        aWriter.WriteEndElement();
    }
    
    public Message ToMessage {
        get {
            return iToMessage;
        }
        set {
            iToMessage = value;
        }
    }
    
    public Message FromMessage {
        get {
            return iFromMessage;
        }
        set {
            iFromMessage = value;
        }
    }
    
    public Message Translate(Message aMessage) {
        if(aMessage.IsEqualTo(iFromMessage)) {
            return iToMessage;
        }
        return null;
    }
    
    private Message iToMessage = null;
    private Message iFromMessage = null;
}

    
public sealed class MessageTranslator : ISerialiseObject
{
    public void Load(XmlNode aXmlNode) {
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Translator");
        if(list != null) {
            foreach(XmlNode node in list) {
                Translator translator = new Translator();
                translator.Load(node);
                iTranslatorList.Add(translator);
            }
        }
    }
    
    public void Link() {
        //Trace.WriteLine(Trace.kGui, "Linking MessageTranslator table");
        foreach(Translator t in iTranslatorList) {
            t.Link();
        }
    }
    
    public void Save(XmlTextWriter aWriter) {
        foreach(Translator translator in iTranslatorList) {
            aWriter.WriteStartElement("Translator");
            translator.Save(aWriter);
            aWriter.WriteEndElement();
        }
    }
    
    public void AddTranslator(Translator aTranslator) {
        iTranslatorList.Add(aTranslator);
    }
    
    public void RemoveTranslator(Translator aTranslator) {
        iTranslatorList.Remove(aTranslator);
    }
    
    public List<Translator> Translators {
        get {
            return iTranslatorList;
        }
    }
    
    public bool SendMessage(Message aMessage) {
        int numTranslated = 0;
        foreach(Translator translator in iTranslatorList) {
            Message message = translator.Translate(aMessage);
            if(message != null) {
                Trace.WriteLine(Trace.kGui, "MessageTranslator.SendMessage: aMessage(" + aMessage.Fullname + ")=" + aMessage + " to message(" + message.Fullname + ")=" + message);
                Messenger.Instance.PresentationMessage(message);
                numTranslated++;
            }
        }
        return(numTranslated > 0);
    }
    
    private List<Translator> iTranslatorList = new List<Translator>();
}
    
} // Resources
} // Gui
} // Linn
