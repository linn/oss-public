import wx
from Ft.Xml import XPath
import Linn.Gui.Serialize
import Linn.Gui.Renderer
import Linn.Gui.SceneGraph.NodeHit
import copy
import Linn.Gui.Resources.Message
    

class NodeText(Linn.Gui.SceneGraph.NodeHit.NodeHit):
    iLeft = 0
    iCentre = 1
    iRight = 2
    def __init__(self, aName=None, aText='', aWidth=0, aHeight=0, aFont=None, aFontColour=wx.BLACK):
        if aName == None:
            aName = self.UniqueName('NodeText')
        Linn.Gui.SceneGraph.NodeHit.NodeHit.__init__(self, aName, aWidth, aHeight)
        self.iJustification = NodeText.iLeft
        self.iText = aText
        self.iFont = aFont
        self.iFontColour = aFontColour
        self.iReqTranslation = copy.copy(self.iLocalSrt.Translation())
        self.iMeasureText = True
        
    def Text(self):
        return self.iText
    
    def SetText(self, aText):
        self.iText = aText
        self.iMeasureText = True
        self.ObserverUpdate()
        
    def SetFont(self, aFont):
        self.iFont = aFont
        self.iMeasureText = True
        self.ObserverUpdate()
                
    def Font(self):
        return self.iFont
    
    def SetFontColour(self, aFontColour):
        self.iFontColour = aFontColour
        self.ObserverUpdate()
        
    def FontColour(self):
        return self.iFontColour
    
    def Justification(self):
        return self.iJustification
    
    def SetJustification(self, aJustification):
        if self.iJustification != aJustification:
            self.iJustification = aJustification
            self.Justify()
            self.ObserverUpdate()
        
    def Justify(self):
        #print self.iReqTranslation
        trans = copy.copy(self.iReqTranslation)
        if self.iJustification == NodeText.iLeft:
            self.iLocalSrt.SetTranslation(trans)
        elif self.iJustification == NodeText.iRight:
            trans.iX = trans.iX - self.iWidth
            self.iLocalSrt.SetTranslation(trans)
        elif self.iJustification == NodeText.iCentre:
            trans.iX = int(trans.iX - (0.5 * self.iWidth))
            self.iLocalSrt.SetTranslation(trans)
        self.Update()
        
    def MeasureNodeText(self):
        (self.iWidth, self.iHeight) = Linn.Gui.Renderer.GetRenderer().TextSize(self)
        
    def Load(self, aStream):
        Linn.Gui.SceneGraph.NodeHit.NodeHit.Load(self, aStream)
        
        text = aStream.LoadNode('Text')
        if len(text) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Text>', len(text), 1)
        self.iText = text[0].firstChild.nodeValue
        
        justification = aStream.LoadNode('Justification')
        if len(justification) > 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Justification>', len(justification), 1)
        elif len(justification) == 0:
            self.iJustification = NodeText.iLeft
        else:
            self.iJustification = int(justification[0].firstChild.nodeValue)
        
        font = aStream.LoadNode('Font')
        if len(font) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Font>', len(font), 1)
        if font[0].hasChildNodes():
            aStream.PushContextNode(font[0])
            
            family = aStream.LoadNode('Family')
            if len(family) != 1:
                #raise Linn.Gui.Serialize.InvalidLayoutFile('<Family>', len(family), 1)
                family = -1
            else:
                family = int(family[0].firstChild.nodeValue)
            faceName = aStream.LoadNode('FaceName')
            if len(faceName) != 1:
                raise Linn.Gui.Serialize.InvalidLayoutFile('<FaceName>', len(faceName), 1)
            faceName = faceName[0].firstChild.nodeValue
            point = aStream.LoadNode('PointSize')
            if len(point) != 1:
                raise Linn.Gui.Serialize.InvalidLayoutFile('<PointSize>', len(point), 1)
            point = int(point[0].firstChild.nodeValue)
            weight = aStream.LoadNode('Weight')
            if len(weight) != 1:
                weight = aStream.LoadNode('Bold')
                if len(weight) != 1:
                    raise Linn.Gui.Serialize.InvalidLayoutFile('<Weight/Bold>', len(weight), 1)
                if weight[0].firstChild.nodeValue == 'true':
                    weight = wx.FONTWEIGHT_BOLD
                else:
                    weight = wx.FONTWEIGHT_NORMAL
            else:
                weight = int(weight[0].firstChild.nodeValue)
            style = aStream.LoadNode('Style')
            if len(style) != 1:
                style = aStream.LoadNode('Italic')
                if len(style) != 1:
                    raise Linn.Gui.Serialize.InvalidLayoutFile('<Style/Italic>', len(style), 1)
                if style[0].firstChild.nodeValue == 'true':
                    style = wx.FONTSTYLE_ITALIC
                else:
                    style = wx.FONTSTYLE_NORMAL
            else:
                style = int(style[0].firstChild.nodeValue)
            underlined = aStream.LoadNode('Underlined')
            if len(underlined) != 1:
                raise Linn.Gui.Serialize.InvalidLayoutFile('<Underlined>', len(underlined), 1)
            if underlined[0].firstChild.nodeValue.lower() == 'false' or underlined[0].firstChild.nodeValue == '0':
                underlined = False
            else:
                underlined = True
            encoding = aStream.LoadNode('Encoding')
            if len(encoding) == 1:
                encoding = int(encoding[0].firstChild.nodeValue)
            else:
                encoding = wx.FONTENCODING_DEFAULT
                #raise Linn.Gui.Serialize.InvalidLayoutFile('<Encoding>', len(encoding), 1)
            
            self.iFont = wx.Font(point, family, style, weight, underlined, faceName, encoding)
        
            aStream.PopContextNode()
            
        colour = aStream.LoadNode('Colour')
        if len(colour) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Colour>', len(colour), 1)
        if colour[0].hasChildNodes():
            aStream.PushContextNode(colour[0])
            
            red = aStream.LoadNode('R')
            if len(red) != 1:
                raise Linn.Gui.Serialize.InvalidLayoutFile('<R>', len(red), 1)
            red = int(red[0].firstChild.nodeValue)
            green = aStream.LoadNode('G')
            if len(green) != 1:
                raise Linn.Gui.Serialize.InvalidLayoutFile('<G>', len(green), 1)
            green = int(green[0].firstChild.nodeValue)
            blue = aStream.LoadNode('B')
            if len(blue) != 1:
                raise Linn.Gui.Serialize.InvalidLayoutFile('<B>', len(blue), 1)
            blue = int(blue[0].firstChild.nodeValue)
            alpha = aStream.LoadNode('A')
            #if len(alpha) != 1:
            #    raise Linn.Gui.Serialize.InvalidLayoutFile('<A>', len(alpha), 1)
            if len(alpha) == 1:
                alpha = int(alpha[0].firstChild.nodeValue)

            self.iFontColour = wx.Colour(red, green, blue)
            
            aStream.PopContextNode()
            
    def Link(self):
        Linn.Gui.SceneGraph.NodeHit.NodeHit.Link(self)
        trans = copy.copy(self.iLocalSrt.Translation())
        if self.iJustification == NodeText.iLeft:
            self.SetLocalTranslation(trans)
        elif self.iJustification == NodeText.iRight:
            trans.iX = trans.iX - self.iWidth
            self.SetLocalTranslation(trans)
        elif self.iJustification == NodeText.iCentre:
            trans.iX = int(trans.iX - (0.5 * self.iWidth))
            self.SetLocalTranslation(trans)
        
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'NodeText')

        oldTrans = self.LocalSrt().Translation()
        oldReqTrans = copy.copy(self.iReqTranslation)
        self.SetLocalTranslation(self.iReqTranslation)
        
        Linn.Gui.SceneGraph.NodeHit.NodeHit.Save(self, aStream)
        
        text = aStream.CreateElement('Text')
        text.appendChild(aStream.CreateTextNode(self.iText))
        aStream.SaveNode(text)
        
        justification = aStream.CreateElement('Justification')
        justification.appendChild(aStream.CreateTextNode(self.iJustification))
        aStream.SaveNode(justification)
        
        font = aStream.CreateElement('Font')
        aStream.PushContextNode(font)
        #family = aStream.CreateElement('Family')
        #family.appendChild(aStream.CreateTextNode(self.iFont.GetFamily()))
        #aStream.SaveNode(family)
        faceName = aStream.CreateElement('FaceName')
        faceName.appendChild(aStream.CreateTextNode(self.iFont.GetFaceName()))
        aStream.SaveNode(faceName)
        point = aStream.CreateElement('PointSize')
        point.appendChild(aStream.CreateTextNode(self.iFont.GetPointSize()))
        aStream.SaveNode(point)
        bold = aStream.CreateElement('Bold')
        bold.appendChild(aStream.CreateTextNode(str(self.iFont.GetWeight() == wx.FONTWEIGHT_BOLD).lower()))
        aStream.SaveNode(bold)
        italic = aStream.CreateElement('Italic')
        italic.appendChild(aStream.CreateTextNode(str(self.iFont.GetStyle() == wx.FONTSTYLE_ITALIC).lower()))
        aStream.SaveNode(italic)
        underline = aStream.CreateElement('Underlined')
        underline.appendChild(aStream.CreateTextNode(str(self.iFont.GetUnderlined()).lower()))
        aStream.SaveNode(underline)
        #encoding = aStream.CreateElement('Encoding')
        #encoding.appendChild(aStream.CreateTextNode(self.iFont.GetDefaultEncoding()))
        #aStream.SaveNode(encoding)
        aStream.PopContextNode()
        aStream.SaveNode(font)
        
        colour = aStream.CreateElement('Colour')
        aStream.PushContextNode(colour)
        red = aStream.CreateElement('R')
        red.appendChild(aStream.CreateTextNode(self.iFontColour.Red()))
        aStream.SaveNode(red)
        green = aStream.CreateElement('G')
        green.appendChild(aStream.CreateTextNode(self.iFontColour.Green()))
        aStream.SaveNode(green)
        blue = aStream.CreateElement('B')
        blue.appendChild(aStream.CreateTextNode(self.iFontColour.Blue()))
        aStream.SaveNode(blue)
        alpha = aStream.CreateElement('A')
        alpha.appendChild(aStream.CreateTextNode(255))
        aStream.SaveNode(alpha)
        aStream.PopContextNode()
        aStream.SaveNode(colour)
        
        self.SetLocalTranslation(oldTrans)
        self.iReqTranslation = oldReqTrans
        
    def SetLocalTranslation(self, aVector3d):
        trans = copy.copy(aVector3d)
        if self.iJustification == NodeText.iLeft:
            self.iReqTranslation = aVector3d
        elif self.iJustification == NodeText.iRight:
            trans.iX = trans.iX + self.iWidth
            self.iReqTranlation = trans
        elif self.iJustification == NodeText.iCentre:
            trans.iX = int(trans.iX + (0.5 * self.iWidth))
            self.iReqTranslation = trans
        Linn.Gui.SceneGraph.NodeHit.NodeHit.SetLocalTranslation(self, aVector3d)
    
    def Draw(self, aRenderer):
        if self.iMeasureText:
            self.MeasureNodeText()
            self.Justify()
            self.iMeasureText = False
        aRenderer.DrawText(self)
        
    def Visit(self, aVisitor):
        Linn.Gui.SceneGraph.NodeHit.NodeHit.Visit(self, aVisitor)
        # accept visitor for this node
        aVisitor.AcceptText(self)
        
    def ReceiveMessage(self, aMessage):
        Linn.Gui.SceneGraph.NodeHit.NodeHit.ReceiveMessage(self, aMessage)
        if isinstance(aMessage, Linn.Gui.Resources.Message.MsgSetText):
            if aMessage.Fullname() == self.Fullname():
                self.SetText(aMessage.Text())
            
            
            
            
            

    
    

    