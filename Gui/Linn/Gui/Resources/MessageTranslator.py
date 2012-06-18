import Linn.Gui.Serialize
import Linn.Gui.Messenger
import Message

class Translator(Linn.Gui.Serialize.ISerialize):
    def __init__(self, aFromMsg=None, aToMsg=None):
        self.iFromMsg = aFromMsg
        self.iToMsg = aToMsg
        
    def Load(self, aStream):
        fromMsg = aStream.LoadNode('FromMessage')
        if len(fromMsg) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<FromMessage>', len(fromMsg), 1)
        
        classType = fromMsg[0].attributes[(None,'class')].value
        aStream.PushContextNode(fromMsg[0])
        self.iFromMsg = Message.CreateMessage(classType, aStream)
        aStream.PopContextNode()
        
        toMsg = aStream.LoadNode('ToMessage')
        if len(toMsg) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<ToMessage>', len(toMsg), 1)
        
        classType = toMsg[0].attributes[(None,'class')].value
        aStream.PushContextNode(toMsg[0])
        self.iToMsg = Message.CreateMessage(classType, aStream)
        aStream.PopContextNode()
    
    def Save(self, aStream):
        fromMsg = aStream.CreateElement('FromMessage')
        classType = aStream.CreateAttribute('class', None)
        fromMsg.setAttributeNodeNS(classType)
        aStream.PushContextNode(fromMsg)
        
        self.iFromMsg.Save(aStream)
        
        aStream.PopContextNode()
        aStream.SaveNode(fromMsg)
        
        toMsg = aStream.CreateElement('ToMessage')
        classType = aStream.CreateAttribute('class', None)
        toMsg.setAttributeNodeNS(classType)
        aStream.PushContextNode(toMsg)
        
        self.iToMsg.Save(aStream)
        
        aStream.PopContextNode()
        aStream.SaveNode(toMsg)
    
    def Link(self):
        pass
        
    def FromMsg(self):
        return self.iFromMsg
    
    def SetFromMsg(self, aFromMsg):
        self.iFromMsg = aFromMsg
    
    def ToMsg(self):
        return self.iToMsg
    
    def SetToMsg(self, aToMsg):
        self.iToMsg = aToMsg
        
    def Translate(self, aMessage):
        if aMessage == self.iFromMsg:
            #print 'Translating:\n', self.iFromMsg, 'to\n', self.iToMsg
            return self.iToMsg
        else:
            return None


class MessageTranslator(Linn.Gui.Serialize.ISerialize):
    def __init__(self):
        self.iTranslators = []
        
    def Load(self, aStream):
        translationList = aStream.LoadNode('Translator')
        for translation in translationList:
            translator = Translator()
            aStream.PushContextNode(translation)
            translator.Load(aStream)
            aStream.PopContextNode()
            self.iTranslators.append(translator)
    
    def Save(self, aStream):      
        for trans in self.iTranslators:
            translator = aStream.CreateElement('Translator')
            aStream.PushContextNode(translator)
            trans.Save(aStream)
            aStream.PopContextNode()
            aStream.SaveNode(translator)
    
    def Link(self):
        pass
        
    def AddTranslator(self, aFromMsg, aToMsg):
        self.iTranslators.append(Translator(aFromMsg, aToMsg))
        
    def RemoveTranslator(self, aFromMsg, aToMsg):
        for trans in self.iTranslators:
            if trans.FromMsg() == aFromMsg and trans.ToMsg() == aToMsg:
                self.iTranslators.remove(trans)
                
    def RemoveTranslatorAtIndex(self, aIndex):
        self.iTranslators.remove(self.iTranslators[aIndex])
                
    def TranslatorList(self):
        return self.iTranslators
        
    def SendMessage(self, aMsg):
        translated = False
        for trans in self.iTranslators:
            msg = trans.Translate(aMsg)
            if msg:
                Linn.Gui.Messenger.GetMessenger().PresentationMessage(msg)
                translated = True
        return translated
        
        
        