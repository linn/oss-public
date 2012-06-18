from Ft.Xml import XPath
import Linn.Gui.Serialize
import Linn.Gui.SceneGraph.Node

class NodeHit(Linn.Gui.SceneGraph.Node.Node):
    def __init__(self, aName=None, aWidth=0, aHeight=0):
        if aName == None:
            aName = self.UniqueName('NodeHit')
        Linn.Gui.SceneGraph.Node.Node.__init__(self, aName)
        self.iWidth = aWidth
        self.iHeight = aHeight
        
    def Load(self, aStream):
        Linn.Gui.SceneGraph.Node.Node.Load(self, aStream)
            
        width = aStream.LoadNode('Width')
        if len(width) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Width>', len(width), 1)
        self.iWidth = float(width[0].firstChild.nodeValue)
        #print 'width = ', self.iWidth
        
        height = aStream.LoadNode('Height')
        if len(height) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Height>', len(height), 1)
        self.iHeight = float(height[0].firstChild.nodeValue)
        #print 'height = ', self.iHeight
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'NodeHit')
            
        Linn.Gui.SceneGraph.Node.Node.Save(self, aStream)
        
        width = aStream.CreateElement('Width')
        width.appendChild(aStream.CreateTextNode(str(self.iWidth)))
        aStream.SaveNode(width)
        
        height = aStream.CreateElement('Height')
        height.appendChild(aStream.CreateTextNode(str(self.iHeight)))
        aStream.SaveNode(height)
    
    def Visit(self, aVisitor):
        Linn.Gui.SceneGraph.Node.Node.Visit(self, aVisitor)
        # accept visitor for this node
        aVisitor.AcceptHit(self)
            
    def Width(self):
        return self.iWidth
    
    def SetWidth(self, aWidth):
        self.iWidth = aWidth
        self.ObserverUpdate()
    
    def Height(self):
        return self.iHeight
    
    def SetHeight(self, aHeight):
        self.iHeight = aHeight
        self.ObserverUpdate()
            
    def IsInside(self, aPosition):
        if aPosition.iX > 0 and aPosition.iX < self.iWidth and \
           aPosition.iY > 0 and aPosition.iY < self.iHeight:
               return True
        else:
            return False

        
 
