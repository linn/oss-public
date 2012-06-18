from Ft.Xml import XPath
import Linn.Gui.Serialize
import Linn.Gui.Renderer
import Linn.Gui.SceneGraph.NodePolygon

class NodeInput(Linn.Gui.SceneGraph.NodePolygon.NodePolygon):
    def __init__(self, aName=None, aWidth=0, aHeight=0):
        if aName == None:
            aName = self.UniqueName('NodeInput')                  
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.__init__(self, aName, aWidth, aHeight)
        
    def Load(self, aStream):
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.Load(self, aStream)
        
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'NodeInput')
            
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.Save(self, aStream)
        
    def Link(self):
        #print 'Linking', self.Name(), '[NodeInput]'
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.Link(self)
        
    def Hit(self, aVector3d):
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.Hit(self, aVector3d)
    
    def Motion(self, aVector3d):
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.Motion(self, aVector3d)
        
    def UnHit(self):
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.UnHit(self)
        
    def SendCommand(self):
        pass
