from Ft.Xml import XPath
import Linn.Gui.Serialize
import Linn.Gui.Resources.TextureManager

class RenderState(Linn.Gui.Serialize.ISerialize):
    def __init__(self, aTexture=None):
        self.iTexture = aTexture
        
    def Texture(self):
        return self.iTexture
    
    def SetTexture(self, aTexture):
        self.iTexture = aTexture
        
    def Load(self, aStream):
        texture = aStream.LoadNode('Texture')
        if len(texture) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Texture>', len(texture), 1)
        self.iTexture = texture[0].firstChild.nodeValue
        
    def Save(self, aStream):
        rs = aStream.CreateElement('RenderState')
        if self.iTexture:
            texture = aStream.CreateElement('Texture')
            texture.appendChild(aStream.CreateTextNode(self.iTexture.Filename()))
            rs.appendChild(texture)
        aStream.SaveNode(rs)
        
    def Link(self):
        #print 'Linking [RenderState]'
        if self.iTexture:
            self.iTexture = Linn.Gui.Resources.TextureManager.GetTextureManager().TextureByNameOrLoad(self.iTexture)
            
            