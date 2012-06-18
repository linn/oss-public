from Ft.Xml import XPath
import Linn.Gui.Serialize
import Linn.Gui.Renderer
import Linn.Gui.SceneGraph.NodeHit
import Linn.Gui.SceneGraph.RenderState


class NodePolygon(Linn.Gui.SceneGraph.NodeHit.NodeHit):
    def __init__(self, aName=None, aWidth=0, aHeight=0, aTextureScale=False):
        if aName == None:
            aName = self.UniqueName('NodePolygon')
        Linn.Gui.SceneGraph.NodeHit.NodeHit.__init__(self, aName, aWidth, aHeight)
        self.iRenderState = Linn.Gui.SceneGraph.RenderState.RenderState()
        self.iTextureScale = aTextureScale
        
    def Load(self, aStream):
        Linn.Gui.SceneGraph.NodeHit.NodeHit.Load(self, aStream)
        
        rs = aStream.LoadNode('RenderState')
        if len(rs) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<RenderState>', len(rs), 1)
        if rs[0].hasChildNodes():
            aStream.PushContextNode(rs[0])
            self.iRenderState.Load(aStream)
            aStream.PopContextNode()
        
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'NodePolygon')
            
        Linn.Gui.SceneGraph.NodeHit.NodeHit.Save(self, aStream)
        
        self.iRenderState.Save(aStream)
        
    def Link(self):
        #print 'Linking', self.Name(), '[NodePolygon]'
        Linn.Gui.SceneGraph.NodeHit.NodeHit.Link(self)

        self.iRenderState.Link()
        #self.Texture(self.iRenderState.Texture())
        
    def Update(self, aRecurse=True):
        Linn.Gui.SceneGraph.NodeHit.NodeHit.Update(self, aRecurse)
        if not self.iTextureScale and self.iRenderState.Texture():
            if self.iWidth != self.iRenderState.Texture().Width() or self.iHeight != self.iRenderState.Texture().Height():
                self.iWidth = self.iRenderState.Texture().Width()
                self.iHeight = self.iRenderState.Texture().Height()
                self.ObserverUpdate()
        
    def RenderState(self):
        return self.iRenderState
    
    def SetRenderState(self, aRenderState):
        self.iRenderState = aRenderState
        
    def Draw(self, aRenderer):
        # NOTE: this should not be done here... Some sort of refresh required on texture reload
        if not self.iTextureScale and self.iRenderState.Texture():
            if self.iWidth != self.iRenderState.Texture().Width() and self.iHeight != self.iRenderState.Texture().Height():
                self.iWidth = self.iRenderState.Texture().Width()
                self.iHeight = self.iRenderState.Texture().Height()
                self.ObserverUpdate()
        aRenderer.DrawPolygon(self)
        
    def Visit(self, aVisitor):
        Linn.Gui.SceneGraph.NodeHit.NodeHit.Visit(self, aVisitor)
        # accept visitor for this node
        aVisitor.AcceptPolygon(self)
        
    def Texture(self, aTexture):
        self.iRenderState.SetTexture(aTexture)
        if not self.iTextureScale and aTexture:
            self.iWidth = aTexture.Width()
            self.iHeight = aTexture.Height()
            self.ObserverUpdate()
        
    def ScaleTexture(self):
        return self.iTextureScale





            