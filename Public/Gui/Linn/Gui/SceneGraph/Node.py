import sys
from Ft.Xml import XPath
import Linn.Gui.Serialize
import Linn.Gui.Messenger
import Linn.Gui.Renderer
import Linn.Gui.Resources.Message
import Linn.Gui.Resources.Plugin
import Linn.Gui.Resources.MessageTranslator
import Linn.Gui.SceneGraph.Srt

class DuplicateElement(Exception):
    def __init__(self, aSrcName, aName):
        self.iSrcName = aSrcName
        self.iName = aName
    def __str__(self):
        return "%s already exists in %s's list" % (self.iName, self.iSrcName)
    
class ElementNotFound(Exception):
    def __init__(self, aSrcName, aName):
        self.iSrcName = aSrcName
        self.iName = aName
    def __str__(self):
        return "%s not found in %s's list" % (self.iName, self.iSrcName)
    
class NodeNotParent(Exception):
    def __init__(self, aSrcName, aName):
        self.iSrcName = aSrcName
        self.iName = aName
    def __str__(self):
        return "%s is not %s's parent" % (self.iSrcName, self.iName)
        

class Node(Linn.Gui.Resources.Plugin.Plugin):
    def __init__(self, aName=None, aActive=True):
        if aName == None:
            aName = self.UniqueName('Node')
        Linn.Gui.Resources.Plugin.Plugin.__init__(self, aName)
        self.iParent = None
        self.iChildren = []
        self.iInMsgTrans = Linn.Gui.Resources.MessageTranslator.MessageTranslator()
        self.iOutMsgTrans = Linn.Gui.Resources.MessageTranslator.MessageTranslator()
        self.iActive = aActive
        self.iWorldSrt = Linn.Gui.SceneGraph.Srt.Srt()
        self.iLocalSrt = Linn.Gui.SceneGraph.Srt.Srt()
        self.iMsgQ = []
        
    def PackageDependencies(self):
        deps = []
        for i in self.iChildren:
            deps.append(i.Namespace())
        for i in self.iOutMsgTrans.TranslatorList():
            deps.append(i.iFromMsg.Namespace())
            deps.append(i.iToMsg.Namespace())
        return deps
        
    def Load(self, aStream):
        Linn.Gui.Resources.Plugin.Plugin.Load(self, aStream)
        
        parent = aStream.LoadNode('Parent')
        if len(parent) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Parent>', len(parent), 1)
        if parent[0].hasChildNodes():
            self.iParent = parent[0].firstChild.nodeValue
        #print 'parent =', self.iParent
        
        active = aStream.LoadNode('Active')
        if len(active) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Active>', len(active), 1)
        if active[0].hasChildNodes():
            value = active[0].firstChild.nodeValue.lower()
            if(value == '1' or value == 'true'):
                self.iActive = True
            else:
                self.iActive = False
        #print 'active =', self.iActive
        
        srt = aStream.LoadNode('Srt')
        if len(srt) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Srt>', len(srt), 1)
        aStream.PushContextNode(srt[0])
        self.iLocalSrt.Load(aStream)
        aStream.PopContextNode()
        
        msgTrans = aStream.LoadNode('OutMessageTranslator')
        if len(msgTrans) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<OutMessageTranslator>', len(msgTrans), 1)
        aStream.PushContextNode(msgTrans[0])
        self.iOutMsgTrans.Load(aStream)
        aStream.PopContextNode()
                    
        children = aStream.LoadNode('Children')
        if len(children) == 1:
            children = aStream.LoadNode('Children/Child')
            for child in children:
                try:
                    self.iChildren.index(child.firstChild.nodeValue)
                    print 'Warning: Removing %s from list (duplicate)' % child.firstChild.nodeValue
                except ValueError, e:
                    self.iChildren.append(child.firstChild.nodeValue)
                #print 'child =', child.firstChild.nodeValue
        elif len(children) > 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Children>', len(children), 1)
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'Node')
            
        Linn.Gui.Resources.Plugin.Plugin.Save(self, aStream)
        
        parent = aStream.CreateElement('Parent')
        if self.iParent and self.iParent.Name() != 'SceneGraphRoot':
            parent.appendChild(aStream.CreateTextNode(self.iParent.Fullname()))
        aStream.SaveNode(parent)
        
        active = aStream.CreateElement('Active')
        active.appendChild(aStream.CreateTextNode(str(self.iActive).lower()))
        aStream.SaveNode(active)
        
        self.iLocalSrt.Save(aStream)
        
        outTranslator = aStream.CreateElement('OutMessageTranslator')
        aStream.PushContextNode(outTranslator)
        self.iOutMsgTrans.Save(aStream)
        aStream.PopContextNode()
        aStream.SaveNode(outTranslator)
        
        children = aStream.CreateElement('Children')
        if len(self.iChildren):
            for child in self.iChildren:
                childRef = aStream.CreateElement('Child')
                childRef.appendChild(aStream.CreateTextNode(child.Fullname()))
                aStream.SaveNode(childRef)
                children.appendChild(childRef)
        aStream.SaveNode(children)
        
    def Link(self):
        #print 'Linking', self.Name(), '[Node]'
        Linn.Gui.Resources.Plugin.Plugin.Link(self)
        
        if not isinstance(self.iParent, Linn.Gui.SceneGraph.Node.Node):
            self.iParent = self.LinkPluginByName(self.iParent)
        for i in range(len(self.iChildren)):
            self.iChildren[i] = self.LinkPluginByName(self.iChildren[i])
            if self.iChildren[i].Parent() == None:
                self.iChildren[i].SetParent(self.LinkPluginByName(self.Fullname()))
        
    def Parent(self):
        return self.iParent
    
    def SetParent(self, aParent):
        self.iParent = aParent
        self.ObserverUpdate()
        
    def Active(self):
        return self.iActive
    
    def SetActive(self, aActive):
        self.iActive = aActive
        for child in self.iChildren:
            child.SetActive(aActive)
        self.Update()
        self.ObserverUpdate()
        
    def WorldSrt(self):
        return self.iWorldSrt
    
    def LocalSrt(self):
        return self.iLocalSrt
    
    def SetLocalTranslation(self, aVector3d):
        self.iLocalSrt.SetTranslation(aVector3d)
        self.Update()
        self.ObserverUpdate()
        
    def SetWorldTranslation(self, aVector3d):
        self.iWorldSrt.SetTranslation(aVector3d)
        if self.iParent:
            self.iLocalSrt.SetTranslation(self.iWorldSrt.Translation() - self.iParent.LocalSrt().Translation())
        else:
            self.iLocalSrt.SetTranslation(aVector3d)
        self.Update()
        self.ObserverUpdate()
            
    def OutMsgTranslator(self):
        return self.iOutMsgTrans
    
    def AddChild(self, aChild):
        """Add child to children list.
           If the child already has a parent:
            - Remove child from current parent
            - Add child to desired parent"""
        if aChild.Parent() == self:
            self.iChildren.index(aChild)
            raise DuplicateElement(self.Fullname(), aChild.Fullname())
        
        if aChild.Parent():
            aChild.Parent().RemoveChild(aChild)
            
        # set child's parent to self and add
        aChild.SetParent(self)
        self.iChildren.append(aChild)
        aChild.Update()
        self.ObserverUpdate()
        
    def RemoveChild(self, aChild):
        """remove child from children list (if found)."""
        if aChild.Parent() != self:
            raise NodeNotParent(self.Fullname(), aChild.Fullname())
        try:
            # set child's parent to None and remove
            self.iChildren.remove(aChild)
            aChild.SetParent(None)
            self.ObserverUpdate()
        except ValueError, e:
            raise ElementNotFound(self.Fullname(), aChild.Fullname())
    
    def Child(self, aIndex):
        """obtain child at index aIndex."""
        return self.iChildren[aIndex]
    
    def NumChildren(self):
        return len(self.iChildren)
    
    def Children(self):
        return self.iChildren
    
    def Draw(self, aRenderer):
        pass
    
    def Visit(self, aVisitor):
        # accept visitor for this node
        aVisitor.AcceptNode(self)
        # visit children
        for child in self.iChildren:
            child.Visit(aVisitor)
            
    def Update(self, aRecurse=True):
        if self.iActive:
            if self.iParent:
                parentPos = self.iParent.WorldSrt().Translation()
                self.iWorldSrt.SetTranslation(parentPos + self.iLocalSrt.Translation())
            else:
                self.iWorldSrt.SetTranslation(self.iLocalSrt.Translation())
                
            if aRecurse:
                for child in self.iChildren:
                    child.Update()
            self.ObserverUpdate()
                    
    def Matrix(self, aMatrix):
        Linn.Gui.Resources.Plugin.Plugin.Matrix(self, aMatrix)
        for child in self.iChildren:
            child.Matrix(aMatrix)
        
    def Vector3d(self, aVector3d):
        Linn.Gui.Resources.Plugin.Plugin.Vector3d(self, aVector3d)
        for child in self.iChildren:
            child.Vector3d(aVector3d)
            
    def ProcessMessage(self, aMessage):
        try:
            # check to see if we have recursed with the same message
            # this occurs once a message has gone around its plugin chain
            # in this case we just dont process the message
            self.iMsgQ.index(aMessage)
            return False
        except ValueError:
            return True
            
    def ReceiveMessage(self, aMessage):
        if self.ProcessMessage(aMessage):
            # add message we are processing to the queue
            self.iMsgQ.append(aMessage)
            
            if aMessage.Fullname() == self.Fullname():
                if isinstance(aMessage, Linn.Gui.Resources.Message.MsgSetActive):
                    self.SetActive(aMessage.Active())
                self.iMsgQ.remove(aMessage)
                return
                
            Linn.Gui.Resources.Plugin.Plugin.ReceiveMessage(self, aMessage)
            for child in self.iChildren:
                child.ReceiveMessage(aMessage)
            # remove processed message from the queue
            self.iMsgQ.remove(aMessage)
                
    def SendMessage(self, aMessage):
        if not self.iOutMsgTrans.SendMessage(aMessage):
            Linn.Gui.Messenger.GetMessenger().ApplicationMessage(aMessage)


        
 
