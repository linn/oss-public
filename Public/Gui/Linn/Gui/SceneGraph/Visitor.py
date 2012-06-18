import sys

class DuplicateNodeNameFound(Exception):
    pass

class Visitor:
    """A Visitor used to search the scene graph."""
    def VisitNode(self, aNode):
        if aNode:
            aNode.Visit(self)
            
    def Accept(self, aNode):
        pass
    
    def AcceptNode(self, aNode):
        self.Accept(aNode)
        
    def AcceptHit(self, aHit):
        self.Accept(aHit)
        
    def AcceptPolygon(self, aPolygon):
        self.Accept(aPolygon)
        
    def AcceptText(self, aText):
        self.Accept(aText)
        
        
class SearchVisitor(Visitor):
    """Search the scene graph for a node matching aName."""
    def __init__(self, aName):
        self.iName = aName
        self.iResult = None
        
    def Search(self, aNode):
        self.VisitNode(aNode)
        return self.iResult
        
    def AcceptNode(self, aNode):
        if aNode.Name() == self.iName:
            if self.iResult:
                raise DuplicateNodeNameFound
            else:
                self.iResult = aNode
                
                
class FindGreatestZ(Visitor):
    """Seach the scene graph for the greatest z value."""
    def __init__(self):
        self.iZ = None
        
    def Search(self, aNode):
        self.VisitNode(aNode)
        return self.iZ
    
    def AcceptNode(self, aNode):
        if self.iZ:
            if aNode.WorldSrt().Translation().iZ > self.iZ:
                self.iZ = aNode.WorldSrt().Translation().iZ
        else:
            self.iZ = aNode.WorldSrt().Translation().iZ
                
                
class HitProxyVisitor(Visitor):
    """Search the scene graph for a node containing aPosition."""
    def __init__(self, aPosition):
        self.iPosition = aPosition
        self.iResults = []
        
    def Hit(self, aNode):
        self.VisitNode(aNode)
        if len(self.iResults):
            self.iResults.sort(lambda x,y: cmp(x.WorldSrt().Translation().iZ,y.WorldSrt().Translation().iZ))
            return self.iResults[-1]
        return None
    
    def AcceptHit(self, aHit):
        if aHit.Active():
            if aHit.IsInside(self.iPosition - aHit.WorldSrt().Translation()):
                self.iResults.append(aHit)


class RenderVisitor(Visitor):
    """Render the scene graph."""
    def __init__(self, aRenderer):
        self.iRenderer = aRenderer
        self.iRenderList = []
        
    def Render(self, aNode):
        self.VisitNode(aNode)
        self.iRenderList.sort(lambda x,y: cmp(x.WorldSrt().Translation().iZ,y.WorldSrt().Translation().iZ))
        for elem in self.iRenderList:
            elem.Draw(self.iRenderer)
    
    def AcceptPolygon(self, aPolygon):
        if aPolygon.Active():
            #self.iRenderer.DrawPolygon(aPolygon)
            self.iRenderList.append(aPolygon)
            
    def AcceptText(self, aText):
        if aText.Active():
            #self.iRenderer.DrawText(aText)
            self.iRenderList.append(aText)
    

class PrintVisitor(Visitor):
    """Print the scene graph structure to the console."""
    def __init__(self, aRoot):
        self.iRoot = aRoot
        
    def Print(self):
        self.VisitNode(self.iRoot)
        
    def AcceptNode(self, aNode):
        parent = aNode.Parent()
        while parent:
            parent = parent.Parent()
            print '  ',
        print aNode.Name()
                