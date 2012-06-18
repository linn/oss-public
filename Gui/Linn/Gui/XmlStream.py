import xml.dom.ext
import xml.dom.minidom
from Ft.Xml import XPath
import Ft.Xml.Domlette

kNone  = 0x00
kRead  = 0x01
kWrite = 0x02

class InvalidFileMode:
    pass

class XmlStream:
    def __init__(self):
        self.iDocument = None
        self.iContext = None
        self.iMode = kNone
        self.iFilename = None
        self.iRootNode = None
        self.iContextNode = []
        
    def Open(self, aFilename, aMode):
        """Open the specified file in the appropriate mode."""
        if aMode == kWrite:
            self.iDocument = xml.dom.minidom.Document()
        elif aMode == kRead:
            xmlFile = open(aFilename, 'r')
            xmlString = xmlFile.read()
            xmlFile.close()
            self.iDocument = Ft.Xml.Domlette.NonvalidatingReader.parseString(xmlString, uri='uri:layoutXmlFile')
            self.iContext = XPath.Context.Context(self.iDocument)
        else:
            raise InvalidFileMode
        self.iMode = aMode
        self.iFilename = aFilename
        self.iContextNode.append(self.iDocument)
        
    def Close(self):
        """Close/write the specified file."""
        if self.iMode == kWrite:
            xml.dom.ext.PrettyPrint(self.iDocument, open(self.iFilename,'w'))
            
    def Filename(self):
        return self.iFilename
            
    def Document(self):
        return self.iDocument
    
    def Context(self):
        return self.iContext
    
    def RootNode(self):
        return self.iRootNode
    
    def SetRootNode(self, aRootNode):
        self.iRootNode = aRootNode
    
    def LoadNode(self, aNode):
        return XPath.Evaluate(aNode, contextNode=self.iContextNode[-1], context=self.iContext)
    
    def SetContextAttribute(self, aAttribute, aValue):
        self.iContextNode[-1].attributes[(None,aAttribute)].value = aValue
        
    def ContextAttribute(self, aAttribute):
        return self.iContextNode[-1].attributes[(None,aAttribute)].value
        
    def SaveNode(self, aNode):
        self.iContextNode[-1].appendChild(aNode)
    
    def PushContextNode(self, aContextNode):
        self.iContextNode.append(aContextNode)
        
    def PopContextNode(self):
        return self.iContextNode.pop()
    
    def CreateElement(self, aElement):
        return self.iDocument.createElement(aElement)
    
    def CreateAttribute(self, aAttribute, aValue):
        attrib = self.iDocument.createAttributeNS(None, aAttribute)
        attrib.value = aValue
        return attrib
    
    def CreateTextNode(self, aNode):
        return self.iDocument.createTextNode(str(aNode))
    
    
    
    