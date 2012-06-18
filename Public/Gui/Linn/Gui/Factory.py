import SceneGraph.Node
import SceneGraph.NodeHit
import SceneGraph.NodeInput
import SceneGraph.NodePolygon
import SceneGraph.NodeDiscovery
import SceneGraph.NodeText
import Resources.Controller
import Resources.TextureArray
import Resources.Texture
import Resources.TextureManager

class UnknownClassType(Exception):
    def __init__(self, aClassType):
        self.iClassType = aClassType
    def __str__(self):
        return 'Unknown class of type %s' % self.iClassType

def CreatePlugin(aNamespace, aClassType, aStream):
    """Using the plugin XML create a plugin of correct type
       and initialise it with saved XML data."""
    #print 'Creating class of type', aClassType
    newClass = None
    if aClassType == 'Node':
        newClass = SceneGraph.Node.Node()
    elif aClassType == 'NodeDiscovery':
        newClass = SceneGraph.NodeDiscovery.NodeDiscovery()
    elif aClassType == 'NodeHit':
        newClass = SceneGraph.NodeHit.NodeHit()
    elif aClassType == 'NodeInput':
        newClass = SceneGraph.NodeInput.NodeInput()
    elif aClassType == 'NodePolygon':
        newClass = SceneGraph.NodePolygon.NodePolygon()
    elif aClassType == 'NodeText':
        newClass = SceneGraph.NodeText.NodeText()
    elif aClassType == 'Monostable':
        newClass = Resources.Controller.Monostable()
    elif aClassType == 'Bistable':
        newClass = Resources.Controller.Bistable()
    elif aClassType == 'Counter':
        newClass = Resources.Controller.Counter()
    elif aClassType == 'TextureArray':
        newClass = Resources.TextureArray.TextureArray()
    elif aClassType == 'TextureArrayFixed':
        newClass = Resources.TextureArray.TextureArrayFixed()
    if newClass:
        newClass.Load(aStream)
        newClass.SetNamespace(aNamespace)
        return newClass
    raise UnknownClassType(aClassType)

    
    
    